using GorillaLocomotion;
using GorillaScience.Behaviors;
using GorillaScience.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LightsCameraAction.Modules
{
    public class Cinematic : LCAModule
    {
        private GameObject laptopPrefab;
        private Laptop laptop;
        private Camera laptopCamera;

        public Path path;
        public Transform pathPoints;

        void Awake()
        {
            try
            {
                laptopPrefab = Plugin.Load<GameObject>("LCA Laptop");
                laptopPrefab.SetActive(false);
                laptopPrefab.transform.SetParent(Plugin.Instance.transform);
            }
            catch (Exception e) { Plugin.log.Exception(e); }
        }

        void OnEnable()
        {
            try
            {
                if (!Plugin.Instance.Initialized) return;
                LCAModule.brain.enabled = false;

                path = new GameObject("LCA Path Component").AddComponent<Path>();
                laptop = Instantiate(laptopPrefab).AddComponent<Laptop>();
                laptop.gameObject.SetActive(true);
                laptop.path = path;
                laptop.transform.localRotation = Quaternion.Euler(0, 90, 90);

                laptopCamera = laptop.GetComponentInChildren<Camera>();
                laptopCamera.targetTexture =
                    laptop.transform.Find("Screen")
                        .GetComponent<Renderer>()
                            .material.mainTexture as RenderTexture;
                laptopCamera.aspect = 1;
                laptopCamera.fieldOfView = 90;
                laptopCamera.nearClipPlane = shoulderCamera.nearClipPlane;
                laptopCamera.farClipPlane = 10000;
                laptopCamera.transform.SetParent(null);
                laptopCamera.cullingMask = shoulderCamera.cullingMask;

                laptop.transform.SetParent(Player.Instance.leftControllerTransform, false);
                laptop.transform.localPosition = Vector3.right * .05f;

                path.pathFollower = laptopCamera.transform;
            }
            catch (Exception e) { Plugin.log.Exception(e); }
        }

        void OnDisable()
        {
            path?.Obliterate();
            laptop?.gameObject?.Obliterate();
            laptopCamera?.gameObject.Obliterate();
            LCAModule.Reset();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            try
            {
                shoulderCamera.transform.position = laptopCamera.transform.position;
                shoulderCamera.transform.rotation = laptopCamera.transform.rotation;
            }
            catch (Exception e)
            {
                Plugin.log.Exception(e);
            }
        }

        public Cinematic Disable()
        {
            this.enabled = false;
            return this;
        }

        public override string DisplayName()
        {
            return "Dolly";
        }

        void OnDestroy()
        {
            path?.Obliterate();
        }
    }

    public class Laptop : MonoBehaviour
    {
        public Path path;
        public static Laptop Instance;
        public GameObject physicalCamera;
        public float palmUpTime;
        public bool Locked { get; private set; }
        public bool palmUp { get; private set; }
        Dictionary<string, LaptopButton> buttons;
        Text speedText;

        void Awake()
        {
            Instance = this;
            physicalCamera = this.transform.Find("Laptop Camera/Physical Camera").gameObject;
            speedText = GetComponentInChildren<Text>();
        }

        void Start()
        {
            try
            {
                string name;
                LaptopButton button;
                buttons = new Dictionary<string, LaptopButton>();
                foreach (Transform t in this.transform.Find("Buttons"))
                {
                    name = t.name.ToLower();
                    button = t.gameObject.AddComponent<LaptopButton>();
                    buttons.Add(button.name, button);
                    if (name == "add") button.OnPress += AddPoint;
                    if (name == "undo") button.OnPress += UndoPoint;
                    if (name == "lock") button.OnPress += LockUnlock;
                    if (name == "play") button.OnPress += PlayPause;
                    if (name == "loop") button.OnPress += Loop;
                    if (name == "speed") button.OnPress += Speed;
                }
                speedText.text = $"{path.CycleSpeed()}x";
            }
            catch (Exception e) { Plugin.log.Exception(e); }
        }

        void FixedUpdate()
        {
            if (Time.frameCount % 10 == 0)
            {
                UpdateIcons();
            }


            bool palmWasUp = palmUp;
            Vector3 palmNormal =
                Plugin.Instance.interactionManager.leftPalmCollider.transform.forward;
            palmUp = Vector3.Dot(palmNormal, Vector3.up) > .75f;

            if (!palmWasUp && palmUp)
                palmUpTime = Time.time;

            this.transform.localScale =
                palmUp && Time.time - Laptop.Instance.palmUpTime > 1 ?
                Vector3.one :
                Vector3.zero;
        }

        void UpdateIcons()
        {
            try
            {
                buttons["Add"].SetColor(!path.playing? Color.cyan : Color.black);
                buttons["Undo"].SetColor(!path.playing ? Color.cyan : Color.black);
                buttons["Lock"].SetColor(Locked ? Color.cyan : Color.black);
                buttons["Play"].SetColor(path.playing ? Color.cyan : Color.black);
                buttons["Loop"].SetColor(Path.loop ? Color.cyan : Color.black);
                buttons["Speed"].SetColor(true ? Color.cyan : Color.black);

                var playBtn = transform.Find("Buttons/Play");

                playBtn.Find("Play Button/Play Icon")
                    .gameObject.SetActive(!path.playing);
                playBtn.Find("Play Button/Pause Icon")
                    .gameObject.SetActive(path.playing);
            }
            catch (Exception e) { Plugin.log.Exception(e); }
        }

        private void LockUnlock()
        {
            this.Locked = !this.Locked;
            path.target = Locked? Player.Instance.headCollider.transform : null;
            Sound.Play();
            InputTracker.Instance.HapticPulse(false);
            UpdateIcons();
        }

        private void Speed()
        {
            speedText.text = $"{path.CycleSpeed()}x";
            Sound.Play();
            InputTracker.Instance.HapticPulse(false);
        }

        private void Loop()
        {
            Path.loop = !Path.loop;
            Sound.Play();
            InputTracker.Instance.HapticPulse(false);
            UpdateIcons();
        }

        private void PlayPause()
        {
            if (path.playing)
                path.Pause();
            else
                path.Resume();
            UpdateIcons();
            Sound.Play();
            InputTracker.Instance.HapticPulse(false);
        }

        private void UndoPoint()
        {
            if (path.RemoveLastPoint())
            {
                Sound.Play();
                InputTracker.Instance.HapticPulse(false);
            }
        }

        private void AddPoint()
        {
            if (path.playing) return;
            try
            {
                Transform point =
                    Instantiate(Plugin.Load<GameObject>("Ghost Camera")).transform;
                point.gameObject.AddComponent<Repositionable>();
                var playerHead = Player.Instance.headCollider.transform;
                point.transform.position = playerHead.position;
                point.transform.rotation = Quaternion.Euler(playerHead.rotation.eulerAngles.x, playerHead.rotation.eulerAngles.y, 0);
                if (path.AddPoint(point))
                {
                    Sound.Play();
                    InputTracker.Instance.HapticPulse(false);
                }
                else
                    point.gameObject.Obliterate();
            }
            catch (Exception e) { Plugin.log.Exception(e); }
        }
    }

    public class LaptopButton : MonoBehaviour
    {
        public Action OnPress;
        private float lastRelease;
        public void SetColor(Color color)
        {
            Material material;
            foreach (Transform icon in transform.GetChild(0))
            {
                material = icon.gameObject.GetComponent<Renderer>().material;
                material.color = color;
                material.SetColor("_EmissionColor", color);
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            if (collider != Plugin.Instance.interactionManager.rightPointerCollider
                || Time.time - lastRelease < .1f) return;

            //if (!name.ToLower().Contains("lock") && Laptop.Instance.Locked) return;

            transform.GetChild(0).localPosition = Vector3.back * .01f;
            OnPress?.Invoke();
        }

        void OnTriggerExit(Collider collider)
        {
            if (collider != Plugin.Instance.interactionManager.rightPointerCollider) return;
            transform.GetChild(0).localPosition = Vector3.zero;
            lastRelease = Time.time;
        }

    }

    public class Path : MonoBehaviour
    {
        public Transform container, pathFollower, target;
        public float speed = 1f;
        private int currentPoint = 0;
        public LineRenderer lineRenderer;
        private float startTime;
        public bool playing, reachedEndOfPath;

        // Loop is static so the setting will stay even if this is destroyed
        public static bool loop;

        void Awake()
        {
            try
            {
                var points = GameObject.Find("LCA Path Points");
                if (!points)
                    points = new GameObject("LCA Path Points");
                container = points.transform;
                lineRenderer = Instantiate(
                    Plugin.Load<GameObject>("LCA Path Renderer")
                        .GetComponent<LineRenderer>());
                lineRenderer.enabled = false;
            }
            catch (Exception e) { Plugin.log.Exception(e); }
        }

        void Start()
        {
            startTime = Time.time;
            this.playing = false;
        }

        private void FixedUpdate()
        {
            if (Time.frameCount % 10 == 0)
                UpdateLine();

            if (!playing)
            {
                Transform headTransform = Player.Instance.headCollider.transform;
                pathFollower.position = headTransform
                    .TransformPoint(Vector3.forward * .2f);
                pathFollower.rotation = headTransform.rotation;
                Laptop.Instance.physicalCamera.SetActive(false);
                return;
            }

            if (container.childCount == 0) return;
            Laptop.Instance.physicalCamera.SetActive(true);

            Transform start = container.GetChild(currentPoint);
            Transform end = container.GetChild((currentPoint + 1) % container.childCount);

            float distance = Vector3.Distance(start.position, end.position);
            float timeToReachTarget = Mathf.Max(distance / speed, 1) / Player.Instance.scale;

            float progress = (Time.time - startTime) / timeToReachTarget;
            pathFollower.position = Vector3.Lerp(start.position, end.position, progress);
            if (target)
                pathFollower.transform.LookAt(target, Vector3.up);
            else
                pathFollower.rotation = Quaternion.Lerp(start.rotation, end.rotation, progress);

            if (progress > 0.999f)
            {
                if (!loop && currentPoint + 1 == container.childCount - 1)
                {
                    reachedEndOfPath = true;
                    return;
                }
                currentPoint = (currentPoint + 1) % container.childCount;
                startTime = Time.time;
            }
        }

        public void SetGhostsVisible(bool visible)
        {
            foreach (Transform point in container)
            {
                point.GetComponentInChildren<Renderer>().enabled = visible;
            }
        }

        public void UpdateLine()
        {
            if (container.childCount == 0 || playing)
            {
                lineRenderer.enabled = false;
                SetGhostsVisible(false);
                return;
            }

            lineRenderer.enabled = true;
            SetGhostsVisible(true);

            Vector3[] positions = new Vector3[container.childCount];
            for (int i = 0; i < container.childCount; i++)
            {
                container.GetChild(i).transform.localScale = Vector3.one * Player.Instance.scale;
                positions[container.childCount - 1 - i] = container.GetChild(i).position;

            }
            lineRenderer.loop = loop;
            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }

        public bool AddPoint(Transform point)
        {
            if (playing) return false;
            point.SetParent(container);
            if (container.childCount == 2)
            {
                Pause();
            }
            return true;
        }

        public bool RemoveLastPoint()
        {
            if (playing || container.childCount < 1) return false;
            LastPoint()?.gameObject.Obliterate();
            return true;
        }

        public void Pause()
        {
            Plugin.log.Debug("Pausing");
            playing = false;
            UpdateLine();
        }

        public void Resume()
        {
            UpdateLine();
            if (reachedEndOfPath)
                Restart();
            if (container.childCount == 0) return;
            Plugin.log.Debug("Resuming");
            startTime = Time.time;
            playing = true;
        }

        public void Restart()
        {
            if (container.childCount < 1) return;
            currentPoint = 0;
            transform.position = container.GetChild(0).position;
            transform.rotation = container.GetChild(0).rotation;
            reachedEndOfPath = false;
        }

        public float CycleSpeed()
        {
            speed = (speed * 2);
            if (speed >= 10)
                speed = 1;
            return speed;
        }

        void OnDestroy()
        {
            SetGhostsVisible(false);
            lineRenderer?.gameObject?.Obliterate();
        }

        Transform LastPoint()
        {
            if (container.childCount == 0) return null;
            return container.GetChild(container.childCount - 1);
        }
    }
}
