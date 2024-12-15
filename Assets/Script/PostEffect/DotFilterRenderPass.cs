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

                // RenderPassEvent.AfterRendering�ł̓|�X�g�G�t�F�N�g���|������̃J���[�e�N�X�`�������̖��O�Ŏ擾�ł���
        _afterPostProcessTexture.Init("_AfterPostProcessTexture");
    }

    public void Setup(RenderTargetIdentifier cameraColorTarget)
    {
        _cameraColorTarget = cameraColorTarget;


    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {

        // Volume�R���|�[�l���g���擾
        var volumeStack = VolumeManager.instance.stack;
        _volume = volumeStack.GetComponent<DotFilterPostEffect>();

        if (_material == null)
        {
            return;
        }

        // �J�����̃|�X�g�v���Z�X�ݒ肪�����ɂȂ��Ă����牽�����Ȃ�
        if (!renderingData.cameraData.postProcessEnabled)
        {
            return;
        }

        // �J�������V�[���r���[�J�������V�[���r���[�ɓK�p���Ȃ��ꍇ�ɂ͉������Ȃ�
        if (!_applyToSceneView && renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            return;
        }

        if (!_volume.IsActive())
        {
            return;
        }

        _cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;

        // renderPassEvent��AfterRendering�̏ꍇ�A�J�����̃J���[�^�[�Q�b�g�ł͂Ȃ�_AfterPostProcessTexture���g��
        var source = renderPassEvent == RenderPassEvent.AfterRendering && renderingData.cameraData.resolveFinalTarget
            ? _afterPostProcessTexture.Identifier()
            : _cameraColorTarget;

        // �R�}���h�o�b�t�@���쐬
        var cmd = CommandBufferPool.Get(RenderPassName);
        cmd.Clear();

        // Camera�̃^�[�Q�b�g�Ɠ���Description�iDepth�͖����j��RenderTexture���擾����
        var tempTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        tempTargetDescriptor.depthBufferBits = 0;
        cmd.GetTemporaryRT(_tempRenderTargetHandle.id, tempTargetDescriptor);

        using (new ProfilingScope(cmd, _profilingSampler))
        {
            // Volume����TintColor���擾���Ĕ��f
            _material.SetFloat(_tintColorPropertyId, _volume.dotFactor.value);
            cmd.SetGlobalTexture(_mainTexPropertyId, source);

            // ���̃e�N�X�`������ꎞ�I�ȃe�N�X�`���ɃG�t�F�N�g��K�p���`��
            Blit(cmd, source, _tempRenderTargetHandle.Identifier(), _material);
        }

        // �ꎞ�I�ȃe�N�X�`�����猳�̃e�N�X�`���Ɍ��ʂ������߂�
        Blit(cmd, _tempRenderTargetHandle.Identifier(), source);

        // �ꎞ�I��RenderTexture���������
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