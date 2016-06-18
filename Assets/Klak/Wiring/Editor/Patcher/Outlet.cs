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
using UnityEditor;

namespace Klak.Wiring.Patcher
{
    // Editor representation of node outlet
    public class Outlet
    {
        #region Public members

        public string memberName {
            get { return _memberName; }
        }

        public string displayname {
            get { return _displayName; }
        }

        public Rect buttonRect {
            get { return _buttonRect; }
        }

        public UnityEventBase boundEvent {
            get { return _event; }
        }

        public Outlet(string memberName, UnityEventBase boundEvent)
        {
            _memberName = memberName;
            _displayName = MakeDisplayName(memberName);
            _event = boundEvent;
        }

        public bool DrawGUI(bool updateRect)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(_displayName, GUIStyles.labelRight);

            var result = GUILayout.Button("  ", GUIStyles.button);
            if (updateRect) _buttonRect = GUILayoutUtility.GetLastRect();

            EditorGUILayout.EndHorizontal();

            return result;
        }

        #endregion

        #region Private fields

        string _memberName;
        string _displayName;
        UnityEventBase _event;
        Rect _buttonRect;

        static string MakeDisplayName(string name)
        {
            // Apply the standard nicifying rule.
            var temp = ObjectNames.NicifyVariableName(name);

            // Remove tailing "Event".
            if (temp.EndsWith(" Event"))
                temp = temp.Substring(0, temp.Length - 6);

            return temp;
        }

        #endregion
    }
}
