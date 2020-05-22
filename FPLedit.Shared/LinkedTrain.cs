using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FPLedit.Shared
{
    /// <summary>
    /// Object model class representing a single train (linked an thus not writable) of this timetable.
    /// </summary>
    [DebuggerDisplay("Linked: {" + nameof(TName) + "}")]
    [XElmName("ti", "ta", "tr")]
    [Templating.TemplateSafe]
    public class LinkedTrain : Entity, ITrain
    {
        private readonly ITrain baseTrain;
        private readonly TrainLink link;
        private readonly int countingIndex;

        /// <summary>
        /// The train link, that is used to create this train.
        /// </summary>
        public TrainLink Link => link;

        /// <summary>
        /// The zero-based counting index, that is assigned to this train.
        /// </summary>
        public int LinkCountingIndex => countingIndex;

        /// <inheritdoc />
        [XAttrName("id")]
        public int Id => GetAttribute<int>("id", -1);

        /// <inheritdoc />
        public string QualifiedId => Id + ";" + link.TrainLinkIndex + ";" + countingIndex;

        /// <inheritdoc />
        public string TName => link.GetChildTrainName(countingIndex);

        /// <inheritdoc />
        public string Comment => baseTrain.Comment;

        /// <inheritdoc />
        public bool IsLink => true;

        /// <inheritdoc />
        public Days Days => baseTrain.Days;

        /// <inheritdoc />
        public TrainDirection Direction => baseTrain.Direction;

        /// <inheritdoc />
        public string Last => baseTrain.Last;

        /// <inheritdoc />
        public string Locomotive => baseTrain.Locomotive;

        /// <inheritdoc />
        public string Mbr => baseTrain.Mbr;

        /// <inheritdoc />
        public List<Station> GetPath() => baseTrain.GetPath();

        /// <inheritdoc />
        public ArrDep GetArrDep(Station sta)
        {
            if (TryGetArrDep(sta, out var arrDep))
                return arrDep;
            throw new Exception($"No ArrDep found for station {sta.SName}!");
        }

        /// <inheritdoc />
        public bool TryGetArrDep(Station sta, [NotNullWhen(returnValue: true)] out ArrDep? arrDep)
        {
            arrDep = null;
            if (baseTrain.TryGetArrDep(sta, out var tempArrDep))
                arrDep = link.ProcessArrDep(tempArrDep, countingIndex);
            return arrDep != null;
        }

        /// <inheritdoc />
        public Dictionary<Station, ArrDep> GetArrDepsUnsorted()
        {
            return baseTrain
                .GetArrDepsUnsorted()
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => link.ProcessArrDep(kvp.Value, countingIndex));
        }

        /// <summary>
        /// Create a new linked train instance, based on an existing xml structure.
        /// </summary>
        /// <param name="link">Parent link object.</param>
        /// <param name="countingIndex">The counting index of this train, relative to <paramref name="link"/>.</param>
        /// <param name="entity">The pre-existing xml structure.</param>
        public LinkedTrain(TrainLink link, int countingIndex, XMLEntity entity) : base(entity, link._parent)
        {
            this.link = link;
            this.countingIndex = countingIndex;
            baseTrain = link.ParentTrain;
        }
        
        /// <summary>
        /// Create a new linked train instance.
        /// </summary>
        /// <param name="link">Parent link object.</param>
        /// <param name="countingIndex">The counting index of this train, relative to <paramref name="link"/>.</param>
        public LinkedTrain(TrainLink link, int countingIndex) : base(link.ParentTrain.XMLEntity.XName, link._parent)
        {
            this.link = link;
            this.countingIndex = countingIndex;
            baseTrain = link.ParentTrain;
            link._InternalInjectLinkedTrain(this, countingIndex);

            link.Apply(true, true);
        }
    }
}