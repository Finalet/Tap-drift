using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Dreamteck.Splines
{
    public class SplineComputerMergeEditor : SplinePointEditor
    {
        SplineComputer[] availableMergeComputers = new SplineComputer[0];
        public enum MergeSide { Start, End }
        public MergeSide mergeSide = MergeSide.End;
        public bool mergeEndpoints = false;

        public override void LoadState()
        {
            mergeEndpoints = LoadBool("mergeEndpoints");
            mergeSide = (MergeSide)LoadInt("mergeSide");
        }

        public override void SaveState()
        {
            SaveBool("mergeEndpoints", mergeEndpoints);
            SaveInt("mergeSide", (int)mergeSide);
        }

        public void Init()
        {
            SplineComputer[] found = GameObject.FindObjectsOfType<SplineComputer>();
            List<SplineComputer> available = new List<SplineComputer>();
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] != computer && !computer.isClosed && computer.pointCount >= 2) available.Add(found[i]);
            }
            availableMergeComputers = available.ToArray();
        }

        public override bool SceneEdit(ref SplinePoint[] points, ref List<int> selected)
        {
            bool change = false;
            Camera editorCamera = SceneView.currentDrawingSceneView.camera;
            for (int i = 0; i < availableMergeComputers.Length; i++)
            {
                SplineDrawer.DrawSplineComputer(availableMergeComputers[i]);
                SplinePoint startPoint = availableMergeComputers[i].GetPoint(0);
                SplinePoint endPoint = availableMergeComputers[i].GetPoint(availableMergeComputers[i].pointCount-1);
                Handles.color = availableMergeComputers[i].editorPathColor;
                
                if (SplineEditorHandles.CircleButton(startPoint.position, Quaternion.LookRotation(editorCamera.transform.position - startPoint.position), HandleUtility.GetHandleSize(startPoint.position) * 0.15f, 1f, availableMergeComputers[i].editorPathColor))
                {
                    Merge(i, mergeSide, MergeSide.Start, ref points);
                    change = true;
                    break;
                }
                if (SplineEditorHandles.CircleButton(endPoint.position, Quaternion.LookRotation(editorCamera.transform.position - endPoint.position), HandleUtility.GetHandleSize(endPoint.position) * 0.15f, 1f, availableMergeComputers[i].editorPathColor))
                {
                    Merge(i, mergeSide, MergeSide.End, ref points);
                    change = true;
                    break;
                }
            }
            Handles.color = Color.white;
            return change;
        }

        void Merge(int index, MergeSide currentSide, MergeSide otherSide, ref SplinePoint[] points)
        {
            SplinePoint[] mergedPoints = availableMergeComputers[index].GetPoints();
            SplinePoint[] original = new SplinePoint[points.Length];
            points.CopyTo(original, 0);
            List<SplinePoint> pointsList = new List<SplinePoint>();
            if (!mergeEndpoints) points = new SplinePoint[mergedPoints.Length + original.Length];
            else points = new SplinePoint[mergedPoints.Length + original.Length - 1];

            if(currentSide == MergeSide.End)
            {
                if(otherSide == MergeSide.Start)
                {
                    for (int i = 0; i < original.Length; i++) pointsList.Add(original[i]);
                    for (int i = mergeEndpoints ? 1 : 0; i < mergedPoints.Length; i++) pointsList.Add(mergedPoints[i]);
                } else
                {
                    for (int i = 0; i < original.Length; i++) pointsList.Add(original[i]);
                    for (int i = 0; i < mergedPoints.Length - (mergeEndpoints ? 1 : 0); i++) pointsList.Add(mergedPoints[(mergedPoints.Length-1)-i]);
                }
            } else
            {
                if (otherSide == MergeSide.Start)
                {
                    for (int i = mergeEndpoints ? 1 : 0; i < mergedPoints.Length; i++) pointsList.Add(mergedPoints[i]);
                    for (int i = 0; i < original.Length; i++) pointsList.Add(original[i]);
                }
                else
                {
                    for (int i = 0; i < mergedPoints.Length - (mergeEndpoints ? 1 : 0); i++) pointsList.Add(mergedPoints[(mergedPoints.Length - 1) - i]);
                    for (int i = 0; i < original.Length; i++) pointsList.Add(original[i]);
                }
            }
            points = pointsList.ToArray();
            double mergedPercent = (double)(mergedPoints.Length-1) / (points.Length-1);
            double from = 0.0;
            double to = 1.0;
            if (currentSide == MergeSide.End)
            {
                from = 1.0 - mergedPercent;
                to = 1.0;
            }
            else
            {
                from = 0.0;
                to = mergedPercent;
            }
            MergeComputer(availableMergeComputers[index], from, to);
            Init();
        }

        void MergeComputer(SplineComputer from, double mapFrom, double mapTo)
        {
            SplineUser[] subs = from.GetSubscribers();
            for(int i = 0; i < subs.Length; i++)
            {
                from.Unsubscribe(subs[i]);
                subs[i].computer = computer;
                subs[i].clipFrom = DMath.Lerp(mapFrom, mapTo, subs[i].clipFrom);
                subs[i].clipTo = DMath.Lerp(mapFrom, mapTo, subs[i].clipTo);
            }
            if(EditorUtility.DisplayDialog("Keep merged computer's GameObject?", "Do you want to keep the merged computer's game object? This will transfer all subscribed users to the current computer.", "Yes", "No"))
            {
                GameObject.DestroyImmediate(from);
            } else GameObject.DestroyImmediate(from.gameObject); 
        }
    }
}
