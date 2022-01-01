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
        private static Point3d GetCentroidByFace (List<Point3d> faces_points, int FaceType)
        {
            Point3d centroid = Point3d.Origin;
            double p_x = 0d; double p_y = 0d; double p_z = 0d;
            foreach (Point3d p in faces_points)
            {
                p_x+=p.X; p_y += p.Y; p_z += p.Z;
            }
            return new Point3d(p_x / FaceType, p_y / FaceType, p_z / FaceType);
        }
        /// <summary>
        /// Convert AutoCAD's Point3d point to string with accuracy
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private static string GetPointStringRepresentation (Point3d point, int Precision)
        {
            return $"{CoordCut(point.X)}_{CoordCut(point.Y)}_{CoordCut(point.Z)}";
            double CoordCut (double coord)
            {
                return Math.Round(coord, Precision);
            }
        }
        /// <summary>
        /// Get all centroids by faces of solid3d as Dictionary with faces's centroid coordinates (Dynamo point) and identificators (solid's handle and face's id)
        /// </summary>
        /// <param name="doc_dyn">Currnt document</param>
        /// <param name="solids_id_list">List with solid3d's ObjectId</param>
        /// <param name="FaceType">Count edges in face; as default =3</param>
        /// <returns>Dictionary with faces's centroid coordinates (Dynamo point) and identificators (solid's handle and face's id)</returns>
        [MultiReturn(new[] { "FacesId", "FacesCentroid" })]
        public static Dictionary<string, object> GetSolid3dFacesCentroids (Autodesk.AutoCAD.DynamoNodes.Document doc_dyn, List<ObjectId> solids_id_list, int FaceType = 3) //List <Dictionary<string, object>>
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
                    foreach (ObjectId OneSolidId in solids_id_list)
                    {
                        FullSubentityPath path = new FullSubentityPath(new ObjectId[1] { OneSolidId }, new SubentityId(SubentityType.Null, IntPtr.Zero));

                        using (Brep brep = new Brep(path))
                        {
                            //List<Autodesk.AutoCAD.BoundaryRepresentation.Face> solid_faces = brep.Faces.ToList();
                            //for (int i1 = 0; i1 < solid_faces.Count; i1++)
                            foreach (Autodesk.AutoCAD.BoundaryRepresentation.Face face in brep.Faces)
                            {
                                //Autodesk.AutoCAD.BoundaryRepresentation.Face face = solid_faces[i1];
                                string Face_Id = $"{OneSolidId.Handle}_{face.SubentityPath.SubentId.IndexPtr.ToInt64()}";
                                //string Face_Id = $"{OneSolidId.Handle}_{i1}";
                                LoopVertexCollection faces_vertexes = null;
                                try
                                {
                                    faces_vertexes = face.Loops.First().Vertices;
                                }
                                catch (Autodesk.AutoCAD.BoundaryRepresentation.Exception ex)
                                {
                                    doc.Editor.WriteMessage("Error " + ex);
                                }

                                if (faces_vertexes != null && faces_vertexes.Count() == FaceType)
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
                                    if (faces_points.Count == FaceType)
                                    {
                                        Point3d face_centroid = GetCentroidByFace(faces_points, FaceType);
                                        string centroid_str = GetPointStringRepresentation(face_centroid, 3);
                                        if (!FacesId.Contains(Face_Id))
                                        {
                                            FacesId.Add(Face_Id);
                                            FacesCentroid.Add(DynGeom.Point.ByCoordinates(face_centroid.X, face_centroid.Y, face_centroid.Z));
                                        }
                                    }
                                }
                                //face.Dispose();
                            }
                        }
                    }                  
                    tr.Commit();
                }
            }
            FacesInfo.Add("FacesId", FacesId); FacesInfo.Add("FacesCentroid", FacesCentroid);
            return FacesInfo;
        }
        /// <summary>
        /// Struct for keep data about face in nethod SetMaterialByFacesCentroids
        /// </summary>
        private struct FacesProps
        {
            public SubentityId id;
            public AutoCAD.Colors.Color color;
            public ObjectId material;
        }

        //[Obsolete]
        /// <summary>
        /// Assign to each face by it's identificator a color or materials by boolean. 
        /// </summary>
        /// <param name="doc_dyn">Current document</param>
        /// <param name="solids_id_list">List with solid3d ObjectId</param>
        /// <param name="faces_info_list">List with Dictionaries for each solid (created in Python script).</param>
        /// <param name="MaterialsByName">Dictionary with materials for names</param>
        /// <param name="ColorsByNames">Dictionary with colors for names</param>
        /// <param name="UseColors">If true - using colors; else -- materials</param>
        public static void SetMaterialByFacesCentroids (Autodesk.AutoCAD.DynamoNodes.Document doc_dyn, List<ObjectId> solids_id_list, List<Dictionary <string,string>> faces_info_list, 
            Dictionary <string, ObjectId> MaterialsByName, Dictionary<string, Autodesk.AutoCAD.Colors.Color> ColorsByNames, bool UseColors = false)
        {
            Document doc = doc_dyn.AcDocument;
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            for (int i2 = 0; i2 < solids_id_list.Count; i2++)
            {
                ObjectId OneSolidId = solids_id_list[i2];
                Dictionary<string, string> faces_info = faces_info_list[i2];
                

                using (Solid3d OneSolid = OneSolidId.Open(OpenMode.ForWrite) as Solid3d)
                {
                    FullSubentityPath path = new FullSubentityPath(new ObjectId[1] { OneSolidId }, new SubentityId(SubentityType.Null, IntPtr.Zero));
                    List<FacesProps> faces = new List<FacesProps>();
                    using (Brep brep = new Brep(path))
                    {
                        //List<Autodesk.AutoCAD.BoundaryRepresentation.Face> solid_faces = brep.Faces.ToList();
                        //foreach (KeyValuePair<string, string> face_info in faces_info)
                        foreach (Autodesk.AutoCAD.BoundaryRepresentation.Face face in brep.Faces)
                        {
                            //int FaceNumber = Convert.ToInt32(face_info.Key.Split('_')[1]);
                            //long FaceNumber = Convert.ToInt64(face_info.Key.Split('_')[1]);
                            //IntPtr face_IntPtr = (IntPtr)FaceNumber;
                            //FullSubentityPath path2 = new FullSubentityPath(new ObjectId[1] { OneSolidId }, new SubentityId(SubentityType.Null, face_IntPtr));
                            //Autodesk.AutoCAD.BoundaryRepresentation.Face face = solid_faces[FaceNumber];
                            string Face_Id = $"{OneSolidId.Handle}_{face.SubentityPath.SubentId.IndexPtr.ToInt64()}";
                            if (faces_info.ContainsKey(Face_Id))
                            {
                                SubentityId face_id = face.SubentityPath.SubentId;
                                var face_info_material_name = faces_info[Face_Id];

                                FacesProps face_data = new FacesProps();
                                if (UseColors == true)
                                {
                                    face_data.color = ColorsByNames[face_info_material_name];
                                }
                                else
                                {
                                    face_data.material = MaterialsByName[face_info_material_name];
                                }
                                face_data.id = face_id;
                                faces.Add(face_data);
                            }

                        }
                    }
                    if (UseColors == true)
                    {
                        foreach (FacesProps face_one in faces)
                        {
                            OneSolid.SetSubentityColor(face_one.id, face_one.color);
                            //doc.Editor.WriteMessage($"Color {face_one.color} was assigned for {face_one.id}");
                        }
                    }
                    else
                    {
                        foreach (FacesProps face_one in faces)
                        {
                            OneSolid.SetSubentityMaterial(face_one.id, face_one.material);
                            //doc.Editor.WriteMessage($"Material was assigned for {face_one.id}");
                        }
                    }
                }
                
                
            }
        }
    }
}
