using UnityEngine;
using System.Collections;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Surface Generator")]
    public class SurfaceGenerator : MeshGenerator
    {
        public float expand
        {
            get { return _expand; }
            set
            {
                if (value != _expand)
                {
                    _expand = value;
                    Rebuild(false);
                }
            }
        }

        public float extrude
        {
            get { return _extrude; }
            set
            {
                if (value != _extrude)
                {
                    _extrude = value;
                    Rebuild(false);
                } 
            }
        }

        public double extrudeClipFrom
        {
            get { return _extrudeFrom; }
            set
            {
                if (value != _extrudeFrom)
                {
                    _extrudeFrom = value;
                    Rebuild(false);
                }
            }
        }

        public double extrudeClipTo
        {
            get { return _extrudeTo; }
            set
            {
                if (value != _extrudeTo)
                {
                    _extrudeTo = value;
                    Rebuild(false);
                }
            }
        }

        public Vector2 sideUvScale
        {
            get { return _sideUvScale; }
            set
            {
                if (value != _sideUvScale)
                {
                    _sideUvScale = value;
                    Rebuild(false);
                }
                else _sideUvScale = value;
            }
        }

        public Vector2 sideUvOffset
        {
            get { return _sideUvOffset; }
            set
            {
                if (value != _sideUvOffset)
                {
                    _sideUvOffset = value;
                    Rebuild(false);
                } else _sideUvOffset = value;
            }
        }

        public SplineComputer extrudeComputer
        {
            get { return _extrudeComputer; }
            set
            {
                if (value != _extrudeComputer)
                {
                    if (_extrudeComputer != null) _extrudeComputer.Unsubscribe(this);
                    _extrudeComputer = value;
                    if(value != null)_extrudeComputer.Subscribe(this);
                    Rebuild(false);
                }
            }
        }

        public bool uniformUvs
        {
            get { return _uniformUvs; }
            set
            {
                if (value != _uniformUvs)
                {
                    _uniformUvs = value;
                    Rebuild(false);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private float _expand = 0f;
        [SerializeField]
        [HideInInspector]
        private float _extrude = 0f;
        [SerializeField]
        [HideInInspector]
        private Vector2 _sideUvScale = Vector2.one;
        [SerializeField]
        [HideInInspector]
        private Vector2 _sideUvOffset = Vector2.zero;
        [SerializeField]
        [HideInInspector]
        private SplineComputer _extrudeComputer;
        [SerializeField]
        [HideInInspector]
        private SplineResult[] extrudeResults = new SplineResult[0];
        [SerializeField]
        [HideInInspector]
        private Vector3[] identityVertices = new Vector3[0];
        [SerializeField]
        [HideInInspector]
        private Vector3[] identityNormals = new Vector3[0];
        [SerializeField]
        [HideInInspector]
        private Vector2[] projectedVerts = new Vector2[0];
        [SerializeField]
        [HideInInspector]
        private int[] capTris = new int[0];
        [SerializeField]
        [HideInInspector]
        private int[] wallTris = new int[0];

        [SerializeField]
        [HideInInspector]
        private double _extrudeFrom = 0.0;
        [SerializeField]
        [HideInInspector]
        private double _extrudeTo = 1.0;
        [SerializeField]
        [HideInInspector]
        private bool _uniformUvs = false;

        protected override void Awake()
        {
            base.Awake();
            mesh.name = "surface";
        }

        protected override void BuildMesh()
        {
            if (computer.pointCount == 0) return;
            base.BuildMesh();
            Generate();
        }

        public void Generate()
        {
            if (_extrudeComputer != null) _extrudeComputer.Evaluate(ref extrudeResults, _extrudeFrom, _extrudeTo);
            int capVertexCount = clippedSamples.Length;
            int wallVertexCount = 0;
            int totalVertexCount = 0;
            if (computer.isClosed) capVertexCount--;
            bool pathExtrude = _extrudeComputer != null && extrudeResults.Length > 0;
            bool simpleExtrude = !pathExtrude && _extrude != 0f;
            totalVertexCount = capVertexCount;
            if (pathExtrude)
            {
                wallVertexCount = clippedSamples.Length * extrudeResults.Length;
                totalVertexCount = capVertexCount*2 + wallVertexCount;
            } else if (simpleExtrude)
            {
                wallVertexCount = clippedSamples.Length * 2;
                totalVertexCount = capVertexCount * 2 + wallVertexCount;
            }

            AllocateMesh(totalVertexCount, tsMesh.triangles.Length);
            Vector3 avgPos = Vector3.zero;
            Vector3 avgNormal = Vector3.zero;
            Vector3 off = transform.right * offset.x + transform.up * offset.y + transform.forward * offset.z;
            for (int i = 0; i < capVertexCount; i++)
            {
                tsMesh.vertices[i] = clippedSamples[i].position + off;
                tsMesh.normals[i] = clippedSamples[i].normal;
                tsMesh.colors[i] = clippedSamples[i].color;
                tsMesh.colors[i] *= color;
                avgPos += tsMesh.vertices[i];
                avgNormal += tsMesh.normals[i];
            }
            avgNormal.Normalize();
            avgPos /= capVertexCount; 
            GetProjectedVertices(tsMesh.vertices, avgNormal, avgPos, capVertexCount);
            Vector2 min = projectedVerts[0];
            Vector2 max = projectedVerts[0];
            for (int i = 1; i < projectedVerts.Length; i++)
            {
                if (min.x < projectedVerts[i].x) min.x = projectedVerts[i].x;
                if (min.y < projectedVerts[i].y) min.y = projectedVerts[i].y;
                if (max.x > projectedVerts[i].x) max.x = projectedVerts[i].x;
                if (max.y > projectedVerts[i].y) max.y = projectedVerts[i].y;
            }
            for (int i = 0; i < projectedVerts.Length; i++)
            {
                tsMesh.uv[i].x = Mathf.InverseLerp(max.x, min.x, projectedVerts[i].x) * uvScale.x - uvScale.x * 0.5f + uvOffset.x + 0.5f;
                tsMesh.uv[i].y = Mathf.InverseLerp(min.y, max.y, projectedVerts[i].y) * uvScale.y - uvScale.y * 0.5f + uvOffset.y + 0.5f;
            }
            
            bool clockwise = IsClockwise(projectedVerts);
            bool flipCap = false;
            bool flipSide = false;
            if (!clockwise) flipSide = !flipSide;
            if (simpleExtrude && _extrude < 0f)
            {
                flipCap = !flipCap;
                flipSide = !flipSide;
            }
            GenerateCapTris(flipCap);
            if (flipCap)
            {
                for (int i = 0; i < capVertexCount; i++)
                {
                    tsMesh.normals[i] *= -1f;
                }
            }
            if (pathExtrude)
            {
                GetIdentityVerts(avgPos, avgNormal, clockwise);
                //Generate cap vertices with flipped normals
                for (int i = 0; i < capVertexCount; i++)
                {
                    tsMesh.vertices[i + capVertexCount] = extrudeResults[0].position + extrudeResults[0].rotation * identityVertices[i] + off;
                    tsMesh.normals[i + capVertexCount] = -extrudeResults[0].direction;
                    tsMesh.colors[i + capVertexCount] = tsMesh.colors[i] * extrudeResults[0].color;
                    tsMesh.uv[i + capVertexCount] = new Vector2(1f-tsMesh.uv[i].x, tsMesh.uv[i].y);

                    tsMesh.vertices[i] = extrudeResults[extrudeResults.Length - 1].position + extrudeResults[extrudeResults.Length-1].rotation * identityVertices[i] + off;
                    tsMesh.normals[i] = extrudeResults[extrudeResults.Length - 1].direction;
                    tsMesh.colors[i] *= extrudeResults[extrudeResults.Length - 1].color;
                }
                //Add wall vertices
                float totalLength = 0f;
                for (int i = 0; i < extrudeResults.Length; i++)
                {
                    if (_uniformUvs && i > 0) totalLength += Vector3.Distance(extrudeResults[i].position, extrudeResults[i - 1].position);
                    int startIndex = capVertexCount * 2 + i * clippedSamples.Length;
                    for(int n = 0; n < identityVertices.Length; n++)
                    {
                        tsMesh.vertices[startIndex + n] = extrudeResults[i].position + extrudeResults[i].rotation * identityVertices[n] + off;
                        tsMesh.normals[startIndex + n] = extrudeResults[i].rotation * identityNormals[n];
                        if (_uniformUvs) tsMesh.uv[startIndex + n] = new Vector2((float)n / (identityVertices.Length - 1) * _sideUvScale.x + _sideUvOffset.x, totalLength * _sideUvScale.y + _sideUvOffset.y);
                        else tsMesh.uv[startIndex + n] = new Vector2((float)n / (identityVertices.Length - 1) * _sideUvScale.x + _sideUvOffset.x, (float)i / (extrudeResults.Length - 1) * _sideUvScale.y + _sideUvOffset.y);
                        if (clockwise) tsMesh.uv[startIndex + n].x = 1f - tsMesh.uv[startIndex + n].x;
                    }
                }
                MeshUtility.GeneratePlaneTriangles(ref wallTris, clippedSamples.Length - 1, extrudeResults.Length, flipSide, 0, 0, true);
                tsMesh.triangles = new int[capTris.Length * 2 + wallTris.Length];
                int written = WriteTris(ref capTris, ref tsMesh.triangles, 0, 0, false);
                written = WriteTris(ref capTris, ref tsMesh.triangles, capVertexCount, written, true);
                written = WriteTris(ref wallTris, ref tsMesh.triangles, capVertexCount * 2, written, false);
            } else if (simpleExtrude)
            {
                //Duplicate cap vertices with flipped normals
                for (int i = 0; i < capVertexCount; i++)
                {
                    tsMesh.vertices[i + capVertexCount] = tsMesh.vertices[i];
                    if (_expand != 0f) tsMesh.vertices[i + capVertexCount] += (clockwise ? -clippedSamples[i].right : clippedSamples[i].right) * _expand;
                    tsMesh.normals[i + capVertexCount] = -tsMesh.normals[i];
                    tsMesh.colors[i + capVertexCount] = tsMesh.colors[i];
                    tsMesh.uv[i + capVertexCount] = new Vector2(1f - tsMesh.uv[i].x, tsMesh.uv[i].y);

                    tsMesh.vertices[i] += avgNormal * _extrude;
                    if (_expand != 0f) tsMesh.vertices[i] += (clockwise ? -clippedSamples[i].right : clippedSamples[i].right) * _expand;
                }
                //Add wall vertices
                for (int i = 0; i < clippedSamples.Length; i++)
                {   
                    tsMesh.vertices[i + capVertexCount * 2] = clippedSamples[i].position + off;
                    if (_expand != 0f) tsMesh.vertices[i + capVertexCount * 2] += (clockwise ? -clippedSamples[i].right : clippedSamples[i].right) * _expand;
                    tsMesh.normals[i + capVertexCount * 2] = clockwise ? -clippedSamples[i].right : clippedSamples[i].right;
                    tsMesh.colors[i + capVertexCount * 2] = clippedSamples[i].color;
                    tsMesh.uv[i + capVertexCount * 2] = new Vector2((float)i / (capVertexCount - 1) * _sideUvScale.x + _sideUvOffset.x, 0f + _sideUvOffset.y);
                    if (clockwise) tsMesh.uv[i + capVertexCount * 2].x = 1f - tsMesh.uv[i + capVertexCount * 2].x;

                    tsMesh.vertices[i + capVertexCount * 2 + clippedSamples.Length] = tsMesh.vertices[i + capVertexCount * 2] + avgNormal * _extrude;
                    tsMesh.normals[i + capVertexCount * 2 + clippedSamples.Length] = clockwise ? -clippedSamples[i].right : clippedSamples[i].right;
                    tsMesh.colors[i + capVertexCount * 2 + clippedSamples.Length] = clippedSamples[i].color;
                    if (_uniformUvs) tsMesh.uv[i + capVertexCount * 2 + clippedSamples.Length] = new Vector2((float)i / (capVertexCount - 1) * _sideUvScale.x + _sideUvOffset.x, _extrude * _sideUvScale.y + _sideUvOffset.y);
                    else tsMesh.uv[i + capVertexCount * 2 + clippedSamples.Length] = new Vector2((float)i / (capVertexCount - 1) * _sideUvScale.x + _sideUvOffset.x, 1f * _sideUvScale.y + _sideUvOffset.y);
                    if (clockwise) tsMesh.uv[i + capVertexCount * 2 + clippedSamples.Length].x = 1f - tsMesh.uv[i + capVertexCount * 2 + clippedSamples.Length].x;
                }
                MeshUtility.GeneratePlaneTriangles(ref wallTris, clippedSamples.Length-1, 2, flipSide, 0, 0, true);
                int trisCount = capTris.Length * 2 + wallTris.Length;
                if (doubleSided) trisCount *= 2;
                if (tsMesh.triangles.Length != trisCount) tsMesh.triangles = new int[trisCount];
                int written = WriteTris(ref capTris, ref tsMesh.triangles, 0, 0, false);
                written = WriteTris(ref capTris, ref tsMesh.triangles, capVertexCount, written, true);
                written = WriteTris(ref wallTris, ref tsMesh.triangles, capVertexCount * 2, written, false);
            } else {
                //for (int i = 0; i < tsMesh.vertices.Length; i++) tsMesh.vertices[i] += clockwise ? -clippedSamples[i].right : clippedSamples[i].right;
                int trisCount = capTris.Length;
                if (doubleSided) trisCount *= 2;
                if(tsMesh.triangles.Length != trisCount) tsMesh.triangles = new int[trisCount];
                WriteTris(ref capTris, ref tsMesh.triangles, 0, 0, false);
            }
        }

        void GenerateCapTris(bool flip)
        {
            MeshUtility.Triangulate(projectedVerts, ref capTris);
            if (flip) MeshUtility.FlipTriangles(ref capTris);
        }

        int WriteTris(ref int[] tris, ref int[] target, int vertexOffset, int trisOffset, bool flip)
        {
            for (int i = trisOffset; i < trisOffset + tris.Length; i+=3)
            {
                if (flip)
                {
                    target[i] = tris[i+2 - trisOffset] + vertexOffset;
                    target[i + 1] = tris[i + 1 - trisOffset] + vertexOffset;
                    target[i + 2] = tris[i - trisOffset] + vertexOffset;
                }
                else
                {
                    target[i] = tris[i - trisOffset] + vertexOffset;
                    target[i+1] = tris[i+1 - trisOffset] + vertexOffset;
                    target[i+2] = tris[i+2 - trisOffset] + vertexOffset;
                }
            }
            return trisOffset + tris.Length;
        }

        bool IsClockwise(Vector2[] points2D)
        {
            float sum = 0f;
            for (int i = 1; i < points2D.Length; i++)
            {
                Vector2 v1 = points2D[i];
                Vector2 v2 = points2D[(i + 1) % points2D.Length];
                sum += (v2.x - v1.x) * (v2.y + v1.y);
            }
            sum += (points2D[0].x - points2D[points2D.Length - 1].x) * (points2D[0].y + points2D[points2D.Length - 1].y);
            return sum <= 0f;
        }

        void GetIdentityVerts(Vector3 center, Vector3 normal, bool clockwise)
        {
            Quaternion vertsRotation = Quaternion.Inverse(Quaternion.LookRotation(normal));
            if (identityVertices.Length != clippedSamples.Length)
            {
                identityVertices = new Vector3[clippedSamples.Length];
                identityNormals = new Vector3[clippedSamples.Length];
            }
            for (int i = 0; i < clippedSamples.Length; i++)
            {
                identityVertices[i] = vertsRotation * (clippedSamples[i].position - center + (clockwise ? -clippedSamples[i].right : clippedSamples[i].right) * _expand);
                identityNormals[i] = vertsRotation * (clockwise ? -clippedSamples[i].right : clippedSamples[i].right);
            }
        }

        void GetProjectedVertices(Vector3[] points, Vector3 normal, Vector3 center, int count = 0)
        {
            Quaternion rot = Quaternion.LookRotation(normal, Vector3.up);
            Vector3 up = rot * Vector3.up;
            Vector3 right = rot * Vector3.right;
            int length = count > 0 ? count : points.Length;
            if (projectedVerts.Length != length) projectedVerts = new Vector2[length];
            for (int i = 0; i < length; i++)
            {
                Vector3 point = points[i] - center;
                float projectionPointX = Vector3.Project(point, right).magnitude;
                if (Vector3.Dot(point, right) < 0.0f) projectionPointX *= -1f;
                float projectionPointY = Vector3.Project(point, up).magnitude;
                if (Vector3.Dot(point, up) < 0.0f) projectionPointY *= -1f;
                projectedVerts[i].x = projectionPointX;
                projectedVerts[i].y = projectionPointY;
            }
        }

    }
}
