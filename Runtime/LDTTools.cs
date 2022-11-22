using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LandscapeDesignTool
{
    public static class LDTTools
    {
        public static Material MakeMaterial( Color col)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            // General Transparent Material Settings
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_ZWrite", 0.0f);
            material.renderQueue = (int)RenderQueue.Transparent;
            material.renderQueue += material.HasProperty("_QueueOffset") ? (int)material.GetFloat("_QueueOffset") : 0;
            material.SetShaderPassEnabled("ShadowCaster", false);
            material.SetColor("_BaseColor", col);

            return material;
        }
    }
}
