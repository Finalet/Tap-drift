#if !UNITY_4 && !UNITY_2017_1_OR_NEWER
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;
using System.Linq;

public abstract class ES2EditorAutoSaveHierarchy : ES2EditorWindowContent
{
	[NonSerialized]
	protected ES2EditorColumn[] columns = new ES2EditorColumn[0];
	[NonSerialized]
	protected ES2EditorRowCurve[] curves = new ES2EditorRowCurve[0];

	// The length of the first straight curve going from a selected row to a column.
	private const float firstCurveLength = 1.45f;

	public static Event currentEvent = null;

	protected abstract void GetColumns();

	// Detects whether we just updated the Auto Saves, so they don't need updating again.
	private static bool sceneAutoSavesMightNeedUpdating = true;

	public ES2EditorAutoSaveHierarchy()
	{
		SceneView.onSceneGUIDelegate += GetEvent;
		ES2EditorAutoSaveUtility.RefreshSceneAutoSaves();
	}

	/*
	 * 	Delegate used to get event info.
	 */
	public void GetEvent(SceneView sceneView)
	{
		currentEvent = Event.current;
	}
	
	public void OnHierarchyChange()
	{
		if(!ES2EditorAutoSaveUtility.AutomaticallyRefreshSceneAutoSaves)
			return;
		
		if(sceneAutoSavesMightNeedUpdating)
		{
			ES2EditorAutoSaveUtility.RefreshSceneAutoSaves();
			sceneAutoSavesMightNeedUpdating = false;
		}
		else
			sceneAutoSavesMightNeedUpdating = true;
	}

	public void OnProjectChange()
	{
		//ES2EditorAutoSaveUtility.RefreshPrefabAutoSaves();
	}

	public void Draw()
	{
		// Don't allow Auto Save to be modified when playing.
		if(Application.isPlaying)
		{
			EditorGUILayout.BeginHorizontal(ES2EditorWindow.instance.style.windowContentStyle);
			GUIStyle centerStyle = new GUIStyle(ES2EditorWindow.instance.style.contentTextStyle);
			centerStyle.stretchHeight = true;
			centerStyle.alignment = TextAnchor.MiddleCenter;
			EditorGUILayout.LabelField("Auto Save can not be modified in Play mode.", centerStyle);
			EditorGUILayout.EndHorizontal();
			return;
		}

		EditorGUILayout.BeginHorizontal(ES2EditorWindow.instance.style.windowContentStyle);

		GetColumns();
		DrawColumns();
		DrawCurves();

		EditorGUILayout.EndHorizontal();
	}

	protected void GetColumnsForAutoSave(ES2AutoSave autoSave, int hierarchyDepth)
	{
		ES2EditorColumn column = GetColumn(0);
		ES2EditorRow row = column.AddRow(autoSave.gameObject.name, autoSave, ES2EditorWindow.instance.style.saveButtonStyle, ES2EditorWindow.instance.style.saveButtonSelectedStyle, hierarchyDepth);

		if(autoSave.selected)
		{
			if(autoSave.selectionChanged)
				ES2EditorAutoSaveUtility.UpdateAutoSave(autoSave);

			GetComponentsColumnForAutoSave(autoSave, column, row);
		}

		if(autoSave.buttonSelected && autoSave.buttonSelectionChanged)
		{
			// Add support for any Components which are currently unsupported.
			Component[] components = autoSave.GetComponents<Component>();
			foreach(Component c in components)
			{
				// Handle unassigned components.
				if(c == null)
					continue;
				// If this Component isn't currently supported, add support for it and isn't an ES2AutoSave.
				if(!ES2EditorTypeUtility.TypeIsSupported(c.GetType()) 
					&& !typeof(ES2AutoSave).IsAssignableFrom(c.GetType())
					&& !typeof(ES2AutoSaveManager).IsAssignableFrom(c.GetType()))
						ES2EditorTypeUtility.AddType(c.GetType());
			}
		}
	
		foreach(Transform t in autoSave.transform)
		{
			ES2AutoSave child = ES2AutoSave.GetAutoSave(t.gameObject);
			if(child != null)
				GetColumnsForAutoSave(child, hierarchyDepth+1);
		}
	}

	protected void GetComponentsColumnForAutoSave(ES2AutoSave autoSave, ES2EditorColumn previousColumn, ES2EditorRow previousRow)
	{
		ES2EditorColumn column = GetColumn(1);

		ES2EditorRow firstRow = null;

		// GameObject instance variables. These have to be handled seperately.
		ES2AutoSaveVariableInfo[] instanceVariables = new ES2AutoSaveVariableInfo[]{autoSave.activeSelfVariable, autoSave.parentVariable, autoSave.nameVariable, autoSave.tagVariable, autoSave.layerVariable};
		for(int i=0; i<instanceVariables.Length; i++)
		{
			ES2AutoSaveVariableInfo variable = instanceVariables[i];

			ES2EditorRow newRow = column.AddRow(variable.name, variable, ES2EditorWindow.instance.style.saveButtonStyle, ES2EditorWindow.instance.style.saveButtonSelectedStyle, 0);
			if(firstRow == null)
				firstRow = newRow;

			SetTooltips(variable, newRow);

			// If this component was selected, also select it's Auto Save.
			if(variable.buttonSelectionChanged && variable.buttonSelected)
				autoSave.buttonSelected = true;

			// If the button for the Auto Save of this Component was deselected, deselect this one too.
			if(autoSave.buttonSelectionChanged && !autoSave.buttonSelected)
				newRow.obj.buttonSelected = false;

			// Get variables column if this Component is selected.
			if(variable.selected)
			{
				// Update this component if we've only just selected this Component.
				if(variable.selectionChanged)
					ES2EditorAutoSaveUtility.UpdateVariablesForVariable(variable);

				GetVariablesColumnForVariable(variable, column, newRow, 2);
			}
		}

		// Create rows for Component's attached to this GameObject.
		for(int i=0; i<autoSave.components.Count; i++)
		{
			ES2AutoSaveComponentInfo componentInfo = autoSave.components[i];
			ES2EditorRow newRow = column.AddRow(componentInfo.name, componentInfo, ES2EditorWindow.instance.style.saveButtonStyle, ES2EditorWindow.instance.style.saveButtonSelectedStyle, 0);
			if(firstRow == null)
				firstRow = newRow;

			SetTooltips(componentInfo, newRow);

			// If this component was selected ...
			if(componentInfo.buttonSelectionChanged && componentInfo.buttonSelected)
			{
				// ... also select it's Auto Save.
				autoSave.buttonSelected = true;
				// If this Component isn't currently supported, add support for it.
				if(!ES2EditorTypeUtility.TypeIsSupported(componentInfo.type))
					ES2EditorTypeUtility.AddType(componentInfo.type);
			}

			// If the button for the Auto Save of this Component was deselected, deselect this one too.
			if(autoSave.buttonSelectionChanged && !autoSave.buttonSelected)
					newRow.obj.buttonSelected = false;

			// Get variables column if this Component is selected.
			if(componentInfo.selected)
			{
				// Update this component if we've only just selected this Component.
				if(componentInfo.selectionChanged)
					ES2EditorAutoSaveUtility.UpdateVariablesForVariable(componentInfo);

				GetVariablesColumnForVariable(componentInfo, column, newRow, 2);
			}
		}
		
		if(autoSave.components.Count == 0)
			firstRow = column.AddRow("No supportable Components", null, null, null, 0);

		// Add seperator row.
		column.AddRow("", null, null, null);
		// Add curve line between columns.
		ArrayUtility.Add(ref curves, new ES2EditorRowCurve(previousColumn, previousRow, column, firstRow, autoSave.color)); 
	}

	/* 
	 * 	Displays all Properties for the given VariableInfo (which can also be a ComponentInfo.
	 * 	Finish GetVariablesColumn Method 
	 */
	private ES2EditorColumn GetVariablesColumnForVariable(ES2AutoSaveVariableInfo info, ES2EditorColumn previousColumn, ES2EditorRow previousRow, int columnIndex)
	{
		ES2EditorColumn column = GetColumn(columnIndex);

		ES2EditorRow firstRow = null;

		foreach(string variableID in info.variableIDs)
		{
			ES2AutoSaveVariableInfo variable = info.autoSave.GetCachedVariableInfo(variableID);
			ES2EditorRow row = column.AddRow(variable.name, variable, ES2EditorWindow.instance.style.saveButtonStyle, ES2EditorWindow.instance.style.saveButtonSelectedStyle);
			if(firstRow == null)
				firstRow = row;

			SetTooltips(variable, row);

			// If this variable was just selected ...
			if(variable.buttonSelectionChanged && variable.buttonSelected)
			{
				// ... select all of it's ancestors.
				SelectParentButtons(variable);

				// If this type isn't currently supported, add support for it.
				if(!ES2EditorTypeUtility.TypeIsSupported(variable.type))
					ES2EditorTypeUtility.AddType(variable.type);
			}
			
			// If the button for the Auto Save of this Component was deselected, deselect this one too.
			if(info.buttonSelectionChanged && !info.buttonSelected)
				row.obj.buttonSelected = false;

			if(variable.selected)
			{
				// If we just selected this variable, update it.
				if(variable.selectionChanged && variable.selected)
					ES2EditorAutoSaveUtility.UpdateVariablesForVariable(variable);

				GetVariablesColumnForVariable(variable, column, row, columnIndex+1);
			}
		}
	
		if(info.variableIDs.Count == 0)
		{
			//firstRow = column.AddRow("No supportable types", null, null, null);
			return null;
		}

		// Add seperator row.
		column.AddRow("", null, null, null);

		ArrayUtility.Add(ref curves, new ES2EditorRowCurve(previousColumn, previousRow, column, firstRow, info.autoSave.color)); 
		
		return column;
	}

	private void SetTooltips(ES2AutoSaveVariableInfo info, ES2EditorRow row)
	{
		/*
		 * Set the row's buttonTooltip and labelTooltip fields here to show a tooltip for each.
		 */
	}

	private void SelectParentButtons(ES2AutoSaveVariableInfo info)
	{
		if(!string.IsNullOrEmpty(info.previousID))
		{
			ES2AutoSaveVariableInfo previous = info.autoSave.GetCachedVariableInfo(info.previousID);
			if(previous == null)
				previous = info.autoSave.GetComponentInfo(info.previousID);
			previous.buttonSelected = true;
			SelectParentButtons(previous);
		}
		else if(info.isComponent)
			info.autoSave.buttonSelected = true;
	}

	protected ES2EditorColumn GetColumn(int index)
	{
		if(columns.Length <= index)
			ArrayUtility.Add(ref columns, new ES2EditorColumn(ES2EditorWindow.instance.style.columnStyle));
		return columns[index];
	}

	private void DrawColumns()
	{
		foreach(ES2EditorColumn column in columns)
			column.Draw();
	}

	private void DrawCurves()
	{
		if(curves == null || curves.Length == 0)
			return;
		
		foreach(ES2EditorRowCurve curve in curves)
			DrawCurve(curve);
	}

	private void DrawCurve(ES2EditorRowCurve curve)
	{
		Rect firstColumnRect = GetColumnRect(curve.leftColumn);
		Rect selectedRowRect = GetRowRect(curve.leftColumn, curve.leftRow);

		Rect firstRowRect = GetRowRect(curve.rightColumn, curve.rightRow);

		// First curve (straight line) should be between row's right margin and column's right margin.
		// Use firstCurveLength variable to determine where along this line it should be.
		float firstCurveEndPos = firstColumnRect.xMax - ((firstColumnRect.xMax - selectedRowRect.xMax) / firstCurveLength);
		
		DrawCurveBetween(	new Vector2(selectedRowRect.xMax, selectedRowRect.center.y), 
		                 	new Vector2(firstCurveEndPos, selectedRowRect.center.y), curve.color, null, 2, 0);
		
		DrawCurveBetween(	new Vector2(firstCurveEndPos, selectedRowRect.center.y), 
		                 	new Vector2(firstRowRect.xMin, firstRowRect.center.y), curve.color, null, 2, 1);

		// Arrow head.
		float arrowHeadSize = firstRowRect.height/4;
		DrawArrowHeadRight(new Vector2(firstRowRect.xMin + arrowHeadSize, firstRowRect.center.y), arrowHeadSize, curve.color);
	}

	/* Gets the Rect of the column relative to the Horizontal layout */
	private Rect GetColumnRect(ES2EditorColumn column)
	{
		// Add up the widths of all preceeding columns to get the column's left margin.
		float columnLeftMargin = GetColumnLeftMargin(column);
		Rect columnRect = column.GetRect();
		RectOffset padding = ES2EditorWindow.instance.style.windowContentStyle.padding;

		return new Rect(columnLeftMargin + padding.left, padding.top, columnRect.width, columnRect.height); 
	}
	
	private float GetColumnLeftMargin(ES2EditorColumn column)
	{
		// Sum all of previous column widths to get left margin of column.
		float leftMargin = 0;
		foreach(ES2EditorColumn col in columns)
		{
			if(col == column)
				break;
			leftMargin += col.width;
		}
		return leftMargin;
	}

	/* Gets the Rect of the row relative to the Horizontal layout */
	private Rect GetRowRect(ES2EditorColumn column, ES2EditorRow row)
	{
		Rect columnRect = GetColumnRect(column);
		Rect rowRect = column.GetRowRect(row);

		return new Rect(columnRect.xMin + rowRect.x, 
		                columnRect.yMin + rowRect.y, 
		                rowRect.width, 
		                rowRect.height);
	}

	/* Draws a bezier curve between two points, using a default set of tangents */
	private static void DrawCurveBetween(Vector2 a, Vector2 b, Color color, Texture2D tex, float width, float curveDepth=1)
	{
		float horizontalDistance = Mathf.Abs(a.x - b.x);

		Vector3 tanA = new Vector2(a.x + (horizontalDistance * curveDepth), a.y);
		Vector3 tanB = new Vector2(b.x - (horizontalDistance * curveDepth), b.y);
		
		Handles.DrawBezier(a, b, tanA, tanB, color, tex, width);
	}

	private static void DrawArrowHeadRight(Vector2 pointPosition, float size, Color color)
	{
		Color oldColor = Handles.color;
		Handles.color = color;

		Handles.DrawAAConvexPolygon(new Vector3[]{ 	new Vector3( pointPosition.x-size, pointPosition.y + (size/2)),
													new Vector3( pointPosition.x-size, pointPosition.y - (size/2)),
													new Vector3( pointPosition.x, pointPosition.y)});

		Handles.color = oldColor;
	}

	/* Draws a Rectangle at the given Rect */
	private static void DrawRect(Rect rect, Color color, Color border)
	{
		Handles.DrawSolidRectangleWithOutline(new Vector3[]{new Vector3(rect.xMin, rect.yMin),
		                                      				new Vector3(rect.xMax, rect.yMin),
		                                                  	new Vector3(rect.xMax, rect.yMax),
															new Vector3(rect.xMin, rect.yMax)},
		                                      color, border);
	}
}
#endif