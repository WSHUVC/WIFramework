using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace WIFramework
{
    public class MeshTools : WIEditorTool
    {
        [MenuItem(path + "Material/Find Unused Material")]
        public static void FindUnusedMaterial()
        {
            //var materials = AssetDatabase.FindAssets("t:Material");
            //Debug.Log($"Total Material Count : {materials.Count()}");
            //HashSet<string> id = new HashSet<string>();
            //foreach(var m in materials)
            //{
            //    if(!id.Add(m))
            //    {
            //        Debug.Log("Same GUID!");
            //    }
            //}

            var meshs = AssetDatabase.FindAssets("t:Mesh");
            var models = AssetDatabase.FindAssets("t:Model");
            Debug.Log($"Total Mesh Count : {meshs.Count()}");
            Debug.Log($"Total Model Count : {models.Count()}");

            foreach(var m in models)
            {
                var path = AssetDatabase.GUIDToAssetPath(m);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Debug.Log(model.GetComponent<MeshFilter>().mesh.subMeshCount);
            }

            //var scenes = AssetDatabase.FindAssets("t:Scene");
            //Debug.Log($"Total Scene Count : {scenes.Count()}");

            //int lineIndex = 0;

            //foreach(var scene in scenes)
            //{
            //    //Read Scene File
            //    var path = AssetDatabase.GUIDToAssetPath(scene);
            //    var sceneText = System.IO.File.ReadLines(path);
            //    foreach(var s in sceneText)
            //    {
            //    }
            //}
        }
        
        [MenuItem(path + "Mesh/RemoveVertexColor")]
        public static void RemoveVertexColor()
        {
            if (!GetCurrentSelectGameObject(out var target))
                return;

            var childs = new List<MeshRenderer>();
            target.transform.FindAll(ref childs);

            foreach (var c in childs)
            {
                var mesh = c.GetComponent<MeshFilter>().sharedMesh;
                mesh.colors = null;
            }
        }

        static void FlipNormal(Mesh mesh)
        {
            var normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.SetNormals(normals);
            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                var triangles = mesh.GetTriangles(m);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    var temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                mesh.SetTriangles(triangles, m);
            }
        }

        [MenuItem(path+"Mesh/NormalFlip %#X")]
        public static void MeshNormalFlip()
        {
            if (!GetCurrentSelectGameObject(out var target))
                return;

            var childs = new List<MeshFilter>();
            target.transform.FindAll(ref childs);

            foreach (var c in childs)
            {
                var mesh = c.sharedMesh;
                FlipNormal(mesh);
            }
        }
        [MenuItem(path + "Mesh/PointMeshCleaning")]
        public static void PointMeshCleaning()
        {
            if (!GetCurrentSelectGameObject(out var obj))
            {
                return;
            }

            var meshs = new List<MeshFilter>();
            obj.transform.FindAll(ref meshs);

            List<MeshFilter> trashs = new List<MeshFilter>();
            foreach (var m in meshs)
            {
                if (m.sharedMesh.vertexCount <= 1)
                {
                    trashs.Add(m);
                }
            }
            int count = meshs.Count;
            int trashCount = trashs.Count;
            for (int i = 0; i < trashCount; ++i)
            {
                DestroyImmediate(trashs[0]);
                trashs.RemoveAt(0);
            }
            Debug.Log($"SinglePoint Mesh Cleaning. Before:{count}, After:{count - trashCount}");
        }

        [MenuItem(path+"Mesh/Remove Missing Mesh Renderer")]
        public static void RemoveMissingMeshRenderer()
        {
            if (!GetCurrentSelectGameObject(out var target))
            {
                return;
            }

            var childs = new List<MeshRenderer>();
            target.transform.FindAll(ref childs);
            int childCount = childs.Count;
            int removeCount = 0;
            for(int i = 0; i<childCount;++i)
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

        [MenuItem(path + "Mesh/Find Missing Mesh Renderer")]
        public static void FindMissingMeshRenderer()
        {
            if (!GetCurrentSelectGameObject(out var target))
            {
                return;
            }

            var childs = new List<MeshRenderer>();
            target.transform.FindAll(ref childs);

            var missingRenderer = new List<MeshRenderer>();
            foreach (var c in childs)
            {
                if (c.sharedMaterial == null)
                {
                    missingRenderer.Add(c);
                    continue;
                }

                if (!c.gameObject.TryGetComponent<MeshFilter>(out var filter))
                {
                    missingRenderer.Add(c);
                    continue;
                }

                if (filter.sharedMesh == null)
                {
                    missingRenderer.Add(c);
                    continue;
                }

                for (int i = 0; i < c.sharedMaterials.Length; ++i)
                {
                    if (c.sharedMaterials[i] == null)
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
        [MenuItem(path + "Mesh/FindTrashMaterials")]
        public static void FindTrashAssets()
        {
            var assets = AssetDatabase.FindAssets("t:Material");
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            //Scene To Text
            var scenePath = currentScene.path;
            var sceneName = currentScene.name;
            var sceneText = System.IO.File.ReadLines(scenePath);

            List<Material> list = new List<Material>();
            foreach (var a in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(a);
                var obj = AssetDatabase.LoadAssetAtPath<Material>(path);
                Debug.Log(obj.name);
                list.Add(obj);
            }

            Debug.Log(list.Count);
            Dictionary<string, Object> mats = new Dictionary<string, Object>();
            foreach (var s in sceneText)
            {
                var parts = s.Split(',', '{', '}');
                foreach (var p in parts)
                {
                    if (p.Contains("guid:"))
                    {
                        var guid = p.Split(':')[1];
                        var obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Material));
                        if (!mats.TryGetValue(guid, out var value))
                        {
                            mats.Add(guid, obj);
                        }
                    }
                }
            }
        }
        [MenuItem(path + "Mesh/TrashLineCleaning")]
        public static void TrashLineCleaning()
        {
            if (!GetCurrentSelectGameObject(out var target))
                return;
            Debug.Log($"Root:{target.gameObject.name}");
            List<MeshFilter> meshs = new List<MeshFilter>();
            target.transform.FindAll(ref meshs);

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

            foreach (var vt in verticesTable)
            {
                foreach (var vtt in vt.Value)
                {
                    foreach (var m in vtt.Value)
                    {
                        var center = m.sharedMesh.bounds.center;
                        var size = m.sharedMesh.bounds.size;
                        if (!boundTable.TryGetValue(center, out var next))
                        {
                            boundTable.Add(center, new Dictionary<Vector3, MeshFilter>());
                            boundTable[center].Add(size, m);
                            continue;
                        }

                        if (!next.TryGetValue(size, out var value))
                        {
                            next.Add(size, m);
                            continue;
                        }

                        if (value != null)
                        {
                            if (m.transform.position == value.transform.position)
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
    }
}