using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Reflection;
using System.IO;

public class ES2EditorTemplates
{
	public static string GetTemplate(string templateName)
	{
		return File.ReadAllText (Application.dataPath + "/Plugins/Easy Save 2/Templates/" + templateName + ".bytes");
	}
}