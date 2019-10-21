#if !UNITY_4
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ES2EditorColumn
{
	private GUIStyle columnStyle = null;
	private GUIStyle style = null;
	
	[SerializeField]
	public ES2EditorRow[] rows = new ES2EditorRow[0];

	public bool selectionChanged = false;

	private ES2EditorRow _iconSelectedRow = null;
	public bool iconSelectionChanged = false;

	private float _width = 0f;


	public ES2EditorColumn(GUIStyle columnStyle)
	{
		this.columnStyle = columnStyle;
	}

	private void CalculateWidth()
	{
		_width = 0f;
		this.style = new GUIStyle(columnStyle);
		this.style.fixedWidth = width;
	}

	public ES2EditorRow iconSelectedRow
	{
		get { return _iconSelectedRow; }
		set 
		{
			_iconSelectedRow = value;
			iconSelectionChanged = true;
		}
	}

	public Rect GetRect()
	{
		return new Rect(0,0, width, height);
	}

	public Rect GetRowRect(ES2EditorRow row)
	{
		return new Rect(columnStyle.padding.left, columnStyle.padding.top+(row.rowNo*row.height), row.width, row.height);
	}

	public void Draw()
	{
		if(_width == 0f)
			CalculateWidth();

		selectionChanged = false;
		iconSelectionChanged = false;

		EditorGUILayout.BeginVertical(style);

		// Draw each row, and check if the selection changed.
		foreach(ES2EditorRow row in rows)
			row.Draw();

		EditorGUILayout.EndVertical();
	}

	public float width
	{
		get 
		{
			if(_width == 0)
				foreach(ES2EditorRow row in rows)
					_width = Mathf.Max(row.width + columnStyle.padding.left + columnStyle.padding.right, _width);
			return _width;
		}
	}

	public float height
	{
		get
		{
			float height = columnStyle.padding.top + columnStyle.padding.bottom;
			foreach(ES2EditorRow row in rows)
				height += row.height;
			return height;
		}
	}

	public ES2EditorRow AddRow(string label, IES2Selectable obj, GUIStyle iconStyle, GUIStyle selectedIconStyle, int indentLevel = 0)
	{
		ES2EditorRow row = new ES2EditorRow(this, label, obj, rows.Length, iconStyle, selectedIconStyle, indentLevel);

		// Add the item to the rows array.
		ArrayUtility.Add(ref rows, row);

		return row;
	}
}

public class ES2EditorRow
{
	private GUIStyle iconStyle = null;
	private GUIStyle selectedIconStyle = null;

	private static GUIStyle labelStyle 			= null;
	private static GUIStyle labelActiveStyle	= null;
	private static GUIStyle dividerStyle		= null;
	private static GUIStyle horizontalStyle		= null;
	
	private const int rowHeight 		= 20;
	private const int indentDepth 		= 12;
	private const int saveButtonSize 	= 20;
	private RectOffset padding	= new RectOffset(5, 10, 0, 0);

	public string label;
	public IES2Selectable obj;
	public int indentLevel;

	public string labelTooltip = "";
	public string buttonTooltip = "";

	public int rowNo;
	
	public bool iconRightClicked = false;

	private float _width = 0f;
	public float width
	{
		get
		{
			// Initialise GUIStyles first so we can use CalcSize.
			InitGUIStyles();

			if(_width == 0f)
			{
				Vector2 labelSize = labelStyle.CalcSize(new GUIContent(label)); // Label Width
				Vector2 btnSize = Vector2.zero;
				if(iconStyle != null)
					btnSize = iconStyle.CalcSize(new GUIContent());	// Icon Width
				_width = labelSize.x + btnSize.x + (indentDepth * indentLevel) + padding.left + padding.right;
			}

			return _width;
		}
	}

	public float height
	{
		get { return rowHeight + padding.top + padding.bottom; }
	}

	public ES2EditorRow(ES2EditorColumn column, string label, IES2Selectable obj, int rowNo, GUIStyle iconStyle, GUIStyle selectedIconStyle, int indentLevel = 0)
	{
		this.label = label;
		this.obj = obj;
		this.rowNo = rowNo;
		this.indentLevel = indentLevel;
		this.iconStyle = iconStyle;
		this.selectedIconStyle = selectedIconStyle;
	}

	public void Draw()
	{
		InitGUIStyles();

		iconRightClicked = false;

		// Display the row for this object in the hierarchy.
		EditorGUILayout.BeginHorizontal(horizontalStyle);

		// If label is empty or null, this is a divider.
		if(string.IsNullOrEmpty(label))
			GUILayout.Button(new GUIContent(), dividerStyle);
		// Else if object is null, this is a label without a button.
		else if(obj == null)
			GUILayout.Button(new GUIContent(label, labelTooltip), labelStyle);
		else
		{
			// Indent using GUILayout.Space.
			GUILayout.Space(indentLevel * indentDepth); 

			// Display the row's icon.
			if(GUILayout.Button(new GUIContent("", buttonTooltip), obj.buttonSelected ? selectedIconStyle : iconStyle))
			{
				//EditorApplication.MarkSceneDirty();
				Undo.RecordObject(obj.undoRecordObject, "Changes to Auto Save");

#if UNITY_2018_2_OR_NEWER
				if(PrefabUtility.GetPrefabInstanceStatus(obj.undoRecordObject) != PrefabInstanceStatus.NotAPrefab)
#else
				if(PrefabUtility.GetPrefabType(obj.undoRecordObject) == PrefabType.Prefab)
#endif
					EditorUtility.SetDirty(obj.undoRecordObject);

				// If we right click the icon, mark it as right-clicked in the column.
				if(Event.current.button == 1)
					iconRightClicked = true;
				// Else, toggle the icon's selection variable.
				else
					obj.buttonSelected = !obj.buttonSelected;
			}
			else
				obj.buttonSelectionChanged = false;

			// Choose right style for button label depending on whether it is selected.
			if(GUILayout.Button(new GUIContent(label, labelTooltip), obj.selected ? labelActiveStyle : labelStyle))
				obj.selected = !obj.selected;
			else
				obj.selectionChanged = false;
		}

		EditorGUILayout.EndHorizontal();
	}

	private void InitGUIStyles()
	{
		// Reset width so this can be recalculated.
		_width = 0f;

		// Load Textures
		if(labelStyle == null)
		{
			dividerStyle = new GUIStyle();
			dividerStyle.fixedHeight = saveButtonSize;

			labelStyle = new GUIStyle();
			labelStyle = new GUIStyle(EditorStyles.label);
			labelStyle.fixedHeight = rowHeight;
			labelStyle.normal.textColor = Color.grey;
			labelStyle.active.textColor = Color.white;

			labelActiveStyle = new GUIStyle(labelStyle);
			labelActiveStyle.normal.textColor = Color.white;

			horizontalStyle = new GUIStyle();
			horizontalStyle.padding = padding;
		}
	}
}

public struct ES2EditorRowCurve
{
	public ES2EditorColumn leftColumn;
	public ES2EditorRow leftRow;
	public ES2EditorColumn rightColumn;
	public ES2EditorRow rightRow;
	public Color color;
	
	public ES2EditorRowCurve(ES2EditorColumn leftColumn, ES2EditorRow leftRow, ES2EditorColumn rightColumn, ES2EditorRow rightRow, Color color)
	{
		this.leftColumn = leftColumn;
		this.rightColumn = rightColumn;
		this.leftRow = leftRow;
		this.rightRow = rightRow;
		this.color = color;
	}
}
#endif