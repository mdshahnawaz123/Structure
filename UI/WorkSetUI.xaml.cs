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

namespace Structure.UI
{
    /// <summary>
    /// Interaction logic for WorkSetUI.xaml
    /// </summary>
    public partial class WorkSetUI : Window
    {
        private Document doc;
        private UIDocument uidoc;

        public WorkSetUI(Document doc,UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;
            info();
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        
        private void AssignBtn(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception exception)
            {
                TaskDialog.Show("Error", $"Failed to Assign WorkSet {exception.Message}");
            }
        }

        private void info()
        {
            CategoryComboBox.Items.Clear();
            ElementTypeComboBox.Items.Clear();
            WorkSetComboBox.Items.Clear();
        }
    }
}
