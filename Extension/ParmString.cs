using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace Structure.Extension
{
    public static class ParmString
    {
        /// <summary>
        /// Returns a list of all writable string parameters from a given element.
        /// </summary>
        public static List<string> GetWritableStringParameterNames(this Element element)
        {
            var validParameters = new List<string>();

            if (element == null)
                return validParameters;

            foreach (Parameter param in element.Parameters)
            {
                if (param.Definition != null &&
                    param.StorageType == StorageType.String &&
                    !param.IsReadOnly &&
                    !string.IsNullOrWhiteSpace(param.Definition.Name))
                {
                    validParameters.Add(param.Definition.Name);
                }
            }

            return validParameters.Distinct().OrderBy(p => p).ToList();
        }
    }
}