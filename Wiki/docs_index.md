#Guide of Dynamo's package "Civil3D.CustomNodes"
## About program's classes
### class __DataShortcuts__
This class contains newest features in Civil3D update 2022.1:
Read more at [that article] (https://zen.yandex.ru/media/id/5d0dba97ecd5cf00afaf2938/civil-3d-api-rabotaem-s-bystrymi-ssylkami-chast-1-617e68890e9e264c09053b71)
- **Dictionary** _GetAllElementsFromDSFolder_: Get all objects from data shortcuts's folder as xml-parse data (Dictionary). There are (at that moment) four outer-datas: DataShortcutEntityType, PathsToDWG, NamesOfObjects, HandleLowValies

- **DataShortcutEntityType** _Alignment_: DataShortcutEntityType for alignments

- **DataShortcutEntityType** _PipeNetwork_: DataShortcutEntityType for PipeNetworks

- **DataShortcutEntityType** _PressurePipeNetwork_: DataShortcutEntityType for PressurePipeNetworks

- **DataShortcutEntityType** _Corridor_: DataShortcutEntityType for Corridors

- **DataShortcutEntityType** _Profile(_: DataShortcutEntityType for Profile(s

- **DataShortcutEntityType** _SampleLineGroup_: DataShortcutEntityType for SampleLineGroupts

- **DataShortcutEntityType** _ViewFrameGroup_: DataShortcutEntityType for ViewFrameGroups

- **DataShortcutEntityType** _Surface_: DataShortcutEntityType for Surfaces

### class __Landxml__

This class contain's nodes to work with surface definition by linear objects in xml. Read more in [that article] (https://zen.yandex.ru/media/id/5d0dba97ecd5cf00afaf2938/zanimatelnoe-dynamo-perestroenie-poverhnostei-dlia-lineinyh-obektov-61aefa2cf59e5562bfea7b67).

- **void** _ConvertLandXmlStructure_: Change structure of LandXML file (move breaklins from surfaces to PlanFeatures (read more about all nodes in class in article above)

- **void** _CreateSurfaces_: Create surfaces by PlanFeatures' names (as empty structures)

- **void** _AddDataToSurfaces_: Adding data from PlanFeatures (character lines) to surfaces and also inser PointsCogoGrous to surfaces

###class __ProjectProperties__

This class work with Civil3d's documents properties. There are no article for it.

- *string* _GetDrawingUnits_: Method GetDrawingUnits return Units of current drawing as "Meters" ot "Feet" as string

- *boolean* _CheckDrawingUnitsToEqualMeter_: Method CheckDrawingUnitsToEqualMeter return true, if Units of drawing = Meters and false if isn't.

- *boolean* _ChangeDrawingUnitsToMetersBool_: Method ChangeDrawingUnitsToMetersBool changes Unit's system of drawing to Metric with input bool parameter = false (from f.e. CheckDrawingUnitsToEqualMeter). Otherwise Unit's system resetting to Imperial

### class __Geometry__

This class is thought to contain methods to creation geometry features or work with Acad's geometry. [This article] (https://zen.yandex.ru/media/id/5d0dba97ecd5cf00afaf2938/autocad-3d-api-i-osm-naznachaem-materialy-graniam-3dtel-61cd4b2205c49671f55b07af) contains some info about methods.

- *void* __SetAcadPointsStyle__: Set representation for AutoCAD's points (_point). Look https://clck.ru/af4Um to find styles (nums)

- *ObjectId* _CreateAcadPoint_: Create AutoCAD's object-point by Dynamo's point and option include Z-cordinate. Return an object id of item.

### class __Other__

This class contains different nodes. 

- *void* _CreateMLeaderByPoint_: Create an AutoCAD's MLeader's object by two points and text's string. [Read more in that article] (https://zen.yandex.ru/media/id/5d0dba97ecd5cf00afaf2938/civil-3d--dynamo-podhody-k-ocifrovke-topografii-chast-1-multivynoski-i-vyborka-dannyh-61c66f2a4d08f2221ddf96d2)

- *void* _GetMaterials_: Get document's materials with their names and ObjectId. Read more about using in charpter 2.4-2.5 [that article](https://zen.yandex.ru/media/id/5d0dba97ecd5cf00afaf2938/autocad-3d-api-i-osm-naznachaem-materialy-graniam-3dtel-61cd4b2205c49671f55b07af).


### class __Selection__

This class contains methods that extenf functionality to search objects in AutoCAD's objects by differenet conditions. Look [that article] (https://zen.yandex.ru/media/id/5d0dba97ecd5cf00afaf2938/autocad-i-ego-obekty-ispolzovanie-rtree-dlia-prostranstvennoi-indeksacii-dannyh-61c8acda95db9f294ad7b26c) for more information.

- *Dictionary* _GetDxfCodesToTypedValues_: Some of AutoCAD DxfCodes to search object's property

- *List< ObjectId >* _GetLinesByLength_: Getting new ObjectId collection with objects which length are satisfy "needing_length". About using that node read [that article, chapter 3.1] (https://zen.yandex.ru/media/id/5d0dba97ecd5cf00afaf2938/civil-3d--dynamo-podhody-k-ocifrovke-topografii-chast-1-multivynoski-i-vyborka-dannyh-61c66f2a4d08f2221ddf96d2).

- *List< string >* _GetHandlesByObjectsId_: Get Object's handle as string by it's ObjectId (List)

- *List< ObjectId >* _GetObjectIdsByObjects_AcadObj_: Get list with AutoCAD's internal ObjectId for input object collection (their's handle)

- *Autodesk.AutoCAD.DatabaseServicesOpenMode* _ModeForRead_: Read's mode (OpenMode.ForRead) as tag of returning objects after closing transaction

- *Autodesk.AutoCAD.DatabaseServicesOpenMode* _ModeForWrite_:  Write's mode (OpenMode.ForWrite) as tag of returning objects after closing transaction

- *List< Autodesk.AutoCAD.DynamoNodes.Object >* _GetAcadObjectsByObjectsId_: Get Autodesk.AutoCAD.DatabaseServices.DBObject by ObjectId

- *List< ObjectId >* _SelectObjectsByConditions_: Creating "search pattern" by dxf code (GetDxfCodesToTypedValues) and string's data. Read developer docs and article above!


### class __Solids__

- *Dictionary* _GetSolid3dFacesCentroids_: Get all centroids by faces of solid3d as Dictionary with faces's centroid coordinates (Dynamo point) and identificators (solid's handle and face's id). About using read at [that article] (https://zen.yandex.ru/media/id/5d0dba97ecd5cf00afaf2938/autocad-3d-api-i-osm-naznachaem-materialy-graniam-3dtel-61cd4b2205c49671f55b07af).

- *void* _SetMaterialByFacesCentroids_: Assign to each face by it's identificator a color or materials by boolean. 