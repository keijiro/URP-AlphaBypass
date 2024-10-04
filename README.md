# URP Alpha Bypass Feature Sample

<img width="1024" alt="Frame 1-2" src="https://github.com/user-attachments/assets/e8abdec9-d811-40e3-9401-cfd6add61083">

**URP-AlphaBypass** is a Unity URP sample project that prevents the alpha
channel in the color target from being overwritten by the post-processing pass.

in URP, the post-processing pass typically clears the alpha channel (setting
alpha = 1), which can be problematic if you're rendering to a render texture
and need to preserve the alpha channel for compositing.

Starting from Unity 6, the Alpha Processing option was introduced in URP
settings. However, this modifies how post-processing effects are applied. For
instance, when Bloom and Tonemapping are used together, Tonemapping only affects
areas with alpha > 0 (as shown in the figure above, where the bloom becomes
overly intense due to the absence of Tonemapping in certain areas).

The Alpha Bypass feature solved this issue by storing the alpha channel before
post-processing and restoring it afterward. This allows you to apply
post-processing effects to the entire frame while preserving the original alpha
channel values.
