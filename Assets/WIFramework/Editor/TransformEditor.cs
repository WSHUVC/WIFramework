using UnityEngine;
using UnityEditor;
using WIFramework;
using System.Collections.Generic;

[CustomEditor(typeof(Transform))]
public class TrasnformEditor : Editor
{
    GameObject transform;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        transform = target as GameObject;
        EditorGUI.BeginChangeCheck();

        if (GUILayout.Button("Transform Cleaning"))
        {
            TransformCleaning();
        }
        if (GUILayout.Button("Remove Missing Renderer"))
        {
            RemoveMissingMeshRenderer(); 
        }
        bool childIndexing = GUILayout.Button("Child Indexing");
        
        if(EditorGUI.EndChangeCheck())
        {
            if (childIndexing)
            {
                //Undo.RecordObject(transform, "Child Indexing Start");
                ObjectNameIndexingFromParentName();
            }
            //PrefabUtility.ApplyRemovedComponent(transform, );
            PrefabUtility.RecordPrefabInstancePropertyModifications(transform);
        }
    }

    public void RemoveMissingMeshRenderer()
    {
        var target = transform.gameObject;
        var childs = new List<MeshRenderer>();
        target.transform.FindAll(ref childs);
        int childCount = childs.Count;
        int removeCount = 0;
        for (int i = 0; i < childCount; ++i)
        {
            var c = childs[0];
            childs.RemoveAt(0);


            if (!c.gameObject.TryGetComponent<MeshFilter>(out var filter))
            {
                removeCount++;
                DestroyImmediate(c);
                continue;
            }

            if (filter.sharedMesh == null)
            {
                removeCount++;
                DestroyImmediate(c);
                DestroyImmediate(filter);
                continue;
            }

            if (c.sharedMaterial == null)
            {
                removeCount++;
                DestroyImmediate(c);
                DestroyImmediate(filter);
                continue;
            }

            for (int j = 0; j < c.sharedMaterials.Length; ++j)
            {
                if (c.sharedMaterials[j] == null)
                {
                    removeCount++;
                    DestroyImmediate(c);
                    DestroyImmediate(filter);
                    break;
                }
            }
        }

        Debug.Log($"Remove Missing Renderer = {removeCount}");
    }

    void TransformCleaning()
    {
        GameObject target = transform.gameObject;
        Debug.Log($"Cleaning Target:{target.name}");
        List<Transform> childs = new List<Transform>();
        target.transform.FindAll(ref childs);
        GameObject trashBin = new GameObject("TrashBin");
        trashBin.transform.SetParent(target.transform);

        var before = childs.Count;
        foreach (var c in childs)
        {
            c.transform.SetParent(target.transform);
            if (c.transform.GetComponents<Component>().Length == 1)
            {
                c.transform.SetParent(trashBin.transform);
            }
        }
        var tc = trashBin.transform.childCount;
        var after = before - tc;
        DestroyImmediate(trashBin);
        Debug.Log($"Clean Before:{before}, After:{after}");
    }

    void ObjectNameIndexingFromParentName()
    {
        var childs = new List<Transform>();
        transform.transform.FindAll(ref childs);

        var header = transform.name + "_";
        for (int i = 1; i < childs.Count; ++i)
        {
            //Undo.RecordObject(transform, "Child Name Change");
            childs[i].gameObject.name = header + i;
        }
    }
}