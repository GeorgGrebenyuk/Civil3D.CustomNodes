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

namespace Civil3D_DrawingProperties
{
    public class ProjectProperties
    {
		//Dynamo nodes Package for changing Civil 3D's document parameters
		/// <summary>
		/// Method GetDrawingUnits return Units of current drawing as "Meters" ot "Feet" as string
		/// </summary>
		/// <returns>Units of current drawing as "Meters" ot "Feet"</returns>
		public static string GetDrawingUnits() 
        {
            CivilDocument c3d_doc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
            var Units_Window = c3d_doc.Settings.DrawingSettings;
            var Project_Units = Units_Window.UnitZoneSettings.DrawingUnits;
            string Project_Units_str = Convert.ToString(Project_Units);

            return Project_Units_str;
			//Возвращает либо Meters либо Feet
        }
		/// <summary>
		/// Method CheckDrawingUnitsToEqualMeter return true, if Units of drawing = Meters and false if isn't.
		/// </summary>
		/// <param name="Project_Units_str">String with Project units</param>
		/// <returns>false/true</returns>
		public static bool CheckDrawingUnitsToEqualMeter(string Project_Units_str)
		{
			CivilDocument c3d_doc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
			var Units_Window = c3d_doc.Settings.DrawingSettings;
			bool condition_meters;
			if (Project_Units_str == "Meters") condition_meters = true;
			else condition_meters = false;
			return condition_meters;
		}

		/// <summary>
		/// Method CheckDrawingUnitsToEqualFeet return true, if Units of drawing = Feet and false if isn't.
		/// </summary>
		/// <param name="Project_Units_str">String with Project units</param>
		/// <returns>false/true</returns>
		public static bool CheckDrawingUnitsToEqualFeets(string Project_Units_str)
		{
			CivilDocument c3d_doc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
			var Units_Window = c3d_doc.Settings.DrawingSettings;
			bool condition_feet;
			if (Project_Units_str == "Feet") condition_feet = true;
			else condition_feet = false;
			return condition_feet;
		}
		/// <summary>
		/// Method ChangeDrawingUnitsToMetersBool changes Unit's system of drawing to Metric with input bool parameter = false (from f.e. CheckDrawingUnitsToEqualMeter). 
		/// Otherwise Unit's system resetting to Imperial
		/// </summary>
		/// <param name="condition_meters">Bool parameter; if bool = false, that method is active</param>
		/// <returns>String with resulting changing</returns>
		public static string ChangeDrawingUnitsToMetersBool (bool condition_meters)
		{
			Document m_Doc = Application.DocumentManager.MdiActiveDocument;
			Database db = m_Doc.Database; 

			CivilDocument c3d_doc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
			var Units_Window = c3d_doc.Settings.DrawingSettings;
			string result;
			if (condition_meters == false)
			{
				using (Transaction tr = db.TransactionManager.StartTransaction())
				{

					Units_Window.UnitZoneSettings.DrawingUnits = Autodesk.Civil.Settings.DrawingUnitType.Meters; //2
					Units_Window.UnitZoneSettings.AngularUnits = Autodesk.Civil.AngleUnitType.Degree; //179
					Units_Window.UnitZoneSettings.ImperialToMetricConversion = Autodesk.Civil.Settings.ImperialToMetricConversionType.InternationalFoot; //536870912;
					Units_Window.UnitZoneSettings.ScaleObjectsFromOtherDrawings = true;
					Units_Window.UnitZoneSettings.MatchAutoCADVariables = true;
					Units_Window.UnitZoneSettings.DrawingScale = 1.0;// 1.0
					Units_Window.AmbientSettings.General.DrivingDirection.Value = Autodesk.Civil.DrivingDirectionType.RightSideOfTheRoad;// 0;
					Units_Window.AmbientSettings.Time.Unit.Value = Autodesk.Civil.TimeUnitType.Minute;//301;
					Units_Window.AmbientSettings.Distance.Unit.Value = Autodesk.Civil.LinearUnitType.Meter;// 2;
					Units_Window.AmbientSettings.Dimension.Unit.Value = Autodesk.Civil.LinearUnitType.Meter;// 2;
					Units_Window.AmbientSettings.Coordinate.Unit.Value = Autodesk.Civil.LinearUnitType.Meter;// 2;
					Units_Window.AmbientSettings.GridCoordinate.Unit.Value = Autodesk.Civil.LinearUnitType.Meter; // 2;
					Units_Window.AmbientSettings.Elevation.Unit.Value = Autodesk.Civil.LinearUnitType.Meter; // 2;
					Units_Window.AmbientSettings.Area.Unit.Value = Autodesk.Civil.AreaUnitType.SquareMeter; //55;
					Units_Window.AmbientSettings.Volume.Unit.Value = Autodesk.Civil.VolumeUnitType.CubicMeter;// 96;
					Units_Window.AmbientSettings.Speed.Unit.Value = Autodesk.Civil.SpeedUnitType.KilometerPerHour;// 190;
					Units_Window.AmbientSettings.Angle.Unit.Value = Autodesk.Civil.AngleUnitType.Degree;// 179;
					Units_Window.AmbientSettings.Direction.Unit.Value = Autodesk.Civil.AngleUnitType.Degree;// 179;
					Units_Window.AmbientSettings.LatLong.Unit.Value = Autodesk.Civil.AngleUnitType.Degree;// 179;
					Units_Window.AmbientSettings.Grade.Format.Value = Autodesk.Civil.GradeFormatType.Decimal;// 2468;
					Units_Window.AmbientSettings.Slope.Format.Value = Autodesk.Civil.SlopeFormatType.RiseRun;// 2465;
					Units_Window.AmbientSettings.GradeSlope.Format.Value = Autodesk.Civil.GradeSlopeFormatType.Decimal;// 2468;
					Units_Window.AmbientSettings.Station.Unit.Value = Autodesk.Civil.LinearUnitType.Meter;// 2;
					Units_Window.AmbientSettings.Acceleration.Unit.Value = Autodesk.Civil.AccelerationUnitType.MeterPerSecSquared;// 202;
					Units_Window.AmbientSettings.Pressure.Unit.Value = Autodesk.Civil.PressureUnitType.Kilopascal;// 240;
					result = "Drawing's units were reseted to Metric";
					tr.Commit();
				}
			}
			else
			{
				using (Transaction tr = db.TransactionManager.StartTransaction())
				{
					Units_Window.UnitZoneSettings.DrawingUnits = Autodesk.Civil.Settings.DrawingUnitType.Feet; //30
					Units_Window.UnitZoneSettings.AngularUnits = Autodesk.Civil.AngleUnitType.Degree; //179
					Units_Window.UnitZoneSettings.ImperialToMetricConversion = Autodesk.Civil.Settings.ImperialToMetricConversionType.InternationalFoot; //536870912;
					Units_Window.UnitZoneSettings.ScaleObjectsFromOtherDrawings = false;
					Units_Window.UnitZoneSettings.MatchAutoCADVariables = false;
					Units_Window.UnitZoneSettings.DrawingScale = 1.0;// 1.0; ???
					Units_Window.AmbientSettings.General.DrivingDirection.Value = Autodesk.Civil.DrivingDirectionType.RightSideOfTheRoad;// 0;
					Units_Window.AmbientSettings.Time.Unit.Value = Autodesk.Civil.TimeUnitType.Minute;//301;
					Units_Window.AmbientSettings.Distance.Unit.Value = Autodesk.Civil.LinearUnitType.Foot;// 30;
					Units_Window.AmbientSettings.Dimension.Unit.Value = Autodesk.Civil.LinearUnitType.Inch;// 31;
					Units_Window.AmbientSettings.Coordinate.Unit.Value = Autodesk.Civil.LinearUnitType.Foot;// 30;
					Units_Window.AmbientSettings.GridCoordinate.Unit.Value = Autodesk.Civil.LinearUnitType.Foot;// 30;
					Units_Window.AmbientSettings.Elevation.Unit.Value = Autodesk.Civil.LinearUnitType.Foot;// 30;
					Units_Window.AmbientSettings.Area.Unit.Value = Autodesk.Civil.AreaUnitType.SquareFoot; //56;
					Units_Window.AmbientSettings.Volume.Unit.Value = Autodesk.Civil.VolumeUnitType.CubicYard;// 153;
					Units_Window.AmbientSettings.Speed.Unit.Value = Autodesk.Civil.SpeedUnitType.MilePerHour;// 196;
					Units_Window.AmbientSettings.Angle.Unit.Value = Autodesk.Civil.AngleUnitType.Degree;// 179;
					Units_Window.AmbientSettings.Direction.Unit.Value = Autodesk.Civil.AngleUnitType.Degree;// 179;
					Units_Window.AmbientSettings.LatLong.Unit.Value = Autodesk.Civil.AngleUnitType.Degree;// 179;
					Units_Window.AmbientSettings.Grade.Format.Value = Autodesk.Civil.GradeFormatType.Percent;// 2467;
					Units_Window.AmbientSettings.Slope.Format.Value = Autodesk.Civil.SlopeFormatType.RunRise;// 2466;
					Units_Window.AmbientSettings.GradeSlope.Format.Value = Autodesk.Civil.GradeSlopeFormatType.Percent;// 2467;
					Units_Window.AmbientSettings.Station.Unit.Value = Autodesk.Civil.LinearUnitType.Foot;// 2;
					Units_Window.AmbientSettings.Acceleration.Unit.Value = Autodesk.Civil.AccelerationUnitType.FootPerSecSquared;// 203;
					Units_Window.AmbientSettings.Pressure.Unit.Value = Autodesk.Civil.PressureUnitType.PoundPerSquareInch;// 242;
					result = "Drawing's units were reseted to Imperial";
					tr.Commit();
				}
			}
			return result;
		}

		/// <summary>
		/// Method ChangeDrawingUnitsToMeters changes Unit's system of drawing to Metric independently of drawing's parameters
		/// </summary>
		/// <returns>String with resulting changing</returns>
		public static void ChangeDrawingUnitsToMeters()
		{
			Document m_Doc = Application.DocumentManager.MdiActiveDocument;
			Database db = m_Doc.Database;

			CivilDocument c3d_doc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
			var Units_Window = c3d_doc.Settings.DrawingSettings;
			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				Units_Window.UnitZoneSettings.DrawingUnits = Autodesk.Civil.Settings.DrawingUnitType.Meters; //2
				Units_Window.UnitZoneSettings.AngularUnits = Autodesk.Civil.AngleUnitType.Degree; //179
				Units_Window.UnitZoneSettings.ImperialToMetricConversion = Autodesk.Civil.Settings.ImperialToMetricConversionType.InternationalFoot; //536870912;
				Units_Window.UnitZoneSettings.ScaleObjectsFromOtherDrawings = true;
				Units_Window.UnitZoneSettings.MatchAutoCADVariables = true;
				Units_Window.UnitZoneSettings.DrawingScale = 1.0;// 1.0
				Units_Window.AmbientSettings.General.DrivingDirection.Value = Autodesk.Civil.DrivingDirectionType.RightSideOfTheRoad;// 0;
				Units_Window.AmbientSettings.Time.Unit.Value = Autodesk.Civil.TimeUnitType.Minute;//301;
				Units_Window.AmbientSettings.Distance.Unit.Value = Autodesk.Civil.LinearUnitType.Meter;// 2;
				Units_Window.AmbientSettings.Dimension.Unit.Value = Autodesk.Civil.LinearUnitType.Meter;// 2;
				Units_Window.AmbientSettings.Coordinate.Unit.Value = Autodesk.Civil.LinearUnitType.Meter;// 2;
				Units_Window.AmbientSettings.GridCoordinate.Unit.Value = Autodesk.Civil.LinearUnitType.Meter; // 2;
				Units_Window.AmbientSettings.Elevation.Unit.Value = Autodesk.Civil.LinearUnitType.Meter; // 2;
				Units_Window.AmbientSettings.Area.Unit.Value = Autodesk.Civil.AreaUnitType.SquareMeter; //55;
				Units_Window.AmbientSettings.Volume.Unit.Value = Autodesk.Civil.VolumeUnitType.CubicMeter;// 96;
				Units_Window.AmbientSettings.Speed.Unit.Value = Autodesk.Civil.SpeedUnitType.KilometerPerHour;// 190;
				Units_Window.AmbientSettings.Angle.Unit.Value = Autodesk.Civil.AngleUnitType.Degree;// 179;
				Units_Window.AmbientSettings.Direction.Unit.Value = Autodesk.Civil.AngleUnitType.Degree;// 179;
				Units_Window.AmbientSettings.LatLong.Unit.Value = Autodesk.Civil.AngleUnitType.Degree;// 179;
				Units_Window.AmbientSettings.Grade.Format.Value = Autodesk.Civil.GradeFormatType.Decimal;// 2468;
				Units_Window.AmbientSettings.Slope.Format.Value = Autodesk.Civil.SlopeFormatType.RiseRun;// 2465;
				Units_Window.AmbientSettings.GradeSlope.Format.Value = Autodesk.Civil.GradeSlopeFormatType.Decimal;// 2468;
				Units_Window.AmbientSettings.Station.Unit.Value = Autodesk.Civil.LinearUnitType.Meter;// 2;
				Units_Window.AmbientSettings.Acceleration.Unit.Value = Autodesk.Civil.AccelerationUnitType.MeterPerSecSquared;// 202;
				Units_Window.AmbientSettings.Pressure.Unit.Value = Autodesk.Civil.PressureUnitType.Kilopascal;// 240;
				tr.Commit();
			}
		}
		/// <summary>
		/// Method ChangeDrawingUnitsToFeets changes Unit's system of drawing to Feet independently of drawing's parameters
		/// </summary>
		/// <returns>Fuction</returns>
		public static void ChangeDrawingUnitsToFeet()
		{
			Document m_Doc = Application.DocumentManager.MdiActiveDocument;
			Database db = m_Doc.Database;

			CivilDocument c3d_doc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
			var Units_Window = c3d_doc.Settings.DrawingSettings;
			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				Units_Window.UnitZoneSettings.DrawingUnits = Autodesk.Civil.Settings.DrawingUnitType.Feet; //30
				Units_Window.UnitZoneSettings.AngularUnits = Autodesk.Civil.AngleUnitType.Degree; //179
				Units_Window.UnitZoneSettings.ImperialToMetricConversion = Autodesk.Civil.Settings.ImperialToMetricConversionType.InternationalFoot; //536870912;
				Units_Window.UnitZoneSettings.ScaleObjectsFromOtherDrawings = false;
				Units_Window.UnitZoneSettings.MatchAutoCADVariables = false;
				Units_Window.UnitZoneSettings.DrawingScale = 1.0;// 1.0; ???
				Units_Window.AmbientSettings.General.DrivingDirection.Value = Autodesk.Civil.DrivingDirectionType.RightSideOfTheRoad;// 0;
				Units_Window.AmbientSettings.Time.Unit.Value = Autodesk.Civil.TimeUnitType.Minute;//301;
				Units_Window.AmbientSettings.Distance.Unit.Value = Autodesk.Civil.LinearUnitType.Foot;// 30;
				Units_Window.AmbientSettings.Dimension.Unit.Value = Autodesk.Civil.LinearUnitType.Inch;// 31;
				Units_Window.AmbientSettings.Coordinate.Unit.Value = Autodesk.Civil.LinearUnitType.Foot;// 30;
				Units_Window.AmbientSettings.GridCoordinate.Unit.Value = Autodesk.Civil.LinearUnitType.Foot;// 30;
				Units_Window.AmbientSettings.Elevation.Unit.Value = Autodesk.Civil.LinearUnitType.Foot;// 30;
				Units_Window.AmbientSettings.Area.Unit.Value = Autodesk.Civil.AreaUnitType.SquareFoot; //56;
				Units_Window.AmbientSettings.Volume.Unit.Value = Autodesk.Civil.VolumeUnitType.CubicYard;// 153;
				Units_Window.AmbientSettings.Speed.Unit.Value = Autodesk.Civil.SpeedUnitType.MilePerHour;// 196;
				Units_Window.AmbientSettings.Angle.Unit.Value = Autodesk.Civil.AngleUnitType.Degree;// 179;
				Units_Window.AmbientSettings.Direction.Unit.Value = Autodesk.Civil.AngleUnitType.Degree;// 179;
				Units_Window.AmbientSettings.LatLong.Unit.Value = Autodesk.Civil.AngleUnitType.Degree;// 179;
				Units_Window.AmbientSettings.Grade.Format.Value = Autodesk.Civil.GradeFormatType.Percent;// 2467;
				Units_Window.AmbientSettings.Slope.Format.Value = Autodesk.Civil.SlopeFormatType.RunRise;// 2466;
				Units_Window.AmbientSettings.GradeSlope.Format.Value = Autodesk.Civil.GradeSlopeFormatType.Percent;// 2467;
				Units_Window.AmbientSettings.Station.Unit.Value = Autodesk.Civil.LinearUnitType.Foot;// 2;
				Units_Window.AmbientSettings.Acceleration.Unit.Value = Autodesk.Civil.AccelerationUnitType.FootPerSecSquared;// 203;
				Units_Window.AmbientSettings.Pressure.Unit.Value = Autodesk.Civil.PressureUnitType.PoundPerSquareInch;// 242;
				tr.Commit();
			}
		}

		/// <summary>
		/// Method AssignCoordinateSystem assign coordinate system of drawing from User's input string. Note: there are no checking if User's input is valid CS code in MAPCSLIBRARY.
		/// </summary>
		/// <param name="CS_name"> String with code of Coordinate System that determine in MAPCSLIBRARY</param>
		/// <returns>Fuction</returns>
		public static void AssignCoordinateSystem(string CS_name)
		{
			CivilDocument c3d_doc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
			var Units_Window = c3d_doc.Settings.DrawingSettings;

			Document m_Doc = Application.DocumentManager.MdiActiveDocument;
			Database db = m_Doc.Database;
			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				Units_Window.UnitZoneSettings.CoordinateSystemCode = CS_name;
				tr.Commit();
			}
		}

	}
}
