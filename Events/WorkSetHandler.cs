using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Structure.Events
{
    public class WorkSetHandler : IExternalEventHandler
    {
        public Document? doc { get; set; }
        public UIDocument? uidoc { get; set; }
        public List<Category>? Categories { get; set; }
        public List<Element>? ElementTypes { get; set; }
        public List<Workset>? WorkSets { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                if (doc == null || uidoc == null || Categories == null || ElementTypes == null || WorkSets == null)
                {
                    TaskDialog.Show("Error", "Please ensure all inputs are provided.");
                    return;
                }

                var selectedWorkset = WorkSets.FirstOrDefault();
                if (selectedWorkset == null)
                {
                    TaskDialog.Show("Error", "No Workset selected from UI.");
                    return;
                }

                int successCount = 0;

                using (Transaction trans = new Transaction(doc, "Assign Workset"))
                {
                    trans.Start();

                    foreach (var element in ElementTypes)
                    {
                        if (element == null) continue;

                        var worksetParam = element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
                        if (worksetParam != null && !worksetParam.IsReadOnly)
                        {
                            worksetParam.Set(selectedWorkset.Id.IntegerValue); // ✅ Assign by ID
                            successCount++;
                        }
                    }

                    trans.Commit();
                }

                TaskDialog.Show("Workset Assignment",
                    $"{successCount} element(s) assigned to workset: {selectedWorkset.Name}");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Workset assignment failed: {ex.Message}");
            }
        }

        public string GetName() => "WorkSet Handler";
    }
}
