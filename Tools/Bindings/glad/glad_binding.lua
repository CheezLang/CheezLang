source_file = "glad_binding_source.cpp"

function prepend_to_cpp()
	return [[
#include <memory>

typedef void* (*GLADloadproc)(const char* name);
extern "C" int gladLoadGL(void);
extern "C" int gladLoadGLLoader(GLADloadproc);

]]
end

function on_global_variable(decl, name, type)
	index = name:find("glad_gl")
	if (index == nil)
	then
		return false, nil
	else
		sub_name = name:sub(6)
		result = sub_name .. " : " .. Type.to_cheez_string(type) .. " #extern #linkname(\"" .. name .. "\")"
		return true, result
	end
end
