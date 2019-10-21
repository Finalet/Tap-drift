#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SplineFollower), true)]
    public class SplineFollowerEditor : SplineTracerEditor
    {
        SplineResult result = new SplineResult();
        void OnSetDistance(float distance)
        {
            SplineFollower follower = (SplineFollower)target;
            follower.SetDistance(distance);
        }

        protected override void BodyGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Follower", EditorStyles.boldLabel);
            SplineFollower follower = (SplineFollower)target;
            follower.followMode = (SplineFollower.FollowMode)EditorGUILayout.EnumPopup("Follow mode", follower.followMode);
            follower.wrapMode = (SplineFollower.Wrap)EditorGUILayout.EnumPopup("Wrap mode", follower.wrapMode);


            EditorGUILayout.BeginHorizontal();
            if (!follower.autoStartPosition) EditorGUIUtility.labelWidth = 80;
            EditorGUILayout.LabelField("Start position", GUILayout.Width(EditorGUIUtility.labelWidth));
            DistanceWindowMenu(GUILayoutUtility.GetLastRect(), OnSetDistance);
            EditorGUIUtility.labelWidth = 0;

            double startPosition = follower.ClipPercent(follower.result.percent);
            if (!follower.autoStartPosition && !Application.isPlaying) startPosition = EditorGUILayout.Slider((float)startPosition, 0f, 1f);
            EditorGUIUtility.labelWidth = 55f;
            follower.autoStartPosition = EditorGUILayout.Toggle("Auto", follower.autoStartPosition, GUILayout.Width(80f));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            follower.autoFollow = EditorGUILayout.Toggle("Auto follow", follower.autoFollow);
            if (follower.followMode == SplineFollower.FollowMode.Uniform)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(20));
                follower.followSpeed = EditorGUILayout.FloatField("Follow speed", follower.followSpeed);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(20));
                follower.followDuration = EditorGUILayout.FloatField("Follow duration", follower.followDuration);
                EditorGUILayout.EndHorizontal();
            }
            
            if(follower.motion.applyRotation) follower.applyDirectionRotation = EditorGUILayout.Toggle("Face Direction", follower.applyDirectionRotation);

            base.BodyGUI();

            if (GUI.changed && !Application.isPlaying && follower.samples.Length > 0)
            {
                if (!follower.autoStartPosition)
                {
                    follower.SetPercent(startPosition);
                    if (!follower.autoFollow) SceneView.RepaintAll();
                }
            }
        }


        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            SplineFollower user = (SplineFollower)target;
            if (Application.isPlaying)
            {
                if (!user.autoFollow) DrawResult(user.offsettedResult);
                return;
            }
            if (user.computer == null) return;
            if (user.autoStartPosition)
            {
                user.Evaluate(result, user.address.Project(user.transform.position, 4, user.clipFrom, user.clipTo));
                DrawResult(result);
            } else if(!user.autoFollow) DrawResult(user.result);
            
        }
    }
}
#endif
