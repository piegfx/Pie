using System;
using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop.DirectX;
using static TerraFX.Interop.DirectX.D3D11_CULL_MODE;
using static TerraFX.Interop.DirectX.D3D11_FILL_MODE;
using static Pie.Direct3D11.DxUtils;

namespace Pie.Direct3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11RasterizerState : RasterizerState
{
    public readonly ID3D11RasterizerState* State;
    
    public override bool IsDisposed { get; protected set; }

    public override RasterizerStateDescription Description { get; }

    public D3D11RasterizerState(ID3D11Device* device, RasterizerStateDescription description)
    {
        Description = description;

       D3D11_CULL_MODE cullMode = description.CullFace switch
        {
            CullFace.None => D3D11_CULL_NONE,
            CullFace.Front => D3D11_CULL_FRONT,
            CullFace.Back => D3D11_CULL_BACK,
            _ => throw new ArgumentOutOfRangeException()
        };

        D3D11_FILL_MODE fm = description.FillMode switch
        {
            FillMode.Solid => D3D11_FILL_SOLID,
            FillMode.Wireframe => D3D11_FILL_WIREFRAME,
            _ => throw new ArgumentOutOfRangeException()
        };

        D3D11_RASTERIZER_DESC desc = new()
        {
            CullMode = cullMode,
            FrontCounterClockwise = description.CullDirection == CullDirection.CounterClockwise,
            FillMode = fm,
            ScissorEnable = description.ScissorTest
        };

        ID3D11RasterizerState* state;
        if (Failed(device->CreateRasterizerState(&desc, &state)))
            throw new PieException("Failed to create rasterizer state.");

        State = state;
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        State->Release();
    }
}