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
    public class DoorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            var doorUI = new Structure.UI.DoorUI(doc,uidoc);
            doorUI.Show();

            return Result.Succeeded;
        }
    }
}
