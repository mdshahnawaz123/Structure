using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Structure.Command
{
    [Transaction(TransactionMode.Manual)]
    public class FloorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            var frm = new UI.FloorUI(doc, uidoc);
            frm.Show();
            return Result.Succeeded;
        }
    }
}
