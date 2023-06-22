﻿using System.Text;
using Pie.ShaderCompiler;
using static Pie.Debugging.DebugGraphicsDevice;

namespace Pie.DebugLayer;

internal sealed class DebugShader : Shader
{
    public Shader Shader;
    
    public override bool IsDisposed { get; protected set; }

    public DebugShader(ShaderAttachment[] attachments, SpecializationConstant[] constants)
    {
        StringBuilder builder = new StringBuilder();

        foreach (ShaderAttachment attachment in attachments)
        {
            builder.AppendLine($@"    Attachment:
        Stage: {attachment.Stage}");
        }

        if (constants != null)
        {
            foreach (SpecializationConstant constant in constants)
            {
                builder.AppendLine($@"    Constant:
        ID: {constant.ID}
        Type: {constant.Type},
        PackedValue: {constant.Value}");
            }
        }

        PieLog.Log(LogType.Debug, $"Shader info:\n{builder}");

        Shader = Device.CreateShader(attachments, constants);
    }
    
    public override void Dispose()
    {
        Shader.Dispose();
        IsDisposed = Shader.IsDisposed;
        PieLog.Log(LogType.Debug, "Shader disposed.");
    }
}