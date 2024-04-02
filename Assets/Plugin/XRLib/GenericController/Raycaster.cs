using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.Presets;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace WI
{
    [DefaultExecutionOrder(int.MinValue)]
    public class Raycaster : MonoBehaviour, ISingle
    {
        PointerEventData pointerEvent = new(EventSystem.current);
        List<RaycastResult> uiRaycastResults = new();
        RaycastHit[] hitInfo = new RaycastHit[16];
        RaycastHit[] tempInfo;
        HashSet<Type> typeLayers = new();
        Dictionary<Type, Action<RaycastHit, Component>> onExitEvent_TypeLayer = new();
        Dictionary<Type, Action<RaycastHit, Component>> onStayEvent_TypeLayer = new();
        Dictionary<Type, Action<RaycastHit, Component>> onEnterEvent_TypeLayer = new();
        Dictionary<Type, Action<RaycastHit, Component>> onLeftClickEvent_TypeLayer = new();
        Dictionary<Type, Action<RaycastHit, Component>> onRightClickEvent_TypeLayer = new();
        
        Dictionary<Type, Action<RaycastHit, Component>> onLeftClickFirst = new();
        Dictionary<Type, Action<RaycastHit, Component>> onRightClickFirst = new();
        Dictionary<Type, Action<RaycastHit, Component>> onEnterFirst = new();
        Dictionary<Type, Action<RaycastHit, Component>> onStayFirst = new();
        Dictionary<Type, Action<RaycastHit, Component>> onExitFirst = new();
        
        bool onLeftClick;
        bool onRightClick;
        Dictionary<Type, RaycastHit> firstHit = new();
        Dictionary<Type, RaycastHit> tempFirstHit = new();
        Dictionary<Transform, RaycastHit> fth = new();

        int hitCount;

        Camera cam;
        public RaycastHit hit => hitInfo[0];
        HashSet<Transform> hitTransform = new();
        HashSet<Transform> tempHit = new();
        Dictionary<Transform, RaycastHit> transformToHitinfo = new();
        List<(Action<RaycastHit, Component>, RaycastHit, Component)> eventList = new();
        
        public float uiHoverTime;
        float uiHoverTimer;
#pragma warning disable IDE0044 // 읽기 전용 한정자 추가
#pragma warning disable CS0649 // 'Raycaster.onUIHoverEvent' 필드에는 할당되지 않으므로 항상 null 기본값을 사용합니다.
        Action<RaycastResult> onUIHoverEvent;
#pragma warning restore CS0649 
#pragma warning restore IDE0044
        GameObject prevOnUI;

        
        void Awake()
        {
            cam = Camera.main;
        }

        void Update()
        {
            UIRaycast();
            PhysicsRaycast();
            EventInvoking();
        }
        
        void EventInvoking()
        {
            foreach(var e in eventList)
            {
                e.Item1.Invoke(e.Item2, e.Item3);
            }
            eventList.Clear();
        }
        void UIRaycast()
        {
            pointerEvent.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointerEvent, uiRaycastResults);
            
            if(uiRaycastResults.Count != 0)
            {
                if (uiRaycastResults[0].gameObject == null)
                {
                    Debug.Log("A");
                    return;
                }
                if (prevOnUI != uiRaycastResults[0].gameObject)
                {
                    uiHoverTimer = 0f;
                    prevOnUI = uiRaycastResults[0].gameObject;
                }

                if (uiHoverTimer >= uiHoverTime)
                {
                    onUIHoverEvent?.Invoke(uiRaycastResults[0]);
                }
                else
                {
                    uiHoverTimer += Time.deltaTime;
                }
            }
            else
            {
                prevOnUI = null;
            }
        }

        void PhysicsRaycast()
        {
            onLeftClick = Input.GetMouseButtonDown(0);
            onRightClick = Input.GetMouseButtonDown(1);
            Rayfire();
            SingleCasting();
            MultiCasting();
        }

        void Rayfire()
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            //Physics.Raycast(ray, out singleHit, Mathf.Infinity);
            tempInfo = new RaycastHit[16];
            hitCount = Physics.RaycastNonAlloc(ray, tempInfo, Mathf.Infinity);
            hitInfo = SortingHitInfos(tempInfo);
        }

        public bool Casting(LayerMask layer, out RaycastHit hit)
        {
            hit = new RaycastHit();
            if (hitCount == 0)
            {
                return false;
            }

            foreach (var h in hitInfo)
            {
                if (h.transform == null)
                    continue;

                if ((layer.value >> h.transform.gameObject.layer) == 1)
                {
                    hit = h;
                    return true;
                }
            }
            return false;
        }

        void SingleCasting(in RaycastHit hitInfo)
        {
            fth.TryAdd(hitInfo.transform, hitInfo);

            foreach (var tl in typeLayers)
            {
                SingleCasting(hitInfo, tl);
            }
        }

        void SingleCasting(in RaycastHit hitInfo, Type tl)
        {
            if (tempFirstHit.ContainsKey(tl))
            {
                return;
            }

            if (!hitInfo.transform.TryGetComponent(tl, out var value))
                return;

            if (firstHit.Remove(tl, out var prev))
            {
                if (prev.transform == hitInfo.transform)
                {
                    //Debug.Log($"OnStayFirst : {prev.transform.name}");
                    EventInvoke(onStayFirst, tl, hitInfo, value);
                }
                else
                {
                    //Debug.Log($"OnExitFirst : {prev.transform.name}");
                    EventInvoke(onExitFirst, tl, prev, prev.transform.GetComponent(tl));

                    //Debug.Log($"OnEnterFirst : {hitInfo.transform.name}");
                    EventInvoke(onEnterFirst, tl, hitInfo, value);
                    fth.Remove(prev.transform);
                }
            }
            else
            {
                //Debug.Log($"OnEnterFirst : {hitInfo.transform.name}");

                EventInvoke(onEnterFirst, tl, hitInfo, value);
            }
            fth[hitInfo.transform] = hitInfo;
            tempFirstHit.Add(tl, hitInfo);
        }

        void SingleCasting()
        {
            //fth.Clear();
            tempFirstHit.Clear();
            for (int i = 0; i < hitCount; ++i)
            {
                SingleCasting(hitInfo[i]);
            }

            FirstExitEvent();
            FirstClickEvent();
        }
        void FirstClickEvent()
        {

            foreach (var p in tempFirstHit)
            {
                firstHit.Add(p.Key, p.Value);

                if (onLeftClick)
                {
                    EventInvoke(onLeftClickFirst, p.Key, p.Value, p.Value.transform.GetComponent(p.Key));
                }
                if (onRightClick)
                {
                    EventInvoke(onRightClickFirst, p.Key, p.Value, p.Value.transform.GetComponent(p.Key));
                }
            }
        }
        
        void FirstExitEvent()
        {
            foreach (var f in firstHit)
            {
                //Debug.Log($"OnExitFirst :{f.Value.transform.name}");
                if (f.Value.transform == null)
                    continue;

                EventInvoke(onExitFirst, f.Key, f.Value, f.Value.transform.GetComponent(f.Key));
                fth.Remove(f.Value.transform);
                //tempFirstHit.Remove(f.Key);
            }
            firstHit.Clear();
        }
        
        RaycastHit[] SortingHitInfos(in RaycastHit[] hitInfo)
        {
            if (hitInfo[0].transform == null)
                return null;

            var sortHitInfo = hitInfo.Where(hi => hi.transform != null).OrderBy(hi => hi.distance).ToArray();
            //var sortHitInfo = hitInfo.OrderBy(hi => hi.distance).ToArray();

            return sortHitInfo;
        }

        void MultiCasting()
        {
            tempHit.Clear();
            for (int i = 0; i < hitCount; ++i)
            {
                var ht = hitInfo[i].transform;
                tempHit.Add(ht);
                transformToHitinfo.TryAdd(ht, hitInfo[i]);
                bool isStay = hitTransform.Remove(ht);

                foreach (var tl in typeLayers)
                {
                    if (!ht.TryGetComponent(tl, out var value))
                        continue;

                    if (onLeftClick)
                    {
                        EventInvoke(onLeftClickEvent_TypeLayer, tl, hitInfo[i], value);
                        //Debug.Log($"OnClick {tl} {value}");
                    }
                    if(onRightClick)
                    {
                        EventInvoke(onRightClickEvent_TypeLayer, tl, hitInfo[i], value);
                    }

                    if (!isStay)
                    {
                        //Debug.Log($"OnEnter {tl} {value}");
                        EventInvoke(onEnterEvent_TypeLayer, tl, hitInfo[i], value);
                    }
                    else
                    {
                        EventInvoke(onStayEvent_TypeLayer, tl, hitInfo[i], value);
                        //Debug.Log($"OnStay {tl} {value}");
                    }
                }
            }

            foreach (var h in hitTransform)
            {
                if (h == null)
                    continue;
                foreach (var tl in typeLayers)
                {
                    if (!h.TryGetComponent(tl, out var value))
                        continue;

                    EventInvoke(onExitEvent_TypeLayer, tl, transformToHitinfo[h], value);

                    //Debug.Log($"OnExit {tl} {value}");
                }
                transformToHitinfo.Remove(h);
            }

            hitTransform.Clear();
            foreach (var p in tempHit)
            {
                hitTransform.Add(p);
            }
        }
        void EventInvoke(Dictionary<Type, Action<RaycastHit, Component>> eventTable, Type layer, in RaycastHit hitInfo, Component value)
        {
            if (eventTable.TryGetValue(layer, out var action))
            {
                eventList.Add((action,hitInfo, value));
                //action?.Invoke(hitInfo, value);
            }
        }

        public bool IsFirstHit(Transform target)
        {
            if (hitInfo.Length == 0)
                return false;
            return hitInfo[0].transform == target;
        }

        public void AddTypeLayer(Type t)
        {
            typeLayers.Add(t);
            typeLayers.RemoveWhere(t => t == null);
        }

        public void RemoveTypeLayer<T>()
        {
            typeLayers.Remove(typeof(T));
        }

        public void AddEvent_FirstEnter(Type layer, Action<RaycastHit, Component> action)
        {
            onEnterFirst.TryAdd(layer, null);
            onEnterFirst[layer] += action;
        }

        public void AddEvent_FirstExit(Type layer, Action<RaycastHit, Component> action)
        {
            onExitFirst.TryAdd(layer, null);
            onExitFirst[layer] += action;
        }

        public void AddEvent_FirstStay(Type layer, Action<RaycastHit, Component> action)
        {
            onStayFirst.TryAdd(layer, null);
            onStayFirst[layer] += action;
        }

        public void AddEvent_FirstLeftClick(Type layer, Action<RaycastHit, Component> action)
        {
            onLeftClickFirst.TryAdd(layer, null);
            onLeftClickFirst[layer] += action;
        }

        public void AddEvent_FirstRightClick(Type layer, Action<RaycastHit, Component> action)
        {
            onRightClickFirst.TryAdd(layer, null);
            onRightClickFirst[layer] += action;
        }

        public void AddEvent_RightClick(Type layer, Action<RaycastHit, Component> action)
        {
            onRightClickEvent_TypeLayer.TryAdd(layer, null);
            onRightClickEvent_TypeLayer[layer] += action;
        }

        public void AddEvent_Enter(Type layer, Action<RaycastHit, Component> action)
        {
            onEnterEvent_TypeLayer.TryAdd(layer, null);
            onEnterEvent_TypeLayer[layer] += action;
        }

        public void AddEvent_Exit(Type layer, Action<RaycastHit, Component> action)
        {
            onExitEvent_TypeLayer.TryAdd(layer, null);
            onExitEvent_TypeLayer[layer] += action;
        }

        public void AddEvent_Stay(Type layer, Action<RaycastHit, Component> action)
        {
            onStayEvent_TypeLayer.TryAdd(layer, null);
            onStayEvent_TypeLayer[layer] += action;
        }

        public void AddEvent_LeftClick(Type layer, Action<RaycastHit, Component> action)
        {
            onLeftClickEvent_TypeLayer.TryAdd(layer, null);
            onLeftClickEvent_TypeLayer[layer] += action;
        }
    }
}