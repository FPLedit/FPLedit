﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [Serializable]
    public class Route
    {
        public int Index { get; set; }

        public List<Station> Stations { get; set; }

        public float MinPosition
            => Stations.Min(s => s.Positions.GetPosition(Index)) ?? 0f;

        public float MaxPosition
            => Stations.Max(s => s.Positions.GetPosition(Index)) ?? 0f;

        public Route()
        {
            Stations = new List<Station>();
        }

        public string GetRouteName()
        {
            var st = GetOrderedStations();
            return (st.FirstOrDefault()?.SName ?? "") + " - " + (st.LastOrDefault()?.SName ?? "");
        }

        public List<Station> GetOrderedStations()
            => Stations.OrderBy(s => s.Positions.GetPosition(Index)).ToList();
    }
}