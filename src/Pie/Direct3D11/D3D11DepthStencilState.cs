using System;
using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop.DirectX;
using static TerraFX.Interop.DirectX.D3D11_DEPTH_WRITE_MASK;
using static Pie.Direct3D11.DxUtils;
using static TerraFX.Interop.DirectX.D3D11_COMPARISON_FUNC;
using static TerraFX.Interop.DirectX.D3D11_STENCIL_OP;

namespace Pie.Direct3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11DepthStencilState : DepthStencilState
{
    public readonly ID3D11DepthStencilState* State;
    
    public D3D11DepthStencilState(ID3D11Device* device, DepthStencilStateDescription description)
    {
        Description = description;

        D3D11_DEPTH_STENCIL_DESC desc = new();
        desc.DepthEnable = description.DepthEnabled;
        desc.DepthWriteMask = description.DepthMask ? D3D11_DEPTH_WRITE_MASK_ALL : D3D11_DEPTH_WRITE_MASK_ZERO;
        desc.DepthFunc = ComparisonFuncToFunction(description.DepthComparison);

        desc.StencilEnable = description.StencilEnabled;
        desc.StencilReadMask = description.StencilReadMask;
        desc.StencilWriteMask = description.StencilWriteMask;

        desc.FrontFace.StencilFunc = ComparisonFuncToFunction(description.StencilFrontFace.StencilFunc);
        desc.FrontFace.StencilFailOp = StencilOpToOperation(description.StencilFrontFace.StencilFailOp);
        desc.FrontFace.StencilPassOp = StencilOpToOperation(description.StencilFrontFace.DepthStencilPassOp);
        desc.FrontFace.StencilDepthFailOp = StencilOpToOperation(description.StencilFrontFace.DepthFailOp);
        
        desc.BackFace.StencilFunc = ComparisonFuncToFunction(description.StencilFrontFace.StencilFunc);
        desc.BackFace.StencilFailOp = StencilOpToOperation(description.StencilFrontFace.StencilFailOp);
        desc.BackFace.StencilPassOp = StencilOpToOperation(description.StencilFrontFace.DepthStencilPassOp);
        desc.BackFace.StencilDepthFailOp = StencilOpToOperation(description.StencilFrontFace.DepthFailOp);

        ID3D11DepthStencilState* state;
        if (Failed(device->CreateDepthStencilState(&desc, &state)))
            throw new PieException("Failed to create depth stencil state.");

        State = state;
    }

    public override bool IsDisposed { get; protected set; }
    
    public override DepthStencilStateDescription Description { get; }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        State->Release();
    }

    public D3D11_COMPARISON_FUNC ComparisonFuncToFunction(ComparisonFunc func)
    {
        return func switch
        {
            ComparisonFunc.Never => D3D11_COMPARISON_NEVER,
            ComparisonFunc.Less => D3D11_COMPARISON_LESS,
            ComparisonFunc.Equal => D3D11_COMPARISON_EQUAL,
            ComparisonFunc.LessEqual => D3D11_COMPARISON_LESS_EQUAL,
            ComparisonFunc.Greater => D3D11_COMPARISON_GREATER,
            ComparisonFunc.NotEqual => D3D11_COMPARISON_NOT_EQUAL,
            ComparisonFunc.GreaterEqual => D3D11_COMPARISON_GREATER_EQUAL,
            ComparisonFunc.Always => D3D11_COMPARISON_ALWAYS,
            _ => throw new ArgumentOutOfRangeException(nameof(func), func, null)
        };
    }
    
    private D3D11_STENCIL_OP StencilOpToOperation(StencilOp op)
    {
        return op switch
        {
            StencilOp.Keep => D3D11_STENCIL_OP_KEEP,
            StencilOp.Zero => D3D11_STENCIL_OP_ZERO,
            StencilOp.Replace => D3D11_STENCIL_OP_REPLACE,
            StencilOp.Increment => D3D11_STENCIL_OP_INCR_SAT,
            StencilOp.IncrementWrap => D3D11_STENCIL_OP_INCR,
            StencilOp.Decrement => D3D11_STENCIL_OP_DECR_SAT,
            StencilOp.DecrementWrap => D3D11_STENCIL_OP_DECR,
            StencilOp.Invert => D3D11_STENCIL_OP_INVERT,
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }
}