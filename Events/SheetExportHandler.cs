using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace Structure.Events
{
    public class SheetExportHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Sheet Exported in format of Cad and Pdf";
        }
    }
}
