using System;

namespace Pie.Freetype;

[Flags]
public enum FaceFlags
{
    None = 1 << 0,
    
    Antialiased = 1 << 1,
    RgbaConvert = 1 << 2
}