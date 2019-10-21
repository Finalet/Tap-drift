/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal static class ES3Debug
{
	public enum Category {GameObject, Component, Member, IO, Reflection, Other}
	public static Category[] categories = new Category[]{Category.GameObject, Category.Component, Category.IO, Category.Member, Category.Other, Category.Reflection};
	public static bool enabled = true;

	public static int indentLevel = 0;
	private static string indentStr = "-";


	public static void Log(string msg, Category category, int indent=0)
	{
		#if DEBUG

		if(!enabled || !CategoryIsEnabled(category))
			return;
		
		Debug.Log(GetIndent() + msg);
		indentLevel += indent;

		#endif
	}
		
	public static void LogError(string msg, Category category)
	{
		#if DEBUG
		if(!enabled || ! CategoryIsEnabled(category))
			return;

		Debug.LogError(GetIndent() + msg);

		#endif
	}

	public static void LogWarning(string msg, Category category)
	{
		#if DEBUG
		if(!enabled || ! CategoryIsEnabled(category))
			return;

		Debug.LogWarning(GetIndent() + msg);

		#endif
	}

	public static void ResetIndent()
	{
		indentLevel = 0;
	}

	private static string GetIndent()
	{
		string str = "";
		for(int i = 0; i < indentLevel; i++)
			str += indentStr;
		return str;
	}

	private static bool CategoryIsEnabled(Category category)
	{
		for(int i = 0; i < categories.Length; i++)
			if(categories[i] == category)
				return true;
		return false;
	}
}*/
