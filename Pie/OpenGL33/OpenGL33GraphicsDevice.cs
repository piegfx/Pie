using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;

namespace Pie.OpenGL33;

internal sealed class OpenGL33GraphicsDevice : GraphicsDevice
{
    private IGLContext _context;
    internal static GL Gl;
    internal static bool Debug;
    
    // The poor, lone vao that powers the entire GL graphics device.
    private uint _vao;

    // TODO: Implement these same optimizations for D3D
    private InputLayout _currentLayout;
    private RasterizerState _currentRState;
    private BlendState _currentBState;
    private DepthState _currentDState;
    private Silk.NET.OpenGL.PrimitiveType _glType;
    private PrimitiveType _currentPType;
    private DrawElementsType _currentEType;
    private int _eTypeSize;
    private bool _primitiveTypeInitialized;
    private int _boundTexture = -1;
    private int _bindingSlot = -1;
    
    public unsafe OpenGL33GraphicsDevice(IGLContext context, Size winSize, GraphicsDeviceOptions options)
    {
        _context = context;
        Gl = GL.GetApi(context);
        _vao = Gl.GenVertexArray();
        Gl.BindVertexArray(_vao);
        Debug = options.Debug;

        Viewport = new Rectangle(Point.Empty, winSize);
        _renderSize = winSize;

        if (Debug)
        {
            Logging.Log(LogType.Info, "Vendor info: " + Gl.GetStringS(StringName.Vendor));
            Logging.Log(LogType.Info, "Version info: " + Gl.GetStringS(StringName.Version));
            Logging.Log(LogType.Info, "GLSL Version: " + Gl.GetStringS(StringName.ShadingLanguageVersion));
            Logging.Log(LogType.Info, "Renderer: " + Gl.GetStringS(StringName.Renderer));
            Logging.Log(LogType.Debug, "Howdy! Thanks for using pie! Be sure to create an issue if you find any bugs.");
            
            Gl.Enable(EnableCap.DebugOutput);
            Gl.Enable(EnableCap.DebugOutputSynchronous);
            Gl.DebugMessageCallback(DebugCallback, null);
        }
    }
    
    public override GraphicsApi Api => GraphicsApi.OpenGl33;

    private Size _renderSize;
    private Rectangle _viewport;
    public override Rectangle Viewport
    {
        get => _viewport;
        set
        {
            Gl.Viewport(value.X, _renderSize.Height - value.Height - value.Y, (uint) value.Width, (uint) value.Height);
            _viewport = value;
        }
    }

    private Rectangle _scissor;

    public override Rectangle Scissor
    {
        get => _scissor;
        set
        {
            Gl.Scissor(value.X, _viewport.Height - value.Y - value.Height, (uint) value.Width, (uint) value.Height);
            _scissor = value;
        }
    }

    public override void Clear(Color color, ClearFlags flags = ClearFlags.None)
    {
        Vector4 nC = color.Normalize();
        Clear(nC, flags);
    }

    public override void Clear(Vector4 color, ClearFlags flags = ClearFlags.None)
    {
        InvalidateCaches();
        
        Gl.ClearColor(color.X, color.Y, color.Z, color.W);

        uint mask = (uint) ClearBufferMask.ColorBufferBit;
        if ((flags & ClearFlags.Depth) == ClearFlags.Depth)
            mask |= (uint) ClearBufferMask.DepthBufferBit;
        if ((flags & ClearFlags.Stencil) == ClearFlags.Stencil)
            mask |= (uint) ClearBufferMask.StencilBufferBit;
        Gl.Clear(mask);
    }

    public override void Clear(ClearFlags flags)
    {
        InvalidateCaches();
        uint mask = 0;
        if ((flags & ClearFlags.Depth) == ClearFlags.Depth)
            mask |= (uint) ClearBufferMask.DepthBufferBit;
        if ((flags & ClearFlags.Stencil) == ClearFlags.Stencil)
            mask |= (uint) ClearBufferMask.StencilBufferBit;
        Gl.Clear(mask);
    }

    private void InvalidateCaches()
    {
        _currentLayout = null;
        _currentRState = null;
        _currentBState = null;
        PieMetrics.DrawCalls = 0;
        PieMetrics.TriCount = 0;
    }

    public override unsafe GraphicsBuffer CreateBuffer<T>(BufferType bufferType, T[] data, bool dynamic = false)
    {
        fixed (void* dat = data)
            return OpenGL33GraphicsBuffer.CreateBuffer(bufferType, (uint) (data.Length * Unsafe.SizeOf<T>()), dat, dynamic);
    }

    public override unsafe GraphicsBuffer CreateBuffer<T>(BufferType bufferType, T data, bool dynamic = false)
    {
        fixed (void* dat = new T[] { data })
            return OpenGL33GraphicsBuffer.CreateBuffer(bufferType, (uint) Unsafe.SizeOf<T>(), dat, dynamic);
    }

    public override unsafe GraphicsBuffer CreateBuffer(BufferType bufferType, uint sizeInBytes, bool dynamic = false)
    {
        return OpenGL33GraphicsBuffer.CreateBuffer(bufferType, sizeInBytes, null, dynamic);
    }

    public override unsafe GraphicsBuffer CreateBuffer(BufferType bufferType, uint sizeInBytes, IntPtr data, bool dynamic = false)
    {
        return OpenGL33GraphicsBuffer.CreateBuffer(bufferType, sizeInBytes, data.ToPointer(), dynamic);
    }

    public override unsafe GraphicsBuffer CreateBuffer(BufferType bufferType, uint sizeInBytes, void* data, bool dynamic = false)
    {
        return OpenGL33GraphicsBuffer.CreateBuffer(bufferType, sizeInBytes, data, dynamic);
    }

    public override unsafe Texture CreateTexture(TextureDescription description)
    {
        return OpenGL33Texture.CreateTexture(description, null);
    }

    public override unsafe Texture CreateTexture<T>(TextureDescription description, T[] data)
    {
        fixed (void* ptr = data)
            return OpenGL33Texture.CreateTexture(description, ptr);
    }

    public override unsafe Texture CreateTexture<T>(TextureDescription description, T[][] data)
    {
        fixed (void* ptr = PieUtils.Combine(data))
            return OpenGL33Texture.CreateTexture(description, ptr);
    }

    public override unsafe Texture CreateTexture(TextureDescription description, IntPtr data)
    {
        return OpenGL33Texture.CreateTexture(description, data.ToPointer());
    }

    public override unsafe Texture CreateTexture(TextureDescription description, void* data)
    {
        return OpenGL33Texture.CreateTexture(description, data);
    }


    public override Shader CreateShader(params ShaderAttachment[] attachments)
    {
        return new OpenGL33Shader(attachments);
    }

    public override InputLayout CreateInputLayout(params InputLayoutDescription[] descriptions)
    {
        return new OpenGL33InputLayout(descriptions);
    }

    public override RasterizerState CreateRasterizerState(RasterizerStateDescription description)
    {
        return new OpenGL33RasterizerState(description);
    }

    public override BlendState CreateBlendState(BlendStateDescription description)
    {
        return new OpenGL33BlendState(description);
    }

    public override DepthState CreateDepthState(DepthStateDescription description)
    {
        return new OpenGL33DepthState(description);
    }

    public override SamplerState CreateSamplerState(SamplerStateDescription description)
    {
        return new OpenGL33SamplerState(description);
    }

    public override Framebuffer CreateFramebuffer(params FramebufferAttachment[] attachments)
    {
        return new OpenGL33Framebuffer(attachments);
    }

    public override unsafe void UpdateBuffer<T>(GraphicsBuffer buffer, uint offsetInBytes, T[] data)
    {
        fixed (void* dat = data)
            ((OpenGL33GraphicsBuffer) buffer).Update(offsetInBytes, (uint) (data.Length * Unsafe.SizeOf<T>()), dat);
    }

    public override unsafe void UpdateBuffer<T>(GraphicsBuffer buffer, uint offsetInBytes, T data)
    {
        fixed (void* dat = new T[] { data })
            ((OpenGL33GraphicsBuffer) buffer).Update(offsetInBytes, (uint) Unsafe.SizeOf<T>(), dat);
    }

    public override unsafe void UpdateTexture<T>(Texture texture, int x, int y, uint width, uint height, T[] data)
    {
        fixed (void* ptr = data)
            ((OpenGL33Texture) texture).Update(x, y, width, height, ptr);
    }

    public override void UpdateTexture(Texture texture, int x, int y, uint width, uint height, IntPtr data)
    {
        throw new NotImplementedException();
    }

    public override unsafe void UpdateTexture(Texture texture, int x, int y, uint width, uint height, void* data)
    {
        throw new NotImplementedException();
    }

    public override unsafe IntPtr MapBuffer(GraphicsBuffer buffer, MapMode mode)
    {
        OpenGL33GraphicsBuffer glBuf = (OpenGL33GraphicsBuffer) buffer;
        Gl.BindBuffer(glBuf.Target, glBuf.Handle);
        return (IntPtr) Gl.MapBuffer(glBuf.Target, mode.ToGlMapMode());
    }

    public override void UnmapBuffer(GraphicsBuffer buffer)
    {
        Gl.UnmapBuffer(((OpenGL33GraphicsBuffer) buffer).Target);
    }

    public override void SetShader(Shader shader)
    {
        OpenGL33Shader glShader = (OpenGL33Shader) shader;
        if (glShader.Handle == OpenGL33Shader.BoundHandle)
            return;
        Gl.UseProgram(glShader.Handle);
        OpenGL33Shader.BoundHandle = glShader.Handle;
    }

    public override void SetTexture(uint bindingSlot, Texture texture, SamplerState state)
    {
        OpenGL33Texture glTex = (OpenGL33Texture) texture;
        //if (glTex.Handle == _boundTexture && bindingSlot == _bindingSlot)
        //    return;
        //_boundTexture = (int) glTex.Handle;
        //_bindingSlot = (int) bindingSlot;
        Gl.ActiveTexture(TextureUnit.Texture0 + (int) bindingSlot);
        Gl.BindTexture(glTex.Target, glTex.Handle);
        Gl.BindSampler(bindingSlot, ((OpenGL33SamplerState) state).Handle);
    }

    public override void SetRasterizerState(RasterizerState state)
    {
        if (_currentRState != null && _currentRState.Equals(state))
            return;
        _currentRState = state;
        ((OpenGL33RasterizerState) state).Set();
    }

    public override void SetBlendState(BlendState state)
    {
        if (_currentBState != null && _currentBState.Equals(state))
            return;
        _currentBState = state;
        ((OpenGL33BlendState) state).Set();
    }

    public override void SetDepthState(DepthState state)
    {
        if (_currentDState != null && _currentDState.Equals(state))
            return;
        _currentDState = state;
        ((OpenGL33DepthState) state).Set();
    }

    public override void SetPrimitiveType(PrimitiveType type)
    {
        if (_primitiveTypeInitialized && _currentPType == type)
            return;
        _primitiveTypeInitialized = true;
        _currentPType = type;
        _glType = type switch
        {
            PrimitiveType.TriangleList => Silk.NET.OpenGL.PrimitiveType.Triangles,
            PrimitiveType.TriangleStrip => Silk.NET.OpenGL.PrimitiveType.TriangleStrip,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public override void SetVertexBuffer(uint slot, GraphicsBuffer buffer, uint stride, InputLayout layout)
    {
        OpenGL33GraphicsBuffer glBuf = (OpenGL33GraphicsBuffer) buffer;
        if (glBuf.Target != BufferTargetARB.ArrayBuffer)
            throw new PieException("Given buffer is not a vertex buffer.");
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, glBuf.Handle); 
        //if (_currentLayout == null || !_currentLayout.Equals(layout))
        //{
            ((OpenGL33InputLayout) layout).Set(slot, stride);
        //    _currentLayout = layout;
        //}
    }

    public override void SetIndexBuffer(GraphicsBuffer buffer, IndexType type)
    {
        OpenGL33GraphicsBuffer glBuf = (OpenGL33GraphicsBuffer) buffer;
        if (glBuf.Target != BufferTargetARB.ElementArrayBuffer)
            throw new PieException("Given buffer is not an index buffer.");
        Gl.BindBuffer(GLEnum.ElementArrayBuffer, glBuf.Handle);

        switch (type)
        {
            case IndexType.UShort:
                _currentEType = DrawElementsType.UnsignedShort;
                _eTypeSize = 2;
                break;
            case IndexType.UInt:
                _currentEType = DrawElementsType.UnsignedInt;
                _eTypeSize = 4;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public override void SetUniformBuffer(uint bindingSlot, GraphicsBuffer buffer)
    {
        //Gl.UniformBlockBinding(OpenGL33Shader.BoundHandle, slot, slot);
        Gl.BindBufferBase(BufferTargetARB.UniformBuffer, bindingSlot, ((OpenGL33GraphicsBuffer) buffer).Handle);
    }

    public override unsafe void SetFramebuffer(Framebuffer framebuffer)
    {
        if (framebuffer == null)
        {
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //Gl.DrawBuffer(DrawBufferMode.Front);
            return;
        }

        OpenGL33Framebuffer fb = (OpenGL33Framebuffer) framebuffer;
        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, fb.Handle);
        fixed (GLEnum* e = fb.DrawBuffers)
            Gl.DrawBuffers((uint) fb.DrawBuffers.Length, e);
    }

    public override void Draw(uint vertexCount)
    {
        Gl.DrawArrays(_glType, 0, vertexCount);
        PieMetrics.DrawCalls++;
        PieMetrics.TriCount += vertexCount / 3;
    }

    public override void Draw(uint vertexCount, int startVertex)
    {
        Gl.DrawArrays(_glType, startVertex, vertexCount);
        PieMetrics.DrawCalls++;
        PieMetrics.TriCount += (ulong) (vertexCount - startVertex) / 3;
    }

    public override unsafe void DrawIndexed(uint indexCount)
    {
        Gl.DrawElements(_glType, indexCount, _currentEType, null);
        PieMetrics.DrawCalls++;
        PieMetrics.TriCount += indexCount / 3;
    }

    public override unsafe void DrawIndexed(uint indexCount, int startIndex)
    {
        Gl.DrawElements(_glType, indexCount, _currentEType, (void*) (startIndex * _eTypeSize));
        PieMetrics.DrawCalls++;
        PieMetrics.TriCount += (ulong) (indexCount - startIndex) / 3;
    }

    public override unsafe void DrawIndexed(uint indexCount, int startIndex, int baseVertex)
    {
        Gl.DrawElementsBaseVertex(_glType, indexCount, _currentEType, (void*) (startIndex * _eTypeSize), baseVertex);
        PieMetrics.DrawCalls++;
        PieMetrics.TriCount += (ulong) (indexCount - startIndex) / 3;
    }

    public override unsafe void DrawIndexedInstanced(uint indexCount, uint instanceCount)
    {
        Gl.DrawElementsInstanced(_glType, indexCount, _currentEType, null, instanceCount);
    }

    public override void Present(int swapInterval)
    {
        _context.SwapInterval(swapInterval);
        _context.SwapBuffers();
    }

    public override void ResizeSwapchain(Size newSize)
    {
        Viewport = new Rectangle(Point.Empty, newSize);
        _renderSize = newSize;
    }

    public override void GenerateMipmaps(Texture texture)
    {
        OpenGL33Texture tex = (OpenGL33Texture) texture;
        Gl.BindTexture(tex.Target, tex.Handle);
        Gl.GenerateMipmap(tex.Target);
    }

    public override void Dispose()
    {
        Gl.BindVertexArray(0);
        Gl.DeleteVertexArray(_vao);
    }

    private void DebugCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
    {
        string msg = Marshal.PtrToStringAnsi(message);
        DebugType debugType = (DebugType) type;
        LogType logType = debugType switch
        {
            DebugType.DontCare => LogType.Debug,
            DebugType.DebugTypeError => LogType.Critical,
            DebugType.DebugTypeDeprecatedBehavior => LogType.Error,
            DebugType.DebugTypeUndefinedBehavior => LogType.Critical,
            DebugType.DebugTypePortability => LogType.Warning,
            DebugType.DebugTypePerformance => LogType.Warning,
            DebugType.DebugTypeOther => LogType.Debug,
            DebugType.DebugTypeMarker => LogType.Debug,
            DebugType.DebugTypePushGroup => LogType.Debug,
            DebugType.DebugTypePopGroup => LogType.Debug,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        Logging.Log(logType, msg);
    }
}