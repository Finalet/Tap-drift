using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamteck.Splines
{
    public class SplineTracer : SplineUser
    {
        Trigger[] triggerInvokeQueue = new Trigger[0];
        int addTriggerIndex = 0;
        public enum PhysicsMode { Transform, Rigidbody, Rigidbody2D }
        public PhysicsMode physicsMode
        {
            get { return _physicsMode; }
            set
            {
                _physicsMode = value;
                RefreshTargets();
            }
        }

        public TransformModule motion
        {
            get
            {
                if (_motion == null) _motion = new TransformModule();
                return _motion;
            }
        }

        public CustomRotationModule customRotations
        {
            get
            {
                if (_customRotations == null) _customRotations = new CustomRotationModule();
                return _customRotations;
            }
        }

        public CustomOffsetModule customOffsets
        {
            get
            {
                if (_customOffsets == null) _customOffsets = new CustomOffsetModule();
                return _customOffsets;
            }
        }

        /// <summary>
        /// Returns the unmodified result from the evaluation
        /// </summary>
        public SplineResult result
        {
            get { return _result; }
        }

        /// <summary>
        /// Returns the offsetted evaluation result from the current follow position
        /// </summary>
        public SplineResult offsettedResult
        {
            get
            {
                SplineResult offsetted = new SplineResult(_result);
                Vector2 customOffset = customOffsets.Evaluate(offsetted.percent);
                Vector3 lastDirection = offsetted.direction;
                offsetted.position += offsetted.right * (motion.offset.x+customOffset.x) + offsetted.normal * (motion.offset.y+customOffset.y);
                Quaternion rot = customRotations.Evaluate(Quaternion.Euler(motion.rotationOffset)*Quaternion.LookRotation(offsetted.direction, offsetted.normal), offsetted.percent);
                offsetted.direction = rot * Vector3.forward;
                rot = customRotations.Evaluate(Quaternion.Euler(motion.rotationOffset)*Quaternion.LookRotation(offsetted.normal, lastDirection), offsetted.percent);
                offsetted.normal = rot * Vector3.forward;
                return offsetted;
            }
        }

        public Spline.Direction direction
        {
            get { return _direction; }
            set
            {
                if (value != _direction)
                {
                    _direction = value;
                    ApplyMotion();
                }
            }
        }

        public double clampedPercent
        {
            get
            {
                return ClipPercent(_result.percent);
            }
        }

        [HideInInspector]
        public bool applyDirectionRotation = true;
        [SerializeField]
        [HideInInspector]
        protected Spline.Direction _direction = Spline.Direction.Forward;

        [SerializeField]
        [HideInInspector]
        protected PhysicsMode _physicsMode = PhysicsMode.Transform;
        [SerializeField]
        [HideInInspector]
        protected TransformModule _motion = null;

        [HideInInspector]
        [SerializeField]
        [FormerlySerializedAs("triggers")]
        protected SplineTrigger[] triggers_old = new SplineTrigger[0]; //Field that holds the triggers as scriptable objects. Triggers are no longer scriptable objects so this prevents data loss
        [HideInInspector]
        public Trigger[] triggers = new Trigger[0];
        [SerializeField]
        [HideInInspector]
        protected CustomRotationModule _customRotations = new CustomRotationModule();
        [SerializeField]
        [HideInInspector]
        protected CustomOffsetModule _customOffsets = new CustomOffsetModule();

        [SerializeField]
        [HideInInspector]
        protected Rigidbody targetRigidbody = null;
        [SerializeField]
        [HideInInspector]
        protected Rigidbody2D targetRigidbody2D = null;
        [SerializeField]
        [HideInInspector]
        protected Transform targetTransform = null;
        [SerializeField]
        [HideInInspector]
        protected SplineResult _result = new SplineResult();

        private bool setPercentOnRebuild = false;
        private double targetPercentOnRebuild = 0.0;


        //Obsolete properties
        [System.Obsolete("Deprecated in version 1.0.7. Use motion.applyPosition instead")]
        public bool applyPosition
        {
            get { return motion.applyPosition; }
            set { motion.applyPosition = value; }
        }

        [System.Obsolete("Deprecated in version 1.0.7. Use motion.applyRotation instead")]
        public bool applyRotation
        {
            get { return motion.applyRotation; }
            set { motion.applyRotation = value; }
        }

        [System.Obsolete("Deprecated in version 1.0.7. Use motion.applyScale instead")]
        public bool applyScale
        {
            get { return motion.applyScale; }
            set { motion.applyScale = value; }
        }

        [System.Obsolete("Deprecated in version 1.0.7. User motion.offset instead")]
        public Vector2 offset
        {
            get { return motion.offset; }
            set { motion.offset = value; }
        }

        [System.Obsolete("Deprecated in version 1.0.7. User motion.rotationOffset instead")]
        public Vector3 rotationOffset
        {
            get { return motion.rotationOffset; }
            set { motion.rotationOffset = value; }
        }

#if UNITY_EDITOR
        public override void EditorAwake()
        {
            base.EditorAwake();
            RefreshTargets();
            if (MigrateTriggers()) UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        protected virtual void Start()
        {
            RefreshTargets();
        }


        /// <summary>
        /// Get the available node in front or behind the follower
        /// </summary>
        /// <returns></returns>
        public Node GetNextNode()
        {
            SplineComputer comp;
            double evaluatePercent = 0.0;
            Spline.Direction dir = Spline.Direction.Forward;
            _address.GetEvaluationValues(_result.percent, out comp, out evaluatePercent, out dir);
            if (_direction == Spline.Direction.Backward)
            {
                if (dir == Spline.Direction.Forward) dir = Spline.Direction.Backward;
                else dir = Spline.Direction.Forward;
            }
            int[] links = comp.GetAvailableNodeLinksAtPosition(evaluatePercent, dir);
            if (links.Length == 0) return null;
            //Find the closest one
            if (dir == Spline.Direction.Forward)
            {
                int min = comp.pointCount - 1;
                int index = 0;
                for (int i = 0; i < links.Length; i++)
                {
                    if (comp.nodeLinks[links[i]].pointIndex < min)
                    {
                        min = comp.nodeLinks[links[i]].pointIndex;
                        index = i;
                    }
                }
                return comp.nodeLinks[links[index]].node;
            }
            else
            {
                int max = 0;
                int index = 0;
                for (int i = 0; i < links.Length; i++)
                {
                    if (comp.nodeLinks[links[i]].pointIndex > max)
                    {
                        max = comp.nodeLinks[links[i]].pointIndex;
                        index = i;
                    }
                }
                return comp.nodeLinks[links[index]].node;
            }
        }

        /// <summary>
        /// Get the current computer the follower is on at the moment
        /// </summary>
        /// <returns></returns>
        public void GetCurrentComputer(out SplineComputer comp, out double percent, out Spline.Direction dir)
        {
            _address.GetEvaluationValues(_result.percent, out comp, out percent, out dir);
        }

        public void ResetTriggers()
        {
            for (int i = 0; i < triggers.Length; i++) triggers[i].ResetWorkOnce();
        }

        public virtual void SetPercent(double percent, bool checkTriggers = false)
        {
            if (samples.Length == 0) return;
            double lastPercent = _result.percent;
            UnclipPercent(ref percent);
            Evaluate(_result, percent);
            ApplyMotion();
            if (checkTriggers)
            {
                CheckTriggers(lastPercent, percent);
                InvokeTriggers();
            }
        }

        public virtual void SetDistance(float distance, bool checkTriggers = false)
        {
            if (clippedSamples.Length == 0) return;
            double lastPercent = _result.percent;
            EvaluateClipped(_result, TravelClipped(0.0, distance, Spline.Direction.Forward));
            ApplyMotion();
            if (checkTriggers)
            {
                CheckTriggers(lastPercent, _result.percent);
                InvokeTriggers();
            }
        }

        protected override void PostBuild()
        {
            if (setPercentOnRebuild)
            {
                SetPercent(targetPercentOnRebuild);
                setPercentOnRebuild = false;
            }
        }

        public override void EnterAddress(Node node, int pointIndex, Spline.Direction direction = Spline.Direction.Forward)
        {
            int element = _address.GetElementIndex(_result.percent);
            double localPercent = _address.PathToLocalPercent(_result.percent, element);
            base.EnterAddress(node, pointIndex, direction);
            double newPercent = _address.LocalToPathPercent(localPercent, element);
            setPercentOnRebuild = true;
            targetPercentOnRebuild = newPercent;
        }

        public override void AddComputer(SplineComputer computer, int connectionIndex, int connectedIndex, Spline.Direction direction = Spline.Direction.Forward)
        {
            int element = _address.GetElementIndex(_result.percent);
            double localPercent = _address.PathToLocalPercent(_result.percent, element);
            base.AddComputer(computer, connectionIndex, connectedIndex, direction);
            double newPercent = _address.LocalToPathPercent(localPercent, element);
            setPercentOnRebuild = true;
            targetPercentOnRebuild = newPercent;
        }

        public override void ExitAddress(int depth)
        {
            int element = _address.GetElementIndex(_result.percent);
            double localPercent = _address.PathToLocalPercent(_result.percent, element);
            base.ExitAddress(depth);
            double newPercent = _address.LocalToPathPercent(localPercent, element);
            setPercentOnRebuild = true;
            targetPercentOnRebuild = newPercent;
        }

        protected virtual Rigidbody GetRigidbody()
        {
            return GetComponent<Rigidbody>();
        }

        protected virtual Rigidbody2D GetRigidbody2D()
        {
            return GetComponent<Rigidbody2D>();
        }

        protected virtual Transform GetTransform()
        {
            return transform;
        }

        protected void ApplyMotion()
        {
            motion.targetUser = this;
            motion.splineResult = _result;
            motion.customRotation = _customRotations;
            motion.customOffset = _customOffsets;
            if (applyDirectionRotation) motion.direction = _direction;
            else motion.direction = Spline.Direction.Forward;

#if UNITY_EDITOR
                if (!Application.isPlaying)
            {
                if (targetTransform == null) RefreshTargets();
                if (targetTransform == null) return;
                motion.ApplyTransform(targetTransform);
                return;
            }
#endif
            switch (_physicsMode)
            {
                case PhysicsMode.Transform:
                    if (targetTransform == null) RefreshTargets();
                    if (targetTransform == null) return;
                    motion.ApplyTransform(targetTransform);
                    break;
                case PhysicsMode.Rigidbody:
                    if (targetRigidbody == null)
                    {
                        RefreshTargets();
                        if (targetRigidbody == null)  throw new MissingComponentException("There is no Rigidbody attached to " + name + " but the Physics mode is set to use one.");
                    }
                    motion.ApplyRigidbody(targetRigidbody);
                    break;
                case PhysicsMode.Rigidbody2D:
                    if (targetRigidbody2D == null)
                    {
                        RefreshTargets();
                        if (targetRigidbody2D == null) throw new MissingComponentException("There is no Rigidbody2D attached to " + name + " but the Physics mode is set to use one.");
                    }
                    motion.ApplyRigidbody2D(targetRigidbody2D);
                    break;
            }
        }

        protected void CheckTriggers(double from, double to)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            for (int i = 0; i < triggers.Length; i++)
            {
                if (triggers[i] == null) continue;
                if(triggers[i].Check(from, to)) AddTriggerToQueue(triggers[i]);
            }
        }

        protected void CheckTriggersClipped(double from, double to)
        {
            if(_direction == Spline.Direction.Forward)
            {
                if (from <= to) CheckTriggers(from, to);
                else
                {
                    CheckTriggers(from, 1.0);
                    CheckTriggers(0.0, to);
                }
            } else
            {
                if (from >= to) CheckTriggers(from, to);
                else
                {
                    CheckTriggers(from, 0.0);
                    CheckTriggers(1.0, to);
                }
            }
        }

        protected void InvokeTriggers()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            for (int i = 0; i < addTriggerIndex; i++)
            {
                if (triggerInvokeQueue[i] != null) triggerInvokeQueue[i].Invoke();
            }
            addTriggerIndex = 0;
        }

        //Used to migrate the old ScriptableObject-based triggers to the new ones
        private bool MigrateTriggers()
        {
            if (triggers_old.Length > 0)
            {
                Trigger[] newTriggers = new Trigger[triggers_old.Length];
                for (int i = 0; i < triggers_old.Length; i++)
                {
                    if (triggers_old[i] == null) continue;
                    newTriggers[i] = new Trigger();
                    newTriggers[i].name = triggers_old[i].name;
                    newTriggers[i].color = triggers_old[i].color;
                    newTriggers[i].position = triggers_old[i].position;
                    newTriggers[i].type = (Trigger.Type)((int)triggers_old[i].type);
                    newTriggers[i].actions = triggers_old[i].actions;
                    newTriggers[i].gameObjects = triggers_old[i].gameObjects;
                }
                triggers_old = new SplineTrigger[0];
                triggers = newTriggers;
                return true;
            }
            return false;
        }

        protected void RefreshTargets()
        {
            switch (_physicsMode)
            {
                case PhysicsMode.Transform:
                    targetTransform = GetTransform();
                    break;
                case PhysicsMode.Rigidbody:
                    targetRigidbody = GetRigidbody();
                    break;
                case PhysicsMode.Rigidbody2D:
                    targetRigidbody2D = GetRigidbody2D();
                    break;
            }
        }

        private void AddTriggerToQueue(Trigger trigger)
        {
            if (addTriggerIndex >= triggerInvokeQueue.Length)
            {
                Trigger[] newQueue = new Trigger[triggerInvokeQueue.Length + triggers.Length];
                triggerInvokeQueue.CopyTo(newQueue, 0);
                triggerInvokeQueue = newQueue;
            }
            triggerInvokeQueue[addTriggerIndex] = trigger;
            addTriggerIndex++;
        }

        private void AddTrigger(Trigger trigger)
        {
            Trigger[] newTriggers = new Trigger[triggers.Length + 1];
            triggers.CopyTo(newTriggers, 0);
            newTriggers[newTriggers.Length - 1] = trigger;
            triggers = newTriggers;
        }

        public void AddTrigger(UnityAction call, double position = 0.0, Trigger.Type type = Trigger.Type.Double)
        {
            Trigger trigger = new Trigger();
            trigger.Create(type, call);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(UnityAction<int> call, int value, double position = 0.0, Trigger.Type type = Trigger.Type.Double)
        {
            Trigger trigger = new Trigger();
            trigger.Create(type, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(UnityAction<float> call, float value, double position = 0.0, Trigger.Type type = Trigger.Type.Double)
        {
            Trigger trigger = new Trigger();
            trigger.Create(type, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(UnityAction<double> call, double value, double position = 0.0, Trigger.Type type = Trigger.Type.Double)
        {
            Trigger trigger = new Trigger();
            trigger.Create(type, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(UnityAction<string> call, string value, double position = 0.0, Trigger.Type type = Trigger.Type.Double)
        {
            Trigger trigger = new Trigger();
            trigger.Create(type, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(UnityAction<bool> call, bool value, double position = 0.0, Trigger.Type type = Trigger.Type.Double)
        {
            Trigger trigger = new Trigger();
            trigger.Create(type, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(UnityAction<GameObject> call, GameObject value, double position = 0.0, Trigger.Type type = Trigger.Type.Double)
        {
            Trigger trigger = new Trigger();
            trigger.Create(type, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void AddTrigger(UnityAction<Transform> call, Transform value, double position = 0.0, Trigger.Type type = Trigger.Type.Double)
        {
            Trigger trigger = new Trigger();
            trigger.Create(type, call, value);
            trigger.position = position;
            trigger.type = type;
            AddTrigger(trigger);
        }

        public void EvaluateClipped(SplineResult result, double clippedPercent)
        {
            Evaluate(result, UnclipPercent(clippedPercent));
        }

        public Vector3 EvaluatePositionClipped(double clippedPercent)
        {
            return EvaluatePosition(UnclipPercent(clippedPercent));
        }

        public double TravelClipped(double start, float distance, Spline.Direction direction)
        {
            if (clippedSamples.Length <= 1) return 0.0;
            if (distance == 0f) return start;
            float moved = 0f;
            Vector3 lastPosition = EvaluatePositionClipped(start);
            double lastPercent = start;
            int nextSampleIndex = direction == Spline.Direction.Forward ? DMath.CeilInt(start * (clippedSamples.Length - 1)) : DMath.FloorInt(start * (clippedSamples.Length - 1));
            float lastDistance = 0f;
            while (true)
            {
                lastDistance = Vector3.Distance(clippedSamples[nextSampleIndex].position, lastPosition);
                lastPosition = clippedSamples[nextSampleIndex].position;
                moved += lastDistance;
                if (moved >= distance) break;
                lastPercent = ClipPercent(clippedSamples[nextSampleIndex].percent);
                if (direction == Spline.Direction.Forward)
                {
                    if (nextSampleIndex == clippedSamples.Length - 1) break;
                    nextSampleIndex++;
                }
                else
                {
                    if (nextSampleIndex == 0) break;
                    nextSampleIndex--;
                }
            }
            return DMath.Lerp(lastPercent, ClipPercent(clippedSamples[nextSampleIndex].percent), 1f - (moved - distance) / lastDistance);
        }

        public SplineResult ProjectClipped(Vector3 point)
        {
            return Project(point, clipFrom, clipTo);
        }
    }
}
