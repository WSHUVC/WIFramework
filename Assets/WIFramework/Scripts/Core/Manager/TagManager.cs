using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace WIFramework
{
    public enum Tags
    {
    }

    public enum Layers
    {
        TopFloor,
        DeactiveIcon,
    }
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class TagManager
    {
        
        public const int defaultLayerLength = 5;

#if UNITY_EDITOR
        static TagManager()
        {
            UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((asset != null) && (asset.Length > 0))
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");

                var customTags = typeof(Tags).GetEnumNames();
                tags.ClearArray();
                for(int i = 0; i < customTags.Length; ++i)
                {
                    tags.InsertArrayElementAtIndex(i);
                    tags.GetArrayElementAtIndex(i).stringValue = customTags[i];
                }

                var layers = so.FindProperty("layers");
                var customLayers = typeof(Layers).GetEnumNames();

                for(int i = 6; i < layers.arraySize; ++i)
                {
                    var index = i - 6;
                    layers.GetArrayElementAtIndex(i).stringValue = "";
                    if(index<customLayers.Length)
                    {
                        layers.GetArrayElementAtIndex(i).stringValue = customLayers[index];
                    }
                }
                so.ApplyModifiedProperties();
                so.Update();
            }
        }
#endif
    }
}