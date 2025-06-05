using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Structure.Events
{
    public class MarkHandler : IExternalEventHandler
    {
        public List<Category>? Categories { get; set; }
        public Document? doc { get; set; }
        public string? Prefix { get; set; }
        public string? ParameterName { get; set; }

        public void Execute(UIApplication app)
        {
            if (doc == null || Categories == null || string.IsNullOrWhiteSpace(Prefix) || string.IsNullOrWhiteSpace(ParameterName))
            {
                TaskDialog.Show("Error", "Check all inputs.");
                return;
            }

            try
            {
                using (Transaction trans = new Transaction(doc, "Set Tag Parameter"))
                {
                    trans.Start();

                    foreach (var cat in Categories)
                    {
                        var elements = new FilteredElementCollector(doc)
                            .WhereElementIsNotElementType()
                            .OfCategoryId(cat.Id)
                            .ToElements();
                        var sortedElements = elements
                            .Select(el => new
                            {
                                Element = el,
                                Location = GetXY(el)
                            })
                            .Where(xy => xy.Location != null)
                            .OrderBy(xy => xy.Location!.X)
                            .ThenBy(xy => xy.Location!.Y)
                            .ToList();

                        int count = 1;
                        foreach (var item in sortedElements)
                        {
                            var param = item.Element.LookupParameter(ParameterName);
                            if (param != null &&
                                !param.IsReadOnly &&
                                param.StorageType == StorageType.String)
                            {
                                string tagValue = $"{Prefix}{count:D2}";
                                param.Set(tagValue);
                                count++;
                            }
                        }
                    }

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        private XYZ? GetXY(Element el)
        {
            Location? loc = el.Location;

            if (loc is LocationPoint pointLoc)
            {
                return pointLoc.Point;
            }
            else if (loc is LocationCurve curveLoc)
            {
                var curve = curveLoc.Curve;
                if (curve != null)
                {
                    return curve.Evaluate(0.5, true);
                }
            }

            return null;
        }

        public string GetName() => "Mark Handler";
    }
}
