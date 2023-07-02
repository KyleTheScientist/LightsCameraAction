using GorillaScience.Behaviors;
using GorillaScience.Tools;
using LightsCameraAction.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LightsCameraAction.GUI
{
    public class ModeButton : MonoBehaviour
    {
        public LCAModule module;
        private Material material;
        bool hovered;
        Color selectedColor = new Color(0, 1, 0.873f, .5f);
        Color normalColor = new Color(1, 1, 1, .3f);

        void Awake()
        {
            this.material = this.GetComponentInChildren<MeshRenderer>().material;
            material.color = normalColor;
            material.name = "Mode Button Material";
        }

        public void Hover()
        {
            if (hovered) return;

            Plugin.Instance.menuController.UnhoverAll();
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, false, 0.05f);
            InputTracker.Instance.leftController.SendHapticImpulse(0u, 0.1f, 0.1f);
            this.transform.localScale = Vector3.one * 1.1f;
            material.color = selectedColor;
            hovered = true;
        }

        public void Unhover()
        {
            if (!hovered) return;

            this.transform.localScale = Vector3.one * .9f;
            material.color = normalColor;
            hovered = false;
        }
    }
}
