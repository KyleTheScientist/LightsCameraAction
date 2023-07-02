using Cinemachine;
using GorillaScience.Behaviors;
using GorillaScience.Extensions;
using GorillaScience.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CinemachineThirpy = Cinemachine.Cinemachine3rdPersonFollow;
namespace LightsCameraAction.Modules
{
    public abstract class LCAModule : MonoBehaviour
    {
        public abstract string DisplayName();


        public static CinemachineThirpy thirpy;
        public static CinemachineBrain brain;
        public static Vector3 baseOffset, baseScale;
        public static float baseDistance, baseRadius, baseArmLength;
        public static Camera shoulderCamera;
        public static Transform baseParent;
        static DebugRay x, y, z;
        static Transform debugVisual;
        public static void Baseline()
        {
            try
            {
                thirpy = FindObjectOfType<CinemachineThirpy>();
                brain = thirpy.gameObject.GetComponentInParent<CinemachineBrain>();
                shoulderCamera = thirpy.gameObject.GetComponentInParent<Camera>();
                baseParent = shoulderCamera.transform.parent;
                baseDistance = thirpy.CameraDistance;
                baseOffset = thirpy.ShoulderOffset;
                baseScale = thirpy.FollowTarget.localScale;
                baseRadius = thirpy.CameraRadius;
                baseArmLength = thirpy.VerticalArmLength;

                //debugVisual = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                //debugVisual.GetComponent<Collider>().Obliterate();
                //debugVisual.SetParent(shoulderCamera.gameObject.transform, false);
                //debugVisual.localScale = Vector3.one * .25f;

                //x = new GameObject("LCA DebugRay X").AddComponent<DebugRay>().SetColor(Color.red);
                //y = new GameObject("LCA DebugRay Y").AddComponent<DebugRay>().SetColor(Color.green);
                //z = new GameObject("LCA DebugRay Z").AddComponent<DebugRay>().SetColor(Color.blue);
            } catch (Exception e) { Plugin.log.Exception(e); }
        }

        protected virtual void LateUpdate()
        {
            //x.Set(shoulderCamera.transform.position, shoulderCamera.transform.right);
            //y.Set(shoulderCamera.transform.position, shoulderCamera.transform.up);
            //z.Set(shoulderCamera.transform.position, shoulderCamera.transform.forward);
        }

        protected static void Reset()
        {
            thirpy.enabled = true;
            brain.enabled = true;
            shoulderCamera.transform.parent = baseParent ;
            thirpy.CameraDistance = baseDistance ;
            thirpy.ShoulderOffset = baseOffset ;
            thirpy.FollowTarget.localScale = baseScale ;
            thirpy.CameraRadius  = baseRadius ;
            thirpy.VerticalArmLength = baseArmLength;
        }
    }
}
