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
    public class ColorMapper : MonoBehaviour
    {
        #region Nested Public Classes

        public enum ColorMode { Gradient, ColorArray }

        [System.Serializable]
        public class ColorEvent : UnityEvent<Color> {}

        #endregion

        #region Editable Properties

        [SerializeField]
        ColorMode _colorMode = ColorMode.Gradient;

        [SerializeField]
        Gradient _gradient = new Gradient();

        [SerializeField]
        [ColorUsage(true, true, 0, 16, 0.125f, 3)]
        Color[] _colorArray = new Color[2] { Color.black, Color.white };

        [SerializeField]
        ColorEvent _colorEvent;

        #endregion

        #region Public Properties

        public float inputValue {
            set {
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

        #endregion
    }
}
