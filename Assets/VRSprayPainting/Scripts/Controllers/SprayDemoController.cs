using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRSprayPainting
{
    /// <summary>
    /// Intro demo: the real spray gun lifts off the stool and paints the panel by itself (real paint, particles,
    /// sound and aim ring), showing the sweet spot, too close, too far and colour blending. Captions are raised
    /// through CaptionChanged. Skipping flies the gun back to the stool; actually grabbing it ends the demo in
    /// the user's hand. Pointer rays passing over the gun are ignored.
    /// </summary>
    [DefaultExecutionOrder(-50)] // move the gun before SprayPainter aims this frame
    public class SprayDemoController : MonoBehaviour
    {
        [Serializable]
        private struct Move
        {
            public string caption;
            [Tooltip("Start / end on the panel, -0.5..0.5 of the demo area.")]
            public Vector2 from;
            public Vector2 to;
            [Tooltip("Nozzle distance from the surface, metres.")]
            public float distance;
            public float duration;
            public bool spray;
            [Tooltip("Load the second demo colour before this move.")]
            public bool switchColor;
        }

        [SerializeField] private SprayGun sprayGun;
        [SerializeField] private SprayPainter sprayPainter;
        [SerializeField] private XRGrabInteractable gunGrab;
        [SerializeField] private Rigidbody gunBody;
        [SerializeField] private PaintResetInput paintReset;
        [Tooltip("Centre of the demo area; forward points INTO the surface, right/up span it.")]
        [SerializeField] private Transform demoAnchor;
        [SerializeField] private Vector2 areaSize = new Vector2(0.7f, 0.7f);
        [SerializeField] private Color secondColor = new Color(0.1f, 0.35f, 0.95f);
        [SerializeField] private float travelTime = 0.6f;
        [SerializeField] private float flyTime = 1.2f;
        [SerializeField] private float endHold = 1.5f;
        [SerializeField] private Move[] moves = DefaultMoves();

        public event Action<string> CaptionChanged;
        public event Action Finished;
        public bool IsPlaying { get; private set; }

        private Vector3 homePosition;
        private Quaternion homeRotation;
        private bool wasKinematic;
        private bool usedGravity;
        private Color originalColor;
        private Coroutine routine;
        private bool returning;
        private bool restorePhysicsOnRelease;

        private static Move[] DefaultMoves() => new[]
        {
            new Move { caption = "Hold the gun 15-30 cm away, straight on. The ring turns GREEN in the sweet spot.", from = new Vector2(-0.45f, 0.3f), to = new Vector2(-0.45f, 0.3f), distance = 0.22f, duration = 1.2f },
            new Move { caption = "Smooth, steady passes in the green zone give an even coat.", from = new Vector2(-0.45f, 0.3f), to = new Vector2(0.45f, 0.3f), distance = 0.22f, duration = 2.2f, spray = true },
            new Move { caption = "Overlap each pass by about half.", from = new Vector2(0.45f, 0.15f), to = new Vector2(-0.45f, 0.15f), distance = 0.22f, duration = 2.2f, spray = true },
            new Move { caption = "Too close (ORANGE): a small, heavy, wet spot.", from = new Vector2(-0.4f, -0.05f), to = new Vector2(0.4f, -0.05f), distance = 0.07f, duration = 2f, spray = true },
            new Move { caption = "Too far (YELLOW): thin, dry and speckled.", from = new Vector2(0.45f, -0.3f), to = new Vector2(-0.45f, -0.3f), distance = 0.4f, duration = 2.2f, spray = true },
            new Move { caption = "New colour over old paint blends like real paint: thin = tinted, heavy = covers.", from = new Vector2(0f, 0.42f), to = new Vector2(0f, 0.42f), distance = 0.22f, duration = 1.2f, switchColor = true },
            new Move { caption = "New colour over old paint blends like real paint: thin = tinted, heavy = covers.", from = new Vector2(0f, 0.42f), to = new Vector2(0f, -0.42f), distance = 0.22f, duration = 3f, spray = true },
        };

        // The stool pose from scene start: the demo always ends with the gun back there.
        private void Awake()
        {
            homePosition = gunBody.transform.position;
            homeRotation = gunBody.transform.rotation;
        }

        private void OnEnable()
        {
            gunGrab.selectEntered.AddListener(OnGunGrabbed);
            gunGrab.selectExited.AddListener(OnGunReleased);
        }

        private void OnDisable()
        {
            gunGrab.selectEntered.RemoveListener(OnGunGrabbed);
            gunGrab.selectExited.RemoveListener(OnGunReleased);
            if (IsPlaying)
                End(returnHome: true);
        }

        /// <summary>Plays the demo. Returns false (and does nothing) if the gun is already in a hand.</summary>
        public bool Play()
        {
            if (IsPlaying || returning || gunGrab.isSelected)
                return false;

            IsPlaying = true;
            wasKinematic = gunBody.isKinematic;
            usedGravity = gunBody.useGravity;
            gunBody.isKinematic = true;
            gunBody.useGravity = false;
            originalColor = sprayPainter.Color;
            sprayGun.SetAutopilot(true);
            routine = StartCoroutine(Run());
            return true;
        }

        /// <summary>Skip: stops spraying and flies the gun back to the stool, then ends (Finished fires).</summary>
        public void Stop()
        {
            if (!IsPlaying || returning)
                return;
            if (routine != null)
                StopCoroutine(routine);
            routine = StartCoroutine(ReturnHome());
        }

        private IEnumerator ReturnHome()
        {
            returning = true;
            sprayGun.SetAutoSpray(false);
            Transform t = gunBody.transform;
            yield return Fly(t.position, t.rotation, homePosition, homeRotation, flyTime);
            End(returnHome: true);
        }

        private IEnumerator Run()
        {
            paintReset.ResetAll();

            // Lift off the stool to the first point.
            Pose start = PoseAt(moves[0].from, moves[0].distance);
            Transform gun = gunBody.transform;
            yield return Fly(gun.position, gun.rotation, start.position, start.rotation, flyTime);

            Vector2 at = moves[0].from;
            float distance = moves[0].distance;
            foreach (Move move in moves)
            {
                Caption(move.caption);
                if (move.switchColor)
                    ApplyColor(secondColor);

                // Travel to the start of the move with the trigger released.
                if ((move.from - at).sqrMagnitude > 1e-6f || !Mathf.Approximately(move.distance, distance))
                {
                    sprayGun.SetAutoSpray(false);
                    yield return Glide(at, distance, move.from, move.distance, travelTime);
                }

                sprayGun.SetAutoSpray(move.spray);
                yield return Glide(move.from, move.distance, move.to, move.distance, move.duration);
                sprayGun.SetAutoSpray(false);
                at = move.to;
                distance = move.distance;
            }

            Caption("Your turn! Pick up the spray gun.");
            yield return new WaitForSeconds(endHold);
            yield return ReturnHome();
        }

        private IEnumerator Glide(Vector2 from, float fromDist, Vector2 to, float toDist, float duration)
        {
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, time / duration);
                Place(PoseAt(Vector2.Lerp(from, to, k), Mathf.Lerp(fromDist, toDist, k)));
                yield return null;
            }
            Place(PoseAt(to, toDist));
        }

        // Moves the gun body (not the nozzle) between two gun poses with an arc, used to leave and return to the stool.
        private IEnumerator Fly(Vector3 fromPos, Quaternion fromRot, Vector3 toPos, Quaternion toRot, float duration)
        {
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, time / duration);
                Vector3 p = Vector3.Lerp(fromPos, toPos, k) + Vector3.up * (Mathf.Sin(k * Mathf.PI) * 0.12f);
                SetGun(p, Quaternion.Slerp(fromRot, toRot, k));
                yield return null;
            }
            SetGun(toPos, toRot);
        }

        // Gun pose that puts the nozzle at a point of the demo area, pointing straight into the surface.
        private Pose PoseAt(Vector2 areaPoint, float distance)
        {
            Transform gun = gunBody.transform;
            Transform nozzle = sprayGun.Nozzle;
            Vector3 nozzlePos = demoAnchor.position
                                + demoAnchor.right * (areaPoint.x * areaSize.x)
                                + demoAnchor.up * (areaPoint.y * areaSize.y)
                                - demoAnchor.forward * distance;

            Quaternion nozzleInGun = Quaternion.Inverse(gun.rotation) * nozzle.rotation;
            Quaternion gunRot = Quaternion.LookRotation(demoAnchor.forward, demoAnchor.up) * Quaternion.Inverse(nozzleInGun);
            Vector3 nozzleOffset = gunRot * Quaternion.Inverse(gun.rotation) * (nozzle.position - gun.position);
            return new Pose(nozzlePos - nozzleOffset, gunRot);
        }

        private void Place(Pose pose) => SetGun(pose.position, pose.rotation);

        private void SetGun(Vector3 position, Quaternion rotation)
        {
            gunBody.position = position;
            gunBody.rotation = rotation;
            gunBody.transform.SetPositionAndRotation(position, rotation);
        }

        private void End(bool returnHome)
        {
            if (!IsPlaying)
                return;

            IsPlaying = false;
            returning = false;
            if (routine != null && !returnHome)
                StopCoroutine(routine);
            routine = null;

            sprayGun.SetAutoSpray(false);
            sprayGun.SetAutopilot(false);
            if (returnHome)
                SetGun(homePosition, homeRotation);
            // While held, XRI owns the body; the real flags go back on release (OnGunReleased).
            if (!restorePhysicsOnRelease)
            {
                gunBody.isKinematic = wasKinematic;
                gunBody.useGravity = usedGravity;
                if (!gunBody.isKinematic)
                {
                    gunBody.linearVelocity = Vector3.zero;
                    gunBody.angularVelocity = Vector3.zero;
                }
            }

            ApplyColor(originalColor);
            paintReset.ResetAll();
            Finished?.Invoke();
        }

        private void ApplyColor(Color color)
        {
            sprayPainter.SetColor(color);
            sprayGun.SetSprayColor(color);
        }

        private void Caption(string text) => CaptionChanged?.Invoke(text);

        // Grabbed mid-demo: it stays in the hand. XRI recorded the demo's kinematic/no-gravity state at grab time
        // and would restore it on release (gun floating in the air), so put the real values back after release.
        private void OnGunGrabbed(SelectEnterEventArgs args)
        {
            if (!IsPlaying)
                return;
            restorePhysicsOnRelease = true;
            End(returnHome: false);
        }

        private void OnGunReleased(SelectExitEventArgs args)
        {
            if (!restorePhysicsOnRelease)
                return;
            restorePhysicsOnRelease = false;
            gunBody.isKinematic = wasKinematic;
            gunBody.useGravity = usedGravity;
        }
    }
}
