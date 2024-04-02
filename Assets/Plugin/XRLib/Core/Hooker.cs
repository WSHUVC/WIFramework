using System;
using System.Collections.Generic;
using UnityEngine;

namespace WI
{
    public partial class Hooker
    {
        //static Array codes;
        internal static void Initialize()
        {
            //codes = Enum.GetValues(typeof(KeyCode));
            PlayerLoopInterface.InsertSystemAfter(typeof(FieldBinder), FieldBinder.Update, typeof(UnityEngine.PlayerLoop.EarlyUpdate.UpdatePreloading));
            PlayerLoopInterface.InsertSystemAfter(typeof(Hooker), MonoTracking, typeof(UnityEngine.PlayerLoop.EarlyUpdate.UpdatePreloading));
            //PlayerLoopInterface.InsertSystemBefore(typeof(Hooker), DetectingKey, typeof(UnityEngine.PlayerLoop.PreUpdate.NewInputUpdate));
            //SetHook();
        }

        internal static Queue<MonoBehaviour> initializerList = new();
        static Queue<MonoBehaviour> afterInitializerList = new();
        static Queue<MonoBehaviour> registWatingQueue = new();
        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void MonoTracking()
        {
            while (registWatingQueue.Count > 0)
            {
                var curr = registWatingQueue.Dequeue();
                if (curr != null)
                {
                    if (Core.Regist(curr))
                        initializerList.Enqueue(curr);
                }
            }

            Initializing();
        }

        internal static void Initializing()
        {
            while (initializerList.Count > 0)
            {
                var mono = initializerList.Dequeue();
                mono.AfterAwake();
                //Debug.Log($"{mono.GetType().Name} Initialized");
                afterInitializerList.Enqueue(mono);
            }

            while (afterInitializerList.Count > 0)
            {
                var curr = afterInitializerList.Dequeue();
                curr.AfterStart();
                // Debug.Log($"{curr.GetType().Name} AfterInitialized");
            }
        }

        public static void RegistReady(MonoBehaviour target)
        {
            registWatingQueue.Enqueue(target);
        }
    }
}
