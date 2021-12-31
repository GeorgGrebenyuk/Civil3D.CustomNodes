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
using ds_g = Autodesk.DesignScript.Geometry;
using System.Globalization;
using DynamoRTree;
using dyn = Autodesk.AutoCAD.DynamoNodes;

namespace Autodesk.Civil3D_CustomNodes
{
    public class TopoRecognize
    {
        private TopoRecognize() { }
        private static double GetSideOfPointByLine (Point3d p1, Point3d p2, Point3d p3)
        {
            return (p2.X - p1.X) * (p3.Y - p2.Y) - (p2.Y - p1.Y) * (p3.X - p2.X);
        }
        private static double GetLenByPoints (Point3d p1, Point3d p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
        [MultiReturn(new[] { "LineId", "LineEndPoint"})]
        public static Dictionary <string,object> GetLineIdBetweenLines(Document doc, ds_g.Point pnt, List<ObjectId> all_objects, List<ObjectId> arrow_lines, double SearchRadius = 0.01)
        {
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Point3d pnt_acad = new Point3d(pnt.X, pnt.Y, 0.0);

            List<ObjectId> lines_need_check = new List<ObjectId>();
            ObjectId need_object = ObjectId.Null;
            ds_g.Point EndPoint = null;
            foreach (ObjectId OneLine in all_objects)
            {
                if (!arrow_lines.Contains(OneLine)) lines_need_check.Add(OneLine);
            }
            if (arrow_lines.Count == 2 && all_objects.Count > 2)
            {
                using (DocumentLock acDocLock = doc.LockDocument())
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        Autodesk.AutoCAD.DatabaseServices.Line arrow1 = tr.GetObject(arrow_lines[0], OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.Line;
                        Autodesk.AutoCAD.DatabaseServices.Line arrow2 = tr.GetObject(arrow_lines[1], OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.Line;
                        foreach (ObjectId line_id in lines_need_check)
                        {
                            Autodesk.AutoCAD.DatabaseServices.Line OneObject = tr.GetObject(line_id, OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.Line;
                            Point3d line_sp = OneObject.StartPoint; Point3d line_ep = OneObject.EndPoint;
                            double LineLen = OneObject.Length;

                            Point3d p1 = new Point3d(line_sp.X + SearchRadius / LineLen * (line_ep.X -line_sp.X), line_sp.Y + SearchRadius / LineLen * (line_ep.Y - line_sp.Y), 0.0);
                            Point3d p2 = new Point3d(line_ep.X + SearchRadius / LineLen * (line_sp.X - line_ep.X), line_ep.Y + SearchRadius / LineLen * (line_sp.Y - line_ep.Y), 0.0);
                            Point3d p_middle = new Point3d((line_sp.X + line_ep.X) / 2.0, (line_sp.Y + line_ep.Y) / 2.0, 0.0);
                            Point3d p_start;

                            if (GetLenByPoints(pnt_acad, p1) < SearchRadius * 2)
                            {
                                p_start = p1;
                                EndPoint = ds_g.Point.ByCoordinates(line_ep.X, line_ep.Y, 0);
                            }
                            else 
                            {
                                p_start = p2;
                                EndPoint = ds_g.Point.ByCoordinates(line_sp.X, line_sp.Y, 0);
                            }

                            //if ( GetSideOfPointByLine(arrow1.StartPoint, arrow1.EndPoint, p_start) * GetSideOfPointByLine(arrow2.StartPoint, arrow2.EndPoint, p_start) <0 &&
                            //    GetSideOfPointByLine(arrow2.EndPoint, arrow1.EndPoint, p_start) * GetSideOfPointByLine(arrow2.EndPoint, arrow1.EndPoint, p_middle) < 0)
                            if (IsPointIntoTriangle(arrow1.EndPoint, arrow2.EndPoint, pnt_acad, p_start))
                            {
                                need_object = line_id;
                                break;
                            }
                            bool IsPointIntoTriangle (Point3d t1, Point3d t2, Point3d t3, Point3d c)
                            {
                                double ch1 = (t1.X - c.X) * (t2.Y - t1.Y) - (t2.X - t1.X) * (t1.Y - c.Y);
                                double ch2 = (t2.X - c.X) * (t3.Y - t2.Y) - (t3.X - t2.X) * (t2.Y - c.Y);
                                double ch3 = (t3.X - c.X) * (t1.Y - t3.Y) - (t1.X - t3.X) * (t3.Y - c.Y);

                                if ((ch1 >= 0 && ch2 >= 0 && ch3 >= 0) || (ch1 <= 0 && ch2 <= 0 && ch3 <= 0))
                                    return true;
                                else
                                    return false;
                            }
                        }
                        tr.Commit();
                    }
                }
            }

            return new Dictionary<string, object>
            {
                {"LineId",need_object }, {"LineEndPoint",EndPoint}
            };
        }

    }
}
