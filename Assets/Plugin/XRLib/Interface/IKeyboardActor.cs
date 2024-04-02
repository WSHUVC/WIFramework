using UnityEngine;

namespace WI
{
    public interface IKeyboardActor
    {
        //public void GetKey(KeyCode key);
    }
    public interface IGetKeyDown : IKeyboardActor
    {
        void GetKeyDown(KeyCode code);
    }
    public interface IGetKey : IKeyboardActor
    {
        void GetKey(KeyCode code);
    }
    public interface IGetKeyUp : IKeyboardActor
    {
        void GetKeyUp(KeyCode code);
    }
}