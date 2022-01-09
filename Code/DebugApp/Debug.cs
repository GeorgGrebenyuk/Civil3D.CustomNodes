using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DataShortcuts;
using System.Globalization;
using Autodesk.Civil3D_CustomNodes;

namespace DebugApp
{
    public class Debug
    {
        [CommandMethod("test2")]
        public static void RunTest()
        {
            var dict = Selection.SelectObjectsByConditions(new Dictionary<string, string> { { "0", "3DSOLID" } });
            Console.Write(dict.Count());
            //var dict2 = Solids.GetSolid3dFacesCentroids(dict[0], true);
        }
    }
}
