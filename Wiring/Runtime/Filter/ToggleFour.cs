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
    [AddComponentMenu("Klak/Wiring/Switching/Toggle Four")]
    public class ToggleFour : NodeBase
    {
        #region Editable properties

        [SerializeField, Range(2, 4)]
        int _stateCount = 4;

        [SerializeField]
        float _value1 = 0.0f;

        [SerializeField]
        float _value2 = 1.0f;

        [SerializeField]
        float _value3 = 2.0f;

        [SerializeField]
        float _value4 = 3.0f;

        [SerializeField]
        FloatInterpolator.Config _interpolator = null;

        [SerializeField]
        bool _sendOnStartUp = false;

        #endregion

        #region Node I/O

        [Inlet]
        public float trigger {
            set {
                if (!enabled) return;

                _state = (_state + 1) % _stateCount;

                switch (_state)
                {
                    case 0:
                        _value.targetValue = _value1;
                        _state1Event.Invoke();
                        break;
                    case 1:
                        _value.targetValue = _value2;
                        _state2Event.Invoke();
                        break;
                    case 2:
                        _value.targetValue = _value3;
                        _state3Event.Invoke();
                        break;
                    default:
                        _value.targetValue = _value4;
                        _state4Event.Invoke();
                        break;
                }
            }
        }

        [SerializeField, Outlet]
        VoidEvent _state1Event = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _state2Event = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _state3Event = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _state4Event = new VoidEvent();

        [SerializeField, Outlet]
        FloatEvent _valueEvent = new FloatEvent();

        #endregion

        #region Private members

        int _state;
        FloatInterpolator _value;

        #endregion

        #region MonoBehaviour functions

        void Start()
        {
            _value = new FloatInterpolator(_value1, _interpolator);
            if (_sendOnStartUp) _state1Event.Invoke();
        }

        void Update()
        {
            _valueEvent.Invoke(_value.Step());
        }

        #endregion
    }
}
