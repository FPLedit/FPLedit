using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FPLedit.Shared
{
    [DebuggerDisplay("Linked: {" + nameof(TName) + "}")]
    [XElmName("ti", "ta", "tr")]
    [Templating.TemplateSafe]
    public class LinkedTrain : Entity, ITrain
    {
        private readonly ITrain baseTrain;
        private readonly TrainLink link;
        private readonly int countingIndex;

        public TrainLink Link => link;

        public int LinkCountingIndex => countingIndex;

        /// <inheritdoc />
        [XAttrName("id")]
        public int Id => GetAttribute<int>("id", -1);

        public string QualifiedId => Id + ";" + link.TrainLinkIndex + ";" + countingIndex;

        public string TName => link.GetChildTrainName(countingIndex);

        public string Comment => baseTrain.Comment;

        public bool IsLink => true;

        public Days Days => baseTrain.Days;

        public TrainDirection Direction => baseTrain.Direction;

        public string Last => baseTrain.Last;

        public string Locomotive => baseTrain.Locomotive;

        public string Mbr => baseTrain.Mbr;

        public List<Station> GetPath() => baseTrain.GetPath();

        public ArrDep GetArrDep(Station sta)
        {
            if (TryGetArrDep(sta, out var arrDep))
                return arrDep;
            throw new Exception($"No ArrDep found for station {sta.SName}!");
        }

        public bool TryGetArrDep(Station sta, out ArrDep arrDep)
        {
            var ret = baseTrain.TryGetArrDep(sta, out var tempArrDep);
            if (ret)
                arrDep = link.ProcessArrDep(tempArrDep, countingIndex);
            else
                arrDep = null;
            return ret;
        }

        public Dictionary<Station, ArrDep> GetArrDepsUnsorted()
        {
            return baseTrain
                .GetArrDepsUnsorted()
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => link.ProcessArrDep(kvp.Value, countingIndex));
        }

        public LinkedTrain(TrainLink link, int countingIndex, XMLEntity entity) : base(entity, link._parent)
        {
            this.link = link;
            this.countingIndex = countingIndex;
            baseTrain = link.ParentTrain;
        }
        
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