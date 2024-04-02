using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisableInjection : MonoBehaviour
{
    public TextMeshProUGUI enableText;
    public TextMeshProUGUI disableText;
    public Transform enableObject;
    public Transform disableObject;

    public Transform getEnableObject;
    public Transform getDisableObject;

    private void Awake()
    {
        getEnableObject = transform.Find(nameof(enableObject));
        getDisableObject = transform.Find(nameof(disableObject));
    }
}
