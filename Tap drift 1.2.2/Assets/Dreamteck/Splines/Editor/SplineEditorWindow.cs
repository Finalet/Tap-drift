using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines {
    public class SplineEditorWindow : EditorWindow
    {
        protected Editor editor;
        protected SplineEditor splineEditor;

        public void Init(Editor e, string inputTitle, Vector2 min, Vector2 max)
        {
            minSize = min;
            maxSize = max;
            Init(e, inputTitle);
        }

        public void Init(Editor e, Vector2 min, Vector2 max)
        {
            minSize = min;
            maxSize = max;
            Init(e);
        }

        public void Init(Editor e, Vector2 size)
        {
            minSize = maxSize = size;
            Init(e);
        }

        public void Init(Editor e, string inputTitle)
        {
            Init(e);
            Title(inputTitle);
        }

        public void Init(Editor e)
        {
            editor = e;
            if (editor is SplineEditor) splineEditor = (SplineEditor)editor;
            else splineEditor = null;
            Title(GetTitle());
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {

        }

        protected virtual string GetTitle()
        {
            return "Spline Editor Window";
        }

        private void Title(string inputTitle)
        {
#if UNITY_5_0
            title = inputTitle;
#else
            titleContent = new GUIContent(inputTitle);
#endif
        }
    }
}
