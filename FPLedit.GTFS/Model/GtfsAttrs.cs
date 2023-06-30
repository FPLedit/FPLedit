using FPLedit.Shared;
using System;
using System.Linq;
using System.Text;

namespace FPLedit.GTFS.Model
{
    [XElmName("gtfs_attrs", IsFpleditElement = true)]
    public sealed class GtfsAttrs : Entity, IPatternSource
    {
        [XAttrName("tp")]
        public string TrainPatterns
        {
            get => GetAttribute("tp", "");
            set => SetAttribute("tp", value);
        }

        [XAttrName("sp")]
        public string StationPatterns
        {
            get => GetAttribute("sp", "");
            set => SetAttribute("sp", value);
        }

        [XAttrName("agency_name")]
        public string AgencyName
        {
            get => GetAttribute("agency_name", "");
            set => SetAttribute("agency_name", value);
        }
        
        [XAttrName("agency_lang")]
        public string AgencyLang
        {
            get => GetAttribute("agency_lang", "");
            set => SetAttribute("agency_lang", value);
        }
        
        [XAttrName("agency_url")]
        public string AgencyUrl
        {
            get => GetAttribute("agency_url", "");
            set => SetAttribute("agency_url", value);
        }
        
        [XAttrName("agency_timezone")]
        public string AgencyTimezone
        {
            get => GetAttribute("agency_timezone", "");
            set => SetAttribute("agency_timezone", value);
        }

        [XAttrName("route_name")]
        public string RouteName
        {
            get => GetAttribute("route_name", "");
            set => SetAttribute("route_name", value);
        }
        
        private GtfsAttrs(Timetable tt) : base("gtfs_attrs", tt)
        {
        }

        private GtfsAttrs(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        public static GtfsAttrs GetAttrs(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "gtfs_attrs");
            if (attrsEn != null)
                return new GtfsAttrs(attrsEn, tt);
            return null;
        }

        public static GtfsAttrs CreateAttrs(Timetable tt)
        {
            var attrs = new GtfsAttrs(tt);
            tt.Children.Add(attrs.XMLEntity);
            return attrs;
        }
    }
}
