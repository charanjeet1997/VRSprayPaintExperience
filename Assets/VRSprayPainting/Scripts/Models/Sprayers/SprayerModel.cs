using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// One look of the held sprayer (gun body, can...). Lives inside the sprayer object; the SprayGun turns the
    /// model for the active profile on and moves the shared nozzle (spray effects, sound) and grab point onto it.
    /// </summary>
    public class SprayerModel : MonoBehaviour
    {
        [SerializeField] private SprayerProfile profile;
        [Tooltip("Visual meshes, shown only for this profile.")]
        [SerializeField] private GameObject visualRoot;
        [Tooltip("Where the spray comes out; forward = spray direction.")]
        [SerializeField] private Transform nozzleAnchor;
        [Tooltip("Where the hand holds it.")]
        [SerializeField] private Transform attachAnchor;
        [Tooltip("Grab/physics colliders of this shape.")]
        [SerializeField] private Collider[] colliders = new Collider[0];
        [Tooltip("Shows the loaded paint colour (paint cup, can cap...).")]
        [SerializeField] private Renderer paintTintRenderer;
        [Tooltip("Resting pose in the booth (on the stool). Scene object outside the sprayer.")]
        [SerializeField] private Transform restPose;

        public SprayerProfile Profile => profile;
        public Transform NozzleAnchor => nozzleAnchor;
        public Transform AttachAnchor => attachAnchor;
        public Renderer PaintTintRenderer => paintTintRenderer;
        public Transform RestPose => restPose;

        public void SetActive(bool active)
        {
            if (visualRoot != null)
                visualRoot.SetActive(active);
            foreach (Collider col in colliders)
                if (col != null)
                    col.enabled = active;
        }
    }
}
