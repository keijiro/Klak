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
    public class ThresholdTrigger : MonoBehaviour
    {
        #region Editable Properties

        [SerializeField] float _threshold = 0.01f;
        [SerializeField] float _delayToOff = 0.0f;

        [SerializeField] UnityEvent _onEvent;
        [SerializeField] UnityEvent _offEvent;

        #endregion

        #region Public Properties

        public float threshold {
            get { return _threshold; }
            set { _threshold = value; }
        }

        public float offDelay {
            get { return _delayToOff; }
            set { _delayToOff = value; }
        }

        public float inputValue {
            set { _currentValue = value; }
        }

        #endregion

        #region Private Variables

        enum State { Dormant, Enabled, Disabled }

        State _currentState;
        float _currentValue;
        float _delayTimer;

        #endregion

        #region MonoBehaviour Functions

        void Update()
        {
            if (_currentValue >= _threshold)
            {
                if (_currentState != State.Enabled)
                {
                    _onEvent.Invoke();
                    _currentState = State.Enabled;
                }
                _delayTimer = 0;
            }
            else if (_currentValue < _threshold &&
                     _currentState != State.Disabled)
            {
                _delayTimer += Time.deltaTime;
                if (_delayTimer >= _delayToOff)
                {
                    _offEvent.Invoke();
                    _currentState = State.Disabled;
                }
            }
        }

        #endregion
    }
}
