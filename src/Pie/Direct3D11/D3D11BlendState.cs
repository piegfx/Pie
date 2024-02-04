using System;
using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop.DirectX;
using static TerraFX.Interop.DirectX.D3D11_BLEND;
using static TerraFX.Interop.DirectX.D3D11_BLEND_OP;
using static Pie.Direct3D11.DxUtils;

namespace Pie.Direct3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11BlendState : BlendState
{
    public override bool IsDisposed { get; protected set; }

    public readonly ID3D11BlendState* State;
    
    public D3D11BlendState(ID3D11Device* device, BlendStateDescription description)
    {
        Description = description;

        D3D11_BLEND_DESC desc = new();
        desc.IndependentBlendEnable = false;
        desc.AlphaToCoverageEnable = false;
        desc.RenderTarget[0] = new()
        {
            BlendEnable = description.Enabled,
            SrcBlend = GetBlendFromBlendType(description.Source),
            DestBlend = GetBlendFromBlendType(description.Destination),
            BlendOp = GetOpFromOp(description.BlendOperation),
            SrcBlendAlpha = GetBlendFromBlendType(description.SourceAlpha),
            DestBlendAlpha = GetBlendFromBlendType(description.DestinationAlpha),
            BlendOpAlpha = GetOpFromOp(description.AlphaBlendOperation),
            // TODO: This should be a safe operation, as the two enums are compatible. But it's going to be better to not cast, so that should probably be done at some point.
            RenderTargetWriteMask = (byte) description.ColorWriteMask
        };

        ID3D11BlendState* state;
        if (Failed(device->CreateBlendState(&desc, &state)))
            throw new PieException("Failed to create blend state.");
        
        State = state;
    }

    public override BlendStateDescription Description { get; }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        State->Release();
    }

    private static D3D11_BLEND GetBlendFromBlendType(BlendType type)
    {
        return type switch
        {
            BlendType.Zero => D3D11_BLEND_ZERO,
            BlendType.One => D3D11_BLEND_ONE,
            BlendType.SrcColor => D3D11_BLEND_SRC_COLOR,
            BlendType.OneMinusSrcColor => D3D11_BLEND_INV_SRC_COLOR,
            BlendType.DestColor => D3D11_BLEND_DEST_COLOR,
            BlendType.OneMinusDestColor => D3D11_BLEND_INV_DEST_COLOR,
            BlendType.SrcAlpha => D3D11_BLEND_SRC_ALPHA,
            BlendType.OneMinusSrcAlpha => D3D11_BLEND_INV_SRC_ALPHA,
            BlendType.DestAlpha => D3D11_BLEND_DEST_ALPHA,
            BlendType.OneMinusDestAlpha => D3D11_BLEND_INV_DEST_ALPHA,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static D3D11_BLEND_OP GetOpFromOp(BlendOperation operation)
    {
        return operation switch
        {
            BlendOperation.Add => D3D11_BLEND_OP_ADD,
            BlendOperation.Subtract => D3D11_BLEND_OP_SUBTRACT,
            BlendOperation.ReverseSubtract => D3D11_BLEND_OP_REV_SUBTRACT,
            BlendOperation.Min => D3D11_BLEND_OP_MIN,
            BlendOperation.Max => D3D11_BLEND_OP_MAX,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }
}