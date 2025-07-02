using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Structure.Events
{
    public class SharedParamEvent : IExternalEventHandler
    {
        private Document doc;
        private UIDocument uidoc;
        public void Execute(UIApplication app)
        {
            try
            {



                using (var Trans = new Transaction(doc, "Create Shared Parameter"))
                {
                    Trans.Start();


                    Trans.Commit();
                }

            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
            }
        }

        public string GetName()
        {
            return "Shared Parameter Create";
        }
    }
}
