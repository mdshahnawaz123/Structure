using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Structure.Events
{
    public class FloorHandler : IExternalEventHandler
    {
        public ExternalEvent? externalEvent;

        public Document? doc { get; set; } = null!;
        public FloorType? SlabType { get; set; }
        public string? LayerName { get; set; }
        public Level? ReferenceLevel { get; set; }

        public IList<PolyLine> PolyLines { get; set; } = new List<PolyLine>();

        public void Execute(UIApplication app)
        {
            try
            {
                if (SlabType == null || ReferenceLevel == null || LayerName == null)
                {
                    TaskDialog.Show("Error", "Slab type, reference level, or layer not selected.");
                    return;
                }

                using (Transaction trans = new Transaction(doc, "Create Slabs"))
                {
                    trans.Start();

                    ElementId slabTypeId = SlabType.Id;
                    ElementId levelId = ReferenceLevel.Id;
                    double tolerance = doc.Application.ShortCurveTolerance;

                    int createdCount = 0;

                    foreach (var pLine in PolyLines)
                    {
                        var points = pLine.GetCoordinates();

                        if (points == null || points.Count < 3)
                            continue;

                        var curveLoop = new CurveLoop();
                        int validSegments = 0;

                        for (int i = 0; i < points.Count; i++)
                        {
                            XYZ pt1 = points[i];
                            XYZ pt2 = points[(i + 1) % points.Count];

                            if (pt1.DistanceTo(pt2) < tolerance)
                                continue;

                            Line line = Line.CreateBound(pt1, pt2);
                            curveLoop.Append(line);
                            validSegments++;
                        }

                        if (validSegments < 3)
                        {
                            TaskDialog.Show("Warning", "Polyline has too few valid segments. Skipping.");
                            continue;
                        }

                        try
                        {
                            Floor.Create(doc, new List<CurveLoop> { curveLoop }, slabTypeId, levelId);
                            createdCount++;
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Error", $"Failed to create floor: {ex.Message}");
                        }
                    }

                    trans.Commit();

                    if (createdCount == 0)
                    {
                        TaskDialog.Show("Result", "No floors were created. Check polyline geometry and layers.");
                    }
                    else
                    {
                        TaskDialog.Show("Success", $"{createdCount} floor(s) created successfully.");
                    }
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", $"Slab creation failed: {e.Message}");
            }
        }


        public string GetName()
        {
            return "Slab Created";
        }
    }
}
