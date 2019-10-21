#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(ExtrudeMesh), true)]
    public class ExtrudeMeshEditor : MeshGenEditor
    {
        private Mesh GetMeshFromObject(Object obj)
        {
            ExtrudeMesh user = (ExtrudeMesh)target;
            if (!(obj is GameObject)) return null;
            GameObject gameObj = (GameObject)obj;
            MeshFilter filter = gameObj.GetComponent<MeshFilter>();
            Mesh returnMesh = null;
            if (filter != null && filter.sharedMesh != null) returnMesh = filter.sharedMesh;
            MeshRenderer rend = user.GetComponent<MeshRenderer>();
            if (rend == null) return returnMesh;
            if (rend.sharedMaterials.Length > 0 && user.hasAnyMesh) return returnMesh;
            MeshRenderer meshRend = gameObj.GetComponent<MeshRenderer>();
            if (meshRend != null)
            {
                if (meshRend.sharedMaterials != null) rend.sharedMaterials = meshRend.sharedMaterials;
                else if (meshRend.materials != null) rend.materials = meshRend.materials;
            }
            return returnMesh;
        }

        protected override void BodyGUI()
        {
            EditorGUILayout.HelpBox("The ExtrudeMesh component has been deprecated in 1.0.9. We encourage using the SplineMesh component for future projects.", MessageType.Info);
            showSize = false;
            showColor = false;
            showDoubleSided = false;
            showFlipFaces = false;
            base.BodyGUI();

            ExtrudeMesh user = (ExtrudeMesh)target;
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Meshes", EditorStyles.boldLabel);
            Object obj = null;
            ExtrudeMesh.MirrorMethod mirror = ExtrudeMesh.MirrorMethod.None;
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < user.GetMeshCount(); i++)
            {
                EditorGUILayout.BeginHorizontal();
                obj = user.GetMesh(i);
                obj = EditorGUILayout.ObjectField(user.GetMesh(i), typeof(Mesh), true);
                if (user.GetMesh(i) != obj)
                {
                    if (obj is Mesh) user.SetMesh(i, (Mesh)obj);
                    else user.SetMesh(i, GetMeshFromObject(obj));
                }

                mirror = user.GetMeshMirror(i);
                EditorGUIUtility.labelWidth = 50;
                mirror = (ExtrudeMesh.MirrorMethod)EditorGUILayout.EnumPopup("Mirror", mirror, GUILayout.Width(100));
                if (user.GetMeshMirror(i) != mirror) user.SetMeshMirror(i, mirror);

                EditorGUIUtility.labelWidth = 0;

                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    user.RemoveMesh(i);
                    i--;
                    continue;
                }
                if (i > 0)
                {
                    if (GUILayout.Button("▲", GUILayout.Width(20)))
                    {
                        Mesh temp = user.GetMesh(i - 1);
                        user.SetMesh(i - 1, user.GetMesh(i));
                        user.SetMesh(i, temp);
                    }
                }
                if (i < user.GetMeshCount() - 1)
                {
                    if (GUILayout.Button("▼", GUILayout.Width(20)))
                    {
                        Mesh temp = user.GetMesh(i + 1);
                        user.SetMesh(i + 1, user.GetMesh(i));
                        user.SetMesh(i, temp);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            obj = null;
            obj = EditorGUILayout.ObjectField("Add Mesh", obj, typeof(Object), true);
            if (obj != null)
            {
                if (obj is Mesh) user.AddMesh((Mesh)obj);
                else user.AddMesh(GetMeshFromObject(obj));
            }

            bool hasMeshes = user.hasAnyMesh;
            if (hasMeshes)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Cap Meshes", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                obj = user.GetStartMesh();
                obj = EditorGUILayout.ObjectField("Start Cap", obj, typeof(Object), true);
                if (user.GetStartMesh() != obj)
                {
                    if (obj is Mesh) user.SetStartMesh((Mesh)obj);
                    else user.SetStartMesh(GetMeshFromObject(obj));
                }
                mirror = user.GetStartMeshMirror();
                EditorGUIUtility.labelWidth = 50;
                mirror = (ExtrudeMesh.MirrorMethod)EditorGUILayout.EnumPopup("Mirror", mirror, GUILayout.Width(100));
                EditorGUIUtility.labelWidth = 0;
                if (user.GetStartMeshMirror() != mirror) user.SetStartMeshMirror(mirror);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                obj = user.GetEndMesh();
                obj = EditorGUILayout.ObjectField("End Cap", obj, typeof(Object), true);
                if (user.GetEndMesh() != obj)
                {
                    if (obj is Mesh) user.SetEndMesh((Mesh)obj);
                    else user.SetEndMesh(GetMeshFromObject(obj));
                }
                mirror = user.GetEndMeshMirror();
                EditorGUIUtility.labelWidth = 50;
                mirror = (ExtrudeMesh.MirrorMethod)EditorGUILayout.EnumPopup("Mirror", mirror, GUILayout.Width(100));
                EditorGUIUtility.labelWidth = 0;
                if (user.GetEndMeshMirror() != mirror) user.SetEndMeshMirror(mirror);
                EditorGUILayout.EndHorizontal();
                user.dontStretchCaps = EditorGUILayout.Toggle("Don't stretch caps", user.dontStretchCaps);
            }

            if (user.GetStartMesh() == null && user.GetEndMesh() == null && !hasMeshes)
            {
                EditorGUILayout.HelpBox("No meshes assigned. Assign at least one mesh to Meshes.", MessageType.Warning);
            }

            user.axis = (ExtrudeMesh.Axis)EditorGUILayout.EnumPopup("Axis", user.axis);
            user.iteration = (ExtrudeMesh.Iteration)EditorGUILayout.EnumPopup("Iteration", user.iteration);
            if (user.iteration == ExtrudeMesh.Iteration.Random) user.randomSeed = EditorGUILayout.IntField("Random Seed", user.randomSeed);
            user.repeat = EditorGUILayout.IntField("Repeat", user.repeat);
            if (user.repeat < 1) user.repeat = 1;
            user.spacing = EditorGUILayout.Slider("Spacing", (float)user.spacing, 0f, 1f);
            user.scale = EditorGUILayout.Vector2Field("Scale", user.scale);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Uv Coordinates", EditorStyles.boldLabel);
           
            user.uvOffset = EditorGUILayout.Vector2Field("UV Offset", user.uvOffset);
            user.uvScale = EditorGUILayout.Vector2Field("UV Scale", user.uvScale);
            user.tileUVs = (ExtrudeMesh.TileUVs)EditorGUILayout.EnumPopup("Tile UVs", user.tileUVs);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(user);
            }

        }
    }
}
#endif
