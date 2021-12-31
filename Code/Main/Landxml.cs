using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Globalization;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Autodesk.Civil3D_CustomNodes
{
    public class Landxml
    {
		private Landxml() { }
		//[CommandMethod("ConvertLandXmlStructure")]
		public static void ConvertLandXmlStructure_1(string PathToCurrentFile)
		{
			//Document doc = doc_dyn.AcDocument;
			var CivilApp = CivilApplication.ActiveDocument;
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Database db = doc.Database;
			Editor ed = doc.Editor;

			string NewFilePath = PathToCurrentFile.Replace(".xml", Guid.NewGuid().ToString() + ".xml");
			File.Copy(PathToCurrentFile, NewFilePath);
			XDocument doc_LandXml = XDocument.Load(NewFilePath);
			XNamespace ns = "http://www.landxml.org/schema/LandXML-1.2";

			//Копируем текущий LandXML и удаляем из него коллекцию с поверхностями
			XDocument doc_LandXmlNew = XDocument.Load(NewFilePath);
			XElement el_RootNew = doc_LandXmlNew.Root;
			doc_LandXmlNew.Descendants().Where(a => a.Name.LocalName == "Surfaces").First().RemoveAll();

			XElement el_CgPoints = new XElement(ns + "CgPoints"); long CG_Counter = 1; el_RootNew.Add(el_CgPoints);

			foreach (XElement OneSurface in doc_LandXml.Descendants().Where(a => a.Name.LocalName == "Surface"))
			{
				string Surf_Name = OneSurface.Attribute("name").Value;
				IEnumerable<XElement> Breaklines = OneSurface.Descendants().Where(a => (a.Name.LocalName == "Breakline" && a.Parent.Name.LocalName == "Breaklines") || (a.Name.LocalName == "Boundary" && a.Parent.Name.LocalName == "Boundaries"));
				//IEnumerable<XElement> Boundaries = OneSurface.Descendants().Where(a => a.Name.LocalName == "Boundary" && a.Parent.Name.LocalName == "Boundaries");
				//Сделать потом разделение группы точек по категориям "к чему оно принадлежит"
				IEnumerable<XElement> DataPoints = OneSurface.Descendants().Where(a => a.Name.LocalName == "PntList3D" && a.Parent.Name.LocalName == "DataPoints");

				//Для случая добавления СЛ по границам (когда плошадка с таким именем уже будет существовать)
				XElement el_PlanFeatures = new XElement(ns + "PlanFeatures", new XAttribute("name", Surf_Name));
				//Запуск действий по изменению поверхности
				ActionsWithBreakLines(Breaklines);
				//ActionsWithBreakLines(Boundaries, "Boundary");
				ActionsWithPoints(DataPoints);

				void ActionsWithBreakLines(IEnumerable<XElement> GroupXElemetnts)
				{
					foreach (var OneBreakline in GroupXElemetnts)
					{
						string Breakline_Name = OneBreakline.Attribute("name").Value;
						string Breakline_Descr = OneBreakline.Attribute("desc").Value;
						string BoundaryType = null;
						if (OneBreakline.Attributes().Where(a => a.Name.LocalName == "bndType").Count() > 0)
						{
							BoundaryType = OneBreakline.Attribute("bndType").Value;
							Breakline_Name = BoundaryType + "_" + Breakline_Name;
						}

						string[] CoordsOfBreakline_Str = OneBreakline.Descendants().Where(a => a.Name.LocalName == "PntList3D").First().Value.Split(' ');
						double[] CoordsOfBreakline = CoordsOfBreakline_Str.Select(x => double.Parse(x, CultureInfo.GetCultureInfo("en-US"))).ToArray();

						//foreach (var OneValue in CoordsOfBreakline) { Console.WriteLine(OneValue); }

						XElement el_PlanFeature = new XElement(ns + "PlanFeature", new XAttribute("name", Breakline_Name), new XAttribute("desc", Breakline_Descr));
						XElement el_CoordGeom = new XElement(ns + "CoordGeom");

						//Геометрия ХЛ (набор точек)
						for (int i1 = 0; i1 < CoordsOfBreakline.Length - 5; i1 += 3)
						{
							double X1, Y1, Z1, X2, Y2, Z2;
							X1 = CoordsOfBreakline[i1 + 1]; Y1 = CoordsOfBreakline[i1 + 0]; Z1 = CoordsOfBreakline[i1 + 2];
							X2 = CoordsOfBreakline[i1 + 4]; Y2 = CoordsOfBreakline[i1 + 3]; Z2 = CoordsOfBreakline[i1 + 5];
							XElement el_FL_Line = new XElement(ns + "Line", new XElement(ns + "Start", $"{Y1} {X1} {Z1}"), new XElement(ns + "End", $"{Y2} {X2} {Z2}"));
							el_CoordGeom.Add(el_FL_Line);
						}
						el_PlanFeature.Add(el_CoordGeom);

						//Свойствах ХЛ
						XElement el_Feature = new XElement(ns + "Feature",
							new XAttribute("code", "FeatureLine"),
							new XAttribute("source", "Autodesk Civil 3D"),
							new XElement(ns + "Property", new XAttribute("label", "site"), new XAttribute("value", Surf_Name)),
							new XElement(ns + "Property", new XAttribute("label", "layer"), new XAttribute("value", Surf_Name + "_Breaklines")));
						el_PlanFeature.Add(el_Feature);
						el_PlanFeatures.Add(el_PlanFeature);
					}
					el_RootNew.Add(el_PlanFeatures);
				}

				void ActionsWithPoints(IEnumerable<XElement> GroupXElemetnts)
				{
					XElement el_CgPointsForSurface = new XElement(ns + "CgPoints", new XAttribute("name", Surf_Name));
					foreach (var OnePointsGroup in GroupXElemetnts)
					{
						double[] CoordsGroup = OnePointsGroup.Value.Split(' ').Select(x => double.Parse(x, CultureInfo.GetCultureInfo("en-US"))).ToArray();
						for (int i1 = 0; i1 < CoordsGroup.Length - 2; i1 += 3)
						{
							XElement NewPoint_GlobalGroup = new XElement(ns + "CgPoint", new XAttribute("name", $"DataSource_{CG_Counter}"), new XAttribute("code", $"Data Point"));
							NewPoint_GlobalGroup.SetValue(CoordsGroup[i1 + 0].ToString() + " " + CoordsGroup[i1 + 1].ToString() + " " + CoordsGroup[i1 + 2].ToString());
							el_CgPoints.Add(NewPoint_GlobalGroup);

							XElement NewPoint_ForSurface = new XElement(ns + "CgPoint", new XAttribute("pntRef", $"DataSource_{CG_Counter}"));
							el_CgPointsForSurface.Add(NewPoint_ForSurface);
							CG_Counter++;
						}
					}
					el_RootNew.Add(el_CgPointsForSurface);

				}
			}
			doc_LandXmlNew.Save(NewFilePath);

		}
		public static void CreateSurfaces()
		{
			//Document doc = doc_dyn.AcDocument;
			var CivilApp = CivilApplication.ActiveDocument;
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Database db = doc.Database;
			Editor ed = doc.Editor;

			using (Transaction ts = db.TransactionManager.StartTransaction())
			{
				foreach (ObjectId OneSiteItem in CivilApp.GetSiteIds())
				{
					Site OneSite = ts.GetObject(OneSiteItem, OpenMode.ForRead) as Site;
					string Site_Name = OneSite.Name;

					bool IsSurfaceInDrawing = false;
					foreach (ObjectId SurfaceId in CivilApp.GetSurfaceIds())
					{
						TinSurface OneSurf = ts.GetObject(SurfaceId, OpenMode.ForRead) as TinSurface;
						if (OneSurf.Name == Site_Name)
						{
							IsSurfaceInDrawing = true;
							break;
						}
					}
					if (IsSurfaceInDrawing == true) continue;

					try
					{
						ObjectId SurfaceStyle = CivilApp.Styles.SurfaceStyles[0];
						ObjectId tinSurfaceId = TinSurface.Create(Site_Name, SurfaceStyle);
					}
					catch (System.Exception e)
					{
						ed.WriteMessage(e.Message);
					}
				}
				ts.Commit();
			}
		}
		//[CommandMethod("AddDataToSurfaces")]
		public static void AddDataToSurfaces(double midOrdinate = 0.1, double maxDist = 10.0, double weedingDist = 0.1, double weedingAngle = 0.5)
		{
			//Document doc = doc_dyn.AcDocument;
			var CivilApp = CivilApplication.ActiveDocument;
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Database db = doc.Database;
			Editor ed = doc.Editor;
			//double midOrdinate = 0.1; double maxDist = 10.0; double weedingDist = 0.1; double weedingAngle = 0.5; -- if start as CommandMethod
			using (Transaction ts = db.TransactionManager.StartTransaction())
			{
				try
				{
					foreach (ObjectId OneSiteItem in CivilApp.GetSiteIds())
					{
						Site OneSite = ts.GetObject(OneSiteItem, OpenMode.ForRead) as Site;
						string Site_Name = OneSite.Name;
						//ed.WriteMessage("\n Site_Name =" + Site_Name);

						foreach (ObjectId OneSurfaceId in CivilApp.GetSurfaceIds())
						{
							//ObjectId SurfaceId = new ObjectId();
							TinSurface CurrentSurface = ts.GetObject(OneSurfaceId, OpenMode.ForWrite) as TinSurface;
							//ed.WriteMessage("\n OneSurf.Name =" + CurrentSurface.Name);
							if (CurrentSurface.Name == Site_Name)
							{
								ed.WriteMessage($"\n Site with surface's name {CurrentSurface.Name} is exist!");
								//ed.WriteMessage("\n Start adding standard breaklines for " + CurrentSurface.Name);
								AddBreakLines(null);
								//ed.WriteMessage("\n Start adding point group for " + CurrentSurface.Name);
								AddPointsGroup();
								//ed.WriteMessage("\n Start adding out boundary for " + CurrentSurface.Name);
								AddBreakLines("Out_Boundary");
								//ed.WriteMessage("\n Start adding internal boundary for " + CurrentSurface.Name);
								AddBreakLines("Internal_Boundary");
								CurrentSurface.Rebuild();
								ed.WriteMessage("\n Start rebuilding for " + CurrentSurface.Name);

								void AddBreakLines(string TypeOfLines)
								{
									ObjectIdCollection GroupOfBreaklines = new ObjectIdCollection();
									foreach (ObjectId OneFlineItem in OneSite.GetFeatureLineIds())
									{

										FeatureLine OneFline = ts.GetObject(OneFlineItem, OpenMode.ForRead) as FeatureLine;
										string OneFline_Name = OneFline.Name;

										if (OneFline_Name.Contains("outer_") && TypeOfLines == "Out_Boundary")
										{
											ed.WriteMessage("\n Start adding outer boundary for " + OneFline_Name);
											CurrentSurface.BoundariesDefinition.AddBoundaries(OneFline.GetPoints(Autodesk.Civil.FeatureLinePointType.AllPoints), midOrdinate, Autodesk.Civil.SurfaceBoundaryType.Outer, true);
										}
										else if (OneFline_Name.Contains("void_") && TypeOfLines == "Internal_Boundary")
										{
											ed.WriteMessage("\n Start adding internal boundary for " + OneFline_Name);
											CurrentSurface.BoundariesDefinition.AddBoundaries(OneFline.GetPoints(Autodesk.Civil.FeatureLinePointType.AllPoints), midOrdinate, Autodesk.Civil.SurfaceBoundaryType.Hide, true);
										}
										else
										{
											GroupOfBreaklines.Add(OneFlineItem);
										}
									}
									if (TypeOfLines == null)
									{
										ed.WriteMessage("\n Start adding standard breaklines");
										CurrentSurface.BreaklinesDefinition.AddStandardBreaklines(GroupOfBreaklines, midOrdinate, maxDist, weedingDist, weedingAngle);
									}
								}

								void AddPointsGroup()
								{
									CogoPointCollection CG_AllPoints = CivilApp.CogoPoints;
									foreach (ObjectId OneCogoItem in CG_AllPoints)
									{
										CogoPoint COGO_Single = ts.GetObject(OneCogoItem, OpenMode.ForRead) as CogoPoint;
										ObjectId CG_GroupId = COGO_Single.PrimaryPointGroupId;
										PointGroup CG_Group = ts.GetObject(CG_GroupId, OpenMode.ForRead) as PointGroup;
										if (CG_Group.Name == Site_Name)
										{
											ed.WriteMessage("\n COGO points group for surface's name was find - it's name = " + CG_Group.Name);
											CurrentSurface.PointGroupsDefinition.AddPointGroup(CG_GroupId);
											break;
										}
									}
								}
								break;
							}
						}
					}
				}
				catch (System.Exception e)
				{
					ed.WriteMessage(e.Message);
				}

				ts.Commit();
			}
		}

		private static Point3dCollection GetPoint3dCollectionFromData(double[] Coords)
		{
			Point3dCollection newPoint3dCollection = new Point3dCollection();
			for (int i1 = 0; i1 < Coords.Length - 2; i1 += 3)
			{
				double Coord_X = Coords[i1 + 1]; double Coord_Y = Coords[i1 + 0]; double Coord_Z = Coords[i1 + 2];
				Point3d newPoint = new Point3d(Coord_X, Coord_Y, Coord_Z);
				newPoint3dCollection.Add(newPoint);
			}
			return newPoint3dCollection;
		}
	}
}
