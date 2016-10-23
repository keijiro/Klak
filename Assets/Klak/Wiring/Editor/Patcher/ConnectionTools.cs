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
using UnityEditor.Events;
using System.Reflection;
using System;
using Graphs = UnityEditor.Graphs;

namespace Klak.Wiring.Patcher
{
    public static class ConnectionTools
    {
        #region Public functions

        // Determine data type of a given event.
        public static Type GetEventDataType(Type eventType)
        {
            if (typeof(UnityEvent<float     >).IsAssignableFrom(eventType)) return typeof(float);
            if (typeof(UnityEvent<Vector3   >).IsAssignableFrom(eventType)) return typeof(Vector3);
            if (typeof(UnityEvent<Quaternion>).IsAssignableFrom(eventType)) return typeof(Quaternion);
            if (typeof(UnityEvent<Color     >).IsAssignableFrom(eventType)) return typeof(Color);
            return null;
        }

        // Create a connection between two slots.
        public static bool ConnectSlots(Graphs.Slot fromSlot, Graphs.Slot toSlot)
        {
            var nodeTo = ((Node)toSlot.node).runtimeInstance;
            var triggerEvent = GetEventOfOutputSlot(fromSlot);
            var targetMethod = GetMethodOfInputSlot(toSlot);

            // Determine the type of the target action.
            var actionType = GetUnityActionToInvokeMethod(targetMethod);

            if (actionType == null) return false; // invalid target method type

            // Create an action that is bound to the target method.
            var targetAction = Delegate.CreateDelegate(
                actionType, nodeTo, targetMethod
            );

            if (triggerEvent is UnityEvent)
            {
                // The trigger event has no parameter.
                // Add the action to the event with a default parameter.
                if (actionType == typeof(UnityAction))
                {
                    UnityEventTools.AddVoidPersistentListener(
                        triggerEvent, (UnityAction)targetAction
                    );
                    return true;
                }
                if (actionType == typeof(UnityAction<float>))
                {
                    UnityEventTools.AddFloatPersistentListener(
                        triggerEvent, (UnityAction<float>)targetAction, 1.0f
                    );
                    return true;
                }
            }
            else if (triggerEvent is UnityEvent<float>)
            {
                // The trigger event has a float parameter.
                // Then the target method should have a float parameter too.
                if (actionType == typeof(UnityAction<float>))
                {
                    // Add the action to the event.
                    UnityEventTools.AddPersistentListener(
                       (UnityEvent<float>)triggerEvent,
                       (UnityAction<float>)targetAction
                    );
                    return true;
                }
            }
            else if (triggerEvent is UnityEvent<Vector3>)
            {
                // The trigger event has a Vector3 parameter.
                // Then the target method should have a Vector3 parameter too.
                if (actionType == typeof(UnityAction<Vector3>))
                {
                    // Add the action to the event.
                    UnityEventTools.AddPersistentListener(
                       (UnityEvent<Vector3>)triggerEvent,
                       (UnityAction<Vector3>)targetAction
                    );
                    return true;
                }
            }
            else if (triggerEvent is UnityEvent<Quaternion>)
            {
                // The trigger event has a Quaternion parameter.
                // Then the target method should have a Quaternion parameter too.
                if (actionType == typeof(UnityAction<Quaternion>))
                {
                    // Add the action to the event.
                    UnityEventTools.AddPersistentListener(
                       (UnityEvent<Quaternion>)triggerEvent,
                       (UnityAction<Quaternion>)targetAction
                    );
                    return true;
                }
            }
            else if (triggerEvent is UnityEvent<Color>)
            {
                // The trigger event has a color parameter.
                // Then the target method should have a color parameter too.
                if (actionType == typeof(UnityAction<Color>))
                {
                    // Add the action to the event.
                    UnityEventTools.AddPersistentListener(
                       (UnityEvent<Color>)triggerEvent,
                       (UnityAction<Color>)targetAction
                    );
                    return true;
                }
            }

            return false; // trigger-target mismatch
        }

        // Disconnect given two slots.
        public static void DisconnectSlots(Graphs.Slot fromSlot, Graphs.Slot toSlot)
        {
            var nodeTo = ((Node)toSlot.node).runtimeInstance;
            var triggerEvent = GetEventOfOutputSlot(fromSlot);
            var targetMethod = GetMethodOfInputSlot(toSlot);

            var methodName = targetMethod.Name;

            var eventCount = triggerEvent.GetPersistentEventCount();
            for (var i = 0; i < eventCount; i++)
            {
                if (nodeTo == triggerEvent.GetPersistentTarget(i) &&
                    methodName == triggerEvent.GetPersistentMethodName(i))
                {
                    UnityEventTools.RemovePersistentListener(triggerEvent, i);
                    break;
                }
            }
        }

        #endregion

        #region Private functions

        // Returns an event instance that is bound to a given output slot.
        static UnityEventBase GetEventOfOutputSlot(Graphs.Slot slot)
        {
            var node = ((Node)slot.node).runtimeInstance;
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var field = node.GetType().GetField(slot.name, flags);
            return (UnityEventBase)field.GetValue(node);
        }

        // Returns a method that is bound to a given input slot.
        static MethodInfo GetMethodOfInputSlot(Graphs.Slot slot)
        {
            var node = ((Node)slot.node).runtimeInstance;
            return node.GetType().GetMethod(slot.name);
        }

        // Returns a UnityAction type that can be used to call the given method.
        static Type GetUnityActionToInvokeMethod(MethodInfo method)
        {
            var args = method.GetParameters();

            // The method has no parameter: Use UnityAction.
            if (args.Length == 0) return typeof(UnityAction);

            // Only refer to the first parameter.
            var paramType = args[0].ParameterType;

            // Returns one of the corrensponding action types.
            if (paramType == typeof(float     )) return typeof(UnityAction<float     >);
            if (paramType == typeof(Vector3   )) return typeof(UnityAction<Vector3   >);
            if (paramType == typeof(Quaternion)) return typeof(UnityAction<Quaternion>);
            if (paramType == typeof(Color     )) return typeof(UnityAction<Color     >);

            // No one matches the method type.
            return null;
        }

        #endregion
    }
}
