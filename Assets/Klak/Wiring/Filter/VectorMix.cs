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
    [AddComponentMenu("Klak/Wiring/Mixing/Vector Mix")]
    public class VectorMix : NodeBase
    {
        #region Editable properties

        public enum ModulationType {
            Off, Add, Subtract, Multiply, Cross, Minimum, Maximum
        }

        [SerializeField]
        ModulationType _modulationType = ModulationType.Add;

        #endregion

        #region Node I/O

        [Inlet]
        public Vector3 input {
            set {
                if (!enabled) return;
                _inputValue = value;
                _outputEvent.Invoke(MixValues());
            }
        }

        [Inlet]
        public Vector3 modulation {
            set {
                if (!enabled) return;
                _modulationValue = value;
                _outputEvent.Invoke(MixValues());
            }
        }

        [SerializeField, Outlet]
        Vector3Event _outputEvent = new Vector3Event();

        #endregion

        #region Private members

        Vector3 _inputValue;
        Vector3 _modulationValue;

        Vector3 MixValues()
        {
            switch (_modulationType)
            {
                case ModulationType.Add:
                    return _inputValue + _modulationValue;
                case ModulationType.Subtract:
                    return _inputValue - _modulationValue;
                case ModulationType.Multiply:
                    return Vector3.Scale(_inputValue, _modulationValue);
                case ModulationType.Cross:
                    return Vector3.Cross(_inputValue, _modulationValue);
                case ModulationType.Minimum:
                    return Vector3.Min(_inputValue, _modulationValue);
                case ModulationType.Maximum:
                    return Vector3.Max(_inputValue, _modulationValue);
            }
            // Off
            return _inputValue;
        }

        #endregion
    }
}
