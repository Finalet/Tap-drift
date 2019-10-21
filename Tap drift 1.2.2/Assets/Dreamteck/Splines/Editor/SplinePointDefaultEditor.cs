using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Dreamteck.Splines
{
    public class SplinePointDefaultEditor : SplinePointEditor
    {
        public bool additive = false;
        public bool shift = false;
        public bool excludeSelected = false;
        public bool selectOnMove = true;

        public bool deleteMode = false;
        public bool click = false;

        public int minimumRectSize = 5;
        private Vector2 rectStart = Vector2.zero;
        private Vector2 rectEnd = Vector2.zero;
        private Rect rect;
        private bool drag = false;
        private bool finalize = false;

        public bool isDragging
        {
            get
            {
                return drag && rect.width >= minimumRectSize && rect.height >= minimumRectSize;
            }
        }


        public override bool SceneEdit(ref SplinePoint[] points, ref List<int> selected)
        {
            bool change = false;
            Transform camTransform = SceneView.currentDrawingSceneView.camera.transform;
            if (focused)
            {
                if (!drag)
                {
                    if (finalize)
                    {
                        if (rect.width > 0f && rect.height > 0f)
                        {
                            if (!additive) ClearSelection(ref selected);
                            for (int i = 0; i < points.Length; i++)
                            {
                                Vector2 guiPoint = HandleUtility.WorldToGUIPoint(points[i].position);
                                if (rect.Contains(guiPoint))
                                {
                                    Vector3 local = camTransform.InverseTransformPoint(points[i].position);
                                    if (local.z >= 0f)
                                    {
                                        AddPointSelection(i, ref selected);
                                        change = true;
                                    }
                                }
                            }
                        }
                        finalize = false;
                    }
                }
                else
                {
                    rectEnd = Event.current.mousePosition;
                    rect = new Rect(Mathf.Min(rectStart.x, rectEnd.x), Mathf.Min(rectStart.y, rectEnd.y), Mathf.Abs(rectEnd.x - rectStart.x), Mathf.Abs(rectEnd.y - rectStart.y));
                    if (rect.width >= minimumRectSize && rect.height >= minimumRectSize)
                    {
                        Color col = SplinePrefs.highlightColor;
                        if (deleteMode) col = Color.red;
                        col.a = 0.4f;
                        GUI.color = col;
                        Handles.BeginGUI();
                        GUI.Box(rect, "", SplineEditorGUI.whiteBox);
                        GUI.color = Color.white;
                        Handles.EndGUI();
                        SceneView.RepaintAll();
                    }
                }
            }
            TextAnchor originalAlignment = GUI.skin.label.alignment;
            Color originalColor = GUI.skin.label.normal.textColor;

            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.normal.textColor = computer.editorPathColor;

            for (int i = 0; i < points.Length; i++)
            {
                if (computer.isClosed && i == points.Length - 1) break;
                bool moved = false;
                bool isSelected = selected.Contains(i);
                Vector3 lastPos = points[i].position;
                Handles.color = Color.clear;
                if (SplinePrefs.showPointNumbers && camTransform.InverseTransformPoint(points[i].position).z > 0f)
                {
                    Handles.Label(points[i].position + Camera.current.transform.up * HandleUtility.GetHandleSize(points[i].position) * 0.3f, (i + 1).ToString());
                }
                if (excludeSelected && isSelected) SplineEditorHandles.FreeMoveRectangle(points[i].position, HandleUtility.GetHandleSize(points[i].position) * 0.1f);
               else  points[i].SetPosition(SplineEditorHandles.FreeMoveRectangle(points[i].position, HandleUtility.GetHandleSize(points[i].position) * 0.1f));
                
                if (!change)
                {
                    if (lastPos != points[i].position)
                    {
                        moved = true;
                        change = true;
                        if (isSelected)
                        {
                            for (int n = 0; n < selected.Count; n++)
                            {
                                if (selected[n] == i) continue;
                                points[selected[n]].SetPosition(points[selected[n]].position + (points[i].position - lastPos));
                            }
                        }
                        else if (selectOnMove)
                        {
                            selected.Clear();
                            selected.Add(i);
                            SceneView.RepaintAll();
                        }
                    }
                }

                 if (!moved)
                 {
                    if(SplineEditorHandles.HoverArea(points[i].position, 0.12f) && click)
                    {
                        if (shift) ShiftSelect(i, points.Length, ref selected);
                        else if (additive) AddPointSelection(i, ref selected);
                        else SelectPoint(i, ref selected);
                        SceneView.RepaintAll();
                        change = true;
                    }
               }
                if (!excludeSelected || !isSelected)
                {
                    Handles.color = computer.editorPathColor;
                    if (deleteMode) Handles.color = Color.red;
                    else if (isSelected) Handles.color = SplinePrefs.highlightColor;
                    SplineEditorHandles.DrawRectangle(points[i].position, Quaternion.LookRotation(-SceneView.currentDrawingSceneView.camera.transform.forward), HandleUtility.GetHandleSize(points[i].position) * 0.1f);
                }
                moved = false;
            }
            GUI.skin.label.alignment = originalAlignment;
            GUI.skin.label.normal.textColor = originalColor;

            if (computer.type == Spline.Type.Bezier)
            {
                Handles.color = computer.editorPathColor;
                for (int i = 0; i < selected.Count; i++)
                {
                    Handles.DrawDottedLine(points[selected[i]].position, points[selected[i]].tangent, 6f);
                    Handles.DrawDottedLine(points[selected[i]].position, points[selected[i]].tangent2, 6f);
                    Vector3 lastPos = points[selected[i]].tangent;
                    points[selected[i]].SetTangentPosition(SplineEditorHandles.FreeMoveCircle(points[selected[i]].tangent, HandleUtility.GetHandleSize(points[selected[i]].tangent) * 0.1f));
                    if (lastPos != points[selected[i]].tangent) change = true;
                    lastPos = points[selected[i]].tangent2;
                    points[selected[i]].SetTangent2Position(SplineEditorHandles.FreeMoveCircle(points[selected[i]].tangent2, HandleUtility.GetHandleSize(points[selected[i]].tangent2) * 0.1f));
                    if (lastPos != points[selected[i]].tangent2) change = true;
                }
            }
            return change;
        }

        void ShiftSelect(int index, int pointCount, ref List<int> selected)
        {
            if (selected.Count == 0)
            {
                AddPointSelection(index, ref selected);
                return;
            }
            int minSelected = pointCount-1, maxSelected = 0;
            for (int i = 0; i < selected.Count; i++)
            {
                if (minSelected > selected[i]) minSelected = selected[i];
                if (maxSelected < selected[i]) maxSelected = selected[i];
            }

            if(index > maxSelected)
            {
                for (int i = maxSelected + 1; i <= index; i++) AddPointSelection(i, ref selected);
            } else if(index < minSelected)
            {
                for (int i = minSelected-1; i >= index; i--) AddPointSelection(i, ref selected);
            } else
            {
                for (int i = minSelected + 1; i <= index; i++) AddPointSelection(i, ref selected);
            }
        }

        public void ClearSelection(ref List<int> selected)
        {
            selected.Clear();
            SceneView.RepaintAll();
        }

        public void SelectPoint(int index, ref List<int> selected)
        {
            if (computer.isClosed && index == computer.pointCount - 1) return;
            selected.Clear();
            selected.Add(index);
            SceneView.RepaintAll();
        }

        public void SelectPoints(List<int> indices, ref List<int> selected)
        {
            selected.Clear();
            for (int i = 0; i < indices.Count; i++)
            {
                if (computer.isClosed && i == computer.pointCount - 1) continue;
                selected.Add(indices[i]);
            }
            SceneView.RepaintAll();
        }

        public void AddPointSelection(int index, ref List<int> selected)
        {
            if (computer.isClosed && index == computer.pointCount - 1) return;
            if (selected.Contains(index)) return;
            selected.Add(index);
            SceneView.RepaintAll();
        }

        public void StartDrag(Vector2 position)
        {
            if (!focused) return;
            rectStart = position;
            drag = true;
        }

        public void FinishDrag()
        {
            if (!focused) return;
            if (!drag) return;
            drag = false;
            finalize = true;
        }

        public void CancelDrag()
        {
            if (!focused) return;
            drag = false;
        }

    }
}
