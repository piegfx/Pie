﻿using System;
using System.Drawing;
using System.Numerics;
using Pie.OpenGL33;
using Silk.NET.Core.Contexts;

namespace Pie;

public abstract class GraphicsDevice : IDisposable
{
    public abstract RasterizerState RasterizerState { get; set; }
    
    public abstract DepthMode DepthMode { get; set; }
    
    public abstract Rectangle Viewport { get; set; }
    
    /// <summary>
    /// Clears the set Framebuffer with the given color and flags. If no framebuffer is set, this clears the back buffer.
    /// </summary>
    /// <param name="color">The color to clear with.</param>
    /// <param name="flags">The flags for clearing other bits.</param>
    public abstract void Clear(Color color, ClearFlags flags = ClearFlags.None);

    /// <summary>
    /// Clears the set Framebuffer with the given normalized color and flags. If no framebuffer is set, this clears the
    /// back buffer.
    /// </summary>
    /// <param name="color">The color to clear with.</param>
    /// <param name="flags">The flags for clearing other bits.</param>
    public abstract void Clear(Vector4 color, ClearFlags flags = ClearFlags.None);

    /// <summary>
    /// Clears the set Framebuffer with the given flags. If no framebuffer is set, this clears the back buffer.
    /// </summary>
    /// <param name="flags">The flags for clearing bits.</param>
    public abstract void Clear(ClearFlags flags);

    public abstract GraphicsBuffer CreateBuffer(BufferType bufferType, uint sizeInBytes, bool dynamic = false);

    public abstract Texture CreateTexture(uint width, uint height, PixelFormat format, TextureSample sample = TextureSample.Linear, bool mipmap = true);

    public abstract Shader CreateShader(params ShaderAttachment[] attachments);

    public abstract InputLayout CreateInputLayout(params InputLayoutDescription[] descriptions);

    public abstract void SetShader(Shader shader);

    public abstract void SetTexture(uint slot, Texture texture);
    
    public abstract void SetVertexBuffer(GraphicsBuffer buffer, InputLayout layout);

    public abstract void SetIndexBuffer(GraphicsBuffer buffer);

    public abstract void Draw(uint elements);

    public abstract void Present();

    public abstract void ResizeMainFramebuffer(Size newSize);

    public abstract void Dispose();

    public static GraphicsDevice CreateOpenGL33(IGLContext context, bool debug)
    {
        return new OpenGL33GraphicsDevice(context, debug);
    }
}