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
    [AddComponentMenu("Klak/Wiring/Output/Component/Transform Out")]
    public class TransformOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Transform _targetTransform;

        [SerializeField]
        bool _addToOriginal = true;

        #endregion

        #region Node I/O

        [Inlet]
        public Vector3 position {
            set {
                if (!enabled || _targetTransform == null) return;
                _targetTransform.localPosition =
                    _addToOriginal ? _originalPosition + value : value;
            }
        }

        [Inlet]
        public Quaternion rotation {
            set {
                if (!enabled || _targetTransform == null) return;
                _targetTransform.localRotation =
                    _addToOriginal ? _originalRotation * value : value;
            }
        }

        [Inlet]
        public Vector3 scale {
            set {
                if (!enabled || _targetTransform == null) return;
                _targetTransform.localScale =
                    _addToOriginal ? _originalScale + value : value;
            }
        }

        [Inlet]
        public float uniformScale {
            set {
                if (!enabled || _targetTransform == null) return;
                var s = Vector3.one * value;
                if (_addToOriginal) s += _originalScale;
                _targetTransform.localScale = s;
            }
        }

        #endregion

        #region Private members

        Vector3 _originalPosition;
        Quaternion _originalRotation;
        Vector3 _originalScale;

        void OnEnable()
        {
            if (_targetTransform != null)
            {
                _originalPosition = _targetTransform.localPosition;
                _originalRotation = _targetTransform.localRotation;
                _originalScale = _targetTransform.localScale;
            }
        }

        void OnDisable()
        {
            if (_targetTransform != null)
            {
                _targetTransform.localPosition = _originalPosition;
                _targetTransform.localRotation = _originalRotation;
                _targetTransform.localScale = _originalScale;
            }
        }

        #endregion
    }
}
