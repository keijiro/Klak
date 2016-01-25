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
using UnityEngine.Events;
using Klak.Math;
using System;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Multi-State Toggle")]
    public class MultiStateToggle : MonoBehaviour
    {
        #region Nested Public Classes

        public enum EventType { Trigger, Value }

        [Serializable]
        public class ValueEvent : UnityEvent<float> {}

        #endregion

        #region Editable Properties

        [SerializeField]
        EventType _eventType = EventType.Trigger;

        [SerializeField]
        FloatInterpolator.Config _interpolator;

        [SerializeField]
        UnityEvent[] _triggerEvents = new UnityEvent[1];

        [SerializeField]
        float[] _stateValues = new float[1];

        [SerializeField]
        ValueEvent _valueEvent;

        #endregion

        #region Public Properties And Methods

        public int stateCount {
            get {
                if (_eventType == EventType.Trigger)
                    return _triggerEvents.Length;
                else
                    return _stateValues.Length;
            }
        }

        public int currentState {
            get { return _state; }
            set {
                _state = Mathf.Min(value, stateCount - 1);
                SendTrigger();
            }
        }

        public void Toggle()
        {
            _state = (_state + 1) % stateCount;
            SendTrigger();
        }

        public void ResetState()
        {
            _state = 0;
            SendTrigger();
        }

        #endregion

        #region Private Variables And Methods

        int _state;
        FloatInterpolator _value;

        void SendTrigger()
        {
            if (_eventType == EventType.Trigger)
                _triggerEvents[_state].Invoke();
        }

        #endregion

        #region MonoBehaviour Functions

        void Start()
        {
            _value = new FloatInterpolator(0, _interpolator);
        }

        void Update()
        {
            if (_eventType == EventType.Value)
            {
                _value.targetValue = _stateValues[_state];
                _valueEvent.Invoke(_value.Step());
            }
        }

        #endregion
    }
}
