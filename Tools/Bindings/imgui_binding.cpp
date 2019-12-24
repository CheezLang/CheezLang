#include <memory>
#include "imgui_binding_all.cpp"

extern "C" void __c__ImVec2_new_2(ImVec2* self) {
    new (self) ImVec2();
}
extern "C" void __c__ImVec2_new_3(ImVec2* self, float __x, float __y) {
    new (self) ImVec2(__x, __y);
}
extern "C" void __c__ImVec4_new_4(ImVec4* self) {
    new (self) ImVec4();
}
extern "C" void __c__ImVec4_new_5(ImVec4* self, float __x, float __y, float __z, float __w) {
    new (self) ImVec4(__x, __y, __z, __w);
}
extern "C" void __c__ImGuiStyle_new_35(ImGuiStyle* self) {
    new (self) ImGuiStyle();
}
extern "C" void __c__ImGuiStyle_ScaleAllSizes_36(ImGuiStyle* self, float _scale_factor) {
    self->ScaleAllSizes(_scale_factor);
}
extern "C" void __c__ImGuiIO_AddInputCharacter_53(ImGuiIO* self, uint32_t _c) {
    self->AddInputCharacter(_c);
}
extern "C" void __c__ImGuiIO_AddInputCharactersUTF8_54(ImGuiIO* self, char * _str) {
    self->AddInputCharactersUTF8(_str);
}
extern "C" void __c__ImGuiIO_ClearInputCharacters_55(ImGuiIO* self) {
    self->ClearInputCharacters();
}
extern "C" void __c__ImGuiIO_new_87(ImGuiIO* self) {
    new (self) ImGuiIO();
}
extern "C" void __c__ImGuiInputTextCallbackData_new_12(ImGuiInputTextCallbackData* self) {
    new (self) ImGuiInputTextCallbackData();
}
extern "C" void __c__ImGuiInputTextCallbackData_DeleteChars_13(ImGuiInputTextCallbackData* self, int32_t _pos, int32_t _bytes_count) {
    self->DeleteChars(_pos, _bytes_count);
}
extern "C" void __c__ImGuiInputTextCallbackData_InsertChars_14(ImGuiInputTextCallbackData* self, int32_t _pos, char * _text, char * _text_end) {
    self->InsertChars(_pos, _text, _text_end);
}
extern "C" void __c__ImGuiInputTextCallbackData_HasSelection_15(ImGuiInputTextCallbackData* self, bool *ret) {
    *ret = (bool )self->HasSelection();
}
extern "C" void __c__ImGuiWindowClass_new_6(ImGuiWindowClass* self) {
    new (self) ImGuiWindowClass();
}
extern "C" void __c__ImGuiPayload_new_8(ImGuiPayload* self) {
    new (self) ImGuiPayload();
}
extern "C" void __c__ImGuiPayload_Clear_9(ImGuiPayload* self) {
    self->Clear();
}
extern "C" void __c__ImGuiPayload_IsDataType_10(ImGuiPayload* self, bool *ret, char * _type) {
    *ret = (bool )self->IsDataType(_type);
}
extern "C" void __c__ImGuiPayload_IsPreview_11(ImGuiPayload* self, bool *ret) {
    *ret = (bool )self->IsPreview();
}
extern "C" void __c__ImGuiPayload_IsDelivery_12(ImGuiPayload* self, bool *ret) {
    *ret = (bool )self->IsDelivery();
}
extern "C" void __c__ImGuiOnceUponAFrame_new_0(ImGuiOnceUponAFrame* self) {
    new (self) ImGuiOnceUponAFrame();
}
extern "C" void __c__ImGuiTextFilter_new_0(ImGuiTextFilter* self, char * _default_filter) {
    new (self) ImGuiTextFilter(_default_filter);
}
extern "C" void __c__ImGuiTextFilter_Draw_1(ImGuiTextFilter* self, bool *ret, char * _label, float _width) {
    *ret = (bool )self->Draw(_label, _width);
}
extern "C" void __c__ImGuiTextFilter_PassFilter_2(ImGuiTextFilter* self, bool *ret, char * _text, char * _text_end) {
    *ret = (bool )self->PassFilter(_text, _text_end);
}
extern "C" void __c__ImGuiTextFilter_Build_3(ImGuiTextFilter* self) {
    self->Build();
}
extern "C" void __c__ImGuiTextFilter_Clear_4(ImGuiTextFilter* self) {
    self->Clear();
}
extern "C" void __c__ImGuiTextFilter_IsActive_5(ImGuiTextFilter* self, bool *ret) {
    *ret = (bool )self->IsActive();
}
extern "C" void __c__ImGuiTextRange_new_2(ImGuiTextFilter::ImGuiTextRange * self) {
    new (self) ImGuiTextFilter::ImGuiTextRange();
}
extern "C" void __c__ImGuiTextRange_new_3(ImGuiTextFilter::ImGuiTextRange * self, char * __b, char * __e) {
    new (self) ImGuiTextFilter::ImGuiTextRange(__b, __e);
}
extern "C" void __c__ImGuiTextRange_empty_4(ImGuiTextFilter::ImGuiTextRange * self, bool *ret) {
    *ret = (bool )self->empty();
}
extern "C" void __c__ImGuiTextRange_split_5(ImGuiTextFilter::ImGuiTextRange * self, char _separator, ImVector<ImGuiTextFilter::ImGuiTextRange> * _out) {
    self->split(_separator, _out);
}
extern "C" void __c__ImGuiTextBuffer_new_2(ImGuiTextBuffer* self) {
    new (self) ImGuiTextBuffer();
}
extern "C" void __c__ImGuiTextBuffer_begin_4(ImGuiTextBuffer* self, char * *ret) {
    *ret = (char * )self->begin();
}
extern "C" void __c__ImGuiTextBuffer_end_5(ImGuiTextBuffer* self, char * *ret) {
    *ret = (char * )self->end();
}
extern "C" void __c__ImGuiTextBuffer_size_6(ImGuiTextBuffer* self, int32_t *ret) {
    *ret = (int32_t )self->size();
}
extern "C" void __c__ImGuiTextBuffer_empty_7(ImGuiTextBuffer* self, bool *ret) {
    *ret = (bool )self->empty();
}
extern "C" void __c__ImGuiTextBuffer_clear_8(ImGuiTextBuffer* self) {
    self->clear();
}
extern "C" void __c__ImGuiTextBuffer_reserve_9(ImGuiTextBuffer* self, int32_t _capacity) {
    self->reserve(_capacity);
}
extern "C" void __c__ImGuiTextBuffer_c_str_10(ImGuiTextBuffer* self, char * *ret) {
    *ret = (char * )self->c_str();
}
extern "C" void __c__ImGuiTextBuffer_append_11(ImGuiTextBuffer* self, char * _str, char * _str_end) {
    self->append(_str, _str_end);
}
extern "C" void __c__ImGuiTextBuffer_appendf_12(ImGuiTextBuffer* self, char * _fmt) {
    self->appendf(_fmt);
}
extern "C" void __c__ImGuiTextBuffer_appendfv_13(ImGuiTextBuffer* self, char * _fmt, char * _args) {
    self->appendfv(_fmt, _args);
}
extern "C" void __c__ImGuiStorage_Clear_2(ImGuiStorage* self) {
    self->Clear();
}
extern "C" void __c__ImGuiStorage_GetInt_3(ImGuiStorage* self, int32_t *ret, uint32_t _key, int32_t _default_val) {
    *ret = (int32_t )self->GetInt(_key, _default_val);
}
extern "C" void __c__ImGuiStorage_SetInt_4(ImGuiStorage* self, uint32_t _key, int32_t _val) {
    self->SetInt(_key, _val);
}
extern "C" void __c__ImGuiStorage_GetBool_5(ImGuiStorage* self, bool *ret, uint32_t _key, bool _default_val) {
    *ret = (bool )self->GetBool(_key, _default_val);
}
extern "C" void __c__ImGuiStorage_SetBool_6(ImGuiStorage* self, uint32_t _key, bool _val) {
    self->SetBool(_key, _val);
}
extern "C" void __c__ImGuiStorage_GetFloat_7(ImGuiStorage* self, float *ret, uint32_t _key, float _default_val) {
    *ret = (float )self->GetFloat(_key, _default_val);
}
extern "C" void __c__ImGuiStorage_SetFloat_8(ImGuiStorage* self, uint32_t _key, float _val) {
    self->SetFloat(_key, _val);
}
extern "C" void __c__ImGuiStorage_GetVoidPtr_9(ImGuiStorage* self, void * *ret, uint32_t _key) {
    *ret = (void * )self->GetVoidPtr(_key);
}
extern "C" void __c__ImGuiStorage_SetVoidPtr_10(ImGuiStorage* self, uint32_t _key, void * _val) {
    self->SetVoidPtr(_key, _val);
}
extern "C" void __c__ImGuiStorage_GetIntRef_11(ImGuiStorage* self, int32_t * *ret, uint32_t _key, int32_t _default_val) {
    *ret = (int32_t * )self->GetIntRef(_key, _default_val);
}
extern "C" void __c__ImGuiStorage_GetBoolRef_12(ImGuiStorage* self, bool * *ret, uint32_t _key, bool _default_val) {
    *ret = (bool * )self->GetBoolRef(_key, _default_val);
}
extern "C" void __c__ImGuiStorage_GetFloatRef_13(ImGuiStorage* self, float * *ret, uint32_t _key, float _default_val) {
    *ret = (float * )self->GetFloatRef(_key, _default_val);
}
extern "C" void __c__ImGuiStorage_GetVoidPtrRef_14(ImGuiStorage* self, void * * *ret, uint32_t _key, void * _default_val) {
    *ret = (void * * )self->GetVoidPtrRef(_key, _default_val);
}
extern "C" void __c__ImGuiStorage_SetAllInt_15(ImGuiStorage* self, int32_t _val) {
    self->SetAllInt(_val);
}
extern "C" void __c__ImGuiStorage_BuildSortByKey_16(ImGuiStorage* self) {
    self->BuildSortByKey();
}
extern "C" void __c__ImGuiStoragePair_new_2(ImGuiStorage::ImGuiStoragePair * self, uint32_t __key, int32_t __val_i) {
    new (self) ImGuiStorage::ImGuiStoragePair(__key, __val_i);
}
extern "C" void __c__ImGuiStoragePair_new_3(ImGuiStorage::ImGuiStoragePair * self, uint32_t __key, float __val_f) {
    new (self) ImGuiStorage::ImGuiStoragePair(__key, __val_f);
}
extern "C" void __c__ImGuiStoragePair_new_4(ImGuiStorage::ImGuiStoragePair * self, uint32_t __key, void * __val_p) {
    new (self) ImGuiStorage::ImGuiStoragePair(__key, __val_p);
}
extern "C" void __c__ImGuiListClipper_new_6(ImGuiListClipper* self, int32_t _items_count, float _items_height) {
    new (self) ImGuiListClipper(_items_count, _items_height);
}
extern "C" void __c__ImGuiListClipper_dtor(ImGuiListClipper* self) {
    self->~ImGuiListClipper();
}
extern "C" void __c__ImGuiListClipper_Step_8(ImGuiListClipper* self, bool *ret) {
    *ret = (bool )self->Step();
}
extern "C" void __c__ImGuiListClipper_Begin_9(ImGuiListClipper* self, int32_t _items_count, float _items_height) {
    self->Begin(_items_count, _items_height);
}
extern "C" void __c__ImGuiListClipper_End_10(ImGuiListClipper* self) {
    self->End();
}
extern "C" void __c__ImColor_new_1(ImColor* self) {
    new (self) ImColor();
}
extern "C" void __c__ImColor_new_2(ImColor* self, int32_t _r, int32_t _g, int32_t _b, int32_t _a) {
    new (self) ImColor(_r, _g, _b, _a);
}
extern "C" void __c__ImColor_new_3(ImColor* self, uint32_t _rgba) {
    new (self) ImColor(_rgba);
}
extern "C" void __c__ImColor_new_4(ImColor* self, float _r, float _g, float _b, float _a) {
    new (self) ImColor(_r, _g, _b, _a);
}
extern "C" void __c__ImColor_new_5(ImColor* self, ImVec4 * _col) {
    new (self) ImColor(*_col);
}
extern "C" void __c__ImColor_SetHSV_8(ImColor* self, float _h, float _s, float _v, float _a) {
    self->SetHSV(_h, _s, _v, _a);
}
extern "C" void __c__ImColor_HSV_9(ImColor* self, ImColor *ret, float _h, float _s, float _v, float _a) {
    *ret = (ImColor )self->HSV(_h, _s, _v, _a);
}
extern "C" void __c__ImDrawCmd_new_7(ImDrawCmd* self) {
    new (self) ImDrawCmd();
}
extern "C" void __c__ImDrawListSplitter_new_3(ImDrawListSplitter* self) {
    new (self) ImDrawListSplitter();
}
extern "C" void __c__ImDrawListSplitter_dtor(ImDrawListSplitter* self) {
    self->~ImDrawListSplitter();
}
extern "C" void __c__ImDrawListSplitter_Clear_5(ImDrawListSplitter* self) {
    self->Clear();
}
extern "C" void __c__ImDrawListSplitter_ClearFreeMemory_6(ImDrawListSplitter* self) {
    self->ClearFreeMemory();
}
extern "C" void __c__ImDrawListSplitter_Split_7(ImDrawListSplitter* self, ImDrawList * _draw_list, int32_t _count) {
    self->Split(_draw_list, _count);
}
extern "C" void __c__ImDrawListSplitter_Merge_8(ImDrawListSplitter* self, ImDrawList * _draw_list) {
    self->Merge(_draw_list);
}
extern "C" void __c__ImDrawListSplitter_SetCurrentChannel_9(ImDrawListSplitter* self, ImDrawList * _draw_list, int32_t _channel_idx) {
    self->SetCurrentChannel(_draw_list, _channel_idx);
}
extern "C" void __c__ImDrawList_new_14(ImDrawList* self, ImDrawListSharedData * _shared_data) {
    new (self) ImDrawList(_shared_data);
}
extern "C" void __c__ImDrawList_dtor(ImDrawList* self) {
    self->~ImDrawList();
}
extern "C" void __c__ImDrawList_PushClipRect_16(ImDrawList* self, ImVec2* _clip_rect_min, ImVec2* _clip_rect_max, bool _intersect_with_current_clip_rect) {
    self->PushClipRect(*_clip_rect_min, *_clip_rect_max, _intersect_with_current_clip_rect);
}
extern "C" void __c__ImDrawList_PushClipRectFullScreen_17(ImDrawList* self) {
    self->PushClipRectFullScreen();
}
extern "C" void __c__ImDrawList_PopClipRect_18(ImDrawList* self) {
    self->PopClipRect();
}
extern "C" void __c__ImDrawList_PushTextureID_19(ImDrawList* self, void * _texture_id) {
    self->PushTextureID(_texture_id);
}
extern "C" void __c__ImDrawList_PopTextureID_20(ImDrawList* self) {
    self->PopTextureID();
}
extern "C" void __c__ImDrawList_GetClipRectMin_21(ImDrawList* self, ImVec2 *ret) {
    *ret = (ImVec2 )self->GetClipRectMin();
}
extern "C" void __c__ImDrawList_GetClipRectMax_22(ImDrawList* self, ImVec2 *ret) {
    *ret = (ImVec2 )self->GetClipRectMax();
}
extern "C" void __c__ImDrawList_AddLine_23(ImDrawList* self, ImVec2 * _p1, ImVec2 * _p2, uint32_t _col, float _thickness) {
    self->AddLine(*_p1, *_p2, _col, _thickness);
}
extern "C" void __c__ImDrawList_AddRect_24(ImDrawList* self, ImVec2 * _p_min, ImVec2 * _p_max, uint32_t _col, float _rounding, int32_t _rounding_corners, float _thickness) {
    self->AddRect(*_p_min, *_p_max, _col, _rounding, _rounding_corners, _thickness);
}
extern "C" void __c__ImDrawList_AddRectFilled_25(ImDrawList* self, ImVec2 * _p_min, ImVec2 * _p_max, uint32_t _col, float _rounding, int32_t _rounding_corners) {
    self->AddRectFilled(*_p_min, *_p_max, _col, _rounding, _rounding_corners);
}
extern "C" void __c__ImDrawList_AddRectFilledMultiColor_26(ImDrawList* self, ImVec2 * _p_min, ImVec2 * _p_max, uint32_t _col_upr_left, uint32_t _col_upr_right, uint32_t _col_bot_right, uint32_t _col_bot_left) {
    self->AddRectFilledMultiColor(*_p_min, *_p_max, _col_upr_left, _col_upr_right, _col_bot_right, _col_bot_left);
}
extern "C" void __c__ImDrawList_AddQuad_27(ImDrawList* self, ImVec2 * _p1, ImVec2 * _p2, ImVec2 * _p3, ImVec2 * _p4, uint32_t _col, float _thickness) {
    self->AddQuad(*_p1, *_p2, *_p3, *_p4, _col, _thickness);
}
extern "C" void __c__ImDrawList_AddQuadFilled_28(ImDrawList* self, ImVec2 * _p1, ImVec2 * _p2, ImVec2 * _p3, ImVec2 * _p4, uint32_t _col) {
    self->AddQuadFilled(*_p1, *_p2, *_p3, *_p4, _col);
}
extern "C" void __c__ImDrawList_AddTriangle_29(ImDrawList* self, ImVec2 * _p1, ImVec2 * _p2, ImVec2 * _p3, uint32_t _col, float _thickness) {
    self->AddTriangle(*_p1, *_p2, *_p3, _col, _thickness);
}
extern "C" void __c__ImDrawList_AddTriangleFilled_30(ImDrawList* self, ImVec2 * _p1, ImVec2 * _p2, ImVec2 * _p3, uint32_t _col) {
    self->AddTriangleFilled(*_p1, *_p2, *_p3, _col);
}
extern "C" void __c__ImDrawList_AddCircle_31(ImDrawList* self, ImVec2 * _center, float _radius, uint32_t _col, int32_t _num_segments, float _thickness) {
    self->AddCircle(*_center, _radius, _col, _num_segments, _thickness);
}
extern "C" void __c__ImDrawList_AddCircleFilled_32(ImDrawList* self, ImVec2 * _center, float _radius, uint32_t _col, int32_t _num_segments) {
    self->AddCircleFilled(*_center, _radius, _col, _num_segments);
}
extern "C" void __c__ImDrawList_AddNgon_33(ImDrawList* self, ImVec2 * _center, float _radius, uint32_t _col, int32_t _num_segments, float _thickness) {
    self->AddNgon(*_center, _radius, _col, _num_segments, _thickness);
}
extern "C" void __c__ImDrawList_AddNgonFilled_34(ImDrawList* self, ImVec2 * _center, float _radius, uint32_t _col, int32_t _num_segments) {
    self->AddNgonFilled(*_center, _radius, _col, _num_segments);
}
extern "C" void __c__ImDrawList_AddText_35(ImDrawList* self, ImVec2 * _pos, uint32_t _col, char * _text_begin, char * _text_end) {
    self->AddText(*_pos, _col, _text_begin, _text_end);
}
extern "C" void __c__ImDrawList_AddText_36(ImDrawList* self, ImFont * _font, float _font_size, ImVec2 * _pos, uint32_t _col, char * _text_begin, char * _text_end, float _wrap_width, ImVec4 * _cpu_fine_clip_rect) {
    self->AddText(_font, _font_size, *_pos, _col, _text_begin, _text_end, _wrap_width, _cpu_fine_clip_rect);
}
extern "C" void __c__ImDrawList_AddPolyline_37(ImDrawList* self, ImVec2 * _points, int32_t _num_points, uint32_t _col, bool _closed, float _thickness) {
    self->AddPolyline(_points, _num_points, _col, _closed, _thickness);
}
extern "C" void __c__ImDrawList_AddConvexPolyFilled_38(ImDrawList* self, ImVec2 * _points, int32_t _num_points, uint32_t _col) {
    self->AddConvexPolyFilled(_points, _num_points, _col);
}
extern "C" void __c__ImDrawList_AddBezierCurve_39(ImDrawList* self, ImVec2 * _p1, ImVec2 * _p2, ImVec2 * _p3, ImVec2 * _p4, uint32_t _col, float _thickness, int32_t _num_segments) {
    self->AddBezierCurve(*_p1, *_p2, *_p3, *_p4, _col, _thickness, _num_segments);
}
extern "C" void __c__ImDrawList_AddImage_40(ImDrawList* self, void * _user_texture_id, ImVec2 * _p_min, ImVec2 * _p_max, ImVec2 * _uv_min, ImVec2 * _uv_max, uint32_t _col) {
    self->AddImage(_user_texture_id, *_p_min, *_p_max, *_uv_min, *_uv_max, _col);
}
extern "C" void __c__ImDrawList_AddImageQuad_41(ImDrawList* self, void * _user_texture_id, ImVec2 * _p1, ImVec2 * _p2, ImVec2 * _p3, ImVec2 * _p4, ImVec2 * _uv1, ImVec2 * _uv2, ImVec2 * _uv3, ImVec2 * _uv4, uint32_t _col) {
    self->AddImageQuad(_user_texture_id, *_p1, *_p2, *_p3, *_p4, *_uv1, *_uv2, *_uv3, *_uv4, _col);
}
extern "C" void __c__ImDrawList_AddImageRounded_42(ImDrawList* self, void * _user_texture_id, ImVec2 * _p_min, ImVec2 * _p_max, ImVec2 * _uv_min, ImVec2 * _uv_max, uint32_t _col, float _rounding, int32_t _rounding_corners) {
    self->AddImageRounded(_user_texture_id, *_p_min, *_p_max, *_uv_min, *_uv_max, _col, _rounding, _rounding_corners);
}
extern "C" void __c__ImDrawList_PathClear_43(ImDrawList* self) {
    self->PathClear();
}
extern "C" void __c__ImDrawList_PathLineTo_44(ImDrawList* self, ImVec2 * _pos) {
    self->PathLineTo(*_pos);
}
extern "C" void __c__ImDrawList_PathLineToMergeDuplicate_45(ImDrawList* self, ImVec2 * _pos) {
    self->PathLineToMergeDuplicate(*_pos);
}
extern "C" void __c__ImDrawList_PathFillConvex_46(ImDrawList* self, uint32_t _col) {
    self->PathFillConvex(_col);
}
extern "C" void __c__ImDrawList_PathStroke_47(ImDrawList* self, uint32_t _col, bool _closed, float _thickness) {
    self->PathStroke(_col, _closed, _thickness);
}
extern "C" void __c__ImDrawList_PathArcTo_48(ImDrawList* self, ImVec2 * _center, float _radius, float _a_min, float _a_max, int32_t _num_segments) {
    self->PathArcTo(*_center, _radius, _a_min, _a_max, _num_segments);
}
extern "C" void __c__ImDrawList_PathArcToFast_49(ImDrawList* self, ImVec2 * _center, float _radius, int32_t _a_min_of_12, int32_t _a_max_of_12) {
    self->PathArcToFast(*_center, _radius, _a_min_of_12, _a_max_of_12);
}
extern "C" void __c__ImDrawList_PathBezierCurveTo_50(ImDrawList* self, ImVec2 * _p2, ImVec2 * _p3, ImVec2 * _p4, int32_t _num_segments) {
    self->PathBezierCurveTo(*_p2, *_p3, *_p4, _num_segments);
}
extern "C" void __c__ImDrawList_PathRect_51(ImDrawList* self, ImVec2 * _rect_min, ImVec2 * _rect_max, float _rounding, int32_t _rounding_corners) {
    self->PathRect(*_rect_min, *_rect_max, _rounding, _rounding_corners);
}
extern "C" void __c__ImDrawList_AddCallback_52(ImDrawList* self, ImDrawCallback _callback, void * _callback_data) {
    self->AddCallback(_callback, _callback_data);
}
extern "C" void __c__ImDrawList_AddDrawCmd_53(ImDrawList* self) {
    self->AddDrawCmd();
}
extern "C" void __c__ImDrawList_CloneOutput_54(ImDrawList* self, ImDrawList * *ret) {
    *ret = (ImDrawList * )self->CloneOutput();
}
extern "C" void __c__ImDrawList_ChannelsSplit_55(ImDrawList* self, int32_t _count) {
    self->ChannelsSplit(_count);
}
extern "C" void __c__ImDrawList_ChannelsMerge_56(ImDrawList* self) {
    self->ChannelsMerge();
}
extern "C" void __c__ImDrawList_ChannelsSetCurrent_57(ImDrawList* self, int32_t _n) {
    self->ChannelsSetCurrent(_n);
}
extern "C" void __c__ImDrawList_Clear_58(ImDrawList* self) {
    self->Clear();
}
extern "C" void __c__ImDrawList_ClearFreeMemory_59(ImDrawList* self) {
    self->ClearFreeMemory();
}
extern "C" void __c__ImDrawList_PrimReserve_60(ImDrawList* self, int32_t _idx_count, int32_t _vtx_count) {
    self->PrimReserve(_idx_count, _vtx_count);
}
extern "C" void __c__ImDrawList_PrimUnreserve_61(ImDrawList* self, int32_t _idx_count, int32_t _vtx_count) {
    self->PrimUnreserve(_idx_count, _vtx_count);
}
extern "C" void __c__ImDrawList_PrimRect_62(ImDrawList* self, ImVec2 * _a, ImVec2 * _b, uint32_t _col) {
    self->PrimRect(*_a, *_b, _col);
}
extern "C" void __c__ImDrawList_PrimRectUV_63(ImDrawList* self, ImVec2 * _a, ImVec2 * _b, ImVec2 * _uv_a, ImVec2 * _uv_b, uint32_t _col) {
    self->PrimRectUV(*_a, *_b, *_uv_a, *_uv_b, _col);
}
extern "C" void __c__ImDrawList_PrimQuadUV_64(ImDrawList* self, ImVec2 * _a, ImVec2 * _b, ImVec2 * _c, ImVec2 * _d, ImVec2 * _uv_a, ImVec2 * _uv_b, ImVec2 * _uv_c, ImVec2 * _uv_d, uint32_t _col) {
    self->PrimQuadUV(*_a, *_b, *_c, *_d, *_uv_a, *_uv_b, *_uv_c, *_uv_d, _col);
}
extern "C" void __c__ImDrawList_PrimWriteVtx_65(ImDrawList* self, ImVec2 * _pos, ImVec2 * _uv, uint32_t _col) {
    self->PrimWriteVtx(*_pos, *_uv, _col);
}
extern "C" void __c__ImDrawList_PrimWriteIdx_66(ImDrawList* self, uint16_t _idx) {
    self->PrimWriteIdx(_idx);
}
extern "C" void __c__ImDrawList_PrimVtx_67(ImDrawList* self, ImVec2 * _pos, ImVec2 * _uv, uint32_t _col) {
    self->PrimVtx(*_pos, *_uv, _col);
}
extern "C" void __c__ImDrawList_UpdateClipRect_68(ImDrawList* self) {
    self->UpdateClipRect();
}
extern "C" void __c__ImDrawList_UpdateTextureID_69(ImDrawList* self) {
    self->UpdateTextureID();
}
extern "C" void __c__ImDrawData_new_9(ImDrawData* self) {
    new (self) ImDrawData();
}
extern "C" void __c__ImDrawData_dtor(ImDrawData* self) {
    self->~ImDrawData();
}
extern "C" void __c__ImDrawData_Clear_11(ImDrawData* self) {
    self->Clear();
}
extern "C" void __c__ImDrawData_DeIndexAllBuffers_12(ImDrawData* self) {
    self->DeIndexAllBuffers();
}
extern "C" void __c__ImDrawData_ScaleClipRects_13(ImDrawData* self, ImVec2 * _fb_scale) {
    self->ScaleClipRects(*_fb_scale);
}
extern "C" void __c__ImFontConfig_new_19(ImFontConfig* self) {
    new (self) ImFontConfig();
}
extern "C" void __c__ImFontGlyphRangesBuilder_new_1(ImFontGlyphRangesBuilder* self) {
    new (self) ImFontGlyphRangesBuilder();
}
extern "C" void __c__ImFontGlyphRangesBuilder_Clear_2(ImFontGlyphRangesBuilder* self) {
    self->Clear();
}
extern "C" void __c__ImFontGlyphRangesBuilder_GetBit_3(ImFontGlyphRangesBuilder* self, bool *ret, int32_t _n) {
    *ret = (bool )self->GetBit(_n);
}
extern "C" void __c__ImFontGlyphRangesBuilder_SetBit_4(ImFontGlyphRangesBuilder* self, int32_t _n) {
    self->SetBit(_n);
}
extern "C" void __c__ImFontGlyphRangesBuilder_AddChar_5(ImFontGlyphRangesBuilder* self, uint16_t _c) {
    self->AddChar(_c);
}
extern "C" void __c__ImFontGlyphRangesBuilder_AddText_6(ImFontGlyphRangesBuilder* self, char * _text, char * _text_end) {
    self->AddText(_text, _text_end);
}
extern "C" void __c__ImFontGlyphRangesBuilder_AddRanges_7(ImFontGlyphRangesBuilder* self, const ImWchar * _ranges) {
    self->AddRanges(_ranges);
}
extern "C" void __c__ImFontGlyphRangesBuilder_BuildRanges_8(ImFontGlyphRangesBuilder* self, ImVector<ImWchar> * _out_ranges) {
    self->BuildRanges(_out_ranges);
}
extern "C" void __c__ImFontAtlasCustomRect_new_8(ImFontAtlasCustomRect* self) {
    new (self) ImFontAtlasCustomRect();
}
extern "C" void __c__ImFontAtlasCustomRect_IsPacked_9(ImFontAtlasCustomRect* self, bool *ret) {
    *ret = (bool )self->IsPacked();
}
extern "C" void __c__ImFontAtlas_new_0(ImFontAtlas* self) {
    new (self) ImFontAtlas();
}
extern "C" void __c__ImFontAtlas_dtor(ImFontAtlas* self) {
    self->~ImFontAtlas();
}
extern "C" void __c__ImFontAtlas_AddFont_2(ImFontAtlas* self, ImFont * *ret, ImFontConfig * _font_cfg) {
    *ret = (ImFont * )self->AddFont(_font_cfg);
}
extern "C" void __c__ImFontAtlas_AddFontDefault_3(ImFontAtlas* self, ImFont * *ret, ImFontConfig * _font_cfg) {
    *ret = (ImFont * )self->AddFontDefault(_font_cfg);
}
extern "C" void __c__ImFontAtlas_AddFontFromFileTTF_4(ImFontAtlas* self, ImFont * *ret, char * _filename, float _size_pixels, ImFontConfig * _font_cfg, const ImWchar * _glyph_ranges) {
    *ret = (ImFont * )self->AddFontFromFileTTF(_filename, _size_pixels, _font_cfg, _glyph_ranges);
}
extern "C" void __c__ImFontAtlas_AddFontFromMemoryTTF_5(ImFontAtlas* self, ImFont * *ret, void * _font_data, int32_t _font_size, float _size_pixels, ImFontConfig * _font_cfg, const ImWchar * _glyph_ranges) {
    *ret = (ImFont * )self->AddFontFromMemoryTTF(_font_data, _font_size, _size_pixels, _font_cfg, _glyph_ranges);
}
extern "C" void __c__ImFontAtlas_AddFontFromMemoryCompressedTTF_6(ImFontAtlas* self, ImFont * *ret, void * _compressed_font_data, int32_t _compressed_font_size, float _size_pixels, ImFontConfig * _font_cfg, const ImWchar * _glyph_ranges) {
    *ret = (ImFont * )self->AddFontFromMemoryCompressedTTF(_compressed_font_data, _compressed_font_size, _size_pixels, _font_cfg, _glyph_ranges);
}
extern "C" void __c__ImFontAtlas_AddFontFromMemoryCompressedBase85TTF_7(ImFontAtlas* self, ImFont * *ret, char * _compressed_font_data_base85, float _size_pixels, ImFontConfig * _font_cfg, const ImWchar * _glyph_ranges) {
    *ret = (ImFont * )self->AddFontFromMemoryCompressedBase85TTF(_compressed_font_data_base85, _size_pixels, _font_cfg, _glyph_ranges);
}
extern "C" void __c__ImFontAtlas_ClearInputData_8(ImFontAtlas* self) {
    self->ClearInputData();
}
extern "C" void __c__ImFontAtlas_ClearTexData_9(ImFontAtlas* self) {
    self->ClearTexData();
}
extern "C" void __c__ImFontAtlas_ClearFonts_10(ImFontAtlas* self) {
    self->ClearFonts();
}
extern "C" void __c__ImFontAtlas_Clear_11(ImFontAtlas* self) {
    self->Clear();
}
extern "C" void __c__ImFontAtlas_Build_12(ImFontAtlas* self, bool *ret) {
    *ret = (bool )self->Build();
}
extern "C" void __c__ImFontAtlas_GetTexDataAsAlpha8_13(ImFontAtlas* self, uint8_t * * _out_pixels, int32_t * _out_width, int32_t * _out_height, int32_t * _out_bytes_per_pixel) {
    self->GetTexDataAsAlpha8(_out_pixels, _out_width, _out_height, _out_bytes_per_pixel);
}
extern "C" void __c__ImFontAtlas_GetTexDataAsRGBA32_14(ImFontAtlas* self, uint8_t * * _out_pixels, int32_t * _out_width, int32_t * _out_height, int32_t * _out_bytes_per_pixel) {
    self->GetTexDataAsRGBA32(_out_pixels, _out_width, _out_height, _out_bytes_per_pixel);
}
extern "C" void __c__ImFontAtlas_IsBuilt_15(ImFontAtlas* self, bool *ret) {
    *ret = (bool )self->IsBuilt();
}
extern "C" void __c__ImFontAtlas_SetTexID_16(ImFontAtlas* self, void * _id) {
    self->SetTexID(_id);
}
extern "C" void __c__ImFontAtlas_GetGlyphRangesDefault_17(ImFontAtlas* self, const ImWchar * *ret) {
    *ret = (const ImWchar * )self->GetGlyphRangesDefault();
}
extern "C" void __c__ImFontAtlas_GetGlyphRangesKorean_18(ImFontAtlas* self, const ImWchar * *ret) {
    *ret = (const ImWchar * )self->GetGlyphRangesKorean();
}
extern "C" void __c__ImFontAtlas_GetGlyphRangesJapanese_19(ImFontAtlas* self, const ImWchar * *ret) {
    *ret = (const ImWchar * )self->GetGlyphRangesJapanese();
}
extern "C" void __c__ImFontAtlas_GetGlyphRangesChineseFull_20(ImFontAtlas* self, const ImWchar * *ret) {
    *ret = (const ImWchar * )self->GetGlyphRangesChineseFull();
}
extern "C" void __c__ImFontAtlas_GetGlyphRangesChineseSimplifiedCommon_21(ImFontAtlas* self, const ImWchar * *ret) {
    *ret = (const ImWchar * )self->GetGlyphRangesChineseSimplifiedCommon();
}
extern "C" void __c__ImFontAtlas_GetGlyphRangesCyrillic_22(ImFontAtlas* self, const ImWchar * *ret) {
    *ret = (const ImWchar * )self->GetGlyphRangesCyrillic();
}
extern "C" void __c__ImFontAtlas_GetGlyphRangesThai_23(ImFontAtlas* self, const ImWchar * *ret) {
    *ret = (const ImWchar * )self->GetGlyphRangesThai();
}
extern "C" void __c__ImFontAtlas_GetGlyphRangesVietnamese_24(ImFontAtlas* self, const ImWchar * *ret) {
    *ret = (const ImWchar * )self->GetGlyphRangesVietnamese();
}
extern "C" void __c__ImFontAtlas_AddCustomRectRegular_25(ImFontAtlas* self, int32_t *ret, uint32_t _id, int32_t _width, int32_t _height) {
    *ret = (int32_t )self->AddCustomRectRegular(_id, _width, _height);
}
extern "C" void __c__ImFontAtlas_AddCustomRectFontGlyph_26(ImFontAtlas* self, int32_t *ret, ImFont * _font, uint16_t _id, int32_t _width, int32_t _height, float _advance_x, ImVec2 * _offset) {
    *ret = (int32_t )self->AddCustomRectFontGlyph(_font, _id, _width, _height, _advance_x, *_offset);
}
extern "C" void __c__ImFontAtlas_GetCustomRectByIndex_27(ImFontAtlas* self, ImFontAtlasCustomRect * *ret, int32_t _index) {
    *ret = (ImFontAtlasCustomRect * )self->GetCustomRectByIndex(_index);
}
extern "C" void __c__ImFontAtlas_CalcCustomRectUV_28(ImFontAtlas* self, ImFontAtlasCustomRect * _rect, ImVec2 * _out_uv_min, ImVec2 * _out_uv_max) {
    self->CalcCustomRectUV(_rect, _out_uv_min, _out_uv_max);
}
extern "C" void __c__ImFontAtlas_GetMouseCursorTexData_29(ImFontAtlas* self, bool *ret, int32_t _cursor, ImVec2 * _out_offset, ImVec2 * _out_size, ImVec2 * _out_uv_border, ImVec2 * _out_uv_fill) {
    *ret = (bool )self->GetMouseCursorTexData(_cursor, _out_offset, _out_size, _out_uv_border, _out_uv_fill);
}
extern "C" void __c__ImFont_new_17(ImFont* self) {
    new (self) ImFont();
}
extern "C" void __c__ImFont_dtor(ImFont* self) {
    self->~ImFont();
}
extern "C" void __c__ImFont_FindGlyph_19(ImFont* self, ImFontGlyph * *ret, uint16_t _c) {
    *ret = (ImFontGlyph * )self->FindGlyph(_c);
}
extern "C" void __c__ImFont_FindGlyphNoFallback_20(ImFont* self, ImFontGlyph * *ret, uint16_t _c) {
    *ret = (ImFontGlyph * )self->FindGlyphNoFallback(_c);
}
extern "C" void __c__ImFont_GetCharAdvance_21(ImFont* self, float *ret, uint16_t _c) {
    *ret = (float )self->GetCharAdvance(_c);
}
extern "C" void __c__ImFont_IsLoaded_22(ImFont* self, bool *ret) {
    *ret = (bool )self->IsLoaded();
}
extern "C" void __c__ImFont_GetDebugName_23(ImFont* self, char * *ret) {
    *ret = (char * )self->GetDebugName();
}
extern "C" void __c__ImFont_CalcTextSizeA_24(ImFont* self, ImVec2 *ret, float _size, float _max_width, float _wrap_width, char * _text_begin, char * _text_end, const char * * _remaining) {
    *ret = (ImVec2 )self->CalcTextSizeA(_size, _max_width, _wrap_width, _text_begin, _text_end, _remaining);
}
extern "C" void __c__ImFont_CalcWordWrapPositionA_25(ImFont* self, char * *ret, float _scale, char * _text, char * _text_end, float _wrap_width) {
    *ret = (char * )self->CalcWordWrapPositionA(_scale, _text, _text_end, _wrap_width);
}
extern "C" void __c__ImFont_RenderChar_26(ImFont* self, ImDrawList * _draw_list, float _size, ImVec2* _pos, uint32_t _col, uint16_t _c) {
    self->RenderChar(_draw_list, _size, *_pos, _col, _c);
}
extern "C" void __c__ImFont_RenderText_27(ImFont* self, ImDrawList * _draw_list, float _size, ImVec2* _pos, uint32_t _col, ImVec4 * _clip_rect, char * _text_begin, char * _text_end, float _wrap_width, bool _cpu_fine_clip) {
    self->RenderText(_draw_list, _size, *_pos, _col, *_clip_rect, _text_begin, _text_end, _wrap_width, _cpu_fine_clip);
}
extern "C" void __c__ImFont_BuildLookupTable_28(ImFont* self) {
    self->BuildLookupTable();
}
extern "C" void __c__ImFont_ClearOutputData_29(ImFont* self) {
    self->ClearOutputData();
}
extern "C" void __c__ImFont_GrowIndex_30(ImFont* self, int32_t _new_size) {
    self->GrowIndex(_new_size);
}
extern "C" void __c__ImFont_AddGlyph_31(ImFont* self, uint16_t _c, float _x0, float _y0, float _x1, float _y1, float _u0, float _v0, float _u1, float _v1, float _advance_x) {
    self->AddGlyph(_c, _x0, _y0, _x1, _y1, _u0, _v0, _u1, _v1, _advance_x);
}
extern "C" void __c__ImFont_AddRemapChar_32(ImFont* self, uint16_t _dst, uint16_t _src, bool _overwrite_dst) {
    self->AddRemapChar(_dst, _src, _overwrite_dst);
}
extern "C" void __c__ImFont_SetFallbackChar_33(ImFont* self, uint16_t _c) {
    self->SetFallbackChar(_c);
}
extern "C" void __c__ImGuiPlatformMonitor_new_5(ImGuiPlatformMonitor* self) {
    new (self) ImGuiPlatformMonitor();
}
extern "C" void __c__ImGuiPlatformIO_new_27(ImGuiPlatformIO* self) {
    new (self) ImGuiPlatformIO();
}
extern "C" void __c__ImGuiViewport_new_14(ImGuiViewport* self) {
    new (self) ImGuiViewport();
}
extern "C" void __c__ImGuiViewport_dtor(ImGuiViewport* self) {
    self->~ImGuiViewport();
}
extern "C" void __c__CreateContext(ImGuiContext * *ret, ImFontAtlas * _shared_font_atlas) {
    *ret = (ImGuiContext * )ImGui::CreateContext(_shared_font_atlas);
}
extern "C" void __c__DestroyContext(ImGuiContext * _ctx) {
    ImGui::DestroyContext(_ctx);
}
extern "C" void __c__GetCurrentContext(ImGuiContext * *ret) {
    *ret = (ImGuiContext * )ImGui::GetCurrentContext();
}
extern "C" void __c__SetCurrentContext(ImGuiContext * _ctx) {
    ImGui::SetCurrentContext(_ctx);
}
extern "C" void __c__DebugCheckVersionAndDataLayout(bool *ret, char * _version_str, uint64_t _sz_io, uint64_t _sz_style, uint64_t _sz_vec2, uint64_t _sz_vec4, uint64_t _sz_drawvert, uint64_t _sz_drawidx) {
    *ret = (bool )ImGui::DebugCheckVersionAndDataLayout(_version_str, _sz_io, _sz_style, _sz_vec2, _sz_vec4, _sz_drawvert, _sz_drawidx);
}
extern "C" void __c__GetIO(ImGuiIO * *ret) {
    *ret = (ImGuiIO * )&ImGui::GetIO();
}
extern "C" void __c__GetStyle(ImGuiStyle * *ret) {
    *ret = (ImGuiStyle * )&ImGui::GetStyle();
}
extern "C" void __c__NewFrame() {
    ImGui::NewFrame();
}
extern "C" void __c__EndFrame() {
    ImGui::EndFrame();
}
extern "C" void __c__Render() {
    ImGui::Render();
}
extern "C" void __c__GetDrawData(ImDrawData * *ret) {
    *ret = (ImDrawData * )ImGui::GetDrawData();
}
extern "C" void __c__ShowDemoWindow(bool * _p_open) {
    ImGui::ShowDemoWindow(_p_open);
}
extern "C" void __c__ShowAboutWindow(bool * _p_open) {
    ImGui::ShowAboutWindow(_p_open);
}
extern "C" void __c__ShowMetricsWindow(bool * _p_open) {
    ImGui::ShowMetricsWindow(_p_open);
}
extern "C" void __c__ShowStyleEditor(ImGuiStyle * _ref) {
    ImGui::ShowStyleEditor(_ref);
}
extern "C" void __c__ShowStyleSelector(bool *ret, char * _label) {
    *ret = (bool )ImGui::ShowStyleSelector(_label);
}
extern "C" void __c__ShowFontSelector(char * _label) {
    ImGui::ShowFontSelector(_label);
}
extern "C" void __c__ShowUserGuide() {
    ImGui::ShowUserGuide();
}
extern "C" void __c__GetVersion(char * *ret) {
    *ret = (char * )ImGui::GetVersion();
}
extern "C" void __c__StyleColorsDark(ImGuiStyle * _dst) {
    ImGui::StyleColorsDark(_dst);
}
extern "C" void __c__StyleColorsClassic(ImGuiStyle * _dst) {
    ImGui::StyleColorsClassic(_dst);
}
extern "C" void __c__StyleColorsLight(ImGuiStyle * _dst) {
    ImGui::StyleColorsLight(_dst);
}
extern "C" void __c__Begin(bool *ret, char * _name, bool * _p_open, int32_t _flags) {
    *ret = (bool )ImGui::Begin(_name, _p_open, _flags);
}
extern "C" void __c__End() {
    ImGui::End();
}
extern "C" void __c__BeginChild(bool *ret, char * _str_id, ImVec2 * _size, bool _border, int32_t _flags) {
    *ret = (bool )ImGui::BeginChild(_str_id, *_size, _border, _flags);
}
extern "C" void __c__BeginChild_2(bool *ret, uint32_t _id, ImVec2 * _size, bool _border, int32_t _flags) {
    *ret = (bool )ImGui::BeginChild(_id, *_size, _border, _flags);
}
extern "C" void __c__EndChild() {
    ImGui::EndChild();
}
extern "C" void __c__IsWindowAppearing(bool *ret) {
    *ret = (bool )ImGui::IsWindowAppearing();
}
extern "C" void __c__IsWindowCollapsed(bool *ret) {
    *ret = (bool )ImGui::IsWindowCollapsed();
}
extern "C" void __c__IsWindowFocused(bool *ret, int32_t _flags) {
    *ret = (bool )ImGui::IsWindowFocused(_flags);
}
extern "C" void __c__IsWindowHovered(bool *ret, int32_t _flags) {
    *ret = (bool )ImGui::IsWindowHovered(_flags);
}
extern "C" void __c__GetWindowDrawList(ImDrawList * *ret) {
    *ret = (ImDrawList * )ImGui::GetWindowDrawList();
}
extern "C" void __c__GetWindowDpiScale(float *ret) {
    *ret = (float )ImGui::GetWindowDpiScale();
}
extern "C" void __c__GetWindowViewport(ImGuiViewport * *ret) {
    *ret = (ImGuiViewport * )ImGui::GetWindowViewport();
}
extern "C" void __c__GetWindowPos(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetWindowPos();
}
extern "C" void __c__GetWindowSize(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetWindowSize();
}
extern "C" void __c__GetWindowWidth(float *ret) {
    *ret = (float )ImGui::GetWindowWidth();
}
extern "C" void __c__GetWindowHeight(float *ret) {
    *ret = (float )ImGui::GetWindowHeight();
}
extern "C" void __c__SetNextWindowPos(ImVec2 * _pos, int32_t _cond, ImVec2 * _pivot) {
    ImGui::SetNextWindowPos(*_pos, _cond, *_pivot);
}
extern "C" void __c__SetNextWindowSize(ImVec2 * _size, int32_t _cond) {
    ImGui::SetNextWindowSize(*_size, _cond);
}
extern "C" void __c__SetNextWindowSizeConstraints(ImVec2 * _size_min, ImVec2 * _size_max, ImGuiSizeCallback _custom_callback, void * _custom_callback_data) {
    ImGui::SetNextWindowSizeConstraints(*_size_min, *_size_max, _custom_callback, _custom_callback_data);
}
extern "C" void __c__SetNextWindowContentSize(ImVec2 * _size) {
    ImGui::SetNextWindowContentSize(*_size);
}
extern "C" void __c__SetNextWindowCollapsed(bool _collapsed, int32_t _cond) {
    ImGui::SetNextWindowCollapsed(_collapsed, _cond);
}
extern "C" void __c__SetNextWindowFocus() {
    ImGui::SetNextWindowFocus();
}
extern "C" void __c__SetNextWindowBgAlpha(float _alpha) {
    ImGui::SetNextWindowBgAlpha(_alpha);
}
extern "C" void __c__SetNextWindowViewport(uint32_t _viewport_id) {
    ImGui::SetNextWindowViewport(_viewport_id);
}
extern "C" void __c__SetWindowPos(ImVec2 * _pos, int32_t _cond) {
    ImGui::SetWindowPos(*_pos, _cond);
}
extern "C" void __c__SetWindowSize(ImVec2 * _size, int32_t _cond) {
    ImGui::SetWindowSize(*_size, _cond);
}
extern "C" void __c__SetWindowCollapsed(bool _collapsed, int32_t _cond) {
    ImGui::SetWindowCollapsed(_collapsed, _cond);
}
extern "C" void __c__SetWindowFocus() {
    ImGui::SetWindowFocus();
}
extern "C" void __c__SetWindowFontScale(float _scale) {
    ImGui::SetWindowFontScale(_scale);
}
extern "C" void __c__SetWindowPos_2(char * _name, ImVec2 * _pos, int32_t _cond) {
    ImGui::SetWindowPos(_name, *_pos, _cond);
}
extern "C" void __c__SetWindowSize_2(char * _name, ImVec2 * _size, int32_t _cond) {
    ImGui::SetWindowSize(_name, *_size, _cond);
}
extern "C" void __c__SetWindowCollapsed_2(char * _name, bool _collapsed, int32_t _cond) {
    ImGui::SetWindowCollapsed(_name, _collapsed, _cond);
}
extern "C" void __c__SetWindowFocus_2(char * _name) {
    ImGui::SetWindowFocus(_name);
}
extern "C" void __c__GetContentRegionMax(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetContentRegionMax();
}
extern "C" void __c__GetContentRegionAvail(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetContentRegionAvail();
}
extern "C" void __c__GetWindowContentRegionMin(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetWindowContentRegionMin();
}
extern "C" void __c__GetWindowContentRegionMax(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetWindowContentRegionMax();
}
extern "C" void __c__GetWindowContentRegionWidth(float *ret) {
    *ret = (float )ImGui::GetWindowContentRegionWidth();
}
extern "C" void __c__GetScrollX(float *ret) {
    *ret = (float )ImGui::GetScrollX();
}
extern "C" void __c__GetScrollY(float *ret) {
    *ret = (float )ImGui::GetScrollY();
}
extern "C" void __c__GetScrollMaxX(float *ret) {
    *ret = (float )ImGui::GetScrollMaxX();
}
extern "C" void __c__GetScrollMaxY(float *ret) {
    *ret = (float )ImGui::GetScrollMaxY();
}
extern "C" void __c__SetScrollX(float _scroll_x) {
    ImGui::SetScrollX(_scroll_x);
}
extern "C" void __c__SetScrollY(float _scroll_y) {
    ImGui::SetScrollY(_scroll_y);
}
extern "C" void __c__SetScrollHereX(float _center_x_ratio) {
    ImGui::SetScrollHereX(_center_x_ratio);
}
extern "C" void __c__SetScrollHereY(float _center_y_ratio) {
    ImGui::SetScrollHereY(_center_y_ratio);
}
extern "C" void __c__SetScrollFromPosX(float _local_x, float _center_x_ratio) {
    ImGui::SetScrollFromPosX(_local_x, _center_x_ratio);
}
extern "C" void __c__SetScrollFromPosY(float _local_y, float _center_y_ratio) {
    ImGui::SetScrollFromPosY(_local_y, _center_y_ratio);
}
extern "C" void __c__PushFont(ImFont * _font) {
    ImGui::PushFont(_font);
}
extern "C" void __c__PopFont() {
    ImGui::PopFont();
}
extern "C" void __c__PushStyleColor(int32_t _idx, uint32_t _col) {
    ImGui::PushStyleColor(_idx, _col);
}
extern "C" void __c__PushStyleColor_2(int32_t _idx, ImVec4 * _col) {
    ImGui::PushStyleColor(_idx, *_col);
}
extern "C" void __c__PopStyleColor(int32_t _count) {
    ImGui::PopStyleColor(_count);
}
extern "C" void __c__PushStyleVar(int32_t _idx, float _val) {
    ImGui::PushStyleVar(_idx, _val);
}
extern "C" void __c__PushStyleVar_2(int32_t _idx, ImVec2 * _val) {
    ImGui::PushStyleVar(_idx, *_val);
}
extern "C" void __c__PopStyleVar(int32_t _count) {
    ImGui::PopStyleVar(_count);
}
extern "C" void __c__GetStyleColorVec4(ImVec4 * *ret, int32_t _idx) {
    *ret = (ImVec4 * )&ImGui::GetStyleColorVec4(_idx);
}
extern "C" void __c__GetFont(ImFont * *ret) {
    *ret = (ImFont * )ImGui::GetFont();
}
extern "C" void __c__GetFontSize(float *ret) {
    *ret = (float )ImGui::GetFontSize();
}
extern "C" void __c__GetFontTexUvWhitePixel(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetFontTexUvWhitePixel();
}
extern "C" void __c__GetColorU32(ImU32 *ret, int32_t _idx, float _alpha_mul) {
    *ret = (uint32_t )ImGui::GetColorU32(_idx, _alpha_mul);
}
extern "C" void __c__GetColorU32_2(ImU32 *ret, ImVec4 * _col) {
    *ret = (uint32_t )ImGui::GetColorU32(*_col);
}
extern "C" void __c__GetColorU32_3(ImU32 *ret, uint32_t _col) {
    *ret = (uint32_t )ImGui::GetColorU32(_col);
}
extern "C" void __c__PushItemWidth(float _item_width) {
    ImGui::PushItemWidth(_item_width);
}
extern "C" void __c__PopItemWidth() {
    ImGui::PopItemWidth();
}
extern "C" void __c__SetNextItemWidth(float _item_width) {
    ImGui::SetNextItemWidth(_item_width);
}
extern "C" void __c__CalcItemWidth(float *ret) {
    *ret = (float )ImGui::CalcItemWidth();
}
extern "C" void __c__PushTextWrapPos(float _wrap_local_pos_x) {
    ImGui::PushTextWrapPos(_wrap_local_pos_x);
}
extern "C" void __c__PopTextWrapPos() {
    ImGui::PopTextWrapPos();
}
extern "C" void __c__PushAllowKeyboardFocus(bool _allow_keyboard_focus) {
    ImGui::PushAllowKeyboardFocus(_allow_keyboard_focus);
}
extern "C" void __c__PopAllowKeyboardFocus() {
    ImGui::PopAllowKeyboardFocus();
}
extern "C" void __c__PushButtonRepeat(bool _repeat) {
    ImGui::PushButtonRepeat(_repeat);
}
extern "C" void __c__PopButtonRepeat() {
    ImGui::PopButtonRepeat();
}
extern "C" void __c__Separator() {
    ImGui::Separator();
}
extern "C" void __c__SameLine(float _offset_from_start_x, float _spacing) {
    ImGui::SameLine(_offset_from_start_x, _spacing);
}
extern "C" void __c__NewLine() {
    ImGui::NewLine();
}
extern "C" void __c__Spacing() {
    ImGui::Spacing();
}
extern "C" void __c__Dummy(ImVec2 * _size) {
    ImGui::Dummy(*_size);
}
extern "C" void __c__Indent(float _indent_w) {
    ImGui::Indent(_indent_w);
}
extern "C" void __c__Unindent(float _indent_w) {
    ImGui::Unindent(_indent_w);
}
extern "C" void __c__BeginGroup() {
    ImGui::BeginGroup();
}
extern "C" void __c__EndGroup() {
    ImGui::EndGroup();
}
extern "C" void __c__GetCursorPos(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetCursorPos();
}
extern "C" void __c__GetCursorPosX(float *ret) {
    *ret = (float )ImGui::GetCursorPosX();
}
extern "C" void __c__GetCursorPosY(float *ret) {
    *ret = (float )ImGui::GetCursorPosY();
}
extern "C" void __c__SetCursorPos(ImVec2 * _local_pos) {
    ImGui::SetCursorPos(*_local_pos);
}
extern "C" void __c__SetCursorPosX(float _local_x) {
    ImGui::SetCursorPosX(_local_x);
}
extern "C" void __c__SetCursorPosY(float _local_y) {
    ImGui::SetCursorPosY(_local_y);
}
extern "C" void __c__GetCursorStartPos(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetCursorStartPos();
}
extern "C" void __c__GetCursorScreenPos(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetCursorScreenPos();
}
extern "C" void __c__SetCursorScreenPos(ImVec2 * _pos) {
    ImGui::SetCursorScreenPos(*_pos);
}
extern "C" void __c__AlignTextToFramePadding() {
    ImGui::AlignTextToFramePadding();
}
extern "C" void __c__GetTextLineHeight(float *ret) {
    *ret = (float )ImGui::GetTextLineHeight();
}
extern "C" void __c__GetTextLineHeightWithSpacing(float *ret) {
    *ret = (float )ImGui::GetTextLineHeightWithSpacing();
}
extern "C" void __c__GetFrameHeight(float *ret) {
    *ret = (float )ImGui::GetFrameHeight();
}
extern "C" void __c__GetFrameHeightWithSpacing(float *ret) {
    *ret = (float )ImGui::GetFrameHeightWithSpacing();
}
extern "C" void __c__PushID(char * _str_id) {
    ImGui::PushID(_str_id);
}
extern "C" void __c__PushID_2(char * _str_id_begin, char * _str_id_end) {
    ImGui::PushID(_str_id_begin, _str_id_end);
}
extern "C" void __c__PushID_3(void * _ptr_id) {
    ImGui::PushID(_ptr_id);
}
extern "C" void __c__PushID_4(int32_t _int_id) {
    ImGui::PushID(_int_id);
}
extern "C" void __c__PopID() {
    ImGui::PopID();
}
extern "C" void __c__GetID(ImGuiID *ret, char * _str_id) {
    *ret = (uint32_t )ImGui::GetID(_str_id);
}
extern "C" void __c__GetID_2(ImGuiID *ret, char * _str_id_begin, char * _str_id_end) {
    *ret = (uint32_t )ImGui::GetID(_str_id_begin, _str_id_end);
}
extern "C" void __c__GetID_3(ImGuiID *ret, void * _ptr_id) {
    *ret = (uint32_t )ImGui::GetID(_ptr_id);
}
extern "C" void __c__TextUnformatted(char * _text, char * _text_end) {
    ImGui::TextUnformatted(_text, _text_end);
}
extern "C" void __c__Text(char * _fmt) {
    ImGui::Text(_fmt);
}
extern "C" void __c__TextV(char * _fmt, char * _args) {
    ImGui::TextV(_fmt, _args);
}
extern "C" void __c__TextColored(ImVec4 * _col, char * _fmt) {
    ImGui::TextColored(*_col, _fmt);
}
extern "C" void __c__TextColoredV(ImVec4 * _col, char * _fmt, char * _args) {
    ImGui::TextColoredV(*_col, _fmt, _args);
}
extern "C" void __c__TextDisabled(char * _fmt) {
    ImGui::TextDisabled(_fmt);
}
extern "C" void __c__TextDisabledV(char * _fmt, char * _args) {
    ImGui::TextDisabledV(_fmt, _args);
}
extern "C" void __c__TextWrapped(char * _fmt) {
    ImGui::TextWrapped(_fmt);
}
extern "C" void __c__TextWrappedV(char * _fmt, char * _args) {
    ImGui::TextWrappedV(_fmt, _args);
}
extern "C" void __c__LabelText(char * _label, char * _fmt) {
    ImGui::LabelText(_label, _fmt);
}
extern "C" void __c__LabelTextV(char * _label, char * _fmt, char * _args) {
    ImGui::LabelTextV(_label, _fmt, _args);
}
extern "C" void __c__BulletText(char * _fmt) {
    ImGui::BulletText(_fmt);
}
extern "C" void __c__BulletTextV(char * _fmt, char * _args) {
    ImGui::BulletTextV(_fmt, _args);
}
extern "C" void __c__Button(bool *ret, char * _label, ImVec2 * _size) {
    *ret = (bool )ImGui::Button(_label, *_size);
}
extern "C" void __c__SmallButton(bool *ret, char * _label) {
    *ret = (bool )ImGui::SmallButton(_label);
}
extern "C" void __c__InvisibleButton(bool *ret, char * _str_id, ImVec2 * _size) {
    *ret = (bool )ImGui::InvisibleButton(_str_id, *_size);
}
extern "C" void __c__ArrowButton(bool *ret, char * _str_id, int32_t _dir) {
    *ret = (bool )ImGui::ArrowButton(_str_id, _dir);
}
extern "C" void __c__Image(void * _user_texture_id, ImVec2 * _size, ImVec2 * _uv0, ImVec2 * _uv1, ImVec4 * _tint_col, ImVec4 * _border_col) {
    ImGui::Image(_user_texture_id, *_size, *_uv0, *_uv1, *_tint_col, *_border_col);
}
extern "C" void __c__ImageButton(bool *ret, void * _user_texture_id, ImVec2 * _size, ImVec2 * _uv0, ImVec2 * _uv1, int32_t _frame_padding, ImVec4 * _bg_col, ImVec4 * _tint_col) {
    *ret = (bool )ImGui::ImageButton(_user_texture_id, *_size, *_uv0, *_uv1, _frame_padding, *_bg_col, *_tint_col);
}
extern "C" void __c__Checkbox(bool *ret, char * _label, bool * _v) {
    *ret = (bool )ImGui::Checkbox(_label, _v);
}
extern "C" void __c__CheckboxFlags(bool *ret, char * _label, uint32_t * _flags, uint32_t _flags_value) {
    *ret = (bool )ImGui::CheckboxFlags(_label, _flags, _flags_value);
}
extern "C" void __c__RadioButton(bool *ret, char * _label, bool _active) {
    *ret = (bool )ImGui::RadioButton(_label, _active);
}
extern "C" void __c__RadioButton_2(bool *ret, char * _label, int32_t * _v, int32_t _v_button) {
    *ret = (bool )ImGui::RadioButton(_label, _v, _v_button);
}
extern "C" void __c__ProgressBar(float _fraction, ImVec2 * _size_arg, char * _overlay) {
    ImGui::ProgressBar(_fraction, *_size_arg, _overlay);
}
extern "C" void __c__Bullet() {
    ImGui::Bullet();
}
extern "C" void __c__BeginCombo(bool *ret, char * _label, char * _preview_value, int32_t _flags) {
    *ret = (bool )ImGui::BeginCombo(_label, _preview_value, _flags);
}
extern "C" void __c__EndCombo() {
    ImGui::EndCombo();
}
extern "C" void __c__Combo(bool *ret, char * _label, int32_t * _current_item, char * * _items, int32_t _items_count, int32_t _popup_max_height_in_items) {
    *ret = (bool )ImGui::Combo(_label, _current_item, _items, _items_count, _popup_max_height_in_items);
}
extern "C" void __c__Combo_2(bool *ret, char * _label, int32_t * _current_item, char * _items_separated_by_zeros, int32_t _popup_max_height_in_items) {
    *ret = (bool )ImGui::Combo(_label, _current_item, _items_separated_by_zeros, _popup_max_height_in_items);
}
extern "C" void __c__Combo_3(bool *ret, char * _label, int32_t * _current_item, bool (*_items_getter)(void * , int32_t , const char * * ), void * _data, int32_t _items_count, int32_t _popup_max_height_in_items) {
    *ret = (bool )ImGui::Combo(_label, _current_item, _items_getter, _data, _items_count, _popup_max_height_in_items);
}
extern "C" void __c__DragFloat(bool *ret, char * _label, float * _v, float _v_speed, float _v_min, float _v_max, char * _format, float _power) {
    *ret = (bool )ImGui::DragFloat(_label, _v, _v_speed, _v_min, _v_max, _format, _power);
}
extern "C" void __c__DragFloat2(bool *ret, char * _label, float * _v, float _v_speed, float _v_min, float _v_max, char * _format, float _power) {
    *ret = (bool )ImGui::DragFloat2(_label, _v, _v_speed, _v_min, _v_max, _format, _power);
}
extern "C" void __c__DragFloat3(bool *ret, char * _label, float * _v, float _v_speed, float _v_min, float _v_max, char * _format, float _power) {
    *ret = (bool )ImGui::DragFloat3(_label, _v, _v_speed, _v_min, _v_max, _format, _power);
}
extern "C" void __c__DragFloat4(bool *ret, char * _label, float * _v, float _v_speed, float _v_min, float _v_max, char * _format, float _power) {
    *ret = (bool )ImGui::DragFloat4(_label, _v, _v_speed, _v_min, _v_max, _format, _power);
}
extern "C" void __c__DragFloatRange2(bool *ret, char * _label, float * _v_current_min, float * _v_current_max, float _v_speed, float _v_min, float _v_max, char * _format, char * _format_max, float _power) {
    *ret = (bool )ImGui::DragFloatRange2(_label, _v_current_min, _v_current_max, _v_speed, _v_min, _v_max, _format, _format_max, _power);
}
extern "C" void __c__DragInt(bool *ret, char * _label, int32_t * _v, float _v_speed, int32_t _v_min, int32_t _v_max, char * _format) {
    *ret = (bool )ImGui::DragInt(_label, _v, _v_speed, _v_min, _v_max, _format);
}
extern "C" void __c__DragInt2(bool *ret, char * _label, int32_t * _v, float _v_speed, int32_t _v_min, int32_t _v_max, char * _format) {
    *ret = (bool )ImGui::DragInt2(_label, _v, _v_speed, _v_min, _v_max, _format);
}
extern "C" void __c__DragInt3(bool *ret, char * _label, int32_t * _v, float _v_speed, int32_t _v_min, int32_t _v_max, char * _format) {
    *ret = (bool )ImGui::DragInt3(_label, _v, _v_speed, _v_min, _v_max, _format);
}
extern "C" void __c__DragInt4(bool *ret, char * _label, int32_t * _v, float _v_speed, int32_t _v_min, int32_t _v_max, char * _format) {
    *ret = (bool )ImGui::DragInt4(_label, _v, _v_speed, _v_min, _v_max, _format);
}
extern "C" void __c__DragIntRange2(bool *ret, char * _label, int32_t * _v_current_min, int32_t * _v_current_max, float _v_speed, int32_t _v_min, int32_t _v_max, char * _format, char * _format_max) {
    *ret = (bool )ImGui::DragIntRange2(_label, _v_current_min, _v_current_max, _v_speed, _v_min, _v_max, _format, _format_max);
}
extern "C" void __c__DragScalar(bool *ret, char * _label, int32_t _data_type, void * _p_data, float _v_speed, void * _p_min, void * _p_max, char * _format, float _power) {
    *ret = (bool )ImGui::DragScalar(_label, _data_type, _p_data, _v_speed, _p_min, _p_max, _format, _power);
}
extern "C" void __c__DragScalarN(bool *ret, char * _label, int32_t _data_type, void * _p_data, int32_t _components, float _v_speed, void * _p_min, void * _p_max, char * _format, float _power) {
    *ret = (bool )ImGui::DragScalarN(_label, _data_type, _p_data, _components, _v_speed, _p_min, _p_max, _format, _power);
}
extern "C" void __c__SliderFloat(bool *ret, char * _label, float * _v, float _v_min, float _v_max, char * _format, float _power) {
    *ret = (bool )ImGui::SliderFloat(_label, _v, _v_min, _v_max, _format, _power);
}
extern "C" void __c__SliderFloat2(bool *ret, char * _label, float * _v, float _v_min, float _v_max, char * _format, float _power) {
    *ret = (bool )ImGui::SliderFloat2(_label, _v, _v_min, _v_max, _format, _power);
}
extern "C" void __c__SliderFloat3(bool *ret, char * _label, float * _v, float _v_min, float _v_max, char * _format, float _power) {
    *ret = (bool )ImGui::SliderFloat3(_label, _v, _v_min, _v_max, _format, _power);
}
extern "C" void __c__SliderFloat4(bool *ret, char * _label, float * _v, float _v_min, float _v_max, char * _format, float _power) {
    *ret = (bool )ImGui::SliderFloat4(_label, _v, _v_min, _v_max, _format, _power);
}
extern "C" void __c__SliderAngle(bool *ret, char * _label, float * _v_rad, float _v_degrees_min, float _v_degrees_max, char * _format) {
    *ret = (bool )ImGui::SliderAngle(_label, _v_rad, _v_degrees_min, _v_degrees_max, _format);
}
extern "C" void __c__SliderInt(bool *ret, char * _label, int32_t * _v, int32_t _v_min, int32_t _v_max, char * _format) {
    *ret = (bool )ImGui::SliderInt(_label, _v, _v_min, _v_max, _format);
}
extern "C" void __c__SliderInt2(bool *ret, char * _label, int32_t * _v, int32_t _v_min, int32_t _v_max, char * _format) {
    *ret = (bool )ImGui::SliderInt2(_label, _v, _v_min, _v_max, _format);
}
extern "C" void __c__SliderInt3(bool *ret, char * _label, int32_t * _v, int32_t _v_min, int32_t _v_max, char * _format) {
    *ret = (bool )ImGui::SliderInt3(_label, _v, _v_min, _v_max, _format);
}
extern "C" void __c__SliderInt4(bool *ret, char * _label, int32_t * _v, int32_t _v_min, int32_t _v_max, char * _format) {
    *ret = (bool )ImGui::SliderInt4(_label, _v, _v_min, _v_max, _format);
}
extern "C" void __c__SliderScalar(bool *ret, char * _label, int32_t _data_type, void * _p_data, void * _p_min, void * _p_max, char * _format, float _power) {
    *ret = (bool )ImGui::SliderScalar(_label, _data_type, _p_data, _p_min, _p_max, _format, _power);
}
extern "C" void __c__SliderScalarN(bool *ret, char * _label, int32_t _data_type, void * _p_data, int32_t _components, void * _p_min, void * _p_max, char * _format, float _power) {
    *ret = (bool )ImGui::SliderScalarN(_label, _data_type, _p_data, _components, _p_min, _p_max, _format, _power);
}
extern "C" void __c__VSliderFloat(bool *ret, char * _label, ImVec2 * _size, float * _v, float _v_min, float _v_max, char * _format, float _power) {
    *ret = (bool )ImGui::VSliderFloat(_label, *_size, _v, _v_min, _v_max, _format, _power);
}
extern "C" void __c__VSliderInt(bool *ret, char * _label, ImVec2 * _size, int32_t * _v, int32_t _v_min, int32_t _v_max, char * _format) {
    *ret = (bool )ImGui::VSliderInt(_label, *_size, _v, _v_min, _v_max, _format);
}
extern "C" void __c__VSliderScalar(bool *ret, char * _label, ImVec2 * _size, int32_t _data_type, void * _p_data, void * _p_min, void * _p_max, char * _format, float _power) {
    *ret = (bool )ImGui::VSliderScalar(_label, *_size, _data_type, _p_data, _p_min, _p_max, _format, _power);
}
extern "C" void __c__InputText(bool *ret, char * _label, char * _buf, uint64_t _buf_size, int32_t _flags, ImGuiInputTextCallback _callback, void * _user_data) {
    *ret = (bool )ImGui::InputText(_label, _buf, _buf_size, _flags, _callback, _user_data);
}
extern "C" void __c__InputTextMultiline(bool *ret, char * _label, char * _buf, uint64_t _buf_size, ImVec2 * _size, int32_t _flags, ImGuiInputTextCallback _callback, void * _user_data) {
    *ret = (bool )ImGui::InputTextMultiline(_label, _buf, _buf_size, *_size, _flags, _callback, _user_data);
}
extern "C" void __c__InputTextWithHint(bool *ret, char * _label, char * _hint, char * _buf, uint64_t _buf_size, int32_t _flags, ImGuiInputTextCallback _callback, void * _user_data) {
    *ret = (bool )ImGui::InputTextWithHint(_label, _hint, _buf, _buf_size, _flags, _callback, _user_data);
}
extern "C" void __c__InputFloat(bool *ret, char * _label, float * _v, float _step, float _step_fast, char * _format, int32_t _flags) {
    *ret = (bool )ImGui::InputFloat(_label, _v, _step, _step_fast, _format, _flags);
}
extern "C" void __c__InputFloat2(bool *ret, char * _label, float * _v, char * _format, int32_t _flags) {
    *ret = (bool )ImGui::InputFloat2(_label, _v, _format, _flags);
}
extern "C" void __c__InputFloat3(bool *ret, char * _label, float * _v, char * _format, int32_t _flags) {
    *ret = (bool )ImGui::InputFloat3(_label, _v, _format, _flags);
}
extern "C" void __c__InputFloat4(bool *ret, char * _label, float * _v, char * _format, int32_t _flags) {
    *ret = (bool )ImGui::InputFloat4(_label, _v, _format, _flags);
}
extern "C" void __c__InputInt(bool *ret, char * _label, int32_t * _v, int32_t _step, int32_t _step_fast, int32_t _flags) {
    *ret = (bool )ImGui::InputInt(_label, _v, _step, _step_fast, _flags);
}
extern "C" void __c__InputInt2(bool *ret, char * _label, int32_t * _v, int32_t _flags) {
    *ret = (bool )ImGui::InputInt2(_label, _v, _flags);
}
extern "C" void __c__InputInt3(bool *ret, char * _label, int32_t * _v, int32_t _flags) {
    *ret = (bool )ImGui::InputInt3(_label, _v, _flags);
}
extern "C" void __c__InputInt4(bool *ret, char * _label, int32_t * _v, int32_t _flags) {
    *ret = (bool )ImGui::InputInt4(_label, _v, _flags);
}
extern "C" void __c__InputDouble(bool *ret, char * _label, double * _v, double _step, double _step_fast, char * _format, int32_t _flags) {
    *ret = (bool )ImGui::InputDouble(_label, _v, _step, _step_fast, _format, _flags);
}
extern "C" void __c__InputScalar(bool *ret, char * _label, int32_t _data_type, void * _p_data, void * _p_step, void * _p_step_fast, char * _format, int32_t _flags) {
    *ret = (bool )ImGui::InputScalar(_label, _data_type, _p_data, _p_step, _p_step_fast, _format, _flags);
}
extern "C" void __c__InputScalarN(bool *ret, char * _label, int32_t _data_type, void * _p_data, int32_t _components, void * _p_step, void * _p_step_fast, char * _format, int32_t _flags) {
    *ret = (bool )ImGui::InputScalarN(_label, _data_type, _p_data, _components, _p_step, _p_step_fast, _format, _flags);
}
extern "C" void __c__ColorEdit3(bool *ret, char * _label, float * _col, int32_t _flags) {
    *ret = (bool )ImGui::ColorEdit3(_label, _col, _flags);
}
extern "C" void __c__ColorEdit4(bool *ret, char * _label, float * _col, int32_t _flags) {
    *ret = (bool )ImGui::ColorEdit4(_label, _col, _flags);
}
extern "C" void __c__ColorPicker3(bool *ret, char * _label, float * _col, int32_t _flags) {
    *ret = (bool )ImGui::ColorPicker3(_label, _col, _flags);
}
extern "C" void __c__ColorPicker4(bool *ret, char * _label, float * _col, int32_t _flags, float * _ref_col) {
    *ret = (bool )ImGui::ColorPicker4(_label, _col, _flags, _ref_col);
}
extern "C" void __c__ColorButton(bool *ret, char * _desc_id, ImVec4 * _col, int32_t _flags, ImVec2* _size) {
    *ret = (bool )ImGui::ColorButton(_desc_id, *_col, _flags, *_size);
}
extern "C" void __c__SetColorEditOptions(int32_t _flags) {
    ImGui::SetColorEditOptions(_flags);
}
extern "C" void __c__TreeNode(bool *ret, char * _label) {
    *ret = (bool )ImGui::TreeNode(_label);
}
extern "C" void __c__TreeNode_2(bool *ret, char * _str_id, char * _fmt) {
    *ret = (bool )ImGui::TreeNode(_str_id, _fmt);
}
extern "C" void __c__TreeNode_3(bool *ret, void * _ptr_id, char * _fmt) {
    *ret = (bool )ImGui::TreeNode(_ptr_id, _fmt);
}
extern "C" void __c__TreeNodeV(bool *ret, char * _str_id, char * _fmt, char * _args) {
    *ret = (bool )ImGui::TreeNodeV(_str_id, _fmt, _args);
}
extern "C" void __c__TreeNodeV_2(bool *ret, void * _ptr_id, char * _fmt, char * _args) {
    *ret = (bool )ImGui::TreeNodeV(_ptr_id, _fmt, _args);
}
extern "C" void __c__TreeNodeEx(bool *ret, char * _label, int32_t _flags) {
    *ret = (bool )ImGui::TreeNodeEx(_label, _flags);
}
extern "C" void __c__TreeNodeEx_2(bool *ret, char * _str_id, int32_t _flags, char * _fmt) {
    *ret = (bool )ImGui::TreeNodeEx(_str_id, _flags, _fmt);
}
extern "C" void __c__TreeNodeEx_3(bool *ret, void * _ptr_id, int32_t _flags, char * _fmt) {
    *ret = (bool )ImGui::TreeNodeEx(_ptr_id, _flags, _fmt);
}
extern "C" void __c__TreeNodeExV(bool *ret, char * _str_id, int32_t _flags, char * _fmt, char * _args) {
    *ret = (bool )ImGui::TreeNodeExV(_str_id, _flags, _fmt, _args);
}
extern "C" void __c__TreeNodeExV_2(bool *ret, void * _ptr_id, int32_t _flags, char * _fmt, char * _args) {
    *ret = (bool )ImGui::TreeNodeExV(_ptr_id, _flags, _fmt, _args);
}
extern "C" void __c__TreePush(char * _str_id) {
    ImGui::TreePush(_str_id);
}
extern "C" void __c__TreePush_2(void * _ptr_id) {
    ImGui::TreePush(_ptr_id);
}
extern "C" void __c__TreePop() {
    ImGui::TreePop();
}
extern "C" void __c__GetTreeNodeToLabelSpacing(float *ret) {
    *ret = (float )ImGui::GetTreeNodeToLabelSpacing();
}
extern "C" void __c__CollapsingHeader(bool *ret, char * _label, int32_t _flags) {
    *ret = (bool )ImGui::CollapsingHeader(_label, _flags);
}
extern "C" void __c__CollapsingHeader_2(bool *ret, char * _label, bool * _p_open, int32_t _flags) {
    *ret = (bool )ImGui::CollapsingHeader(_label, _p_open, _flags);
}
extern "C" void __c__SetNextItemOpen(bool _is_open, int32_t _cond) {
    ImGui::SetNextItemOpen(_is_open, _cond);
}
extern "C" void __c__Selectable(bool *ret, char * _label, bool _selected, int32_t _flags, ImVec2 * _size) {
    *ret = (bool )ImGui::Selectable(_label, _selected, _flags, *_size);
}
extern "C" void __c__Selectable_2(bool *ret, char * _label, bool * _p_selected, int32_t _flags, ImVec2 * _size) {
    *ret = (bool )ImGui::Selectable(_label, _p_selected, _flags, *_size);
}
extern "C" void __c__ListBox(bool *ret, char * _label, int32_t * _current_item, char * * _items, int32_t _items_count, int32_t _height_in_items) {
    *ret = (bool )ImGui::ListBox(_label, _current_item, _items, _items_count, _height_in_items);
}
extern "C" void __c__ListBox_2(bool *ret, char * _label, int32_t * _current_item, bool (*_items_getter)(void * , int32_t , const char * * ), void * _data, int32_t _items_count, int32_t _height_in_items) {
    *ret = (bool )ImGui::ListBox(_label, _current_item, _items_getter, _data, _items_count, _height_in_items);
}
extern "C" void __c__ListBoxHeader(bool *ret, char * _label, ImVec2 * _size) {
    *ret = (bool )ImGui::ListBoxHeader(_label, *_size);
}
extern "C" void __c__ListBoxHeader_2(bool *ret, char * _label, int32_t _items_count, int32_t _height_in_items) {
    *ret = (bool )ImGui::ListBoxHeader(_label, _items_count, _height_in_items);
}
extern "C" void __c__ListBoxFooter() {
    ImGui::ListBoxFooter();
}
extern "C" void __c__PlotLines(char * _label, float * _values, int32_t _values_count, int32_t _values_offset, char * _overlay_text, float _scale_min, float _scale_max, ImVec2* _graph_size, int32_t _stride) {
    ImGui::PlotLines(_label, _values, _values_count, _values_offset, _overlay_text, _scale_min, _scale_max, *_graph_size, _stride);
}
extern "C" void __c__PlotLines_2(char * _label, float (*_values_getter)(void * , int32_t ), void * _data, int32_t _values_count, int32_t _values_offset, char * _overlay_text, float _scale_min, float _scale_max, ImVec2* _graph_size) {
    ImGui::PlotLines(_label, _values_getter, _data, _values_count, _values_offset, _overlay_text, _scale_min, _scale_max, *_graph_size);
}
extern "C" void __c__PlotHistogram(char * _label, float * _values, int32_t _values_count, int32_t _values_offset, char * _overlay_text, float _scale_min, float _scale_max, ImVec2* _graph_size, int32_t _stride) {
    ImGui::PlotHistogram(_label, _values, _values_count, _values_offset, _overlay_text, _scale_min, _scale_max, *_graph_size, _stride);
}
extern "C" void __c__PlotHistogram_2(char * _label, float (*_values_getter)(void * , int32_t ), void * _data, int32_t _values_count, int32_t _values_offset, char * _overlay_text, float _scale_min, float _scale_max, ImVec2* _graph_size) {
    ImGui::PlotHistogram(_label, _values_getter, _data, _values_count, _values_offset, _overlay_text, _scale_min, _scale_max, *_graph_size);
}
extern "C" void __c__Value(char * _prefix, bool _b) {
    ImGui::Value(_prefix, _b);
}
extern "C" void __c__Value_2(char * _prefix, int32_t _v) {
    ImGui::Value(_prefix, _v);
}
extern "C" void __c__Value_3(char * _prefix, uint32_t _v) {
    ImGui::Value(_prefix, _v);
}
extern "C" void __c__Value_4(char * _prefix, float _v, char * _float_format) {
    ImGui::Value(_prefix, _v, _float_format);
}
extern "C" void __c__BeginMenuBar(bool *ret) {
    *ret = (bool )ImGui::BeginMenuBar();
}
extern "C" void __c__EndMenuBar() {
    ImGui::EndMenuBar();
}
extern "C" void __c__BeginMainMenuBar(bool *ret) {
    *ret = (bool )ImGui::BeginMainMenuBar();
}
extern "C" void __c__EndMainMenuBar() {
    ImGui::EndMainMenuBar();
}
extern "C" void __c__BeginMenu(bool *ret, char * _label, bool _enabled) {
    *ret = (bool )ImGui::BeginMenu(_label, _enabled);
}
extern "C" void __c__EndMenu() {
    ImGui::EndMenu();
}
extern "C" void __c__MenuItem(bool *ret, char * _label, char * _shortcut, bool _selected, bool _enabled) {
    *ret = (bool )ImGui::MenuItem(_label, _shortcut, _selected, _enabled);
}
extern "C" void __c__MenuItem_2(bool *ret, char * _label, char * _shortcut, bool * _p_selected, bool _enabled) {
    *ret = (bool )ImGui::MenuItem(_label, _shortcut, _p_selected, _enabled);
}
extern "C" void __c__BeginTooltip() {
    ImGui::BeginTooltip();
}
extern "C" void __c__EndTooltip() {
    ImGui::EndTooltip();
}
extern "C" void __c__SetTooltip(char * _fmt) {
    ImGui::SetTooltip(_fmt);
}
extern "C" void __c__SetTooltipV(char * _fmt, char * _args) {
    ImGui::SetTooltipV(_fmt, _args);
}
extern "C" void __c__OpenPopup(char * _str_id) {
    ImGui::OpenPopup(_str_id);
}
extern "C" void __c__BeginPopup(bool *ret, char * _str_id, int32_t _flags) {
    *ret = (bool )ImGui::BeginPopup(_str_id, _flags);
}
extern "C" void __c__BeginPopupContextItem(bool *ret, char * _str_id, int32_t _mouse_button) {
    *ret = (bool )ImGui::BeginPopupContextItem(_str_id, _mouse_button);
}
extern "C" void __c__BeginPopupContextWindow(bool *ret, char * _str_id, int32_t _mouse_button, bool _also_over_items) {
    *ret = (bool )ImGui::BeginPopupContextWindow(_str_id, _mouse_button, _also_over_items);
}
extern "C" void __c__BeginPopupContextVoid(bool *ret, char * _str_id, int32_t _mouse_button) {
    *ret = (bool )ImGui::BeginPopupContextVoid(_str_id, _mouse_button);
}
extern "C" void __c__BeginPopupModal(bool *ret, char * _name, bool * _p_open, int32_t _flags) {
    *ret = (bool )ImGui::BeginPopupModal(_name, _p_open, _flags);
}
extern "C" void __c__EndPopup() {
    ImGui::EndPopup();
}
extern "C" void __c__OpenPopupOnItemClick(bool *ret, char * _str_id, int32_t _mouse_button) {
    *ret = (bool )ImGui::OpenPopupOnItemClick(_str_id, _mouse_button);
}
extern "C" void __c__IsPopupOpen(bool *ret, char * _str_id) {
    *ret = (bool )ImGui::IsPopupOpen(_str_id);
}
extern "C" void __c__CloseCurrentPopup() {
    ImGui::CloseCurrentPopup();
}
extern "C" void __c__Columns(int32_t _count, char * _id, bool _border) {
    ImGui::Columns(_count, _id, _border);
}
extern "C" void __c__NextColumn() {
    ImGui::NextColumn();
}
extern "C" void __c__GetColumnIndex(int32_t *ret) {
    *ret = (int32_t )ImGui::GetColumnIndex();
}
extern "C" void __c__GetColumnWidth(float *ret, int32_t _column_index) {
    *ret = (float )ImGui::GetColumnWidth(_column_index);
}
extern "C" void __c__SetColumnWidth(int32_t _column_index, float _width) {
    ImGui::SetColumnWidth(_column_index, _width);
}
extern "C" void __c__GetColumnOffset(float *ret, int32_t _column_index) {
    *ret = (float )ImGui::GetColumnOffset(_column_index);
}
extern "C" void __c__SetColumnOffset(int32_t _column_index, float _offset_x) {
    ImGui::SetColumnOffset(_column_index, _offset_x);
}
extern "C" void __c__GetColumnsCount(int32_t *ret) {
    *ret = (int32_t )ImGui::GetColumnsCount();
}
extern "C" void __c__BeginTabBar(bool *ret, char * _str_id, int32_t _flags) {
    *ret = (bool )ImGui::BeginTabBar(_str_id, _flags);
}
extern "C" void __c__EndTabBar() {
    ImGui::EndTabBar();
}
extern "C" void __c__BeginTabItem(bool *ret, char * _label, bool * _p_open, int32_t _flags) {
    *ret = (bool )ImGui::BeginTabItem(_label, _p_open, _flags);
}
extern "C" void __c__EndTabItem() {
    ImGui::EndTabItem();
}
extern "C" void __c__SetTabItemClosed(char * _tab_or_docked_window_label) {
    ImGui::SetTabItemClosed(_tab_or_docked_window_label);
}
extern "C" void __c__DockSpace(uint32_t _id, ImVec2 * _size, int32_t _flags, ImGuiWindowClass * _window_class) {
    ImGui::DockSpace(_id, *_size, _flags, _window_class);
}
extern "C" void __c__DockSpaceOverViewport(ImGuiID *ret, ImGuiViewport * _viewport, int32_t _flags, ImGuiWindowClass * _window_class) {
    *ret = (uint32_t )ImGui::DockSpaceOverViewport(_viewport, _flags, _window_class);
}
extern "C" void __c__SetNextWindowDockID(uint32_t _dock_id, int32_t _cond) {
    ImGui::SetNextWindowDockID(_dock_id, _cond);
}
extern "C" void __c__SetNextWindowClass(ImGuiWindowClass * _window_class) {
    ImGui::SetNextWindowClass(_window_class);
}
extern "C" void __c__GetWindowDockID(ImGuiID *ret) {
    *ret = (uint32_t )ImGui::GetWindowDockID();
}
extern "C" void __c__IsWindowDocked(bool *ret) {
    *ret = (bool )ImGui::IsWindowDocked();
}
extern "C" void __c__LogToTTY(int32_t _auto_open_depth) {
    ImGui::LogToTTY(_auto_open_depth);
}
extern "C" void __c__LogToFile(int32_t _auto_open_depth, char * _filename) {
    ImGui::LogToFile(_auto_open_depth, _filename);
}
extern "C" void __c__LogToClipboard(int32_t _auto_open_depth) {
    ImGui::LogToClipboard(_auto_open_depth);
}
extern "C" void __c__LogFinish() {
    ImGui::LogFinish();
}
extern "C" void __c__LogButtons() {
    ImGui::LogButtons();
}
extern "C" void __c__LogText(char * _fmt) {
    ImGui::LogText(_fmt);
}
extern "C" void __c__BeginDragDropSource(bool *ret, int32_t _flags) {
    *ret = (bool )ImGui::BeginDragDropSource(_flags);
}
extern "C" void __c__SetDragDropPayload(bool *ret, char * _type, void * _data, uint64_t _sz, int32_t _cond) {
    *ret = (bool )ImGui::SetDragDropPayload(_type, _data, _sz, _cond);
}
extern "C" void __c__EndDragDropSource() {
    ImGui::EndDragDropSource();
}
extern "C" void __c__BeginDragDropTarget(bool *ret) {
    *ret = (bool )ImGui::BeginDragDropTarget();
}
extern "C" void __c__AcceptDragDropPayload(ImGuiPayload * *ret, char * _type, int32_t _flags) {
    *ret = (ImGuiPayload * )ImGui::AcceptDragDropPayload(_type, _flags);
}
extern "C" void __c__EndDragDropTarget() {
    ImGui::EndDragDropTarget();
}
extern "C" void __c__GetDragDropPayload(ImGuiPayload * *ret) {
    *ret = (ImGuiPayload * )ImGui::GetDragDropPayload();
}
extern "C" void __c__PushClipRect(ImVec2 * _clip_rect_min, ImVec2 * _clip_rect_max, bool _intersect_with_current_clip_rect) {
    ImGui::PushClipRect(*_clip_rect_min, *_clip_rect_max, _intersect_with_current_clip_rect);
}
extern "C" void __c__PopClipRect() {
    ImGui::PopClipRect();
}
extern "C" void __c__SetItemDefaultFocus() {
    ImGui::SetItemDefaultFocus();
}
extern "C" void __c__SetKeyboardFocusHere(int32_t _offset) {
    ImGui::SetKeyboardFocusHere(_offset);
}
extern "C" void __c__IsItemHovered(bool *ret, int32_t _flags) {
    *ret = (bool )ImGui::IsItemHovered(_flags);
}
extern "C" void __c__IsItemActive(bool *ret) {
    *ret = (bool )ImGui::IsItemActive();
}
extern "C" void __c__IsItemFocused(bool *ret) {
    *ret = (bool )ImGui::IsItemFocused();
}
extern "C" void __c__IsItemClicked(bool *ret, int32_t _mouse_button) {
    *ret = (bool )ImGui::IsItemClicked(_mouse_button);
}
extern "C" void __c__IsItemVisible(bool *ret) {
    *ret = (bool )ImGui::IsItemVisible();
}
extern "C" void __c__IsItemEdited(bool *ret) {
    *ret = (bool )ImGui::IsItemEdited();
}
extern "C" void __c__IsItemActivated(bool *ret) {
    *ret = (bool )ImGui::IsItemActivated();
}
extern "C" void __c__IsItemDeactivated(bool *ret) {
    *ret = (bool )ImGui::IsItemDeactivated();
}
extern "C" void __c__IsItemDeactivatedAfterEdit(bool *ret) {
    *ret = (bool )ImGui::IsItemDeactivatedAfterEdit();
}
extern "C" void __c__IsItemToggledOpen(bool *ret) {
    *ret = (bool )ImGui::IsItemToggledOpen();
}
extern "C" void __c__IsAnyItemHovered(bool *ret) {
    *ret = (bool )ImGui::IsAnyItemHovered();
}
extern "C" void __c__IsAnyItemActive(bool *ret) {
    *ret = (bool )ImGui::IsAnyItemActive();
}
extern "C" void __c__IsAnyItemFocused(bool *ret) {
    *ret = (bool )ImGui::IsAnyItemFocused();
}
extern "C" void __c__GetItemRectMin(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetItemRectMin();
}
extern "C" void __c__GetItemRectMax(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetItemRectMax();
}
extern "C" void __c__GetItemRectSize(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetItemRectSize();
}
extern "C" void __c__SetItemAllowOverlap() {
    ImGui::SetItemAllowOverlap();
}
extern "C" void __c__IsRectVisible(bool *ret, ImVec2 * _size) {
    *ret = (bool )ImGui::IsRectVisible(*_size);
}
extern "C" void __c__IsRectVisible_2(bool *ret, ImVec2 * _rect_min, ImVec2 * _rect_max) {
    *ret = (bool )ImGui::IsRectVisible(*_rect_min, *_rect_max);
}
extern "C" void __c__GetTime(double *ret) {
    *ret = (double )ImGui::GetTime();
}
extern "C" void __c__GetFrameCount(int32_t *ret) {
    *ret = (int32_t )ImGui::GetFrameCount();
}
extern "C" void __c__GetBackgroundDrawList(ImDrawList * *ret) {
    *ret = (ImDrawList * )ImGui::GetBackgroundDrawList();
}
extern "C" void __c__GetForegroundDrawList(ImDrawList * *ret) {
    *ret = (ImDrawList * )ImGui::GetForegroundDrawList();
}
extern "C" void __c__GetBackgroundDrawList_2(ImDrawList * *ret, ImGuiViewport * _viewport) {
    *ret = (ImDrawList * )ImGui::GetBackgroundDrawList(_viewport);
}
extern "C" void __c__GetForegroundDrawList_2(ImDrawList * *ret, ImGuiViewport * _viewport) {
    *ret = (ImDrawList * )ImGui::GetForegroundDrawList(_viewport);
}
extern "C" void __c__GetDrawListSharedData(ImDrawListSharedData * *ret) {
    *ret = (ImDrawListSharedData * )ImGui::GetDrawListSharedData();
}
extern "C" void __c__GetStyleColorName(char * *ret, int32_t _idx) {
    *ret = (char * )ImGui::GetStyleColorName(_idx);
}
extern "C" void __c__SetStateStorage(ImGuiStorage * _storage) {
    ImGui::SetStateStorage(_storage);
}
extern "C" void __c__GetStateStorage(ImGuiStorage * *ret) {
    *ret = (ImGuiStorage * )ImGui::GetStateStorage();
}
extern "C" void __c__CalcTextSize(ImVec2 *ret, char * _text, char * _text_end, bool _hide_text_after_double_hash, float _wrap_width) {
    *ret = (ImVec2 )ImGui::CalcTextSize(_text, _text_end, _hide_text_after_double_hash, _wrap_width);
}
extern "C" void __c__CalcListClipping(int32_t _items_count, float _items_height, int32_t * _out_items_display_start, int32_t * _out_items_display_end) {
    ImGui::CalcListClipping(_items_count, _items_height, _out_items_display_start, _out_items_display_end);
}
extern "C" void __c__BeginChildFrame(bool *ret, uint32_t _id, ImVec2 * _size, int32_t _flags) {
    *ret = (bool )ImGui::BeginChildFrame(_id, *_size, _flags);
}
extern "C" void __c__EndChildFrame() {
    ImGui::EndChildFrame();
}
extern "C" void __c__ColorConvertU32ToFloat4(ImVec4 *ret, uint32_t _in) {
    *ret = (ImVec4 )ImGui::ColorConvertU32ToFloat4(_in);
}
extern "C" void __c__ColorConvertFloat4ToU32(ImU32 *ret, ImVec4 * _in) {
    *ret = (uint32_t )ImGui::ColorConvertFloat4ToU32(*_in);
}
extern "C" void __c__ColorConvertRGBtoHSV(float _r, float _g, float _b, float * _out_h, float * _out_s, float * _out_v) {
    ImGui::ColorConvertRGBtoHSV(_r, _g, _b, *_out_h, *_out_s, *_out_v);
}
extern "C" void __c__ColorConvertHSVtoRGB(float _h, float _s, float _v, float * _out_r, float * _out_g, float * _out_b) {
    ImGui::ColorConvertHSVtoRGB(_h, _s, _v, *_out_r, *_out_g, *_out_b);
}
extern "C" void __c__GetKeyIndex(int32_t *ret, int32_t _imgui_key) {
    *ret = (int32_t )ImGui::GetKeyIndex(_imgui_key);
}
extern "C" void __c__IsKeyDown(bool *ret, int32_t _user_key_index) {
    *ret = (bool )ImGui::IsKeyDown(_user_key_index);
}
extern "C" void __c__IsKeyPressed(bool *ret, int32_t _user_key_index, bool _repeat) {
    *ret = (bool )ImGui::IsKeyPressed(_user_key_index, _repeat);
}
extern "C" void __c__IsKeyReleased(bool *ret, int32_t _user_key_index) {
    *ret = (bool )ImGui::IsKeyReleased(_user_key_index);
}
extern "C" void __c__GetKeyPressedAmount(int32_t *ret, int32_t _key_index, float _repeat_delay, float _rate) {
    *ret = (int32_t )ImGui::GetKeyPressedAmount(_key_index, _repeat_delay, _rate);
}
extern "C" void __c__CaptureKeyboardFromApp(bool _want_capture_keyboard_value) {
    ImGui::CaptureKeyboardFromApp(_want_capture_keyboard_value);
}
extern "C" void __c__IsMouseDown(bool *ret, int32_t _button) {
    *ret = (bool )ImGui::IsMouseDown(_button);
}
extern "C" void __c__IsMouseClicked(bool *ret, int32_t _button, bool _repeat) {
    *ret = (bool )ImGui::IsMouseClicked(_button, _repeat);
}
extern "C" void __c__IsMouseReleased(bool *ret, int32_t _button) {
    *ret = (bool )ImGui::IsMouseReleased(_button);
}
extern "C" void __c__IsMouseDoubleClicked(bool *ret, int32_t _button) {
    *ret = (bool )ImGui::IsMouseDoubleClicked(_button);
}
extern "C" void __c__IsMouseHoveringRect(bool *ret, ImVec2 * _r_min, ImVec2 * _r_max, bool _clip) {
    *ret = (bool )ImGui::IsMouseHoveringRect(*_r_min, *_r_max, _clip);
}
extern "C" void __c__IsMousePosValid(bool *ret, ImVec2 * _mouse_pos) {
    *ret = (bool )ImGui::IsMousePosValid(_mouse_pos);
}
extern "C" void __c__IsAnyMouseDown(bool *ret) {
    *ret = (bool )ImGui::IsAnyMouseDown();
}
extern "C" void __c__GetMousePos(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetMousePos();
}
extern "C" void __c__GetMousePosOnOpeningCurrentPopup(ImVec2 *ret) {
    *ret = (ImVec2 )ImGui::GetMousePosOnOpeningCurrentPopup();
}
extern "C" void __c__IsMouseDragging(bool *ret, int32_t _button, float _lock_threshold) {
    *ret = (bool )ImGui::IsMouseDragging(_button, _lock_threshold);
}
extern "C" void __c__GetMouseDragDelta(ImVec2 *ret, int32_t _button, float _lock_threshold) {
    *ret = (ImVec2 )ImGui::GetMouseDragDelta(_button, _lock_threshold);
}
extern "C" void __c__ResetMouseDragDelta(int32_t _button) {
    ImGui::ResetMouseDragDelta(_button);
}
extern "C" void __c__GetMouseCursor(ImGuiMouseCursor *ret) {
    *ret = (int32_t )ImGui::GetMouseCursor();
}
extern "C" void __c__SetMouseCursor(int32_t _cursor_type) {
    ImGui::SetMouseCursor(_cursor_type);
}
extern "C" void __c__CaptureMouseFromApp(bool _want_capture_mouse_value) {
    ImGui::CaptureMouseFromApp(_want_capture_mouse_value);
}
extern "C" void __c__GetClipboardText(char * *ret) {
    *ret = (char * )ImGui::GetClipboardText();
}
extern "C" void __c__SetClipboardText(char * _text) {
    ImGui::SetClipboardText(_text);
}
extern "C" void __c__LoadIniSettingsFromDisk(char * _ini_filename) {
    ImGui::LoadIniSettingsFromDisk(_ini_filename);
}
extern "C" void __c__LoadIniSettingsFromMemory(char * _ini_data, uint64_t _ini_size) {
    ImGui::LoadIniSettingsFromMemory(_ini_data, _ini_size);
}
extern "C" void __c__SaveIniSettingsToDisk(char * _ini_filename) {
    ImGui::SaveIniSettingsToDisk(_ini_filename);
}
extern "C" void __c__SaveIniSettingsToMemory(char * *ret, size_t * _out_ini_size) {
    *ret = (char * )ImGui::SaveIniSettingsToMemory(_out_ini_size);
}
extern "C" void __c__SetAllocatorFunctions(void * (*_alloc_func)(uint64_t , void * ), void (*_free_func)(void * , void * ), void * _user_data) {
    ImGui::SetAllocatorFunctions(_alloc_func, _free_func, _user_data);
}
extern "C" void __c__MemAlloc(void * *ret, uint64_t _size) {
    *ret = (void * )ImGui::MemAlloc(_size);
}
extern "C" void __c__MemFree(void * _ptr) {
    ImGui::MemFree(_ptr);
}
extern "C" void __c__GetPlatformIO(ImGuiPlatformIO * *ret) {
    *ret = (ImGuiPlatformIO * )&ImGui::GetPlatformIO();
}
extern "C" void __c__GetMainViewport(ImGuiViewport * *ret) {
    *ret = (ImGuiViewport * )ImGui::GetMainViewport();
}
extern "C" void __c__UpdatePlatformWindows() {
    ImGui::UpdatePlatformWindows();
}
extern "C" void __c__RenderPlatformWindowsDefault(void * _platform_arg, void * _renderer_arg) {
    ImGui::RenderPlatformWindowsDefault(_platform_arg, _renderer_arg);
}
extern "C" void __c__DestroyPlatformWindows() {
    ImGui::DestroyPlatformWindows();
}
extern "C" void __c__FindViewportByID(ImGuiViewport * *ret, uint32_t _id) {
    *ret = (ImGuiViewport * )ImGui::FindViewportByID(_id);
}
extern "C" void __c__FindViewportByPlatformHandle(ImGuiViewport * *ret, void * _platform_handle) {
    *ret = (ImGuiViewport * )ImGui::FindViewportByPlatformHandle(_platform_handle);
}
extern "C" void __c__InputFloat_2(bool *ret, char * _label, float * _v, float _step, float _step_fast, int32_t _decimal_precision, int32_t _flags) {
    *ret = (bool )ImGui::InputFloat(_label, _v, _step, _step_fast, _decimal_precision, _flags);
}
extern "C" void __c__InputFloat2_2(bool *ret, char * _label, float * _v, int32_t _decimal_precision, int32_t _flags) {
    *ret = (bool )ImGui::InputFloat2(_label, _v, _decimal_precision, _flags);
}
extern "C" void __c__InputFloat3_2(bool *ret, char * _label, float * _v, int32_t _decimal_precision, int32_t _flags) {
    *ret = (bool )ImGui::InputFloat3(_label, _v, _decimal_precision, _flags);
}
extern "C" void __c__InputFloat4_2(bool *ret, char * _label, float * _v, int32_t _decimal_precision, int32_t _flags) {
    *ret = (bool )ImGui::InputFloat4(_label, _v, _decimal_precision, _flags);
}
extern "C" void __c__ImGui_ImplOpenGL3_Init(bool *ret, char * _glsl_version) {
    *ret = (bool )ImGui_ImplOpenGL3_Init(_glsl_version);
}
extern "C" void __c__ImGui_ImplOpenGL3_Shutdown() {
    ImGui_ImplOpenGL3_Shutdown();
}
extern "C" void __c__ImGui_ImplOpenGL3_NewFrame() {
    ImGui_ImplOpenGL3_NewFrame();
}
extern "C" void __c__ImGui_ImplOpenGL3_RenderDrawData(ImDrawData * _draw_data) {
    ImGui_ImplOpenGL3_RenderDrawData(_draw_data);
}
extern "C" void __c__ImGui_ImplOpenGL3_CreateFontsTexture(bool *ret) {
    *ret = (bool )ImGui_ImplOpenGL3_CreateFontsTexture();
}
extern "C" void __c__ImGui_ImplOpenGL3_DestroyFontsTexture() {
    ImGui_ImplOpenGL3_DestroyFontsTexture();
}
extern "C" void __c__ImGui_ImplOpenGL3_CreateDeviceObjects(bool *ret) {
    *ret = (bool )ImGui_ImplOpenGL3_CreateDeviceObjects();
}
extern "C" void __c__ImGui_ImplOpenGL3_DestroyDeviceObjects() {
    ImGui_ImplOpenGL3_DestroyDeviceObjects();
}
extern "C" void __c__ImGui_ImplGlfw_InitForOpenGL(bool *ret, GLFWwindow * _window, bool _install_callbacks) {
    *ret = (bool )ImGui_ImplGlfw_InitForOpenGL(_window, _install_callbacks);
}
extern "C" void __c__ImGui_ImplGlfw_InitForVulkan(bool *ret, GLFWwindow * _window, bool _install_callbacks) {
    *ret = (bool )ImGui_ImplGlfw_InitForVulkan(_window, _install_callbacks);
}
extern "C" void __c__ImGui_ImplGlfw_Shutdown() {
    ImGui_ImplGlfw_Shutdown();
}
extern "C" void __c__ImGui_ImplGlfw_NewFrame() {
    ImGui_ImplGlfw_NewFrame();
}
extern "C" void __c__ImGui_ImplGlfw_MouseButtonCallback(GLFWwindow * _window, int32_t _button, int32_t _action, int32_t _mods) {
    ImGui_ImplGlfw_MouseButtonCallback(_window, _button, _action, _mods);
}
extern "C" void __c__ImGui_ImplGlfw_ScrollCallback(GLFWwindow * _window, double _xoffset, double _yoffset) {
    ImGui_ImplGlfw_ScrollCallback(_window, _xoffset, _yoffset);
}
extern "C" void __c__ImGui_ImplGlfw_KeyCallback(GLFWwindow * _window, int32_t _key, int32_t _scancode, int32_t _action, int32_t _mods) {
    ImGui_ImplGlfw_KeyCallback(_window, _key, _scancode, _action, _mods);
}
extern "C" void __c__ImGui_ImplGlfw_CharCallback(GLFWwindow * _window, uint32_t _c) {
    ImGui_ImplGlfw_CharCallback(_window, _c);
}
