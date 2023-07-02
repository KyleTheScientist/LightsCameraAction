using GorillaScience.Behaviors;
using GorillaScience.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;

namespace LightsCameraAction.GUI
{
    public class MenuController : MonoBehaviour
    {
        public List<ModeButton> modeButtons;
        public ModeButton cinematicButton;
        void Awake()
        {
            modeButtons = new List<ModeButton>();
            foreach (Transform t in transform.Find("Options"))
            {
                modeButtons.Add(t.gameObject.AddComponent<ModeButton>());
            }
            modeButtons[0].module = Plugin.Instance.firstPerson;
            modeButtons[1].module = Plugin.Instance.selfieStick;
            modeButtons[2].module = Plugin.Instance.tripod;
            modeButtons[3].module = Plugin.Instance.cinematic;
            cinematicButton = modeButtons[3];
        }

        public void UnhoverAll()
        {
            foreach (var button in modeButtons)
               button.Unhover();
        }

        List<Vector2> cardinals = new List<Vector2>() {
            Vector2.up, Vector2.right, Vector2.down, Vector2.left
        };
        void Update()
        {
            Vector2 stickDir;
            InputTracker.Instance.leftController
                .TryGetFeatureValue(CommonUsages.primary2DAxis, out stickDir);

            if (stickDir.magnitude < .75f) return;

            Vector2 closest = Vector3.zero;
            float d, closestDistance = Mathf.Infinity;

            foreach (var direction in cardinals)
            {
                d = Vector3.Distance(direction, stickDir);
                if (d < closestDistance)
                {
                    closest = direction;
                    closestDistance = d;
                }
            }


            for(int i = 0; i < cardinals.Count; i++)
            {
                if(closest == cardinals[i])
                {
                    if (!Plugin.Instance.inRoom && modeButtons[i].module == Plugin.Instance.cinematic) return;
                    Plugin.Instance.targetModule = modeButtons[i].module;
                    modeButtons[i].Hover();
                }
            }
        }
    }
}
