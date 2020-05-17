using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;

public class EditorHelper
{

	/// <summary>
	/// 경로 계산 함수.
	/// </summary>
	/// <param name="p_clip"></param>
	/// <returns></returns>
	public static string GetPath(UnityEngine.Object p_clip)
	{
		string retString = string.Empty;
		retString = AssetDatabase.GetAssetPath(p_clip);
		string[] path_node = retString.Split('/'); //Assets/9.ResourcesData/Resources/Sound/BGM.wav
		bool findResource = false;
		for (int i = 0; i < path_node.Length - 1; i++)
		{
			if (findResource == false)
			{
				if (path_node[i] == "Resources")
				{
					findResource = true;
					retString = string.Empty;
				}
			}
			else
			{
				retString += path_node[i] + "/";
			}

		}

		return retString;
	}

	/// <summary>
	/// Data 리스트를 enum structure로 뽑아주는 함수.
	/// </summary>
	public static void CreateEnumStructure(string enumName, StringBuilder data)
	{
		string templateFilePath = "Assets/Editor/EnumTemplate.txt";

		string entittyTemplate = File.ReadAllText(templateFilePath);

		entittyTemplate = entittyTemplate.Replace("$DATA$", data.ToString());
		entittyTemplate = entittyTemplate.Replace("$ENUM$", enumName);
		string folderPath = "Assets/1.Scripts/GameData/";
		if (Directory.Exists(folderPath) == false)
		{
			Directory.CreateDirectory(folderPath);
		}

		string FilePath = folderPath + enumName + ".cs";
		if (File.Exists(FilePath))
		{
			File.Delete(FilePath);
		}
		File.WriteAllText(FilePath, entittyTemplate);
	}

}
