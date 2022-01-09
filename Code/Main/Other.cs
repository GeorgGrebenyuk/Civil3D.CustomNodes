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
using ds = Autodesk.DesignScript.Geometry;
using System.Globalization;
using dyn = Autodesk.AutoCAD.DynamoNodes;
using System.Collections;

namespace Autodesk.Civil3D_CustomNodes
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
        /// <summary>
        /// Create an AutoCAD's MLeader's object by two points and text's string
        /// </summary>
        /// <param name="place_point"></param>
        /// <param name="leader_line_start"></param>
        /// <param name="leader_text"></param>
        /// <param name="text_width"></param>
        /// <param name="text_height"></param>
        public static void CreateMLeaderByPoint (Autodesk.AutoCAD.DynamoNodes.Document doc_dyn, ds.Point place_point, ds.Point leader_line_start, string leader_text, double TextRotation = 0d, double LandingGap = 0.04, double text_width = 5, double text_height = 0.2, double arrow_size = 0.5)
        {
            /* Help docs
            *http://bushman-andrey.blogspot.com/2013/01/blog-post.html
            *https://adn-cis.org/kak-sozdat-multivyinosku-v-.net.html
            *https://adn-cis.org/forum/index.php?topic=10503.msg49118#msg49118
            */
            Document doc = doc_dyn.AcDocument;
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction Tx = db.TransactionManager.StartTransaction())
            {
                BlockTable table = Tx.GetObject(
                    db.BlockTableId,
                    OpenMode.ForRead)
                        as BlockTable;

                BlockTableRecord model = Tx.GetObject(
                    table[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite)
                        as BlockTableRecord;

                MLeader leader = new MLeader();
                var temp_leader = leader.AddLeader();
                //leader.SetArrowSize(0, arrow_size);
                leader.ArrowSize = arrow_size;
                leader.AddLeaderLine(temp_leader);
                leader.AddFirstVertex(temp_leader, new Point3d(place_point.X, place_point.Y, 0.0));
                leader.AddLastVertex(temp_leader, new Point3d(leader_line_start.X, leader_line_start.Y, 0.0));

                leader.SetDatabaseDefaults();

                leader.ContentType = ContentType.MTextContent;
                leader.SetTextAttachmentType(
                            Autodesk.AutoCAD.DatabaseServices.TextAttachmentType.AttachmentBottomLine,
                             Autodesk.AutoCAD.DatabaseServices.LeaderDirectionType.LeftLeader);

                MText mText = new MText();
                mText.SetDatabaseDefaults();
                mText.Width = text_width;
                mText.Height = text_height;
                
                mText.SetContentsRtf(leader_text);
                mText.Rotation = TextRotation;
                leader.MText = mText;
                leader.LandingGap = LandingGap;
                mText.BackgroundFill = false;

                model.AppendEntity(leader);
                Tx.AddNewlyCreatedDBObject(leader, true);

                Tx.Commit();
            }
        }
        /// <summary>
        /// Get document's materials with their names and ObjectId
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ObjectId> GetMaterials (Autodesk.AutoCAD.DynamoNodes.Document doc_dyn)
        {
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            Document doc = doc_dyn.AcDocument;
            Database db = doc.Database;
            Dictionary<string, ObjectId> MaterialAndName = new Dictionary<string, ObjectId>();

            //using (DocumentLock acDocLock = doc.LockDocument())
            //{

            //}
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary MaterialsLib = (DBDictionary)tr.GetObject(db.MaterialDictionaryId, OpenMode.ForRead);
                foreach (DBDictionaryEntry entry in MaterialsLib)
                {
                    Material OneMaterial = tr.GetObject((ObjectId)entry.Value, OpenMode.ForRead) as Material;
                    if (!MaterialAndName.ContainsKey(OneMaterial.Name))
                    {
                        MaterialAndName.Add(OneMaterial.Name, OneMaterial.ObjectId);
                    }
                }
                tr.Commit();
            }
            return MaterialAndName;
        }
        public static Dictionary <string,object> GetMyColors ()
        {
            AutoCAD.Colors.Color dirt = AutoCAD.Colors.Color.FromRgb(133, 105, 73);
            AutoCAD.Colors.Color wood = AutoCAD.Colors.Color.FromRgb(245,179,12);
            AutoCAD.Colors.Color water = AutoCAD.Colors.Color.FromRgb(12,35,245);
            AutoCAD.Colors.Color wetland = AutoCAD.Colors.Color.FromRgb(234,192,235);
            AutoCAD.Colors.Color scrub = AutoCAD.Colors.Color.FromRgb(20,247,27);
            return new Dictionary<string, object>
            {
                { "dirt",dirt}, { "wood",wood}, { "water",water}, { "wetland",wetland}, { "scrub",scrub}
            };
        }
    }
}
