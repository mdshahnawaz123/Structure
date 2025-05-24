using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Structure.Command
{
    [Transaction(TransactionMode.Manual)]
    public class ColumnDimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;

            if (view.ViewType != ViewType.FloorPlan && view.ViewType != ViewType.CeilingPlan)
            {
                TaskDialog.Show("Error", "This command works only in floor or ceiling plan views.");
                return Result.Failed;
            }

            var columns = new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();

            var grids = new FilteredElementCollector(doc)
                .OfClass(typeof(Grid))
                .Cast<Grid>()
                .ToList();

            if (columns.Count == 0 || grids.Count == 0)
            {
                TaskDialog.Show("Error", "No columns or grids found in the active view.");
                return Result.Failed;
            }

            var dimType = new FilteredElementCollector(doc)
                .OfClass(typeof(DimensionType))
                .Cast<DimensionType>()
                .FirstOrDefault(dt => dt.StyleType == DimensionStyleType.Linear);

            if (dimType == null)
            {
                TaskDialog.Show("Error", "No linear dimension type found.");
                return Result.Failed;
            }

            using (Transaction trans = new Transaction(doc, "Dimension Columns to Grids"))
            {
                trans.Start();

                foreach (var column in columns)
                {
                    LocationPoint location = column.Location as LocationPoint;
                    if (location == null) continue;

                    XYZ colPoint = location.Point;

                    var nearestGrid = grids
                        .OrderBy(g => g.Curve.Distance(colPoint))
                        .FirstOrDefault();

                    if (nearestGrid == null) continue;

                    // Get the reference from a planar face of the column
                    Options geomOptions = new Options { ComputeReferences = true, IncludeNonVisibleObjects = true };
                    GeometryElement geomElem = column.get_Geometry(geomOptions);

                    var columnRef = geomElem
                        .OfType<Solid>()
                        .SelectMany(s => s.Faces.Cast<Face>())
                        .OfType<PlanarFace>()
                        .Where(f => Math.Abs(f.FaceNormal.Z) < 0.1)
                        .Select(f => f.Reference)
                        .FirstOrDefault();

                    if (columnRef == null) continue;

                    // Use the grid's curve to create a reference
                    Curve gridCurve = nearestGrid.Curve;
                    Reference gridRef = null;

                    // Use the GeometryObject of the grid to get a reference from its curve
                    GeometryElement gridGeom = nearestGrid.get_Geometry(geomOptions);
                    foreach (GeometryObject gObj in gridGeom)
                    {
                        if (gObj is Line line && gridCurve is Line refLine &&
                            line.GetEndPoint(0).IsAlmostEqualTo(refLine.GetEndPoint(0)) &&
                            line.GetEndPoint(1).IsAlmostEqualTo(refLine.GetEndPoint(1)))
                        {
                            gridRef = line.Reference;
                            break;
                        }
                    }

                    if (gridRef == null) continue;

                    ReferenceArray refArray = new ReferenceArray();
                    refArray.Append(columnRef);
                    refArray.Append(gridRef);

                    // Create a dimension line that lies in the same plane
                    XYZ gridDir = gridCurve.GetEndPoint(1) - gridCurve.GetEndPoint(0);
                    XYZ offsetDir = gridDir.CrossProduct(XYZ.BasisZ).Normalize();
                    XYZ dimLineOrigin = colPoint + offsetDir.Multiply(3.0);

                    Line dimLine = Line.CreateBound(
                        dimLineOrigin - offsetDir.Multiply(5),
                        dimLineOrigin + offsetDir.Multiply(5)
                    );

                    doc.Create.NewDimension(view, dimLine, refArray, dimType);
                }

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}