using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Dreamteck.Splines
{
    public class SplineComputerSplitEditor : SplinePointEditor
    {
        public override void LoadState()
        {

        }

        public override void SaveState()
        {

        }

        public void Init()
        {

        }

        public override bool SceneEdit(ref SplinePoint[] points, ref List<int> selected)
        {
            bool change = false;
            Camera editorCamera = SceneView.currentDrawingSceneView.camera;

            for (int i = 0; i < computer.pointCount; i++)
            {
                Vector3 pos = computer.GetPointPosition(i);
                if (SplineEditorHandles.CircleButton(pos, Quaternion.LookRotation(editorCamera.transform.position - pos), HandleUtility.GetHandleSize(pos) * 0.12f, 1f, computer.editorPathColor))
                {
                    SplitAtPoint(i, ref points);
                    change = true;
                    break;
                }
            }
            SplineResult projected  = computer.Evaluate(ProjectMouse());
            if (!change)
            {
                float pointValue = (float)projected.percent * (computer.pointCount - 1);
                int pointIndex = Mathf.FloorToInt(pointValue);
                    float size = HandleUtility.GetHandleSize(projected.position) * 0.3f;
                    Vector3 up = Vector3.Cross(editorCamera.transform.forward, projected.direction).normalized * size + projected.position;
                    Vector3 down = Vector3.Cross(projected.direction, editorCamera.transform.forward).normalized * size + projected.position;
                    Handles.color = computer.editorPathColor;
                    Handles.DrawLine(up, down);
                    Handles.color = Color.white;
                if (pointValue - pointIndex > computer.moveStep) { 
                    if (SplineEditorHandles.CircleButton(projected.position, Quaternion.LookRotation(editorCamera.transform.position - projected.position), HandleUtility.GetHandleSize(projected.position) * 0.12f, 1f, computer.editorPathColor))
                    {
                        SplitAtPercent(projected.percent, ref points);
                        change = true;
                    }
                }
                SceneView.RepaintAll();
            }
            Handles.color = Color.white;
            SplineDrawer.DrawSplineComputer(computer, 0.0, projected.percent, 1f);
            SplineDrawer.DrawSplineComputer(computer, projected.percent, 1.0, 0.4f);
            return change;
        }

        void HandleNodes(SplineComputer newSpline, int splitIndex)
        {
            List<Node> nodes = new List<Node>();
            List<int> indices = new List<int>();

            for (int i = computer.nodeLinks.Length - 1; i >= 0; i--)
            {
                if (computer.nodeLinks[i].pointIndex > splitIndex)
                {
                    nodes.Add(computer.nodeLinks[i].node);
                    indices.Add(computer.nodeLinks[i].pointIndex);
                }
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                Undo.RecordObject(nodes[i], "Split");
                EditorUtility.SetDirty(nodes[i]);
                nodes[i].RemoveConnection(computer, indices[i]);
            }

            for (int i = 0; i < nodes.Count; i++) nodes[i].AddConnection(newSpline, indices[i] - splitIndex);
            
        }

       void SplitAtPercent(double percent, ref SplinePoint[] points)
       {
            Undo.RecordObject(computer, "Split At Percent ");
            EditorUtility.SetDirty(computer);
            float pointValue = (computer.pointCount - 1) * (float)percent;
            int lastPointIndex = Mathf.FloorToInt(pointValue);
            int nextPointIndex = Mathf.CeilToInt(pointValue);
            SplinePoint[] splitPoints = new SplinePoint[computer.pointCount - lastPointIndex];
            float lerpPercent = Mathf.InverseLerp(lastPointIndex, nextPointIndex, pointValue);
            SplinePoint splitPoint = SplinePoint.Lerp(computer.GetPoint(lastPointIndex), computer.GetPoint(nextPointIndex), lerpPercent);
            splitPoint.SetPosition(computer.EvaluatePosition(percent));
            splitPoints[0] = splitPoint;
            for (int i = 1; i < splitPoints.Length; i++) splitPoints[i] = computer.GetPoint(lastPointIndex + i);
            SplineComputer spline = CreateNewSpline();
            spline.SetPoints(splitPoints);
            SplineUser[] users = spline.GetSubscribers();
            for (int i = 0; i < users.Length; i++)
            {
                users[i].clipFrom = DMath.InverseLerp(percent, 1.0, users[i].clipFrom);
                users[i].clipTo = DMath.InverseLerp(percent, 1.0, users[i].clipTo);
            }
            splitPoints = new SplinePoint[lastPointIndex + 2];
            for (int i = 0; i <= lastPointIndex; i++) splitPoints[i] = computer.GetPoint(i);
            splitPoints[splitPoints.Length - 1] = splitPoint;
            points = splitPoints;
            users = computer.GetSubscribers();
            for (int i = 0; i < users.Length; i++)
            {
                users[i].clipFrom = DMath.InverseLerp(0.0, percent, users[i].clipFrom);
                users[i].clipTo = DMath.InverseLerp(0.0, percent, users[i].clipTo);
            }
            HandleNodes(spline, lastPointIndex);
        }

        void SplitAtPoint(int index, ref SplinePoint[] points)
        {
            Undo.RecordObject(computer, "Split At Point " + index);
            EditorUtility.SetDirty(computer);
            SplinePoint[] splitPoints = new SplinePoint[computer.pointCount - index];
            for(int i = 0; i < splitPoints.Length; i++) splitPoints[i] = computer.GetPoint(index + i);
            SplineComputer spline = CreateNewSpline();
            spline.SetPoints(splitPoints);

            SplineUser[] users = spline.GetSubscribers();
            for (int i = 0; i < users.Length; i++)
            {
                users[i].clipFrom = DMath.InverseLerp((double)index / (computer.pointCount - 1), 1.0, users[i].clipFrom);
                users[i].clipTo = DMath.InverseLerp((double)index / (computer.pointCount - 1), 1.0, users[i].clipTo);
            }
            splitPoints = new SplinePoint[index + 1];
            for (int i = 0; i <= index; i++) splitPoints[i] = computer.GetPoint(i);
            points = splitPoints;
            users = computer.GetSubscribers();
            for (int i = 0; i < users.Length; i++)
            {
                users[i].clipFrom = DMath.InverseLerp(0.0, ((double)index) / (computer.pointCount - 1), users[i].clipFrom);
                users[i].clipTo = DMath.InverseLerp(0.0, ((double)index) / (computer.pointCount - 1), users[i].clipTo);
            }
            HandleNodes(spline, index);
        }

        SplineComputer CreateNewSpline()
        {
            GameObject go = GameObject.Instantiate(computer.gameObject);
            go.name = computer.name + "_split";
            SplineUser[] users = go.GetComponents<SplineUser>();
            SplineComputer spline = go.GetComponent<SplineComputer>();
            for (int i = 0; i < users.Length; i++)
            {
                computer.Unsubscribe(users[i]);
                users[i].computer = spline;
                spline.Subscribe(users[i]);
            }
            for(int i = go.transform.childCount-1; i>=0; i--)
            {
                GameObject.DestroyImmediate(go.transform.GetChild(i).gameObject);
            }
            return spline;
        }

        private double ProjectMouse()
        {
            if (computer.pointCount == 0) return 0.0;
            float closestDistance = (Event.current.mousePosition - HandleUtility.WorldToGUIPoint(computer.GetPointPosition(0))).sqrMagnitude;
            double closestPercent = 0.0;
            double add = computer.moveStep;
            if (computer.type == Spline.Type.Linear) add /= 2.0;
            int count = 0;
            for (double i = add; i < 1.0; i += add)
            {
                SplineResult result = computer.Evaluate(i);
                Vector2 point = HandleUtility.WorldToGUIPoint(result.position);
                float dist = (point - Event.current.mousePosition).sqrMagnitude;
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestPercent = i;
                }
                count++;
            }
            return closestPercent;
        }
    }
}
