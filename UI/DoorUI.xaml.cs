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
using Structure.Events;
using Structure.Extension;

namespace Structure.UI
{
    /// <summary>
    /// Interaction logic for DoorUI.xaml
    /// </summary>
    public partial class DoorUI : Window
    {
        private readonly Document doc;
        private readonly UIDocument? uidoc;
        private ImportInstance? selectedImportInstance;
        private readonly DoorHandler handler;
        private readonly ExternalEvent externalEvent;
        public DoorUI(Document doc, UIDocument? uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc = uidoc;
            handler = new DoorHandler();
            externalEvent = ExternalEvent.Create(handler);
            info();
        }

        private void PickCadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sel = uidoc?.Selection.PickObject(ObjectType.Element,new CadSelectionFilter(),"Select Cad Layer");

                selectedImportInstance = doc.GetElement(sel) as ImportInstance;

                if(selectedImportInstance == null)
                {
                    TaskDialog.Show("Error", "No Cad Layer Selected");
                    return;
                }

                var cadgom = new CadGeometry();
                var cadLayer = cadgom.GetCadGeometry(selectedImportInstance);
                if (cadLayer == null)
                {
                    TaskDialog.Show("Error", "No Cad Layer Found");
                    return;
                }
                CBLayer.ItemsSource = cadLayer;

            }
            catch (Exception exception)
            {
                TaskDialog.Show("Error", $"Cad Selection failed {exception.Message}");
            }
        }

        private void CreateDoorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(CBLayer.SelectedItem == null || CBDoorType.SelectedItem == null || CBLevel.SelectedItem == null)
                {
                    TaskDialog.Show("Error", "Please check the valid input");
                    return;
                }

                string? selectedLayer = CBLayer.SelectedItem as string;
                var doorType = CBDoorType.SelectedItem as FamilySymbol;
                var level = CBLevel.SelectedItem as Level;




                handler.doc = doc;
                handler.importInstance = selectedImportInstance;
                handler.layerName = selectedLayer;
                handler.DoorFamilySymbol = doorType;
                handler.RefLevel = level;;
                externalEvent.Raise();

            }
            catch (Exception exception)
            {
                TaskDialog.Show("Error", $"Cad Selection failed {exception.Message}");
            }
        }

        private void ResetFormButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception exception)
            {
                TaskDialog.Show("Error", $"Cad Selection failed {exception.Message}");
            }
        }

        private void Email_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        public void info()
        {
            CBLayer.Items.Clear();
            CBDoorType.Items.Clear();
            CBLevel.Items.Clear();

            var doorType = Extension.DoorType.GetDoorTypes(doc);
            CBDoorType.ItemsSource = doorType;
            CBDoorType.DisplayMemberPath = "Name";
            var levels = Extension.LevelExtension.GetLevels(doc);
            CBLevel.ItemsSource = levels;
            CBLevel.DisplayMemberPath = "Name";


        }

        public class CadSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return elem is ImportInstance;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return true;
            }
        }
    }
}
