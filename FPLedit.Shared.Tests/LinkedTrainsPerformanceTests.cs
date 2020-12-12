using System;
using System.Diagnostics;
using NUnit.Framework;

namespace FPLedit.Shared.Tests
{
    public class LinkedTrainsPerformanceTests
    {
        [Test]
        public void LinkedTrainsPerformanceTest()
        {
            var tt = new Timetable(TimetableType.Linear);
            var stations = new Station[100];
            for (int i = 0; i < 100; i++)
            {
                var sta = new Station(tt);
                sta.SName = "Station " + i;
                sta.Positions.SetPosition(Timetable.LINEAR_ROUTE_ID, i);
                tt.AddStation(sta, Timetable.LINEAR_ROUTE_ID);
                stations[i] = sta;
            }

            var normalTrain = new Train(TrainDirection.ti, tt)
            {
                TName = "P 01"
            };
            tt.AddTrain(normalTrain);
            normalTrain.AddLinearArrDeps();
            for (int i = 0; i < 100; i++)
            {
                var ardp = normalTrain.GetArrDep(stations[i]);
                ardp.Arrival = new TimeEntry(0, i*30);
                ardp.Departure = new TimeEntry(0, i*30 + 10);
            }

            var link = new TrainLink(normalTrain, 10)
            {
                TimeDifference = new TimeEntry(0, 30), 
                TimeOffset = new TimeEntry(0, 0), 
                TrainNamingScheme = new AutoTrainNameCalculator(normalTrain.TName, 2)
            };
            normalTrain.AddLink(link);
            LinkedTrain linkedTrain = null;
            for (int i = 0; i < link.TrainCount; i++)
            {
                linkedTrain = new LinkedTrain(link, i);
                tt.AddTrain(linkedTrain);
            }

            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 100; i++)
            {
                var ardps = linkedTrain.GetArrDepsUnsorted();
            }
            sw.Stop();
            
            Console.WriteLine("Linked read: " + ((float)sw.ElapsedMilliseconds)/100);
            
            var sw2 = new Stopwatch();
            sw2.Start();
            for (int i = 0; i < 100; i++)
            {
                var ardps = normalTrain.GetArrDepsUnsorted();
            }
            sw2.Stop();
            
            Console.WriteLine("Normal read: " + ((float)sw2.ElapsedMilliseconds)/100);
            
            var sw3 = new Stopwatch();
            var sta3 = stations[0];
            var ardp3 = normalTrain.GetArrDep(sta3);
            sw3.Start();
            for (int i = 0; i < 100; i++)
            {
                ardp3.Arrival = new TimeEntry(0, i);
            }
            sw3.Stop();
            
            Console.WriteLine("Write: " + ((float)sw3.ElapsedMilliseconds)/100);
        }
    }
}