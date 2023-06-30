using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace FPLedit.GTFS.GeoProvider;

public class KmlGeoProvider : IGeoProvider
{
    private readonly XmlDocument? kmlRoot;
    private readonly XmlNamespaceManager? nsmgr;

    public KmlGeoProvider(string filename)
    {
        (kmlRoot, nsmgr) = ReadKml(filename);
    }

    private static (XmlDocument?, XmlNamespaceManager?) ReadKml(string filename)
    {
        using var stream = File.Open(filename, FileMode.Open, FileAccess.Read);

        var doc = new XmlDocument { XmlResolver = null! };
        var xmlReaderSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };

        try
        {
            using var reader = XmlReader.Create(new StreamReader(stream), xmlReaderSettings);
            doc.Load(reader);
        }
        catch { return (null, null); }
        
        if (doc.DocumentElement == null || doc.DocumentElement.Name != "kml" || !doc.DocumentElement.NamespaceURI.StartsWith("http://www.opengis.net/kml/"))
            return (null, null);

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("k", doc.DocumentElement.NamespaceURI);

        return (doc, nsmgr);
    }

    public (float lat, float lon)? GetGeoPoint(string stationName)
    {
        if (kmlRoot == null)
            throw new Exception("KML Document not initialized!");

        var nodes = kmlRoot.DocumentElement!.SelectNodes("//k:coordinates/parent::k:Point/parent::k:Placemark/k:name/parent::k:Placemark", nsmgr!);
        if (nodes == null) return null;
        foreach (var node in nodes)
        {
            if (node is not XmlElement el) continue;
            var name = el.SelectSingleNode("k:name", nsmgr!)?.InnerText;
            if (name != null && name == stationName)
            {
                var coords = el.SelectSingleNode("k:Point/k:coordinates", nsmgr!)?.InnerText;
                return CoordsFromKml(coords);
            }
        }
        return null;
    }

    private (float lat, float lon)? CoordsFromKml(string? coords)
    {
        if (string.IsNullOrWhiteSpace(coords)) return null;

        var coordRegex = new Regex(@"^\s*(?<lat>-?\d*\.\d+),(?<lon>-?\d*\.\d+)(?:,-?\d*\.?\d*)?\s*$");
        var coordsMatch = coordRegex.Match(coords);
        if (coordsMatch.Success)
            return (float.Parse(coordsMatch.Groups["lat"].Value, CultureInfo.InvariantCulture), float.Parse(coordsMatch.Groups["lon"].Value, CultureInfo.InvariantCulture));

        return null;
    }

    public (float lat, float lon)[] GetGeoLine(string routeName)
    {
        if (kmlRoot == null)
            throw new Exception("KML Document not initialized!");
        
        var nodes = kmlRoot.DocumentElement!.SelectNodes("//k:coordinates/parent::k:LineString/parent::k:Placemark/k:name/parent::k:Placemark", nsmgr!);
        if (nodes == null) return Array.Empty<(float lat, float lon)>();
        foreach (var node in nodes)
        {
            if (node is not XmlElement el) continue;
            var name = el.SelectSingleNode("k:name", nsmgr!)?.InnerText;
            if (name != null && name == routeName)
            {
                var coords = el.SelectSingleNode("k:LineString/k:coordinates", nsmgr!)?.InnerText;
                if (string.IsNullOrWhiteSpace(coords)) return Array.Empty<(float lat, float lon)>();

                return Regex.Split(coords, @"\s+", RegexOptions.Multiline)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => CoordsFromKml(s) ?? (0f, 0f))
                    .ToArray();
            }
        }
        return Array.Empty<(float lat, float lon)>();
    }
}