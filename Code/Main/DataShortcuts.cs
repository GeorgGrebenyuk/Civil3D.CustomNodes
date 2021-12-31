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
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DataShortcuts;
using static Autodesk.Civil.DataShortcuts.DataShortcuts.DataShortcutManager;
using Autodesk.DesignScript.Runtime;
using System.Globalization;

namespace Autodesk.Civil3D_CustomNodes
{
	public class DataShortcuts
	{
		private DataShortcuts() { }
		/// <summary>
		/// Get all objects from data shortcuts's folder as xml-parse data
		/// </summary>
		/// <param name="PathToDSDir">Absolute file-path to DataShortcuts folder</param>
		/// <returns>Dictionary with info about objects (name, source drawing and Type)</returns>
		[MultiReturn(new[] { "DataShortcutEntityType", "ParentDrawingPath", "ObjName", "HandleLow" })]
		public static Dictionary<string, object> GetAllElementsFromDSFolder(string PathToDSDir)
		{			
			List<DataShortcutEntityType> DTypes = new List<DataShortcutEntityType>();
			List<string> DTypes_Temp = new List<string>();

			List<string> PathsToDWG = new List<string>();
			List<string> NamesOfObjects = new List<string>();
			List<int> HandleLowValies = new List<int>();
			//Проверим, есть ли в папке БС база данных от прошлой итерации
			//Временно не учитываем версионность - как есть
			string PathTo_DTypes = PathToDSDir + "\\CustomNodesDB_DTypes.txt";
			string PathTo_PathsToDWG = PathToDSDir + "\\CustomNodesDB_PathsToDWG.txt";
			string PathTo_NamesOfObjects = PathToDSDir + "\\CustomNodesDB_NamesOfObjects.txt";
			string PathTo_HandleLowValies = PathToDSDir + "\\CustomNodesDB_HandleLowValies.txt";
			if (File.Exists(PathTo_DTypes) && File.Exists(PathTo_PathsToDWG) && File.Exists(PathTo_NamesOfObjects)&& File.Exists(PathTo_HandleLowValies))
			{
				DTypes_Temp = File.ReadAllLines(PathTo_DTypes).OfType<string>().ToList();
				foreach (string OneDTypeStr in DTypes_Temp)
				{
					DTypes.Add(GetDSEnityType_Str(OneDTypeStr));
				}
				PathsToDWG = File.ReadAllLines(PathTo_PathsToDWG).OfType<string>().ToList();
				NamesOfObjects = File.ReadAllLines(PathTo_NamesOfObjects).OfType<string>().ToList();
				HandleLowValies = File.ReadAllLines(PathTo_HandleLowValies).Select(x => int.Parse(x, CultureInfo.GetCultureInfo("en-US"))).ToArray().OfType<int>().ToList();
			}
			
			else
			{
				foreach (string OneXmlFilePath in Directory.GetFiles(PathToDSDir,"*.xml",SearchOption.AllDirectories))
				{
					if (Path.GetFileNameWithoutExtension(OneXmlFilePath) != "ShortcutsHistory")
					{
						XDocument XmlDoc = XDocument.Load(OneXmlFilePath);
						//Find a parent drawing:
						XElement ParentDrawingEl = XmlDoc.Descendants().Where(a => a.Name.LocalName == "File").First();
						string ParentDrawingPath = ParentDrawingEl.Attribute("name").Value;
						//Find an element name and type
						XElement ObjEl = XmlDoc.Descendants().Where(a => a.Name.LocalName == "Object").First();
						string ObjName = ObjEl.Attribute("name").Value;
						string ObjType = ObjEl.Attribute("type").Value;
						int LowHandleValue = Convert.ToInt32( ObjEl.Attribute("handleLow").Value);
						DataShortcutEntityType DType = GetDSEnityType(ObjType);
						DTypes_Temp.Add(ObjType);

						DTypes.Add(DType);
						PathsToDWG.Add(ParentDrawingPath);
						NamesOfObjects.Add(ObjName);
						HandleLowValies.Add(LowHandleValue);
					}
				}
				File.WriteAllText(PathTo_DTypes,string.Join( "\r\n", DTypes_Temp));
				File.WriteAllText(PathTo_PathsToDWG,string.Join("\r\n", PathsToDWG));
				File.WriteAllText(PathTo_NamesOfObjects,string.Join("\r\n", NamesOfObjects));
				File.WriteAllText(PathTo_HandleLowValies,string.Join("\r\n", HandleLowValies));
			}

			//Создаем списки из словарей под каждый элемент
			return new Dictionary<string, object>
			{
					{ "DataShortcutEntityType",DTypes },{ "ParentDrawingPath",PathsToDWG },{ "ObjName",NamesOfObjects },{"HandleLow",HandleLowValies }
			};
		}
		/// <summary>
		/// Auxilary method to return DataShortcutEntityType of each object by folder's name where placed that xml fles
		/// </summary>
		/// <param name="FolderName"></param>
		/// <returns></returns>
		private static DataShortcutEntityType GetDSEnityType (string FolderName)
		{
			DataShortcutEntityType DType = 0;
			switch (FolderName)
			{
				case "AeccDbAlignment":
					DType = DataShortcutEntityType.Alignment;
					break;
				case "PAeccDbPipeNetwork":
					DType = DataShortcutEntityType.PipeNetwork;
					break;
				case "AeccDbPressurePipeNetwork":
					DType = DataShortcutEntityType.PressurePipeNetwork;
					break;
				case "AeccDbCorridor":
					DType = DataShortcutEntityType.Corridor;
					break;
				case "AeccDbProfile":
					DType = DataShortcutEntityType.Profile;
					break;
				case "AeccDbSampleLineGroup":
					DType = DataShortcutEntityType.SampleLineGroup;
					break;
				case "AeccDbViewFrameGroup":
					DType = DataShortcutEntityType.ViewFrameGroup;
					break;
				case "AeccDbSurface":
					DType = DataShortcutEntityType.Surface;
					break;
			}
			return DType;
		}
		private static DataShortcutEntityType GetDSEnityType_Str(string FolderName)
		{
			DataShortcutEntityType DType = 0;
			switch (FolderName)
			{
				case "Alignment":
					DType = DataShortcutEntityType.Alignment;
					break;
				case "PipeNetwork":
					DType = DataShortcutEntityType.PipeNetwork;
					break;
				case "PressurePipeNetwork":
					DType = DataShortcutEntityType.PressurePipeNetwork;
					break;
				case "Corridor":
					DType = DataShortcutEntityType.Corridor;
					break;
				case "Profile":
					DType = DataShortcutEntityType.Profile;
					break;
				case "SampleLineGroup":
					DType = DataShortcutEntityType.SampleLineGroup;
					break;
				case "ViewFrameGroup":
					DType = DataShortcutEntityType.ViewFrameGroup;
					break;
				case "Surface":
					DType = DataShortcutEntityType.Surface;
					break;
			}
			return DType;
		}
		/// <summary>
		/// DataShortcutEntityType for alignments
		/// </summary>
		/// <returns></returns>
		public static DataShortcutEntityType Alignment () { return DataShortcutEntityType.Alignment; }
		/// <summary>
		/// DataShortcutEntityType for PipeNetworks
		/// </summary>
		/// <returns></returns>
		public static DataShortcutEntityType PipeNetwork() { return DataShortcutEntityType.PipeNetwork; }
		/// <summary>
		/// DataShortcutEntityType for PressurePipeNetwork
		/// </summary>
		/// <returns></returns>
		public static DataShortcutEntityType PressurePipeNetwork() { return DataShortcutEntityType.PressurePipeNetwork; }
		/// <summary>
		/// DataShortcutEntityType for Corridor
		/// </summary>
		/// <returns></returns>
		public static DataShortcutEntityType Corridor() { return DataShortcutEntityType.Corridor; }
		/// <summary>
		/// DataShortcutEntityType for Profile
		/// </summary>
		/// <returns></returns>
		public static DataShortcutEntityType Profile() { return DataShortcutEntityType.Profile; }
		/// <summary>
		/// DataShortcutEntityType for SampleLineGroup
		/// </summary>
		/// <returns></returns>
		public static DataShortcutEntityType SampleLineGroup() { return DataShortcutEntityType.SampleLineGroup; }
		/// <summary>
		/// DataShortcutEntityType for ViewFrameGroup
		/// </summary>
		/// <returns></returns>
		public static DataShortcutEntityType ViewFrameGroup() { return DataShortcutEntityType.ViewFrameGroup; }
		/// <summary>
		/// DataShortcutEntityType for Surface
		/// </summary>
		/// <returns></returns>
		public static DataShortcutEntityType Surface() { return DataShortcutEntityType.Surface; }
		//public static int GetIndexOfItem(string TtemName)
		//{
		//	Document m_Doc = Application.DocumentManager.MdiActiveDocument;
		//	Database db = m_Doc.Database;
		//	Database hostDb = new Database(false, true);
		//	hostDb.ReadDwgFile(hostDwgName, FileOpenMode.OpenForReadAndAllShare, false, null);

		//	var dsManager = DataShortcuts.CreateDataShortcutManager(ref isValidCreation);

		//	CivilDocument c3d_doc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
		//	c3d_doc.GetViewFrameGroupIds()
		//	using (Transaction tr = db.TransactionManager.StartTransaction())
		//	{

		//		tr.Commit();
		//	}
		//}


	}
}
