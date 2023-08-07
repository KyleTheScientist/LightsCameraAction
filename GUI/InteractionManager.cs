using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GorillaScience.Extensions;
using GorillaScience.Behaviors;
using System.Drawing;
using GorillaScience.Tools;

namespace LightsCameraAction.GUI
{
    public class InteractionManager : MonoBehaviour
    {
        public const string palmPath =
            PluginInfo.localRigPath + PluginInfo.palmPath;
        public const string pointerFingerPath =
            palmPath + "/f_index.01.{0}/f_index.02.{0}/f_index.03.{0}";

        public SphereCollider
            leftPalmCollider, rightPalmCollider,
            leftPointerCollider, rightPointerCollider;
        public void Awake()
        {
            try
            {
                var colliders = new List<SphereCollider>();
                foreach (var side in new string[] { "L", "R" })
                {
                    colliders.Add(
                        CreateCollider(
                            string.Format(palmPath, side),
                            $"LCA Palm Collider ({side})",
                            .025f
                        )
                    );

                    colliders.Add(
                        CreateCollider(
                            string.Format(pointerFingerPath, side),
                            $"LCA Pointer Collider ({side})",
                            .025f
                        )
                    );
                }

                leftPalmCollider = colliders[0];
                leftPointerCollider = colliders[1];

                rightPalmCollider = colliders[2];
                rightPointerCollider = colliders[3];
            }
            catch (Exception e) { Plugin.log.Exception(e); }
        }

        SphereCollider CreateCollider(string parentPath, string name, float radius)
        {
            Transform parent = GameObject.Find(parentPath).transform;
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<Rigidbody>().isKinematic = true;
            obj.layer = 4;

            var collider = obj.AddComponent<SphereCollider>();
            collider.radius = radius;
            collider.isTrigger = true;
            //collider.gameObject.AddComponent<ColliderRenderer>();
            return collider;
        }

        void OnDestroy()
        {
            leftPalmCollider?.gameObject?.Obliterate();
            rightPalmCollider?.gameObject?.Obliterate();
        }
    }
}
