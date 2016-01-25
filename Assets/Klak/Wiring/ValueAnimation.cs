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
using System;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Value Animation")]
    public class ValueAnimation : MonoBehaviour
    {
        #region Nested Public Classes

        [Serializable]
        public class ValueEvent : UnityEvent<float> {}

        #endregion

        #region Editable Properties

        [SerializeField]
        AnimationCurve[] _animations = new AnimationCurve[1] {
            AnimationCurve.Linear(0, 0, 1, 1)
        };

        [SerializeField]
        bool _playOnStart = true;

        [SerializeField]
        float _speed = 1.0f;

        [SerializeField]
        ValueEvent _valueEvent;

        #endregion

        #region Public Properties And Methods

        public float speed {
            get { return _speed; }
            set { _speed = value; }
        }

        public bool isPlaying { get; set; }
        public float time { get; set; }

        public int animationIndex {
            get { return _animationIndex; }
        }

        public void Play(int index)
        {
            _animationIndex = Mathf.Clamp(index, 0, _animations.Length - 1);
            isPlaying = true;
            time = 0;
        }

        public void PlayNext()
        {
            _animationIndex = (_animationIndex + 1) % _animations.Length;
            isPlaying = true;
            time = 0;
        }

        public void TogglePlayState()
        {
            isPlaying = !isPlaying;
        }

        #endregion

        #region Private Members

        int _animationIndex;

        #endregion

        #region MonoBehaviour Functions

        void Start()
        {
            isPlaying = _playOnStart;
        }

        void Update()
        {
            if (isPlaying)
            {
                time += Time.deltaTime * _speed;
                var v = _animations[_animationIndex].Evaluate(time);
                _valueEvent.Invoke(v);
            }
        }

        #endregion
    }
}
