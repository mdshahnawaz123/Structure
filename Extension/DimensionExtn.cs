using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Structure.Extension
{
    public static class DimensionExtn
    {
        public static Dimension CreateDimension(this Document doc, Line line,ReferenceArray referenceArray, DimensionType dimensionType,View activeView)
        {
            var newDim = doc.Create.NewDimension(activeView, line, referenceArray, dimensionType);
            return newDim;
        }
    }
}
