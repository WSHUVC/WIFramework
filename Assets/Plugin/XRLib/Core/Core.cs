using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace WI
{
    public static partial class Core
    {
#pragma warning disable IDE0090 // 'new(...)' 사용
        static Queue<MonoBehaviour> watingQueue = new Queue<MonoBehaviour>();
        internal static Dictionary<MonoBehaviour, GameObject> monoTable = new Dictionary<MonoBehaviour, GameObject>();
        internal static Dictionary<int, MonoBehaviour> singleTable = new Dictionary<int, MonoBehaviour>();
        internal static HashSet<IGetKey> getKeyActors = new HashSet<IGetKey>();
        internal static HashSet<IGetKeyUp> getKeyUpActors = new HashSet<IGetKeyUp>();
        internal static HashSet<IGetKeyDown> getKeyDownActors = new HashSet<IGetKeyDown>();
        internal static Dictionary<int, MonoBehaviour> keyTable = new Dictionary<int, MonoBehaviour>();
        static Dictionary<Type, List<MonoBehaviour>> diWaitingTable = new Dictionary<Type, List<MonoBehaviour>>();
#pragma warning restore IDE0090 // 'new(...)' 사용

        static Core()
        {
            //Debug.Log("WISystem Active");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            Init();
        }

        static void Init()
        {
            monoTable.Clear();
            singleTable.Clear();
            diWaitingTable.Clear();
            //keyTable.NullCleaning();
            getKeyActors.Clear();
            getKeyUpActors.Clear();
            getKeyDownActors.Clear();
        }

        public static void Regist(Object mo)
        {
            if (mo is not MonoBehaviour)
                return;

            if (!Regist(mo as MonoBehaviour))
                return;
         
            Hooker.initializerList.Enqueue(mo as MonoBehaviour);
            Hooker.Initializing();
        }
        internal static bool Regist(MonoBehaviour mb)
        {
            //?? Addressable 다운로드를 하면 null mb가 생성된다.
            if (mb.gameObject == null)
                return false;

            if (!mb.gameObject.scene.isLoaded)
                return false;

            //Debug.Log($"Regist {mb.name}");
            if (!monoTable.TryAdd(mb, mb.gameObject))
            {
                //Debug.Log($"Regist failed : {mb.name}");
                return false;
            }

            if (!RegistSingleBehaviour(mb))
                return false;

            //if (!RegistHotKeyBinding(mb))
            //    return false;


            //RegistKeyboardActor(mb);
            Injection(mb);
            return true;
        }

        static void RegistKeyboardActor(MonoBehaviour mb)
        {
            if (mb is not IKeyboardActor)
                return;
            if (mb is IGetKey gk)
                getKeyActors.Add(gk);

            if (mb is IGetKeyUp gu)
                getKeyUpActors.Add(gu);

            if (mb is IGetKeyDown gd)
                getKeyDownActors.Add(gd);
        }

        internal static void Unregist(MonoBehaviour mb)
        {
            //Debug.Log($"Unregist:{mb.name}");
            monoTable.Remove(mb);
            if (mb is ISingle)
                singleTable.Remove(mb.GetHashCode());

            //if (mb is IHotkeyAction hk)
            //    keyTable.Remove(mb.GetHashCode());

            //if (mb is IKeyboardActor)
            //{
            //    if (mb is IGetKey gk)
            //        getKeyActors.Remove(gk);

            //    if (mb is IGetKeyUp gu)
            //        getKeyUpActors.Remove(gu);

            //    if (mb is IGetKeyDown gd)
            //        getKeyDownActors.Remove(gd);
            //}
        }
        static bool RegistHotKeyBinding(MonoBehaviour mb)
        {
            if (mb is not IHotkeyAction)
                return true;
            var hashCode = mb.GetType().GetHashCode();
            if (keyTable.ContainsKey(hashCode))
            {
                if (keyTable[hashCode].GetHashCode() != mb.GetHashCode())
                {
                    TrashThrow(mb, keyTable[hashCode]);
                    return false;
                }
                return false;
            }

            keyTable.Add(hashCode, mb);

            return true;
        }
        static bool RegistSingleBehaviour(MonoBehaviour mb)
        {
            if (mb is not ISingle)
                return true;

            var hashCode = mb.GetType().GetHashCode();
            //Debug.Log($"Regist Single {wi}");
            if (singleTable.ContainsKey(hashCode))
            {
                //Debug.Log($"SameCode! Origin={singleTable[hashCode].gameObject.name}, New={wi.gameObject.name}");
                if (singleTable[hashCode].GetHashCode() != mb.GetHashCode())
                {
                    //Debug.Log($"Move To Trash");
                    TrashThrow(mb, singleTable[hashCode]);
                    return false;
                }
                return false;
            }

            singleTable.Add(hashCode, mb);
            if (diWaitingTable.TryGetValue(mb.GetType(), out var watingList))
            {
                //Debug.Log($"Find Wating Table");
                foreach (var w in watingList)
                {
                    var injectTargets = w.GetType().GetAllFields();

                    foreach (var t in injectTargets)
                    {
                        if (t.FieldType == mb.GetType())
                        {
                            //Debug.Log($"Find Missing");
                            t.SetValue(w, mb);
                        }
                    }
                }
            }
            //Debug.Log($"RS_{mb.name}");
            return true;
        }
        internal static void Injection(MonoBehaviour mb)
        {
            var wiType = mb.GetType();
            var targetFields = wiType.GetAllFields();

            List<Component> childs = mb.transform.FindAll<Component>();
            List<UIBehaviour> uiElements = new ();
            List<Transform> childTransforms = new ();

            //Filtering 
            foreach (var c in childs)
            {
                if (c == null)
                    continue;

                var cType = c.GetType();
                if (cType.IsSubclassOf(typeof(UIBehaviour)))
                {
                    uiElements.Add(c as UIBehaviour);
                    continue;
                }

                if (cType.IsSubclassOf(typeof(Transform)) || cType.Equals(typeof(Transform)) || cType.Equals(typeof(RectTransform)))
                {
                    childTransforms.Add(c as Transform);
                    continue;
                }
            }

            foreach (var f in targetFields)
            {
                #region LabelInjection
                Label label = (Label)f.GetCustomAttribute(typeof(Label));
                if (label != null)
                {
                    foreach (var c in childs)
                    {
                        if (c.name.Equals(label.name))
                        {
                            if (c.TryGetComponent(label.target, out var value))
                            {
                                f.SetValue(mb, value);
                            }
                        }
                    }
                }
                #endregion

                #region ISingleInjection
                if (f.FieldType.GetInterface(nameof(ISingle)) != null)
                {
                    //Debug.Log($"{wi} in ISingle.");
                    if (singleTable.TryGetValue(f.FieldType.GetHashCode(), out var value))
                    {
                        //Debug.Log($"Inject {value}");
                        f.SetValue(mb, value);
                    }
                    else
                    {
                        if (!diWaitingTable.ContainsKey(f.FieldType))
                        {
                            diWaitingTable.Add(f.FieldType, new List<MonoBehaviour>());
                        }

                        diWaitingTable[f.FieldType].Add(mb);
                        //Debug.Log($"Inject Waiting {wi}");
                    }

                    continue;
                }
                #endregion
                #region IHotkeybinding
                //if (f.FieldType.GetInterface(nameof(IHotkeyAction)) != null)
                //{
                //    //Debug.Log($"{wi} in ISingle.");
                //    if (keyTable.TryGetValue(f.FieldType.GetHashCode(), out var value))
                //    {
                //        //Debug.Log($"Inject {value}");
                //        f.SetValue(mb, value);
                //    }

                //    continue;
                //}
                #endregion
                #region UIBehaviourInjection
                if (f.FieldType.IsSubclassOf(typeof(UIBehaviour)))
                {
                    InjectNameAndType(uiElements, f, mb);
                    continue;
                }
                #endregion

                #region TransformInjection
                if (f.FieldType.IsSubclassOf(typeof(Transform)) || f.FieldType.Equals(typeof(Transform)))
                {
                    InjectNameAndType(childTransforms, f, mb);
                    continue;
                }

                if (f.FieldType.Equals(typeof(RectTransform)))
                {
                    InjectNameAndType(childTransforms, f, mb);
                }
                #endregion

                if (f.FieldType.IsGenericType)
                {
                    if (f.FieldType.GetGenericTypeDefinition().Equals(typeof(Container<>)))
                    {
                        var loadType = f.FieldType.GetGenericArguments()[0];
                        var containerType = typeof(Container<>).MakeGenericType(new Type[] { loadType });
                        var container = Activator.CreateInstance(containerType);

                        var stuffs = childs
                            .Where(c => loadType.Equals(c.GetType()));

                        container.GetType().GetMethod("Loading").Invoke(container, new object[] { stuffs });
                        f.SetValue(mb, container);
                    }
                }
            }
        }
        static void TrashThrow(MonoBehaviour target, MonoBehaviour origin)
        {
            var trash = target.gameObject.AddComponent<TrashBehaviour>();
            trash.originBehaviour = origin;
            monoTable.Remove(target);
            GameObject.Destroy(target);
        }
        static void InjectNameAndType<T>(List<T> arr, FieldInfo info, MonoBehaviour target) where T : Component
        {
            foreach (var a in arr)
            {
                if (!a.name.Equals(info.Name))
                    continue;
                if (!a.GetType().Equals(info.FieldType))
                    continue;

                info.SetValue(target, a);
                return;
            }
        }
    }
}