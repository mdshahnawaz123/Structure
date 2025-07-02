using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace Structure.Events
{
    public class ClashEvent : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            };
        }

        public string GetName()
        {
            return "Clash Master File";
        }
    }
}
