using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.Events
{
    public class BeamHandler : IExternalEventHandler
    {
        public FamilySymbol? familySymbol { get; set; }
        public string? LayerName { get; set; }
        public Level? RefLvl { get; set; }
        public Document? doc { get; set; }
        public ImportInstance? importInstance { get; set; }
        public List<PolyLine>? PLines { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                if (doc == null) doc = app.ActiveUIDocument.Document;

                if (familySymbol == null || RefLvl == null || LayerName == null || PLines == null || PLines.Count == 0)
                {
                    TaskDialog.Show("Error", "Missing data. Cannot proceed.");
                    return;
                }

                int createdCount = 0;

                using (Transaction trans = new Transaction(doc, "Create Beams"))
                {
                    trans.Start();

                    if (!familySymbol.IsActive)
                    {
                        familySymbol.Activate();
                        doc.Regenerate();
                    }

                    foreach (var polyLine in PLines)
                    {
                        var coords = polyLine.GetCoordinates();

                        if (coords == null || coords.Count < 4)
                            continue;

                        if (coords.First().IsAlmostEqualTo(coords.Last()))
                            coords.RemoveAt(coords.Count - 1);

                        if (coords.Count != 4)
                            continue;

                        var edges = new List<(XYZ Start, XYZ End, double Length)>();
                        for (int i = 0; i < coords.Count; i++)
                        {
                            var start = coords[i];
                            var end = coords[(i + 1) % coords.Count];
                            var length = start.DistanceTo(end);
                            edges.Add((start, end, length));
                        }

                        var longestEdge = edges.OrderByDescending(e => e.Length).First();
                        var direction = (longestEdge.End - longestEdge.Start).Normalize();
                        var lengthOfBeam = longestEdge.Length;

                        var shortEdges = edges.OrderBy(e => e.Length).Take(2).ToList();
                        var mid1 = (shortEdges[0].Start + shortEdges[0].End) / 2;
                        var mid2 = (shortEdges[1].Start + shortEdges[1].End) / 2;

                        var center = (mid1 + mid2) / 2;
                        var halfDir = direction.Multiply(lengthOfBeam / 2);
                        var p1 = center - halfDir;
                        var p2 = center + halfDir;

                        var line = Line.CreateBound(p1, p2);
                        doc.Create.NewFamilyInstance(line, familySymbol, RefLvl, StructuralType.Beam);
                        createdCount++;
                    }

                    trans.Commit();
                }

                TaskDialog.Show("Success", $"{createdCount} beam(s) created.");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Handler Error", $"Exception during beam creation: {ex.Message}");
            }
        }

        public string GetName() => "Beam Handler";
    }
}
