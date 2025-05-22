using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Structure.Extension
{
    public class CadGeometry
    {
        public IList<string> GetCadGeometry(ImportInstance importInstance)
        {
            IList<string> cadGeometry = new List<string>();
            cadGeometry.Clear();

            try
            {

                if (importInstance != null)
                {
                    var geoEle = importInstance.get_Geometry(new Options());

                    var layerNames = geoEle
                        .OfType<GeometryInstance>()
                        .SelectMany(x => x.GetInstanceGeometry())
                        .Where(x => x.GraphicsStyleId != ElementId.InvalidElementId)
                        .Select(x => importInstance.Document.GetElement(x.GraphicsStyleId) as GraphicsStyle)
                        .Where(x => x != null)
                        .Cast<GraphicsStyle>()
                        .Select(x => x.GraphicsStyleCategory)
                        .Where(x => x != null)
                        .Select(x => x.Name)
                        .Distinct()
                        .ToList();
                    foreach (var layerName in layerNames)
                    {
                        cadGeometry.Add(layerName);
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return cadGeometry;
        }

        public IList<Line> GetPline(ImportInstance importInstance, string? selectedLayer)
        {
            IList<Line> pLines = new List<Line>();

            try
            {
                if (importInstance != null)
                {
                    var geoEle = importInstance.get_Geometry(new Options());

                    var geometryObjects = geoEle
                        .OfType<GeometryInstance>()
                        .SelectMany(x => x.GetInstanceGeometry());

                    foreach (var obj in geometryObjects)
                    {
                        if (obj is Line line && obj.GraphicsStyleId != ElementId.InvalidElementId)
                        {
                            var style = importInstance.Document.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                            if (style != null && style.GraphicsStyleCategory != null &&
                                style.GraphicsStyleCategory.Name.Equals(selectedLayer,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                pLines.Add(line);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return pLines;
        }


        public IList<Curve> GetCurves(ImportInstance importInstance)
        {
            IList<Curve> curves = new List<Curve>();
            curves.Clear();
            try
            {
                if (importInstance != null)
                {
                    var geoEle = importInstance.get_Geometry(new Options());
                    var pLine = geoEle
                        .OfType<GeometryInstance>()
                        .SelectMany(x => x.GetInstanceGeometry())
                        .OfType<Curve>().ToList();
                    foreach (var line in pLine)
                    {
                        curves.Add(line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return curves;
        }


        public IList<Arc> GetArc(ImportInstance importInstance)
        {
            IList<Arc> arcs = new List<Arc>();
            arcs.Clear();
            try
            {
                if (importInstance != null)
                {
                    var geoEle = importInstance.get_Geometry(new Options());
                    var pLine = geoEle
                        .OfType<GeometryInstance>()
                        .SelectMany(x => x.GetInstanceGeometry())
                        .OfType<Arc>().ToList();
                    foreach (var line in pLine)
                    {
                        arcs.Add(line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return arcs;
        }

        public IList<PolyLine> GetPolyLine(ImportInstance importInstance, string selectedLayer)
        {
            IList<PolyLine> polyLines = new List<PolyLine>();

            try
            {
                if (importInstance != null)
                {
                    var geoEle = importInstance.get_Geometry(new Options());
                    var geometryObjects = geoEle
                        .OfType<GeometryInstance>()
                        .SelectMany(x => x.GetInstanceGeometry());

                    foreach (var obj in geometryObjects)
                    {
                        if (obj is PolyLine poly && obj.GraphicsStyleId != ElementId.InvalidElementId)
                        {
                            var style = importInstance.Document.GetElement(obj.GraphicsStyleId) as GraphicsStyle;
                            if (style != null &&
                                style.GraphicsStyleCategory != null &&
                                style.GraphicsStyleCategory.Name.Equals(selectedLayer, StringComparison.OrdinalIgnoreCase))
                            {
                                polyLines.Add(poly);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", $"Failed to extract polylines: {e.Message}");
                throw;
            }

            return polyLines;
        }



    }

}
