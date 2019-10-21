using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SplineTracer), true)]
    public class SplineTracerEditor : SplineUserEditor
    {
        private int trigger = -1;
        private bool triggerFoldout = false;
        private bool cameraFoldout = false;
        private TransformModuleEditor motionEditor;
        private CustomRotationModuleEditor customRotationEditor;
        private CustomOffsetModuleEditor customOffsetEditor;
        private RenderTexture rt;
        private Texture2D renderCanvas = null;
        private Camera cam;

        public delegate void DistanceReceiver(float distance);

        void OnEnable()
        {
            SplineTracer tracer = (SplineTracer)target;
            motionEditor = new TransformModuleEditor(this, tracer.motion);
            customRotationEditor = new CustomRotationModuleEditor(this, tracer.customRotations);
            customOffsetEditor = new CustomOffsetModuleEditor(this, tracer.customOffsets);
        }

        private int GetRTWidth()
        {
            return Mathf.RoundToInt(EditorGUIUtility.currentViewWidth)-50;
        }

        private int GetRTHeight()
        {
            return Mathf.RoundToInt(GetRTWidth()/cam.aspect);
        }

        private void CreateRT()
        {
            if(rt != null)
            {
                DestroyImmediate(rt);
                DestroyImmediate(renderCanvas);
            }
            rt = new RenderTexture(GetRTWidth(), GetRTHeight(), 16, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            renderCanvas = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyImmediate(rt);
        }

        protected void DistanceWindowMenu(Rect rect, DistanceReceiver receiver)
        {
            Vector2 mousePos = Event.current.mousePosition;
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(mousePos))
            {
                GenericMenu menu = new GenericMenu();
                SplineTracer tracer = (SplineTracer)target;
                menu.AddItem(new GUIContent("Set Distance"), false, delegate { DistanceWindow w = EditorWindow.GetWindow<DistanceWindow>(true); w.Init(tracer, receiver, tracer.CalculateLength()); });
                menu.ShowAsContext();
            }
        }

        protected override void BodyGUI()
        {
            base.BodyGUI();
            EditorGUILayout.LabelField("Tracer", EditorStyles.boldLabel);
            SplineTracer tracer = (SplineTracer)target;
            tracer.direction = (Spline.Direction)EditorGUILayout.EnumPopup("Direction", tracer.direction);
            tracer.physicsMode = (SplineFollower.PhysicsMode)EditorGUILayout.EnumPopup("Physics mode", tracer.physicsMode);
            if (tracer.physicsMode == SplineFollower.PhysicsMode.Rigidbody)
            {
                Rigidbody rb = tracer.GetComponent<Rigidbody>();
                if (rb == null) EditorGUILayout.HelpBox("Assign a Rigidbody component.", MessageType.Error);
                else if (rb.interpolation == RigidbodyInterpolation.None && tracer.updateMethod != SplineUser.UpdateMethod.FixedUpdate) EditorGUILayout.HelpBox("Switch to FixedUpdate mode to ensure smooth update for non-interpolated rigidbodies", MessageType.Warning);

            }
            else if (tracer.physicsMode == SplineFollower.PhysicsMode.Rigidbody2D)
            {
                Rigidbody2D rb = tracer.GetComponent<Rigidbody2D>();
                if (rb == null) EditorGUILayout.HelpBox("Assign a Rigidbody2D component.", MessageType.Error);
                else if (rb.interpolation == RigidbodyInterpolation2D.None && tracer.updateMethod != SplineUser.UpdateMethod.FixedUpdate) EditorGUILayout.HelpBox("Switch to FixedUpdate mode to ensure smooth update for non-interpolated rigidbodies", MessageType.Warning);
            }

            motionEditor.DrawInspector();
            customOffsetEditor.allowSelection = editIndex == -1;
            customOffsetEditor.DrawInspector();
            customRotationEditor.allowSelection = editIndex == -1;
            customRotationEditor.DrawInspector();
            triggerFoldout = EditorGUILayout.Foldout(triggerFoldout, "Triggers");
            if (triggerFoldout)
            {
                int lastTrigger = trigger;
                SplineEditorGUI.TriggerArray(ref tracer.triggers, ref trigger);
                if (lastTrigger != trigger) Repaint();
            }
            cameraFoldout = EditorGUILayout.Foldout(cameraFoldout, "Camera preview");
            if (cameraFoldout) { 
                if (cam == null)
                {
                    cam = tracer.GetComponentInChildren<Camera>();
                }
                if (cam != null)
                {
                    if (rt == null || rt.width != GetRTWidth() || rt.height != GetRTHeight()) CreateRT();
                    GUILayout.Box("", GUILayout.Width(rt.width), GUILayout.Height(rt.height));
                    RenderTexture prevTarget = cam.targetTexture;
                    RenderTexture prevActive = RenderTexture.active;
                    CameraClearFlags lastFlags = cam.clearFlags;
                    Color lastColor = cam.backgroundColor;
                    cam.targetTexture = rt;
                    cam.clearFlags = CameraClearFlags.Color;
                    cam.backgroundColor = Color.black;
                    cam.Render();
                    RenderTexture.active = rt;
                    renderCanvas.SetPixels(new Color[renderCanvas.width * renderCanvas.height]);
                    renderCanvas.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); 
                    renderCanvas.Apply();
                    RenderTexture.active = prevActive;
                    cam.targetTexture = prevTarget;
                    cam.clearFlags = lastFlags;
                    cam.backgroundColor = lastColor;
                    GUI.DrawTexture(GUILayoutUtility.GetLastRect(), renderCanvas, ScaleMode.StretchToFill);
                }
                else EditorGUILayout.HelpBox("There is no camera attached to the selected object or its children.", MessageType.Info);
            }
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            SplineTracer tracer = (SplineTracer)target;

            if (triggerFoldout)
            {
                for (int i = 0; i < tracer.triggers.Length; i++)
                {
                    SplineEditorHandles.SplineSliderGizmo gizmo = SplineEditorHandles.SplineSliderGizmo.DualArrow;
                    switch (tracer.triggers[i].type)
                    {
                        case Trigger.Type.Backward: gizmo = SplineEditorHandles.SplineSliderGizmo.BackwardTriangle; break;
                        case Trigger.Type.Forward: gizmo = SplineEditorHandles.SplineSliderGizmo.ForwardTriangle; break;
                        case Trigger.Type.Double: gizmo = SplineEditorHandles.SplineSliderGizmo.DualArrow; break;
                    }
                    double last = tracer.triggers[i].position;
                    if (SplineEditorHandles.Slider(tracer, ref tracer.triggers[i].position, tracer.triggers[i].color, tracer.triggers[i].name, gizmo) || last != tracer.triggers[i].position)
                    {
                        trigger = i;
                        Repaint();
                    }
                }
            }
            if (customOffsetEditor.isOpen)
            {
                if (customOffsetEditor.DrawScene(tracer)) tracer.Rebuild(false);
            }

            if (customRotationEditor.isOpen)
            {
                if (customRotationEditor.DrawScene(tracer)) tracer.Rebuild(false);
            }
        }

        protected void DrawResult(SplineResult result)
        {
            SplineTracer tracer = (SplineTracer)target;
            Handles.color = Color.white;
            Handles.DrawLine(tracer.transform.position, result.position);
            SplineEditorHandles.DrawSolidSphere(result.position, HandleUtility.GetHandleSize(result.position) * 0.2f);
            Handles.color = Color.blue;
            Handles.DrawLine(result.position, result.position + result.direction * HandleUtility.GetHandleSize(result.position) * 0.5f);
            Handles.color = Color.green;
            Handles.DrawLine(result.position, result.position + result.normal * HandleUtility.GetHandleSize(result.position) * 0.5f);
            Handles.color = Color.red;
            Handles.DrawLine(result.position, result.position + result.right * HandleUtility.GetHandleSize(result.position) * 0.5f);
            Handles.color = Color.white;
        }

        public class DistanceWindow : EditorWindow
        {
            float distance = 0f;
            SplineTracer target;
            DistanceReceiver rcv;
            float length = 0f;
            public void Init(SplineTracer tracer, DistanceReceiver receiver, float totalLength)
            {
                rcv = receiver;
                length = totalLength;
                target = tracer;
                titleContent = new GUIContent("Set Distance");
                minSize = maxSize = new Vector2(240, 90);
            }

            private void OnGUI()
            {
                if(target == null)
                {
                    Close();
                    return;
                }
                distance = EditorGUILayout.FloatField("Distance", distance);
                EditorGUILayout.HelpBox("Enter the distance and press Enter. Current spline length: " + length, MessageType.Info);
                if(Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
                {
                    rcv(distance);
                    Close();
                }
            }
        }
    }
}
