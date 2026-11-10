using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Cached shader property IDs used by the paint pipeline.
    /// </summary>
    public static class ShaderIDs
    {
        public static readonly int PaintTex = Shader.PropertyToID("_PaintTex");
        public static readonly int MainTex = Shader.PropertyToID("_MainTex");
        public static readonly int Center = Shader.PropertyToID("_Center");
        public static readonly int Softness = Shader.PropertyToID("_Softness");
        public static readonly int Color = Shader.PropertyToID("_Color");
        public static readonly int Amount = Shader.PropertyToID("_Amount");
        public static readonly int Seed = Shader.PropertyToID("_Seed");
        public static readonly int AxisU = Shader.PropertyToID("_AxisU");
        public static readonly int AxisV = Shader.PropertyToID("_AxisV");
        public static readonly int Grain = Shader.PropertyToID("_Grain");
        public static readonly int Overspray = Shader.PropertyToID("_Overspray");
        /// <summary>Global: 1 shows paint thickness as a heat map on every painted surface.</summary>
        public static readonly int ThicknessView = Shader.PropertyToID("_VRSP_ThicknessView");
        /// <summary>Global: film target min / max as paint volume, for the heat map bands.</summary>
        public static readonly int FilmLow = Shader.PropertyToID("_VRSP_FilmLow");
        public static readonly int FilmHigh = Shader.PropertyToID("_VRSP_FilmHigh");
        /// <summary>Per renderer: 1 = wet paint just sprayed, 0 = dry.</summary>
        public static readonly int Wetness = Shader.PropertyToID("_Wetness");
    }
}
