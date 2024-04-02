using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WI
{
    //TODO::Eat Viewmode
    [RequireComponent(typeof(Camera))]
    public abstract class GenericController : MonoBehaviour
    {
        public new Camera camera;
        public GenericControllerOption option;
        public MaxRangeLimitter maxRangeLimitter = new();
        protected Vector3 moveVector;
        protected Vector3 cameraPosition;
        public Vector3 nextPosition;
        
        public override void AfterAwake()
        {
            camera = GetComponent<Camera>();
            option.Apply(this);
            Collider MaxRange = transform.parent.Find(nameof(MaxRange)).GetComponent<BoxCollider>();
            maxRangeLimitter.SetRange(MaxRange);
        }

        public virtual void Movement()
        {
            Move();
            Zoom();
            Rotate();
        }
        protected abstract void Move();
        protected abstract void Zoom();
        protected abstract void Rotate();
        public abstract void LastPositioning(bool limit);
        public abstract void Rewind();

        protected UserInput input;

        public bool IsClickUI
        {
            get
            {
                bool result = false;
                if (Input.GetMouseButtonDown(0))
                {
                    result = EventSystem.current.IsPointerOverGameObject();
                }
                
                return result;
            }
        }

        protected virtual void LateUpdate()
        {
            if (IsClickUI)
                return;

            input.GetInput();
            Movement();
            var limitCheck = maxRangeLimitter.MoveRangeLimit(nextPosition);
            LastPositioning(limitCheck);
        }
    }
}