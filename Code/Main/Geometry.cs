using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using AdGeom = Autodesk.AutoCAD.Geometry;
using DynGeom = Autodesk.DesignScript.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DataShortcuts;
using Autodesk.DesignScript.Runtime;
using System.Globalization;
using Autodesk.AutoCAD.BoundaryRepresentation;

namespace Autodesk.Civil3D_CustomNodes
{
    public class Geometry
    {
        private Geometry() { }
        /// <summary>
        /// Set representation for AutoCAD's points (_point). Look https://clck.ru/af4Um to find styles (nums)
        /// </summary>
        /// <param name="doc_dyn"></param>
        /// <param name="Pdmode">Integer type of point</param>
        /// <param name="Psize">Size of point</param>
        public static void SetAcadPointsStyle (int Pdmode = 34, double Psize = 0.2)
        {
            //
            //http://docs.autodesk.com/ACD/2010/ENU/AutoCAD%20.NET%20Developer%27s%20Guide/files/WS1a9193826455f5ff2566ffd511ff6f8c7ca-415b.htm
            //Document doc = doc_dyn.AcDocument;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (DocumentLock acDocLock = doc.LockDocument())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    db.Pdmode = Pdmode;
                    db.Pdsize = Psize;
                    tr.Commit();
                }
            }

        }
        /// <summary>
        /// Create AutoCAD's object-point by Dynamo's point and option include Z-cordinate. Return an object id of item.
        /// </summary>
        /// <param name="doc_dyn"></param>
        /// <param name="Point_position"></param>
        /// <param name="IncludeZ"></param>
        /// <returns></returns>
        public static ObjectId CreateAcadPoint (Autodesk.AutoCAD.DynamoNodes.Document doc_dyn, DynGeom.Point Point_position, bool IncludeZ = true)
        {

            //Document doc = Application.DocumentManager.MdiActiveDocument;
            Document doc = doc_dyn.AcDocument;
            Database db = doc.Database;
            ObjectId point_id = ObjectId.Null;

            using (DocumentLock acDocLock = doc.LockDocument())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable acBlkTbl;
                    acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    double coord_z = Point_position.Z; if (!IncludeZ) coord_z = 0d;
                    DBPoint acPoint = new DBPoint(new Point3d(Point_position.X, Point_position.Y, coord_z));
                    acBlkTblRec.AppendEntity(acPoint);
                    tr.AddNewlyCreatedDBObject(acPoint, true);
                    point_id = acPoint.ObjectId;


                    tr.Commit();
                }
            }
            return point_id;

        }
    }
}
