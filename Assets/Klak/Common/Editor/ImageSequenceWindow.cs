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

namespace Klak
{
    public class ImageSequenceWindow : EditorWindow
    {
        #region Private Variables

        // recorder settings
        int _frameRate = 30;
        int _superSampling = 1;
        bool _autoRecord;

        // recorder state
        bool _isRecording;
        int _frameCount;
        int _previousFrame;

        #endregion

        #region Recorder Functions

        void InitializeRecorder()
        {
            _isRecording = false;
            if (_autoRecord) StartRecord();
        }

        void StartRecord()
        {
            System.IO.Directory.CreateDirectory("Capture");
            Time.captureFramerate = _frameRate;
            _frameCount = -1;
            _isRecording = true;
        }

        void EndRecord()
        {
            Time.captureFramerate = 0;
            _isRecording = false;
        }

        void StepRecorder()
        {
            if (_frameCount > 0)
            {
                var name = "Capture/frame" + _frameCount.ToString("0000") + ".png";
                Application.CaptureScreenshot(name, _superSampling);
            }
            _frameCount++;
        }

        #endregion

        #region EditorWindow Functions

        [MenuItem ("Window/Image Sequence")]
        static void Init()
        {
            var instance = CreateInstance<ImageSequenceWindow>();
            instance.minSize = instance.maxSize =
                new Vector2(20, 6) * EditorGUIUtility.singleLineHeight;
            instance.titleContent = new GUIContent("Image Sequence");
            instance.ShowUtility();
        }

        void OnEnable()
        {
            EditorApplication.playmodeStateChanged += OnPlaymodeChanged;
        }

        void OnDisable()
        {
            EditorApplication.playmodeStateChanged -= OnPlaymodeChanged;
        }

        void OnPlaymodeChanged()
        {
            // detecting a start of the play mode
            if (!Application.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
                InitializeRecorder();
            Repaint();
        }

        void OnGUI()
        {
            _frameRate = EditorGUILayout.IntSlider("Frame Rate", _frameRate, 1, 100);
            _superSampling = EditorGUILayout.IntSlider("Supersampling", _superSampling, 1, 4);
            _autoRecord = EditorGUILayout.Toggle("Auto Recording", _autoRecord);

            if (EditorApplication.isPlaying)
            {
                var fatButton = GUILayout.Height(30);

                if (!_isRecording)
                {
                    if (GUILayout.Button("REC", fatButton)) StartRecord();
                }
                else
                {
                    var time = (float)_frameCount / _frameRate;
                    var label = "STOP  (" + time.ToString("0.0") + "s)";
                    if (GUILayout.Button(label, fatButton)) EndRecord();
                }
            }
        }

        void Update()
        {
            var frame = Time.frameCount;
            if (_previousFrame != frame)
            {
                if (Application.isPlaying && _isRecording)
                {
                    StepRecorder();
                    Repaint();
                }
                _previousFrame = frame;
            }
        }

        #endregion
    }
}
