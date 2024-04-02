using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePrefabCreater : MonoBehaviour
{
    public ISinglePrefab singlePrefab;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Instantiate(singlePrefab);
        }
    }
}
