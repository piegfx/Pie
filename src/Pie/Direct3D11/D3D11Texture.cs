﻿using System;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Pie.Direct3D11.D3D11GraphicsDevice;
using Size = System.Drawing.Size;

namespace Pie.Direct3D11;

internal sealed class D3D11Texture : Texture
{
    public ID3D11Resource Texture;
    public ID3D11ShaderResourceView View;

    public override bool IsDisposed { get; protected set; }
    
    public override TextureDescription Description { get; set; }

    public D3D11Texture(ID3D11Resource texture, ID3D11ShaderResourceView view, TextureDescription description)
    {
        Texture = texture;
        View = view;
        Description = description;
    }

    public static unsafe Texture CreateTexture(TextureDescription description, void* data)
    {
        int pitch = PieUtils.CalculatePitch(description.Format, description.Width, out int bpp);

        Vortice.DXGI.Format fmt =
            description.Format.ToDxgiFormat((description.Usage & TextureUsage.ShaderResource) ==
                                            TextureUsage.ShaderResource);
        
        BindFlags flags = BindFlags.None;
        if ((description.Usage & TextureUsage.ShaderResource) == TextureUsage.ShaderResource)
            flags |= BindFlags.ShaderResource;

        if (description.Format is Format.D24_UNorm_S8_UInt or Format.D32_Float or Format.D16_UNorm)
            flags |= BindFlags.DepthStencil;
        else if ((description.Usage & TextureUsage.Framebuffer) == TextureUsage.Framebuffer || description.MipLevels == 0)
            flags |= BindFlags.RenderTarget;

        Vortice.DXGI.Format svFmt = description.Format.ToDxgiFormat(false);

        svFmt = svFmt switch
        {
            Vortice.DXGI.Format.D32_Float => Vortice.DXGI.Format.R32_Float,
            Vortice.DXGI.Format.D16_UNorm => Vortice.DXGI.Format.R16_UNorm,
            Vortice.DXGI.Format.D24_UNorm_S8_UInt => Vortice.DXGI.Format.R24_UNorm_X8_Typeless,
            _ => svFmt
        };
        
        ID3D11Resource texture;
        ShaderResourceViewDescription svDesc = new ShaderResourceViewDescription()
        {
            Format = svFmt,
        };

        switch (description.TextureType)
        {
            case TextureType.Texture1D:
                throw new NotImplementedException();
                break;
            case TextureType.Texture2D:
                int mipLevels = description.MipLevels == 0
                    ? PieUtils.CalculateMipLevels(description.Width, description.Height)
                    : description.MipLevels;

                Texture2DDescription desc2d = new Texture2DDescription()
                {
                    Width = description.Width,
                    Height = description.Height,
                    Format = fmt,
                    MipLevels = mipLevels,
                    ArraySize = description.ArraySize,
                    SampleDescription = new SampleDescription(1, 0),
                    //Usage = description.Dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
                    Usage = ResourceUsage.Default,
                    BindFlags = flags,
                    CPUAccessFlags = CpuAccessFlags.None,
                    MiscFlags = description.MipLevels == 0 ? ResourceOptionFlags.GenerateMips : ResourceOptionFlags.None
                };

                texture = Device.CreateTexture2D(desc2d);

                if (data != null)
                {
                    if (description.MipLevels <= 1)
                        Context.UpdateSubresource(texture, 0, null, (IntPtr) data, pitch, 0);
                    else
                    {
                        int totalOffset = 0;

                        int width = description.Width;
                        int height = description.Height;
                        
                        for (int i = 0; i < mipLevels; i++)
                        {
                            int newPitch = PieUtils.CalculatePitch(description.Format, width, out _);
                            
                            Context.UpdateSubresource(texture, D3D11.CalculateSubResourceIndex(i, 0, mipLevels), null,
                                (IntPtr) ((byte*) data + totalOffset), newPitch, 0);

                            totalOffset += (int) (width * height * (bpp / 8f));
                            
                            width /= 2;
                            height /= 2;

                            if (width < 1)
                                width = 1;
                            if (height < 1)
                                height = 1;
                        }
                    }
                }
                
                if (description.ArraySize == 1)
                {
                    svDesc.ViewDimension = ShaderResourceViewDimension.Texture2D;
                    svDesc.Texture2D = new Texture2DShaderResourceView()
                    {
                        MipLevels = -1,
                        MostDetailedMip = 0
                    };
                }
                else
                {
                    svDesc.ViewDimension = ShaderResourceViewDimension.Texture2DArray;
                    svDesc.Texture2DArray = new Texture2DArrayShaderResourceView()
                    {
                        MipLevels = -1,
                        MostDetailedMip = 0,
                        ArraySize = description.ArraySize,
                        FirstArraySlice = 0
                    };
                }
                
                break;
            case TextureType.Texture3D:
                throw new NotImplementedException();
                break;
            case TextureType.Cubemap:
                throw new NotImplementedException();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ID3D11ShaderResourceView view = null;

        if ((description.Usage & TextureUsage.ShaderResource) == TextureUsage.ShaderResource)
          view = Device.CreateShaderResourceView(texture, svDesc);

        // TODO: Clean up D3D texture bits
        
        return new D3D11Texture(texture, view, description);
    }

    public unsafe void Update(int x, int y, int z, int width, int height, int depth, int mipLevel, int arrayIndex, void* data)
    {
        int subresource = D3D11.CalculateSubResourceIndex(mipLevel, arrayIndex,
            Description.MipLevels == 0
                ? PieUtils.CalculateMipLevels(Description.Width, Description.Height)
                : Description.MipLevels);

        // TODO: Figure out depth pitch correctly.
        // TODO: Make sure this works properly as well.
        // TODO: This does not work properly - It probably needs something similar to the OpenGL texture function.
        // TODO: front and back of the box have been reverted to 0 and 1 as this broke texture updating on D3D. FIX THIS!!!
        Context.UpdateSubresource(Texture, subresource, new Box(x, y, z, x + width, y + height, z + depth + 1),
            new IntPtr(data), PieUtils.CalculatePitch(Description.Format, width, out _), 0);
    }

    public override void Dispose()
    {
        Texture.Dispose();
        View.Dispose();
    }
}