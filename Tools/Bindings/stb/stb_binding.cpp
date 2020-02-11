#include <memory>
#include "stb_binding_source.cpp"

extern "C" void __c____underflow(int32_t *ret, _IO_FILE * _0) {
    *ret = (int32_t )__underflow(_0);
}
extern "C" void __c____uflow(int32_t *ret, _IO_FILE * _0) {
    *ret = (int32_t )__uflow(_0);
}
extern "C" void __c____overflow(int32_t *ret, _IO_FILE * _0, int32_t _1) {
    *ret = (int32_t )__overflow(_0, _1);
}
extern "C" void __c___IO_getc(int32_t *ret, _IO_FILE * ___fp) {
    *ret = (int32_t )_IO_getc(___fp);
}
extern "C" void __c___IO_putc(int32_t *ret, int32_t ___c, _IO_FILE * ___fp) {
    *ret = (int32_t )_IO_putc(___c, ___fp);
}
extern "C" void __c___IO_feof(int32_t *ret, _IO_FILE * ___fp) {
    *ret = (int32_t )_IO_feof(___fp);
}
extern "C" void __c___IO_ferror(int32_t *ret, _IO_FILE * ___fp) {
    *ret = (int32_t )_IO_ferror(___fp);
}
extern "C" void __c___IO_peekc_locked(int32_t *ret, _IO_FILE * ___fp) {
    *ret = (int32_t )_IO_peekc_locked(___fp);
}
extern "C" void __c___IO_flockfile(_IO_FILE * _0) {
    _IO_flockfile(_0);
}
extern "C" void __c___IO_funlockfile(_IO_FILE * _0) {
    _IO_funlockfile(_0);
}
extern "C" void __c___IO_ftrylockfile(int32_t *ret, _IO_FILE * _0) {
    *ret = (int32_t )_IO_ftrylockfile(_0);
}
extern "C" void __c___IO_vfscanf(int32_t *ret, _IO_FILE * _0, char * _1, char * _2, int32_t * _3) {
    *ret = (int32_t )_IO_vfscanf(_0, _1, _2, _3);
}
extern "C" void __c___IO_vfprintf(int32_t *ret, _IO_FILE * _0, char * _1, char * _2) {
    *ret = (int32_t )_IO_vfprintf(_0, _1, _2);
}
extern "C" void __c___IO_padn(__ssize_t *ret, _IO_FILE * _0, int32_t _1, int32_t _2) {
    *ret = (int32_t )_IO_padn(_0, _1, _2);
}
extern "C" void __c___IO_sgetn(size_t *ret, _IO_FILE * _0, void * _1, uint64_t _2) {
    *ret = (uint64_t )_IO_sgetn(_0, _1, _2);
}
extern "C" void __c___IO_seekoff(__off64_t *ret, _IO_FILE * _0, int32_t _1, int32_t _2, int32_t _3) {
    *ret = (int32_t )_IO_seekoff(_0, _1, _2, _3);
}
extern "C" void __c___IO_seekpos(__off64_t *ret, _IO_FILE * _0, int32_t _1, int32_t _2) {
    *ret = (int32_t )_IO_seekpos(_0, _1, _2);
}
extern "C" void __c___IO_free_backup_area(_IO_FILE * _0) {
    _IO_free_backup_area(_0);
}
extern "C" void __c__remove(int32_t *ret, char * ___filename) {
    *ret = (int32_t )remove(___filename);
}
extern "C" void __c__rename(int32_t *ret, char * ___old, char * ___new) {
    *ret = (int32_t )rename(___old, ___new);
}
extern "C" void __c__renameat(int32_t *ret, int32_t ___oldfd, char * ___old, int32_t ___newfd, char * ___new) {
    *ret = (int32_t )renameat(___oldfd, ___old, ___newfd, ___new);
}
extern "C" void __c__tmpfile(FILE * *ret) {
    *ret = (FILE * )tmpfile();
}
extern "C" void __c__tmpnam(char * *ret, char * ___s) {
    *ret = (char * )tmpnam(___s);
}
extern "C" void __c__tmpnam_r(char * *ret, char * ___s) {
    *ret = (char * )tmpnam_r(___s);
}
extern "C" void __c__tempnam(char * *ret, char * ___dir, char * ___pfx) {
    *ret = (char * )tempnam(___dir, ___pfx);
}
extern "C" void __c__fclose(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )fclose(___stream);
}
extern "C" void __c__fflush(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )fflush(___stream);
}
extern "C" void __c__fflush_unlocked(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )fflush_unlocked(___stream);
}
extern "C" void __c__fopen(FILE * *ret, char * ___filename, char * ___modes) {
    *ret = (FILE * )fopen(___filename, ___modes);
}
extern "C" void __c__freopen(FILE * *ret, char * ___filename, char * ___modes, FILE * ___stream) {
    *ret = (FILE * )freopen(___filename, ___modes, ___stream);
}
extern "C" void __c__fdopen(FILE * *ret, int32_t ___fd, char * ___modes) {
    *ret = (FILE * )fdopen(___fd, ___modes);
}
extern "C" void __c__fmemopen(FILE * *ret, void * ___s, uint64_t ___len, char * ___modes) {
    *ret = (FILE * )fmemopen(___s, ___len, ___modes);
}
extern "C" void __c__open_memstream(FILE * *ret, char * * ___bufloc, size_t * ___sizeloc) {
    *ret = (FILE * )open_memstream(___bufloc, ___sizeloc);
}
extern "C" void __c__setbuf(FILE * ___stream, char * ___buf) {
    setbuf(___stream, ___buf);
}
extern "C" void __c__setvbuf(int32_t *ret, FILE * ___stream, char * ___buf, int32_t ___modes, uint64_t ___n) {
    *ret = (int32_t )setvbuf(___stream, ___buf, ___modes, ___n);
}
extern "C" void __c__setbuffer(FILE * ___stream, char * ___buf, uint64_t ___size) {
    setbuffer(___stream, ___buf, ___size);
}
extern "C" void __c__setlinebuf(FILE * ___stream) {
    setlinebuf(___stream);
}
extern "C" void __c__fprintf(int32_t *ret, FILE * ___stream, char * ___format) {
    *ret = (int32_t )fprintf(___stream, ___format);
}
extern "C" void __c__printf(int32_t *ret, char * ___format) {
    *ret = (int32_t )printf(___format);
}
extern "C" void __c__sprintf(int32_t *ret, char * ___s, char * ___format) {
    *ret = (int32_t )sprintf(___s, ___format);
}
extern "C" void __c__vfprintf(int32_t *ret, FILE * ___s, char * ___format, char * ___arg) {
    *ret = (int32_t )vfprintf(___s, ___format, ___arg);
}
extern "C" void __c__vprintf(int32_t *ret, char * ___format, char * ___arg) {
    *ret = (int32_t )vprintf(___format, ___arg);
}
extern "C" void __c__vsprintf(int32_t *ret, char * ___s, char * ___format, char * ___arg) {
    *ret = (int32_t )vsprintf(___s, ___format, ___arg);
}
extern "C" void __c__snprintf(int32_t *ret, char * ___s, uint64_t ___maxlen, char * ___format) {
    *ret = (int32_t )snprintf(___s, ___maxlen, ___format);
}
extern "C" void __c__vsnprintf(int32_t *ret, char * ___s, uint64_t ___maxlen, char * ___format, char * ___arg) {
    *ret = (int32_t )vsnprintf(___s, ___maxlen, ___format, ___arg);
}
extern "C" void __c__vdprintf(int32_t *ret, int32_t ___fd, char * ___fmt, char * ___arg) {
    *ret = (int32_t )vdprintf(___fd, ___fmt, ___arg);
}
extern "C" void __c__dprintf(int32_t *ret, int32_t ___fd, char * ___fmt) {
    *ret = (int32_t )dprintf(___fd, ___fmt);
}
extern "C" void __c__fscanf(int32_t *ret, FILE * ___stream, char * ___format) {
    *ret = (int32_t )fscanf(___stream, ___format);
}
extern "C" void __c__scanf(int32_t *ret, char * ___format) {
    *ret = (int32_t )scanf(___format);
}
extern "C" void __c__sscanf(int32_t *ret, char * ___s, char * ___format) {
    *ret = (int32_t )sscanf(___s, ___format);
}
extern "C" void __c__fscanf_2(int32_t *ret, FILE * ___stream, char * ___format) {
    *ret = (int32_t )fscanf(___stream, ___format);
}
extern "C" void __c__scanf_2(int32_t *ret, char * ___format) {
    *ret = (int32_t )scanf(___format);
}
extern "C" void __c__sscanf_2(int32_t *ret, char * ___s, char * ___format) {
    *ret = (int32_t )sscanf(___s, ___format);
}
extern "C" void __c__vfscanf(int32_t *ret, FILE * ___s, char * ___format, char * ___arg) {
    *ret = (int32_t )vfscanf(___s, ___format, ___arg);
}
extern "C" void __c__vscanf(int32_t *ret, char * ___format, char * ___arg) {
    *ret = (int32_t )vscanf(___format, ___arg);
}
extern "C" void __c__vsscanf(int32_t *ret, char * ___s, char * ___format, char * ___arg) {
    *ret = (int32_t )vsscanf(___s, ___format, ___arg);
}
extern "C" void __c__vfscanf_2(int32_t *ret, FILE * ___s, char * ___format, char * ___arg) {
    *ret = (int32_t )vfscanf(___s, ___format, ___arg);
}
extern "C" void __c__vscanf_2(int32_t *ret, char * ___format, char * ___arg) {
    *ret = (int32_t )vscanf(___format, ___arg);
}
extern "C" void __c__vsscanf_2(int32_t *ret, char * ___s, char * ___format, char * ___arg) {
    *ret = (int32_t )vsscanf(___s, ___format, ___arg);
}
extern "C" void __c__fgetc(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )fgetc(___stream);
}
extern "C" void __c__getc(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )getc(___stream);
}
extern "C" void __c__getchar(int32_t *ret) {
    *ret = (int32_t )getchar();
}
extern "C" void __c__getc_unlocked(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )getc_unlocked(___stream);
}
extern "C" void __c__getchar_unlocked(int32_t *ret) {
    *ret = (int32_t )getchar_unlocked();
}
extern "C" void __c__fgetc_unlocked(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )fgetc_unlocked(___stream);
}
extern "C" void __c__fputc(int32_t *ret, int32_t ___c, FILE * ___stream) {
    *ret = (int32_t )fputc(___c, ___stream);
}
extern "C" void __c__putc(int32_t *ret, int32_t ___c, FILE * ___stream) {
    *ret = (int32_t )putc(___c, ___stream);
}
extern "C" void __c__putchar(int32_t *ret, int32_t ___c) {
    *ret = (int32_t )putchar(___c);
}
extern "C" void __c__fputc_unlocked(int32_t *ret, int32_t ___c, FILE * ___stream) {
    *ret = (int32_t )fputc_unlocked(___c, ___stream);
}
extern "C" void __c__putc_unlocked(int32_t *ret, int32_t ___c, FILE * ___stream) {
    *ret = (int32_t )putc_unlocked(___c, ___stream);
}
extern "C" void __c__putchar_unlocked(int32_t *ret, int32_t ___c) {
    *ret = (int32_t )putchar_unlocked(___c);
}
extern "C" void __c__getw(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )getw(___stream);
}
extern "C" void __c__putw(int32_t *ret, int32_t ___w, FILE * ___stream) {
    *ret = (int32_t )putw(___w, ___stream);
}
extern "C" void __c__fgets(char * *ret, char * ___s, int32_t ___n, FILE * ___stream) {
    *ret = (char * )fgets(___s, ___n, ___stream);
}
extern "C" void __c____getdelim(__ssize_t *ret, char * * ___lineptr, size_t * ___n, int32_t ___delimiter, FILE * ___stream) {
    *ret = (int32_t )__getdelim(___lineptr, ___n, ___delimiter, ___stream);
}
extern "C" void __c__getdelim(__ssize_t *ret, char * * ___lineptr, size_t * ___n, int32_t ___delimiter, FILE * ___stream) {
    *ret = (int32_t )getdelim(___lineptr, ___n, ___delimiter, ___stream);
}
extern "C" void __c__getline(__ssize_t *ret, char * * ___lineptr, size_t * ___n, FILE * ___stream) {
    *ret = (int32_t )getline(___lineptr, ___n, ___stream);
}
extern "C" void __c__fputs(int32_t *ret, char * ___s, FILE * ___stream) {
    *ret = (int32_t )fputs(___s, ___stream);
}
extern "C" void __c__puts(int32_t *ret, char * ___s) {
    *ret = (int32_t )puts(___s);
}
extern "C" void __c__ungetc(int32_t *ret, int32_t ___c, FILE * ___stream) {
    *ret = (int32_t )ungetc(___c, ___stream);
}
extern "C" void __c__fread(size_t *ret, void * ___ptr, uint64_t ___size, uint64_t ___n, FILE * ___stream) {
    *ret = (uint64_t )fread(___ptr, ___size, ___n, ___stream);
}
extern "C" void __c__fwrite(size_t *ret, void * ___ptr, uint64_t ___size, uint64_t ___n, FILE * ___s) {
    *ret = (uint64_t )fwrite(___ptr, ___size, ___n, ___s);
}
extern "C" void __c__fread_unlocked(size_t *ret, void * ___ptr, uint64_t ___size, uint64_t ___n, FILE * ___stream) {
    *ret = (uint64_t )fread_unlocked(___ptr, ___size, ___n, ___stream);
}
extern "C" void __c__fwrite_unlocked(size_t *ret, void * ___ptr, uint64_t ___size, uint64_t ___n, FILE * ___stream) {
    *ret = (uint64_t )fwrite_unlocked(___ptr, ___size, ___n, ___stream);
}
extern "C" void __c__fseek(int32_t *ret, FILE * ___stream, int32_t ___off, int32_t ___whence) {
    *ret = (int32_t )fseek(___stream, ___off, ___whence);
}
extern "C" void __c__ftell(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )ftell(___stream);
}
extern "C" void __c__rewind(FILE * ___stream) {
    rewind(___stream);
}
extern "C" void __c__fseeko(int32_t *ret, FILE * ___stream, int32_t ___off, int32_t ___whence) {
    *ret = (int32_t )fseeko(___stream, ___off, ___whence);
}
extern "C" void __c__ftello(__off_t *ret, FILE * ___stream) {
    *ret = (int32_t )ftello(___stream);
}
extern "C" void __c__fgetpos(int32_t *ret, FILE * ___stream, fpos_t * ___pos) {
    *ret = (int32_t )fgetpos(___stream, ___pos);
}
extern "C" void __c__fsetpos(int32_t *ret, FILE * ___stream, const fpos_t * ___pos) {
    *ret = (int32_t )fsetpos(___stream, ___pos);
}
extern "C" void __c__clearerr(FILE * ___stream) {
    clearerr(___stream);
}
extern "C" void __c__feof(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )feof(___stream);
}
extern "C" void __c__ferror(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )ferror(___stream);
}
extern "C" void __c__clearerr_unlocked(FILE * ___stream) {
    clearerr_unlocked(___stream);
}
extern "C" void __c__feof_unlocked(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )feof_unlocked(___stream);
}
extern "C" void __c__ferror_unlocked(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )ferror_unlocked(___stream);
}
extern "C" void __c__perror(char * ___s) {
    perror(___s);
}
extern "C" void __c__fileno(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )fileno(___stream);
}
extern "C" void __c__fileno_unlocked(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )fileno_unlocked(___stream);
}
extern "C" void __c__popen(FILE * *ret, char * ___command, char * ___modes) {
    *ret = (FILE * )popen(___command, ___modes);
}
extern "C" void __c__pclose(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )pclose(___stream);
}
extern "C" void __c__ctermid(char * *ret, char * ___s) {
    *ret = (char * )ctermid(___s);
}
extern "C" void __c__flockfile(FILE * ___stream) {
    flockfile(___stream);
}
extern "C" void __c__ftrylockfile(int32_t *ret, FILE * ___stream) {
    *ret = (int32_t )ftrylockfile(___stream);
}
extern "C" void __c__funlockfile(FILE * ___stream) {
    funlockfile(___stream);
}
extern "C" void __c____ctype_get_mb_cur_max(size_t *ret) {
    *ret = (uint64_t )__ctype_get_mb_cur_max();
}
extern "C" void __c__atof(double *ret, char * ___nptr) {
    *ret = (double )atof(___nptr);
}
extern "C" void __c__atoi(int32_t *ret, char * ___nptr) {
    *ret = (int32_t )atoi(___nptr);
}
extern "C" void __c__atol(int32_t *ret, char * ___nptr) {
    *ret = (int32_t )atol(___nptr);
}
extern "C" void __c__atoll(int64_t *ret, char * ___nptr) {
    *ret = (int64_t )atoll(___nptr);
}
extern "C" void __c__strtod(double *ret, char * ___nptr, char * * ___endptr) {
    *ret = (double )strtod(___nptr, ___endptr);
}
extern "C" void __c__strtof(float *ret, char * ___nptr, char * * ___endptr) {
    *ret = (float )strtof(___nptr, ___endptr);
}
extern "C" void __c__strtold(__UNKNOWN__ *ret, char * ___nptr, char * * ___endptr) {
    *ret = (__UNKNOWN__* )strtold(___nptr, ___endptr);
}
extern "C" void __c__strtol(int32_t *ret, char * ___nptr, char * * ___endptr, int32_t ___base) {
    *ret = (int32_t )strtol(___nptr, ___endptr, ___base);
}
extern "C" void __c__strtoul(uint32_t *ret, char * ___nptr, char * * ___endptr, int32_t ___base) {
    *ret = (uint32_t )strtoul(___nptr, ___endptr, ___base);
}
extern "C" void __c__strtoq(int64_t *ret, char * ___nptr, char * * ___endptr, int32_t ___base) {
    *ret = (int64_t )strtoq(___nptr, ___endptr, ___base);
}
extern "C" void __c__strtouq(uint64_t *ret, char * ___nptr, char * * ___endptr, int32_t ___base) {
    *ret = (uint64_t )strtouq(___nptr, ___endptr, ___base);
}
extern "C" void __c__strtoll(int64_t *ret, char * ___nptr, char * * ___endptr, int32_t ___base) {
    *ret = (int64_t )strtoll(___nptr, ___endptr, ___base);
}
extern "C" void __c__strtoull(uint64_t *ret, char * ___nptr, char * * ___endptr, int32_t ___base) {
    *ret = (uint64_t )strtoull(___nptr, ___endptr, ___base);
}
extern "C" void __c__l64a(char * *ret, int32_t ___n) {
    *ret = (char * )l64a(___n);
}
extern "C" void __c__a64l(int32_t *ret, char * ___s) {
    *ret = (int32_t )a64l(___s);
}
extern "C" void __c__select(int32_t *ret, int32_t ___nfds, fd_set * ___readfds, fd_set * ___writefds, fd_set * ___exceptfds, timeval* ___timeout) {
    *ret = (int32_t )select(___nfds, ___readfds, ___writefds, ___exceptfds, ___timeout);
}
extern "C" void __c__pselect(int32_t *ret, int32_t ___nfds, fd_set * ___readfds, fd_set * ___writefds, fd_set * ___exceptfds, timespec* ___timeout, const __sigset_t * ___sigmask) {
    *ret = (int32_t )pselect(___nfds, ___readfds, ___writefds, ___exceptfds, ___timeout, ___sigmask);
}
extern "C" void __c__gnu_dev_major(uint32_t *ret, uint32_t ___dev) {
    *ret = (uint32_t )gnu_dev_major(___dev);
}
extern "C" void __c__gnu_dev_minor(uint32_t *ret, uint32_t ___dev) {
    *ret = (uint32_t )gnu_dev_minor(___dev);
}
extern "C" void __c__gnu_dev_makedev(__dev_t *ret, uint32_t ___major, uint32_t ___minor) {
    *ret = (uint32_t )gnu_dev_makedev(___major, ___minor);
}
extern "C" void __c__random(int32_t *ret) {
    *ret = (int32_t )random();
}
extern "C" void __c__srandom(uint32_t ___seed) {
    srandom(___seed);
}
extern "C" void __c__initstate(char * *ret, uint32_t ___seed, char * ___statebuf, uint64_t ___statelen) {
    *ret = (char * )initstate(___seed, ___statebuf, ___statelen);
}
extern "C" void __c__setstate(char * *ret, char * ___statebuf) {
    *ret = (char * )setstate(___statebuf);
}
extern "C" void __c__random_r(int32_t *ret, random_data* ___buf, int32_t * ___result) {
    *ret = (int32_t )random_r(___buf, ___result);
}
extern "C" void __c__srandom_r(int32_t *ret, uint32_t ___seed, random_data* ___buf) {
    *ret = (int32_t )srandom_r(___seed, ___buf);
}
extern "C" void __c__initstate_r(int32_t *ret, uint32_t ___seed, char * ___statebuf, uint64_t ___statelen, random_data* ___buf) {
    *ret = (int32_t )initstate_r(___seed, ___statebuf, ___statelen, ___buf);
}
extern "C" void __c__setstate_r(int32_t *ret, char * ___statebuf, random_data* ___buf) {
    *ret = (int32_t )setstate_r(___statebuf, ___buf);
}
extern "C" void __c__rand(int32_t *ret) {
    *ret = (int32_t )rand();
}
extern "C" void __c__srand(uint32_t ___seed) {
    srand(___seed);
}
extern "C" void __c__rand_r(int32_t *ret, uint32_t * ___seed) {
    *ret = (int32_t )rand_r(___seed);
}
extern "C" void __c__drand48(double *ret) {
    *ret = (double )drand48();
}
extern "C" void __c__erand48(double *ret, uint16_t * ___xsubi) {
    *ret = (double )erand48(___xsubi);
}
extern "C" void __c__lrand48(int32_t *ret) {
    *ret = (int32_t )lrand48();
}
extern "C" void __c__nrand48(int32_t *ret, uint16_t * ___xsubi) {
    *ret = (int32_t )nrand48(___xsubi);
}
extern "C" void __c__mrand48(int32_t *ret) {
    *ret = (int32_t )mrand48();
}
extern "C" void __c__jrand48(int32_t *ret, uint16_t * ___xsubi) {
    *ret = (int32_t )jrand48(___xsubi);
}
extern "C" void __c__srand48(int32_t ___seedval) {
    srand48(___seedval);
}
extern "C" void __c__seed48(uint16_t * *ret, uint16_t * ___seed16v) {
    *ret = (uint16_t * )seed48(___seed16v);
}
extern "C" void __c__lcong48(uint16_t * ___param) {
    lcong48(___param);
}
extern "C" void __c__drand48_r(int32_t *ret, drand48_data* ___buffer, double * ___result) {
    *ret = (int32_t )drand48_r(___buffer, ___result);
}
extern "C" void __c__erand48_r(int32_t *ret, uint16_t * ___xsubi, drand48_data* ___buffer, double * ___result) {
    *ret = (int32_t )erand48_r(___xsubi, ___buffer, ___result);
}
extern "C" void __c__lrand48_r(int32_t *ret, drand48_data* ___buffer, int32_t * ___result) {
    *ret = (int32_t )lrand48_r(___buffer, ___result);
}
extern "C" void __c__nrand48_r(int32_t *ret, uint16_t * ___xsubi, drand48_data* ___buffer, int32_t * ___result) {
    *ret = (int32_t )nrand48_r(___xsubi, ___buffer, ___result);
}
extern "C" void __c__mrand48_r(int32_t *ret, drand48_data* ___buffer, int32_t * ___result) {
    *ret = (int32_t )mrand48_r(___buffer, ___result);
}
extern "C" void __c__jrand48_r(int32_t *ret, uint16_t * ___xsubi, drand48_data* ___buffer, int32_t * ___result) {
    *ret = (int32_t )jrand48_r(___xsubi, ___buffer, ___result);
}
extern "C" void __c__srand48_r(int32_t *ret, int32_t ___seedval, drand48_data* ___buffer) {
    *ret = (int32_t )srand48_r(___seedval, ___buffer);
}
extern "C" void __c__seed48_r(int32_t *ret, uint16_t * ___seed16v, drand48_data* ___buffer) {
    *ret = (int32_t )seed48_r(___seed16v, ___buffer);
}
extern "C" void __c__lcong48_r(int32_t *ret, uint16_t * ___param, drand48_data* ___buffer) {
    *ret = (int32_t )lcong48_r(___param, ___buffer);
}
extern "C" void __c__malloc(void * *ret, uint64_t ___size) {
    *ret = (void * )malloc(___size);
}
extern "C" void __c__calloc(void * *ret, uint64_t ___nmemb, uint64_t ___size) {
    *ret = (void * )calloc(___nmemb, ___size);
}
extern "C" void __c__realloc(void * *ret, void * ___ptr, uint64_t ___size) {
    *ret = (void * )realloc(___ptr, ___size);
}
extern "C" void __c__free(void * ___ptr) {
    free(___ptr);
}
extern "C" void __c__alloca(void * *ret, uint64_t ___size) {
    *ret = (void * )alloca(___size);
}
extern "C" void __c__valloc(void * *ret, uint64_t ___size) {
    *ret = (void * )valloc(___size);
}
extern "C" void __c__posix_memalign(int32_t *ret, void * * ___memptr, uint64_t ___alignment, uint64_t ___size) {
    *ret = (int32_t )posix_memalign(___memptr, ___alignment, ___size);
}
extern "C" void __c__aligned_alloc(void * *ret, uint64_t ___alignment, uint64_t ___size) {
    *ret = (void * )aligned_alloc(___alignment, ___size);
}
extern "C" void __c__abort() {
    abort();
}
extern "C" void __c__atexit(int32_t *ret, void (*___func)()) {
    *ret = (int32_t )atexit(___func);
}
extern "C" void __c__at_quick_exit(int32_t *ret, void (*___func)()) {
    *ret = (int32_t )at_quick_exit(___func);
}
extern "C" void __c__on_exit(int32_t *ret, void (*___func)(int32_t , void * ), void * ___arg) {
    *ret = (int32_t )on_exit(___func, ___arg);
}
extern "C" void __c__exit(int32_t ___status) {
    exit(___status);
}
extern "C" void __c__quick_exit(int32_t ___status) {
    quick_exit(___status);
}
extern "C" void __c___Exit(int32_t ___status) {
    _Exit(___status);
}
extern "C" void __c__getenv(char * *ret, char * ___name) {
    *ret = (char * )getenv(___name);
}
extern "C" void __c__putenv(int32_t *ret, char * ___string) {
    *ret = (int32_t )putenv(___string);
}
extern "C" void __c__setenv(int32_t *ret, char * ___name, char * ___value, int32_t ___replace) {
    *ret = (int32_t )setenv(___name, ___value, ___replace);
}
extern "C" void __c__unsetenv(int32_t *ret, char * ___name) {
    *ret = (int32_t )unsetenv(___name);
}
extern "C" void __c__clearenv(int32_t *ret) {
    *ret = (int32_t )clearenv();
}
extern "C" void __c__mktemp(char * *ret, char * ___template) {
    *ret = (char * )mktemp(___template);
}
extern "C" void __c__mkstemp(int32_t *ret, char * ___template) {
    *ret = (int32_t )mkstemp(___template);
}
extern "C" void __c__mkstemps(int32_t *ret, char * ___template, int32_t ___suffixlen) {
    *ret = (int32_t )mkstemps(___template, ___suffixlen);
}
extern "C" void __c__mkdtemp(char * *ret, char * ___template) {
    *ret = (char * )mkdtemp(___template);
}
extern "C" void __c__system(int32_t *ret, char * ___command) {
    *ret = (int32_t )system(___command);
}
extern "C" void __c__realpath(char * *ret, char * ___name, char * ___resolved) {
    *ret = (char * )realpath(___name, ___resolved);
}
extern "C" void __c__bsearch(void * *ret, void * ___key, void * ___base, uint64_t ___nmemb, uint64_t ___size, __compar_fn_t ___compar) {
    *ret = (void * )bsearch(___key, ___base, ___nmemb, ___size, ___compar);
}
extern "C" void __c__qsort(void * ___base, uint64_t ___nmemb, uint64_t ___size, __compar_fn_t ___compar) {
    qsort(___base, ___nmemb, ___size, ___compar);
}
extern "C" void __c__abs(int32_t *ret, int32_t ___x) {
    *ret = (int32_t )abs(___x);
}
extern "C" void __c__labs(int32_t *ret, int32_t ___x) {
    *ret = (int32_t )labs(___x);
}
extern "C" void __c__llabs(int64_t *ret, int64_t ___x) {
    *ret = (int64_t )llabs(___x);
}
extern "C" void __c__div(div_t *ret, int32_t ___numer, int32_t ___denom) {
    *ret = (div_t )div(___numer, ___denom);
}
extern "C" void __c__ldiv(ldiv_t *ret, int32_t ___numer, int32_t ___denom) {
    *ret = (ldiv_t )ldiv(___numer, ___denom);
}
extern "C" void __c__lldiv(lldiv_t *ret, int64_t ___numer, int64_t ___denom) {
    *ret = (lldiv_t )lldiv(___numer, ___denom);
}
extern "C" void __c__ecvt(char * *ret, double ___value, int32_t ___ndigit, int32_t * ___decpt, int32_t * ___sign) {
    *ret = (char * )ecvt(___value, ___ndigit, ___decpt, ___sign);
}
extern "C" void __c__fcvt(char * *ret, double ___value, int32_t ___ndigit, int32_t * ___decpt, int32_t * ___sign) {
    *ret = (char * )fcvt(___value, ___ndigit, ___decpt, ___sign);
}
extern "C" void __c__gcvt(char * *ret, double ___value, int32_t ___ndigit, char * ___buf) {
    *ret = (char * )gcvt(___value, ___ndigit, ___buf);
}
extern "C" void __c__qecvt(char * *ret, __UNKNOWN__* ___value, int32_t ___ndigit, int32_t * ___decpt, int32_t * ___sign) {
    *ret = (char * )qecvt(*___value, ___ndigit, ___decpt, ___sign);
}
extern "C" void __c__qfcvt(char * *ret, __UNKNOWN__* ___value, int32_t ___ndigit, int32_t * ___decpt, int32_t * ___sign) {
    *ret = (char * )qfcvt(*___value, ___ndigit, ___decpt, ___sign);
}
extern "C" void __c__qgcvt(char * *ret, __UNKNOWN__* ___value, int32_t ___ndigit, char * ___buf) {
    *ret = (char * )qgcvt(*___value, ___ndigit, ___buf);
}
extern "C" void __c__ecvt_r(int32_t *ret, double ___value, int32_t ___ndigit, int32_t * ___decpt, int32_t * ___sign, char * ___buf, uint64_t ___len) {
    *ret = (int32_t )ecvt_r(___value, ___ndigit, ___decpt, ___sign, ___buf, ___len);
}
extern "C" void __c__fcvt_r(int32_t *ret, double ___value, int32_t ___ndigit, int32_t * ___decpt, int32_t * ___sign, char * ___buf, uint64_t ___len) {
    *ret = (int32_t )fcvt_r(___value, ___ndigit, ___decpt, ___sign, ___buf, ___len);
}
extern "C" void __c__qecvt_r(int32_t *ret, __UNKNOWN__* ___value, int32_t ___ndigit, int32_t * ___decpt, int32_t * ___sign, char * ___buf, uint64_t ___len) {
    *ret = (int32_t )qecvt_r(*___value, ___ndigit, ___decpt, ___sign, ___buf, ___len);
}
extern "C" void __c__qfcvt_r(int32_t *ret, __UNKNOWN__* ___value, int32_t ___ndigit, int32_t * ___decpt, int32_t * ___sign, char * ___buf, uint64_t ___len) {
    *ret = (int32_t )qfcvt_r(*___value, ___ndigit, ___decpt, ___sign, ___buf, ___len);
}
extern "C" void __c__mblen(int32_t *ret, char * ___s, uint64_t ___n) {
    *ret = (int32_t )mblen(___s, ___n);
}
extern "C" void __c__mbtowc(int32_t *ret, wchar_t * ___pwc, char * ___s, uint64_t ___n) {
    *ret = (int32_t )mbtowc(___pwc, ___s, ___n);
}
extern "C" void __c__wctomb(int32_t *ret, char * ___s, wchar_t ___wchar) {
    *ret = (int32_t )wctomb(___s, ___wchar);
}
extern "C" void __c__mbstowcs(size_t *ret, wchar_t * ___pwcs, char * ___s, uint64_t ___n) {
    *ret = (uint64_t )mbstowcs(___pwcs, ___s, ___n);
}
extern "C" void __c__wcstombs(size_t *ret, char * ___s, wchar_t * ___pwcs, uint64_t ___n) {
    *ret = (uint64_t )wcstombs(___s, ___pwcs, ___n);
}
extern "C" void __c__rpmatch(int32_t *ret, char * ___response) {
    *ret = (int32_t )rpmatch(___response);
}
extern "C" void __c__getsubopt(int32_t *ret, char * * ___optionp, char * * ___tokens, char * * ___valuep) {
    *ret = (int32_t )getsubopt(___optionp, ___tokens, ___valuep);
}
extern "C" void __c__getloadavg(int32_t *ret, double * ___loadavg, int32_t ___nelem) {
    *ret = (int32_t )getloadavg(___loadavg, ___nelem);
}
extern "C" void __c__stbi_load_from_memory(stbi_uc * *ret, const stbi_uc * _buffer, int32_t _len, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (stbi_uc * )stbi_load_from_memory(_buffer, _len, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_load_from_callbacks(stbi_uc * *ret, const stbi_io_callbacks * _clbk, void * _user, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
    *ret = (stbi_uc * )stbi_load_from_callbacks(_clbk, _user, _x, _y, _channels_in_file, _desired_channels);
}
extern "C" void __c__stbi_load(stbi_uc * *ret, char * _filename, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
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
extern "C" void __c__stbi_load_16(stbi_us * *ret, char * _filename, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
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
extern "C" void __c__stbi_loadf(float * *ret, char * _filename, int32_t * _x, int32_t * _y, int32_t * _channels_in_file, int32_t _desired_channels) {
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
extern "C" void __c__stbi_is_hdr(int32_t *ret, char * _filename) {
    *ret = (int32_t )stbi_is_hdr(_filename);
}
extern "C" void __c__stbi_is_hdr_from_file(int32_t *ret, FILE * _f) {
    *ret = (int32_t )stbi_is_hdr_from_file(_f);
}
extern "C" void __c__stbi_failure_reason(char * *ret) {
    *ret = (char * )stbi_failure_reason();
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
extern "C" void __c__stbi_info(int32_t *ret, char * _filename, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi_info(_filename, _x, _y, _comp);
}
extern "C" void __c__stbi_info_from_file(int32_t *ret, FILE * _f, int32_t * _x, int32_t * _y, int32_t * _comp) {
    *ret = (int32_t )stbi_info_from_file(_f, _x, _y, _comp);
}
extern "C" void __c__stbi_is_16_bit(int32_t *ret, char * _filename) {
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
extern "C" void __c__stbi_zlib_decode_malloc_guesssize(char * *ret, char * _buffer, int32_t _len, int32_t _initial_size, int32_t * _outlen) {
    *ret = (char * )stbi_zlib_decode_malloc_guesssize(_buffer, _len, _initial_size, _outlen);
}
extern "C" void __c__stbi_zlib_decode_malloc_guesssize_headerflag(char * *ret, char * _buffer, int32_t _len, int32_t _initial_size, int32_t * _outlen, int32_t _parse_header) {
    *ret = (char * )stbi_zlib_decode_malloc_guesssize_headerflag(_buffer, _len, _initial_size, _outlen, _parse_header);
}
extern "C" void __c__stbi_zlib_decode_malloc(char * *ret, char * _buffer, int32_t _len, int32_t * _outlen) {
    *ret = (char * )stbi_zlib_decode_malloc(_buffer, _len, _outlen);
}
extern "C" void __c__stbi_zlib_decode_buffer(int32_t *ret, char * _obuffer, int32_t _olen, char * _ibuffer, int32_t _ilen) {
    *ret = (int32_t )stbi_zlib_decode_buffer(_obuffer, _olen, _ibuffer, _ilen);
}
extern "C" void __c__stbi_zlib_decode_noheader_malloc(char * *ret, char * _buffer, int32_t _len, int32_t * _outlen) {
    *ret = (char * )stbi_zlib_decode_noheader_malloc(_buffer, _len, _outlen);
}
extern "C" void __c__stbi_zlib_decode_noheader_buffer(int32_t *ret, char * _obuffer, int32_t _olen, char * _ibuffer, int32_t _ilen) {
    *ret = (int32_t )stbi_zlib_decode_noheader_buffer(_obuffer, _olen, _ibuffer, _ilen);
}
extern "C" void __c__memcpy(void * *ret, void * ___dest, void * ___src, uint64_t ___n) {
    *ret = (void * )memcpy(___dest, ___src, ___n);
}
extern "C" void __c__memmove(void * *ret, void * ___dest, void * ___src, uint64_t ___n) {
    *ret = (void * )memmove(___dest, ___src, ___n);
}
extern "C" void __c__memccpy(void * *ret, void * ___dest, void * ___src, int32_t ___c, uint64_t ___n) {
    *ret = (void * )memccpy(___dest, ___src, ___c, ___n);
}
extern "C" void __c__memset(void * *ret, void * ___s, int32_t ___c, uint64_t ___n) {
    *ret = (void * )memset(___s, ___c, ___n);
}
extern "C" void __c__memcmp(int32_t *ret, void * ___s1, void * ___s2, uint64_t ___n) {
    *ret = (int32_t )memcmp(___s1, ___s2, ___n);
}
extern "C" void __c__memchr(void * *ret, void * ___s, int32_t ___c, uint64_t ___n) {
    *ret = (void * )memchr(___s, ___c, ___n);
}
extern "C" void __c__strcpy(char * *ret, char * ___dest, char * ___src) {
    *ret = (char * )strcpy(___dest, ___src);
}
extern "C" void __c__strncpy(char * *ret, char * ___dest, char * ___src, uint64_t ___n) {
    *ret = (char * )strncpy(___dest, ___src, ___n);
}
extern "C" void __c__strcat(char * *ret, char * ___dest, char * ___src) {
    *ret = (char * )strcat(___dest, ___src);
}
extern "C" void __c__strncat(char * *ret, char * ___dest, char * ___src, uint64_t ___n) {
    *ret = (char * )strncat(___dest, ___src, ___n);
}
extern "C" void __c__strcmp(int32_t *ret, char * ___s1, char * ___s2) {
    *ret = (int32_t )strcmp(___s1, ___s2);
}
extern "C" void __c__strncmp(int32_t *ret, char * ___s1, char * ___s2, uint64_t ___n) {
    *ret = (int32_t )strncmp(___s1, ___s2, ___n);
}
extern "C" void __c__strcoll(int32_t *ret, char * ___s1, char * ___s2) {
    *ret = (int32_t )strcoll(___s1, ___s2);
}
extern "C" void __c__strxfrm(size_t *ret, char * ___dest, char * ___src, uint64_t ___n) {
    *ret = (uint64_t )strxfrm(___dest, ___src, ___n);
}
extern "C" void __c__strcoll_l(int32_t *ret, char * ___s1, char * ___s2, __locale_struct* ___l) {
    *ret = (int32_t )strcoll_l(___s1, ___s2, ___l);
}
extern "C" void __c__strxfrm_l(size_t *ret, char * ___dest, char * ___src, uint64_t ___n, __locale_struct* ___l) {
    *ret = (uint64_t )strxfrm_l(___dest, ___src, ___n, ___l);
}
extern "C" void __c__strdup(char * *ret, char * ___s) {
    *ret = (char * )strdup(___s);
}
extern "C" void __c__strndup(char * *ret, char * ___string, uint64_t ___n) {
    *ret = (char * )strndup(___string, ___n);
}
extern "C" void __c__strchr(char * *ret, char * ___s, int32_t ___c) {
    *ret = (char * )strchr(___s, ___c);
}
extern "C" void __c__strrchr(char * *ret, char * ___s, int32_t ___c) {
    *ret = (char * )strrchr(___s, ___c);
}
extern "C" void __c__strcspn(size_t *ret, char * ___s, char * ___reject) {
    *ret = (uint64_t )strcspn(___s, ___reject);
}
extern "C" void __c__strspn(size_t *ret, char * ___s, char * ___accept) {
    *ret = (uint64_t )strspn(___s, ___accept);
}
extern "C" void __c__strpbrk(char * *ret, char * ___s, char * ___accept) {
    *ret = (char * )strpbrk(___s, ___accept);
}
extern "C" void __c__strstr(char * *ret, char * ___haystack, char * ___needle) {
    *ret = (char * )strstr(___haystack, ___needle);
}
extern "C" void __c__strtok(char * *ret, char * ___s, char * ___delim) {
    *ret = (char * )strtok(___s, ___delim);
}
extern "C" void __c____strtok_r(char * *ret, char * ___s, char * ___delim, char * * ___save_ptr) {
    *ret = (char * )__strtok_r(___s, ___delim, ___save_ptr);
}
extern "C" void __c__strtok_r(char * *ret, char * ___s, char * ___delim, char * * ___save_ptr) {
    *ret = (char * )strtok_r(___s, ___delim, ___save_ptr);
}
extern "C" void __c__strlen(size_t *ret, char * ___s) {
    *ret = (uint64_t )strlen(___s);
}
extern "C" void __c__strnlen(size_t *ret, char * ___string, uint64_t ___maxlen) {
    *ret = (uint64_t )strnlen(___string, ___maxlen);
}
extern "C" void __c__strerror(char * *ret, int32_t ___errnum) {
    *ret = (char * )strerror(___errnum);
}
extern "C" void __c__strerror_r(int32_t *ret, int32_t ___errnum, char * ___buf, uint64_t ___buflen) {
    *ret = (int32_t )strerror_r(___errnum, ___buf, ___buflen);
}
extern "C" void __c__strerror_l(char * *ret, int32_t ___errnum, __locale_struct* ___l) {
    *ret = (char * )strerror_l(___errnum, ___l);
}
extern "C" void __c__bcmp(int32_t *ret, void * ___s1, void * ___s2, uint64_t ___n) {
    *ret = (int32_t )bcmp(___s1, ___s2, ___n);
}
extern "C" void __c__bcopy(void * ___src, void * ___dest, uint64_t ___n) {
    bcopy(___src, ___dest, ___n);
}
extern "C" void __c__bzero(void * ___s, uint64_t ___n) {
    bzero(___s, ___n);
}
extern "C" void __c__index(char * *ret, char * ___s, int32_t ___c) {
    *ret = (char * )index(___s, ___c);
}
extern "C" void __c__rindex(char * *ret, char * ___s, int32_t ___c) {
    *ret = (char * )rindex(___s, ___c);
}
extern "C" void __c__ffs(int32_t *ret, int32_t ___i) {
    *ret = (int32_t )ffs(___i);
}
extern "C" void __c__ffsl(int32_t *ret, int32_t ___l) {
    *ret = (int32_t )ffsl(___l);
}
extern "C" void __c__ffsll(int32_t *ret, int64_t ___ll) {
    *ret = (int32_t )ffsll(___ll);
}
extern "C" void __c__strcasecmp(int32_t *ret, char * ___s1, char * ___s2) {
    *ret = (int32_t )strcasecmp(___s1, ___s2);
}
extern "C" void __c__strncasecmp(int32_t *ret, char * ___s1, char * ___s2, uint64_t ___n) {
    *ret = (int32_t )strncasecmp(___s1, ___s2, ___n);
}
extern "C" void __c__strcasecmp_l(int32_t *ret, char * ___s1, char * ___s2, __locale_struct* ___loc) {
    *ret = (int32_t )strcasecmp_l(___s1, ___s2, ___loc);
}
extern "C" void __c__strncasecmp_l(int32_t *ret, char * ___s1, char * ___s2, uint64_t ___n, __locale_struct* ___loc) {
    *ret = (int32_t )strncasecmp_l(___s1, ___s2, ___n, ___loc);
}
extern "C" void __c__explicit_bzero(void * ___s, uint64_t ___n) {
    explicit_bzero(___s, ___n);
}
extern "C" void __c__strsep(char * *ret, char * * ___stringp, char * ___delim) {
    *ret = (char * )strsep(___stringp, ___delim);
}
extern "C" void __c__strsignal(char * *ret, int32_t ___sig) {
    *ret = (char * )strsignal(___sig);
}
extern "C" void __c____stpcpy(char * *ret, char * ___dest, char * ___src) {
    *ret = (char * )__stpcpy(___dest, ___src);
}
extern "C" void __c__stpcpy(char * *ret, char * ___dest, char * ___src) {
    *ret = (char * )stpcpy(___dest, ___src);
}
extern "C" void __c____stpncpy(char * *ret, char * ___dest, char * ___src, uint64_t ___n) {
    *ret = (char * )__stpncpy(___dest, ___src, ___n);
}
extern "C" void __c__stpncpy(char * *ret, char * ___dest, char * ___src, uint64_t ___n) {
    *ret = (char * )stpncpy(___dest, ___src, ___n);
}
extern "C" void __c____fpclassify(int32_t *ret, double ___value) {
    *ret = (int32_t )__fpclassify(___value);
}
extern "C" void __c____signbit(int32_t *ret, double ___value) {
    *ret = (int32_t )__signbit(___value);
}
extern "C" void __c____isinf(int32_t *ret, double ___value) {
    *ret = (int32_t )__isinf(___value);
}
extern "C" void __c____finite(int32_t *ret, double ___value) {
    *ret = (int32_t )__finite(___value);
}
extern "C" void __c____isnan(int32_t *ret, double ___value) {
    *ret = (int32_t )__isnan(___value);
}
extern "C" void __c____iseqsig(int32_t *ret, double ___x, double ___y) {
    *ret = (int32_t )__iseqsig(___x, ___y);
}
extern "C" void __c____issignaling(int32_t *ret, double ___value) {
    *ret = (int32_t )__issignaling(___value);
}
extern "C" void __c__acos(double *ret, double ___x) {
    *ret = (double )acos(___x);
}
extern "C" void __c____acos(double *ret, double ___x) {
    *ret = (double )__acos(___x);
}
extern "C" void __c__asin(double *ret, double ___x) {
    *ret = (double )asin(___x);
}
extern "C" void __c____asin(double *ret, double ___x) {
    *ret = (double )__asin(___x);
}
extern "C" void __c__atan(double *ret, double ___x) {
    *ret = (double )atan(___x);
}
extern "C" void __c____atan(double *ret, double ___x) {
    *ret = (double )__atan(___x);
}
extern "C" void __c__atan2(double *ret, double ___y, double ___x) {
    *ret = (double )atan2(___y, ___x);
}
extern "C" void __c____atan2(double *ret, double ___y, double ___x) {
    *ret = (double )__atan2(___y, ___x);
}
extern "C" void __c__cos(double *ret, double ___x) {
    *ret = (double )cos(___x);
}
extern "C" void __c____cos(double *ret, double ___x) {
    *ret = (double )__cos(___x);
}
extern "C" void __c__sin(double *ret, double ___x) {
    *ret = (double )sin(___x);
}
extern "C" void __c____sin(double *ret, double ___x) {
    *ret = (double )__sin(___x);
}
extern "C" void __c__tan(double *ret, double ___x) {
    *ret = (double )tan(___x);
}
extern "C" void __c____tan(double *ret, double ___x) {
    *ret = (double )__tan(___x);
}
extern "C" void __c__cosh(double *ret, double ___x) {
    *ret = (double )cosh(___x);
}
extern "C" void __c____cosh(double *ret, double ___x) {
    *ret = (double )__cosh(___x);
}
extern "C" void __c__sinh(double *ret, double ___x) {
    *ret = (double )sinh(___x);
}
extern "C" void __c____sinh(double *ret, double ___x) {
    *ret = (double )__sinh(___x);
}
extern "C" void __c__tanh(double *ret, double ___x) {
    *ret = (double )tanh(___x);
}
extern "C" void __c____tanh(double *ret, double ___x) {
    *ret = (double )__tanh(___x);
}
extern "C" void __c__acosh(double *ret, double ___x) {
    *ret = (double )acosh(___x);
}
extern "C" void __c____acosh(double *ret, double ___x) {
    *ret = (double )__acosh(___x);
}
extern "C" void __c__asinh(double *ret, double ___x) {
    *ret = (double )asinh(___x);
}
extern "C" void __c____asinh(double *ret, double ___x) {
    *ret = (double )__asinh(___x);
}
extern "C" void __c__atanh(double *ret, double ___x) {
    *ret = (double )atanh(___x);
}
extern "C" void __c____atanh(double *ret, double ___x) {
    *ret = (double )__atanh(___x);
}
extern "C" void __c__exp(double *ret, double ___x) {
    *ret = (double )exp(___x);
}
extern "C" void __c____exp(double *ret, double ___x) {
    *ret = (double )__exp(___x);
}
extern "C" void __c__frexp(double *ret, double ___x, int32_t * ___exponent) {
    *ret = (double )frexp(___x, ___exponent);
}
extern "C" void __c____frexp(double *ret, double ___x, int32_t * ___exponent) {
    *ret = (double )__frexp(___x, ___exponent);
}
extern "C" void __c__ldexp(double *ret, double ___x, int32_t ___exponent) {
    *ret = (double )ldexp(___x, ___exponent);
}
extern "C" void __c____ldexp(double *ret, double ___x, int32_t ___exponent) {
    *ret = (double )__ldexp(___x, ___exponent);
}
extern "C" void __c__log(double *ret, double ___x) {
    *ret = (double )log(___x);
}
extern "C" void __c____log(double *ret, double ___x) {
    *ret = (double )__log(___x);
}
extern "C" void __c__log10(double *ret, double ___x) {
    *ret = (double )log10(___x);
}
extern "C" void __c____log10(double *ret, double ___x) {
    *ret = (double )__log10(___x);
}
extern "C" void __c__modf(double *ret, double ___x, double * ___iptr) {
    *ret = (double )modf(___x, ___iptr);
}
extern "C" void __c____modf(double *ret, double ___x, double * ___iptr) {
    *ret = (double )__modf(___x, ___iptr);
}
extern "C" void __c__expm1(double *ret, double ___x) {
    *ret = (double )expm1(___x);
}
extern "C" void __c____expm1(double *ret, double ___x) {
    *ret = (double )__expm1(___x);
}
extern "C" void __c__log1p(double *ret, double ___x) {
    *ret = (double )log1p(___x);
}
extern "C" void __c____log1p(double *ret, double ___x) {
    *ret = (double )__log1p(___x);
}
extern "C" void __c__logb(double *ret, double ___x) {
    *ret = (double )logb(___x);
}
extern "C" void __c____logb(double *ret, double ___x) {
    *ret = (double )__logb(___x);
}
extern "C" void __c__exp2(double *ret, double ___x) {
    *ret = (double )exp2(___x);
}
extern "C" void __c____exp2(double *ret, double ___x) {
    *ret = (double )__exp2(___x);
}
extern "C" void __c__log2(double *ret, double ___x) {
    *ret = (double )log2(___x);
}
extern "C" void __c____log2(double *ret, double ___x) {
    *ret = (double )__log2(___x);
}
extern "C" void __c__pow(double *ret, double ___x, double ___y) {
    *ret = (double )pow(___x, ___y);
}
extern "C" void __c____pow(double *ret, double ___x, double ___y) {
    *ret = (double )__pow(___x, ___y);
}
extern "C" void __c__sqrt(double *ret, double ___x) {
    *ret = (double )sqrt(___x);
}
extern "C" void __c____sqrt(double *ret, double ___x) {
    *ret = (double )__sqrt(___x);
}
extern "C" void __c__hypot(double *ret, double ___x, double ___y) {
    *ret = (double )hypot(___x, ___y);
}
extern "C" void __c____hypot(double *ret, double ___x, double ___y) {
    *ret = (double )__hypot(___x, ___y);
}
extern "C" void __c__cbrt(double *ret, double ___x) {
    *ret = (double )cbrt(___x);
}
extern "C" void __c____cbrt(double *ret, double ___x) {
    *ret = (double )__cbrt(___x);
}
extern "C" void __c__ceil(double *ret, double ___x) {
    *ret = (double )ceil(___x);
}
extern "C" void __c____ceil(double *ret, double ___x) {
    *ret = (double )__ceil(___x);
}
extern "C" void __c__fabs(double *ret, double ___x) {
    *ret = (double )fabs(___x);
}
extern "C" void __c____fabs(double *ret, double ___x) {
    *ret = (double )__fabs(___x);
}
extern "C" void __c__floor(double *ret, double ___x) {
    *ret = (double )floor(___x);
}
extern "C" void __c____floor(double *ret, double ___x) {
    *ret = (double )__floor(___x);
}
extern "C" void __c__fmod(double *ret, double ___x, double ___y) {
    *ret = (double )fmod(___x, ___y);
}
extern "C" void __c____fmod(double *ret, double ___x, double ___y) {
    *ret = (double )__fmod(___x, ___y);
}
extern "C" void __c__isinf(int32_t *ret, double ___value) {
    *ret = (int32_t )isinf(___value);
}
extern "C" void __c__finite(int32_t *ret, double ___value) {
    *ret = (int32_t )finite(___value);
}
extern "C" void __c__drem(double *ret, double ___x, double ___y) {
    *ret = (double )drem(___x, ___y);
}
extern "C" void __c____drem(double *ret, double ___x, double ___y) {
    *ret = (double )__drem(___x, ___y);
}
extern "C" void __c__significand(double *ret, double ___x) {
    *ret = (double )significand(___x);
}
extern "C" void __c____significand(double *ret, double ___x) {
    *ret = (double )__significand(___x);
}
extern "C" void __c__copysign(double *ret, double ___x, double ___y) {
    *ret = (double )copysign(___x, ___y);
}
extern "C" void __c____copysign(double *ret, double ___x, double ___y) {
    *ret = (double )__copysign(___x, ___y);
}
extern "C" void __c__nan(double *ret, char * ___tagb) {
    *ret = (double )nan(___tagb);
}
extern "C" void __c____nan(double *ret, char * ___tagb) {
    *ret = (double )__nan(___tagb);
}
extern "C" void __c__isnan(int32_t *ret, double ___value) {
    *ret = (int32_t )isnan(___value);
}
extern "C" void __c__j0(double *ret, double _0) {
    *ret = (double )j0(_0);
}
extern "C" void __c____j0(double *ret, double _0) {
    *ret = (double )__j0(_0);
}
extern "C" void __c__j1(double *ret, double _0) {
    *ret = (double )j1(_0);
}
extern "C" void __c____j1(double *ret, double _0) {
    *ret = (double )__j1(_0);
}
extern "C" void __c__jn(double *ret, int32_t _0, double _1) {
    *ret = (double )jn(_0, _1);
}
extern "C" void __c____jn(double *ret, int32_t _0, double _1) {
    *ret = (double )__jn(_0, _1);
}
extern "C" void __c__y0(double *ret, double _0) {
    *ret = (double )y0(_0);
}
extern "C" void __c____y0(double *ret, double _0) {
    *ret = (double )__y0(_0);
}
extern "C" void __c__y1(double *ret, double _0) {
    *ret = (double )y1(_0);
}
extern "C" void __c____y1(double *ret, double _0) {
    *ret = (double )__y1(_0);
}
extern "C" void __c__yn(double *ret, int32_t _0, double _1) {
    *ret = (double )yn(_0, _1);
}
extern "C" void __c____yn(double *ret, int32_t _0, double _1) {
    *ret = (double )__yn(_0, _1);
}
extern "C" void __c__erf(double *ret, double _0) {
    *ret = (double )erf(_0);
}
extern "C" void __c____erf(double *ret, double _0) {
    *ret = (double )__erf(_0);
}
extern "C" void __c__erfc(double *ret, double _0) {
    *ret = (double )erfc(_0);
}
extern "C" void __c____erfc(double *ret, double _0) {
    *ret = (double )__erfc(_0);
}
extern "C" void __c__lgamma(double *ret, double _0) {
    *ret = (double )lgamma(_0);
}
extern "C" void __c____lgamma(double *ret, double _0) {
    *ret = (double )__lgamma(_0);
}
extern "C" void __c__tgamma(double *ret, double _0) {
    *ret = (double )tgamma(_0);
}
extern "C" void __c____tgamma(double *ret, double _0) {
    *ret = (double )__tgamma(_0);
}
extern "C" void __c__gamma(double *ret, double _0) {
    *ret = (double )gamma(_0);
}
extern "C" void __c____gamma(double *ret, double _0) {
    *ret = (double )__gamma(_0);
}
extern "C" void __c__lgamma_r(double *ret, double _0, int32_t * ___signgamp) {
    *ret = (double )lgamma_r(_0, ___signgamp);
}
extern "C" void __c____lgamma_r(double *ret, double _0, int32_t * ___signgamp) {
    *ret = (double )__lgamma_r(_0, ___signgamp);
}
extern "C" void __c__rint(double *ret, double ___x) {
    *ret = (double )rint(___x);
}
extern "C" void __c____rint(double *ret, double ___x) {
    *ret = (double )__rint(___x);
}
extern "C" void __c__nextafter(double *ret, double ___x, double ___y) {
    *ret = (double )nextafter(___x, ___y);
}
extern "C" void __c____nextafter(double *ret, double ___x, double ___y) {
    *ret = (double )__nextafter(___x, ___y);
}
extern "C" void __c__nexttoward(double *ret, double ___x, __UNKNOWN__* ___y) {
    *ret = (double )nexttoward(___x, *___y);
}
extern "C" void __c____nexttoward(double *ret, double ___x, __UNKNOWN__* ___y) {
    *ret = (double )__nexttoward(___x, *___y);
}
extern "C" void __c__remainder(double *ret, double ___x, double ___y) {
    *ret = (double )remainder(___x, ___y);
}
extern "C" void __c____remainder(double *ret, double ___x, double ___y) {
    *ret = (double )__remainder(___x, ___y);
}
extern "C" void __c__scalbn(double *ret, double ___x, int32_t ___n) {
    *ret = (double )scalbn(___x, ___n);
}
extern "C" void __c____scalbn(double *ret, double ___x, int32_t ___n) {
    *ret = (double )__scalbn(___x, ___n);
}
extern "C" void __c__ilogb(int32_t *ret, double ___x) {
    *ret = (int32_t )ilogb(___x);
}
extern "C" void __c____ilogb(int32_t *ret, double ___x) {
    *ret = (int32_t )__ilogb(___x);
}
extern "C" void __c__scalbln(double *ret, double ___x, int32_t ___n) {
    *ret = (double )scalbln(___x, ___n);
}
extern "C" void __c____scalbln(double *ret, double ___x, int32_t ___n) {
    *ret = (double )__scalbln(___x, ___n);
}
extern "C" void __c__nearbyint(double *ret, double ___x) {
    *ret = (double )nearbyint(___x);
}
extern "C" void __c____nearbyint(double *ret, double ___x) {
    *ret = (double )__nearbyint(___x);
}
extern "C" void __c__round(double *ret, double ___x) {
    *ret = (double )round(___x);
}
extern "C" void __c____round(double *ret, double ___x) {
    *ret = (double )__round(___x);
}
extern "C" void __c__trunc(double *ret, double ___x) {
    *ret = (double )trunc(___x);
}
extern "C" void __c____trunc(double *ret, double ___x) {
    *ret = (double )__trunc(___x);
}
extern "C" void __c__remquo(double *ret, double ___x, double ___y, int32_t * ___quo) {
    *ret = (double )remquo(___x, ___y, ___quo);
}
extern "C" void __c____remquo(double *ret, double ___x, double ___y, int32_t * ___quo) {
    *ret = (double )__remquo(___x, ___y, ___quo);
}
extern "C" void __c__lrint(int32_t *ret, double ___x) {
    *ret = (int32_t )lrint(___x);
}
extern "C" void __c____lrint(int32_t *ret, double ___x) {
    *ret = (int32_t )__lrint(___x);
}
extern "C" void __c__llrint(int64_t *ret, double ___x) {
    *ret = (int64_t )llrint(___x);
}
extern "C" void __c____llrint(int64_t *ret, double ___x) {
    *ret = (int64_t )__llrint(___x);
}
extern "C" void __c__lround(int32_t *ret, double ___x) {
    *ret = (int32_t )lround(___x);
}
extern "C" void __c____lround(int32_t *ret, double ___x) {
    *ret = (int32_t )__lround(___x);
}
extern "C" void __c__llround(int64_t *ret, double ___x) {
    *ret = (int64_t )llround(___x);
}
extern "C" void __c____llround(int64_t *ret, double ___x) {
    *ret = (int64_t )__llround(___x);
}
extern "C" void __c__fdim(double *ret, double ___x, double ___y) {
    *ret = (double )fdim(___x, ___y);
}
extern "C" void __c____fdim(double *ret, double ___x, double ___y) {
    *ret = (double )__fdim(___x, ___y);
}
extern "C" void __c__fmax(double *ret, double ___x, double ___y) {
    *ret = (double )fmax(___x, ___y);
}
extern "C" void __c____fmax(double *ret, double ___x, double ___y) {
    *ret = (double )__fmax(___x, ___y);
}
extern "C" void __c__fmin(double *ret, double ___x, double ___y) {
    *ret = (double )fmin(___x, ___y);
}
extern "C" void __c____fmin(double *ret, double ___x, double ___y) {
    *ret = (double )__fmin(___x, ___y);
}
extern "C" void __c__fma(double *ret, double ___x, double ___y, double ___z) {
    *ret = (double )fma(___x, ___y, ___z);
}
extern "C" void __c____fma(double *ret, double ___x, double ___y, double ___z) {
    *ret = (double )__fma(___x, ___y, ___z);
}
extern "C" void __c__scalb(double *ret, double ___x, double ___n) {
    *ret = (double )scalb(___x, ___n);
}
extern "C" void __c____scalb(double *ret, double ___x, double ___n) {
    *ret = (double )__scalb(___x, ___n);
}
extern "C" void __c____fpclassifyf(int32_t *ret, float ___value) {
    *ret = (int32_t )__fpclassifyf(___value);
}
extern "C" void __c____signbitf(int32_t *ret, float ___value) {
    *ret = (int32_t )__signbitf(___value);
}
extern "C" void __c____isinff(int32_t *ret, float ___value) {
    *ret = (int32_t )__isinff(___value);
}
extern "C" void __c____finitef(int32_t *ret, float ___value) {
    *ret = (int32_t )__finitef(___value);
}
extern "C" void __c____isnanf(int32_t *ret, float ___value) {
    *ret = (int32_t )__isnanf(___value);
}
extern "C" void __c____iseqsigf(int32_t *ret, float ___x, float ___y) {
    *ret = (int32_t )__iseqsigf(___x, ___y);
}
extern "C" void __c____issignalingf(int32_t *ret, float ___value) {
    *ret = (int32_t )__issignalingf(___value);
}
extern "C" void __c__acosf(float *ret, float ___x) {
    *ret = (float )acosf(___x);
}
extern "C" void __c____acosf(float *ret, float ___x) {
    *ret = (float )__acosf(___x);
}
extern "C" void __c__asinf(float *ret, float ___x) {
    *ret = (float )asinf(___x);
}
extern "C" void __c____asinf(float *ret, float ___x) {
    *ret = (float )__asinf(___x);
}
extern "C" void __c__atanf(float *ret, float ___x) {
    *ret = (float )atanf(___x);
}
extern "C" void __c____atanf(float *ret, float ___x) {
    *ret = (float )__atanf(___x);
}
extern "C" void __c__atan2f(float *ret, float ___y, float ___x) {
    *ret = (float )atan2f(___y, ___x);
}
extern "C" void __c____atan2f(float *ret, float ___y, float ___x) {
    *ret = (float )__atan2f(___y, ___x);
}
extern "C" void __c__cosf(float *ret, float ___x) {
    *ret = (float )cosf(___x);
}
extern "C" void __c____cosf(float *ret, float ___x) {
    *ret = (float )__cosf(___x);
}
extern "C" void __c__sinf(float *ret, float ___x) {
    *ret = (float )sinf(___x);
}
extern "C" void __c____sinf(float *ret, float ___x) {
    *ret = (float )__sinf(___x);
}
extern "C" void __c__tanf(float *ret, float ___x) {
    *ret = (float )tanf(___x);
}
extern "C" void __c____tanf(float *ret, float ___x) {
    *ret = (float )__tanf(___x);
}
extern "C" void __c__coshf(float *ret, float ___x) {
    *ret = (float )coshf(___x);
}
extern "C" void __c____coshf(float *ret, float ___x) {
    *ret = (float )__coshf(___x);
}
extern "C" void __c__sinhf(float *ret, float ___x) {
    *ret = (float )sinhf(___x);
}
extern "C" void __c____sinhf(float *ret, float ___x) {
    *ret = (float )__sinhf(___x);
}
extern "C" void __c__tanhf(float *ret, float ___x) {
    *ret = (float )tanhf(___x);
}
extern "C" void __c____tanhf(float *ret, float ___x) {
    *ret = (float )__tanhf(___x);
}
extern "C" void __c__acoshf(float *ret, float ___x) {
    *ret = (float )acoshf(___x);
}
extern "C" void __c____acoshf(float *ret, float ___x) {
    *ret = (float )__acoshf(___x);
}
extern "C" void __c__asinhf(float *ret, float ___x) {
    *ret = (float )asinhf(___x);
}
extern "C" void __c____asinhf(float *ret, float ___x) {
    *ret = (float )__asinhf(___x);
}
extern "C" void __c__atanhf(float *ret, float ___x) {
    *ret = (float )atanhf(___x);
}
extern "C" void __c____atanhf(float *ret, float ___x) {
    *ret = (float )__atanhf(___x);
}
extern "C" void __c__expf(float *ret, float ___x) {
    *ret = (float )expf(___x);
}
extern "C" void __c____expf(float *ret, float ___x) {
    *ret = (float )__expf(___x);
}
extern "C" void __c__frexpf(float *ret, float ___x, int32_t * ___exponent) {
    *ret = (float )frexpf(___x, ___exponent);
}
extern "C" void __c____frexpf(float *ret, float ___x, int32_t * ___exponent) {
    *ret = (float )__frexpf(___x, ___exponent);
}
extern "C" void __c__ldexpf(float *ret, float ___x, int32_t ___exponent) {
    *ret = (float )ldexpf(___x, ___exponent);
}
extern "C" void __c____ldexpf(float *ret, float ___x, int32_t ___exponent) {
    *ret = (float )__ldexpf(___x, ___exponent);
}
extern "C" void __c__logf(float *ret, float ___x) {
    *ret = (float )logf(___x);
}
extern "C" void __c____logf(float *ret, float ___x) {
    *ret = (float )__logf(___x);
}
extern "C" void __c__log10f(float *ret, float ___x) {
    *ret = (float )log10f(___x);
}
extern "C" void __c____log10f(float *ret, float ___x) {
    *ret = (float )__log10f(___x);
}
extern "C" void __c__modff(float *ret, float ___x, float * ___iptr) {
    *ret = (float )modff(___x, ___iptr);
}
extern "C" void __c____modff(float *ret, float ___x, float * ___iptr) {
    *ret = (float )__modff(___x, ___iptr);
}
extern "C" void __c__expm1f(float *ret, float ___x) {
    *ret = (float )expm1f(___x);
}
extern "C" void __c____expm1f(float *ret, float ___x) {
    *ret = (float )__expm1f(___x);
}
extern "C" void __c__log1pf(float *ret, float ___x) {
    *ret = (float )log1pf(___x);
}
extern "C" void __c____log1pf(float *ret, float ___x) {
    *ret = (float )__log1pf(___x);
}
extern "C" void __c__logbf(float *ret, float ___x) {
    *ret = (float )logbf(___x);
}
extern "C" void __c____logbf(float *ret, float ___x) {
    *ret = (float )__logbf(___x);
}
extern "C" void __c__exp2f(float *ret, float ___x) {
    *ret = (float )exp2f(___x);
}
extern "C" void __c____exp2f(float *ret, float ___x) {
    *ret = (float )__exp2f(___x);
}
extern "C" void __c__log2f(float *ret, float ___x) {
    *ret = (float )log2f(___x);
}
extern "C" void __c____log2f(float *ret, float ___x) {
    *ret = (float )__log2f(___x);
}
extern "C" void __c__powf(float *ret, float ___x, float ___y) {
    *ret = (float )powf(___x, ___y);
}
extern "C" void __c____powf(float *ret, float ___x, float ___y) {
    *ret = (float )__powf(___x, ___y);
}
extern "C" void __c__sqrtf(float *ret, float ___x) {
    *ret = (float )sqrtf(___x);
}
extern "C" void __c____sqrtf(float *ret, float ___x) {
    *ret = (float )__sqrtf(___x);
}
extern "C" void __c__hypotf(float *ret, float ___x, float ___y) {
    *ret = (float )hypotf(___x, ___y);
}
extern "C" void __c____hypotf(float *ret, float ___x, float ___y) {
    *ret = (float )__hypotf(___x, ___y);
}
extern "C" void __c__cbrtf(float *ret, float ___x) {
    *ret = (float )cbrtf(___x);
}
extern "C" void __c____cbrtf(float *ret, float ___x) {
    *ret = (float )__cbrtf(___x);
}
extern "C" void __c__ceilf(float *ret, float ___x) {
    *ret = (float )ceilf(___x);
}
extern "C" void __c____ceilf(float *ret, float ___x) {
    *ret = (float )__ceilf(___x);
}
extern "C" void __c__fabsf(float *ret, float ___x) {
    *ret = (float )fabsf(___x);
}
extern "C" void __c____fabsf(float *ret, float ___x) {
    *ret = (float )__fabsf(___x);
}
extern "C" void __c__floorf(float *ret, float ___x) {
    *ret = (float )floorf(___x);
}
extern "C" void __c____floorf(float *ret, float ___x) {
    *ret = (float )__floorf(___x);
}
extern "C" void __c__fmodf(float *ret, float ___x, float ___y) {
    *ret = (float )fmodf(___x, ___y);
}
extern "C" void __c____fmodf(float *ret, float ___x, float ___y) {
    *ret = (float )__fmodf(___x, ___y);
}
extern "C" void __c__isinff(int32_t *ret, float ___value) {
    *ret = (int32_t )isinff(___value);
}
extern "C" void __c__finitef(int32_t *ret, float ___value) {
    *ret = (int32_t )finitef(___value);
}
extern "C" void __c__dremf(float *ret, float ___x, float ___y) {
    *ret = (float )dremf(___x, ___y);
}
extern "C" void __c____dremf(float *ret, float ___x, float ___y) {
    *ret = (float )__dremf(___x, ___y);
}
extern "C" void __c__significandf(float *ret, float ___x) {
    *ret = (float )significandf(___x);
}
extern "C" void __c____significandf(float *ret, float ___x) {
    *ret = (float )__significandf(___x);
}
extern "C" void __c__copysignf(float *ret, float ___x, float ___y) {
    *ret = (float )copysignf(___x, ___y);
}
extern "C" void __c____copysignf(float *ret, float ___x, float ___y) {
    *ret = (float )__copysignf(___x, ___y);
}
extern "C" void __c__nanf(float *ret, char * ___tagb) {
    *ret = (float )nanf(___tagb);
}
extern "C" void __c____nanf(float *ret, char * ___tagb) {
    *ret = (float )__nanf(___tagb);
}
extern "C" void __c__isnanf(int32_t *ret, float ___value) {
    *ret = (int32_t )isnanf(___value);
}
extern "C" void __c__j0f(float *ret, float _0) {
    *ret = (float )j0f(_0);
}
extern "C" void __c____j0f(float *ret, float _0) {
    *ret = (float )__j0f(_0);
}
extern "C" void __c__j1f(float *ret, float _0) {
    *ret = (float )j1f(_0);
}
extern "C" void __c____j1f(float *ret, float _0) {
    *ret = (float )__j1f(_0);
}
extern "C" void __c__jnf(float *ret, int32_t _0, float _1) {
    *ret = (float )jnf(_0, _1);
}
extern "C" void __c____jnf(float *ret, int32_t _0, float _1) {
    *ret = (float )__jnf(_0, _1);
}
extern "C" void __c__y0f(float *ret, float _0) {
    *ret = (float )y0f(_0);
}
extern "C" void __c____y0f(float *ret, float _0) {
    *ret = (float )__y0f(_0);
}
extern "C" void __c__y1f(float *ret, float _0) {
    *ret = (float )y1f(_0);
}
extern "C" void __c____y1f(float *ret, float _0) {
    *ret = (float )__y1f(_0);
}
extern "C" void __c__ynf(float *ret, int32_t _0, float _1) {
    *ret = (float )ynf(_0, _1);
}
extern "C" void __c____ynf(float *ret, int32_t _0, float _1) {
    *ret = (float )__ynf(_0, _1);
}
extern "C" void __c__erff(float *ret, float _0) {
    *ret = (float )erff(_0);
}
extern "C" void __c____erff(float *ret, float _0) {
    *ret = (float )__erff(_0);
}
extern "C" void __c__erfcf(float *ret, float _0) {
    *ret = (float )erfcf(_0);
}
extern "C" void __c____erfcf(float *ret, float _0) {
    *ret = (float )__erfcf(_0);
}
extern "C" void __c__lgammaf(float *ret, float _0) {
    *ret = (float )lgammaf(_0);
}
extern "C" void __c____lgammaf(float *ret, float _0) {
    *ret = (float )__lgammaf(_0);
}
extern "C" void __c__tgammaf(float *ret, float _0) {
    *ret = (float )tgammaf(_0);
}
extern "C" void __c____tgammaf(float *ret, float _0) {
    *ret = (float )__tgammaf(_0);
}
extern "C" void __c__gammaf(float *ret, float _0) {
    *ret = (float )gammaf(_0);
}
extern "C" void __c____gammaf(float *ret, float _0) {
    *ret = (float )__gammaf(_0);
}
extern "C" void __c__lgammaf_r(float *ret, float _0, int32_t * ___signgamp) {
    *ret = (float )lgammaf_r(_0, ___signgamp);
}
extern "C" void __c____lgammaf_r(float *ret, float _0, int32_t * ___signgamp) {
    *ret = (float )__lgammaf_r(_0, ___signgamp);
}
extern "C" void __c__rintf(float *ret, float ___x) {
    *ret = (float )rintf(___x);
}
extern "C" void __c____rintf(float *ret, float ___x) {
    *ret = (float )__rintf(___x);
}
extern "C" void __c__nextafterf(float *ret, float ___x, float ___y) {
    *ret = (float )nextafterf(___x, ___y);
}
extern "C" void __c____nextafterf(float *ret, float ___x, float ___y) {
    *ret = (float )__nextafterf(___x, ___y);
}
extern "C" void __c__nexttowardf(float *ret, float ___x, __UNKNOWN__* ___y) {
    *ret = (float )nexttowardf(___x, *___y);
}
extern "C" void __c____nexttowardf(float *ret, float ___x, __UNKNOWN__* ___y) {
    *ret = (float )__nexttowardf(___x, *___y);
}
extern "C" void __c__remainderf(float *ret, float ___x, float ___y) {
    *ret = (float )remainderf(___x, ___y);
}
extern "C" void __c____remainderf(float *ret, float ___x, float ___y) {
    *ret = (float )__remainderf(___x, ___y);
}
extern "C" void __c__scalbnf(float *ret, float ___x, int32_t ___n) {
    *ret = (float )scalbnf(___x, ___n);
}
extern "C" void __c____scalbnf(float *ret, float ___x, int32_t ___n) {
    *ret = (float )__scalbnf(___x, ___n);
}
extern "C" void __c__ilogbf(int32_t *ret, float ___x) {
    *ret = (int32_t )ilogbf(___x);
}
extern "C" void __c____ilogbf(int32_t *ret, float ___x) {
    *ret = (int32_t )__ilogbf(___x);
}
extern "C" void __c__scalblnf(float *ret, float ___x, int32_t ___n) {
    *ret = (float )scalblnf(___x, ___n);
}
extern "C" void __c____scalblnf(float *ret, float ___x, int32_t ___n) {
    *ret = (float )__scalblnf(___x, ___n);
}
extern "C" void __c__nearbyintf(float *ret, float ___x) {
    *ret = (float )nearbyintf(___x);
}
extern "C" void __c____nearbyintf(float *ret, float ___x) {
    *ret = (float )__nearbyintf(___x);
}
extern "C" void __c__roundf(float *ret, float ___x) {
    *ret = (float )roundf(___x);
}
extern "C" void __c____roundf(float *ret, float ___x) {
    *ret = (float )__roundf(___x);
}
extern "C" void __c__truncf(float *ret, float ___x) {
    *ret = (float )truncf(___x);
}
extern "C" void __c____truncf(float *ret, float ___x) {
    *ret = (float )__truncf(___x);
}
extern "C" void __c__remquof(float *ret, float ___x, float ___y, int32_t * ___quo) {
    *ret = (float )remquof(___x, ___y, ___quo);
}
extern "C" void __c____remquof(float *ret, float ___x, float ___y, int32_t * ___quo) {
    *ret = (float )__remquof(___x, ___y, ___quo);
}
extern "C" void __c__lrintf(int32_t *ret, float ___x) {
    *ret = (int32_t )lrintf(___x);
}
extern "C" void __c____lrintf(int32_t *ret, float ___x) {
    *ret = (int32_t )__lrintf(___x);
}
extern "C" void __c__llrintf(int64_t *ret, float ___x) {
    *ret = (int64_t )llrintf(___x);
}
extern "C" void __c____llrintf(int64_t *ret, float ___x) {
    *ret = (int64_t )__llrintf(___x);
}
extern "C" void __c__lroundf(int32_t *ret, float ___x) {
    *ret = (int32_t )lroundf(___x);
}
extern "C" void __c____lroundf(int32_t *ret, float ___x) {
    *ret = (int32_t )__lroundf(___x);
}
extern "C" void __c__llroundf(int64_t *ret, float ___x) {
    *ret = (int64_t )llroundf(___x);
}
extern "C" void __c____llroundf(int64_t *ret, float ___x) {
    *ret = (int64_t )__llroundf(___x);
}
extern "C" void __c__fdimf(float *ret, float ___x, float ___y) {
    *ret = (float )fdimf(___x, ___y);
}
extern "C" void __c____fdimf(float *ret, float ___x, float ___y) {
    *ret = (float )__fdimf(___x, ___y);
}
extern "C" void __c__fmaxf(float *ret, float ___x, float ___y) {
    *ret = (float )fmaxf(___x, ___y);
}
extern "C" void __c____fmaxf(float *ret, float ___x, float ___y) {
    *ret = (float )__fmaxf(___x, ___y);
}
extern "C" void __c__fminf(float *ret, float ___x, float ___y) {
    *ret = (float )fminf(___x, ___y);
}
extern "C" void __c____fminf(float *ret, float ___x, float ___y) {
    *ret = (float )__fminf(___x, ___y);
}
extern "C" void __c__fmaf(float *ret, float ___x, float ___y, float ___z) {
    *ret = (float )fmaf(___x, ___y, ___z);
}
extern "C" void __c____fmaf(float *ret, float ___x, float ___y, float ___z) {
    *ret = (float )__fmaf(___x, ___y, ___z);
}
extern "C" void __c__scalbf(float *ret, float ___x, float ___n) {
    *ret = (float )scalbf(___x, ___n);
}
extern "C" void __c____scalbf(float *ret, float ___x, float ___n) {
    *ret = (float )__scalbf(___x, ___n);
}
extern "C" void __c____fpclassifyl(int32_t *ret, __UNKNOWN__* ___value) {
    *ret = (int32_t )__fpclassifyl(*___value);
}
extern "C" void __c____signbitl(int32_t *ret, __UNKNOWN__* ___value) {
    *ret = (int32_t )__signbitl(*___value);
}
extern "C" void __c____isinfl(int32_t *ret, __UNKNOWN__* ___value) {
    *ret = (int32_t )__isinfl(*___value);
}
extern "C" void __c____finitel(int32_t *ret, __UNKNOWN__* ___value) {
    *ret = (int32_t )__finitel(*___value);
}
extern "C" void __c____isnanl(int32_t *ret, __UNKNOWN__* ___value) {
    *ret = (int32_t )__isnanl(*___value);
}
extern "C" void __c____iseqsigl(int32_t *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (int32_t )__iseqsigl(*___x, *___y);
}
extern "C" void __c____issignalingl(int32_t *ret, __UNKNOWN__* ___value) {
    *ret = (int32_t )__issignalingl(*___value);
}
extern "C" void __c__acosl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )acosl(*___x);
}
extern "C" void __c____acosl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__acosl(*___x);
}
extern "C" void __c__asinl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )asinl(*___x);
}
extern "C" void __c____asinl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__asinl(*___x);
}
extern "C" void __c__atanl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )atanl(*___x);
}
extern "C" void __c____atanl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__atanl(*___x);
}
extern "C" void __c__atan2l(__UNKNOWN__ *ret, __UNKNOWN__* ___y, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )atan2l(*___y, *___x);
}
extern "C" void __c____atan2l(__UNKNOWN__ *ret, __UNKNOWN__* ___y, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__atan2l(*___y, *___x);
}
extern "C" void __c__cosl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )cosl(*___x);
}
extern "C" void __c____cosl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__cosl(*___x);
}
extern "C" void __c__sinl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )sinl(*___x);
}
extern "C" void __c____sinl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__sinl(*___x);
}
extern "C" void __c__tanl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )tanl(*___x);
}
extern "C" void __c____tanl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__tanl(*___x);
}
extern "C" void __c__coshl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )coshl(*___x);
}
extern "C" void __c____coshl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__coshl(*___x);
}
extern "C" void __c__sinhl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )sinhl(*___x);
}
extern "C" void __c____sinhl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__sinhl(*___x);
}
extern "C" void __c__tanhl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )tanhl(*___x);
}
extern "C" void __c____tanhl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__tanhl(*___x);
}
extern "C" void __c__acoshl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )acoshl(*___x);
}
extern "C" void __c____acoshl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__acoshl(*___x);
}
extern "C" void __c__asinhl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )asinhl(*___x);
}
extern "C" void __c____asinhl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__asinhl(*___x);
}
extern "C" void __c__atanhl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )atanhl(*___x);
}
extern "C" void __c____atanhl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__atanhl(*___x);
}
extern "C" void __c__expl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )expl(*___x);
}
extern "C" void __c____expl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__expl(*___x);
}
extern "C" void __c__frexpl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, int32_t * ___exponent) {
    *ret = (__UNKNOWN__* )frexpl(*___x, ___exponent);
}
extern "C" void __c____frexpl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, int32_t * ___exponent) {
    *ret = (__UNKNOWN__* )__frexpl(*___x, ___exponent);
}
extern "C" void __c__ldexpl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, int32_t ___exponent) {
    *ret = (__UNKNOWN__* )ldexpl(*___x, ___exponent);
}
extern "C" void __c____ldexpl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, int32_t ___exponent) {
    *ret = (__UNKNOWN__* )__ldexpl(*___x, ___exponent);
}
extern "C" void __c__logl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )logl(*___x);
}
extern "C" void __c____logl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__logl(*___x);
}
extern "C" void __c__log10l(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )log10l(*___x);
}
extern "C" void __c____log10l(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__log10l(*___x);
}
extern "C" void __c__modfl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__ * ___iptr) {
    *ret = (__UNKNOWN__* )modfl(*___x, ___iptr);
}
extern "C" void __c____modfl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__ * ___iptr) {
    *ret = (__UNKNOWN__* )__modfl(*___x, ___iptr);
}
extern "C" void __c__expm1l(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )expm1l(*___x);
}
extern "C" void __c____expm1l(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__expm1l(*___x);
}
extern "C" void __c__log1pl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )log1pl(*___x);
}
extern "C" void __c____log1pl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__log1pl(*___x);
}
extern "C" void __c__logbl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )logbl(*___x);
}
extern "C" void __c____logbl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__logbl(*___x);
}
extern "C" void __c__exp2l(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )exp2l(*___x);
}
extern "C" void __c____exp2l(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__exp2l(*___x);
}
extern "C" void __c__log2l(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )log2l(*___x);
}
extern "C" void __c____log2l(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__log2l(*___x);
}
extern "C" void __c__powl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )powl(*___x, *___y);
}
extern "C" void __c____powl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )__powl(*___x, *___y);
}
extern "C" void __c__sqrtl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )sqrtl(*___x);
}
extern "C" void __c____sqrtl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__sqrtl(*___x);
}
extern "C" void __c__hypotl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )hypotl(*___x, *___y);
}
extern "C" void __c____hypotl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )__hypotl(*___x, *___y);
}
extern "C" void __c__cbrtl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )cbrtl(*___x);
}
extern "C" void __c____cbrtl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__cbrtl(*___x);
}
extern "C" void __c__ceill(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )ceill(*___x);
}
extern "C" void __c____ceill(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__ceill(*___x);
}
extern "C" void __c__fabsl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )fabsl(*___x);
}
extern "C" void __c____fabsl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__fabsl(*___x);
}
extern "C" void __c__floorl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )floorl(*___x);
}
extern "C" void __c____floorl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__floorl(*___x);
}
extern "C" void __c__fmodl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )fmodl(*___x, *___y);
}
extern "C" void __c____fmodl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )__fmodl(*___x, *___y);
}
extern "C" void __c__isinfl(int32_t *ret, __UNKNOWN__* ___value) {
    *ret = (int32_t )isinfl(*___value);
}
extern "C" void __c__finitel(int32_t *ret, __UNKNOWN__* ___value) {
    *ret = (int32_t )finitel(*___value);
}
extern "C" void __c__dreml(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )dreml(*___x, *___y);
}
extern "C" void __c____dreml(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )__dreml(*___x, *___y);
}
extern "C" void __c__significandl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )significandl(*___x);
}
extern "C" void __c____significandl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__significandl(*___x);
}
extern "C" void __c__copysignl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )copysignl(*___x, *___y);
}
extern "C" void __c____copysignl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )__copysignl(*___x, *___y);
}
extern "C" void __c__nanl(__UNKNOWN__ *ret, char * ___tagb) {
    *ret = (__UNKNOWN__* )nanl(___tagb);
}
extern "C" void __c____nanl(__UNKNOWN__ *ret, char * ___tagb) {
    *ret = (__UNKNOWN__* )__nanl(___tagb);
}
extern "C" void __c__isnanl(int32_t *ret, __UNKNOWN__* ___value) {
    *ret = (int32_t )isnanl(*___value);
}
extern "C" void __c__j0l(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )j0l(*_0);
}
extern "C" void __c____j0l(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )__j0l(*_0);
}
extern "C" void __c__j1l(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )j1l(*_0);
}
extern "C" void __c____j1l(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )__j1l(*_0);
}
extern "C" void __c__jnl(__UNKNOWN__ *ret, int32_t _0, __UNKNOWN__* _1) {
    *ret = (__UNKNOWN__* )jnl(_0, *_1);
}
extern "C" void __c____jnl(__UNKNOWN__ *ret, int32_t _0, __UNKNOWN__* _1) {
    *ret = (__UNKNOWN__* )__jnl(_0, *_1);
}
extern "C" void __c__y0l(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )y0l(*_0);
}
extern "C" void __c____y0l(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )__y0l(*_0);
}
extern "C" void __c__y1l(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )y1l(*_0);
}
extern "C" void __c____y1l(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )__y1l(*_0);
}
extern "C" void __c__ynl(__UNKNOWN__ *ret, int32_t _0, __UNKNOWN__* _1) {
    *ret = (__UNKNOWN__* )ynl(_0, *_1);
}
extern "C" void __c____ynl(__UNKNOWN__ *ret, int32_t _0, __UNKNOWN__* _1) {
    *ret = (__UNKNOWN__* )__ynl(_0, *_1);
}
extern "C" void __c__erfl(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )erfl(*_0);
}
extern "C" void __c____erfl(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )__erfl(*_0);
}
extern "C" void __c__erfcl(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )erfcl(*_0);
}
extern "C" void __c____erfcl(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )__erfcl(*_0);
}
extern "C" void __c__lgammal(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )lgammal(*_0);
}
extern "C" void __c____lgammal(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )__lgammal(*_0);
}
extern "C" void __c__tgammal(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )tgammal(*_0);
}
extern "C" void __c____tgammal(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )__tgammal(*_0);
}
extern "C" void __c__gammal(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )gammal(*_0);
}
extern "C" void __c____gammal(__UNKNOWN__ *ret, __UNKNOWN__* _0) {
    *ret = (__UNKNOWN__* )__gammal(*_0);
}
extern "C" void __c__lgammal_r(__UNKNOWN__ *ret, __UNKNOWN__* _0, int32_t * ___signgamp) {
    *ret = (__UNKNOWN__* )lgammal_r(*_0, ___signgamp);
}
extern "C" void __c____lgammal_r(__UNKNOWN__ *ret, __UNKNOWN__* _0, int32_t * ___signgamp) {
    *ret = (__UNKNOWN__* )__lgammal_r(*_0, ___signgamp);
}
extern "C" void __c__rintl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )rintl(*___x);
}
extern "C" void __c____rintl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__rintl(*___x);
}
extern "C" void __c__nextafterl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )nextafterl(*___x, *___y);
}
extern "C" void __c____nextafterl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )__nextafterl(*___x, *___y);
}
extern "C" void __c__nexttowardl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )nexttowardl(*___x, *___y);
}
extern "C" void __c____nexttowardl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )__nexttowardl(*___x, *___y);
}
extern "C" void __c__remainderl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )remainderl(*___x, *___y);
}
extern "C" void __c____remainderl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )__remainderl(*___x, *___y);
}
extern "C" void __c__scalbnl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, int32_t ___n) {
    *ret = (__UNKNOWN__* )scalbnl(*___x, ___n);
}
extern "C" void __c____scalbnl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, int32_t ___n) {
    *ret = (__UNKNOWN__* )__scalbnl(*___x, ___n);
}
extern "C" void __c__ilogbl(int32_t *ret, __UNKNOWN__* ___x) {
    *ret = (int32_t )ilogbl(*___x);
}
extern "C" void __c____ilogbl(int32_t *ret, __UNKNOWN__* ___x) {
    *ret = (int32_t )__ilogbl(*___x);
}
extern "C" void __c__scalblnl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, int32_t ___n) {
    *ret = (__UNKNOWN__* )scalblnl(*___x, ___n);
}
extern "C" void __c____scalblnl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, int32_t ___n) {
    *ret = (__UNKNOWN__* )__scalblnl(*___x, ___n);
}
extern "C" void __c__nearbyintl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )nearbyintl(*___x);
}
extern "C" void __c____nearbyintl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__nearbyintl(*___x);
}
extern "C" void __c__roundl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )roundl(*___x);
}
extern "C" void __c____roundl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__roundl(*___x);
}
extern "C" void __c__truncl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )truncl(*___x);
}
extern "C" void __c____truncl(__UNKNOWN__ *ret, __UNKNOWN__* ___x) {
    *ret = (__UNKNOWN__* )__truncl(*___x);
}
extern "C" void __c__remquol(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y, int32_t * ___quo) {
    *ret = (__UNKNOWN__* )remquol(*___x, *___y, ___quo);
}
extern "C" void __c____remquol(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y, int32_t * ___quo) {
    *ret = (__UNKNOWN__* )__remquol(*___x, *___y, ___quo);
}
extern "C" void __c__lrintl(int32_t *ret, __UNKNOWN__* ___x) {
    *ret = (int32_t )lrintl(*___x);
}
extern "C" void __c____lrintl(int32_t *ret, __UNKNOWN__* ___x) {
    *ret = (int32_t )__lrintl(*___x);
}
extern "C" void __c__llrintl(int64_t *ret, __UNKNOWN__* ___x) {
    *ret = (int64_t )llrintl(*___x);
}
extern "C" void __c____llrintl(int64_t *ret, __UNKNOWN__* ___x) {
    *ret = (int64_t )__llrintl(*___x);
}
extern "C" void __c__lroundl(int32_t *ret, __UNKNOWN__* ___x) {
    *ret = (int32_t )lroundl(*___x);
}
extern "C" void __c____lroundl(int32_t *ret, __UNKNOWN__* ___x) {
    *ret = (int32_t )__lroundl(*___x);
}
extern "C" void __c__llroundl(int64_t *ret, __UNKNOWN__* ___x) {
    *ret = (int64_t )llroundl(*___x);
}
extern "C" void __c____llroundl(int64_t *ret, __UNKNOWN__* ___x) {
    *ret = (int64_t )__llroundl(*___x);
}
extern "C" void __c__fdiml(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )fdiml(*___x, *___y);
}
extern "C" void __c____fdiml(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )__fdiml(*___x, *___y);
}
extern "C" void __c__fmaxl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )fmaxl(*___x, *___y);
}
extern "C" void __c____fmaxl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )__fmaxl(*___x, *___y);
}
extern "C" void __c__fminl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )fminl(*___x, *___y);
}
extern "C" void __c____fminl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y) {
    *ret = (__UNKNOWN__* )__fminl(*___x, *___y);
}
extern "C" void __c__fmal(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y, __UNKNOWN__* ___z) {
    *ret = (__UNKNOWN__* )fmal(*___x, *___y, *___z);
}
extern "C" void __c____fmal(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___y, __UNKNOWN__* ___z) {
    *ret = (__UNKNOWN__* )__fmal(*___x, *___y, *___z);
}
extern "C" void __c__scalbl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___n) {
    *ret = (__UNKNOWN__* )scalbl(*___x, *___n);
}
extern "C" void __c____scalbl(__UNKNOWN__ *ret, __UNKNOWN__* ___x, __UNKNOWN__* ___n) {
    *ret = (__UNKNOWN__* )__scalbl(*___x, *___n);
}
extern "C" void __c____assert_fail(char * ___assertion, char * ___file, uint32_t ___line, char * ___function) {
    __assert_fail(___assertion, ___file, ___line, ___function);
}
extern "C" void __c____assert_perror_fail(int32_t ___errnum, char * ___file, uint32_t ___line, char * ___function) {
    __assert_perror_fail(___errnum, ___file, ___line, ___function);
}
extern "C" void __c____assert(char * ___assertion, char * ___file, int32_t ___line) {
    __assert(___assertion, ___file, ___line);
}
extern "C" void __c__posix_memalign_2(int32_t *ret, void * * ___memptr, uint64_t ___alignment, uint64_t ___size) {
    *ret = (int32_t )posix_memalign(___memptr, ___alignment, ___size);
}
extern "C" void __c___mm_sfence() {
    _mm_sfence();
}
extern "C" void __c___mm_getcsr(uint32_t *ret) {
    *ret = (uint32_t )_mm_getcsr();
}
extern "C" void __c___mm_setcsr(uint32_t ___i) {
    _mm_setcsr(___i);
}
extern "C" void __c___mm_clflush(void * ___p) {
    _mm_clflush(___p);
}
extern "C" void __c___mm_lfence() {
    _mm_lfence();
}
extern "C" void __c___mm_mfence() {
    _mm_mfence();
}
extern "C" void __c___mm_pause() {
    _mm_pause();
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
