using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Structure.Extension;

namespace Structure.Events
{
    public class ColumnHandler : IExternalEventHandler
    {
        public FamilySymbol SelectedColumnType { get; set; } = null!;
        public Level BaseLevel { get; set; } = null!;
        public Level TopLevel { get; set; } = null!;
        public IList<PolyLine> PolyLines { get; set; } = new List<PolyLine>();
        public Document doc { get; set; } = null!;

        public void Execute(UIApplication app)
        {
            try
            {
                if (SelectedColumnType == null || BaseLevel == null || TopLevel == null || PolyLines == null || PolyLines.Count == 0)
                {
                    TaskDialog.Show("Error", "Missing column type, levels, or polylines.");
                    return;
                }

                int columnCount = 0;

                using (Transaction trans = new Transaction(doc, "Create Columns from CAD"))
                {
                    trans.Start();

                    if (!SelectedColumnType.IsActive)
                        SelectedColumnType.Activate();

                    foreach (var pline in PolyLines)
                    {
                        var outline = pline.GetOutline();
                        var mid = MidPoint.midPoint(
                            outline.MinimumPoint.X, outline.MaximumPoint.X,
                            outline.MinimumPoint.Y, outline.MaximumPoint.Y,
                            outline.MinimumPoint.Z, outline.MaximumPoint.Z);

                        var column = doc.Create.NewFamilyInstance(mid, SelectedColumnType, BaseLevel,StructuralType.Column);
                        column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).Set(BaseLevel.Id);
                        column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(TopLevel.Id);
                        columnCount++;
                    }

                    trans.Commit();

                    TaskDialog.Show("Success", $"{columnCount} columns created successfully.");
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.Message);
            }
        }

        public string GetName() => "Create Columns from CAD Layer";
    }
}
