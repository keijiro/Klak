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
    [AddComponentMenu("Klak/Wiring/Input/Noise")]
    public class Noise : NodeBase
    {
        #region Editable properties

        [SerializeField]
        float _frequency = 1.0f;

        [SerializeField, Range(1, 8)]
        int _octaves = 1;

        [SerializeField]
        float _bias = 0.0f;

        [SerializeField]
        float _amplitude = 1.0f;

        #endregion

        #region Node I/O

        [SerializeField, Outlet]
        FloatEvent _outputEvent = new FloatEvent();

        #endregion

        #region Private functions

        void InvokeEvent(float noise)
        {
            _outputEvent.Invoke((noise + _bias) * _amplitude);
        }

        #endregion

        #region MonoBehaviour functions

        float _time;

        void Start()
        {
            _time = Random.Range(-10000.0f, 0.0f);
        }

        void Update()
        {
            _time += Time.deltaTime * _frequency;

            if (_octaves > 1)
                InvokeEvent(Perlin.Fbm(_time, _octaves));
            else
                InvokeEvent(Perlin.Noise(_time));
        }

        #endregion
    }
}
