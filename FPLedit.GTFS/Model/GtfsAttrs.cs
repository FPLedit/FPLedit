using FPLedit.Shared;
using System;
using System.Linq;
using System.Text;

namespace FPLedit.GTFS.Model
{
    [XElmName("gtfs_attrs", IsFpleditElement = true)]
    public sealed class GTFSAttrs : Entity, IPatternSource
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

        private GTFSAttrs(Timetable tt) : base("gtfs_attrs", tt)
        {
        }

        private GTFSAttrs(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        public static GTFSAttrs GetAttrs(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "gtfs_attrs");
            if (attrsEn != null)
                return new GTFSAttrs(attrsEn, tt);
            return null;
        }

        public static GTFSAttrs CreateAttrs(Timetable tt)
        {
            var attrs = new GTFSAttrs(tt);
            tt.Children.Add(attrs.XMLEntity);
            return attrs;
        }
    }
}
