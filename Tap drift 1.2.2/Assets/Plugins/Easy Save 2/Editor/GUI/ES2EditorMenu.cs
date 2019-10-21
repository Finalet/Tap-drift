#if !UNITY_4
using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
public class ES2EditorMenu
{
	private int selectedMenuItemIndex = 0;
	private int selectedSubMenuItemIndex = 0;

	string[] mainButtons = new string[]{"Auto Save", "Settings"};
	string[][] subButtons = new string[][]
	{ 
		new string[]{"Scene", "Prefabs", "Settings"}, 
		new string[]{"General", "Tools", "Information", "Change Log", "Readme"}
	};

	public ES2EditorMenu()
	{
	}
		

	public void Draw()
	{
		ES2EditorWindowStyle style = ES2EditorWindow.instance.style;

		float mainButtonHeight = style.menuMainButtonStyle.fixedHeight;
		float menuHeight = style.menuStyle.fixedHeight;
		style.menuSubButtonStyle.fixedHeight = menuHeight-mainButtonHeight;

		//SetGUIStyles();
		Rect rect = EditorGUILayout.GetControlRect(false, menuHeight, style.menuStyle);
		
		GUI.BeginGroup(rect, style.menuStyle);
		
		for (int i = 0; i < mainButtons.Length; i++)
			if (GUI.Button (new Rect ((rect.width / mainButtons.Length) * i, rect.y, rect.width / mainButtons.Length, mainButtonHeight), new GUIContent (mainButtons [i], style.windowIcon), style.menuMainButtonStyle)) {
				selectedSubMenuItemIndex = 0;
				selectedMenuItemIndex = i;
			}
		
		string[] activeSubButtons = subButtons[selectedMenuItemIndex];
		
		for(int i=0; i<activeSubButtons.Length; i++)
			if(GUI.Button(new Rect((rect.width/activeSubButtons.Length)*i, rect.y+mainButtonHeight, rect.width/activeSubButtons.Length, menuHeight-mainButtonHeight), activeSubButtons[i], style.menuSubButtonStyle))
				selectedSubMenuItemIndex = i;
		
		GUI.EndGroup();
	}

	public string SelectedMenu
	{
		get{ return mainButtons[selectedMenuItemIndex]; }
	}

	public string SelectedSubMenu
	{
		get{ return subButtons[selectedMenuItemIndex][selectedSubMenuItemIndex]; }
	}
}
#endif
