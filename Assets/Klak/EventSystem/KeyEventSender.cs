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

namespace Klak
{
    public class KeyEventSender : MonoBehaviour
    {
        #region Nested Public Classes

        public enum EventType {
            Trigger, Gate, Toggle, Value
        }

        [System.Serializable]
        public class ValueEvent : UnityEvent<float> {}

        #endregion

        #region Editable Properties

        [SerializeField]
        EventType _eventType = EventType.Trigger;

        [SerializeField]
        KeyCode _keyCode;

        [SerializeField]
        float _offValue = 0.0f;

        [SerializeField]
        float _onValue = 1.0f;

        [SerializeField]
        FloatInterpolator.Config _interpolator;

        [SerializeField]
        UnityEvent _triggerEvent;

        [SerializeField]
        UnityEvent _keyDownEvent;

        [SerializeField]
        UnityEvent _keyUpEvent;

        [SerializeField]
        UnityEvent _toggleOnEvent;

        [SerializeField]
        UnityEvent _toggleOffEvent;

        [SerializeField]
        ValueEvent _valueEvent;

        #endregion

        #region Private Properties And Variables

        bool IsKeyDown {
            get { return Input.GetKeyDown(_keyCode); }
        }

        bool IsKeyUp {
            get { return Input.GetKeyUp(_keyCode); }
        }

        FloatInterpolator _value;
        bool _toggle;

        #endregion

        #region MonoBehaviour Functions

        void Start()
        {
            _value = new FloatInterpolator(0, _interpolator);
        }

        void Update()
        {
            if (_eventType == EventType.Trigger)
            {
                if (IsKeyDown)
                    _triggerEvent.Invoke();
            }
            else if (_eventType == EventType.Gate)
            {
                if (IsKeyDown)
                    _keyDownEvent.Invoke();
                else if (IsKeyUp)
                    _keyUpEvent.Invoke();
            }
            else if (_eventType == EventType.Toggle)
            {
                if (IsKeyDown)
                {
                    _toggle ^= true;
                    if (_toggle)
                        _toggleOnEvent.Invoke();
                    else
                        _toggleOffEvent.Invoke();
                }
            }
            else // EventType.Value
            {
                if (IsKeyDown)
                    _value.targetValue = _onValue;
                else if (IsKeyUp)
                    _value.targetValue = _offValue;

                _valueEvent.Invoke(_value.Step());
            }
        }

        #endregion
    }
}
