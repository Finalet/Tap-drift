using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Serialization;

namespace Dreamteck.Splines
{
    [AddComponentMenu("Dreamteck/Splines/Spline Projector")]
    public class SplineProjector : SplineTracer
    {
        public enum Mode {Accurate, Cached}
        public Mode mode
        {
            get { return _mode; }
            set
            {
                if(value != _mode)
                {
                    _mode = value;
                    Rebuild(false);
                }
            }
        }

        public bool autoProject
        {
            get { return _autoProject; }
            set
            {
                if(value != _autoProject)
                {
                    _autoProject = value;
                    if (_autoProject) Rebuild(false);
                }
            }
        }

        public int subdivide
        {
            get { return _subdivide; }
            set
            {
                if (value != _subdivide)
                {
                    _subdivide = value;
                    if (_mode == Mode.Accurate) Rebuild(false);
                }
            }
        }

        public Transform projectTarget
        {
            get {
                if (_projectTarget == null) return this.transform;
                return _projectTarget; 
            }
            set
            {
                if (value != _projectTarget)
                {
                    _projectTarget = value;
                    finalTarget = new TS_Transform(_projectTarget);
                    Rebuild(false);
                }
            }
        }

        [System.Obsolete("Deprecated in 1.0.8. Use targetObject instead")]
        public Transform target
        {
            get { return targetObject.transform; }
            set
            {
                if (value != applyTarget)
                {
                    applyTarget = value;
                    Rebuild(false);
                }
            }
        }

        public GameObject targetObject
        {
            get
            {
                if (_targetObject == null)
                {
                    if (applyTarget != null) //Temporary check to migrate SplineProjectors that use target
                    {
                        _targetObject = applyTarget.gameObject;
                        applyTarget = null;
                        return _targetObject;
                    }
                }
                return _targetObject;
            }

            set
            {
                if (value != _targetObject)
                {
                    _targetObject = value;
                    RefreshTargets();
                    Rebuild(false);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private Mode _mode = Mode.Accurate;
        [SerializeField]
        [HideInInspector]
        private bool _autoProject = true;
        [SerializeField]
        [HideInInspector]
        private int _subdivide = 3;
        [SerializeField]
        [HideInInspector]
        private Transform _projectTarget;


        [SerializeField]
        [HideInInspector]
        private Transform applyTarget = null;
        [SerializeField]
        [HideInInspector]
        private GameObject _targetObject;



        [System.Obsolete("Deprecated in 1.0.8. Use result instead.")]
        public SplineResult projectResult
        {
            get { return result; }
        }

        [SerializeField]
        [HideInInspector]
        private TS_Transform finalTarget;
        double traceFromA = -1.0, traceToA = -1.0, traceFromB = -1.0;

        [SerializeField]
        [HideInInspector]
        public Vector2 _offset;
        [SerializeField]
        [HideInInspector]
        public Vector3 _rotationOffset = Vector3.zero;

        public event SplineReachHandler onEndReached;
        public event SplineReachHandler onBeginningReached;

        // Use this for initialization
        protected override void Awake()
        {
            base.Awake();
            GetProjectTarget();
        }

#if UNITY_EDITOR
        public override void EditorAwake()
        {
            base.EditorAwake();
            GetProjectTarget();
        }
#endif

        protected override Transform GetTransform()
        {
            if (targetObject == null) return null;
            return targetObject.transform;
        }

        protected override Rigidbody GetRigidbody()
        {
            if (targetObject == null) return null;
            return targetObject.GetComponent<Rigidbody>();
        }

        protected override Rigidbody2D GetRigidbody2D()
        {
            if (targetObject == null) return null;
            return targetObject.GetComponent<Rigidbody2D>();
        }

        private void GetProjectTarget()
        {
            if (_projectTarget != null) finalTarget = new TS_Transform(_projectTarget);
            else finalTarget = new TS_Transform(this.transform);
        }

        protected override void LateRun()
        {
            base.LateRun();
            if (autoProject)
            {
                if (finalTarget == null) GetProjectTarget();
                else if (finalTarget.transform == null) GetProjectTarget();
                if (finalTarget.HasPositionChange())
                {
                    finalTarget.Update();
                    RebuildImmediate(false);
                }
            }
         }

        protected override void PostBuild()
        {
            base.PostBuild();
            InternalCalculateProjection();
            if(targetObject != null) ApplyMotion();
            CheckTriggers();
            InvokeTriggers();
        }

        private void CheckTriggers()
        {
            if (traceFromA >= 0f)
            {
                if (clipTo - traceFromA > traceFromB)
                {
                    traceToA = clipTo;
                    traceFromB = clipFrom;
                }
                else
                {
                    traceToA = clipFrom;
                    traceFromB = clipTo;
                }
                if (System.Math.Abs(traceToA - traceFromA) + System.Math.Abs(result.percent - traceFromB) < System.Math.Abs(result.percent - traceFromA))
                {
                    CheckTriggers(traceFromA, traceToA);
                    CheckTriggers(traceFromB, result.percent);
                }
                else CheckTriggers(traceFromA, result.percent);
            }
        }

        public void CalculateProjection()
        {
            finalTarget.Update();
            Rebuild(false);
        }

        private void InternalCalculateProjection()
        {
            if (computer == null || samples.Length == 0)
            {
                _result = new SplineResult();
                return;
            }
            traceFromA = -1.0;
            traceToA = -1.0;
            traceFromB = -1.0;
            double lastPercent = result.percent;
            if (result != null) traceFromA = result.percent;
            if (_mode == Mode.Accurate)
            {
                double percent = _address.Project(finalTarget.position, subdivide, clipFrom, clipTo);
                _result = _address.Evaluate(percent);
            } else _result = Project(finalTarget.position);
            if (onBeginningReached != null && result.percent <= clipFrom)
            {
                if (!Mathf.Approximately((float)lastPercent, (float)result.percent)) onBeginningReached();
            }
            else if (onEndReached != null && result.percent >= clipTo)
            {
                if (!Mathf.Approximately((float)lastPercent, (float)result.percent)) onEndReached();
            }
        }
    }
}
