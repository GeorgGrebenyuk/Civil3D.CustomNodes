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
using RTree;
using DynamoRTree;

namespace Civil3D_CustomNodes
{
    public class Selection
    {
        private Selection() { }
        /// <summary>
        /// Some of AutoCAD DxfCodes to search object's property
        /// </summary>
        /// <returns>Integer values of DxfCodes for further using</returns>
        [MultiReturn(new[] { "DxfCode.Color", "DxfCode.ColorRgb", "DxfCode.Comment", "DxfCode.Elevation", "DxfCode.LayerLinetype", "DxfCode.LayerName",
        "DxfCode.LayoutName","DxfCode.LineWeight","DxfCode.TxtSize","DxfCode.Start","DxfCode.BlockName"})]
        public static Dictionary <string, object> GetDxfCodesToTypedValues ()
        {
            return new Dictionary<string, object>
            {
                {"DxfCode.Color", (int)DxfCode.Color },
                {"DxfCode.ColorRgb",(int) DxfCode.ColorRgb },
                {"DxfCode.Comment", (int)DxfCode.Comment },
                {"DxfCode.Elevation", (int)DxfCode.Elevation },
                {"DxfCode.LayerLinetype", (int)DxfCode.LayerLinetype },
                {"DxfCode.LayerName", (int)DxfCode.LayerName },
                {"DxfCode.LayoutName", (int)DxfCode.LayoutName },
                {"DxfCode.LineWeight",(int) DxfCode.LineWeight },
                {"DxfCode.TxtSize", (int)DxfCode.TxtSize },
                {"DxfCode.Start",(int) DxfCode.Start },
                {"DxfCode.BlockName",(int) DxfCode.BlockName },
            };
        }
        /// <summary>
        /// Get list with AutoCAD's internal ObjectId for input object collection
        /// </summary>
        /// <param name="acad_objects_list">Input AutoCAD's objects</param>
        /// <returns>List witj ObjectId for objects</returns>
        [MultiReturn(new[] { "acad_objects_list" })]
        public static List<ObjectId> GetObjectIdsByObjects_AcadObj (List<Autodesk.AutoCAD.DatabaseServices.DBObject> acad_objects_list)
        {
            
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            List<ObjectId> ObjectIdsByObjects_AcadObjs = new List<ObjectId>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (Autodesk.AutoCAD.DatabaseServices.DBObject line_id in acad_objects_list)
                {
                    ObjectIdsByObjects_AcadObjs.Add(line_id.ObjectId);
                }
                tr.Commit();
            }
            return ObjectIdsByObjects_AcadObjs;
        }
        /// <summary>
        /// DANGEROUS!!!. Can create a FATAL or internal error. Read's mode (OpenMode.ForRead) as tag of returning objects after closing transaction
        /// </summary>
        /// <returns>OpenMode.ForRead</returns>
        
        public static OpenMode ModeForRead () { return OpenMode.ForRead; }
        /// <summary>
        /// DANGEROUS!!!. Can create a FATAL or internal error. Write's mode (OpenMode.ForWrite) as tag of returning objects after closing transaction
        /// </summary>
        /// <returns>OpenMode.ForWrite</returns>
        public static OpenMode ModeForWrite() { return OpenMode.ForWrite; }
        /// <summary>
        /// DANGEROUS!!!. Can create a FATAL or internal error. Get Autodesk.AutoCAD.DatabaseServices.DBObject by ObjectId
        /// </summary>
        /// <param name="objects_id">List with ObjectId</param>
        /// <param name="mode">OpenMode (read or write) -- actions after returning objects</param>
        /// <returns>Autodesk.AutoCAD.DatabaseServices.DBObject list</returns>
        [MultiReturn(new[] { "AcadObjects" })]
        public static List<Autodesk.AutoCAD.DatabaseServices.DBObject> GetAcadObjectsByObjectsId (List<ObjectId> objects_id, OpenMode mode)
        {
            //Не работает
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            List<Autodesk.AutoCAD.DatabaseServices.DBObject> AcadObjects = new List<Autodesk.AutoCAD.DatabaseServices.DBObject>();
            using (DocumentLock acDocLock = doc.LockDocument())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId line_id in objects_id)
                    {
                        Autodesk.AutoCAD.DatabaseServices.DBObject OneObject = tr.GetObject(line_id, mode);
                        AcadObjects.Add(OneObject);
                    }
                    tr.Commit();
                }
            }

            return AcadObjects;
        }
        
        public static void debug_only ()
        {
            List<ObjectId> coll = SelectObjectsByConditions(new Dictionary<string, string>()
            {
                {((int)DxfCode.Start).ToString(), "LINE" }, {((int)DxfCode.LayerName).ToString(),"Водосток"}
            } );
            //List<ObjectId> ids;
            //List<RTree.Rectangle> rects;
            Dictionary<string,object> dict = RTree_acad.GetRTReeRectangleByObjects(coll);
            RTree<ObjectId> tree = RTree_acad.CreateRTreeByRTreeRectangles(dict);


            GetObjectsByCirclesSearching(coll,tree, new List<double>(2) { 0.03699437, 0.03907046 }, 0.5,1.0,0, true);
        }
        /// <summary>
        /// Creating "search pattern" by dxf code (GetDxfCodesToTypedValues) and string's data. Read developer docs!
        /// </summary>
        /// <param name="SearchConditions"></param>
        /// <returns>List with object's id if search's result was successfully finished</returns>
        public static List<ObjectId> SelectObjectsByConditions (Dictionary <string,string> SearchConditions)
        {
           TypedValue[] search_conditions = new TypedValue[SearchConditions.Count];
            for (int i1 = 0; i1 < SearchConditions.Count; i1++)
            {
                int dxf_code = Convert.ToInt32(SearchConditions.ElementAt(i1).Key);
                string string_pattern = SearchConditions.ElementAt(i1).Value;
                search_conditions[i1] = new TypedValue(dxf_code, string_pattern);
            }
            PromptSelectionResult obj_group = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll(new SelectionFilter(search_conditions));
            if (obj_group.Status == PromptStatus.OK) return obj_group.Value.GetObjectIds().ToList();
            else return null;
        }

        /// <summary>
        /// Auxilary node that delete or choosing drawing's linear objects in selected area by each (Radius value) 
        /// non more than MaxLength's value and (if SearchMode =0) which length is equal at least one of value in LineLength's list or (if SearchMode =1)
        /// which length is more than LineLengt[0] and smaller than LineLengt[1]
        /// </summary>
        /// <param name="obj_group">List with object's id</param>
        /// <param name="tree">RTree</param>
        /// <param name="LineLength">Double array with at least two numbers</param>
        /// <param name="MaxLength">Maximum length of line</param>
        /// <param name="Radius">Value of searching's value</param>
        /// <param name="SearchMode">Mode for work with LineLength, read node's description</param>
        /// <param name="NeedDeleteObjects">Boolean, if true -- selected objects will be removed</param>
        public static List<ObjectId> GetObjectsByCirclesSearching (List<ObjectId> obj_group, RTree<ObjectId> tree, List<double> LineLength,  double MaxLength, double Radius, int SearchMode = 0, bool NeedDeleteObjects = false)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            int debug_counter1 = 0;

            //Индексируем все отрезки чертежа
            //Dictionary<ObjectId, RTree.Rectangle> rect_list = RTree_acad.GetRTReeRectangleByObjects (obj_group.ToList());
            //RTree<ObjectId> drawings_lines = RTree_acad.CreateRTreeByRTreeRectangles(rect_list);

            //Список для удаления объектов
            List<ObjectId> lines_for_deleting = new List<ObjectId>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId line_id in obj_group)
                {
                    Autodesk.AutoCAD.DatabaseServices.Line OneLine = tr.GetObject(line_id, OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.Line;
                    bool IsObjectValid = false;
                    double LineLen = Math.Round(OneLine.Length, 8);
                    if (SearchMode == 0)
                    {
                        if (LineLength.Contains(LineLen)) IsObjectValid = true;
                    }
                    else if (SearchMode == 1)
                    {
                        if (LineLen >= LineLength[0] && LineLen <= LineLength[1]) IsObjectValid = true;
                    }
                    if (IsObjectValid)
                    {
                        Point3d line_start = OneLine.StartPoint; Point3d line_end = OneLine.EndPoint;
                        Point3d line_center = new Point3d(new double[3] { (line_start.X + line_end.X) / 2.0, (line_start.Y + line_end.Y) / 2.0, 0 });
                        double[] pnt = new double[3] { line_center.X, line_center.Y, 0 };

                        List<ObjectId> intersects_lines = RTree_acad.GetObgects_Intersects(tree, RTree_acad.GetRTReeRectangleByPoint(pnt, (float)Radius));
                        List<ObjectId> internal_lines = RTree_acad.GetObgects_Contains(tree, RTree_acad.GetRTReeRectangleByPoint(pnt, (float)Radius));
                        AddLines(intersects_lines); AddLines(internal_lines);
                        void AddLines(List<ObjectId> IndexedList)
                        {
                            foreach (ObjectId OneCode in IndexedList)
                            {
                                Autodesk.AutoCAD.DatabaseServices.Line OneLine2 = tr.GetObject(OneCode, OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.Line;
                                if (!lines_for_deleting.Contains(OneCode) && OneLine2.Length < 1) lines_for_deleting.Add(OneCode);
                            }
                        }

                        debug_counter1++;
                    }
                }

                tr.Commit();
            }
            if (NeedDeleteObjects == true && lines_for_deleting.Count > 0)
            {
                using (DocumentLock acDocLock = doc.LockDocument())
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        foreach (ObjectId line_id in lines_for_deleting)
                        {
                            Autodesk.AutoCAD.DatabaseServices.DBObject OneObject = tr.GetObject(line_id, OpenMode.ForWrite) as Autodesk.AutoCAD.DatabaseServices.DBObject;
                            OneObject.Erase(true);
                        }
                        tr.Commit();
                    }
                }
               
                return null;
            }
            else return lines_for_deleting;

        }
       
       

        
        
    }
    
}