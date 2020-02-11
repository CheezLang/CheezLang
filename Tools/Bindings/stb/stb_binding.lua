source_file = "stb_binding_source.cpp"

function prepend_to_cpp()
	return [[
#include <memory>
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
