using static UnityEngine.GraphicsBuffer;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace WI
{
    public class EditorUtil
    {

        public static int DrawSubclassDropdown(string label, Type target, int selectedIndex)
        {
            int result = 0;
#if UNITY_EDITOR
            var subclass = Assembly
                .GetAssembly(target)
                .GetTypes()
                .Where(t => t.IsSubclassOf(target))
                .Select(t2 => t2.Name).Append(target.Name).ToArray();
                
            result = EditorGUILayout.Popup(label, selectedIndex, subclass);
#endif
            return result;
        }
    }
}