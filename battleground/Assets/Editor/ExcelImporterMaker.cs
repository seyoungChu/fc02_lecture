//#pragma warning disable 0219

using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using System.Collections.Generic;
using System.Text;

public class ExcelImporterMaker : EditorWindow
{
    private enum ValueType : int
    {
        BOOL,
        STRING,
        INT,
        FLOAT,
        DOUBLE,
    }

    private string filePath = string.Empty;
    private bool sepalateSheet = false;
    private List<ExcelRowParameter> typeList = new List<ExcelRowParameter>();
    private List<ExcelSheetParameter> sheetList = new List<ExcelSheetParameter>();
    private string className = string.Empty;
    private string StructureClassName = string.Empty;
    private string DataClassName = string.Empty;
    private string TabClassName = string.Empty;
    private string fileName = string.Empty;
    private static string s_key_prefix = "seyoung.exel-importer-maker.";

    private class ExcelSheetParameter
    {
        public string sheetName;
        public bool isEnable;
    }

    private class ExcelRowParameter
    {
        public ValueType type;
        public string name;
        public bool isEnable;
        public bool isArray;
        public ExcelRowParameter nextArrayItem;
    }


    private Vector2 curretScroll = Vector2.zero;

    void OnGUI()
    {
        GUILayout.Label("making importer", EditorStyles.boldLabel);
        className = EditorGUILayout.TextField("class name", className);
        sepalateSheet = EditorGUILayout.Toggle("sepalate sheet", sepalateSheet);

        EditorPrefs.SetBool(s_key_prefix + fileName + ".separateSheet", sepalateSheet);

        if (GUILayout.Button("create"))
        {
            EditorPrefs.SetString(s_key_prefix + fileName + ".className", className);
            Debug.Log(s_key_prefix + fileName + ".className");
            ExportEntity();
            ExportImporter();
            //ExportStruct();
            //ExportData();
            //ExportTab();
            Debug.Log(filePath);
            AssetDatabase.ImportAsset(filePath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Close();
        }

        // selecting sheets

        EditorGUILayout.LabelField("sheet settings");
        EditorGUILayout.BeginVertical("box");
        foreach (ExcelSheetParameter sheet in sheetList)
        {
            GUILayout.BeginHorizontal();
            sheet.isEnable = EditorGUILayout.BeginToggleGroup("enable", sheet.isEnable);
            EditorGUILayout.LabelField(sheet.sheetName);
            EditorGUILayout.EndToggleGroup();
            EditorPrefs.SetBool(s_key_prefix + fileName + ".sheet." + sheet.sheetName, sheet.isEnable);
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();

        // selecting parameters
        EditorGUILayout.LabelField("parameter settings");
        curretScroll = EditorGUILayout.BeginScrollView(curretScroll);
        EditorGUILayout.BeginVertical("box");
        string lastCellName = string.Empty;
        foreach (ExcelRowParameter cell in typeList)
        {
            if (cell.isArray && lastCellName != null && cell.name.Equals(lastCellName))
            {
                continue;
            }

            cell.isEnable = EditorGUILayout.BeginToggleGroup("enable", cell.isEnable);
            if (cell.isArray)
            {
                EditorGUILayout.LabelField("---[array]---");
            }

            GUILayout.BeginHorizontal();
            cell.name = EditorGUILayout.TextField(cell.name);
            cell.type = (ValueType) EditorGUILayout.EnumPopup(cell.type, GUILayout.MaxWidth(100));
            EditorPrefs.SetInt(s_key_prefix + fileName + ".type." + cell.name, (int) cell.type);
            GUILayout.EndHorizontal();

            EditorGUILayout.EndToggleGroup();
            lastCellName = cell.name;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    [MenuItem("Assets/XLS Import Settings...")]
    static void ExportExcelToAssetbundle()
    {
        foreach (UnityEngine.Object obj in Selection.objects)
        {
            var window = ScriptableObject.CreateInstance<ExcelImporterMaker>();
            window.filePath = AssetDatabase.GetAssetPath(obj);
            window.fileName = Path.GetFileNameWithoutExtension(window.filePath);


            using (FileStream stream = File.Open(window.filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook book = new HSSFWorkbook(stream);

                for (int i = 0; i < book.NumberOfSheets; ++i)
                {
                    ISheet s = book.GetSheetAt(i);
                    ExcelSheetParameter sht = new ExcelSheetParameter();
                    sht.sheetName = s.SheetName;
                    sht.isEnable =
                        EditorPrefs.GetBool(s_key_prefix + window.fileName + ".sheet." + sht.sheetName, true);
                    window.sheetList.Add(sht);
                }

                ISheet sheet = book.GetSheetAt(0);

                window.className = EditorPrefs.GetString(s_key_prefix + window.fileName + ".className",
                    "Entity_" + sheet.SheetName);

                window.sepalateSheet = EditorPrefs.GetBool(s_key_prefix + window.fileName + ".separateSheet");

                IRow titleRow = sheet.GetRow(0);
                IRow dataRow = sheet.GetRow(1);
                for (int i = 0; i < titleRow.LastCellNum; i++)
                {
                    ExcelRowParameter lastParser = null;
                    ExcelRowParameter parser = new ExcelRowParameter();
                    parser.name = titleRow.GetCell(i).StringCellValue;
                    parser.isArray = parser.name.Contains("[]");
                    if (parser.isArray)
                    {
                        parser.name = parser.name.Remove(parser.name.LastIndexOf("[]"));
                    }

                    ICell cell = dataRow.GetCell(i);
                    if (cell == null)
                        continue;

                    // array support
                    if (window.typeList.Count > 0)
                    {
                        lastParser = window.typeList[window.typeList.Count - 1];
                        if (lastParser.isArray && parser.isArray && lastParser.name.Equals(parser.name))
                        {
                            // trailing array items must be the same as the top type
                            parser.isEnable = lastParser.isEnable;
                            parser.type = lastParser.type;
                            lastParser.nextArrayItem = parser;
                            window.typeList.Add(parser);
                            continue;
                        }
                    }

                    if (cell.CellType != NPOI.SS.UserModel.CellType.Unknown &&
                        cell.CellType != NPOI.SS.UserModel.CellType.BLANK)
                    {
                        parser.isEnable = true;

                        try
                        {
                            if (EditorPrefs.HasKey(s_key_prefix + window.fileName + ".type." + parser.name))
                            {
                                parser.type =
                                    (ValueType) EditorPrefs.GetInt(
                                        s_key_prefix + window.fileName + ".type." + parser.name);
                            }
                            else
                            {
                                string sampling = cell.StringCellValue;
                                parser.type = ValueType.STRING;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(e.Message);
                        }

                        try
                        {
                            if (EditorPrefs.HasKey(s_key_prefix + window.fileName + ".type." + parser.name))
                            {
                                parser.type =
                                    (ValueType) EditorPrefs.GetInt(
                                        s_key_prefix + window.fileName + ".type." + parser.name);
                            }
                            else
                            {
                                double sampling = cell.NumericCellValue;
                                parser.type = ValueType.INT;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(e.Message);
                        }

                        try
                        {
                            if (EditorPrefs.HasKey(s_key_prefix + window.fileName + ".type." + parser.name))
                            {
                                parser.type =
                                    (ValueType) EditorPrefs.GetInt(
                                        s_key_prefix + window.fileName + ".type." + parser.name);
                            }
                            else
                            {
                                bool sampling = cell.BooleanCellValue;
                                parser.type = ValueType.BOOL;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(e.Message);
                        }
                    }

                    window.typeList.Add(parser);
                }

                window.Show();
            }
        }
    }

    void ExportEntity()
    {
        string templateFilePath =
            (sepalateSheet) ? "Assets/Editor/EntityTemplate2.txt" : "Assets/Editor/EntityTemplate.txt";
        string entittyTemplate = File.ReadAllText(templateFilePath);
        StringBuilder builder = new StringBuilder();
        bool isInbetweenArray = false;
        foreach (ExcelRowParameter row in typeList)
        {
            if (row.isEnable)
            {
                if (!row.isArray)
                {
                    builder.AppendLine();
                    builder.AppendFormat("		public {0} {1};", row.type.ToString().ToLower(), row.name);
                }
                else
                {
                    if (!isInbetweenArray)
                    {
                        builder.AppendLine();
                        builder.AppendFormat("		public {0}[] {1};", row.type.ToString().ToLower(), row.name);
                    }

                    isInbetweenArray = (row.nextArrayItem != null);
                }
            }
        }

        entittyTemplate = entittyTemplate.Replace("$Types$", builder.ToString());
        entittyTemplate = entittyTemplate.Replace("$ExcelData$", className);

        Directory.CreateDirectory("Assets/Classes/");
        string FilePath = "Assets/Classes/" + className + ".cs";
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }

        File.WriteAllText(FilePath, entittyTemplate);
    }

    void ExportImporter()
    {
        string templateFilePath =
            (sepalateSheet) ? "Assets/Editor/ExportTemplate2.txt" : "Assets/Editor/ExportTemplate.txt";

        string importerTemplate = File.ReadAllText(templateFilePath);

        StringBuilder builder = new StringBuilder();
        StringBuilder sheetListbuilder = new StringBuilder();
        int rowCount = 0;
        string tab = "					";
        bool isInbetweenArray = false;

        //$SheetList$
        foreach (ExcelSheetParameter sht in sheetList)
        {
            if (sht.isEnable)
            {
                sheetListbuilder.Append("\"" + sht.sheetName + "\",");
            }

            /*
            if (sht != sheetList [sheetList.Count - 1])
            {
                sheetListbuilder.Append(",");
            }
            */
        }

        foreach (ExcelRowParameter row in typeList)
        {
            if (row.isEnable)
            {
                if (!row.isArray)
                {
                    builder.AppendLine();
                    switch (row.type)
                    {
                        case ValueType.BOOL:
                            builder.AppendFormat(
                                tab +
                                "cell = row.GetCell({1}); p.{0} = (cell == null ? false : cell.BooleanCellValue);",
                                row.name, rowCount);
                            break;
                        case ValueType.DOUBLE:
                            builder.AppendFormat(
                                tab + "cell = row.GetCell({1}); p.{0} = (cell == null ? 0.0 : cell.NumericCellValue);",
                                row.name, rowCount);
                            break;
                        case ValueType.INT:
                            builder.AppendFormat(
                                tab +
                                "cell = row.GetCell({1}); p.{0} = (int)(cell == null ? 0 : cell.NumericCellValue);",
                                row.name, rowCount);
                            break;
                        case ValueType.FLOAT:
                            builder.AppendFormat(
                                tab +
                                "cell = row.GetCell({1}); p.{0} = (float)(cell == null ? 0 : cell.NumericCellValue);",
                                row.name, rowCount);
                            break;
                        case ValueType.STRING:
                            builder.AppendFormat(
                                tab + "cell = row.GetCell({1}); p.{0} = (cell == null ? \"\" : cell.StringCellValue);",
                                row.name, rowCount);
                            break;
                    }
                }
                else
                {
                    // only the head of array should generate code

                    if (!isInbetweenArray)
                    {
                        int arrayLength = 0;
                        for (ExcelRowParameter r = row; r != null; r = r.nextArrayItem, ++arrayLength)
                        {
                        }

                        builder.AppendLine();
                        switch (row.type)
                        {
                            case ValueType.BOOL:
                                builder.AppendFormat(tab + "p.{0} = new bool[{1}];", row.name, arrayLength);
                                break;
                            case ValueType.DOUBLE:
                                builder.AppendFormat(tab + "p.{0} = new double[{1}];", row.name, arrayLength);
                                break;
                            case ValueType.INT:
                                builder.AppendFormat(tab + "p.{0} = new int[{1}];", row.name, arrayLength);
                                break;
                            case ValueType.FLOAT:
                                builder.AppendFormat(tab + "p.{0} = new float[{1}];", row.name, arrayLength);
                                break;
                            case ValueType.STRING:
                                builder.AppendFormat(tab + "p.{0} = new string[{1}];", row.name, arrayLength);
                                break;
                        }

                        for (int i = 0; i < arrayLength; ++i)
                        {
                            builder.AppendLine();
                            switch (row.type)
                            {
                                case ValueType.BOOL:
                                    builder.AppendFormat(
                                        tab +
                                        "cell = row.GetCell({1}); p.{0}[{2}] = (cell == null ? false : cell.BooleanCellValue);",
                                        row.name, rowCount + i, i);
                                    break;
                                case ValueType.DOUBLE:
                                    builder.AppendFormat(
                                        tab +
                                        "cell = row.GetCell({1}); p.{0}[{2}] = (cell == null ? 0.0 : cell.NumericCellValue);",
                                        row.name, rowCount + i, i);
                                    break;
                                case ValueType.INT:
                                    builder.AppendFormat(
                                        tab +
                                        "cell = row.GetCell({1}); p.{0}[{2}] = (int)(cell == null ? 0 : cell.NumericCellValue);",
                                        row.name, rowCount + i, i);
                                    break;
                                case ValueType.FLOAT:
                                    builder.AppendFormat(
                                        tab +
                                        "cell = row.GetCell({1}); p.{0}[{2}] = (float)(cell == null ? 0.0 : cell.NumericCellValue);",
                                        row.name, rowCount + i, i);
                                    break;
                                case ValueType.STRING:
                                    builder.AppendFormat(
                                        tab +
                                        "cell = row.GetCell({1}); p.{0}[{2}] = (cell == null ? \"\" : cell.StringCellValue);",
                                        row.name, rowCount + i, i);
                                    break;
                            }
                        }
                    }

                    isInbetweenArray = (row.nextArrayItem != null);
                }
            }

            rowCount += 1;
        }

        importerTemplate = importerTemplate.Replace("$IMPORT_PATH$", filePath);
        importerTemplate = importerTemplate.Replace("$ExportAssetDirectry$", Path.GetDirectoryName(filePath));
        importerTemplate = importerTemplate.Replace("$EXPORT_PATH$", Path.ChangeExtension(filePath, ".asset"));
        importerTemplate = importerTemplate.Replace("$ExcelData$", className);
        importerTemplate = importerTemplate.Replace("$SheetList$", sheetListbuilder.ToString());
        importerTemplate = importerTemplate.Replace("$EXPORT_DATA$", builder.ToString());
        importerTemplate = importerTemplate.Replace("$ExportTemplate$", fileName + "Importer");

        Directory.CreateDirectory("Assets/Classes/Editor/");

        string FilePath = "Assets/Classes/Editor/" + fileName + "Importer.cs";
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }

        File.WriteAllText(FilePath, importerTemplate);
    }


    void ExportStruct()
    {
        string templateFilePath = "Assets/Editor/StructureTemplate.txt";
        string entittyTemplate = File.ReadAllText(templateFilePath);
        StringBuilder builder = new StringBuilder();
        bool isInbetweenArray = false;
        foreach (ExcelRowParameter row in typeList)
        {
            if (row.isEnable)
            {
                if (!row.isArray)
                {
                    builder.AppendLine();
                    builder.AppendFormat("		public {0} {1};", row.type.ToString().ToLower(), row.name);
                }
                else
                {
                    if (!isInbetweenArray)
                    {
                        builder.AppendLine();
                        builder.AppendFormat("		public {0}[] {1};", row.type.ToString().ToLower(), row.name);
                    }

                    isInbetweenArray = (row.nextArrayItem != null);
                }
            }
        }

        entittyTemplate = entittyTemplate.Replace("$Types$", builder.ToString());
        StructureClassName = className + "Param";
        entittyTemplate = entittyTemplate.Replace("$ExcelData$", StructureClassName);

        Directory.CreateDirectory("Assets/Classes/");
        string FilePath = "Assets/Classes/" + StructureClassName + ".cs";
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }

        File.WriteAllText(FilePath, entittyTemplate);
    }

    void ExportData()
    {
        string templateFilePath = "Assets/Editor/DataTemplate.txt";
        string entittyTemplate = File.ReadAllText(templateFilePath);
        StringBuilder builder = new StringBuilder();
        //bool isInbetweenArray = false;

        this.DataClassName = className + "Data";


        entittyTemplate = entittyTemplate.Replace("$ExcelData$", this.StructureClassName);
        entittyTemplate = entittyTemplate.Replace("$ExcelDataName$", StructureClassName.ToLower());
        entittyTemplate = entittyTemplate.Replace("$ExcelDataNameString$", "\"" + this.DataClassName.ToLower() + "\"");

        entittyTemplate = entittyTemplate.Replace("$TitleName$", "\"" + this.StructureClassName.ToUpper() + "S" + "\"");
        entittyTemplate = entittyTemplate.Replace("$SubTitle$", "\"" + this.StructureClassName.ToUpper() + "\"");

        entittyTemplate = entittyTemplate.Replace("$DataClass$", this.DataClassName);

        Directory.CreateDirectory("Assets/Classes/");
        string FilePath = "Assets/Classes/" + this.DataClassName + ".cs";
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }

        File.WriteAllText(FilePath, entittyTemplate);
    }

    void ExportTab()
    {
        string templateFilePath = "Assets/Editor/TabTemplate.txt";
        string entittyTemplate = File.ReadAllText(templateFilePath);
        StringBuilder builder = new StringBuilder();
        //bool isInbetweenArray = false;

        this.TabClassName = this.className + "Tab";

        entittyTemplate = entittyTemplate.Replace("$TabClass$", this.TabClassName);
        entittyTemplate = entittyTemplate.Replace("$DataHolderDatas$", "DataHolder." + StructureClassName + "s()");
        entittyTemplate = entittyTemplate.Replace("$DataHolderData$", "DataHolder." + StructureClassName);

        Directory.CreateDirectory("Assets/Classes/Editor/");
        string FilePath = "Assets/Classes/Editor/" + this.TabClassName + ".cs";
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }

        File.WriteAllText(FilePath, entittyTemplate);
    }
}