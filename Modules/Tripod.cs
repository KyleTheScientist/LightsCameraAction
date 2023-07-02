using GorillaLocomotion;
using GorillaScience.Extensions;
using GorillaScience.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LightsCameraAction.Modules
{
    public class Tripod : LCAModule
    {
        private Transform gameCameraTransform, tripodTransform, screenTransform;
        public float followRate = 20f;
        private GameObject tripodPrefab;
        private Camera previewCamera;

        void Awake()
        {
            try
            {
                tripodPrefab = Plugin.Load<GameObject>("LCA Tripod Camera");
                tripodPrefab.AddComponent<Repositionable>().canRotate = false;
                tripodPrefab.SetActive(false);
                tripodPrefab.transform.SetParent(Plugin.Instance.transform);
            }
            catch (Exception e) { Plugin.log.Exception(e); }
        }

        void OnEnable()
        {
            try
            {
                if (!Plugin.Instance.Initialized) return;
                LCAModule.brain.enabled = false;
                gameCameraTransform = LCAModule.shoulderCamera.transform;
                tripodTransform = Instantiate(tripodPrefab).transform;
                tripodTransform.gameObject.SetActive(true);
                screenTransform = tripodTransform.Find("Model/Screen");
                previewCamera = tripodTransform.GetComponentInChildren<Camera>();
                previewCamera.targetTexture =
                    screenTransform.GetComponent<Renderer>().material.mainTexture as RenderTexture;
                previewCamera.aspect = 1;
                previewCamera.fieldOfView = 90;
                previewCamera.nearClipPlane = shoulderCamera.nearClipPlane;

                tripodTransform.position = Player.Instance.leftControllerTransform.position;
            }
            catch (Exception e) { Plugin.log.Exception(e); }
        }

        void OnDisable()
        {
            LCAModule.Reset();
            tripodTransform?.gameObject?.Obliterate();
        }

        protected override void LateUpdate()
        {
            try
            {
                Vector3 currentEuler = tripodTransform.rotation.eulerAngles;
                tripodTransform.LookAt(Player.Instance.headCollider.transform.position);
                Vector3 targetEuler = tripodTransform.rotation.eulerAngles;
                currentEuler.x = Mathf.LerpAngle(currentEuler.x, targetEuler.x, Time.deltaTime * followRate);
                currentEuler.y = Mathf.LerpAngle(currentEuler.y, targetEuler.y, Time.deltaTime * followRate);
                currentEuler.z = 0;
                tripodTransform.rotation = Quaternion.Euler(currentEuler);
                gameCameraTransform.position = tripodTransform.position;
                gameCameraTransform.rotation = tripodTransform.rotation;
                tripodTransform.localScale = Vector3.one * Player.Instance.scale;
                base.LateUpdate();
            }
            catch (Exception e) { Plugin.log.Exception(e); }
        }

        public Tripod Disable()
        {
            this.enabled = false;
            return this;
        }

        public override string DisplayName()
        {
            return "Selfie Stick";
        }
    }
}
