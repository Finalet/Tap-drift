using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dreamteck.Splines
{

    public class SplineUserSubEditor
    {
        public int editIndex = 1;
        bool foldout = false;
        protected string title = "";
        protected SplineUserEditor parentEditor = null;

        public bool isOpen
        {
            get { return foldout; }
        }

        public SplineUserSubEditor(SplineUserEditor parent)
        {
            parentEditor = parent;
        }

        public void DrawInspector()
        {
            foldout = EditorGUILayout.Foldout(foldout, title);
            if (foldout) DrawInspectorLogic();
        }

        public void DrawScene()
        {
            DrawSceneLogic();
        }

        protected virtual void DrawInspectorLogic()
        {

        }

        protected virtual void DrawSceneLogic()
        {

        }
    }
}