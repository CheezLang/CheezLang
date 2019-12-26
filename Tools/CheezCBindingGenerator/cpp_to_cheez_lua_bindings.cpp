#pragma once

#include "lua\lua.hpp"

#include "cpp_to_cheez.hpp"

typedef int (CppToCheezGenerator::* mem_func)(lua_State* L);

// This template wraps a member function into a C-style "free" function compatible with lua.
template <mem_func func>
int dispatch(lua_State* L) {
    auto ptr = *(CppToCheezGenerator**)lua_getextraspace(L);
    return ((*ptr).*func)(L);
}

int CppToCheezGenerator::to_cheez_string_lua(lua_State* L) {
    auto type = (CXType*)lua_touserdata(L, 1);
    luaL_argcheck(L, type != NULL, 1, "`type' expected");

    std::stringstream ss;
    emit_cheez_type(ss, *type, false);
    auto result = ss.str();

    luaL_Buffer b;
    luaL_buffinitsize(L, &b, result.length());
    luaL_addlstring(&b, result.data(), result.length());
    luaL_pushresult(&b);
    return 1;
}

int CppToCheezGenerator::to_c_string_lua(lua_State* L) {
    auto type = (CXType*)lua_touserdata(L, 1);
    auto name = lua_tostring(L, 2);
    luaL_argcheck(L, type != NULL, 1, "`type' expected");

    std::stringstream ss;
    emit_c_type(ss, *type, name, false);
    auto result = ss.str();

    luaL_Buffer b;
    luaL_buffinitsize(L, &b, result.length());
    luaL_addlstring(&b, result.data(), result.length());
    luaL_pushresult(&b);
    return 1;
}

int CppToCheezGenerator::get_size_lua(lua_State* L) {
    auto type = (CXType*)lua_touserdata(L, 1);
    luaL_argcheck(L, type != NULL, 1, "`type' expected");
    lua_pushinteger(L, clang_Type_getSizeOf(*type));
    return 1;
}

const luaL_Reg CppToCheezGenerator::lua_lib[4] = {
    { "to_cheez_string", &dispatch<&CppToCheezGenerator::to_cheez_string_lua>},
    { "to_c_string", &dispatch<&CppToCheezGenerator::to_c_string_lua> },
    { "get_size", &dispatch<&CppToCheezGenerator::get_size_lua> },
    { NULL, NULL }
};
