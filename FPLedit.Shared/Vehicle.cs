using System.Collections;
using System.Collections.Generic;

namespace FPLedit.Shared
{
    [XElmName(DEFAULT_XNAME, ParentElements = new []{ "vehicles" })]
    [Templating.TemplateSafe]
    public class Vehicle : Entity
    {
        internal const string DEFAULT_XNAME = "veh";
        
        [XAttrName("id")]
        public int Id
        {
            get => GetAttribute("id", -1);
            set => SetAttribute("id", value.ToString());
        }

        [XAttrName("name")]
        public string VName
        {
            get => GetAttribute("name", "");
            set => SetAttribute("name", value);
        }

        public IList<VehicleTrainStart> StartTrains
            => new ObservableChildrenCollection<VehicleTrainStart>(this, VehicleTrainStart.DEFAULT_XNAME, ParentTimetable);
        
        public IList<NextVehicle> NextVehicles
            => new ObservableChildrenCollection<NextVehicle>(this, NextVehicle.DEFAULT_XNAME, ParentTimetable);
        
        public Vehicle(Timetable tt) : base(DEFAULT_XNAME, tt)
        {
        }

        public Vehicle(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }
    }

    [XElmName(DEFAULT_XNAME, ParentElements = new []{ Vehicle.DEFAULT_XNAME })]
    [Templating.TemplateSafe]
    public class VehicleTrainStart : Entity
    {
        internal const string DEFAULT_XNAME = "as";
        
        [XAttrName("zid")]
        public int TrainId
        {
            get => GetAttribute("zid", -1);
            set => SetAttribute("zid", value.ToString());
        }

        [XAttrName("d")]
        public Days Days
        {
            get => Days.Parse(GetAttribute("d", "1111111"));
            set => SetAttribute("d", value.ToBinString());
        }
        
        public VehicleTrainStart(Timetable tt) : base(DEFAULT_XNAME, tt)
        {
        }

        public VehicleTrainStart(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }
    }
    
    [XElmName(DEFAULT_XNAME, ParentElements = new []{ Vehicle.DEFAULT_XNAME })]
    [Templating.TemplateSafe]
    public class NextVehicle : Entity
    {
        internal const string DEFAULT_XNAME = "nextv";
        
        [XAttrName("vid")]
        public int TrainId
        {
            get => GetAttribute("vid", -1);
            set => SetAttribute("vid", value.ToString());
        }

        [XAttrName("d")]
        public Days Days
        {
            get => Days.Parse(GetAttribute("d", "1111111"));
            set => SetAttribute("d", value.ToBinString());
        }
        
        public NextVehicle(Timetable tt) : base(DEFAULT_XNAME, tt)
        {
        }

        public NextVehicle(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }
    }
}