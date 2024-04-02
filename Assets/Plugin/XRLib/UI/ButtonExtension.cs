using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

namespace WI
{
    public static class ButtonExtension
    {
        public delegate void CustomAction();
        public delegate void CustomAction<T>(List<T> values);
    }

    public static class ScrollRectExtension
    {
        public static void Clear(this ScrollRect rect)
        {
            var childCount = rect.content.transform.childCount;
            for(int i =0;i<childCount;++i)
            {
                GameObject.DestroyImmediate(rect.content.transform.GetChild(0).gameObject);
            }
        }   
    }

}