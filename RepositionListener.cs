using GorillaLocomotion;
using GorillaScience.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LightsCameraAction
{
    public class RepositionListener : MonoBehaviour
    {
        Repositionable moveTarget, rotateTarget;
        Transform activeHand;
        public float followRate = 40f;

        void Start()
        {
            Plugin.log.Debug("Start");

            Plugin.Instance.inputTracker.leftGrip.OnPressed += LeftTryMoving;
            Plugin.Instance.inputTracker.leftTrigger.OnPressed += LeftTryRotating;
            Plugin.Instance.inputTracker.rightGrip.OnPressed += RightTryMoving;
            Plugin.Instance.inputTracker.rightTrigger.OnPressed += RightTryRotating;

            Plugin.Instance.inputTracker.leftGrip.OnReleased += LeftClearMoving;
            Plugin.Instance.inputTracker.leftTrigger.OnReleased += LeftClearRotating;
            Plugin.Instance.inputTracker.rightGrip.OnReleased += RightClearMoving;
            Plugin.Instance.inputTracker.rightTrigger.OnReleased += RightClearRotating;
            Plugin.log.Debug("End");
        }

        void LeftTryMoving() { TryMoving(true); }
        void RightTryMoving() { TryMoving(false); }
        void LeftTryRotating() { TryRotating(true); }
        void RightTryRotating() { TryRotating(false); }

        void LeftClearMoving() { moveTarget = null; }
        void RightClearMoving() { moveTarget = null; }
        void LeftClearRotating() { rotateTarget = null; }
        void RightClearRotating() { rotateTarget = null; }

        void TryMoving(bool left)
        {
            var target = FindClosestRepositionable(left);
            if (!target) return;
            moveTarget = target;
            if (rotateTarget != moveTarget)
                rotateTarget = null;
            activeHand = left ? Player.Instance.leftControllerTransform : Player.Instance.rightControllerTransform;
        }

        void TryRotating(bool left)
        {
            var target = FindClosestRepositionable(left);
            if (!target) return;
            rotateTarget = target;
            if (moveTarget != rotateTarget)
                moveTarget = null;
            activeHand = left ? Player.Instance.leftControllerTransform : Player.Instance.rightControllerTransform;
        }

        void FixedUpdate()
        {
            if (moveTarget && moveTarget.canMove)
            {
                moveTarget.transform.position = Vector3.Lerp(
                    moveTarget.transform.position,
                    activeHand.position + Vector3.up * .1f * Player.Instance.scale,
                    followRate * Time.deltaTime
                );
            }
            if (rotateTarget && rotateTarget.canRotate)
            {
                Vector3 currentEuler = rotateTarget.transform.rotation.eulerAngles;
                Vector3 targetEuler = activeHand.rotation.eulerAngles;
                currentEuler.x = Mathf.LerpAngle(currentEuler.x, targetEuler.x, Time.deltaTime * followRate);
                currentEuler.y = Mathf.LerpAngle(currentEuler.y, targetEuler.y, Time.deltaTime * followRate);
                currentEuler.z = 0;
                rotateTarget.transform.rotation = Quaternion.Euler(currentEuler);
            }

        }

        public Repositionable FindClosestRepositionable(bool left)
        {
            Repositionable closestObject = null;
            float closestDistance = Mathf.Infinity;
            Transform playerHand = left ? Player.Instance.leftControllerTransform : Player.Instance.rightControllerTransform;

            // Iterate through each object to find the closest one
            foreach (Repositionable repositionableObject in FindObjectsOfType<Repositionable>())
            {
                float distanceToPlayerHand = Vector3.Distance(
                    playerHand.position, 
                    repositionableObject.transform.position - repositionableObject.transform.forward * .1f * Player.Instance.scale
                );
                if (distanceToPlayerHand < closestDistance && distanceToPlayerHand < .2f * Player.Instance.scale)
                {
                    closestDistance = distanceToPlayerHand;
                    closestObject = repositionableObject;
                }
            }

            return closestObject;
        }
    }

    public class Repositionable : MonoBehaviour { public bool canRotate = true, canMove = true; }
}
