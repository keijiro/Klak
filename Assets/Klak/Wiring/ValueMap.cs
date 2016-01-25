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
using Klak.Math;
using System;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Value Map")]
    public class ValueMap : MonoBehaviour
    {
        #region Nested Public Classes

        public enum ModulationType {
            Off, Add, Subtract, Multiply, Divide, Minimum, Maximum
        }

        public enum OutputType {
            Bool, Int, Float, Vector3
        }

        [Serializable]
        public class BoolEvent : UnityEvent<bool> {}

        [Serializable]
        public class IntEvent : UnityEvent<int> {}

        [Serializable]
        public class FloatEvent : UnityEvent<float> {}

        [Serializable]
        public class Vector3Event : UnityEvent<Vector3> {}

        #endregion

        #region Editable Properties

        [SerializeField]
        AnimationCurve _inputCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        ModulationType _modulationType = ModulationType.Off;

        [SerializeField]
        AnimationCurve _modulationCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        FloatInterpolator.Config _interpolator;

        [SerializeField]
        OutputType _outputType = OutputType.Float;

        [SerializeField]
        float _threshold = 0.01f;

        [SerializeField]
        int _intOutput0 = 0;

        [SerializeField]
        int _intOutput1 = 100;

        [SerializeField]
        float _floatOutput0 = 0.0f;

        [SerializeField]
        float _floatOutput1 = 1.0f;

        [SerializeField]
        Vector3 _vector3Output0 = Vector3.zero;

        [SerializeField]
        Vector3 _vector3Output1 = Vector3.one;

        [SerializeField]
        BoolEvent _boolEvent;

        [SerializeField]
        IntEvent _intEvent;

        [SerializeField]
        FloatEvent _floatEvent;

        [SerializeField]
        Vector3Event _vector3Event;

        #endregion

        #region Public Properties

        public float inputValue {
            set {
                _inputValue = value;
                if (_interpolator.enabled)
                    _value.targetValue = CalculateTargetValue();
                else
                    InvokeValueEvent(CalculateTargetValue());
            }
        }

        public float modulationValue {
            set {
                _modulationValue = value;
                if (_interpolator.enabled)
                    _value.targetValue = CalculateTargetValue();
                else
                    InvokeValueEvent(CalculateTargetValue());
            }
        }

        #endregion

        #region Private Variables And Methods

        float _inputValue;
        float _modulationValue;
        FloatInterpolator _value;

        float EvalInputCurve()
        {
            return _inputCurve.Evaluate(_inputValue);
        }

        float EvalModulationCurve()
        {
            return _modulationCurve.Evaluate(_modulationValue);
        }

        float CalculateTargetValue()
        {
            var x = EvalInputCurve();

            switch (_modulationType)
            {
                case ModulationType.Add:
                    x += EvalModulationCurve();
                    break;
                case ModulationType.Subtract:
                    x -= EvalModulationCurve();
                    break;
                case ModulationType.Multiply:
                    x *= EvalModulationCurve();
                    break;
                case ModulationType.Divide:
                    x /= EvalModulationCurve();
                    break;
                case ModulationType.Minimum:
                    x = Mathf.Min(x, EvalModulationCurve());
                    break;
                case ModulationType.Maximum:
                    x = Mathf.Max(x, EvalModulationCurve());
                    break;
            }

            return x;
        }

        void InvokeValueEvent(float p)
        {
            switch (_outputType)
            {
                case OutputType.Bool:
                    _boolEvent.Invoke(p > _threshold);
                    break;
                case OutputType.Int:
                    var i = BasicMath.Lerp(_intOutput0, _intOutput1, p);
                    _intEvent.Invoke((int)i);
                    break;
                case OutputType.Float:
                    var f = BasicMath.Lerp(_floatOutput0, _floatOutput1, p);
                    _floatEvent.Invoke(f);
                    break;
                case OutputType.Vector3:
                    var v = BasicMath.Lerp(_vector3Output0, _vector3Output1, p);
                    _vector3Event.Invoke(v);
                    break;
            }
        }

        #endregion

        #region MonoBehaviour Functions

        void Start()
        {
            _value = new FloatInterpolator(0, _interpolator);
        }

        void Update()
        {
            if (_interpolator.enabled)
                InvokeValueEvent(_value.Step());
        }

        #endregion
    }
}
