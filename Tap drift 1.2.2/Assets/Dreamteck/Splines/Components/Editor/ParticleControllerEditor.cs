using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(ParticleController))]
    public class ParticleControllerEditor : SplineUserEditor
    {
        protected override void BodyGUI()
        {
            base.BodyGUI();
            ParticleController user = (ParticleController)target;
            user._particleSystem = (ParticleSystem)EditorGUILayout.ObjectField("Particle System", user._particleSystem, typeof(ParticleSystem), true);
            if (user._particleSystem == null)
            {
                EditorGUILayout.HelpBox("No particle system is assigned", MessageType.Error);
                return;
            }
            user.emitPoint = (ParticleController.EmitPoint)EditorGUILayout.EnumPopup("Emit Point", user.emitPoint);
            user.volumetric = EditorGUILayout.Toggle("Volumetric", user.volumetric);
            if (user.volumetric)
            {
                user.emitFromShell = EditorGUILayout.Toggle("Shell Only", user.emitFromShell);
                user.scale = EditorGUILayout.Vector2Field("Size", user.scale);
            }
            user.motionType = (ParticleController.MotionType)EditorGUILayout.EnumPopup("Motion Type", user.motionType);
            if(user.motionType == ParticleController.MotionType.FollowForward || user.motionType == ParticleController.MotionType.FollowBackward)
            {
                user.wrapMode = (ParticleController.Wrap)EditorGUILayout.EnumPopup("Wrap mode", user.wrapMode);
                EditorGUILayout.Space();
#if UNITY_5_5_OR_NEWER
                EditorGUILayout.LabelField("Path cycles (over " + user._particleSystem.main.startLifetime.constantMax + "s.)", EditorStyles.boldLabel);
#else
                EditorGUILayout.LabelField("Path cycles (over " + user._particleSystem.startLifetime + "s.)", EditorStyles.boldLabel);
#endif
                user.minCycles = EditorGUILayout.FloatField("Min. Cycles", user.minCycles);
                if (user.minCycles < 0f) user.minCycles = 0f;
                user.maxCycles = EditorGUILayout.FloatField("Max. Cycles", user.maxCycles);
                if (user.maxCycles < user.minCycles) user.maxCycles = user.minCycles; 
            }
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Particles may twitch or flash in the editor preview. Play the game to see the in-game result.", MessageType.Info);
            }

        }
    }
}
