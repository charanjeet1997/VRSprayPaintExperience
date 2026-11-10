using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// On a [SerializeReference] field or list: the Inspector shows a dropdown of the concrete subclasses to pick
    /// from (e.g. which lesson step or practice goal). Drawn by SubclassSelectorDrawer.
    /// </summary>
    public class SubclassSelectorAttribute : PropertyAttribute
    {
    }
}
