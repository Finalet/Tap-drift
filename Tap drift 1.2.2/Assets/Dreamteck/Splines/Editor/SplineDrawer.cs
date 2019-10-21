#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_5_3 || UNITY_5_4_OR_NEWER
using UnityEditor.SceneManagement;
#endif


namespace Dreamteck.Splines {
    [InitializeOnLoad]
    public static class SplineDrawer
    {
        private static bool refreshComputers = false;
        private static List<SplineComputer> drawComputers = new List<SplineComputer>();
        private static Vector3[] positions = new Vector3[0];
#if UNITY_5_3 || UNITY_5_4_OR_NEWER
        private static UnityEngine.SceneManagement.Scene currentScene;
#else
        private static string currentScene = "";
#endif

        static SplineDrawer()
        {
            SceneView.duringSceneGui += AutoDrawComputers;
            FindComputers();
#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged += HerarchyWindowChanged;
#else
            EditorApplication.hierarchyWindowChanged += HerarchyWindowChanged;
#endif

#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += ModeChanged;
#else
            EditorApplication.playmodeStateChanged += ModeChanged;
#endif
        }


#if UNITY_2017_2_OR_NEWER
        static void ModeChanged(PlayModeStateChange stateChange)
        {
            refreshComputers = true;
        }
#else
        static void ModeChanged()
        {
            refreshComputers = true;
        }
#endif

        static void HerarchyWindowChanged()
        {
#if UNITY_5_3 || UNITY_5_4_OR_NEWER
        if (currentScene != EditorSceneManager.GetActiveScene())
            {
                currentScene = EditorSceneManager.GetActiveScene();
                FindComputers();
            }
#else
        if(EditorApplication.currentScene != currentScene)
            {
                currentScene = EditorApplication.currentScene;
                FindComputers();
            }
#endif
            
        }

        static void FindComputers()
        {
            drawComputers.Clear();
            SplineComputer[] computers = GameObject.FindObjectsOfType<SplineComputer>();
            drawComputers.AddRange(computers);
        }

        private static void AutoDrawComputers(SceneView current)
        {
            if (refreshComputers)
            {
                refreshComputers = false;
                FindComputers();
            }
            for (int i = 0; i < drawComputers.Count; i++)
            {
                if (!drawComputers[i].alwaysDraw)
                {
                    drawComputers.RemoveAt(i);
                    i--;
                    continue;
                }
                DrawSplineComputer(drawComputers[i]);
            }
        }

        public static void RegisterComputer(SplineComputer comp)
        {
            if (drawComputers.Contains(comp)) return;
            comp.alwaysDraw = true;
            drawComputers.Add(comp);
        }

        public static void UnregisterComputer(SplineComputer comp)
        {
            for(int i = 0; i < drawComputers.Count; i++)
            {
                if(drawComputers[i] == comp)
                {
                    drawComputers[i].alwaysDraw = false;
                    drawComputers.RemoveAt(i);
                    return;
                }
            }
        }

        public static void DrawSplineComputer(SplineComputer comp, double fromPercent = 0.0, double toPercent = 1.0, float alpha = 1f)
        {
            if (comp == null) return;
            Color prevColor = Handles.color;
            Color orange = new Color(1f, 0.564f, 0f);
            Color handleColor = comp.hasMorph && !MorphWindow.editShapeMode ? orange : comp.editorPathColor;
            handleColor.a = alpha;
            Handles.color = handleColor;
            int iterations = Mathf.CeilToInt(comp.iterations * Mathf.Abs((float)(toPercent - fromPercent)));
            if (comp.pointCount < 2) return;

            if (comp.type == Spline.Type.BSpline && comp.pointCount > 1)
            {
                SplinePoint[] compPoints = comp.GetPoints();
                Handles.color = new Color(handleColor.r, handleColor.g, handleColor.b, 0.5f * alpha);
                for (int i = 0; i < compPoints.Length - 1; i++) Handles.DrawLine(compPoints[i].position, compPoints[i + 1].position);
                Handles.color = handleColor;
            }

            if (!comp.drawThinckness)
            {
                if (positions.Length != iterations * 2) positions = new Vector3[iterations * 2];
                Vector3 prevPoint = comp.EvaluatePosition(fromPercent);
                int pointIndex = 0;
                for (int i = 1; i < iterations; i++)
                {
                    double p = DMath.Lerp(fromPercent, toPercent, (double)i / (iterations - 1));
                    positions[pointIndex] = prevPoint;
                    pointIndex++;
                    positions[pointIndex] = comp.EvaluatePosition(p);
                    pointIndex++;
                    prevPoint = positions[pointIndex - 1];
                }
                Handles.DrawLines(positions);
            }
            else
            {
                Transform editorCamera = SceneView.currentDrawingSceneView.camera.transform;
                if (positions.Length != iterations * 6) positions = new Vector3[iterations * 6];
                SplineResult prevResult = comp.Evaluate(fromPercent);
                Vector3 prevNormal = prevResult.normal;
                if (comp.billboardThickness) prevNormal = (editorCamera.position - prevResult.position).normalized;
                Vector3 prevRight = Vector3.Cross(prevResult.direction, prevNormal).normalized * prevResult.size * 0.5f;
                int pointIndex = 0;
                for (int i = 1; i < iterations; i++)
                {
                    double p = DMath.Lerp(fromPercent, toPercent, (double)i / (iterations - 1));
                    SplineResult newResult = comp.Evaluate(p);
                    Vector3 newNormal = newResult.normal;
                    if (comp.billboardThickness) newNormal = (editorCamera.position - newResult.position).normalized;
                    Vector3 newRight = Vector3.Cross(newResult.direction, newNormal).normalized * newResult.size * 0.5f;

                    positions[pointIndex] = prevResult.position + prevRight;
                    positions[pointIndex + iterations * 2] = prevResult.position - prevRight;
                    positions[pointIndex + iterations * 4] = newResult.position - newRight;
                    pointIndex++;
                    positions[pointIndex] = newResult.position + newRight;
                    positions[pointIndex + iterations * 2] = newResult.position - newRight;
                    positions[pointIndex + iterations * 4] = newResult.position + newRight;
                    pointIndex++;
                    prevResult = newResult;
                    prevRight = newRight;
                    prevNormal = newNormal;
                }
                Handles.DrawLines(positions);
            }
            Handles.color = prevColor;
        }

        static void DrawPath(ref Vector3[] points)
        {
            Vector3[] linePoints = new Vector3[points.Length * 2];
            Vector3 prevPoint = points[0];
            int pointIndex = 0;
            for (int currObjectIndex = 1; currObjectIndex < points.Length; currObjectIndex++)
            {
                linePoints[pointIndex] = prevPoint;
                pointIndex++;
                linePoints[pointIndex] = points[currObjectIndex];
                pointIndex++;
                prevPoint = points[currObjectIndex];
            }
            Handles.DrawLines(linePoints);
        }

        public static void DrawSpline(Spline spline, Color color, double from = 0.0, double to = 1.0, bool drawThickness = false, bool thicknessAutoRotate = false)
        {
            double add = spline.moveStep;
            if (add < 0.0025) add = 0.0025;
            Color prevColor = Handles.color;
            Handles.color = color;
            int iterations = spline.iterations;
            if (drawThickness)
            {
                Transform editorCamera = SceneView.currentDrawingSceneView.camera.transform;
                if (positions.Length != iterations * 6) positions = new Vector3[iterations * 6];
                SplineResult prevResult = spline.Evaluate(from);
                Vector3 prevNormal = prevResult.normal;
                if (thicknessAutoRotate) prevNormal = (editorCamera.position - prevResult.position).normalized;
                Vector3 prevRight = Vector3.Cross(prevResult.direction, prevNormal).normalized * prevResult.size * 0.5f;
                int pointIndex = 0;
                for (int i = 1; i < iterations; i++)
                {
                    double p = DMath.Lerp(from, to, (double)i / (iterations - 1));
                    SplineResult newResult = spline.Evaluate(p);
                    Vector3 newNormal = newResult.normal;
                    if (thicknessAutoRotate) newNormal = (editorCamera.position - newResult.position).normalized;
                    Vector3 newRight = Vector3.Cross(newResult.direction, newNormal).normalized * newResult.size * 0.5f;

                    positions[pointIndex] = prevResult.position + prevRight;
                    positions[pointIndex + iterations * 2] = prevResult.position - prevRight;
                    positions[pointIndex + iterations * 4] = newResult.position - newRight;
                    pointIndex++;
                    positions[pointIndex] = newResult.position + newRight;
                    positions[pointIndex + iterations * 2] = newResult.position - newRight;
                    positions[pointIndex + iterations * 4] = newResult.position + newRight;
                    pointIndex++;
                    prevResult = newResult;
                    prevRight = newRight;
                    prevNormal = newNormal;
                }
                Handles.DrawLines(positions);
            }
            else
            {
                if (positions.Length != iterations * 2) positions = new Vector3[iterations * 2];
                Vector3 prevPoint = spline.EvaluatePosition(from);
                int pointIndex = 0;
                for (int i = 1; i < iterations; i++)
                {
                    double p = DMath.Lerp(from, to, (double)i / (iterations - 1));
                    positions[pointIndex] = prevPoint;
                    pointIndex++;
                    positions[pointIndex] = spline.EvaluatePosition(p);
                    pointIndex++;
                    prevPoint = positions[pointIndex - 1];
                }
                Handles.DrawLines(positions);
            }
            Handles.color = prevColor;
        }
    }
}
#endif
