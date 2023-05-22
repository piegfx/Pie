namespace Pie.ShaderCompiler.Spirv
{
    [NativeTypeName("unsigned int")]
    public enum spvc_resource_type : uint
    {
        SPVC_RESOURCE_TYPE_UNKNOWN = 0,
        SPVC_RESOURCE_TYPE_UNIFORM_BUFFER = 1,
        SPVC_RESOURCE_TYPE_STORAGE_BUFFER = 2,
        SPVC_RESOURCE_TYPE_STAGE_INPUT = 3,
        SPVC_RESOURCE_TYPE_STAGE_OUTPUT = 4,
        SPVC_RESOURCE_TYPE_SUBPASS_INPUT = 5,
        SPVC_RESOURCE_TYPE_STORAGE_IMAGE = 6,
        SPVC_RESOURCE_TYPE_SAMPLED_IMAGE = 7,
        SPVC_RESOURCE_TYPE_ATOMIC_COUNTER = 8,
        SPVC_RESOURCE_TYPE_PUSH_CONSTANT = 9,
        SPVC_RESOURCE_TYPE_SEPARATE_IMAGE = 10,
        SPVC_RESOURCE_TYPE_SEPARATE_SAMPLERS = 11,
        SPVC_RESOURCE_TYPE_ACCELERATION_STRUCTURE = 12,
        SPVC_RESOURCE_TYPE_RAY_QUERY = 13,
        SPVC_RESOURCE_TYPE_SHADER_RECORD_BUFFER = 14,
        SPVC_RESOURCE_TYPE_INT_MAX = 0x7fffffff,
    }
}
