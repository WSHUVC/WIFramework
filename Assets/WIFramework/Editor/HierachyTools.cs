using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WIFramework
{
    public class HierachyTools : WIEditorTool
    {
        //[MenuItem(path + "MakeTransparents")]
        //public static void MakeTransparents()
        //{
        //    if (!GetCurrentSelectGameObject(out var target))
        //        return;

        //    var childs = new List<MeshRenderer>();
        //    target.transform.FindAll(ref childs);
        //    var root = target.transform.parent.Find("Transparent");
        //    if (root == null)
        //    {
        //        root = new GameObject("Transparent").transform;
        //        root.SetParent(target.transform.parent);
        //    }
        //    var building = FindObjectOfType<Building>();
        //    var transparentMaterial = Resources.Load<Material>("Mat_Transparent");
        //    foreach (var c in childs)
        //    {
        //        var copy = Instantiate(c);
        //        copy.transform.position = c.transform.position;
        //        copy.transform.SetParent(root);
        //        copy.sharedMaterial = transparentMaterial;
        //    }
        //    root.gameObject.SetActive(false);
        //}

        [MenuItem(path + "Hierachy/Object Name Indexing From parentName")]
        public static void ObjectNameIndexingFromParentName(Transform root = null)
        {
            GameObject target = root.gameObject;
            if (target==null)
            {
                if (!GetCurrentSelectGameObject(out target))
                {
                    return;
                }
            }

            var childs = new List<Transform>();
            target.transform.FindAll(ref childs);

            var header = target.name + "_";
            for (int i = 1; i < childs.Count; ++i)
            {
                Undo.RecordObject(root.transform, "Child Indexing");
                childs[i].gameObject.name = header + i;
            }
        }

        [MenuItem(path + "Hierachy/Object Name Indexing")]
        public static void ObjectNameIndexing()
        {
            if (!GetCurrentSelectGameObject(out var target))
            {
                return;
            }

            var childs = new List<Transform>();
            target.transform.FindAll(ref childs);
            var nameTable = new Dictionary<string, List<Transform>>();
            foreach (var c in childs)
            {
                if (!nameTable.TryGetValue(c.gameObject.name, out var list))
                {
                    nameTable.Add(c.gameObject.name, new List<Transform>());
                }

                nameTable[c.gameObject.name].Add(c);
            }

            foreach (var t in nameTable)
            {
                bool add = false;
                if (t.Key.Last() != '_')
                {
                    add = true;
                }

                string header;
                if (add)
                    header = t.Key + '_';
                else
                    header = t.Key;
                for (int i = 0; i < t.Value.Count; ++i)
                {
                    t.Value[i].gameObject.name = header + i;
                }
            }
        }

        [MenuItem(path + "Hierachy/CountingTrasnform")]
        public static void CountingTrasnform()
        {
            if (!GetCurrentSelectGameObject(out var target))
                return;
            var childs = new List<Transform>();
            target.transform.FindAll(ref childs);
            Debug.Log($"{target.name} Leaf Count:{childs.Count}");
        }
        
        [MenuItem(path+ "Hierachy/TransformCleaning")]
        public static void TransformCleaning(Transform root = null)
        {
            GameObject target = root.gameObject;
            if (target == null)
            {
                if (!GetCurrentSelectGameObject(out target))
                    return;
            }
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

    }
}