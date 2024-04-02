using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCycleTester : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("0 Awake");
    }
    void Start()
    {
        Debug.Log("4 Start");
    }

    private void OnEnable()
    {
        Debug.Log("1 OnEnable");
    }

    public override void AfterAwake()
    {
        Debug.Log("2 AfterAwake");
    }

    public override void AfterStart()
    {
        Debug.Log("3 AfterStart");
    }
    GameObject prev;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("---GameObject Instantiated---");
            if(prev!=null)
            {
                Destroy(prev.gameObject);
            }
            prev = Instantiate(gameObject);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("---This Instantiated---");
            if (prev != null)
            {
                Destroy(prev.gameObject);
            }
            prev = Instantiate(this).gameObject;
        }
    }
}
