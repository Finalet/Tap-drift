#if !UNITY_4
using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.IO;
using System.Resources;
using System.Reflection;

[Serializable]
public class ES2EditorWindow : EditorWindow
{
	const string windowStylePath = "Assets/Easy Save 2/GUI/ES2 Window Styles.prefab";
	private Vector2 scrollPos = Vector2.zero;

	[SerializeField]
	private ES2EditorMenu menu = null;
	[SerializeField]
	private ES2EditorWindowContent windowContent = null;
	[SerializeField]
	private ES2EditorWindowStyle _style = null;
	public ES2EditorWindowStyle style
	{
		get
		{
			if(_style == null)
				_style = ((GameObject)AssetDatabase.LoadAssetAtPath(windowStylePath, typeof(GameObject))).GetComponent<ES2EditorWindowStyle>();
			return _style;
		}
	}

	public static ES2EditorWindow instance = null;

	[MenuItem("Window/Easy Save 2", false, 1000)]
	public static void Init()
	{
		// New Editor window does not support Unity 4.
		if(Application.unityVersion[0] == '4')
		{
			EditorUtility.DisplayDialog("Not supported in Unity 4", "The Easy Save Window is only supported on Unity 5 and above. Please use the tools found in 'Assets > Easy Save 2' instead.", "Ok");
			return;
		}
		
		ES2EditorWindow window = (ES2EditorWindow)EditorWindow.GetWindow<ES2EditorWindow>();
		window.Show();
	}

	/*
	 * 	Initialization should go here.
	 */
	public void OnEnable()
	{
		instance = this;
	}

	public void OnHierarchyChange()
	{
		if(windowContent != null)
		{
			windowContent.OnHierarchyChange();
			Repaint();
		}
	}

	public void OnProjectChange()
	{
		if(windowContent != null)
		{
			windowContent.OnProjectChange();
			Repaint();
		}
	}

	private void SetGUIStyles()
	{
		/*GUIStyle verticalScrollbarThumbStyle = new GUIStyle(GUI.skin.verticalScrollbarThumb);
		verticalScrollbarThumbStyle.normal.background = es2LogoGrey16x16;
		GUI.skin.verticalScrollbarThumb = verticalScrollbarThumbStyle;

		GUIStyle horizontalScrollbarThumbStyle = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
		horizontalScrollbarThumbStyle.normal.background = es2LogoGrey16x16;
		GUI.skin.horizontalScrollbarThumb = horizontalScrollbarThumbStyle;

		GUIStyle horizontalScrollbarLeftButtonStyle = new GUIStyle(GUI.skin.horizontalScrollbarLeftButton);
		horizontalScrollbarLeftButtonStyle.normal.background = es2LogoGrey16x16;
		GUI.skin.horizontalScrollbarLeftButton = horizontalScrollbarLeftButtonStyle;*/
	}

	public void OnGUI()
	{
		SetWindowTitleAndIcon(this, "Easy Save", style.windowIcon);

		if(menu == null)
			menu = new ES2EditorMenu();

		SetGUIStyles();
		SetBackgroundColor(style.windowBackground);

		menu.Draw();

		Rect rect = EditorGUILayout.GetControlRect(false, 62);

		GUI.BeginGroup(rect);

		EditorGUI.LabelField(new Rect(0,0,200,200), menu.SelectedMenu + "/" + menu.SelectedSubMenu, style.titleStyle);

		GUI.EndGroup();

		EditorGUILayout.BeginScrollView(scrollPos);

		MainPanel();

		EditorGUILayout.EndScrollView();

	}

	private void MainPanel()
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		string selectedMenuItem = menu.SelectedMenu;
		string selectedSubMenuItem 	= menu.SelectedSubMenu;
		if(selectedMenuItem == "Auto Save")
        {
#if UNITY_2017_1_OR_NEWER
            EditorGUILayout.LabelField("Easy Save 2 Auto Save was deprected in Unity 2017. Use Easy Save 3 Auto Save instead.");
#else

            if (selectedSubMenuItem == "Scene")
			{
				if(windowContent == null  || windowContent.GetType() != typeof(ES2EditorAutoSaveScene))
					windowContent = new ES2EditorAutoSaveScene();
				windowContent.Draw();
			}

			if(selectedSubMenuItem == "Prefabs")
			{
				if(windowContent == null || windowContent.GetType() != typeof(ES2EditorAutoSavePrefabs))
					windowContent = new ES2EditorAutoSavePrefabs();
				windowContent.Draw();
			}

			if(selectedSubMenuItem == "Settings")
			{
				if(windowContent == null || windowContent.GetType() != typeof(ES2EditorAutoSaveSettings))
					windowContent = new ES2EditorAutoSaveSettings();
				windowContent.Draw();
			}
#endif
        }
		else if(selectedMenuItem == "Settings")
		{
			if(selectedSubMenuItem == "General")
			{
				if(windowContent == null  || windowContent.GetType() != typeof(ES2EditorSettingsGeneral))
					windowContent = new ES2EditorSettingsGeneral();
				windowContent.Draw();
			}
			else if(selectedSubMenuItem == "Tools")
			{
				if(windowContent == null  || windowContent.GetType() != typeof(ES2EditorSettingsTools))
					windowContent = new ES2EditorSettingsTools();
				windowContent.Draw();
			}
			else if(selectedSubMenuItem == "Information")
			{
				if(windowContent == null  || windowContent.GetType() != typeof(ES2EditorSettingsInformation))
					windowContent = new ES2EditorSettingsInformation();
				windowContent.Draw();
			}
			else if(selectedSubMenuItem == "Change Log")
			{
				if(windowContent == null  || windowContent.GetType() != typeof(ES2EditorSettingsChangeLog))
					windowContent = new ES2EditorSettingsChangeLog();
				windowContent.Draw();
			}
			else if(selectedSubMenuItem == "Readme")
			{
				if(windowContent == null  || windowContent.GetType() != typeof(ES2EditorSettingsReadme))
					windowContent = new ES2EditorSettingsReadme();
				windowContent.Draw();
			}
		}

		EditorGUILayout.EndScrollView();
	}

	private static void SetWindowTitleAndIcon(EditorWindow window, string windowTitle, Texture tex)
	{
		PropertyInfo p = typeof(EditorWindow).GetProperty("titleContent", BindingFlags.Instance | BindingFlags.Public);
		if(p == null)
			return;

		GUIContent guiContent = new GUIContent(windowTitle, tex);
		p.SetValue(window, guiContent, null);

	}

	private void SetBackgroundColor(Texture2D texture)
	{
		GUI.DrawTexture(new Rect(0f, 0f, position.width, position.height), texture);
	}

	private void Error(string message)
	{
		Debug.LogError(message);
	}
}

public interface ES2EditorWindowContent
{
	void OnHierarchyChange();
	void OnProjectChange();
	void Draw();
}
#endif