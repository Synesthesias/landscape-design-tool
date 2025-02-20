using UnityEngine;

namespace Landscape2.Runtime.Common
{
    public static class LayerMaskUtil
    {
        public static void SetIgnore(GameObject target, bool isIgnore, int defaultLayer = 0)
        {
            if (defaultLayer == 0)
            {
                defaultLayer = LayerMask.NameToLayer("Default");
            }
            target.layer = isIgnore ? LayerMask.NameToLayer("Ignore Raycast") : defaultLayer;
        }
    }
}