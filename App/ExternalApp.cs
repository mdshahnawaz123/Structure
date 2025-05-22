using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace Structure.App
{
    public class ExternalApp : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            TaskDialog.Show("Message","Good By Shahnawaz");
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            


            return Result.Succeeded;
        }
    }
}
