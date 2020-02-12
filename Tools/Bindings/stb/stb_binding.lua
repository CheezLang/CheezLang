source_file = "stb_binding_gen_source.cpp"

function prepend_to_cpp()
	return [[
#include <memory>
#define STB_IMAGE_IMPLEMENTATION
#include "stb_binding_source.cpp"
]]
end

function prepend_to_cheez()
	return [[
#lib("./lib/stb_binding.lib")
#export_scope
]]
end

function on_global_variable(decl, name, type)
	return true, nil
end

function on_typedef(decl, name, type)
	index = name:find("stbi_")
	if (index == nil)
	then
		-- doesn't start with GLFW*, so don't emit anything
		return true, nil
	else
		-- starts with GLFW*, so emit default
		return false, nil
	end
end

function on_macro(decl, name)
	index = name:find("stbi_")
	if (index == nil)
	then
		-- doesn't start with stbi_*, so don't emit anything
		return true, nil
	else
		-- starts with stbi_*, so emit default
		return false, nil
	end
end

function on_function(decl, name)
	index = name:find("stbi_")
	if (index == nil)
	then
		-- doesn't start with glfw*, so don't emit anything
		return true, nil
	else
		-- starts with glfw*, so emit default
		return false, nil
	end
end

function on_struct(decl, name)
	index = name:find("stbi_")
	if (index == nil)
	then
		-- doesn't start with GLFW*, so don't emit anything
		return true, nil
	else
		-- starts with GLFW*, so emit default
		return false, nil
	end
end
