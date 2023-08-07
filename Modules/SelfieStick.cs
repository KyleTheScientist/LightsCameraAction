using GorillaLocomotion;
using GorillaScience.Extensions;
using GorillaScience.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LightsCameraAction.Modules
{
    public class SelfieStick : LCAModule
    {
        private Transform cameraTransform, stickTransform, screenTransform;
        public float followRate = 20f;
        private GameObject stickPrefab;
        private Camera selfieCamera;

        public const string localRigPath =
            PluginInfo.localRigPath;
        public const string palmPath =
            PluginInfo.palmPath;

        void Awake()
        {
            try
            {
                stickPrefab = Plugin.Load<GameObject>("LCA Selfie Stick");
                stickPrefab.SetActive(false);
                stickPrefab.transform.SetParent(Plugin.Instance.transform);
            }
            catch (Exception e)
            {
                Plugin.log.Exception(e);
            }
        }

        void OnEnable()
        {
            try
            {
                if (!Plugin.Instance.Initialized) return;
                LCAModule.brain.enabled = false;
                cameraTransform = LCAModule.shoulderCamera.transform;
                stickTransform = Instantiate(stickPrefab).transform;
                stickTransform.localRotation = Quaternion.Euler(82, 180, 0);
                stickTransform.localPosition = new Vector3(-0.025f, 0.025f, 0);
                stickTransform.gameObject.SetActive(true);
                screenTransform = stickTransform.Find("Screen");
                selfieCamera = screenTransform.GetComponentInChildren<Camera>();
                selfieCamera.targetTexture =
                    screenTransform.GetComponent<Renderer>().material.mainTexture as RenderTexture;
                selfieCamera.aspect = 1;
                selfieCamera.fieldOfView = shoulderCamera.fieldOfView;
                stickTransform.SetParent(GameObject.Find(localRigPath + string.Format(palmPath, "R")).transform, false);
            }
            catch (Exception e) { Plugin.log.Exception(e); }
        }

        void OnDisable()
        {
            LCAModule.Reset();
            stickTransform?.gameObject?.Obliterate();
        }

        protected override void LateUpdate()
        {
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                screenTransform.TransformPoint(new Vector3(0, .1f, 0.1f) * Player.Instance.scale),
                followRate * Time.deltaTime
            );
            selfieCamera.transform.position = cameraTransform.position;
            Vector3 currentEuler = cameraTransform.rotation.eulerAngles;
            Vector3 targetEuler = screenTransform.rotation.eulerAngles;
            currentEuler.x = Mathf.LerpAngle(currentEuler.x, targetEuler.x, Time.deltaTime * followRate);
            currentEuler.y = Mathf.LerpAngle(currentEuler.y, targetEuler.y, Time.deltaTime * followRate);
            currentEuler.z = 0;
            cameraTransform.rotation = Quaternion.Euler(currentEuler);
            selfieCamera.transform.position = cameraTransform.position;
            selfieCamera.transform.rotation = cameraTransform.rotation;
            base.LateUpdate();
        }

        public SelfieStick Disable()
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
