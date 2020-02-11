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
        public Train Train { get; }
        
        private readonly TimetableStyle ttStyle;
        public TrainStyle(Train tra, TimetableStyle ttStyle) : base(tra._parent)
        {
            Train = tra;
            this.ttStyle = ttStyle;
        }

        public TrainStyle(Train tra) : base(tra._parent)
        {
            Train = tra;
        }
        
        public void ResetDefaults()
        {
            TrainColor = null;
            TrainWidth = null;
            Show = true;
            LineStyle = 0;
        }

        public MColor TrainColor
        {
            get => ParseColor(Train.GetAttribute<string>("cl"), null);
            set => Train.SetAttribute("cl", ColorToString(value ?? MColor.White));
        }
        public MColor CalcedColor => overrideEntityStyle ? ttStyle.TrainColor : (TrainColor ?? ttStyle.TrainColor);
        public string HexColor
        {
            get => TrainColor != null ? ColorFormatter.ToString(TrainColor, false) : null;
            set => TrainColor = ColorFormatter.FromString(value, MColor.White);
        }
        
        public int? TrainWidth
        {
            get
            {
                var val = Train.GetAttribute("sz", -1);
                if (val == -1)
                    return null;
                return val;
            }
            set => Train.SetAttribute("sz", value.ToString());
        }
        public int CalcedWidth => overrideEntityStyle ? ttStyle.TrainWidth : (TrainWidth ?? ttStyle.TrainWidth);
        public int TrainWidthInt
        {
            get => Train.GetAttribute("sz", -1);
            set => Train.SetAttribute("sz", value.ToString());
        }

        public bool Show
        {
            get => Train.GetAttribute("sh", true);
            set => Train.SetAttribute("sh", value.ToString().ToLower());
        }
        public bool CalcedShow => overrideEntityStyle || Show;

        public int LineStyle
        {
            get => Train.GetAttribute<int>("sy", 0);
            set => Train.SetAttribute("sy", value.ToString());
        }
        public int CalcedLineStyle => overrideEntityStyle ? 0 : LineStyle;
    }
}
