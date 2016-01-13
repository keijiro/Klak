//
// Klak - Utilities for creative coding with Unity
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Klak
{
    public class EnvelopeGenerator : MonoBehaviour
    {
        #region Public Nested Classes

        public enum SignalMode { Trigger, Gate }

        [System.Serializable]
        public class EnvelopeEvent : UnityEvent<float> {}

        #endregion

        #region Editable Properties

        [SerializeField]
        SignalMode _signalMode = SignalMode.Trigger;

        [SerializeField, Range(0, 1)]
        float _attackTime = 0.02f;

        [SerializeField, Range(0.01f, 1)]
        float _decayTime = 0.04f;

        [SerializeField, Range(0, 1)]
        float _sustainLevel = 0.5f;

        [SerializeField, Range(0.01f, 1)]
        float _releaseTime = 0.1f;

        [SerializeField, Range(1, 8)]
        float _exponent = 1;

        [SerializeField]
        float _amplitude = 1;

        [SerializeField]
        float _bias = 0;

        [SerializeField]
        EnvelopeEvent[] _envelopeEvents = new EnvelopeEvent[1];

        #endregion

        #region Public Properties

        public float attackTime {
            get { return _attackTime; }
            set { _attackTime = value; }
        }

        public float decayTime {
            get { return _decayTime; }
            set { _decayTime = value; }
        }

        public float sustainLevel {
            get { return _sustainLevel; }
            set { _sustainLevel = value; }
        }

        public float releaseTime {
            get { return _releaseTime; }
            set { _releaseTime = value; }
        }

        public float exponent {
            get { return _exponent; }
            set { _exponent = value; }
        }

        public float amplitude {
            get { return _amplitude; }
            set { _amplitude = value; }
        }

        public float bias {
            get { return _bias; }
            set { _bias = value; }
        }

        #endregion

        #region Public Methods

        public void Trigger()
        {
            Trigger(1);
        }

        public void Trigger(float velocity)
        {
            Assert.IsTrue(_signalMode == SignalMode.Trigger);
            _voices.Peek().Trigger(velocity);
        }

        public void NoteOn(int note)
        {
            NoteOn(note, 1);
        }

        public void NoteOn(int note, float velocity)
        {
            Assert.IsTrue(_signalMode == SignalMode.Gate);
            var v = _voices.Dequeue();
            if (!v.Available) v.NoteOff();
            v.NoteOn(note, velocity);
            _voices.Enqueue(v);
        }

        public void NoteOff(int note)
        {
            Assert.IsTrue(_signalMode == SignalMode.Gate);
            foreach (var v in _voices)
                if (v.NoteNumber == note)
                    v.NoteOff();
        }

        #endregion

        #region Voice Queue

        class Voice
        {
            public int NoteNumber { get; set; }
            public float Velocity { get; set; }
            public float NoteLength { get; set; }
            public float CurrentTime { get; set; }
            public EnvelopeEvent Event { get; set; }

            public bool Available {
                get { return NoteNumber < 0; }
            }

            public Voice(EnvelopeEvent e)
            {
                NoteNumber = -1;
                CurrentTime = 1e6f;
                Event = e;
            }

            public void Trigger(float velocity)
            {
                Velocity = velocity;
                CurrentTime = 0;
            }

            public void NoteOn(int note, float velocity)
            {
                NoteNumber = note;
                Velocity = velocity;
                NoteLength = 0;
                CurrentTime = 0;
            }

            public void NoteOff()
            {
                NoteNumber = -1;
                NoteLength = CurrentTime;
            }
        }

        Queue<Voice> _voices;

        #endregion

        #region Envelope Curve Functions

        float Exp(float level)
        {
            return Mathf.Pow(level, _exponent);
        }

        float ARCurve(float time)
        {
            if (time < _attackTime)
                return Exp(time / _attackTime); 

            if (time < _attackTime + _releaseTime)
                return Exp(1 - (time - _attackTime) / _releaseTime);

            return 0;
        }

        float ADCurve(float time)
        {
            if (time < _attackTime)
                return Exp(time / _attackTime); 

            if (time < _attackTime + _decayTime)
            {
                var c = Exp(1 - (time - _attackTime) / _decayTime);
                return Mathf.Lerp(c, 1.0f, _sustainLevel);
            }

            return _sustainLevel;
        }

        float RCurve(float time)
        {
            if (time < _releaseTime)
                return Exp(1 - time / _releaseTime);
            else
                return 0;
        }

        #endregion

        #region MonoBehaviour Functions

        void Start()
        {
            _voices = new Queue<Voice>();
            foreach (var e in _envelopeEvents)
                _voices.Enqueue(new Voice(e));
        }

        void Update()
        {
            var dt = Time.deltaTime;

            foreach (var v in _voices)
            {
                v.CurrentTime += dt;

                var env = 0.0f;

                if (_signalMode == SignalMode.Trigger)
                {
                    env = ARCurve(v.CurrentTime);
                }
                else if (v.NoteNumber >= 0)
                {
                    env = ADCurve(v.CurrentTime);
                }
                else
                {
                    env = ADCurve(v.NoteLength);
                    env *= RCurve(v.CurrentTime - v.NoteLength);
                }

                v.Event.Invoke(env * _amplitude * v.Velocity + _bias);
            }
        }

        #endregion
    }
}
