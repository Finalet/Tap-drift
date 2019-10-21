#if !UNITY_4 && !UNITY_2017_1_OR_NEWER
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;
using System.Linq;

public class ES2EditorAutoSaveScene : ES2EditorAutoSaveHierarchy
{
	public ES2EditorAutoSaveScene() : base()
	{
		if(ES2EditorAutoSaveUtility.AutomaticallyRefreshSceneAutoSaves)
			ES2EditorAutoSaveUtility.RefreshSceneAutoSaves();
	}

	protected override void GetColumns()
	{
		columns = new ES2EditorColumn[0];
		curves = new ES2EditorRowCurve[0];

		if(ES2EditorAutoSaveUtility.mgr == null)
		{
			GUIStyle style = new GUIStyle(ES2EditorWindow.instance.style.contentButtonStyle);
			// Stretch style to full height.
			style.stretchWidth = true;
			style.stretchHeight = true;

			if(GUILayout.Button("Click to enable Auto Save for this scene", ES2EditorWindow.instance.style.contentButtonStyle))
			   ES2EditorAutoSaveUtility.AddManagerToScene();
			return;
		}

		// Get ES2AutoSaves at top of hierarchy.
		ES2AutoSave[] autoSaves = ES2EditorAutoSaveUtility.mgr.sceneObjects;

		foreach(ES2AutoSave autoSave in autoSaves)
			if(autoSave != null && autoSave.transform != null)
				if(autoSave.transform.parent == null)
					GetColumnsForAutoSave(autoSave, 0);
	}
}
#endif