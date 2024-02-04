using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TerraFX.Interop.DirectX;
using static TerraFX.Interop.DirectX.D3D11_BIND_FLAG;
using static TerraFX.Interop.DirectX.D3D11_CPU_ACCESS_FLAG;
using static TerraFX.Interop.DirectX.D3D11_RESOURCE_MISC_FLAG;
using static TerraFX.Interop.DirectX.D3D11_USAGE;
using static Pie.Direct3D11.DxUtils;
using static TerraFX.Interop.DirectX.D3D11_MAP;

namespace Pie.Direct3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11GraphicsBuffer : GraphicsBuffer
{
    private readonly ID3D11DeviceContext* _context;
    private readonly BufferType _type;
    private readonly bool _dynamic;
    
    public override bool IsDisposed { get; protected set; }

    public readonly ID3D11Buffer* Buffer;

    public D3D11GraphicsBuffer(ID3D11Device* device, ID3D11DeviceContext* context, BufferType type, uint sizeInBytes, void* data, bool dynamic)
    {
        _context = context;
        
        _dynamic = dynamic;
        _type = type;

        D3D11_BIND_FLAG flags;

        switch (type)
        {
            case BufferType.VertexBuffer:
                flags = D3D11_BIND_VERTEX_BUFFER;
                PieMetrics.VertexBufferCount++;
                break;
            case BufferType.IndexBuffer:
                flags = D3D11_BIND_INDEX_BUFFER;
                PieMetrics.IndexBufferCount++;
                break;
            case BufferType.UniformBuffer:
                flags = D3D11_BIND_CONSTANT_BUFFER;
                PieMetrics.UniformBufferCount++;
                break;
            case BufferType.ShaderStorageBuffer:
                flags = D3D11_BIND_SHADER_RESOURCE;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        D3D11_BUFFER_DESC description = new()
        {
            BindFlags = (uint) flags,
            ByteWidth = sizeInBytes,
            Usage = dynamic ? D3D11_USAGE_DYNAMIC : D3D11_USAGE_DEFAULT,
            CPUAccessFlags = dynamic ? (uint) D3D11_CPU_ACCESS_WRITE : 0
        };

        if (type == BufferType.ShaderStorageBuffer)
        {
            description.MiscFlags = (uint) D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;
            description.StructureByteStride = 1;
        }

        D3D11_SUBRESOURCE_DATA subData = new()
        {
            pSysMem = data
        };

        //Buffer = device.CreateBuffer(description, data == null ? null : subData);
        ID3D11Buffer* buffer;
        if (Failed(device->CreateBuffer(&description, data == null ? null : &subData, &buffer)))
            throw new PieException("Failed to create buffer.");

        Buffer = buffer;
    }

    public void Update(uint offsetInBytes, uint sizeInBytes, void* data)
    {
        if (_dynamic)
        {
            D3D11_MAPPED_SUBRESOURCE subresource;
            if (Failed(_context->Map((ID3D11Resource*) Buffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &subresource)))
                throw new PieException("Failed to map buffer.");
            
            Unsafe.CopyBlock((byte*) subresource.pData + (int) offsetInBytes, data, sizeInBytes);
            _context->Unmap((ID3D11Resource*) Buffer, 0);
        }
        else
        {
            D3D11_BOX box = new D3D11_BOX((int) offsetInBytes, 0, 0, (int) (sizeInBytes + offsetInBytes), 1, 1);
            _context->UpdateSubresource((ID3D11Resource*) Buffer, 0, &box, data, 0, 0);
        }
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        Buffer->Release();
        switch (_type)
        {
            case BufferType.VertexBuffer:
                PieMetrics.VertexBufferCount--;
                break;
            case BufferType.IndexBuffer:
                PieMetrics.IndexBufferCount--;
                break;
            case BufferType.UniformBuffer:
                PieMetrics.UniformBufferCount--;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal override MappedSubresource Map(MapMode mode)
    {
        D3D11_MAPPED_SUBRESOURCE subresource;
        if (Failed(_context->Map((ID3D11Resource*) Buffer, 0, mode.ToDx11MapMode(), 0, &subresource)))
            throw new PieException("Failed to map buffer.");
        
        return new MappedSubresource((IntPtr) subresource.pData);
    }

    internal override void Unmap()
    {
        _context->Unmap((ID3D11Resource*) Buffer, 0);
    }
}