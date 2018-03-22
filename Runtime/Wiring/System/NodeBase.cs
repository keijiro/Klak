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

// Suppress "unused variable" warning messages.
#pragma warning disable 0414

using UnityEngine;
using UnityEngine.Events;
using System;

namespace Klak.Wiring
{
    // Attribute for marking inlets
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class InletAttribute : Attribute
    {
        public InletAttribute() {}
    }

    // Attribute for marking outlets
    [AttributeUsage(AttributeTargets.Field)]
    public class OutletAttribute : Attribute
    {
        public OutletAttribute() {}
    }

    // Base class of wiring node classes
    public class NodeBase : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        Vector2 _wiringNodePosition = uninitializedNodePosition;

        [Serializable]
        public class VoidEvent : UnityEvent {}

        [Serializable]
        public class FloatEvent : UnityEvent<float> {}

        [Serializable]
        public class Vector3Event : UnityEvent<Vector3> {}

        [Serializable]
        public class QuaternionEvent : UnityEvent<Quaternion> {}

        [Serializable]
        public class ColorEvent : UnityEvent<Color> {}

        static public Vector2 uninitializedNodePosition {
            get { return new Vector2(-1000, -1000); }
        }
    }
}
