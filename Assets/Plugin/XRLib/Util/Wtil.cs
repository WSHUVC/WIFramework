using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WI
{
    public static partial class Wtil
    {
#if UNITY_EDITOR
        public static bool GetCurrentSelectGameObject(out GameObject obj)
        {
            obj = Selection.activeGameObject;
            if (obj == null)
                return false;
            return true;
        }
#endif
        public static bool isScreenOver(this Camera cam, Vector3 pos)
        {
            var viewPos = cam.WorldToViewportPoint(pos);
            return viewPos.z < 0 || (viewPos.x < 0 || viewPos.x > 1) || (viewPos.y < 0 || viewPos.y > 1);
        }

        public static void AddLayer(this Camera cam, int layer)
        {
            cam.cullingMask |= 1 << layer;
        }
        public static bool Overlaps(this RectTransform rectTrans1, RectTransform rectTrans2)
        {
            var rect1 = new Rect(rectTrans1.localPosition.x, rectTrans1.localPosition.y, rectTrans1.rect.width, rectTrans1.rect.height);
            var rect2 = new Rect(rectTrans2.localPosition.x, rectTrans2.localPosition.y, rectTrans2.rect.width, rectTrans2.rect.height);
            return rect1.Overlaps(rect2);
        }

        public static void RemoveLayer(this Camera cam, int layer)
        {
            cam.cullingMask &= ~(1 << layer);
        }


        public static void NullCleaning<T, T2>(this Dictionary<T, T2> table)
        {
            var keys = table.Keys;

            foreach (var k in keys)
            {
                if (k == null)
                {
                    table.Remove(k);
                    continue;
                }

                if (table[k] == null)
                {
                    table.Remove(k);
                }
            }
        }

        public static void FindAll<T>(this Transform root, ref List<T> container)
        {
            if (root.TryGetComponents<T>(out var value))
            {
                container.AddRange(value);
            }
            var childCount = root.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                FindAll(root.GetChild(i), ref container);
            }
        }
        public static List<T> FindAll<T>(this MonoBehaviour root)
        {
            List<T> result = new List<T>();
            if (root.transform.TryGetComponents<T>(out var value))
            {
                result.AddRange(value);
            }

            var childCount = root.transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                var childComponents = FindAll<T>(root.GetChild(i));
                result.AddRange(childComponents);
            }
            return result;
        }
        static Transform GetChild(this MonoBehaviour root, int index)
        {
            return root.transform.GetChild(index);
        }
        public static List<T> FindAll<T>(this Transform root)
        {
            List<T> result = new List<T>();
            if (root.TryGetComponents<T>(out var value))
            {
                result.AddRange(value);
            }
            var childCount = root.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                var childComponents = FindAll<T>(root.GetChild(i));
                result.AddRange(childComponents);
            }
            return result;
        }

        public static bool Add<T>(this HashSet<T> hash, T[] values)
        {
            bool result = true;
            for(int i = 0; i < values.Length; ++i)
            {
                result = hash.Add(values[i]);
            }
            return result;
        }

        public static T Find<T>(this Transform root, string name) where T : Component
        {
            T result = default;
            int count = root.childCount;
            for(int i =0;i<count;++i)
            {
                var child = root.GetChild(i);
                if (child.TryGetComponent<T>(out result))
                {
                    if(result.name == name)
                        return result;
                }

                result = child.Find<T>(name);
            }
            return result;
        }
        public static T Find<T>(this Transform root) where T : Component
        {
            T result = default;
            if (root.Find<T>(ref result))
            {
                return result;
            }

            return result;
        }
        public static List<Transform> GetParents(this Transform root)
        {
            List<Transform> result = new List<Transform>();
            var parent = root.parent;
            while (parent != null)
            {
                result.Add(parent);
                parent = parent.parent;
            }
            return result;
        }
        /// <summary>
        /// T 가 나올때 까지 자식오브젝트를 순회한다. 여러개가 있다면 가장 먼저 발견된 것을 ref 로 내보낸다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool Find<T>(this Transform root, ref T result) where T : Component
        {
            if (root.TryGetComponent<T>(out result))
            {
                return true;
            }
            var childCount = root.transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                if (root.GetChild(i).Find<T>(ref result))
                    return true;
            }
            return false;
        }
        public static bool TryGetComponents<T>(this Transform root, out T[] result)
        {
            result = root.GetComponents<T>();
            return result.Length > 0;
        }
        public static T2[] ArrayConvertor<T, T2>(T[] origin) where T : MonoBehaviour where T2 : MonoBehaviour
        {
            T2[] result = new T2[origin.Length];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = origin[i] as T2;
            }
            return result;
        }
        public static void AddFlag<TEnum>(this ref TEnum currentValue, TEnum flagToAdd) where TEnum : struct, Enum
        {
            // currentValue와 flagToAdd가 같은 Enum 타입인지 확인합니다.
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum must be an enumerated type.");
            }

            // currentValue와 flagToAdd를 숫자형식으로 변환한 후 OR 연산합니다.
            int currentValueInt = Convert.ToInt32(currentValue);
            int flagToAddInt = Convert.ToInt32(flagToAdd);
            int resultInt = currentValueInt | flagToAddInt;

            // OR 연산 결과를 Enum 타입으로 변환하여 currentValue 매개변수에 할당합니다.
            currentValue = (TEnum)Enum.ToObject(typeof(TEnum), resultInt);
        }
        public static void RemoveFlag<TEnum>(this ref TEnum currentValue, TEnum flagToAdd) where TEnum : struct, Enum
        {
            // currentValue와 flagToAdd가 같은 Enum 타입인지 확인합니다.
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum must be an enumerated type.");
            }

            // currentValue와 flagToAdd를 숫자형식으로 변환한 후 OR 연산합니다.
            int currentValueInt = Convert.ToInt32(currentValue);
            int flagToAddInt = Convert.ToInt32(flagToAdd);
            int resultInt = currentValueInt & ~flagToAddInt;

            // OR 연산 결과를 Enum 타입으로 변환하여 currentValue 매개변수에 할당합니다.
            currentValue = (TEnum)Enum.ToObject(typeof(TEnum), resultInt);
        }
        public static Component GetOrAddComponent(this Transform mono, Type type)
        {
            if (mono.TryGetComponent(type, out var result))
            {
                return result;
            }
            return mono.gameObject.AddComponent(type);
        }
        public static T GetOrAddComponent<T>(this Transform mono) where T : Component
        {
            if (mono.TryGetComponent<T>(out T result))
            {
                return result;
            }
            return mono.gameObject.AddComponent<T>();
        }
        public static T GetOrAddComponent<T>(this GameObject mono) where T : Component
        {
            if (mono.TryGetComponent<T>(out T result))
            {
                return result;
            }
            return mono.gameObject.AddComponent<T>();
        }

        static Dictionary<Component, MeshRenderer[]> meshGroupCache = new();
        public static Vector3 GetMeshGroupCenter(this Component root)
        {
            if (!meshGroupCache.TryGetValue(root, out _))
            {
                meshGroupCache.Add(root, root.GetComponentsInChildren<MeshRenderer>());
            }
            var meshs = meshGroupCache[root];
            Vector3 total = Vector3.zero;
            foreach (var m in meshs)
            {
                total += m.bounds.center;
            }
            return total / meshs.Length;
        }

        public static Vector3 GetMeshGroupCenter(MeshRenderer[] meshGroup)
        {
            Vector3 total = Vector3.zero;
            foreach (var m in meshGroup)
            {
                total += m.bounds.center;
            }
            return total / meshGroup.Length;

        }

        public static bool StringToEnum<T>(string value, out T result)
        {
            result = default;
            if (!Enum.TryParse(typeof(T), value, out var parse))
                return false;

            result = (T)parse;
            return true;
        }
    }
}