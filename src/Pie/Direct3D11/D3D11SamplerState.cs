using System;
using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop.DirectX;
using static TerraFX.Interop.DirectX.D3D11_COMPARISON_FUNC;
using static TerraFX.Interop.DirectX.D3D11_TEXTURE_ADDRESS_MODE;
using static Pie.Direct3D11.DxUtils;

namespace Pie.Direct3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11SamplerState : SamplerState
{
    public readonly ID3D11SamplerState* State;
    
    public D3D11SamplerState(ID3D11Device* device, SamplerStateDescription description)
    {
        Description = description;
        
        D3D11_FILTER filter = description.Filter switch
        {
            TextureFilter.Anisotropic => D3D11_FILTER.D3D11_FILTER_ANISOTROPIC,
            TextureFilter.MinMagMipPoint => D3D11_FILTER.D3D11_FILTER_MIN_MAG_MIP_POINT,
            TextureFilter.MinMagPointMipLinear => D3D11_FILTER.D3D11_FILTER_MIN_MAG_POINT_MIP_LINEAR,
            TextureFilter.MinPointMagLinearMipPoint => D3D11_FILTER.D3D11_FILTER_MIN_POINT_MAG_LINEAR_MIP_POINT,
            TextureFilter.MinPointMagMipLinear => D3D11_FILTER.D3D11_FILTER_MIN_POINT_MAG_MIP_LINEAR,
            TextureFilter.MinLinearMagMipPoint => D3D11_FILTER.D3D11_FILTER_MIN_LINEAR_MAG_MIP_POINT,
            TextureFilter.MinLinearMagPointMipLinear => D3D11_FILTER.D3D11_FILTER_MIN_LINEAR_MAG_POINT_MIP_LINEAR,
            TextureFilter.MinMagLinearMipPoint => D3D11_FILTER.D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT,
            TextureFilter.MinMagMipLinear => D3D11_FILTER.D3D11_FILTER_MIN_MAG_MIP_LINEAR,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        D3D11_SAMPLER_DESC desc = new()
        {
            Filter = filter,
            AddressU = GetAddressModeFromTextureAddress(description.AddressU),
            AddressV = GetAddressModeFromTextureAddress(description.AddressV),
            AddressW = GetAddressModeFromTextureAddress(description.AddressW),
            MipLODBias = 0,
            MaxAnisotropy = (uint) description.MaxAnisotropy,
            ComparisonFunc = D3D11_COMPARISON_LESS_EQUAL,
            MinLOD = description.MinLOD,
            MaxLOD = description.MaxLOD
        };

        // TODO: ???????????????????????????
        desc.BorderColor[0] = description.BorderColor.R / 255f;
        desc.BorderColor[1] = description.BorderColor.G / 255f;
        desc.BorderColor[2] = description.BorderColor.B / 255f;
        desc.BorderColor[3] = description.BorderColor.A / 255f;

        ID3D11SamplerState* state;
        if (Failed(device->CreateSamplerState(&desc, &state)))
            throw new PieException("Failed to create sampler state.");

        State = state;
    }
    
    public override bool IsDisposed { get; protected set; }
    
    public override SamplerStateDescription Description { get; }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        State->Release();
    }

    private static D3D11_TEXTURE_ADDRESS_MODE GetAddressModeFromTextureAddress(TextureAddress address)
    {
        return address switch
        {
            TextureAddress.Repeat => D3D11_TEXTURE_ADDRESS_WRAP,
            TextureAddress.Mirror => D3D11_TEXTURE_ADDRESS_MIRROR,
            TextureAddress.ClampToEdge => D3D11_TEXTURE_ADDRESS_CLAMP,
            TextureAddress.ClampToBorder => D3D11_TEXTURE_ADDRESS_BORDER,
            _ => throw new ArgumentOutOfRangeException(nameof(address), address, null)
        };
    }
}