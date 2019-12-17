#include <clang-c/Index.h>

#include <cstdio>

#define C_API extern "C"

typedef enum CXChildVisitResult(*_CXCursorVisitor)(CXCursor* cursor, CXCursor* parent, void* client_data);

struct _CXClientData
{
    _CXCursorVisitor func;
    void* data;
};
C_API unsigned clang_visitChildrenHelper(CXCursor* parent, _CXCursorVisitor visitor, void* client_data) {
    _CXClientData data;
    data.func = visitor;
    data.data = client_data;
    return clang_visitChildren(*parent, [](CXCursor c, CXCursor parent, CXClientData client_data) {
        _CXClientData* data = (_CXClientData*)client_data;
        return data->func(&c, &parent, data->data);
    }, &data);
    return 0;
}
C_API int clang_getCursorKindHelper(CXCursor* cursor) { return clang_getCursorKind(*cursor); }
C_API void clang_getCursorSpellingHelper(CXString* ret, CXCursor* cursor) { *ret = clang_getCursorSpelling(*cursor); }
C_API void clang_getCursorKindSpellingHelper(CXString* ret, int kind) { *ret = clang_getCursorKindSpelling((CXCursorKind)kind); }
C_API void clang_getCursorTypeHelper(CXType* ret, CXCursor* C) { *ret = clang_getCursorType(*C); }


C_API void clang_getTypeSpellingHelper(CXString* ret, CXType* cursor) { *ret = clang_getTypeSpelling(*cursor); }
C_API void clang_getTypeKindSpellingHelper(CXString* ret, int kind) { *ret = clang_getTypeKindSpelling((CXTypeKind)kind); }

C_API const char* clang_getCStringHelper(CXString* string) { return clang_getCString(*string); }
C_API void clang_disposeStringHelper(CXString* string) { clang_disposeString(*string); }

C_API long long clang_Type_getSizeOfHelper(CXType* T) { return clang_Type_getSizeOf(*T); }
C_API long long clang_getArraySizeHelper(CXType* T) { return clang_getArraySize(*T); }
C_API void clang_getResultTypeHelper(CXType* ret, CXType* T) { *ret = clang_getResultType(*T); }
C_API void clang_getArrayElementTypeHelper(CXType* ret, CXType* T) { *ret = clang_getArrayElementType(*T); }
C_API void clang_getPointeeTypeHelper(CXType* ret, CXType* T) { *ret = clang_getPointeeType(*T); }
C_API void clang_getTypeDeclarationHelper(CXCursor* ret, CXType* T) { *ret = clang_getTypeDeclaration(*T); }
C_API void clang_getTypedefDeclUnderlyingTypeHelper(CXType* ret, CXCursor* C) { *ret = clang_getTypedefDeclUnderlyingType(*C); }
C_API void clang_Type_getNamedTypeHelper(CXType* ret, CXType* T) { *ret = clang_Type_getNamedType(*T); }
