using UnityEngine;
using System.Collections;

namespace Dreamteck.Splines
{
    public partial class SplineComputer : MonoBehaviour
    {
        [System.Serializable]
        public class Morph
        {
            [System.Serializable]
            internal class SplineMorphState
            {
                public SplinePoint[] points = new SplinePoint[0];
                public float percent = 1f;
                public string name = "";
            }
            [SerializeField]
            private SplineComputer computer = null;
            [SerializeField]
            private SplineMorphState[] morphStates = new SplineMorphState[0];
            [SerializeField]
            internal bool initialized = false;

            public Morph(SplineComputer input)
            {
                computer = input;
                initialized = true;
            }

            public void SetWeight(int index, float weight)
            {
                morphStates[index].percent = Mathf.Clamp01(weight);
                Update();
            }

            public void SetWeight(string name, float weight)
            {
                int index = GetChannelIndex(name);
                morphStates[index].percent = Mathf.Clamp01(weight);
                Update();
            }

            public void CaptureSnapshot(int index)
            {
                if ((morphStates.Length > 0 && computer.pointCount != morphStates[0].points.Length && index != 0))
                {
                    Debug.LogError("Point count must be the same as " + computer.pointCount);
                    return;
                }
                computer.spline.points.CopyTo(morphStates[index].points, 0);
                Update();
            }

            public void CaptureSnapshot(string name)
            {
                int index = GetChannelIndex(name);
                CaptureSnapshot(index);
            }

            public void Clear()
            {
                morphStates = new SplineMorphState[0];
            }

            public SplinePoint[] GetSnapshot(int index)
            {
                return morphStates[index].points;
            }

            public SplinePoint[] GetSnapshot(string name)
            {
                int index = GetChannelIndex(name);
                return morphStates[index].points;
            }

            public float GetWeight(int index)
            {
                return morphStates[index].percent;
            }

            public float GetWeight(string name)
            {
                int index = GetChannelIndex(name);
                return morphStates[index].percent;
            }

            public void AddChannel(string name)
            {
                if (morphStates.Length > 0 && computer.pointCount != morphStates[0].points.Length)
                {
                    Debug.LogError("Point count must be the same as " + computer.pointCount);
                    return;
                }
                SplineMorphState newMorph = new SplineMorphState();
                newMorph.points = computer.GetPoints(SplineComputer.Space.Local);
                newMorph.name = name;
                if (morphStates.Length > 0) newMorph.percent = 0f;
                SplineMorphState[] newMorphs = new SplineMorphState[morphStates.Length + 1];
                morphStates.CopyTo(newMorphs, 0);
                morphStates = newMorphs;
                morphStates[morphStates.Length - 1] = newMorph;
            }

            public void RemoveChannel(string name)
            {
                int index = GetChannelIndex(name);
                RemoveChannel(index);
            }

            public void RemoveChannel(int index)
            {
                if (index < 0 || index >= morphStates.Length) return;
                SplineMorphState[] newStates = new SplineMorphState[morphStates.Length - 1];
                for (int i = 0; i < morphStates.Length; i++)
                {
                    if (i == index) continue;
                    else if (i < index) newStates[i] = morphStates[i];
                    else if (i >= index) newStates[i - 1] = morphStates[i];
                }
                morphStates = newStates;
            }

            private void Update()
            {
                if (morphStates.Length == 0) return;
                for (int i = 0; i < computer.pointCount; i++)
                {
                    Vector3 pos = morphStates[0].points[i].position;
                    Vector3 tan = morphStates[0].points[i].tangent;
                    Vector3 tan2 = morphStates[0].points[i].tangent2;
                    Vector3 normal = morphStates[0].points[i].normal;
                    Color col = morphStates[0].points[i].color;
                    float size = morphStates[0].points[i].size;
                    for (int n = 1; n < morphStates.Length; n++)
                    {
                        pos += (morphStates[n].points[i].position - morphStates[0].points[i].position) * morphStates[n].percent;
                        tan += (morphStates[n].points[i].tangent - morphStates[0].points[i].tangent) * morphStates[n].percent;
                        tan2 += (morphStates[n].points[i].tangent2 - morphStates[0].points[i].tangent2) * morphStates[n].percent;
                        normal += (morphStates[n].points[i].normal - morphStates[0].points[i].normal) * morphStates[n].percent;
                        col += (morphStates[n].points[i].color - morphStates[0].points[i].color) * morphStates[n].percent;
                        size += (morphStates[n].points[i].size - morphStates[0].points[i].size) * morphStates[n].percent;
                    }
                    SplinePoint point = computer.GetPoint(i, Space.Local);
                    point.type = SplinePoint.Type.Broken;
                    point.position = pos;
                    point.tangent = tan;
                    point.tangent2 = tan2;
                    point.normal = normal;
                    point.color = col;
                    point.size = size;
                    computer.SetPoint(i, point, Space.Local);
                }
            }

            private int GetChannelIndex(string name)
            {
                for (int i = 0; i < morphStates.Length; i++)
                {
                    if (morphStates[i].name == name)
                    {
                        return i;
                    }
                }
                Debug.Log("Channel not found " + name);
                return 0;
            }

            public int GetChannelCount()
            {
                if (morphStates == null) return 0;
                return morphStates.Length;
            }

            public string[] GetChannelNames()
            {
                if (morphStates == null) return new string[0];
                string[] names = new string[morphStates.Length];
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = morphStates[i].name;
                }
                return names;
            }

        }
    }
}
