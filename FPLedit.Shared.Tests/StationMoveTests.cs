using System;
using System.Linq;
using System.Xml.Linq;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Helpers;
using FPLedit.Shared.Tests.TestClasses;
using FPLedit.Tests.Common;
using NUnit.Framework;

namespace FPLedit.Shared.Tests;

public sealed class StationMoveTests : BaseFileTests
{
    private (Timetable tt, Station a, Station b, Station c, Station d) SetupLinearTestTimetable()
    {
        var tt = new Timetable(TimetableType.Linear);
        var staA = new Station(tt) { SName = "A" };
        staA.Positions.SetPosition(Timetable.LINEAR_ROUTE_ID, 1);
        tt.AddStation(staA, Timetable.LINEAR_ROUTE_ID);
        var staB = new Station(tt) { SName = "B" };
        staB.Positions.SetPosition(Timetable.LINEAR_ROUTE_ID, 10);
        tt.AddStation(staB, Timetable.LINEAR_ROUTE_ID);
        var staC = new Station(tt) { SName = "C" };
        staC.Positions.SetPosition(Timetable.LINEAR_ROUTE_ID, 12);
        tt.AddStation(staC, Timetable.LINEAR_ROUTE_ID);
        var staD = new Station(tt) { SName = "D" };
        staD.Positions.SetPosition(Timetable.LINEAR_ROUTE_ID, 15);
        tt.AddStation(staD, Timetable.LINEAR_ROUTE_ID);
        
        return (tt, staA, staB, staC, staD);
    }

    /**
     * Testcase for the station moving issue fixed in version 2.6.4. */
    [Test]
    public void LinearMoveTests()
    {
        var (tt, a, b, c, d) = SetupLinearTestTimetable();

        var sElm = tt.XMLEntity.Children.Single(x => x.XName == "stations");
        Assert.AreEqual(sElm.Children.Count, tt.Stations.Count);
        CollectionAssert.AreEqual(sElm.Children, new [] {a.XMLEntity, b.XMLEntity, c.XMLEntity, d.XMLEntity});

        Assert.IsTrue(StationMoveHelper.TrySafeMove(c, true, 8, Timetable.LINEAR_ROUTE_ID));
        CollectionAssert.AreEqual(sElm.Children, new [] {a.XMLEntity, c.XMLEntity, b.XMLEntity, d.XMLEntity});
    }
}