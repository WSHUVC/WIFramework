using System;
using UnityEngine;

namespace WI
{
    
    [CreateAssetMenu(fileName = "OrbitalControllerOption", menuName = "GenericControllerOption/OrbitalControllerOption")]
    public class OrbitalControllerOption : GenericControllerOption
    {
        public float elevationSensivity;
        public float azimuthSensivity;
        public float maxElevation;
        public float minElevation;
        public float maxDistance;
        public float minDistance;
        public float moveClamper;

        public float originDistance;
        public float originElevation;
        public float originAzimuth;

        public Vector3 originTargetPos;
        public Vector3 originTargetRot;
        public float currentElevation;
        public float currentAzimuth;
        public float currentDistance;
        public Transform target;
        //public Camera outlineCamera;

        public override void Apply(GenericController controller)
        {
            target = controller.FindSingle<OrbitalControllerTarget>().transform;
            //outlineCamera = FindObjectOfType<OutlineCamera>().outlineCamera;
            base.Apply(controller);
            originTargetPos = target.position;
            originTargetRot = target.eulerAngles;
            currentAzimuth = originAzimuth;
            currentDistance = originDistance;
            currentElevation = originElevation;
            //outlineCamera.orthographicSize = originDistance;
        }
        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
        }
    }
}