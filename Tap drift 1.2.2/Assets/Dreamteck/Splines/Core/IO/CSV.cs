﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace Dreamteck.Splines.IO
{
    public class CSV : SplineParser
    {
        public enum ColumnType { Position, Tangent, Tangent2, Normal, Size, Color }
        public List<ColumnType> columns = new List<ColumnType>();

        public CSV(SplineComputer computer)
        {
            Spline spline = new Spline(computer.type, computer.precision);
            spline.points = computer.GetPoints();
            if (spline.type != Spline.Type.Bezier && spline.type != Spline.Type.Linear) spline.ConvertToBezier();
            if (computer.isClosed) spline.Close();
            buffer = new SplineDefinition(computer.name, spline);
            fileName = computer.name;
            columns.Add(ColumnType.Position);
            columns.Add(ColumnType.Tangent);
            columns.Add(ColumnType.Tangent2);
        }

        public CSV(string filePath, List<ColumnType> customColumns = null)
        {
            if (File.Exists(filePath))
            {
                string ext = Path.GetExtension(filePath).ToLower();
                fileName = Path.GetFileNameWithoutExtension(filePath);
                if (ext != ".csv")
                {
                    Debug.LogError("CSV Parsing ERROR: Wrong format. Please use SVG or XML");
                    return;
                }
                string[] lines = File.ReadAllLines(filePath);
                if (customColumns == null)
                {
                    columns.Add(ColumnType.Position);
                    columns.Add(ColumnType.Tangent);
                    columns.Add(ColumnType.Tangent2);
                    columns.Add(ColumnType.Normal);
                    columns.Add(ColumnType.Size);
                    columns.Add(ColumnType.Color);
                } else columns = new List<ColumnType>(customColumns);
                buffer = new SplineDefinition(fileName, Spline.Type.Hermite);
                Read(lines);
            }
        }

        void Read(string[] lines)
        {
            int expectedElementCount = 0;
            foreach (ColumnType col in columns)
            {
                switch (col)
                {
                    case ColumnType.Position: expectedElementCount +=3; break;
                    case ColumnType.Tangent: expectedElementCount += 3; break;
                    case ColumnType.Tangent2: expectedElementCount += 3; break;
                    case ColumnType.Normal: expectedElementCount += 3; break;
                    case ColumnType.Size: expectedElementCount ++; break;
                    case ColumnType.Color: expectedElementCount += 4; break;
                }
            }
            for (int i = 1; i < lines.Length; i++)
            {
                lines[i] = Regex.Replace(lines[i], @"\s+", "");
                string[] elements = lines[i].Split(',');
                if(elements.Length != expectedElementCount)
                {
                    Debug.LogError("Unexpected element count on row " + i + ". Expected " + expectedElementCount +  " found " + elements.Length + " Please make sure that all values exist and the column order is correct.");
                    continue;
                }
                float[] values = new float[elements.Length];
                for (int j = 0; j < elements.Length; j++)
                {
                    float.TryParse(elements[j], out values[j]);
                }
                int currentValue = 0;
                foreach (ColumnType col in columns)
                {
                    switch (col)
                    {
                        case ColumnType.Position: buffer.position = new Vector3(values[currentValue++], values[currentValue++], values[currentValue++]); break;
                        case ColumnType.Tangent: buffer.tangent = new Vector3(values[currentValue++], values[currentValue++], values[currentValue++]); break;
                        case ColumnType.Tangent2: buffer.tangent2 = new Vector3(values[currentValue++], values[currentValue++], values[currentValue++]); break;
                        case ColumnType.Normal: buffer.normal = new Vector3(values[currentValue++], values[currentValue++], values[currentValue++]); break;
                        case ColumnType.Size: buffer.size = values[currentValue++]; break;
                        case ColumnType.Color: buffer.color = new Color(values[currentValue++], values[currentValue++], values[currentValue++], values[currentValue++]); break;
                    }
                }
                buffer.CreateSmooth();
            }
        }

        public SplineComputer CreateSplineComputer(Vector3 position, Quaternion rotation)
        {
            return buffer.CreateSplineComputer(position, rotation);
        }

        public Spline CreateSpline()
        {
            return buffer.CreateSpline();
        }


        public void FlatX()
        {
            for (int i = 0; i < buffer.pointCount; i++)
            {
                SplinePoint p = buffer.points[i];
                p.position.x = 0f;
                p.tangent.x = 0f;
                p.tangent2.x = 0f;
                p.normal = Vector3.right;
                buffer.points[i] = p;
            }
        }

        public void FlatY()
        {
            for (int i = 0; i < buffer.pointCount; i++)
            {
                SplinePoint p = buffer.points[i];
                p.position.y = 0f;
                p.tangent.y = 0f;
                p.tangent2.y = 0f;
                p.normal = Vector3.up;
                buffer.points[i] = p;
            }
        }

        public void FlatZ()
        {
            for (int i = 0; i < buffer.pointCount; i++)
            {
                SplinePoint p = buffer.points[i];
                p.position.z = 0f;
                p.tangent.z = 0f;
                p.tangent2.z = 0f;
                p.normal = Vector3.back;
                buffer.points[i] = p;
            }
        }

        void AddTitle(ref string[] content, string title)
        {
            if (!string.IsNullOrEmpty(content[0])) content[0] += ",";
            content[0] += title;
        }

        void AddVector3Title(ref string[] content, string prefix)
        {
            AddTitle(ref content, prefix + "X," + prefix + "Y," + prefix + "Z");
        }

        void AddColorTitle(ref string[] content, string prefix)
        {
            AddTitle(ref content, prefix + "R," + prefix + "G," + prefix + "B" + prefix + "A");
        }

        void AddVector3(ref string[] content, int index, Vector3 vector)
        {
            AddFloat(ref content, index, vector.x);
            AddFloat(ref content, index, vector.y);
            AddFloat(ref content, index, vector.z);
        }

        void AddColor(ref string[] content, int index, Color color)
        {
            AddFloat(ref content, index, color.r);
            AddFloat(ref content, index, color.g);
            AddFloat(ref content, index, color.b);
            AddFloat(ref content, index, color.a);
        }

        void AddFloat(ref string[] content, int index, float value)
        {
            if (!string.IsNullOrEmpty(content[index])) content[index] += ",";
            content[index] += value.ToString();
        }

        public void Write(string filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))  throw new DirectoryNotFoundException("The file is being saved to a non-existing directory.");
            List<SplinePoint> csvPoints = buffer.points;
            string[] content = new string[csvPoints.Count+1];
            //Add the column titles
            foreach(ColumnType col in columns)
            {
                switch (col)
                {
                    case ColumnType.Position: AddVector3Title(ref content, "Position"); break;
                    case ColumnType.Tangent: AddVector3Title(ref content, "Tangent"); break;
                    case ColumnType.Tangent2: AddVector3Title(ref content, "Tangent2"); break;
                    case ColumnType.Normal: AddVector3Title(ref content, "Normal"); break;
                    case ColumnType.Size: AddTitle(ref content, "Size"); break;
                    case ColumnType.Color: AddColorTitle(ref content, "Color"); break;
                }
            }
            //Add the content for each column
            foreach (ColumnType col in columns)
            {
                for (int i = 1; i <= csvPoints.Count; i++)
                {
                    int index = i - 1;
                    switch (col)
                    {
                        case ColumnType.Position: AddVector3(ref content, i, csvPoints[index].position); break;
                        case ColumnType.Tangent: AddVector3(ref content, i, csvPoints[index].tangent); break;
                        case ColumnType.Tangent2: AddVector3(ref content, i, csvPoints[index].tangent2); break;
                        case ColumnType.Normal: AddVector3(ref content, i, csvPoints[index].normal); break;
                        case ColumnType.Size: AddFloat(ref content, i, csvPoints[index].size); break;
                        case ColumnType.Color: AddColor(ref content, i, csvPoints[index].color); break;
                    }
                }
            }
            File.WriteAllLines(filePath, content);
        }
    }
}