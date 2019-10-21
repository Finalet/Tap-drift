using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if !UNITY_WSA
using System.Threading;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dreamteck.Splines {
    //SplineUser _samples SplineComputer and supports multithreading.
    public class SplineUser : MonoBehaviour {
        [HideInInspector]
        public SplineAddress _address = null;
        public enum UpdateMethod { Update, FixedUpdate, LateUpdate }
        [SerializeField]
        [HideInInspector]
        private SplineUser[] subscribers = new SplineUser[0];
        [HideInInspector]
        public UpdateMethod updateMethod = UpdateMethod.Update;
        [HideInInspector]
        [SerializeField]
        private SplineUser _user = null;
        public SplineUser user
        {
            get
            {
                return _user;
            }
            set
            {
                if (Application.isPlaying && value != null && value.rootUser == this) return;
                if (value != _user)
                {
                    if (value != null && computer != null)
                    {
                        computer.Unsubscribe(this);
                        computer = null;
                    }
                    if (_user != null) _user.Unsubscribe(this);
                    _user = value;
                    if (_user != null)
                    {
                        _user.Subscribe(this);
                        sampleUser = true;
                    }
                    if (computer == null)
                    {
                        _samples = new SplineResult[0];
                        _clippedSamples = new SplineResult[0];
                    }
                    Rebuild(false);
                }
            }
        }
        public SplineUser rootUser
        {
            get
            {
                SplineUser root = _user;
                while (root != null)
                {
                    if (root._user == null) break;
                    root = root._user;
                    if (root == this) break;
                }
                if (root == null) root = this;
                return root;
            }
        }

        public SplineComputer computer
        {
            get {
                return address.root;
            }
            set
            {
                if (_address == null)
                {
                    _address = new SplineAddress(value);
                    value.Subscribe(this);
                    if (value != null) RebuildImmediate(true);
                    return;
                }
                if (value != _address.root)
                {
                    if (value != null && sampleUser)
                    {
                        _user.Unsubscribe(this);
                        _user = null;
                    }
                    if (_address.root != null) _address.root.Unsubscribe(this);
                    _address.root = value;
                    if (value != null)
                    {
                        value.Subscribe(this);
                        sampleUser = false;
                    }
                    if (_address.root != null) RebuildImmediate(true);
                }
            }
        }

        public double resolution
        {
            get
            {
                return _resolution;
            }
            set
            {
                if (value != _resolution)
                {
                    animResolution = (float)_resolution;
                    _resolution = value;
                    if (sampleUser) return;
                    Rebuild(true);
                }
            }
        }

        public double clipFrom
        {
            get
            {
                return _clipFrom;
            }
            set
            {
                if (value != _clipFrom)
                {
                    animClipFrom = (float)_clipFrom;
                    _clipFrom = DMath.Clamp01(value);
                    if (_clipFrom > _clipTo)
                    {
                        if (!rootUser.computer.isClosed) _clipTo = _clipFrom;
                    }
                    getClippedSamples = true;
                    Rebuild(false);
                }
            }
        }

        public double clipTo
        {
            get
            {
                return _clipTo;
            }
            set
            {

                if (value != _clipTo)
                {
                    animClipTo = (float)_clipTo;
                    _clipTo = DMath.Clamp01(value);
                    if (_clipTo < _clipFrom)
                    {
                        if (!rootUser.computer.isClosed) _clipFrom = _clipTo;
                    }
                    getClippedSamples = true;
                    Rebuild(false);
                }
            }
        }

        public bool autoUpdate
        {
            get
            {
                return _autoUpdate;
            }
            set
            {
                if (value != _autoUpdate)
                {
                    _autoUpdate = value;
                    if (value) Rebuild(true);
                }
            }
        }

        public bool loopSamples
        {
            get
            {
                return _loopSamples;
            }
            set
            {
                if (value != _loopSamples)
                {
                    _loopSamples = value;
                    if (value) Rebuild(true);
                }
            }
        }

        public bool uniformSample
        {
            get
            {
                return _uniformSample;
            }
            set
            {
                if (value != _uniformSample)
                {
                    _uniformSample = value;
                    Rebuild(true);
                }
            }
        }

        public bool uniformPreserveClipRange
        {
            get
            {
                return _uniformPreserveClipRange;
            }
            set
            {
                if (value != _uniformPreserveClipRange)
                {
                    _uniformPreserveClipRange = value;
                    Rebuild(true);
                }
            }
        }

        //The percent of the spline that we're traversing
        public double span
        {
            get
            {
                if (samplesAreLooped) return (1.0 - _clipFrom) + _clipTo; 
                return _clipTo - _clipFrom;
            }
        }

        public SplineAddress address
        {
            get
            {
                if (_address == null) _address = new SplineAddress((SplineComputer)null);
                return _address;
            }
        }

        public bool samplesAreLooped
        {
            get
            {
                if (rootUser.computer == null) return false;
                return rootUser.computer.isClosed && _loopSamples && clipFrom >= clipTo;
            }
        }

        //Serialized values
        [SerializeField]
        [HideInInspector]
        private double _resolution = 1.0;
        [SerializeField]
        [HideInInspector]
        private double _clipTo = 1.0;
        [SerializeField]
        [HideInInspector]
        private double _clipFrom = 0.0;
        [SerializeField]
        [HideInInspector]
        private bool _autoUpdate = true;
        [SerializeField]
        [HideInInspector]
        private bool _loopSamples = false;
        [SerializeField]
        [HideInInspector]
        private bool _uniformSample = false;
        [SerializeField]
        [HideInInspector]
        private bool _uniformPreserveClipRange = false;
        [SerializeField]
        [HideInInspector]
        private SplineResult[] _samples = new SplineResult[0];
        public SplineResult[] samples
        {
            get
            {
                if (sampleUser) return _user.samples;
                else return _samples;
            }
        }
        [SerializeField]
        [HideInInspector]
        private SplineResult[] _clippedSamples = new SplineResult[0];
        public SplineResult[] clippedSamples
        {
            get
            {
                if (_clippedSamples.Length == 0 && _samples.Length > 0) GetClippedSamples();
                return _clippedSamples;
            }
        }

        //float values used for making animations
        [SerializeField]
        [HideInInspector]
        private float animClipFrom = 0f;
        [SerializeField]
        [HideInInspector]
        private float animClipTo = 1f;
        [SerializeField]
        [HideInInspector]
        private double animResolution = 1.0;
        [SerializeField]
        [HideInInspector]
        protected bool sampleUser = false;

        private bool rebuild = false;
        private bool sample = false;
        private volatile bool getClippedSamples = false;

        protected bool willRebuild
        {
            get
            {
                return rebuild;
            }
        }

        protected Transform trs = null;

        //Threading values
        [HideInInspector]
        public volatile bool multithreaded = false;
        [HideInInspector]
        public bool buildOnAwake = false;
        [HideInInspector]
        public bool buildOnEnable = false;
#if !UNITY_WSA
        private Thread buildThread = null;
#endif
        private volatile bool postThread = false;
        private volatile bool threadSample = false;
        private volatile bool threadWork = false;
        private bool _threadWorking = false;
        public bool threadWorking
        {
            get { return _threadWorking; }
        }
        private object locker = new object();

#if UNITY_EDITOR
        /// <summary>
        /// USE THIS ONLY IN A COMPILER DIRECTIVE REQUIRING UNITY_EDITOR
        /// </summary>
        protected bool isPlaying = false;
#endif


#if UNITY_EDITOR
        /// <summary>
        /// Used by the custom editor. DO NO CALL THIS METHOD IN YOUR RUNTIME CODE
        /// </summary>
        public virtual void EditorAwake()
        {
            trs = transform;
            //Create a new instance of the address. Otherwise it would be a reference
            if (!Application.isPlaying) _address = new SplineAddress(_address);
            if (sampleUser)
            {
                if (!user.IsSubscribed(this)) user.Subscribe(this);
            }
            else
            {
                if (computer == null) computer = GetComponent<SplineComputer>();
                else if (!computer.IsSubscribed(this)) computer.Subscribe(this);
            }
            RebuildImmediate(true);
            GetClippedSamplesImmediate();
        }
#endif

        protected virtual void Awake() {
#if UNITY_EDITOR
            isPlaying = true;
#endif
            trs = transform;
            if (sampleUser)
            {
                if (!user.IsSubscribed(this)) user.Subscribe(this);
            }
            else
            {
                if (computer == null) computer = GetComponent<SplineComputer>();
                else if (!computer.IsSubscribed(this)) computer.Subscribe(this);
            }
            if (buildOnAwake) RebuildImmediate(true);
        }

        protected virtual void Reset()
        {
#if UNITY_EDITOR
            EditorAwake();
#endif
        }

        protected virtual void OnEnable()
        {
            if (computer != null) computer.Subscribe(this);
            if (buildOnEnable) RebuildImmediate(true);
        }

        protected virtual void OnDisable()
        {
            if (computer != null) computer.Unsubscribe(this);
            threadWork = false;
        }

        protected virtual void OnDestroy()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && computer != null) computer.Unsubscribe(this); //Unsubscribe if DestroyImmediate is called
#endif
#if !UNITY_WSA
            if (buildThread != null)
            {
                threadWork = false;
                buildThread.Abort();
                buildThread = null;
                _threadWorking = false;
            }
#endif
        }

        protected virtual void OnApplicationQuit()
        {
#if !UNITY_WSA
            if (buildThread != null)
            {
                threadWork = false;
                buildThread.Abort();
                buildThread = null;
                _threadWorking = false;
            }
#endif
        }

        protected virtual void OnDidApplyAnimationProperties()
        {
            bool clip = false;
            if (_clipFrom != animClipFrom || _clipTo != animClipTo) clip = true;
            bool resample = false;
            if (_resolution != animResolution) resample = true;
            _clipFrom = animClipFrom;
            _clipTo = animClipTo;
            _resolution = animResolution;
            Rebuild(resample);
            if (!resample && clip) GetClippedSamples();
        }

        /// <summary>
        /// Rebuild the SplineUser. This will cause Build and Build_MT to be called.
        /// </summary>
        /// <param name="sampleComputer">Should the SplineUser sample the SplineComputer</param>
        public virtual void Rebuild(bool sampleComputer)
        {
            if (sampleUser)
            {
                sampleComputer = false;
                getClippedSamples = true;
            }
#if UNITY_EDITOR
            //If it's the editor and it's not playing, then rebuild immediate
            if (Application.isPlaying)
            {
                if (!autoUpdate) return;
                rebuild = true;
                if (sampleComputer)
                {
                    sample = true;
                    if (threadWorking) StartCoroutine(UpdateSubscribersRoutine());
                }
            } else RebuildImmediate(sampleComputer);
#else
             if (!autoUpdate) return;
             rebuild = true;
             if (sampleComputer)
             {
                sample = true;
                if (threadWorking) StartCoroutine(UpdateSubscribersRoutine());
             }
#endif
        }

        IEnumerator UpdateSubscribersRoutine()
        {
            while (rebuild) yield return null;
            UpdateSubscribers();
        }

        /// <summary>
        /// Rebuild the SplineUser immediate. This method will call sample samples and call Build as soon as it's called even if the component is disabled.
        /// </summary>
        /// <param name="sampleComputer">Should the SplineUser sample the SplineComputer</param>
        public virtual void RebuildImmediate(bool sampleComputer)
        {
            if (sampleUser)
            {
                sampleComputer = false;
                GetClippedSamples();
            }
#if UNITY_EDITOR
#if !UNITY_2018_3_OR_NEWER
            if (PrefabUtility.GetPrefabType(gameObject) == PrefabType.Prefab) return;
#endif
#endif
            if (threadWork) {
#if !UNITY_WSA
                if (sampleComputer) threadSample = true;
                buildThread.Interrupt();
                StartCoroutine(UpdateSubscribersRoutine());
#else
                threadWork = threadSample = false;
#endif
            }
            else
            {
                if (sampleComputer) SampleComputer();
                else if (getClippedSamples) GetClippedSamples();
                UpdateSubscribers();
                Build();
                PostBuild();
            }
            rebuild = false;
            sampleComputer = false;
            getClippedSamples = false;
        }

        public void GetClippedSamplesImmediate()
        {
            GetClippedSamples();
            if(sample) getClippedSamples = true;
        }

        /// <summary>
        /// Enter a junction address.
        /// </summary>
        /// <param name="element">The address element to add to the address</param>
        public virtual void EnterAddress(Node node, int connectionIndex, Spline.Direction direction = Spline.Direction.Forward)
        {
            if (sampleUser) return;
            int lastDepth = _address.depth;
            address.AddSpline(node, connectionIndex, direction);
            if (_address.depth != lastDepth) Rebuild(true);
        }

        /// <summary>
        /// Enter a junction address.
        /// </summary>
        /// <param name="element">The address element to add to the address</param>
        public virtual void AddComputer(SplineComputer computer, int connectionIndex, int connectedIndex, Spline.Direction direction = Spline.Direction.Forward)
        {
            if (sampleUser) return;
            int lastDepth = _address.depth;
            address.AddSpline(computer, connectionIndex, connectedIndex, direction);
            if (_address.depth != lastDepth) Rebuild(true);
        }

        public virtual void CollapseAddress()
        {
            if (sampleUser) return;
            address.Collapse();
            Rebuild(true);
        }

        /// <summary>
        /// Clear the junction address.
        /// </summary>
        public virtual void ClearAddress()
        {
            if (sampleUser) return;
            int lastDepth = _address.depth;
            _address.Clear();
            if (_address.depth != lastDepth) Rebuild(true);
        }

        /// <summary>
        /// Exit junction address.
        /// </summary>
        /// <param name="depth">How many address elements to exit</param>
        public virtual void ExitAddress(int depth)
        {
            if (sampleUser) return;
            int lastDepth = _address.depth;
            _address.Exit(depth);
            if (_address.depth != lastDepth) Rebuild(true);
        }

        private void Update()
        {
            if (updateMethod == UpdateMethod.Update) RunMain();
        }

        private void LateUpdate()
        {
            if (updateMethod == UpdateMethod.LateUpdate) RunMain();
        }

        private void FixedUpdate()
        {
            if (updateMethod == UpdateMethod.FixedUpdate) RunMain();
        }

        void UpdateSubscribers()
        {
            for (int i = subscribers.Length - 1; i >= 0; i--)
            {
                if (subscribers[i] == null) RemoveSubscriber(i);
                else subscribers[i].RebuildImmediate(false);
            }
        }

        //Update logic for handling threads and rebuilding
        private void RunMain()
        {
            Run();
            //Handle threading
#if UNITY_EDITOR
            if (multithreaded) threadWork = Application.isPlaying && System.Environment.ProcessorCount > 1;
            else threadWork = postThread = false;
#else
            if (multithreaded) threadWork = System.Environment.ProcessorCount > 1; //Don't check Application.isplaying if it's not the UnityEditor
            else threadWork = postThread = false;
#endif
            //Handle multithreading
            if (threadWork)
            {
#if !UNITY_WSA
                if (postThread)
                {
                    PostBuild();
                    postThread = false;
                }
                if (buildThread == null)
                {
                    buildThread = new Thread(RunThread);
                    buildThread.Start();
                } else if (!buildThread.IsAlive)
                {
                    Debug.Log("Thread died - unknown error");
                    buildThread = new Thread(RunThread);
                    buildThread.Start();
                }
#else
                threadWork = false;
#endif
            }
            else if (_threadWorking)
            {
#if !UNITY_WSA
                buildThread.Abort();
                buildThread = null;
#endif
                _threadWorking = false;
            }

            //Handle rebuilding
            if (rebuild && this.enabled)
            {
                if (_threadWorking)
                {
#if !UNITY_WSA
                    threadSample = sample;
                    buildThread.Interrupt();
                    sample = false;
#else
                    _threadWorking = false;
#endif
                }
                else
                {
                    if (sample)
                    {
                        SampleComputer();
                        sample = false;
                        UpdateSubscribers();
                    }
                    else if (getClippedSamples)
                    {
                        GetClippedSamples();
                        UpdateSubscribers();
                    }
                    Build();
                    PostBuild();
                }
                rebuild = false;
            }
            LateRun();
        }

#if !UNITY_WSA
        //Update logic for threads.
        private void RunThread()
        {
            lock (locker)
            {
                _threadWorking = true;
            }
            while (true)
            {
                try
                {
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (ThreadInterruptedException)
                {
                    lock (locker)
                    {
                        if (threadSample)
                        {
                            SampleComputer();
                            threadSample = false;
                        } else if (getClippedSamples) GetClippedSamples();
                        Build();
                        postThread = true;
                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
            }
        }
#endif

        /// Code to run every Update/FixedUpdate/LateUpdate before any building has taken place
        protected virtual void Run()
        {

        }

        /// Code to run every Update/FixedUpdate/LateUpdate after any rabuilding has taken place
        protected virtual void LateRun()
        {

        }

        //Used for calculations. Called on the main or the worker thread.
        protected virtual void Build()
        {
        }

        //Called on the Main thread only - used for applying the results from Build
        protected virtual void PostBuild()
        {

        }

        public void SetClipRange(double from, double to)
        {
            if (!rootUser.computer.isClosed && to < from) to = from;
            _clipFrom = DMath.Clamp01(from);
            _clipTo = DMath.Clamp01(to);
            GetClippedSamples();
            Rebuild(false);
        }

        //Sample the computer
        private void SampleComputer()
        {
            if (computer == null) return;
            if (computer.pointCount == 0) return;
            if(computer.pointCount == 1)
            {
                if (_samples.Length != 1)
                {
                    _samples = new SplineResult[1];
                    _samples[0] = new SplineResult();
                }
                _address.Evaluate(_samples[0], 0.0);
                return;
            }
            if (_resolution == 0f)
            {
                if (_samples.Length != 0) _samples = new SplineResult[0];
                _clippedSamples = new SplineResult[0];
                return;
            }
            double moveStep = _address.moveStep / _resolution;
            int fullIterations = DMath.CeilInt(1.0 / moveStep) + 1;
            if (_samples.Length != fullIterations)
            {
                _samples = new SplineResult[fullIterations];
                for (int i = 0; i < _samples.Length; i++) _samples[i] = new SplineResult();
            }
            if (uniformSample)
            {
                float lengthStep = computer.CalculateLength() / (fullIterations-1);
                _address.Evaluate(_samples[0], 0.0);
                _samples[0].percent = 0.0;
                for (int i = 1; i < fullIterations-1; i++) _address.Evaluate(_samples[i], _address.Travel(_samples[i-1].percent, lengthStep, Spline.Direction.Forward, fullIterations));
                if (computer.isClosed) _samples[samples.Length - 1] = new SplineResult(_samples[0]);
                else _address.Evaluate(_samples[_samples.Length - 1], 1.0);
                _samples[_samples.Length - 1].percent = 1.0;
            }
            else
            {
                for (int i = 0; i < fullIterations; i++)
                {
                    double eval = (double)i / (fullIterations - 1);
                    if (computer.isClosed && i == fullIterations - 1) eval = 0.0;
                    _address.Evaluate(_samples[i], eval);
                    _samples[i].percent = eval;
                }
            }
            if (_samples.Length == 0)
            {
                _clippedSamples = new SplineResult[0];
                GetClippedSamples();
                return;
            }
            if (computer.isClosed && clipTo == 1.0) _samples[_samples.Length - 1] = new SplineResult(_samples[0]); //Handle closed splines
            _samples[_samples.Length - 1].percent = 1.0;
            GetClippedSamples();
        }

        /// <summary>
        /// Gets the clipped samples defined by clipFrom and clipTo
        /// </summary>
        private void GetClippedSamples()
        {
            getClippedSamples = false;
            if (span == 1.0 && !samplesAreLooped)
            {
                _clippedSamples = samples;
                return;
            }

            double clipFromValue = clipFrom * (samples.Length - 1);
            double clipToValue = clipTo * (samples.Length - 1);

            int clipFromIndex = DMath.FloorInt(clipFromValue);
            int clipToIndex = DMath.CeilInt(clipToValue);

            if (samplesAreLooped) //Handle looping segments
            {
                if (_uniformSample)
                {
                    int endCount = 0, startCount = 0;
                    int startIndex = -1;
                    for (int i = 0; i < samples.Length-1; i++) //iterate through all samples but skip the last one as it's the same as the first one
                    {
                        if (samples[i].percent > clipFrom)
                        {
                            endCount++;
                            if (startIndex < 0) startIndex = i - 1;
                        } else if (samples[i].percent < clipTo) startCount++;
                        
                    }
                    endCount += 1;
                    if (_clippedSamples.Length != endCount + startCount || _clippedSamples == samples) _clippedSamples = new SplineResult[endCount+startCount];
                    for (int i = 1; i <= endCount; i++) _clippedSamples[i] = _samples[startIndex + i];
                    for (int i = 0; i < startCount-1; i++) _clippedSamples[i + endCount + 1] = _samples[i];
                    _clippedSamples[0] = Evaluate(clipFrom);
                    _clippedSamples[_clippedSamples.Length - 1] = Evaluate(clipTo);
                    return;
                }

                int toSamples = DMath.CeilInt(clipToValue)+1;
                int fromSamples = samples.Length - DMath.FloorInt(clipFromValue)-1;
                if (_clippedSamples.Length != toSamples + fromSamples) _clippedSamples = new SplineResult[toSamples + fromSamples];
                _clippedSamples[0] = Evaluate(_clipFrom);
                for (int i = 1; i < fromSamples; i++) _clippedSamples[i] = samples[samples.Length - fromSamples + i - 1];
                for (int i = 0; i < toSamples - 1; i++) _clippedSamples[fromSamples + i] = new SplineResult(samples[i]);
                _clippedSamples[_clippedSamples.Length-1] = Evaluate(_clipTo);
                return;
            }

            if (_uniformSample)
            {
                int count = 0;
                int startIndex = -1;
                for (int i = 0; i < samples.Length; i++)
                {
                    if (samples[i].percent > clipFrom && samples[i].percent < clipTo)
                    {
                        count++;
                        if (startIndex < 0) startIndex = i-1;
                    }
                }
                count += 2;
                if (_clippedSamples.Length != count || _clippedSamples == samples) _clippedSamples = new SplineResult[count];
                for (int i = 1; i < _clippedSamples.Length - 1; i++) _clippedSamples[i] = samples[startIndex + i];
                _clippedSamples[0] = Evaluate(clipFrom);
                _clippedSamples[_clippedSamples.Length - 1] = Evaluate(clipTo);
                return;
            }

            int clippedIterations = DMath.CeilInt(clipToValue) - DMath.FloorInt(clipFromValue) + 1;
            if (_clippedSamples.Length != clippedIterations || _clippedSamples == samples) _clippedSamples = new SplineResult[clippedIterations];
            if (clipFromIndex + 1 < samples.Length) _clippedSamples[0] = SplineResult.Lerp(samples[clipFromIndex], samples[clipFromIndex + 1], clipFromValue - clipFromIndex);
            for (int i = 1; i < _clippedSamples.Length - 1; i++) _clippedSamples[i] = samples[clipFromIndex + i];
            if (clipToIndex - 1 >= 0) _clippedSamples[_clippedSamples.Length - 1] = SplineResult.Lerp(samples[clipToIndex], samples[clipToIndex - 1], clipToIndex - clipToValue);
        }

        /// <summary>
        /// Evaluate the sampled samples
        /// </summary>
        /// <param name="percent">Percent [0-1] of evaulation</param>
        /// <returns></returns>
        public virtual SplineResult Evaluate(double percent)
        {
            if (samples.Length == 0) return new SplineResult();
            if (samples.Length == 1) return samples[0];

            //Uniform samples should be handled differently
            if (_uniformSample && _uniformPreserveClipRange)
            {
                double minDelta = 1.0;
                int closestIndex = 0;
                for (int i = 0; i < samples.Length; i++)
                {
                    double delta = DMath.Abs(percent - samples[i].percent);
                    if (delta < minDelta)
                    {
                        minDelta = delta;
                        closestIndex = i;
                    }
                }
                if (percent > samples[closestIndex].percent) return SplineResult.Lerp(samples[closestIndex], samples[closestIndex + 1], Mathf.InverseLerp((float)samples[closestIndex].percent, (float)samples[closestIndex + 1].percent, (float)percent));
                else if (percent < samples[closestIndex].percent) return SplineResult.Lerp(samples[closestIndex - 1], samples[closestIndex], Mathf.InverseLerp((float)samples[closestIndex - 1].percent, (float)samples[closestIndex].percent, (float)percent));
                else return new SplineResult(samples[closestIndex]);
            }

            percent = DMath.Clamp01(percent);
            int index = GetSampleIndex(percent);
            double percentExcess = (samples.Length - 1) * percent - index;
            if (percentExcess > 0.0 && index < samples.Length - 1) return SplineResult.Lerp(samples[index], samples[index + 1], percentExcess);
            else return new SplineResult(samples[index]);
        }

        /// <summary>
        /// Evaluate the sampled samples
        /// </summary>
        /// <param name="percent">Percent [0-1] of evaulation</param>
        /// <returns></returns>
        public virtual void Evaluate(SplineResult result, double percent)
        {
            if (samples.Length == 0)
            {
                result = new SplineResult();
                return;
            }
            if (samples.Length == 1)
            {
                result.CopyFrom(samples[0]);
                return;
            }
            if (_uniformSample && _uniformPreserveClipRange)
            {
                double minDelta = 1.0;
                int closestIndex = 0;
                for (int i = 0; i < samples.Length; i++)
                {
                    double delta = DMath.Abs(percent - samples[i].percent);
                    if (delta < minDelta)
                    {
                        minDelta = delta;
                        closestIndex = i;
                    }
                }
                if (percent > samples[closestIndex].percent) SplineResult.Lerp(samples[closestIndex], samples[closestIndex + 1], Mathf.InverseLerp((float)samples[closestIndex].percent, (float)samples[closestIndex + 1].percent, (float)percent), result);
                else if (percent < samples[closestIndex].percent) SplineResult.Lerp(samples[closestIndex - 1], samples[closestIndex], Mathf.InverseLerp((float)samples[closestIndex - 1].percent, (float)samples[closestIndex].percent, (float)percent), result);
                else  result.CopyFrom(samples[closestIndex]);
            }
            else
            {
                percent = DMath.Clamp01(percent);
                int index = GetSampleIndex(percent);
                double percentExcess = (samples.Length - 1) * percent - index;
                if (percentExcess > 0.0 && index < samples.Length - 1) SplineResult.Lerp(samples[index], samples[index + 1], percentExcess, result);
                else result.CopyFrom(samples[index]);
            }
        }

        /// <summary>
        /// Evaluate the sampled samples' positions
        /// </summary>
        /// <param name="percent">Percent [0-1] of evaulation</param>
        /// <returns></returns>
        public virtual Vector3 EvaluatePosition(double percent, bool overrideUniformClipRange = false)
        {
            if (samples.Length == 0) return Vector3.zero;
            if (samples.Length == 1) return samples[0].position;
            percent = DMath.Clamp01(percent);
            //Uniform samples should be handled differently
            if (_uniformSample && overrideUniformClipRange)
            {
                double minDelta = 1.0;
                int closestIndex = 0;
                for (int i = 0; i < samples.Length; i++)
                {
                    double delta = DMath.Abs(percent - samples[i].percent);
                    if(delta < minDelta)
                    {
                        minDelta = delta;
                        closestIndex = i;
                    }
                }
                if (percent > samples[closestIndex].percent) return Vector3.Lerp(samples[closestIndex].position, samples[closestIndex + 1].position, Mathf.InverseLerp((float)samples[closestIndex].percent, (float)samples[closestIndex + 1].percent, (float)percent));
                else if (percent < samples[closestIndex].percent) return Vector3.Lerp(samples[closestIndex - 1].position, samples[closestIndex].position, Mathf.InverseLerp((float)samples[closestIndex - 1].percent, (float)samples[closestIndex].percent, (float)percent));
                else return samples[closestIndex].position;
            }

            int index = GetSampleIndex(percent);
            double percentExcess = (samples.Length - 1) * percent - index;
            if (percentExcess > 0.0 && index < samples.Length - 1) return Vector3.Lerp(samples[index].position, samples[index + 1].position, (float)percentExcess);
            else return samples[index].position;
        }

        /// <summary>
        /// Takes a regular 0-1 percent mapped to the start and end of the spline and maps it to the clipFrom and clipTo valies. Useful for working with clipped samples
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        public double ClipPercent(double percent)
        {
            ClipPercent(ref percent);
            return percent;
        }

        /// <summary>
        /// Takes a regular 0-1 percent mapped to the start and end of the spline and maps it to the clipFrom and clipTo valies. Useful for working with clipped samples
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        public void ClipPercent(ref double percent)
        {
            if(_clippedSamples.Length == 0)
            {
               percent = 0.0;
               return;
            }
            double from = _clippedSamples[0].percent;
            double to = _clippedSamples[_clippedSamples.Length - 1].percent;
            if (samplesAreLooped)
            {
                if (percent >= from && percent <= 1.0) { percent = DMath.InverseLerp(from, from + span, percent); }//If in the range clipFrom - 1.0
                else if (percent <= to) { percent = DMath.InverseLerp(to - span, to, percent); } //if in the range 0.0 - clipTo
                else
                {
                    //Find the nearest clip start
                    if (DMath.InverseLerp(to, from, percent) < 0.5) percent = 1.0;
                    else percent = 0.0;
                }
            } else percent = DMath.InverseLerp(from, to, percent);
        }

        public double UnclipPercent(double percent)
        {
            UnclipPercent(ref percent);
            return percent;
        }

        public void UnclipPercent(ref double percent)
        {
            double from = _clippedSamples[0].percent;
            double to = _clippedSamples[_clippedSamples.Length - 1].percent;
            if (samplesAreLooped)
            {
                double fromLength = (1.0 - from) / span;
                if (fromLength == 0.0)
                {
                    percent = 0.0;
                    return;
                }
                if (percent < fromLength) percent = DMath.Lerp(from, 1.0, percent / fromLength);
                else if (to == 0.0)
                {
                    percent = 0.0;
                    return;
                } else percent = DMath.Lerp(0.0, to, (percent - fromLength) / (to / span));
            }
            else percent = DMath.Lerp(from, to, percent);
            percent = DMath.Clamp01(percent);
        }

        /// <summary>
        /// Get the index of the sampled result at percent
        /// </summary>
        /// <param name="percent">Percent [0-1] of evaulation</param>
        /// <returns></returns>
        public int GetSampleIndex(double percent)
        {
            return DMath.FloorInt(percent * (samples.Length - 1));
        }

        /// <summary>
        /// Get the index of the clipped sample at percent
        /// </summary>
        /// <param name="percent">Percent [0-1] of evaulation</param>
        /// <returns></returns>
        public int GetClippedSampleIndex(double percent)
        {
            return DMath.FloorInt(percent * (clippedSamples.Length - 1));
        }

        /// <summary>
        /// Project a point onto the sampled SplineComputer
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <param name="from">Start check from</param>
        /// <param name="to">End check at</param>
        /// <returns></returns>
        public virtual SplineResult Project(Vector3 point, double from = 0.0, double to = 1.0)
        {
            SplineResult result = new SplineResult();
            Project(result, point, from, to);
            return result;
        }

        public virtual void Project(SplineResult result, Vector3 point, double from = 0.0, double to = 1.0)
        {
            if (samples.Length == 0) return;
            if (samples.Length == 1)
            {
                if (result == null) result = new SplineResult(samples[0]);
                else result.CopyFrom(samples[0]);
                return;
            }
            if (computer == null)
            {
                result = new SplineResult();
                return;
            }
            //First make a very rough sample of the from-to region 
            int steps = (computer.pointCount - 1) * 6; //Sampling six points per segment is enough to find the closest point range
            int step = samples.Length / steps;
            if (step < 1) step = 1;
            float minDist = (point - samples[0].position).sqrMagnitude;
            int fromIndex = 0;
            int toIndex = samples.Length - 1;
            if (from != 0.0) fromIndex = GetSampleIndex(from);
            if (to != 1.0) toIndex = Mathf.CeilToInt((float)to * (samples.Length - 1));
            int checkFrom = fromIndex;
            int checkTo = toIndex;

            //Find the closest point range which will be checked in detail later
            for (int i = fromIndex; i <= toIndex; i += step)
            {
                if (i > toIndex) i = toIndex;
                float dist = (point - samples[i].position).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    checkFrom = Mathf.Max(i - step, 0);
                    checkTo = Mathf.Min(i + step, samples.Length - 1);
                }
                if (i == toIndex) break;
            }
            minDist = (point - samples[checkFrom].position).sqrMagnitude;

            int index = checkFrom;
            //Find the closest result within the range
            for (int i = checkFrom + 1; i <= checkTo; i++)
            {
                float dist = (point - samples[i].position).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    index = i;
                }
            }
            //Project the point on the line between the two closest samples
            int backIndex = index - 1;
            if (backIndex < 0) backIndex = 0;
            int frontIndex = index + 1;
            if (frontIndex > samples.Length - 1) frontIndex = samples.Length - 1;
            Vector3 back = LinearAlgebraUtility.ProjectOnLine(samples[backIndex].position, samples[index].position, point);
            Vector3 front = LinearAlgebraUtility.ProjectOnLine(samples[index].position, samples[frontIndex].position, point);
            float backLength = (samples[index].position - samples[backIndex].position).magnitude;
            float frontLength = (samples[index].position - samples[frontIndex].position).magnitude;
            float backProjectDist = (back - samples[backIndex].position).magnitude;
            float frontProjectDist = (front - samples[frontIndex].position).magnitude;
            if (backIndex < index && index < frontIndex)
            {
                if ((point - back).sqrMagnitude < (point - front).sqrMagnitude) SplineResult.Lerp(samples[backIndex], samples[index], backProjectDist / backLength, result);
                else SplineResult.Lerp(samples[frontIndex], samples[index], frontProjectDist / frontLength, result);
            } else if (backIndex < index) SplineResult.Lerp(samples[backIndex], samples[index], backProjectDist / backLength, result);
            else SplineResult.Lerp(samples[frontIndex], samples[index], frontProjectDist / frontLength, result);
            if(from == 0.0 && to == 1.0 && result.percent < _address.moveStep / _resolution) //Handle looped splines
            {
                Vector3 projected = LinearAlgebraUtility.ProjectOnLine(samples[samples.Length - 1].position, samples[samples.Length - 2].position, point);
                if((point-projected).sqrMagnitude < (point - result.position).sqrMagnitude)
                {
                    SplineResult.Lerp(samples[samples.Length - 1], samples[samples.Length - 2], LinearAlgebraUtility.InverseLerp(samples[samples.Length - 1].position, samples[samples.Length - 2].position, projected), result);
                }
            }
        }

        /// <summary>
        /// Returns the percent from the spline at a given distance from the start point
        /// </summary>
        /// <param name="start">The start point</param>
        /// /// <param name="distance">The distance to travel</param>
        /// <param name="direction">The direction towards which to move</param>
        /// <returns></returns>
        public virtual double Travel(double start, float distance, Spline.Direction direction)
        {
            if (samples.Length <= 1) return 0.0;
            if (direction == Spline.Direction.Forward && start >= 1.0) return 1.0;
            else if (direction == Spline.Direction.Backward && start <= 0.0) return 0.0;
            if (distance == 0f) return DMath.Clamp01(start);
            float moved = 0f;
            Vector3 lastPosition = EvaluatePosition(start);
            double lastPercent = start;
            int nextSampleIndex = direction == Spline.Direction.Forward ? DMath.CeilInt(start * (samples.Length - 1)) : DMath.FloorInt(start * (samples.Length - 1));
            float lastDistance = 0f;
            while(true)
            {
                lastDistance = Vector3.Distance(samples[nextSampleIndex].position, lastPosition);
                lastPosition = samples[nextSampleIndex].position;
                moved += lastDistance;
                if (moved >= distance) break; 
                lastPercent = samples[nextSampleIndex].percent;
                if (direction == Spline.Direction.Forward)
                {
                    if (nextSampleIndex == samples.Length - 1) break;
                    nextSampleIndex++;
                }
                else
                {
                    if (nextSampleIndex == 0) break;
                    nextSampleIndex--;
                }
            }
            return DMath.Lerp(lastPercent, samples[nextSampleIndex].percent, 1f - (moved - distance) / lastDistance);
        }

        //-----------Subscribing logic for users that reference a SplineUser instad of a SplineComputer

        /// <summary>
        /// Subscribe a SplineUser to this User. This will rebuild the user automatically when there are changes.
        /// </summary>
        /// <param name="input">The SplineUser to subscribe</param>
        private void Subscribe(SplineUser input)
        {
            if (input == this) return;
            int emptySlot = -1;
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (subscribers[i] == input) return;
                else if (subscribers[i] == null && emptySlot < 0) emptySlot = i;
            }
            if (emptySlot >= 0) subscribers[emptySlot] = input;
            else
            {
                SplineUser[] newSubscribers = new SplineUser[subscribers.Length + 1];
                subscribers.CopyTo(newSubscribers, 0);
                newSubscribers[subscribers.Length] = input;
                subscribers = newSubscribers;
            }
        }

        /// <summary>
        /// Unsubscribe a SplineUser from this computer's updates
        /// </summary>
        /// <param name="input">The SplineUser to unsubscribe</param>
        private void Unsubscribe(SplineUser input)
        {
            int removeSlot = -1;
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (subscribers[i] == input)
                {
                    removeSlot = i;
                    break;
                }
            }
            if (removeSlot < 0) return;
            SplineUser[] newSubscribers = new SplineUser[subscribers.Length - 1];
            int index = subscribers.Length - 1;
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (index == removeSlot) continue;
                else if (i < index) newSubscribers[i] = subscribers[i];
                else newSubscribers[i - 1] = subscribers[i - 1];
            }
            subscribers = newSubscribers;
        }

        /// <summary>
        /// Calculate the length of the sampled spline
        /// </summary>
        /// <param name="from">Calculate from [0-1] default: 0f</param>
        /// <param name="to">Calculate to [0-1] default: 1f</param>
        /// <returns></returns>
        public virtual float CalculateLength(double from = 0.0, double to = 1.0) {
            float length = 0f;
            Vector3 pos = EvaluatePosition(from);
            int sampleIndex = DMath.CeilInt(from * (samples.Length-1));
            int endSampleIndex = GetSampleIndex(to);
            for (int i = sampleIndex; i < endSampleIndex; i++)
            {
                length += Vector3.Distance(samples[i].position, pos);
                pos = samples[i].position;
            }
            length += Vector3.Distance(EvaluatePosition(to), pos);
            return length;
        }

        private void RemoveSubscriber(int index)
        {
            SplineUser[] newSubscribers = new SplineUser[subscribers.Length - 1];
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (i == index) continue;
                else if (i < index) newSubscribers[i] = subscribers[i];
                else newSubscribers[i - 1] = subscribers[i];
            }
            subscribers = newSubscribers;
        }

        private bool IsSubscribed(SplineUser user)
        {
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (subscribers[i] == user)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
