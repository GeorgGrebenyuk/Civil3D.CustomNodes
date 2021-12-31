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
        /// Getting new ObjectId collection with objects which length are satisfy "needing_length"
        /// </summary>
        /// <param name="objects_id">List with ObjectId</param>
        /// <param name="needing_length">if 1 value -> search all line's that length is equal current;
        /// if 2 value -> search all line's that length is more first and less second; if 3 and more values -> search all line's that length is equal one of current list</param>
        /// <returns>List with ObjectId</returns>
        public static List<ObjectId> GetLinesByLength (Autodesk.AutoCAD.DynamoNodes.Document doc_dyn, List<ObjectId> objects_id, List<double> needing_length, int Accuracy = 8)
        {
            Document doc = doc_dyn.AcDocument;
            for (int i1 = 0; i1 < needing_length.Count; i1++)
            {
                double OneCheckeNum = needing_length[i1];
                needing_length[i1] = Math.Round(OneCheckeNum, Accuracy);
            }
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            List<Autodesk.AutoCAD.DynamoNodes.Object> AcadObjects = new List<Autodesk.AutoCAD.DynamoNodes.Object>();
            List<ObjectId> selected_objects = new List<ObjectId>();
            using (DocumentLock acDocLock = doc.LockDocument())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId line_id in objects_id)
                    {
                        Autodesk.AutoCAD.DatabaseServices.Line OneObject = tr.GetObject(line_id, OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.Line;
                        //if (LineType == 1) OneObject = OneObject as Polyline;
                        //else if (LineType == 2) OneObject = OneObject as Arc;
                        //else OneObject = OneObject as Line;
                        double LineLen = Math.Round(OneObject.Length, Accuracy);
                        //Autodesk.AutoCAD.DatabaseServices.Line OneObject = tr.GetObject(line_id, OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.Line;
                        if (needing_length.Count == 1 && LineLen == needing_length[0])
                        {
                            selected_objects.Add(line_id);
                        }
                        else if (needing_length.Count == 2 && LineLen >= needing_length[0] && LineLen <= needing_length[1])
                        {
                            selected_objects.Add(line_id);
                        }
                        else if (needing_length.Contains(LineLen)) selected_objects.Add(line_id);
                    } 
                    tr.Commit();
                }
            }
            return selected_objects;
        }
        /// <summary>
        /// Get list with AutoCAD's internal ObjectId for input object collection (their's handle)
        /// </summary>
        /// <param name="acad_objects_list">Input AutoCAD's objects</param>
        /// <returns>List witj ObjectId for objects</returns>]
        public static List<ObjectId> GetObjectIdsByObjects_AcadObj (Autodesk.AutoCAD.DynamoNodes.Document doc_dyn, List<Autodesk.AutoCAD.DynamoNodes.Object> acad_objects_list)
        {
            Document doc = doc_dyn.AcDocument;
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            List<ObjectId> ObjectIdsByObjects_AcadObjs = new List<ObjectId>();
            using (DocumentLock acDocLock = doc.LockDocument())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    foreach (Autodesk.AutoCAD.DynamoNodes.Object obj_instance in acad_objects_list)
                    {
                        long obj_handle = Convert.ToInt64(obj_instance.Handle, 16);
                        ObjectId id = db.GetObjectId(false, new Handle(obj_handle), 0);
                        ObjectIdsByObjects_AcadObjs.Add(id);
                    }
                    tr.Commit();
                }
            }
            return ObjectIdsByObjects_AcadObjs;
        }
        /// <summary>
        /// Read's mode (OpenMode.ForRead) as tag of returning objects after closing transaction
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
        public static List<Autodesk.AutoCAD.DynamoNodes.Object> GetAcadObjectsByObjectsId (Autodesk.AutoCAD.DynamoNodes.Document doc_dyn, List<ObjectId> objects_id, OpenMode mode)
        {
            Document doc = doc_dyn.AcDocument;
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            List<Autodesk.AutoCAD.DynamoNodes.Object> AcadObjects = new List<Autodesk.AutoCAD.DynamoNodes.Object>();
            List<string> handle_list = new List<string>();
            using (DocumentLock acDocLock = doc.LockDocument())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId line_id in objects_id)
                    {
                        Autodesk.AutoCAD.DatabaseServices.DBObject OneObject = tr.GetObject(line_id, mode);
                        Handle obj_handle = OneObject.Handle;
                        handle_list.Add(obj_handle.ToString());
                    }
                    tr.Commit();
                }
            }
            foreach (string OneObjHandle in handle_list)
            {
                AcadObjects.Add(dyn.SelectionByQuery.GetObjectByObjectHandle(OneObjHandle));
            }
            return AcadObjects;
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

        
       
       

        
        
    }
    
}