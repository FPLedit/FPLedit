using System;
using NUnit.Framework;

namespace FPLedit.Shared.Tests
{
    public class StressTests
    {
        [Test]
        public void GenerateHugeTimetable()
        {
            // this should not fail
            Timetable tt = new Timetable(TimetableType.Network);
            var rand = new Random();
            Station branchS = null;
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

                    tt.AddStation(sta, i);
                    
                    if (j == 1 && i > 0)
                        tt.AddRoute(branchS, sta, 0f, 1f);
                    else
                        sta.Positions.SetPosition(i, j);
                }
            }
        }
    }
}