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
using UnityEditor;
using System.Collections.Generic;

namespace Klak.Wiring.Patcher
{
    // Queue class used to feedback user actions from UI controls
    public static class FeedbackQueue
    {
        #region Feedback record class

        // Base record class
        public abstract class RecordBase {}

        // Record class: delete a node
        public class DeleteNodeRecord : RecordBase
        {
            public Node node { get; private set; }

            public DeleteNodeRecord(Node node)
            {
                this.node = node;
            }
        }

        // Record class: an inlet button was pressed
        public class InletButtonRecord : RecordBase
        {
            public Node node { get; private set; }
            public Inlet inlet { get; private set; }

            public InletButtonRecord(Node node, Inlet inlet)
            {
                this.node = node;
                this.inlet = inlet;
            }
        }

        // Record class: an outlet button was pressed
        public class OutletButtonRecord : RecordBase
        {
            public Node node { get; private set; }
            public Outlet outlet { get; private set; }

            public OutletButtonRecord(Node node, Outlet outlet)
            {
                this.node = node;
                this.outlet = outlet;
            }
        }

        #endregion

        #region Queuing properties and methods

        public static bool IsEmpty {
            get { return _queue.Count == 0; }
        }

        public static void Reset()
        {
            _queue.Clear();
        }

        public static RecordBase Dequeue()
        {
            return _queue.Dequeue();
        }

        public static void Enqueue(RecordBase record)
        {
            _queue.Enqueue(record);
        }

        static Queue<RecordBase> _queue = new Queue<RecordBase>();

        #endregion
    }
}
