using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Dreamteck.Splines {
    public class PathWindow : SplineEditorWindow {
        private SplineUser user;
        private Vector2 scroll = Vector2.zero;
        private Vector2 scroll2 = Vector2.zero;

        SplineComputer currentComputer;
        List<Node> nodes = new List<Node>();
        List<int> connectionIndices = new List<int>();
        List<int> pointIndices = new List<int>();

        protected override void OnInitialize()
        {
            user = (SplineUser)editor.target;
            GetAvailable();
        }

        protected override string GetTitle()
        {
            return "Junction Path";
        }

        void OnGUI()
        {
            if (user == null) Close();
            Rect rect = new Rect(5, 5, position.width * 0.4f - 10, position.height - 10);
            GUI.Box(rect, "Current Address");
            Rect viewRect = new Rect(0, 0, position.width * 0.4f - 25, user.address.depth * 20 + 20);
            scroll = GUI.BeginScrollView(rect, scroll, viewRect);
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            GUI.Label(new Rect(0, 30, viewRect.width * 0.75f, 20), user.address.root.name + " [" + user.address.elements[0].startPoint + " - " + user.address.elements[0].endPoint + "] ");
            GUI.color = Color.white;
            for (int i = 1; i < user.address.depth; i++)
            {
                string dirString = "─►";
                if (user.address.elements[i].startPoint > user.address.elements[i].endPoint) dirString = "◄─";
                GUI.Label(new Rect(0, 30 + 20 * i, viewRect.width - 35f, 20), user.address.elements[i].computer.name + " ["+ user.address.elements[i].startPoint + " - " + user.address.elements[i].endPoint + "] " + dirString);
                if (GUI.Button(new Rect(viewRect.width - 30f, 30 + 20 * i, 30, 20), "x"))
                {
                    user.ExitAddress(user.address.depth - i);
                    GetAvailable();
                    SceneView.RepaintAll();
                    break;
                }
            }
            GUI.EndScrollView();
            rect = new Rect(position.width * 0.4f, 5, position.width * 0.6f-5, position.height - 10);
            GUI.Box(rect, "Available junctions");
            viewRect = new Rect(0, 0, position.width * 0.6f - 20, nodes.Count*20);
            scroll2 = GUI.BeginScrollView(rect, scroll2, viewRect);
            for (int i = 0; i < nodes.Count; i++) {
                Node.Connection connection = nodes[i].GetConnections()[connectionIndices[i]];
                GUI.Label(new Rect(0, 30 + 20 * i, viewRect.width * 0.75f, viewRect.height), connection.computer.name + " [at P" + pointIndices[i] + "]");
                float x = viewRect.width - 70f;
                if(connection.pointIndex > 0)
                {
                    if (GUI.Button(new Rect(x, 30 + 20 * i, 35, 20), "◄─"))
                    {
                        user.EnterAddress(nodes[i], connectionIndices[i], Spline.Direction.Backward);
                        GetAvailable();
                        SceneView.RepaintAll();
                        break;
                    }
                    x += 35f;
                }
                if(connection.pointIndex < connection.computer.pointCount - 1)
                {
                    if (GUI.Button(new Rect(x, 30 + 20 * i, 35, 20), "─►"))
                    {
                        user.EnterAddress(nodes[i], connectionIndices[i], Spline.Direction.Forward);
                        GetAvailable();
                        SceneView.RepaintAll();
                        break;
                    }
                }
            }
            GUI.EndScrollView();
        }

        void GetAvailable()
        {
            if (user == null) return;
            currentComputer = user.address.elements[user.address.depth - 1].computer;
            if (currentComputer == null) return;
            double startPercent = (double)user.address.elements[user.address.depth - 1].startPoint / (currentComputer.pointCount - 1);
            Spline.Direction dir = Spline.Direction.Forward;
            if (user.address.elements[user.address.depth - 1].startPoint > user.address.elements[user.address.depth - 1].endPoint) dir = Spline.Direction.Backward;
            int[] available = currentComputer.GetAvailableNodeLinksAtPosition(startPercent, dir);
            nodes = new List<Node>();
            connectionIndices = new List<int>();
            pointIndices = new List<int>();
            for (int i = 0; i < available.Length; i++)
            {
                Node node = currentComputer.nodeLinks[available[i]].node;
                if (currentComputer.nodeLinks[available[i]].pointIndex == user.address.elements[user.address.depth - 1].startPoint) continue;
                Node.Connection[] connections = node.GetConnections();
                for (int n = 0; n < connections.Length; n++)
                {
                    if (connections[n].computer == currentComputer) continue;
                    nodes.Add(node);
                    connectionIndices.Add(n);
                    pointIndices.Add(currentComputer.nodeLinks[available[i]].pointIndex);
                }
            }
        }

        void OnFocus()
        {
            GetAvailable();
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            Handles.BeginGUI();
            for (int i = 0; i < pointIndices.Count; i++)
            {
                Vector2 screenPosition = HandleUtility.WorldToGUIPoint(currentComputer.GetPoint(pointIndices[i]).position);
                screenPosition.y -= 25f;
                string pointName = "P" + pointIndices[i];
                DreamteckEditorGUI.Label(new Rect(screenPosition.x - 120 + pointName.Length * 4, screenPosition.y, 120, 25), pointName);
            }
            Handles.EndGUI();
        }
    }
}
