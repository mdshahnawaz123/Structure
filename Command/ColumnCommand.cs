using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Structure.UI;

namespace Structure.Command
{
    [Transaction(TransactionMode.Manual)]
    public class ColumnCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            var columnForm = new ColumnUI(doc, uidoc);
            columnForm.Show();

            //var loginForm = new Loginform();
            //bool? loginResult = loginForm.ShowDialog();

            //if (loginResult == true)
            //{
            //    var columnForm = new ColumnUI(doc, uidoc);
            //    columnForm.Show();
            //}

            return Result.Succeeded;
        }
    }
}