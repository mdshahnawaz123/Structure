using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Structure.Extension;

namespace Structure.Events
{
    public class DoorHandler : IExternalEventHandler
    {
        public Document? doc { get; set; }

        public ImportInstance? importInstance { get; set; }

        public string? layerName { get; set; }

        public FamilySymbol? DoorFamilySymbol { get; set; }

        public Level? RefLevel { get; set; }

        public IList<PolyLine>? polyLine { get; set; } = new List<PolyLine>();

        public void Execute(UIApplication app)
        {
            try
            {
                if(doc == null || importInstance == null || layerName == null || DoorFamilySymbol == null || RefLevel == null || polyLine == null)
                {
                    TaskDialog.Show("Door Handler Error", "Door Creation Failed");
                    return;
                }

                using (Transaction Trans = new Transaction(doc,"Create Door"))
                {
                    Trans.Start();

                    if(!DoorFamilySymbol.IsActive)
                        DoorFamilySymbol.Activate();

                    foreach (var pline in polyLine)
                    {
                        var coord = pline.GetOutline();
                        var mid = MidPoint.midPoint(
                            coord.MinimumPoint.X, coord.MaximumPoint.X,
                            coord.MinimumPoint.Y, coord.MaximumPoint.Y,
                            coord.MinimumPoint.Z, coord.MaximumPoint.Z);

                        var door = doc.Create.NewFamilyInstance(mid, DoorFamilySymbol, RefLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    }



                    Trans.Commit();
                }

            }
            catch (Exception e)
            {
                TaskDialog.Show("Door Handler Error", $"Door Creation Failed {e.Message}");
            }
            ;
        }

        public string GetName()
        {
            return "Door Created";
        }
    }
}
