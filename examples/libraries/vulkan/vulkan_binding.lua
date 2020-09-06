source_file = "vulkan_binding_source.cpp"

function prepend_to_cpp()
	return [[
#include <memory>
#include "vulkan_binding_source.cpp"
]]
end

excludes = {
    "BENIGN_RACE_BEGIN",
    "BENIGN_RACE_END",
    "NO_COMPETING_THREAD_BEGIN",
    "NO_COMPETING_THREAD_END",
    "NULL",
    "errno",
    "INT8_MIN",
    "INT16_MIN",
    "INT32_MIN",
    "INT64_MIN",
    "INT8_MAX",
    "INT16_MAX",
    "INT32_MAX",
    "INT64_MAX",
    "UINT8_MAX",
    "UINT16_MAX",
    "UINT32_MAX",
    "UINT64_MAX",
    "INT_LEAST8_MIN",
    "INT_LEAST16_MIN",
    "INT_LEAST32_MIN",
    "INT_LEAST64_MIN",
    "INT_LEAST8_MAX",
    "INT_LEAST16_MAX",
    "INT_LEAST32_MAX",
    "INT_LEAST64_MAX",
    "UINT_LEAST8_MAX",
    "UINT_LEAST16_MAX",
    "UINT_LEAST32_MAX",
    "UINT_LEAST64_MAX",
    "INT_FAST8_MIN",
    "INT_FAST16_MIN",
    "INT_FAST32_MIN",
    "INT_FAST64_MIN",
    "INT_FAST8_MAX",
    "INT_FAST16_MAX",
    "INT_FAST32_MAX",
    "INT_FAST64_MAX",
    "UINT_FAST8_MAX",
    "UINT_FAST16_MAX",
    "UINT_FAST32_MAX",
    "UINT_FAST64_MAX",
    "INTPTR_MIN",
    "INTPTR_MAX",
    "UINTPTR_MAX",
    "INTMAX_MIN",
    "INTMAX_MAX",
    "UINTMAX_MAX",
    "PTRDIFF_MIN",
    "PTRDIFF_MAX",
    "SIZE_MAX",
    "SIG_ATOMIC_MIN",
    "SIG_ATOMIC_MAX",
    "WCHAR_MIN",
    "WCHAR_MAX",
    "WINT_MIN",
    "WINT_MAX",
    "VULKAN_CORE_H_",
    "uintptr_t",
    "va_list",
    "size_t",
    "ptrdiff_t",
    "intptr_t",
    "__vcrt_bool",
    "__crt_bool",
    "errno_t",
    "wint_t",
    "wctype_t",
    "__time32_t",
    "__time64_t",
    "__crt_locale_data_public",
    "__crt_locale_pointers",
    "_locale_t",
    "_Mbstatet",
    "mbstate_t",
    "time_t",
    "rsize_t",
    "int8_t",
    "int16_t",
    "int32_t",
    "int64_t",
    "uint8_t",
    "uint16_t",
    "uint32_t",
    "uint64_t",
    "int_least8_t",
    "int_least16_t",
    "int_least32_t",
    "int_least64_t",
    "uint_least8_t",
    "uint_least16_t",
    "uint_least32_t",
    "uint_least64_t",
    "int_fast8_t",
    "int_fast16_t",
    "int_fast32_t",
    "int_fast64_t",
    "uint_fast8_t",
    "uint_fast16_t",
    "uint_fast32_t",
    "uint_fast64_t",
    "intmax_t",
    "uintmax_t"
}

function exclude(val)
    for index, value in ipairs(excludes) do
        if value == val then
            return true
        end
    end

    return false
end

function on_typedef(decl, name, type)
	if exclude(name) then
        return true, nil
    else
        return false, nil
    end
end

function on_macro(decl, name)
	if exclude(name) then
        return true, nil
    else
        return false, nil
    end
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

	-- print(enum_name .. ": " .. member_name .. " -> " .. result)
	
	return result
end
