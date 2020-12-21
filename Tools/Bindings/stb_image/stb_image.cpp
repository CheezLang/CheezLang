#include <memory>
#include "../binding_source.cpp"

extern "C" void __c__stbi_load_from_memory(stbi_uc * *ret, const stbi_uc * _buffer, int32_t _len, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (stbi_uc * )stbi_load_from_memory(_buffer, _len, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_load_from_callbacks(stbi_uc * *ret, const stbi_io_callbacks * _clbk, void * _user, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (stbi_uc * )stbi_load_from_callbacks(_clbk, _user, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_load(stbi_uc * *ret, const char * _filename, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (stbi_uc * )stbi_load(_filename, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_load_from_file(stbi_uc * *ret, FILE * _f, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (stbi_uc * )stbi_load_from_file(_f, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_load_gif_from_memory(stbi_uc * *ret, const stbi_uc * _buffer, int32_t _len, int32_t * * _delays, int32_t * _x, int32_t * _y, int32_t * _z, int32_t * _comp, int32_t _req_comp) {
    *ret = (stbi_uc * )stbi_load_gif_from_memory(_buffer, _len, _delays, _x, _y, _z, _comp, _req_comp);
}
extern "C" void __c__stbi_load_16_from_memory(stbi_us * *ret, const stbi_uc * _buffer, int32_t _len, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (stbi_us * )stbi_load_16_from_memory(_buffer, _len, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_load_16_from_callbacks(stbi_us * *ret, const stbi_io_callbacks * _clbk, void * _user, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (stbi_us * )stbi_load_16_from_callbacks(_clbk, _user, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_load_16(stbi_us * *ret, const char * _filename, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (stbi_us * )stbi_load_16(_filename, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_load_from_file_16(stbi_us * *ret, FILE * _f, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (stbi_us * )stbi_load_from_file_16(_f, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_loadf_from_memory(float * *ret, const stbi_uc * _buffer, int32_t _len, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (float * )stbi_loadf_from_memory(_buffer, _len, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_loadf_from_callbacks(float * *ret, const stbi_io_callbacks * _clbk, void * _user, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (float * )stbi_loadf_from_callbacks(_clbk, _user, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_loadf(float * *ret, const char * _filename, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (float * )stbi_loadf(_filename, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_loadf_from_file(float * *ret, FILE * _f, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (float * )stbi_loadf_from_file(_f, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_hdr_to_ldr_gamma(float _gamma) {
    stbi_hdr_to_ldr_gamma(_gamma);
}
extern "C" void __c__stbi_hdr_to_ldr_scale(float _scale) {
    stbi_hdr_to_ldr_scale(_scale);
}
extern "C" void __c__stbi_ldr_to_hdr_gamma(float _gamma) {
    stbi_ldr_to_hdr_gamma(_gamma);
}
extern "C" void __c__stbi_ldr_to_hdr_scale(float _scale) {
    stbi_ldr_to_hdr_scale(_scale);
}
extern "C" void __c__stbi_is_hdr_from_callbacks(int32_t *ret, const stbi_io_callbacks * _clbk, void * _user) {
    *ret = (int32_t )stbi_is_hdr_from_callbacks(_clbk, _user);
}
extern "C" void __c__stbi_is_hdr_from_memory(int32_t *ret, const stbi_uc * _buffer, int32_t _len) {
    *ret = (int32_t )stbi_is_hdr_from_memory(_buffer, _len);
}
extern "C" void __c__stbi_is_hdr(int32_t *ret, const char * _filename) {
    *ret = (int32_t )stbi_is_hdr(_filename);
}
extern "C" void __c__stbi_is_hdr_from_file(int32_t *ret, FILE * _f) {
    *ret = (int32_t )stbi_is_hdr_from_file(_f);
}
extern "C" void __c__stbi_failure_reason(const char * *ret) {
    *ret = (const char * )stbi_failure_reason();
}
extern "C" void __c__stbi_image_free(void * _retval_from_stbi_load) {
    stbi_image_free(_retval_from_stbi_load);
}
extern "C" void __c__stbi_info_from_memory(int32_t *ret, const stbi_uc * _buffer, int32_t _len, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi_info_from_memory(_buffer, _len, _x, _y, _comp);
}
extern "C" void __c__stbi_info_from_callbacks(int32_t *ret, const stbi_io_callbacks * _clbk, void * _user, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi_info_from_callbacks(_clbk, _user, _x, _y, _comp);
}
extern "C" void __c__stbi_is_16_bit_from_memory(int32_t *ret, const stbi_uc * _buffer, int32_t _len) {
    *ret = (int32_t )stbi_is_16_bit_from_memory(_buffer, _len);
}
extern "C" void __c__stbi_is_16_bit_from_callbacks(int32_t *ret, const stbi_io_callbacks * _clbk, void * _user) {
    *ret = (int32_t )stbi_is_16_bit_from_callbacks(_clbk, _user);
}
extern "C" void __c__stbi_info(int32_t *ret, const char * _filename, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi_info(_filename, _x, _y, _comp);
}
extern "C" void __c__stbi_info_from_file(int32_t *ret, FILE * _f, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi_info_from_file(_f, _x, _y, _comp);
}
extern "C" void __c__stbi_is_16_bit(int32_t *ret, const char * _filename) {
    *ret = (int32_t )stbi_is_16_bit(_filename);
}
extern "C" void __c__stbi_is_16_bit_from_file(int32_t *ret, FILE * _f) {
    *ret = (int32_t )stbi_is_16_bit_from_file(_f);
}
extern "C" void __c__stbi_set_unpremultiply_on_load(int32_t _flag_true_if_should_unpremultiply) {
    stbi_set_unpremultiply_on_load(_flag_true_if_should_unpremultiply);
}
extern "C" void __c__stbi_convert_iphone_png_to_rgb(int32_t _flag_true_if_should_convert) {
    stbi_convert_iphone_png_to_rgb(_flag_true_if_should_convert);
}
extern "C" void __c__stbi_set_flip_vertically_on_load(int32_t _flag_true_if_should_flip) {
    stbi_set_flip_vertically_on_load(_flag_true_if_should_flip);
}
extern "C" void __c__stbi_set_flip_vertically_on_load_thread(int32_t _flag_true_if_should_flip) {
    stbi_set_flip_vertically_on_load_thread(_flag_true_if_should_flip);
}
extern "C" void __c__stbi_zlib_decode_malloc_guesssize(char * *ret, const char * _buffer, int32_t _len, int32_t _initial_size, int32_t * _outlen) {
    *ret = (char * )stbi_zlib_decode_malloc_guesssize(_buffer, _len, _initial_size, _outlen);
}
extern "C" void __c__stbi_zlib_decode_malloc_guesssize_headerflag(char * *ret, const char * _buffer, int32_t _len, int32_t _initial_size, int32_t * _outlen, int32_t _parse_header) {
    *ret = (char * )stbi_zlib_decode_malloc_guesssize_headerflag(_buffer, _len, _initial_size, _outlen, _parse_header);
}
extern "C" void __c__stbi_zlib_decode_malloc(char * *ret, const char * _buffer, int32_t _len, int32_t * _outlen) {
    *ret = (char * )stbi_zlib_decode_malloc(_buffer, _len, _outlen);
}
extern "C" void __c__stbi_zlib_decode_buffer(int32_t *ret, char * _obuffer, int32_t _olen, const char * _ibuffer, int32_t _ilen) {
    *ret = (int32_t )stbi_zlib_decode_buffer(_obuffer, _olen, _ibuffer, _ilen);
}
extern "C" void __c__stbi_zlib_decode_noheader_malloc(char * *ret, const char * _buffer, int32_t _len, int32_t * _outlen) {
    *ret = (char * )stbi_zlib_decode_noheader_malloc(_buffer, _len, _outlen);
}
extern "C" void __c__stbi_zlib_decode_noheader_buffer(int32_t *ret, char * _obuffer, int32_t _olen, const char * _ibuffer, int32_t _ilen) {
    *ret = (int32_t )stbi_zlib_decode_noheader_buffer(_obuffer, _olen, _ibuffer, _ilen);
}
extern "C" void __c__stbi__refill_buffer(stbi__context * _s) {
    stbi__refill_buffer(_s);
}
extern "C" void __c__stbi__jpeg_test(int32_t *ret, stbi__context * _s) {
    *ret = (int32_t )stbi__jpeg_test(_s);
}
extern "C" void __c__stbi__jpeg_load(void * *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp, int32_t _req_comp, stbi__result_info * _ri) {
    *ret = (void * )stbi__jpeg_load(_s, _x, _y, _comp, _req_comp, _ri);
}
extern "C" void __c__stbi__jpeg_info(int32_t *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi__jpeg_info(_s, _x, _y, _comp);
}
extern "C" void __c__stbi__png_test(int32_t *ret, stbi__context * _s) {
    *ret = (int32_t )stbi__png_test(_s);
}
extern "C" void __c__stbi__png_load(void * *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp, int32_t _req_comp, stbi__result_info * _ri) {
    *ret = (void * )stbi__png_load(_s, _x, _y, _comp, _req_comp, _ri);
}
extern "C" void __c__stbi__png_info(int32_t *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi__png_info(_s, _x, _y, _comp);
}
extern "C" void __c__stbi__png_is16(int32_t *ret, stbi__context * _s) {
    *ret = (int32_t )stbi__png_is16(_s);
}
extern "C" void __c__stbi__bmp_test(int32_t *ret, stbi__context * _s) {
    *ret = (int32_t )stbi__bmp_test(_s);
}
extern "C" void __c__stbi__bmp_load(void * *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp, int32_t _req_comp, stbi__result_info * _ri) {
    *ret = (void * )stbi__bmp_load(_s, _x, _y, _comp, _req_comp, _ri);
}
extern "C" void __c__stbi__bmp_info(int32_t *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi__bmp_info(_s, _x, _y, _comp);
}
extern "C" void __c__stbi__tga_test(int32_t *ret, stbi__context * _s) {
    *ret = (int32_t )stbi__tga_test(_s);
}
extern "C" void __c__stbi__tga_load(void * *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp, int32_t _req_comp, stbi__result_info * _ri) {
    *ret = (void * )stbi__tga_load(_s, _x, _y, _comp, _req_comp, _ri);
}
extern "C" void __c__stbi__tga_info(int32_t *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi__tga_info(_s, _x, _y, _comp);
}
extern "C" void __c__stbi__psd_test(int32_t *ret, stbi__context * _s) {
    *ret = (int32_t )stbi__psd_test(_s);
}
extern "C" void __c__stbi__psd_load(void * *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp, int32_t _req_comp, stbi__result_info * _ri, int32_t _bpc) {
    *ret = (void * )stbi__psd_load(_s, _x, _y, _comp, _req_comp, _ri, _bpc);
}
extern "C" void __c__stbi__psd_info(int32_t *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi__psd_info(_s, _x, _y, _comp);
}
extern "C" void __c__stbi__psd_is16(int32_t *ret, stbi__context * _s) {
    *ret = (int32_t )stbi__psd_is16(_s);
}
extern "C" void __c__stbi__hdr_test(int32_t *ret, stbi__context * _s) {
    *ret = (int32_t )stbi__hdr_test(_s);
}
extern "C" void __c__stbi__hdr_load(float * *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp, int32_t _req_comp, stbi__result_info * _ri) {
    *ret = (float * )stbi__hdr_load(_s, _x, _y, _comp, _req_comp, _ri);
}
extern "C" void __c__stbi__hdr_info(int32_t *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi__hdr_info(_s, _x, _y, _comp);
}
extern "C" void __c__stbi__pic_test(int32_t *ret, stbi__context * _s) {
    *ret = (int32_t )stbi__pic_test(_s);
}
extern "C" void __c__stbi__pic_load(void * *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp, int32_t _req_comp, stbi__result_info * _ri) {
    *ret = (void * )stbi__pic_load(_s, _x, _y, _comp, _req_comp, _ri);
}
extern "C" void __c__stbi__pic_info(int32_t *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi__pic_info(_s, _x, _y, _comp);
}
extern "C" void __c__stbi__gif_test(int32_t *ret, stbi__context * _s) {
    *ret = (int32_t )stbi__gif_test(_s);
}
extern "C" void __c__stbi__gif_load(void * *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp, int32_t _req_comp, stbi__result_info * _ri) {
    *ret = (void * )stbi__gif_load(_s, _x, _y, _comp, _req_comp, _ri);
}
extern "C" void __c__stbi__load_gif_main(void * *ret, stbi__context * _s, int32_t * * _delays, int32_t * _x, int32_t * _y, int32_t * _z, int32_t * _comp, int32_t _req_comp) {
    *ret = (void * )stbi__load_gif_main(_s, _delays, _x, _y, _z, _comp, _req_comp);
}
extern "C" void __c__stbi__gif_info(int32_t *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi__gif_info(_s, _x, _y, _comp);
}
extern "C" void __c__stbi__pnm_test(int32_t *ret, stbi__context * _s) {
    *ret = (int32_t )stbi__pnm_test(_s);
}
extern "C" void __c__stbi__pnm_load(void * *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp, int32_t _req_comp, stbi__result_info * _ri) {
    *ret = (void * )stbi__pnm_load(_s, _x, _y, _comp, _req_comp, _ri);
}
extern "C" void __c__stbi__pnm_info(int32_t *ret, stbi__context * _s, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi__pnm_info(_s, _x, _y, _comp);
}
extern "C" void __c__stbi__ldr_to_hdr(float * *ret, stbi_uc * _data, int32_t _x, int32_t _y, int32_t _comp) {
    *ret = (float * )stbi__ldr_to_hdr(_data, _x, _y, _comp);
}
extern "C" void __c__stbi__hdr_to_ldr(stbi_uc * *ret, float * _data, int32_t _x, int32_t _y, int32_t _comp) {
    *ret = (stbi_uc * )stbi__hdr_to_ldr(_data, _x, _y, _comp);
}
