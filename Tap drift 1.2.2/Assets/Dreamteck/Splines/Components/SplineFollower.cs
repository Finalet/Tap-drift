using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    public delegate void SplineReachHandler();
    [AddComponentMenu("Dreamteck/Splines/Spline Follower")]
    public class SplineFollower : SplineTracer
    {
        public enum FollowMode { Uniform, Time }
        public enum Wrap { Default, Loop, PingPong }
        [HideInInspector]
        public Wrap wrapMode = Wrap.Default;
        [HideInInspector]
        public FollowMode followMode = FollowMode.Uniform;

        [HideInInspector]
        public bool autoStartPosition = false;

        [HideInInspector]
        public bool autoFollow = true;
        /// <summary>
        /// Used when follow mode is set to Uniform. Defines the speed of the follower
        /// </summary>
        public float followSpeed
        {
            get { return _followSpeed; }
            set
            {
                if (_followSpeed != value)
                {
                    if (value < 0f) value = 0f;
                    _followSpeed = value;
                }
            }
        }

        /// <summary>
        /// Used when follow mode is set to Time. Defines how much time it takes for the follower to travel through the path
        /// </summary>
        public float followDuration
        {
            get { return _followDuration; }
            set
            {
                if (_followDuration != value)
                {
                    if (value < 0f) value = 0f;
                    _followDuration = value;
                }
            }
        }

        [System.Obsolete("Deprecated in 1.0.8. Use result instead")]
        public SplineResult followResult
        {
            get { return _result; }
        }

        [System.Obsolete("Deprecated in 1.0.8. Use offsettedResult instead")]
        public SplineResult offsettedFollowResult
        {
            get { return offsettedResult;  }
        }

        public event SplineReachHandler onEndReached;
        public event SplineReachHandler onBeginningReached;

        [SerializeField]
        [HideInInspector]
        private float _followSpeed = 1f;
        [SerializeField]
        [HideInInspector]
        private float _followDuration = 1f;

        private double lastClippedPercent = -1.0;

        private SplineResult from = new SplineResult();
        private SplineResult to = new SplineResult();
        private bool followStarted = false;

#if UNITY_EDITOR
        public bool editorSetPosition = true;
#endif

        protected override void Start()
        {
            base.Start();
            if (autoFollow && autoStartPosition) SetPercent(Project(GetTransform().position).percent);
        }

        protected override void LateRun()
        {
            base.LateRun();
            if (autoFollow) AutoFollow();
        }

        protected override void PostBuild()
        {
            base.PostBuild();
            if (samples.Length == 0) return;
            EvaluateClipped(_result, ClipPercent(_result.percent));
            if (autoFollow && !autoStartPosition) ApplyMotion();
        }

        void AutoFollow()
        {
            if (!followStarted && autoStartPosition) SetPercent(Project(GetTransform().position).percent);
            followStarted = true;
            switch (followMode)
            {
                case FollowMode.Uniform: Move(Time.deltaTime * _followSpeed); break;
                case FollowMode.Time: 
                    if(_followDuration == 0.0) Move(0.0);
                    else Move((double)Time.deltaTime / _followDuration);
                    break;
            }
        }

        public void Restart(double startPosition = 0.0)
        {
            ResetTriggers();
            followStarted = false;
            SetPercent(startPosition);
        }

        public override void SetPercent(double percent, bool checkTriggers = false)
        {
            if (clippedSamples.Length == 0) GetClippedSamplesImmediate();
            base.SetPercent(percent, checkTriggers);
            lastClippedPercent = percent;
        }

        public override void SetDistance(float distance, bool checkTriggers = false)
        {
            base.SetDistance(distance, checkTriggers);
            lastClippedPercent = ClipPercent(_result.percent);
            if (samplesAreLooped && clipFrom == clipTo && distance > 0f && lastClippedPercent == 0.0) lastClippedPercent = 1.0; 
        }

        public void Move(double percent)
        {
			if(percent == 0.0) return;
            if (clippedSamples.Length <= 1)
            {
                if (clippedSamples.Length == 1)
                {
                    _result.CopyFrom(clippedSamples[0]);
                    ApplyMotion();
                }
                return;
            }
            
            EvaluateClipped(_result, ClipPercent(_result.percent));
            double startPercent = ClipPercent(_result.percent);
            if (wrapMode == Wrap.Default && lastClippedPercent >= 1.0 && startPercent == 0.0) startPercent = 1.0;
            double p = startPercent + (_direction == Spline.Direction.Forward ? percent : -percent);
            bool callOnEndReached = false, callOnBeginningReached = false;
            lastClippedPercent = p;
            if(_direction == Spline.Direction.Forward && p >= 1.0)
            {
                if (onEndReached != null && startPercent < 1.0) callOnEndReached = true;
                switch (wrapMode)
                {
                    case Wrap.Default:
                        p = 1.0;
                        break;
                    case Wrap.Loop:
                        CheckTriggersClipped(UnclipPercent(startPercent), UnclipPercent(1.0));
                        while (p > 1.0) p -= 1.0;
                        startPercent = 0.0;
                        break;
                    case Wrap.PingPong:
                        p = DMath.Clamp01(1.0-(p-1.0));
                        startPercent = 1.0;
                        _direction = Spline.Direction.Backward;
                        break;
                }
            } else if(_direction == Spline.Direction.Backward && p <= 0.0)
            {
                if (onBeginningReached != null && startPercent > 0.0) callOnBeginningReached = true;
                switch (wrapMode)
                {
                    case Wrap.Default:
                        p = 0.0; 
                        break;
                    case Wrap.Loop:
                        CheckTriggersClipped(UnclipPercent(startPercent), UnclipPercent(0.0));
                        while (p < 0.0) p += 1.0;
                        startPercent = 1.0;
                        break;
                    case Wrap.PingPong:
                        p = DMath.Clamp01(-p);
                        startPercent = 0.0;
                        _direction = Spline.Direction.Forward;
                        break;
                }
            }
            CheckTriggersClipped(UnclipPercent(startPercent), UnclipPercent(p));
            Evaluate(_result, UnclipPercent(p));
            ApplyMotion();
            if (callOnEndReached) onEndReached();
            else if (callOnBeginningReached) onBeginningReached();
            InvokeTriggers();
        }

        public void Move(float distance)
        {
            if (distance < 0f) distance = 0f;
            if (distance == 0f) return;
            if (samples.Length <= 1)
            {
                if (samples.Length == 1)
                {
                    _result.CopyFrom(samples[0]);
                    ApplyMotion();
                }
                return;
            }
            bool callOnEndReached = false, callOnBeginningReached = false;

            float moved = 0f;
            int nextIndex = 0;
            double clippedPercent = ClipPercent(_result.percent);
            if (clippedPercent == 0.0 && lastClippedPercent == 1.0 && clipFrom == clipTo) clippedPercent = 1.0;
            from.CopyFrom(_result);
            to.CopyFrom(_result);

            if (_direction == Spline.Direction.Forward)
            {

                nextIndex = DMath.FloorInt(clippedPercent * (clippedSamples.Length - 1))-1;
                if (nextIndex < 0) nextIndex = 0;
                for (int i = nextIndex; i < clippedSamples.Length-1; i++)
                {
                    if (ClipPercent(clippedSamples[i].percent) > clippedPercent) break;
                    nextIndex = i;
                }
            }
            else
            {
                nextIndex = DMath.CeilInt(clippedPercent * (clippedSamples.Length - 1)) + 1;
                if (nextIndex >= clippedSamples.Length) nextIndex = clippedSamples.Length - 1;
                for (int i = nextIndex; i >= 1; i--)
                {
                    double percent = ClipPercent(clippedSamples[i].percent);
                    if (i == clippedSamples.Length - 1) percent = 1.0;
                    if (percent < clippedPercent) break;
                    nextIndex = i;
                }
            }
            while (moved < distance)
            {
                from.CopyFrom(to); //Get the current sample
                if (_direction == Spline.Direction.Forward)
                {
                    nextIndex++;
                    if ((samplesAreLooped && _result.percent >= clippedSamples[clippedSamples.Length - 1].percent && _result.percent < clippedSamples[0].percent) || (!samplesAreLooped && _result.percent >= clippedSamples[clippedSamples.Length - 1].percent) || nextIndex >= clippedSamples.Length)
                    {
                        if (onEndReached != null && lastClippedPercent < 1.0) callOnEndReached = true;
                        if (wrapMode == Wrap.Default)
                        {
                            lastClippedPercent = 1.0;
                            _result.CopyFrom(clippedSamples[clippedSamples.Length - 1]);
                            CheckTriggersClipped(from.percent, _result.percent);
                            break;
                        }
                        else if (wrapMode == Wrap.Loop)
                        {
                            CheckTriggersClipped(from.percent, clippedSamples[clippedSamples.Length-1].percent);
                            from.CopyFrom(clippedSamples[0]);
                            nextIndex = 1;
                        }
                        else if (wrapMode == Wrap.PingPong)
                        {
                            _direction = Spline.Direction.Backward;
                            CheckTriggersClipped(from.percent, clippedSamples[clippedSamples.Length - 1].percent);
                            from.CopyFrom(clippedSamples[clippedSamples.Length - 1]);
                            nextIndex = clippedSamples.Length - 2;
                        }
                    }
                }
                else
                {
                    nextIndex--;
                    if ((samplesAreLooped && _result.percent <= clippedSamples[0].percent && _result.percent > clippedSamples[clippedSamples.Length - 1].percent) || (!samplesAreLooped && _result.percent <= clippedSamples[0].percent) || nextIndex < 0)
                    {
                        if (onBeginningReached != null && lastClippedPercent > 0.0) callOnBeginningReached = true;
                        if (wrapMode == Wrap.Default)
                        {
                            lastClippedPercent = 0.0;
                            _result.CopyFrom(clippedSamples[0]);
                            CheckTriggersClipped(from.percent, _result.percent);
                            break;
                        }
                        else if (wrapMode == Wrap.Loop)
                        {
                            CheckTriggersClipped(from.percent, clippedSamples[0].percent);
                            from.CopyFrom(clippedSamples[clippedSamples.Length - 1]);
                            nextIndex = clippedSamples.Length - 2;
                        }
                        else if (wrapMode == Wrap.PingPong)
                        {
                            _direction = Spline.Direction.Forward;
                            CheckTriggersClipped(from.percent, clippedSamples[0].percent);
                            from.CopyFrom(clippedSamples[0]);
                            nextIndex = 1;
                        }
                    }
                }
                lastClippedPercent = ClipPercent(_result.percent);
                to.CopyFrom(clippedSamples[nextIndex]); //Get the next sample
                float traveled = Vector3.Distance(to.position, from.position);
                moved += traveled;
                if (moved >= distance)
                {
                    float excess = moved - distance;
                    double lerpPercent = 1.0 - excess / traveled;
                    SplineResult.Lerp(from, to, lerpPercent, _result);
                    if (samplesAreLooped)
                    {
                        if (_direction == Spline.Direction.Forward && from.percent > to.percent) _result.percent = DMath.Lerp(from.percent, 1.0, lerpPercent);  
                        else if (_direction == Spline.Direction.Backward && from.percent < to.percent) _result.percent = DMath.Lerp(1.0, to.percent, lerpPercent);
                    }
                    CheckTriggersClipped(from.percent, _result.percent);
                    break; 
                }
                CheckTriggersClipped(from.percent, to.percent);
            }

            ApplyMotion();
            if (callOnEndReached) onEndReached();
            else if (callOnBeginningReached) onBeginningReached();
            InvokeTriggers();
        }
    }
}
