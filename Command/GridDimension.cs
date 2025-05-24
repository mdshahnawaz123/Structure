using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Structure.Command
{
    [Transaction(TransactionMode.Manual)]
    public class GridDimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;

            // Collect all grid elements
            List<Grid> grids = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Grids)
                .OfClass(typeof(Grid))
                .Cast<Grid>()
                .ToList();

            if (grids.Count == 0)
            {
                TaskDialog.Show("Error", "No grids found in the Active View.");
                return Result.Failed;
            }

            // Get a linear dimension type
            var dimType = new FilteredElementCollector(doc)
                .OfClass(typeof(DimensionType))
                .Cast<DimensionType>()
                .FirstOrDefault(dt => dt.StyleType == DimensionStyleType.Linear);

            if (dimType == null)
            {
                TaskDialog.Show("Error", "No linear dimension type found.");
                return Result.Failed;
            }

            using (Transaction trans = new Transaction(doc, "Create Grid Dimensions"))
            {
                trans.Start();

                // Separate horizontal and vertical grids
                var horizontalGrids = grids
                    .Where(g => Math.Abs(g.Curve.GetEndPoint(0).Y - g.Curve.GetEndPoint(1).Y) < 0.01)
                    .OrderBy(g => g.Curve.GetEndPoint(0).X)
                    .ToList();

                var verticalGrids = grids
                    .Where(g => Math.Abs(g.Curve.GetEndPoint(0).X - g.Curve.GetEndPoint(1).X) < 0.01)
                    .OrderBy(g => g.Curve.GetEndPoint(0).Y)
                    .ToList();

                // Horizontal dimension (across vertical grids)
                if (horizontalGrids.Count >= 2)
                {
                    ReferenceArray refArrayX = new ReferenceArray();
                    foreach (var g in horizontalGrids)
                        refArrayX.Append(new Reference(g));

                    XYZ p1 = horizontalGrids.First().Curve.GetEndPoint(0);
                    XYZ p2 = horizontalGrids.Last().Curve.GetEndPoint(0);
                    XYZ offset = new XYZ(0, 5, 0); // Y offset

                    Line dimLineX = Line.CreateBound(p1 + offset, p2 + offset);
                    doc.Create.NewDimension(view, dimLineX, refArrayX, dimType);
                }

                // Vertical dimension (across horizontal grids)
                if (verticalGrids.Count >= 2)
                {
                    ReferenceArray refArrayY = new ReferenceArray();
                    foreach (var g in verticalGrids)
                        refArrayY.Append(new Reference(g));

                    XYZ p3 = verticalGrids.First().Curve.GetEndPoint(0);
                    XYZ p4 = verticalGrids.Last().Curve.GetEndPoint(0);
                    XYZ offset = new XYZ(5, 0, 0); // X offset

                    Line dimLineY = Line.CreateBound(p3 + offset, p4 + offset);
                    doc.Create.NewDimension(view, dimLineY, refArrayY, dimType);
                }

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}
