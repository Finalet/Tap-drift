#if UNITY_EDITOR
namespace Dreamteck {
    using UnityEngine;
    using UnityEditor;
    using System.IO;

    public static class ScriptableObjectUtility
    {
        public static ScriptableObject CreateAsset<T>(string name = "") where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            SaveAsset<T>(asset, name);
            return asset;
        }

        public static ScriptableObject CreateAsset(string type, string name = "")
        {
            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            SaveAsset<ScriptableObject>(asset, name);
            return asset;
        }

        static void SaveAsset<T>(T asset, string name = "") where T : ScriptableObject
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string assetName = "New " + typeof(T).ToString();
            if (name != "") assetName = name;
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + assetName + ".asset");
            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
#endif
