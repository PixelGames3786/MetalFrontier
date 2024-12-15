using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Unity.VisualScripting.Member;

public enum PostprocessTiming
{
    AfterOpaque,
    BeforePostprocess,
    AfterPostprocess
}

public class DotFilterRenderPass : ScriptableRenderPass
{
    private Material _material;
    private bool _applyToSceneView;

    private const string RenderPassName = nameof(DotFilterRenderPass);
    private const string ProfilingSamplerName = "SrcToDest";

    private readonly int _mainTexPropertyId = Shader.PropertyToID("_MainTex");
    private readonly ProfilingSampler _profilingSampler;
    private readonly int _tintColorPropertyId = Shader.PropertyToID("_DotFactor");

    private RenderTargetHandle _afterPostProcessTexture;
    private RenderTargetIdentifier _cameraColorTarget;
    private RenderTargetHandle _tempRenderTargetHandle;
    private DotFilterPostEffect _volume;

    public DotFilterRenderPass(bool applyToSceneView, Shader shader)
    {
        _applyToSceneView = applyToSceneView;
        if (shader != null)
        {
            _material = new Material(shader);
        }

                // RenderPassEvent.AfterRenderingではポストエフェクトを掛けた後のカラーテクスチャがこの名前で取得できる
        _afterPostProcessTexture.Init("_AfterPostProcessTexture");
    }

    public void Setup(RenderTargetIdentifier cameraColorTarget)
    {
        _cameraColorTarget = cameraColorTarget;


    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {

        // Volumeコンポーネントを取得
        var volumeStack = VolumeManager.instance.stack;
        _volume = volumeStack.GetComponent<DotFilterPostEffect>();

        if (_material == null)
        {
            return;
        }

        // カメラのポストプロセス設定が無効になっていたら何もしない
        if (!renderingData.cameraData.postProcessEnabled)
        {
            return;
        }

        // カメラがシーンビューカメラかつシーンビューに適用しない場合には何もしない
        if (!_applyToSceneView && renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            return;
        }

        if (!_volume.IsActive())
        {
            return;
        }

        _cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;

        // renderPassEventがAfterRenderingの場合、カメラのカラーターゲットではなく_AfterPostProcessTextureを使う
        var source = renderPassEvent == RenderPassEvent.AfterRendering && renderingData.cameraData.resolveFinalTarget
            ? _afterPostProcessTexture.Identifier()
            : _cameraColorTarget;

        // コマンドバッファを作成
        var cmd = CommandBufferPool.Get(RenderPassName);
        cmd.Clear();

        // Cameraのターゲットと同じDescription（Depthは無し）のRenderTextureを取得する
        var tempTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        tempTargetDescriptor.depthBufferBits = 0;
        cmd.GetTemporaryRT(_tempRenderTargetHandle.id, tempTargetDescriptor);

        using (new ProfilingScope(cmd, _profilingSampler))
        {
            // VolumeからTintColorを取得して反映
            _material.SetFloat(_tintColorPropertyId, _volume.dotFactor.value);
            cmd.SetGlobalTexture(_mainTexPropertyId, source);

            // 元のテクスチャから一時的なテクスチャにエフェクトを適用しつつ描画
            Blit(cmd, source, _tempRenderTargetHandle.Identifier(), _material);
        }

        // 一時的なテクスチャから元のテクスチャに結果を書き戻す
        Blit(cmd, _tempRenderTargetHandle.Identifier(), source);

        // 一時的なRenderTextureを解放する
        cmd.ReleaseTemporaryRT(_tempRenderTargetHandle.id);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    private static RenderPassEvent GetRenderPassEvent(PostprocessTiming postprocessTiming)
    {
        switch (postprocessTiming)
        {
            case PostprocessTiming.AfterOpaque:
                return RenderPassEvent.AfterRenderingSkybox;
            case PostprocessTiming.BeforePostprocess:
                return RenderPassEvent.BeforeRenderingPostProcessing;
            case PostprocessTiming.AfterPostprocess:
                return RenderPassEvent.AfterRendering;
            default:
                throw new ArgumentOutOfRangeException(nameof(postprocessTiming), postprocessTiming, null);
        }
    }
}