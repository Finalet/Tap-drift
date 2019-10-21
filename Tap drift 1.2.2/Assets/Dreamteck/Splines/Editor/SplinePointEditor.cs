#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace Dreamteck.Splines
{ 
    public class SplinePointEditor
    {
        public SplineComputer computer;
        public bool focused = true;
        protected Quaternion orientation = Quaternion.identity;
        protected string prefPrefix = "";

        public virtual bool ToolbarGUI(float scale)
        {
            return false;
        }

        public virtual bool SceneEdit(ref SplinePoint[] points, ref List<int> selected)
        {
            return false;
        }

        protected void SaveBool(string variableName, bool value)
        {
            if(prefPrefix == "") prefPrefix = GetType().ToString();
            EditorPrefs.SetBool(prefPrefix+"."+ variableName, value);
        }

        protected void SaveInt(string variableName, int value)
        {
            if (prefPrefix == "") prefPrefix = GetType().ToString();
            EditorPrefs.SetInt(prefPrefix + "." + variableName, value);
        }

        protected void SaveFloat(string variableName, float value)
        {
            if (prefPrefix == "") prefPrefix = GetType().ToString();
            EditorPrefs.SetFloat(prefPrefix + "." + variableName, value);
        }

        protected void SaveString(string variableName, string value)
        {
            if (prefPrefix == "") prefPrefix = GetType().ToString();
            EditorPrefs.SetString(prefPrefix + "." + variableName, value);
        }

        protected bool LoadBool(string variableName)
        {
            if (prefPrefix == "") prefPrefix = GetType().ToString();
            return EditorPrefs.GetBool(prefPrefix + "." + variableName, false);
        }

        protected int LoadInt(string variableName)
        {
            if (prefPrefix == "") prefPrefix = GetType().ToString();
            return EditorPrefs.GetInt(prefPrefix + "." + variableName, 0);
        }

        protected float LoadFloat(string variableName, float d = 0f)
        {
            if (prefPrefix == "") prefPrefix = GetType().ToString();
            return EditorPrefs.GetFloat(prefPrefix + "." + variableName, d);
        }

        protected string LoadString(string variableName)
        {
            if (prefPrefix == "") prefPrefix = GetType().ToString();
            return EditorPrefs.GetString(prefPrefix + "." + variableName, "");
        }

        public virtual void SaveState()
        {
           
        }

        public virtual void LoadState()
        {
            
        }
    }
}
#endif