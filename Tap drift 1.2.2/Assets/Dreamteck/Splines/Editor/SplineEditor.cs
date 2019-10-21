#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SplineComputer), true)]
    public partial class SplineEditor : Editor 
    {
        public enum PointTool { None, Create, Delete, Move, Rotate, Scale, NormalEdit, Mirror, Merge };
        public PointTool tool = PointTool.None;
        public int createPointMode = 0;
        public int createPointMode2D = 0;
        public bool createNodeOnCreatePoint = false;
        public int appendMode = 0;
        private int lastAppendMode = 0;
        public float createPointOffset = 0f;
        public int createNormalMode = 0;
        private Spline createPointVisualizer;
        private bool mirrorMode = false;
        private bool mergeMode = false;
        private bool splitMode = false;

        private List<int> selectedPoints = new List<int>();

        public int[] pointSelection
        {
            get
            {
                return selectedPoints.ToArray();
            }
        }
        public bool mouseLeft = false;
        public bool mouseRight = false;
        public bool mouseLeftDown = false;
        public bool mouseRightDown = false;
        public bool mouseLeftUp = false;
        public bool mouserightUp = false;
        public bool control = false;
        public bool shift = false;
        public bool alt = false;
        public SplineComputer computer;


        private bool closedOnMirror = false;
        private Tool lastTool = Tool.None;
        private Camera editorCamera = null;
        private bool interpolationFoldout = false;
        private static bool isFocused = false;

        public static float addSize = 1f;
        public static Color addColor = Color.white;

        private SplinePoint[] points = new SplinePoint[0];


        private bool emptyClick = false;

        private TS_Transform tsTransform;
        private bool movePivot = false;
        private Vector3 idealPivot = Vector3.zero;

        public SplinePointPositionEditor positionEditor = new SplinePointPositionEditor();
        public SplinePointScaleEditor scaleEditor = new SplinePointScaleEditor();
        public SplinePointNormalEditor normalEditor = new SplinePointNormalEditor();
        public SplinePointRotationEditor rotationEditor = new SplinePointRotationEditor();
        public SplinePointMirrorEditor mirrorEditor = new SplinePointMirrorEditor();
        public SplineComputerMergeEditor mergeEditor = new SplineComputerMergeEditor();
        public SplineComputerSplitEditor splitEditor = new SplineComputerSplitEditor();

        public SplinePointDefaultEditor defaultEditor = new SplinePointDefaultEditor();

        private PointSelector pointSelector;

        private bool refreshEditors = false;

        private Vector2 lastClickPoint = Vector2.zero;
        public static bool hold = false;

        MorphWindow morphWindow = null;

        private bool precisionWarning = false;
        private float targetPrecision = 0f;

        private Quaternion selectedPointOrientation = Quaternion.identity;
        private bool exitedCreateMode = false;


        public int selectedPointsCount
        {
            get { return selectedPoints.Count; }
            set { }
        }

        [MenuItem("GameObject/Dreamteck/Spline/Computer")]
        private static void NewEmptySpline()
        {
            int count = GameObject.FindObjectsOfType<SplineComputer>().Length;
            string objName = "Spline";
            if (count > 0) objName += " " + count;
            GameObject obj = new GameObject(objName);
            obj.AddComponent<SplineComputer>();
            if (Selection.activeGameObject != null)
            {
                if (EditorUtility.DisplayDialog("Make child?", "Do you want to make the new spline a child of " + Selection.activeGameObject.name + "?", "Yes", "No"))
                {
                    obj.transform.parent = Selection.activeGameObject.transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                }
            }
            Selection.activeGameObject = obj;
        }

        [MenuItem("GameObject/Dreamteck/Spline/Node")]
        private static void NewSplineNode()
        {
            int count = GameObject.FindObjectsOfType<Node>().Length;
            string objName = "Node";
            if (count > 0) objName += " " + count;
            GameObject obj = new GameObject(objName);
            obj.AddComponent<Node>();
            if(Selection.activeGameObject != null)
            {
                if(EditorUtility.DisplayDialog("Make child?", "Do you want to make the new node a child of " + Selection.activeGameObject.name + "?", "Yes", "No"))
                {
                    obj.transform.parent = Selection.activeGameObject.transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                }
            }
            Selection.activeGameObject = obj;
        }

        public void Refresh()
        {
            points = computer.GetPoints();
            RefreshPointEditors();
            computer.EditorUpdateConnectedNodes();
            computer.Rebuild();
        }

        void OnEnable()
        {
            EnableToolbar();
            computer = (SplineComputer)target;
            computer.EditorAwake();
            Refresh();
            positionEditor.computer = computer;
            defaultEditor.computer = computer;
            scaleEditor.computer = computer;
            rotationEditor.computer = computer;
            normalEditor.computer = computer;
            mirrorEditor.computer = computer;
            mergeEditor.computer = computer;
            splitEditor.computer = computer;
            tsTransform = new TS_Transform(computer.transform);
            tool = PointTool.None;
            lastTool = Tools.current;
            Tools.current = Tool.None;
            hold = false;
            ClearSelection();
            positionEditor.LoadState();
            scaleEditor.LoadState();
            rotationEditor.LoadState();
            normalEditor.LoadState();
            mirrorEditor.LoadState();
            Undo.undoRedoPerformed += Refresh;
            pointSelector = new PointSelector("Select Points",computer, true, SelectPoints);
            surfaceLayerMask = EditorPrefs.GetInt("SplineEditor_surfaceLayerMask", ~0);
            editSpace = EditorPrefs.GetInt("SplineEditor_editSpace", 0);
            createPointMode = EditorPrefs.GetInt("SplineEditor_createPointMode", 0);
            createPointMode2D = EditorPrefs.GetInt("SplineEditor_createPointMode2D", 0);
            appendMode = EditorPrefs.GetInt("SplineEditor_appendMode", 0);
            lastAppendMode = appendMode;
            createNodeOnCreatePoint = EditorPrefs.GetBool("SplineEditor_createNodeOnCreatePoint", false);
            createNormalMode = EditorPrefs.GetInt("SplineEditor_createNormalMode", 0);
            createPointOffset = EditorPrefs.GetFloat("SplineEditor_createPointOffset", 0f);
        }

        void OnDisable()
        {
            DisableToolbar();
            if (Tools.current == Tool.None) Tools.current = lastTool;
            positionEditor.SaveState();
            scaleEditor.SaveState();
            rotationEditor.SaveState();
            normalEditor.SaveState();
            mirrorEditor.SaveState();
            Undo.undoRedoPerformed -= Refresh;
            EditorPrefs.SetInt("SplineEditor_surfaceLayerMask", surfaceLayerMask);
            EditorPrefs.SetInt("SplineEditor_createPointMode", createPointMode);
            EditorPrefs.SetInt("SplineEditor_createPointMode2D", createPointMode2D);
            EditorPrefs.SetInt("SplineEditor_appendMode", appendMode);
            EditorPrefs.SetInt("SplineEditor_editSpace", editSpace);
            EditorPrefs.SetInt("SplineEditor_createNormalMode", createNormalMode);
            EditorPrefs.SetFloat("SplineEditor_createPointOffset", createPointOffset);
            EditorPrefs.SetBool("SplineEditor_createNodeOnCreatePoint", createNodeOnCreatePoint);
            if (morphWindow != null) morphWindow.Close();
            if (InMirrorMode()) ExitMirrorMode();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Undo.RecordObject(computer, "Edit Points");
            computer = (SplineComputer)target;
            if (computer.hasMorph && morphWindow == null && HasSelection()) ClearSelection();
            for (int i = 0; i < selectedPoints.Count; i++)
            {
                if (selectedPoints[i] >= points.Length)
                {
                    ClearSelection();
                    break;
                }
            }

            computer.space = (SplineComputer.Space)EditorGUILayout.EnumPopup("Space", computer.space);
            Spline.Type lastType = computer.type;
            Spline.Type newType = computer.type;
            newType = (Spline.Type)EditorGUILayout.EnumPopup("Type", newType);
            if (lastType != newType)
            {
                if (newType == Spline.Type.Bezier)
                {
                    if (EditorUtility.DisplayDialog("Convert to Bezier", "Would you like to convert the tangents to Bezier. This will retain the shape of the spline.", "Yes", "No"))
                    {
                        computer.ConvertToBezier();
                        Refresh();
                    }
                    else computer.type = newType;
                } else computer.type = newType;
            } 
            if (points.Length > 1)
            {
                if (computer.type != Spline.Type.Linear)
                {
                    float precision = (float)computer.precision;
                    float lastPrecision = precision;

                    if (precisionWarning) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    precision = EditorGUILayout.Slider("Precision", precision, 0f, 0.9999f);
                    GUI.color = Color.white;
                    if (lastPrecision <= 0.99f && precision > 0.99f && computer.subscriberCount > 0)
                    {
                        if (!precisionWarning)
                        {
                            targetPrecision = precision;
                            precisionWarning = true;
                        }
                    }
                   
                    if (precisionWarning)
                    {
                        computer.precision = lastPrecision;
                        EditorGUILayout.HelpBox("You are about to set a high precision value for this spline. Note that this spline computer is used by SplineUser components and in some cases this action may cause the editor to freeze. Are you sure you want to continue increasing?", MessageType.Warning);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Yes"))
                        {
                            computer.precision = targetPrecision;
                            precisionWarning = false;
                        }
                        if (GUILayout.Button("No"))
                        {
                            computer.precision = 0.99f;
                            precisionWarning = false;
                        }
                        EditorGUILayout.EndHorizontal();
                    } else  computer.precision = precision;
                }
                interpolationFoldout = EditorGUILayout.Foldout(interpolationFoldout, "Custom interpolation");
                if (interpolationFoldout)
                {
                    if (computer.customValueInterpolation == null || computer.customValueInterpolation.keys.Length == 0)
                    {
                        if (GUILayout.Button("Add Value Interpolation"))
                        {
                            AnimationCurve curve = new AnimationCurve();
                            curve.AddKey(new Keyframe(0, 0, 0, 0));
                            curve.AddKey(new Keyframe(1, 1, 0, 0));
                            computer.customValueInterpolation = curve;
                        }
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        computer.customValueInterpolation = EditorGUILayout.CurveField("Value interpolation", computer.customValueInterpolation);
                        if (GUILayout.Button("x", GUILayout.MaxWidth(25))) computer.customValueInterpolation = null;
                        EditorGUILayout.EndHorizontal();
                    }
                    if (computer.customNormalInterpolation == null || computer.customNormalInterpolation.keys.Length == 0)
                    {
                        if (GUILayout.Button("Add Normal Interpolation"))
                        {
                            AnimationCurve curve = new AnimationCurve();
                            curve.AddKey(new Keyframe(0, 0, 0, 0));
                            curve.AddKey(new Keyframe(1, 1, 0, 0));
                            computer.customNormalInterpolation = curve;
                        }
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        computer.customNormalInterpolation = EditorGUILayout.CurveField("Normal interpolation", computer.customNormalInterpolation);
                        if (GUILayout.Button("x", GUILayout.MaxWidth(25))) computer.customNormalInterpolation = null;
                        EditorGUILayout.EndHorizontal();
                    }
                }
                
            }
            EditorGUILayout.BeginHorizontal();
            string buttonText = "Close Path";
            bool highlight = false;
            if (computer.isClosed)
            {
                buttonText = "Break Path";
                if (selectedPointsCount == 1) highlight = true;
                if (computer.pointCount < 4) computer.Break();
            }
            if (SplineEditorGUI.EditorLayoutSelectableButton(new GUIContent(buttonText), computer.pointCount >= 4 && !computer.hasMorph, highlight))
            {
                if (computer.isClosed)
                {
                    if (selectedPointsCount == 1) BreakSelected();
                    else BreakPath();
                } else if(computer.pointCount >= 4) ClosePath();
            }
            if (SplineEditorGUI.EditorLayoutSelectableButton(new GUIContent("Inverse Point Order"), computer.pointCount >= 2 && !computer.hasMorph, false)) InversePointOrder();
            if (SplineEditorGUI.EditorLayoutSelectableButton(new GUIContent("Morph states"), computer.pointCount > 0, computer.hasMorph))
            {
                if (morphWindow == null)
                {
                    morphWindow = EditorWindow.GetWindow<MorphWindow>();
                    morphWindow.Init(this, new Vector2(150, 300), new Vector2(1000, 1000));
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            Color prevColor = GUI.color;
            if (computer.hasMorph) GUI.color = new Color(1f, 1f, 1f, 0.5f);
            if (computer.is2D)
            {
                if (GUILayout.Button("2D", SplineEditorGUI.bigButton, GUILayout.Width(55), GUILayout.Height(55)) && !computer.hasMorph)
                {
                    Undo.RecordObject(computer, "Convert to 3D");
                    computer.is2D = false;
                }
            }
            else
            {
                if (GUILayout.Button("3D", SplineEditorGUI.bigButton, GUILayout.Width(55), GUILayout.Height(55)) && !computer.hasMorph)
                {
                    Undo.RecordObject(computer, "Convert to 2D");
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i].position.z = 0f;
                        points[i].tangent.z = 0f;
                        points[i].tangent2.z = 0f;
                        points[i].normal = Vector3.back;
                    }
                    computer.SetPoints(points);
                    Refresh();
                    RefreshPointEditors();
                    computer.is2D = true;
                }
            }
            GUI.color = prevColor;
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Drawing", EditorStyles.boldLabel);
            bool lastAlwaysDraw = computer.alwaysDraw;
            computer.alwaysDraw = GUILayout.Toggle(computer.alwaysDraw, "Always Draw Spline");
            if (lastAlwaysDraw != computer.alwaysDraw)
            {
                if (computer.alwaysDraw) SplineDrawer.RegisterComputer(computer);
                else SplineDrawer.UnregisterComputer(computer);
            }
            computer.drawThinckness = GUILayout.Toggle(computer.drawThinckness, "Draw thickness");
            if (computer.drawThinckness) computer.billboardThickness = GUILayout.Toggle(computer.billboardThickness, "Always face camera");
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Control points", EditorStyles.boldLabel);
            PointMenu();

            if (GUI.changed)
            {
               if (computer.isClosed) points[points.Length - 1] = points[0];
                UpdateComputer();
                EditorUtility.SetDirty(computer);
                SceneView.RepaintAll();
            }
        }

        void InversePointOrder()
        {
            Refresh();
            for(int i = 0; i < Mathf.FloorToInt(points.Length/2); i++)
            {
                SplinePoint temp = points[i];
                points[i] = points[(points.Length - 1) - i];
                Vector3 tempTan = points[i].tangent;
                points[i].tangent = points[i].tangent2;
                points[i].tangent2 = tempTan;
                int opposideIndex = (points.Length - 1) - i;
                points[opposideIndex] = temp;
                tempTan = points[opposideIndex].tangent;
                points[opposideIndex].tangent = points[opposideIndex].tangent2;
                points[opposideIndex].tangent2 = tempTan;
            }
            if (points.Length % 2 != 0) {
                Vector3 tempTan = points[Mathf.CeilToInt(points.Length / 2)].tangent;
                points[Mathf.CeilToInt(points.Length / 2)].tangent = points[Mathf.CeilToInt(points.Length / 2)].tangent2;
                points[Mathf.CeilToInt(points.Length / 2)].tangent2 = tempTan;
            }
            UpdateComputer();
        }

        public bool IsPointSelected(int index)
        {
            return selectedPoints.Contains(index);
        }

        public bool HasSelection()
        {
            return selectedPoints.Count > 0;
        }

        public void ClearSelection()
        {
            selectedPoints.Clear();
            SceneView.RepaintAll();
            Repaint();
        }

        public void SelectPoint(int index)
        {
            if (computer.isClosed && index == computer.pointCount - 1) return;
            selectedPoints.Clear();
            selectedPoints.Add(index);
            RefreshPointEditors();
            Repaint();
        }

        void RefreshPointEditors()
        {
            switch (editSpace)
            {
                case 0: selectedPointOrientation = Quaternion.identity; break;
                case 1: selectedPointOrientation = computer.transform.rotation; break;
                case 2:
                    if(selectedPointsCount == 1) selectedPointOrientation = computer.Evaluate((double)selectedPoints[0] / (computer.pointCount - 1)).rotation;
                    else selectedPointOrientation = Quaternion.identity;
                    break;
            }
            positionEditor.Reset(selectedPointOrientation);
            rotationEditor.Reset(ref points, ref selectedPoints, selectedPointOrientation);
            scaleEditor.Reset(ref points, ref selectedPoints, selectedPointOrientation);
        }

        public void SelectPoints(List<int> indices)
        {
            selectedPoints.Clear();
            for (int i = 0; i < indices.Count; i++)
            {
                if (computer.isClosed && i == computer.pointCount - 1) continue;
                selectedPoints.Add(indices[i]);
            }
            RefreshPointEditors();
            Repaint();
        }

        public void AddPointSelection(int index)
        {
            if (computer.isClosed && index == computer.pointCount - 1) return;
            if (selectedPoints.Contains(index)) return;
            selectedPoints.Add(index);
            RefreshPointEditors();
            Repaint();
        }

        void UpdateComputer()
        {
            if (computer.is2D)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    points[i].position.z = 0f;
                    points[i].tangent.z = 0f;
                    points[i].tangent2.z = 0f;
                    points[i].normal = Vector3.back;
                }
            }
            computer.SetPoints(points);
            if (mirrorMode && closedOnMirror)
            {
                if (computer.pointCount >= 4 && !computer.isClosed) computer.Close();
            }
        }

        void OnSceneGUI()
        {
            if (hold) return;
            isFocused = SceneView.currentDrawingSceneView == SceneView.lastActiveSceneView;
            GetInput();
            computer = (SplineComputer)target;
            editorCamera = SceneView.currentDrawingSceneView.camera;
            if (isFocused)
            {
                if (!editorCamera.pixelRect.Contains(Event.current.mousePosition))
                {
                    SplineEditorGUI.Update();
                    if (defaultEditor.isDragging)
                    {
                        defaultEditor.FinishDrag();
                        refreshEditors = true;
                    }
                }
                if (computer.hasMorph && morphWindow == null && HasSelection()) ClearSelection();
                for (int i = 0; i < selectedPoints.Count; i++)
                {
                    if (selectedPoints[i] >= points.Length)
                    {
                        ClearSelection();
                        break;
                    }
                }
            }

            List<SplineComputer> computers = computer.GetConnectedComputers();

            if (tool != PointTool.Create || (!computer.is2D && createPointMode == 1) || (computer.is2D && createPointMode2D == 1) || mouseHoversToolbar)
            {
                if (!InSplitMode())
                {
                    SplineDrawer.DrawSplineComputer(computer);
                    for (int i = 0; i < computers.Count; i++) SplineDrawer.DrawSplineComputer(computers[i], 0.0, 1.0, 0.5f);
                }
            } else if(isFocused)
            {
                if (createPointVisualizer != null)
                {
                    if (createPointVisualizer.points.Length - 1 != points.Length || lastAppendMode != appendMode)
                    {
                        SetVisualizerPoints(Vector3.zero);
                        lastAppendMode = appendMode;
                    }
                    if (createPointVisualizer.isClosed != computer.isClosed)
                    {
                        if (computer.isClosed) createPointVisualizer.Close();
                        else createPointVisualizer.Break();
                    }
                    createPointVisualizer.type = computer.type;
                    createPointVisualizer.customNormalInterpolation = computer.customNormalInterpolation;
                    createPointVisualizer.customValueInterpolation = computer.customValueInterpolation;
                    createPointVisualizer.precision = computer.precision;
                    SplineDrawer.DrawSpline(createPointVisualizer, computer.editorPathColor, 0.0, 1.0, computer.drawThinckness, computer.billboardThickness);
                }
            }


            if (isFocused)
            {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete && HasSelection())
                {
                    DeleteSelectedPoints();
                    Event.current.Use();
                }

                if (tool == PointTool.Delete || tool == PointTool.Create) selectedPoints.Clear();

                if (tool == PointTool.Create)
                {
                    if (mouseLeftDown && mouseRight)
                    {
                        ToggleCreateTool();
                        emptyClick = false;
                        exitedCreateMode = true;
                    }
                    if (!mouseHoversToolbar)
                    {
                        if (computer.is2D) CreatePoint2D();
                        else CreatePoint();
                    }
                    SceneView.RepaintAll();
                }
            }

            if (Tools.current == Tool.None && (!computer.hasMorph || MorphWindow.editShapeMode) && !alt && !(mouseRight && mouseLeft) ) EditPoints();

            if (Tools.current == Tool.None)
            {
                float axisSize = HandleUtility.GetHandleSize(computer.transform.position);
                Handles.color = Color.red;
                Handles.DrawLine(computer.transform.position, computer.transform.position + computer.transform.right * 0.3f * axisSize);
                Handles.color = Color.green;
                Handles.DrawLine(computer.transform.position, computer.transform.position + computer.transform.up * 0.3f * axisSize);
                Handles.color = Color.blue;
                Handles.DrawLine(computer.transform.position, computer.transform.position + computer.transform.forward * 0.3f * axisSize);
                Vector3 lastPos = computer.transform.position;
                Undo.RecordObject(computer.transform, "Move");
                Handles.color = new Color(1f, 1f, 1f, 0.5f);
                computer.transform.position = SplineEditorHandles.FreeMoveRectangle(computer.transform.position, axisSize * 0.07f);
                Handles.color = Color.white;
                if (lastPos != computer.transform.position) emptyClick = false;
            }

            if(isFocused) SplineEditorGUI.Reset();
            Handles.BeginGUI();
            if (!defaultEditor.isDragging && Tools.current == Tool.None) DrawToolbar();
            else if (Tools.current != Tool.None)
            {
                Rect rect = new Rect(5 * SplineEditorGUI.scale, 5 * SplineEditorGUI.scale, 150 * SplineEditorGUI.scale, 30 * SplineEditorGUI.scale);
                if (rect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
                if (SplineEditorGUI.Button(rect, "Enter Edit Mode")) {
                    lastTool = Tools.current;
                    Tools.current = Tool.None;
                }
                DreamteckEditorGUI.Label(new Rect(EditorGUIUtility.currentViewWidth / 2f - 80 * SplineEditorGUI.scale, 5 * SplineEditorGUI.scale, 160 * SplineEditorGUI.scale, 40 * SplineEditorGUI.scale), "Editing the Transform");
            }
            Handles.EndGUI();


            bool rebuild = false;
            if (!Application.isPlaying)
            {
                if (tsTransform == null || tsTransform.transform == null)
                {
                    rebuild = true;
                    tsTransform = new TS_Transform(computer.transform);
                }
                rebuild = tsTransform.HasChange();
                if (rebuild)
                {
                    //Update the linked nodes when the computer's transform moves
                    for (int i = 0; i < computer.nodeLinks.Length; i++)
                    {
                        computer.nodeLinks[i].node.UpdatePoint(computer, computer.nodeLinks[i].pointIndex, points[computer.nodeLinks[i].pointIndex]);
                        computer.nodeLinks[i].node.UpdateConnectedComputers(computer);
                    }
                }
                tsTransform.Update();
            }


            if (GUI.changed || rebuild)
            {
                if (rebuild)
                {
                    List<SplineComputer> computerList = computer.GetConnectedComputers();
                    for (int i = 0; i < computerList.Count; i++)
                    {
                        computerList[i].RebuildImmediate();
                    }
                }
                if (computer.isClosed && points.Length >= 4) points[points.Length - 1] = points[0];
                if (!GUI.changed) Refresh();
                UpdateComputer();
                EditorUtility.SetDirty(computer);
            }

            if (isFocused)
            {
                if (emptyClick && !alt && !control && !exitedCreateMode)
                {
                    if (Tools.current == Tool.None && tool != PointTool.Create && tool != PointTool.Delete)
                    {
                        if (mouseLeftUp)
                        {
                            if (morphWindow == null)
                            {
                                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                                RaycastHit hit;
                                if (Physics.Raycast(ray, out hit, Mathf.Infinity)) Selection.activeGameObject = hit.transform.gameObject;
                                else Selection.activeGameObject = null;
                            }
                        }
                    }
                }
                else if (mouseLeftUp) exitedCreateMode = false;

                if (alt && defaultEditor.isDragging)
                {
                    defaultEditor.FinishDrag();
                    refreshEditors = true;
                    Repaint();
                }
            }
        }

        void FramePoints()
        {
            if (points.Length == 0) return;
            Vector3 center = Vector3.zero;
            Camera camera = SceneView.lastActiveSceneView.camera;
            Transform cam = camera.transform;
            Vector3 min = Vector3.zero, max = Vector3.zero;
            if (HasSelection())
            {
                for (int i = 0; i < selectedPoints.Count; i++)
                {
                    center += points[selectedPoints[i]].position;
                    Vector3 local = cam.InverseTransformPoint(points[selectedPoints[i]].position);
                    if (local.x < min.x) min.x = local.x;
                    if (local.y < min.y) min.y = local.y;
                    if (local.z < min.z) min.z = local.z;
                    if (local.x > max.x) max.x = local.x;
                    if (local.y > max.y) max.y = local.y;
                    if (local.z > max.z) max.z = local.z;
                }
                center /= selectedPointsCount;
            }
            else
            {
                for (int i = 0; i < points.Length; i++)
                {
                    center += points[i].position;
                    Vector3 local = cam.InverseTransformPoint(points[i].position);
                    if (local.x < min.x) min.x = local.x;
                    if (local.y < min.y) min.y = local.y;
                    if (local.z < min.z) min.z = local.z;
                    if (local.x > max.x) max.x = local.x;
                    if (local.y > max.y) max.y = local.y;
                    if (local.z > max.z) max.z = local.z;
                }
               center /= points.Length;
            }
            movePivot = true;
            idealPivot = center;
        }

        void CreatePoint()
        {
            Vector3 createPoint = Vector3.zero;
            Vector3 normal = Vector3.up;
            bool canCreate = false;
            if (createPointMode == 0)
            {
                GetCreatePointOnPlane(-editorCamera.transform.forward, editorCamera.transform.position + editorCamera.transform.forward * createPointOffset, out createPoint);
                Handles.color = new Color(1f, 0.78f, 0.12f);
                DrawGrid(createPoint, editorCamera.transform.forward, Vector2.one * 10, 2.5f);
                Handles.color = Color.white;
                canCreate = true;
                normal = -editorCamera.transform.forward;
            }

            if (createPointMode == 2)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity,  surfaceLayerMask))
                {
                    canCreate = true;
                    createPoint = hit.point + hit.normal * createPointOffset;
                    Handles.color = Color.blue;
                    Handles.DrawLine(hit.point, createPoint);
                    SplineEditorHandles.DrawRectangle(createPoint, Quaternion.LookRotation(-editorCamera.transform.forward, editorCamera.transform.up), HandleUtility.GetHandleSize(createPoint) * 0.1f);
                    Handles.color = Color.white;
                    normal = hit.normal;
                }
            }

            if (createPointMode == 3)
            {
                canCreate = AxisGrid(Vector3.right, new Color(0.85f, 0.24f, 0.11f, 0.92f), out createPoint);
                normal = Vector3.right;
            }

            if (createPointMode == 4)
            {
                canCreate = AxisGrid(Vector3.up, new Color(0.6f, 0.95f, 0.28f, 0.92f), out createPoint);
                normal = Vector3.up;
            }

            if (createPointMode == 5)
            {
                canCreate = AxisGrid(Vector3.forward, new Color(0.22f, 0.47f, 0.97f, 0.92f), out createPoint);
                normal = Vector3.back;
            }
            if (createPointMode == 1)
            {
                canCreate = true;
                if (points.Length < 2) createPointMode = 0;
                else InsertSplinePoint(Event.current.mousePosition);
            } else if (mouseLeftDown && !mouseHoversToolbar && canCreate && !mouseRight) CreateSplinePoint(createPoint, normal);
            if (!canCreate) DrawMouseCross();

            if(createPointVisualizer != null)
            {
                if(appendMode == 0) createPointVisualizer.points[createPointVisualizer.points.Length - 1].SetPosition(createPoint);
                else createPointVisualizer.points[0].SetPosition(createPoint);
                if (createPointVisualizer.isClosed) createPointVisualizer.points[0].SetPosition(createPoint);
            }
        }

        void CreatePoint2D()
        {
            Vector3 createPoint = Vector3.zero;
            Vector3 normal = Vector3.up;
            bool canCreate = false;

            if (createPointMode2D == 0)
            {
                canCreate = AxisGrid(Vector3.forward, new Color(0.22f, 0.47f, 0.97f, 0.92f), out createPoint);
                normal = Vector3.back;
            }
            if (createPointMode2D == 1)
            {
                canCreate = true;
                if (points.Length < 2) createPointMode2D = 0;
                else InsertSplinePoint(Event.current.mousePosition);
            } else if (mouseLeftDown && !mouseHoversToolbar && canCreate && !mouseRight) CreateSplinePoint(createPoint, normal);

            if (createPointVisualizer != null)
            {
                if (appendMode == 0) createPointVisualizer.points[createPointVisualizer.points.Length - 1].SetPosition(createPoint);
                else createPointVisualizer.points[0].SetPosition(createPoint);
                if (createPointVisualizer.isClosed) createPointVisualizer.points[0].SetPosition(createPoint);
            }
        }

        private void SetVisualizerPoints(Vector3 newPointPosition)
        {
            SplinePoint[] p = new SplinePoint[points.Length + 1];
            if (appendMode == 0)
            {
                for (int i = 0; i < points.Length; i++) p[i] = points[i];
                Vector3 normal = Vector3.up;
                if (points.Length > 0) normal = points[points.Length-1].normal;
                p[p.Length - 1] = new SplinePoint(Vector3.zero, Vector3.zero, normal, 1f, Color.white);
            } else
            {
                for (int i = 1; i < p.Length; i++) p[i] = points[i-1];
                Vector3 normal = Vector3.up;
                if (points.Length > 0) normal = points[0].normal;
                p[0] = new SplinePoint(Vector3.zero, Vector3.zero, normal, 1f, Color.white);
            }
            createPointVisualizer.points = p;
        }

        bool AxisGrid(Vector3 axis, Color color, out Vector3 origin)
        {
            float dot = Vector3.Dot(editorCamera.transform.position.normalized, axis);
            if (dot < 0f) axis = -axis;
            Plane plane = new Plane(axis, Vector3.zero);
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float rayDistance;
            if (plane.Raycast(ray, out rayDistance))
            {
                origin = ray.GetPoint(rayDistance) + axis * createPointOffset;
                Handles.color = color;
                float distance = 1f;
                ray = new Ray(editorCamera.transform.position, -axis);
                if (!editorCamera.orthographic && plane.Raycast(ray, out rayDistance)) distance = Vector3.Distance(editorCamera.transform.position + axis * createPointOffset, origin);
                else if (editorCamera.orthographic) distance = 2f * editorCamera.orthographicSize;
                DrawGrid(origin, axis, Vector2.one * distance * 0.3f, distance*2.5f * 0.03f);
                Handles.DrawLine(origin, origin - axis * createPointOffset);
                Handles.color = Color.white;
                return true;
            }
            else
            {
                origin = Vector3.zero;
                return false;
            }
        }

        private void DrawMouseCross()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Vector3 origin = ray.GetPoint(1f);
            float size = 0.4f * HandleUtility.GetHandleSize(origin);
            Vector3 a = origin + editorCamera.transform.up * size - editorCamera.transform.right * size;
            Vector3 b = origin - editorCamera.transform.up * size + editorCamera.transform.right * size;
            Handles.color = Color.red;
            Handles.DrawLine(a, b);
            a = origin - editorCamera.transform.up * size - editorCamera.transform.right * size;
            b = origin + editorCamera.transform.up * size + editorCamera.transform.right * size;
            Handles.DrawLine(a, b);
            Handles.color = Color.white;
        }

        private double ProjectScreenSpace(Vector2 screenPoint)
        {
            float closestDistance = (screenPoint - HandleUtility.WorldToGUIPoint(points[0].position)).sqrMagnitude;
            double closestPercent = 0.0;
            double add = computer.moveStep;
            if (computer.type == Spline.Type.Linear) add /= 2.0;
            int count = 0;
            for (double i = add; i < 1.0; i += add)
            {
                SplineResult result = computer.Evaluate(i);
                Vector2 point = HandleUtility.WorldToGUIPoint(result.position);
                float dist = (point - screenPoint).sqrMagnitude;
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestPercent = i;
                }
                count++;
            }
            return closestPercent;
        }

        bool GetCreatePointOnPlane(Vector3 normal, Vector3 origin, out Vector3 result)
        {
            Plane plane = new Plane(normal, origin);
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float rayDistance;
            if (plane.Raycast(ray, out rayDistance))
            {
                result = ray.GetPoint(rayDistance);
                return true;
            } else if(normal == Vector3.zero)
            {
                result = origin;
                return true;
            }
            else
            {
                result = ray.GetPoint(0f);
                return true;
            }
        }

        string GetSelectedPointNodeNamesString(int maxLength)
        {
            string result = "";
            for(int i = 0; i < computer.nodeLinks.Length; i++)
            {
                if (selectedPoints.Contains(computer.nodeLinks[i].pointIndex) && computer.nodeLinks[i].node.GetConnections().Length == 1)
                {
                    if (result != "") result += ", ";
                    string trimmed = computer.nodeLinks[i].node.name.Trim();
                    if (result.Length + trimmed.Length > maxLength) result += "...";
                    else result += computer.nodeLinks[i].node.name.Trim();
                }
            }
            return result;
        }


        private void CreateSplinePoint(Vector3 position, Vector3 normal)
        {
            if (alt) return;
            Undo.RecordObject(computer, "Create Point");
            EditorUtility.SetDirty(computer);
            computer = (SplineComputer)target;
            SplinePoint newPoint = new SplinePoint(position, position);
            newPoint.size = addSize;
            newPoint.color = addColor;
            SplinePoint[] newPoints = new SplinePoint[points.Length + 1];
            List<int> indices = new List<int>();
            List<Node> nodes = new List<Node>();
            if (appendMode == 1)
            {
                for (int i = computer.nodeLinks.Length - 1; i >= 0 ; i--)
                {
                    if (computer.nodeLinks[i].node == null) continue;
                    Undo.RecordObject(computer.nodeLinks[i].node, "Create Point");
                    EditorUtility.SetDirty(computer.nodeLinks[i].node);
                    indices.Add(computer.nodeLinks[i].pointIndex);
                    nodes.Add(computer.nodeLinks[i].node);
                    computer.nodeLinks[i].node.RemoveConnection(computer, computer.nodeLinks[i].pointIndex);
                }
            }

            for (int i = 0; i < newPoints.Length; i++)
            {
                if (appendMode == 0)
                {
                    if (i < points.Length) newPoints[i] = points[i];
                    else newPoints[i] = newPoint;
                }
                else
                {
                    if (i == 0) newPoints[i] = newPoint;
                    else newPoints[i] = points[i-1];
                }
            }

            if (computer.isClosed) newPoints[0] = newPoint;
            points = newPoints;
            bool closeSpline = false;
            if (!computer.isClosed && points.Length > 2)
            {
                Vector2 first = HandleUtility.WorldToGUIPoint(points[0].position);
                Vector2 last = HandleUtility.WorldToGUIPoint(points[points.Length - 1].position);
                if (Vector2.Distance(first, last) <= 20f) if (EditorUtility.DisplayDialog("Close spline?", "Do you want to make the spline path closed ?", "Yes", "No")) closeSpline = true;
            }
            if (computer.is2D) points[points.Length - 1].normal = Vector3.back;
            else
            {
                if (createNormalMode == 0) points[points.Length - 1].normal = normal;
                else
                {
                    List<int> selected = new List<int>();
                    selected.Add(points.Length - 1);
                    normalEditor.SetNormals(ref points, ref selected);
                }
            }
            UpdateComputer();
            if(appendMode == 1)
            {
                for (int i = 0; i < indices.Count; i++) nodes[i].AddConnection(computer, indices[i] + 1);
            }
            if (createNodeOnCreatePoint)
            {
                if (appendMode == 0) CreateNodeForPoint(points.Length - 1);
                else CreateNodeForPoint(0);
            }
            if (closeSpline) computer.Close();
            EditorUtility.SetDirty(computer);
        }

        private void InsertSplinePoint(Vector3 screenCoordinates)
        {
            if (alt) return;
            double percent = ProjectScreenSpace(screenCoordinates);
            SplineResult result = computer.Evaluate(percent);
            if (mouseRight)
            {
                SplineEditorHandles.DrawCircle(result.position, Quaternion.LookRotation(editorCamera.transform.position - result.position), HandleUtility.GetHandleSize(result.position) * 0.2f);
                return;
            }
            if (SplineEditorHandles.CircleButton(result.position, Quaternion.LookRotation(editorCamera.transform.position - result.position), HandleUtility.GetHandleSize(result.position) * 0.2f, 1.5f, computer.editorPathColor))
            {
                Undo.RecordObject(computer, "Create Point");
                EditorUtility.SetDirty(computer);
                SplinePoint newPoint = new SplinePoint(result.position, result.position);
                newPoint.size = result.size;
                newPoint.color = result.color;
                newPoint.normal = result.normal;
                SplinePoint[] newPoints = new SplinePoint[points.Length + 1];
                double floatIndex = (points.Length - 1) * percent;
                int pointIndex = Mathf.Clamp(DMath.FloorInt(floatIndex), 0, points.Length - 2);
                for (int i = 0; i < newPoints.Length; i++)
                {
                    if (i <= pointIndex) newPoints[i] = points[i];
                    else if (i == pointIndex + 1) newPoints[i] = newPoint;
                    else newPoints[i] = points[i - 1];
                }
                List<Node> nodes = new List<Node>();
                List<int> indices = new List<int>();
                for (int i = computer.nodeLinks.Length - 1; i >= 0; i--)
                {
                    if (computer.nodeLinks[i].pointIndex > pointIndex)
                    {
                        nodes.Add(computer.nodeLinks[i].node);
                        indices.Add(computer.nodeLinks[i].pointIndex);
                    }
                }
                for (int i = 0; i < nodes.Count; i++)
                {
                    Undo.RecordObject(nodes[i], "Create Point");
                    EditorUtility.SetDirty(nodes[i]);
                    nodes[i].RemoveConnection(computer, indices[i]);
                }
                points = newPoints;
                UpdateComputer();
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].AddConnection(computer, indices[i] + 1);
                }
                if (createNodeOnCreatePoint) CreateNodeForPoint(pointIndex + 1);
            }
        }


        void CreateNodeForPoint(int index)
        {
            GameObject obj = new GameObject("Node_" + (points.Length - 1));
            obj.transform.parent = computer.transform;
            Node node = obj.AddComponent<Node>();
            node.transform.localRotation = Quaternion.identity;
            node.transform.position = points[index].position;
            node.AddConnection(computer, index);
        }

        void PointMenu()
        {
            pointSelector.selection = selectedPoints;
            pointSelector.Draw();
            if (selectedPoints.Count == 0 || points.Length == 0) return;
            //Otherwise show the editing menu + the point selection menu
            Vector3 avgPos = Vector3.zero;
            Vector3 avgTan = Vector3.zero;
            Vector3 avgTan2 = Vector3.zero;
            Vector3 avgNormal = Vector3.zero;
            float avgSize = 0f;
            Color avgColor = Color.clear;

            for (int i = 0; i < selectedPoints.Count; i++)
            {
                avgPos += points[selectedPoints[i]].position;
                avgNormal += points[selectedPoints[i]].normal;
                avgSize += points[selectedPoints[i]].size;
                avgTan += points[selectedPoints[i]].tangent;
                avgTan2 += points[selectedPoints[i]].tangent2;
                avgColor += points[selectedPoints[i]].color;
            }

            avgPos /= selectedPoints.Count;
            avgTan /= selectedPoints.Count;
            avgTan2 /= selectedPoints.Count;
            avgSize /= selectedPoints.Count;
            avgColor /= selectedPoints.Count;
            avgNormal.Normalize();

            SplinePoint avgPoint = new SplinePoint(avgPos, avgPos);
            avgPoint.tangent = avgTan;
            avgPoint.tangent2 = avgTan2;
            avgPoint.size = avgSize;
            avgPoint.color = avgColor;
            avgPoint.type = points[selectedPoints[0]].type;
            SplinePoint.Type lastType = avgPoint.type;
            
            avgPoint.normal = avgNormal;
            string title = "Point";
            if (selectedPoints.Count == 1) title += " " + (selectedPoints[0]+1);
            else title = "Multiple Points (Average values)";
            if (computer.isClosed)
            {
                for (int i = 0; i < selectedPoints.Count; i++)
                {
                    if (selectedPoints[i] == points.Length - 1)
                    {
                        if (selectedPoints.Count - 1 == 1) title = "Point " + selectedPoints[0];
                        break;
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(title);
            if (computer.type == Spline.Type.Bezier)
            {
                if (computer.is2D)
                {
                    avgPoint.SetTangentPosition(TransformedPositionField2D("Tangent 1", avgPoint.tangent));
                    avgPoint.SetTangent2Position(TransformedPositionField2D("Tangent 2", avgPoint.tangent2));
                }
                else
                {
                    avgPoint.SetTangentPosition(TransformedPositionField("Tangent 1", avgPoint.tangent));
                    avgPoint.SetTangent2Position(TransformedPositionField("Tangent 2", avgPoint.tangent2));
                }
            }
            if(computer.is2D) avgPoint.SetPosition(TransformedPositionField2D("Position", avgPoint.position));
            else avgPoint.SetPosition(TransformedPositionField("Position", avgPoint.position));
            if (!computer.is2D)
            {
                EditorGUILayout.LabelField("Normal");
                EditorGUIUtility.labelWidth = 20f;
                if (computer.space == SplineComputer.Space.Local) avgPoint.normal = computer.transform.InverseTransformDirection(avgPoint.normal);
                float last = avgPoint.normal.x;
                avgPoint.normal.x = EditorGUILayout.Slider("X", avgPoint.normal.x, -1f, 1f);
                if (!Mathf.Approximately(last, avgPoint.normal.x))
                {
                    avgPoint.normal.y = Mathf.MoveTowards(avgPoint.normal.y, 0f, Mathf.Abs(last - avgPoint.normal.x) * Mathf.Abs(avgPoint.normal.y));
                    avgPoint.normal.z = Mathf.MoveTowards(avgPoint.normal.z, 0f, Mathf.Abs(last - avgPoint.normal.x) * Mathf.Abs(avgPoint.normal.z));
                }
                last = avgPoint.normal.y;
                avgPoint.normal.y = EditorGUILayout.Slider("Y", avgPoint.normal.y, -1f, 1f);
                if (!Mathf.Approximately(last, avgPoint.normal.y))
                {
                    avgPoint.normal.x = Mathf.MoveTowards(avgPoint.normal.x, 0f, Mathf.Abs(last - avgPoint.normal.y) * Mathf.Abs(avgPoint.normal.x));
                    avgPoint.normal.z = Mathf.MoveTowards(avgPoint.normal.z, 0f, Mathf.Abs(last - avgPoint.normal.y) * Mathf.Abs(avgPoint.normal.z));
                }
                last = avgPoint.normal.z;
                avgPoint.normal.z = EditorGUILayout.Slider("Z", avgPoint.normal.z, -1f, 1f);
                if (!Mathf.Approximately(last, avgPoint.normal.z))
                {
                    avgPoint.normal.x = Mathf.MoveTowards(avgPoint.normal.x, 0f, Mathf.Abs(last - avgPoint.normal.y) * Mathf.Abs(avgPoint.normal.x));
                    avgPoint.normal.y = Mathf.MoveTowards(avgPoint.normal.y, 0f, Mathf.Abs(last - avgPoint.normal.z) * Mathf.Abs(avgPoint.normal.y));
                }
                avgPoint.normal.Normalize();

                EditorGUIUtility.labelWidth = 0f;
                if (avgPoint.normal == Vector3.zero) avgPoint.normal = avgNormal;
                if (computer.space == SplineComputer.Space.Local) avgPoint.normal = computer.transform.TransformDirection(avgPoint.normal);
            }
            avgPoint.size = EditorGUILayout.FloatField("Size", avgPoint.size);
            avgPoint.color = EditorGUILayout.ColorField("Color", avgPoint.color);
            if (computer.type == Spline.Type.Bezier) avgPoint.type = (SplinePoint.Type)EditorGUILayout.EnumPopup("Point Type", avgPoint.type);
            EditorGUILayout.EndVertical();
            if (!EditorGUI.EndChangeCheck()) return;

            for (int i = 0; i < selectedPoints.Count; i++)
            {
                if (!Mathf.Approximately(avgPos.x, avgPoint.position.x))
                {
                    Vector3 newPos = points[selectedPoints[i]].position;
                    newPos.x = avgPoint.position.x;
                    points[selectedPoints[i]].SetPosition(newPos);
                }
                if (!Mathf.Approximately(avgPos.y, avgPoint.position.y))
                {
                    Vector3 newPos = points[selectedPoints[i]].position;
                    newPos.y = avgPoint.position.y;
                    points[selectedPoints[i]].SetPosition(newPos);
                }
                if (!Mathf.Approximately(avgPos.z, avgPoint.position.z))
                {
                    Vector3 newPos = points[selectedPoints[i]].position;
                    newPos.z = avgPoint.position.z;
                    points[selectedPoints[i]].SetPosition(newPos);
                }


                if (avgPoint.normal != avgNormal)  points[selectedPoints[i]].normal = avgPoint.normal;
                if (avgPoint.size != avgSize) points[selectedPoints[i]].size = avgPoint.size;
                if (avgColor != avgPoint.color) points[selectedPoints[i]].color = avgPoint.color;
                if (lastType != avgPoint.type) points[selectedPoints[i]].type = avgPoint.type;

                if (computer.type != Spline.Type.Bezier) continue;

                if (!Mathf.Approximately(avgTan.x, avgPoint.tangent.x))
                {
                    Vector3 newPos = points[selectedPoints[i]].tangent;
                    newPos.x = avgPoint.tangent.x;
                    points[selectedPoints[i]].SetTangentPosition(newPos);
                }

                if (!Mathf.Approximately(avgTan.y, avgPoint.tangent.y))
                {
                    Vector3 newPos = points[selectedPoints[i]].tangent;
                    newPos.y = avgPoint.tangent.y;
                    points[selectedPoints[i]].SetTangentPosition(newPos);
                }

                if (!Mathf.Approximately(avgTan.z, avgPoint.tangent.z))
                {
                    Vector3 newPos = points[selectedPoints[i]].tangent;
                    newPos.z = avgPoint.tangent.z;
                    points[selectedPoints[i]].SetTangentPosition(newPos);
                }

                if (!Mathf.Approximately(avgTan2.x, avgPoint.tangent2.x))
                {
                    Vector3 newPos = points[selectedPoints[i]].tangent2;
                    newPos.x = avgPoint.tangent2.x;
                    points[selectedPoints[i]].SetTangent2Position(newPos);
                    UpdateComputer();
                }
                if (!Mathf.Approximately(avgTan2.y, avgPoint.tangent2.y))
                {
                    Vector3 newPos = points[selectedPoints[i]].tangent2;
                    newPos.y = avgPoint.tangent2.y;
                    points[selectedPoints[i]].SetTangent2Position(newPos);
                    UpdateComputer();
                }
                if (!Mathf.Approximately(avgTan2.z, avgPoint.tangent2.z))
                {
                    Vector3 newPos = points[selectedPoints[i]].tangent2;
                    newPos.z = avgPoint.tangent2.z;
                    points[selectedPoints[i]].SetTangent2Position(newPos);
                    UpdateComputer();
                }

                if (!Mathf.Approximately(avgPos.z, avgPoint.position.z))
                {
                    Vector3 newPos = points[selectedPoints[i]].position;
                    newPos.z = avgPoint.position.z;
                    points[selectedPoints[i]].SetPosition(newPos);
                }
            }
        }

        Vector3 TransformedPositionField(string title, Vector3 worldPoint)
        {
            Vector3 pos = worldPoint;
            if (computer.space == SplineComputer.Space.Local) pos = computer.transform.InverseTransformPoint(worldPoint);
            pos = EditorGUILayout.Vector3Field(title, pos);
            if (computer.space == SplineComputer.Space.Local) pos = computer.transform.TransformPoint(pos);
            return pos;
        }

        Vector2 TransformedPositionField2D(string title, Vector3 worldPoint)
        {
            Vector2 pos = worldPoint;
            if (computer.space == SplineComputer.Space.Local) pos = computer.transform.InverseTransformPoint(worldPoint);
            pos = EditorGUILayout.Vector2Field(title, pos);
            if (computer.space == SplineComputer.Space.Local) pos = computer.transform.TransformPoint(pos);
            return pos;
        }

        public void BreakSelected()
        {
            Undo.RecordObject(computer, "Break path");
            EditorUtility.SetDirty(computer);
            computer.Break(selectedPoints[0]);
            Refresh();
            ClearSelection();
        }

        public void BreakPath()
        {
            Undo.RecordObject(computer, "Break path");
            EditorUtility.SetDirty(computer);
            computer.Break();
            Refresh();
            ClearSelection();
        }

        public void ClosePath()
        {
            Undo.RecordObject(computer, "Close path");
            EditorUtility.SetDirty(computer);
            computer.Close();
            Refresh();
            computer.RebuildImmediate();
            ClearSelection();
        }

        public void CenterSelection()
        {
            Vector3 avg = Vector3.zero;
            for (int i = 0; i < selectedPoints.Count; i++)
            {
                avg += points[selectedPoints[i]].position;
            }
            avg /= selectedPoints.Count;
            Vector3 delta = computer.transform.position - avg;
            for (int i = 0; i < selectedPoints.Count; i++)
            {
               points[selectedPoints[i]].SetPosition(points[selectedPoints[i]].position + delta);
            }
        }

        public void MoveTransformToSelection()
        {
            Vector3 avg = Vector3.zero;
            for (int i = 0; i < selectedPoints.Count; i++)
            {
                avg += points[selectedPoints[i]].position;
            }
            avg /= selectedPoints.Count;
            computer.transform.position = avg;
        }

        public void FlatSelection(int axis)
        {
            Undo.RecordObject(computer, "Flat points");
            EditorUtility.SetDirty(computer);
            Vector3 avg = Vector3.zero;
            bool flatTangent = false;
            bool flatPosition = true;
            if (computer.type == Spline.Type.Bezier)
            {
                switch(EditorUtility.DisplayDialogComplex("Flat Bezier", "How do you want to flat the selected Bezier points?", "Points Only", "Tangens Only", "Everything"))
                {
                    case 0: flatTangent = false; flatPosition = true; break;
                    case 1: flatTangent = true; flatPosition = false; break;
                    case 2: flatTangent = true; flatPosition = true; break;
                }
            }
            if (flatPosition)
            {
                for (int i = 0; i < selectedPoints.Count; i++)
                {
                    avg += points[selectedPoints[i]].position;
                }
                avg /= selectedPoints.Count;
                for (int i = 0; i < selectedPoints.Count; i++)
                {
                    Vector3 pos = points[selectedPoints[i]].position;
                    switch (axis)
                    {
                        case 0: pos.x = avg.x; points[selectedPoints[i]].normal.x = 0f; break;
                        case 1: pos.y = avg.y; points[selectedPoints[i]].normal.y = 0f; break;
                        case 2: pos.z = avg.z; points[selectedPoints[i]].normal.z = 0f; break;
                    }
                    points[selectedPoints[i]].normal.Normalize();
                    if (points[selectedPoints[i]].normal == Vector3.zero) points[selectedPoints[i]].normal = Vector3.up;
                    points[selectedPoints[i]].SetPosition(pos);
                    if (flatTangent)
                    {
                        Vector3 tan = points[selectedPoints[i]].tangent;
                        Vector3 tan2 = points[selectedPoints[i]].tangent2;
                        switch (axis)
                        {
                            case 0: tan.x = avg.x; tan2.x = avg.x; break;
                            case 1: tan.y = avg.y; tan2.y = avg.y; break;
                            case 2: tan.z = avg.z; tan2.z = avg.z; break;
                        }
                        points[selectedPoints[i]].SetTangentPosition(tan);
                        points[selectedPoints[i]].SetTangent2Position(tan2);
                    }
                }
            } else
            {
                for (int i = 0; i < selectedPoints.Count; i++)
                {
                    Vector3 tan = points[selectedPoints[i]].tangent;
                    Vector3 tan2 = points[selectedPoints[i]].tangent2;
                    Vector3 pos = points[selectedPoints[i]].position;
                    switch (axis)
                    {
                        case 0: tan.x = pos.x; tan2.x = pos.x; break;
                        case 1: tan.y = pos.y; tan2.y = pos.y; break;
                        case 2: tan.z = pos.z; tan2.z = pos.z; break;
                    }
                    points[selectedPoints[i]].SetTangentPosition(tan);
                    points[selectedPoints[i]].SetTangent2Position(tan2);
                }
            }
        }

        public void MirrorSelection(int axis)
        {
            bool mirrorTangents = false;
            if (computer.type == Spline.Type.Bezier)
            {
                if (EditorUtility.DisplayDialog("Mirror tangents", "Do you want to mirror the tangents too ?", "Yes", "No")) mirrorTangents = true;
            }
            float min = 0f, max = 0f;
            switch (axis)
            {
                case 0: min = max = points[selectedPoints[0]].position.x; break;
                case 1: min = max = points[selectedPoints[0]].position.y; break;
                case 2: min = max = points[selectedPoints[0]].position.z; break;
            }
            if (mirrorTangents)
            {
                float value = 0f;
                switch (axis)
                {
                    case 0: value = points[selectedPoints[0]].tangent.x; break;
                    case 1: value = points[selectedPoints[0]].tangent.y; break;
                    case 2: value = points[selectedPoints[0]].tangent.z; break;
                }
                if (value < min) min = value;
                if (value > max) max = value;
                switch (axis)
                {
                    case 0: value = points[selectedPoints[0]].tangent2.x; break;
                    case 1: value = points[selectedPoints[0]].tangent2.y; break;
                    case 2: value = points[selectedPoints[0]].tangent2.z; break;
                }
                if (value < min) min = value;
                if (value > max) max = value;
            }
            for (int i = 1; i < selectedPoints.Count; i++)
            {
                float value = 0f;
                switch (axis)
                {
                    case 0: value = points[selectedPoints[i]].position.x; break;
                    case 1: value = points[selectedPoints[i]].position.y; break;
                    case 2: value = points[selectedPoints[i]].position.z; break;
                }
                if (value < min) min = value;
                if (value > max) max = value;
                if (mirrorTangents)
                {
                    switch (axis)
                    {
                        case 0: value = points[selectedPoints[i]].tangent.x; break;
                        case 1: value = points[selectedPoints[i]].tangent.y; break;
                        case 2: value = points[selectedPoints[i]].tangent.z; break;
                    }
                    if (value < min) min = value;
                    if (value > max) max = value;
                    switch (axis)
                    {
                        case 0: value = points[selectedPoints[i]].tangent2.x; break;
                        case 1: value = points[selectedPoints[i]].tangent2.y; break;
                        case 2: value = points[selectedPoints[i]].tangent2.z; break;
                    }
                    if (value < min) min = value;
                    if (value > max) max = value;
                }
            }

            for (int i = 0; i < selectedPoints.Count; i++)
            {
                float value = 0f;
                if (mirrorTangents)
                {
                    //Point position
                    switch (axis)
                    {
                        case 0: value = points[selectedPoints[i]].position.x; break;
                        case 1: value = points[selectedPoints[i]].position.y; break;
                        case 2: value = points[selectedPoints[i]].position.z; break;
                    }
                    float percent = Mathf.InverseLerp(min, max, value);
                    value = Mathf.Lerp(max, min, percent);
                    switch (axis)
                    {
                        case 0: points[selectedPoints[i]].position.x = value; break;
                        case 1: points[selectedPoints[i]].position.y = value; break;
                        case 2: points[selectedPoints[i]].position.z = value; break;
                    }
                    //Tangent 1
                    switch (axis)
                    {
                        case 0: value = points[selectedPoints[i]].tangent.x; break;
                        case 1: value = points[selectedPoints[i]].tangent.y; break;
                        case 2: value = points[selectedPoints[i]].tangent.z; break;
                    }
                    percent = Mathf.InverseLerp(min, max, value);
                    value = Mathf.Lerp(max, min, percent);
                    switch (axis)
                    {
                        case 0: points[selectedPoints[i]].tangent.x = value; break;
                        case 1: points[selectedPoints[i]].tangent.y = value; break;
                        case 2: points[selectedPoints[i]].tangent.z = value; break;
                    }
                    //Tangent 2
                    switch (axis)
                    {
                        case 0: value = points[selectedPoints[i]].tangent2.x; break;
                        case 1: value = points[selectedPoints[i]].tangent2.y; break;
                        case 2: value = points[selectedPoints[i]].tangent2.z; break;
                    }
                    percent = Mathf.InverseLerp(min, max, value);
                    value = Mathf.Lerp(max, min, percent);
                    switch (axis)
                    {
                        case 0: points[selectedPoints[i]].tangent2.x = value; break;
                        case 1: points[selectedPoints[i]].tangent2.y = value; break;
                        case 2: points[selectedPoints[i]].tangent2.z = value; break;
                    }
                }
                else
                {
                    Vector3 pos = points[selectedPoints[i]].position;
                    switch (axis)
                    {
                        case 0: value = pos.x; break;
                        case 1: value = pos.y; break;
                        case 2: value = pos.z; break;
                    }
                    float percent = Mathf.InverseLerp(min, max, value);
                    value = Mathf.Lerp(max, min, percent);
                    switch (axis)
                    {
                        case 0: pos.x = value; break;
                        case 1: pos.y = value; break;
                        case 2: pos.z = value; break;
                    }
                    points[selectedPoints[i]].SetPosition(pos);
                }
                //Normal
                switch (axis)
                {
                    case 0: points[selectedPoints[i]].normal.x *= -1f; break;
                    case 1: points[selectedPoints[i]].normal.y *= -1f; break;
                    case 2: points[selectedPoints[i]].normal.z *= -1f; break;
                }
                points[selectedPoints[i]].normal.Normalize();
            }
        }

        public void DistributeEvenly()
        {
            if (selectedPoints.Count < 3) return;
            Undo.RecordObject(computer, "Distribute points evenly");
            EditorUtility.SetDirty(computer);
            float avgDistance = 0f;
            List<int> tempSelected = new List<int>(selectedPoints.ToArray());
            if (computer.isClosed && IsPointSelected(0)) tempSelected.Add(points.Length - 1);
            Vector3[] directions = new Vector3[tempSelected.Count - 1];
            for (int i = 1; i < tempSelected.Count; i++)
            {
                Vector3 direction = points[tempSelected[i]].position - points[tempSelected[i - 1]].position;
                avgDistance += direction.magnitude;
                directions[i - 1] = direction.normalized;
            }
            avgDistance /= directions.Length;
            for (int i = 1; i < tempSelected.Count-1; i++)
            {
                points[tempSelected[i]].SetPosition(points[tempSelected[i - 1]].position + directions[i - 1] * avgDistance);
            }

            if (computer.type != Spline.Type.Bezier)
            {
                UpdateComputer();
                return;
            }
            if (!EditorUtility.DisplayDialog("Distribute tangents", "Do you want to distribute the tangents too ?", "Yes", "No")) return;
            avgDistance = 0f;
            directions = new Vector3[tempSelected.Count*2];
            int dirIndex = 0;
            for (int i = 0; i < tempSelected.Count; i++)
            {
                avgDistance += Vector3.Distance(points[tempSelected[i]].tangent, points[tempSelected[i]].position);
                avgDistance += Vector3.Distance(points[tempSelected[i]].tangent2, points[tempSelected[i]].position);
                directions[dirIndex] = Vector3.Normalize(points[tempSelected[i]].tangent - points[tempSelected[i]].position);
                dirIndex++;
                directions[dirIndex] = Vector3.Normalize(points[tempSelected[i]].tangent2 - points[tempSelected[i]].position);
                dirIndex++;
            }
            avgDistance /= directions.Length;
            for (int i = 0; i < tempSelected.Count; i++)
            {
                points[tempSelected[i]].SetTangentPosition(points[tempSelected[i]].position + directions[i * 2]*avgDistance);
                points[tempSelected[i]].SetTangent2Position(points[tempSelected[i]].position + directions[i * 2+1]*avgDistance);
            }
            UpdateComputer();
        }

        public void RelaxSelected(int iterations)
        {
            Undo.RecordObject(computer, "Relax points");
            for (int i = 0; i < iterations; i++) RelaxIteration();
            EditorUtility.SetDirty(computer);
        }

        void RelaxIteration()
        {
            float totalLength = computer.CalculateLength();
            float travelStep = totalLength / (computer.pointCount - 1);
            for (int i = 0; i < selectedPoints.Count; i++) points[selectedPoints[i]].SetPosition(computer.EvaluatePosition(computer.Travel(0.0, selectedPoints[i] * travelStep, Spline.Direction.Forward)));
            UpdateComputer();
        }

        public void EnterMirrorMode()
        {
            ExitMergeMode();
            ExitSplitMode();
            tool = PointTool.None;
            mirrorMode = true;
            mirrorEditor.Reset();
            closedOnMirror = computer.isClosed;
            mirrorEditor.SceneEdit(ref points, ref selectedPoints);
            Repaint();
            SceneView.RepaintAll();
        }

        public void ExitMirrorMode()
        {
            if (!mirrorMode) return;
            mirrorMode = false;
            mirrorEditor.Revert(ref points);
            UpdateComputer();
        }

        public bool InMirrorMode()
        {
            return mirrorMode;
        }

        public void SaveMirror()
        {
            mirrorMode = false;
        }

        public void EnterMergeMode()
        {
            ExitMirrorMode();
            ExitSplitMode();
            mergeMode = true;
            mergeEditor.Init();
        }

        public void ExitMergeMode()
        {
            mergeMode = false;
        }

        public bool InMergeMode()
        {
            return mergeMode;
        }

        public void EnterSplitMode()
        {
            if (InMergeMode()) ExitMergeMode();
            if (InMirrorMode()) ExitMirrorMode();
            splitMode = true;
            splitEditor.Init();
        }

        public void ExitSplitMode()
        {
            splitMode = false;
        }

        public bool InSplitMode()
        {
            return splitMode;
        }

        private void EditPoints()
        {
            if (hold) return;
            bool change = false;

            if (mirrorMode)
            {
                if (mirrorEditor.SceneEdit(ref points, ref selectedPoints)) change = true;
            } else if (mergeMode) {
                if (mergeEditor.SceneEdit(ref points, ref selectedPoints))
                {
                    change = true;
                    UpdateComputer();
                    Refresh();
                }
            } else if (splitMode)
            {
                if (splitEditor.SceneEdit(ref points, ref selectedPoints))
                {
                    change = true;
                    UpdateComputer();
                    Refresh();
                }
                if (change) emptyClick = false;
                return;
            }
            else Undo.RecordObject(computer, "Edit Points");
            if (computer.pointCount == 0) return;

            defaultEditor.deleteMode = tool == PointTool.Delete;
            defaultEditor.additive = control;
            defaultEditor.shift = shift;
            defaultEditor.click = mouseLeftDown;
            defaultEditor.focused = isFocused;
            //defaultEditor.excludeSelected = tool == PointTool.Move;
            if (defaultEditor.SceneEdit(ref points, ref selectedPoints))
            {
                if(tool == PointTool.Delete && HasSelection())  DeleteSelectedPoints();
                change = true;
                refreshEditors = true;
                Repaint();
            }

            if (refreshEditors)
            {
                RefreshPointEditors();
                refreshEditors = false;
            }

            if (tool == PointTool.Scale)
            {
                if (change) RefreshPointEditors();
                if (scaleEditor.SceneEdit(ref points, ref selectedPoints)) change = true;
            }
            else if (tool == PointTool.Rotate)
            {
                if (change) RefreshPointEditors();
                if (rotationEditor.SceneEdit(ref points, ref selectedPoints)) change = true;
            } else if(tool == PointTool.Move)
            {
                if (positionEditor.SceneEdit(ref points, ref selectedPoints)) change = true;
            }
            else if (tool == PointTool.NormalEdit)
            {
                if (normalEditor.SceneEdit(ref points, ref selectedPoints)) change = true;
            }
            if (change)
            {
                emptyClick = false;
                defaultEditor.CancelDrag();
            }

            if (emptyClick)
            {
                if (tool != PointTool.Create && Tools.current == Tool.None && isFocused)
                {
                    if (mouseLeft && !defaultEditor.isDragging && Vector2.Distance(lastClickPoint, Event.current.mousePosition) >= defaultEditor.minimumRectSize && !alt) defaultEditor.StartDrag(lastClickPoint);
                }
            }
            if (alt && defaultEditor.isDragging) defaultEditor.CancelDrag();

            if (mouseLeftUp)
            {
                if (HasSelection() && !defaultEditor.isDragging)
                {
                    if(emptyClick) ClearSelection();
                    defaultEditor.CancelDrag();
                    emptyClick = false;
                }
                
                if (emptyClick)
                {
                    if (defaultEditor.isDragging) {
                        defaultEditor.FinishDrag();
                        refreshEditors = true;
                        Repaint();
                        emptyClick = false;
                    }
                } else defaultEditor.CancelDrag();

                if (tool == PointTool.Rotate || tool == PointTool.Scale || tool == PointTool.Move) refreshEditors = true;
            }
        }

        public void SetSelectedNormals()
        {
            normalEditor.SetNormals(ref points, ref selectedPoints);
        }

        private void DrawWireSphere(Vector3 position, float radius)
        {
            Handles.DrawWireDisc(position, Vector3.up, radius / 2f);
            Handles.DrawWireDisc(position, editorCamera.transform.position - position, radius / 2f);
            Handles.DrawWireDisc(position, Vector3.forward, radius / 2f);
        }

        private void DeleteSelectedPoints()
        {
            List<SplineComputer.NodeLink> linksFound = new List<SplineComputer.NodeLink>();
            for (int i = 0; i < computer.nodeLinks.Length; i++)
            {
                if (computer.nodeLinks[i].node.GetConnections().Length == 1 && selectedPoints.Contains(computer.nodeLinks[i].pointIndex))
                {
                    if (computer.nodeLinks[i].node != null) linksFound.Add(computer.nodeLinks[i]);
                }
            }
            if (linksFound.Count > 0)
            {
                string nodeString = GetSelectedPointNodeNamesString(80);
                string message = "The following nodes are only connected to the currently selected points: \r\n" + nodeString + "\r\n would you like to delete them?";
                if (EditorUtility.DisplayDialog("Remove nodes?", message, "Yes", "No"))
                {
                    for (int i = 0; i < linksFound.Count; i++)
                    {
                        linksFound[i].node.RemoveConnection(computer, linksFound[i].pointIndex);
                        DestroyImmediate(linksFound[i].node.gameObject);
                    }
                }
            }
            if (computer.isClosed && selectedPoints.Count == points.Length - 1)
            {
                for (int i = points.Length - 1; i >= 0; i--) DeletePoint(i);
            }
            else
            {
                for (int i = 0; i < selectedPoints.Count; i++)
                {
                    DeletePoint(selectedPoints[i]);
                    for (int n = i; n < selectedPoints.Count; n++) selectedPoints[n]--;
                }
            }
            ClearSelection();
        }

        private void DeletePoint(int index)
        {
            if (computer.hasMorph)
            {
                Debug.Log("Cannot delete points when there are morphs");
                return;
            }
            Undo.RecordObject(computer, "Delete Point");
            EditorUtility.SetDirty(computer);
            SplinePoint[] newPoints = new SplinePoint[points.Length - 1];
            for (int i = 0; i < newPoints.Length; i++)
            {
                if (i < index) newPoints[i] = points[i];
                else newPoints[i] = points[i + 1];
            }
            List<Node> nodes = new List<Node>();
            List<int> indices = new List<int>();
            for (int i = computer.nodeLinks.Length - 1; i >= 0; i--)
            {
                if (computer.nodeLinks[i].pointIndex >= index)
                {
                    nodes.Add(computer.nodeLinks[i].node);
                    indices.Add(computer.nodeLinks[i].pointIndex);
                }
            }
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null) continue;
                Undo.RecordObject(nodes[i], "Create Point");
                EditorUtility.SetDirty(nodes[i]);
                nodes[i].RemoveConnection(computer, indices[i]);
            }
            points = newPoints;
            UpdateComputer();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null) continue;
                nodes[i].AddConnection(computer, indices[i] - 1);
            }
        }

        void DrawGrid(Vector3 center, Vector3 normal, Vector2 size, float scale)
        {
            Vector3 right = Vector3.Cross(Vector3.up, normal).normalized;
            if (Mathf.Abs(Vector3.Dot(Vector3.up, normal)) >= 0.9999f) right = Vector3.Cross(Vector3.forward, normal).normalized;
            Vector3 up = Vector3.Cross(normal, right).normalized;
            Vector3 startPoint = center - right * size.x * 0.5f + up * size.y * 0.5f;
            float i = 0f;
            float add = scale;
            while (i <= size.x)
            {
                Vector3 point = startPoint + right * i;
                Handles.DrawLine(point, point - up * size.y);
                i += add;
            }

            i = 0f;
            add = scale;
            while (i <= size.x)
            {
                Vector3 point = startPoint - up * i;
                Handles.DrawLine(point, point + right * size.x);
                i += add;
            }
        }

        public void ToggleCreateTool()
        {
            if (tool == PointTool.Create)
            {
                tool = PointTool.None;
                createPointVisualizer = null;
            }
            else
            {
                createPointVisualizer = new Spline(computer.type, computer.precision);
                SetVisualizerPoints(Vector3.zero);
                if (computer.isClosed) createPointVisualizer.Close();
                tool = PointTool.Create;
            }
        }

        void ToggleTool(PointTool targetTool, Tool editorAnalogue)
        {
            if (tool != targetTool && Tools.current == Tool.None)
            {
                if (tool == PointTool.Create)
                {
                    if (appendMode == 0) SelectPoint(points.Length - 1);
                    else SelectPoint(0);
                }
                tool = targetTool;
                Tools.current = Tool.None;
            }
            else if (tool == targetTool || (Tools.current != Tool.None && Tools.current != editorAnalogue) || (!HasSelection() && Tools.current != editorAnalogue))
            {
                tool = PointTool.None;
                lastTool = Tools.current = editorAnalogue;
            }
            else
            {
                tool = PointTool.None;
                Tools.current = Tool.None;
            }
        }

        public void ToggleMoveTool()
        {
            ToggleTool(PointTool.Move, Tool.Move);
        }

       public void ToggleRotateTool()
        {
            ToggleTool(PointTool.Rotate, Tool.Rotate);
            RefreshPointEditors();
        }

        public void ToggleScaleTool()
        {
            ToggleTool(PointTool.Scale, Tool.Scale);
            RefreshPointEditors();
        }

        Vector3 CalculatePointNormal(int index)
        {
            if (points.Length < 3)
            {
                Debug.Log("Spline needs to have at least 3 control points in order to calculate normals");
                return Vector3.zero;
            }
            Vector3 side1 = Vector3.zero;
            Vector3 side2 = Vector3.zero;
            if (index == 0)
            {
                if (computer.isClosed)
                {
                    side1 = points[index].position - points[index + 1].position;
                    side2 = points[index].position - points[points.Length - 2].position;
                }
                else
                {
                    side1 = points[0].position - points[1].position;
                    side2 = points[0].position - points[2].position;
                }
            }
            else if (index == points.Length - 1)
            {
                side1 = points[points.Length - 1].position - points[points.Length - 3].position;
                side2 = points[points.Length - 1].position - points[points.Length - 2].position;
            }
            else
            {
                side1 = points[index].position - points[index + 1].position;
                side2 = points[index].position - points[index - 1].position;
            }
            return Vector3.Cross(side1.normalized, side2.normalized).normalized;
        }

        void GetInput()
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            Event e = Event.current;
            mouseLeftDown = mouseLeftUp = mouseRightDown = mouserightUp = false;
            control = e.control;
            shift = e.shift;
            alt = e.alt;
            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        mouseLeftDown = true;
                        mouseLeft = true;
                        emptyClick = !mouseHoversToolbar;
                        lastClickPoint = e.mousePosition;
                    }
                    if (e.button == 1) mouseRightDown = mouseRight = true;
                    break;
                case EventType.MouseUp:
                    if (e.button == 0)
                    {
                        mouseLeftUp = true;
                        mouseLeft = false;
                    }
                    if (e.button == 1)
                    {
                        mouseRightDown = true;
                        mouseRight = false;
                    }
                    break;
                case EventType.Layout:
                    HandleUtility.AddDefaultControl(controlID); 
                    break;
            }
            if(mouseLeft && alt) emptyClick = false;

            if (movePivot)
            {
                SceneView.lastActiveSceneView.pivot = Vector3.Lerp(SceneView.lastActiveSceneView.pivot, idealPivot, 0.02f);
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) movePivot = false;
                if (Vector3.Distance(SceneView.lastActiveSceneView.pivot, idealPivot) <= 0.05f)
                {
                    SceneView.lastActiveSceneView.pivot = idealPivot;
                    movePivot = false;
                }
            }
            if (!mouseRight && !mouseLeft)
            {
                if (e.type == EventType.KeyDown && e.keyCode == KeyCode.R)
                {
                    ToggleScaleTool();
                    e.Use();
                }
                if (e.type == EventType.KeyDown && e.keyCode == KeyCode.E)
                {
                    ToggleRotateTool();
                    e.Use();
                }
                if (e.type == EventType.KeyDown && e.keyCode == KeyCode.W)
                {
                    ToggleMoveTool();
                    e.Use();
                }
                if (e.type != EventType.Layout && e.commandName == "FrameSelected" && Tools.current == Tool.None)
                {
                    if (points.Length > 0)
                    {
                        e.commandName = "";
                        FramePoints();
                        e.Use();
                    }
                }
            }
        }

    }
}
#endif
