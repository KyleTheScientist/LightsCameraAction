using GorillaLocomotion;
using GorillaScience.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LightsCameraAction.Modules
{
    public class FirstPerson : LCAModule
    {
        private Transform cameraTransform, headTransform;
        public float followRate = 20f;
        void OnEnable()
        {
            if (!Plugin.Instance.Initialized) return;
            LCAModule.brain.enabled = false;
            cameraTransform = LCAModule.shoulderCamera.transform;
            headTransform = Player.Instance.headCollider.transform;
            cameraTransform.SetParent(null);
        }
        
        void OnDisable()
        {
            LCAModule.Reset();
        }

        protected override void LateUpdate()
        {
            float zoffset =
                headTransform.InverseTransformPoint(cameraTransform.position).z;
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                headTransform.TransformPoint(Vector3.forward * .2f),
                zoffset < .05f ? 1 : followRate * Time.deltaTime //If the camera is in the player's head, force it out
            );

            Vector3 currentEuler = cameraTransform.rotation.eulerAngles;
            Vector3 targetEuler = headTransform.rotation.eulerAngles;
            currentEuler.x = Mathf.LerpAngle(currentEuler.x, targetEuler.x, Time.deltaTime * followRate);
            currentEuler.y = Mathf.LerpAngle(currentEuler.y, targetEuler.y, Time.deltaTime * followRate);
            currentEuler.z = 0;
            cameraTransform.rotation = Quaternion.Euler(currentEuler);
            base.LateUpdate();
        }

        public FirstPerson Disable()
        {
            this.enabled = false;
            return this;
        }

        public override string DisplayName()
        {
            return "First Person";
        }
    }
}
