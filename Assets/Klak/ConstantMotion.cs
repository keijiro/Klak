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

namespace Klak
{
    public class ConstantMotion : MonoBehaviour
    {
        #region Editable Properties

        [SerializeField]
        MotionElement _position = new MotionElement(1.0f);

        [SerializeField]
        MotionElement _rotation = new MotionElement(30.0f);

        [SerializeField]
        bool _useLocalCoordinate = true;

        #endregion

        #region Nested Classes

        public enum MotionBasis {
            Off, XAxis, YAxis, ZAxis, Vector, Random
        };

        [System.Serializable]
        public class MotionElement
        {
            [SerializeField] MotionBasis _basis = MotionBasis.Off;
            [SerializeField] Vector3 _basisVector = Vector3.up;
            [SerializeField] float _speed = 1.0f;
            [SerializeField, Range(0, 1)] float _randomScale = 0.0f;

            Vector3 _randomVector;
            float _randomScaleFactor;

            public MotionElement(float speed)
            {
                _speed = speed;
            }

            public void Initialize()
            {
                _randomVector = Random.onUnitSphere;
                _randomScaleFactor = Random.value;
            }

            public bool Enabled {
                get { return _basis != MotionBasis.Off; }
            }

            public Vector3 Vector {
                get {
                    switch (_basis)
                    {
                        case MotionBasis.XAxis:  return Vector3.right;
                        case MotionBasis.YAxis:  return Vector3.up;
                        case MotionBasis.ZAxis:  return Vector3.forward;
                        case MotionBasis.Vector: return _basisVector;
                        case MotionBasis.Random: return _randomVector;
                    }
                    return Vector3.zero;
                }
            }

            public float Delta {
                get {
                    var scale = (1.0f - _randomScale * _randomScaleFactor);
                    return _speed * scale * Time.deltaTime;
                }
            }
        }

        #endregion

        #region MonoBehaviour Functions

        void Awake()
        {
            _position.Initialize();
            _rotation.Initialize();
        }

        void Update()
        {
            if (_position.Enabled)
            {
                var delta = _position.Vector * _position.Delta;
                if (_useLocalCoordinate)
                    transform.localPosition += delta;
                else
                    transform.position += delta;
            }

            if (_rotation.Enabled)
            {
                var delta = Quaternion.AngleAxis(
                    _rotation.Delta, _rotation.Vector);
                if (_useLocalCoordinate)
                    transform.localRotation = delta * transform.localRotation;
                else
                    transform.rotation = delta * transform.rotation;
            }
        }

        #endregion
    }
}
