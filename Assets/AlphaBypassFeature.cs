using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

sealed class SaveAlphaPass : ScriptableRenderPass
{
    Material _material;

    public TextureHandle Buffer { get; set; }

    public SaveAlphaPass(Material material)
    {
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        _material = material;
    }

    public override void RecordRenderGraph(RenderGraph graph,
                                           ContextContainer context)
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
        Buffer = graph.CreateTexture(desc);

        // Blit
        var param = new RenderGraphUtils.BlitMaterialParameters(source, Buffer, _material, 0);
        graph.AddBlitPass(param, passName: "Save Alpha");
    }
}

sealed class LoadAlphaPass : ScriptableRenderPass
{
    Material _material;
    SaveAlphaPass _saver;

    public LoadAlphaPass(Material material, SaveAlphaPass saver)
    {
        renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        _material = material;
        _saver = saver;
    }

    public override void RecordRenderGraph(RenderGraph graph,
                                           ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // Blit
        var param = new RenderGraphUtils.BlitMaterialParameters
          (_saver.Buffer, resource.activeColorTexture, _material, 1);
        graph.AddBlitPass(param, passName: "Load Alpha");
    }
}

public sealed class AlphaBypassFeature : ScriptableRendererFeature
{
    [SerializeField] Shader _shader = null;

    Material _material;
    (SaveAlphaPass save, LoadAlphaPass load) _passes;

    public override void Create()
    {
        _material = CoreUtils.CreateEngineMaterial(_shader);
        _passes.save = new SaveAlphaPass(_material);
        _passes.load = new LoadAlphaPass(_material, _passes.save);
    }

    protected override void Dispose(bool disposing)
      => CoreUtils.Destroy(_material);

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                         ref RenderingData data)
    {
        if (data.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_passes.save);
        renderer.EnqueuePass(_passes.load);
    }
}
