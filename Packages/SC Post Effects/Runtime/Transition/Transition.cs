﻿using UnityEngine.Rendering.Universal;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SCPE
{
    [Serializable, VolumeComponentMenu("SC Post Effects/Screen/Transition")]
    [SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
    public sealed class Transition : VolumeComponent, IPostProcessComponent
    {
        public TextureParameter gradientTex = new TextureParameter(null);

        public ClampedFloatParameter progress = new ClampedFloatParameter(0f, 0f, 1f);

        public bool IsActive() => progress.value > 0f && this.active;

        public bool IsTileCompatible() => false;
        
        [SerializeField]
        public Shader shader;

        private void Reset()
        {
            SerializeShader();
        }
        
        private bool SerializeShader()
        {
            bool wasSerialized = !shader;
            shader = Shader.Find(ShaderNames.Transition);

            return wasSerialized;
        }
    }
}