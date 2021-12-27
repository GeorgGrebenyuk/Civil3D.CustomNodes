using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3D_CustomNodes
{
    public class Other
    {
        private Other () { }
        /// <summary>
        /// .NET realisation of Dictinary (allow use as keys not only sting's data)
        /// </summary>
        /// <param name="Dict_keys"></param>
        /// <param name="Dict_values"></param>
        /// <returns>Working dictionary by input data</returns>
        private static Dictionary<object, object> GetDictionaryByConditions (List<object> Dict_keys, List<object> Dict_values)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>();
            for (int i1 = 0; i1 < Dict_keys.Count; i1++)
            {
                if (!dict.ContainsKey(Dict_keys[i1])) dict.Add(Dict_keys[i1], Dict_values[i1]);
            }
            return dict;
        }

    }
}
