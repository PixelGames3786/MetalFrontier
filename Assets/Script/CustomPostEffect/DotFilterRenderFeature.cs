using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class DotFilterRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader _shader;
    [SerializeField] private PostprocessTiming _timing = PostprocessTiming.AfterOpaque;
    [SerializeField] private bool _applyToSceneView = true;

    private DotFilterRenderPass _postProcessPass;

    public override void Create()
    {
        _postProcessPass = new DotFilterRenderPass(_applyToSceneView, _shader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_postProcessPass);
    }
}
