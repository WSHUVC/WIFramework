using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WI;

public class Counter : MonoBehaviour, ISingle
{
    public int count;
    int count2;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            count++;

        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            count--;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            count = 0;
        }
        count2 = count+1;
    }
}
