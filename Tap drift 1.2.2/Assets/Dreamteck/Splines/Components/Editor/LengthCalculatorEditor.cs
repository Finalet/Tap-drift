#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(LengthCalculator), true)]
    public class LengthCalculatorEditor : SplineUserEditor
    {
        public override void OnInspectorGUI()
        {
            showAveraging = false;
            base.OnInspectorGUI();
        }

        protected override void BodyGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Length Calculator", EditorStyles.boldLabel);
            base.BodyGUI();
            LengthCalculator calculator = (LengthCalculator)target;
            EditorGUILayout.HelpBox("Length: " + calculator.length, MessageType.Info);

            for (int i = 0; i < calculator.lengthEvents.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                calculator.lengthEvents[i].enabled = EditorGUILayout.Toggle(calculator.lengthEvents[i].enabled);
                calculator.lengthEvents[i].targetLength = EditorGUILayout.FloatField("Target Length", calculator.lengthEvents[i].targetLength);
                calculator.lengthEvents[i].type = (LengthCalculator.LengthEvent.Type)EditorGUILayout.EnumPopup(calculator.lengthEvents[i].type);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                SplineEditorGUI.ActionField(calculator.lengthEvents[i].action);
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    LengthCalculator.LengthEvent[] newEvents = new LengthCalculator.LengthEvent[calculator.lengthEvents.Length - 1];
                    for (int n = 0; n < calculator.lengthEvents.Length; n++)
                    {
                        if (i == n) continue;
                        else if (n < i) newEvents[n] = calculator.lengthEvents[n];
                        else if (n > i) newEvents[n - 1] = calculator.lengthEvents[n];
                    }
                    calculator.lengthEvents = newEvents;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Length Event"))
            {
                LengthCalculator.LengthEvent[] newEvents = new LengthCalculator.LengthEvent[calculator.lengthEvents.Length + 1];
                calculator.lengthEvents.CopyTo(newEvents, 0);
                newEvents[newEvents.Length - 1] = new LengthCalculator.LengthEvent();
                newEvents[newEvents.Length - 1].action = new SplineAction();
                calculator.lengthEvents = newEvents;
            }
        }
    }
}
#endif
