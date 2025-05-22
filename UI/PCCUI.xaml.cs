using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Structure.UI
{
    /// <summary>
    /// Interaction logic for PCCUI.xaml
    /// </summary>
    public partial class PCCUI : Window
    {
        private Document doc;
        private UIDocument uidoc;
        public PCCUI()
        {
            InitializeComponent();
        }

        private void SelectFoundation_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var sel = uidoc.Selection.PickObjects(ObjectType.Element,new FoundationAndSlabSelectionFilter());
                if (sel != null)
                {

                }
                return;

            }
            catch (Exception exception)
            {
                TaskDialog.Show("Error", $"Foundation selection failed {exception.Message}");
                return;
            }
        }

        private void CreatePCC(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private class FoundationAndSlabSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem?.Category == null) return false;

                BuiltInCategory cat = (BuiltInCategory)elem.Category.Id.Value;

                return cat == BuiltInCategory.OST_StructuralFoundation || cat == BuiltInCategory.OST_Floors;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false; // Optional: set true if face picking is needed
            }
        }


    }
}
