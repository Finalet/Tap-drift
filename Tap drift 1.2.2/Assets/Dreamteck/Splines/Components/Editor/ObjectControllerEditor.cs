using UnityEngine;
using System.Collections;
using UnityEditor;
using Dreamteck.Splines;    

[CustomEditor(typeof(ObjectController))]
public class ObjectControllerEditor : SplineUserEditor
{

        protected override void BodyGUI()
        {
        base.BodyGUI();
        ObjectController user = (ObjectController)target;
        user.objectMethod = (ObjectController.ObjectMethod)EditorGUILayout.EnumPopup("Object Method", user.objectMethod);
        if (user.objectMethod == ObjectController.ObjectMethod.Instantiate) user.retainPrefabInstancesInEditor = EditorGUILayout.Toggle("Retain Prefab Instances", user.retainPrefabInstancesInEditor);
        if(user.objectMethod == ObjectController.ObjectMethod.Instantiate)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Instantiate Objects", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical();
            bool objectsChanged = false;
            for (int i = 0; i < user.objects.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                user.objects[i] = (GameObject)EditorGUILayout.ObjectField(user.objects[i], typeof(GameObject), true);
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    GameObject[] newObjects = new GameObject[user.objects.Length - 1];
                    for (int n = 0; n < user.objects.Length; n++)
                    {
                        if (n < i) newObjects[n] = user.objects[n];
                        else if (n == i) continue;
                        else newObjects[n - 1] = user.objects[n];
                        objectsChanged = true;
                    }
                    user.objects = newObjects;
                }
                if (i > 0)
                {
                    if (GUILayout.Button("▲", GUILayout.Width(20)))
                    {
                        GameObject temp = user.objects[i - 1];
                        user.objects[i - 1] = user.objects[i];
                        user.objects[i] = temp;
                        objectsChanged = true;
                    }
                }
                if (i < user.objects.Length - 1)
                {
                    if (GUILayout.Button("▼", GUILayout.Width(20)))
                    {
                        GameObject temp = user.objects[i + 1];
                        user.objects[i + 1] = user.objects[i];
                        user.objects[i] = temp;
                        objectsChanged = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            GameObject newObj = null;
            newObj = (GameObject)EditorGUILayout.ObjectField("Add Object", newObj, typeof(GameObject), true);
            if (newObj != null)
            {
                GameObject[] newObjects = new GameObject[user.objects.Length + 1];
                user.objects.CopyTo(newObjects, 0);
                newObjects[newObjects.Length - 1] = newObj;
                user.objects = newObjects;
                objectsChanged = true;
            }

            bool hasObj = false;
            for(int i = 0; i < user.objects.Length; i++)
            {
                if(user.objects[i] != null)
                {
                    hasObj = true;
                    break;
                }
            }

            if (hasObj) user.spawnCount = EditorGUILayout.IntField("Spawn count", user.spawnCount);
            else user.spawnCount = 0;
            user.delayedSpawn = EditorGUILayout.Toggle("Delayed spawn", user.delayedSpawn);
            if (user.delayedSpawn) user.spawnDelay = EditorGUILayout.FloatField("Spawn Delay", user.spawnDelay);
            
            ObjectController.Iteration lastIteration = user.iteration;
            user.iteration = (ObjectController.Iteration)EditorGUILayout.EnumPopup("Iteration", user.iteration);
            if(lastIteration != user.iteration) objectsChanged = true;
            if(objectsChanged)
            {
                user.Clear();
                user.Spawn();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Transform", EditorStyles.boldLabel);
        user.applyRotation = EditorGUILayout.Toggle("Apply Rotation", user.applyRotation);
        if (user.applyRotation)
        {
            EditorGUI.indentLevel++;
            user.minRotationOffset = EditorGUILayout.Vector3Field("Min. Rotation Offset", user.minRotationOffset);
            user.maxRotationOffset = EditorGUILayout.Vector3Field("Max. Rotation Offset", user.maxRotationOffset);
            EditorGUI.indentLevel--;
        }
        user.applyScale = EditorGUILayout.Toggle("Apply Scale", user.applyScale);
        if (user.applyScale)
        {
            EditorGUI.indentLevel++;
            user.minScaleMultiplier = EditorGUILayout.Vector3Field("Min. Scale Multiplier", user.minScaleMultiplier);
            user.maxScaleMultiplier = EditorGUILayout.Vector3Field("Max. Scale Multiplier", user.maxScaleMultiplier);
            EditorGUI.indentLevel--;
        }
        //Add random rotation
        //Add random scale

        user.objectPositioning = (ObjectController.Positioning)EditorGUILayout.EnumPopup("Object Positioning", user.objectPositioning);
        user.positionOffset = EditorGUILayout.Slider("Evaluate Offset", user.positionOffset, -1f, 1f);

        user.offset = EditorGUILayout.Vector2Field("Offset", user.offset);
        user.randomizeOffset = EditorGUILayout.Toggle("Randomize Offset", user.randomizeOffset);
        if (user.randomizeOffset)
        {
            user.randomSize = EditorGUILayout.Vector2Field("Size", user.randomSize);
            user.randomSeed = EditorGUILayout.IntField("Random Seed", user.randomSeed);
            //user.randomOffsetSize = EditorGUILayout.FloatField("Size", user.randomOffsetSize);
            user.shellOffset = EditorGUILayout.Toggle("Shell", user.shellOffset);
            if(user.applyRotation) user.useRandomOffsetRotation = EditorGUILayout.Toggle("Apply offset rotation", user.useRandomOffsetRotation);
        }
    }

}
