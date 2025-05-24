using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;

namespace Structure.App
{
    public class ExternalApp : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            TaskDialog.Show("Message","See you Next Time");
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "BIM Digital Design";
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch
            {
                // Tab already exists
            }

            // Create a ribbon panel
            RibbonPanel cadAutomationPanel = application.CreateRibbonPanel(tabName, "CAD Automation");

            // Get the path to the current assembly
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

            // === Join/Unjoin Button ===
            var join = new PushButtonData("Join Automation", "Join/Unjoin", assemblyPath, "Structure.Command.JoinCommand");
            PushButton joinButton = ModelAutomationPanel.AddItem(join) as PushButton;
            joinButton.ToolTip = "Joins or unjoins structural elements.";
            joinButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Join.png"));


            // === Coordinates Button ===
            var coordinates = new PushButtonData("Coordinates Automation", "Coordinates", assemblyPath, "Structure.Command.CoordinatesCommand");
            PushButton coordinatesButton = ModelAutomationPanel.AddItem(coordinates) as PushButton;
            coordinatesButton.ToolTip = "Generates pile coordinates based on predefined rules.";
            coordinatesButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Coordinate.png"));


            // === PCC Button ===
            var pcc = new PushButtonData("Pcc Automation", "PCC", assemblyPath, "Structure.Command.PCommand");
            PushButton PButton = ModelAutomationPanel.AddItem(pcc) as PushButton;
            PButton.ToolTip = "Generates Foundation PCC based on predefined rules.";
            PButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/PCC.png"));


            // === Workset Button ===
            var workset = new PushButtonData("WorkSet Automation", "Workset", assemblyPath, "Structure.Command.WorkSetCommand");
            PushButton WorksetButton = ModelAutomationPanel.AddItem(workset) as PushButton;
            WorksetButton.ToolTip = "Assign WorkSet based on predefined rules.";
            WorksetButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/WorkSet.png"));
            ModelAutomationPanel.AddSeparator();


            var DimensionPanel = application.CreateRibbonPanel(tabName, "Dimension Automation");

            // === Grid Dimension ===
            var GDimension = new PushButtonData("Grid Dimension Automation", "Dim-Grid", assemblyPath, "Structure.Command.GridDimension");
            PushButton GButton = DimensionPanel.AddItem(GDimension) as PushButton;
            GButton.ToolTip = "Create Grid to Grid Dimension as per predefined rules.";
            GButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Dim.png"));


            // === Column Dimension ===
            var CDimension = new PushButtonData("Column Dimension Automation", "Dim-Column", assemblyPath, "Structure.Command.ColumnDimension");
            PushButton CButton = DimensionPanel.AddItem(CDimension) as PushButton;
            CButton.ToolTip = "Create Column to Grid Dimension as per predefined rules.";
            CButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Dim.png"));

            // === ChatGPT ===
            var chatGpt = new PushButtonData("ChatGPT AI", "ChatGPT", assemblyPath, "Structure.Command.ChatGPTCommand");
            PushButton ChatButton = ModelAutomationPanel.AddItem(chatGpt) as PushButton;
            ChatButton.ToolTip = "Chat With your AI.";
            ChatButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/AI.png"));


            RibbonPanel ParameterPanel = application.CreateRibbonPanel(tabName, "Parameter Automation");

            // === Mark Command ===
            var markparm = new PushButtonData("Mark Automation", "Mark", assemblyPath, "Structure.Command.MarkCommand");
            PushButton MarkButton = ParameterPanel.AddItem(markparm) as PushButton;
            MarkButton.ToolTip = "Create Grid to Grid Dimension as per predefined rules.";
            MarkButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Mark.png"));







            RibbonPanel DataPanel = application.CreateRibbonPanel(tabName, "Data Visualizer");

            // === Data Panel ===
            var data = new PushButtonData("Foundation Data Visualization", "Foundation", assemblyPath, "Structure.Command.FoundationData");
            PushButton dataButton = DataPanel.AddItem(data) as PushButton;
            data.ToolTip = "Visualize the Data as per predefined rules.";
            data.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/Data.png"));




            RibbonPanel DataExportPanel = application.CreateRibbonPanel(tabName, "Data Export");

            var Exportdata = new PushButtonData("Export Data to Excel", "Export", assemblyPath, "Structure.Command.FoundationData");
            PushButton dataExportButton = DataExportPanel.AddItem(Exportdata) as PushButton;
            dataExportButton.ToolTip = "Export data as per predefined rules.";
            dataExportButton.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Structure;component/Resources/XML.png"));






            return Result.Succeeded;
        }
    }
}
