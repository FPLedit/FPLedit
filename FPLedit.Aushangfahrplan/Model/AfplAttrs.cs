﻿using FPLedit.Shared;
using System;
using System.Linq;
using System.Text;

namespace FPLedit.Aushangfahrplan.Model;

[XElmName("afpl_attrs", IsFpleditElement = true)]
public sealed class AfplAttrs : Entity, IPatternSource
{
    [XAttrName("font")]
    public string Font
    {
        get => GetAttribute("font", "");
        set => SetAttribute("font", value);
    }

    [XAttrName("hefont")]
    public string HwFont
    {
        get => GetAttribute("hwfont", "");
        set => SetAttribute("hwfont", value);
    }

    [XAttrName("css")]
    public string Css
    {
        get
        {
            var val = Children.FirstOrDefault(x => x.XName == "css")?.Value ?? "";
            var bytes = Convert.FromBase64String(val);
            return Encoding.UTF8.GetString(bytes);
        }
        set
        {
            var bytes = Encoding.UTF8.GetBytes(value);

            var elm = Children.FirstOrDefault(x => x.XName == "css");
            if (elm == null)
            {
                elm = new XMLEntity("css");
                Children.Add(elm);
            }
            elm.Value = Convert.ToBase64String(bytes);
        }
    }

    [XAttrName("tmpl")]
    public string Template
    {
        get => GetAttribute("tmpl", "");
        set => SetAttribute("tmpl", value);
    }

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

    [XAttrName("shT")]
    public bool ShowTracks
    {
        get => GetAttribute("shT", true);
        set => SetAttribute("shT", value.ToString().ToLower());
    }

    [XAttrName("omit-single-tr")]
    public bool OmitTracksWhenSingle
    {
        get => GetAttribute("omit-single-tr", true);
        set => SetAttribute("omit-single-tr", value.ToString().ToLower());
    }

    private AfplAttrs(Timetable tt) : base("afpl_attrs", tt)
    {
    }

    private AfplAttrs(XMLEntity en, Timetable tt) : base(en, tt)
    {
    }

    public static AfplAttrs? GetAttrs(Timetable tt)
    {
        var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "afpl_attrs");
        if (attrsEn != null)
            return new AfplAttrs(attrsEn, tt);
        return null;
    }

    public static AfplAttrs CreateAttrs(Timetable tt)
    {
        var attrs = new AfplAttrs(tt);
        tt.Children.Add(attrs.XMLEntity);
        return attrs;
    }
}