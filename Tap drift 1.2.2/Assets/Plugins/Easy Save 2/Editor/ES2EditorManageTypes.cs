using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

public class ES2EditorManageTypes : EditorWindow
{
	private const float typeListWidth = 320f;
	
	private static Vector2 leftScrollPosition = Vector2.zero;
	private static Vector2 rightScrollPosition = Vector2.zero;
	
	private static List<Type> types = new List<Type>();
	private static string[] typeNames = new string[0];

	private static Dictionary<Type, ES2Type> es2Types = new Dictionary<Type, ES2Type>();

	// The index of the currently selected type.
	private static int selected = -1;
	// The index of the previously selected type.
	private static int previouslySelected = -1;
	
	private static string searchTerm = "";
	
	// Properties for the currently selected type.
	private static List<ES2EditorProperty> properties = new List<ES2EditorProperty>();

	private const string file_prefix = @"ES2UserType_";
	private const string directory_path = "/Easy Save 2/Types/";
	
	private const string es2InitDirectory = "/Easy Save 2/";
	private const string es2InitFilename = "ES2Init.cs";

	[MenuItem ("Assets/Easy Save 2/Manage Types...", false, 1002)]
	public static void OpenWindow()
	{
		// Get existing open window or if none, make a new one:
		ES2EditorManageTypes window = (ES2EditorManageTypes)EditorWindow.GetWindow (typeof (ES2EditorManageTypes));
		window.minSize = new Vector2(720f, 512f);
	}

	public void Awake()
	{
		GetTypes();
		es2Types = ES2EditorReflection.GetSupportedTypes();
	}

	public void OnGUI()
	{
		try
		{	
			// If type list hasn't been initialised, initialise it.
			if(types.Count == 0)
			{
				GetTypes();
				es2Types = ES2EditorReflection.GetSupportedTypes();
			}
			
			GUILayout.Label("Easy Save 2: Manage Types", EditorStyles.largeLabel);
			//GUILayout.Label("Select a type to add support for it.", EditorStyles.boldLabel);
		
			GUI.skin.button.alignment = TextAnchor.MiddleCenter;
			if(GUILayout.Button (new GUIContent("Refresh ES2Init", "Use this to reset the type list if you manually remove an ES2Type, or if it becomes corrupt.")))
			{
				CreateES2UserInit();
				AssetDatabase.Refresh();
			}
			
			DisplaySearchField();
			
			GUILayout.BeginHorizontal(GUILayout.MinWidth(512f));
			
			GUI.skin.button.alignment = TextAnchor.MiddleLeft;
			// TYPE LIST
			leftScrollPosition = EditorGUILayout.BeginScrollView(leftScrollPosition,GUILayout.Width(typeListWidth));
			selected = GUILayout.SelectionGrid(selected, typeNames, 1);
			// If the selected type has changed ...
			if(selected != previouslySelected)
				OnTypeChange();
			EditorGUILayout.EndScrollView();
				
			// TYPE INFO
				
			// If a type is selected
			rightScrollPosition = EditorGUILayout.BeginScrollView(rightScrollPosition, GUILayout.MinWidth(400));
			if(selected != -1)
				DisplayType(types[selected]);
			else
				GUILayout.Label("<- Please select a type from the left.", EditorStyles.boldLabel);
			EditorGUILayout.EndScrollView();
	
	
			EditorGUILayout.EndHorizontal();
		}
		catch(System.Exception e)
		{
			Debug.LogError(e);
		}
	}
	
	private static void DisplaySearchField()
	{
		string searchFieldValue = EditorGUILayout.TextField(searchTerm, EditorStyles.toolbarTextField, GUILayout.Width(typeListWidth-25f));
		if(searchTerm != searchFieldValue)
		{
			selected = -1;
			OnTypeChange();
			searchTerm = searchFieldValue;
			GetTypes();
		}
	}
	
	public static void OnTypeChange()
	{
		previouslySelected = selected;
		// If no type is selected, do nothing.
		if(selected == -1)
			return;
		
		Type type = types[selected];
		
		// Get properties and fields from current type.
		properties = new List<ES2EditorProperty>();
		
		foreach(PropertyInfo info in type.GetProperties())
			properties.Add(new ES2EditorProperty(info));
			
		foreach(FieldInfo info in type.GetFields())
			properties.Add(new ES2EditorProperty(info));
		// Sort the properties.
		properties = properties.OrderByDescending(o=>o.isSupported).ToList();
		
	}

	public static void DisplayType(Type type)
	{
		// Type info title
		EditorGUILayout.LabelField(type.Name, EditorStyles.boldLabel);
		EditorGUILayout.LabelField(type.Assembly.GetName().Name + " | " + type.Namespace);
		EditorGUILayout.Space();
		
		ES2Type es2Type;
		if(es2Types.TryGetValue(type, out es2Type))
		{
			// If the ES2Type was defined in ES2.dll, it's a natively supported type.
			if(es2Type.GetType().Assembly.GetName().Name == "ES2")
				EditorGUILayout.HelpBox("This type is already built in to Easy Save. Modifying it here will override Easy Save's implementation.", MessageType.Warning);
			else
				EditorGUILayout.HelpBox("This type has already been added. Modifying or updating it will make previously saved data of this type incompatible with the new version.", MessageType.Warning);
		}
		
		if(typeof(ScriptableObject).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) == null)
			EditorGUILayout.HelpBox("Support can not be automatically added for this type as it does not have a parameterless constructor.\n\nTry manually adding support by adding the type and then editing the type file.", MessageType.Warning);	

		EditorGUILayout.BeginHorizontal();
		if(EditorGUILayout.Toggle("Select All", false))
			foreach(ES2EditorProperty property in properties)
				property.isSelected = true;

		if(EditorGUILayout.Toggle("Select None", false))
			foreach(ES2EditorProperty property in properties)
				property.isSelected = false;

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		foreach(ES2EditorProperty property in properties)
			property.DisplayToggle();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		DisplayButtons(types[selected]);
		EditorGUILayout.EndHorizontal();
	}
	
	/*
		Determines whether a type is supported.
		Returns a blank string if supported, or a message describing why it's not supported if not.
	*/
	public static string TypeIsSupported(Type type)
	{	
		// If this is a collection or array, see if the type of the contents are supported.
		if(ES2EditorReflection.IsCollectionType(type))
		{
			// If it's a generic collection ...
			if(type.IsGenericType)
			{
				// Check that it's a generic collection type supported by Easy Save.
				if(!(type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) ||
				     type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)) ||
				     type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Queue<>)) ||
				     type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Stack<>)) ||
				     type.GetGenericTypeDefinition().IsAssignableFrom(typeof(HashSet<>))))
					return "This type of collection is not currently supported.";
				else
				{
					foreach(Type arg in type.GetGenericArguments())
						if(!arg.IsEnum && !es2Types.ContainsKey(arg))
							return "A type inside this collection is not supported, but you may be able to add support for it.";
				}		
			}
			// Else if it's a normal array ...
			else if(type.IsArray)
			{
				// Check that it's a type of array that we support ...
				if(type.GetArrayRank() > 2)
					return "Only 1D and 2D arrays are supported.";
				else if(!type.GetElementType().IsEnum && !es2Types.ContainsKey(type.GetElementType()))
					return "The type of data inside this array is not supported, but you may be able to add support for it.";
			}
		}
		else if(type.IsEnum)
			return "";
		// If there's not an ES2Type for this ...
		else if(!es2Types.ContainsKey(type))
			return "Saving this type is not currently supported, but you may be able to add support for it.";
		
		return "";
	}
	
	private static void DisplayButtons(Type type)
	{
		bool fileExists = File.Exists( GetPathToES2Type(types[selected]) );
		
		if(fileExists)
		{
			if(GUILayout.Button("Update Type", GUILayout.ExpandWidth(false)))
				CreateTypeFile(type, properties);
			
			if(GUILayout.Button("Delete Type", GUILayout.ExpandWidth(false)))
			{
				AssetDatabase.DeleteAsset(GetRelativePathToES2Type(types[selected]));
				es2Types.Remove(types[selected]);
				CreateES2UserInit();
				AssetDatabase.Refresh();
			}
			
			if(GUILayout.Button("Edit Type", GUILayout.ExpandWidth(false)))
				UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(GetPathToES2Type(types[selected]), 1);
		}
		else
		{
			if(GUILayout.Button("Add Type", GUILayout.ExpandWidth(false)))
				CreateTypeFile(type, properties);
		}
	}
	
	/* Gets the path to an ES2Type file, whether it exists or not */
	private static string GetPathToES2Type(Type type)
	{
		return Application.dataPath + directory_path + file_prefix + GetES2TypeName(type) +".cs";
	}
	
	/* Gets the path to an ES2Type file relative to the Project folder, whether it exists or not */
	private static string GetRelativePathToES2Type(Type type)
	{
		return "Assets"+directory_path + file_prefix + GetES2TypeName(type) +".cs";
	}
	
	private static string GetES2TypeName(Type type)
	{
		return type.ToString().Replace(".", string.Empty).Replace("+", string.Empty);
	}

	/*
	 * 	Safely gets the name of a type.
	 * 	For example, nested types usually contain a '+', but this is replaced by this method.
	 */
	private static string GetTypeName(Type type)
	{
		string typeName = type.ToString();

		if(type.IsNested)
			typeName = typeName.Replace('+','.');
		
		// If it's a generic type, replace syntax with angled brackets.
		int genericArgumentCount = type.GetGenericArguments().Length;
		if(genericArgumentCount > 0)
			typeName = typeName.Replace("`"+genericArgumentCount+"[","<").Replace("]",">");
		
		return typeName;
	}
	
	public static string GetPathToES2Init()
	{
		return Application.dataPath + es2InitDirectory + es2InitFilename;
	}
	
	/* Creates an ES2Type file template with reader.Read and writer.Write calls automatically added, for the type at the given index. */
	private static void CreateTypeFile(Type type, List<ES2EditorProperty> properties)
	{
		string writes = "";
		string reads = "";

		foreach(ES2EditorProperty property in properties)
		{
			if(type.IsEnum)
			{
				writes += @"		writer.Write((int)data);"+"\n";
				break;
			}
			
			if(property.isSupported && property.isSelected)
			{
				writes += @"		writer.Write(data."+property.name+");\n";
				
				string typeName = GetTypeName(property.type);
				
				if(property.isCollection)
				{
					// Get the names of the collection types as comma seperated values.
					string genericParams = "";
					for(int i=0; i<property.collectionContentTypes.Length; i++)
					{
						if(i!=0)
							genericParams += ",";
						genericParams += GetTypeName(property.collectionContentTypes[i]);
					}
					
					reads += @"		data."+property.name+" = reader.Read"+property.collectionType+"<"+genericParams+">();\n";
				}
				else
					reads += @"		data."+property.name+" = reader.Read<"+typeName+">();\n";
			}
		}
		
		string namespace_string = "";
		if(!String.IsNullOrEmpty(type.Namespace) && type.Namespace != "UnityEngine")
			namespace_string = "using "+type.Namespace+";";
			
		string create_instance = ""; // Used when creating a new instance of this object.
		if((typeof(Component).IsAssignableFrom(type)))
			create_instance = "GetOrCreate<"+GetTypeName(type)+">()";
		else if((typeof(ScriptableObject).IsAssignableFrom(type)))
			create_instance = "ScriptableObject.CreateInstance<"+GetTypeName(type)+">()";
		else if(type.IsEnum)
			create_instance = "("+GetTypeName(type)+")reader.reader.ReadInt32()";
		else
			create_instance = "new "+GetTypeName(type)+"()";
		
		// If it's a value type, use value type template.
		string template = type.IsValueType ? ES2EditorTemplates.GetTemplate("ES2ValueTypeTemplate") : ES2EditorTemplates.GetTemplate("ES2TypeTemplate") ;
		string file;

		file = String.Format(template, file_prefix, GetES2TypeName(type), GetTypeName(type), writes, reads, namespace_string, create_instance);
		
		// Write the file.
		File.WriteAllText(GetPathToES2Type(types[selected]), file);
		CreateES2UserInit(type);
		AssetDatabase.Refresh();
	}

	public static void CreateES2UserInit()
	{
		CreateES2UserInit (null);
	}

	/* Recreates the ES2UserInit, adding to it the given type*/
	public static void CreateES2UserInit(Type type)
	{			
		string es2TypeList = "";
		string userTypeList = "";
		
		foreach(KeyValuePair<Type, ES2Type> es2Type in es2Types)
		{
			// Don't add the same type twice.
			if(es2Type.Key == type)
				continue;
			
			// Add ES2Types built in to ES2 to the list first, and then the user ones later.
			if(es2Type.Value.GetType().Assembly.GetName().Name == "Assembly-CSharp-firstpass")
				es2TypeList += @"		ES2TypeManager.types[typeof("+GetTypeName(es2Type.Key)+")] = new "+es2Type.Value.GetType ().Name+"();\n";
			else
				userTypeList += @"		ES2TypeManager.types[typeof("+GetTypeName(es2Type.Key)+")] = new "+es2Type.Value.GetType ().Name+"();\n";
		}
		// Add the type specified as a parameter.
		if(type != null)
			userTypeList += @"		ES2TypeManager.types[typeof("+GetTypeName(type)+")] = new "+file_prefix+GetES2TypeName(type)+"();\n";
		
		File.WriteAllText(GetPathToES2Init(), String.Format (ES2EditorTemplates.GetTemplate("ES2InitTemplate"), es2TypeList+userTypeList) );
	}

	/* Gets all of the types of the assemblies in the assemblies array, but filters out generic and abstract types */
	private static void GetTypes()
	{
		types = ES2EditorReflection.GetSupportableTypes(searchTerm);
		typeNames = new string[types.Count];
		for(int i=0; i<types.Count; i++)
			typeNames[i] = types[i].Name;
	}
}