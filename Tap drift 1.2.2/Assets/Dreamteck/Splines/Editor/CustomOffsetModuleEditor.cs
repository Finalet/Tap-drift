using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dreamteck.Splines
{
    public class CustomOffsetModuleEditor : SplineUserSubEditor
    {
        public bool allowSelection = true;
        private float addTime = 0f;
        private CustomOffsetModule group;
        private int selected = -1;
        private bool editOffset = false;
        Matrix4x4 matrix = new Matrix4x4();
        public CustomOffsetModule.Key selectedKey
        {
            get { return group.keys[selected]; }
        }

        public CustomOffsetModuleEditor(SplineUserEditor parent, CustomOffsetModule input) : base(parent)
        {
            group = input;
            title = "Custom offsets";
        }

        public void ClearSelection()
        {
            selected = -1;
        }

        protected override void DrawInspectorLogic()
        {
            if (!allowSelection) selected = -1;
            if (selected >= 0 && selected < group.keys.Count)
            {
                if (SplineEditorGUI.EditorLayoutSelectableButton(new GUIContent("Offset Handle"), true, editOffset))
                {
                    SceneView.RepaintAll();
                    editOffset = !editOffset;
                }
            }

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < group.keys.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 55f;
                group.keys[i].offset = EditorGUILayout.Vector2Field("Offset", group.keys[i].offset);
                if (group.keys[i].interpolation == null) group.keys[i].interpolation = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                EditorGUIUtility.labelWidth = 90f;
                group.keys[i].interpolation = EditorGUILayout.CurveField("Interpolation", group.keys[i].interpolation);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (allowSelection)
                {
                    if (parentEditor.EditButton(selected == i))
                    {
                        if (selected == i) selected = -1;
                        else selected = i;
                        SceneView.RepaintAll();
                    }
                }
                EditorGUIUtility.labelWidth = 50f;
                group.keys[i].from = EditorGUILayout.Slider("Start", (float)group.keys[i].from, 0f, 1f);
                group.keys[i].to = EditorGUILayout.Slider("End", (float)group.keys[i].to, 0f, 1f);
                group.keys[i].center = EditorGUILayout.Slider("Center", (float)group.keys[i].center, 0f, 1f);
                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0f;
                EditorGUILayout.EndVertical();
                if (GUILayout.Button("x", GUILayout.Width(30)))
                {
                    selected = -1;
                    group.keys.RemoveAt(i);
                    i--;
                    EditorGUILayout.EndHorizontal();
                    continue;
                }
                EditorGUILayout.EndHorizontal();
                if (i < group.keys.Count - 1) EditorGUILayout.Space();
            }
            if (group.keys.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Blend", EditorStyles.boldLabel);
                group.blend = EditorGUILayout.Slider(group.blend, 0f, 1f);
                EditorGUILayout.Space();
            }
            if (GUILayout.Button("Add New Offset")) group.AddKey(Vector2.zero, addTime - 0.1, addTime + 0.1, 0.5);

            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
        }

        public bool DrawScene(SplineUser user)
        {
            bool changed = false;
            bool is2D = user.rootUser.computer != null && user.rootUser.computer.is2D;
            SplineResult result = new SplineResult();
            for (int i = 0; i < group.keys.Count; i++)
            {
                if (selected >= 0 && i != selected) continue;
                user.Evaluate(result, group.keys[i].position);
                matrix.SetTRS(result.position, Quaternion.LookRotation(result.direction, result.normal), Vector3.one * result.size);
                Vector3 pos = matrix.MultiplyPoint(group.keys[i].offset);
                if (is2D)
                {
                    Handles.DrawLine(result.position, result.position + result.right * group.keys[i].offset.x * result.size);
                    Handles.DrawLine(result.position, result.position - result.right * group.keys[i].offset.x * result.size);
                }
                else Handles.DrawWireDisc(result.position, result.direction, group.keys[i].offset.magnitude * result.size);
                Handles.DrawLine(result.position, pos);
            }

            if (selected >= 0 && selected < group.keys.Count)
            {
                double value = group.keys[selected].position;
                user.Evaluate(result, value);
                matrix.SetTRS(result.position, Quaternion.LookRotation(result.direction, result.normal), Vector3.one * result.size);
                Vector3 pos = matrix.MultiplyPoint(group.keys[selected].offset);
                if (editOffset)
                {
                    Vector3 lastPos = pos;
                    pos = SplineEditorHandles.FreeMoveRectangle(pos, HandleUtility.GetHandleSize(pos) * 0.1f);
                    if (pos != lastPos)
                    {
                        pos = matrix.inverse.MultiplyPoint(pos);
                        pos.z = 0f;
                        if (is2D) group.keys[selected].offset = Vector2.right * pos.x;
                        else group.keys[selected].offset = pos;
                        changed = true;
                    }
                }
                else
                {
                    double lastValue = value;
                    SplineEditorHandles.Slider(user, ref value, user.rootUser.computer.editorPathColor, "Center", SplineEditorHandles.SplineSliderGizmo.Circle, 0.6f);
                    if (value != lastValue) changed = true;
                    if (group.keys[selected].from > group.keys[selected].to)
                    {
                        double fromToEndDistance = 1.0 - group.keys[selected].from;
                        double toToBeginningDistance = group.keys[selected].to;
                        double totalDistance = fromToEndDistance + toToBeginningDistance;
                        if (value > group.keys[selected].from) group.keys[selected].center = DMath.InverseLerp(group.keys[selected].from, group.keys[selected].from + totalDistance, value);
                        else if (value < group.keys[selected].to) group.keys[selected].center = DMath.InverseLerp(-fromToEndDistance, group.keys[selected].to, value);
                    }
                    else group.keys[selected].center = DMath.InverseLerp(group.keys[selected].from, group.keys[selected].to, value);
                }
                value = group.keys[selected].from;
                SplineEditorHandles.Slider(user, ref value, user.rootUser.computer.editorPathColor, "Start", SplineEditorHandles.SplineSliderGizmo.ForwardTriangle, 1f);
                if (group.keys[selected].from != value)
                {
                    group.keys[selected].from = value;
                    changed = true;
                }

                value = group.keys[selected].to;
                SplineEditorHandles.Slider(user, ref value, user.rootUser.computer.editorPathColor, "End", SplineEditorHandles.SplineSliderGizmo.BackwardTriangle, 1f);
                if (group.keys[selected].to != value)
                {
                    group.keys[selected].to = value;
                    changed = true;
                }
            }
            return changed;
        }
    }
}
