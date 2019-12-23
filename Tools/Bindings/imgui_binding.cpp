#include <memory>
#include "imgui.cpp"

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
extern "C" void __c__ImGuiIO_AddInputCharacter_46(ImGuiIO* self, uint32_t _c) {
    self->AddInputCharacter(_c);
}
extern "C" void __c__ImGuiIO_AddInputCharactersUTF8_47(ImGuiIO* self, char * _str) {
    self->AddInputCharactersUTF8(_str);
}
extern "C" void __c__ImGuiIO_ClearInputCharacters_48(ImGuiIO* self) {
    self->ClearInputCharacters();
}
extern "C" void __c__ImGuiIO_new_80(ImGuiIO* self) {
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
extern "C" void __c__ImDrawData_new_8(ImDrawData* self) {
    new (self) ImDrawData();
}
extern "C" void __c__ImDrawData_dtor(ImDrawData* self) {
    self->~ImDrawData();
}
extern "C" void __c__ImDrawData_Clear_10(ImDrawData* self) {
    self->Clear();
}
extern "C" void __c__ImDrawData_DeIndexAllBuffers_11(ImDrawData* self) {
    self->DeIndexAllBuffers();
}
extern "C" void __c__ImDrawData_ScaleClipRects_12(ImDrawData* self, ImVec2 * _fb_scale) {
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
extern "C" void __c__ImFontGlyphRangesBuilder_BuildRanges_8(ImFontGlyphRangesBuilder* self, __UNKNOWN__ * _out_ranges) {
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
extern "C" void __c__ImFont_CalcTextSizeA_24(ImFont* self, ImVec2 *ret, float _size, float _max_width, float _wrap_width, char * _text_begin, char * _text_end, char * * _remaining) {
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
extern "C" void __c__ImBoolVector_new_1(ImBoolVector* self) {
    new (self) ImBoolVector();
}
extern "C" void __c__ImBoolVector_Resize_2(ImBoolVector* self, int32_t _sz) {
    self->Resize(_sz);
}
extern "C" void __c__ImBoolVector_Clear_3(ImBoolVector* self) {
    self->Clear();
}
extern "C" void __c__ImBoolVector_GetBit_4(ImBoolVector* self, bool *ret, int32_t _n) {
    *ret = (bool )self->GetBit(_n);
}
extern "C" void __c__ImBoolVector_SetBit_5(ImBoolVector* self, int32_t _n, bool _v) {
    self->SetBit(_n, _v);
}
extern "C" void __c__ImVec1_new_1(ImVec1* self) {
    new (self) ImVec1();
}
extern "C" void __c__ImVec1_new_2(ImVec1* self, float __x) {
    new (self) ImVec1(__x);
}
extern "C" void __c__ImVec2ih_new_2(ImVec2ih* self) {
    new (self) ImVec2ih();
}
extern "C" void __c__ImVec2ih_new_3(ImVec2ih* self, int16_t __x, int16_t __y) {
    new (self) ImVec2ih(__x, __y);
}
extern "C" void __c__ImRect_new_2(ImRect* self) {
    new (self) ImRect();
}
extern "C" void __c__ImRect_new_3(ImRect* self, ImVec2 * _min, ImVec2 * _max) {
    new (self) ImRect(*_min, *_max);
}
extern "C" void __c__ImRect_new_4(ImRect* self, ImVec4 * _v) {
    new (self) ImRect(*_v);
}
extern "C" void __c__ImRect_new_5(ImRect* self, float _x1, float _y1, float _x2, float _y2) {
    new (self) ImRect(_x1, _y1, _x2, _y2);
}
extern "C" void __c__ImRect_GetCenter_6(ImRect* self, ImVec2 *ret) {
    *ret = (ImVec2 )self->GetCenter();
}
extern "C" void __c__ImRect_GetSize_7(ImRect* self, ImVec2 *ret) {
    *ret = (ImVec2 )self->GetSize();
}
extern "C" void __c__ImRect_GetWidth_8(ImRect* self, float *ret) {
    *ret = (float )self->GetWidth();
}
extern "C" void __c__ImRect_GetHeight_9(ImRect* self, float *ret) {
    *ret = (float )self->GetHeight();
}
extern "C" void __c__ImRect_GetTL_10(ImRect* self, ImVec2 *ret) {
    *ret = (ImVec2 )self->GetTL();
}
extern "C" void __c__ImRect_GetTR_11(ImRect* self, ImVec2 *ret) {
    *ret = (ImVec2 )self->GetTR();
}
extern "C" void __c__ImRect_GetBL_12(ImRect* self, ImVec2 *ret) {
    *ret = (ImVec2 )self->GetBL();
}
extern "C" void __c__ImRect_GetBR_13(ImRect* self, ImVec2 *ret) {
    *ret = (ImVec2 )self->GetBR();
}
extern "C" void __c__ImRect_Contains_14(ImRect* self, bool *ret, ImVec2 * _p) {
    *ret = (bool )self->Contains(*_p);
}
extern "C" void __c__ImRect_Contains_15(ImRect* self, bool *ret, ImRect * _r) {
    *ret = (bool )self->Contains(*_r);
}
extern "C" void __c__ImRect_Overlaps_16(ImRect* self, bool *ret, ImRect * _r) {
    *ret = (bool )self->Overlaps(*_r);
}
extern "C" void __c__ImRect_Add_17(ImRect* self, ImVec2 * _p) {
    self->Add(*_p);
}
extern "C" void __c__ImRect_Add_18(ImRect* self, ImRect * _r) {
    self->Add(*_r);
}
extern "C" void __c__ImRect_Expand_19(ImRect* self, float _amount) {
    self->Expand(_amount);
}
extern "C" void __c__ImRect_Expand_20(ImRect* self, ImVec2 * _amount) {
    self->Expand(*_amount);
}
extern "C" void __c__ImRect_Translate_21(ImRect* self, ImVec2 * _d) {
    self->Translate(*_d);
}
extern "C" void __c__ImRect_TranslateX_22(ImRect* self, float _dx) {
    self->TranslateX(_dx);
}
extern "C" void __c__ImRect_TranslateY_23(ImRect* self, float _dy) {
    self->TranslateY(_dy);
}
extern "C" void __c__ImRect_ClipWith_24(ImRect* self, ImRect * _r) {
    self->ClipWith(*_r);
}
extern "C" void __c__ImRect_ClipWithFull_25(ImRect* self, ImRect * _r) {
    self->ClipWithFull(*_r);
}
extern "C" void __c__ImRect_Floor_26(ImRect* self) {
    self->Floor();
}
extern "C" void __c__ImRect_IsInverted_27(ImRect* self, bool *ret) {
    *ret = (bool )self->IsInverted();
}
extern "C" void __c__ImGuiStyleMod_new_2(ImGuiStyleMod* self, int32_t _idx, int32_t _v) {
    new (self) ImGuiStyleMod(_idx, _v);
}
extern "C" void __c__ImGuiStyleMod_new_3(ImGuiStyleMod* self, int32_t _idx, float _v) {
    new (self) ImGuiStyleMod(_idx, _v);
}
extern "C" void __c__ImGuiStyleMod_new_4(ImGuiStyleMod* self, int32_t _idx, ImVec2* _v) {
    new (self) ImGuiStyleMod(_idx, *_v);
}
extern "C" void __c__ImGuiMenuColumns_new_5(ImGuiMenuColumns* self) {
    new (self) ImGuiMenuColumns();
}
extern "C" void __c__ImGuiMenuColumns_Update_6(ImGuiMenuColumns* self, int32_t _count, float _spacing, bool _clear) {
    self->Update(_count, _spacing, _clear);
}
extern "C" void __c__ImGuiMenuColumns_DeclColumns_7(ImGuiMenuColumns* self, float *ret, float _w0, float _w1, float _w2) {
    *ret = (float )self->DeclColumns(_w0, _w1, _w2);
}
extern "C" void __c__ImGuiMenuColumns_CalcExtraSpace_8(ImGuiMenuColumns* self, float *ret, float _avail_w) {
    *ret = (float )self->CalcExtraSpace(_avail_w);
}
extern "C" void __c__ImGuiInputTextState_new_16(ImGuiInputTextState* self) {
    new (self) ImGuiInputTextState();
}
extern "C" void __c__ImGuiInputTextState_ClearText_17(ImGuiInputTextState* self) {
    self->ClearText();
}
extern "C" void __c__ImGuiInputTextState_ClearFreeMemory_18(ImGuiInputTextState* self) {
    self->ClearFreeMemory();
}
extern "C" void __c__ImGuiInputTextState_GetUndoAvailCount_19(ImGuiInputTextState* self, int32_t *ret) {
    *ret = (int32_t )self->GetUndoAvailCount();
}
extern "C" void __c__ImGuiInputTextState_GetRedoAvailCount_20(ImGuiInputTextState* self, int32_t *ret) {
    *ret = (int32_t )self->GetRedoAvailCount();
}
extern "C" void __c__ImGuiInputTextState_OnKeyPressed_21(ImGuiInputTextState* self, int32_t _key) {
    self->OnKeyPressed(_key);
}
extern "C" void __c__ImGuiInputTextState_CursorAnimReset_22(ImGuiInputTextState* self) {
    self->CursorAnimReset();
}
extern "C" void __c__ImGuiInputTextState_CursorClamp_23(ImGuiInputTextState* self) {
    self->CursorClamp();
}
extern "C" void __c__ImGuiInputTextState_HasSelection_24(ImGuiInputTextState* self, bool *ret) {
    *ret = (bool )self->HasSelection();
}
extern "C" void __c__ImGuiInputTextState_ClearSelection_25(ImGuiInputTextState* self) {
    self->ClearSelection();
}
extern "C" void __c__ImGuiInputTextState_SelectAll_26(ImGuiInputTextState* self) {
    self->SelectAll();
}
extern "C" void __c__ImGuiWindowSettings_new_4(ImGuiWindowSettings* self) {
    new (self) ImGuiWindowSettings();
}
extern "C" void __c__ImGuiWindowSettings_GetName_5(ImGuiWindowSettings* self, char * *ret) {
    *ret = (char * )self->GetName();
}
extern "C" void __c__ImGuiSettingsHandler_new_6(ImGuiSettingsHandler* self) {
    new (self) ImGuiSettingsHandler();
}
extern "C" void __c__ImGuiPopupData_new_7(ImGuiPopupData* self) {
    new (self) ImGuiPopupData();
}
extern "C" void __c__ImGuiColumnData_new_4(ImGuiColumnData* self) {
    new (self) ImGuiColumnData();
}
extern "C" void __c__ImGuiColumns_new_15(ImGuiColumns* self) {
    new (self) ImGuiColumns();
}
extern "C" void __c__ImGuiColumns_Clear_16(ImGuiColumns* self) {
    self->Clear();
}
extern "C" void __c__ImDrawListSharedData_new_7(ImDrawListSharedData* self) {
    new (self) ImDrawListSharedData();
}
extern "C" void __c__ImDrawDataBuilder_Clear_1(ImDrawDataBuilder* self) {
    self->Clear();
}
extern "C" void __c__ImDrawDataBuilder_ClearFreeMemory_2(ImDrawDataBuilder* self) {
    self->ClearFreeMemory();
}
extern "C" void __c__ImDrawDataBuilder_FlattenIntoSingleLayer_3(ImDrawDataBuilder* self) {
    self->FlattenIntoSingleLayer();
}
extern "C" void __c__ImGuiNavMoveResult_new_7(ImGuiNavMoveResult* self) {
    new (self) ImGuiNavMoveResult();
}
extern "C" void __c__ImGuiNavMoveResult_Clear_8(ImGuiNavMoveResult* self) {
    self->Clear();
}
extern "C" void __c__ImGuiNextWindowData_new_14(ImGuiNextWindowData* self) {
    new (self) ImGuiNextWindowData();
}
extern "C" void __c__ImGuiNextWindowData_ClearFlags_15(ImGuiNextWindowData* self) {
    self->ClearFlags();
}
extern "C" void __c__ImGuiNextItemData_new_4(ImGuiNextItemData* self) {
    new (self) ImGuiNextItemData();
}
extern "C" void __c__ImGuiNextItemData_ClearFlags_5(ImGuiNextItemData* self) {
    self->ClearFlags();
}
extern "C" void __c__ImGuiPtrOrIndex_new_2(ImGuiPtrOrIndex* self, void * _ptr) {
    new (self) ImGuiPtrOrIndex(_ptr);
}
extern "C" void __c__ImGuiPtrOrIndex_new_3(ImGuiPtrOrIndex* self, int32_t _index) {
    new (self) ImGuiPtrOrIndex(_index);
}
extern "C" void __c__ImGuiContext_new_172(ImGuiContext* self, ImFontAtlas * _shared_font_atlas) {
    new (self) ImGuiContext(_shared_font_atlas);
}
extern "C" void __c__ImGuiWindowTempData_new_40(ImGuiWindowTempData* self) {
    new (self) ImGuiWindowTempData();
}
extern "C" void __c__ImGuiWindow_new_76(ImGuiWindow* self, ImGuiContext * _context, char * _name) {
    new (self) ImGuiWindow(_context, _name);
}
extern "C" void __c__ImGuiWindow_dtor(ImGuiWindow* self) {
    self->~ImGuiWindow();
}
extern "C" void __c__ImGuiWindow_GetID_78(ImGuiWindow* self, ImGuiID *ret, char * _str, char * _str_end) {
    *ret = (uint32_t )self->GetID(_str, _str_end);
}
extern "C" void __c__ImGuiWindow_GetID_79(ImGuiWindow* self, ImGuiID *ret, void * _ptr) {
    *ret = (uint32_t )self->GetID(_ptr);
}
extern "C" void __c__ImGuiWindow_GetID_80(ImGuiWindow* self, ImGuiID *ret, int32_t _n) {
    *ret = (uint32_t )self->GetID(_n);
}
extern "C" void __c__ImGuiWindow_GetIDNoKeepAlive_81(ImGuiWindow* self, ImGuiID *ret, char * _str, char * _str_end) {
    *ret = (uint32_t )self->GetIDNoKeepAlive(_str, _str_end);
}
extern "C" void __c__ImGuiWindow_GetIDNoKeepAlive_82(ImGuiWindow* self, ImGuiID *ret, void * _ptr) {
    *ret = (uint32_t )self->GetIDNoKeepAlive(_ptr);
}
extern "C" void __c__ImGuiWindow_GetIDNoKeepAlive_83(ImGuiWindow* self, ImGuiID *ret, int32_t _n) {
    *ret = (uint32_t )self->GetIDNoKeepAlive(_n);
}
extern "C" void __c__ImGuiWindow_GetIDFromRectangle_84(ImGuiWindow* self, ImGuiID *ret, ImRect * _r_abs) {
    *ret = (uint32_t )self->GetIDFromRectangle(*_r_abs);
}
extern "C" void __c__ImGuiWindow_Rect_85(ImGuiWindow* self, ImRect *ret) {
    *ret = (ImRect )self->Rect();
}
extern "C" void __c__ImGuiWindow_CalcFontSize_86(ImGuiWindow* self, float *ret) {
    *ret = (float )self->CalcFontSize();
}
extern "C" void __c__ImGuiWindow_TitleBarHeight_87(ImGuiWindow* self, float *ret) {
    *ret = (float )self->TitleBarHeight();
}
extern "C" void __c__ImGuiWindow_TitleBarRect_88(ImGuiWindow* self, ImRect *ret) {
    *ret = (ImRect )self->TitleBarRect();
}
extern "C" void __c__ImGuiWindow_MenuBarHeight_89(ImGuiWindow* self, float *ret) {
    *ret = (float )self->MenuBarHeight();
}
extern "C" void __c__ImGuiWindow_MenuBarRect_90(ImGuiWindow* self, ImRect *ret) {
    *ret = (ImRect )self->MenuBarRect();
}
extern "C" void __c__ImGuiItemHoveredDataBackup_new_4(ImGuiItemHoveredDataBackup* self) {
    new (self) ImGuiItemHoveredDataBackup();
}
extern "C" void __c__ImGuiItemHoveredDataBackup_Backup_5(ImGuiItemHoveredDataBackup* self) {
    self->Backup();
}
extern "C" void __c__ImGuiItemHoveredDataBackup_Restore_6(ImGuiItemHoveredDataBackup* self) {
    self->Restore();
}
extern "C" void __c__ImGuiTabItem_new_8(ImGuiTabItem* self) {
    new (self) ImGuiTabItem();
}
extern "C" void __c__ImGuiTabBar_new_24(ImGuiTabBar* self) {
    new (self) ImGuiTabBar();
}
extern "C" void __c__ImGuiTabBar_GetTabOrder_25(ImGuiTabBar* self, int32_t *ret, ImGuiTabItem * _tab) {
    *ret = (int32_t )self->GetTabOrder(_tab);
}
extern "C" void __c__ImGuiTabBar_GetTabName_26(ImGuiTabBar* self, char * *ret, ImGuiTabItem * _tab) {
    *ret = (char * )self->GetTabName(_tab);
}
extern "C" void __c__ImGuiStyleVarInfo_GetVarPtr_3(ImGuiStyleVarInfo* self, void * *ret, ImGuiStyle * _style) {
    *ret = (void * )self->GetVarPtr(_style);
}
