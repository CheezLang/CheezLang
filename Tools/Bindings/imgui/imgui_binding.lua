source_file = "imgui_binding_source.cpp"

function prepend_to_cpp()
	return [[
#include <memory>
#include "imgui_binding_source.cpp"
]]
end
