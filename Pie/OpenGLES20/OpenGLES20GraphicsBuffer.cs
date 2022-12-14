using System;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGLES;
using static Pie.OpenGLES20.OpenGLES20GraphicsDevice;

namespace Pie.OpenGLES20;

internal sealed class OpenGLES20GraphicsBuffer : GraphicsBuffer
{
    public override bool IsDisposed { get; protected set; }

    public readonly uint Handle;
    public readonly BufferTargetARB Target;

    public OpenGLES20GraphicsBuffer(uint handle, BufferTargetARB target)
    {
        Handle = handle;
        Target = target;
    }

    public static unsafe GraphicsBuffer CreateBuffer<T>(BufferType type, uint sizeInBytes, T[] data, bool dynamic) where T : unmanaged
    {
        BufferTargetARB target;

        switch (type)
        {
            case BufferType.VertexBuffer:
                target = BufferTargetARB.ArrayBuffer;
                PieMetrics.VertexBufferCount++;
                break;
            case BufferType.IndexBuffer:
                target = BufferTargetARB.ElementArrayBuffer;
                PieMetrics.IndexBufferCount++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        BufferUsageARB usage = dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StaticDraw;

        uint handle = Gl.GenBuffer();
        Gl.BindBuffer(target, handle);
        if (data == null)
            Gl.BufferData(target, sizeInBytes, null, usage);
        else
        {
            fixed (void* d = data)
                Gl.BufferData(target, sizeInBytes, d, usage);
        }

        return new OpenGLES20GraphicsBuffer(handle, target);
    }

    public unsafe void Update<T>(uint offsetInBytes, T[] data) where T : unmanaged
    {
        Gl.BindBuffer(Target, Handle);
        fixed (void* d = data)
            Gl.BufferSubData(Target, (nint) offsetInBytes, (nuint) (data.Length * Unsafe.SizeOf<T>()), d);
    }

    public unsafe void Update<T>(uint offsetInBytes, T data) where T : unmanaged
    {
        Gl.BindBuffer(Target, Handle);
        Gl.BufferSubData(Target, (nint) offsetInBytes, (nuint) Unsafe.SizeOf<T>(), data);
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        Gl.DeleteBuffer(Handle);
        switch (Target)
        {
            case BufferTargetARB.ArrayBuffer:
                PieMetrics.VertexBufferCount--;
                break;
            case BufferTargetARB.ElementArrayBuffer:
                PieMetrics.IndexBufferCount--;
                break;
        }
    }
}