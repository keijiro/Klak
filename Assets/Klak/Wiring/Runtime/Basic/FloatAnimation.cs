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

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Animation/Float Animation")]
    public class FloatAnimation : NodeBase
    {
        #region Editable properties

        [SerializeField]
        AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        float _speed = 1.0f;

        [SerializeField]
        bool _playOnStart = true;

        #endregion

        #region Node I/O

        [Inlet]
        public float timeScale {
            set { _timeScale = value; }
        }
        
        [Inlet]
        public void Play()
        {
            _time = 0;
            _isPlaying = true;
        }

        [Inlet]
        public void Stop()
        {
            _isPlaying = false;
        }

        [Inlet]
        public void TogglePause()
        {
            _isPlaying = !_isPlaying;
        }

        [SerializeField, Outlet]
        FloatEvent _floatEvent = new FloatEvent();

        #endregion

        #region Private variables

        float _time;
        float _timeScale;
        bool _isPlaying;

        #endregion

        #region MonoBehaviour functions

        void Start()
        {
            _isPlaying = _playOnStart;
            _timeScale = 1;
        }

        void Update()
        {
            if (_isPlaying)
            {
                _time += Time.deltaTime * _speed * _timeScale;
                _floatEvent.Invoke(_curve.Evaluate(_time));
            }
        }

        #endregion
    }
}
