using System;
using UnityEngine;

namespace WI
{
    public abstract class GenericControllerOption : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]bool _orthographic;
        [SerializeField]float _moveSensivity;
        [SerializeField]float _zoomSensivity;
        [SerializeField]float _rotateSensivity;
        [SerializeField]float _moveSpeed;
        [SerializeField]float _zoomSpeed;
        [SerializeField]float _rotateSpeed;
        [SerializeField]float _fieldOfView;
        [SerializeField]bool _moveLimit;
        [SerializeField]bool _rotateLimit;
        [SerializeField]bool _azimuthRotateLimit;
        [SerializeField]bool _elevationRotateLimit;

        /**initialize Values*/
        [NonSerialized]
        public bool orthographic;
        [NonSerialized]
        public float moveSensivity;
        [NonSerialized]
        public float zoomSensivity;
        [NonSerialized]
        public float rotateSensivity;
        [NonSerialized]
        public float moveSpeed;
        [NonSerialized]
        public float zoomSpeed;
        [NonSerialized]
        public float rotateSpeed;
        [NonSerialized]
        public float fieldOfView;
        [NonSerialized]
        public bool moveLimit;
        [NonSerialized]
        public bool rotateLimit;
        [NonSerialized]
        public bool azimuthRotateLimit;
        [NonSerialized]
        public bool elevationRotateLimit;

        public virtual void OnAfterDeserialize()
        {
            orthographic = _orthographic;
            moveSensivity = _moveSensivity;
            zoomSensivity = _zoomSensivity;
            rotateSensivity = _rotateSensivity;
            moveSpeed = _moveSpeed;
            zoomSpeed = _zoomSpeed;
            rotateSpeed = _rotateSpeed;
            fieldOfView = _fieldOfView;
            moveLimit = _moveLimit;
            rotateLimit = _rotateLimit;
            azimuthRotateLimit = _azimuthRotateLimit;
            elevationRotateLimit = _elevationRotateLimit;
        }

        public void OnBeforeSerialize()
        {
            //throw new NotImplementedException();
        }

        public virtual void Apply(GenericController controller)
        {
            controller.camera.orthographic = orthographic;
            controller.camera.fieldOfView = fieldOfView;
        }
    }
}