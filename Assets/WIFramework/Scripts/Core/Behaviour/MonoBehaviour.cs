using UnityEngine;
using UnityEngine.Rendering;
using WIFramework;

public class MonoBehaviour : UnityEngine.MonoBehaviour
{
    public MonoBehaviour()
    {
        if (this.GetType() != typeof(TrashBehaviour))
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