using System;
using System.Diagnostics.CodeAnalysis;
using Pie.ShaderCompiler;
using TerraFX.Interop.DirectX;
using static TerraFX.Interop.DirectX.DirectX;
using static Pie.Direct3D11.DxUtils;

namespace Pie.Direct3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal sealed unsafe class D3D11Shader : Shader
{
    private readonly ID3D11DeviceContext* _context;
    private readonly ShaderObject[] _shaders;
    
    public override bool IsDisposed { get; protected set; }

    public D3D11Shader(ID3D11Device* device, ID3D11DeviceContext* context, ShaderAttachment[] attachments, SpecializationConstant[] constants)
    {
        _context = context;
        _shaders = new ShaderObject[attachments.Length];
        
        for (int i = 0; i < attachments.Length; i++)
        {
            ref ShaderAttachment attachment = ref attachments[i];

            CompilerResult result = Compiler.FromSpirv(Language.HLSL, attachment.Stage, attachment.Spirv,
                attachment.EntryPoint, constants);
            if (!result.IsSuccess)
                throw new PieException(result.Error);
        
            byte[] hlsl = result.Result;

            switch (attachment.Stage)
            {
                case ShaderStage.Vertex:
                    ID3DBlob* vShaderBlob = CompileShader(hlsl, "main"u8, "vs_5_0"u8);
                    ID3D11VertexShader* vShader;

                    if (Failed(device->CreateVertexShader(vShaderBlob->GetBufferPointer(), vShaderBlob->GetBufferSize(),
                            null, &vShader)))
                    {
                        throw new PieException("Failed to create vertex shader.");
                    }
                    
                    _shaders[i] = new ShaderObject(ShaderStage.Vertex, (ID3D11DeviceChild*) vShader);
                    break;
                case ShaderStage.Fragment:
                    ID3DBlob* pShaderBlob = CompileShader(hlsl, "main"u8, "ps_5_0"u8);
                    ID3D11PixelShader* pShader;

                    if (Failed(device->CreatePixelShader(pShaderBlob->GetBufferPointer(), pShaderBlob->GetBufferSize(),
                            null, &pShader)))
                    {
                        throw new PieException("Failed to create pixel shader.");
                    }
                    
                    _shaders[i] = new ShaderObject(ShaderStage.Pixel, (ID3D11DeviceChild*) pShader);
                    break;
                case ShaderStage.Geometry:
                    ID3DBlob* gShaderBlob = CompileShader(hlsl, "main"u8, "gs_5_0"u8);
                    ID3D11GeometryShader* gShader;

                    if (Failed(device->CreateGeometryShader(gShaderBlob->GetBufferPointer(), gShaderBlob->GetBufferSize(),
                            null, &gShader)))
                    {
                        throw new PieException("Failed to create geometry shader.");
                    }
                    
                    _shaders[i] = new ShaderObject(ShaderStage.Geometry, (ID3D11DeviceChild*) gShader);
                    break;
                case ShaderStage.Compute:
                    ID3DBlob* cShaderBlob = CompileShader(hlsl, "main"u8, "cs_5_0"u8);
                    ID3D11ComputeShader* cShader;

                    if (Failed(device->CreateComputeShader(cShaderBlob->GetBufferPointer(), cShaderBlob->GetBufferSize(),
                            null, &cShader)))
                    {
                        throw new PieException("Failed to create compute shader.");
                    }
                    
                    _shaders[i] = new ShaderObject(ShaderStage.Compute, (ID3D11DeviceChild*) cShader);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        foreach (ShaderObject obj in _shaders)
            obj.Shader->Release();
    }

    public void Use()
    {
        foreach (ShaderObject obj in _shaders)
        {
            switch (obj.Stage)
            {
                case ShaderStage.Vertex:
                    _context->VSSetShader((ID3D11VertexShader*) obj.Shader, null, 0);
                    break;
                case ShaderStage.Fragment:
                    _context->PSSetShader((ID3D11PixelShader*) obj.Shader, null, 0);
                    break;
                case ShaderStage.Geometry:
                    _context->GSSetShader((ID3D11GeometryShader*) obj.Shader, null, 0);
                    break;
                case ShaderStage.Compute:
                    _context->CSSetShader((ID3D11ComputeShader*) obj.Shader, null, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal static ID3DBlob* CompileShader(byte[] code, ReadOnlySpan<byte> entryPoint, ReadOnlySpan<byte> profile)
    {
        /*fixed (byte* cPtr = code)
        {
            if (!Succeeded(D3DCompiler.Compile(cPtr, (nuint) code.Length, (byte*) null, null,
                    ref Unsafe.NullRef<ID3DInclude>(), entryPoint, profile, 0, 0, ref compiledBlob, ref errorBlob)))
            {
                throw new PieException(
                    $"Failed to compile shader! {Marshal.PtrToStringAnsi((IntPtr) errorBlob.GetBufferPointer())}");
            }
        }*/

        ID3DBlob* compiledBlob;
        ID3DBlob* errorBlob;
        
        fixed (byte* pCode = code)
        fixed (byte* pEntryPoint = entryPoint)
        fixed (byte* pProfile = profile)
        {
            if (Failed(D3DCompile(pCode, (nuint) code.Length, null, null, null, (sbyte*) pEntryPoint, (sbyte*) pProfile,
                    0, 0, &compiledBlob, &errorBlob)))
            {
                throw new PieException("Failed to compile shader: " + new string((sbyte*) errorBlob->GetBufferPointer(),
                    0, (int) errorBlob->GetBufferSize()));
            }
        }

        errorBlob->Release();

        return compiledBlob;
    }

    private struct ShaderObject
    {
        public ShaderStage Stage;
        public ID3D11DeviceChild* Shader;

        public ShaderObject(ShaderStage stage, ID3D11DeviceChild* shader)
        {
            Stage = stage;
            Shader = shader;
        }
    }
}