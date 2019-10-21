using UnityEngine;
using System.Collections;
using System.Threading;
namespace Dreamteck.Splines
{
    [AddComponentMenu("Dreamteck/Splines/Polygon Collider Generator")]
    [RequireComponent(typeof(PolygonCollider2D))]
    public class PolygonColliderGenerator : SplineUser
    {
        public enum Type { Path, Shape }
        public Type type
        {
            get
            {
                return _type;
            }
            set
            {
                if (value != _type)
                {
                    _type = value;
                    Rebuild(false);
                }
            }
        }

        public float size
        {
            get { return _size; }
            set
            {
                if (value != _size)
                {
                    _size = value;
                    Rebuild(false);
                }
            }
        }

        public float offset
        {
            get { return _offset; }
            set
            {
                if (value != _offset)
                {
                    _offset = value;
                    Rebuild(false);
                }
            }
        }
        [SerializeField]
        [HideInInspector]
        private Type _type = Type.Path;
        [SerializeField]
        [HideInInspector]
        private float _size = 1f;
        [SerializeField]
        [HideInInspector]
        private float _offset = 0f;
        [SerializeField]
        [HideInInspector]
        protected PolygonCollider2D polygonCollider;

        [SerializeField]
        [HideInInspector]
        protected Vector2[] vertices = new Vector2[0];

        [HideInInspector]
        public float updateRate = 0.1f;
        protected float lastUpdateTime = 0f;

        private bool updateCollider = false;

#if UNITY_EDITOR
        public override void EditorAwake()
        {
            base.EditorAwake();
            polygonCollider = GetComponent<PolygonCollider2D>();
            Awake();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            polygonCollider = GetComponent<PolygonCollider2D>();
        }


        protected override void Reset()
        {
            base.Reset();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void LateRun()
        {
            base.LateRun();
            if (updateCollider)
            {
                if (polygonCollider != null)
                {
                    if (Time.time - lastUpdateTime >= updateRate)
                    {
                        lastUpdateTime = Time.time;
                        updateCollider = false;
                        polygonCollider.SetPath(0, vertices);
                    }
                }
            }
        }

        protected override void Build()
        {
            base.Build();
            if (clippedSamples.Length == 0) return;
            switch(type){
                case Type.Path:
                GeneratePath();
                break;
                case Type.Shape: GenerateShape(); break;
            }

        }

        protected override void PostBuild()
        {
            base.PostBuild();
            if (polygonCollider == null) return;
            for(int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = this.transform.InverseTransformPoint(vertices[i]);
            }
#if UNITY_EDITOR
            if (!Application.isPlaying || updateRate <= 0f) polygonCollider.SetPath(0, vertices);
            else updateCollider = true;
#else
            if(updateRate == 0f) polygonCollider.SetPath(0, vertices);
            else updateCollider = true;
#endif
        }

        private void GeneratePath()
        {
            int vertexCount = clippedSamples.Length * 2;
            if (vertices.Length != vertexCount) vertices = new Vector2[vertexCount];
            for (int i = 0; i < clippedSamples.Length; i++)
            {
                Vector2 right = new Vector2(-clippedSamples[i].direction.y, clippedSamples[i].direction.x).normalized * clippedSamples[i].size;
                vertices[i] = new Vector2(clippedSamples[i].position.x, clippedSamples[i].position.y) + right * size * 0.5f + right * offset;
                vertices[clippedSamples.Length + (clippedSamples.Length - 1) - i] = new Vector2(clippedSamples[i].position.x, clippedSamples[i].position.y) - right * size * 0.5f + right * offset;
            }
        }

        private void GenerateShape()
        {
            if (vertices.Length != clippedSamples.Length) vertices = new Vector2[clippedSamples.Length];
            for (int i = 0; i < clippedSamples.Length; i++)
            {
                vertices[i] = clippedSamples[i].position;
                if (offset != 0f)
                {
                    Vector2 right = new Vector2(-clippedSamples[i].direction.y, clippedSamples[i].direction.x).normalized * clippedSamples[i].size;
                    vertices[i] += right * offset;
                }
            }
        }
    }

  
}
