source_file = "vulkan_binding_source.cpp"

function prepend_to_cpp()
	return [[
#include <memory>
#include "vulkan_binding_source.cpp"
]]
end

function prepend_to_cheez()
    return [[
#lib("./lib/Bindings.lib")

#export_scope

VK_MAKE_VERSION :: (major: u32, minor: u32, patch: u32) #macro {
    @bin_or(@bin_lsl(major, 16), @bin_lsl(minor, 8), patch)
}
]]
end

excludes = {
    BENIGN_RACE_BEGIN         = true,
    BENIGN_RACE_END           = true,
    NO_COMPETING_THREAD_BEGIN = true,
    NO_COMPETING_THREAD_END   = true,
    NULL                      = true,
    errno                     = true,
    INT8_MIN                  = true,
    INT16_MIN                 = true,
    INT32_MIN                 = true,
    INT64_MIN                 = true,
    INT8_MAX                  = true,
    INT16_MAX                 = true,
    INT32_MAX                 = true,
    INT64_MAX                 = true,
    UINT8_MAX                 = true,
    UINT16_MAX                = true,
    UINT32_MAX                = true,
    UINT64_MAX                = true,
    INT_LEAST8_MIN            = true,
    INT_LEAST16_MIN           = true,
    INT_LEAST32_MIN           = true,
    INT_LEAST64_MIN           = true,
    INT_LEAST8_MAX            = true,
    INT_LEAST16_MAX           = true,
    INT_LEAST32_MAX           = true,
    INT_LEAST64_MAX           = true,
    UINT_LEAST8_MAX           = true,
    UINT_LEAST16_MAX          = true,
    UINT_LEAST32_MAX          = true,
    UINT_LEAST64_MAX          = true,
    INT_FAST8_MIN             = true,
    INT_FAST16_MIN            = true,
    INT_FAST32_MIN            = true,
    INT_FAST64_MIN            = true,
    INT_FAST8_MAX             = true,
    INT_FAST16_MAX            = true,
    INT_FAST32_MAX            = true,
    INT_FAST64_MAX            = true,
    UINT_FAST8_MAX            = true,
    UINT_FAST16_MAX           = true,
    UINT_FAST32_MAX           = true,
    UINT_FAST64_MAX           = true,
    INTPTR_MIN                = true,
    INTPTR_MAX                = true,
    UINTPTR_MAX               = true,
    INTMAX_MIN                = true,
    INTMAX_MAX                = true,
    UINTMAX_MAX               = true,
    PTRDIFF_MIN               = true,
    PTRDIFF_MAX               = true,
    SIZE_MAX                  = true,
    SIG_ATOMIC_MIN            = true,
    SIG_ATOMIC_MAX            = true,
    WCHAR_MIN                 = true,
    WCHAR_MAX                 = true,
    WINT_MIN                  = true,
    WINT_MAX                  = true,
    VULKAN_CORE_H_            = true,
    va_list                   = true,
    ptrdiff_t                 = true,
    intptr_t                  = true,
    __vcrt_bool               = true,
    __crt_bool                = true,
    errno_t                   = true,
    wint_t                    = true,
    wctype_t                  = true,
    __time32_t                = true,
    __time64_t                = true,
    __crt_locale_data_public  = true,
    __crt_locale_pointers     = true,
    _locale_t                 = true,
    _Mbstatet                 = true,
    mbstate_t                 = true,
    time_t                    = true,
    rsize_t                   = true,
    int_least8_t              = true,
    int_least16_t             = true,
    int_least32_t             = true,
    int_least64_t             = true,
    uint_least8_t             = true,
    uint_least16_t            = true,
    uint_least32_t            = true,
    uint_least64_t            = true,
    int_fast8_t               = true,
    int_fast16_t              = true,
    int_fast32_t              = true,
    int_fast64_t              = true,
    uint_fast8_t              = true,
    uint_fast16_t             = true,
    uint_fast32_t             = true,
    uint_fast64_t             = true,
    intmax_t                  = true,
    uintmax_t                 = true,
    VULKAN_H_                 = true,
    VKAPI_CALL                = true,
    VKAPI_PTR                 = true,
    __crt_locale_data_public  = true,
    __crt_locale_pointers     = true,
    _Mbstatet                 = true
}

special_typedefs = {
    VkBuffer              = true,
    VkImage               = true,
    VkInstance            = true,
    VkPhysicalDevice      = true,
    VkDevice              = true,
    VkQueue               = true,
    VkSemaphore           = true,
    VkCommandBuffer       = true,
    VkFence               = true,
    VkDeviceMemory        = true,
    VkEvent               = true,
    VkQueryPool           = true,
    VkBufferView          = true,
    VkImageView           = true,
    VkShaderModule        = true,
    VkPipelineCache       = true,
    VkPipelineLayout      = true,
    VkPipeline            = true,
    VkRenderPass          = true,
    VkDescriptorSetLayout = true,
    VkSampler             = true,
    VkDescriptorSet       = true,
    VkDescriptorPool      = true,
    VkFramebuffer         = true,
    VkCommandPool         = true,
    VkSamplerYcbcrConversion   = true,
    VkDescriptorUpdateTemplate = true,
    VkSurfaceKHR               = true,
    VkSwapchainKHR             = true,
    VkDisplayKHR               = true,
    VkDisplayKHR               = true,
    VkDisplayModeKHR           = true,
    VkDebugReportCallbackEXT   = true,
    VkDebugUtilsMessengerEXT   = true,
    VkValidationCacheEXT       = true,
    VkAccelerationStructureKHR      = true,
    VkPerformanceConfigurationINTEL = true,
    VkIndirectCommandsLayoutNV      = true,
    VkPrivateDataSlotEXT            = true
}

function on_struct(decl, name, type)
	if excludes[name] then
        return true, nil
    end

    return false, nil
end

function on_typedef(decl, name, type)
	if excludes[name] then
        return true, nil
    end

    local special = special_typedefs[name]
    if special then
        return false, name .. "_T :: struct #copy {}"
    end

    return false, nil
end


special_macros = {
    VK_ATTACHMENT_UNUSED        = "u32.max",
    VK_QUEUE_FAMILY_IGNORED     = "u32.max",
    VK_REMAINING_ARRAY_LAYERS   = "u32.max",
    VK_REMAINING_MIP_LEVELS     = "u32.max",
    VK_SUBPASS_EXTERNAL         = "u32.max",
    VK_WHOLE_SIZE               = "u64.max",
    VK_QUEUE_FAMILY_EXTERNAL    = "u32.max - 1",
    VK_QUEUE_FAMILY_FOREIGN_EXT = "u32.max - 2",
    VK_SHADER_UNUSED_KHR        = "u32.max"
}

function on_macro(decl, name)
	if excludes[name] then
        return true, nil
    end

    local special = special_macros[name]
    if special == nil then
        return false, nil
    end

    return true, (name .. " :: " .. special)
end

function on_function(decl, name)
	index = name:find("vk")
	if (index == nil)
	then
		-- doesn't start with glfw*, so don't emit anything
		return true, nil
	else
		-- starts with glfw*, so emit default
		return false, nil
	end
end

builtin_types = {
    i8 = true,
    i16 = true,
    i32 = true,
    i64 = true,
    u8 = true,
    u16 = true,
    u32 = true,
    u64 = true,
    f32 = true,
    f64 = true,
    bool = true,
    string = true
}

function transform_enum_member_name(c, enum_name, member_name)
	-- enum_name is in CamelCase
	-- member_name is in WHATEVEL_CASE
	
	local result = ""
	for i=1, #member_name do
		local prev = member_name:sub(i-1, i-1)
		local c = member_name:sub(i,i)

		if i == 1 or prev == "_" then
			c = c:upper()
		else
			c = c:lower()
		end

		if not (c == "_") then
			result = result .. c
		end
	end

	local i = 1
	while i <= #enum_name and i <= #result and enum_name:sub(i, i) == result:sub(i, i) do
		i = i + 1
	end

	result = result:sub(i)

	if tonumber(result:sub(1, 1)) ~= nil then
		result = "_" .. result
	end

    if builtin_types[result] then
        print(result)
		result = "_" .. result
	end

	-- print(enum_name .. ": " .. member_name .. " -> " .. result)
	
	return result
end

function transform_union_member_name(c, union_name, member_name)
	-- union_name is in CamelCase
	-- member_name is in WHATEVEL_CASE
	
	local result = member_name
    if builtin_types[result] then
		result = "_" .. result
	end

	-- print(union_name .. ": " .. member_name .. " -> " .. result)
	
	return result
end

function on_global_variable(decl, name, type)
	return true, nil
end