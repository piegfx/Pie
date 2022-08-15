﻿using System;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Pie.Direct3D11.D3D11GraphicsDevice;
using Size = System.Drawing.Size;

namespace Pie.Direct3D11;

internal class D3D11Texture : Texture
{
    private ID3D11Texture2D _texture;
    public ID3D11ShaderResourceView View;
    public ID3D11SamplerState SamplerState;
    
    public override bool IsDisposed { get; protected set; }
    
    public override Size Size { get; set; }

    public D3D11Texture(ID3D11Texture2D texture, ID3D11ShaderResourceView view, ID3D11SamplerState samplerState, Size size)
    {
        _texture = texture;
        View = view;
        SamplerState = samplerState;
        Size = size;
    }

    public static unsafe Texture CreateTexture<T>(int width, int height, PixelFormat format, T[] data,
        TextureSample sample = TextureSample.Linear, bool mipmap = true) where T : unmanaged
    {
        // ???
        Format fmt = format switch
        {
            PixelFormat.RGB8 => Format.R8G8B8A8_UNorm,
            PixelFormat.RGBA8 => Format.R8G8B8A8_UNorm,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        Texture2DDescription description = new Texture2DDescription()
        {
            Width = width,
            Height = height,
            Format = fmt,
            MipLevels = 0,
            ArraySize = 1,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
            CPUAccessFlags = CpuAccessFlags.None,
            MiscFlags = ResourceOptionFlags.GenerateMips
        };

        ID3D11Texture2D texture = Device.CreateTexture2D(description);
        Context.UpdateSubresource(data, texture, 0, width * 4 * sizeof(byte));

        ShaderResourceViewDescription svDesc = new ShaderResourceViewDescription()
        {
            Format = description.Format,
            ViewDimension = ShaderResourceViewDimension.Texture2D,
            Texture2D = new Texture2DShaderResourceView()
            {
                MipLevels = -1,
                MostDetailedMip = 0
            }
        };
        ID3D11ShaderResourceView view = Device.CreateShaderResourceView(texture, svDesc);
        Context.GenerateMips(view);
        
        SamplerDescription samplerDescription = new SamplerDescription()
        {
            Filter = Filter.MinMagMipLinear,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            MipLODBias = 0,
            MaxAnisotropy = 1,
            ComparisonFunction = ComparisonFunction.Always,
            BorderColor = new Color4(0, 0, 0, 0),
            MinLOD = 0,
            MaxLOD = float.MaxValue
        };
        ID3D11SamplerState samplerState = Device.CreateSamplerState(samplerDescription);
        
        return new D3D11Texture(texture, view, samplerState, new Size(width, height));
    }
    
    public override void Update<T>(int x, int y, uint width, uint height, T[] data)
    {
        throw new System.NotImplementedException();
    }

    public override void Dispose()
    {
        _texture.Dispose();
        View.Dispose();
        SamplerState.Dispose();
    }
}