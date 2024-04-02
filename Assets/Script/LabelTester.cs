using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WI;

public class LabelTester : MonoBehaviour
{
    [Label(typeof(Button), "TargetButton")]
    public Button labelingButton;
    [Label(typeof(Button), "unknownButton")]
    public Button unknownButton;
}
