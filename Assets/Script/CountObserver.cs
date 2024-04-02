using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WI;
public class CountObserver : MonoBehaviour
{
    Counter counter;
    TextMeshProUGUI text_Count;
    public override void AfterAwake()
    {
        var model = FieldBinder.Regist(counter);
        model.Binding(nameof(counter.count), PublicCounterUpdate);
        model.Binding("count2", PrivateCounterUpdate);
    }

    void PublicCounterUpdate(object data)
    {
        text_Count.SetText(data.ToString());
    }

    void PrivateCounterUpdate(object data)
    {
        Debug.Log($"PrevateCounter Update : {data.ToString()}");
    }
}
