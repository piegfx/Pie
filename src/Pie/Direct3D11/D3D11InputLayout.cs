using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using TerraFX.Interop.DirectX;
using static Pie.Direct3D11.DxUtils;

namespace Pie.Direct3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11InputLayout : InputLayout
{
    public readonly ID3D11InputLayout* Layout;

    public D3D11InputLayout(ID3D11Device* device, InputLayoutDescription[] descriptions)
    {
        uint numDescriptions = (uint) descriptions.Length;
        
        D3D11_INPUT_ELEMENT_DESC* iedesc = stackalloc D3D11_INPUT_ELEMENT_DESC[descriptions.Length];
        fixed (byte* pTexCoordStr = "TEXCOORD"u8)
        {
            for (uint i = 0; i < numDescriptions; i++)
            {
                ref D3D11_INPUT_ELEMENT_DESC d = ref iedesc[i];
                ref InputLayoutDescription desc = ref descriptions[i];

                DXGI_FORMAT fmt = desc.Format.ToDxgiFormat(false);

                d = new D3D11_INPUT_ELEMENT_DESC
                {
                    SemanticName = (sbyte*) pTexCoordStr,
                    SemanticIndex = i,
                    AlignedByteOffset = desc.Offset,
                    Format = fmt,
                    InputSlot = desc.Slot,
                    InputSlotClass = (D3D11_INPUT_CLASSIFICATION) desc.InputType,
                    InstanceDataStepRate = (uint) desc.InputType
                };
            }
        }

        Descriptions = descriptions;

        ID3DBlob* dummyBlob = GenerateDummyShader(descriptions);

        ID3D11InputLayout* layout;
        if (Failed(device->CreateInputLayout(iedesc, numDescriptions, dummyBlob->GetBufferPointer(),
                dummyBlob->GetBufferSize(), &layout)))
        {
            throw new PieException("Failed to create input layout.");
        }

        Layout = layout;
        
        dummyBlob->Release();
    }

    private ID3DBlob* GenerateDummyShader(InputLayoutDescription[] descriptions)
    {
        StringBuilder dummyShader = new StringBuilder();
        dummyShader.AppendLine("struct DummyInput {");
        for (int i = 0; i < descriptions.Length; i++)
        {
            ref InputLayoutDescription desc = ref descriptions[i];

            switch (desc.Format)
            {
                case Format.R8_UNorm:
                    dummyShader.Append("float1 ");
                    break;
                case Format.R8G8_UNorm:
                    dummyShader.Append("float2 ");
                    break;
                case Format.R8G8B8A8_UNorm:
                    dummyShader.Append("float4 ");
                    break;
                case Format.B8G8R8A8_UNorm:
                    break;
                case Format.R16G16B16A16_UNorm:
                    dummyShader.Append("half4 ");
                    break;
                case Format.R16G16B16A16_SNorm:
                    dummyShader.Append("half4 ");
                    break;
                case Format.R16G16B16A16_SInt:
                    dummyShader.Append("int4 ");
                    break;
                case Format.R16G16B16A16_UInt:
                    dummyShader.Append("uint4 ");
                    break;
                case Format.R16G16B16A16_Float:
                    dummyShader.Append("half4 ");
                    break;
                case Format.R32G32_SInt:
                    dummyShader.Append("int2 ");
                    break;
                case Format.R32G32_UInt:
                    dummyShader.Append("uint2 ");
                    break;
                case Format.R32G32_Float:
                    dummyShader.Append("float2 ");
                    break;
                case Format.R32G32B32_SInt:
                    dummyShader.Append("int3 ");
                    break;
                case Format.R32G32B32_UInt:
                    dummyShader.Append("uint3 ");
                    break;
                case Format.R32G32B32_Float:
                    dummyShader.Append("float3 ");
                    break;
                case Format.R32G32B32A32_SInt:
                    dummyShader.Append("int4 ");
                    break;
                case Format.R32G32B32A32_UInt:
                    dummyShader.Append("uint4 ");
                    break;
                case Format.R32G32B32A32_Float:
                    dummyShader.Append("float4 ");
                    break;
                case Format.D24_UNorm_S8_UInt:
                    break;
                case Format.R8_SNorm:
                    dummyShader.Append("float1 ");
                    break;
                case Format.R8_SInt:
                    dummyShader.Append("int1 ");
                    break;
                case Format.R8_UInt:
                    dummyShader.Append("uint1 ");
                    break;
                case Format.R8G8_SNorm:
                    dummyShader.Append("float2 ");
                    break;
                case Format.R8G8_SInt:
                    dummyShader.Append("int2 ");
                    break;
                case Format.R8G8_UInt:
                    dummyShader.Append("uint2 ");
                    break;
                case Format.R8G8B8A8_SNorm:
                    dummyShader.Append("float4 ");
                    break;
                case Format.R8G8B8A8_SInt:
                    dummyShader.Append("int4 ");
                    break;
                case Format.R8G8B8A8_UInt:
                    dummyShader.Append("uint4 ");
                    break;
                case Format.R16_UNorm:
                    dummyShader.Append("half1 ");
                    break;
                case Format.R16_SNorm:
                    dummyShader.Append("half1 ");
                    break;
                case Format.R16_SInt:
                    dummyShader.Append("int1 ");
                    break;
                case Format.R16_UInt:
                    dummyShader.Append("uint1 ");
                    break;
                case Format.R16_Float:
                    dummyShader.Append("half1 ");
                    break;
                case Format.R16G16_UNorm:
                    dummyShader.Append("half2 ");
                    break;
                case Format.R16G16_SNorm:
                    dummyShader.Append("half2 ");
                    break;
                case Format.R16G16_SInt:
                    dummyShader.Append("int2 ");
                    break;
                case Format.R16G16_UInt:
                    dummyShader.Append("uint2 ");
                    break;
                case Format.R16G16_Float:
                    dummyShader.Append("half2 ");
                    break;
                case Format.R32_SInt:
                    dummyShader.Append("int1 ");
                    break;
                case Format.R32_UInt:
                    dummyShader.Append("uint1 ");
                    break;
                case Format.R32_Float:
                    dummyShader.Append("float1 ");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            dummyShader.AppendLine("var" + i + ": TEXCOORD" + i + ";");
        }

        dummyShader.AppendLine("}; void main(DummyInput input) {}");

        return D3D11Shader.CompileShader(Encoding.UTF8.GetBytes(dummyShader.ToString()), "main"u8, "vs_5_0"u8);
    }

    public override bool IsDisposed { get; protected set; }

    public override InputLayoutDescription[] Descriptions { get; }

    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        Layout->Release();
    }
}