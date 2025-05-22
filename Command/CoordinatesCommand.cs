using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Structure.UI;

namespace Structure
{
    [Transaction(TransactionMode.Manual)]
    public class CoordinatesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var window = new PileCoordinatesUI(uidoc);
            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}
