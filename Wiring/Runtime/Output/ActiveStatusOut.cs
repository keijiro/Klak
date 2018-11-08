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
using System.Reflection;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Output/Component/Active Status Out")]
    public class ActiveStatusOut : NodeBase
    {
        #region Editable properties

        [SerializeField]
        Component _targetComponent = null;

        [SerializeField]
        GameObject _targetGameObject = null;

        #endregion

        #region Node I/O

        [Inlet]
        public void Enable()
        {
            if (enabled) SetActive(true);
        }

        [Inlet]
        public void Disable()
        {
            if (enabled) SetActive(false);
        }

        #endregion

        #region Private members

        PropertyInfo _propertyInfo;

        void OnEnable()
        {
            if (_targetComponent != null)
                _propertyInfo = _targetComponent.GetType().GetProperty("enabled");
        }

        void SetActive(bool flag)
        {
            if (_targetComponent != null && _propertyInfo != null)
                _propertyInfo.SetValue(_targetComponent, flag, null);

            if (_targetGameObject != null)
                _targetGameObject.SetActive(flag);
        }

        #endregion
    }
}
