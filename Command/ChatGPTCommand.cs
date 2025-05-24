using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Structure.UI;
using System;
using System.Windows.Interop;

namespace Structure.Command
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class ChatGPTCommand : IExternalCommand
    {
        private static ChatGPTWindow _window;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (_window == null || !_window.IsLoaded)
            {
                _window = new ChatGPTWindow();

                IntPtr revitHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                WindowInteropHelper helper = new WindowInteropHelper(_window)
                {
                    Owner = revitHandle
                };

                _window.Topmost = true;
                _window.Topmost = false;
                _window.Show();
            }
            else
            {
                _window.Focus();
            }

            return Result.Succeeded;
        }
    }
}