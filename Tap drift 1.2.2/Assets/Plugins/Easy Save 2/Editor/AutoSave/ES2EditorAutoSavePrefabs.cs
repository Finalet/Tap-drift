#if !UNITY_4 && !UNITY_2017_1_OR_NEWER
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;
using System.Linq;

public class ES2EditorAutoSavePrefabs : ES2EditorAutoSaveHierarchy
{
	public ES2EditorAutoSavePrefabs() : base()
	{
		ES2EditorAutoSaveUtility.RefreshPrefabAutoSaves();
	}

	protected override void GetColumns()
	{
		columns = new ES2EditorColumn[0];
		curves = new ES2EditorRowCurve[0];
		// Get ES2AutoSaves at top of hierarchy.
		ES2AutoSave[] autoSaves = ES2EditorAutoSaveUtility.globalMgr.prefabArray;

		foreach(ES2AutoSave autoSavePrefab in autoSaves)
			if(autoSavePrefab != null)
				GetColumnsForAutoSave(autoSavePrefab, 0);

		if(columns.Length == 0)
		{
			GUIStyle centerStyle = new GUIStyle(ES2EditorWindow.instance.style.contentTextStyle);
			centerStyle.stretchHeight = true;
			centerStyle.alignment = TextAnchor.MiddleCenter;
			EditorGUILayout.LabelField("To enable Auto Save for a prefab, right-click it in Project and select \'Easy Save 2 / Enable Auto Save for Prefab\'.", centerStyle);
			return;
		}
	}
}
#endif