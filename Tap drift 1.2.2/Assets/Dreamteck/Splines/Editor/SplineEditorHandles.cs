using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    public static class SplineEditorHandles
    {
        public enum SplineSliderGizmo { ForwardTriangle, BackwardTriangle, DualArrow, Rectangle, Circle }

        public static bool Slider(SplineUser user, ref double percent, Color color, string text = "", SplineSliderGizmo gizmo = SplineSliderGizmo.Rectangle, float buttonSize = 1f)
        {
            Camera cam = SceneView.currentDrawingSceneView.camera;
            SplineResult result = user.Evaluate(percent);
            float size = HandleUtility.GetHandleSize(result.position);

            Handles.color = new Color(color.r, color.g, color.b, 0.4f);
            Handles.DrawSolidDisc(result.position, cam.transform.position - result.position, size * 0.2f * buttonSize);
            Handles.color = Color.white;
            if ((color.r + color.g + color.b + color.a) / 4f >= 0.9f) Handles.color = Color.black;

            Vector3 center = result.position;
            Vector2 screenPosition = HandleUtility.WorldToGUIPoint(center);
            screenPosition.y += 20f;
            Vector3 localPos = cam.transform.InverseTransformPoint(center);
            if (text != "" && localPos.z > 0f)
            {
                Handles.BeginGUI();
                DreamteckEditorGUI.Label(new Rect(screenPosition.x - 120 + text.Length * 4, screenPosition.y, 120, 25), text);
                Handles.EndGUI();
            }
            bool buttonClick = SliderButton(center, false, Color.white, 0.3f);
            Vector3 lookAtCamera = (cam.transform.position - result.position).normalized;
            Vector3 right = Vector3.Cross(lookAtCamera, result.direction).normalized * size * 0.1f * buttonSize;
            Vector3 front = Vector3.forward;
            switch (gizmo)
            {
                case SplineSliderGizmo.BackwardTriangle:
                    center += result.direction * size * 0.06f * buttonSize;
                    front = center - result.direction * size * 0.2f * buttonSize;
                    Handles.DrawLine(center + right, front);
                    Handles.DrawLine(front, center - right);
                    Handles.DrawLine(center - right, center + right);
                    break;

                case SplineSliderGizmo.ForwardTriangle:
                    center -= result.direction * size * 0.06f * buttonSize;
                    front = center + result.direction * size * 0.2f * buttonSize;
                    Handles.DrawLine(center + right, front);
                    Handles.DrawLine(front, center - right);
                    Handles.DrawLine(center - right, center + right);
                    break;

                case SplineSliderGizmo.DualArrow:
                    center += result.direction * size * 0.025f * buttonSize;
                    front = center + result.direction * size * 0.17f * buttonSize;
                    Handles.DrawLine(center + right, front);
                    Handles.DrawLine(front, center - right);
                    Handles.DrawLine(center - right, center + right);
                    center -= result.direction * size * 0.05f * buttonSize;
                    front = center - result.direction * size * 0.17f * buttonSize;
                    Handles.DrawLine(center + right, front);
                    Handles.DrawLine(front, center - right);
                    Handles.DrawLine(center - right, center + right);
                    break;
                case SplineSliderGizmo.Rectangle:

                    break;

                case SplineSliderGizmo.Circle:
                    Handles.DrawWireDisc(center, lookAtCamera, 0.13f * size * buttonSize);
                    break;
            }
            Vector3 lastPos = result.position;
            Handles.color = Color.clear;
#if UNITY_5_5_OR_NEWER
            result.position = Handles.FreeMoveHandle(result.position, Quaternion.LookRotation(cam.transform.position - result.position), size * 0.2f * buttonSize, Vector3.zero, Handles.CircleHandleCap);
#else
            result.position = Handles.FreeMoveHandle(result.position, Quaternion.LookRotation(cam.transform.position - result.position), size * 0.2f * buttonSize, Vector3.zero, Handles.CircleCap);
#endif
            if (result.position != lastPos) percent = user.Project(result.position).percent;
            Handles.color = Color.white;
            return buttonClick;
        }

        static bool SliderButton(Vector3 position, bool drawHandle, Color color, float size)
        {
            Camera cam = SceneView.currentDrawingSceneView.camera;
            Vector3 localPos = cam.transform.InverseTransformPoint(position);
            if (localPos.z < 0f) return false;

            size *= HandleUtility.GetHandleSize(position);
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(position);
            Vector2 screenRectBase = HandleUtility.WorldToGUIPoint(position - cam.transform.right * size + cam.transform.up * size);
            Rect rect = new Rect(screenRectBase.x, screenRectBase.y, (screenPos.x - screenRectBase.x) * 2f, (screenPos.y - screenRectBase.y) * 2f);
            if (drawHandle)
            {
                Color previousColor = Handles.color;
                Handles.color = color;
#if UNITY_5_5_OR_NEWER
                Handles.RectangleHandleCap(0, position, Quaternion.LookRotation(-cam.transform.forward), HandleUtility.GetHandleSize(position) * 0.1f, EventType.Repaint);
#else
                 Handles.RectangleCap(0, position, Quaternion.LookRotation(-cam.transform.forward), HandleUtility.GetHandleSize(position) * 0.1f);
#endif
                Handles.color = previousColor;
            }
            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CircleButton(Vector3 position, Quaternion rotation, float size, float clickableAreaMultiplier, Color color)
        {
            Color prev = Handles.color;
            bool result = false;
            Handles.color = color;
#if UNITY_5_5_OR_NEWER
            result = Handles.Button(position, rotation, size, size * clickableAreaMultiplier, Handles.CircleHandleCap);
#else
           result = Handles.Button(position, rotation, size, size * clickableAreaMultiplier, Handles.CircleCap);
#endif
            Handles.color = prev;
            return result;
        }

        public static Vector3 FreeMoveRectangle(Vector3 position, float size) 
        {
#if UNITY_5_5_OR_NEWER
           return Handles.FreeMoveHandle(position, Quaternion.identity, size, Vector3.zero, Handles.RectangleHandleCap);
#else
           return Handles.FreeMoveHandle(position, Quaternion.identity, size, Vector3.zero, Handles.RectangleCap);
#endif
        }

        public static Vector3 FreeMoveCircle(Vector3 position, float size)
        {
#if UNITY_5_5_OR_NEWER
            return Handles.FreeMoveHandle(position, Quaternion.identity, size, Vector3.zero, Handles.CircleHandleCap);
#else
           return Handles.FreeMoveHandle(position, Quaternion.identity, size, Vector3.zero, Handles.CircleCap);
#endif
        }

        public static void DrawSolidSphere(Vector3 position, float radius)
        {
#if UNITY_5_5_OR_NEWER
            Handles.SphereHandleCap(0, position, Quaternion.identity, radius, EventType.Repaint);
#else
            Handles.SphereCap(0, position, Quaternion.identity, radius);
#endif
        }

        public static void DrawCircle(Vector3 position, Quaternion rotation, float radius)
        {
#if UNITY_5_5_OR_NEWER
            Handles.CircleHandleCap(0, position, rotation, radius, EventType.Repaint);
#else
            Handles.CircleCap(0, position, rotation, radius);
#endif
        }

        public static void DrawRectangle(Vector3 position, Quaternion rotation, float size)
        {
#if UNITY_5_5_OR_NEWER
            Handles.RectangleHandleCap(0, position, rotation, size, EventType.Repaint);
#else
            Handles.RectangleCap(0, position, rotation, size);
#endif
        }

        public static void DrawArrowCap(Vector3 position, Quaternion rotation, float size)
        {
#if UNITY_5_5_OR_NEWER
            Handles.ArrowHandleCap(0, position, rotation, size, EventType.Repaint);
#else
            Handles.ArrowCap(0, position, rotation, size);
#endif
        }

        public static bool HoverArea(Vector3 position, float size)
        {
            Camera cam = SceneView.currentDrawingSceneView.camera;
            Vector3 localPos = cam.transform.InverseTransformPoint(position);
            if (localPos.z < 0f) return false;

            size *= HandleUtility.GetHandleSize(position);
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(position);
            Vector2 screenRectBase = HandleUtility.WorldToGUIPoint(position - cam.transform.right * size + cam.transform.up * size);
            Rect rect = new Rect(screenRectBase.x, screenRectBase.y, (screenPos.x - screenRectBase.x) * 2f, (screenPos.y - screenRectBase.y) * 2f);
            if (rect.Contains(Event.current.mousePosition)) return true;
            else return false;
        }
    }
}

