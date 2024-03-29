namespace Pie;

/// <summary>
/// Used to describe how a new <see cref="Texture"/> should be stored and sampled from.
/// </summary>
public struct TextureDescription
{
    /// <summary>
    /// The type of this texture.
    /// </summary>
    public TextureType TextureType;

    /// <summary>
    /// The width of this texture.
    /// </summary>
    public int Width;

    /// <summary>
    /// The height of this texture.
    /// </summary>
    public int Height;

    /// <summary>
    /// The depth of this texture.
    /// </summary>
    public int Depth;

    /// <summary>
    /// The <see cref="Pie.Format"/> of this texture.
    /// </summary>
    public Format Format;

    /// <summary>
    /// The number of mipmaps this texture will hold.
    /// </summary>
    public int MipLevels;

    /// <summary>
    /// The size of the texture array in elements.
    /// </summary>
    public int ArraySize;

    /*/// <summary>
    /// Whether or not this texture is dynamic.
    /// </summary>
    public bool Dynamic;*/

    /// <summary>
    /// The <see cref="Pie.TextureUsage"/> of this texture.
    /// </summary>
    public TextureUsage Usage;

    /// <summary>
    /// Create a new <see cref="TextureDescription"/>.
    /// </summary>
    /// <param name="textureType">The type of this texture.</param>
    /// <param name="width">The width of this texture.</param>
    /// <param name="height">The height of this texture.</param>
    /// <param name="depth">The depth of this texture.</param>
    /// <param name="format">The <see cref="Pie.Format"/> of this texture.</param>
    /// <param name="mipLevels">The number of mipmaps this texture will hold.</param>
    /// <param name="arraySize">The size of the texture array in elements.</param>
    /// <param name="usage">The <see cref="Pie.TextureUsage"/> of this texture.</param>
    public TextureDescription(TextureType textureType, int width, int height, int depth, Format format, int mipLevels, int arraySize, TextureUsage usage)
    {
        TextureType = textureType;
        Width = width;
        Height = height;
        Depth = depth;
        Format = format;
        MipLevels = mipLevels;
        ArraySize = arraySize;
        Usage = usage;
    }
    
    /// <summary>
    /// Create a new 1D <see cref="TextureDescription"/>.
    /// </summary>
    /// <param name="width">The width of this texture.</param>
    /// <param name="format">The <see cref="Pie.Format"/> of this texture.</param>
    /// <param name="mipLevels">The number of mipmaps this texture will hold.</param>
    /// <param name="arraySize">The size of the texture array in elements.</param>
    /// <param name="usage">The <see cref="Pie.TextureUsage"/> of this texture.</param>
    public static TextureDescription Texture1D(int width, Format format, int mipLevels, int arraySize, TextureUsage usage)
    {
        return new TextureDescription(TextureType.Texture1D, width, 0, 0, format, mipLevels, arraySize, usage);
    }
    
    /// <summary>
    /// Create a new 2D <see cref="TextureDescription"/>.
    /// </summary>
    /// <param name="width">The width of this texture.</param>
    /// <param name="height">The height of this texture.</param>
    /// <param name="format">The <see cref="Pie.Format"/> of this texture.</param>
    /// <param name="mipLevels">The number of mipmaps this texture will hold.</param>
    /// <param name="arraySize">The size of the texture array in elements.</param>
    /// <param name="usage">The <see cref="Pie.TextureUsage"/> of this texture.</param>
    public static TextureDescription Texture2D(int width, int height, Format format, int mipLevels, int arraySize, TextureUsage usage)
    {
        return new TextureDescription(TextureType.Texture2D, width, height, 0, format, mipLevels, arraySize, usage);
    }
    
    /// <summary>
    /// Create a new 3D <see cref="TextureDescription"/>.
    /// </summary>
    /// <param name="width">The width of this texture.</param>
    /// <param name="height">The height of this texture.</param>
    /// <param name="depth">The depth of this texture.</param>
    /// <param name="format">The <see cref="Pie.Format"/> of this texture.</param>
    /// <param name="mipLevels">The number of mipmaps this texture will hold.</param>
    /// <param name="usage">The <see cref="Pie.TextureUsage"/> of this texture.</param>
    public static TextureDescription Texture3D(int width, int height, int depth, Format format, int mipLevels, TextureUsage usage)
    {
        return new TextureDescription(TextureType.Texture3D, width, height, depth, format, mipLevels, 1, usage);
    }
    
    /// <summary>
    /// Create a new Cubemap <see cref="TextureDescription"/>.
    /// </summary>
    /// <param name="width">The width of this texture.</param>
    /// <param name="height">The height of this texture.</param>
    /// <param name="format">The <see cref="Pie.Format"/> of this texture.</param>
    /// <param name="mipLevels">The number of mipmaps this texture will hold.</param>
    /// <param name="usage">The <see cref="Pie.TextureUsage"/> of this texture.</param>
    public static TextureDescription Cubemap(int width, int height, Format format, int mipLevels, TextureUsage usage)
    {
        return new TextureDescription(TextureType.Cubemap, width, height, 0, format, mipLevels,1, usage);
    }

    /// <summary>
    /// Check if this <see cref="TextureDescription"/> is valid.
    /// </summary>
    public Validity Validity
    {
        get
        {
            if (Width < 0 || Height < 0 || Depth < 0)
                return new Validity(false, "Texture width, height, and depth must be at least 0.");

            if (MipLevels < 0)
                return new Validity(false, "Mipmap levels must be at least 0.");
            
            if (ArraySize < 1)
                return new Validity(false, "Array size must be at least 1.");

            if (ArraySize > 1 && TextureType == TextureType.Texture3D)
                return new Validity(false, "3D textures do not support an array size of >1.");

            if (TextureType == TextureType.Cubemap && Width != Height)
                return new Validity(false, "Cubemap width must equal height.");

            if (Format is >= Format.BC1_UNorm and <= Format.BC7_UNorm_SRgb && MipLevels == 0)
                return new Validity(false, "Compressed textures must have an explicit number of mipmaps defined.");

            return new Validity(true, null);
        }
    }
}