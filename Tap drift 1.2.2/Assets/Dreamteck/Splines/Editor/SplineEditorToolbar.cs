using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Dreamteck.Splines
{
    public class LayermaskWindow : SplineEditorWindow
    {
        protected override string GetTitle()
        {
            return "Layermask";
        }

        private void OnGUI()
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & splineEditor.surfaceLayerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = EditorGUILayout.MaskField(maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            splineEditor.surfaceLayerMask.value = mask;
        }
    }

    public class RelaxWindow : SplineEditorWindow
    {
        public int iterations = 1;
        protected override string GetTitle()
        {
            return "Relax";
        }

        private void OnGUI()
        {
            if (splineEditor == null || splineEditor.selectedPointsCount == 0)
            {
                Close();
                return;
            }
            EditorGUIUtility.labelWidth = 60;
            iterations = EditorGUILayout.IntSlider("Iterations", iterations, 1, 10);
            EditorGUIUtility.labelWidth = 0;
            if (GUILayout.Button("Relax"))
            {
                splineEditor.RelaxSelected(iterations);
                Close();
            }
        }
    }


    public partial class SplineEditor
    {
        private bool mouseHoversToolbar = false;
        private int minWidth = 800;
        private float scale = 1f;
        private PresetsWindow presetWindow = null;
        public LayerMask surfaceLayerMask = new LayerMask();
        private int editSpace = 0;

        private string[] operations = new string[] {"Operations", "Center to Transform", "Move Transform to", "Flat X", "Flat Y", "Flat Z", "Mirror X", "Mirror Y", "Mirror Z", "Distribute evenly", "Relax" };
        private string[] editSpaceText = new string[] { "World", "Transform", "Spline" };


        private static GUIContent presetButtonContent = new GUIContent("P", "Open Primitives and Presets");
        private static GUIContent moveButtonContent = new GUIContent("M", "Move points");
        private static GUIContent rotateButtonContent = new GUIContent("R", "Rotate points");
        private static GUIContent scaleButtonContent = new GUIContent("S", "Scale points");
        private static GUIContent normalsButtonContent = new GUIContent("N", "Edit point normals");
        private static GUIContent mirrorButtonContent = new GUIContent("||", "Symmetry editor");
        private static GUIContent mergeButtonContent = new GUIContent("><", "Merge Splines");
        private static GUIContent splitButtonContent = new GUIContent("-/-", "Split Spline");
        private static GUIContent addButtonContent = new GUIContent("+", "Enter point creation mode");
        private static GUIContent removeButtonContent = new GUIContent("-", "Enter point removal mode");

        public void DisableToolbar()
        {
            if (presetWindow != null) presetWindow.Close();
        }

        public void EnableToolbar()
        {
            Texture2D tex = ImageDB.GetImage("presets.png", "Splines/Editor/Icons");
            if (tex != null) { presetButtonContent.image = tex; presetButtonContent.text = ""; }
            tex = ImageDB.GetImage("move.png", "Splines/Editor/Icons");
            if (tex != null) { moveButtonContent.image = tex; moveButtonContent.text = ""; }
            tex = ImageDB.GetImage("rotate.png", "Splines/Editor/Icons");
            if (tex != null) { rotateButtonContent.image = tex; rotateButtonContent.text = ""; }
            tex = ImageDB.GetImage("scale.png", "Splines/Editor/Icons");
            if (tex != null) { scaleButtonContent.image = tex; scaleButtonContent.text = ""; }
            tex = ImageDB.GetImage("normals.png", "Splines/Editor/Icons");
            if (tex != null) { normalsButtonContent.image = tex; normalsButtonContent.text = ""; }
            tex = ImageDB.GetImage("mirror.png", "Splines/Editor/Icons");
            if (tex != null) { mirrorButtonContent.image = tex; mirrorButtonContent.text = ""; }
            tex = ImageDB.GetImage("merge.png", "Splines/Editor/Icons");
            if (tex != null) { mergeButtonContent.image = tex; mergeButtonContent.text = ""; }
            tex = ImageDB.GetImage("split.png", "Splines/Editor/Icons");
            if (tex != null) { splitButtonContent.image = tex; splitButtonContent.text = ""; }
        }

        public void DrawToolbar()
        { 
            if (Screen.width < minWidth) scale = (float)Screen.width/minWidth;
            else scale = 1f;
            SplineEditorGUI.SetScale(scale);
            DreamteckEditorGUI.SetScale(scale);
            mouseHoversToolbar = false;
            minWidth = 770;
            if (InMirrorMode()) Mirror(Mathf.RoundToInt(44 * scale));
            else if(InMergeMode()) Merge(Mathf.RoundToInt(44 * scale));
            else
            {
                switch (tool)
                {
                    case PointTool.Create:
                        if (computer.is2D) Create2DToolbar(Mathf.RoundToInt(44 * scale));
                        else CreateToolbar(Mathf.RoundToInt(44 * scale));
                        break;
                    case PointTool.NormalEdit: Normals(Mathf.RoundToInt(44 * scale)); break;
                    case PointTool.Move: Translate(Mathf.RoundToInt(44 * scale)); break;
                    case PointTool.Scale: Scale(Mathf.RoundToInt(44 * scale)); break;
                    case PointTool.Rotate: Rotate(Mathf.RoundToInt(44 * scale)); break;
                }
            }
            Main();
        }
         
        private void Main() {
            Rect barRect = new Rect(0f, 0f, Screen.width, 45*scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;

            if (computer.hasMorph && !MorphWindow.editShapeMode)
            {
                DreamteckEditorGUI.Label(new Rect(Screen.width / 2f - 200f, 10, 400, 30), "Editing unavailable outside of morph states.");
                return;
            }
            if (computer.hasMorph)
            {
                if (tool == PointTool.Create || tool == PointTool.Delete) tool = PointTool.None;
            }
            else
            {
                if (SplineEditorGUI.BigButton(new Rect(5 * scale, 5 * scale, 35 * scale, 35 * scale), addButtonContent, true, tool == PointTool.Create)) ToggleCreateTool();
                if (SplineEditorGUI.BigButton(new Rect(45 * scale, 5 * scale, 35 * scale, 35 * scale), removeButtonContent, computer.pointCount > 0, tool == PointTool.Delete))
                {
                    if (tool != PointTool.Delete) tool = PointTool.Delete;
                    else tool = PointTool.None;
                }
                if (SplineEditorGUI.BigButton(new Rect(85 * scale, 5 * scale, 35 * scale, 35 * scale), presetButtonContent, true, presetWindow != null))
                {
                    if (presetWindow == null)
                    {
                        presetWindow = EditorWindow.GetWindow<PresetsWindow>(true);
                        presetWindow.Init(this, Vector2.zero, new Vector2(350, 20 * 22));
                    }
                }
            }

            if (SplineEditorGUI.BigButton(new Rect(150 * scale, 5 * scale, 35 * scale, 35 * scale), moveButtonContent, true, tool == PointTool.Move)) {
                if (tool != PointTool.Move) ToggleMoveTool();
                else tool = PointTool.None;
            }
            if (SplineEditorGUI.BigButton(new Rect(190 * scale, 5 * scale, 35 * scale, 35 * scale), rotateButtonContent, true, tool == PointTool.Rotate))
            {
                if (tool != PointTool.Rotate) ToggleRotateTool();
                else tool = PointTool.None;
            }
            if (SplineEditorGUI.BigButton(new Rect(230 * scale, 5 * scale, 35 * scale, 35 * scale), scaleButtonContent, true, tool == PointTool.Scale))
            {
                if (tool != PointTool.Scale) ToggleScaleTool();
                else tool = PointTool.None;
            }
            if (SplineEditorGUI.BigButton(new Rect(270 * scale, 5 * scale, 35 * scale, 35 * scale), normalsButtonContent, !computer.is2D, tool == PointTool.NormalEdit))
            {
                if (tool != PointTool.NormalEdit) tool = PointTool.NormalEdit;
                else tool = PointTool.None;
            }
            if (SplineEditorGUI.BigButton(new Rect(330 * scale, 5 * scale, 35 * scale, 35 * scale), mirrorButtonContent, computer.pointCount > 0 || InMirrorMode(), InMirrorMode()))
            {
                if (InMirrorMode()) ExitMirrorMode();
                else EnterMirrorMode();
            }

            if (SplineEditorGUI.BigButton(new Rect(370 * scale, 5 * scale, 35 * scale, 35 * scale), mergeButtonContent, computer.pointCount > 0 && !computer.isClosed, InMergeMode()))
            {
                if (InMergeMode()) ExitMergeMode();
                else EnterMergeMode();
            }

            if (SplineEditorGUI.BigButton(new Rect(410 * scale, 5 * scale, 35 * scale, 35 * scale), splitButtonContent, computer.pointCount > 0 && !computer.isClosed, InSplitMode()))
            {
                if (InSplitMode()) ExitSplitMode();
                else EnterSplitMode();
            }
            int operation = 0;
            float operationsPosition = Screen.width - (190 * scale + 100);
#if UNITY_EDITOR_OSX
            operationsPosition = 430 * scale;
#endif

            bool hover = SplineEditorGUI.DropDown(new Rect(operationsPosition, 10 * scale, 150 * scale, 25 * scale), SplineEditorGUI.defaultButton, operations, HasSelection(), ref operation);
            if (hover) mouseHoversToolbar = true;
            if (selectedPointsCount > 0)
            {
                switch (operation)
                {
                    case 1: CenterSelection(); break;
                    case 2: MoveTransformToSelection(); break;
                    case 3: FlatSelection(0); break;
                    case 4: FlatSelection(1); break;
                    case 5: FlatSelection(2); break;
                    case 6: MirrorSelection(0); break;
                    case 7: MirrorSelection(1); break;
                    case 8: MirrorSelection(2); break;
                    case 9: DistributeEvenly(); break;
                    case 10: EditorWindow.GetWindow<RelaxWindow>(true).Init(this, new Vector2(250, 90)); break;
                }
            }
            GUI.color = SplineEditorGUI.activeColor;
            ((SplineComputer)target).editorPathColor = EditorGUI.ColorField(new Rect(operationsPosition + 160 * scale, 13 * scale, 40 * scale, 20 * scale), ((SplineComputer)target).editorPathColor);
        }

        private void CreateToolbar(int verticalOffset)
        {
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            DreamteckEditorGUI.Label(new Rect(5 * scale, verticalOffset+5 * scale, 105 * scale, 25 * scale), "Place method:", true);
            bool hover = SplineEditorGUI.DropDown(new Rect(110 * scale, verticalOffset+5 * scale, 130 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] { "Camera Plane", "Insert", "Surface", "X Plane", "Y Plane", "Z Plane" }, true, ref createPointMode);
            if (hover) mouseHoversToolbar = true;
            DreamteckEditorGUI.Label(new Rect(220 * scale, verticalOffset+5 * scale, 100 * scale, 25 * scale), "Normal:", true);
            hover = SplineEditorGUI.DropDown(new Rect(320 * scale, verticalOffset+5 * scale, 160 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] { "Auto", "Look at Camera", "Align with Camera", "Calculate", "Left", "Right", "Up", "Down", "Forward", "Back" }, true, ref createNormalMode);
            if (hover) mouseHoversToolbar = true;
            DreamteckEditorGUI.Label(new Rect(800 * scale, verticalOffset + 5 * scale, 90 * scale, 25 * scale), "Add Node", true);
            createNodeOnCreatePoint = GUI.Toggle(new Rect(890 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), createNodeOnCreatePoint, "");

            bool showNormalField = false;

            if (createPointMode >= 3 && createPointMode <= 5)
            {
                DreamteckEditorGUI.Label(new Rect(500 * scale, verticalOffset+5 * scale, 80 * scale, 30 * scale), "Grid offset:", true);
                showNormalField = true;
            } else if(createPointMode == 2){
                DreamteckEditorGUI.Label(new Rect(480 * scale, verticalOffset + 5 * scale, 100 * scale, 30 * scale), "Normal offset:", true);
                showNormalField = true;
                if(SplineEditorGUI.Button(new Rect(925 * scale, verticalOffset * scale + 5, 95 * scale, 25 * scale), "LayerMask"))
                {
                    LayermaskWindow maskWindow = EditorWindow.GetWindow<LayermaskWindow>(true);
                    maskWindow.position = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 10, 10);
                    maskWindow.Init(this, new Vector2(150, 30), new Vector2(150, 30));
                }
            } else if (createPointMode == 0)
            {
                DreamteckEditorGUI.Label(new Rect(500 * scale, verticalOffset + 5 * scale, 80 * scale, 30 * scale), "Far plane:", true);
                showNormalField = true;
            }

            if (showNormalField)
            {
                createPointOffset = SplineEditorGUI.FloatField(new Rect(580 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), createPointOffset);
                createPointOffset = SplineEditorGUI.FloatDrag(new Rect(500 * scale, verticalOffset + 5 * scale, 80 * scale, 25 * scale), createPointOffset);
                if (createPointOffset < 0f && createPointMode < 3) createPointOffset = 0f;
            }

            minWidth = 790;
            if (createPointMode != 1)
            {
                DreamteckEditorGUI.Label(new Rect(625 * scale, verticalOffset + 5 * scale, 80 * scale, 30 * scale), "Append:", true);
                if (SplineEditorGUI.DropDown(new Rect(705 * scale, verticalOffset + 5 * scale, 100 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] { "End", "Beginning"}, true, ref appendMode)) mouseHoversToolbar = true;
                minWidth = 1100;
            }
        }

        private void Create2DToolbar(int verticalOffset)
        {
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            DreamteckEditorGUI.Label(new Rect(5 * scale, verticalOffset + 5 * scale, 105 * scale, 25 * scale), "Place method:", true);
            bool hover = SplineEditorGUI.DropDown(new Rect(110 * scale, verticalOffset + 5 * scale, 130 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] { "Free", "Insert"}, true, ref createPointMode2D);
            if (hover) mouseHoversToolbar = true;
            DreamteckEditorGUI.Label(new Rect(230 * scale, verticalOffset + 5 * scale, 90 * scale, 25 * scale), "Add Node", true);
            createNodeOnCreatePoint = GUI.Toggle(new Rect(320 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), createNodeOnCreatePoint, "");



            minWidth = 790;
            if (createPointMode != 1)
            {
                DreamteckEditorGUI.Label(new Rect(325 * scale, verticalOffset + 5 * scale, 80 * scale, 30 * scale), "Append:", true);
                if (SplineEditorGUI.DropDown(new Rect(405 * scale, verticalOffset + 5 * scale, 100 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] { "End", "Beginning" }, true, ref appendMode)) mouseHoversToolbar = true;
                minWidth = 1100;
            }
        }

        private void Normals(int verticalOffset)
        {
            if (computer.hasMorph && !MorphWindow.editShapeMode) return;
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            if(SplineEditorGUI.Button(new Rect(5 * scale, verticalOffset+5 * scale, 130 * scale, 25 * scale), "Set Normals:")) SetSelectedNormals();
            bool hover = SplineEditorGUI.DropDown(new Rect(160 * scale, verticalOffset+5 * scale, 150 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] {"At Camera", "Align with Camera", "Calculate", "Left", "Right", "Up", "Down", "Forward", "Back", "Inverse", "At Avg. Center", "By Direction"}, true, ref normalEditor.setNormalMode);
            if (hover) mouseHoversToolbar = true;
        }

        private void Translate(int verticalOffset)
        {
            if (computer.hasMorph && !MorphWindow.editShapeMode) return;
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            DreamteckEditorGUI.Label(new Rect(110 * scale, verticalOffset + 5 * scale, 90 * scale, 25 * scale), "Grid snap", true);
            positionEditor.snap = GUI.Toggle(new Rect(200 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), positionEditor.snap, "");

            if (positionEditor.snap)
            {
                DreamteckEditorGUI.Label(new Rect(220 * scale, verticalOffset + 5 * scale, 80 * scale, 30 * scale), "Grid size:", true);
                positionEditor.snapGridSize = SplineEditorGUI.FloatField(new Rect(300 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), positionEditor.snapGridSize);
                positionEditor.snapGridSize = SplineEditorGUI.FloatDrag(new Rect(220 * scale, verticalOffset + 5 * scale, 80 * scale, 25 * scale), positionEditor.snapGridSize);
                if (positionEditor.snapGridSize < 0.001f) positionEditor.snapGridSize = 0.001f;
            }

            EditSpaceMenu(verticalOffset);
        }

        private void Scale(int verticalOffset)
        {
            if (computer.hasMorph && !MorphWindow.editShapeMode) return;
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            EditSpaceMenu(verticalOffset);
            DreamteckEditorGUI.Label(new Rect(120 * scale, verticalOffset + 5 * scale, 90 * scale, 25 * scale), "Scale sizes", true);
            scaleEditor.scaleSize = GUI.Toggle(new Rect(210 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), scaleEditor.scaleSize, "");
            DreamteckEditorGUI.Label(new Rect(235 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Scale tangents", true);
            scaleEditor.scaleTangents = GUI.Toggle(new Rect(355 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), scaleEditor.scaleTangents, "");
        }

        private void Rotate(int verticalOffset)
        {
            if (computer.hasMorph && !MorphWindow.editShapeMode) return;
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            EditSpaceMenu(verticalOffset);

            DreamteckEditorGUI.Label(new Rect(120 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Rotate normals", true);
            rotationEditor.rotateNormals = GUI.Toggle(new Rect(240 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), rotationEditor.rotateNormals, "");

            DreamteckEditorGUI.Label(new Rect(260 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Rotate tangents", true);
            rotationEditor.rotateTangents = GUI.Toggle(new Rect(380 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), rotationEditor.rotateTangents, "");
        }

        private void Mirror(int verticalOffset)
        {
            if (computer.hasMorph && !MorphWindow.editShapeMode) return;
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            if (SplineEditorGUI.Button(new Rect(5 * scale, verticalOffset + 5 * scale, 100 * scale, 25 * scale), "Cancel")) ExitMirrorMode();
            if (SplineEditorGUI.Button(new Rect(115 * scale, verticalOffset + 5 * scale, 100 * scale, 25 * scale), "Save")) SaveMirror();

            DreamteckEditorGUI.Label(new Rect(215 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), "Axis", true);
            int axis = (int)mirrorEditor.axis;
            bool hover = SplineEditorGUI.DropDown(new Rect(270 * scale, verticalOffset + 5 * scale, 60 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] { "X", "Y", "Z"}, true, ref axis);
            mirrorEditor.axis = (SplinePointMirrorEditor.Axis)axis;
            if (hover) mouseHoversToolbar = true;

            DreamteckEditorGUI.Label(new Rect(315 * scale, verticalOffset + 5 * scale, 60 * scale, 25 * scale), "Flip", true);
            mirrorEditor.flip = GUI.Toggle(new Rect(380 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), mirrorEditor.flip, "");

            DreamteckEditorGUI.Label(new Rect(390 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Weld Distance", true);

            mirrorEditor.weldDistance = SplineEditorGUI.FloatField(new Rect(525 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), mirrorEditor.weldDistance);
            mirrorEditor.weldDistance = SplineEditorGUI.FloatDrag(new Rect(390 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), mirrorEditor.weldDistance);
            if (mirrorEditor.weldDistance < 0f) mirrorEditor.weldDistance = 0f;

            DreamteckEditorGUI.Label(new Rect(570 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Center  X:", true);
            mirrorEditor.center.x = SplineEditorGUI.FloatField(new Rect(700 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), mirrorEditor.center.x);

            DreamteckEditorGUI.Label(new Rect(720 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), "Y:", true);
            mirrorEditor.center.y = SplineEditorGUI.FloatField(new Rect(770 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), mirrorEditor.center.y);
            DreamteckEditorGUI.Label(new Rect(790 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), "Z:", true);
            mirrorEditor.center.z = SplineEditorGUI.FloatField(new Rect(840 * scale, verticalOffset + 5 * scale, 50 * scale, 25 * scale), mirrorEditor.center.z);
        }

        private void Merge(int verticalOffset)
        {
            if (computer.hasMorph && !MorphWindow.editShapeMode) return;
            Rect barRect = new Rect(0f, verticalOffset, Screen.width, 35 * scale);
            if (barRect.Contains(Event.current.mousePosition)) mouseHoversToolbar = true;
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(barRect, "", SplineEditorGUI.whiteBox);
            GUI.color = SplineEditorGUI.activeColor;
            DreamteckEditorGUI.Label(new Rect(5 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Merge endpoints", true);
            mergeEditor.mergeEndpoints = GUI.Toggle(new Rect(130 * scale, verticalOffset + 10 * scale, 25 * scale, 25 * scale), mergeEditor.mergeEndpoints, "");
            int mergeSide = (int)mergeEditor.mergeSide;
            DreamteckEditorGUI.Label(new Rect(120 * scale, verticalOffset + 5 * scale, 120 * scale, 25 * scale), "Merge side", true);
            bool hover = SplineEditorGUI.DropDown(new Rect(250, verticalOffset + 5 * scale, 100 * scale, 25 * scale), SplineEditorGUI.defaultButton, new string[] { "Start", "End" }, true, ref mergeSide);
            mergeEditor.mergeSide = (SplineComputerMergeEditor.MergeSide)mergeSide;
            if (hover) mouseHoversToolbar = true;
        }

        private void EditSpaceMenu(int verticalOffset)
        {
            int lastSpace = editSpace;
            bool hover = SplineEditorGUI.DropDown(new Rect(5, verticalOffset + 5 * scale, 115 * scale, 25 * scale), SplineEditorGUI.defaultButton, editSpaceText, true, ref editSpace);
            if (hover) mouseHoversToolbar = true;
            if (lastSpace != editSpace) RefreshPointEditors();
        }
    }
}
