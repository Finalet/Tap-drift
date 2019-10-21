using UnityEngine;
using System.Collections;

namespace Dreamteck.Splines
{
    [ExecuteInEditMode]
    [AddComponentMenu("Dreamteck/Splines/Particle Controller")]
    public class ParticleController : SplineUser
    {
        [HideInInspector]
        public ParticleSystem _particleSystem;
        public enum EmitPoint { Beginning, Ending, Random, Ordered }
        public enum MotionType { None, UseParticleSystem, FollowForward, FollowBackward, ByNormal, ByNormalRandomized }
        public enum Wrap { Default, Loop }

        [HideInInspector]
        public bool volumetric = false;
        [HideInInspector]
        public bool emitFromShell = false;
        [HideInInspector]
        public Vector2 scale = Vector2.one;
        [HideInInspector]
        public EmitPoint emitPoint = EmitPoint.Beginning;
        [HideInInspector]
        public MotionType motionType = MotionType.UseParticleSystem;
        [HideInInspector]
        public Wrap wrapMode = Wrap.Default;
        [HideInInspector]
        public float minCycles = 1f;
        [HideInInspector]
        public float maxCycles = 2f;

        private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[0];
        private Particle[] controllers = new Particle[0];
        private float[] lifetimes = new float[0];
        private int particleCount = 0;
        private int birthIndex = 0;
        SplineResult evaluateResult = new SplineResult();

        protected override void Awake() 
        {
            base.Awake();
            updateMethod = UpdateMethod.LateUpdate;
        }

        protected override void LateRun()
        {
            if (_particleSystem == null) return;
            int maxParticles = _particleSystem.main.maxParticles;
            if (particles.Length != maxParticles)
            {
                particles = new ParticleSystem.Particle[maxParticles];
                Particle[] newControllers = new Particle[maxParticles];
                float[] newLifetimes = new float[maxParticles];
                for (int i = 0; i < newControllers.Length; i++)
                {
                    if (i >= controllers.Length) break;
                    newControllers[i] = controllers[i];
                    newLifetimes[i] = lifetimes[i];
                }
                controllers = newControllers;
                lifetimes = newLifetimes;
            }
            particleCount = _particleSystem.GetParticles(particles);

            bool isLocal = _particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.Local;

            Transform particleSystemTransform = _particleSystem.transform;
            for (int i = 0; i < particleCount; i++)
            {
                if (isLocal)
                {
                    particles[i].position = particleSystemTransform.TransformPoint(particles[i].position);
                    particles[i].velocity = particleSystemTransform.TransformDirection(particles[i].velocity);
                }
                if (controllers[i] == null ||  particles[i].remainingLifetime >= particles[i].startLifetime - Time.deltaTime) OnParticleBorn(i);
                HandleParticle(i);
                if (isLocal)
                {
                    particles[i].position = particleSystemTransform.InverseTransformPoint(particles[i].position);
                    particles[i].velocity = particleSystemTransform.InverseTransformDirection(particles[i].velocity);
                }
            }
            _particleSystem.SetParticles(particles, particleCount);


            for (int i = particleCount; i < controllers.Length; i++)
            {
                if (controllers[i] == null) break;
                controllers[i] = null;
            }
            int availableOffset = 0;
            for (int i = 0; i < particleCount; i++)
            {
                if(particles[i].remainingLifetime - Time.deltaTime <= 0f)
                {
                    controllers[i] = controllers[particleCount - 1 - availableOffset];
                    controllers[particleCount - 1 - availableOffset] = null;
                    availableOffset++;
                }
            }
        }

        protected override void Reset()
        {
            base.Reset();
            if (_particleSystem == null) _particleSystem = GetComponent<ParticleSystem>();
        }

        void HandleParticle(int index)
        {
            float lifePercent = particles[index].remainingLifetime / particles[index].startLifetime;

            if (motionType == MotionType.FollowBackward || motionType == MotionType.FollowForward || motionType == MotionType.None)
            {
                Evaluate(evaluateResult, UnclipPercent(controllers[index].GetSplinePercent(wrapMode)));
                particles[index].position = evaluateResult.position;
                if (volumetric)
                {
                    Vector3 right = -Vector3.Cross(evaluateResult.direction, evaluateResult.normal);
                    Vector2 offset = controllers[index].startOffset;
                    if (motionType != MotionType.None) offset = Vector2.Lerp(controllers[index].startOffset, controllers[index].endOffset, 1f - lifePercent);
                    particles[index].position += right * offset.x * scale.x * evaluateResult.size + evaluateResult.normal * offset.y * scale.y * evaluateResult.size;
                }
                particles[index].velocity = evaluateResult.direction;
                particles[index].startColor = controllers[index].startColor * evaluateResult.color;
            }
            controllers[index].remainingLifetime -= Time.deltaTime;
            particles[index].remainingLifetime = controllers[index].remainingLifetime;
        }

        void OnParticleDie(int index)
        {

        }

        void OnParticleBorn(int index)
        {
            birthIndex++;
            double percent = 0.0;
            float emissionRate = Mathf.Lerp(_particleSystem.emission.rateOverTime.constantMin, _particleSystem.emission.rateOverTime.constantMax, 0.5f);
            float expectedParticleCount = emissionRate * _particleSystem.main.startLifetime.constantMax;
            if (birthIndex > expectedParticleCount) birthIndex = 0;
            switch (emitPoint)
            {
                case EmitPoint.Beginning: percent = 0f; break;
                case EmitPoint.Ending: percent = 1f; break;
                case EmitPoint.Random: percent = Random.Range(0f, 1f); break;
                case EmitPoint.Ordered: percent = expectedParticleCount > 0 ? (float)birthIndex / expectedParticleCount : 0f;  break;
            }
            Evaluate(evaluateResult, UnclipPercent(percent));
            if (controllers[index] == null) controllers[index] = new Particle();
            controllers[index].startColor = particles[index].startColor;
            controllers[index].startPercent = percent;
            controllers[index].startLifetime = particles[index].startLifetime;
            controllers[index].remainingLifetime = particles[index].remainingLifetime;
          
            controllers[index].cycleSpeed = Random.Range(minCycles, maxCycles);
            Vector2 circle = Vector2.zero;
            if (volumetric)
            {
                if (emitFromShell) circle = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward) * Vector2.right;
                else circle = Random.insideUnitCircle;
            }
            controllers[index].startOffset = circle * 0.5f;
            controllers[index].endOffset = Random.insideUnitCircle * 0.5f;


            Vector3 right = Vector3.Cross(evaluateResult.direction, evaluateResult.normal);
            particles[index].position = evaluateResult.position + right * controllers[index].startOffset.x * evaluateResult.size * scale.x + evaluateResult.normal * controllers[index].startOffset.y * evaluateResult.size * scale.y;

            float forceX = _particleSystem.forceOverLifetime.x.constantMax;
            float forceY = _particleSystem.forceOverLifetime.y.constantMax;
            float forceZ = _particleSystem.forceOverLifetime.z.constantMax;
            if (_particleSystem.forceOverLifetime.randomized)
            {
                forceX = Random.Range(_particleSystem.forceOverLifetime.x.constantMin, _particleSystem.forceOverLifetime.x.constantMax);
                forceY = Random.Range(_particleSystem.forceOverLifetime.y.constantMin, _particleSystem.forceOverLifetime.y.constantMax);
                forceZ = Random.Range(_particleSystem.forceOverLifetime.z.constantMin, _particleSystem.forceOverLifetime.z.constantMax);
            }

            float time = particles[index].startLifetime - particles[index].remainingLifetime;
            Vector3 forceDistance = new Vector3(forceX, forceY, forceZ) * 0.5f * (time * time);

            float startSpeed = _particleSystem.main.startSpeed.constantMax;

            if (motionType == MotionType.ByNormal)
            {
                particles[index].position += evaluateResult.normal * startSpeed * (particles[index].startLifetime - particles[index].remainingLifetime);
                particles[index].position += forceDistance;
                particles[index].velocity = evaluateResult.normal * startSpeed + new Vector3(forceX, forceY, forceZ) * time;
            }
            else if (motionType == MotionType.ByNormalRandomized)
            {
                Vector3 normal = Quaternion.AngleAxis(Random.Range(0f, 360f), evaluateResult.direction) * evaluateResult.normal;
                particles[index].position += normal * startSpeed * (particles[index].startLifetime - particles[index].remainingLifetime);
                particles[index].position += forceDistance;
                particles[index].velocity = normal * startSpeed + new Vector3(forceX, forceY, forceZ) * time;
            }
        }

        public class Particle
        {
            internal Vector2 startOffset = Vector2.zero;
            internal Vector2 endOffset = Vector2.zero;
            internal float cycleSpeed = 0f;
            internal float startLifetime = 0f;
            internal Color startColor = Color.white;
            internal float remainingLifetime = 0f;
            internal double startPercent = 0.0;

            internal double GetSplinePercent(Wrap wrap)
            {
                switch (wrap)
                {
                    case Wrap.Default: return DMath.Clamp01(startPercent + (1f - remainingLifetime / startLifetime) * cycleSpeed);
                    case Wrap.Loop:
                        double loopPoint = startPercent + (1.0 - remainingLifetime / startLifetime) * cycleSpeed;
                        if(loopPoint > 1.0) loopPoint -= Mathf.FloorToInt((float)loopPoint);
                        return loopPoint;
                }
                return 0.0;
            }
        }
    }
}
