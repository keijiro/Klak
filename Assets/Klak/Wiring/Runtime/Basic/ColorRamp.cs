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
    [AddComponentMenu("Klak/Wiring/Convertion/Color Ramp")]
    public class ColorRamp : NodeBase
    {
        #region Editable properties

        public enum ColorMode { Gradient, ColorArray }

        [SerializeField]
        ColorMode _colorMode = ColorMode.Gradient;

        [SerializeField]
        Gradient _gradient = new Gradient();

        [SerializeField]
        [ColorUsage(true, true)]
        Color[] _colorArray = new Color[2] { Color.black, Color.white };

        #endregion

        #region Node I/O

        [Inlet]
        public float parameter {
            set {
                if (!enabled) return;

                if (_colorMode == ColorMode.Gradient)
                {
                    _colorEvent.Invoke(_gradient.Evaluate(value));
                }
                else // ColorArray
                {
                    var len = _colorArray.Length;

                    var idx = Mathf.FloorToInt(value * (len - 1));
                    idx = Mathf.Clamp(idx, 0, len - 2);

                    var x = value * (len - 1) - idx;
                    var y = Color.Lerp(_colorArray[idx], _colorArray[idx + 1], x);

                    _colorEvent.Invoke(y);
                }
            }
        }

        [SerializeField, Outlet]
        ColorEvent _colorEvent = new ColorEvent();

        #endregion
    }
}
