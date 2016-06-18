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
using Klak.Math;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Switching/Toggle")]
    public class Toggle : NodeBase
    {
        #region Editable properties

        [SerializeField]
        float _offValue = 0.0f;

        [SerializeField]
        float _onValue = 1.0f;

        [SerializeField]
        FloatInterpolator.Config _interpolator;

        [SerializeField]
        bool _sendOnStartUp = false;

        #endregion

        #region Node I/O

        [Inlet]
        public float trigger {
            set {
                if (!enabled) return;

                _state = !_state;

                if (_state)
                {
                    _value.targetValue = _onValue;
                    _onEvent.Invoke();
                }
                else
                {
                    _value.targetValue = _offValue;
                    _offEvent.Invoke();
                }
            }
        }

        [SerializeField, Outlet]
        VoidEvent _offEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _onEvent = new VoidEvent();

        [SerializeField, Outlet]
        FloatEvent _valueEvent = new FloatEvent();

        #endregion

        #region Private members

        bool _state;
        FloatInterpolator _value;

        #endregion

        #region MonoBehaviour functions

        void Start()
        {
            _value = new FloatInterpolator(0, _interpolator);
            if (_sendOnStartUp) _offEvent.Invoke();
        }

        void Update()
        {
            _valueEvent.Invoke(_value.Step());
        }

        #endregion
    }
}
