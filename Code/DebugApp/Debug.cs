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

using Civil3D_CustomNodes;


namespace DebugApp
{
    public class Debug
    {
        [CommandMethod("test1")]
        public static void RunTest()
        {
            //Selection.SelectGeometryByRectangle("Водосток", 0.58497763, -0.3, 0.7, 1.1, "LINE", true); //0.63, 0.952
            Selection.debug_only();
        }
    }
}
