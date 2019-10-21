using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ES2Spreadsheet
{	
	// Matrix representing our spreadsheet.
	List<ES2SpreadsheetRow> sheet = new List<ES2SpreadsheetRow>();
	
	// Whether this appends rows if the file already exists when writing.
	public bool append = false;
	
	// Character contants.
	private static byte[] comma = null;
	private static byte[] newline = null;
	
	public ES2Spreadsheet()
	{
		GetCharacterConstants();
	}
	
	/* 
		Sets the cell at the given column and row, expanding the spreadsheet if necessary.
		Uses ToString() to get the string representation of the object.
	*/
	public void SetCell(int col, int row, object value)
	{
		SizeSpreadsheetToFitRow(row);
		if(sheet[row] == null)
			sheet[row] = new ES2SpreadsheetRow();
		sheet[row].SetCell(col, value.ToString());
	}
	
	public string GetCell(int col, int row)
	{
		int highestRow = sheet.Count-1;
		if(highestRow <= row || sheet[row] == null)
			return null;
			
		ES2SpreadsheetRow rowObject = sheet[row];
		if(rowObject == null)
			return null;
		return rowObject[col];
	}
	
	/* 	
		If necessary, resizes the spreadsheet so that it has enough rows 
		to add a value to this row. 
	*/
	private void SizeSpreadsheetToFitRow(int row)
	{
		int highestRow = sheet.Count-1;
		if(highestRow < row)
			sheet.AddRange(new ES2SpreadsheetRow[row-highestRow]);
	}
	
	/* Saves the CSV to file */
	public void Save(string path)
	{
		Save(path, new ES2Settings());
	}
	
	public void Save(string path, ES2Settings settings)
	{
		if(append) // If append is enabled, set file stream to append.
			settings.fileMode = ES2Settings.ES2FileMode.Append;
			
		using(ES2Writer writer = ES2Writer.Create(path, settings))
		{
			for(int rowNo=0; rowNo<sheet.Count; rowNo++) // For each row ...
			{
				ES2SpreadsheetRow row = sheet[rowNo];
					
				if(row != null) // If row is set ...
				{
					for(int colNo=0; colNo<row.cellCount; colNo++) // Write each cell.
					{
						if(colNo != 0) // Only prepend a comma if this isn't the first column.
							writer.WriteRaw(comma);
						writer.WriteRaw(row.GetCellBytes(colNo));
					}
				}
				writer.WriteRaw (newline); // Write a newline to signify the end of the row.
			}
			writer.Save(false);
		}
	}
	
	/* Loads the UTF-8 constants for characters into their static variables */
	private static void GetCharacterConstants()
	{
		if(comma == null) // If one of the constants is null, they've not yet been initialised.
		{
			comma = System.Text.Encoding.UTF8.GetBytes(",");
			newline = System.Text.Encoding.UTF8.GetBytes("\n");
		}
	}
	
	public string this[int col, int row]
	{
		get { return GetCell(col, row); }
		set { SetCell(col, row, value); }
	}
	
	public override string ToString()
	{
		string str = "";
		for(int i=0; i<sheet.Count; i++)
		{
			if(i != 0)
				str += "\n";
			ES2SpreadsheetRow row = sheet[i];
			if(row == null)
				str += "{}";
			else
				str += row.ToString();
		}
		str += "}";
		return str;
	}
}

public class ES2SpreadsheetRow
{
	private List<string> cells = new List<string>();
	
	public int cellCount
	{
		get{ return cells.Count; }
	}
	
	public void SetCell(int col, string value)
	{
		SizeRowToFitColumn(col);
		cells[col] = value;
	}
	
	public string GetCell(int col)
	{
		int highestCol = cells.Count-1;
		if(highestCol < col || cells[col] == null)
			return null;
		return cells[col];
	}
	
	/*
		Gets the cell as a byte array, performing necessary validation,
		or returns an empty byte array if the cell is empty.
	*/
	public byte[] GetCellBytes(int col)
	{
		string value = GetCell(col);
		return GetBytes(value); 
	}
	
	/* 	
		If necessary, resizes the row so that it is at least long 
		enough to add a value to this column 
	*/
	private void SizeRowToFitColumn(int col)
	{
		int highestCol = cells.Count-1;
		if(highestCol < col)
			cells.AddRange(new string[col-highestCol]);
	}
	
	/* 
		Escapes data so that it doesn't put any invalid characters into the CSV,
		And then converts it to a byte array. Returns an empty byte array if the data is empty or null.
	*/
	private byte[] GetBytes(string data)
	{
		if(data == null || data == "")
			return new byte[0];
			
		if (data.Contains("\""))
		{
			data = data.Replace("\"", "\"\"");
		}
		
		if (data.Contains(","))
		{
			data = System.String.Format("\"{0}\"", data);
		}
		
		if (data.Contains(System.Environment.NewLine))
		{
			data = System.String.Format("\"{0}\"", data);
		}
		
		return System.Text.Encoding.UTF8.GetBytes(data);
	}
	
	public string this[int col]
	{
		get { return GetCell(col); }
		set { SetCell(col, value); }
	}
	
	public override string ToString()
	{
		string str = "{";
		for(int i=0; i<cells.Count; i++)
		{
			if(i != 0)
				str += ",";
			str += "\""+cells[i]+"\"";
		}
		str += "}";
		return str;
	}
}

