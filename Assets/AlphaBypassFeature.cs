using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

class AlphaData : ContextItem
{
    public TextureHandle Buffer { get; set; }
    public override void Reset() => Buffer = TextureHandle.nullHandle;
}

sealed class SaveAlphaPass : ScriptableRenderPass
{
    Material _material;

    public SaveAlphaPass(Material material) => _material = material;

    public override void RecordRenderGraph(RenderGraph graph, ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // Destination texture allocation
        var source = resource.activeColorTexture;
        var desc = graph.GetTextureDesc(source);
        desc.name = "Alpha Bypass";
        desc.colorFormat = GraphicsFormat.R8_UNorm;
        desc.clearBuffer = false;
        desc.depthBufferBits = 0;
        var buffer = graph.CreateTexture(desc);

        // Custom context data for transferring the texture
        var data = context.Create<AlphaData>();
        data.Buffer = buffer;

        // Blit
        var param = new RenderGraphUtils.
          BlitMaterialParameters(source, buffer, _material, 0);
        graph.AddBlitPass(param, passName: "Save Alpha");
    }
}

sealed class LoadAlphaPass : ScriptableRenderPass
{
    Material _material;

    public LoadAlphaPass(Material material) => _material = material;

    public override void RecordRenderGraph(RenderGraph graph, ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // Alpha texture from the custom context data
        var data = context.Get<AlphaData>();

        // Blit
        var param = new RenderGraphUtils.
          BlitMaterialParameters(data.Buffer, resource.activeColorTexture, _material, 1);
        graph.AddBlitPass(param, passName: "Load Alpha");
    }
}

public sealed class AlphaBypassFeature : ScriptableRendererFeature
{
    [SerializeField] Shader _shader = null;

    Material _material;
    SaveAlphaPass _savePass;
    LoadAlphaPass _loadPass;

    public override void Create()
    {
        _material = CoreUtils.CreateEngineMaterial(_shader);
        _savePass = new SaveAlphaPass(_material);
        _loadPass = new LoadAlphaPass(_material);
        _savePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        _loadPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    protected override void Dispose(bool disposing)
      => CoreUtils.Destroy(_material);

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                         ref RenderingData data)
    {
        if (data.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_savePass);
        renderer.EnqueuePass(_loadPass);
    }
}
