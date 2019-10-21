using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Tube Generator")]
    public class TubeGenerator : MeshGenerator
    {
        public enum CapMethod { None, Flat, Round }

        public int sides
        {
            get { return _sides; }
            set
            {
                if (value != _sides)
                {
                    if (value < 3) value = 3;
                    _sides = value;
                    Rebuild(false);
                }
            }
        }

        public CapMethod capMode
        {
            get { return _capMode; }
            set
            {
                if (value != _capMode)
                {
                    _capMode = value;
                    Rebuild(false);
                }
            }
        }

        public int roundCapLatitude
        {
            get { return _roundCapLatitude; }
            set
            {
                if (value < 1) value = 1;
                if (value != _roundCapLatitude)
                {
                    _roundCapLatitude = value;
                    if(_capMode == CapMethod.Round) Rebuild(false);
                }
            }
        }

        public float integrity
        {
            get { return _integrity; }
            set
            {
                if (value != _integrity)
                {
                    _integrity = value;
                    Rebuild(false);
                }
            }
        }

        public float capUVScale
        {
            get { return _capUVScale; }
            set
            {
                if (value != _capUVScale)
                {
                    _capUVScale = value;
                    Rebuild(false);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private int _sides = 12;
        [SerializeField]
        [HideInInspector]
        private int _roundCapLatitude = 6;
        [SerializeField]
        [HideInInspector]
        private CapMethod _capMode = CapMethod.None;
        [SerializeField]
        [HideInInspector]
        private float _integrity = 360f;
        [SerializeField]
        [HideInInspector]
        private float _capUVScale = 1f;

        private bool useCap
        {
            get
            {
                bool isCapSet = _capMode != CapMethod.None;
                if (computer != null) return isCapSet && (!computer.isClosed || span < 1f);
                else if (sampleUser)
                {
                    SplineUser root = rootUser;
                    if (root == null) return isCapSet;
                    if (root.computer != null) return isCapSet && (!root.computer.isClosed || root.span < 1f);
                }
                return isCapSet;
            }
        }

        private int bodyVertexCount = 0;
        private int bodyTrisCount = 0;
        private int capVertexCount = 0;
        private int capTrisCount = 0;

        protected override void Reset()
        {
            base.Reset();
        }

        protected override void Awake()
        {
            base.Awake();
            mesh.name = "tube";
        }


        protected override void BuildMesh()
        {
            if (_sides <= 2) return;
            base.BuildMesh();
            bodyVertexCount = (_sides + 1) * clippedSamples.Length;
            CapMethod _capModeFinal = _capMode;
            if (!useCap) _capModeFinal = CapMethod.None;
            switch (_capModeFinal)
            {
                case CapMethod.Flat: capVertexCount = _sides + 1; break;
                case CapMethod.Round: capVertexCount = _roundCapLatitude * (sides + 1); break;
                default: capVertexCount = 0; break;
            }
            int vertexCount = bodyVertexCount + capVertexCount * 2;

            bodyTrisCount = _sides * (clippedSamples.Length - 1) * 2 * 3;
            switch (_capModeFinal)
            {
                case CapMethod.Flat: capTrisCount = (_sides - 1) * 3 * 2; break;
                case CapMethod.Round: capTrisCount = _sides * _roundCapLatitude * 6; break;
                default: capTrisCount = 0; break;
            }
            AllocateMesh(vertexCount, bodyTrisCount + capTrisCount * 2);

            Generate();
            switch (_capModeFinal)
            {
                case CapMethod.Flat: GenerateFlatCaps(); break;
                case CapMethod.Round: GenerateRoundCaps(); break;
            }
        }

        void Generate()
        {
            int vertexIndex = 0;
            ResetUVDistance();
            for (int i = 0; i < clippedSamples.Length; i++)
            {
                Vector3 center = clippedSamples[i].position;
                Vector3 right = clippedSamples[i].right;
                if (offset != Vector3.zero) center += offset.x * right + offset.y * clippedSamples[i].normal + offset.z * clippedSamples[i].direction;
                if(uvMode == UVMode.UniformClamp || uvMode == UVMode.UniformClip)  AddUVDistance(i);
                for (int n = 0; n < _sides + 1; n++)
                {
                    float anglePercent = (float)(n) / _sides;
                    Quaternion rot = Quaternion.AngleAxis(_integrity * anglePercent + rotation + 180f, clippedSamples[i].direction);
                    tsMesh.vertices[vertexIndex] = center + rot * right * size * clippedSamples[i].size * 0.5f;
                    CalculateUVs(clippedSamples[i].percent, anglePercent);
                    tsMesh.uv[vertexIndex] = Vector2.one * 0.5f + (Vector2)(Quaternion.AngleAxis(uvRotation, Vector3.forward) * (Vector2.one * 0.5f - uvs));
                    tsMesh.normals[vertexIndex] = Vector3.Normalize(tsMesh.vertices[vertexIndex] - center);
                    tsMesh.colors[vertexIndex] = clippedSamples[i].color * color;
                    vertexIndex++;
                }
            }
            MeshUtility.GeneratePlaneTriangles(ref tsMesh.triangles, _sides, clippedSamples.Length, false);
        }

        void GenerateFlatCaps()
        {
            //Start Cap
            for (int i = 0; i < _sides+1; i++)
            {
                int index = bodyVertexCount + i;
                tsMesh.vertices[index] = tsMesh.vertices[i];
                tsMesh.normals[index] = -clippedSamples[0].direction;
                tsMesh.colors[index] = tsMesh.colors[i];
                tsMesh.uv[index] = Quaternion.AngleAxis(_integrity * (((float)i) / (_sides - 1)), Vector3.forward) * Vector2.right * 0.5f * capUVScale + Vector3.right * 0.5f + Vector3.up * 0.5f;
            }

            //End Cap
            for (int i = 0; i < _sides + 1; i++)
            {
                int index = bodyVertexCount + (_sides + 1) + i;
                int bodyIndex = bodyVertexCount - (_sides + 1) + i;
                tsMesh.vertices[index] = tsMesh.vertices[bodyIndex];
                tsMesh.normals[index] = clippedSamples[clippedSamples.Length-1].direction;
                tsMesh.colors[index] = tsMesh.colors[bodyIndex];
                tsMesh.uv[index] = Quaternion.AngleAxis(_integrity * ((float)(bodyIndex) / (_sides - 1)), Vector3.forward) * Vector2.right * 0.5f * capUVScale + Vector3.right * 0.5f + Vector3.up * 0.5f;
            }

            int t = bodyTrisCount;
            bool fullIntegrity = _integrity == 360f;
            int finalSides = fullIntegrity ? _sides - 1 : _sides;
            //Start cap
            for (int i = 0; i < finalSides - 1; i++)
            {
                tsMesh.triangles[t++] = i + bodyVertexCount + 2;
                tsMesh.triangles[t++] = i + +bodyVertexCount + 1;
                tsMesh.triangles[t++] = bodyVertexCount;
            }

            //End cap
            for (int i = 0; i < finalSides - 1; i++)
            {
                tsMesh.triangles[t++] = bodyVertexCount + (_sides + 1);
                tsMesh.triangles[t++] = i + 1 + bodyVertexCount + (_sides + 1);
                tsMesh.triangles[t++] = i + 2 + bodyVertexCount + (_sides + 1);
            }
        }

        void GenerateRoundCaps()
        {
            //Start Cap
            Vector3 center = clippedSamples[0].position;
            Quaternion lookRot = Quaternion.LookRotation(-clippedSamples[0].direction, clippedSamples[0].normal);
            float startV = 0f;
                        float capLengthPercent = 0f;
            switch (uvMode)
            {
                case UVMode.Clip: startV = (float)clippedSamples[0].percent;
                    capLengthPercent = (size * 0.5f) / CalculateLength(); break;
                case UVMode.UniformClip:
                    startV = CalculateLength(0.0, clippedSamples[0].percent);
                    capLengthPercent = size * 0.5f; break;
                case UVMode.UniformClamp:
                    startV = 0f;
                    capLengthPercent = size * 0.5f / (float)span;
                    break;
                case UVMode.Clamp: capLengthPercent = (size * 0.5f) / CalculateLength(clipFrom, clipTo); break;
            }
            for (int lat = 1; lat < _roundCapLatitude+1; lat++)
            {
                float latitudePercent = ((float)lat / _roundCapLatitude);
                float latAngle = 90f * latitudePercent;
                for (int lon = 0; lon <= sides; lon++)
                {
                    float anglePercent = (float)lon / sides;
                    int index = bodyVertexCount + lon + (lat-1) * (sides + 1);
                    Quaternion rot = Quaternion.AngleAxis(_integrity * anglePercent + rotation + 180f, -Vector3.forward) * Quaternion.AngleAxis(latAngle, Vector3.up);
                    tsMesh.vertices[index] = center + lookRot * rot * -Vector3.right * size * 0.5f * clippedSamples[0].size;
                    tsMesh.colors[index] = clippedSamples[0].color * color;
                    tsMesh.normals[index] = (tsMesh.vertices[index] - center).normalized;
                    tsMesh.uv[index] = new Vector2(anglePercent * uvScale.x, (startV - capLengthPercent * latitudePercent) * uvScale.y) - uvOffset;
                }
            }


            //Triangles
            int t = bodyTrisCount;
            for (int z = -1; z < _roundCapLatitude - 1; z++)
            {
                for (int x = 0; x < sides; x++)
                {
                    int current = bodyVertexCount + x + z * (sides + 1);
                    int next = current + (sides + 1);
                    if (z == -1)
                    {
                        current = x;
                        next = bodyVertexCount + x;
                    }
                    tsMesh.triangles[t++] = next + 1;
                    tsMesh.triangles[t++] = current + 1;
                    tsMesh.triangles[t++] = current;
                    tsMesh.triangles[t++] = next;
                    tsMesh.triangles[t++] = next + 1;
                    tsMesh.triangles[t++] = current;
                }
            }


            //End Cap
            center = clippedSamples[clippedSamples.Length-1].position;
            lookRot = Quaternion.LookRotation(clippedSamples[clippedSamples.Length - 1].direction, clippedSamples[clippedSamples.Length - 1].normal);
            switch (uvMode)
            {
                case UVMode.Clip: startV = (float)clippedSamples[clippedSamples.Length-1].percent; break;
                case UVMode.UniformClip: startV = CalculateLength(0.0, clippedSamples[clippedSamples.Length - 1].percent); break;
                case UVMode.Clamp: startV = 1f; break;
                case UVMode.UniformClamp: startV = CalculateLength(); break;
            }
            for (int lat = 1; lat < _roundCapLatitude+1; lat++)
            {
                float latitudePercent = ((float)lat / _roundCapLatitude);
                float latAngle = 90f * latitudePercent;
                for (int lon = 0; lon <= sides; lon++)
                {
                    float anglePercent = (float)lon / sides;
                    int index = bodyVertexCount + capVertexCount + lon + (lat - 1) * (sides + 1);
                    Quaternion rot = Quaternion.AngleAxis(_integrity * anglePercent + rotation + 180f, Vector3.forward) * Quaternion.AngleAxis(latAngle, -Vector3.up);
                    tsMesh.vertices[index] = center + lookRot * rot * Vector3.right * size * 0.5f * clippedSamples[clippedSamples.Length-1].size;
                    tsMesh.normals[index] = (tsMesh.vertices[index] - center).normalized;
                    tsMesh.colors[index] = clippedSamples[clippedSamples.Length - 1].color * color;
                    tsMesh.uv[index] = new Vector2(anglePercent*uvScale.x, (startV + capLengthPercent * latitudePercent)*uvScale.y) - uvOffset;
                }
            }

            //Triangles
            for (int z = -1; z < _roundCapLatitude - 1; z++)
            {
                for (int x = 0; x < sides; x++)
                {
                    int current = bodyVertexCount + capVertexCount + x + z * (sides + 1);
                    int next = current + (sides + 1);
                    if (z == -1)
                    {
                        current = bodyVertexCount - (_sides+1) + x;
                        next = bodyVertexCount + capVertexCount + x;
                    }

                    tsMesh.triangles[t++] = current+1;
                    tsMesh.triangles[t++] = next + 1;
                    tsMesh.triangles[t++] = next;
                    tsMesh.triangles[t++] = next;
                    tsMesh.triangles[t++] = current;
                    tsMesh.triangles[t++] = current + 1;
                }
            }
            
        }
    }
}
