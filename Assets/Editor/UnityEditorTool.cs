using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WIFramework;

public class UnityEditorTool : Editor
{

    [MenuItem("Tools/CustomTools/Object Name Indexing From parentName")]
    public static void ObjectNameIndexingFromParentName()
    {
        if (!GetCurrentSelectGameObject(out var target))
        {
            return;
        }

        var childs = new List<Transform>();
        target.transform.Search(ref childs);

        var header = target.name + "_";
        for(int i =1;i<childs.Count;++i)
        {
            childs[i].gameObject.name = header + i;
        }
    }

    [MenuItem("Tools/CustomTools/Object Name Indexing")]
    public static void ObjectNameIndexing()
    {
        if (!GetCurrentSelectGameObject(out var target))
        {
            return;
        }

        var childs = new List<Transform>();
        target.transform.Search(ref childs);
        var nameTable = new Dictionary<string, List<Transform>>();
        foreach(var c in childs)
        {
            if(!nameTable.TryGetValue(c.gameObject.name, out var list))
            {
                nameTable.Add(c.gameObject.name, new List<Transform>());
            }

            nameTable[c.gameObject.name].Add(c);
        }

        foreach(var t in nameTable)
        {
            bool add = false;
            if(t.Key.Last()!='_')
            {
                add = true;
            }

            string header;
            if (add)
                header = t.Key + '_';
            else
                header = t.Key;
            for(int i =0;i<t.Value.Count;++i)
            {
                t.Value[i].gameObject.name = header + i; 
            }
        }
    }

    [MenuItem("Tools/CustomTools/Find Missing Mesh Renderer")]
    public static void FindMissingMeshRenderer()
    {
        if(!GetCurrentSelectGameObject(out var target))
        {
            return;
        }

        var childs = new List<MeshRenderer>();
        target.transform.Search(ref childs);

        var missingRenderer = new List<MeshRenderer>();
        foreach(var c in childs)
        {
            if(c.sharedMaterial==null)
            {
                missingRenderer.Add(c);
                continue;
            }

            if (!c.gameObject.TryGetComponent<MeshFilter>(out var filter))
            {
                missingRenderer.Add(c);
                continue;
            }

            if(filter.sharedMesh == null)
            {
                missingRenderer.Add(c);
                continue;
            }

            for(int i =0;i<c.sharedMaterials.Length;++i)
            {
                if(c.sharedMaterials[i]== null)
                {
                    missingRenderer.Add(c);
                    break;
                }
            }
        }

        var errorParent = new GameObject("ErrorRenderers");
        errorParent.transform.SetParent(target.transform);

        foreach (var m in missingRenderer)
        {
            m.transform.SetParent(errorParent.transform);
        }

        Debug.Log($"Find Missing Renderer = {missingRenderer.Count}");
    }
    
    //[MenuItem("Tools/CountingTrasnform")]
    public static void CountingTrasnform()
    {
        if (!GetCurrentSelectGameObject(out var target))
            return;
        var childs = new List<Transform>();
        target.transform.Search(ref childs);
        Debug.Log($"{target.name} Leaf Count:{childs.Count}");
    }

    static bool GetCurrentSelectGameObject(out GameObject obj)
    {
        obj = Selection.activeGameObject;
        if (obj == null)
            return false;
        return true;
    }
    [MenuItem("Tools/TransformCleaning")]
    public static void TransformCleaning()
    {
        if(!GetCurrentSelectGameObject(out var target))
            return;
        Debug.Log($"Cleaning Target:{target.name}");
        List<Transform> childs = new List<Transform>();
        GameObject trashBin = new GameObject("TrashBin");
        trashBin.transform.SetParent(target.transform);
        target.transform.Search(ref childs);

        var before = childs.Count;
        foreach(var c in childs)
        {
            c.transform.SetParent(target.transform);
            if (c.transform.GetComponents<Component>().Length == 1)
            {
                c.transform.SetParent(trashBin.transform);
            }
        }
        var tc = trashBin.transform.childCount;
        var after = before- tc;
        DestroyImmediate(trashBin);
        Debug.Log($"Clean Before:{before}, After:{after}");
    }
    
    [MenuItem("Tools/TrashLineCleaning")]
    public static void TrashLineCleaning()
    {
        if (!GetCurrentSelectGameObject(out var target))
            return;
        Debug.Log($"Root:{target.gameObject.name}");
        List<MeshFilter> meshs = new List<MeshFilter>();
        target.transform.Search(ref meshs);

        List<MeshFilter> lineMeshs = new List<MeshFilter>();
        foreach (var m in meshs)
        {
            var points = m.sharedMesh.vertices;
            if (points.Length == 2)
                lineMeshs.Add(m);
        }

        Dictionary<Vector3, Dictionary<Vector3, List<MeshFilter>>> verticesTable = new Dictionary<Vector3, Dictionary<Vector3, List<MeshFilter>>>();
        Dictionary<Vector3, Dictionary<Vector3, MeshFilter>> boundTable = new Dictionary<Vector3, Dictionary<Vector3, MeshFilter>>();

        GameObject trashBin = new GameObject("bin");
        trashBin.transform.SetParent(target.transform);

        foreach (var l in lineMeshs)
        {
            var p1 = l.sharedMesh.vertices[0];
            var p2 = l.sharedMesh.vertices[1];
            if (!verticesTable.TryGetValue(p1, out var next))
            {
                verticesTable.Add(p1, new Dictionary<Vector3, List<MeshFilter>>());
                verticesTable[p1].Add(p2, new List<MeshFilter>());
                verticesTable[p1][p2].Add(l);
                continue;
            }

            if (!next.TryGetValue(p2, out var final))
            {
                next.Add(p2, new List<MeshFilter>());
                next[p2].Add(l);
                continue;
            }

            if (final == null)
            {
                final = new List<MeshFilter>();
            }
            final.Add(l);
        }

        foreach(var vt in verticesTable)
        {
            foreach(var vtt in vt.Value)
            {
                foreach (var m in vtt.Value)
                {
                    var center = m.sharedMesh.bounds.center;
                    var size = m.sharedMesh.bounds.size;
                    if(!boundTable.TryGetValue(center, out var next))
                    {
                        boundTable.Add(center, new Dictionary<Vector3, MeshFilter>());
                        boundTable[center].Add(size, m);
                        continue;
                    }

                    if(!next.TryGetValue(size, out var value))
                    {
                        next.Add(size, m);
                        continue;
                    }

                    if (value != null)
                    {
                        if(m.transform.position == value.transform.position)
                            m.gameObject.transform.SetParent(trashBin.transform);
                    }
                }
            }
        }

        int before = meshs.Count;
        int after = before - trashBin.transform.childCount;
        DestroyImmediate(trashBin);

        Debug.Log($"LineCleaning Before:{before}, After:{after}");
    }

    [MenuItem("Tools/PointMeshCleaning")]
    public static void PointMeshCleaning()
    {
        if (!GetCurrentSelectGameObject(out var obj))
        {
            return;
        }

        var meshs = new List<MeshFilter>();
        obj.transform.Search(ref meshs);

        List<MeshFilter> trashs = new List<MeshFilter>();
        foreach(var m in meshs)
        {
            if (m.sharedMesh.vertexCount <= 1)
            {
                trashs.Add(m);
            }
        }
        int count = meshs.Count;
        int trashCount = trashs.Count;
        for(int i = 0; i < trashCount; ++i)
        {
            DestroyImmediate(trashs[0]);
            trashs.RemoveAt(0);
        }
        Debug.Log($"SinglePoint Mesh Cleaning. Before:{count}, After:{count-trashCount}");
    }

    [MenuItem("Tools/Combine Materials")]
    public static void CombineMaterials()
    {
        if (!GetCurrentSelectGameObject(out var obj))
        {
            return;
        }

        var renderers = new List<MeshRenderer>();
        obj.transform.Search(ref renderers);

        var meshs = new List<MeshFilter>();
        obj.transform.Search(ref meshs);
        GameObject bin = new GameObject("bin");
        for (int i = 0; i < meshs.Count; ++i)
        {
            var newMesh = new Mesh();
            var vertics = meshs[i].sharedMesh.vertices;
            for(int j = 0; j < meshs[i].sharedMesh.subMeshCount; ++j)
            {
                var subMesh = meshs[i].sharedMesh.GetSubMesh(j);
                var vertexStart = subMesh.firstVertex;
                var vertexEnd = subMesh.vertexCount + vertexStart;

                Vector3[] newVertics = new Vector3[subMesh.indexCount];
                for(int q = vertexStart; q < vertexEnd; ++q)
                {
                    //newVertics[q] = vertics[q];
                    var point = new GameObject("Point");
                    point.transform.position = vertics[q];
                    point.transform.SetParent(bin.transform);
                }
            }

        }
    }
}
