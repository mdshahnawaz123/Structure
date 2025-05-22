using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Structure.Extension
{
    public static class MidPoint
    {
        public static XYZ midPoint( double X1,double X2,double Y1,double Y2,double Z1,double Z2)
        {
            var midPoint = new XYZ((X1 + X2) / 2, (Y1 + Y2) / 2, (Z1 + Z2) / 2);
            return midPoint;
        }
    }
}
