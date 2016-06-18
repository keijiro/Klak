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
using System.Collections.Generic;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Switching/Delay")]
    public class Delay : NodeBase
    {
        #region Editable properties

        public enum TimeUnit { Second, Frame }

        [SerializeField]
        TimeUnit _timeUnit;

        [SerializeField]
        float _interval = 1;

        #endregion

        #region Node I/O

        [Inlet]
        public void Trigger()
        {
            _timeQueue.Enqueue(CurrentTime);
        }

        [SerializeField, Outlet]
        VoidEvent _outputEvent = new VoidEvent();

        #endregion

        #region Private members

        Queue<float> _timeQueue = new Queue<float>();

        float CurrentTime {
            get {
                return _timeUnit == TimeUnit.Second ? Time.time : Time.frameCount;
            }
        }

        #endregion

        #region MonoBehaviour functions

        void Update()
        {
            while (_timeQueue.Count > 0 &&
                   _timeQueue.Peek() + _interval < CurrentTime)
            {
                _outputEvent.Invoke();
                _timeQueue.Dequeue();
            }
        }

        #endregion
    }
}
