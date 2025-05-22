using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Linq;

[Transaction(TransactionMode.Manual)]
public class CmdGridToColumnDimension : IExternalCommand
{
    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        UIDocument uidoc = commandData.Application.ActiveUIDocument;
        Document doc = uidoc.Document;
        View view = doc.ActiveView;

        if (!(view is ViewPlan))
        {
            TaskDialog.Show("Error", "Please run this command in a plan view.");
            return Result.Failed;
        }

        try
        {
            // Select a Grid
            Reference gridRef = uidoc.Selection.PickObject(ObjectType.Element, new GridSelectionFilter(), "Select a Grid");
            Grid grid = doc.GetElement(gridRef) as Grid;

            // Select a Column (FamilyInstance)
            Reference colRef = uidoc.Selection.PickObject(ObjectType.Element, new ColumnSelectionFilter(), "Select a Column");
            FamilyInstance column = doc.GetElement(colRef) as FamilyInstance;

            // Get reference to the grid
            Reference gridReference = new Reference(grid);

            // Get the closest face reference from the column
            Reference columnFaceRef = GetClosestFaceReference(column, grid.Curve.GetEndPoint(0));

            if (columnFaceRef == null)
            {
                TaskDialog.Show("Error", "Unable to find a valid face on the column.");
                return Result.Failed;
            }

            // Create a reference array
            ReferenceArray refArray = new ReferenceArray();
            refArray.Append(gridReference);
            refArray.Append(columnFaceRef);

            // Create dimension line (must be straight and lie on the view plane)
            XYZ p1 = grid.Curve.GetEndPoint(0);
            XYZ p2 = ((LocationPoint)column.Location).Point;
            Line dimLine = Line.CreateBound(p1, p2);

            // Get any available dimension type
            DimensionType dimType = new FilteredElementCollector(doc)
                .OfClass(typeof(DimensionType))
                .Cast<DimensionType>()
                .FirstOrDefault();

            using (Transaction tx = new Transaction(doc, "Create Grid to Column Dimension"))
            {
                tx.Start();
                doc.Create.NewDimension(view, dimLine, refArray, dimType);
                tx.Commit();
            }

            return Result.Succeeded;
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            return Result.Cancelled;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }

    private Reference GetClosestFaceReference(FamilyInstance column, XYZ targetPoint)
    {
        Document doc = column.Document;
        Options options = new Options { ComputeReferences = true, IncludeNonVisibleObjects = false };

        GeometryElement geomElement = column.get_Geometry(options);
        double minDistance = double.MaxValue;
        Reference closestRef = null;

        foreach (GeometryObject geomObj in geomElement)
        {
            if (geomObj is Solid solid)
            {
                foreach (Face face in solid.Faces)
                {
                    BoundingBoxUV bbox = face.GetBoundingBox();
                    UV centerUV = (bbox.Min + bbox.Max) * 0.5;
                    XYZ facePoint = face.Evaluate(centerUV);
                    double distance = facePoint.DistanceTo(targetPoint);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestRef = face.Reference;
                    }
                }
            }
        }

        return closestRef;
    }

    // Filter for Grid selection
    private class GridSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem) => elem is Grid;
        public bool AllowReference(Reference reference, XYZ position) => true;
    }

    // Filter for Column selection
    private class ColumnSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem) =>
            elem is FamilyInstance fi && fi.Category.Id.Value == (int)BuiltInCategory.OST_StructuralColumns;
        public bool AllowReference(Reference reference, XYZ position) => true;
    }
}

