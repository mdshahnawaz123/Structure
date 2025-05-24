using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Structure.Events
{
    public class PCCHandler : IExternalEventHandler
    {
        private Document doc;
        private IList<PlanarFace> bottomFaces;
        private IList<Element> foundations;
        private FloorType pccFloorType;
        private double offset; // mm input from user

        // Receive data from UI
        public void SetData(Document doc, IList<PlanarFace> faces, IList<Element> elements, FloorType type, double offsetMM)
        {
            this.doc = doc;
            this.bottomFaces = faces;
            this.foundations = elements;
            this.pccFloorType = type;
            this.offset = offsetMM;
        }

        public void Execute(UIApplication app)
        {
            try
            {
                // Convert mm to feet (Revit internal units)
                double offsetFt = offset / 304.8;

                using (Transaction trans = new Transaction(doc, "Create PCC Aligned Slab"))
                {
                    trans.Start();

                    for (int i = 0; i < bottomFaces.Count; i++)
                    {
                        PlanarFace face = bottomFaces[i];
                        Element foundation = foundations[i];

                        // Get fallback level (required for Floor.Create)
                        Level fallbackLevel = new FilteredElementCollector(doc)
                            .OfClass(typeof(Level))
                            .Cast<Level>()
                            .OrderBy(l => Math.Abs(l.Elevation - face.Origin.Z))
                            .FirstOrDefault();

                        if (fallbackLevel == null)
                        {
                            TaskDialog.Show("Error", "No level found in the project.");
                            continue;
                        }

                        // Get boundary loops from the face
                        var originalLoops = face.GetEdgesAsCurveLoops();

                        // Step 1: Apply horizontal offset outward
                        var expandedLoops = originalLoops
                            .Select(loop => CurveLoop.CreateViaOffset(loop, offsetFt, face.FaceNormal))
                            .ToList();

                        // Step 2: Get the true lowest Z value from all edge points (precise face bottom)
                        double faceZ = originalLoops
                            .SelectMany(loop => loop)
                            .SelectMany(c => new[] { c.GetEndPoint(0).Z, c.GetEndPoint(1).Z })
                            .Min();

                        double levelZ = fallbackLevel.Elevation;
                        double deltaZ = faceZ - levelZ;

                        // Step 3: Align the PCC to the bottom of the face
                        Transform alignToFace = Transform.CreateTranslation(new XYZ(0, 0, deltaZ));

                        var alignedLoops = expandedLoops
                            .Select(loop => CurveLoop.Create(loop.Select(c => c.CreateTransformed(alignToFace)).ToList()))
                            .ToList();

                        // Step 4: Create the PCC floor slab
                        Floor.Create(doc, alignedLoops, pccFloorType.Id, fallbackLevel.Id);
                    }

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"PCC creation failed: {ex.Message}");
            }
        }

        public string GetName() => "PCC Creation Handler";
    }
}
