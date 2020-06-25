using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Xml.Serialization;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

public class EntityTableImporter : AssetPostprocessor {
	private static readonly string filePath = "Assets/9.ResourcesData/Resources/Data/EntityTable.xls";
	private static readonly string exportPath = "Assets/9.ResourcesData/Resources/Data/EntityTable.asset";
	private static readonly string[] sheetNames = { "sheet", };
	
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string asset in importedAssets) {
			if (!filePath.Equals (asset))
				continue;
				
			ClassStats data = (ClassStats)AssetDatabase.LoadAssetAtPath (exportPath, typeof(ClassStats));
			if (data == null) {
				data = ScriptableObject.CreateInstance<ClassStats> ();
				AssetDatabase.CreateAsset ((ScriptableObject)data, exportPath);
				data.hideFlags = HideFlags.NotEditable;
			}
			
			data.sheets.Clear ();
			using (FileStream stream = File.Open (filePath, FileMode.Open, FileAccess.Read)) {
				IWorkbook book = new HSSFWorkbook (stream);
				
				foreach(string sheetName in sheetNames) {
					ISheet sheet = book.GetSheet(sheetName);
					if( sheet == null ) {
						Debug.LogError("[Data] sheet not found:" + sheetName);
						continue;
					}

					ClassStats.Sheet s = new ClassStats.Sheet ();
					s.name = sheetName;
				
					for (int i=1; i <= sheet.LastRowNum; i++) {
						IRow row = sheet.GetRow (i);
						ICell cell = null;
						
						ClassStats.Param p = new ClassStats.Param ();
						
					cell = row.GetCell(0); p.ID = (cell == null ? "" : cell.StringCellValue);
					p.AimOffset = new float[1];
					cell = row.GetCell(1); p.AimOffset[0] = (float)(cell == null ? 0.0 : cell.NumericCellValue);
					cell = row.GetCell(4); p.ChangeCoverChance = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(5); p.WeaponType = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(6); p.BulletDamage = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(7); p.ShotRateFactor = (float)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(8); p.ShotErrorRate = (float)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(9); p.Effect_MuzzleFlash = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(10); p.Effect_Shot = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(11); p.Effect_Sparks = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(12); p.Effect_BulletHole = (cell == null ? "" : cell.StringCellValue);
						s.list.Add (p);
					}
					data.sheets.Add(s);
				}
			}

			ScriptableObject obj = AssetDatabase.LoadAssetAtPath (exportPath, typeof(ScriptableObject)) as ScriptableObject;
			EditorUtility.SetDirty (obj);
		}
	}
}
