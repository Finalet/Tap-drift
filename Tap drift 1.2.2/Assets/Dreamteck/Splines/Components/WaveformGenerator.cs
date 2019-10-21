using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Waveform Generator")]
    public class WaveformGenerator : MeshGenerator
    {
        public enum Axis { X, Y, Z }
        public enum Space { World, Local }
        public enum UVWrapMode { Clamp, UniformX, UniformY, Uniform }

        public Axis axis
        {
            get { return _axis; }
            set
            {
                if (value != _axis)
                {
                    _axis = value;
                    Rebuild(false);
                }
            }
        }

        public bool symmetry
        {
            get { return _symmetry; }
            set
            {
                if (value != _symmetry)
                {
                    _symmetry = value;
                    Rebuild(false);
                }
            }
        }

        public UVWrapMode uvWrapMode
        {
            get { return _uvWrapMode; }
            set
            {
                if (value != _uvWrapMode)
                {
                    _uvWrapMode = value;
                    Rebuild(false);
                }
            }
        }

        public int slices
        {
            get { return _slices; }
            set
            {
                if (value != _slices)
                {
                    if (value < 1) value = 1;
                    _slices = value;
                    Rebuild(false);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private Axis _axis = Axis.Y;
        [SerializeField]
        [HideInInspector]
        private bool _symmetry = false;
        [SerializeField]
        [HideInInspector]
        private UVWrapMode _uvWrapMode = UVWrapMode.Clamp;
        [SerializeField]
        [HideInInspector]
        private int _slices = 1;

        protected override void Awake()
        {
            base.Awake();
            mesh.name = "waveform";
        }

        protected override void BuildMesh()
        {
            base.BuildMesh();
            Generate();
        }

        protected override void Build()
        {
            base.Build();
        }

        protected override void LateRun()
        {
            base.LateRun();
        }

        private void Generate()
        {
            int vertexCount = clippedSamples.Length * (_slices + 1);
            AllocateMesh(vertexCount, _slices * (clippedSamples.Length - 1) * 6);
            int vertIndex = 0;
            float avgTop = 0f;
            float totalLength = 0f;
            SplineComputer rootComputer = rootUser.computer;
            Vector3 computerPosition = rootComputer.position;
            Vector3 normal = rootComputer.TransformDirection(Vector3.right);
            switch (_axis)
            {
                case Axis.Y: normal = rootComputer.TransformDirection(Vector3.up); break;
                case Axis.Z: normal = rootComputer.TransformDirection(Vector3.forward); break;
            }

            for (int i = 0; i < clippedSamples.Length; i++)
            {
                Vector3 samplePosition = clippedSamples[i].position;
                Vector3 localSamplePosition = rootComputer.InverseTransformPoint(samplePosition);
                Vector3 bottomPosition = localSamplePosition;
                Vector3 sampleDirection = clippedSamples[i].direction;
                Vector3 sampleNormal = clippedSamples[i].normal;

                float heightPercent = 1f;
                if (_uvWrapMode == UVWrapMode.UniformX || _uvWrapMode == UVWrapMode.Uniform)
                {
                    if (i > 0) totalLength += Vector3.Distance(clippedSamples[i].position, clippedSamples[i - 1].position);
                }
                switch (_axis)
                {
                    case Axis.X: bottomPosition.x = _symmetry ? -localSamplePosition.x : 0f; heightPercent = uvScale.y * Mathf.Abs(localSamplePosition.x); avgTop += localSamplePosition.x; break;
                    case Axis.Y: bottomPosition.y = _symmetry ? -localSamplePosition.y : 0f;  heightPercent = uvScale.y * Mathf.Abs(localSamplePosition.y); avgTop += localSamplePosition.y; break;
                    case Axis.Z: bottomPosition.z = _symmetry ? -localSamplePosition.z : 0f;  heightPercent = uvScale.y * Mathf.Abs(localSamplePosition.z); avgTop += localSamplePosition.z; break;
                }
                bottomPosition = rootComputer.TransformPoint(bottomPosition);
                Vector3 right = Vector3.Cross(normal, sampleDirection).normalized;
                Vector3 offsetRight = Vector3.Cross(sampleNormal, sampleDirection);
                
                for (int n = 0; n < _slices + 1; n++)
                {
                    float slicePercent = ((float)n / _slices);
                    tsMesh.vertices[vertIndex] = Vector3.Lerp(bottomPosition, samplePosition, slicePercent) + normal * offset.y + offsetRight * offset.x;
                    tsMesh.normals[vertIndex] = right;
                    switch (_uvWrapMode)
                    {
                        case UVWrapMode.Clamp: tsMesh.uv[vertIndex] = new Vector2((float)clippedSamples[i].percent * uvScale.x + uvOffset.x, slicePercent * uvScale.y + uvOffset.y); break;
                        case UVWrapMode.UniformX: tsMesh.uv[vertIndex] = new Vector2(totalLength * uvScale.x + uvOffset.x, slicePercent * uvScale.y + uvOffset.y); break;
                        case UVWrapMode.UniformY: tsMesh.uv[vertIndex] = new Vector2((float)clippedSamples[i].percent * uvScale.x + uvOffset.x, heightPercent * slicePercent * uvScale.y + uvOffset.y); break;
                        case UVWrapMode.Uniform: tsMesh.uv[vertIndex] = new Vector2(totalLength * uvScale.x + uvOffset.x, heightPercent * slicePercent * uvScale.y + uvOffset.y); break;
                    }
                    tsMesh.colors[vertIndex] = clippedSamples[i].color * color;
                    vertIndex++;
                }
            }
            if (clippedSamples.Length > 0) avgTop /= clippedSamples.Length;
            MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, _slices, clippedSamples.Length, avgTop < 0f);
        }
    }
}
