namespace FPLedit.GTFS.GTFSLib;

public sealed class Shape
{
    [GtfsField("shape_id", GtfsType.Id)]
    public string ShapeId { get; init; }

    [GtfsField("shape_pt_lat", GtfsType.CoordLat)]
    public float ShapePtLat { get; init; }
    [GtfsField("shape_pt_lon", GtfsType.CoordLon)]
    public float ShapePtLon { get; init; }

    [GtfsField("shape_pt_sequence", GtfsType.UInt)]
    public uint ShapePtSequence { get; init; }
}