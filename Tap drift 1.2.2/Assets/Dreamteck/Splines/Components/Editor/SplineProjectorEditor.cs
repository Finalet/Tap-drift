#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SplineProjector), true)]
    public class SplineProjectorEditor : SplineTracerEditor
    {
        private Vector3 lastPos = Vector3.zero;
        private bool info = false;

        public override void OnInspectorGUI()
        {
            SplineProjector user = (SplineProjector)target;
            if (user.mode == SplineProjector.Mode.Accurate)
            {
                showResolution = false;
                showAveraging = false;
            }
            else
            {
                showResolution = true;
                showAveraging = true;
            }
            base.OnInspectorGUI();
        }

        protected override void BodyGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Projector", EditorStyles.boldLabel);
            SplineProjector user = (SplineProjector)target;

            user.mode = (SplineProjector.Mode)EditorGUILayout.EnumPopup("Mode", user.mode);
            if(user.mode == SplineProjector.Mode.Accurate) user.subdivide = EditorGUILayout.IntSlider("Subdivisions", user.subdivide, 1, 8);
            user.projectTarget = (Transform)EditorGUILayout.ObjectField("Project Target", user.projectTarget, typeof(Transform), true);
            user.targetObject = (GameObject)EditorGUILayout.ObjectField("Apply Target", user.targetObject, typeof(GameObject), true);
            GUI.color = Color.white;
            user.autoProject = EditorGUILayout.Toggle("Auto Project", user.autoProject);

            base.BodyGUI();

            info = EditorGUILayout.Foldout(info, "Info");
            if (info) EditorGUILayout.HelpBox("Projection percent: " + user.result.percent, MessageType.Info);

            if (GUI.changed && !Application.isPlaying && user.computer != null)
            {
                if (user.autoProject) {
                    user.CalculateProjection();
                    if (user.targetObject == null) SceneView.RepaintAll();
                }
            }
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            SplineProjector user = (SplineProjector)target;
            if (user.computer == null) return;
            if (Application.isPlaying) return;
            Vector3 projectPos = user.projectTarget.position;
            if (user.autoProject && lastPos != projectPos)
            {
                lastPos = projectPos;
                user.CalculateProjection();
            }
            if (!user.autoProject) return;
            if (user.targetObject == null) DrawResult(user.offsettedResult);
        }
    }
}
#endif
