using System;
using System.Collections.Generic;
using Eto.Drawing;
using FPLedit.Shared.Rendering;
using NUnit.Framework;

namespace FPLedit.Shared.Tests
{
    public class StressTests
    {
        [Test]
        [Category("Stress")]
        [Ignore("Stress test - only used to generate test files")]
        public void GenerateHugeTimetable()
        {
            // this should not fail
            Timetable tt = new Timetable(TimetableType.Network);
            var handler = new StationCanvasPositionHandler();
            Dictionary<Station, Point> stapos = new Dictionary<Station, Point>();
            var rand = new Random();
            Station? branchS = null;
            for (int i = 0; i < 2000; i++)
            {
                var branch = rand.Next(2, 98);
                for (int j = 1; j < 100; j++)
                {
                    var sta = new Station(tt) { SName = $"Teststation {i},{j}" };

                    if (j == branch)
                        branchS = sta;
                    for (int k = 0; k < rand.Next(10); k++)
                    {
                        sta.Tracks.Add(new Track(tt)
                        {
                            Name = "Neues Gleis " + k,
                        });
                    }

                    if (j == 1 && i > 0 && branchS != null)
                        tt.AddRoute(branchS, sta, 0f, 1f);
                    else
                    {
                        tt.AddStation(sta, i);
                        sta.Positions.SetPosition(i, j);
                    }
                    stapos.Add(sta, new Point(j * 70, i * 70));
                }
            }
            handler.WriteStapos(tt, stapos);
        }
    }
}