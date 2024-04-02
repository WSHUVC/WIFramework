using System;
using UnityEngine;
using UnityEngineInternal;
using WI;
using Object = UnityEngine.Object;

[Serializable]
public partial class MonoBehaviour : UnityEngine.MonoBehaviour
{
    public virtual void AfterAwake()
    {
    }
    public virtual void AfterStart()
    {
    }

    public MonoBehaviour()
    {
        if (this.GetType() != typeof(TrashBehaviour))
            Hooker.RegistReady(this);
    }

    public T FindSingle<T>() where T : MonoBehaviour, ISingle
    {
        var hashCode = typeof(T).GetHashCode();
        T result = default;
        if (Core.singleTable.TryGetValue(hashCode, out var mb))
        {
            result = (T)mb;
            return result;
        }
        return result;
    }

    public T Find<T>(string name) where T : Component
    {
        return transform.Find<T>(name);
    }
    public new static Object Instantiate(Object original, Vector3 position, Quaternion rotation)
    {
        var no = Object.Instantiate(original, position, rotation);
        Core.Regist(no);
        return no;
    }

    [TypeInferenceRule(TypeInferenceRules.TypeOfFirstArgument)]
    public new static Object Instantiate(Object original, Vector3 position, Quaternion rotation, Transform parent)
    {
        var no = Object.Instantiate(original, position, rotation, parent);
        Core.Regist(no);
        return no;
    }

    [TypeInferenceRule(TypeInferenceRules.TypeOfFirstArgument)]
    public new static Object Instantiate(Object original)
    {
        var no = Object.Instantiate(original);
        Core.Regist(no);
        return no;
    }
    [TypeInferenceRule(TypeInferenceRules.TypeOfFirstArgument)]
    public new static Object Instantiate(Object original, Transform parent)
    {
        var no = Object.Instantiate(original, parent, instantiateInWorldSpace: false);
        Core.Regist(no);
        return no;
    }

    [TypeInferenceRule(TypeInferenceRules.TypeOfFirstArgument)]
    public new static Object Instantiate(Object original, Transform parent, bool instantiateInWorldSpace)
    {
        var no = Object.Instantiate(original, parent, instantiateInWorldSpace);
        Core.Regist(no);
        return no;
    }

    public new static T Instantiate<T>(T original) where T : Object
    {
        var no = Object.Instantiate(original);
        Core.Regist(no);
        return no;
    }

    public new static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object
    {
        var no = (T)Object.Instantiate((Object)original, position, rotation);
        Core.Regist(no);
        return no;
    }

    public new static T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object
    {
        var no = (T)Object.Instantiate((Object)original, position, rotation, parent);
        Core.Regist(no);
        return no;
    }

    public new static T Instantiate<T>(T original, Transform parent) where T : Object
    {
        var no = Object.Instantiate(original, parent, worldPositionStays: false);
        Core.Regist(no);
        return no;
    }

    public new static T Instantiate<T>(T original, Transform parent, bool worldPositionStays) where T : Object
    {
        var no = (T)Object.Instantiate((Object)original, parent, worldPositionStays);
        Core.Regist(no);
        return no;
    }
    protected virtual void OnDestroy()
    {
        if (this.GetType() != typeof(TrashBehaviour))
            WI.Core.Unregist(this);
    }
}