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
using System;

namespace Klak.Wiring.Patcher
{
    // Class for managing the list of patches.
    public class PatchManager
    {
        #region Public properties and methods

        // Total count of patches.
        public int patchCount {
            get { return _instances.Length; }
        }

        // Validity check
        public bool isValid {
            get { return !Array.Exists(_instances, p => p == null); }
        }

        // Constructor
        public PatchManager()
        {
            Reset();
        }

        // Reset and scan the patches.
        public void Reset()
        {
            _instances = UnityEngine.Object.FindObjectsOfType<Wiring.Patch>();
        }

        // Create a list of the names of the patches.
        public string[] MakeNameList()
        {
            return Array.ConvertAll(_instances, p => p.name);
        }

        // Determine the index of a given patch.
        public int GetIndexOf(Patch patch)
        {
            return Array.FindIndex(
                _instances, i => patch.IsRepresentationOf(i)
            );
        }

        // Create an editor representation of the patch at the index.
        public Patch RetrieveAt(int index)
        {
            return new Patch(_instances[index]);
        }

        // Create an editor representation of the previously chosesn patch.
        public Patch RetrieveLastSelected()
        {
            var instance = Array.Find(
                _instances, i => i._wiringSelected
            );

            // If no instance was found, use the first entry in the array.
            if (instance == null && _instances.Length > 0)
                instance = _instances[0];

            return instance == null ? null : new Patch(instance);
        }

        // Select a given patch for later use.
        public void Select(Patch patch)
        {
            foreach (var instance in _instances)
                instance._wiringSelected = patch.IsRepresentationOf(instance);
        }

        #endregion

        #region Static private mebers

        Wiring.Patch[] _instances;

        #endregion
    }
}
