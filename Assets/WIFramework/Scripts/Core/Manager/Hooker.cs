using System;
using System.Collections.Generic;
using UnityEngine;
namespace WIFramework
{
    public class Hooker
    {
        static Array codes;
        internal static void Initialize()
        {
            codes = Enum.GetValues(typeof(KeyCode));
            PlayerLoopInterface.InsertSystemAfter(typeof(Hooker), MonoTracking, typeof(UnityEngine.PlayerLoop.EarlyUpdate.UpdatePreloading));
            PlayerLoopInterface.InsertSystemBefore(typeof(Hooker), DetectingKey, typeof(UnityEngine.PlayerLoop.PreUpdate.NewInputUpdate));
            //SetHook();
        }

        static Queue<MonoBehaviour> initializerList = new Queue<MonoBehaviour>();
        static Queue<MonoBehaviour> afterInitializerList = new Queue<MonoBehaviour>();
        static Queue<MonoBehaviour> registWatingQueue = new Queue<MonoBehaviour>();
        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void MonoTracking()
        {
            while (registWatingQueue.Count > 0)
            {
                var curr = registWatingQueue.Dequeue();
                if (curr != null)
                {
                    if (WIManager.Regist(curr))
                        initializerList.Enqueue(curr);
                }
            }
            Initializing();
        }

        static void Initializing()
        {
            while (initializerList.Count >0)
            {
                var mono = initializerList.Dequeue();
                mono.Initialize();
                afterInitializerList.Enqueue(mono);
            }

            while (afterInitializerList.Count > 0)
            {
                var curr = afterInitializerList.Dequeue();
                curr.AfterInitialize();
            }
        }

        public static void RegistReady(MonoBehaviour target)
        {
            registWatingQueue.Enqueue(target);
        }

        static void DetectingKey()
        {
            foreach (KeyCode k in codes)
            {
                if (Input.GetKey(k))
                {
                    Posting(k, WIManager.getKeyActors);
                }
                if (Input.GetKeyDown(k))
                {
                    Posting(k, WIManager.getKeyDownActors);
                }
                if (Input.GetKeyUp(k))
                {
                    Posting(k, WIManager.getKeyUpActors);
                }
            }
        }
        static void Posting<T>(KeyCode key, HashSet<T> actorList) where T : IKeyboardActor
        {
            foreach (var actor in actorList)
            {
                if (actor is null)
                {
                    Debug.Log("A");
                    continue;
                }
                actor.GetKey(key);
            }
        }

    }
}
