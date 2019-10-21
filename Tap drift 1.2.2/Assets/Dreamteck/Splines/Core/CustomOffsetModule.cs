using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines
{
    [System.Serializable]
    public class CustomOffsetModule
    {
        [System.Serializable]
        public class Key
        {
            public Vector2 offset
            {
                get { return _offset; }
                set { _offset = value;  }
            }

            public double center
            {
                get { return _center; }
                set { _center = DMath.Clamp01(value); }
            }

            public double from
            {
                get { return _from; }
                set { _from = DMath.Clamp01(value); }
            }

            public double to
            {
                get { return _to; }
                set { _to = DMath.Clamp01(value); }
                
            }

            public bool loop
            {
                get { return _loop; }
                set { _loop = value; }
            }

            public double position
            {
                get {
                    if (from > to)
                    {
                        double pos = DMath.Lerp(_from, _to, _center);
                        double fromToEndDistance = 1.0 - _from;
                        double centerDistance = _center * (fromToEndDistance + _to);
                        pos = _from + centerDistance;
                        if (pos > 1.0) pos -= 1.0;
                        return pos;
                    } else  return DMath.Lerp(_from, _to, _center);

                }
                set
                {
                    double delta = value - position;
                    from += delta;
                    to += delta;
                    center = DMath.InverseLerp(from, to, value);
                }
            }

            [SerializeField]
            private Vector2 _offset = Vector2.zero;
            [SerializeField]
            private double _from = 0.0;
            [SerializeField]
            private double _to = 0.0;
            [SerializeField]
            private double _center = 0.0;
            [SerializeField]
            private bool _loop = false;
            public AnimationCurve interpolation;

            public Key(Vector2 o, double f, double t, double c)
            {
                _offset = o;
                from = f;
                to = t;
                center = c;
                interpolation = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            }

            public float Evaluate(float t)
            {
                return interpolation.Evaluate(t);
            }
        }
        public List<Key> keys = new List<Key>();
        public float blend
        {
            get { return _blend; }
            set { _blend = Mathf.Clamp01(value); }
        }
        [SerializeField]
        private float _blend = 1f;

        public CustomOffsetModule()
        {
            keys = new List<Key>();
        }

        public void AddKey(Vector2 offset, double f, double t, double c)
        {
            keys.Add(new Key(offset, f, t, c));
        }

        public Vector2 Evaluate(double time)
        {
            if (keys.Count == 0) return Vector2.zero;
            Vector2 offset = Vector2.zero;
            for(int i = 0; i < keys.Count; i++)
            {
                double position = keys[i].position;
                float lerp = 0f;
                if (keys[i].from > keys[i].to) //Handle looping segments
                {
                    if (position >= keys[i].from) //Center is within the [from-1.0] range
                    {
                        //Determine where the current sample is
                        if (time > keys[i].from)
                        {
                            if (time <= position) lerp = Mathf.Clamp01(keys[i].Evaluate((float)DMath.InverseLerp(keys[i].from, position, time))) * _blend;
                            else lerp = Mathf.Clamp01(keys[i].Evaluate((float)DMath.InverseLerp(1.0 + keys[i].to, position, time))) * _blend;
                        }
                        else lerp = Mathf.Clamp01(keys[i].Evaluate((float)DMath.InverseLerp(keys[i].to, -(1.0 - position), time))) * _blend;
                    }
                    else //Center is within the [to-0.0] range
                    {
                        //Determine where the current sample is
                        if (time > keys[i].from) lerp = Mathf.Clamp01(keys[i].Evaluate((float)DMath.InverseLerp(keys[i].from, 1.0 + position, time))) * _blend;
                        else
                        {
                            if (time <= position) lerp = Mathf.Clamp01(keys[i].Evaluate((float)DMath.InverseLerp(-(1.0 - keys[i].from), position, time))) * _blend;
                            else lerp = Mathf.Clamp01(keys[i].Evaluate((float)DMath.InverseLerp(keys[i].to, position, time))) * _blend;
                        }
                    }
                }
                else
                {
                    if (time < position) lerp =Mathf.Clamp01(keys[i].Evaluate((float)DMath.InverseLerp(keys[i].from, position, time)))*_blend;
                    else lerp = Mathf.Clamp01(keys[i].Evaluate((float)DMath.InverseLerp(keys[i].to, position, time))) * _blend;
                }
                offset += keys[i].offset * lerp;
            }
            return offset;
        }
    }
}
