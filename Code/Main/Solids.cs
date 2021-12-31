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
using static Autodesk.Civil.DataShortcuts.DataShortcuts.DataShortcutManager;
using Autodesk.DesignScript.Runtime;
using System.Globalization;
using Autodesk.AutoCAD.BoundaryRepresentation;

//using Autodesk.AutoCAD.Interop.Common;

namespace Autodesk.Civil3D_CustomNodes
{
   public class Solids
    {
        private Solids() { }
        private static Point3d GetCentroidByFace (List<Point3d> faces_points)
        {
            return new Point3d((faces_points[0].X + faces_points[1].X + faces_points[2].X) / 3,
                                (faces_points[0].Y + faces_points[1].Y + faces_points[2].Y) / 3,
                                (faces_points[0].Z + faces_points[1].Z + faces_points[2].Z) / 3);
        }
        /// <summary>
        /// Convert Dynamo's point to string with accuracy
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static string GetPointStringRepresentation (Point3d point, int Precision)
        {
            return $"{CoordCut(point.X)}_{CoordCut(point.Y)}_{CoordCut(point.Z)}";
            double CoordCut (double coord)
            {
                return Math.Round(coord, Precision);
            }
        }
        [MultiReturn(new[] { "FacesId", "FacesCentroid" })]
        public static Dictionary<string, object> GetSolid3dFacesCentroids (Autodesk.AutoCAD.DynamoNodes.Document doc_dyn, ObjectId solid_id, bool CreatePoints = false) //List <Dictionary<string, object>>
        {

            //Document doc = Application.DocumentManager.MdiActiveDocument;
            Document doc = doc_dyn.AcDocument;
            Database db = doc.Database;

            //List<DynGeom.Point> FacesCentroids = new List<DynGeom.Point>();
            //List<Dictionary<string, object>> FacesInfo = new List<Dictionary<string, object>>();
            Dictionary<string, object> FacesInfo = new Dictionary<string, object>();
            List<string> FacesId = new List<string>();
            List< DynGeom.Point> FacesCentroid = new List<DynGeom.Point>();
            using (DocumentLock acDocLock = doc.LockDocument())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    //BlockTable acBlkTbl;
                    //acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                    //// Open the Block table record Model space for write
                    //BlockTableRecord acBlkTblRec;
                    //acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    FullSubentityPath path = new FullSubentityPath(new ObjectId[1] { solid_id }, new SubentityId(SubentityType.Null, IntPtr.Zero));
                    
                    Autodesk.AutoCAD.DatabaseServices.Solid3d OneSolid = tr.GetObject(solid_id, OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.Solid3d;
                    //Acad3DSolid OneSolid2 = (Acad3DSolid)OneSolid.AcadObject;
                    using (Brep brep = new Brep(path))
                    {
                        foreach (Autodesk.AutoCAD.BoundaryRepresentation.Face face in brep.Faces)
                        {
                            LoopVertexCollection faces_vertexes = null;
                            try
                            {
                                faces_vertexes = face.Loops.First().Vertices;
                            }
                            catch (Autodesk.AutoCAD.BoundaryRepresentation.Exception ex)
                            {
                                doc.Editor.WriteMessage("Error " + ex);
                            }
                            
                            if (faces_vertexes!= null && faces_vertexes.Count() == 3)
                            {
                                List<Point3d> faces_points_temp = new List<Point3d>();

                                foreach (Autodesk.AutoCAD.BoundaryRepresentation.Vertex OneFaceVertex in faces_vertexes)
                                {
                                    faces_points_temp.Add(OneFaceVertex.Point);
                                }

                                List<Point3d> faces_points = new List<Point3d>();
                                List<string> temp_points_str = new List<string>();
                                foreach (Point3d OnePnt in faces_points_temp)
                                {
                                    string temp_point_str = GetPointStringRepresentation(OnePnt, 4);
                                    if (!temp_points_str.Contains(temp_point_str))
                                    {
                                        faces_points.Add(OnePnt);
                                        temp_points_str.Add(temp_point_str);
                                    }
                                }
                                if (faces_points.Count == 3)
                                {
                                    Point3d face_centroid = GetCentroidByFace(faces_points);
                                    string centroid_str = GetPointStringRepresentation(face_centroid, 3);
                                    if (!FacesId.Contains(centroid_str))
                                    {
                                        FacesId.Add(centroid_str);
                                        FacesCentroid.Add(DynGeom.Point.ByCoordinates(face_centroid.X, face_centroid.Y, face_centroid.Z));
                                    }

                                }
                            }
 

                        }

                        //Autodesk.Civil3D_CustomNodes.Geometry.SetAcadPointsStyle(0, 1);
                    }
                    tr.Commit();
                }
            }
            FacesInfo.Add("FacesId", FacesId); FacesInfo.Add("FacesCentroid", FacesCentroid);
            return FacesInfo;
        }
        private struct FacesProps
        {
            public SubentityId id;
            public AutoCAD.Colors.Color color;
            public ObjectId material;
        }
        public static void SetMaterialByFacesCentroids (Autodesk.AutoCAD.DynamoNodes.Document doc_dyn, ObjectId solid_id, Dictionary <string,string> faces_info, 
            Dictionary <string, ObjectId> MaterialsByName, Dictionary<string, Autodesk.AutoCAD.Colors.Color> ColorsByNames, bool UseColors = false)
        {
            Document doc = doc_dyn.AcDocument;
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (DocumentLock acDocLock = doc.LockDocument())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    FullSubentityPath path = new FullSubentityPath(new ObjectId[1] { solid_id }, new SubentityId(SubentityType.Null, IntPtr.Zero));
                    Autodesk.AutoCAD.DatabaseServices.Solid3d OneSolid = tr.GetObject(solid_id, OpenMode.ForWrite) as Autodesk.AutoCAD.DatabaseServices.Solid3d;

                    //Временные коллекции
                    List<FacesProps> faces = new List<FacesProps>();
                    using (Brep brep = new Brep(path))
                    {
                        foreach (Autodesk.AutoCAD.BoundaryRepresentation.Face face in brep.Faces)
                        {
                            LoopVertexCollection faces_vertexes = null;
                            SubentityId face_id = face.SubentityPath.SubentId;
                            try
                            {
                                faces_vertexes = face.Loops.First().Vertices;
                            }
                            catch (Autodesk.AutoCAD.BoundaryRepresentation.Exception ex)
                            {
                                doc.Editor.WriteMessage("Error " + ex);
                            }

                            if (faces_vertexes != null && faces_vertexes.Count() == 3)
                            {
                                List<Point3d> faces_points_temp = new List<Point3d>();

                                foreach (Autodesk.AutoCAD.BoundaryRepresentation.Vertex OneFaceVertex in faces_vertexes)
                                {
                                    faces_points_temp.Add(OneFaceVertex.Point);
                                }

                                List<Point3d> faces_points = new List<Point3d>();
                                List<string> temp_points_str = new List<string>();
                                foreach (Point3d OnePnt in faces_points_temp)
                                {
                                    string temp_point_str = GetPointStringRepresentation(OnePnt, 4);
                                    if (!temp_points_str.Contains(temp_point_str))
                                    {
                                        faces_points.Add(OnePnt);
                                        temp_points_str.Add(temp_point_str);
                                    }
                                }
                                if (faces_points.Count == 3)
                                {
                                    Point3d face_centroid = GetCentroidByFace(faces_points);
                                    string centroid_str = GetPointStringRepresentation(face_centroid, 3);

                                    var face_info_material_name = faces_info[centroid_str];

                                    FacesProps face_data = new FacesProps();
                                    if (UseColors == true)
                                    {
                                        //OneSolid.SetSubentityColor(face_id, ColorsByNames[face_info_material_name]);
                                        face_data.color = ColorsByNames[face_info_material_name];
                                    }
                                    else
                                    {
                                        //ObjectId material_id = MaterialsByName[face_info_material_name];
                                        //OneSolid.SetSubentityMaterial(face_id, material_id);
                                        face_data.material = MaterialsByName[face_info_material_name];
                                    }
                                    
                                    face_data.id = face_id;
                                    faces.Add(face_data);

                                    

                                }
                            }
                            face.Dispose();
                        }
                        //Autodesk.Civil3D_CustomNodes.Geometry.SetAcadPointsStyle(0, 1);
                    }
                    if (UseColors == true)
                    {
                        foreach (FacesProps face_one in faces)
                        {
                            OneSolid.SetSubentityColor(face_one.id, face_one.color);
                            doc.Editor.WriteMessage($"Color {face_one.color} was assigned for {face_one.id}");
                        }
                    }
                    else
                    {
                        foreach (FacesProps face_one in faces)
                        {
                            OneSolid.SetSubentityMaterial(face_one.id, face_one.material);
                            doc.Editor.WriteMessage($"Material was assigned for {face_one.id}");
                        }
                    }

                    tr.Commit();
                }
            }
        }
    }
}
