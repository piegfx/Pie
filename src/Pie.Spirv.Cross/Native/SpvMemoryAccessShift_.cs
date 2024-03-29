namespace Pie.Spirv.Cross.Native;

public enum SpvMemoryAccessShift_
{
    SpvMemoryAccessVolatileShift = 0,
    SpvMemoryAccessAlignedShift = 1,
    SpvMemoryAccessNontemporalShift = 2,
    SpvMemoryAccessMakePointerAvailableShift = 3,
    SpvMemoryAccessMakePointerAvailableKHRShift = 3,
    SpvMemoryAccessMakePointerVisibleShift = 4,
    SpvMemoryAccessMakePointerVisibleKHRShift = 4,
    SpvMemoryAccessNonPrivatePointerShift = 5,
    SpvMemoryAccessNonPrivatePointerKHRShift = 5,
    SpvMemoryAccessAliasScopeINTELMaskShift = 16,
    SpvMemoryAccessNoAliasINTELMaskShift = 17,
    SpvMemoryAccessMax = 0x7fffffff,
}
