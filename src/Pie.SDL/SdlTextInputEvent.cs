namespace Pie.SDL;

public unsafe struct SdlTextInputEvent
{
    public uint Type;
    public uint Timestamp;
    public uint WindowID;
    public fixed char Text[32];
}