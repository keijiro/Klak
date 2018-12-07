using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Input/ConstantInput")]
    public class ConstantInput : NodeBase
    {
        #region Editable properties
        [SerializeField]
        float _value=1.0f;
        
        #endregion
        #region Node I/O
        [SerializeField, Outlet]
        FloatEvent _outputEvent = new FloatEvent();
        #endregion

        #region Private functions
        void InvokeEvent(float _value)
        {
            _outputEvent.Invoke(_value);
        }
        #endregion
        #region MonoBehaviour functions
      
        void Start()
        {
            InvokeEvent(_value);
        }
         void Upadte()
        {
            InvokeEvent(_value);
        }
        #endregion

    }
}
