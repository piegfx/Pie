using System;

namespace Pie.Null;

internal sealed class NullDepthState : DepthState
{
    public override bool IsDisposed { get; protected set; }
    public override DepthStateDescription Description { get; }

    public override void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;

        GC.SuppressFinalize(this);
    }
}
