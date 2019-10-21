using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dreamteck.Splines
{
    [AddComponentMenu("Dreamteck/Splines/Object Controller")]
    public class ObjectController : SplineUser
    {
        [System.Serializable]
        internal class ObjectControl
        {
            public bool isNull
            {
                get
                {
                    return gameObject == null;
                }
            }
            public Transform transform
            {
                get {
                    if (gameObject == null) return null;
                    return gameObject.transform;  
                }
            }
            public GameObject gameObject;
            public Vector3 position = Vector3.zero;
            public Quaternion rotation = Quaternion.identity;
            public Vector3 scale = Vector3.one;
            public bool active = true;

            public Vector3 baseScale = Vector3.one;

            public ObjectControl(GameObject input)
            {
                gameObject = input;
                baseScale = gameObject.transform.localScale;
            }

            public void Destroy()
            {
                if (gameObject == null) return;
                GameObject.Destroy(gameObject);
            }

            public void DestroyImmediate()
            {
                if (gameObject == null) return;
                GameObject.DestroyImmediate(gameObject);
            }

            public void Apply()
            {
                if (gameObject == null) return;
                transform.position = position;
                transform.rotation = rotation;
                transform.localScale = scale;
                gameObject.SetActive(active);
            }

        }

        public enum ObjectMethod { Instantiate, GetChildren }
        public enum Positioning { Stretch, Clip }
        public enum Iteration { Ordered, Random }

        [SerializeField]
        [HideInInspector]
        public GameObject[] objects = new GameObject[0];

        public ObjectMethod objectMethod
        {
            get { return _objectMethod; }
            set
            {
                if (value != _objectMethod)
                {
                    if (value == ObjectMethod.GetChildren)
                    {
                        _objectMethod = value;
                        Spawn();
                    }
                    else _objectMethod = value;
                }
            }
        }

        public int spawnCount
        {
            get { return _spawnCount; }
            set
            {
                if (value != _spawnCount)
                {
                    if (value < 0) value = 0;
                    if (_objectMethod == ObjectMethod.Instantiate)
                    {
                        if (value < _spawnCount)
                        {
                            _spawnCount = value;
                            Remove();
                        }
                        else
                        {
                            _spawnCount = value;
                            Spawn();
                        }
                    }
                    else _spawnCount = value;
                }
            }
        }

        public Positioning objectPositioning
        {
            get { return _objectPositioning; }
            set
            {
                if (value != _objectPositioning)
                {
                    _objectPositioning = value;
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
                    Rebuild(false);
                }
            }
        }

#if UNITY_EDITOR
        public bool retainPrefabInstancesInEditor
        {
            get { return _retainPrefabInstancesInEditor; }
            set
            {
                if (value != _retainPrefabInstancesInEditor)
                {
                    _retainPrefabInstancesInEditor = value;
                    Clear();
                    Spawn();
                    Rebuild(false);
                }
            }
        }
#endif

        public int randomSeed
        {
            get { return _randomSeed; }
            set
            {
                if (value != _randomSeed)
                {
                    _randomSeed = value;
                    Rebuild(false);
                }
            }
        }

        public Vector2 offset
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

        public Vector3 minRotationOffset
        {
            get { return _minRotationOffset; }
            set
            {
                if (value != _minRotationOffset)
                {
                    _minRotationOffset = value;
                    Rebuild(false);
                }
            }
        }

        public Vector3 maxRotationOffset
        {
            get { return _maxRotationOffset; }
            set
            {
                if (value != _maxRotationOffset)
                {
                    _maxRotationOffset = value;
                    Rebuild(false);
                }
            }
        }

        public Vector3 rotationOffset
        {
            get { return (_maxRotationOffset+_minRotationOffset)/2f; }
            set
            {
                if (value != _minRotationOffset || value != _maxRotationOffset)
                {
                    _minRotationOffset = _maxRotationOffset = value;
                    Rebuild(false);
                }
            }
        }

        public Vector3 minScaleMultiplier
        {
            get { return _minScaleMultiplier; }
            set
            {
                if (value != _minScaleMultiplier)
                {
                    _minScaleMultiplier = value;
                    Rebuild(false);
                }
            }
        }

        public Vector3 maxScaleMultiplier
        {
            get { return _maxScaleMultiplier; }
            set
            {
                if (value != _maxScaleMultiplier)
                {
                    _maxScaleMultiplier = value;
                    Rebuild(false);
                }
            }
        }

        public Vector3 scaleMultiplier
        {
            get { return (_minScaleMultiplier + _maxScaleMultiplier) / 2f; }
            set
            {
                if (value != _minScaleMultiplier || value != _maxScaleMultiplier)
                {
                    _minScaleMultiplier = _maxScaleMultiplier = value;
                    Rebuild(false);
                }
            }
        }

        public bool randomizeOffset
        {
            get { return _randomizeOffset; }
            set
            {
                if (value != _randomizeOffset)
                {
                    _randomizeOffset = value;
                    Rebuild(false);
                }
            }
        }

        public bool useRandomOffsetRotation
        {
            get { return _useRandomOffsetRotation; }
            set
            {
                if (value != _useRandomOffsetRotation)
                {
                    _useRandomOffsetRotation = value;
                    Rebuild(false);
                }
            }
        }

        public bool shellOffset
        {
            get { return _shellOffset; }
            set
            {
                if (value != _shellOffset)
                {
                    _shellOffset = value;
                    Rebuild(false);
                }
            }
        }

        public bool randomOffset
        {
            get { return _randomOffset; }
            set
            {
                if (value != _randomOffset)
                {
                    _randomOffset = value;
                    Rebuild(false);
                }
            }
        }

        public bool applyRotation
        {
            get { return _applyRotation; }
            set
            {
                if (value != _applyRotation)
                {
                    _applyRotation = value;
                    Rebuild(false);
                }
            }
        }

        public bool applyScale
        {
            get { return _applyScale; }
            set
            {
                if (value != _applyScale)
                {
                    _applyScale = value;
                    Rebuild(false);
                }
            }
        }

        public Vector2 randomSize
        {
            get { return _randomSize; }
            set
            {
                if (value != _randomSize)
                {
                    _randomSize = value;
                    Rebuild(false);
                }
            }
        }

        public float positionOffset
        {
            get { return _positionOffset; }
            set
            {
                if (value != _positionOffset)
                {
                    _positionOffset = value;
                    Rebuild(false);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private float _positionOffset = 0f;
        [SerializeField]
        [HideInInspector]
        private int _spawnCount = 0;
#if UNITY_EDITOR
        [SerializeField]
        [HideInInspector]
        private bool _retainPrefabInstancesInEditor = true;
#endif
        [SerializeField]
        [HideInInspector]
        private Positioning _objectPositioning = Positioning.Stretch;
        [SerializeField]
        [HideInInspector]
        private Iteration _iteration = Iteration.Ordered;
        [SerializeField]
        [HideInInspector]
        private int _randomSeed = 1;
        [SerializeField]
        [HideInInspector]
        private Vector2 _randomSize = Vector2.one;
        [SerializeField]
        [HideInInspector]
        private Vector2 _offset = Vector2.zero;
        [SerializeField]
        [HideInInspector]
        private Vector3 _minRotationOffset = Vector3.zero;
        [SerializeField]
        [HideInInspector]
        private Vector3 _maxRotationOffset = Vector3.zero;
        [SerializeField]
        [HideInInspector]
        private Vector3 _minScaleMultiplier = Vector3.one;
        [SerializeField]
        [HideInInspector]
        private Vector3 _maxScaleMultiplier = Vector3.one;
        [SerializeField]
        [HideInInspector]
        private bool _randomizeOffset = false;
        [SerializeField]
        [HideInInspector]
        private bool _useRandomOffsetRotation = false;
        [SerializeField]
        [HideInInspector]
        private bool _shellOffset = true;
        [SerializeField]
        [HideInInspector]
        private bool _randomOffset = false;
        [SerializeField]
        [HideInInspector]
        private bool _applyRotation = true;
        [SerializeField]
        [HideInInspector]
        private bool _applyScale = false;
        [SerializeField]
        [HideInInspector]
        private ObjectMethod _objectMethod = ObjectMethod.Instantiate;
        [HideInInspector]
        public bool delayedSpawn = false;
        [HideInInspector]
        public float spawnDelay = 0.1f;
        [SerializeField]
        [HideInInspector]
        private int lastChildCount = 0;
        [SerializeField]
        [HideInInspector]
        private ObjectControl[] spawned = new ObjectControl[0];

        SplineResult evaluateResult = new SplineResult();

        System.Random randomizer, randomizer2, rotationRandomizer, scaleRandomizer;

        public void Clear()
        {
            for (int i = 0; i < spawned.Length; i++)
            {
                if (spawned[i] == null) continue;
                spawned[i].transform.localScale = spawned[i].baseScale;
                if (_objectMethod == ObjectMethod.GetChildren) spawned[i].gameObject.SetActive(false);
                else
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying) spawned[i].DestroyImmediate();
                    else spawned[i].Destroy();
#else
                    spawned[i].Destroy();
#endif

                }
            }
            spawned = new ObjectControl[0];
        }

        private void Remove()
        {
#if UNITY_EDITOR
#if !UNITY_2018_3_OR_NEWER
            if (PrefabUtility.GetPrefabType(gameObject) == PrefabType.Prefab) return;
#endif
#endif
            if (_spawnCount >= spawned.Length) return;
            for (int i = spawned.Length - 1; i >= _spawnCount; i--)
            {
                if (i >= spawned.Length) break;
                if (spawned[i] == null) continue;
                spawned[i].transform.localScale = spawned[i].baseScale;
                if (_objectMethod == ObjectMethod.GetChildren) spawned[i].gameObject.SetActive(false);
                else
                {
                    if (Application.isEditor) spawned[i].DestroyImmediate();
                    else spawned[i].Destroy();

                }
            }
            ObjectControl[] newSpawned = new ObjectControl[_spawnCount];
            for (int i = 0; i < newSpawned.Length; i++)
            {
                newSpawned[i] = spawned[i];
            }
            spawned = newSpawned;
            Rebuild(false);
        }

        public void GetAll()
        {
            ObjectControl[] newSpawned = new ObjectControl[this.transform.childCount];
            int index = 0;
            foreach (Transform child in this.transform)
            {
                if (newSpawned[index] == null)
                {
                    newSpawned[index++] = new ObjectControl(child.gameObject);
                    continue;
                }
                bool found = false;
                for (int i = 0; i < spawned.Length; i++)
                {
                    if (spawned[i].gameObject == child.gameObject)
                    {
                        newSpawned[index++] = spawned[i];
                        found = true;
                        break;
                    }
                }
                if (!found) newSpawned[index++] = new ObjectControl(child.gameObject);
            }
            spawned = newSpawned;
        }

        public void Spawn()
        {
#if UNITY_EDITOR
#if !UNITY_2018_3_OR_NEWER
            if (PrefabUtility.GetPrefabType(gameObject) == PrefabType.Prefab) return;
#endif
#endif
            if (_objectMethod == ObjectMethod.Instantiate)
            {
                if (delayedSpawn && Application.isPlaying)
                {
                    StopCoroutine("InstantiateAllWithDelay");
                    StartCoroutine(InstantiateAllWithDelay());
                }
                else InstantiateAll();
            }
            else GetAll();
            Rebuild(false);
        }

        protected override void LateRun()
        {
            base.LateRun();
            if (_objectMethod == ObjectMethod.GetChildren && lastChildCount != this.transform.childCount)
            {
                Spawn();
                lastChildCount = this.transform.childCount;
            }
        }


        IEnumerator InstantiateAllWithDelay()
        {
            if (computer == null) yield break;
            if (objects.Length == 0) yield break;
            for (int i = spawned.Length; i <= spawnCount; i++)
            {
                InstantiateSingle();
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        private void InstantiateAll()
        {
            if (computer == null) return;
            if (objects.Length == 0) return;
            for (int i = spawned.Length; i < spawnCount; i++)
            {
                InstantiateSingle();
            }
        }

        private void InstantiateSingle()
        {
            if (objects.Length == 0) return;
            int index = 0;
            if (_iteration == Iteration.Ordered)
            {
                index = spawned.Length - Mathf.FloorToInt(spawned.Length / objects.Length) * objects.Length;
            }
            else index = Random.Range(0, objects.Length);
            if (objects[index] == null) return;

            ObjectControl[] newSpawned = new ObjectControl[spawned.Length + 1];
            spawned.CopyTo(newSpawned, 0);
#if UNITY_EDITOR
            if (!Application.isPlaying && retainPrefabInstancesInEditor)
            {
                GameObject go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(objects[index]);
                go.transform.position = transform.position;
                go.transform.rotation = transform.rotation;
                newSpawned[newSpawned.Length - 1] = new ObjectControl(go);
            } else
            {
                newSpawned[newSpawned.Length - 1] = new ObjectControl((GameObject)Instantiate(objects[index], this.transform.position, this.transform.rotation));
            }
#else
            newSpawned[newSpawned.Length - 1] = new ObjectControl((GameObject)Instantiate(objects[index], this.transform.position, this.transform.rotation));
#endif
            newSpawned[newSpawned.Length - 1].transform.parent = transform;
            spawned = newSpawned;
        }

        protected override void Build()
        {
            base.Build();
            randomizer = new System.Random(_randomSeed);
            randomizer2 = new System.Random(_randomSeed + 1);
            rotationRandomizer = new System.Random(_randomSeed + 2);
            scaleRandomizer = new System.Random(_randomSeed + 3);
            Quaternion offsetRot = Quaternion.Euler(_minRotationOffset);
            bool randomRotOffset = _minRotationOffset != _maxRotationOffset;
            bool randomScaleMultiplier = _minScaleMultiplier != _maxScaleMultiplier;
            for (int i = 0; i < spawned.Length; i++)
            {
                if (spawned[i] == null)
                {
                    Clear();
                    Spawn();
                    break;
                }
                float percent = 0f;
                if (spawned.Length > 1) percent = (float)i / (spawned.Length - 1);
                percent += positionOffset;
                if (percent > 1f) percent -= 1f;
                else if (percent < 0f) percent += 1f;
                if (objectPositioning == Positioning.Clip) Evaluate(evaluateResult, percent);
                else Evaluate(evaluateResult, DMath.Lerp(clipFrom, clipTo, percent));
                spawned[i].position = evaluateResult.position;

                if (_applyScale)
                {
                    Vector3 scale = spawned[i].baseScale * evaluateResult.size;
                    if(randomScaleMultiplier)
                    {
                        scale.x *= Mathf.Lerp(_minScaleMultiplier.x, _maxScaleMultiplier.x, (float)scaleRandomizer.NextDouble());
                        scale.y *= Mathf.Lerp(_minScaleMultiplier.y, _maxScaleMultiplier.y, (float)scaleRandomizer.NextDouble());
                        scale.z *= Mathf.Lerp(_minScaleMultiplier.z, _maxScaleMultiplier.z, (float)scaleRandomizer.NextDouble());
                    } else
                    {
                        scale.x *= scaleMultiplier.x;
                        scale.y *= scaleMultiplier.y;
                        scale.z *= scaleMultiplier.z;
                    }
                    spawned[i].scale = scale;
                }
                else spawned[i].scale = spawned[i].baseScale;
                Vector3 right = Vector3.Cross(evaluateResult.direction, evaluateResult.normal).normalized;
                spawned[i].position += -right * _offset.x + evaluateResult.normal * _offset.y;
                if (_applyRotation)
                {
                    if (randomRotOffset) offsetRot = Quaternion.Euler(Mathf.Lerp(_minRotationOffset.x, _maxRotationOffset.x, (float)rotationRandomizer.NextDouble()), Mathf.Lerp(_minRotationOffset.y, _maxRotationOffset.y, (float)rotationRandomizer.NextDouble()), Mathf.Lerp(_minRotationOffset.z, _maxRotationOffset.z, (float)rotationRandomizer.NextDouble()));
                    if (!_randomizeOffset || !_useRandomOffsetRotation) spawned[i].rotation = evaluateResult.rotation* offsetRot;
                }

                if (_randomizeOffset)
                {
                    float distance = (float)randomizer.NextDouble();
                    float angleInRadians = (float)randomizer2.NextDouble() * 360f * Mathf.Deg2Rad;
                    Vector2 randomCircle = new Vector2(distance * Mathf.Cos(angleInRadians), distance * Mathf.Sin(angleInRadians));
                    if (_shellOffset) randomCircle.Normalize();
                    else randomCircle = Vector2.ClampMagnitude(randomCircle, 1f);
                    Vector3 center = spawned[i].position;
                    spawned[i].position += randomCircle.x * right * _randomSize.x * evaluateResult.size * 0.5f + randomCircle.y * evaluateResult.normal * _randomSize.y * evaluateResult.size * 0.5f;
                    if (_useRandomOffsetRotation) spawned[i].rotation = Quaternion.LookRotation(evaluateResult.direction, spawned[i].position - center) * offsetRot;
                }

                if (_objectPositioning == Positioning.Clip)
                {
                    if (percent < clipFrom || percent > clipTo) spawned[i].active = false;
                    else spawned[i].active = true;
                }
            }
        }

        protected override void PostBuild()
        {
            base.PostBuild();
            for (int i = 0; i < spawned.Length; i++)
            {
                spawned[i].Apply();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (objectMethod == ObjectMethod.Instantiate)
            {
                Clear();
            }
        }
    }
}
