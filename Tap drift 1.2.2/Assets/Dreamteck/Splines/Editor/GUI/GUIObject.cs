#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    public class GUIObject
    {
        public string title = "";
        public GUIContent content = null;

        public GUIObject(string t)
        {
            title = t;
        }

        public GUIObject(GUIContent c)
        {
            content = c;
        }

        public virtual void Draw()
        {

        }

        protected void BeginPanel(string t = "")
        {
            GUILayout.BeginVertical(t, GUI.skin.box);
            EditorGUILayout.Space();
            if (t != "")
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
        }

        protected void EndPanel()
        {
            EditorGUILayout.Space();
            GUILayout.EndVertical();
        }

        protected int ButtonGroup(string[] labels, float height, bool horizontal)
        {
            
            int result = -1;
            if (horizontal) EditorGUILayout.BeginHorizontal();
            for(int i = 0; i < labels.Length; i++)
            {
                if (GUILayout.Button(labels[i], GUILayout.Height(height))) result = i;
            }
            if (horizontal) EditorGUILayout.EndHorizontal();
            return result;
        }

        protected GUIContent GetTitleConent()
        {
            if (content == null) content = new GUIContent(title);
            return content;
        }

        protected string GetTitle()
        {
            if (content != null) return content.text;
            else return title;
        }
    }
}

#endif