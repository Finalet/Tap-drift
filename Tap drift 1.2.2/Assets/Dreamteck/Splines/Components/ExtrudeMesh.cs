using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Dreamteck.Splines
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Dreamteck/Splines/Extrude Mesh")]
    public class ExtrudeMesh : MeshGenerator
    {
        public enum Axis { X, Y, Z }
        public enum Iteration { Ordered, Random }
        public enum MirrorMethod { None, X, Y, Z }
        public enum TileUVs { None, U, V, UniformU, UniformV }

        public Axis axis
        {
            get { return _axis; }
            set
            {
                if (value != _axis)
                {
                    _axis = value;
                    UpdateExtrudableMeshes();
                    Rebuild(false);
                }
            }
        }

        public Iteration iteration
        {
            get { return _iteration; }
            set
            {
                if (value != _iteration)
                {
                    _iteration = value;
                    UpdateExtrudableMeshes();
                    Rebuild(false);
                }
            }
        }

        public int randomSeed
        {
            get { return _randomSeed; }
            set
            {
                if (value != _randomSeed)
                {
                    _randomSeed = value;
                    if (_iteration == Iteration.Random)
                    {
                        UpdateExtrudableMeshes();
                        Rebuild(false);
                    }
                }
            }
        }

        public int repeat
        {
            get { return _repeat; }
            set
            {
                if (value != _repeat)
                {
                    _repeat = value;
                    if (_repeat < 1) _repeat = 1;
                    UpdateEndExtrudeMesh();
                    Rebuild(false);
                }
            }
        }

        public bool dontStretchCaps
        {
            get { return _dontStretchCaps; }
            set
            {
                if (value != _dontStretchCaps)
                {
                    _dontStretchCaps = value;
                    Rebuild(false);
                }
            }
        }

        public TileUVs tileUVs
        {
            get { return _tileUVs; }
            set
            {
                if (value != _tileUVs)
                {
                    _tileUVs = value;
                    Rebuild(false);
                }
            }
        }

        public double spacing
        {
            get { return _spacing; }
            set
            {
                if (value != _spacing)
                {
                    if ((_spacing == 0f && value > 0f) || (value == 0f && _spacing > 0f)) UpdateExtrudableMeshes();
                    _spacing = value;
                    Rebuild(false);
                }
            }
        }

        public Vector2 scale
        {
            get { return _scale; }
            set
            {
                if (value != _scale)
                {
                    _scale = value;
                    Rebuild(false);
                }
            }
        }

        //Mesh data
        [SerializeField]
        [HideInInspector]
        private Mesh _startMesh = null;
        [SerializeField]
        [HideInInspector]
        private Mesh _endMesh = null;
        [SerializeField]
        [HideInInspector]
        private bool _dontStretchCaps = false;
        [SerializeField]
        [HideInInspector]
        private TileUVs _tileUVs = TileUVs.None;
        [SerializeField]
        [HideInInspector]
        private Mesh[] _middleMeshes = new Mesh[0];
        [SerializeField]
        [HideInInspector]
        private List<ExtrudableMesh> extrudableMeshes = new List<ExtrudableMesh>();
        [SerializeField]
        [HideInInspector]
        private Axis _axis = Axis.Z;
        [SerializeField]
        [HideInInspector]
        private Iteration _iteration = Iteration.Ordered;
        [SerializeField]
        [HideInInspector]
        private int _randomSeed = 0;
        [SerializeField]
        [HideInInspector]
        private int _repeat = 1;
        [SerializeField]
        [HideInInspector]
        private double _spacing = 0.0;
        [SerializeField]
        [HideInInspector]
        private Vector2 _scale = Vector2.one;
        private SplineResult lastResult = new SplineResult();
        private bool useLastResult = false;
        private List<TS_Mesh> combineMeshes = new List<TS_Mesh>();
        private System.Random random;
        private int iterations = 0;
        private bool _hasAnyMesh = false;
        private bool _hasStartMesh = false;
        private bool _hasEndMesh = false;

        Matrix4x4 vertexMatrix = new Matrix4x4();
        Matrix4x4 normalMatrix = new Matrix4x4();

        public bool hasAnyMesh
        {
            get { return _hasAnyMesh; }
        }

#if UNITY_EDITOR
        public override void EditorAwake()
        {
            UpdateExtrudableMeshes();
            CheckMeshes();
            base.EditorAwake();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            CheckMeshes();
            mesh.name = "Stretch Mesh";
        }

        public Mesh GetStartMesh()
        {
            return _startMesh;
        }

        public Mesh GetEndMesh()
        {
            return _endMesh;
        }

        public MirrorMethod GetStartMeshMirror()
        {
            if (extrudableMeshes.Count == 0) return MirrorMethod.None;
            if (extrudableMeshes[0] == null) return MirrorMethod.None;
            return extrudableMeshes[0].mirror;
        }

        public MirrorMethod GetEndMeshMirror()
        {
            if (extrudableMeshes.Count < 2) return MirrorMethod.None;
            if (extrudableMeshes[1] == null) return MirrorMethod.None;
            return extrudableMeshes[1].mirror;
        }

        public void SetStartMeshMirror(MirrorMethod mirror)
        {
            if (extrudableMeshes.Count == 0) return;
            if (extrudableMeshes[0] == null) return;
            extrudableMeshes[0].mirror = mirror;
            CheckMeshes();
            Rebuild(false);
        }

        public void SetEndMeshMirror(MirrorMethod mirror)
        {
            if (extrudableMeshes.Count < 2) return;
            if (extrudableMeshes[1] == null) return; 
            extrudableMeshes[1].mirror = mirror;
            CheckMeshes();
            Rebuild(false);
        }

        public void SetMeshMirror(int index, MirrorMethod mirror)
        {
            if (extrudableMeshes.Count < 2 + index) return;
            if (extrudableMeshes[2 + index] == null) return;
            extrudableMeshes[2 + index].mirror = mirror;
            CheckMeshes();
            Rebuild(false);
        }

        public void SetStartMesh(Mesh inputMesh, MirrorMethod mirror = MirrorMethod.None)
        {
            _startMesh = inputMesh;
            if (extrudableMeshes.Count == 0) extrudableMeshes.Add(null);
            if(_startMesh == null)
            {
                extrudableMeshes[0] = null;
                _hasStartMesh = false;
                Rebuild(false);
                return;
            }
            extrudableMeshes[0] = new ExtrudableMesh(_startMesh, _axis);
            extrudableMeshes[0].mirror = mirror;
            CheckMeshes();
            Rebuild(false);
        }

        public void SetEndMesh(Mesh inputMesh, MirrorMethod mirror = MirrorMethod.None)
        {
            _endMesh = inputMesh;
            if (extrudableMeshes.Count < 2) extrudableMeshes.AddRange(new ExtrudableMesh[2-extrudableMeshes.Count]);
            if (_endMesh == null)
            {
                extrudableMeshes[1] = null;
                _hasEndMesh = false;
                Rebuild(false);
                return;
            }
            extrudableMeshes[1] = new ExtrudableMesh(_endMesh, _axis);
            extrudableMeshes[1].mirror = mirror;
            CheckMeshes();
            Rebuild(false);
        }

        public Mesh GetMesh(int index)
        {
            return _middleMeshes[index];
        }

        public MirrorMethod GetMeshMirror(int index)
        {
            if (extrudableMeshes[index+2] == null) return MirrorMethod.None;
            return extrudableMeshes[index+2].mirror;
        }

        public void SetMesh(int index, Mesh inputMesh, MirrorMethod mirror = MirrorMethod.None)
        {
            if (inputMesh == null)
            {
                RemoveMesh(index);
                return;
            }
            _middleMeshes[index] = inputMesh;
            UpdateExtrudableMeshes();
            extrudableMeshes[2 + index].mirror = mirror;
            CheckMeshes();
            Rebuild(false);
        }

        public void RemoveMesh(int index)
        {
            Mesh[] newMeshes = new Mesh[_middleMeshes.Length - 1];
            for (int i = 0; i < _middleMeshes.Length; i++)
            {
                if (i < index) newMeshes[i] = _middleMeshes[i];
                else if (i > index) newMeshes[i - 1] = _middleMeshes[i];
            }
            _middleMeshes = newMeshes;
            extrudableMeshes.RemoveAt(index+2);
            UpdateExtrudableMeshes();
            CheckMeshes();
            Rebuild(false);
        }

        public void AddMesh(Mesh inputMesh)
        {
            if (inputMesh == null) return;
            Mesh[] newMeshes = new Mesh[_middleMeshes.Length + 1];
            _middleMeshes.CopyTo(newMeshes, 0);
            newMeshes[newMeshes.Length - 1] = inputMesh;
            _middleMeshes = newMeshes;
            UpdateExtrudableMeshes();
            CheckMeshes();
            Rebuild(false);
        }

        void CheckMeshes()
        {
            _hasAnyMesh = false;
            _hasStartMesh = false;
            _hasEndMesh = false;
            if (_startMesh != null)
            {
                _hasAnyMesh = true;
                _hasStartMesh = true;
            }
            for(int i = 0; i < _middleMeshes.Length; i++)
            {
                if (_middleMeshes[i] != null)
                {
                    _hasAnyMesh = true;
                    break;
                }
            }
            if (_endMesh != null)
            {
                _hasAnyMesh = true;
                _hasEndMesh = true;
            }
        }

        public int GetMeshCount()
        {
            return _middleMeshes.Length;
        }

        protected override void BuildMesh()
        {
            if (clippedSamples.Length == 0) return;
            base.BuildMesh();
            if (!_hasAnyMesh && !multithreaded)
            {
                CheckMeshes();
                UpdateExtrudableMeshes();
                if(!_hasAnyMesh) return;
            }
            Generate();
        }

        void Generate()
        {
            random = new System.Random(_randomSeed);
            useLastResult = false;
            iterations = 0;
            if (_hasStartMesh) iterations++;
            if (_hasEndMesh) iterations++;
            iterations += (extrudableMeshes.Count-2) * _repeat;
            double step = 1.0 / iterations;
            double space = step * _spacing * 0.5;
            if (combineMeshes.Count < iterations) combineMeshes.AddRange(new TS_Mesh[iterations - combineMeshes.Count]);
            else if (combineMeshes.Count > iterations) combineMeshes.RemoveRange((combineMeshes.Count - 1) - (combineMeshes.Count - iterations), combineMeshes.Count - iterations);

            for (int i = 0; i < iterations; i++)
            {
                double from = i * step + space;
                double to = i * step + step - space;
                if(combineMeshes[i] == null) combineMeshes[i] = new TS_Mesh();
                Stretch(extrudableMeshes[GetMeshIndex(i)], combineMeshes[i], from, to);
                if (_spacing == 0f) useLastResult = true;
            }


            if (_dontStretchCaps)
            {
                if(_hasStartMesh)
                {
                    TS_Mesh addMesh = new TS_Mesh();
                    TRS(extrudableMeshes[0], addMesh, 0.0);
                    combineMeshes.Add(addMesh);
                }
                if (_hasEndMesh)
                {
                    TS_Mesh addMesh = new TS_Mesh();
                    TRS(extrudableMeshes[1], addMesh, 1.0);
                    combineMeshes.Add(addMesh);
                }
            }
            if (tsMesh == null) tsMesh = new TS_Mesh();
            tsMesh.Combine(combineMeshes, true);
        }

        /// <summary>
        /// Used to get the index of the extrudableMesh at the given repeat iteration
        /// </summary>
        /// <param name="repeatIndex"></param>
        /// <returns></returns>
        private int GetMeshIndex(int repeatIndex)
        {
            if (repeatIndex == 0 && _hasStartMesh && !_dontStretchCaps) return 0;
            if (repeatIndex == iterations - 1 && _hasEndMesh && !_dontStretchCaps) return 1;
            if (_middleMeshes.Length == 0)
            {
                if (_hasStartMesh && !_dontStretchCaps) return 0;
                else if (_hasEndMesh && !_dontStretchCaps) return 1;
            }
            if (_middleMeshes.Length == 1) return 2;
            if (_iteration == Iteration.Random) return 2 + random.Next(_middleMeshes.Length);
            return 2 + (repeatIndex - (_hasStartMesh ? 0 : 1)) % _middleMeshes.Length; //this might create a problem
        }

        private void TRS(ExtrudableMesh source, TS_Mesh target, double percent)
        {
            CreateTSFromExtrudableMesh(source, ref target);
            SplineResult result = Evaluate(percent);

            Quaternion axisRotation = Quaternion.identity;
            switch (axis)
            {
                case Axis.X: axisRotation = Quaternion.LookRotation(Vector3.right); break;
                case Axis.Y: axisRotation = Quaternion.LookRotation(Vector3.up, Vector3.back); break;
            }
            vertexMatrix.SetTRS(result.position + result.right * offset.x + result.normal * offset.y + result.direction * offset.z, result.rotation * Quaternion.AngleAxis(rotation, Vector3.forward) * axisRotation, new Vector3(_scale.x, _scale.y, 1f) * result.size);
            normalMatrix = vertexMatrix.inverse.transpose;
            for (int i = 0; i < target.vertexCount; i++)
            {
                target.vertices[i] = vertexMatrix.MultiplyPoint3x4(source.vertices[i]);
                target.normals[i] = normalMatrix.MultiplyVector(source.normals[i]);
            }
        }

        private void CreateTSFromExtrudableMesh(ExtrudableMesh source, ref TS_Mesh target)
        {
            if (target.vertices.Length != source.vertices.Length) target.vertices = new Vector3[source.vertices.Length];
            if (target.normals.Length != source.normals.Length) target.normals = new Vector3[source.normals.Length];
            if (target.tangents.Length != source.tangents.Length) target.tangents = new Vector4[source.tangents.Length];
            if (target.colors.Length != source.colors.Length) target.colors = new Color[source.colors.Length];
            if (target.uv.Length != source.uv.Length) target.uv = new Vector2[source.uv.Length];
            source.uv.CopyTo(target.uv, 0);
            if (target.uv.Length != target.vertices.Length)
            {
                Vector2[] newUv = new Vector2[target.vertices.Length];
                for (int i = 0; i < target.vertices.Length; i++)
                {
                    if (i < target.uv.Length) newUv[i] = target.uv[i];
                    else newUv[i] = Vector2.zero;
                }
                target.uv = newUv;
            }
            source.colors.CopyTo(target.colors, 0);
            target.subMeshes.Clear();
            for (int n = 0; n < source.subMeshes.Count; n++) target.subMeshes.Add(source.subMeshes[n].triangles);
        }

        private void Stretch(ExtrudableMesh source, TS_Mesh target, double from, double to)
        {
            CreateTSFromExtrudableMesh(source, ref target);
            SplineResult result = new SplineResult();
            Vector2 uv = Vector2.zero;
            Vector3 trsVector = Vector3.zero;
            Matrix4x4 trsMatrix = new Matrix4x4();
            Quaternion axisRotation = Quaternion.identity;
            switch (axis)
            {
                case Axis.X: axisRotation = Quaternion.LookRotation(Vector3.left); break;
                case Axis.Y: axisRotation = Quaternion.LookRotation(Vector3.up, Vector3.forward); break;
            }

            for (int i = 0; i < source.vertexGroups.Count; i++)
            {
                double evalPercent = 0.0;
                switch (axis)
                {
                    case Axis.X: evalPercent = DMath.Clamp01(Mathf.InverseLerp(source.bounds.min.x, source.bounds.max.x, source.vertexGroups[i].value)); break;
                    case Axis.Y: evalPercent = DMath.Clamp01(Mathf.InverseLerp(source.bounds.min.y, source.bounds.max.y, source.vertexGroups[i].value)); break;
                    case Axis.Z: evalPercent = DMath.Clamp01(Mathf.InverseLerp(source.bounds.min.z, source.bounds.max.z, source.vertexGroups[i].value)); break;
                }

                if (useLastResult && i == source.vertexGroups.Count) result = lastResult;
                else Evaluate(result, UnclipPercent(DMath.Lerp(from, to, evalPercent)));
                trsMatrix.SetTRS(result.position + result.right * offset.x + result.normal * offset.y + result.direction * offset.z, result.rotation * Quaternion.AngleAxis(rotation, Vector3.forward), new Vector3(_scale.x, _scale.y, 1f) * result.size);
                if (i == 0) lastResult.CopyFrom(result);

                for (int n = 0; n < source.vertexGroups[i].ids.Length; n++)
                {
                    int index = source.vertexGroups[i].ids[n];
                    trsVector = axisRotation * source.vertices[index];
                    trsVector.z = 0f;
                    target.vertices[index] = trsMatrix.MultiplyPoint3x4(trsVector);
                    trsVector = axisRotation * source.normals[index];
                    target.normals[index] = trsMatrix.MultiplyVector(trsVector);
                    target.colors[index] = target.colors[index] * result.color;
                    uv = target.uv[index];
                    switch (_tileUVs)
                    {
                        case TileUVs.U: uv.x = (float)result.percent; break;
                        case TileUVs.V: uv.y = (float)result.percent; break;
                        case TileUVs.UniformU: uv.x = CalculateLength(0.0, result.percent); break;
                        case TileUVs.UniformV: uv.y = CalculateLength(0.0, result.percent); break;
                    }
                    target.uv[index] = new Vector2(uv.x * uvScale.x, uv.y * uvScale.y);
                    target.uv[index] += uvOffset;
                }
            }
        }

        /// <summary>
        /// Creates the mesh array that will be referenced when extruding the meshes. Elements 0 and 1 are the start and end cap meshes
        /// </summary>
        void UpdateExtrudableMeshes()
        {
            iterations = 0;
            if (_startMesh != null) iterations++;
            if (_endMesh != null) iterations++;
            iterations += (extrudableMeshes.Count - 2) * _repeat;
            int targetCount = 2 + _middleMeshes.Length;
            if (extrudableMeshes.Count < targetCount) extrudableMeshes.AddRange(new ExtrudableMesh[targetCount - extrudableMeshes.Count]);

            for (int i = 0; i < _middleMeshes.Length; i++)
            {
                if (_middleMeshes[i] == null)
                {
                    RemoveMesh(i);
                    i--;
                    continue;
                }
                MirrorMethod lastMirror = MirrorMethod.None;
                if (extrudableMeshes[i + 2] != null)
                {
                    lastMirror = extrudableMeshes[i + 2].mirror;
                    extrudableMeshes[i + 2].Update(_middleMeshes[i], _axis);
                } else extrudableMeshes[i + 2] = new ExtrudableMesh(_middleMeshes[i], _axis);
                extrudableMeshes[i+2].mirror = lastMirror;
            }
            UpdateStartExtrudeMesh();
            UpdateEndExtrudeMesh();
        }

        void UpdateStartExtrudeMesh()
        {
            MirrorMethod lastMirror = MirrorMethod.None;
            if (extrudableMeshes[0] != null) lastMirror = extrudableMeshes[0].mirror;
            if (_startMesh != null) extrudableMeshes[0] = new ExtrudableMesh(_startMesh, _axis);
            else if (_middleMeshes.Length > 0)
            {
                if (_iteration == Iteration.Ordered) extrudableMeshes[0] = new ExtrudableMesh(_middleMeshes[0], _axis);
                else
                {
                    random = new System.Random(_randomSeed);
                    extrudableMeshes[0] = new ExtrudableMesh(_middleMeshes[random.Next(_middleMeshes.Length - 1)], _axis);
                }
            }
            if (extrudableMeshes[0] != null) extrudableMeshes[0].mirror = lastMirror;
        }

        void UpdateEndExtrudeMesh()
        {
            MirrorMethod lastMirror = MirrorMethod.None;
            lastMirror = MirrorMethod.None;
            if (extrudableMeshes[1] != null) lastMirror = extrudableMeshes[1].mirror;
            if (_endMesh != null) extrudableMeshes[1] = new ExtrudableMesh(_endMesh, _axis);
            else if (_middleMeshes.Length > 0)
            {
                if (_iteration == Iteration.Ordered) extrudableMeshes[1] = new ExtrudableMesh(_middleMeshes[_startMesh != null ? (iterations - 2) % _middleMeshes.Length : (iterations - 1) % _middleMeshes.Length], _axis);
                else
                {
                    random = new System.Random(_randomSeed);
                    for (int i = 0; i < iterations - 1; i++) random.Next(_middleMeshes.Length - 1);
                    extrudableMeshes[1] = new ExtrudableMesh(_middleMeshes[random.Next(_middleMeshes.Length - 1)], _axis);
                }
            }
            if (extrudableMeshes[1] != null) extrudableMeshes[1].mirror = lastMirror;
        }

        //Internal classes
        [System.Serializable]
        internal class ExtrudableMesh
        {
            [System.Serializable]
            public class VertexGroup
            {
                public float value;
                public int[] ids;

                public VertexGroup(float val, int[] vertIds)
                {
                    value = val;
                    ids = vertIds;
                }

                public void AddId(int id)
                {
                    int[] newIds = new int[ids.Length + 1];
                    ids.CopyTo(newIds, 0);
                    newIds[newIds.Length - 1] = id;
                    ids = newIds;
                }
            }
            [System.Serializable]
            public class Submesh
            {
                public int[] triangles = new int[0];

                public Submesh()
                {

                }

                public Submesh(int[] input)
                {
                    triangles = new int[input.Length];
                    input.CopyTo(triangles, 0);
                }
            }

            public Vector3[] vertices = new Vector3[0];
            public Vector3[] normals = new Vector3[0];
            public Vector4[] tangents = new Vector4[0];
            public Color[] colors = new Color[0];
            public Vector2[] uv = new Vector2[0];
            public List<Submesh> subMeshes = new List<Submesh>();
            public TS_Bounds bounds = new TS_Bounds(Vector3.zero, Vector3.zero);
            public List<VertexGroup> vertexGroups = new List<VertexGroup>();

            public MirrorMethod mirror
            {
                get { return _mirror;  }
                set
                {
                    if(_mirror != value)
                    {
                        Mirror(_mirror);
                        _mirror = value;
                        Mirror(_mirror);
                    }
                }
            }

            [SerializeField]
            private MirrorMethod _mirror = MirrorMethod.None;
            [SerializeField]
            private Axis _axis = Axis.Z;

            public ExtrudableMesh()
            {
                vertices = new Vector3[0];
                normals = new Vector3[0];
                tangents = new Vector4[0];
                colors = new Color[0];
                uv = new Vector2[0];
                subMeshes = new List<Submesh>();
                bounds = new TS_Bounds(Vector3.zero, Vector3.zero);
                vertexGroups = new List<VertexGroup>();
            }

            public ExtrudableMesh(Mesh inputMesh, Axis axis)
            {
                Update(inputMesh, axis);
            }

            public void Update(Mesh inputMesh, Axis axis)
            {
                vertices = inputMesh.vertices;
                normals = inputMesh.normals;
                tangents = inputMesh.tangents;
                colors = inputMesh.colors;
                if(colors.Length != vertices.Length)
                {
                    colors = new Color[vertices.Length];
                    for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
                }
                uv = inputMesh.uv;
                bounds = new TS_Bounds(inputMesh.bounds);
                subMeshes.Clear();
                for (int i = 0; i < inputMesh.subMeshCount; i++) subMeshes.Add(new Submesh(inputMesh.GetTriangles(i)));
                _axis = axis;
                Mirror(_mirror);
                GroupVertices(axis);
            }

            private void Mirror(MirrorMethod method)
            {
                if (method == MirrorMethod.None) return;
                switch (method)
                {
                    case MirrorMethod.X:
                        for(int i = 0; i < vertices.Length; i++)
                        {
                            float percent = Mathf.InverseLerp(bounds.min.x, bounds.max.x, vertices[i].x);
                            vertices[i].x = Mathf.Lerp(bounds.min.x, bounds.max.x, 1f - percent);
                            normals[i].x = -normals[i].x;
                        }
                        if (_axis == Axis.X)
                        {
                            for (int i = 0; i < vertexGroups.Count; i++)
                            {
                                float percent = Mathf.InverseLerp(bounds.min.x, bounds.max.x, vertexGroups[i].value);
                                vertexGroups[i].value = Mathf.Lerp(bounds.min.x, bounds.max.x, 1f - percent);
                            }
                        }
                        break;
                    case MirrorMethod.Y:
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            float percent = Mathf.InverseLerp(bounds.min.y, bounds.max.y, vertices[i].y);
                            vertices[i].y = Mathf.Lerp(bounds.min.y, bounds.max.y, 1f - percent);
                            normals[i].y = -normals[i].y;
                        }
                        if (_axis == Axis.Y)
                        {
                            for (int i = 0; i < vertexGroups.Count; i++)
                            {
                                float percent = Mathf.InverseLerp(bounds.min.y, bounds.max.y, vertexGroups[i].value);
                                vertexGroups[i].value = Mathf.Lerp(bounds.min.y, bounds.max.y, 1f - percent);
                            }
                        }
                        break;
                    case MirrorMethod.Z:
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            float percent = Mathf.InverseLerp(bounds.min.z, bounds.max.z, vertices[i].z);
                            vertices[i].z = Mathf.Lerp(bounds.min.z, bounds.max.z, 1f - percent);
                            normals[i].z = -normals[i].z;
                        }
                        if (_axis == Axis.Z)
                        {
                            for (int i = 0; i < vertexGroups.Count; i++)
                            {
                                float percent = Mathf.InverseLerp(bounds.min.z, bounds.max.z, vertexGroups[i].value);
                                vertexGroups[i].value = Mathf.Lerp(bounds.min.z, bounds.max.z, 1f - percent);
                            }
                        }
                        break;
                }
                for (int i = 0; i < subMeshes.Count; i++)
                {
                    for (int n = 0; n < subMeshes[i].triangles.Length; n += 3)
                    {
                        int temp = subMeshes[i].triangles[n];
                        subMeshes[i].triangles[n] = subMeshes[i].triangles[n + 2];
                        subMeshes[i].triangles[n + 2] = temp;
                    }
                }
                CalculateTangents();
            }

            void GroupVertices(Axis axis)
            {
                vertexGroups = new List<VertexGroup>();
                int ax = (int)axis;
                if (ax > 2) ax -= 2;
                for (int i = 0; i < vertices.Length; i++)
                {
                    float value = 0f;
                    switch (ax)
                    {
                        case 0: value = vertices[i].x; break;
                        case 1: value = vertices[i].y; break;
                        case 2: value = vertices[i].z; break;
                    }
                    int index = FindInsertIndex(vertices[i], value);
                    if (index >= vertexGroups.Count) vertexGroups.Add(new VertexGroup(value, new int[] { i }));
                    else
                    {
                        if (Mathf.Approximately(vertexGroups[index].value, value)) vertexGroups[index].AddId(i);
                        else if (vertexGroups[index].value < value) vertexGroups.Insert(index, new VertexGroup(value, new int[] { i }));
                        else
                        {
                            if (index < vertexGroups.Count - 1) vertexGroups.Insert(index + 1, new VertexGroup(value, new int[] { i }));
                            else vertexGroups.Add(new VertexGroup(value, new int[] { i }));
                        }
                    }
                }
            }

            int FindInsertIndex(Vector3 pos, float value)
            {
                int lower = 0;
                int upper = vertexGroups.Count - 1;

                while (lower <= upper)
                {
                    int middle = lower + (upper - lower) / 2;
                    if (vertexGroups[middle].value == value) return middle;
                    else if (vertexGroups[middle].value < value) upper = middle - 1;
                    else lower = middle + 1;
                }
                return lower;
            }

            void CalculateTangents()
            {
                if (vertices.Length == 0)
                {
                    tangents = new Vector4[0];
                    return;
                }
                tangents = new Vector4[vertices.Length];
                Vector3[] tan1 = new Vector3[vertices.Length];
                Vector3[] tan2 = new Vector3[vertices.Length];
                for (int i = 0; i < subMeshes.Count; i++)
                {
                    for (int n = 0; n < subMeshes[i].triangles.Length; n += 3)
                    {
                        int i1 = subMeshes[i].triangles[n];
                        int i2 = subMeshes[i].triangles[n + 1];
                        int i3 = subMeshes[i].triangles[n + 2];
                        float x1 = vertices[i2].x - vertices[i1].x;
                        float x2 = vertices[i3].x - vertices[i1].x;
                        float y1 = vertices[i2].y - vertices[i1].y;
                        float y2 = vertices[i3].y - vertices[i1].y;
                        float z1 = vertices[i2].z - vertices[i1].z;
                        float z2 = vertices[i3].z - vertices[i1].z;
                        float s1 = uv[i2].x - uv[i1].x;
                        float s2 = uv[i3].x - uv[i1].x;
                        float t1 = uv[i2].y - uv[i1].y;
                        float t2 = uv[i3].y - uv[i1].y;
                        float div = s1 * t2 - s2 * t1;
                        float r = div == 0f ? 0f : 1f / div;
                        Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                        Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
                        tan1[i1] += sdir;
                        tan1[i2] += sdir;
                        tan1[i3] += sdir;
                        tan2[i1] += tdir;
                        tan2[i2] += tdir;
                        tan2[i3] += tdir;
                    }
                }
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 n = normals[i];
                    Vector3 t = tan1[i];
                    Vector3.OrthoNormalize(ref n, ref t);
                    tangents[i].x = t.x;
                    tangents[i].y = t.y;
                    tangents[i].z = t.z;
                    tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
                }
            }
        }

    }
}
