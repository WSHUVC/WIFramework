using System;
using UnityEngine;
using WIFramework;
public class MonoBehaviour : UnityEngine.MonoBehaviour
{
    public MonoBehaviour()
    {
        Hooker.RegistReady(this);
    }

    public virtual void Initialize()
    {
    }
    public virtual void AfterInitialize()
    {
    }
    
    private void OnDestroy()
    {
        if (this.GetType() != typeof(TrashBehaviour))
            WIManager.Unregist(this);
    }
}