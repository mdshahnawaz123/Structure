using Autodesk.Revit.UI;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Structure.App
{
    public class ExternalApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            var login = new Structure.UI.Loginform();

            bool skipLogin = login.TryAutoLoginAndClose();

            if (!skipLogin)
            {
                // Show login window properly
                bool? result = null;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    result = login.ShowDialog();
                });

                if (result != true)
                {
                    TaskDialog.Show("Login Failed", "Login was cancelled.");
                    return Result.Cancelled;
                }
            }

            // === CREATE RIBBON UI ===
            string tabName = "BIM Digital Design";
            try { application.CreateRibbonTab(tabName); } catch { }

            RibbonPanel cadAutomationPanel = application.CreateRibbonPanel(tabName, "CAD Automation");
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            // === Floor Button ===
            var floor = new PushButtonData("Floor Automation", "Floor", assemblyPath, "Structure.Command.FloorCommand");
            PushButton floorButton = cadAutomationPanel.AddItem(floor) as PushButton;
            floorButton.ToolTip = "Generates a structural floor based on predefined parameters.";
            floorButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Floor.png"));

            // === Grid Button ===
            var grid = new PushButtonData("Grid Automation", "Grid", assemblyPath, "Structure.Command.GridCommand");
            PushButton gridButton = cadAutomationPanel.AddItem(grid) as PushButton;
            gridButton.ToolTip = "Generates a structural grid system.";
            gridButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Grid.png"));

            // === Beam Button ===
            var beam = new PushButtonData("beam Automation", "Beam", assemblyPath, "Structure.Command.BeamCommand");
            PushButton beamButton = cadAutomationPanel.AddItem(beam) as PushButton;
            beamButton.ToolTip = "Generates a structural Beam system.";
            beamButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Beam.png"));

            // === Wall Button ===
            var wall = new PushButtonData("Wall Automation", "Wall", assemblyPath, "Structure.Command.WallCommand");
            PushButton wallButton = cadAutomationPanel.AddItem(wall) as PushButton;
            wallButton.ToolTip = "Generates a structural Wall system.";
            wallButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Wall.png"));

            // === Column Button ===
            var column = new PushButtonData("Column Automation", "Column", assemblyPath, "Structure.Command.ColumnCommand");
            PushButton columnButton = cadAutomationPanel.AddItem(column) as PushButton;
            columnButton.ToolTip = "Places structural columns in the project.";
            columnButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Column.png"));

            cadAutomationPanel.AddSeparator();

            RibbonPanel ModelAutomationPanel = application.CreateRibbonPanel(tabName, "Model Automation");

            var join = new PushButtonData("Join Automation", "Join/Unjoin", assemblyPath, "Structure.Command.JoinCommand");
            PushButton joinButton = ModelAutomationPanel.AddItem(join) as PushButton;
            joinButton.ToolTip = "Joins or unjoins structural elements.";
            joinButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Join.png"));

            var coordinates = new PushButtonData("Coordinates Automation", "Coordinates", assemblyPath, "Structure.Command.CoordinatesCommand");
            PushButton coordinatesButton = ModelAutomationPanel.AddItem(coordinates) as PushButton;
            coordinatesButton.ToolTip = "Generates pile coordinates based on predefined rules.";
            coordinatesButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Coordinate.png"));

            var pcc = new PushButtonData("Pcc Automation", "PCC", assemblyPath, "Structure.Command.PCommand");
            PushButton PButton = ModelAutomationPanel.AddItem(pcc) as PushButton;
            PButton.ToolTip = "Generates Foundation PCC based on predefined rules.";
            PButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/PCC.png"));

            var workset = new PushButtonData("WorkSet Automation", "Workset", assemblyPath, "Structure.Command.WorkSetCommand");
            PushButton WorksetButton = ModelAutomationPanel.AddItem(workset) as PushButton;
            WorksetButton.ToolTip = "Assign WorkSet based on predefined rules.";
            WorksetButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/WorkSet.png"));

            // === Door Stiffener Button ===
            var DoorStiffener = new PushButtonData("Door Stiffener Automation", "Stiffener", assemblyPath, "Structure.Command.DoorStiffenerCommand");
            PushButton DoorButton = ModelAutomationPanel.AddItem(DoorStiffener) as PushButton;
            DoorButton.ToolTip = "Create Door Stiffener as per predefined rules.";
            DoorButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/DoorStiffner.png"));

            var chatGpt = new PushButtonData("ChatGPT AI", "ChatGPT", assemblyPath, "Structure.Command.ChatGPTCommand");
            PushButton ChatButton = ModelAutomationPanel.AddItem(chatGpt) as PushButton;
            ChatButton.ToolTip = "Chat With your AI.";
            ChatButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/AI.png"));

            RibbonPanel DimensionPanel = application.CreateRibbonPanel(tabName, "Dimension Automation");

            var GDimension = new PushButtonData("Grid Dimension Automation", "Dim-Grid", assemblyPath, "Structure.Command.GridDimension");
            PushButton GButton = DimensionPanel.AddItem(GDimension) as PushButton;
            GButton.ToolTip = "Create Grid to Grid Dimension as per predefined rules.";
            GButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Dim.png"));

            var CDimension = new PushButtonData("Column Dimension Automation", "Dim-Column", assemblyPath, "Structure.Command.ColumnDimension");
            PushButton CButton = DimensionPanel.AddItem(CDimension) as PushButton;
            CButton.ToolTip = "Create Column to Grid Dimension as per predefined rules.";
            CButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Dim.png"));

            RibbonPanel ParameterPanel = application.CreateRibbonPanel(tabName, "Parameter Automation");

            var markparm = new PushButtonData("Mark Automation", "Mark", assemblyPath, "Structure.Command.MarkCommand");
            PushButton MarkButton = ParameterPanel.AddItem(markparm) as PushButton;
            MarkButton.ToolTip = "Create Grid to Grid Dimension as per predefined rules.";
            MarkButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Mark.png"));

            RibbonPanel DataPanel = application.CreateRibbonPanel(tabName, "Data Visualizer");

            var data = new PushButtonData("Foundation Data Visualization", "Foundation", assemblyPath, "Structure.Command.FoundationData");
            PushButton dataButton = DataPanel.AddItem(data) as PushButton;
            dataButton.ToolTip = "Visualize the Data as per predefined rules.";
            dataButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Data.png"));

            RibbonPanel DataExportPanel = application.CreateRibbonPanel(tabName, "Data Export");

            var Exportdata = new PushButtonData("Export Data to Excel", "Export", assemblyPath, "Structure.Command.FoundationData");
            PushButton dataExportButton = DataExportPanel.AddItem(Exportdata) as PushButton;
            dataExportButton.ToolTip = "Export data as per predefined rules.";
            dataExportButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/XML.png"));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            TaskDialog.Show("Message", "See you Next Time");
            return Result.Succeeded;
        }
    }
}
