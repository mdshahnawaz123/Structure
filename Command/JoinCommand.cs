using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Structure.UI;

namespace ElementJoin
{
    [Transaction(TransactionMode.Manual)]
    public class JoinCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var form = new JoinUI(doc, uidoc);
            form.Show();

            return Result.Succeeded;
        }
    }
}
