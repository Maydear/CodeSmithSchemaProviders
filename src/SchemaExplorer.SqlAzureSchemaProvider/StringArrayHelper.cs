using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaExplorer
{
    internal static class StringArrayHelper
    {
        internal static bool IsAnyNullOrEmpty(params string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return false;
            }
            for (int i = 0; i < values.Length; i++)
            {
                if (string.IsNullOrEmpty(values[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
