using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace WIFramework
{
    //TODO : 미완성 230405
    public class CustomMeshRendererEditor : EditorWindow
    {
        [MenuItem("WI/MeshRendererEditor")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(CustomMeshRendererEditor));
            window.titleContent.text = "Mesh Renderer Editor";
        }

        string prevRootName;
        string rootObjectName;
        int childRendererCount;
        List<MeshRenderer> childRenderers = new List<MeshRenderer>();
        ShadowCastingMode castShadows;
        LightProbeUsage lightProbes;
        bool submit;

        void OnGUI()
        {
            rootObjectName = EditorGUILayout.TextField("Root Object Name", rootObjectName, EditorStyles.textField);

            if (rootObjectName == null)
            {
                return;
            }
            
            if (rootObjectName == prevRootName)
            {
                Draw();
                return;
            }
        
            prevRootName = rootObjectName;
            FilteringChildRenderers();
            Draw();
        }

        void FilteringChildRenderers()
        {
            childRenderers.Clear();
            var renderers = FindObjectsOfType<MeshRenderer>();
            foreach (var r in renderers)
            {
                var parents = r.transform.GetParents();

                foreach (var p in parents)
                {
                    if (p.name == rootObjectName)
                    {
                        childRenderers.Add(r);
                    }
                }
            }
        }

        [SerializeField]
        List<Material> materials;

        void Draw()
        {
            childRendererCount = childRenderers.Count;
            EditorGUILayout.LabelField($" Child Renderer Count: {childRendererCount}");

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Mesh Renderer Option",EditorStyles.boldLabel);

            castShadows = (ShadowCastingMode)EditorGUILayout.EnumPopup(" Cast Shadows", castShadows);
            lightProbes = (LightProbeUsage)EditorGUILayout.EnumPopup(" Light Probes", lightProbes);

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);

            SerializedObject so = new SerializedObject(this);
            var prop = so.FindProperty(nameof(materials));
            list = new ReorderableList(so,prop, true, true,true,true);
            so.Update();
            list.draggable = true;
            list.onAddCallback = delegate
            {
                materials.Add(null);
            };

            list.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, prop.displayName);
            };

            bool pf = false;
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = prop.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                EditorGUI.PropertyField(rect, element);
            };

            list.onRemoveCallback = (list) =>
            {
                materials.Remove(materials.Last());
                //so.Update();
            };

            list.onSelectCallback = (list) =>
            {
                var sp = list.serializedProperty;
                var element = sp.GetArrayElementAtIndex(list.index);
                var asset = element.FindPropertyRelative("Material");
                var selected = asset.objectReferenceValue as GameObject;
                
                if (selected != null)
                {
                    EditorGUIUtility.PingObject(selected);
                }
            };
            

            list.onChangedCallback = (list) =>
            {
                //var index = list.index;
                //Debug.Log("A");
                //var value = (Material)prop.GetArrayElementAtIndex(index).objectReferenceValue;
                //materials[index] = value;
            };

            if(prop.serializedObject.hasModifiedProperties)
            {
                Debug.Log("B");
            }

            if(pf)
            {
                Debug.Log("A");
            }
            so.ApplyModifiedProperties();
            list.DoLayoutList();
            submit = GUILayout.Button("Submit");
        }
        ReorderableList list;
    }
}