// Klak - Creative coding library for Unity
// https://github.com/keijiro/Klak

using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Filter/Bang Filter")]
    public class BangFilter : NodeBase
    {
        #region Editable properties

        [SerializeField]
        bool _state;

        #endregion

        #region Node I/O

        [Inlet]
        public void Bang()
        {
            if (_state) _bangEvent.Invoke();
        }

        [Inlet]
        public void Open()
        {
            _state = true;
        }

        [Inlet]
        public void Close()
        {
            _state = false;
        }

        [SerializeField, Outlet]
        VoidEvent _bangEvent = new VoidEvent();

        #endregion
    }
}
