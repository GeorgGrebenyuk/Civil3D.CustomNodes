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
using Autodesk.DesignScript.Runtime;
using System.Globalization;
namespace Civil3D_CustomNodes
{
    public static class Geometry
    {
        public static Point3d[] ToArray(this Point3dCollection pts)
        {
            var res = new Point3d[pts.Count];
            pts.CopyTo(res, 0);
            return res;
        }
        public static Point3d[] IntersectWith(this Plane p, Curve cur)
        {
            var pts = new Point3dCollection();
            // Get the underlying GeLib curve
            var gcur = cur.GetGeCurve();
            // Project this curve onto our plane
            var proj = gcur.GetProjectedEntity(p, p.Normal) as Curve3d;
            if (proj != null)
            {
                // Create a DB curve from the projected Ge curve
                using (var gcur2 = Curve.CreateFromGeCurve(proj))
                {
                    // Check where it intersects with the original curve:
                    // these should be our intersection points on the plane
                    cur.IntersectWith(
                      gcur2, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero
                    );
                }
            }
            return pts.ToArray();
        }
        ///<summary>
        /// Test whether a point is on this curve.
        ///</summary>
        ///<param name="pt">The point to check against this curve.</param>
        ///<returns>Boolean indicating whether the point is on the curve.</returns>
        public static bool IsOn(this Curve cv, Point3d pt)
        {
            try
            {
                // Return true if operation succeeds
                var p = cv.GetClosestPointTo(pt, false);
                return (p - pt).Length <= Tolerance.Global.EqualPoint;
            }
            catch { }
            // Otherwise we return false
            return false;
        }

        public static void CurvePlaneIntersection(ObjectId ObjectId_OneCurve, ObjectId ObjectId_OnePlSurface)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (null == doc)
                return;
            var db = doc.Database;
            //var ed = doc.EditorAutodesk.AutoCAD.DatabaseServices.

            var curId = ObjectId_OneCurve;

            var planeId = ObjectId_OnePlSurface;
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                // Start by opening the plane
                var plane = tr.GetObject(planeId, OpenMode.ForRead) as PlaneSurface;
                if (plane != null)
                {
                    // Get the PlaneSurface's defining GeLib plane
                    var p = plane.GetPlane();
                    // Open our curve...
                    var cur = tr.GetObject(curId, OpenMode.ForRead) as Curve;
                    if (cur != null) // Should never fail
                    {
                        var pts = p.IntersectWith(cur);
                        // If we have results we'll place them in modelspace
                        var ms =
                          (BlockTableRecord)tr.GetObject(
                            SymbolUtilityServices.GetBlockModelSpaceId(db),
                            OpenMode.ForWrite
                          );
                        foreach (Point3d pt in pts)
                        {
                            // Create a point in the drawing for each intersection
                            var dbp = new DBPoint(pt);
                            dbp.ColorIndex = 2; // Make them yellow
                            ms.AppendEntity(dbp);
                            tr.AddNewlyCreatedDBObject(dbp, true);
                        }
                    }
                }
                tr.Commit();
            }
        }
    }
    
}