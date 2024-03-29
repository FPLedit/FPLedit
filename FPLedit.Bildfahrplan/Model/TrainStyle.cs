﻿using System;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;

namespace FPLedit.Bildfahrplan.Model;

internal sealed class TrainStyle : Style
{
    public ITrain Train { get; }

    private readonly TimetableStyle ttStyle;

    public TrainStyle(ITrain tra, TimetableStyle ttStyle)
    {
        Train = tra;
        this.ttStyle = ttStyle;
    }

    public void ResetDefaults()
    {
        if (!(Train is IWritableTrain))
            throw new InvalidOperationException("Style of linked train cannot be changed!");
        TrainColor = null;
        TrainWidth = null;
        Show = true;
        LineStyle = 0;
    }

    public MColor? TrainColor
    {
        get => ParseColor(Train.GetAttribute<string>("cl"), null);
        set
        {
            if (!(Train is IWritableTrain))
                throw new InvalidOperationException("Style of linked train cannot be changed!");
            Train.SetAttribute("cl", ColorToString(value ?? MColor.White));
        }
    }

    public MColor CalcedColor => OverrideEntityStyle ? ttStyle.TrainColor : (TrainColor ?? ttStyle.TrainColor);
    public string? HexColor
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
        set
        {
            if (!(Train is IWritableTrain))
                throw new InvalidOperationException("Style of linked train cannot be changed!");
            if (value.HasValue)
                Train.SetAttribute("sz", value.Value.ToString());
            else
                Train.RemoveAttribute("sz");
        }
    }
    public int CalcedWidth => OverrideEntityStyle ? ttStyle.TrainWidth : (TrainWidth ?? ttStyle.TrainWidth);

    public int TrainWidthInt
    {
        get => Train.GetAttribute("sz", -1);
        set
        {
            if (!(Train is IWritableTrain))
                throw new InvalidOperationException("Style of linked train cannot be changed!");
            Train.SetAttribute("sz", value.ToString());
        }
    }

    public bool Show
    {
        get => Train.GetAttribute("sh", true);
        set
        {
            if (!(Train is IWritableTrain))
                throw new InvalidOperationException("Style of linked train cannot be changed!");
            Train.SetAttribute("sh", value.ToString().ToLower());
        }
    }
    public bool CalcedShow => OverrideEntityStyle || Show;

    public int LineStyle
    {
        get => Train.GetAttribute("sy", 0);
        set
        {
            if (!(Train is IWritableTrain))
                throw new InvalidOperationException("Style of linked train cannot be changed!");
            Train.SetAttribute("sy", value.ToString());
        }
    }
    public int CalcedLineStyle => OverrideEntityStyle ? 0 : LineStyle;
}