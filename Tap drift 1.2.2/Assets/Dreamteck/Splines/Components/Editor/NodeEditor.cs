using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines{
    [CustomEditor(typeof(Node), true)]
    public class NodeEditor : Editor {
        private SplineComputer addComp = null;
        private int addPoint = 0;
        private Node lastnode = null;
        private Vector2 scroll;
        private Vector3 position, scale;
        private Quaternion rotation;
        private int[] availablePoints;

        public override void OnInspectorGUI()
        {
            Node node = (Node)target;
            SplineComputer lastComp = addComp;
            addComp = (SplineComputer)EditorGUILayout.ObjectField("Add Computer", addComp, typeof(SplineComputer), true);
            if (lastComp != addComp)
            {
                SceneView.RepaintAll();
                if (addComp != null) availablePoints = GetAvailablePoints(addComp);
            }
            if (addComp != null)
            {
                string[] pointNames = new string[availablePoints.Length];
                for (int i = 0; i < pointNames.Length; i++)
                {
                    pointNames[i] = "Point " + (availablePoints[i]+1);
                }
                if (availablePoints.Length > 0) addPoint = EditorGUILayout.Popup("Link point", addPoint, pointNames);
                else EditorGUILayout.LabelField("No Points Available");

                if (GUILayout.Button("Cancel"))
                {
                    addComp = null;
                    addPoint = 0;
                }
                if (addPoint >= 0 && availablePoints.Length > addPoint)
                {
                    if (node.HasConnection(addComp, availablePoints[addPoint])) EditorGUILayout.HelpBox("Connection already exists (" + addComp.name + "," + availablePoints[addPoint], MessageType.Error);
                    else if (GUILayout.Button("Link"))
                    {
                        AddConnection(addComp, availablePoints[addPoint]);
                    }
                }
            } else RenderConnections();
        }

        void RenderConnections()
        {
            Node node = (Node)target;
            Node.Connection[] connections = node.GetConnections();
            scroll = EditorGUILayout.BeginScrollView(scroll, GUI.skin.box, GUILayout.Width(EditorGUIUtility.currentViewWidth - 30), GUILayout.Height(Mathf.Min(75 + connections.Length * 20, 200)));
            EditorGUILayout.LabelField("Connections");
            EditorGUILayout.Space();

            if (connections.Length > 0)
            {
                for (int i = 0; i < connections.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(connections[i].computer.name + " at point " + (connections[i].pointIndex+1));
                    if (GUILayout.Button("Select", GUILayout.Width(70)))
                    {
                        Selection.activeGameObject = connections[i].computer.gameObject;
                    }
                    if (SplineEditorGUI.EditorLayoutSelectableButton(new GUIContent("Swap Tangents"), connections[i].computer.type == Spline.Type.Bezier, connections[i].invertTangents))
                    {
                        connections[i].invertTangents = !connections[i].invertTangents;
                        node.UpdateConnectedComputers();
                        SceneView.RepaintAll();
                    }
                   
                    if (GUILayout.Button("x", GUILayout.Width(20)))
                    {
                        Undo.RecordObject(node, "Remove connection");
                        Undo.RecordObject(connections[i].computer, "Remove node");
                        node.RemoveConnection(connections[i].computer, connections[i].pointIndex);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else EditorGUILayout.HelpBox("Drag & Drop SplineComputers here to link their points.", MessageType.Info);

            EditorGUILayout.EndScrollView();

            Rect rect = GUILayoutUtility.GetLastRect();
            SplineComputer[] addComps;
            SplineComputer lastComp = addComp;
            bool dragged = DreamteckEditorGUI.DropArea<SplineComputer>(rect, out addComps);
            if (dragged && addComps.Length > 0) SelectComputer(addComps[0]);
            if (lastComp != addComp) SceneView.RepaintAll();
            
            node.transformNormals = EditorGUILayout.Toggle("Transform Normals", node.transformNormals);
            node.transformSize = EditorGUILayout.Toggle("Transform Size", node.transformSize);
            node.transformTangents = EditorGUILayout.Toggle("Transform Tangents", node.transformTangents);

            EditorGUI.BeginChangeCheck();
            if (connections.Length > 1) node.type = (Node.Type)EditorGUILayout.EnumPopup("Node type", node.type);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
                node.UpdateConnectedComputers();
            }
        }

        void OnEnable()
        {
            lastnode = ((Node)target);
            lastnode.EditorMaintainConnections();
        }

        void OnDestroy()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                if (((Node)target) == null)
                {
                    Node.Connection[] connections = lastnode.GetConnections();
                    for(int i = 0; i < connections.Length; i++)
                    {
                        if (connections[i].computer == null) continue;
                        Undo.RecordObject(connections[i].computer, "Delete node connections");
                    }
                    lastnode.ClearConnections();
                }
            }
        }

        void SelectComputer(SplineComputer comp)
        {
            addComp = comp;
            if (addComp != null) availablePoints = GetAvailablePoints(addComp);
            SceneView.RepaintAll();
            Repaint();
        }

        void AddConnection(SplineComputer computer, int pointIndex)
        {
            Node node = (Node)target;
            Node.Connection[] connections = node.GetConnections();
            if (EditorUtility.DisplayDialog("Link point?", "Add point " + (pointIndex+1) + " to connections?", "Yes", "No"))
            {
                Undo.RecordObject(addComp, "Add connection");
                Undo.RecordObject(node, "Add Connection");
                if (connections.Length == 0)
                {
                    switch (EditorUtility.DisplayDialogComplex("Align node to point?", "This is the first connection for the node, would you like to snap or align the node's Transform the spline point.", "No", "Snap", "Snap and Align"))
                    {
                        case 1: SplinePoint point = addComp.GetPoint(pointIndex);
                            node.transform.position = point.position;
                            break;
                        case 2:
                            SplineResult result = addComp.Evaluate((double)pointIndex / (addComp.pointCount - 1));
                            node.transform.position = result.position;
                            node.transform.rotation = result.rotation;
                            break;
                    }
                }
                node.AddConnection(computer, pointIndex);
                addComp = null;
                addPoint = 0;
                SceneView.RepaintAll();
                Repaint();
            }
        }

        int[] GetAvailablePoints(SplineComputer computer)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < computer.pointCount; i++)
            {
                bool found = false;
                for (int n = 0; n < computer.nodeLinks.Length; n++)
                {
                    if (computer.nodeLinks[n].pointIndex == i)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) indices.Add(i);
            }
            return indices.ToArray();
        }

        protected virtual void OnSceneGUI()
        {
            Node node = (Node)target;
            Node.Connection[] connections = node.GetConnections();
#if DREAMTECK_SPLINES
            for (int i = 0; i < connections.Length; i++) SplineDrawer.DrawSplineComputer(connections[i].computer, 0.0, 1.0, 0.5f);
#endif
            bool update = false;
            if (position != node.transform.position)
            {
                position = node.transform.position;
                update = true;
            }
            if(scale != node.transform.localScale){
                scale = node.transform.localScale;
                update = true;
            }
            if (rotation != node.transform.rotation)
            {
                rotation = node.transform.rotation;
                update = true;
            }
            if(update) node.UpdateConnectedComputers();

            if (addComp == null)
            {
                if (connections.Length > 0)
                {
                    bool bezier = false;
                    for (int i = 0; i < connections.Length; i++)
                    {
                        if (connections[i].computer == null) continue;
                        if (connections[i].computer.type == Spline.Type.Bezier)
                        {
                            bezier = true;
                            continue;
                        }
                    }
                    if (bezier && node.type == Node.Type.Smooth)  
                    {
                        if (connections[0].computer != null)
                        {
                            SplinePoint point = node.GetPoint(0, true);
                            Handles.DrawDottedLine(node.transform.position, point.tangent, 6f);
                            Handles.DrawDottedLine(node.transform.position, point.tangent2, 6f);
                            Vector3 lastPos = point.tangent;
                            bool setPoint = false;
                            point.SetTangentPosition(Handles.PositionHandle(point.tangent, node.transform.rotation));
                            if (lastPos != point.tangent) setPoint = true;
                            lastPos = point.tangent2;
                            point.SetTangent2Position(Handles.PositionHandle(point.tangent2, node.transform.rotation));
                            if (lastPos != point.tangent2) setPoint = true;

                            if (setPoint)
                            {
                                node.SetPoint(0, point, true);
                                node.UpdateConnectedComputers();
                            }
                        }
                    }
                }
                return;
            }
            SplinePoint[] points = addComp.GetPoints();
            Transform camTransform = SceneView.currentDrawingSceneView.camera.transform;
#if DREAMTECK_SPLINES
            SplineDrawer.DrawSplineComputer(addComp, 0.0, 1.0, 0.5f);
#endif
            TextAnchor originalAlignment = GUI.skin.label.alignment;
            Color originalColor = GUI.skin.label.normal.textColor;

            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.normal.textColor = addComp.editorPathColor;
            for (int i = 0; i < availablePoints.Length; i++)
            {
                if (addComp.isClosed && i == points.Length - 1) break;

                Handles.Label(points[i].position + Camera.current.transform.up * HandleUtility.GetHandleSize(points[i].position) * 0.3f, (i + 1).ToString());
                if (SplineEditorHandles.CircleButton(points[availablePoints[i]].position, Quaternion.LookRotation(-camTransform.forward, camTransform.up), HandleUtility.GetHandleSize(points[availablePoints[i]].position) * 0.1f, 2f, addComp.editorPathColor))
                {
                    AddConnection(addComp, availablePoints[i]);
                    break;
                }
            }
            GUI.skin.label.alignment = originalAlignment;
            GUI.skin.label.normal.textColor = originalColor;

        }
    }
}
