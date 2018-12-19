using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Bildfahrplan.Model
{
    internal class TrainStyle : Style
    {
        private Train train;
        private TimetableStyle ttStyle;
        public TrainStyle(Train tra, TimetableStyle ttStyle) : base(tra._parent)
        {
            train = tra;
            this.ttStyle = ttStyle;
        }

        public TrainStyle(Train tra) : base(tra._parent)
        {
            train = tra;
        }

        public MColor TrainColor
        {
            get => ParseColor(train.GetAttribute<string>("cl"), null);
            set => train.SetAttribute("cl", ColorToString(value ?? MColor.White));
        }
        public MColor CalcedColor => overrideEntityStyle ? ttStyle.TrainColor : (TrainColor ?? ttStyle.TrainColor);
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
        public int CalcedWidth => overrideEntityStyle ? ttStyle.TrainWidth : (TrainWidth ?? ttStyle.TrainWidth);

        public bool Show
        {
            get => train.GetAttribute("sh", true);
            set => train.SetAttribute("sh", value.ToString().ToLower());
        }
        public bool CalcedShow => overrideEntityStyle || Show;

        public int LineStyle
        {
            get => train.GetAttribute<int>("sy", 0);
            set => train.SetAttribute("sy", value.ToString());
        }
        public int CalcedLineStyle => overrideEntityStyle ? 0 : LineStyle;
    }
}
