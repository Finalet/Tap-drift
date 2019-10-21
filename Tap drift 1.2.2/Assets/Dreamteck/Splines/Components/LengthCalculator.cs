using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEngine.Events;

namespace Dreamteck.Splines
{
    [AddComponentMenu("Dreamteck/Splines/Length Calculator")]
    public class LengthCalculator : SplineUser
    {
        [System.Serializable]
        public class LengthEvent
        {
            public bool enabled = true;
            public float targetLength = 0f;
            public SplineAction action = new SplineAction();
            public enum Type { Growing, Shrinking, Both}
            public Type type = Type.Both;

            public LengthEvent()
            {

            }

            public LengthEvent(Type t)
            {
                type = t;
            }

            public LengthEvent(Type t, SplineAction a)
            {
                type = t;
                action = a;
            }

            public void Check(float fromLength, float toLength)
            {
                if (!enabled) return;
                bool condition = false;
                switch (type)
                {
                    case Type.Growing: condition = toLength >= targetLength && fromLength < targetLength; break;
                    case Type.Shrinking: condition = toLength <= targetLength && fromLength > targetLength; break;
                    case Type.Both: condition = toLength >= targetLength && fromLength < targetLength || toLength <= targetLength && fromLength > targetLength; break;
                }
                if (condition) action.Invoke();
            }
        }
        [HideInInspector]
        public LengthEvent[] lengthEvents = new LengthEvent[0];
        [HideInInspector]
        public float idealLength = 1f;
        private float _length = 0f;
        private float lastLength = 0f;
        public float length
        {
            get {
                return _length;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _length = _address.CalculateLength(clipFrom, clipTo);
            lastLength = _length;
            for (int i = 0; i < lengthEvents.Length; i++)
            {
                if (lengthEvents[i].targetLength == _length) lengthEvents[i].action.Invoke();
            }
        }

        protected override void Build()
        {
            base.Build();
            _length = CalculateLength(clipFrom, clipTo);
            if (lastLength != _length)
            {
                for (int i = 0; i < lengthEvents.Length; i++)
                {
                    lengthEvents[i].Check(lastLength, _length);
                }
                lastLength = _length;
            }
        }

        private void AddEvent(LengthEvent lengthEvent)
        {
            LengthEvent[] newEvents = new LengthEvent[lengthEvents.Length + 1];
            lengthEvents.CopyTo(newEvents, 0);
            newEvents[newEvents.Length - 1] = lengthEvent;
            lengthEvents = newEvents;
        }

        public void AddEvent(LengthEvent.Type t, UnityAction call, float targetLength = 0f, LengthEvent.Type type = LengthEvent.Type.Both)
        {
            LengthEvent newEvent = new LengthEvent(t, new SplineAction(call));
            newEvent.targetLength = targetLength;
            newEvent.type = type;
            AddEvent(newEvent);
        }

        public void AddEvent(LengthEvent.Type t, UnityAction<int> call, int value, float targetLength = 0f, LengthEvent.Type type = LengthEvent.Type.Both)
        {
            LengthEvent newEvent = new LengthEvent(t, new SplineAction(call, value));
            newEvent.targetLength = targetLength;
            newEvent.type = type;
            AddEvent(newEvent);
        }

        public void AddEvent(LengthEvent.Type t, UnityAction<float> call, float value, float targetLength = 0f, LengthEvent.Type type = LengthEvent.Type.Both)
        {
            LengthEvent newEvent = new LengthEvent(t, new SplineAction(call, value));
            newEvent.targetLength = targetLength;
            newEvent.type = type;
            AddEvent(newEvent);
        }

        public void AddEvent(LengthEvent.Type t, UnityAction<double> call, double value, float targetLength = 0f, LengthEvent.Type type = LengthEvent.Type.Both)
        {
            LengthEvent newEvent = new LengthEvent(t, new SplineAction(call, value));
            newEvent.targetLength = targetLength;
            newEvent.type = type;
            AddEvent(newEvent);
        }

        public void AddTrigger(LengthEvent.Type t, UnityAction<string> call, string value, float targetLength = 0f, LengthEvent.Type type = LengthEvent.Type.Both)
        {
            LengthEvent newEvent = new LengthEvent(t, new SplineAction(call, value));
            newEvent.targetLength = targetLength;
            newEvent.type = type;
            AddEvent(newEvent);
        }

        public void AddEvent(LengthEvent.Type t, UnityAction<bool> call, bool value, float targetLength = 0f, LengthEvent.Type type = LengthEvent.Type.Both)
        {
            LengthEvent newEvent = new LengthEvent(t, new SplineAction(call, value));
            newEvent.targetLength = targetLength;
            newEvent.type = type;
            AddEvent(newEvent);
        }

        public void AddEvent(LengthEvent.Type t, UnityAction<GameObject> call, GameObject value, float targetLength = 0f, LengthEvent.Type type = LengthEvent.Type.Both)
        {
            LengthEvent newEvent = new LengthEvent(t, new SplineAction(call, value));
            newEvent.targetLength = targetLength;
            newEvent.type = type;
            AddEvent(newEvent);
        }

        public void AddEvent(LengthEvent.Type t, UnityAction<Transform> call, Transform value, float targetLength = 0f, LengthEvent.Type type = LengthEvent.Type.Both)
        {
            LengthEvent newEvent = new LengthEvent(t, new SplineAction(call, value));
            newEvent.targetLength = targetLength;
            newEvent.type = type;
            AddEvent(newEvent);
        }
    }
}
