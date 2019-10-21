using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    public class MorphWindow : SplineEditorWindow
    {
        public static bool editShapeMode = false;
        private int currentMorph = -1;
        private string addName = "";
        private Vector2 scroll;


        protected override void OnInitialize()
        {
            addName = "Channel " + splineEditor.computer.morph.GetChannelCount();
            editShapeMode = false;
        }

        protected override string GetTitle()
        {
            return "Morph States";
        }

        void OnDestroy()
        {
            editShapeMode = false;
        }

        void RenderMorph(int index, string name, SplineComputer.Morph morph)
        {
            SplineComputer computer = ((SplineComputer)editor.target);
            if (currentMorph == index) GUI.color = Color.green;
            GUILayout.Box("", GUILayout.Height(50), GUILayout.Width(position.width-20));
            GUI.color = Color.white;
            Rect boxRect = GUILayoutUtility.GetLastRect();
            GUI.BeginGroup(boxRect);
            GUI.Label(new Rect(10, 5, boxRect.width - 40, 20), name);
            if (editShapeMode && currentMorph == index)
            {
                if (GUI.Button(new Rect(boxRect.width - 55, 5, 50, 20), "Cancel"))
                {
                    currentMorph = -1;
                    editShapeMode = false;
                    morph.SetWeight(index, morph.GetWeight(index));
                }
            }
            else
            {
                if (GUI.Button(new Rect(boxRect.width - 25, 5, 20, 20), "x"))
                {
                    if (EditorUtility.DisplayDialog("Remove morph state", "Do you want to remove this morph state?", "Yes", "No"))
                    {
                        morph.SetWeight(index, 0f);
                        morph.RemoveChannel(index);
                        GUI.EndGroup();
                        return;
                    }
                }
            }
            if (currentMorph == index)
            {
                if (GUI.Button(new Rect(10, 25, boxRect.width / 3f, 20), "Save Shape"))
                {
                    currentMorph = -1;
                    morph.CaptureSnapshot(index);
                    editShapeMode = false;
                }
                GUI.Label(new Rect(20 + boxRect.width / 3f, 30, boxRect.width / 1.5f, 30), "Editing shape (weight is 100%)");
                GUI.EndGroup();
                return;
            }
            if (GUI.Button(new Rect(10, 25, boxRect.width/3f, 20), "Edit Shape"))
            {
                currentMorph = index;
                editShapeMode = true;
                computer.SetPoints(morph.GetSnapshot(index), SplineComputer.Space.Local);
                computer.Rebuild(); 
            }
            if (index == 0)
            {
                GUI.EndGroup();
                return;
            }
            GUI.Label(new Rect(20 + boxRect.width / 3f, 20, 50, 30), "Weight:");
            float weight = morph.GetWeight(index);
            float lastWeight = weight;
            weight = GUI.HorizontalSlider(new Rect(20 + boxRect.width / 3f, 30, boxRect.width / 2.5f, 30), weight, 0f, 1f);
            string strWeight = weight.ToString();
            strWeight = GUI.TextField(new Rect(30 + boxRect.width / 3f + boxRect.width / 2.5f, 30, boxRect.width/8, 16), strWeight);
            float.TryParse(strWeight, out weight);
            if (weight != lastWeight)
            {
                morph.SetWeight(index, weight);
            }
            GUI.EndGroup();
        }

        void OnGUI()
        {
            if (editor == null) {
                Close();
                return;
            }
            SplineComputer computer = ((SplineComputer)editor.target);
            if (computer == null)
            {
                Close();
                return;
            }
            Undo.RecordObject(computer, "Edit Morph State");
            string[] names = computer.morph.GetChannelNames();
            if (names.Length > 0)
            {
                if (computer.morph.GetSnapshot(0).Length != computer.pointCount && (currentMorph < 0 || currentMorph > computer.morph.GetChannelCount()))
                {
                    EditorGUILayout.HelpBox("Recorded morphs require the spline to have " + computer.morph.GetSnapshot(0).Length + ". The spline has " + computer.pointCount, MessageType.Error);
                    EditorGUILayout.BeginHorizontal();
                    if(GUILayout.Button("Clear morph states"))
                    {
                        if (EditorUtility.DisplayDialog("Clear morph states?", "Do you want to clear all morph states?", "Yes", "No"))
                        {
                            computer.morph.Clear();
                        }
                    }
                    string str = "Reduce";
                    if (computer.morph.GetSnapshot(0).Length > computer.pointCount) str = "Increase";
                    if (GUILayout.Button(str + " spline points"))
                    {
                        if (EditorUtility.DisplayDialog(str + " spline points?", "Do you want to " + str + " the spline points?", "Yes", "No"))
                        {
                            computer.SetPoints(computer.morph.GetSnapshot(0), SplineComputer.Space.Local);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    return;
                }
            }

            Rect viewRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth - 30, 54 * names.Length);
            scroll = GUI.BeginScrollView(new Rect(0, 0, EditorGUIUtility.currentViewWidth, Mathf.Min(73 * names.Length, 54 * 4)), scroll, viewRect);
            for (int i = 0; i < names.Length; i++)
            {
                RenderMorph(i, names[i], computer.morph);
                names = computer.morph.GetChannelNames();
            }
            GUI.EndScrollView();
            Rect rect = new Rect(0, Mathf.Min(73 * names.Length, 54 * 4) + 30, EditorGUIUtility.currentViewWidth, 100);
            GUILayout.BeginArea(rect);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            addName = EditorGUILayout.TextField("Channel Name ", addName);
            if (GUILayout.Button("Add"))
            {
                computer.morph.AddChannel(addName);
                addName = "Channel " + computer.morph.GetChannelCount();
            }
            GUILayout.EndArea();
            if (GUI.changed) SceneView.RepaintAll();
        }
    }
}
