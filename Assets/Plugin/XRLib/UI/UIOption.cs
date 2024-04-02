
using System.Collections.Generic;
using UnityEngine;
using WI;

[CreateAssetMenu(fileName = "UIOption", menuName = "UIOption", order = 0)]
public class UIOption : ScriptableObject
{
    public SDictionary<string, string> options = new();
}