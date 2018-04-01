using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FPLedit.BildfahrplanExport.Model
{
    internal class TrainStyle : Style
    {
        private Train train;

        public TrainStyle(Train tra) : base(tra._parent)
        {
            train = tra;
        }

        public Color? TrainColor
        {
            get => ParseColor(train.GetAttribute<string>("cl"), null);
            set => train.SetAttribute("cl", ColorToString(value ?? Color.White));
        }

        public int? TrainWidth
        {
            get
            {
                var val = train.GetAttribute("sz", -1);
                if (val == -1)
                    return null;
                return val;
            }
            set => train.SetAttribute("sz", value.ToString());
        }

        public bool Show
        {
            get => train.GetAttribute("sh", true);
            set => train.SetAttribute("sh", value.ToString().ToLower());
        }

        //TODO: Linestyle
    }
}
