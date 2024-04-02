using UnityEditor;
using UnityEngine;

namespace WIFramework
{
    public class WIEditorTool : Editor
    {
        protected const string path = "WI/";
        public static bool GetCurrentSelectGameObject(out GameObject obj)
        {
            obj = Selection.activeGameObject;
            if (obj == null)
                return false;
            return true;
        }
    }
}