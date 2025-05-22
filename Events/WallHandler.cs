using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Structure.Events
{
    public class WallHandler : IExternalEventHandler
    {
        public Document? doc { get; set; }
        public Level? Toplevel { get; set; }
        public Level? Bottomlevel { get; set; }
        public IList<string>? LayerName { get; set; }
        public IList<PolyLine>? Pline { get; set; }
        public WallType? SelectedWallType { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                if (doc == null || Toplevel == null || Bottomlevel == null || Pline == null || SelectedWallType == null)
                {
                    TaskDialog.Show("Error", "Required inputs are missing.");
                    return;
                }

                double height = Toplevel.ProjectElevation - Bottomlevel.ProjectElevation;

                using (Transaction trans = new Transaction(doc, "Create Wall from CAD Rectangle"))
                {
                    trans.Start();

                    foreach (var poly in Pline)
                    {
                        var coords = poly.GetCoordinates();

                        if (coords.Count < 4)
                            continue;

                        // Remove closing point if polyline is closed
                        if (coords.First().IsAlmostEqualTo(coords.Last()))
                            coords.RemoveAt(coords.Count - 1);

                        if (coords.Count != 4)
                            continue; // Skip non-rectangles

                        // Build edges list
                        var edges = new List<(XYZ Start, XYZ End, double Length)>();
                        for (int i = 0; i < coords.Count; i++)
                        {
                            XYZ start = coords[i];
                            XYZ end = coords[(i + 1) % coords.Count];
                            double length = start.DistanceTo(end);
                            edges.Add((start, end, length));
                        }

                        // Get longest edge for direction and length
                        var longestEdge = edges.OrderByDescending(e => e.Length).First();
                        XYZ direction = (longestEdge.End - longestEdge.Start).Normalize();
                        double wallLength = longestEdge.Length;

                        // Get midpoints of short edges
                        var shortEdges = edges.OrderBy(e => e.Length).Take(2).ToList();
                        XYZ shortMid1 = (shortEdges[0].Start + shortEdges[0].End) / 2;
                        XYZ shortMid2 = (shortEdges[1].Start + shortEdges[1].End) / 2;

                        // Center of rectangle across short sides
                        XYZ centerPoint = (shortMid1 + shortMid2) / 2;

                        // Define wall start and end
                        XYZ halfVector = direction.Multiply(wallLength / 2);
                        XYZ wallStart = centerPoint - halfVector;
                        XYZ wallEnd = centerPoint + halfVector;

                        Line centerline = Line.CreateBound(wallStart, wallEnd);

                        var newWall = Wall.Create(doc, centerline, SelectedWallType.Id, Bottomlevel.Id, height, 0, false, true);

                        newWall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).Set(Bottomlevel.Id);
                        newWall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(Toplevel.Id);
                    }

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Wall creation failed: {ex.Message}");
            }
        }

        public string GetName() => "Wall Creation From CAD";
    }
}
