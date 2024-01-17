using System;
using static Pie.OpenGL.GlGraphicsDevice;

namespace Pie.OpenGL;

internal sealed class GlRasterizerState : RasterizerState
{
    private bool _cullFaceEnabled;
    private TriangleFace _cullFaceMode;
    private FrontFaceDirection _frontFace;
    private PolygonMode _mode;
    private bool _scissor;
    
    public GlRasterizerState(RasterizerStateDescription description)
    {
        Description = description;

        if (description.CullFace == CullFace.None)
            _cullFaceEnabled = false;
        else
        {
            _cullFaceEnabled = true;
            _cullFaceMode = description.CullFace switch
            {
                CullFace.Front => TriangleFace.Front,
                CullFace.Back => TriangleFace.Back,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        _frontFace = description.CullDirection switch
        {
            CullDirection.Clockwise => FrontFaceDirection.CW,
            CullDirection.CounterClockwise => FrontFaceDirection.Ccw,
            _ => throw new ArgumentOutOfRangeException()
        };

        _mode = description.FillMode switch
        {
            FillMode.Solid => PolygonMode.Fill,
            FillMode.Wireframe => PolygonMode.Line,
            _ => throw new ArgumentOutOfRangeException()
        };

        _scissor = description.ScissorTest;
    }
    
    public override bool IsDisposed { get; protected set; }

    public override RasterizerStateDescription Description { get; }
    
    public void Set()
    {
        if (!_cullFaceEnabled)
            Gl.Disable(EnableCap.CullFace);
        else
        {
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(_cullFaceMode);
        }
        
        Gl.FrontFace(_frontFace);

        if (!IsES)
            Gl.PolygonMode(TriangleFace.FrontAndBack, _mode);
        
        if (_scissor)
            Gl.Enable(EnableCap.ScissorTest);
        else
            Gl.Disable(EnableCap.ScissorTest);
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        // There is nothing to dispose.
    }
}