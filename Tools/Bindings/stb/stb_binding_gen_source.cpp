typedef long unsigned int size_t;

typedef unsigned char __u_char;
typedef unsigned short int __u_short;
typedef unsigned int __u_int;
typedef unsigned long int __u_long;

typedef signed char __int8_t;
typedef unsigned char __uint8_t;
typedef signed short int __int16_t;
typedef unsigned short int __uint16_t;
typedef signed int __int32_t;
typedef unsigned int __uint32_t;

typedef signed long int __int64_t;
typedef unsigned long int __uint64_t;

typedef long int __quad_t;
typedef unsigned long int __u_quad_t;

typedef long int __intmax_t;
typedef unsigned long int __uintmax_t;

typedef unsigned long int __dev_t;
typedef unsigned int __uid_t;
typedef unsigned int __gid_t;
typedef unsigned long int __ino_t;
typedef unsigned long int __ino64_t;
typedef unsigned int __mode_t;
typedef unsigned long int __nlink_t;
typedef long int __off_t;
typedef long int __off64_t;
typedef int __pid_t;
typedef struct { int __val[2]; } __fsid_t;
typedef long int __clock_t;
typedef unsigned long int __rlim_t;
typedef unsigned long int __rlim64_t;
typedef unsigned int __id_t;
typedef long int __time_t;
typedef unsigned int __useconds_t;
typedef long int __suseconds_t;

typedef int __daddr_t;
typedef int __key_t;

typedef int __clockid_t;

typedef void* __timer_t;

typedef long int __blksize_t;

typedef long int __blkcnt_t;
typedef long int __blkcnt64_t;

typedef unsigned long int __fsblkcnt_t;
typedef unsigned long int __fsblkcnt64_t;

typedef unsigned long int __fsfilcnt_t;
typedef unsigned long int __fsfilcnt64_t;

typedef long int __fsword_t;

typedef long int __ssize_t;

typedef long int __syscall_slong_t;

typedef unsigned long int __syscall_ulong_t;

typedef __off64_t __loff_t;
typedef char* __caddr_t;

typedef long int __intptr_t;

typedef unsigned int __socklen_t;

typedef int __sig_atomic_t;

struct _IO_FILE;
typedef struct _IO_FILE __FILE;

struct _IO_FILE;

typedef struct _IO_FILE FILE;

typedef struct
{
    int __count;
    union
    {
        unsigned int __wch;
        char __wchb[4];
    } __value;
} __mbstate_t;

typedef struct
{
    __off_t __pos;
    __mbstate_t __state;
} _G_fpos_t;
typedef struct
{
    __off64_t __pos;
    __mbstate_t __state;
} _G_fpos64_t;
typedef __builtin_va_list va_list;
typedef __builtin_va_list __gnuc_va_list;
struct _IO_jump_t; struct _IO_FILE;

typedef void _IO_lock_t;

struct _IO_marker {
    struct _IO_marker* _next;
    struct _IO_FILE* _sbuf;

    int _pos;
};

enum __codecvt_result
{
    __codecvt_ok,
    __codecvt_partial,
    __codecvt_error,
    __codecvt_noconv
};
struct _IO_FILE {
    int _flags;

    char* _IO_read_ptr;
    char* _IO_read_end;
    char* _IO_read_base;
    char* _IO_write_base;
    char* _IO_write_ptr;
    char* _IO_write_end;
    char* _IO_buf_base;
    char* _IO_buf_end;

    char* _IO_save_base;
    char* _IO_backup_base;
    char* _IO_save_end;

    struct _IO_marker* _markers;

    struct _IO_FILE* _chain;

    int _fileno;

    int _flags2;

    __off_t _old_offset;

    unsigned short _cur_column;
    signed char _vtable_offset;
    char _shortbuf[1];

    _IO_lock_t* _lock;
    __off64_t _offset;

    void* __pad1;
    void* __pad2;
    void* __pad3;
    void* __pad4;

    size_t __pad5;
    int _mode;

    char _unused2[15 * sizeof(int) - 4 * sizeof(void*) - sizeof(size_t)];

};

typedef struct _IO_FILE _IO_FILE;

struct _IO_FILE_plus;

extern struct _IO_FILE_plus _IO_2_1_stdin_;
extern struct _IO_FILE_plus _IO_2_1_stdout_;
extern struct _IO_FILE_plus _IO_2_1_stderr_;
typedef __ssize_t __io_read_fn(void* __cookie, char* __buf, size_t __nbytes);

typedef __ssize_t __io_write_fn(void* __cookie, const char* __buf,
    size_t __n);

typedef int __io_seek_fn(void* __cookie, __off64_t* __pos, int __w);

typedef int __io_close_fn(void* __cookie);
extern int __underflow(_IO_FILE*);
extern int __uflow(_IO_FILE*);
extern int __overflow(_IO_FILE*, int);
extern int _IO_getc(_IO_FILE* __fp);
extern int _IO_putc(int __c, _IO_FILE* __fp);
extern int _IO_feof(_IO_FILE* __fp) __attribute__((__nothrow__));
extern int _IO_ferror(_IO_FILE* __fp) __attribute__((__nothrow__));

extern int _IO_peekc_locked(_IO_FILE* __fp);

extern void _IO_flockfile(_IO_FILE*) __attribute__((__nothrow__));
extern void _IO_funlockfile(_IO_FILE*) __attribute__((__nothrow__));
extern int _IO_ftrylockfile(_IO_FILE*) __attribute__((__nothrow__));
extern int _IO_vfscanf(_IO_FILE* __restrict, const char* __restrict,
    __gnuc_va_list, int* __restrict);
extern int _IO_vfprintf(_IO_FILE* __restrict, const char* __restrict,
    __gnuc_va_list);
extern __ssize_t _IO_padn(_IO_FILE*, int, __ssize_t);
extern size_t _IO_sgetn(_IO_FILE*, void*, size_t);

extern __off64_t _IO_seekoff(_IO_FILE*, __off64_t, int, int);
extern __off64_t _IO_seekpos(_IO_FILE*, __off64_t, int);

extern void _IO_free_backup_area(_IO_FILE*) __attribute__((__nothrow__));

typedef __gnuc_va_list va_list;
typedef __off_t off_t;
typedef __ssize_t ssize_t;

typedef _G_fpos_t fpos_t;

extern struct _IO_FILE* stdin;
extern struct _IO_FILE* stdout;
extern struct _IO_FILE* stderr;

extern int remove(const char* __filename) __attribute__((__nothrow__));

extern int rename(const char* __old, const char* __new) __attribute__((__nothrow__));

extern int renameat(int __oldfd, const char* __old, int __newfd,
    const char* __new) __attribute__((__nothrow__));

extern FILE* tmpfile(void);
extern char* tmpnam(char* __s) __attribute__((__nothrow__));

extern char* tmpnam_r(char* __s) __attribute__((__nothrow__));
extern char* tempnam(const char* __dir, const char* __pfx)
__attribute__((__nothrow__)) __attribute__((__malloc__));

extern int fclose(FILE* __stream);

extern int fflush(FILE* __stream);
extern int fflush_unlocked(FILE* __stream);
extern FILE* fopen(const char* __restrict __filename,
    const char* __restrict __modes);

extern FILE* freopen(const char* __restrict __filename,
    const char* __restrict __modes,
    FILE* __restrict __stream);
extern FILE* fdopen(int __fd, const char* __modes) __attribute__((__nothrow__));
extern FILE* fmemopen(void* __s, size_t __len, const char* __modes)
__attribute__((__nothrow__));

extern FILE* open_memstream(char** __bufloc, size_t* __sizeloc) __attribute__((__nothrow__));

extern void setbuf(FILE* __restrict __stream, char* __restrict __buf) __attribute__((__nothrow__));

extern int setvbuf(FILE* __restrict __stream, char* __restrict __buf,
    int __modes, size_t __n) __attribute__((__nothrow__));

extern void setbuffer(FILE* __restrict __stream, char* __restrict __buf,
    size_t __size) __attribute__((__nothrow__));

extern void setlinebuf(FILE* __stream) __attribute__((__nothrow__));

extern int fprintf(FILE* __restrict __stream,
    const char* __restrict __format, ...);

extern int printf(const char* __restrict __format, ...);

extern int sprintf(char* __restrict __s,
    const char* __restrict __format, ...) __attribute__((__nothrow__));

extern int vfprintf(FILE* __restrict __s, const char* __restrict __format,
    __gnuc_va_list __arg);

extern int vprintf(const char* __restrict __format, __gnuc_va_list __arg);

extern int vsprintf(char* __restrict __s, const char* __restrict __format,
    __gnuc_va_list __arg) __attribute__((__nothrow__));

extern int snprintf(char* __restrict __s, size_t __maxlen,
    const char* __restrict __format, ...)
    __attribute__((__nothrow__)) __attribute__((__format__(__printf__, 3, 4)));

extern int vsnprintf(char* __restrict __s, size_t __maxlen,
    const char* __restrict __format, __gnuc_va_list __arg)
    __attribute__((__nothrow__)) __attribute__((__format__(__printf__, 3, 0)));
extern int vdprintf(int __fd, const char* __restrict __fmt,
    __gnuc_va_list __arg)
    __attribute__((__format__(__printf__, 2, 0)));
extern int dprintf(int __fd, const char* __restrict __fmt, ...)
__attribute__((__format__(__printf__, 2, 3)));

extern int fscanf(FILE* __restrict __stream,
    const char* __restrict __format, ...);

extern int scanf(const char* __restrict __format, ...);

extern int sscanf(const char* __restrict __s,
    const char* __restrict __format, ...) __attribute__((__nothrow__));
extern int fscanf(FILE* __restrict __stream, const char* __restrict __format, ...) __asm__("" "__isoc99_fscanf");

extern int scanf(const char* __restrict __format, ...) __asm__("" "__isoc99_scanf");

extern int sscanf(const char* __restrict __s, const char* __restrict __format, ...) __asm__("" "__isoc99_sscanf") __attribute__((__nothrow__));
extern int vfscanf(FILE* __restrict __s, const char* __restrict __format,
    __gnuc_va_list __arg)
    __attribute__((__format__(__scanf__, 2, 0)));

extern int vscanf(const char* __restrict __format, __gnuc_va_list __arg)
__attribute__((__format__(__scanf__, 1, 0)));

extern int vsscanf(const char* __restrict __s,
    const char* __restrict __format, __gnuc_va_list __arg)
    __attribute__((__nothrow__)) __attribute__((__format__(__scanf__, 2, 0)));
extern int vfscanf(FILE* __restrict __s, const char* __restrict __format, __gnuc_va_list __arg) __asm__("" "__isoc99_vfscanf")

__attribute__((__format__(__scanf__, 2, 0)));
extern int vscanf(const char* __restrict __format, __gnuc_va_list __arg) __asm__("" "__isoc99_vscanf")

__attribute__((__format__(__scanf__, 1, 0)));
extern int vsscanf(const char* __restrict __s, const char* __restrict __format, __gnuc_va_list __arg) __asm__("" "__isoc99_vsscanf") __attribute__((__nothrow__))

__attribute__((__format__(__scanf__, 2, 0)));
extern int fgetc(FILE* __stream);
extern int getc(FILE* __stream);

extern int getchar(void);
extern int getc_unlocked(FILE* __stream);
extern int getchar_unlocked(void);
extern int fgetc_unlocked(FILE* __stream);
extern int fputc(int __c, FILE* __stream);
extern int putc(int __c, FILE* __stream);

extern int putchar(int __c);
extern int fputc_unlocked(int __c, FILE* __stream);

extern int putc_unlocked(int __c, FILE* __stream);
extern int putchar_unlocked(int __c);

extern int getw(FILE* __stream);

extern int putw(int __w, FILE* __stream);

extern char* fgets(char* __restrict __s, int __n, FILE* __restrict __stream)
extern __ssize_t __getdelim(char** __restrict __lineptr,
    size_t* __restrict __n, int __delimiter,
    FILE* __restrict __stream);
extern __ssize_t getdelim(char** __restrict __lineptr,
    size_t* __restrict __n, int __delimiter,
    FILE* __restrict __stream);

extern __ssize_t getline(char** __restrict __lineptr,
    size_t* __restrict __n,
    FILE* __restrict __stream);

extern int fputs(const char* __restrict __s, FILE* __restrict __stream);

extern int puts(const char* __s);

extern int ungetc(int __c, FILE* __stream);

extern size_t fread(void* __restrict __ptr, size_t __size,
    size_t __n, FILE* __restrict __stream);

extern size_t fwrite(const void* __restrict __ptr, size_t __size,
    size_t __n, FILE* __restrict __s);
extern size_t fread_unlocked(void* __restrict __ptr, size_t __size,
    size_t __n, FILE* __restrict __stream);
extern size_t fwrite_unlocked(const void* __restrict __ptr, size_t __size,
    size_t __n, FILE* __restrict __stream);

extern int fseek(FILE* __stream, long int __off, int __whence);

extern long int ftell(FILE* __stream);

extern void rewind(FILE* __stream);
extern int fseeko(FILE* __stream, __off_t __off, int __whence);

extern __off_t ftello(FILE* __stream);
extern int fgetpos(FILE* __restrict __stream, fpos_t* __restrict __pos);

extern int fsetpos(FILE* __stream, const fpos_t* __pos);
extern void clearerr(FILE* __stream) __attribute__((__nothrow__));

extern int feof(FILE* __stream) __attribute__((__nothrow__));

extern int ferror(FILE* __stream) __attribute__((__nothrow__));

extern void clearerr_unlocked(FILE* __stream) __attribute__((__nothrow__));
extern int feof_unlocked(FILE* __stream) __attribute__((__nothrow__));
extern int ferror_unlocked(FILE* __stream) __attribute__((__nothrow__));

extern void perror(const char* __s);

extern int sys_nerr;
extern const char* const sys_errlist[];

extern int fileno(FILE* __stream) __attribute__((__nothrow__));

extern int fileno_unlocked(FILE* __stream) __attribute__((__nothrow__));
extern FILE* popen(const char* __command, const char* __modes);

extern int pclose(FILE* __stream);

extern char* ctermid(char* __s) __attribute__((__nothrow__));
extern void flockfile(FILE* __stream) __attribute__((__nothrow__));

extern int ftrylockfile(FILE* __stream) __attribute__((__nothrow__));

extern void funlockfile(FILE* __stream) __attribute__((__nothrow__));

enum
{
    STBI_default = 0,

    STBI_grey = 1,
    STBI_grey_alpha = 2,
    STBI_rgb = 3,
    STBI_rgb_alpha = 4
};

typedef int wchar_t;

typedef enum
{
    P_ALL,
    P_PID,
    P_PGID
} idtype_t;
typedef float _Float32;
typedef double _Float64;
typedef double _Float32x;
typedef long double _Float64x;

typedef struct
{
    int quot;
    int rem;
} div_t;

typedef struct
{
    long int quot;
    long int rem;
} ldiv_t;

__extension__ typedef struct
{
    long long int quot;
    long long int rem;
} lldiv_t;
extern size_t __ctype_get_mb_cur_max(void) __attribute__((__nothrow__));

extern double atof(const char* __nptr)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1)));

extern int atoi(const char* __nptr)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1)));

extern long int atol(const char* __nptr)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1)));

__extension__ extern long long int atoll(const char* __nptr)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1)));

extern double strtod(const char* __restrict __nptr,
    char** __restrict __endptr)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern float strtof(const char* __restrict __nptr,
    char** __restrict __endptr) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern long double strtold(const char* __restrict __nptr,
    char** __restrict __endptr)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));
extern long int strtol(const char* __restrict __nptr,
    char** __restrict __endptr, int __base)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern unsigned long int strtoul(const char* __restrict __nptr,
    char** __restrict __endptr, int __base)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

__extension__
extern long long int strtoq(const char* __restrict __nptr,
    char** __restrict __endptr, int __base)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

__extension__
extern unsigned long long int strtouq(const char* __restrict __nptr,
    char** __restrict __endptr, int __base)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

__extension__
extern long long int strtoll(const char* __restrict __nptr,
    char** __restrict __endptr, int __base)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

__extension__
extern unsigned long long int strtoull(const char* __restrict __nptr,
    char** __restrict __endptr, int __base)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));
extern char* l64a(long int __n) __attribute__((__nothrow__));

extern long int a64l(const char* __s)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1)));

typedef __u_char u_char;
typedef __u_short u_short;
typedef __u_int u_int;
typedef __u_long u_long;
typedef __quad_t quad_t;
typedef __u_quad_t u_quad_t;
typedef __fsid_t fsid_t;

typedef __loff_t loff_t;

typedef __ino_t ino_t;
typedef __dev_t dev_t;

typedef __gid_t gid_t;

typedef __mode_t mode_t;

typedef __nlink_t nlink_t;

typedef __uid_t uid_t;
typedef __pid_t pid_t;

typedef __id_t id_t;
typedef __daddr_t daddr_t;
typedef __caddr_t caddr_t;

typedef __key_t key_t;

typedef __clock_t clock_t;

typedef __clockid_t clockid_t;

typedef __time_t time_t;

typedef __timer_t timer_t;

typedef unsigned long int ulong;
typedef unsigned short int ushort;
typedef unsigned int uint;

typedef __int8_t int8_t;
typedef __int16_t int16_t;
typedef __int32_t int32_t;
typedef __int64_t int64_t;
typedef unsigned int u_int8_t __attribute__((__mode__(__QI__)));
typedef unsigned int u_int16_t __attribute__((__mode__(__HI__)));
typedef unsigned int u_int32_t __attribute__((__mode__(__SI__)));
typedef unsigned int u_int64_t __attribute__((__mode__(__DI__)));

typedef int register_t __attribute__((__mode__(__word__)));

static __inline __uint16_t
__uint16_identity(__uint16_t __x)
{
    return __x;
}

static __inline __uint32_t
__uint32_identity(__uint32_t __x)
{
    return __x;
}

static __inline __uint64_t
__uint64_identity(__uint64_t __x)
{
    return __x;
}

typedef struct
{
    unsigned long int __val[(1024 / (8 * sizeof(unsigned long int)))];
} __sigset_t;

typedef __sigset_t sigset_t;

struct timeval
{
    __time_t tv_sec;
    __suseconds_t tv_usec;
};

struct timespec
{
    __time_t tv_sec;
    __syscall_slong_t tv_nsec;
};

typedef __suseconds_t suseconds_t;

typedef long int __fd_mask;
typedef struct
{

    __fd_mask __fds_bits[1024 / (8 * (int) sizeof(__fd_mask))];

} fd_set;

typedef __fd_mask fd_mask;
extern int select(int __nfds, fd_set* __restrict __readfds,
    fd_set* __restrict __writefds,
    fd_set* __restrict __exceptfds,
    struct timeval* __restrict __timeout);
extern int pselect(int __nfds, fd_set* __restrict __readfds,
    fd_set* __restrict __writefds,
    fd_set* __restrict __exceptfds,
    const struct timespec* __restrict __timeout,
    const __sigset_t* __restrict __sigmask);

extern unsigned int gnu_dev_major(__dev_t __dev) __attribute__((__nothrow__)) __attribute__((__const__));
extern unsigned int gnu_dev_minor(__dev_t __dev) __attribute__((__nothrow__)) __attribute__((__const__));
extern __dev_t gnu_dev_makedev(unsigned int __major, unsigned int __minor) __attribute__((__nothrow__)) __attribute__((__const__));

typedef __blksize_t blksize_t;

typedef __blkcnt_t blkcnt_t;

typedef __fsblkcnt_t fsblkcnt_t;

typedef __fsfilcnt_t fsfilcnt_t;
struct __pthread_rwlock_arch_t
{
    unsigned int __readers;
    unsigned int __writers;
    unsigned int __wrphase_futex;
    unsigned int __writers_futex;
    unsigned int __pad3;
    unsigned int __pad4;

    int __cur_writer;
    int __shared;
    signed char __rwelision;

    unsigned char __pad1[7];

    unsigned long int __pad2;

    unsigned int __flags;
};

typedef struct __pthread_internal_list
{
    struct __pthread_internal_list* __prev;
    struct __pthread_internal_list* __next;
} __pthread_list_t;
struct __pthread_mutex_s
{
    int __lock;
    unsigned int __count;
    int __owner;

    unsigned int __nusers;

    int __kind;

    short __spins; short __elision;
    __pthread_list_t __list;
};

struct __pthread_cond_s
{
    __extension__ union
    {
        __extension__ unsigned long long int __wseq;
        struct
        {
            unsigned int __low;
            unsigned int __high;
        } __wseq32;
    };
    __extension__ union
    {
        __extension__ unsigned long long int __g1_start;
        struct
        {
            unsigned int __low;
            unsigned int __high;
        } __g1_start32;
    };
    unsigned int __g_refs[2];
    unsigned int __g_size[2];
    unsigned int __g1_orig_size;
    unsigned int __wrefs;
    unsigned int __g_signals[2];
};

typedef unsigned long int pthread_t;

typedef union
{
    char __size[4];
    int __align;
} pthread_mutexattr_t;

typedef union
{
    char __size[4];
    int __align;
} pthread_condattr_t;

typedef unsigned int pthread_key_t;

typedef int pthread_once_t;

union pthread_attr_t
{
    char __size[56];
    long int __align;
};

typedef union pthread_attr_t pthread_attr_t;

typedef union
{
    struct __pthread_mutex_s __data;
    char __size[40];
    long int __align;
} pthread_mutex_t;

typedef union
{
    struct __pthread_cond_s __data;
    char __size[48];
    __extension__ long long int __align;
} pthread_cond_t;

typedef union
{
    struct __pthread_rwlock_arch_t __data;
    char __size[56];
    long int __align;
} pthread_rwlock_t;

typedef union
{
    char __size[8];
    long int __align;
} pthread_rwlockattr_t;

typedef volatile int pthread_spinlock_t;

typedef union
{
    char __size[32];
    long int __align;
} pthread_barrier_t;

typedef union
{
    char __size[4];
    int __align;
} pthread_barrierattr_t;

extern long int random(void) __attribute__((__nothrow__));

extern void srandom(unsigned int __seed) __attribute__((__nothrow__));

extern char* initstate(unsigned int __seed, char* __statebuf,
    size_t __statelen) __attribute__((__nothrow__)) __attribute__((__nonnull__(2)));

extern char* setstate(char* __statebuf) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

struct random_data
{
    int32_t* fptr;
    int32_t* rptr;
    int32_t* state;
    int rand_type;
    int rand_deg;
    int rand_sep;
    int32_t* end_ptr;
};

extern int random_r(struct random_data* __restrict __buf,
    int32_t* __restrict __result) __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern int srandom_r(unsigned int __seed, struct random_data* __buf)
__attribute__((__nothrow__)) __attribute__((__nonnull__(2)));

extern int initstate_r(unsigned int __seed, char* __restrict __statebuf,
    size_t __statelen,
    struct random_data* __restrict __buf)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(2, 4)));

extern int setstate_r(char* __restrict __statebuf,
    struct random_data* __restrict __buf)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern int rand(void) __attribute__((__nothrow__));

extern void srand(unsigned int __seed) __attribute__((__nothrow__));

extern int rand_r(unsigned int* __seed) __attribute__((__nothrow__));

extern double drand48(void) __attribute__((__nothrow__));
extern double erand48(unsigned short int __xsubi[3]) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern long int lrand48(void) __attribute__((__nothrow__));
extern long int nrand48(unsigned short int __xsubi[3])
__attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern long int mrand48(void) __attribute__((__nothrow__));
extern long int jrand48(unsigned short int __xsubi[3])
__attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern void srand48(long int __seedval) __attribute__((__nothrow__));
extern unsigned short int* seed48(unsigned short int __seed16v[3])
__attribute__((__nothrow__)) __attribute__((__nonnull__(1)));
extern void lcong48(unsigned short int __param[7]) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

struct drand48_data
{
    unsigned short int __x[3];
    unsigned short int __old_x[3];
    unsigned short int __c;
    unsigned short int __init;
    __extension__ unsigned long long int __a;

};

extern int drand48_r(struct drand48_data* __restrict __buffer,
    double* __restrict __result) __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));
extern int erand48_r(unsigned short int __xsubi[3],
    struct drand48_data* __restrict __buffer,
    double* __restrict __result) __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern int lrand48_r(struct drand48_data* __restrict __buffer,
    long int* __restrict __result)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));
extern int nrand48_r(unsigned short int __xsubi[3],
    struct drand48_data* __restrict __buffer,
    long int* __restrict __result)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern int mrand48_r(struct drand48_data* __restrict __buffer,
    long int* __restrict __result)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));
extern int jrand48_r(unsigned short int __xsubi[3],
    struct drand48_data* __restrict __buffer,
    long int* __restrict __result)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern int srand48_r(long int __seedval, struct drand48_data* __buffer)
__attribute__((__nothrow__)) __attribute__((__nonnull__(2)));

extern int seed48_r(unsigned short int __seed16v[3],
    struct drand48_data* __buffer) __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern int lcong48_r(unsigned short int __param[7],
    struct drand48_data* __buffer)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern void* malloc(size_t __size) __attribute__((__nothrow__)) __attribute__((__malloc__));

extern void* calloc(size_t __nmemb, size_t __size)
__attribute__((__nothrow__)) __attribute__((__malloc__));

extern void* realloc(void* __ptr, size_t __size)
__attribute__((__nothrow__)) __attribute__((__warn_unused_result__));
extern void free(void* __ptr) __attribute__((__nothrow__));

extern void* alloca(size_t __size) __attribute__((__nothrow__));

extern void* valloc(size_t __size) __attribute__((__nothrow__)) __attribute__((__malloc__));

extern int posix_memalign(void** __memptr, size_t __alignment, size_t __size)
__attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern void* aligned_alloc(size_t __alignment, size_t __size)
__attribute__((__nothrow__)) __attribute__((__malloc__));

extern void abort(void) __attribute__((__nothrow__)) __attribute__((__noreturn__));

extern int atexit(void (*__func) (void)) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern int at_quick_exit(void (*__func) (void)) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern int on_exit(void (*__func) (int __status, void* __arg), void* __arg)
__attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern void exit(int __status) __attribute__((__nothrow__)) __attribute__((__noreturn__));

extern void quick_exit(int __status) __attribute__((__nothrow__)) __attribute__((__noreturn__));

extern void _Exit(int __status) __attribute__((__nothrow__)) __attribute__((__noreturn__));

extern char* getenv(const char* __name) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));
extern int putenv(char* __string) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern int setenv(const char* __name, const char* __value, int __replace)
__attribute__((__nothrow__)) __attribute__((__nonnull__(2)));

extern int unsetenv(const char* __name) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern int clearenv(void) __attribute__((__nothrow__));
extern char* mktemp(char* __template) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));
extern int mkstemp(char* __template) __attribute__((__nonnull__(1)));
extern int mkstemps(char* __template, int __suffixlen) __attribute__((__nonnull__(1)));
extern char* mkdtemp(char* __template) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));
extern int system(const char* __command);
extern char* realpath(const char* __restrict __name,
    char* __restrict __resolved) __attribute__((__nothrow__));

typedef int (*__compar_fn_t) (const void*, const void*);
extern void* bsearch(const void* __key, const void* __base,
    size_t __nmemb, size_t __size, __compar_fn_t __compar)
    __attribute__((__nonnull__(1, 2, 5)));

extern void qsort(void* __base, size_t __nmemb, size_t __size,
    __compar_fn_t __compar) __attribute__((__nonnull__(1, 4)));
extern int abs(int __x) __attribute__((__nothrow__)) __attribute__((__const__));
extern long int labs(long int __x) __attribute__((__nothrow__)) __attribute__((__const__));

__extension__ extern long long int llabs(long long int __x)
__attribute__((__nothrow__)) __attribute__((__const__));

extern div_t div(int __numer, int __denom)
__attribute__((__nothrow__)) __attribute__((__const__));
extern ldiv_t ldiv(long int __numer, long int __denom)
__attribute__((__nothrow__)) __attribute__((__const__));

__extension__ extern lldiv_t lldiv(long long int __numer,
    long long int __denom)
    __attribute__((__nothrow__)) __attribute__((__const__));
extern char* ecvt(double __value, int __ndigit, int* __restrict __decpt,
    int* __restrict __sign) __attribute__((__nothrow__)) __attribute__((__nonnull__(3, 4)));

extern char* fcvt(double __value, int __ndigit, int* __restrict __decpt,
    int* __restrict __sign) __attribute__((__nothrow__)) __attribute__((__nonnull__(3, 4)));

extern char* gcvt(double __value, int __ndigit, char* __buf)
__attribute__((__nothrow__)) __attribute__((__nonnull__(3)));

extern char* qecvt(long double __value, int __ndigit,
    int* __restrict __decpt, int* __restrict __sign)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(3, 4)));
extern char* qfcvt(long double __value, int __ndigit,
    int* __restrict __decpt, int* __restrict __sign)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(3, 4)));
extern char* qgcvt(long double __value, int __ndigit, char* __buf)
__attribute__((__nothrow__)) __attribute__((__nonnull__(3)));

extern int ecvt_r(double __value, int __ndigit, int* __restrict __decpt,
    int* __restrict __sign, char* __restrict __buf,
    size_t __len) __attribute__((__nothrow__)) __attribute__((__nonnull__(3, 4, 5)));
extern int fcvt_r(double __value, int __ndigit, int* __restrict __decpt,
    int* __restrict __sign, char* __restrict __buf,
    size_t __len) __attribute__((__nothrow__)) __attribute__((__nonnull__(3, 4, 5)));

extern int qecvt_r(long double __value, int __ndigit,
    int* __restrict __decpt, int* __restrict __sign,
    char* __restrict __buf, size_t __len)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(3, 4, 5)));
extern int qfcvt_r(long double __value, int __ndigit,
    int* __restrict __decpt, int* __restrict __sign,
    char* __restrict __buf, size_t __len)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(3, 4, 5)));

extern int mblen(const char* __s, size_t __n) __attribute__((__nothrow__));

extern int mbtowc(wchar_t* __restrict __pwc,
    const char* __restrict __s, size_t __n) __attribute__((__nothrow__));

extern int wctomb(char* __s, wchar_t __wchar) __attribute__((__nothrow__));

extern size_t mbstowcs(wchar_t* __restrict __pwcs,
    const char* __restrict __s, size_t __n) __attribute__((__nothrow__));

extern size_t wcstombs(char* __restrict __s,
    const wchar_t* __restrict __pwcs, size_t __n)
    __attribute__((__nothrow__));

extern int rpmatch(const char* __response) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));
extern int getsubopt(char** __restrict __optionp,
    char* const* __restrict __tokens,
    char** __restrict __valuep)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2, 3)));
extern int getloadavg(double __loadavg[], int __nelem)
__attribute__((__nothrow__)) __attribute__((__nonnull__(1)));
typedef unsigned char stbi_uc;
typedef unsigned short stbi_us;
typedef struct
{
    int (*read) (void* user, char* data, int size);
    void (*skip) (void* user, int n);
    int (*eof) (void* user);
} stbi_io_callbacks;

extern stbi_uc* stbi_load_from_memory(stbi_uc const* buffer, int len, int* x, int* y, int* channels_in_file, int desired_channels);
extern stbi_uc* stbi_load_from_callbacks(stbi_io_callbacks const* clbk, void* user, int* x, int* y, int* channels_in_file, int desired_channels);

extern stbi_uc* stbi_load(char const* filename, int* x, int* y, int* channels_in_file, int desired_channels);
extern stbi_uc* stbi_load_from_file(FILE* f, int* x, int* y, int* channels_in_file, int desired_channels);

extern stbi_uc* stbi_load_gif_from_memory(stbi_uc const* buffer, int len, int** delays, int* x, int* y, int* z, int* comp, int req_comp);
extern stbi_us* stbi_load_16_from_memory(stbi_uc const* buffer, int len, int* x, int* y, int* channels_in_file, int desired_channels);
extern stbi_us* stbi_load_16_from_callbacks(stbi_io_callbacks const* clbk, void* user, int* x, int* y, int* channels_in_file, int desired_channels);

extern stbi_us* stbi_load_16(char const* filename, int* x, int* y, int* channels_in_file, int desired_channels);
extern stbi_us* stbi_load_from_file_16(FILE* f, int* x, int* y, int* channels_in_file, int desired_channels);

extern float* stbi_loadf_from_memory(stbi_uc const* buffer, int len, int* x, int* y, int* channels_in_file, int desired_channels);
extern float* stbi_loadf_from_callbacks(stbi_io_callbacks const* clbk, void* user, int* x, int* y, int* channels_in_file, int desired_channels);

extern float* stbi_loadf(char const* filename, int* x, int* y, int* channels_in_file, int desired_channels);
extern float* stbi_loadf_from_file(FILE* f, int* x, int* y, int* channels_in_file, int desired_channels);

extern void stbi_hdr_to_ldr_gamma(float gamma);
extern void stbi_hdr_to_ldr_scale(float scale);

extern void stbi_ldr_to_hdr_gamma(float gamma);
extern void stbi_ldr_to_hdr_scale(float scale);

extern int stbi_is_hdr_from_callbacks(stbi_io_callbacks const* clbk, void* user);
extern int stbi_is_hdr_from_memory(stbi_uc const* buffer, int len);

extern int stbi_is_hdr(char const* filename);
extern int stbi_is_hdr_from_file(FILE* f);

extern const char* stbi_failure_reason(void);

extern void stbi_image_free(void* retval_from_stbi_load);

extern int stbi_info_from_memory(stbi_uc const* buffer, int len, int* x, int* y, int* comp);
extern int stbi_info_from_callbacks(stbi_io_callbacks const* clbk, void* user, int* x, int* y, int* comp);
extern int stbi_is_16_bit_from_memory(stbi_uc const* buffer, int len);
extern int stbi_is_16_bit_from_callbacks(stbi_io_callbacks const* clbk, void* user);

extern int stbi_info(char const* filename, int* x, int* y, int* comp);
extern int stbi_info_from_file(FILE* f, int* x, int* y, int* comp);
extern int stbi_is_16_bit(char const* filename);
extern int stbi_is_16_bit_from_file(FILE* f);

extern void stbi_set_unpremultiply_on_load(int flag_true_if_should_unpremultiply);

extern void stbi_convert_iphone_png_to_rgb(int flag_true_if_should_convert);

extern void stbi_set_flip_vertically_on_load(int flag_true_if_should_flip);

extern void stbi_set_flip_vertically_on_load_thread(int flag_true_if_should_flip);

extern char* stbi_zlib_decode_malloc_guesssize(const char* buffer, int len, int initial_size, int* outlen);
extern char* stbi_zlib_decode_malloc_guesssize_headerflag(const char* buffer, int len, int initial_size, int* outlen, int parse_header);
extern char* stbi_zlib_decode_malloc(const char* buffer, int len, int* outlen);
extern int stbi_zlib_decode_buffer(char* obuffer, int olen, const char* ibuffer, int ilen);

extern char* stbi_zlib_decode_noheader_malloc(const char* buffer, int len, int* outlen);
extern int stbi_zlib_decode_noheader_buffer(char* obuffer, int olen, const char* ibuffer, int ilen);
typedef long int ptrdiff_t;
typedef struct {
    long long __clang_max_align_nonce1
        __attribute__((__aligned__(__alignof__(long long))));
    long double __clang_max_align_nonce2
        __attribute__((__aligned__(__alignof__(long double))));
} max_align_t;

extern void* memcpy(void* __restrict __dest, const void* __restrict __src,
    size_t __n) __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern void* memmove(void* __dest, const void* __src, size_t __n)
__attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern void* memccpy(void* __restrict __dest, const void* __restrict __src,
    int __c, size_t __n)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern void* memset(void* __s, int __c, size_t __n) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern int memcmp(const void* __s1, const void* __s2, size_t __n)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2)));
extern void* memchr(const void* __s, int __c, size_t __n)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1)));
extern char* strcpy(char* __restrict __dest, const char* __restrict __src)
__attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern char* strncpy(char* __restrict __dest,
    const char* __restrict __src, size_t __n)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern char* strcat(char* __restrict __dest, const char* __restrict __src)
__attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern char* strncat(char* __restrict __dest, const char* __restrict __src,
    size_t __n) __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern int strcmp(const char* __s1, const char* __s2)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2)));

extern int strncmp(const char* __s1, const char* __s2, size_t __n)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2)));

extern int strcoll(const char* __s1, const char* __s2)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2)));

extern size_t strxfrm(char* __restrict __dest,
    const char* __restrict __src, size_t __n)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(2)));

struct __locale_struct
{

    struct __locale_data* __locales[13];

    const unsigned short int* __ctype_b;
    const int* __ctype_tolower;
    const int* __ctype_toupper;

    const char* __names[13];
};

typedef struct __locale_struct* __locale_t;

typedef __locale_t locale_t;

extern int strcoll_l(const char* __s1, const char* __s2, locale_t __l)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2, 3)));

extern size_t strxfrm_l(char* __dest, const char* __src, size_t __n,
    locale_t __l) __attribute__((__nothrow__)) __attribute__((__nonnull__(2, 4)));

extern char* strdup(const char* __s)
__attribute__((__nothrow__)) __attribute__((__malloc__)) __attribute__((__nonnull__(1)));

extern char* strndup(const char* __string, size_t __n)
__attribute__((__nothrow__)) __attribute__((__malloc__)) __attribute__((__nonnull__(1)));
extern char* strchr(const char* __s, int __c)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1)));
extern char* strrchr(const char* __s, int __c)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1)));
extern size_t strcspn(const char* __s, const char* __reject)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2)));

extern size_t strspn(const char* __s, const char* __accept)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2)));
extern char* strpbrk(const char* __s, const char* __accept)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2)));
extern char* strstr(const char* __haystack, const char* __needle)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2)));

extern char* strtok(char* __restrict __s, const char* __restrict __delim)
__attribute__((__nothrow__)) __attribute__((__nonnull__(2)));

extern char* __strtok_r(char* __restrict __s,
    const char* __restrict __delim,
    char** __restrict __save_ptr)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(2, 3)));

extern char* strtok_r(char* __restrict __s, const char* __restrict __delim,
    char** __restrict __save_ptr)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(2, 3)));
extern size_t strlen(const char* __s)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1)));

extern size_t strnlen(const char* __string, size_t __maxlen)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1)));

extern char* strerror(int __errnum) __attribute__((__nothrow__));
extern int strerror_r(int __errnum, char* __buf, size_t __buflen) __asm__("" "__xpg_strerror_r") __attribute__((__nothrow__)) __attribute__((__nonnull__(2)));
extern char* strerror_l(int __errnum, locale_t __l) __attribute__((__nothrow__));

extern int bcmp(const void* __s1, const void* __s2, size_t __n)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2)));

extern void bcopy(const void* __src, void* __dest, size_t __n)
__attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern void bzero(void* __s, size_t __n) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));
extern char* index(const char* __s, int __c)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1)));
extern char* rindex(const char* __s, int __c)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1)));

extern int ffs(int __i) __attribute__((__nothrow__)) __attribute__((__const__));

extern int ffsl(long int __l) __attribute__((__nothrow__)) __attribute__((__const__));
__extension__ extern int ffsll(long long int __ll)
__attribute__((__nothrow__)) __attribute__((__const__));

extern int strcasecmp(const char* __s1, const char* __s2)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2)));

extern int strncasecmp(const char* __s1, const char* __s2, size_t __n)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2)));

extern int strcasecmp_l(const char* __s1, const char* __s2, locale_t __loc)
__attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2, 3)));

extern int strncasecmp_l(const char* __s1, const char* __s2,
    size_t __n, locale_t __loc)
    __attribute__((__nothrow__)) __attribute__((__pure__)) __attribute__((__nonnull__(1, 2, 4)));

extern void explicit_bzero(void* __s, size_t __n) __attribute__((__nothrow__)) __attribute__((__nonnull__(1)));

extern char* strsep(char** __restrict __stringp,
    const char* __restrict __delim)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern char* strsignal(int __sig) __attribute__((__nothrow__));

extern char* __stpcpy(char* __restrict __dest, const char* __restrict __src)
__attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));
extern char* stpcpy(char* __restrict __dest, const char* __restrict __src)
__attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

extern char* __stpncpy(char* __restrict __dest,
    const char* __restrict __src, size_t __n)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));
extern char* stpncpy(char* __restrict __dest,
    const char* __restrict __src, size_t __n)
    __attribute__((__nothrow__)) __attribute__((__nonnull__(1, 2)));

typedef float float_t;
typedef double double_t;
extern int __fpclassify(double __value) __attribute__((__nothrow__))
__attribute__((__const__));

extern int __signbit(double __value) __attribute__((__nothrow__))
__attribute__((__const__));

extern int __isinf(double __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern int __finite(double __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern int __isnan(double __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern int __iseqsig(double __x, double __y) __attribute__((__nothrow__));

extern int __issignaling(double __value) __attribute__((__nothrow__))
__attribute__((__const__));
extern double acos(double __x) __attribute__((__nothrow__)); extern double __acos(double __x) __attribute__((__nothrow__));

extern double asin(double __x) __attribute__((__nothrow__)); extern double __asin(double __x) __attribute__((__nothrow__));

extern double atan(double __x) __attribute__((__nothrow__)); extern double __atan(double __x) __attribute__((__nothrow__));

extern double atan2(double __y, double __x) __attribute__((__nothrow__)); extern double __atan2(double __y, double __x) __attribute__((__nothrow__));

extern double cos(double __x) __attribute__((__nothrow__)); extern double __cos(double __x) __attribute__((__nothrow__));

extern double sin(double __x) __attribute__((__nothrow__)); extern double __sin(double __x) __attribute__((__nothrow__));

extern double tan(double __x) __attribute__((__nothrow__)); extern double __tan(double __x) __attribute__((__nothrow__));

extern double cosh(double __x) __attribute__((__nothrow__)); extern double __cosh(double __x) __attribute__((__nothrow__));

extern double sinh(double __x) __attribute__((__nothrow__)); extern double __sinh(double __x) __attribute__((__nothrow__));

extern double tanh(double __x) __attribute__((__nothrow__)); extern double __tanh(double __x) __attribute__((__nothrow__));
extern double acosh(double __x) __attribute__((__nothrow__)); extern double __acosh(double __x) __attribute__((__nothrow__));

extern double asinh(double __x) __attribute__((__nothrow__)); extern double __asinh(double __x) __attribute__((__nothrow__));

extern double atanh(double __x) __attribute__((__nothrow__)); extern double __atanh(double __x) __attribute__((__nothrow__));

extern double exp(double __x) __attribute__((__nothrow__)); extern double __exp(double __x) __attribute__((__nothrow__));

extern double frexp(double __x, int* __exponent) __attribute__((__nothrow__)); extern double __frexp(double __x, int* __exponent) __attribute__((__nothrow__));

extern double ldexp(double __x, int __exponent) __attribute__((__nothrow__)); extern double __ldexp(double __x, int __exponent) __attribute__((__nothrow__));

extern double log(double __x) __attribute__((__nothrow__)); extern double __log(double __x) __attribute__((__nothrow__));

extern double log10(double __x) __attribute__((__nothrow__)); extern double __log10(double __x) __attribute__((__nothrow__));

extern double modf(double __x, double* __iptr) __attribute__((__nothrow__)); extern double __modf(double __x, double* __iptr) __attribute__((__nothrow__)) __attribute__((__nonnull__(2)));
extern double expm1(double __x) __attribute__((__nothrow__)); extern double __expm1(double __x) __attribute__((__nothrow__));

extern double log1p(double __x) __attribute__((__nothrow__)); extern double __log1p(double __x) __attribute__((__nothrow__));

extern double logb(double __x) __attribute__((__nothrow__)); extern double __logb(double __x) __attribute__((__nothrow__));

extern double exp2(double __x) __attribute__((__nothrow__)); extern double __exp2(double __x) __attribute__((__nothrow__));

extern double log2(double __x) __attribute__((__nothrow__)); extern double __log2(double __x) __attribute__((__nothrow__));

extern double pow(double __x, double __y) __attribute__((__nothrow__)); extern double __pow(double __x, double __y) __attribute__((__nothrow__));

extern double sqrt(double __x) __attribute__((__nothrow__)); extern double __sqrt(double __x) __attribute__((__nothrow__));

extern double hypot(double __x, double __y) __attribute__((__nothrow__)); extern double __hypot(double __x, double __y) __attribute__((__nothrow__));

extern double cbrt(double __x) __attribute__((__nothrow__)); extern double __cbrt(double __x) __attribute__((__nothrow__));

extern double ceil(double __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern double __ceil(double __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern double fabs(double __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern double __fabs(double __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern double floor(double __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern double __floor(double __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern double fmod(double __x, double __y) __attribute__((__nothrow__)); extern double __fmod(double __x, double __y) __attribute__((__nothrow__));
extern int isinf(double __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern int finite(double __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern double drem(double __x, double __y) __attribute__((__nothrow__)); extern double __drem(double __x, double __y) __attribute__((__nothrow__));

extern double significand(double __x) __attribute__((__nothrow__)); extern double __significand(double __x) __attribute__((__nothrow__));

extern double copysign(double __x, double __y) __attribute__((__nothrow__)) __attribute__((__const__)); extern double __copysign(double __x, double __y) __attribute__((__nothrow__)) __attribute__((__const__));

extern double nan(const char* __tagb) __attribute__((__nothrow__)) __attribute__((__const__)); extern double __nan(const char* __tagb) __attribute__((__nothrow__)) __attribute__((__const__));
extern int isnan(double __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern double j0(double) __attribute__((__nothrow__)); extern double __j0(double) __attribute__((__nothrow__));
extern double j1(double) __attribute__((__nothrow__)); extern double __j1(double) __attribute__((__nothrow__));
extern double jn(int, double) __attribute__((__nothrow__)); extern double __jn(int, double) __attribute__((__nothrow__));
extern double y0(double) __attribute__((__nothrow__)); extern double __y0(double) __attribute__((__nothrow__));
extern double y1(double) __attribute__((__nothrow__)); extern double __y1(double) __attribute__((__nothrow__));
extern double yn(int, double) __attribute__((__nothrow__)); extern double __yn(int, double) __attribute__((__nothrow__));

extern double erf(double) __attribute__((__nothrow__)); extern double __erf(double) __attribute__((__nothrow__));
extern double erfc(double) __attribute__((__nothrow__)); extern double __erfc(double) __attribute__((__nothrow__));
extern double lgamma(double) __attribute__((__nothrow__)); extern double __lgamma(double) __attribute__((__nothrow__));

extern double tgamma(double) __attribute__((__nothrow__)); extern double __tgamma(double) __attribute__((__nothrow__));

extern double gamma(double) __attribute__((__nothrow__)); extern double __gamma(double) __attribute__((__nothrow__));

extern double lgamma_r(double, int* __signgamp) __attribute__((__nothrow__)); extern double __lgamma_r(double, int* __signgamp) __attribute__((__nothrow__));

extern double rint(double __x) __attribute__((__nothrow__)); extern double __rint(double __x) __attribute__((__nothrow__));

extern double nextafter(double __x, double __y) __attribute__((__nothrow__)); extern double __nextafter(double __x, double __y) __attribute__((__nothrow__));

extern double nexttoward(double __x, long double __y) __attribute__((__nothrow__)); extern double __nexttoward(double __x, long double __y) __attribute__((__nothrow__));
extern double remainder(double __x, double __y) __attribute__((__nothrow__)); extern double __remainder(double __x, double __y) __attribute__((__nothrow__));

extern double scalbn(double __x, int __n) __attribute__((__nothrow__)); extern double __scalbn(double __x, int __n) __attribute__((__nothrow__));

extern int ilogb(double __x) __attribute__((__nothrow__)); extern int __ilogb(double __x) __attribute__((__nothrow__));
extern double scalbln(double __x, long int __n) __attribute__((__nothrow__)); extern double __scalbln(double __x, long int __n) __attribute__((__nothrow__));

extern double nearbyint(double __x) __attribute__((__nothrow__)); extern double __nearbyint(double __x) __attribute__((__nothrow__));

extern double round(double __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern double __round(double __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern double trunc(double __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern double __trunc(double __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern double remquo(double __x, double __y, int* __quo) __attribute__((__nothrow__)); extern double __remquo(double __x, double __y, int* __quo) __attribute__((__nothrow__));

extern long int lrint(double __x) __attribute__((__nothrow__)); extern long int __lrint(double __x) __attribute__((__nothrow__));
__extension__
extern long long int llrint(double __x) __attribute__((__nothrow__)); extern long long int __llrint(double __x) __attribute__((__nothrow__));

extern long int lround(double __x) __attribute__((__nothrow__)); extern long int __lround(double __x) __attribute__((__nothrow__));
__extension__
extern long long int llround(double __x) __attribute__((__nothrow__)); extern long long int __llround(double __x) __attribute__((__nothrow__));

extern double fdim(double __x, double __y) __attribute__((__nothrow__)); extern double __fdim(double __x, double __y) __attribute__((__nothrow__));

extern double fmax(double __x, double __y) __attribute__((__nothrow__)) __attribute__((__const__)); extern double __fmax(double __x, double __y) __attribute__((__nothrow__)) __attribute__((__const__));

extern double fmin(double __x, double __y) __attribute__((__nothrow__)) __attribute__((__const__)); extern double __fmin(double __x, double __y) __attribute__((__nothrow__)) __attribute__((__const__));

extern double fma(double __x, double __y, double __z) __attribute__((__nothrow__)); extern double __fma(double __x, double __y, double __z) __attribute__((__nothrow__));
extern double scalb(double __x, double __n) __attribute__((__nothrow__)); extern double __scalb(double __x, double __n) __attribute__((__nothrow__));
extern int __fpclassifyf(float __value) __attribute__((__nothrow__))
__attribute__((__const__));

extern int __signbitf(float __value) __attribute__((__nothrow__))
__attribute__((__const__));

extern int __isinff(float __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern int __finitef(float __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern int __isnanf(float __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern int __iseqsigf(float __x, float __y) __attribute__((__nothrow__));

extern int __issignalingf(float __value) __attribute__((__nothrow__))
__attribute__((__const__));
extern float acosf(float __x) __attribute__((__nothrow__)); extern float __acosf(float __x) __attribute__((__nothrow__));

extern float asinf(float __x) __attribute__((__nothrow__)); extern float __asinf(float __x) __attribute__((__nothrow__));

extern float atanf(float __x) __attribute__((__nothrow__)); extern float __atanf(float __x) __attribute__((__nothrow__));

extern float atan2f(float __y, float __x) __attribute__((__nothrow__)); extern float __atan2f(float __y, float __x) __attribute__((__nothrow__));

extern float cosf(float __x) __attribute__((__nothrow__)); extern float __cosf(float __x) __attribute__((__nothrow__));

extern float sinf(float __x) __attribute__((__nothrow__)); extern float __sinf(float __x) __attribute__((__nothrow__));

extern float tanf(float __x) __attribute__((__nothrow__)); extern float __tanf(float __x) __attribute__((__nothrow__));

extern float coshf(float __x) __attribute__((__nothrow__)); extern float __coshf(float __x) __attribute__((__nothrow__));

extern float sinhf(float __x) __attribute__((__nothrow__)); extern float __sinhf(float __x) __attribute__((__nothrow__));

extern float tanhf(float __x) __attribute__((__nothrow__)); extern float __tanhf(float __x) __attribute__((__nothrow__));
extern float acoshf(float __x) __attribute__((__nothrow__)); extern float __acoshf(float __x) __attribute__((__nothrow__));

extern float asinhf(float __x) __attribute__((__nothrow__)); extern float __asinhf(float __x) __attribute__((__nothrow__));

extern float atanhf(float __x) __attribute__((__nothrow__)); extern float __atanhf(float __x) __attribute__((__nothrow__));

extern float expf(float __x) __attribute__((__nothrow__)); extern float __expf(float __x) __attribute__((__nothrow__));

extern float frexpf(float __x, int* __exponent) __attribute__((__nothrow__)); extern float __frexpf(float __x, int* __exponent) __attribute__((__nothrow__));

extern float ldexpf(float __x, int __exponent) __attribute__((__nothrow__)); extern float __ldexpf(float __x, int __exponent) __attribute__((__nothrow__));

extern float logf(float __x) __attribute__((__nothrow__)); extern float __logf(float __x) __attribute__((__nothrow__));

extern float log10f(float __x) __attribute__((__nothrow__)); extern float __log10f(float __x) __attribute__((__nothrow__));

extern float modff(float __x, float* __iptr) __attribute__((__nothrow__)); extern float __modff(float __x, float* __iptr) __attribute__((__nothrow__)) __attribute__((__nonnull__(2)));
extern float expm1f(float __x) __attribute__((__nothrow__)); extern float __expm1f(float __x) __attribute__((__nothrow__));

extern float log1pf(float __x) __attribute__((__nothrow__)); extern float __log1pf(float __x) __attribute__((__nothrow__));

extern float logbf(float __x) __attribute__((__nothrow__)); extern float __logbf(float __x) __attribute__((__nothrow__));

extern float exp2f(float __x) __attribute__((__nothrow__)); extern float __exp2f(float __x) __attribute__((__nothrow__));

extern float log2f(float __x) __attribute__((__nothrow__)); extern float __log2f(float __x) __attribute__((__nothrow__));

extern float powf(float __x, float __y) __attribute__((__nothrow__)); extern float __powf(float __x, float __y) __attribute__((__nothrow__));

extern float sqrtf(float __x) __attribute__((__nothrow__)); extern float __sqrtf(float __x) __attribute__((__nothrow__));

extern float hypotf(float __x, float __y) __attribute__((__nothrow__)); extern float __hypotf(float __x, float __y) __attribute__((__nothrow__));

extern float cbrtf(float __x) __attribute__((__nothrow__)); extern float __cbrtf(float __x) __attribute__((__nothrow__));

extern float ceilf(float __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern float __ceilf(float __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern float fabsf(float __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern float __fabsf(float __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern float floorf(float __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern float __floorf(float __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern float fmodf(float __x, float __y) __attribute__((__nothrow__)); extern float __fmodf(float __x, float __y) __attribute__((__nothrow__));
extern int isinff(float __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern int finitef(float __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern float dremf(float __x, float __y) __attribute__((__nothrow__)); extern float __dremf(float __x, float __y) __attribute__((__nothrow__));

extern float significandf(float __x) __attribute__((__nothrow__)); extern float __significandf(float __x) __attribute__((__nothrow__));

extern float copysignf(float __x, float __y) __attribute__((__nothrow__)) __attribute__((__const__)); extern float __copysignf(float __x, float __y) __attribute__((__nothrow__)) __attribute__((__const__));

extern float nanf(const char* __tagb) __attribute__((__nothrow__)) __attribute__((__const__)); extern float __nanf(const char* __tagb) __attribute__((__nothrow__)) __attribute__((__const__));
extern int isnanf(float __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern float j0f(float) __attribute__((__nothrow__)); extern float __j0f(float) __attribute__((__nothrow__));
extern float j1f(float) __attribute__((__nothrow__)); extern float __j1f(float) __attribute__((__nothrow__));
extern float jnf(int, float) __attribute__((__nothrow__)); extern float __jnf(int, float) __attribute__((__nothrow__));
extern float y0f(float) __attribute__((__nothrow__)); extern float __y0f(float) __attribute__((__nothrow__));
extern float y1f(float) __attribute__((__nothrow__)); extern float __y1f(float) __attribute__((__nothrow__));
extern float ynf(int, float) __attribute__((__nothrow__)); extern float __ynf(int, float) __attribute__((__nothrow__));

extern float erff(float) __attribute__((__nothrow__)); extern float __erff(float) __attribute__((__nothrow__));
extern float erfcf(float) __attribute__((__nothrow__)); extern float __erfcf(float) __attribute__((__nothrow__));
extern float lgammaf(float) __attribute__((__nothrow__)); extern float __lgammaf(float) __attribute__((__nothrow__));

extern float tgammaf(float) __attribute__((__nothrow__)); extern float __tgammaf(float) __attribute__((__nothrow__));

extern float gammaf(float) __attribute__((__nothrow__)); extern float __gammaf(float) __attribute__((__nothrow__));

extern float lgammaf_r(float, int* __signgamp) __attribute__((__nothrow__)); extern float __lgammaf_r(float, int* __signgamp) __attribute__((__nothrow__));

extern float rintf(float __x) __attribute__((__nothrow__)); extern float __rintf(float __x) __attribute__((__nothrow__));

extern float nextafterf(float __x, float __y) __attribute__((__nothrow__)); extern float __nextafterf(float __x, float __y) __attribute__((__nothrow__));

extern float nexttowardf(float __x, long double __y) __attribute__((__nothrow__)); extern float __nexttowardf(float __x, long double __y) __attribute__((__nothrow__));
extern float remainderf(float __x, float __y) __attribute__((__nothrow__)); extern float __remainderf(float __x, float __y) __attribute__((__nothrow__));

extern float scalbnf(float __x, int __n) __attribute__((__nothrow__)); extern float __scalbnf(float __x, int __n) __attribute__((__nothrow__));

extern int ilogbf(float __x) __attribute__((__nothrow__)); extern int __ilogbf(float __x) __attribute__((__nothrow__));
extern float scalblnf(float __x, long int __n) __attribute__((__nothrow__)); extern float __scalblnf(float __x, long int __n) __attribute__((__nothrow__));

extern float nearbyintf(float __x) __attribute__((__nothrow__)); extern float __nearbyintf(float __x) __attribute__((__nothrow__));

extern float roundf(float __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern float __roundf(float __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern float truncf(float __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern float __truncf(float __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern float remquof(float __x, float __y, int* __quo) __attribute__((__nothrow__)); extern float __remquof(float __x, float __y, int* __quo) __attribute__((__nothrow__));

extern long int lrintf(float __x) __attribute__((__nothrow__)); extern long int __lrintf(float __x) __attribute__((__nothrow__));
__extension__
extern long long int llrintf(float __x) __attribute__((__nothrow__)); extern long long int __llrintf(float __x) __attribute__((__nothrow__));

extern long int lroundf(float __x) __attribute__((__nothrow__)); extern long int __lroundf(float __x) __attribute__((__nothrow__));
__extension__
extern long long int llroundf(float __x) __attribute__((__nothrow__)); extern long long int __llroundf(float __x) __attribute__((__nothrow__));

extern float fdimf(float __x, float __y) __attribute__((__nothrow__)); extern float __fdimf(float __x, float __y) __attribute__((__nothrow__));

extern float fmaxf(float __x, float __y) __attribute__((__nothrow__)) __attribute__((__const__)); extern float __fmaxf(float __x, float __y) __attribute__((__nothrow__)) __attribute__((__const__));

extern float fminf(float __x, float __y) __attribute__((__nothrow__)) __attribute__((__const__)); extern float __fminf(float __x, float __y) __attribute__((__nothrow__)) __attribute__((__const__));

extern float fmaf(float __x, float __y, float __z) __attribute__((__nothrow__)); extern float __fmaf(float __x, float __y, float __z) __attribute__((__nothrow__));
extern float scalbf(float __x, float __n) __attribute__((__nothrow__)); extern float __scalbf(float __x, float __n) __attribute__((__nothrow__));
extern int __fpclassifyl(long double __value) __attribute__((__nothrow__))
__attribute__((__const__));

extern int __signbitl(long double __value) __attribute__((__nothrow__))
__attribute__((__const__));

extern int __isinfl(long double __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern int __finitel(long double __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern int __isnanl(long double __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern int __iseqsigl(long double __x, long double __y) __attribute__((__nothrow__));

extern int __issignalingl(long double __value) __attribute__((__nothrow__))
__attribute__((__const__));
extern long double acosl(long double __x) __attribute__((__nothrow__)); extern long double __acosl(long double __x) __attribute__((__nothrow__));

extern long double asinl(long double __x) __attribute__((__nothrow__)); extern long double __asinl(long double __x) __attribute__((__nothrow__));

extern long double atanl(long double __x) __attribute__((__nothrow__)); extern long double __atanl(long double __x) __attribute__((__nothrow__));

extern long double atan2l(long double __y, long double __x) __attribute__((__nothrow__)); extern long double __atan2l(long double __y, long double __x) __attribute__((__nothrow__));

extern long double cosl(long double __x) __attribute__((__nothrow__)); extern long double __cosl(long double __x) __attribute__((__nothrow__));

extern long double sinl(long double __x) __attribute__((__nothrow__)); extern long double __sinl(long double __x) __attribute__((__nothrow__));

extern long double tanl(long double __x) __attribute__((__nothrow__)); extern long double __tanl(long double __x) __attribute__((__nothrow__));

extern long double coshl(long double __x) __attribute__((__nothrow__)); extern long double __coshl(long double __x) __attribute__((__nothrow__));

extern long double sinhl(long double __x) __attribute__((__nothrow__)); extern long double __sinhl(long double __x) __attribute__((__nothrow__));

extern long double tanhl(long double __x) __attribute__((__nothrow__)); extern long double __tanhl(long double __x) __attribute__((__nothrow__));
extern long double acoshl(long double __x) __attribute__((__nothrow__)); extern long double __acoshl(long double __x) __attribute__((__nothrow__));

extern long double asinhl(long double __x) __attribute__((__nothrow__)); extern long double __asinhl(long double __x) __attribute__((__nothrow__));

extern long double atanhl(long double __x) __attribute__((__nothrow__)); extern long double __atanhl(long double __x) __attribute__((__nothrow__));

extern long double expl(long double __x) __attribute__((__nothrow__)); extern long double __expl(long double __x) __attribute__((__nothrow__));

extern long double frexpl(long double __x, int* __exponent) __attribute__((__nothrow__)); extern long double __frexpl(long double __x, int* __exponent) __attribute__((__nothrow__));

extern long double ldexpl(long double __x, int __exponent) __attribute__((__nothrow__)); extern long double __ldexpl(long double __x, int __exponent) __attribute__((__nothrow__));

extern long double logl(long double __x) __attribute__((__nothrow__)); extern long double __logl(long double __x) __attribute__((__nothrow__));

extern long double log10l(long double __x) __attribute__((__nothrow__)); extern long double __log10l(long double __x) __attribute__((__nothrow__));

extern long double modfl(long double __x, long double* __iptr) __attribute__((__nothrow__)); extern long double __modfl(long double __x, long double* __iptr) __attribute__((__nothrow__)) __attribute__((__nonnull__(2)));
extern long double expm1l(long double __x) __attribute__((__nothrow__)); extern long double __expm1l(long double __x) __attribute__((__nothrow__));

extern long double log1pl(long double __x) __attribute__((__nothrow__)); extern long double __log1pl(long double __x) __attribute__((__nothrow__));

extern long double logbl(long double __x) __attribute__((__nothrow__)); extern long double __logbl(long double __x) __attribute__((__nothrow__));

extern long double exp2l(long double __x) __attribute__((__nothrow__)); extern long double __exp2l(long double __x) __attribute__((__nothrow__));

extern long double log2l(long double __x) __attribute__((__nothrow__)); extern long double __log2l(long double __x) __attribute__((__nothrow__));

extern long double powl(long double __x, long double __y) __attribute__((__nothrow__)); extern long double __powl(long double __x, long double __y) __attribute__((__nothrow__));

extern long double sqrtl(long double __x) __attribute__((__nothrow__)); extern long double __sqrtl(long double __x) __attribute__((__nothrow__));

extern long double hypotl(long double __x, long double __y) __attribute__((__nothrow__)); extern long double __hypotl(long double __x, long double __y) __attribute__((__nothrow__));

extern long double cbrtl(long double __x) __attribute__((__nothrow__)); extern long double __cbrtl(long double __x) __attribute__((__nothrow__));

extern long double ceill(long double __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern long double __ceill(long double __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern long double fabsl(long double __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern long double __fabsl(long double __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern long double floorl(long double __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern long double __floorl(long double __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern long double fmodl(long double __x, long double __y) __attribute__((__nothrow__)); extern long double __fmodl(long double __x, long double __y) __attribute__((__nothrow__));
extern int isinfl(long double __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern int finitel(long double __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern long double dreml(long double __x, long double __y) __attribute__((__nothrow__)); extern long double __dreml(long double __x, long double __y) __attribute__((__nothrow__));

extern long double significandl(long double __x) __attribute__((__nothrow__)); extern long double __significandl(long double __x) __attribute__((__nothrow__));

extern long double copysignl(long double __x, long double __y) __attribute__((__nothrow__)) __attribute__((__const__)); extern long double __copysignl(long double __x, long double __y) __attribute__((__nothrow__)) __attribute__((__const__));

extern long double nanl(const char* __tagb) __attribute__((__nothrow__)) __attribute__((__const__)); extern long double __nanl(const char* __tagb) __attribute__((__nothrow__)) __attribute__((__const__));
extern int isnanl(long double __value) __attribute__((__nothrow__)) __attribute__((__const__));

extern long double j0l(long double) __attribute__((__nothrow__)); extern long double __j0l(long double) __attribute__((__nothrow__));
extern long double j1l(long double) __attribute__((__nothrow__)); extern long double __j1l(long double) __attribute__((__nothrow__));
extern long double jnl(int, long double) __attribute__((__nothrow__)); extern long double __jnl(int, long double) __attribute__((__nothrow__));
extern long double y0l(long double) __attribute__((__nothrow__)); extern long double __y0l(long double) __attribute__((__nothrow__));
extern long double y1l(long double) __attribute__((__nothrow__)); extern long double __y1l(long double) __attribute__((__nothrow__));
extern long double ynl(int, long double) __attribute__((__nothrow__)); extern long double __ynl(int, long double) __attribute__((__nothrow__));

extern long double erfl(long double) __attribute__((__nothrow__)); extern long double __erfl(long double) __attribute__((__nothrow__));
extern long double erfcl(long double) __attribute__((__nothrow__)); extern long double __erfcl(long double) __attribute__((__nothrow__));
extern long double lgammal(long double) __attribute__((__nothrow__)); extern long double __lgammal(long double) __attribute__((__nothrow__));

extern long double tgammal(long double) __attribute__((__nothrow__)); extern long double __tgammal(long double) __attribute__((__nothrow__));

extern long double gammal(long double) __attribute__((__nothrow__)); extern long double __gammal(long double) __attribute__((__nothrow__));

extern long double lgammal_r(long double, int* __signgamp) __attribute__((__nothrow__)); extern long double __lgammal_r(long double, int* __signgamp) __attribute__((__nothrow__));

extern long double rintl(long double __x) __attribute__((__nothrow__)); extern long double __rintl(long double __x) __attribute__((__nothrow__));

extern long double nextafterl(long double __x, long double __y) __attribute__((__nothrow__)); extern long double __nextafterl(long double __x, long double __y) __attribute__((__nothrow__));

extern long double nexttowardl(long double __x, long double __y) __attribute__((__nothrow__)); extern long double __nexttowardl(long double __x, long double __y) __attribute__((__nothrow__));
extern long double remainderl(long double __x, long double __y) __attribute__((__nothrow__)); extern long double __remainderl(long double __x, long double __y) __attribute__((__nothrow__));

extern long double scalbnl(long double __x, int __n) __attribute__((__nothrow__)); extern long double __scalbnl(long double __x, int __n) __attribute__((__nothrow__));

extern int ilogbl(long double __x) __attribute__((__nothrow__)); extern int __ilogbl(long double __x) __attribute__((__nothrow__));
extern long double scalblnl(long double __x, long int __n) __attribute__((__nothrow__)); extern long double __scalblnl(long double __x, long int __n) __attribute__((__nothrow__));

extern long double nearbyintl(long double __x) __attribute__((__nothrow__)); extern long double __nearbyintl(long double __x) __attribute__((__nothrow__));

extern long double roundl(long double __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern long double __roundl(long double __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern long double truncl(long double __x) __attribute__((__nothrow__)) __attribute__((__const__)); extern long double __truncl(long double __x) __attribute__((__nothrow__)) __attribute__((__const__));

extern long double remquol(long double __x, long double __y, int* __quo) __attribute__((__nothrow__)); extern long double __remquol(long double __x, long double __y, int* __quo) __attribute__((__nothrow__));

extern long int lrintl(long double __x) __attribute__((__nothrow__)); extern long int __lrintl(long double __x) __attribute__((__nothrow__));
__extension__
extern long long int llrintl(long double __x) __attribute__((__nothrow__)); extern long long int __llrintl(long double __x) __attribute__((__nothrow__));

extern long int lroundl(long double __x) __attribute__((__nothrow__)); extern long int __lroundl(long double __x) __attribute__((__nothrow__));
__extension__
extern long long int llroundl(long double __x) __attribute__((__nothrow__)); extern long long int __llroundl(long double __x) __attribute__((__nothrow__));

extern long double fdiml(long double __x, long double __y) __attribute__((__nothrow__)); extern long double __fdiml(long double __x, long double __y) __attribute__((__nothrow__));

extern long double fmaxl(long double __x, long double __y) __attribute__((__nothrow__)) __attribute__((__const__)); extern long double __fmaxl(long double __x, long double __y) __attribute__((__nothrow__)) __attribute__((__const__));

extern long double fminl(long double __x, long double __y) __attribute__((__nothrow__)) __attribute__((__const__)); extern long double __fminl(long double __x, long double __y) __attribute__((__nothrow__)) __attribute__((__const__));

extern long double fmal(long double __x, long double __y, long double __z) __attribute__((__nothrow__)); extern long double __fmal(long double __x, long double __y, long double __z) __attribute__((__nothrow__));
extern long double scalbl(long double __x, long double __n) __attribute__((__nothrow__)); extern long double __scalbl(long double __x, long double __n) __attribute__((__nothrow__));
extern int signgam;
enum
{
    FP_NAN =

    0,
    FP_INFINITE =

    1,
    FP_ZERO =

    2,
    FP_SUBNORMAL =

    3,
    FP_NORMAL =

    4
};

extern void __assert_fail(const char* __assertion, const char* __file,
    unsigned int __line, const char* __function)
    __attribute__((__nothrow__)) __attribute__((__noreturn__));

extern void __assert_perror_fail(int __errnum, const char* __file,
    unsigned int __line, const char* __function)
    __attribute__((__nothrow__)) __attribute__((__noreturn__));

extern void __assert(const char* __assertion, const char* __file, int __line)
__attribute__((__nothrow__)) __attribute__((__noreturn__));

typedef __uint8_t uint8_t;
typedef __uint16_t uint16_t;
typedef __uint32_t uint32_t;
typedef __uint64_t uint64_t;

typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;

typedef long int int_least64_t;

typedef unsigned char uint_least8_t;
typedef unsigned short int uint_least16_t;
typedef unsigned int uint_least32_t;

typedef unsigned long int uint_least64_t;
typedef signed char int_fast8_t;

typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
typedef unsigned char uint_fast8_t;

typedef unsigned long int uint_fast16_t;
typedef unsigned long int uint_fast32_t;
typedef unsigned long int uint_fast64_t;
typedef long int intptr_t;

typedef unsigned long int uintptr_t;
typedef __intmax_t intmax_t;
typedef __uintmax_t uintmax_t;
typedef uint16_t stbi__uint16;
typedef int16_t stbi__int16;
typedef uint32_t stbi__uint32;
typedef int32_t stbi__int32;

typedef unsigned char validate_uint32[sizeof(stbi__uint32) == 4 ? 1 : -1];
typedef long long __m64 __attribute__((__vector_size__(8), __aligned__(8)));

typedef long long __v1di __attribute__((__vector_size__(8)));
typedef int __v2si __attribute__((__vector_size__(8)));
typedef short __v4hi __attribute__((__vector_size__(8)));
typedef char __v8qi __attribute__((__vector_size__(8)));
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("mmx")))
_mm_empty(void)
{
    __builtin_ia32_emms();
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_cvtsi32_si64(int __i)
{
    return (__m64)__builtin_ia32_vec_init_v2si(__i, 0);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_cvtsi64_si32(__m64 __m)
{
    return __builtin_ia32_vec_ext_v2si((__v2si)__m, 0);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_cvtsi64_m64(long long __i)
{
    return (__m64)__i;
}
static __inline__ long long __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_cvtm64_si64(__m64 __m)
{
    return (long long)__m;
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_packs_pi16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_packsswb((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_packs_pi32(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_packssdw((__v2si)__m1, (__v2si)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_packs_pu16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_packuswb((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_unpackhi_pi8(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_punpckhbw((__v8qi)__m1, (__v8qi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_unpackhi_pi16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_punpckhwd((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_unpackhi_pi32(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_punpckhdq((__v2si)__m1, (__v2si)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_unpacklo_pi8(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_punpcklbw((__v8qi)__m1, (__v8qi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_unpacklo_pi16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_punpcklwd((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_unpacklo_pi32(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_punpckldq((__v2si)__m1, (__v2si)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_add_pi8(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_paddb((__v8qi)__m1, (__v8qi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_add_pi16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_paddw((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_add_pi32(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_paddd((__v2si)__m1, (__v2si)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_adds_pi8(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_paddsb((__v8qi)__m1, (__v8qi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_adds_pi16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_paddsw((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_adds_pu8(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_paddusb((__v8qi)__m1, (__v8qi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_adds_pu16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_paddusw((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_sub_pi8(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_psubb((__v8qi)__m1, (__v8qi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_sub_pi16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_psubw((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_sub_pi32(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_psubd((__v2si)__m1, (__v2si)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_subs_pi8(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_psubsb((__v8qi)__m1, (__v8qi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_subs_pi16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_psubsw((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_subs_pu8(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_psubusb((__v8qi)__m1, (__v8qi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_subs_pu16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_psubusw((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_madd_pi16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_pmaddwd((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_mulhi_pi16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_pmulhw((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_mullo_pi16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_pmullw((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_sll_pi16(__m64 __m, __m64 __count)
{
    return (__m64)__builtin_ia32_psllw((__v4hi)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_slli_pi16(__m64 __m, int __count)
{
    return (__m64)__builtin_ia32_psllwi((__v4hi)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_sll_pi32(__m64 __m, __m64 __count)
{
    return (__m64)__builtin_ia32_pslld((__v2si)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_slli_pi32(__m64 __m, int __count)
{
    return (__m64)__builtin_ia32_pslldi((__v2si)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_sll_si64(__m64 __m, __m64 __count)
{
    return (__m64)__builtin_ia32_psllq((__v1di)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_slli_si64(__m64 __m, int __count)
{
    return (__m64)__builtin_ia32_psllqi((__v1di)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_sra_pi16(__m64 __m, __m64 __count)
{
    return (__m64)__builtin_ia32_psraw((__v4hi)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_srai_pi16(__m64 __m, int __count)
{
    return (__m64)__builtin_ia32_psrawi((__v4hi)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_sra_pi32(__m64 __m, __m64 __count)
{
    return (__m64)__builtin_ia32_psrad((__v2si)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_srai_pi32(__m64 __m, int __count)
{
    return (__m64)__builtin_ia32_psradi((__v2si)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_srl_pi16(__m64 __m, __m64 __count)
{
    return (__m64)__builtin_ia32_psrlw((__v4hi)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_srli_pi16(__m64 __m, int __count)
{
    return (__m64)__builtin_ia32_psrlwi((__v4hi)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_srl_pi32(__m64 __m, __m64 __count)
{
    return (__m64)__builtin_ia32_psrld((__v2si)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_srli_pi32(__m64 __m, int __count)
{
    return (__m64)__builtin_ia32_psrldi((__v2si)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_srl_si64(__m64 __m, __m64 __count)
{
    return (__m64)__builtin_ia32_psrlq((__v1di)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_srli_si64(__m64 __m, int __count)
{
    return (__m64)__builtin_ia32_psrlqi((__v1di)__m, __count);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_and_si64(__m64 __m1, __m64 __m2)
{
    return __builtin_ia32_pand((__v1di)__m1, (__v1di)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_andnot_si64(__m64 __m1, __m64 __m2)
{
    return __builtin_ia32_pandn((__v1di)__m1, (__v1di)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_or_si64(__m64 __m1, __m64 __m2)
{
    return __builtin_ia32_por((__v1di)__m1, (__v1di)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_xor_si64(__m64 __m1, __m64 __m2)
{
    return __builtin_ia32_pxor((__v1di)__m1, (__v1di)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_cmpeq_pi8(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_pcmpeqb((__v8qi)__m1, (__v8qi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_cmpeq_pi16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_pcmpeqw((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_cmpeq_pi32(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_pcmpeqd((__v2si)__m1, (__v2si)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_cmpgt_pi8(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_pcmpgtb((__v8qi)__m1, (__v8qi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_cmpgt_pi16(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_pcmpgtw((__v4hi)__m1, (__v4hi)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_cmpgt_pi32(__m64 __m1, __m64 __m2)
{
    return (__m64)__builtin_ia32_pcmpgtd((__v2si)__m1, (__v2si)__m2);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_setzero_si64(void)
{
    return __extension__(__m64) { 0LL };
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_set_pi32(int __i1, int __i0)
{
    return (__m64)__builtin_ia32_vec_init_v2si(__i0, __i1);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_set_pi16(short __s3, short __s2, short __s1, short __s0)
{
    return (__m64)__builtin_ia32_vec_init_v4hi(__s0, __s1, __s2, __s3);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_set_pi8(char __b7, char __b6, char __b5, char __b4, char __b3, char __b2,
    char __b1, char __b0)
{
    return (__m64)__builtin_ia32_vec_init_v8qi(__b0, __b1, __b2, __b3,
        __b4, __b5, __b6, __b7);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_set1_pi32(int __i)
{
    return _mm_set_pi32(__i, __i);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_set1_pi16(short __w)
{
    return _mm_set_pi16(__w, __w, __w, __w);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_set1_pi8(char __b)
{
    return _mm_set_pi8(__b, __b, __b, __b, __b, __b, __b, __b);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_setr_pi32(int __i0, int __i1)
{
    return _mm_set_pi32(__i1, __i0);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_setr_pi16(short __w0, short __w1, short __w2, short __w3)
{
    return _mm_set_pi16(__w3, __w2, __w1, __w0);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx"), __min_vector_width__(64)))
_mm_setr_pi8(char __b0, char __b1, char __b2, char __b3, char __b4, char __b5,
    char __b6, char __b7)
{
    return _mm_set_pi8(__b7, __b6, __b5, __b4, __b3, __b2, __b1, __b0);
}

typedef int __v4si __attribute__((__vector_size__(16)));
typedef float __v4sf __attribute__((__vector_size__(16)));
typedef float __m128 __attribute__((__vector_size__(16), __aligned__(16)));

typedef float __m128_u __attribute__((__vector_size__(16), __aligned__(1)));

typedef unsigned int __v4su __attribute__((__vector_size__(16)));

extern int posix_memalign(void** __memptr, size_t __alignment, size_t __size);
static __inline__ void* __attribute__((__always_inline__, __nodebug__,
    __malloc__))
    _mm_malloc(size_t __size, size_t __align)
{
    if (__align == 1) {
        return malloc(__size);
    }

    if (!(__align & (__align - 1)) && __align < sizeof(void*))
        __align = sizeof(void*);

    void* __mallocedMemory;

    if (posix_memalign(&__mallocedMemory, __align, __size))
        return 0;

    return __mallocedMemory;
}

static __inline__ void __attribute__((__always_inline__, __nodebug__))
_mm_free(void* __p)
{
    free(__p);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_add_ss(__m128 __a, __m128 __b)
{
    __a[0] += __b[0];
    return __a;
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_add_ps(__m128 __a, __m128 __b)
{
    return (__m128)((__v4sf)__a + (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_sub_ss(__m128 __a, __m128 __b)
{
    __a[0] -= __b[0];
    return __a;
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_sub_ps(__m128 __a, __m128 __b)
{
    return (__m128)((__v4sf)__a - (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_mul_ss(__m128 __a, __m128 __b)
{
    __a[0] *= __b[0];
    return __a;
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_mul_ps(__m128 __a, __m128 __b)
{
    return (__m128)((__v4sf)__a * (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_div_ss(__m128 __a, __m128 __b)
{
    __a[0] /= __b[0];
    return __a;
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_div_ps(__m128 __a, __m128 __b)
{
    return (__m128)((__v4sf)__a / (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_sqrt_ss(__m128 __a)
{
    return (__m128)__builtin_ia32_sqrtss((__v4sf)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_sqrt_ps(__m128 __a)
{
    return __builtin_ia32_sqrtps((__v4sf)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_rcp_ss(__m128 __a)
{
    return (__m128)__builtin_ia32_rcpss((__v4sf)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_rcp_ps(__m128 __a)
{
    return (__m128)__builtin_ia32_rcpps((__v4sf)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_rsqrt_ss(__m128 __a)
{
    return __builtin_ia32_rsqrtss((__v4sf)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_rsqrt_ps(__m128 __a)
{
    return __builtin_ia32_rsqrtps((__v4sf)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_min_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_minss((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_min_ps(__m128 __a, __m128 __b)
{
    return __builtin_ia32_minps((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_max_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_maxss((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_max_ps(__m128 __a, __m128 __b)
{
    return __builtin_ia32_maxps((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_and_ps(__m128 __a, __m128 __b)
{
    return (__m128)((__v4su)__a & (__v4su)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_andnot_ps(__m128 __a, __m128 __b)
{
    return (__m128)(~(__v4su)__a & (__v4su)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_or_ps(__m128 __a, __m128 __b)
{
    return (__m128)((__v4su)__a | (__v4su)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_xor_ps(__m128 __a, __m128 __b)
{
    return (__m128)((__v4su)__a ^ (__v4su)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpeq_ss(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpeqss((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpeq_ps(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpeqps((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmplt_ss(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpltss((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmplt_ps(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpltps((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmple_ss(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpless((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmple_ps(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpleps((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpgt_ss(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_shufflevector((__v4sf)__a,
        (__v4sf)__builtin_ia32_cmpltss((__v4sf)__b, (__v4sf)__a),
        4, 1, 2, 3);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpgt_ps(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpltps((__v4sf)__b, (__v4sf)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpge_ss(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_shufflevector((__v4sf)__a,
        (__v4sf)__builtin_ia32_cmpless((__v4sf)__b, (__v4sf)__a),
        4, 1, 2, 3);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpge_ps(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpleps((__v4sf)__b, (__v4sf)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpneq_ss(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpneqss((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpneq_ps(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpneqps((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpnlt_ss(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpnltss((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpnlt_ps(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpnltps((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpnle_ss(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpnless((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpnle_ps(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpnleps((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpngt_ss(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_shufflevector((__v4sf)__a,
        (__v4sf)__builtin_ia32_cmpnltss((__v4sf)__b, (__v4sf)__a),
        4, 1, 2, 3);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpngt_ps(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpnltps((__v4sf)__b, (__v4sf)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpnge_ss(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_shufflevector((__v4sf)__a,
        (__v4sf)__builtin_ia32_cmpnless((__v4sf)__b, (__v4sf)__a),
        4, 1, 2, 3);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpnge_ps(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpnleps((__v4sf)__b, (__v4sf)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpord_ss(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpordss((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpord_ps(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpordps((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpunord_ss(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpunordss((__v4sf)__a, (__v4sf)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cmpunord_ps(__m128 __a, __m128 __b)
{
    return (__m128)__builtin_ia32_cmpunordps((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_comieq_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_comieq((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_comilt_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_comilt((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_comile_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_comile((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_comigt_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_comigt((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_comige_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_comige((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_comineq_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_comineq((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_ucomieq_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_ucomieq((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_ucomilt_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_ucomilt((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_ucomile_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_ucomile((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_ucomigt_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_ucomigt((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_ucomige_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_ucomige((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_ucomineq_ss(__m128 __a, __m128 __b)
{
    return __builtin_ia32_ucomineq((__v4sf)__a, (__v4sf)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cvtss_si32(__m128 __a)
{
    return __builtin_ia32_cvtss2si((__v4sf)__a);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cvt_ss2si(__m128 __a)
{
    return _mm_cvtss_si32(__a);
}
static __inline__ long long __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cvtss_si64(__m128 __a)
{
    return __builtin_ia32_cvtss2si64((__v4sf)__a);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvtps_pi32(__m128 __a)
{
    return (__m64)__builtin_ia32_cvtps2pi((__v4sf)__a);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvt_ps2pi(__m128 __a)
{
    return _mm_cvtps_pi32(__a);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cvttss_si32(__m128 __a)
{
    return __builtin_ia32_cvttss2si((__v4sf)__a);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cvtt_ss2si(__m128 __a)
{
    return _mm_cvttss_si32(__a);
}
static __inline__ long long __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cvttss_si64(__m128 __a)
{
    return __builtin_ia32_cvttss2si64((__v4sf)__a);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvttps_pi32(__m128 __a)
{
    return (__m64)__builtin_ia32_cvttps2pi((__v4sf)__a);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvtt_ps2pi(__m128 __a)
{
    return _mm_cvttps_pi32(__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cvtsi32_ss(__m128 __a, int __b)
{
    __a[0] = __b;
    return __a;
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cvt_si2ss(__m128 __a, int __b)
{
    return _mm_cvtsi32_ss(__a, __b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cvtsi64_ss(__m128 __a, long long __b)
{
    __a[0] = __b;
    return __a;
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvtpi32_ps(__m128 __a, __m64 __b)
{
    return __builtin_ia32_cvtpi2ps((__v4sf)__a, (__v2si)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvt_pi2ps(__m128 __a, __m64 __b)
{
    return _mm_cvtpi32_ps(__a, __b);
}
static __inline__ float __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_cvtss_f32(__m128 __a)
{
    return __a[0];
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_loadh_pi(__m128 __a, const __m64* __p)
{
    typedef float __mm_loadh_pi_v2f32 __attribute__((__vector_size__(8)));
    struct __mm_loadh_pi_struct {
        __mm_loadh_pi_v2f32 __u;
    } __attribute__((__packed__, __may_alias__));
    __mm_loadh_pi_v2f32 __b = ((const struct __mm_loadh_pi_struct*)__p)->__u;
    __m128 __bb = __builtin_shufflevector(__b, __b, 0, 1, 0, 1);
    return __builtin_shufflevector(__a, __bb, 0, 1, 4, 5);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_loadl_pi(__m128 __a, const __m64* __p)
{
    typedef float __mm_loadl_pi_v2f32 __attribute__((__vector_size__(8)));
    struct __mm_loadl_pi_struct {
        __mm_loadl_pi_v2f32 __u;
    } __attribute__((__packed__, __may_alias__));
    __mm_loadl_pi_v2f32 __b = ((const struct __mm_loadl_pi_struct*)__p)->__u;
    __m128 __bb = __builtin_shufflevector(__b, __b, 0, 1, 0, 1);
    return __builtin_shufflevector(__a, __bb, 4, 5, 2, 3);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_load_ss(const float* __p)
{
    struct __mm_load_ss_struct {
        float __u;
    } __attribute__((__packed__, __may_alias__));
    float __u = ((const struct __mm_load_ss_struct*)__p)->__u;
    return __extension__(__m128) { __u, 0, 0, 0 };
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_load1_ps(const float* __p)
{
    struct __mm_load1_ps_struct {
        float __u;
    } __attribute__((__packed__, __may_alias__));
    float __u = ((const struct __mm_load1_ps_struct*)__p)->__u;
    return __extension__(__m128) { __u, __u, __u, __u };
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_load_ps(const float* __p)
{
    return *(const __m128*)__p;
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_loadu_ps(const float* __p)
{
    struct __loadu_ps {
        __m128_u __v;
    } __attribute__((__packed__, __may_alias__));
    return ((const struct __loadu_ps*)__p)->__v;
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_loadr_ps(const float* __p)
{
    __m128 __a = _mm_load_ps(__p);
    return __builtin_shufflevector((__v4sf)__a, (__v4sf)__a, 3, 2, 1, 0);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_undefined_ps(void)
{
    return (__m128)__builtin_ia32_undef128();
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_set_ss(float __w)
{
    return __extension__(__m128) { __w, 0, 0, 0 };
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_set1_ps(float __w)
{
    return __extension__(__m128) { __w, __w, __w, __w };
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_set_ps1(float __w)
{
    return _mm_set1_ps(__w);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_set_ps(float __z, float __y, float __x, float __w)
{
    return __extension__(__m128) { __w, __x, __y, __z };
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_setr_ps(float __z, float __y, float __x, float __w)
{
    return __extension__(__m128) { __z, __y, __x, __w };
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_setzero_ps(void)
{
    return __extension__(__m128) { 0, 0, 0, 0 };
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_storeh_pi(__m64* __p, __m128 __a)
{
    typedef float __mm_storeh_pi_v2f32 __attribute__((__vector_size__(8)));
    struct __mm_storeh_pi_struct {
        __mm_storeh_pi_v2f32 __u;
    } __attribute__((__packed__, __may_alias__));
    ((struct __mm_storeh_pi_struct*)__p)->__u = __builtin_shufflevector(__a, __a, 2, 3);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_storel_pi(__m64* __p, __m128 __a)
{
    typedef float __mm_storeh_pi_v2f32 __attribute__((__vector_size__(8)));
    struct __mm_storeh_pi_struct {
        __mm_storeh_pi_v2f32 __u;
    } __attribute__((__packed__, __may_alias__));
    ((struct __mm_storeh_pi_struct*)__p)->__u = __builtin_shufflevector(__a, __a, 0, 1);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_store_ss(float* __p, __m128 __a)
{
    struct __mm_store_ss_struct {
        float __u;
    } __attribute__((__packed__, __may_alias__));
    ((struct __mm_store_ss_struct*)__p)->__u = __a[0];
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_storeu_ps(float* __p, __m128 __a)
{
    struct __storeu_ps {
        __m128_u __v;
    } __attribute__((__packed__, __may_alias__));
    ((struct __storeu_ps*)__p)->__v = __a;
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_store_ps(float* __p, __m128 __a)
{
    *(__m128*)__p = __a;
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_store1_ps(float* __p, __m128 __a)
{
    __a = __builtin_shufflevector((__v4sf)__a, (__v4sf)__a, 0, 0, 0, 0);
    _mm_store_ps(__p, __a);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_store_ps1(float* __p, __m128 __a)
{
    _mm_store1_ps(__p, __a);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_storer_ps(float* __p, __m128 __a)
{
    __a = __builtin_shufflevector((__v4sf)__a, (__v4sf)__a, 3, 2, 1, 0);
    _mm_store_ps(__p, __a);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_stream_pi(__m64* __p, __m64 __a)
{
    __builtin_ia32_movntq(__p, __a);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_stream_ps(float* __p, __m128 __a)
{
    __builtin_nontemporal_store((__v4sf)__a, (__v4sf*)__p);
}
void _mm_sfence(void);
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_max_pi16(__m64 __a, __m64 __b)
{
    return (__m64)__builtin_ia32_pmaxsw((__v4hi)__a, (__v4hi)__b);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_max_pu8(__m64 __a, __m64 __b)
{
    return (__m64)__builtin_ia32_pmaxub((__v8qi)__a, (__v8qi)__b);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_min_pi16(__m64 __a, __m64 __b)
{
    return (__m64)__builtin_ia32_pminsw((__v4hi)__a, (__v4hi)__b);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_min_pu8(__m64 __a, __m64 __b)
{
    return (__m64)__builtin_ia32_pminub((__v8qi)__a, (__v8qi)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_movemask_pi8(__m64 __a)
{
    return __builtin_ia32_pmovmskb((__v8qi)__a);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_mulhi_pu16(__m64 __a, __m64 __b)
{
    return (__m64)__builtin_ia32_pmulhuw((__v4hi)__a, (__v4hi)__b);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_maskmove_si64(__m64 __d, __m64 __n, char* __p)
{
    __builtin_ia32_maskmovq((__v8qi)__d, (__v8qi)__n, __p);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_avg_pu8(__m64 __a, __m64 __b)
{
    return (__m64)__builtin_ia32_pavgb((__v8qi)__a, (__v8qi)__b);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_avg_pu16(__m64 __a, __m64 __b)
{
    return (__m64)__builtin_ia32_pavgw((__v4hi)__a, (__v4hi)__b);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_sad_pu8(__m64 __a, __m64 __b)
{
    return (__m64)__builtin_ia32_psadbw((__v8qi)__a, (__v8qi)__b);
}
unsigned int _mm_getcsr(void);
void _mm_setcsr(unsigned int __i);
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_unpackhi_ps(__m128 __a, __m128 __b)
{
    return __builtin_shufflevector((__v4sf)__a, (__v4sf)__b, 2, 6, 3, 7);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_unpacklo_ps(__m128 __a, __m128 __b)
{
    return __builtin_shufflevector((__v4sf)__a, (__v4sf)__b, 0, 4, 1, 5);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_move_ss(__m128 __a, __m128 __b)
{
    __a[0] = __b[0];
    return __a;
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_movehl_ps(__m128 __a, __m128 __b)
{
    return __builtin_shufflevector((__v4sf)__a, (__v4sf)__b, 6, 7, 2, 3);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_movelh_ps(__m128 __a, __m128 __b)
{
    return __builtin_shufflevector((__v4sf)__a, (__v4sf)__b, 0, 1, 4, 5);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvtpi16_ps(__m64 __a)
{
    __m64 __b, __c;
    __m128 __r;

    __b = _mm_setzero_si64();
    __b = _mm_cmpgt_pi16(__b, __a);
    __c = _mm_unpackhi_pi16(__a, __b);
    __r = _mm_setzero_ps();
    __r = _mm_cvtpi32_ps(__r, __c);
    __r = _mm_movelh_ps(__r, __r);
    __c = _mm_unpacklo_pi16(__a, __b);
    __r = _mm_cvtpi32_ps(__r, __c);

    return __r;
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvtpu16_ps(__m64 __a)
{
    __m64 __b, __c;
    __m128 __r;

    __b = _mm_setzero_si64();
    __c = _mm_unpackhi_pi16(__a, __b);
    __r = _mm_setzero_ps();
    __r = _mm_cvtpi32_ps(__r, __c);
    __r = _mm_movelh_ps(__r, __r);
    __c = _mm_unpacklo_pi16(__a, __b);
    __r = _mm_cvtpi32_ps(__r, __c);

    return __r;
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvtpi8_ps(__m64 __a)
{
    __m64 __b;

    __b = _mm_setzero_si64();
    __b = _mm_cmpgt_pi8(__b, __a);
    __b = _mm_unpacklo_pi8(__a, __b);

    return _mm_cvtpi16_ps(__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvtpu8_ps(__m64 __a)
{
    __m64 __b;

    __b = _mm_setzero_si64();
    __b = _mm_unpacklo_pi8(__a, __b);

    return _mm_cvtpi16_ps(__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvtpi32x2_ps(__m64 __a, __m64 __b)
{
    __m128 __c;

    __c = _mm_setzero_ps();
    __c = _mm_cvtpi32_ps(__c, __b);
    __c = _mm_movelh_ps(__c, __c);

    return _mm_cvtpi32_ps(__c, __a);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvtps_pi16(__m128 __a)
{
    __m64 __b, __c;

    __b = _mm_cvtps_pi32(__a);
    __a = _mm_movehl_ps(__a, __a);
    __c = _mm_cvtps_pi32(__a);

    return _mm_packs_pi32(__b, __c);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse"), __min_vector_width__(64)))
_mm_cvtps_pi8(__m128 __a)
{
    __m64 __b, __c;

    __b = _mm_cvtps_pi16(__a);
    __c = _mm_setzero_si64();

    return _mm_packs_pi16(__b, __c);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse"), __min_vector_width__(128)))
_mm_movemask_ps(__m128 __a)
{
    return __builtin_ia32_movmskps((__v4sf)__a);
}

typedef double __m128d __attribute__((__vector_size__(16), __aligned__(16)));
typedef long long __m128i __attribute__((__vector_size__(16), __aligned__(16)));

typedef double __m128d_u __attribute__((__vector_size__(16), __aligned__(1)));
typedef long long __m128i_u __attribute__((__vector_size__(16), __aligned__(1)));

typedef double __v2df __attribute__((__vector_size__(16)));
typedef long long __v2di __attribute__((__vector_size__(16)));
typedef short __v8hi __attribute__((__vector_size__(16)));
typedef char __v16qi __attribute__((__vector_size__(16)));

typedef unsigned long long __v2du __attribute__((__vector_size__(16)));
typedef unsigned short __v8hu __attribute__((__vector_size__(16)));
typedef unsigned char __v16qu __attribute__((__vector_size__(16)));

typedef signed char __v16qs __attribute__((__vector_size__(16)));
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_add_sd(__m128d __a, __m128d __b)
{
    __a[0] += __b[0];
    return __a;
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_add_pd(__m128d __a, __m128d __b)
{
    return (__m128d)((__v2df)__a + (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sub_sd(__m128d __a, __m128d __b)
{
    __a[0] -= __b[0];
    return __a;
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sub_pd(__m128d __a, __m128d __b)
{
    return (__m128d)((__v2df)__a - (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_mul_sd(__m128d __a, __m128d __b)
{
    __a[0] *= __b[0];
    return __a;
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_mul_pd(__m128d __a, __m128d __b)
{
    return (__m128d)((__v2df)__a * (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_div_sd(__m128d __a, __m128d __b)
{
    __a[0] /= __b[0];
    return __a;
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_div_pd(__m128d __a, __m128d __b)
{
    return (__m128d)((__v2df)__a / (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sqrt_sd(__m128d __a, __m128d __b)
{
    __m128d __c = __builtin_ia32_sqrtsd((__v2df)__b);
    return __extension__(__m128d) { __c[0], __a[1] };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sqrt_pd(__m128d __a)
{
    return __builtin_ia32_sqrtpd((__v2df)__a);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_min_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_minsd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_min_pd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_minpd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_max_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_maxsd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_max_pd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_maxpd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_and_pd(__m128d __a, __m128d __b)
{
    return (__m128d)((__v2du)__a & (__v2du)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_andnot_pd(__m128d __a, __m128d __b)
{
    return (__m128d)(~(__v2du)__a & (__v2du)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_or_pd(__m128d __a, __m128d __b)
{
    return (__m128d)((__v2du)__a | (__v2du)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_xor_pd(__m128d __a, __m128d __b)
{
    return (__m128d)((__v2du)__a ^ (__v2du)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpeq_pd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpeqpd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmplt_pd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpltpd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmple_pd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmplepd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpgt_pd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpltpd((__v2df)__b, (__v2df)__a);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpge_pd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmplepd((__v2df)__b, (__v2df)__a);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpord_pd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpordpd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpunord_pd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpunordpd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpneq_pd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpneqpd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpnlt_pd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpnltpd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpnle_pd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpnlepd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpngt_pd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpnltpd((__v2df)__b, (__v2df)__a);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpnge_pd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpnlepd((__v2df)__b, (__v2df)__a);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpeq_sd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpeqsd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmplt_sd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpltsd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmple_sd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmplesd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpgt_sd(__m128d __a, __m128d __b)
{
    __m128d __c = __builtin_ia32_cmpltsd((__v2df)__b, (__v2df)__a);
    return __extension__(__m128d) { __c[0], __a[1] };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpge_sd(__m128d __a, __m128d __b)
{
    __m128d __c = __builtin_ia32_cmplesd((__v2df)__b, (__v2df)__a);
    return __extension__(__m128d) { __c[0], __a[1] };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpord_sd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpordsd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpunord_sd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpunordsd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpneq_sd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpneqsd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpnlt_sd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpnltsd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpnle_sd(__m128d __a, __m128d __b)
{
    return (__m128d)__builtin_ia32_cmpnlesd((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpngt_sd(__m128d __a, __m128d __b)
{
    __m128d __c = __builtin_ia32_cmpnltsd((__v2df)__b, (__v2df)__a);
    return __extension__(__m128d) { __c[0], __a[1] };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpnge_sd(__m128d __a, __m128d __b)
{
    __m128d __c = __builtin_ia32_cmpnlesd((__v2df)__b, (__v2df)__a);
    return __extension__(__m128d) { __c[0], __a[1] };
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_comieq_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_comisdeq((__v2df)__a, (__v2df)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_comilt_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_comisdlt((__v2df)__a, (__v2df)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_comile_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_comisdle((__v2df)__a, (__v2df)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_comigt_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_comisdgt((__v2df)__a, (__v2df)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_comige_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_comisdge((__v2df)__a, (__v2df)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_comineq_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_comisdneq((__v2df)__a, (__v2df)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_ucomieq_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_ucomisdeq((__v2df)__a, (__v2df)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_ucomilt_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_ucomisdlt((__v2df)__a, (__v2df)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_ucomile_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_ucomisdle((__v2df)__a, (__v2df)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_ucomigt_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_ucomisdgt((__v2df)__a, (__v2df)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_ucomige_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_ucomisdge((__v2df)__a, (__v2df)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_ucomineq_sd(__m128d __a, __m128d __b)
{
    return __builtin_ia32_ucomisdneq((__v2df)__a, (__v2df)__b);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtpd_ps(__m128d __a)
{
    return __builtin_ia32_cvtpd2ps((__v2df)__a);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtps_pd(__m128 __a)
{
    return (__m128d) __builtin_convertvector(
        __builtin_shufflevector((__v4sf)__a, (__v4sf)__a, 0, 1), __v2df);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtepi32_pd(__m128i __a)
{
    return (__m128d) __builtin_convertvector(
        __builtin_shufflevector((__v4si)__a, (__v4si)__a, 0, 1), __v2df);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtpd_epi32(__m128d __a)
{
    return __builtin_ia32_cvtpd2dq((__v2df)__a);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtsd_si32(__m128d __a)
{
    return __builtin_ia32_cvtsd2si((__v2df)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtsd_ss(__m128 __a, __m128d __b)
{
    return (__m128)__builtin_ia32_cvtsd2ss((__v4sf)__a, (__v2df)__b);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtsi32_sd(__m128d __a, int __b)
{
    __a[0] = __b;
    return __a;
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtss_sd(__m128d __a, __m128 __b)
{
    __a[0] = __b[0];
    return __a;
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvttpd_epi32(__m128d __a)
{
    return (__m128i)__builtin_ia32_cvttpd2dq((__v2df)__a);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvttsd_si32(__m128d __a)
{
    return __builtin_ia32_cvttsd2si((__v2df)__a);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse2"), __min_vector_width__(64)))
_mm_cvtpd_pi32(__m128d __a)
{
    return (__m64)__builtin_ia32_cvtpd2pi((__v2df)__a);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse2"), __min_vector_width__(64)))
_mm_cvttpd_pi32(__m128d __a)
{
    return (__m64)__builtin_ia32_cvttpd2pi((__v2df)__a);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse2"), __min_vector_width__(64)))
_mm_cvtpi32_pd(__m64 __a)
{
    return __builtin_ia32_cvtpi2pd((__v2si)__a);
}
static __inline__ double __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtsd_f64(__m128d __a)
{
    return __a[0];
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_load_pd(double const* __dp)
{
    return *(const __m128d*)__dp;
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_load1_pd(double const* __dp)
{
    struct __mm_load1_pd_struct {
        double __u;
    } __attribute__((__packed__, __may_alias__));
    double __u = ((const struct __mm_load1_pd_struct*)__dp)->__u;
    return __extension__(__m128d) { __u, __u };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_loadr_pd(double const* __dp)
{
    __m128d __u = *(const __m128d*)__dp;
    return __builtin_shufflevector((__v2df)__u, (__v2df)__u, 1, 0);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_loadu_pd(double const* __dp)
{
    struct __loadu_pd {
        __m128d_u __v;
    } __attribute__((__packed__, __may_alias__));
    return ((const struct __loadu_pd*)__dp)->__v;
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_loadu_si64(void const* __a)
{
    struct __loadu_si64 {
        long long __v;
    } __attribute__((__packed__, __may_alias__));
    long long __u = ((const struct __loadu_si64*)__a)->__v;
    return __extension__(__m128i)(__v2di) { __u, 0LL };
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_loadu_si32(void const* __a)
{
    struct __loadu_si32 {
        int __v;
    } __attribute__((__packed__, __may_alias__));
    int __u = ((const struct __loadu_si32*)__a)->__v;
    return __extension__(__m128i)(__v4si) { __u, 0, 0, 0 };
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_loadu_si16(void const* __a)
{
    struct __loadu_si16 {
        short __v;
    } __attribute__((__packed__, __may_alias__));
    short __u = ((const struct __loadu_si16*)__a)->__v;
    return __extension__(__m128i)(__v8hi) { __u, 0, 0, 0, 0, 0, 0, 0 };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_load_sd(double const* __dp)
{
    struct __mm_load_sd_struct {
        double __u;
    } __attribute__((__packed__, __may_alias__));
    double __u = ((const struct __mm_load_sd_struct*)__dp)->__u;
    return __extension__(__m128d) { __u, 0 };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_loadh_pd(__m128d __a, double const* __dp)
{
    struct __mm_loadh_pd_struct {
        double __u;
    } __attribute__((__packed__, __may_alias__));
    double __u = ((const struct __mm_loadh_pd_struct*)__dp)->__u;
    return __extension__(__m128d) { __a[0], __u };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_loadl_pd(__m128d __a, double const* __dp)
{
    struct __mm_loadl_pd_struct {
        double __u;
    } __attribute__((__packed__, __may_alias__));
    double __u = ((const struct __mm_loadl_pd_struct*)__dp)->__u;
    return __extension__(__m128d) { __u, __a[1] };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_undefined_pd(void)
{
    return (__m128d)__builtin_ia32_undef128();
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set_sd(double __w)
{
    return __extension__(__m128d) { __w, 0 };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set1_pd(double __w)
{
    return __extension__(__m128d) { __w, __w };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set_pd1(double __w)
{
    return _mm_set1_pd(__w);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set_pd(double __w, double __x)
{
    return __extension__(__m128d) { __x, __w };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_setr_pd(double __w, double __x)
{
    return __extension__(__m128d) { __w, __x };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_setzero_pd(void)
{
    return __extension__(__m128d) { 0, 0 };
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_move_sd(__m128d __a, __m128d __b)
{
    __a[0] = __b[0];
    return __a;
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_store_sd(double* __dp, __m128d __a)
{
    struct __mm_store_sd_struct {
        double __u;
    } __attribute__((__packed__, __may_alias__));
    ((struct __mm_store_sd_struct*)__dp)->__u = __a[0];
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_store_pd(double* __dp, __m128d __a)
{
    *(__m128d*)__dp = __a;
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_store1_pd(double* __dp, __m128d __a)
{
    __a = __builtin_shufflevector((__v2df)__a, (__v2df)__a, 0, 0);
    _mm_store_pd(__dp, __a);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_store_pd1(double* __dp, __m128d __a)
{
    _mm_store1_pd(__dp, __a);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_storeu_pd(double* __dp, __m128d __a)
{
    struct __storeu_pd {
        __m128d_u __v;
    } __attribute__((__packed__, __may_alias__));
    ((struct __storeu_pd*)__dp)->__v = __a;
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_storer_pd(double* __dp, __m128d __a)
{
    __a = __builtin_shufflevector((__v2df)__a, (__v2df)__a, 1, 0);
    *(__m128d*)__dp = __a;
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_storeh_pd(double* __dp, __m128d __a)
{
    struct __mm_storeh_pd_struct {
        double __u;
    } __attribute__((__packed__, __may_alias__));
    ((struct __mm_storeh_pd_struct*)__dp)->__u = __a[1];
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_storel_pd(double* __dp, __m128d __a)
{
    struct __mm_storeh_pd_struct {
        double __u;
    } __attribute__((__packed__, __may_alias__));
    ((struct __mm_storeh_pd_struct*)__dp)->__u = __a[0];
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_add_epi8(__m128i __a, __m128i __b)
{
    return (__m128i)((__v16qu)__a + (__v16qu)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_add_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)((__v8hu)__a + (__v8hu)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_add_epi32(__m128i __a, __m128i __b)
{
    return (__m128i)((__v4su)__a + (__v4su)__b);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse2"), __min_vector_width__(64)))
_mm_add_si64(__m64 __a, __m64 __b)
{
    return (__m64)__builtin_ia32_paddq((__v1di)__a, (__v1di)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_add_epi64(__m128i __a, __m128i __b)
{
    return (__m128i)((__v2du)__a + (__v2du)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_adds_epi8(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_paddsb128((__v16qi)__a, (__v16qi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_adds_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_paddsw128((__v8hi)__a, (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_adds_epu8(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_paddusb128((__v16qi)__a, (__v16qi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_adds_epu16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_paddusw128((__v8hi)__a, (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_avg_epu8(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_pavgb128((__v16qi)__a, (__v16qi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_avg_epu16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_pavgw128((__v8hi)__a, (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_madd_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_pmaddwd128((__v8hi)__a, (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_max_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_pmaxsw128((__v8hi)__a, (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_max_epu8(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_pmaxub128((__v16qi)__a, (__v16qi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_min_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_pminsw128((__v8hi)__a, (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_min_epu8(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_pminub128((__v16qi)__a, (__v16qi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_mulhi_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_pmulhw128((__v8hi)__a, (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_mulhi_epu16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_pmulhuw128((__v8hi)__a, (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_mullo_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)((__v8hu)__a * (__v8hu)__b);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse2"), __min_vector_width__(64)))
_mm_mul_su32(__m64 __a, __m64 __b)
{
    return __builtin_ia32_pmuludq((__v2si)__a, (__v2si)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_mul_epu32(__m128i __a, __m128i __b)
{
    return __builtin_ia32_pmuludq128((__v4si)__a, (__v4si)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sad_epu8(__m128i __a, __m128i __b)
{
    return __builtin_ia32_psadbw128((__v16qi)__a, (__v16qi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sub_epi8(__m128i __a, __m128i __b)
{
    return (__m128i)((__v16qu)__a - (__v16qu)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sub_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)((__v8hu)__a - (__v8hu)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sub_epi32(__m128i __a, __m128i __b)
{
    return (__m128i)((__v4su)__a - (__v4su)__b);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("mmx,sse2"), __min_vector_width__(64)))
_mm_sub_si64(__m64 __a, __m64 __b)
{
    return (__m64)__builtin_ia32_psubq((__v1di)__a, (__v1di)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sub_epi64(__m128i __a, __m128i __b)
{
    return (__m128i)((__v2du)__a - (__v2du)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_subs_epi8(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_psubsb128((__v16qi)__a, (__v16qi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_subs_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_psubsw128((__v8hi)__a, (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_subs_epu8(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_psubusb128((__v16qi)__a, (__v16qi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_subs_epu16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_psubusw128((__v8hi)__a, (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_and_si128(__m128i __a, __m128i __b)
{
    return (__m128i)((__v2du)__a & (__v2du)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_andnot_si128(__m128i __a, __m128i __b)
{
    return (__m128i)(~(__v2du)__a & (__v2du)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_or_si128(__m128i __a, __m128i __b)
{
    return (__m128i)((__v2du)__a | (__v2du)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_xor_si128(__m128i __a, __m128i __b)
{
    return (__m128i)((__v2du)__a ^ (__v2du)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_slli_epi16(__m128i __a, int __count)
{
    return (__m128i)__builtin_ia32_psllwi128((__v8hi)__a, __count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sll_epi16(__m128i __a, __m128i __count)
{
    return (__m128i)__builtin_ia32_psllw128((__v8hi)__a, (__v8hi)__count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_slli_epi32(__m128i __a, int __count)
{
    return (__m128i)__builtin_ia32_pslldi128((__v4si)__a, __count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sll_epi32(__m128i __a, __m128i __count)
{
    return (__m128i)__builtin_ia32_pslld128((__v4si)__a, (__v4si)__count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_slli_epi64(__m128i __a, int __count)
{
    return __builtin_ia32_psllqi128((__v2di)__a, __count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sll_epi64(__m128i __a, __m128i __count)
{
    return __builtin_ia32_psllq128((__v2di)__a, (__v2di)__count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_srai_epi16(__m128i __a, int __count)
{
    return (__m128i)__builtin_ia32_psrawi128((__v8hi)__a, __count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sra_epi16(__m128i __a, __m128i __count)
{
    return (__m128i)__builtin_ia32_psraw128((__v8hi)__a, (__v8hi)__count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_srai_epi32(__m128i __a, int __count)
{
    return (__m128i)__builtin_ia32_psradi128((__v4si)__a, __count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_sra_epi32(__m128i __a, __m128i __count)
{
    return (__m128i)__builtin_ia32_psrad128((__v4si)__a, (__v4si)__count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_srli_epi16(__m128i __a, int __count)
{
    return (__m128i)__builtin_ia32_psrlwi128((__v8hi)__a, __count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_srl_epi16(__m128i __a, __m128i __count)
{
    return (__m128i)__builtin_ia32_psrlw128((__v8hi)__a, (__v8hi)__count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_srli_epi32(__m128i __a, int __count)
{
    return (__m128i)__builtin_ia32_psrldi128((__v4si)__a, __count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_srl_epi32(__m128i __a, __m128i __count)
{
    return (__m128i)__builtin_ia32_psrld128((__v4si)__a, (__v4si)__count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_srli_epi64(__m128i __a, int __count)
{
    return __builtin_ia32_psrlqi128((__v2di)__a, __count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_srl_epi64(__m128i __a, __m128i __count)
{
    return __builtin_ia32_psrlq128((__v2di)__a, (__v2di)__count);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpeq_epi8(__m128i __a, __m128i __b)
{
    return (__m128i)((__v16qi)__a == (__v16qi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpeq_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)((__v8hi)__a == (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpeq_epi32(__m128i __a, __m128i __b)
{
    return (__m128i)((__v4si)__a == (__v4si)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpgt_epi8(__m128i __a, __m128i __b)
{

    return (__m128i)((__v16qs)__a > (__v16qs)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpgt_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)((__v8hi)__a > (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmpgt_epi32(__m128i __a, __m128i __b)
{
    return (__m128i)((__v4si)__a > (__v4si)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmplt_epi8(__m128i __a, __m128i __b)
{
    return _mm_cmpgt_epi8(__b, __a);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmplt_epi16(__m128i __a, __m128i __b)
{
    return _mm_cmpgt_epi16(__b, __a);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cmplt_epi32(__m128i __a, __m128i __b)
{
    return _mm_cmpgt_epi32(__b, __a);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtsi64_sd(__m128d __a, long long __b)
{
    __a[0] = __b;
    return __a;
}
static __inline__ long long __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtsd_si64(__m128d __a)
{
    return __builtin_ia32_cvtsd2si64((__v2df)__a);
}
static __inline__ long long __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvttsd_si64(__m128d __a)
{
    return __builtin_ia32_cvttsd2si64((__v2df)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtepi32_ps(__m128i __a)
{
    return (__m128)__builtin_convertvector((__v4si)__a, __v4sf);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtps_epi32(__m128 __a)
{
    return (__m128i)__builtin_ia32_cvtps2dq((__v4sf)__a);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvttps_epi32(__m128 __a)
{
    return (__m128i)__builtin_ia32_cvttps2dq((__v4sf)__a);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtsi32_si128(int __a)
{
    return __extension__(__m128i)(__v4si) { __a, 0, 0, 0 };
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtsi64_si128(long long __a)
{
    return __extension__(__m128i)(__v2di) { __a, 0 };
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtsi128_si32(__m128i __a)
{
    __v4si __b = (__v4si)__a;
    return __b[0];
}
static __inline__ long long __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_cvtsi128_si64(__m128i __a)
{
    return __a[0];
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_load_si128(__m128i const* __p)
{
    return *__p;
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_loadu_si128(__m128i_u const* __p)
{
    struct __loadu_si128 {
        __m128i_u __v;
    } __attribute__((__packed__, __may_alias__));
    return ((const struct __loadu_si128*)__p)->__v;
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_loadl_epi64(__m128i_u const* __p)
{
    struct __mm_loadl_epi64_struct {
        long long __u;
    } __attribute__((__packed__, __may_alias__));
    return __extension__(__m128i) { ((const struct __mm_loadl_epi64_struct*)__p)->__u, 0 };
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_undefined_si128(void)
{
    return (__m128i)__builtin_ia32_undef128();
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set_epi64x(long long __q1, long long __q0)
{
    return __extension__(__m128i)(__v2di) { __q0, __q1 };
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set_epi64(__m64 __q1, __m64 __q0)
{
    return _mm_set_epi64x((long long)__q1, (long long)__q0);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set_epi32(int __i3, int __i2, int __i1, int __i0)
{
    return __extension__(__m128i)(__v4si) { __i0, __i1, __i2, __i3 };
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set_epi16(short __w7, short __w6, short __w5, short __w4, short __w3, short __w2, short __w1, short __w0)
{
    return __extension__(__m128i)(__v8hi) { __w0, __w1, __w2, __w3, __w4, __w5, __w6, __w7 };
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set_epi8(char __b15, char __b14, char __b13, char __b12, char __b11, char __b10, char __b9, char __b8, char __b7, char __b6, char __b5, char __b4, char __b3, char __b2, char __b1, char __b0)
{
    return __extension__(__m128i)(__v16qi) { __b0, __b1, __b2, __b3, __b4, __b5, __b6, __b7, __b8, __b9, __b10, __b11, __b12, __b13, __b14, __b15 };
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set1_epi64x(long long __q)
{
    return _mm_set_epi64x(__q, __q);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set1_epi64(__m64 __q)
{
    return _mm_set_epi64(__q, __q);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set1_epi32(int __i)
{
    return _mm_set_epi32(__i, __i, __i, __i);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set1_epi16(short __w)
{
    return _mm_set_epi16(__w, __w, __w, __w, __w, __w, __w, __w);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_set1_epi8(char __b)
{
    return _mm_set_epi8(__b, __b, __b, __b, __b, __b, __b, __b, __b, __b, __b, __b, __b, __b, __b, __b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_setr_epi64(__m64 __q0, __m64 __q1)
{
    return _mm_set_epi64(__q1, __q0);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_setr_epi32(int __i0, int __i1, int __i2, int __i3)
{
    return _mm_set_epi32(__i3, __i2, __i1, __i0);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_setr_epi16(short __w0, short __w1, short __w2, short __w3, short __w4, short __w5, short __w6, short __w7)
{
    return _mm_set_epi16(__w7, __w6, __w5, __w4, __w3, __w2, __w1, __w0);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_setr_epi8(char __b0, char __b1, char __b2, char __b3, char __b4, char __b5, char __b6, char __b7, char __b8, char __b9, char __b10, char __b11, char __b12, char __b13, char __b14, char __b15)
{
    return _mm_set_epi8(__b15, __b14, __b13, __b12, __b11, __b10, __b9, __b8, __b7, __b6, __b5, __b4, __b3, __b2, __b1, __b0);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_setzero_si128(void)
{
    return __extension__(__m128i)(__v2di) { 0LL, 0LL };
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_store_si128(__m128i* __p, __m128i __b)
{
    *__p = __b;
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_storeu_si128(__m128i_u* __p, __m128i __b)
{
    struct __storeu_si128 {
        __m128i_u __v;
    } __attribute__((__packed__, __may_alias__));
    ((struct __storeu_si128*)__p)->__v = __b;
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_storeu_si64(void* __p, __m128i __b)
{
    struct __storeu_si64 {
        long long __v;
    } __attribute__((__packed__, __may_alias__));
    ((struct __storeu_si64*)__p)->__v = ((__v2di)__b)[0];
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_storeu_si32(void* __p, __m128i __b)
{
    struct __storeu_si32 {
        int __v;
    } __attribute__((__packed__, __may_alias__));
    ((struct __storeu_si32*)__p)->__v = ((__v4si)__b)[0];
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_storeu_si16(void* __p, __m128i __b)
{
    struct __storeu_si16 {
        short __v;
    } __attribute__((__packed__, __may_alias__));
    ((struct __storeu_si16*)__p)->__v = ((__v8hi)__b)[0];
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_maskmoveu_si128(__m128i __d, __m128i __n, char* __p)
{
    __builtin_ia32_maskmovdqu((__v16qi)__d, (__v16qi)__n, __p);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_storel_epi64(__m128i_u* __p, __m128i __a)
{
    struct __mm_storel_epi64_struct {
        long long __u;
    } __attribute__((__packed__, __may_alias__));
    ((struct __mm_storel_epi64_struct*)__p)->__u = __a[0];
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_stream_pd(double* __p, __m128d __a)
{
    __builtin_nontemporal_store((__v2df)__a, (__v2df*)__p);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_stream_si128(__m128i* __p, __m128i __a)
{
    __builtin_nontemporal_store((__v2di)__a, (__v2di*)__p);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2")))
_mm_stream_si32(int* __p, int __a)
{
    __builtin_ia32_movnti(__p, __a);
}
static __inline__ void __attribute__((__always_inline__, __nodebug__, __target__("sse2")))
_mm_stream_si64(long long* __p, long long __a)
{
    __builtin_ia32_movnti64(__p, __a);
}
void _mm_clflush(void const* __p);
void _mm_lfence(void);
void _mm_mfence(void);
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_packs_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_packsswb128((__v8hi)__a, (__v8hi)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_packs_epi32(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_packssdw128((__v4si)__a, (__v4si)__b);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_packus_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_ia32_packuswb128((__v8hi)__a, (__v8hi)__b);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_movemask_epi8(__m128i __a)
{
    return __builtin_ia32_pmovmskb128((__v16qi)__a);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_unpackhi_epi8(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_shufflevector((__v16qi)__a, (__v16qi)__b, 8, 16 + 8, 9, 16 + 9, 10, 16 + 10, 11, 16 + 11, 12, 16 + 12, 13, 16 + 13, 14, 16 + 14, 15, 16 + 15);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_unpackhi_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_shufflevector((__v8hi)__a, (__v8hi)__b, 4, 8 + 4, 5, 8 + 5, 6, 8 + 6, 7, 8 + 7);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_unpackhi_epi32(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_shufflevector((__v4si)__a, (__v4si)__b, 2, 4 + 2, 3, 4 + 3);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_unpackhi_epi64(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_shufflevector((__v2di)__a, (__v2di)__b, 1, 2 + 1);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_unpacklo_epi8(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_shufflevector((__v16qi)__a, (__v16qi)__b, 0, 16 + 0, 1, 16 + 1, 2, 16 + 2, 3, 16 + 3, 4, 16 + 4, 5, 16 + 5, 6, 16 + 6, 7, 16 + 7);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_unpacklo_epi16(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_shufflevector((__v8hi)__a, (__v8hi)__b, 0, 8 + 0, 1, 8 + 1, 2, 8 + 2, 3, 8 + 3);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_unpacklo_epi32(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_shufflevector((__v4si)__a, (__v4si)__b, 0, 4 + 0, 1, 4 + 1);
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_unpacklo_epi64(__m128i __a, __m128i __b)
{
    return (__m128i)__builtin_shufflevector((__v2di)__a, (__v2di)__b, 0, 2 + 0);
}
static __inline__ __m64 __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_movepi64_pi64(__m128i __a)
{
    return (__m64)__a[0];
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_movpi64_epi64(__m64 __a)
{
    return __extension__(__m128i)(__v2di) { (long long)__a, 0 };
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_move_epi64(__m128i __a)
{
    return __builtin_shufflevector((__v2di)__a, _mm_setzero_si128(), 0, 2);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_unpackhi_pd(__m128d __a, __m128d __b)
{
    return __builtin_shufflevector((__v2df)__a, (__v2df)__b, 1, 2 + 1);
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_unpacklo_pd(__m128d __a, __m128d __b)
{
    return __builtin_shufflevector((__v2df)__a, (__v2df)__b, 0, 2 + 0);
}
static __inline__ int __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_movemask_pd(__m128d __a)
{
    return __builtin_ia32_movmskpd((__v2df)__a);
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_castpd_ps(__m128d __a)
{
    return (__m128)__a;
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_castpd_si128(__m128d __a)
{
    return (__m128i)__a;
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_castps_pd(__m128 __a)
{
    return (__m128d)__a;
}
static __inline__ __m128i __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_castps_si128(__m128 __a)
{
    return (__m128i)__a;
}
static __inline__ __m128 __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_castsi128_ps(__m128i __a)
{
    return (__m128)__a;
}
static __inline__ __m128d __attribute__((__always_inline__, __nodebug__, __target__("sse2"), __min_vector_width__(128)))
_mm_castsi128_pd(__m128i __a)
{
    return (__m128d)__a;
}
void _mm_pause(void);
static int stbi__sse2_available(void)
{

    return 1;
}
typedef struct
{
    stbi__uint32 img_x, img_y;
    int img_n, img_out_n;

    stbi_io_callbacks io;
    void* io_user_data;

    int read_from_callbacks;
    int buflen;
    stbi_uc buffer_start[128];

    stbi_uc* img_buffer, * img_buffer_end;
    stbi_uc* img_buffer_original, * img_buffer_original_end;
} stbi__context;

static void stbi__refill_buffer(stbi__context* s);

static void stbi__start_mem(stbi__context* s, stbi_uc const* buffer, int len)
{
    s->io.read = ((void*)0);
    s->read_from_callbacks = 0;
    s->img_buffer = s->img_buffer_original = (stbi_uc*)buffer;
    s->img_buffer_end = s->img_buffer_original_end = (stbi_uc*)buffer + len;
}

static void stbi__start_callbacks(stbi__context* s, stbi_io_callbacks* c, void* user)
{
    s->io = *c;
    s->io_user_data = user;
    s->buflen = sizeof(s->buffer_start);
    s->read_from_callbacks = 1;
    s->img_buffer_original = s->buffer_start;
    stbi__refill_buffer(s);
    s->img_buffer_original_end = s->img_buffer_end;
}

static int stbi__stdio_read(void* user, char* data, int size)
{
    return (int)fread(data, 1, size, (FILE*)user);
}

static void stbi__stdio_skip(void* user, int n)
{
    fseek((FILE*)user, n, 1);
}

static int stbi__stdio_eof(void* user)
{
    return feof((FILE*)user);
}

static stbi_io_callbacks stbi__stdio_callbacks =
{
   stbi__stdio_read,
   stbi__stdio_skip,
   stbi__stdio_eof,
};

static void stbi__start_file(stbi__context* s, FILE* f)
{
    stbi__start_callbacks(s, &stbi__stdio_callbacks, (void*)f);
}

static void stbi__rewind(stbi__context* s)
{

    s->img_buffer = s->img_buffer_original;
    s->img_buffer_end = s->img_buffer_original_end;
}

enum
{
    STBI_ORDER_RGB,
    STBI_ORDER_BGR
};

typedef struct
{
    int bits_per_channel;
    int num_channels;
    int channel_order;
} stbi__result_info;

static int stbi__jpeg_test(stbi__context* s);
static void* stbi__jpeg_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri);
static int stbi__jpeg_info(stbi__context* s, int* x, int* y, int* comp);

static int stbi__png_test(stbi__context* s);
static void* stbi__png_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri);
static int stbi__png_info(stbi__context* s, int* x, int* y, int* comp);
static int stbi__png_is16(stbi__context* s);

static int stbi__bmp_test(stbi__context* s);
static void* stbi__bmp_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri);
static int stbi__bmp_info(stbi__context* s, int* x, int* y, int* comp);

static int stbi__tga_test(stbi__context* s);
static void* stbi__tga_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri);
static int stbi__tga_info(stbi__context* s, int* x, int* y, int* comp);

static int stbi__psd_test(stbi__context* s);
static void* stbi__psd_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri, int bpc);
static int stbi__psd_info(stbi__context* s, int* x, int* y, int* comp);
static int stbi__psd_is16(stbi__context* s);

static int stbi__hdr_test(stbi__context* s);
static float* stbi__hdr_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri);
static int stbi__hdr_info(stbi__context* s, int* x, int* y, int* comp);

static int stbi__pic_test(stbi__context* s);
static void* stbi__pic_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri);
static int stbi__pic_info(stbi__context* s, int* x, int* y, int* comp);

static int stbi__gif_test(stbi__context* s);
static void* stbi__gif_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri);
static void* stbi__load_gif_main(stbi__context* s, int** delays, int* x, int* y, int* z, int* comp, int req_comp);
static int stbi__gif_info(stbi__context* s, int* x, int* y, int* comp);

static int stbi__pnm_test(stbi__context* s);
static void* stbi__pnm_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri);
static int stbi__pnm_info(stbi__context* s, int* x, int* y, int* comp);

static

_Thread_local

const char* stbi__g_failure_reason;

extern const char* stbi_failure_reason(void)
{
    return stbi__g_failure_reason;
}

static int stbi__err(const char* str)
{
    stbi__g_failure_reason = str;
    return 0;
}

static void* stbi__malloc(size_t size)
{
    return malloc(size);
}
static int stbi__addsizes_valid(int a, int b)
{
    if (b < 0) return 0;

    return a <= 2147483647 - b;
}

static int stbi__mul2sizes_valid(int a, int b)
{
    if (a < 0 || b < 0) return 0;
    if (b == 0) return 1;

    return a <= 2147483647 / b;
}

static int stbi__mad2sizes_valid(int a, int b, int add)
{
    return stbi__mul2sizes_valid(a, b) && stbi__addsizes_valid(a * b, add);
}

static int stbi__mad3sizes_valid(int a, int b, int c, int add)
{
    return stbi__mul2sizes_valid(a, b) && stbi__mul2sizes_valid(a * b, c) &&
        stbi__addsizes_valid(a * b * c, add);
}

static int stbi__mad4sizes_valid(int a, int b, int c, int d, int add)
{
    return stbi__mul2sizes_valid(a, b) && stbi__mul2sizes_valid(a * b, c) &&
        stbi__mul2sizes_valid(a * b * c, d) && stbi__addsizes_valid(a * b * c * d, add);
}

static void* stbi__malloc_mad2(int a, int b, int add)
{
    if (!stbi__mad2sizes_valid(a, b, add)) return ((void*)0);
    return stbi__malloc(a * b + add);
}

static void* stbi__malloc_mad3(int a, int b, int c, int add)
{
    if (!stbi__mad3sizes_valid(a, b, c, add)) return ((void*)0);
    return stbi__malloc(a * b * c + add);
}

static void* stbi__malloc_mad4(int a, int b, int c, int d, int add)
{
    if (!stbi__mad4sizes_valid(a, b, c, d, add)) return ((void*)0);
    return stbi__malloc(a * b * c * d + add);
}
extern void stbi_image_free(void* retval_from_stbi_load)
{
    free(retval_from_stbi_load);
}

static float* stbi__ldr_to_hdr(stbi_uc* data, int x, int y, int comp);

static stbi_uc* stbi__hdr_to_ldr(float* data, int x, int y, int comp);

static int stbi__vertically_flip_on_load_global = 0;

extern void stbi_set_flip_vertically_on_load(int flag_true_if_should_flip)
{
    stbi__vertically_flip_on_load_global = flag_true_if_should_flip;
}

static _Thread_local int stbi__vertically_flip_on_load_local, stbi__vertically_flip_on_load_set;

extern void stbi_set_flip_vertically_on_load_thread(int flag_true_if_should_flip)
{
    stbi__vertically_flip_on_load_local = flag_true_if_should_flip;
    stbi__vertically_flip_on_load_set = 1;
}

static void* stbi__load_main(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri, int bpc)
{
    memset(ri, 0, sizeof(*ri));
    ri->bits_per_channel = 8;
    ri->channel_order = STBI_ORDER_RGB;
    ri->num_channels = 0;

    if (stbi__jpeg_test(s)) return stbi__jpeg_load(s, x, y, comp, req_comp, ri);

    if (stbi__png_test(s)) return stbi__png_load(s, x, y, comp, req_comp, ri);

    if (stbi__bmp_test(s)) return stbi__bmp_load(s, x, y, comp, req_comp, ri);

    if (stbi__gif_test(s)) return stbi__gif_load(s, x, y, comp, req_comp, ri);

    if (stbi__psd_test(s)) return stbi__psd_load(s, x, y, comp, req_comp, ri, bpc);

    if (stbi__pic_test(s)) return stbi__pic_load(s, x, y, comp, req_comp, ri);

    if (stbi__pnm_test(s)) return stbi__pnm_load(s, x, y, comp, req_comp, ri);

    if (stbi__hdr_test(s)) {
        float* hdr = stbi__hdr_load(s, x, y, comp, req_comp, ri);
        return stbi__hdr_to_ldr(hdr, *x, *y, req_comp ? req_comp : *comp);
    }

    if (stbi__tga_test(s))
        return stbi__tga_load(s, x, y, comp, req_comp, ri);

    return ((unsigned char*)(size_t)(stbi__err("unknown image type") ? ((void*)0) : ((void*)0)));
}

static stbi_uc* stbi__convert_16_to_8(stbi__uint16* orig, int w, int h, int channels)
{
    int i;
    int img_len = w * h * channels;
    stbi_uc* reduced;

    reduced = (stbi_uc*)stbi__malloc(img_len);
    if (reduced == ((void*)0)) return ((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));

    for (i = 0; i < img_len; ++i)
        reduced[i] = (stbi_uc)((orig[i] >> 8) & 0xFF);

    free(orig);
    return reduced;
}

static stbi__uint16* stbi__convert_8_to_16(stbi_uc* orig, int w, int h, int channels)
{
    int i;
    int img_len = w * h * channels;
    stbi__uint16* enlarged;

    enlarged = (stbi__uint16*)stbi__malloc(img_len * 2);
    if (enlarged == ((void*)0)) return (stbi__uint16*)((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));

    for (i = 0; i < img_len; ++i)
        enlarged[i] = (stbi__uint16)((orig[i] << 8) + orig[i]);

    free(orig);
    return enlarged;
}

static void stbi__vertical_flip(void* image, int w, int h, int bytes_per_pixel)
{
    int row;
    size_t bytes_per_row = (size_t)w * bytes_per_pixel;
    stbi_uc temp[2048];
    stbi_uc* bytes = (stbi_uc*)image;

    for (row = 0; row < (h >> 1); row++) {
        stbi_uc* row0 = bytes + row * bytes_per_row;
        stbi_uc* row1 = bytes + (h - row - 1) * bytes_per_row;

        size_t bytes_left = bytes_per_row;
        while (bytes_left) {
            size_t bytes_copy = (bytes_left < sizeof(temp)) ? bytes_left : sizeof(temp);
            memcpy(temp, row0, bytes_copy);
            memcpy(row0, row1, bytes_copy);
            memcpy(row1, temp, bytes_copy);
            row0 += bytes_copy;
            row1 += bytes_copy;
            bytes_left -= bytes_copy;
        }
    }
}

static void stbi__vertical_flip_slices(void* image, int w, int h, int z, int bytes_per_pixel)
{
    int slice;
    int slice_size = w * h * bytes_per_pixel;

    stbi_uc* bytes = (stbi_uc*)image;
    for (slice = 0; slice < z; ++slice) {
        stbi__vertical_flip(bytes, w, h, bytes_per_pixel);
        bytes += slice_size;
    }
}

static unsigned char* stbi__load_and_postprocess_8bit(stbi__context* s, int* x, int* y, int* comp, int req_comp)
{
    stbi__result_info ri;
    void* result = stbi__load_main(s, x, y, comp, req_comp, &ri, 8);

    if (result == ((void*)0))
        return ((void*)0);

    if (ri.bits_per_channel != 8) {
        ((void) sizeof((ri.bits_per_channel == 16) ? 1 : 0), __extension__({ if (ri.bits_per_channel == 16); else __assert_fail("ri.bits_per_channel == 16", "./example.c", 1176, __extension__ __PRETTY_FUNCTION__); }));
        result = stbi__convert_16_to_8((stbi__uint16*)result, *x, *y, req_comp == 0 ? *comp : req_comp);
        ri.bits_per_channel = 8;
    }

    if ((stbi__vertically_flip_on_load_set ? stbi__vertically_flip_on_load_local : stbi__vertically_flip_on_load_global)) {
        int channels = req_comp ? req_comp : *comp;
        stbi__vertical_flip(result, *x, *y, channels * sizeof(stbi_uc));
    }

    return (unsigned char*)result;
}

static stbi__uint16* stbi__load_and_postprocess_16bit(stbi__context* s, int* x, int* y, int* comp, int req_comp)
{
    stbi__result_info ri;
    void* result = stbi__load_main(s, x, y, comp, req_comp, &ri, 16);

    if (result == ((void*)0))
        return ((void*)0);

    if (ri.bits_per_channel != 16) {
        ((void) sizeof((ri.bits_per_channel == 8) ? 1 : 0), __extension__({ if (ri.bits_per_channel == 8); else __assert_fail("ri.bits_per_channel == 8", "./example.c", 1200, __extension__ __PRETTY_FUNCTION__); }));
        result = stbi__convert_8_to_16((stbi_uc*)result, *x, *y, req_comp == 0 ? *comp : req_comp);
        ri.bits_per_channel = 16;
    }

    if ((stbi__vertically_flip_on_load_set ? stbi__vertically_flip_on_load_local : stbi__vertically_flip_on_load_global)) {
        int channels = req_comp ? req_comp : *comp;
        stbi__vertical_flip(result, *x, *y, channels * sizeof(stbi__uint16));
    }

    return (stbi__uint16*)result;
}

static void stbi__float_postprocess(float* result, int* x, int* y, int* comp, int req_comp)
{
    if ((stbi__vertically_flip_on_load_set ? stbi__vertically_flip_on_load_local : stbi__vertically_flip_on_load_global) && result != ((void*)0)) {
        int channels = req_comp ? req_comp : *comp;
        stbi__vertical_flip(result, *x, *y, channels * sizeof(float));
    }
}
static FILE* stbi__fopen(char const* filename, char const* mode)
{
    FILE* f;
    f = fopen(filename, mode);

    return f;
}

extern stbi_uc* stbi_load(char const* filename, int* x, int* y, int* comp, int req_comp)
{
    FILE* f = stbi__fopen(filename, "rb");
    unsigned char* result;
    if (!f) return ((unsigned char*)(size_t)(stbi__err("can't fopen") ? ((void*)0) : ((void*)0)));
    result = stbi_load_from_file(f, x, y, comp, req_comp);
    fclose(f);
    return result;
}

extern stbi_uc* stbi_load_from_file(FILE* f, int* x, int* y, int* comp, int req_comp)
{
    unsigned char* result;
    stbi__context s;
    stbi__start_file(&s, f);
    result = stbi__load_and_postprocess_8bit(&s, x, y, comp, req_comp);
    if (result) {

        fseek(f, -(int)(s.img_buffer_end - s.img_buffer), 1);
    }
    return result;
}

extern stbi__uint16* stbi_load_from_file_16(FILE* f, int* x, int* y, int* comp, int req_comp)
{
    stbi__uint16* result;
    stbi__context s;
    stbi__start_file(&s, f);
    result = stbi__load_and_postprocess_16bit(&s, x, y, comp, req_comp);
    if (result) {

        fseek(f, -(int)(s.img_buffer_end - s.img_buffer), 1);
    }
    return result;
}

extern stbi_us* stbi_load_16(char const* filename, int* x, int* y, int* comp, int req_comp)
{
    FILE* f = stbi__fopen(filename, "rb");
    stbi__uint16* result;
    if (!f) return (stbi_us*)((unsigned char*)(size_t)(stbi__err("can't fopen") ? ((void*)0) : ((void*)0)));
    result = stbi_load_from_file_16(f, x, y, comp, req_comp);
    fclose(f);
    return result;
}

extern stbi_us* stbi_load_16_from_memory(stbi_uc const* buffer, int len, int* x, int* y, int* channels_in_file, int desired_channels)
{
    stbi__context s;
    stbi__start_mem(&s, buffer, len);
    return stbi__load_and_postprocess_16bit(&s, x, y, channels_in_file, desired_channels);
}

extern stbi_us* stbi_load_16_from_callbacks(stbi_io_callbacks const* clbk, void* user, int* x, int* y, int* channels_in_file, int desired_channels)
{
    stbi__context s;
    stbi__start_callbacks(&s, (stbi_io_callbacks*)clbk, user);
    return stbi__load_and_postprocess_16bit(&s, x, y, channels_in_file, desired_channels);
}

extern stbi_uc* stbi_load_from_memory(stbi_uc const* buffer, int len, int* x, int* y, int* comp, int req_comp)
{
    stbi__context s;
    stbi__start_mem(&s, buffer, len);
    return stbi__load_and_postprocess_8bit(&s, x, y, comp, req_comp);
}

extern stbi_uc* stbi_load_from_callbacks(stbi_io_callbacks const* clbk, void* user, int* x, int* y, int* comp, int req_comp)
{
    stbi__context s;
    stbi__start_callbacks(&s, (stbi_io_callbacks*)clbk, user);
    return stbi__load_and_postprocess_8bit(&s, x, y, comp, req_comp);
}

extern stbi_uc* stbi_load_gif_from_memory(stbi_uc const* buffer, int len, int** delays, int* x, int* y, int* z, int* comp, int req_comp)
{
    unsigned char* result;
    stbi__context s;
    stbi__start_mem(&s, buffer, len);

    result = (unsigned char*)stbi__load_gif_main(&s, delays, x, y, z, comp, req_comp);
    if ((stbi__vertically_flip_on_load_set ? stbi__vertically_flip_on_load_local : stbi__vertically_flip_on_load_global)) {
        stbi__vertical_flip_slices(result, *x, *y, *z, *comp);
    }

    return result;
}

static float* stbi__loadf_main(stbi__context* s, int* x, int* y, int* comp, int req_comp)
{
    unsigned char* data;

    if (stbi__hdr_test(s)) {
        stbi__result_info ri;
        float* hdr_data = stbi__hdr_load(s, x, y, comp, req_comp, &ri);
        if (hdr_data)
            stbi__float_postprocess(hdr_data, x, y, comp, req_comp);
        return hdr_data;
    }

    data = stbi__load_and_postprocess_8bit(s, x, y, comp, req_comp);
    if (data)
        return stbi__ldr_to_hdr(data, *x, *y, req_comp ? req_comp : *comp);
    return ((float*)(size_t)(stbi__err("unknown image type") ? ((void*)0) : ((void*)0)));
}

extern float* stbi_loadf_from_memory(stbi_uc const* buffer, int len, int* x, int* y, int* comp, int req_comp)
{
    stbi__context s;
    stbi__start_mem(&s, buffer, len);
    return stbi__loadf_main(&s, x, y, comp, req_comp);
}

extern float* stbi_loadf_from_callbacks(stbi_io_callbacks const* clbk, void* user, int* x, int* y, int* comp, int req_comp)
{
    stbi__context s;
    stbi__start_callbacks(&s, (stbi_io_callbacks*)clbk, user);
    return stbi__loadf_main(&s, x, y, comp, req_comp);
}

extern float* stbi_loadf(char const* filename, int* x, int* y, int* comp, int req_comp)
{
    float* result;
    FILE* f = stbi__fopen(filename, "rb");
    if (!f) return ((float*)(size_t)(stbi__err("can't fopen") ? ((void*)0) : ((void*)0)));
    result = stbi_loadf_from_file(f, x, y, comp, req_comp);
    fclose(f);
    return result;
}

extern float* stbi_loadf_from_file(FILE* f, int* x, int* y, int* comp, int req_comp)
{
    stbi__context s;
    stbi__start_file(&s, f);
    return stbi__loadf_main(&s, x, y, comp, req_comp);
}
extern int stbi_is_hdr_from_memory(stbi_uc const* buffer, int len)
{

    stbi__context s;
    stbi__start_mem(&s, buffer, len);
    return stbi__hdr_test(&s);

}

extern int stbi_is_hdr(char const* filename)
{
    FILE* f = stbi__fopen(filename, "rb");
    int result = 0;
    if (f) {
        result = stbi_is_hdr_from_file(f);
        fclose(f);
    }
    return result;
}

extern int stbi_is_hdr_from_file(FILE* f)
{

    long pos = ftell(f);
    int res;
    stbi__context s;
    stbi__start_file(&s, f);
    res = stbi__hdr_test(&s);
    fseek(f, pos, 0);
    return res;

}

extern int stbi_is_hdr_from_callbacks(stbi_io_callbacks const* clbk, void* user)
{

    stbi__context s;
    stbi__start_callbacks(&s, (stbi_io_callbacks*)clbk, user);
    return stbi__hdr_test(&s);

}

static float stbi__l2h_gamma = 2.2f, stbi__l2h_scale = 1.0f;

extern void stbi_ldr_to_hdr_gamma(float gamma) { stbi__l2h_gamma = gamma; }
extern void stbi_ldr_to_hdr_scale(float scale) { stbi__l2h_scale = scale; }

static float stbi__h2l_gamma_i = 1.0f / 2.2f, stbi__h2l_scale_i = 1.0f;

extern void stbi_hdr_to_ldr_gamma(float gamma) { stbi__h2l_gamma_i = 1 / gamma; }
extern void stbi_hdr_to_ldr_scale(float scale) { stbi__h2l_scale_i = 1 / scale; }

enum
{
    STBI__SCAN_load = 0,
    STBI__SCAN_type,
    STBI__SCAN_header
};

static void stbi__refill_buffer(stbi__context* s)
{
    int n = (s->io.read)(s->io_user_data, (char*)s->buffer_start, s->buflen);
    if (n == 0) {

        s->read_from_callbacks = 0;
        s->img_buffer = s->buffer_start;
        s->img_buffer_end = s->buffer_start + 1;
        *s->img_buffer = 0;
    }
    else {
        s->img_buffer = s->buffer_start;
        s->img_buffer_end = s->buffer_start + n;
    }
}

static stbi_uc stbi__get8(stbi__context* s)
{
    if (s->img_buffer < s->img_buffer_end)
        return *s->img_buffer++;
    if (s->read_from_callbacks) {
        stbi__refill_buffer(s);
        return *s->img_buffer++;
    }
    return 0;
}

static int stbi__at_eof(stbi__context* s)
{
    if (s->io.read) {
        if (!(s->io.eof)(s->io_user_data)) return 0;

        if (s->read_from_callbacks == 0) return 1;
    }

    return s->img_buffer >= s->img_buffer_end;
}

static void stbi__skip(stbi__context* s, int n)
{
    if (n < 0) {
        s->img_buffer = s->img_buffer_end;
        return;
    }
    if (s->io.read) {
        int blen = (int)(s->img_buffer_end - s->img_buffer);
        if (blen < n) {
            s->img_buffer = s->img_buffer_end;
            (s->io.skip)(s->io_user_data, n - blen);
            return;
        }
    }
    s->img_buffer += n;
}

static int stbi__getn(stbi__context* s, stbi_uc* buffer, int n)
{
    if (s->io.read) {
        int blen = (int)(s->img_buffer_end - s->img_buffer);
        if (blen < n) {
            int res, count;

            memcpy(buffer, s->img_buffer, blen);

            count = (s->io.read)(s->io_user_data, (char*)buffer + blen, n - blen);
            res = (count == (n - blen));
            s->img_buffer = s->img_buffer_end;
            return res;
        }
    }

    if (s->img_buffer + n <= s->img_buffer_end) {
        memcpy(buffer, s->img_buffer, n);
        s->img_buffer += n;
        return 1;
    }
    else
        return 0;
}

static int stbi__get16be(stbi__context* s)
{
    int z = stbi__get8(s);
    return (z << 8) + stbi__get8(s);
}

static stbi__uint32 stbi__get32be(stbi__context* s)
{
    stbi__uint32 z = stbi__get16be(s);
    return (z << 16) + stbi__get16be(s);
}

static int stbi__get16le(stbi__context* s)
{
    int z = stbi__get8(s);
    return z + (stbi__get8(s) << 8);
}

static stbi__uint32 stbi__get32le(stbi__context* s)
{
    stbi__uint32 z = stbi__get16le(s);
    return z + (stbi__get16le(s) << 16);
}
static stbi_uc stbi__compute_y(int r, int g, int b)
{
    return (stbi_uc)(((r * 77) + (g * 150) + (29 * b)) >> 8);
}

static unsigned char* stbi__convert_format(unsigned char* data, int img_n, int req_comp, unsigned int x, unsigned int y)
{
    int i, j;
    unsigned char* good;

    if (req_comp == img_n) return data;
    ((void) sizeof((req_comp >= 1 && req_comp <= 4) ? 1 : 0), __extension__({ if (req_comp >= 1 && req_comp <= 4); else __assert_fail("req_comp >= 1 && req_comp <= 4", "./example.c", 1663, __extension__ __PRETTY_FUNCTION__); }));

    good = (unsigned char*)stbi__malloc_mad3(req_comp, x, y, 0);
    if (good == ((void*)0)) {
        free(data);
        return ((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));
    }

    for (j = 0; j < (int)y; ++j) {
        unsigned char* src = data + j * x * img_n;
        unsigned char* dest = good + j * x * req_comp;

        switch (((img_n) * 8 + (req_comp))) {
        case ((1) * 8 + (2)): for (i = x - 1; i >= 0; --i, src += 1, dest += 2) { dest[0] = src[0]; dest[1] = 255; } break;
        case ((1) * 8 + (3)): for (i = x - 1; i >= 0; --i, src += 1, dest += 3) { dest[0] = dest[1] = dest[2] = src[0]; } break;
        case ((1) * 8 + (4)): for (i = x - 1; i >= 0; --i, src += 1, dest += 4) { dest[0] = dest[1] = dest[2] = src[0]; dest[3] = 255; } break;
        case ((2) * 8 + (1)): for (i = x - 1; i >= 0; --i, src += 2, dest += 1) { dest[0] = src[0]; } break;
        case ((2) * 8 + (3)): for (i = x - 1; i >= 0; --i, src += 2, dest += 3) { dest[0] = dest[1] = dest[2] = src[0]; } break;
        case ((2) * 8 + (4)): for (i = x - 1; i >= 0; --i, src += 2, dest += 4) { dest[0] = dest[1] = dest[2] = src[0]; dest[3] = src[1]; } break;
        case ((3) * 8 + (4)): for (i = x - 1; i >= 0; --i, src += 3, dest += 4) { dest[0] = src[0]; dest[1] = src[1]; dest[2] = src[2]; dest[3] = 255; } break;
        case ((3) * 8 + (1)): for (i = x - 1; i >= 0; --i, src += 3, dest += 1) { dest[0] = stbi__compute_y(src[0], src[1], src[2]); } break;
        case ((3) * 8 + (2)): for (i = x - 1; i >= 0; --i, src += 3, dest += 2) { dest[0] = stbi__compute_y(src[0], src[1], src[2]); dest[1] = 255; } break;
        case ((4) * 8 + (1)): for (i = x - 1; i >= 0; --i, src += 4, dest += 1) { dest[0] = stbi__compute_y(src[0], src[1], src[2]); } break;
        case ((4) * 8 + (2)): for (i = x - 1; i >= 0; --i, src += 4, dest += 2) { dest[0] = stbi__compute_y(src[0], src[1], src[2]); dest[1] = src[3]; } break;
        case ((4) * 8 + (3)): for (i = x - 1; i >= 0; --i, src += 4, dest += 3) { dest[0] = src[0]; dest[1] = src[1]; dest[2] = src[2]; } break;
        default: ((void) sizeof((0) ? 1 : 0), __extension__({ if (0); else __assert_fail("0", "./example.c", 1692, __extension__ __PRETTY_FUNCTION__); }));
        }

    }

    free(data);
    return good;
}

static stbi__uint16 stbi__compute_y_16(int r, int g, int b)
{
    return (stbi__uint16)(((r * 77) + (g * 150) + (29 * b)) >> 8);
}

static stbi__uint16* stbi__convert_format16(stbi__uint16* data, int img_n, int req_comp, unsigned int x, unsigned int y)
{
    int i, j;
    stbi__uint16* good;

    if (req_comp == img_n) return data;
    ((void) sizeof((req_comp >= 1 && req_comp <= 4) ? 1 : 0), __extension__({ if (req_comp >= 1 && req_comp <= 4); else __assert_fail("req_comp >= 1 && req_comp <= 4", "./example.c", 1720, __extension__ __PRETTY_FUNCTION__); }));

    good = (stbi__uint16*)stbi__malloc(req_comp * x * y * 2);
    if (good == ((void*)0)) {
        free(data);
        return (stbi__uint16*)((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));
    }

    for (j = 0; j < (int)y; ++j) {
        stbi__uint16* src = data + j * x * img_n;
        stbi__uint16* dest = good + j * x * req_comp;

        switch (((img_n) * 8 + (req_comp))) {
        case ((1) * 8 + (2)): for (i = x - 1; i >= 0; --i, src += 1, dest += 2) { dest[0] = src[0]; dest[1] = 0xffff; } break;
        case ((1) * 8 + (3)): for (i = x - 1; i >= 0; --i, src += 1, dest += 3) { dest[0] = dest[1] = dest[2] = src[0]; } break;
        case ((1) * 8 + (4)): for (i = x - 1; i >= 0; --i, src += 1, dest += 4) { dest[0] = dest[1] = dest[2] = src[0]; dest[3] = 0xffff; } break;
        case ((2) * 8 + (1)): for (i = x - 1; i >= 0; --i, src += 2, dest += 1) { dest[0] = src[0]; } break;
        case ((2) * 8 + (3)): for (i = x - 1; i >= 0; --i, src += 2, dest += 3) { dest[0] = dest[1] = dest[2] = src[0]; } break;
        case ((2) * 8 + (4)): for (i = x - 1; i >= 0; --i, src += 2, dest += 4) { dest[0] = dest[1] = dest[2] = src[0]; dest[3] = src[1]; } break;
        case ((3) * 8 + (4)): for (i = x - 1; i >= 0; --i, src += 3, dest += 4) { dest[0] = src[0]; dest[1] = src[1]; dest[2] = src[2]; dest[3] = 0xffff; } break;
        case ((3) * 8 + (1)): for (i = x - 1; i >= 0; --i, src += 3, dest += 1) { dest[0] = stbi__compute_y_16(src[0], src[1], src[2]); } break;
        case ((3) * 8 + (2)): for (i = x - 1; i >= 0; --i, src += 3, dest += 2) { dest[0] = stbi__compute_y_16(src[0], src[1], src[2]); dest[1] = 0xffff; } break;
        case ((4) * 8 + (1)): for (i = x - 1; i >= 0; --i, src += 4, dest += 1) { dest[0] = stbi__compute_y_16(src[0], src[1], src[2]); } break;
        case ((4) * 8 + (2)): for (i = x - 1; i >= 0; --i, src += 4, dest += 2) { dest[0] = stbi__compute_y_16(src[0], src[1], src[2]); dest[1] = src[3]; } break;
        case ((4) * 8 + (3)): for (i = x - 1; i >= 0; --i, src += 4, dest += 3) { dest[0] = src[0]; dest[1] = src[1]; dest[2] = src[2]; } break;
        default: ((void) sizeof((0) ? 1 : 0), __extension__({ if (0); else __assert_fail("0", "./example.c", 1749, __extension__ __PRETTY_FUNCTION__); }));
        }

    }

    free(data);
    return good;
}

static float* stbi__ldr_to_hdr(stbi_uc* data, int x, int y, int comp)
{
    int i, k, n;
    float* output;
    if (!data) return ((void*)0);
    output = (float*)stbi__malloc_mad4(x, y, comp, sizeof(float), 0);
    if (output == ((void*)0)) { free(data); return ((float*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0))); }

    if (comp & 1) n = comp; else n = comp - 1;
    for (i = 0; i < x * y; ++i) {
        for (k = 0; k < n; ++k) {
            output[i * comp + k] = (float)(pow(data[i * comp + k] / 255.0f, stbi__l2h_gamma) * stbi__l2h_scale);
        }
    }
    if (n < comp) {
        for (i = 0; i < x * y; ++i) {
            output[i * comp + n] = data[i * comp + n] / 255.0f;
        }
    }
    free(data);
    return output;
}

static stbi_uc* stbi__hdr_to_ldr(float* data, int x, int y, int comp)
{
    int i, k, n;
    stbi_uc* output;
    if (!data) return ((void*)0);
    output = (stbi_uc*)stbi__malloc_mad3(x, y, comp, 0);
    if (output == ((void*)0)) { free(data); return ((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0))); }

    if (comp & 1) n = comp; else n = comp - 1;
    for (i = 0; i < x * y; ++i) {
        for (k = 0; k < n; ++k) {
            float z = (float)pow(data[i * comp + k] * stbi__h2l_scale_i, stbi__h2l_gamma_i) * 255 + 0.5f;
            if (z < 0) z = 0;
            if (z > 255) z = 255;
            output[i * comp + k] = (stbi_uc)((int)(z));
        }
        if (k < comp) {
            float z = data[i * comp + k] * 255 + 0.5f;
            if (z < 0) z = 0;
            if (z > 255) z = 255;
            output[i * comp + k] = (stbi_uc)((int)(z));
        }
    }
    free(data);
    return output;
}
typedef struct
{
    stbi_uc fast[1 << 9];

    stbi__uint16 code[256];
    stbi_uc values[256];
    stbi_uc size[257];
    unsigned int maxcode[18];
    int delta[17];
} stbi__huffman;

typedef struct
{
    stbi__context* s;
    stbi__huffman huff_dc[4];
    stbi__huffman huff_ac[4];
    stbi__uint16 dequant[4][64];
    stbi__int16 fast_ac[4][1 << 9];

    int img_h_max, img_v_max;
    int img_mcu_x, img_mcu_y;
    int img_mcu_w, img_mcu_h;

    struct
    {
        int id;
        int h, v;
        int tq;
        int hd, ha;
        int dc_pred;

        int x, y, w2, h2;
        stbi_uc* data;
        void* raw_data, * raw_coeff;
        stbi_uc* linebuf;
        short* coeff;
        int coeff_w, coeff_h;
    } img_comp[4];

    stbi__uint32 code_buffer;
    int code_bits;
    unsigned char marker;
    int nomore;

    int progressive;
    int spec_start;
    int spec_end;
    int succ_high;
    int succ_low;
    int eob_run;
    int jfif;
    int app14_color_transform;
    int rgb;

    int scan_n, order[4];
    int restart_interval, todo;

    void (*idct_block_kernel)(stbi_uc* out, int out_stride, short data[64]);
    void (*YCbCr_to_RGB_kernel)(stbi_uc* out, const stbi_uc* y, const stbi_uc* pcb, const stbi_uc* pcr, int count, int step);
    stbi_uc* (*resample_row_hv_2_kernel)(stbi_uc* out, stbi_uc* in_near, stbi_uc* in_far, int w, int hs);
} stbi__jpeg;

static int stbi__build_huffman(stbi__huffman* h, int* count)
{
    int i, j, k = 0;
    unsigned int code;

    for (i = 0; i < 16; ++i)
        for (j = 0; j < count[i]; ++j)
            h->size[k++] = (stbi_uc)(i + 1);
    h->size[k] = 0;

    code = 0;
    k = 0;
    for (j = 1; j <= 16; ++j) {

        h->delta[j] = k - code;
        if (h->size[k] == j) {
            while (h->size[k] == j)
                h->code[k++] = (stbi__uint16)(code++);
            if (code - 1 >= (1u << j)) return stbi__err("bad code lengths");
        }

        h->maxcode[j] = code << (16 - j);
        code <<= 1;
    }
    h->maxcode[j] = 0xffffffff;

    memset(h->fast, 255, 1 << 9);
    for (i = 0; i < k; ++i) {
        int s = h->size[i];
        if (s <= 9) {
            int c = h->code[i] << (9 - s);
            int m = 1 << (9 - s);
            for (j = 0; j < m; ++j) {
                h->fast[c + j] = (stbi_uc)i;
            }
        }
    }
    return 1;
}

static void stbi__build_fast_ac(stbi__int16* fast_ac, stbi__huffman* h)
{
    int i;
    for (i = 0; i < (1 << 9); ++i) {
        stbi_uc fast = h->fast[i];
        fast_ac[i] = 0;
        if (fast < 255) {
            int rs = h->values[fast];
            int run = (rs >> 4) & 15;
            int magbits = rs & 15;
            int len = h->size[fast];

            if (magbits && len + magbits <= 9) {

                int k = ((i << len) & ((1 << 9) - 1)) >> (9 - magbits);
                int m = 1 << (magbits - 1);
                if (k < m) k += (~0U << magbits) + 1;

                if (k >= -128 && k <= 127)
                    fast_ac[i] = (stbi__int16)((k * 256) + (run * 16) + (len + magbits));
            }
        }
    }
}

static void stbi__grow_buffer_unsafe(stbi__jpeg* j)
{
    do {
        unsigned int b = j->nomore ? 0 : stbi__get8(j->s);
        if (b == 0xff) {
            int c = stbi__get8(j->s);
            while (c == 0xff) c = stbi__get8(j->s);
            if (c != 0) {
                j->marker = (unsigned char)c;
                j->nomore = 1;
                return;
            }
        }
        j->code_buffer |= b << (24 - j->code_bits);
        j->code_bits += 8;
    } while (j->code_bits <= 24);
}

static const stbi__uint32 stbi__bmask[17] = { 0,1,3,7,15,31,63,127,255,511,1023,2047,4095,8191,16383,32767,65535 };

static int stbi__jpeg_huff_decode(stbi__jpeg* j, stbi__huffman* h)
{
    unsigned int temp;
    int c, k;

    if (j->code_bits < 16) stbi__grow_buffer_unsafe(j);

    c = (j->code_buffer >> (32 - 9))& ((1 << 9) - 1);
    k = h->fast[c];
    if (k < 255) {
        int s = h->size[k];
        if (s > j->code_bits)
            return -1;
        j->code_buffer <<= s;
        j->code_bits -= s;
        return h->values[k];
    }

    temp = j->code_buffer >> 16;
    for (k = 9 + 1; ; ++k)
        if (temp < h->maxcode[k])
            break;
    if (k == 17) {

        j->code_bits -= 16;
        return -1;
    }

    if (k > j->code_bits)
        return -1;

    c = ((j->code_buffer >> (32 - k))& stbi__bmask[k]) + h->delta[k];
    ((void) sizeof(((((j->code_buffer) >> (32 - h->size[c]))& stbi__bmask[h->size[c]]) == h->code[c]) ? 1 : 0), __extension__({ if ((((j->code_buffer) >> (32 - h->size[c]))& stbi__bmask[h->size[c]]) == h->code[c]); else __assert_fail("(((j->code_buffer) >> (32 - h->size[c]))& stbi__bmask[h->size[c]]) == h->code[c]", "./example.c", 2037, __extension__ __PRETTY_FUNCTION__); }));

    j->code_bits -= k;
    j->code_buffer <<= k;
    return h->values[c];
}

static const int stbi__jbias[16] = { 0,-1,-3,-7,-15,-31,-63,-127,-255,-511,-1023,-2047,-4095,-8191,-16383,-32767 };

static int stbi__extend_receive(stbi__jpeg* j, int n)
{
    unsigned int k;
    int sgn;
    if (j->code_bits < n) stbi__grow_buffer_unsafe(j);

    sgn = (stbi__int32)j->code_buffer >> 31;
    k = (((j->code_buffer) << (n)) | ((j->code_buffer) >> (32 - (n))));
    ((void) sizeof((n >= 0 && n < (int)(sizeof(stbi__bmask) / sizeof(*stbi__bmask))) ? 1 : 0), __extension__({ if (n >= 0 && n < (int)(sizeof(stbi__bmask) / sizeof(*stbi__bmask))); else __assert_fail("n >= 0 && n < (int)(sizeof(stbi__bmask) / sizeof(*stbi__bmask))", "./example.c", 2058, __extension__ __PRETTY_FUNCTION__); }));
    j->code_buffer = k & ~stbi__bmask[n];
    k &= stbi__bmask[n];
    j->code_bits -= n;
    return k + (stbi__jbias[n] & ~sgn);
}

static int stbi__jpeg_get_bits(stbi__jpeg* j, int n)
{
    unsigned int k;
    if (j->code_bits < n) stbi__grow_buffer_unsafe(j);
    k = (((j->code_buffer) << (n)) | ((j->code_buffer) >> (32 - (n))));
    j->code_buffer = k & ~stbi__bmask[n];
    k &= stbi__bmask[n];
    j->code_bits -= n;
    return k;
}

static int stbi__jpeg_get_bit(stbi__jpeg* j)
{
    unsigned int k;
    if (j->code_bits < 1) stbi__grow_buffer_unsafe(j);
    k = j->code_buffer;
    j->code_buffer <<= 1;
    --j->code_bits;
    return k & 0x80000000;
}

static const stbi_uc stbi__jpeg_dezigzag[64 + 15] =
{
    0, 1, 8, 16, 9, 2, 3, 10,
   17, 24, 32, 25, 18, 11, 4, 5,
   12, 19, 26, 33, 40, 48, 41, 34,
   27, 20, 13, 6, 7, 14, 21, 28,
   35, 42, 49, 56, 57, 50, 43, 36,
   29, 22, 15, 23, 30, 37, 44, 51,
   58, 59, 52, 45, 38, 31, 39, 46,
   53, 60, 61, 54, 47, 55, 62, 63,

   63, 63, 63, 63, 63, 63, 63, 63,
   63, 63, 63, 63, 63, 63, 63
};

static int stbi__jpeg_decode_block(stbi__jpeg* j, short data[64], stbi__huffman* hdc, stbi__huffman* hac, stbi__int16* fac, int b, stbi__uint16* dequant)
{
    int diff, dc, k;
    int t;

    if (j->code_bits < 16) stbi__grow_buffer_unsafe(j);
    t = stbi__jpeg_huff_decode(j, hdc);
    if (t < 0) return stbi__err("bad huffman code");

    memset(data, 0, 64 * sizeof(data[0]));

    diff = t ? stbi__extend_receive(j, t) : 0;
    dc = j->img_comp[b].dc_pred + diff;
    j->img_comp[b].dc_pred = dc;
    data[0] = (short)(dc * dequant[0]);

    k = 1;
    do {
        unsigned int zig;
        int c, r, s;
        if (j->code_bits < 16) stbi__grow_buffer_unsafe(j);
        c = (j->code_buffer >> (32 - 9))& ((1 << 9) - 1);
        r = fac[c];
        if (r) {
            k += (r >> 4) & 15;
            s = r & 15;
            j->code_buffer <<= s;
            j->code_bits -= s;

            zig = stbi__jpeg_dezigzag[k++];
            data[zig] = (short)((r >> 8)* dequant[zig]);
        }
        else {
            int rs = stbi__jpeg_huff_decode(j, hac);
            if (rs < 0) return stbi__err("bad huffman code");
            s = rs & 15;
            r = rs >> 4;
            if (s == 0) {
                if (rs != 0xf0) break;
                k += 16;
            }
            else {
                k += r;

                zig = stbi__jpeg_dezigzag[k++];
                data[zig] = (short)(stbi__extend_receive(j, s) * dequant[zig]);
            }
        }
    } while (k < 64);
    return 1;
}

static int stbi__jpeg_decode_block_prog_dc(stbi__jpeg* j, short data[64], stbi__huffman* hdc, int b)
{
    int diff, dc;
    int t;
    if (j->spec_end != 0) return stbi__err("can't merge dc and ac");

    if (j->code_bits < 16) stbi__grow_buffer_unsafe(j);

    if (j->succ_high == 0) {

        memset(data, 0, 64 * sizeof(data[0]));
        t = stbi__jpeg_huff_decode(j, hdc);
        diff = t ? stbi__extend_receive(j, t) : 0;

        dc = j->img_comp[b].dc_pred + diff;
        j->img_comp[b].dc_pred = dc;
        data[0] = (short)(dc << j->succ_low);
    }
    else {

        if (stbi__jpeg_get_bit(j))
            data[0] += (short)(1 << j->succ_low);
    }
    return 1;
}

static int stbi__jpeg_decode_block_prog_ac(stbi__jpeg* j, short data[64], stbi__huffman* hac, stbi__int16* fac)
{
    int k;
    if (j->spec_start == 0) return stbi__err("can't merge dc and ac");

    if (j->succ_high == 0) {
        int shift = j->succ_low;

        if (j->eob_run) {
            --j->eob_run;
            return 1;
        }

        k = j->spec_start;
        do {
            unsigned int zig;
            int c, r, s;
            if (j->code_bits < 16) stbi__grow_buffer_unsafe(j);
            c = (j->code_buffer >> (32 - 9))& ((1 << 9) - 1);
            r = fac[c];
            if (r) {
                k += (r >> 4) & 15;
                s = r & 15;
                j->code_buffer <<= s;
                j->code_bits -= s;
                zig = stbi__jpeg_dezigzag[k++];
                data[zig] = (short)((r >> 8) << shift);
            }
            else {
                int rs = stbi__jpeg_huff_decode(j, hac);
                if (rs < 0) return stbi__err("bad huffman code");
                s = rs & 15;
                r = rs >> 4;
                if (s == 0) {
                    if (r < 15) {
                        j->eob_run = (1 << r);
                        if (r)
                            j->eob_run += stbi__jpeg_get_bits(j, r);
                        --j->eob_run;
                        break;
                    }
                    k += 16;
                }
                else {
                    k += r;
                    zig = stbi__jpeg_dezigzag[k++];
                    data[zig] = (short)(stbi__extend_receive(j, s) << shift);
                }
            }
        } while (k <= j->spec_end);
    }
    else {

        short bit = (short)(1 << j->succ_low);

        if (j->eob_run) {
            --j->eob_run;
            for (k = j->spec_start; k <= j->spec_end; ++k) {
                short* p = &data[stbi__jpeg_dezigzag[k]];
                if (*p != 0)
                    if (stbi__jpeg_get_bit(j))
                        if ((*p & bit) == 0) {
                            if (*p > 0)
                                *p += bit;
                            else
                                *p -= bit;
                        }
            }
        }
        else {
            k = j->spec_start;
            do {
                int r, s;
                int rs = stbi__jpeg_huff_decode(j, hac);
                if (rs < 0) return stbi__err("bad huffman code");
                s = rs & 15;
                r = rs >> 4;
                if (s == 0) {
                    if (r < 15) {
                        j->eob_run = (1 << r) - 1;
                        if (r)
                            j->eob_run += stbi__jpeg_get_bits(j, r);
                        r = 64;
                    }
                    else {

                    }
                }
                else {
                    if (s != 1) return stbi__err("bad huffman code");

                    if (stbi__jpeg_get_bit(j))
                        s = bit;
                    else
                        s = -bit;
                }

                while (k <= j->spec_end) {
                    short* p = &data[stbi__jpeg_dezigzag[k++]];
                    if (*p != 0) {
                        if (stbi__jpeg_get_bit(j))
                            if ((*p & bit) == 0) {
                                if (*p > 0)
                                    *p += bit;
                                else
                                    *p -= bit;
                            }
                    }
                    else {
                        if (r == 0) {
                            *p = (short)s;
                            break;
                        }
                        --r;
                    }
                }
            } while (k <= j->spec_end);
        }
    }
    return 1;
}

static stbi_uc stbi__clamp(int x)
{

    if ((unsigned int)x > 255) {
        if (x < 0) return 0;
        if (x > 255) return 255;
    }
    return (stbi_uc)x;
}
static void stbi__idct_block(stbi_uc* out, int out_stride, short data[64])
{
    int i, val[64], * v = val;
    stbi_uc* o;
    short* d = data;

    for (i = 0; i < 8; ++i, ++d, ++v) {

        if (d[8] == 0 && d[16] == 0 && d[24] == 0 && d[32] == 0
            && d[40] == 0 && d[48] == 0 && d[56] == 0) {

            int dcterm = d[0] * 4;
            v[0] = v[8] = v[16] = v[24] = v[32] = v[40] = v[48] = v[56] = dcterm;
        }
        else {
            int t0, t1, t2, t3, p1, p2, p3, p4, p5, x0, x1, x2, x3; p2 = d[16]; p3 = d[48]; p1 = (p2 + p3) * ((int)(((0.5411961f) * 4096 + 0.5))); t2 = p1 + p3 * ((int)(((-1.847759065f) * 4096 + 0.5))); t3 = p1 + p2 * ((int)(((0.765366865f) * 4096 + 0.5))); p2 = d[0]; p3 = d[32]; t0 = ((p2 + p3) * 4096); t1 = ((p2 - p3) * 4096); x0 = t0 + t3; x3 = t0 - t3; x1 = t1 + t2; x2 = t1 - t2; t0 = d[56]; t1 = d[40]; t2 = d[24]; t3 = d[8]; p3 = t0 + t2; p4 = t1 + t3; p1 = t0 + t3; p2 = t1 + t2; p5 = (p3 + p4) * ((int)(((1.175875602f) * 4096 + 0.5))); t0 = t0 * ((int)(((0.298631336f) * 4096 + 0.5))); t1 = t1 * ((int)(((2.053119869f) * 4096 + 0.5))); t2 = t2 * ((int)(((3.072711026f) * 4096 + 0.5))); t3 = t3 * ((int)(((1.501321110f) * 4096 + 0.5))); p1 = p5 + p1 * ((int)(((-0.899976223f) * 4096 + 0.5))); p2 = p5 + p2 * ((int)(((-2.562915447f) * 4096 + 0.5))); p3 = p3 * ((int)(((-1.961570560f) * 4096 + 0.5))); p4 = p4 * ((int)(((-0.390180644f) * 4096 + 0.5))); t3 += p1 + p4; t2 += p2 + p3; t1 += p2 + p4; t0 += p1 + p3;

            x0 += 512; x1 += 512; x2 += 512; x3 += 512;
            v[0] = (x0 + t3) >> 10;
            v[56] = (x0 - t3) >> 10;
            v[8] = (x1 + t2) >> 10;
            v[48] = (x1 - t2) >> 10;
            v[16] = (x2 + t1) >> 10;
            v[40] = (x2 - t1) >> 10;
            v[24] = (x3 + t0) >> 10;
            v[32] = (x3 - t0) >> 10;
        }
    }

    for (i = 0, v = val, o = out; i < 8; ++i, v += 8, o += out_stride) {

        int t0, t1, t2, t3, p1, p2, p3, p4, p5, x0, x1, x2, x3; p2 = v[2]; p3 = v[6]; p1 = (p2 + p3) * ((int)(((0.5411961f) * 4096 + 0.5))); t2 = p1 + p3 * ((int)(((-1.847759065f) * 4096 + 0.5))); t3 = p1 + p2 * ((int)(((0.765366865f) * 4096 + 0.5))); p2 = v[0]; p3 = v[4]; t0 = ((p2 + p3) * 4096); t1 = ((p2 - p3) * 4096); x0 = t0 + t3; x3 = t0 - t3; x1 = t1 + t2; x2 = t1 - t2; t0 = v[7]; t1 = v[5]; t2 = v[3]; t3 = v[1]; p3 = t0 + t2; p4 = t1 + t3; p1 = t0 + t3; p2 = t1 + t2; p5 = (p3 + p4) * ((int)(((1.175875602f) * 4096 + 0.5))); t0 = t0 * ((int)(((0.298631336f) * 4096 + 0.5))); t1 = t1 * ((int)(((2.053119869f) * 4096 + 0.5))); t2 = t2 * ((int)(((3.072711026f) * 4096 + 0.5))); t3 = t3 * ((int)(((1.501321110f) * 4096 + 0.5))); p1 = p5 + p1 * ((int)(((-0.899976223f) * 4096 + 0.5))); p2 = p5 + p2 * ((int)(((-2.562915447f) * 4096 + 0.5))); p3 = p3 * ((int)(((-1.961570560f) * 4096 + 0.5))); p4 = p4 * ((int)(((-0.390180644f) * 4096 + 0.5))); t3 += p1 + p4; t2 += p2 + p3; t1 += p2 + p4; t0 += p1 + p3;

        x0 += 65536 + (128 << 17);
        x1 += 65536 + (128 << 17);
        x2 += 65536 + (128 << 17);
        x3 += 65536 + (128 << 17);

        o[0] = stbi__clamp((x0 + t3) >> 17);
        o[7] = stbi__clamp((x0 - t3) >> 17);
        o[1] = stbi__clamp((x1 + t2) >> 17);
        o[6] = stbi__clamp((x1 - t2) >> 17);
        o[2] = stbi__clamp((x2 + t1) >> 17);
        o[5] = stbi__clamp((x2 - t1) >> 17);
        o[3] = stbi__clamp((x3 + t0) >> 17);
        o[4] = stbi__clamp((x3 - t0) >> 17);
    }
}

static void stbi__idct_simd(stbi_uc* out, int out_stride, short data[64])
{

    __m128i row0, row1, row2, row3, row4, row5, row6, row7;
    __m128i tmp;
    __m128i rot0_0 = _mm_setr_epi16((((int)(((0.5411961f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5))) + ((int)(((-1.847759065f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5))) + ((int)(((-1.847759065f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5))) + ((int)(((-1.847759065f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5))) + ((int)(((-1.847759065f) * 4096 + 0.5)))));
    __m128i rot0_1 = _mm_setr_epi16((((int)(((0.5411961f) * 4096 + 0.5))) + ((int)(((0.765366865f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5))) + ((int)(((0.765366865f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5))) + ((int)(((0.765366865f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5))) + ((int)(((0.765366865f) * 4096 + 0.5)))), (((int)(((0.5411961f) * 4096 + 0.5)))));
    __m128i rot1_0 = _mm_setr_epi16((((int)(((1.175875602f) * 4096 + 0.5))) + ((int)(((-0.899976223f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5))) + ((int)(((-0.899976223f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5))) + ((int)(((-0.899976223f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5))) + ((int)(((-0.899976223f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5)))));
    __m128i rot1_1 = _mm_setr_epi16((((int)(((1.175875602f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5))) + ((int)(((-2.562915447f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5))) + ((int)(((-2.562915447f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5))) + ((int)(((-2.562915447f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5)))), (((int)(((1.175875602f) * 4096 + 0.5))) + ((int)(((-2.562915447f) * 4096 + 0.5)))));
    __m128i rot2_0 = _mm_setr_epi16((((int)(((-1.961570560f) * 4096 + 0.5))) + ((int)(((0.298631336f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5))) + ((int)(((0.298631336f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5))) + ((int)(((0.298631336f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5))) + ((int)(((0.298631336f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5)))));
    __m128i rot2_1 = _mm_setr_epi16((((int)(((-1.961570560f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5))) + ((int)(((3.072711026f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5))) + ((int)(((3.072711026f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5))) + ((int)(((3.072711026f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5)))), (((int)(((-1.961570560f) * 4096 + 0.5))) + ((int)(((3.072711026f) * 4096 + 0.5)))));
    __m128i rot3_0 = _mm_setr_epi16((((int)(((-0.390180644f) * 4096 + 0.5))) + ((int)(((2.053119869f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5))) + ((int)(((2.053119869f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5))) + ((int)(((2.053119869f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5))) + ((int)(((2.053119869f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5)))));
    __m128i rot3_1 = _mm_setr_epi16((((int)(((-0.390180644f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5))) + ((int)(((1.501321110f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5))) + ((int)(((1.501321110f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5))) + ((int)(((1.501321110f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5)))), (((int)(((-0.390180644f) * 4096 + 0.5))) + ((int)(((1.501321110f) * 4096 + 0.5)))));

    __m128i bias_0 = _mm_set1_epi32(512);
    __m128i bias_1 = _mm_set1_epi32(65536 + (128 << 17));

    row0 = _mm_load_si128((const __m128i*) (data + 0 * 8));
    row1 = _mm_load_si128((const __m128i*) (data + 1 * 8));
    row2 = _mm_load_si128((const __m128i*) (data + 2 * 8));
    row3 = _mm_load_si128((const __m128i*) (data + 3 * 8));
    row4 = _mm_load_si128((const __m128i*) (data + 4 * 8));
    row5 = _mm_load_si128((const __m128i*) (data + 5 * 8));
    row6 = _mm_load_si128((const __m128i*) (data + 6 * 8));
    row7 = _mm_load_si128((const __m128i*) (data + 7 * 8));

    { __m128i rot0_0lo = _mm_unpacklo_epi16((row2), (row6)); __m128i rot0_0hi = _mm_unpackhi_epi16((row2), (row6)); __m128i t2e_l = _mm_madd_epi16(rot0_0lo, rot0_0); __m128i t2e_h = _mm_madd_epi16(rot0_0hi, rot0_0); __m128i t3e_l = _mm_madd_epi16(rot0_0lo, rot0_1); __m128i t3e_h = _mm_madd_epi16(rot0_0hi, rot0_1); __m128i sum04 = _mm_add_epi16(row0, row4); __m128i dif04 = _mm_sub_epi16(row0, row4); __m128i t0e_l = _mm_srai_epi32(_mm_unpacklo_epi16(_mm_setzero_si128(), (sum04)), 4); __m128i t0e_h = _mm_srai_epi32(_mm_unpackhi_epi16(_mm_setzero_si128(), (sum04)), 4); __m128i t1e_l = _mm_srai_epi32(_mm_unpacklo_epi16(_mm_setzero_si128(), (dif04)), 4); __m128i t1e_h = _mm_srai_epi32(_mm_unpackhi_epi16(_mm_setzero_si128(), (dif04)), 4); __m128i x0_l = _mm_add_epi32(t0e_l, t3e_l); __m128i x0_h = _mm_add_epi32(t0e_h, t3e_h); __m128i x3_l = _mm_sub_epi32(t0e_l, t3e_l); __m128i x3_h = _mm_sub_epi32(t0e_h, t3e_h); __m128i x1_l = _mm_add_epi32(t1e_l, t2e_l); __m128i x1_h = _mm_add_epi32(t1e_h, t2e_h); __m128i x2_l = _mm_sub_epi32(t1e_l, t2e_l); __m128i x2_h = _mm_sub_epi32(t1e_h, t2e_h); __m128i rot2_0lo = _mm_unpacklo_epi16((row7), (row3)); __m128i rot2_0hi = _mm_unpackhi_epi16((row7), (row3)); __m128i y0o_l = _mm_madd_epi16(rot2_0lo, rot2_0); __m128i y0o_h = _mm_madd_epi16(rot2_0hi, rot2_0); __m128i y2o_l = _mm_madd_epi16(rot2_0lo, rot2_1); __m128i y2o_h = _mm_madd_epi16(rot2_0hi, rot2_1); __m128i rot3_0lo = _mm_unpacklo_epi16((row5), (row1)); __m128i rot3_0hi = _mm_unpackhi_epi16((row5), (row1)); __m128i y1o_l = _mm_madd_epi16(rot3_0lo, rot3_0); __m128i y1o_h = _mm_madd_epi16(rot3_0hi, rot3_0); __m128i y3o_l = _mm_madd_epi16(rot3_0lo, rot3_1); __m128i y3o_h = _mm_madd_epi16(rot3_0hi, rot3_1); __m128i sum17 = _mm_add_epi16(row1, row7); __m128i sum35 = _mm_add_epi16(row3, row5); __m128i rot1_0lo = _mm_unpacklo_epi16((sum17), (sum35)); __m128i rot1_0hi = _mm_unpackhi_epi16((sum17), (sum35)); __m128i y4o_l = _mm_madd_epi16(rot1_0lo, rot1_0); __m128i y4o_h = _mm_madd_epi16(rot1_0hi, rot1_0); __m128i y5o_l = _mm_madd_epi16(rot1_0lo, rot1_1); __m128i y5o_h = _mm_madd_epi16(rot1_0hi, rot1_1); __m128i x4_l = _mm_add_epi32(y0o_l, y4o_l); __m128i x4_h = _mm_add_epi32(y0o_h, y4o_h); __m128i x5_l = _mm_add_epi32(y1o_l, y5o_l); __m128i x5_h = _mm_add_epi32(y1o_h, y5o_h); __m128i x6_l = _mm_add_epi32(y2o_l, y5o_l); __m128i x6_h = _mm_add_epi32(y2o_h, y5o_h); __m128i x7_l = _mm_add_epi32(y3o_l, y4o_l); __m128i x7_h = _mm_add_epi32(y3o_h, y4o_h); { __m128i abiased_l = _mm_add_epi32(x0_l, bias_0); __m128i abiased_h = _mm_add_epi32(x0_h, bias_0); __m128i sum_l = _mm_add_epi32(abiased_l, x7_l); __m128i sum_h = _mm_add_epi32(abiased_h, x7_h); __m128i dif_l = _mm_sub_epi32(abiased_l, x7_l); __m128i dif_h = _mm_sub_epi32(abiased_h, x7_h); row0 = _mm_packs_epi32(_mm_srai_epi32(sum_l, 10), _mm_srai_epi32(sum_h, 10)); row7 = _mm_packs_epi32(_mm_srai_epi32(dif_l, 10), _mm_srai_epi32(dif_h, 10)); }; { __m128i abiased_l = _mm_add_epi32(x1_l, bias_0); __m128i abiased_h = _mm_add_epi32(x1_h, bias_0); __m128i sum_l = _mm_add_epi32(abiased_l, x6_l); __m128i sum_h = _mm_add_epi32(abiased_h, x6_h); __m128i dif_l = _mm_sub_epi32(abiased_l, x6_l); __m128i dif_h = _mm_sub_epi32(abiased_h, x6_h); row1 = _mm_packs_epi32(_mm_srai_epi32(sum_l, 10), _mm_srai_epi32(sum_h, 10)); row6 = _mm_packs_epi32(_mm_srai_epi32(dif_l, 10), _mm_srai_epi32(dif_h, 10)); }; { __m128i abiased_l = _mm_add_epi32(x2_l, bias_0); __m128i abiased_h = _mm_add_epi32(x2_h, bias_0); __m128i sum_l = _mm_add_epi32(abiased_l, x5_l); __m128i sum_h = _mm_add_epi32(abiased_h, x5_h); __m128i dif_l = _mm_sub_epi32(abiased_l, x5_l); __m128i dif_h = _mm_sub_epi32(abiased_h, x5_h); row2 = _mm_packs_epi32(_mm_srai_epi32(sum_l, 10), _mm_srai_epi32(sum_h, 10)); row5 = _mm_packs_epi32(_mm_srai_epi32(dif_l, 10), _mm_srai_epi32(dif_h, 10)); }; { __m128i abiased_l = _mm_add_epi32(x3_l, bias_0); __m128i abiased_h = _mm_add_epi32(x3_h, bias_0); __m128i sum_l = _mm_add_epi32(abiased_l, x4_l); __m128i sum_h = _mm_add_epi32(abiased_h, x4_h); __m128i dif_l = _mm_sub_epi32(abiased_l, x4_l); __m128i dif_h = _mm_sub_epi32(abiased_h, x4_h); row3 = _mm_packs_epi32(_mm_srai_epi32(sum_l, 10), _mm_srai_epi32(sum_h, 10)); row4 = _mm_packs_epi32(_mm_srai_epi32(dif_l, 10), _mm_srai_epi32(dif_h, 10)); }; };

    {

        tmp = row0; row0 = _mm_unpacklo_epi16(row0, row4); row4 = _mm_unpackhi_epi16(tmp, row4);
        tmp = row1; row1 = _mm_unpacklo_epi16(row1, row5); row5 = _mm_unpackhi_epi16(tmp, row5);
        tmp = row2; row2 = _mm_unpacklo_epi16(row2, row6); row6 = _mm_unpackhi_epi16(tmp, row6);
        tmp = row3; row3 = _mm_unpacklo_epi16(row3, row7); row7 = _mm_unpackhi_epi16(tmp, row7);

        tmp = row0; row0 = _mm_unpacklo_epi16(row0, row2); row2 = _mm_unpackhi_epi16(tmp, row2);
        tmp = row1; row1 = _mm_unpacklo_epi16(row1, row3); row3 = _mm_unpackhi_epi16(tmp, row3);
        tmp = row4; row4 = _mm_unpacklo_epi16(row4, row6); row6 = _mm_unpackhi_epi16(tmp, row6);
        tmp = row5; row5 = _mm_unpacklo_epi16(row5, row7); row7 = _mm_unpackhi_epi16(tmp, row7);

        tmp = row0; row0 = _mm_unpacklo_epi16(row0, row1); row1 = _mm_unpackhi_epi16(tmp, row1);
        tmp = row2; row2 = _mm_unpacklo_epi16(row2, row3); row3 = _mm_unpackhi_epi16(tmp, row3);
        tmp = row4; row4 = _mm_unpacklo_epi16(row4, row5); row5 = _mm_unpackhi_epi16(tmp, row5);
        tmp = row6; row6 = _mm_unpacklo_epi16(row6, row7); row7 = _mm_unpackhi_epi16(tmp, row7);
    }

    { __m128i rot0_0lo = _mm_unpacklo_epi16((row2), (row6)); __m128i rot0_0hi = _mm_unpackhi_epi16((row2), (row6)); __m128i t2e_l = _mm_madd_epi16(rot0_0lo, rot0_0); __m128i t2e_h = _mm_madd_epi16(rot0_0hi, rot0_0); __m128i t3e_l = _mm_madd_epi16(rot0_0lo, rot0_1); __m128i t3e_h = _mm_madd_epi16(rot0_0hi, rot0_1); __m128i sum04 = _mm_add_epi16(row0, row4); __m128i dif04 = _mm_sub_epi16(row0, row4); __m128i t0e_l = _mm_srai_epi32(_mm_unpacklo_epi16(_mm_setzero_si128(), (sum04)), 4); __m128i t0e_h = _mm_srai_epi32(_mm_unpackhi_epi16(_mm_setzero_si128(), (sum04)), 4); __m128i t1e_l = _mm_srai_epi32(_mm_unpacklo_epi16(_mm_setzero_si128(), (dif04)), 4); __m128i t1e_h = _mm_srai_epi32(_mm_unpackhi_epi16(_mm_setzero_si128(), (dif04)), 4); __m128i x0_l = _mm_add_epi32(t0e_l, t3e_l); __m128i x0_h = _mm_add_epi32(t0e_h, t3e_h); __m128i x3_l = _mm_sub_epi32(t0e_l, t3e_l); __m128i x3_h = _mm_sub_epi32(t0e_h, t3e_h); __m128i x1_l = _mm_add_epi32(t1e_l, t2e_l); __m128i x1_h = _mm_add_epi32(t1e_h, t2e_h); __m128i x2_l = _mm_sub_epi32(t1e_l, t2e_l); __m128i x2_h = _mm_sub_epi32(t1e_h, t2e_h); __m128i rot2_0lo = _mm_unpacklo_epi16((row7), (row3)); __m128i rot2_0hi = _mm_unpackhi_epi16((row7), (row3)); __m128i y0o_l = _mm_madd_epi16(rot2_0lo, rot2_0); __m128i y0o_h = _mm_madd_epi16(rot2_0hi, rot2_0); __m128i y2o_l = _mm_madd_epi16(rot2_0lo, rot2_1); __m128i y2o_h = _mm_madd_epi16(rot2_0hi, rot2_1); __m128i rot3_0lo = _mm_unpacklo_epi16((row5), (row1)); __m128i rot3_0hi = _mm_unpackhi_epi16((row5), (row1)); __m128i y1o_l = _mm_madd_epi16(rot3_0lo, rot3_0); __m128i y1o_h = _mm_madd_epi16(rot3_0hi, rot3_0); __m128i y3o_l = _mm_madd_epi16(rot3_0lo, rot3_1); __m128i y3o_h = _mm_madd_epi16(rot3_0hi, rot3_1); __m128i sum17 = _mm_add_epi16(row1, row7); __m128i sum35 = _mm_add_epi16(row3, row5); __m128i rot1_0lo = _mm_unpacklo_epi16((sum17), (sum35)); __m128i rot1_0hi = _mm_unpackhi_epi16((sum17), (sum35)); __m128i y4o_l = _mm_madd_epi16(rot1_0lo, rot1_0); __m128i y4o_h = _mm_madd_epi16(rot1_0hi, rot1_0); __m128i y5o_l = _mm_madd_epi16(rot1_0lo, rot1_1); __m128i y5o_h = _mm_madd_epi16(rot1_0hi, rot1_1); __m128i x4_l = _mm_add_epi32(y0o_l, y4o_l); __m128i x4_h = _mm_add_epi32(y0o_h, y4o_h); __m128i x5_l = _mm_add_epi32(y1o_l, y5o_l); __m128i x5_h = _mm_add_epi32(y1o_h, y5o_h); __m128i x6_l = _mm_add_epi32(y2o_l, y5o_l); __m128i x6_h = _mm_add_epi32(y2o_h, y5o_h); __m128i x7_l = _mm_add_epi32(y3o_l, y4o_l); __m128i x7_h = _mm_add_epi32(y3o_h, y4o_h); { __m128i abiased_l = _mm_add_epi32(x0_l, bias_1); __m128i abiased_h = _mm_add_epi32(x0_h, bias_1); __m128i sum_l = _mm_add_epi32(abiased_l, x7_l); __m128i sum_h = _mm_add_epi32(abiased_h, x7_h); __m128i dif_l = _mm_sub_epi32(abiased_l, x7_l); __m128i dif_h = _mm_sub_epi32(abiased_h, x7_h); row0 = _mm_packs_epi32(_mm_srai_epi32(sum_l, 17), _mm_srai_epi32(sum_h, 17)); row7 = _mm_packs_epi32(_mm_srai_epi32(dif_l, 17), _mm_srai_epi32(dif_h, 17)); }; { __m128i abiased_l = _mm_add_epi32(x1_l, bias_1); __m128i abiased_h = _mm_add_epi32(x1_h, bias_1); __m128i sum_l = _mm_add_epi32(abiased_l, x6_l); __m128i sum_h = _mm_add_epi32(abiased_h, x6_h); __m128i dif_l = _mm_sub_epi32(abiased_l, x6_l); __m128i dif_h = _mm_sub_epi32(abiased_h, x6_h); row1 = _mm_packs_epi32(_mm_srai_epi32(sum_l, 17), _mm_srai_epi32(sum_h, 17)); row6 = _mm_packs_epi32(_mm_srai_epi32(dif_l, 17), _mm_srai_epi32(dif_h, 17)); }; { __m128i abiased_l = _mm_add_epi32(x2_l, bias_1); __m128i abiased_h = _mm_add_epi32(x2_h, bias_1); __m128i sum_l = _mm_add_epi32(abiased_l, x5_l); __m128i sum_h = _mm_add_epi32(abiased_h, x5_h); __m128i dif_l = _mm_sub_epi32(abiased_l, x5_l); __m128i dif_h = _mm_sub_epi32(abiased_h, x5_h); row2 = _mm_packs_epi32(_mm_srai_epi32(sum_l, 17), _mm_srai_epi32(sum_h, 17)); row5 = _mm_packs_epi32(_mm_srai_epi32(dif_l, 17), _mm_srai_epi32(dif_h, 17)); }; { __m128i abiased_l = _mm_add_epi32(x3_l, bias_1); __m128i abiased_h = _mm_add_epi32(x3_h, bias_1); __m128i sum_l = _mm_add_epi32(abiased_l, x4_l); __m128i sum_h = _mm_add_epi32(abiased_h, x4_h); __m128i dif_l = _mm_sub_epi32(abiased_l, x4_l); __m128i dif_h = _mm_sub_epi32(abiased_h, x4_h); row3 = _mm_packs_epi32(_mm_srai_epi32(sum_l, 17), _mm_srai_epi32(sum_h, 17)); row4 = _mm_packs_epi32(_mm_srai_epi32(dif_l, 17), _mm_srai_epi32(dif_h, 17)); }; };

    {

        __m128i p0 = _mm_packus_epi16(row0, row1);
        __m128i p1 = _mm_packus_epi16(row2, row3);
        __m128i p2 = _mm_packus_epi16(row4, row5);
        __m128i p3 = _mm_packus_epi16(row6, row7);

        tmp = p0; p0 = _mm_unpacklo_epi8(p0, p2); p2 = _mm_unpackhi_epi8(tmp, p2);
        tmp = p1; p1 = _mm_unpacklo_epi8(p1, p3); p3 = _mm_unpackhi_epi8(tmp, p3);

        tmp = p0; p0 = _mm_unpacklo_epi8(p0, p1); p1 = _mm_unpackhi_epi8(tmp, p1);
        tmp = p2; p2 = _mm_unpacklo_epi8(p2, p3); p3 = _mm_unpackhi_epi8(tmp, p3);

        tmp = p0; p0 = _mm_unpacklo_epi8(p0, p2); p2 = _mm_unpackhi_epi8(tmp, p2);
        tmp = p1; p1 = _mm_unpacklo_epi8(p1, p3); p3 = _mm_unpackhi_epi8(tmp, p3);

        _mm_storel_epi64((__m128i*) out, p0); out += out_stride;
        _mm_storel_epi64((__m128i*) out, (__m128i)__builtin_ia32_pshufd((__v4si)(__m128i)(p0), (int)(0x4e))); out += out_stride;
        _mm_storel_epi64((__m128i*) out, p2); out += out_stride;
        _mm_storel_epi64((__m128i*) out, (__m128i)__builtin_ia32_pshufd((__v4si)(__m128i)(p2), (int)(0x4e))); out += out_stride;
        _mm_storel_epi64((__m128i*) out, p1); out += out_stride;
        _mm_storel_epi64((__m128i*) out, (__m128i)__builtin_ia32_pshufd((__v4si)(__m128i)(p1), (int)(0x4e))); out += out_stride;
        _mm_storel_epi64((__m128i*) out, p3); out += out_stride;
        _mm_storel_epi64((__m128i*) out, (__m128i)__builtin_ia32_pshufd((__v4si)(__m128i)(p3), (int)(0x4e)));
    }
}
static stbi_uc stbi__get_marker(stbi__jpeg* j)
{
    stbi_uc x;
    if (j->marker != 0xff) { x = j->marker; j->marker = 0xff; return x; }
    x = stbi__get8(j->s);
    if (x != 0xff) return 0xff;
    while (x == 0xff)
        x = stbi__get8(j->s);
    return x;
}

static void stbi__jpeg_reset(stbi__jpeg* j)
{
    j->code_bits = 0;
    j->code_buffer = 0;
    j->nomore = 0;
    j->img_comp[0].dc_pred = j->img_comp[1].dc_pred = j->img_comp[2].dc_pred = j->img_comp[3].dc_pred = 0;
    j->marker = 0xff;
    j->todo = j->restart_interval ? j->restart_interval : 0x7fffffff;
    j->eob_run = 0;

}

static int stbi__parse_entropy_coded_data(stbi__jpeg* z)
{
    stbi__jpeg_reset(z);
    if (!z->progressive) {
        if (z->scan_n == 1) {
            int i, j;
            short data[64] __attribute__((aligned(16)));
            int n = z->order[0];

            int w = (z->img_comp[n].x + 7) >> 3;
            int h = (z->img_comp[n].y + 7) >> 3;
            for (j = 0; j < h; ++j) {
                for (i = 0; i < w; ++i) {
                    int ha = z->img_comp[n].ha;
                    if (!stbi__jpeg_decode_block(z, data, z->huff_dc + z->img_comp[n].hd, z->huff_ac + ha, z->fast_ac[ha], n, z->dequant[z->img_comp[n].tq])) return 0;
                    z->idct_block_kernel(z->img_comp[n].data + z->img_comp[n].w2 * j * 8 + i * 8, z->img_comp[n].w2, data);

                    if (--z->todo <= 0) {
                        if (z->code_bits < 24) stbi__grow_buffer_unsafe(z);

                        if (!((z->marker) >= 0xd0 && (z->marker) <= 0xd7)) return 1;
                        stbi__jpeg_reset(z);
                    }
                }
            }
            return 1;
        }
        else {
            int i, j, k, x, y;
            short data[64] __attribute__((aligned(16)));
            for (j = 0; j < z->img_mcu_y; ++j) {
                for (i = 0; i < z->img_mcu_x; ++i) {

                    for (k = 0; k < z->scan_n; ++k) {
                        int n = z->order[k];

                        for (y = 0; y < z->img_comp[n].v; ++y) {
                            for (x = 0; x < z->img_comp[n].h; ++x) {
                                int x2 = (i * z->img_comp[n].h + x) * 8;
                                int y2 = (j * z->img_comp[n].v + y) * 8;
                                int ha = z->img_comp[n].ha;
                                if (!stbi__jpeg_decode_block(z, data, z->huff_dc + z->img_comp[n].hd, z->huff_ac + ha, z->fast_ac[ha], n, z->dequant[z->img_comp[n].tq])) return 0;
                                z->idct_block_kernel(z->img_comp[n].data + z->img_comp[n].w2 * y2 + x2, z->img_comp[n].w2, data);
                            }
                        }
                    }

                    if (--z->todo <= 0) {
                        if (z->code_bits < 24) stbi__grow_buffer_unsafe(z);
                        if (!((z->marker) >= 0xd0 && (z->marker) <= 0xd7)) return 1;
                        stbi__jpeg_reset(z);
                    }
                }
            }
            return 1;
        }
    }
    else {
        if (z->scan_n == 1) {
            int i, j;
            int n = z->order[0];

            int w = (z->img_comp[n].x + 7) >> 3;
            int h = (z->img_comp[n].y + 7) >> 3;
            for (j = 0; j < h; ++j) {
                for (i = 0; i < w; ++i) {
                    short* data = z->img_comp[n].coeff + 64 * (i + j * z->img_comp[n].coeff_w);
                    if (z->spec_start == 0) {
                        if (!stbi__jpeg_decode_block_prog_dc(z, data, &z->huff_dc[z->img_comp[n].hd], n))
                            return 0;
                    }
                    else {
                        int ha = z->img_comp[n].ha;
                        if (!stbi__jpeg_decode_block_prog_ac(z, data, &z->huff_ac[ha], z->fast_ac[ha]))
                            return 0;
                    }

                    if (--z->todo <= 0) {
                        if (z->code_bits < 24) stbi__grow_buffer_unsafe(z);
                        if (!((z->marker) >= 0xd0 && (z->marker) <= 0xd7)) return 1;
                        stbi__jpeg_reset(z);
                    }
                }
            }
            return 1;
        }
        else {
            int i, j, k, x, y;
            for (j = 0; j < z->img_mcu_y; ++j) {
                for (i = 0; i < z->img_mcu_x; ++i) {

                    for (k = 0; k < z->scan_n; ++k) {
                        int n = z->order[k];

                        for (y = 0; y < z->img_comp[n].v; ++y) {
                            for (x = 0; x < z->img_comp[n].h; ++x) {
                                int x2 = (i * z->img_comp[n].h + x);
                                int y2 = (j * z->img_comp[n].v + y);
                                short* data = z->img_comp[n].coeff + 64 * (x2 + y2 * z->img_comp[n].coeff_w);
                                if (!stbi__jpeg_decode_block_prog_dc(z, data, &z->huff_dc[z->img_comp[n].hd], n))
                                    return 0;
                            }
                        }
                    }

                    if (--z->todo <= 0) {
                        if (z->code_bits < 24) stbi__grow_buffer_unsafe(z);
                        if (!((z->marker) >= 0xd0 && (z->marker) <= 0xd7)) return 1;
                        stbi__jpeg_reset(z);
                    }
                }
            }
            return 1;
        }
    }
}

static void stbi__jpeg_dequantize(short* data, stbi__uint16* dequant)
{
    int i;
    for (i = 0; i < 64; ++i)
        data[i] *= dequant[i];
}

static void stbi__jpeg_finish(stbi__jpeg* z)
{
    if (z->progressive) {

        int i, j, n;
        for (n = 0; n < z->s->img_n; ++n) {
            int w = (z->img_comp[n].x + 7) >> 3;
            int h = (z->img_comp[n].y + 7) >> 3;
            for (j = 0; j < h; ++j) {
                for (i = 0; i < w; ++i) {
                    short* data = z->img_comp[n].coeff + 64 * (i + j * z->img_comp[n].coeff_w);
                    stbi__jpeg_dequantize(data, z->dequant[z->img_comp[n].tq]);
                    z->idct_block_kernel(z->img_comp[n].data + z->img_comp[n].w2 * j * 8 + i * 8, z->img_comp[n].w2, data);
                }
            }
        }
    }
}

static int stbi__process_marker(stbi__jpeg* z, int m)
{
    int L;
    switch (m) {
    case 0xff:
        return stbi__err("expected marker");

    case 0xDD:
        if (stbi__get16be(z->s) != 4) return stbi__err("bad DRI len");
        z->restart_interval = stbi__get16be(z->s);
        return 1;

    case 0xDB:
        L = stbi__get16be(z->s) - 2;
        while (L > 0) {
            int q = stbi__get8(z->s);
            int p = q >> 4, sixteen = (p != 0);
            int t = q & 15, i;
            if (p != 0 && p != 1) return stbi__err("bad DQT type");
            if (t > 3) return stbi__err("bad DQT table");

            for (i = 0; i < 64; ++i)
                z->dequant[t][stbi__jpeg_dezigzag[i]] = (stbi__uint16)(sixteen ? stbi__get16be(z->s) : stbi__get8(z->s));
            L -= (sixteen ? 129 : 65);
        }
        return L == 0;

    case 0xC4:
        L = stbi__get16be(z->s) - 2;
        while (L > 0) {
            stbi_uc* v;
            int sizes[16], i, n = 0;
            int q = stbi__get8(z->s);
            int tc = q >> 4;
            int th = q & 15;
            if (tc > 1 || th > 3) return stbi__err("bad DHT header");
            for (i = 0; i < 16; ++i) {
                sizes[i] = stbi__get8(z->s);
                n += sizes[i];
            }
            L -= 17;
            if (tc == 0) {
                if (!stbi__build_huffman(z->huff_dc + th, sizes)) return 0;
                v = z->huff_dc[th].values;
            }
            else {
                if (!stbi__build_huffman(z->huff_ac + th, sizes)) return 0;
                v = z->huff_ac[th].values;
            }
            for (i = 0; i < n; ++i)
                v[i] = stbi__get8(z->s);
            if (tc != 0)
                stbi__build_fast_ac(z->fast_ac[th], z->huff_ac + th);
            L -= n;
        }
        return L == 0;
    }

    if ((m >= 0xE0 && m <= 0xEF) || m == 0xFE) {
        L = stbi__get16be(z->s);
        if (L < 2) {
            if (m == 0xFE)
                return stbi__err("bad COM len");
            else
                return stbi__err("bad APP len");
        }
        L -= 2;

        if (m == 0xE0 && L >= 5) {
            static const unsigned char tag[5] = { 'J','F','I','F','\0' };
            int ok = 1;
            int i;
            for (i = 0; i < 5; ++i)
                if (stbi__get8(z->s) != tag[i])
                    ok = 0;
            L -= 5;
            if (ok)
                z->jfif = 1;
        }
        else if (m == 0xEE && L >= 12) {
            static const unsigned char tag[6] = { 'A','d','o','b','e','\0' };
            int ok = 1;
            int i;
            for (i = 0; i < 6; ++i)
                if (stbi__get8(z->s) != tag[i])
                    ok = 0;
            L -= 6;
            if (ok) {
                stbi__get8(z->s);
                stbi__get16be(z->s);
                stbi__get16be(z->s);
                z->app14_color_transform = stbi__get8(z->s);
                L -= 6;
            }
        }

        stbi__skip(z->s, L);
        return 1;
    }

    return stbi__err("unknown marker");
}

static int stbi__process_scan_header(stbi__jpeg* z)
{
    int i;
    int Ls = stbi__get16be(z->s);
    z->scan_n = stbi__get8(z->s);
    if (z->scan_n < 1 || z->scan_n > 4 || z->scan_n > (int)z->s->img_n) return stbi__err("bad SOS component count");
    if (Ls != 6 + 2 * z->scan_n) return stbi__err("bad SOS len");
    for (i = 0; i < z->scan_n; ++i) {
        int id = stbi__get8(z->s), which;
        int q = stbi__get8(z->s);
        for (which = 0; which < z->s->img_n; ++which)
            if (z->img_comp[which].id == id)
                break;
        if (which == z->s->img_n) return 0;
        z->img_comp[which].hd = q >> 4; if (z->img_comp[which].hd > 3) return stbi__err("bad DC huff");
        z->img_comp[which].ha = q & 15; if (z->img_comp[which].ha > 3) return stbi__err("bad AC huff");
        z->order[i] = which;
    }

    {
        int aa;
        z->spec_start = stbi__get8(z->s);
        z->spec_end = stbi__get8(z->s);
        aa = stbi__get8(z->s);
        z->succ_high = (aa >> 4);
        z->succ_low = (aa & 15);
        if (z->progressive) {
            if (z->spec_start > 63 || z->spec_end > 63 || z->spec_start > z->spec_end || z->succ_high > 13 || z->succ_low > 13)
                return stbi__err("bad SOS");
        }
        else {
            if (z->spec_start != 0) return stbi__err("bad SOS");
            if (z->succ_high != 0 || z->succ_low != 0) return stbi__err("bad SOS");
            z->spec_end = 63;
        }
    }

    return 1;
}

static int stbi__free_jpeg_components(stbi__jpeg* z, int ncomp, int why)
{
    int i;
    for (i = 0; i < ncomp; ++i) {
        if (z->img_comp[i].raw_data) {
            free(z->img_comp[i].raw_data);
            z->img_comp[i].raw_data = ((void*)0);
            z->img_comp[i].data = ((void*)0);
        }
        if (z->img_comp[i].raw_coeff) {
            free(z->img_comp[i].raw_coeff);
            z->img_comp[i].raw_coeff = 0;
            z->img_comp[i].coeff = 0;
        }
        if (z->img_comp[i].linebuf) {
            free(z->img_comp[i].linebuf);
            z->img_comp[i].linebuf = ((void*)0);
        }
    }
    return why;
}

static int stbi__process_frame_header(stbi__jpeg* z, int scan)
{
    stbi__context* s = z->s;
    int Lf, p, i, q, h_max = 1, v_max = 1, c;
    Lf = stbi__get16be(s); if (Lf < 11) return stbi__err("bad SOF len");
    p = stbi__get8(s); if (p != 8) return stbi__err("only 8-bit");
    s->img_y = stbi__get16be(s); if (s->img_y == 0) return stbi__err("no header height");
    s->img_x = stbi__get16be(s); if (s->img_x == 0) return stbi__err("0 width");
    c = stbi__get8(s);
    if (c != 3 && c != 1 && c != 4) return stbi__err("bad component count");
    s->img_n = c;
    for (i = 0; i < c; ++i) {
        z->img_comp[i].data = ((void*)0);
        z->img_comp[i].linebuf = ((void*)0);
    }

    if (Lf != 8 + 3 * s->img_n) return stbi__err("bad SOF len");

    z->rgb = 0;
    for (i = 0; i < s->img_n; ++i) {
        static const unsigned char rgb[3] = { 'R', 'G', 'B' };
        z->img_comp[i].id = stbi__get8(s);
        if (s->img_n == 3 && z->img_comp[i].id == rgb[i])
            ++z->rgb;
        q = stbi__get8(s);
        z->img_comp[i].h = (q >> 4); if (!z->img_comp[i].h || z->img_comp[i].h > 4) return stbi__err("bad H");
        z->img_comp[i].v = q & 15; if (!z->img_comp[i].v || z->img_comp[i].v > 4) return stbi__err("bad V");
        z->img_comp[i].tq = stbi__get8(s); if (z->img_comp[i].tq > 3) return stbi__err("bad TQ");
    }

    if (scan != STBI__SCAN_load) return 1;

    if (!stbi__mad3sizes_valid(s->img_x, s->img_y, s->img_n, 0)) return stbi__err("too large");

    for (i = 0; i < s->img_n; ++i) {
        if (z->img_comp[i].h > h_max) h_max = z->img_comp[i].h;
        if (z->img_comp[i].v > v_max) v_max = z->img_comp[i].v;
    }

    z->img_h_max = h_max;
    z->img_v_max = v_max;
    z->img_mcu_w = h_max * 8;
    z->img_mcu_h = v_max * 8;

    z->img_mcu_x = (s->img_x + z->img_mcu_w - 1) / z->img_mcu_w;
    z->img_mcu_y = (s->img_y + z->img_mcu_h - 1) / z->img_mcu_h;

    for (i = 0; i < s->img_n; ++i) {

        z->img_comp[i].x = (s->img_x * z->img_comp[i].h + h_max - 1) / h_max;
        z->img_comp[i].y = (s->img_y * z->img_comp[i].v + v_max - 1) / v_max;

        z->img_comp[i].w2 = z->img_mcu_x * z->img_comp[i].h * 8;
        z->img_comp[i].h2 = z->img_mcu_y * z->img_comp[i].v * 8;
        z->img_comp[i].coeff = 0;
        z->img_comp[i].raw_coeff = 0;
        z->img_comp[i].linebuf = ((void*)0);
        z->img_comp[i].raw_data = stbi__malloc_mad2(z->img_comp[i].w2, z->img_comp[i].h2, 15);
        if (z->img_comp[i].raw_data == ((void*)0))
            return stbi__free_jpeg_components(z, i + 1, stbi__err("outofmem"));

        z->img_comp[i].data = (stbi_uc*)(((size_t)z->img_comp[i].raw_data + 15) & ~15);
        if (z->progressive) {

            z->img_comp[i].coeff_w = z->img_comp[i].w2 / 8;
            z->img_comp[i].coeff_h = z->img_comp[i].h2 / 8;
            z->img_comp[i].raw_coeff = stbi__malloc_mad3(z->img_comp[i].w2, z->img_comp[i].h2, sizeof(short), 15);
            if (z->img_comp[i].raw_coeff == ((void*)0))
                return stbi__free_jpeg_components(z, i + 1, stbi__err("outofmem"));
            z->img_comp[i].coeff = (short*)(((size_t)z->img_comp[i].raw_coeff + 15) & ~15);
        }
    }

    return 1;
}
static int stbi__decode_jpeg_header(stbi__jpeg* z, int scan)
{
    int m;
    z->jfif = 0;
    z->app14_color_transform = -1;
    z->marker = 0xff;
    m = stbi__get_marker(z);
    if (!((m) == 0xd8)) return stbi__err("no SOI");
    if (scan == STBI__SCAN_type) return 1;
    m = stbi__get_marker(z);
    while (!((m) == 0xc0 || (m) == 0xc1 || (m) == 0xc2)) {
        if (!stbi__process_marker(z, m)) return 0;
        m = stbi__get_marker(z);
        while (m == 0xff) {

            if (stbi__at_eof(z->s)) return stbi__err("no SOF");
            m = stbi__get_marker(z);
        }
    }
    z->progressive = ((m) == 0xc2);
    if (!stbi__process_frame_header(z, scan)) return 0;
    return 1;
}

static int stbi__decode_jpeg_image(stbi__jpeg* j)
{
    int m;
    for (m = 0; m < 4; m++) {
        j->img_comp[m].raw_data = ((void*)0);
        j->img_comp[m].raw_coeff = ((void*)0);
    }
    j->restart_interval = 0;
    if (!stbi__decode_jpeg_header(j, STBI__SCAN_load)) return 0;
    m = stbi__get_marker(j);
    while (!((m) == 0xd9)) {
        if (((m) == 0xda)) {
            if (!stbi__process_scan_header(j)) return 0;
            if (!stbi__parse_entropy_coded_data(j)) return 0;
            if (j->marker == 0xff) {

                while (!stbi__at_eof(j->s)) {
                    int x = stbi__get8(j->s);
                    if (x == 255) {
                        j->marker = stbi__get8(j->s);
                        break;
                    }
                }

            }
        }
        else if (((m) == 0xdc)) {
            int Ld = stbi__get16be(j->s);
            stbi__uint32 NL = stbi__get16be(j->s);
            if (Ld != 4) return stbi__err("bad DNL len");
            if (NL != j->s->img_y) return stbi__err("bad DNL height");
        }
        else {
            if (!stbi__process_marker(j, m)) return 0;
        }
        m = stbi__get_marker(j);
    }
    if (j->progressive)
        stbi__jpeg_finish(j);
    return 1;
}

typedef stbi_uc* (*resample_row_func)(stbi_uc* out, stbi_uc* in0, stbi_uc* in1,
    int w, int hs);

static stbi_uc* resample_row_1(stbi_uc* out, stbi_uc* in_near, stbi_uc* in_far, int w, int hs)
{
    (void)sizeof(out);
    (void)sizeof(in_far);
    (void)sizeof(w);
    (void)sizeof(hs);
    return in_near;
}

static stbi_uc* stbi__resample_row_v_2(stbi_uc* out, stbi_uc* in_near, stbi_uc* in_far, int w, int hs)
{

    int i;
    (void)sizeof(hs);
    for (i = 0; i < w; ++i)
        out[i] = ((stbi_uc)((3 * in_near[i] + in_far[i] + 2) >> 2));
    return out;
}

static stbi_uc* stbi__resample_row_h_2(stbi_uc* out, stbi_uc* in_near, stbi_uc* in_far, int w, int hs)
{

    int i;
    stbi_uc* input = in_near;

    if (w == 1) {

        out[0] = out[1] = input[0];
        return out;
    }

    out[0] = input[0];
    out[1] = ((stbi_uc)((input[0] * 3 + input[1] + 2) >> 2));
    for (i = 1; i < w - 1; ++i) {
        int n = 3 * input[i] + 2;
        out[i * 2 + 0] = ((stbi_uc)((n + input[i - 1]) >> 2));
        out[i * 2 + 1] = ((stbi_uc)((n + input[i + 1]) >> 2));
    }
    out[i * 2 + 0] = ((stbi_uc)((input[w - 2] * 3 + input[w - 1] + 2) >> 2));
    out[i * 2 + 1] = input[w - 1];

    (void)sizeof(in_far);
    (void)sizeof(hs);

    return out;
}

static stbi_uc* stbi__resample_row_hv_2(stbi_uc* out, stbi_uc* in_near, stbi_uc* in_far, int w, int hs)
{

    int i, t0, t1;
    if (w == 1) {
        out[0] = out[1] = ((stbi_uc)((3 * in_near[0] + in_far[0] + 2) >> 2));
        return out;
    }

    t1 = 3 * in_near[0] + in_far[0];
    out[0] = ((stbi_uc)((t1 + 2) >> 2));
    for (i = 1; i < w; ++i) {
        t0 = t1;
        t1 = 3 * in_near[i] + in_far[i];
        out[i * 2 - 1] = ((stbi_uc)((3 * t0 + t1 + 8) >> 4));
        out[i * 2] = ((stbi_uc)((3 * t1 + t0 + 8) >> 4));
    }
    out[w * 2 - 1] = ((stbi_uc)((t1 + 2) >> 2));

    (void)sizeof(hs);

    return out;
}

static stbi_uc* stbi__resample_row_hv_2_simd(stbi_uc* out, stbi_uc* in_near, stbi_uc* in_far, int w, int hs)
{

    int i = 0, t0, t1;

    if (w == 1) {
        out[0] = out[1] = ((stbi_uc)((3 * in_near[0] + in_far[0] + 2) >> 2));
        return out;
    }

    t1 = 3 * in_near[0] + in_far[0];

    for (; i < ((w - 1) & ~7); i += 8) {

        __m128i zero = _mm_setzero_si128();
        __m128i farb = _mm_loadl_epi64((__m128i*) (in_far + i));
        __m128i nearb = _mm_loadl_epi64((__m128i*) (in_near + i));
        __m128i farw = _mm_unpacklo_epi8(farb, zero);
        __m128i nearw = _mm_unpacklo_epi8(nearb, zero);
        __m128i diff = _mm_sub_epi16(farw, nearw);
        __m128i nears = _mm_slli_epi16(nearw, 2);
        __m128i curr = _mm_add_epi16(nears, diff);

        __m128i prv0 = (__m128i)__builtin_ia32_pslldqi128_byteshift((__v2di)(__m128i)(curr), (int)(2));
        __m128i nxt0 = (__m128i)__builtin_ia32_psrldqi128_byteshift((__v2di)(__m128i)(curr), (int)(2));
        __m128i prev = (__m128i)__builtin_ia32_vec_set_v8hi((__v8hi)(__m128i)(prv0), (int)(t1), (int)(0));
        __m128i next = (__m128i)__builtin_ia32_vec_set_v8hi((__v8hi)(__m128i)(nxt0), (int)(3 * in_near[i + 8] + in_far[i + 8]), (int)(7));

        __m128i bias = _mm_set1_epi16(8);
        __m128i curs = _mm_slli_epi16(curr, 2);
        __m128i prvd = _mm_sub_epi16(prev, curr);
        __m128i nxtd = _mm_sub_epi16(next, curr);
        __m128i curb = _mm_add_epi16(curs, bias);
        __m128i even = _mm_add_epi16(prvd, curb);
        __m128i odd = _mm_add_epi16(nxtd, curb);

        __m128i int0 = _mm_unpacklo_epi16(even, odd);
        __m128i int1 = _mm_unpackhi_epi16(even, odd);
        __m128i de0 = _mm_srli_epi16(int0, 4);
        __m128i de1 = _mm_srli_epi16(int1, 4);

        __m128i outv = _mm_packus_epi16(de0, de1);
        _mm_storeu_si128((__m128i*) (out + i * 2), outv);
        t1 = 3 * in_near[i + 7] + in_far[i + 7];
    }

    t0 = t1;
    t1 = 3 * in_near[i] + in_far[i];
    out[i * 2] = ((stbi_uc)((3 * t1 + t0 + 8) >> 4));

    for (++i; i < w; ++i) {
        t0 = t1;
        t1 = 3 * in_near[i] + in_far[i];
        out[i * 2 - 1] = ((stbi_uc)((3 * t0 + t1 + 8) >> 4));
        out[i * 2] = ((stbi_uc)((3 * t1 + t0 + 8) >> 4));
    }
    out[w * 2 - 1] = ((stbi_uc)((t1 + 2) >> 2));

    (void)sizeof(hs);

    return out;
}

static stbi_uc* stbi__resample_row_generic(stbi_uc* out, stbi_uc* in_near, stbi_uc* in_far, int w, int hs)
{

    int i, j;
    (void)sizeof(in_far);
    for (i = 0; i < w; ++i)
        for (j = 0; j < hs; ++j)
            out[i * hs + j] = in_near[i];
    return out;
}

static void stbi__YCbCr_to_RGB_row(stbi_uc* out, const stbi_uc* y, const stbi_uc* pcb, const stbi_uc* pcr, int count, int step)
{
    int i;
    for (i = 0; i < count; ++i) {
        int y_fixed = (y[i] << 20) + (1 << 19);
        int r, g, b;
        int cr = pcr[i] - 128;
        int cb = pcb[i] - 128;
        r = y_fixed + cr * (((int)((1.40200f) * 4096.0f + 0.5f)) << 8);
        g = y_fixed + (cr * -(((int)((0.71414f) * 4096.0f + 0.5f)) << 8)) + ((cb * -(((int)((0.34414f) * 4096.0f + 0.5f)) << 8)) & 0xffff0000);
        b = y_fixed + cb * (((int)((1.77200f) * 4096.0f + 0.5f)) << 8);
        r >>= 20;
        g >>= 20;
        b >>= 20;
        if ((unsigned)r > 255) { if (r < 0) r = 0; else r = 255; }
        if ((unsigned)g > 255) { if (g < 0) g = 0; else g = 255; }
        if ((unsigned)b > 255) { if (b < 0) b = 0; else b = 255; }
        out[0] = (stbi_uc)r;
        out[1] = (stbi_uc)g;
        out[2] = (stbi_uc)b;
        out[3] = 255;
        out += step;
    }
}

static void stbi__YCbCr_to_RGB_simd(stbi_uc* out, stbi_uc const* y, stbi_uc const* pcb, stbi_uc const* pcr, int count, int step)
{
    int i = 0;

    if (step == 4) {

        __m128i signflip = _mm_set1_epi8(-0x80);
        __m128i cr_const0 = _mm_set1_epi16((short)(1.40200f * 4096.0f + 0.5f));
        __m128i cr_const1 = _mm_set1_epi16(-(short)(0.71414f * 4096.0f + 0.5f));
        __m128i cb_const0 = _mm_set1_epi16(-(short)(0.34414f * 4096.0f + 0.5f));
        __m128i cb_const1 = _mm_set1_epi16((short)(1.77200f * 4096.0f + 0.5f));
        __m128i y_bias = _mm_set1_epi8((char)(unsigned char)128);
        __m128i xw = _mm_set1_epi16(255);

        for (; i + 7 < count; i += 8) {

            __m128i y_bytes = _mm_loadl_epi64((__m128i*) (y + i));
            __m128i cr_bytes = _mm_loadl_epi64((__m128i*) (pcr + i));
            __m128i cb_bytes = _mm_loadl_epi64((__m128i*) (pcb + i));
            __m128i cr_biased = _mm_xor_si128(cr_bytes, signflip);
            __m128i cb_biased = _mm_xor_si128(cb_bytes, signflip);

            __m128i yw = _mm_unpacklo_epi8(y_bias, y_bytes);
            __m128i crw = _mm_unpacklo_epi8(_mm_setzero_si128(), cr_biased);
            __m128i cbw = _mm_unpacklo_epi8(_mm_setzero_si128(), cb_biased);

            __m128i yws = _mm_srli_epi16(yw, 4);
            __m128i cr0 = _mm_mulhi_epi16(cr_const0, crw);
            __m128i cb0 = _mm_mulhi_epi16(cb_const0, cbw);
            __m128i cb1 = _mm_mulhi_epi16(cbw, cb_const1);
            __m128i cr1 = _mm_mulhi_epi16(crw, cr_const1);
            __m128i rws = _mm_add_epi16(cr0, yws);
            __m128i gwt = _mm_add_epi16(cb0, yws);
            __m128i bws = _mm_add_epi16(yws, cb1);
            __m128i gws = _mm_add_epi16(gwt, cr1);

            __m128i rw = _mm_srai_epi16(rws, 4);
            __m128i bw = _mm_srai_epi16(bws, 4);
            __m128i gw = _mm_srai_epi16(gws, 4);

            __m128i brb = _mm_packus_epi16(rw, bw);
            __m128i gxb = _mm_packus_epi16(gw, xw);

            __m128i t0 = _mm_unpacklo_epi8(brb, gxb);
            __m128i t1 = _mm_unpackhi_epi8(brb, gxb);
            __m128i o0 = _mm_unpacklo_epi16(t0, t1);
            __m128i o1 = _mm_unpackhi_epi16(t0, t1);

            _mm_storeu_si128((__m128i*) (out + 0), o0);
            _mm_storeu_si128((__m128i*) (out + 16), o1);
            out += 32;
        }
    }
    for (; i < count; ++i) {
        int y_fixed = (y[i] << 20) + (1 << 19);
        int r, g, b;
        int cr = pcr[i] - 128;
        int cb = pcb[i] - 128;
        r = y_fixed + cr * (((int)((1.40200f) * 4096.0f + 0.5f)) << 8);
        g = y_fixed + cr * -(((int)((0.71414f) * 4096.0f + 0.5f)) << 8) + ((cb * -(((int)((0.34414f) * 4096.0f + 0.5f)) << 8)) & 0xffff0000);
        b = y_fixed + cb * (((int)((1.77200f) * 4096.0f + 0.5f)) << 8);
        r >>= 20;
        g >>= 20;
        b >>= 20;
        if ((unsigned)r > 255) { if (r < 0) r = 0; else r = 255; }
        if ((unsigned)g > 255) { if (g < 0) g = 0; else g = 255; }
        if ((unsigned)b > 255) { if (b < 0) b = 0; else b = 255; }
        out[0] = (stbi_uc)r;
        out[1] = (stbi_uc)g;
        out[2] = (stbi_uc)b;
        out[3] = 255;
        out += step;
    }
}

static void stbi__setup_jpeg(stbi__jpeg* j)
{
    j->idct_block_kernel = stbi__idct_block;
    j->YCbCr_to_RGB_kernel = stbi__YCbCr_to_RGB_row;
    j->resample_row_hv_2_kernel = stbi__resample_row_hv_2;

    if (stbi__sse2_available()) {
        j->idct_block_kernel = stbi__idct_simd;
        j->YCbCr_to_RGB_kernel = stbi__YCbCr_to_RGB_simd;
        j->resample_row_hv_2_kernel = stbi__resample_row_hv_2_simd;
    }

}

static void stbi__cleanup_jpeg(stbi__jpeg* j)
{
    stbi__free_jpeg_components(j, j->s->img_n, 0);
}

typedef struct
{
    resample_row_func resample;
    stbi_uc* line0, * line1;
    int hs, vs;
    int w_lores;
    int ystep;
    int ypos;
} stbi__resample;

static stbi_uc stbi__blinn_8x8(stbi_uc x, stbi_uc y)
{
    unsigned int t = x * y + 128;
    return (stbi_uc)((t + (t >> 8)) >> 8);
}

static stbi_uc* load_jpeg_image(stbi__jpeg* z, int* out_x, int* out_y, int* comp, int req_comp)
{
    int n, decode_n, is_rgb;
    z->s->img_n = 0;

    if (req_comp < 0 || req_comp > 4) return ((unsigned char*)(size_t)(stbi__err("bad req_comp") ? ((void*)0) : ((void*)0)));

    if (!stbi__decode_jpeg_image(z)) { stbi__cleanup_jpeg(z); return ((void*)0); }

    n = req_comp ? req_comp : z->s->img_n >= 3 ? 3 : 1;

    is_rgb = z->s->img_n == 3 && (z->rgb == 3 || (z->app14_color_transform == 0 && !z->jfif));

    if (z->s->img_n == 3 && n < 3 && !is_rgb)
        decode_n = 1;
    else
        decode_n = z->s->img_n;

    {
        int k;
        unsigned int i, j;
        stbi_uc* output;
        stbi_uc* coutput[4] = { ((void*)0), ((void*)0), ((void*)0), ((void*)0) };

        stbi__resample res_comp[4];

        for (k = 0; k < decode_n; ++k) {
            stbi__resample* r = &res_comp[k];

            z->img_comp[k].linebuf = (stbi_uc*)stbi__malloc(z->s->img_x + 3);
            if (!z->img_comp[k].linebuf) { stbi__cleanup_jpeg(z); return ((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0))); }

            r->hs = z->img_h_max / z->img_comp[k].h;
            r->vs = z->img_v_max / z->img_comp[k].v;
            r->ystep = r->vs >> 1;
            r->w_lores = (z->s->img_x + r->hs - 1) / r->hs;
            r->ypos = 0;
            r->line0 = r->line1 = z->img_comp[k].data;

            if (r->hs == 1 && r->vs == 1) r->resample = resample_row_1;
            else if (r->hs == 1 && r->vs == 2) r->resample = stbi__resample_row_v_2;
            else if (r->hs == 2 && r->vs == 1) r->resample = stbi__resample_row_h_2;
            else if (r->hs == 2 && r->vs == 2) r->resample = z->resample_row_hv_2_kernel;
            else r->resample = stbi__resample_row_generic;
        }

        output = (stbi_uc*)stbi__malloc_mad3(n, z->s->img_x, z->s->img_y, 1);
        if (!output) { stbi__cleanup_jpeg(z); return ((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0))); }

        for (j = 0; j < z->s->img_y; ++j) {
            stbi_uc* out = output + n * z->s->img_x * j;
            for (k = 0; k < decode_n; ++k) {
                stbi__resample* r = &res_comp[k];
                int y_bot = r->ystep >= (r->vs >> 1);
                coutput[k] = r->resample(z->img_comp[k].linebuf,
                    y_bot ? r->line1 : r->line0,
                    y_bot ? r->line0 : r->line1,
                    r->w_lores, r->hs);
                if (++r->ystep >= r->vs) {
                    r->ystep = 0;
                    r->line0 = r->line1;
                    if (++r->ypos < z->img_comp[k].y)
                        r->line1 += z->img_comp[k].w2;
                }
            }
            if (n >= 3) {
                stbi_uc* y = coutput[0];
                if (z->s->img_n == 3) {
                    if (is_rgb) {
                        for (i = 0; i < z->s->img_x; ++i) {
                            out[0] = y[i];
                            out[1] = coutput[1][i];
                            out[2] = coutput[2][i];
                            out[3] = 255;
                            out += n;
                        }
                    }
                    else {
                        z->YCbCr_to_RGB_kernel(out, y, coutput[1], coutput[2], z->s->img_x, n);
                    }
                }
                else if (z->s->img_n == 4) {
                    if (z->app14_color_transform == 0) {
                        for (i = 0; i < z->s->img_x; ++i) {
                            stbi_uc m = coutput[3][i];
                            out[0] = stbi__blinn_8x8(coutput[0][i], m);
                            out[1] = stbi__blinn_8x8(coutput[1][i], m);
                            out[2] = stbi__blinn_8x8(coutput[2][i], m);
                            out[3] = 255;
                            out += n;
                        }
                    }
                    else if (z->app14_color_transform == 2) {
                        z->YCbCr_to_RGB_kernel(out, y, coutput[1], coutput[2], z->s->img_x, n);
                        for (i = 0; i < z->s->img_x; ++i) {
                            stbi_uc m = coutput[3][i];
                            out[0] = stbi__blinn_8x8(255 - out[0], m);
                            out[1] = stbi__blinn_8x8(255 - out[1], m);
                            out[2] = stbi__blinn_8x8(255 - out[2], m);
                            out += n;
                        }
                    }
                    else {
                        z->YCbCr_to_RGB_kernel(out, y, coutput[1], coutput[2], z->s->img_x, n);
                    }
                }
                else
                    for (i = 0; i < z->s->img_x; ++i) {
                        out[0] = out[1] = out[2] = y[i];
                        out[3] = 255;
                        out += n;
                    }
            }
            else {
                if (is_rgb) {
                    if (n == 1)
                        for (i = 0; i < z->s->img_x; ++i)
                            *out++ = stbi__compute_y(coutput[0][i], coutput[1][i], coutput[2][i]);
                    else {
                        for (i = 0; i < z->s->img_x; ++i, out += 2) {
                            out[0] = stbi__compute_y(coutput[0][i], coutput[1][i], coutput[2][i]);
                            out[1] = 255;
                        }
                    }
                }
                else if (z->s->img_n == 4 && z->app14_color_transform == 0) {
                    for (i = 0; i < z->s->img_x; ++i) {
                        stbi_uc m = coutput[3][i];
                        stbi_uc r = stbi__blinn_8x8(coutput[0][i], m);
                        stbi_uc g = stbi__blinn_8x8(coutput[1][i], m);
                        stbi_uc b = stbi__blinn_8x8(coutput[2][i], m);
                        out[0] = stbi__compute_y(r, g, b);
                        out[1] = 255;
                        out += n;
                    }
                }
                else if (z->s->img_n == 4 && z->app14_color_transform == 2) {
                    for (i = 0; i < z->s->img_x; ++i) {
                        out[0] = stbi__blinn_8x8(255 - coutput[0][i], coutput[3][i]);
                        out[1] = 255;
                        out += n;
                    }
                }
                else {
                    stbi_uc* y = coutput[0];
                    if (n == 1)
                        for (i = 0; i < z->s->img_x; ++i) out[i] = y[i];
                    else
                        for (i = 0; i < z->s->img_x; ++i) { *out++ = y[i]; *out++ = 255; }
                }
            }
        }
        stbi__cleanup_jpeg(z);
        *out_x = z->s->img_x;
        *out_y = z->s->img_y;
        if (comp) *comp = z->s->img_n >= 3 ? 3 : 1;
        return output;
    }
}

static void* stbi__jpeg_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
{
    unsigned char* result;
    stbi__jpeg* j = (stbi__jpeg*)stbi__malloc(sizeof(stbi__jpeg));
    (void)sizeof(ri);
    j->s = s;
    stbi__setup_jpeg(j);
    result = load_jpeg_image(j, x, y, comp, req_comp);
    free(j);
    return result;
}

static int stbi__jpeg_test(stbi__context* s)
{
    int r;
    stbi__jpeg* j = (stbi__jpeg*)stbi__malloc(sizeof(stbi__jpeg));
    j->s = s;
    stbi__setup_jpeg(j);
    r = stbi__decode_jpeg_header(j, STBI__SCAN_type);
    stbi__rewind(s);
    free(j);
    return r;
}

static int stbi__jpeg_info_raw(stbi__jpeg* j, int* x, int* y, int* comp)
{
    if (!stbi__decode_jpeg_header(j, STBI__SCAN_header)) {
        stbi__rewind(j->s);
        return 0;
    }
    if (x) *x = j->s->img_x;
    if (y) *y = j->s->img_y;
    if (comp) *comp = j->s->img_n >= 3 ? 3 : 1;
    return 1;
}

static int stbi__jpeg_info(stbi__context* s, int* x, int* y, int* comp)
{
    int result;
    stbi__jpeg* j = (stbi__jpeg*)(stbi__malloc(sizeof(stbi__jpeg)));
    j->s = s;
    result = stbi__jpeg_info_raw(j, x, y, comp);
    free(j);
    return result;
}
typedef struct
{
    stbi__uint16 fast[1 << 9];
    stbi__uint16 firstcode[16];
    int maxcode[17];
    stbi__uint16 firstsymbol[16];
    stbi_uc size[288];
    stbi__uint16 value[288];
} stbi__zhuffman;

static int stbi__bitreverse16(int n)
{
    n = ((n & 0xAAAA) >> 1) | ((n & 0x5555) << 1);
    n = ((n & 0xCCCC) >> 2) | ((n & 0x3333) << 2);
    n = ((n & 0xF0F0) >> 4) | ((n & 0x0F0F) << 4);
    n = ((n & 0xFF00) >> 8) | ((n & 0x00FF) << 8);
    return n;
}

static int stbi__bit_reverse(int v, int bits)
{
    ((void) sizeof((bits <= 16) ? 1 : 0), __extension__({ if (bits <= 16); else __assert_fail("bits <= 16", "./example.c", 3995, __extension__ __PRETTY_FUNCTION__); }));

    return stbi__bitreverse16(v) >> (16 - bits);
}

static int stbi__zbuild_huffman(stbi__zhuffman* z, const stbi_uc* sizelist, int num)
{
    int i, k = 0;
    int code, next_code[16], sizes[17];

    memset(sizes, 0, sizeof(sizes));
    memset(z->fast, 0, sizeof(z->fast));
    for (i = 0; i < num; ++i)
        ++sizes[sizelist[i]];
    sizes[0] = 0;
    for (i = 1; i < 16; ++i)
        if (sizes[i] > (1 << i))
            return stbi__err("bad sizes");
    code = 0;
    for (i = 1; i < 16; ++i) {
        next_code[i] = code;
        z->firstcode[i] = (stbi__uint16)code;
        z->firstsymbol[i] = (stbi__uint16)k;
        code = (code + sizes[i]);
        if (sizes[i])
            if (code - 1 >= (1 << i)) return stbi__err("bad codelengths");
        z->maxcode[i] = code << (16 - i);
        code <<= 1;
        k += sizes[i];
    }
    z->maxcode[16] = 0x10000;
    for (i = 0; i < num; ++i) {
        int s = sizelist[i];
        if (s) {
            int c = next_code[s] - z->firstcode[s] + z->firstsymbol[s];
            stbi__uint16 fastv = (stbi__uint16)((s << 9) | i);
            z->size[c] = (stbi_uc)s;
            z->value[c] = (stbi__uint16)i;
            if (s <= 9) {
                int j = stbi__bit_reverse(next_code[s], s);
                while (j < (1 << 9)) {
                    z->fast[j] = fastv;
                    j += (1 << s);
                }
            }
            ++next_code[s];
        }
    }
    return 1;
}

typedef struct
{
    stbi_uc* zbuffer, * zbuffer_end;
    int num_bits;
    stbi__uint32 code_buffer;

    char* zout;
    char* zout_start;
    char* zout_end;
    int z_expandable;

    stbi__zhuffman z_length, z_distance;
} stbi__zbuf;

static stbi_uc stbi__zget8(stbi__zbuf* z)
{
    if (z->zbuffer >= z->zbuffer_end) return 0;
    return *z->zbuffer++;
}

static void stbi__fill_bits(stbi__zbuf* z)
{
    do {
        ((void) sizeof((z->code_buffer < (1U << z->num_bits)) ? 1 : 0), __extension__({ if (z->code_buffer < (1U << z->num_bits)); else __assert_fail("z->code_buffer < (1U << z->num_bits)", "./example.c", 4077, __extension__ __PRETTY_FUNCTION__); }));
        z->code_buffer |= (unsigned int)stbi__zget8(z) << z->num_bits;
        z->num_bits += 8;
    } while (z->num_bits <= 24);
}

static unsigned int stbi__zreceive(stbi__zbuf* z, int n)
{
    unsigned int k;
    if (z->num_bits < n) stbi__fill_bits(z);
    k = z->code_buffer & ((1 << n) - 1);
    z->code_buffer >>= n;
    z->num_bits -= n;
    return k;
}

static int stbi__zhuffman_decode_slowpath(stbi__zbuf* a, stbi__zhuffman* z)
{
    int b, s, k;

    k = stbi__bit_reverse(a->code_buffer, 16);
    for (s = 9 + 1; ; ++s)
        if (k < z->maxcode[s])
            break;
    if (s == 16) return -1;

    b = (k >> (16 - s)) - z->firstcode[s] + z->firstsymbol[s];
    ((void) sizeof((z->size[b] == s) ? 1 : 0), __extension__({ if (z->size[b] == s); else __assert_fail("z->size[b] == s", "./example.c", 4105, __extension__ __PRETTY_FUNCTION__); }));
    a->code_buffer >>= s;
    a->num_bits -= s;
    return z->value[b];
}

static int stbi__zhuffman_decode(stbi__zbuf* a, stbi__zhuffman* z)
{
    int b, s;
    if (a->num_bits < 16) stbi__fill_bits(a);
    b = z->fast[a->code_buffer & ((1 << 9) - 1)];
    if (b) {
        s = b >> 9;
        a->code_buffer >>= s;
        a->num_bits -= s;
        return b & 511;
    }
    return stbi__zhuffman_decode_slowpath(a, z);
}

static int stbi__zexpand(stbi__zbuf* z, char* zout, int n)
{
    char* q;
    int cur, limit, old_limit;
    z->zout = zout;
    if (!z->z_expandable) return stbi__err("output buffer limit");
    cur = (int)(z->zout - z->zout_start);
    limit = old_limit = (int)(z->zout_end - z->zout_start);
    while (cur + n > limit)
        limit *= 2;
    q = (char*)realloc(z->zout_start, limit);
    (void)sizeof(old_limit);
    if (q == ((void*)0)) return stbi__err("outofmem");
    z->zout_start = q;
    z->zout = q + cur;
    z->zout_end = q + limit;
    return 1;
}

static const int stbi__zlength_base[31] = {
   3,4,5,6,7,8,9,10,11,13,
   15,17,19,23,27,31,35,43,51,59,
   67,83,99,115,131,163,195,227,258,0,0 };

static const int stbi__zlength_extra[31] =
{ 0,0,0,0,0,0,0,0,1,1,1,1,2,2,2,2,3,3,3,3,4,4,4,4,5,5,5,5,0,0,0 };

static const int stbi__zdist_base[32] = { 1,2,3,4,5,7,9,13,17,25,33,49,65,97,129,193,
257,385,513,769,1025,1537,2049,3073,4097,6145,8193,12289,16385,24577,0,0 };

static const int stbi__zdist_extra[32] =
{ 0,0,0,0,1,1,2,2,3,3,4,4,5,5,6,6,7,7,8,8,9,9,10,10,11,11,12,12,13,13 };

static int stbi__parse_huffman_block(stbi__zbuf* a)
{
    char* zout = a->zout;
    for (;;) {
        int z = stbi__zhuffman_decode(a, &a->z_length);
        if (z < 256) {
            if (z < 0) return stbi__err("bad huffman code");
            if (zout >= a->zout_end) {
                if (!stbi__zexpand(a, zout, 1)) return 0;
                zout = a->zout;
            }
            *zout++ = (char)z;
        }
        else {
            stbi_uc* p;
            int len, dist;
            if (z == 256) {
                a->zout = zout;
                return 1;
            }
            z -= 257;
            len = stbi__zlength_base[z];
            if (stbi__zlength_extra[z]) len += stbi__zreceive(a, stbi__zlength_extra[z]);
            z = stbi__zhuffman_decode(a, &a->z_distance);
            if (z < 0) return stbi__err("bad huffman code");
            dist = stbi__zdist_base[z];
            if (stbi__zdist_extra[z]) dist += stbi__zreceive(a, stbi__zdist_extra[z]);
            if (zout - a->zout_start < dist) return stbi__err("bad dist");
            if (zout + len > a->zout_end) {
                if (!stbi__zexpand(a, zout, len)) return 0;
                zout = a->zout;
            }
            p = (stbi_uc*)(zout - dist);
            if (dist == 1) {
                stbi_uc v = *p;
                if (len) { do *zout++ = v; while (--len); }
            }
            else {
                if (len) { do *zout++ = *p++; while (--len); }
            }
        }
    }
}

static int stbi__compute_huffman_codes(stbi__zbuf* a)
{
    static const stbi_uc length_dezigzag[19] = { 16,17,18,0,8,7,9,6,10,5,11,4,12,3,13,2,14,1,15 };
    stbi__zhuffman z_codelength;
    stbi_uc lencodes[286 + 32 + 137];
    stbi_uc codelength_sizes[19];
    int i, n;

    int hlit = stbi__zreceive(a, 5) + 257;
    int hdist = stbi__zreceive(a, 5) + 1;
    int hclen = stbi__zreceive(a, 4) + 4;
    int ntot = hlit + hdist;

    memset(codelength_sizes, 0, sizeof(codelength_sizes));
    for (i = 0; i < hclen; ++i) {
        int s = stbi__zreceive(a, 3);
        codelength_sizes[length_dezigzag[i]] = (stbi_uc)s;
    }
    if (!stbi__zbuild_huffman(&z_codelength, codelength_sizes, 19)) return 0;

    n = 0;
    while (n < ntot) {
        int c = stbi__zhuffman_decode(a, &z_codelength);
        if (c < 0 || c >= 19) return stbi__err("bad codelengths");
        if (c < 16)
            lencodes[n++] = (stbi_uc)c;
        else {
            stbi_uc fill = 0;
            if (c == 16) {
                c = stbi__zreceive(a, 2) + 3;
                if (n == 0) return stbi__err("bad codelengths");
                fill = lencodes[n - 1];
            }
            else if (c == 17)
                c = stbi__zreceive(a, 3) + 3;
            else {
                ((void) sizeof((c == 18) ? 1 : 0), __extension__({ if (c == 18); else __assert_fail("c == 18", "./example.c", 4238, __extension__ __PRETTY_FUNCTION__); }));
                c = stbi__zreceive(a, 7) + 11;
            }
            if (ntot - n < c) return stbi__err("bad codelengths");
            memset(lencodes + n, fill, c);
            n += c;
        }
    }
    if (n != ntot) return stbi__err("bad codelengths");
    if (!stbi__zbuild_huffman(&a->z_length, lencodes, hlit)) return 0;
    if (!stbi__zbuild_huffman(&a->z_distance, lencodes + hlit, hdist)) return 0;
    return 1;
}

static int stbi__parse_uncompressed_block(stbi__zbuf* a)
{
    stbi_uc header[4];
    int len, nlen, k;
    if (a->num_bits & 7)
        stbi__zreceive(a, a->num_bits & 7);

    k = 0;
    while (a->num_bits > 0) {
        header[k++] = (stbi_uc)(a->code_buffer & 255);
        a->code_buffer >>= 8;
        a->num_bits -= 8;
    }
    ((void) sizeof((a->num_bits == 0) ? 1 : 0), __extension__({ if (a->num_bits == 0); else __assert_fail("a->num_bits == 0", "./example.c", 4265, __extension__ __PRETTY_FUNCTION__); }));

    while (k < 4)
        header[k++] = stbi__zget8(a);
    len = header[1] * 256 + header[0];
    nlen = header[3] * 256 + header[2];
    if (nlen != (len ^ 0xffff)) return stbi__err("zlib corrupt");
    if (a->zbuffer + len > a->zbuffer_end) return stbi__err("read past buffer");
    if (a->zout + len > a->zout_end)
        if (!stbi__zexpand(a, a->zout, len)) return 0;
    memcpy(a->zout, a->zbuffer, len);
    a->zbuffer += len;
    a->zout += len;
    return 1;
}

static int stbi__parse_zlib_header(stbi__zbuf* a)
{
    int cmf = stbi__zget8(a);
    int cm = cmf & 15;

    int flg = stbi__zget8(a);
    if ((cmf * 256 + flg) % 31 != 0) return stbi__err("bad zlib header");
    if (flg & 32) return stbi__err("no preset dict");
    if (cm != 8) return stbi__err("bad compression");

    return 1;
}

static const stbi_uc stbi__zdefault_length[288] =
{
   8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8, 8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
   8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8, 8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
   8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8, 8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
   8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8, 8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
   8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8, 9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,
   9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9, 9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,
   9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9, 9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,
   9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9, 9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,
   7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7, 7,7,7,7,7,7,7,7,8,8,8,8,8,8,8,8
};
static const stbi_uc stbi__zdefault_distance[32] =
{
   5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5
};
static int stbi__parse_zlib(stbi__zbuf* a, int parse_header)
{
    int final, type;
    if (parse_header)
        if (!stbi__parse_zlib_header(a)) return 0;
    a->num_bits = 0;
    a->code_buffer = 0;
    do {
        final = stbi__zreceive(a, 1);
        type = stbi__zreceive(a, 2);
        if (type == 0) {
            if (!stbi__parse_uncompressed_block(a)) return 0;
        }
        else if (type == 3) {
            return 0;
        }
        else {
            if (type == 1) {

                if (!stbi__zbuild_huffman(&a->z_length, stbi__zdefault_length, 288)) return 0;
                if (!stbi__zbuild_huffman(&a->z_distance, stbi__zdefault_distance, 32)) return 0;
            }
            else {
                if (!stbi__compute_huffman_codes(a)) return 0;
            }
            if (!stbi__parse_huffman_block(a)) return 0;
        }
    } while (!final);
    return 1;
}

static int stbi__do_zlib(stbi__zbuf* a, char* obuf, int olen, int exp, int parse_header)
{
    a->zout_start = obuf;
    a->zout = obuf;
    a->zout_end = obuf + olen;
    a->z_expandable = exp;

    return stbi__parse_zlib(a, parse_header);
}

extern char* stbi_zlib_decode_malloc_guesssize(const char* buffer, int len, int initial_size, int* outlen)
{
    stbi__zbuf a;
    char* p = (char*)stbi__malloc(initial_size);
    if (p == ((void*)0)) return ((void*)0);
    a.zbuffer = (stbi_uc*)buffer;
    a.zbuffer_end = (stbi_uc*)buffer + len;
    if (stbi__do_zlib(&a, p, initial_size, 1, 1)) {
        if (outlen) *outlen = (int)(a.zout - a.zout_start);
        return a.zout_start;
    }
    else {
        free(a.zout_start);
        return ((void*)0);
    }
}

extern char* stbi_zlib_decode_malloc(char const* buffer, int len, int* outlen)
{
    return stbi_zlib_decode_malloc_guesssize(buffer, len, 16384, outlen);
}

extern char* stbi_zlib_decode_malloc_guesssize_headerflag(const char* buffer, int len, int initial_size, int* outlen, int parse_header)
{
    stbi__zbuf a;
    char* p = (char*)stbi__malloc(initial_size);
    if (p == ((void*)0)) return ((void*)0);
    a.zbuffer = (stbi_uc*)buffer;
    a.zbuffer_end = (stbi_uc*)buffer + len;
    if (stbi__do_zlib(&a, p, initial_size, 1, parse_header)) {
        if (outlen) *outlen = (int)(a.zout - a.zout_start);
        return a.zout_start;
    }
    else {
        free(a.zout_start);
        return ((void*)0);
    }
}

extern int stbi_zlib_decode_buffer(char* obuffer, int olen, char const* ibuffer, int ilen)
{
    stbi__zbuf a;
    a.zbuffer = (stbi_uc*)ibuffer;
    a.zbuffer_end = (stbi_uc*)ibuffer + ilen;
    if (stbi__do_zlib(&a, obuffer, olen, 0, 1))
        return (int)(a.zout - a.zout_start);
    else
        return -1;
}

extern char* stbi_zlib_decode_noheader_malloc(char const* buffer, int len, int* outlen)
{
    stbi__zbuf a;
    char* p = (char*)stbi__malloc(16384);
    if (p == ((void*)0)) return ((void*)0);
    a.zbuffer = (stbi_uc*)buffer;
    a.zbuffer_end = (stbi_uc*)buffer + len;
    if (stbi__do_zlib(&a, p, 16384, 1, 0)) {
        if (outlen) *outlen = (int)(a.zout - a.zout_start);
        return a.zout_start;
    }
    else {
        free(a.zout_start);
        return ((void*)0);
    }
}

extern int stbi_zlib_decode_noheader_buffer(char* obuffer, int olen, const char* ibuffer, int ilen)
{
    stbi__zbuf a;
    a.zbuffer = (stbi_uc*)ibuffer;
    a.zbuffer_end = (stbi_uc*)ibuffer + ilen;
    if (stbi__do_zlib(&a, obuffer, olen, 0, 0))
        return (int)(a.zout - a.zout_start);
    else
        return -1;
}
typedef struct
{
    stbi__uint32 length;
    stbi__uint32 type;
} stbi__pngchunk;

static stbi__pngchunk stbi__get_chunk_header(stbi__context* s)
{
    stbi__pngchunk c;
    c.length = stbi__get32be(s);
    c.type = stbi__get32be(s);
    return c;
}

static int stbi__check_png_header(stbi__context* s)
{
    static const stbi_uc png_sig[8] = { 137,80,78,71,13,10,26,10 };
    int i;
    for (i = 0; i < 8; ++i)
        if (stbi__get8(s) != png_sig[i]) return stbi__err("bad png sig");
    return 1;
}

typedef struct
{
    stbi__context* s;
    stbi_uc* idata, * expanded, * out;
    int depth;
} stbi__png;

enum {
    STBI__F_none = 0,
    STBI__F_sub = 1,
    STBI__F_up = 2,
    STBI__F_avg = 3,
    STBI__F_paeth = 4,

    STBI__F_avg_first,
    STBI__F_paeth_first
};

static stbi_uc first_row_filter[5] =
{
   STBI__F_none,
   STBI__F_sub,
   STBI__F_none,
   STBI__F_avg_first,
   STBI__F_paeth_first
};

static int stbi__paeth(int a, int b, int c)
{
    int p = a + b - c;
    int pa = abs(p - a);
    int pb = abs(p - b);
    int pc = abs(p - c);
    if (pa <= pb && pa <= pc) return a;
    if (pb <= pc) return b;
    return c;
}

static const stbi_uc stbi__depth_scale_table[9] = { 0, 0xff, 0x55, 0, 0x11, 0,0,0, 0x01 };

static int stbi__create_png_image_raw(stbi__png* a, stbi_uc* raw, stbi__uint32 raw_len, int out_n, stbi__uint32 x, stbi__uint32 y, int depth, int color)
{
    int bytes = (depth == 16 ? 2 : 1);
    stbi__context* s = a->s;
    stbi__uint32 i, j, stride = x * out_n * bytes;
    stbi__uint32 img_len, img_width_bytes;
    int k;
    int img_n = s->img_n;

    int output_bytes = out_n * bytes;
    int filter_bytes = img_n * bytes;
    int width = x;

    ((void) sizeof((out_n == s->img_n || out_n == s->img_n + 1) ? 1 : 0), __extension__({ if (out_n == s->img_n || out_n == s->img_n + 1); else __assert_fail("out_n == s->img_n || out_n == s->img_n + 1", "./example.c", 4532, __extension__ __PRETTY_FUNCTION__); }));
    a->out = (stbi_uc*)stbi__malloc_mad3(x, y, output_bytes, 0);
    if (!a->out) return stbi__err("outofmem");

    if (!stbi__mad3sizes_valid(img_n, x, depth, 7)) return stbi__err("too large");
    img_width_bytes = (((img_n * x * depth) + 7) >> 3);
    img_len = (img_width_bytes + 1) * y;

    if (raw_len < img_len) return stbi__err("not enough pixels");

    for (j = 0; j < y; ++j) {
        stbi_uc* cur = a->out + stride * j;
        stbi_uc* prior;
        int filter = *raw++;

        if (filter > 4)
            return stbi__err("invalid filter");

        if (depth < 8) {
            ((void) sizeof((img_width_bytes <= x) ? 1 : 0), __extension__({ if (img_width_bytes <= x); else __assert_fail("img_width_bytes <= x", "./example.c", 4554, __extension__ __PRETTY_FUNCTION__); }));
            cur += x * out_n - img_width_bytes;
            filter_bytes = 1;
            width = img_width_bytes;
        }
        prior = cur - stride;

        if (j == 0) filter = first_row_filter[filter];

        for (k = 0; k < filter_bytes; ++k) {
            switch (filter) {
            case STBI__F_none: cur[k] = raw[k]; break;
            case STBI__F_sub: cur[k] = raw[k]; break;
            case STBI__F_up: cur[k] = ((stbi_uc)((raw[k] + prior[k]) & 255)); break;
            case STBI__F_avg: cur[k] = ((stbi_uc)((raw[k] + (prior[k] >> 1)) & 255)); break;
            case STBI__F_paeth: cur[k] = ((stbi_uc)((raw[k] + stbi__paeth(0, prior[k], 0)) & 255)); break;
            case STBI__F_avg_first: cur[k] = raw[k]; break;
            case STBI__F_paeth_first: cur[k] = raw[k]; break;
            }
        }

        if (depth == 8) {
            if (img_n != out_n)
                cur[img_n] = 255;
            raw += img_n;
            cur += out_n;
            prior += out_n;
        }
        else if (depth == 16) {
            if (img_n != out_n) {
                cur[filter_bytes] = 255;
                cur[filter_bytes + 1] = 255;
            }
            raw += filter_bytes;
            cur += output_bytes;
            prior += output_bytes;
        }
        else {
            raw += 1;
            cur += 1;
            prior += 1;
        }

        if (depth < 8 || img_n == out_n) {
            int nk = (width - 1) * filter_bytes;

            switch (filter) {

            case STBI__F_none: memcpy(cur, raw, nk); break;
            case STBI__F_sub: for (k = 0; k < nk; ++k) { cur[k] = ((stbi_uc)((raw[k] + cur[k - filter_bytes]) & 255)); } break;
            case STBI__F_up: for (k = 0; k < nk; ++k) { cur[k] = ((stbi_uc)((raw[k] + prior[k]) & 255)); } break;
            case STBI__F_avg: for (k = 0; k < nk; ++k) { cur[k] = ((stbi_uc)((raw[k] + ((prior[k] + cur[k - filter_bytes]) >> 1)) & 255)); } break;
            case STBI__F_paeth: for (k = 0; k < nk; ++k) { cur[k] = ((stbi_uc)((raw[k] + stbi__paeth(cur[k - filter_bytes], prior[k], prior[k - filter_bytes])) & 255)); } break;
            case STBI__F_avg_first: for (k = 0; k < nk; ++k) { cur[k] = ((stbi_uc)((raw[k] + (cur[k - filter_bytes] >> 1)) & 255)); } break;
            case STBI__F_paeth_first: for (k = 0; k < nk; ++k) { cur[k] = ((stbi_uc)((raw[k] + stbi__paeth(cur[k - filter_bytes], 0, 0)) & 255)); } break;
            }

            raw += nk;
        }
        else {
            ((void) sizeof((img_n + 1 == out_n) ? 1 : 0), __extension__({ if (img_n + 1 == out_n); else __assert_fail("img_n + 1 == out_n", "./example.c", 4619, __extension__ __PRETTY_FUNCTION__); }));

            switch (filter) {
            case STBI__F_none: for (i = x - 1; i >= 1; --i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior += output_bytes) for (k = 0; k < filter_bytes; ++k) { cur[k] = raw[k]; } break;
            case STBI__F_sub: for (i = x - 1; i >= 1; --i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior += output_bytes) for (k = 0; k < filter_bytes; ++k) { cur[k] = ((stbi_uc)((raw[k] + cur[k - output_bytes]) & 255)); } break;
            case STBI__F_up: for (i = x - 1; i >= 1; --i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior += output_bytes) for (k = 0; k < filter_bytes; ++k) { cur[k] = ((stbi_uc)((raw[k] + prior[k]) & 255)); } break;
            case STBI__F_avg: for (i = x - 1; i >= 1; --i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior += output_bytes) for (k = 0; k < filter_bytes; ++k) { cur[k] = ((stbi_uc)((raw[k] + ((prior[k] + cur[k - output_bytes]) >> 1)) & 255)); } break;
            case STBI__F_paeth: for (i = x - 1; i >= 1; --i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior += output_bytes) for (k = 0; k < filter_bytes; ++k) { cur[k] = ((stbi_uc)((raw[k] + stbi__paeth(cur[k - output_bytes], prior[k], prior[k - output_bytes])) & 255)); } break;
            case STBI__F_avg_first: for (i = x - 1; i >= 1; --i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior += output_bytes) for (k = 0; k < filter_bytes; ++k) { cur[k] = ((stbi_uc)((raw[k] + (cur[k - output_bytes] >> 1)) & 255)); } break;
            case STBI__F_paeth_first: for (i = x - 1; i >= 1; --i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior += output_bytes) for (k = 0; k < filter_bytes; ++k) { cur[k] = ((stbi_uc)((raw[k] + stbi__paeth(cur[k - output_bytes], 0, 0)) & 255)); } break;
            }

            if (depth == 16) {
                cur = a->out + stride * j;
                for (i = 0; i < x; ++i, cur += output_bytes) {
                    cur[filter_bytes + 1] = 255;
                }
            }
        }
    }

    if (depth < 8) {
        for (j = 0; j < y; ++j) {
            stbi_uc* cur = a->out + stride * j;
            stbi_uc* in = a->out + stride * j + x * out_n - img_width_bytes;

            stbi_uc scale = (color == 0) ? stbi__depth_scale_table[depth] : 1;

            if (depth == 4) {
                for (k = x * img_n; k >= 2; k -= 2, ++in) {
                    *cur++ = scale * ((*in >> 4));
                    *cur++ = scale * ((*in) & 0x0f);
                }
                if (k > 0) *cur++ = scale * ((*in >> 4));
            }
            else if (depth == 2) {
                for (k = x * img_n; k >= 4; k -= 4, ++in) {
                    *cur++ = scale * ((*in >> 6));
                    *cur++ = scale * ((*in >> 4) & 0x03);
                    *cur++ = scale * ((*in >> 2) & 0x03);
                    *cur++ = scale * ((*in) & 0x03);
                }
                if (k > 0) *cur++ = scale * ((*in >> 6));
                if (k > 1) *cur++ = scale * ((*in >> 4) & 0x03);
                if (k > 2) *cur++ = scale * ((*in >> 2) & 0x03);
            }
            else if (depth == 1) {
                for (k = x * img_n; k >= 8; k -= 8, ++in) {
                    *cur++ = scale * ((*in >> 7));
                    *cur++ = scale * ((*in >> 6) & 0x01);
                    *cur++ = scale * ((*in >> 5) & 0x01);
                    *cur++ = scale * ((*in >> 4) & 0x01);
                    *cur++ = scale * ((*in >> 3) & 0x01);
                    *cur++ = scale * ((*in >> 2) & 0x01);
                    *cur++ = scale * ((*in >> 1) & 0x01);
                    *cur++ = scale * ((*in) & 0x01);
                }
                if (k > 0) *cur++ = scale * ((*in >> 7));
                if (k > 1) *cur++ = scale * ((*in >> 6) & 0x01);
                if (k > 2) *cur++ = scale * ((*in >> 5) & 0x01);
                if (k > 3) *cur++ = scale * ((*in >> 4) & 0x01);
                if (k > 4) *cur++ = scale * ((*in >> 3) & 0x01);
                if (k > 5) *cur++ = scale * ((*in >> 2) & 0x01);
                if (k > 6) *cur++ = scale * ((*in >> 1) & 0x01);
            }
            if (img_n != out_n) {
                int q;

                cur = a->out + stride * j;
                if (img_n == 1) {
                    for (q = x - 1; q >= 0; --q) {
                        cur[q * 2 + 1] = 255;
                        cur[q * 2 + 0] = cur[q];
                    }
                }
                else {
                    ((void) sizeof((img_n == 3) ? 1 : 0), __extension__({ if (img_n == 3); else __assert_fail("img_n == 3", "./example.c", 4711, __extension__ __PRETTY_FUNCTION__); }));
                    for (q = x - 1; q >= 0; --q) {
                        cur[q * 4 + 3] = 255;
                        cur[q * 4 + 2] = cur[q * 3 + 2];
                        cur[q * 4 + 1] = cur[q * 3 + 1];
                        cur[q * 4 + 0] = cur[q * 3 + 0];
                    }
                }
            }
        }
    }
    else if (depth == 16) {

        stbi_uc* cur = a->out;
        stbi__uint16* cur16 = (stbi__uint16*)cur;

        for (i = 0; i < x * y * out_n; ++i, cur16++, cur += 2) {
            *cur16 = (cur[0] << 8) | cur[1];
        }
    }

    return 1;
}

static int stbi__create_png_image(stbi__png* a, stbi_uc* image_data, stbi__uint32 image_data_len, int out_n, int depth, int color, int interlaced)
{
    int bytes = (depth == 16 ? 2 : 1);
    int out_bytes = out_n * bytes;
    stbi_uc* final;
    int p;
    if (!interlaced)
        return stbi__create_png_image_raw(a, image_data, image_data_len, out_n, a->s->img_x, a->s->img_y, depth, color);

    final = (stbi_uc*)stbi__malloc_mad3(a->s->img_x, a->s->img_y, out_bytes, 0);
    for (p = 0; p < 7; ++p) {
        int xorig[] = { 0,4,0,2,0,1,0 };
        int yorig[] = { 0,0,4,0,2,0,1 };
        int xspc[] = { 8,8,4,4,2,2,1 };
        int yspc[] = { 8,8,8,4,4,2,2 };
        int i, j, x, y;

        x = (a->s->img_x - xorig[p] + xspc[p] - 1) / xspc[p];
        y = (a->s->img_y - yorig[p] + yspc[p] - 1) / yspc[p];
        if (x && y) {
            stbi__uint32 img_len = ((((a->s->img_n * x * depth) + 7) >> 3) + 1)* y;
            if (!stbi__create_png_image_raw(a, image_data, image_data_len, out_n, x, y, depth, color)) {
                free(final);
                return 0;
            }
            for (j = 0; j < y; ++j) {
                for (i = 0; i < x; ++i) {
                    int out_y = j * yspc[p] + yorig[p];
                    int out_x = i * xspc[p] + xorig[p];
                    memcpy(final + out_y * a->s->img_x * out_bytes + out_x * out_bytes,
                        a->out + (j * x + i) * out_bytes, out_bytes);
                }
            }
            free(a->out);
            image_data += img_len;
            image_data_len -= img_len;
        }
    }
    a->out = final;

    return 1;
}

static int stbi__compute_transparency(stbi__png* z, stbi_uc tc[3], int out_n)
{
    stbi__context* s = z->s;
    stbi__uint32 i, pixel_count = s->img_x * s->img_y;
    stbi_uc* p = z->out;

    ((void) sizeof((out_n == 2 || out_n == 4) ? 1 : 0), __extension__({ if (out_n == 2 || out_n == 4); else __assert_fail("out_n == 2 || out_n == 4", "./example.c", 4790, __extension__ __PRETTY_FUNCTION__); }));

    if (out_n == 2) {
        for (i = 0; i < pixel_count; ++i) {
            p[1] = (p[0] == tc[0] ? 0 : 255);
            p += 2;
        }
    }
    else {
        for (i = 0; i < pixel_count; ++i) {
            if (p[0] == tc[0] && p[1] == tc[1] && p[2] == tc[2])
                p[3] = 0;
            p += 4;
        }
    }
    return 1;
}

static int stbi__compute_transparency16(stbi__png* z, stbi__uint16 tc[3], int out_n)
{
    stbi__context* s = z->s;
    stbi__uint32 i, pixel_count = s->img_x * s->img_y;
    stbi__uint16* p = (stbi__uint16*)z->out;

    ((void) sizeof((out_n == 2 || out_n == 4) ? 1 : 0), __extension__({ if (out_n == 2 || out_n == 4); else __assert_fail("out_n == 2 || out_n == 4", "./example.c", 4816, __extension__ __PRETTY_FUNCTION__); }));

    if (out_n == 2) {
        for (i = 0; i < pixel_count; ++i) {
            p[1] = (p[0] == tc[0] ? 0 : 65535);
            p += 2;
        }
    }
    else {
        for (i = 0; i < pixel_count; ++i) {
            if (p[0] == tc[0] && p[1] == tc[1] && p[2] == tc[2])
                p[3] = 0;
            p += 4;
        }
    }
    return 1;
}

static int stbi__expand_png_palette(stbi__png* a, stbi_uc* palette, int len, int pal_img_n)
{
    stbi__uint32 i, pixel_count = a->s->img_x * a->s->img_y;
    stbi_uc* p, * temp_out, * orig = a->out;

    p = (stbi_uc*)stbi__malloc_mad2(pixel_count, pal_img_n, 0);
    if (p == ((void*)0)) return stbi__err("outofmem");

    temp_out = p;

    if (pal_img_n == 3) {
        for (i = 0; i < pixel_count; ++i) {
            int n = orig[i] * 4;
            p[0] = palette[n];
            p[1] = palette[n + 1];
            p[2] = palette[n + 2];
            p += 3;
        }
    }
    else {
        for (i = 0; i < pixel_count; ++i) {
            int n = orig[i] * 4;
            p[0] = palette[n];
            p[1] = palette[n + 1];
            p[2] = palette[n + 2];
            p[3] = palette[n + 3];
            p += 4;
        }
    }
    free(a->out);
    a->out = temp_out;

    (void)sizeof(len);

    return 1;
}

static int stbi__unpremultiply_on_load = 0;
static int stbi__de_iphone_flag = 0;

extern void stbi_set_unpremultiply_on_load(int flag_true_if_should_unpremultiply)
{
    stbi__unpremultiply_on_load = flag_true_if_should_unpremultiply;
}

extern void stbi_convert_iphone_png_to_rgb(int flag_true_if_should_convert)
{
    stbi__de_iphone_flag = flag_true_if_should_convert;
}

static void stbi__de_iphone(stbi__png* z)
{
    stbi__context* s = z->s;
    stbi__uint32 i, pixel_count = s->img_x * s->img_y;
    stbi_uc* p = z->out;

    if (s->img_out_n == 3) {
        for (i = 0; i < pixel_count; ++i) {
            stbi_uc t = p[0];
            p[0] = p[2];
            p[2] = t;
            p += 3;
        }
    }
    else {
        ((void) sizeof((s->img_out_n == 4) ? 1 : 0), __extension__({ if (s->img_out_n == 4); else __assert_fail("s->img_out_n == 4", "./example.c", 4900, __extension__ __PRETTY_FUNCTION__); }));
        if (stbi__unpremultiply_on_load) {

            for (i = 0; i < pixel_count; ++i) {
                stbi_uc a = p[3];
                stbi_uc t = p[0];
                if (a) {
                    stbi_uc half = a / 2;
                    p[0] = (p[2] * 255 + half) / a;
                    p[1] = (p[1] * 255 + half) / a;
                    p[2] = (t * 255 + half) / a;
                }
                else {
                    p[0] = p[2];
                    p[2] = t;
                }
                p += 4;
            }
        }
        else {

            for (i = 0; i < pixel_count; ++i) {
                stbi_uc t = p[0];
                p[0] = p[2];
                p[2] = t;
                p += 4;
            }
        }
    }
}

static int stbi__parse_png_file(stbi__png* z, int scan, int req_comp)
{
    stbi_uc palette[1024], pal_img_n = 0;
    stbi_uc has_trans = 0, tc[3] = { 0 };
    stbi__uint16 tc16[3];
    stbi__uint32 ioff = 0, idata_limit = 0, i, pal_len = 0;
    int first = 1, k, interlace = 0, color = 0, is_iphone = 0;
    stbi__context* s = z->s;

    z->expanded = ((void*)0);
    z->idata = ((void*)0);
    z->out = ((void*)0);

    if (!stbi__check_png_header(s)) return 0;

    if (scan == STBI__SCAN_type) return 1;

    for (;;) {
        stbi__pngchunk c = stbi__get_chunk_header(s);
        switch (c.type) {
        case (((unsigned)('C') << 24) + ((unsigned)('g') << 16) + ((unsigned)('B') << 8) + (unsigned)('I')):
            is_iphone = 1;
            stbi__skip(s, c.length);
            break;
        case (((unsigned)('I') << 24) + ((unsigned)('H') << 16) + ((unsigned)('D') << 8) + (unsigned)('R')): {
            int comp, filter;
            if (!first) return stbi__err("multiple IHDR");
            first = 0;
            if (c.length != 13) return stbi__err("bad IHDR len");
            s->img_x = stbi__get32be(s); if (s->img_x > (1 << 24)) return stbi__err("too large");
            s->img_y = stbi__get32be(s); if (s->img_y > (1 << 24)) return stbi__err("too large");
            z->depth = stbi__get8(s); if (z->depth != 1 && z->depth != 2 && z->depth != 4 && z->depth != 8 && z->depth != 16) return stbi__err("1/2/4/8/16-bit only");
            color = stbi__get8(s); if (color > 6) return stbi__err("bad ctype");
            if (color == 3 && z->depth == 16) return stbi__err("bad ctype");
            if (color == 3) pal_img_n = 3; else if (color & 1) return stbi__err("bad ctype");
            comp = stbi__get8(s); if (comp) return stbi__err("bad comp method");
            filter = stbi__get8(s); if (filter) return stbi__err("bad filter method");
            interlace = stbi__get8(s); if (interlace > 1) return stbi__err("bad interlace method");
            if (!s->img_x || !s->img_y) return stbi__err("0-pixel image");
            if (!pal_img_n) {
                s->img_n = (color & 2 ? 3 : 1) + (color & 4 ? 1 : 0);
                if ((1 << 30) / s->img_x / s->img_n < s->img_y) return stbi__err("too large");
                if (scan == STBI__SCAN_header) return 1;
            }
            else {

                s->img_n = 1;
                if ((1 << 30) / s->img_x / 4 < s->img_y) return stbi__err("too large");

            }
            break;
        }

        case (((unsigned)('P') << 24) + ((unsigned)('L') << 16) + ((unsigned)('T') << 8) + (unsigned)('E')): {
            if (first) return stbi__err("first not IHDR");
            if (c.length > 256 * 3) return stbi__err("invalid PLTE");
            pal_len = c.length / 3;
            if (pal_len * 3 != c.length) return stbi__err("invalid PLTE");
            for (i = 0; i < pal_len; ++i) {
                palette[i * 4 + 0] = stbi__get8(s);
                palette[i * 4 + 1] = stbi__get8(s);
                palette[i * 4 + 2] = stbi__get8(s);
                palette[i * 4 + 3] = 255;
            }
            break;
        }

        case (((unsigned)('t') << 24) + ((unsigned)('R') << 16) + ((unsigned)('N') << 8) + (unsigned)('S')): {
            if (first) return stbi__err("first not IHDR");
            if (z->idata) return stbi__err("tRNS after IDAT");
            if (pal_img_n) {
                if (scan == STBI__SCAN_header) { s->img_n = 4; return 1; }
                if (pal_len == 0) return stbi__err("tRNS before PLTE");
                if (c.length > pal_len) return stbi__err("bad tRNS len");
                pal_img_n = 4;
                for (i = 0; i < c.length; ++i)
                    palette[i * 4 + 3] = stbi__get8(s);
            }
            else {
                if (!(s->img_n & 1)) return stbi__err("tRNS with alpha");
                if (c.length != (stbi__uint32)s->img_n * 2) return stbi__err("bad tRNS len");
                has_trans = 1;
                if (z->depth == 16) {
                    for (k = 0; k < s->img_n; ++k) tc16[k] = (stbi__uint16)stbi__get16be(s);
                }
                else {
                    for (k = 0; k < s->img_n; ++k) tc[k] = (stbi_uc)(stbi__get16be(s) & 255) * stbi__depth_scale_table[z->depth];
                }
            }
            break;
        }

        case (((unsigned)('I') << 24) + ((unsigned)('D') << 16) + ((unsigned)('A') << 8) + (unsigned)('T')): {
            if (first) return stbi__err("first not IHDR");
            if (pal_img_n && !pal_len) return stbi__err("no PLTE");
            if (scan == STBI__SCAN_header) { s->img_n = pal_img_n; return 1; }
            if ((int)(ioff + c.length) < (int)ioff) return 0;
            if (ioff + c.length > idata_limit) {
                stbi__uint32 idata_limit_old = idata_limit;
                stbi_uc* p;
                if (idata_limit == 0) idata_limit = c.length > 4096 ? c.length : 4096;
                while (ioff + c.length > idata_limit)
                    idata_limit *= 2;
                (void)sizeof(idata_limit_old);
                p = (stbi_uc*)realloc(z->idata, idata_limit); if (p == ((void*)0)) return stbi__err("outofmem");
                z->idata = p;
            }
            if (!stbi__getn(s, z->idata + ioff, c.length)) return stbi__err("outofdata");
            ioff += c.length;
            break;
        }

        case (((unsigned)('I') << 24) + ((unsigned)('E') << 16) + ((unsigned)('N') << 8) + (unsigned)('D')): {
            stbi__uint32 raw_len, bpl;
            if (first) return stbi__err("first not IHDR");
            if (scan != STBI__SCAN_load) return 1;
            if (z->idata == ((void*)0)) return stbi__err("no IDAT");

            bpl = (s->img_x * z->depth + 7) / 8;
            raw_len = bpl * s->img_y * s->img_n + s->img_y;
            z->expanded = (stbi_uc*)stbi_zlib_decode_malloc_guesssize_headerflag((char*)z->idata, ioff, raw_len, (int*)&raw_len, !is_iphone);
            if (z->expanded == ((void*)0)) return 0;
            free(z->idata); z->idata = ((void*)0);
            if ((req_comp == s->img_n + 1 && req_comp != 3 && !pal_img_n) || has_trans)
                s->img_out_n = s->img_n + 1;
            else
                s->img_out_n = s->img_n;
            if (!stbi__create_png_image(z, z->expanded, raw_len, s->img_out_n, z->depth, color, interlace)) return 0;
            if (has_trans) {
                if (z->depth == 16) {
                    if (!stbi__compute_transparency16(z, tc16, s->img_out_n)) return 0;
                }
                else {
                    if (!stbi__compute_transparency(z, tc, s->img_out_n)) return 0;
                }
            }
            if (is_iphone && stbi__de_iphone_flag && s->img_out_n > 2)
                stbi__de_iphone(z);
            if (pal_img_n) {

                s->img_n = pal_img_n;
                s->img_out_n = pal_img_n;
                if (req_comp >= 3) s->img_out_n = req_comp;
                if (!stbi__expand_png_palette(z, palette, pal_len, s->img_out_n))
                    return 0;
            }
            else if (has_trans) {

                ++s->img_n;
            }
            free(z->expanded); z->expanded = ((void*)0);

            stbi__get32be(s);
            return 1;
        }

        default:

            if (first) return stbi__err("first not IHDR");
            if ((c.type & (1 << 29)) == 0) {

                static char invalid_chunk[] = "XXXX PNG chunk not known";
                invalid_chunk[0] = ((stbi_uc)((c.type >> 24) & 255));
                invalid_chunk[1] = ((stbi_uc)((c.type >> 16) & 255));
                invalid_chunk[2] = ((stbi_uc)((c.type >> 8) & 255));
                invalid_chunk[3] = ((stbi_uc)((c.type >> 0) & 255));

                return stbi__err(invalid_chunk);
            }
            stbi__skip(s, c.length);
            break;
        }

        stbi__get32be(s);
    }
}

static void* stbi__do_png(stbi__png* p, int* x, int* y, int* n, int req_comp, stbi__result_info* ri)
{
    void* result = ((void*)0);
    if (req_comp < 0 || req_comp > 4) return ((unsigned char*)(size_t)(stbi__err("bad req_comp") ? ((void*)0) : ((void*)0)));
    if (stbi__parse_png_file(p, STBI__SCAN_load, req_comp)) {
        if (p->depth < 8)
            ri->bits_per_channel = 8;
        else
            ri->bits_per_channel = p->depth;
        result = p->out;
        p->out = ((void*)0);
        if (req_comp && req_comp != p->s->img_out_n) {
            if (ri->bits_per_channel == 8)
                result = stbi__convert_format((unsigned char*)result, p->s->img_out_n, req_comp, p->s->img_x, p->s->img_y);
            else
                result = stbi__convert_format16((stbi__uint16*)result, p->s->img_out_n, req_comp, p->s->img_x, p->s->img_y);
            p->s->img_out_n = req_comp;
            if (result == ((void*)0)) return result;
        }
        *x = p->s->img_x;
        *y = p->s->img_y;
        if (n) *n = p->s->img_n;
    }
    free(p->out); p->out = ((void*)0);
    free(p->expanded); p->expanded = ((void*)0);
    free(p->idata); p->idata = ((void*)0);

    return result;
}

static void* stbi__png_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
{
    stbi__png p;
    p.s = s;
    return stbi__do_png(&p, x, y, comp, req_comp, ri);
}

static int stbi__png_test(stbi__context* s)
{
    int r;
    r = stbi__check_png_header(s);
    stbi__rewind(s);
    return r;
}

static int stbi__png_info_raw(stbi__png* p, int* x, int* y, int* comp)
{
    if (!stbi__parse_png_file(p, STBI__SCAN_header, 0)) {
        stbi__rewind(p->s);
        return 0;
    }
    if (x) *x = p->s->img_x;
    if (y) *y = p->s->img_y;
    if (comp) *comp = p->s->img_n;
    return 1;
}

static int stbi__png_info(stbi__context* s, int* x, int* y, int* comp)
{
    stbi__png p;
    p.s = s;
    return stbi__png_info_raw(&p, x, y, comp);
}

static int stbi__png_is16(stbi__context* s)
{
    stbi__png p;
    p.s = s;
    if (!stbi__png_info_raw(&p, ((void*)0), ((void*)0), ((void*)0)))
        return 0;
    if (p.depth != 16) {
        stbi__rewind(p.s);
        return 0;
    }
    return 1;
}

static int stbi__bmp_test_raw(stbi__context* s)
{
    int r;
    int sz;
    if (stbi__get8(s) != 'B') return 0;
    if (stbi__get8(s) != 'M') return 0;
    stbi__get32le(s);
    stbi__get16le(s);
    stbi__get16le(s);
    stbi__get32le(s);
    sz = stbi__get32le(s);
    r = (sz == 12 || sz == 40 || sz == 56 || sz == 108 || sz == 124);
    return r;
}

static int stbi__bmp_test(stbi__context* s)
{
    int r = stbi__bmp_test_raw(s);
    stbi__rewind(s);
    return r;
}

static int stbi__high_bit(unsigned int z)
{
    int n = 0;
    if (z == 0) return -1;
    if (z >= 0x10000) { n += 16; z >>= 16; }
    if (z >= 0x00100) { n += 8; z >>= 8; }
    if (z >= 0x00010) { n += 4; z >>= 4; }
    if (z >= 0x00004) { n += 2; z >>= 2; }
    if (z >= 0x00002) { n += 1; }
    return n;
}

static int stbi__bitcount(unsigned int a)
{
    a = (a & 0x55555555) + ((a >> 1) & 0x55555555);
    a = (a & 0x33333333) + ((a >> 2) & 0x33333333);
    a = (a + (a >> 4)) & 0x0f0f0f0f;
    a = (a + (a >> 8));
    a = (a + (a >> 16));
    return a & 0xff;
}

static int stbi__shiftsigned(unsigned int v, int shift, int bits)
{
    static unsigned int mul_table[9] = {
       0,
       0xff , 0x55 , 0x49 , 0x11 ,
       0x21 , 0x41 , 0x81 , 0x01 ,
    };
    static unsigned int shift_table[9] = {
       0, 0,0,1,0,2,4,6,0,
    };
    if (shift < 0)
        v <<= -shift;
    else
        v >>= shift;
    ((void) sizeof((v < 256) ? 1 : 0), __extension__({ if (v < 256); else __assert_fail("v < 256", "./example.c", 5256, __extension__ __PRETTY_FUNCTION__); }));
    v >>= (8 - bits);
    ((void) sizeof((bits >= 0 && bits <= 8) ? 1 : 0), __extension__({ if (bits >= 0 && bits <= 8); else __assert_fail("bits >= 0 && bits <= 8", "./example.c", 5258, __extension__ __PRETTY_FUNCTION__); }));
    return (int)((unsigned)v * mul_table[bits]) >> shift_table[bits];
}

typedef struct
{
    int bpp, offset, hsz;
    unsigned int mr, mg, mb, ma, all_a;
    int extra_read;
} stbi__bmp_data;

static void* stbi__bmp_parse_header(stbi__context* s, stbi__bmp_data* info)
{
    int hsz;
    if (stbi__get8(s) != 'B' || stbi__get8(s) != 'M') return ((unsigned char*)(size_t)(stbi__err("not BMP") ? ((void*)0) : ((void*)0)));
    stbi__get32le(s);
    stbi__get16le(s);
    stbi__get16le(s);
    info->offset = stbi__get32le(s);
    info->hsz = hsz = stbi__get32le(s);
    info->mr = info->mg = info->mb = info->ma = 0;
    info->extra_read = 14;

    if (hsz != 12 && hsz != 40 && hsz != 56 && hsz != 108 && hsz != 124) return ((unsigned char*)(size_t)(stbi__err("unknown BMP") ? ((void*)0) : ((void*)0)));
    if (hsz == 12) {
        s->img_x = stbi__get16le(s);
        s->img_y = stbi__get16le(s);
    }
    else {
        s->img_x = stbi__get32le(s);
        s->img_y = stbi__get32le(s);
    }
    if (stbi__get16le(s) != 1) return ((unsigned char*)(size_t)(stbi__err("bad BMP") ? ((void*)0) : ((void*)0)));
    info->bpp = stbi__get16le(s);
    if (hsz != 12) {
        int compress = stbi__get32le(s);
        if (compress == 1 || compress == 2) return ((unsigned char*)(size_t)(stbi__err("BMP RLE") ? ((void*)0) : ((void*)0)));
        stbi__get32le(s);
        stbi__get32le(s);
        stbi__get32le(s);
        stbi__get32le(s);
        stbi__get32le(s);
        if (hsz == 40 || hsz == 56) {
            if (hsz == 56) {
                stbi__get32le(s);
                stbi__get32le(s);
                stbi__get32le(s);
                stbi__get32le(s);
            }
            if (info->bpp == 16 || info->bpp == 32) {
                if (compress == 0) {
                    if (info->bpp == 32) {
                        info->mr = 0xffu << 16;
                        info->mg = 0xffu << 8;
                        info->mb = 0xffu << 0;
                        info->ma = 0xffu << 24;
                        info->all_a = 0;
                    }
                    else {
                        info->mr = 31u << 10;
                        info->mg = 31u << 5;
                        info->mb = 31u << 0;
                    }
                }
                else if (compress == 3) {
                    info->mr = stbi__get32le(s);
                    info->mg = stbi__get32le(s);
                    info->mb = stbi__get32le(s);
                    info->extra_read += 12;

                    if (info->mr == info->mg && info->mg == info->mb) {

                        return ((unsigned char*)(size_t)(stbi__err("bad BMP") ? ((void*)0) : ((void*)0)));
                    }
                }
                else
                    return ((unsigned char*)(size_t)(stbi__err("bad BMP") ? ((void*)0) : ((void*)0)));
            }
        }
        else {
            int i;
            if (hsz != 108 && hsz != 124)
                return ((unsigned char*)(size_t)(stbi__err("bad BMP") ? ((void*)0) : ((void*)0)));
            info->mr = stbi__get32le(s);
            info->mg = stbi__get32le(s);
            info->mb = stbi__get32le(s);
            info->ma = stbi__get32le(s);
            stbi__get32le(s);
            for (i = 0; i < 12; ++i)
                stbi__get32le(s);
            if (hsz == 124) {
                stbi__get32le(s);
                stbi__get32le(s);
                stbi__get32le(s);
                stbi__get32le(s);
            }
        }
    }
    return (void*)1;
}

static void* stbi__bmp_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
{
    stbi_uc* out;
    unsigned int mr = 0, mg = 0, mb = 0, ma = 0, all_a;
    stbi_uc pal[256][4];
    int psize = 0, i, j, width;
    int flip_vertically, pad, target;
    stbi__bmp_data info;
    (void)sizeof(ri);

    info.all_a = 255;
    if (stbi__bmp_parse_header(s, &info) == ((void*)0))
        return ((void*)0);

    flip_vertically = ((int)s->img_y) > 0;
    s->img_y = abs((int)s->img_y);

    mr = info.mr;
    mg = info.mg;
    mb = info.mb;
    ma = info.ma;
    all_a = info.all_a;

    if (info.hsz == 12) {
        if (info.bpp < 24)
            psize = (info.offset - info.extra_read - 24) / 3;
    }
    else {
        if (info.bpp < 16)
            psize = (info.offset - info.extra_read - info.hsz) >> 2;
    }
    if (psize == 0) {
        ((void) sizeof((info.offset == (s->img_buffer - s->buffer_start)) ? 1 : 0), __extension__({ if (info.offset == (s->img_buffer - s->buffer_start)); else __assert_fail("info.offset == (s->img_buffer - s->buffer_start)", "./example.c", 5392, __extension__ __PRETTY_FUNCTION__); }));
    }

    if (info.bpp == 24 && ma == 0xff000000)
        s->img_n = 3;
    else
        s->img_n = ma ? 4 : 3;
    if (req_comp && req_comp >= 3)
        target = req_comp;
    else
        target = s->img_n;

    if (!stbi__mad3sizes_valid(target, s->img_x, s->img_y, 0))
        return ((unsigned char*)(size_t)(stbi__err("too large") ? ((void*)0) : ((void*)0)));

    out = (stbi_uc*)stbi__malloc_mad3(target, s->img_x, s->img_y, 0);
    if (!out) return ((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));
    if (info.bpp < 16) {
        int z = 0;
        if (psize == 0 || psize > 256) { free(out); return ((unsigned char*)(size_t)(stbi__err("invalid") ? ((void*)0) : ((void*)0))); }
        for (i = 0; i < psize; ++i) {
            pal[i][2] = stbi__get8(s);
            pal[i][1] = stbi__get8(s);
            pal[i][0] = stbi__get8(s);
            if (info.hsz != 12) stbi__get8(s);
            pal[i][3] = 255;
        }
        stbi__skip(s, info.offset - info.extra_read - info.hsz - psize * (info.hsz == 12 ? 3 : 4));
        if (info.bpp == 1) width = (s->img_x + 7) >> 3;
        else if (info.bpp == 4) width = (s->img_x + 1) >> 1;
        else if (info.bpp == 8) width = s->img_x;
        else { free(out); return ((unsigned char*)(size_t)(stbi__err("bad bpp") ? ((void*)0) : ((void*)0))); }
        pad = (-width) & 3;
        if (info.bpp == 1) {
            for (j = 0; j < (int)s->img_y; ++j) {
                int bit_offset = 7, v = stbi__get8(s);
                for (i = 0; i < (int)s->img_x; ++i) {
                    int color = (v >> bit_offset) & 0x1;
                    out[z++] = pal[color][0];
                    out[z++] = pal[color][1];
                    out[z++] = pal[color][2];
                    if (target == 4) out[z++] = 255;
                    if (i + 1 == (int)s->img_x) break;
                    if ((--bit_offset) < 0) {
                        bit_offset = 7;
                        v = stbi__get8(s);
                    }
                }
                stbi__skip(s, pad);
            }
        }
        else {
            for (j = 0; j < (int)s->img_y; ++j) {
                for (i = 0; i < (int)s->img_x; i += 2) {
                    int v = stbi__get8(s), v2 = 0;
                    if (info.bpp == 4) {
                        v2 = v & 15;
                        v >>= 4;
                    }
                    out[z++] = pal[v][0];
                    out[z++] = pal[v][1];
                    out[z++] = pal[v][2];
                    if (target == 4) out[z++] = 255;
                    if (i + 1 == (int)s->img_x) break;
                    v = (info.bpp == 8) ? stbi__get8(s) : v2;
                    out[z++] = pal[v][0];
                    out[z++] = pal[v][1];
                    out[z++] = pal[v][2];
                    if (target == 4) out[z++] = 255;
                }
                stbi__skip(s, pad);
            }
        }
    }
    else {
        int rshift = 0, gshift = 0, bshift = 0, ashift = 0, rcount = 0, gcount = 0, bcount = 0, acount = 0;
        int z = 0;
        int easy = 0;
        stbi__skip(s, info.offset - info.extra_read - info.hsz);
        if (info.bpp == 24) width = 3 * s->img_x;
        else if (info.bpp == 16) width = 2 * s->img_x;
        else width = 0;
        pad = (-width) & 3;
        if (info.bpp == 24) {
            easy = 1;
        }
        else if (info.bpp == 32) {
            if (mb == 0xff && mg == 0xff00 && mr == 0x00ff0000 && ma == 0xff000000)
                easy = 2;
        }
        if (!easy) {
            if (!mr || !mg || !mb) { free(out); return ((unsigned char*)(size_t)(stbi__err("bad masks") ? ((void*)0) : ((void*)0))); }

            rshift = stbi__high_bit(mr) - 7; rcount = stbi__bitcount(mr);
            gshift = stbi__high_bit(mg) - 7; gcount = stbi__bitcount(mg);
            bshift = stbi__high_bit(mb) - 7; bcount = stbi__bitcount(mb);
            ashift = stbi__high_bit(ma) - 7; acount = stbi__bitcount(ma);
        }
        for (j = 0; j < (int)s->img_y; ++j) {
            if (easy) {
                for (i = 0; i < (int)s->img_x; ++i) {
                    unsigned char a;
                    out[z + 2] = stbi__get8(s);
                    out[z + 1] = stbi__get8(s);
                    out[z + 0] = stbi__get8(s);
                    z += 3;
                    a = (easy == 2 ? stbi__get8(s) : 255);
                    all_a |= a;
                    if (target == 4) out[z++] = a;
                }
            }
            else {
                int bpp = info.bpp;
                for (i = 0; i < (int)s->img_x; ++i) {
                    stbi__uint32 v = (bpp == 16 ? (stbi__uint32)stbi__get16le(s) : stbi__get32le(s));
                    unsigned int a;
                    out[z++] = ((stbi_uc)((stbi__shiftsigned(v & mr, rshift, rcount)) & 255));
                    out[z++] = ((stbi_uc)((stbi__shiftsigned(v & mg, gshift, gcount)) & 255));
                    out[z++] = ((stbi_uc)((stbi__shiftsigned(v & mb, bshift, bcount)) & 255));
                    a = (ma ? stbi__shiftsigned(v & ma, ashift, acount) : 255);
                    all_a |= a;
                    if (target == 4) out[z++] = ((stbi_uc)((a) & 255));
                }
            }
            stbi__skip(s, pad);
        }
    }

    if (target == 4 && all_a == 0)
        for (i = 4 * s->img_x * s->img_y - 1; i >= 0; i -= 4)
            out[i] = 255;

    if (flip_vertically) {
        stbi_uc t;
        for (j = 0; j < (int)s->img_y >> 1; ++j) {
            stbi_uc* p1 = out + j * s->img_x * target;
            stbi_uc* p2 = out + (s->img_y - 1 - j) * s->img_x * target;
            for (i = 0; i < (int)s->img_x * target; ++i) {
                t = p1[i]; p1[i] = p2[i]; p2[i] = t;
            }
        }
    }

    if (req_comp && req_comp != target) {
        out = stbi__convert_format(out, target, req_comp, s->img_x, s->img_y);
        if (out == ((void*)0)) return out;
    }

    *x = s->img_x;
    *y = s->img_y;
    if (comp) *comp = s->img_n;
    return out;
}

static int stbi__tga_get_comp(int bits_per_pixel, int is_grey, int* is_rgb16)
{

    if (is_rgb16) *is_rgb16 = 0;
    switch (bits_per_pixel) {
    case 8: return STBI_grey;
    case 16: if (is_grey) return STBI_grey_alpha;

    case 15: if (is_rgb16) *is_rgb16 = 1;
        return STBI_rgb;
    case 24:
    case 32: return bits_per_pixel / 8;
    default: return 0;
    }
}

static int stbi__tga_info(stbi__context* s, int* x, int* y, int* comp)
{
    int tga_w, tga_h, tga_comp, tga_image_type, tga_bits_per_pixel, tga_colormap_bpp;
    int sz, tga_colormap_type;
    stbi__get8(s);
    tga_colormap_type = stbi__get8(s);
    if (tga_colormap_type > 1) {
        stbi__rewind(s);
        return 0;
    }
    tga_image_type = stbi__get8(s);
    if (tga_colormap_type == 1) {
        if (tga_image_type != 1 && tga_image_type != 9) {
            stbi__rewind(s);
            return 0;
        }
        stbi__skip(s, 4);
        sz = stbi__get8(s);
        if ((sz != 8) && (sz != 15) && (sz != 16) && (sz != 24) && (sz != 32)) {
            stbi__rewind(s);
            return 0;
        }
        stbi__skip(s, 4);
        tga_colormap_bpp = sz;
    }
    else {
        if ((tga_image_type != 2) && (tga_image_type != 3) && (tga_image_type != 10) && (tga_image_type != 11)) {
            stbi__rewind(s);
            return 0;
        }
        stbi__skip(s, 9);
        tga_colormap_bpp = 0;
    }
    tga_w = stbi__get16le(s);
    if (tga_w < 1) {
        stbi__rewind(s);
        return 0;
    }
    tga_h = stbi__get16le(s);
    if (tga_h < 1) {
        stbi__rewind(s);
        return 0;
    }
    tga_bits_per_pixel = stbi__get8(s);
    stbi__get8(s);
    if (tga_colormap_bpp != 0) {
        if ((tga_bits_per_pixel != 8) && (tga_bits_per_pixel != 16)) {

            stbi__rewind(s);
            return 0;
        }
        tga_comp = stbi__tga_get_comp(tga_colormap_bpp, 0, ((void*)0));
    }
    else {
        tga_comp = stbi__tga_get_comp(tga_bits_per_pixel, (tga_image_type == 3) || (tga_image_type == 11), ((void*)0));
    }
    if (!tga_comp) {
        stbi__rewind(s);
        return 0;
    }
    if (x) *x = tga_w;
    if (y) *y = tga_h;
    if (comp) *comp = tga_comp;
    return 1;
}

static int stbi__tga_test(stbi__context* s)
{
    int res = 0;
    int sz, tga_color_type;
    stbi__get8(s);
    tga_color_type = stbi__get8(s);
    if (tga_color_type > 1) goto errorEnd;
    sz = stbi__get8(s);
    if (tga_color_type == 1) {
        if (sz != 1 && sz != 9) goto errorEnd;
        stbi__skip(s, 4);
        sz = stbi__get8(s);
        if ((sz != 8) && (sz != 15) && (sz != 16) && (sz != 24) && (sz != 32)) goto errorEnd;
        stbi__skip(s, 4);
    }
    else {
        if ((sz != 2) && (sz != 3) && (sz != 10) && (sz != 11)) goto errorEnd;
        stbi__skip(s, 9);
    }
    if (stbi__get16le(s) < 1) goto errorEnd;
    if (stbi__get16le(s) < 1) goto errorEnd;
    sz = stbi__get8(s);
    if ((tga_color_type == 1) && (sz != 8) && (sz != 16)) goto errorEnd;
    if ((sz != 8) && (sz != 15) && (sz != 16) && (sz != 24) && (sz != 32)) goto errorEnd;

    res = 1;

errorEnd:
    stbi__rewind(s);
    return res;
}

static void stbi__tga_read_rgb16(stbi__context* s, stbi_uc* out)
{
    stbi__uint16 px = (stbi__uint16)stbi__get16le(s);
    stbi__uint16 fiveBitMask = 31;

    int r = (px >> 10)& fiveBitMask;
    int g = (px >> 5)& fiveBitMask;
    int b = px & fiveBitMask;

    out[0] = (stbi_uc)((r * 255) / 31);
    out[1] = (stbi_uc)((g * 255) / 31);
    out[2] = (stbi_uc)((b * 255) / 31);

}

static void* stbi__tga_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
{

    int tga_offset = stbi__get8(s);
    int tga_indexed = stbi__get8(s);
    int tga_image_type = stbi__get8(s);
    int tga_is_RLE = 0;
    int tga_palette_start = stbi__get16le(s);
    int tga_palette_len = stbi__get16le(s);
    int tga_palette_bits = stbi__get8(s);
    int tga_x_origin = stbi__get16le(s);
    int tga_y_origin = stbi__get16le(s);
    int tga_width = stbi__get16le(s);
    int tga_height = stbi__get16le(s);
    int tga_bits_per_pixel = stbi__get8(s);
    int tga_comp, tga_rgb16 = 0;
    int tga_inverted = stbi__get8(s);

    unsigned char* tga_data;
    unsigned char* tga_palette = ((void*)0);
    int i, j;
    unsigned char raw_data[4] = { 0 };
    int RLE_count = 0;
    int RLE_repeating = 0;
    int read_next_pixel = 1;
    (void)sizeof(ri);
    (void)sizeof(tga_x_origin);
    (void)sizeof(tga_y_origin);

    if (tga_image_type >= 8)
    {
        tga_image_type -= 8;
        tga_is_RLE = 1;
    }
    tga_inverted = 1 - ((tga_inverted >> 5) & 1);

    if (tga_indexed) tga_comp = stbi__tga_get_comp(tga_palette_bits, 0, &tga_rgb16);
    else tga_comp = stbi__tga_get_comp(tga_bits_per_pixel, (tga_image_type == 3), &tga_rgb16);

    if (!tga_comp)
        return ((unsigned char*)(size_t)(stbi__err("bad format") ? ((void*)0) : ((void*)0)));

    *x = tga_width;
    *y = tga_height;
    if (comp) *comp = tga_comp;

    if (!stbi__mad3sizes_valid(tga_width, tga_height, tga_comp, 0))
        return ((unsigned char*)(size_t)(stbi__err("too large") ? ((void*)0) : ((void*)0)));

    tga_data = (unsigned char*)stbi__malloc_mad3(tga_width, tga_height, tga_comp, 0);
    if (!tga_data) return ((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));

    stbi__skip(s, tga_offset);

    if (!tga_indexed && !tga_is_RLE && !tga_rgb16) {
        for (i = 0; i < tga_height; ++i) {
            int row = tga_inverted ? tga_height - i - 1 : i;
            stbi_uc* tga_row = tga_data + row * tga_width * tga_comp;
            stbi__getn(s, tga_row, tga_width * tga_comp);
        }
    }
    else {

        if (tga_indexed)
        {

            stbi__skip(s, tga_palette_start);

            tga_palette = (unsigned char*)stbi__malloc_mad2(tga_palette_len, tga_comp, 0);
            if (!tga_palette) {
                free(tga_data);
                return ((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));
            }
            if (tga_rgb16) {
                stbi_uc* pal_entry = tga_palette;
                ((void) sizeof((tga_comp == STBI_rgb) ? 1 : 0), __extension__({ if (tga_comp == STBI_rgb); else __assert_fail("tga_comp == STBI_rgb", "./example.c", 5768, __extension__ __PRETTY_FUNCTION__); }));
                for (i = 0; i < tga_palette_len; ++i) {
                    stbi__tga_read_rgb16(s, pal_entry);
                    pal_entry += tga_comp;
                }
            }
            else if (!stbi__getn(s, tga_palette, tga_palette_len * tga_comp)) {
                free(tga_data);
                free(tga_palette);
                return ((unsigned char*)(size_t)(stbi__err("bad palette") ? ((void*)0) : ((void*)0)));
            }
        }

        for (i = 0; i < tga_width * tga_height; ++i)
        {

            if (tga_is_RLE)
            {
                if (RLE_count == 0)
                {

                    int RLE_cmd = stbi__get8(s);
                    RLE_count = 1 + (RLE_cmd & 127);
                    RLE_repeating = RLE_cmd >> 7;
                    read_next_pixel = 1;
                }
                else if (!RLE_repeating)
                {
                    read_next_pixel = 1;
                }
            }
            else
            {
                read_next_pixel = 1;
            }

            if (read_next_pixel)
            {

                if (tga_indexed)
                {

                    int pal_idx = (tga_bits_per_pixel == 8) ? stbi__get8(s) : stbi__get16le(s);
                    if (pal_idx >= tga_palette_len) {

                        pal_idx = 0;
                    }
                    pal_idx *= tga_comp;
                    for (j = 0; j < tga_comp; ++j) {
                        raw_data[j] = tga_palette[pal_idx + j];
                    }
                }
                else if (tga_rgb16) {
                    ((void) sizeof((tga_comp == STBI_rgb) ? 1 : 0), __extension__({ if (tga_comp == STBI_rgb); else __assert_fail("tga_comp == STBI_rgb", "./example.c", 5821, __extension__ __PRETTY_FUNCTION__); }));
                    stbi__tga_read_rgb16(s, raw_data);
                }
                else {

                    for (j = 0; j < tga_comp; ++j) {
                        raw_data[j] = stbi__get8(s);
                    }
                }

                read_next_pixel = 0;
            }

            for (j = 0; j < tga_comp; ++j)
                tga_data[i * tga_comp + j] = raw_data[j];

            --RLE_count;
        }

        if (tga_inverted)
        {
            for (j = 0; j * 2 < tga_height; ++j)
            {
                int index1 = j * tga_width * tga_comp;
                int index2 = (tga_height - 1 - j) * tga_width * tga_comp;
                for (i = tga_width * tga_comp; i > 0; --i)
                {
                    unsigned char temp = tga_data[index1];
                    tga_data[index1] = tga_data[index2];
                    tga_data[index2] = temp;
                    ++index1;
                    ++index2;
                }
            }
        }

        if (tga_palette != ((void*)0))
        {
            free(tga_palette);
        }
    }

    if (tga_comp >= 3 && !tga_rgb16)
    {
        unsigned char* tga_pixel = tga_data;
        for (i = 0; i < tga_width * tga_height; ++i)
        {
            unsigned char temp = tga_pixel[0];
            tga_pixel[0] = tga_pixel[2];
            tga_pixel[2] = temp;
            tga_pixel += tga_comp;
        }
    }

    if (req_comp && req_comp != tga_comp)
        tga_data = stbi__convert_format(tga_data, tga_comp, req_comp, tga_width, tga_height);

    tga_palette_start = tga_palette_len = tga_palette_bits =
        tga_x_origin = tga_y_origin = 0;
    (void)sizeof(tga_palette_start);

    return tga_data;
}

static int stbi__psd_test(stbi__context* s)
{
    int r = (stbi__get32be(s) == 0x38425053);
    stbi__rewind(s);
    return r;
}

static int stbi__psd_decode_rle(stbi__context* s, stbi_uc* p, int pixelCount)
{
    int count, nleft, len;

    count = 0;
    while ((nleft = pixelCount - count) > 0) {
        len = stbi__get8(s);
        if (len == 128) {

        }
        else if (len < 128) {

            len++;
            if (len > nleft) return 0;
            count += len;
            while (len) {
                *p = stbi__get8(s);
                p += 4;
                len--;
            }
        }
        else if (len > 128) {
            stbi_uc val;

            len = 257 - len;
            if (len > nleft) return 0;
            val = stbi__get8(s);
            count += len;
            while (len) {
                *p = val;
                p += 4;
                len--;
            }
        }
    }

    return 1;
}

static void* stbi__psd_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri, int bpc)
{
    int pixelCount;
    int channelCount, compression;
    int channel, i;
    int bitdepth;
    int w, h;
    stbi_uc* out;
    (void)sizeof(ri);

    if (stbi__get32be(s) != 0x38425053)
        return ((unsigned char*)(size_t)(stbi__err("not PSD") ? ((void*)0) : ((void*)0)));

    if (stbi__get16be(s) != 1)
        return ((unsigned char*)(size_t)(stbi__err("wrong version") ? ((void*)0) : ((void*)0)));

    stbi__skip(s, 6);

    channelCount = stbi__get16be(s);
    if (channelCount < 0 || channelCount > 16)
        return ((unsigned char*)(size_t)(stbi__err("wrong channel count") ? ((void*)0) : ((void*)0)));

    h = stbi__get32be(s);
    w = stbi__get32be(s);

    bitdepth = stbi__get16be(s);
    if (bitdepth != 8 && bitdepth != 16)
        return ((unsigned char*)(size_t)(stbi__err("unsupported bit depth") ? ((void*)0) : ((void*)0)));
    if (stbi__get16be(s) != 3)
        return ((unsigned char*)(size_t)(stbi__err("wrong color format") ? ((void*)0) : ((void*)0)));

    stbi__skip(s, stbi__get32be(s));

    stbi__skip(s, stbi__get32be(s));

    stbi__skip(s, stbi__get32be(s));

    compression = stbi__get16be(s);
    if (compression > 1)
        return ((unsigned char*)(size_t)(stbi__err("bad compression") ? ((void*)0) : ((void*)0)));

    if (!stbi__mad3sizes_valid(4, w, h, 0))
        return ((unsigned char*)(size_t)(stbi__err("too large") ? ((void*)0) : ((void*)0)));

    if (!compression && bitdepth == 16 && bpc == 16) {
        out = (stbi_uc*)stbi__malloc_mad3(8, w, h, 0);
        ri->bits_per_channel = 16;
    }
    else
        out = (stbi_uc*)stbi__malloc(4 * w * h);

    if (!out) return ((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));
    pixelCount = w * h;

    if (compression) {
        stbi__skip(s, h * channelCount * 2);

        for (channel = 0; channel < 4; channel++) {
            stbi_uc* p;

            p = out + channel;
            if (channel >= channelCount) {

                for (i = 0; i < pixelCount; i++, p += 4)
                    *p = (channel == 3 ? 255 : 0);
            }
            else {

                if (!stbi__psd_decode_rle(s, p, pixelCount)) {
                    free(out);
                    return ((unsigned char*)(size_t)(stbi__err("corrupt") ? ((void*)0) : ((void*)0)));
                }
            }
        }

    }
    else {

        for (channel = 0; channel < 4; channel++) {
            if (channel >= channelCount) {

                if (bitdepth == 16 && bpc == 16) {
                    stbi__uint16* q = ((stbi__uint16*)out) + channel;
                    stbi__uint16 val = channel == 3 ? 65535 : 0;
                    for (i = 0; i < pixelCount; i++, q += 4)
                        *q = val;
                }
                else {
                    stbi_uc* p = out + channel;
                    stbi_uc val = channel == 3 ? 255 : 0;
                    for (i = 0; i < pixelCount; i++, p += 4)
                        *p = val;
                }
            }
            else {
                if (ri->bits_per_channel == 16) {
                    stbi__uint16* q = ((stbi__uint16*)out) + channel;
                    for (i = 0; i < pixelCount; i++, q += 4)
                        *q = (stbi__uint16)stbi__get16be(s);
                }
                else {
                    stbi_uc* p = out + channel;
                    if (bitdepth == 16) {
                        for (i = 0; i < pixelCount; i++, p += 4)
                            *p = (stbi_uc)(stbi__get16be(s) >> 8);
                    }
                    else {
                        for (i = 0; i < pixelCount; i++, p += 4)
                            *p = stbi__get8(s);
                    }
                }
            }
        }
    }

    if (channelCount >= 4) {
        if (ri->bits_per_channel == 16) {
            for (i = 0; i < w * h; ++i) {
                stbi__uint16* pixel = (stbi__uint16*)out + 4 * i;
                if (pixel[3] != 0 && pixel[3] != 65535) {
                    float a = pixel[3] / 65535.0f;
                    float ra = 1.0f / a;
                    float inv_a = 65535.0f * (1 - ra);
                    pixel[0] = (stbi__uint16)(pixel[0] * ra + inv_a);
                    pixel[1] = (stbi__uint16)(pixel[1] * ra + inv_a);
                    pixel[2] = (stbi__uint16)(pixel[2] * ra + inv_a);
                }
            }
        }
        else {
            for (i = 0; i < w * h; ++i) {
                unsigned char* pixel = out + 4 * i;
                if (pixel[3] != 0 && pixel[3] != 255) {
                    float a = pixel[3] / 255.0f;
                    float ra = 1.0f / a;
                    float inv_a = 255.0f * (1 - ra);
                    pixel[0] = (unsigned char)(pixel[0] * ra + inv_a);
                    pixel[1] = (unsigned char)(pixel[1] * ra + inv_a);
                    pixel[2] = (unsigned char)(pixel[2] * ra + inv_a);
                }
            }
        }
    }

    if (req_comp && req_comp != 4) {
        if (ri->bits_per_channel == 16)
            out = (stbi_uc*)stbi__convert_format16((stbi__uint16*)out, 4, req_comp, w, h);
        else
            out = stbi__convert_format(out, 4, req_comp, w, h);
        if (out == ((void*)0)) return out;
    }

    if (comp) *comp = 4;
    *y = h;
    *x = w;

    return out;
}
static int stbi__pic_is4(stbi__context* s, const char* str)
{
    int i;
    for (i = 0; i < 4; ++i)
        if (stbi__get8(s) != (stbi_uc)str[i])
            return 0;

    return 1;
}

static int stbi__pic_test_core(stbi__context* s)
{
    int i;

    if (!stbi__pic_is4(s, "\x53\x80\xF6\x34"))
        return 0;

    for (i = 0; i < 84; ++i)
        stbi__get8(s);

    if (!stbi__pic_is4(s, "PICT"))
        return 0;

    return 1;
}

typedef struct
{
    stbi_uc size, type, channel;
} stbi__pic_packet;

static stbi_uc* stbi__readval(stbi__context* s, int channel, stbi_uc* dest)
{
    int mask = 0x80, i;

    for (i = 0; i < 4; ++i, mask >>= 1) {
        if (channel & mask) {
            if (stbi__at_eof(s)) return ((unsigned char*)(size_t)(stbi__err("bad file") ? ((void*)0) : ((void*)0)));
            dest[i] = stbi__get8(s);
        }
    }

    return dest;
}

static void stbi__copyval(int channel, stbi_uc* dest, const stbi_uc* src)
{
    int mask = 0x80, i;

    for (i = 0; i < 4; ++i, mask >>= 1)
        if (channel & mask)
            dest[i] = src[i];
}

static stbi_uc* stbi__pic_load_core(stbi__context* s, int width, int height, int* comp, stbi_uc* result)
{
    int act_comp = 0, num_packets = 0, y, chained;
    stbi__pic_packet packets[10];

    do {
        stbi__pic_packet* packet;

        if (num_packets == sizeof(packets) / sizeof(packets[0]))
            return ((unsigned char*)(size_t)(stbi__err("bad format") ? ((void*)0) : ((void*)0)));

        packet = &packets[num_packets++];

        chained = stbi__get8(s);
        packet->size = stbi__get8(s);
        packet->type = stbi__get8(s);
        packet->channel = stbi__get8(s);

        act_comp |= packet->channel;

        if (stbi__at_eof(s)) return ((unsigned char*)(size_t)(stbi__err("bad file") ? ((void*)0) : ((void*)0)));
        if (packet->size != 8) return ((unsigned char*)(size_t)(stbi__err("bad format") ? ((void*)0) : ((void*)0)));
    } while (chained);

    *comp = (act_comp & 0x10 ? 4 : 3);

    for (y = 0; y < height; ++y) {
        int packet_idx;

        for (packet_idx = 0; packet_idx < num_packets; ++packet_idx) {
            stbi__pic_packet* packet = &packets[packet_idx];
            stbi_uc* dest = result + y * width * 4;

            switch (packet->type) {
            default:
                return ((unsigned char*)(size_t)(stbi__err("bad format") ? ((void*)0) : ((void*)0)));

            case 0: {
                int x;

                for (x = 0; x < width; ++x, dest += 4)
                    if (!stbi__readval(s, packet->channel, dest))
                        return 0;
                break;
            }

            case 1:
            {
                int left = width, i;

                while (left > 0) {
                    stbi_uc count, value[4];

                    count = stbi__get8(s);
                    if (stbi__at_eof(s)) return ((unsigned char*)(size_t)(stbi__err("bad file") ? ((void*)0) : ((void*)0)));

                    if (count > left)
                        count = (stbi_uc)left;

                    if (!stbi__readval(s, packet->channel, value)) return 0;

                    for (i = 0; i < count; ++i, dest += 4)
                        stbi__copyval(packet->channel, dest, value);
                    left -= count;
                }
            }
            break;

            case 2: {
                int left = width;
                while (left > 0) {
                    int count = stbi__get8(s), i;
                    if (stbi__at_eof(s)) return ((unsigned char*)(size_t)(stbi__err("bad file") ? ((void*)0) : ((void*)0)));

                    if (count >= 128) {
                        stbi_uc value[4];

                        if (count == 128)
                            count = stbi__get16be(s);
                        else
                            count -= 127;
                        if (count > left)
                            return ((unsigned char*)(size_t)(stbi__err("bad file") ? ((void*)0) : ((void*)0)));

                        if (!stbi__readval(s, packet->channel, value))
                            return 0;

                        for (i = 0; i < count; ++i, dest += 4)
                            stbi__copyval(packet->channel, dest, value);
                    }
                    else {
                        ++count;
                        if (count > left) return ((unsigned char*)(size_t)(stbi__err("bad file") ? ((void*)0) : ((void*)0)));

                        for (i = 0; i < count; ++i, dest += 4)
                            if (!stbi__readval(s, packet->channel, dest))
                                return 0;
                    }
                    left -= count;
                }
                break;
            }
            }
        }
    }

    return result;
}

static void* stbi__pic_load(stbi__context* s, int* px, int* py, int* comp, int req_comp, stbi__result_info* ri)
{
    stbi_uc* result;
    int i, x, y, internal_comp;
    (void)sizeof(ri);

    if (!comp) comp = &internal_comp;

    for (i = 0; i < 92; ++i)
        stbi__get8(s);

    x = stbi__get16be(s);
    y = stbi__get16be(s);
    if (stbi__at_eof(s)) return ((unsigned char*)(size_t)(stbi__err("bad file") ? ((void*)0) : ((void*)0)));
    if (!stbi__mad3sizes_valid(x, y, 4, 0)) return ((unsigned char*)(size_t)(stbi__err("too large") ? ((void*)0) : ((void*)0)));

    stbi__get32be(s);
    stbi__get16be(s);
    stbi__get16be(s);

    result = (stbi_uc*)stbi__malloc_mad3(x, y, 4, 0);
    memset(result, 0xff, x * y * 4);

    if (!stbi__pic_load_core(s, x, y, comp, result)) {
        free(result);
        result = 0;
    }
    *px = x;
    *py = y;
    if (req_comp == 0) req_comp = *comp;
    result = stbi__convert_format(result, 4, req_comp, x, y);

    return result;
}

static int stbi__pic_test(stbi__context* s)
{
    int r = stbi__pic_test_core(s);
    stbi__rewind(s);
    return r;
}

typedef struct
{
    stbi__int16 prefix;
    stbi_uc first;
    stbi_uc suffix;
} stbi__gif_lzw;

typedef struct
{
    int w, h;
    stbi_uc* out;
    stbi_uc* background;
    stbi_uc* history;
    int flags, bgindex, ratio, transparent, eflags;
    stbi_uc pal[256][4];
    stbi_uc lpal[256][4];
    stbi__gif_lzw codes[8192];
    stbi_uc* color_table;
    int parse, step;
    int lflags;
    int start_x, start_y;
    int max_x, max_y;
    int cur_x, cur_y;
    int line_size;
    int delay;
} stbi__gif;

static int stbi__gif_test_raw(stbi__context* s)
{
    int sz;
    if (stbi__get8(s) != 'G' || stbi__get8(s) != 'I' || stbi__get8(s) != 'F' || stbi__get8(s) != '8') return 0;
    sz = stbi__get8(s);
    if (sz != '9' && sz != '7') return 0;
    if (stbi__get8(s) != 'a') return 0;
    return 1;
}

static int stbi__gif_test(stbi__context* s)
{
    int r = stbi__gif_test_raw(s);
    stbi__rewind(s);
    return r;
}

static void stbi__gif_parse_colortable(stbi__context* s, stbi_uc pal[256][4], int num_entries, int transp)
{
    int i;
    for (i = 0; i < num_entries; ++i) {
        pal[i][2] = stbi__get8(s);
        pal[i][1] = stbi__get8(s);
        pal[i][0] = stbi__get8(s);
        pal[i][3] = transp == i ? 0 : 255;
    }
}

static int stbi__gif_header(stbi__context* s, stbi__gif* g, int* comp, int is_info)
{
    stbi_uc version;
    if (stbi__get8(s) != 'G' || stbi__get8(s) != 'I' || stbi__get8(s) != 'F' || stbi__get8(s) != '8')
        return stbi__err("not GIF");

    version = stbi__get8(s);
    if (version != '7' && version != '9') return stbi__err("not GIF");
    if (stbi__get8(s) != 'a') return stbi__err("not GIF");

    stbi__g_failure_reason = "";
    g->w = stbi__get16le(s);
    g->h = stbi__get16le(s);
    g->flags = stbi__get8(s);
    g->bgindex = stbi__get8(s);
    g->ratio = stbi__get8(s);
    g->transparent = -1;

    if (comp != 0) *comp = 4;

    if (is_info) return 1;

    if (g->flags & 0x80)
        stbi__gif_parse_colortable(s, g->pal, 2 << (g->flags & 7), -1);

    return 1;
}

static int stbi__gif_info_raw(stbi__context* s, int* x, int* y, int* comp)
{
    stbi__gif* g = (stbi__gif*)stbi__malloc(sizeof(stbi__gif));
    if (!stbi__gif_header(s, g, comp, 1)) {
        free(g);
        stbi__rewind(s);
        return 0;
    }
    if (x) *x = g->w;
    if (y) *y = g->h;
    free(g);
    return 1;
}

static void stbi__out_gif_code(stbi__gif* g, stbi__uint16 code)
{
    stbi_uc* p, * c;
    int idx;

    if (g->codes[code].prefix >= 0)
        stbi__out_gif_code(g, g->codes[code].prefix);

    if (g->cur_y >= g->max_y) return;

    idx = g->cur_x + g->cur_y;
    p = &g->out[idx];
    g->history[idx / 4] = 1;

    c = &g->color_table[g->codes[code].suffix * 4];
    if (c[3] > 128) {
        p[0] = c[2];
        p[1] = c[1];
        p[2] = c[0];
        p[3] = c[3];
    }
    g->cur_x += 4;

    if (g->cur_x >= g->max_x) {
        g->cur_x = g->start_x;
        g->cur_y += g->step;

        while (g->cur_y >= g->max_y && g->parse > 0) {
            g->step = (1 << g->parse) * g->line_size;
            g->cur_y = g->start_y + (g->step >> 1);
            --g->parse;
        }
    }
}

static stbi_uc* stbi__process_gif_raster(stbi__context* s, stbi__gif* g)
{
    stbi_uc lzw_cs;
    stbi__int32 len, init_code;
    stbi__uint32 first;
    stbi__int32 codesize, codemask, avail, oldcode, bits, valid_bits, clear;
    stbi__gif_lzw* p;

    lzw_cs = stbi__get8(s);
    if (lzw_cs > 12) return ((void*)0);
    clear = 1 << lzw_cs;
    first = 1;
    codesize = lzw_cs + 1;
    codemask = (1 << codesize) - 1;
    bits = 0;
    valid_bits = 0;
    for (init_code = 0; init_code < clear; init_code++) {
        g->codes[init_code].prefix = -1;
        g->codes[init_code].first = (stbi_uc)init_code;
        g->codes[init_code].suffix = (stbi_uc)init_code;
    }

    avail = clear + 2;
    oldcode = -1;

    len = 0;
    for (;;) {
        if (valid_bits < codesize) {
            if (len == 0) {
                len = stbi__get8(s);
                if (len == 0)
                    return g->out;
            }
            --len;
            bits |= (stbi__int32)stbi__get8(s) << valid_bits;
            valid_bits += 8;
        }
        else {
            stbi__int32 code = bits & codemask;
            bits >>= codesize;
            valid_bits -= codesize;

            if (code == clear) {
                codesize = lzw_cs + 1;
                codemask = (1 << codesize) - 1;
                avail = clear + 2;
                oldcode = -1;
                first = 0;
            }
            else if (code == clear + 1) {
                stbi__skip(s, len);
                while ((len = stbi__get8(s)) > 0)
                    stbi__skip(s, len);
                return g->out;
            }
            else if (code <= avail) {
                if (first) {
                    return ((unsigned char*)(size_t)(stbi__err("no clear code") ? ((void*)0) : ((void*)0)));
                }

                if (oldcode >= 0) {
                    p = &g->codes[avail++];
                    if (avail > 8192) {
                        return ((unsigned char*)(size_t)(stbi__err("too many codes") ? ((void*)0) : ((void*)0)));
                    }

                    p->prefix = (stbi__int16)oldcode;
                    p->first = g->codes[oldcode].first;
                    p->suffix = (code == avail) ? p->first : g->codes[code].first;
                }
                else if (code == avail)
                    return ((unsigned char*)(size_t)(stbi__err("illegal code in raster") ? ((void*)0) : ((void*)0)));

                stbi__out_gif_code(g, (stbi__uint16)code);

                if ((avail & codemask) == 0 && avail <= 0x0FFF) {
                    codesize++;
                    codemask = (1 << codesize) - 1;
                }

                oldcode = code;
            }
            else {
                return ((unsigned char*)(size_t)(stbi__err("illegal code in raster") ? ((void*)0) : ((void*)0)));
            }
        }
    }
}

static stbi_uc* stbi__gif_load_next(stbi__context* s, stbi__gif* g, int* comp, int req_comp, stbi_uc* two_back)
{
    int dispose;
    int first_frame;
    int pi;
    int pcount;
    (void)sizeof(req_comp);

    first_frame = 0;
    if (g->out == 0) {
        if (!stbi__gif_header(s, g, comp, 0)) return 0;
        if (!stbi__mad3sizes_valid(4, g->w, g->h, 0))
            return ((unsigned char*)(size_t)(stbi__err("too large") ? ((void*)0) : ((void*)0)));
        pcount = g->w * g->h;
        g->out = (stbi_uc*)stbi__malloc(4 * pcount);
        g->background = (stbi_uc*)stbi__malloc(4 * pcount);
        g->history = (stbi_uc*)stbi__malloc(pcount);
        if (!g->out || !g->background || !g->history)
            return ((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));

        memset(g->out, 0x00, 4 * pcount);
        memset(g->background, 0x00, 4 * pcount);
        memset(g->history, 0x00, pcount);
        first_frame = 1;
    }
    else {

        dispose = (g->eflags & 0x1C) >> 2;
        pcount = g->w * g->h;

        if ((dispose == 3) && (two_back == 0)) {
            dispose = 2;
        }

        if (dispose == 3) {
            for (pi = 0; pi < pcount; ++pi) {
                if (g->history[pi]) {
                    memcpy(&g->out[pi * 4], &two_back[pi * 4], 4);
                }
            }
        }
        else if (dispose == 2) {

            for (pi = 0; pi < pcount; ++pi) {
                if (g->history[pi]) {
                    memcpy(&g->out[pi * 4], &g->background[pi * 4], 4);
                }
            }
        }
        else {

        }

        memcpy(g->background, g->out, 4 * g->w * g->h);
    }

    memset(g->history, 0x00, g->w * g->h);

    for (;;) {
        int tag = stbi__get8(s);
        switch (tag) {
        case 0x2C:
        {
            stbi__int32 x, y, w, h;
            stbi_uc* o;

            x = stbi__get16le(s);
            y = stbi__get16le(s);
            w = stbi__get16le(s);
            h = stbi__get16le(s);
            if (((x + w) > (g->w)) || ((y + h) > (g->h)))
                return ((unsigned char*)(size_t)(stbi__err("bad Image Descriptor") ? ((void*)0) : ((void*)0)));

            g->line_size = g->w * 4;
            g->start_x = x * 4;
            g->start_y = y * g->line_size;
            g->max_x = g->start_x + w * 4;
            g->max_y = g->start_y + h * g->line_size;
            g->cur_x = g->start_x;
            g->cur_y = g->start_y;

            if (w == 0)
                g->cur_y = g->max_y;

            g->lflags = stbi__get8(s);

            if (g->lflags & 0x40) {
                g->step = 8 * g->line_size;
                g->parse = 3;
            }
            else {
                g->step = g->line_size;
                g->parse = 0;
            }

            if (g->lflags & 0x80) {
                stbi__gif_parse_colortable(s, g->lpal, 2 << (g->lflags & 7), g->eflags & 0x01 ? g->transparent : -1);
                g->color_table = (stbi_uc*)g->lpal;
            }
            else if (g->flags & 0x80) {
                g->color_table = (stbi_uc*)g->pal;
            }
            else
                return ((unsigned char*)(size_t)(stbi__err("missing color table") ? ((void*)0) : ((void*)0)));

            o = stbi__process_gif_raster(s, g);
            if (!o) return ((void*)0);

            pcount = g->w * g->h;
            if (first_frame && (g->bgindex > 0)) {

                for (pi = 0; pi < pcount; ++pi) {
                    if (g->history[pi] == 0) {
                        g->pal[g->bgindex][3] = 255;
                        memcpy(&g->out[pi * 4], &g->pal[g->bgindex], 4);
                    }
                }
            }

            return o;
        }

        case 0x21:
        {
            int len;
            int ext = stbi__get8(s);
            if (ext == 0xF9) {
                len = stbi__get8(s);
                if (len == 4) {
                    g->eflags = stbi__get8(s);
                    g->delay = 10 * stbi__get16le(s);

                    if (g->transparent >= 0) {
                        g->pal[g->transparent][3] = 255;
                    }
                    if (g->eflags & 0x01) {
                        g->transparent = stbi__get8(s);
                        if (g->transparent >= 0) {
                            g->pal[g->transparent][3] = 0;
                        }
                    }
                    else {

                        stbi__skip(s, 1);
                        g->transparent = -1;
                    }
                }
                else {
                    stbi__skip(s, len);
                    break;
                }
            }
            while ((len = stbi__get8(s)) != 0) {
                stbi__skip(s, len);
            }
            break;
        }

        case 0x3B:
            return (stbi_uc*)s;

        default:
            return ((unsigned char*)(size_t)(stbi__err("unknown code") ? ((void*)0) : ((void*)0)));
        }
    }
}

static void* stbi__load_gif_main(stbi__context* s, int** delays, int* x, int* y, int* z, int* comp, int req_comp)
{
    if (stbi__gif_test(s)) {
        int layers = 0;
        stbi_uc* u = 0;
        stbi_uc* out = 0;
        stbi_uc* two_back = 0;
        stbi__gif g;
        int stride;
        memset(&g, 0, sizeof(g));
        if (delays) {
            *delays = 0;
        }

        do {
            u = stbi__gif_load_next(s, &g, comp, req_comp, two_back);
            if (u == (stbi_uc*)s) u = 0;

            if (u) {
                *x = g.w;
                *y = g.h;
                ++layers;
                stride = g.w * g.h * 4;

                if (out) {
                    void* tmp = (stbi_uc*)realloc(out, layers * stride);
                    if (((void*)0) == tmp) {
                        free(g.out);
                        free(g.history);
                        free(g.background);
                        return ((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));
                    }
                    else
                        out = (stbi_uc*)tmp;
                    if (delays) {
                        *delays = (int*)realloc(*delays, sizeof(int) * layers);
                    }
                }
                else {
                    out = (stbi_uc*)stbi__malloc(layers * stride);
                    if (delays) {
                        *delays = (int*)stbi__malloc(layers * sizeof(int));
                    }
                }
                memcpy(out + ((layers - 1) * stride), u, stride);
                if (layers >= 2) {
                    two_back = out - 2 * stride;
                }

                if (delays) {
                    (*delays)[layers - 1U] = g.delay;
                }
            }
        } while (u != 0);

        free(g.out);
        free(g.history);
        free(g.background);

        if (req_comp && req_comp != 4)
            out = stbi__convert_format(out, 4, req_comp, layers * g.w, g.h);

        *z = layers;
        return out;
    }
    else {
        return ((unsigned char*)(size_t)(stbi__err("not GIF") ? ((void*)0) : ((void*)0)));
    }
}

static void* stbi__gif_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
{
    stbi_uc* u = 0;
    stbi__gif g;
    memset(&g, 0, sizeof(g));
    (void)sizeof(ri);

    u = stbi__gif_load_next(s, &g, comp, req_comp, 0);
    if (u == (stbi_uc*)s) u = 0;
    if (u) {
        *x = g.w;
        *y = g.h;

        if (req_comp && req_comp != 4)
            u = stbi__convert_format(u, 4, req_comp, g.w, g.h);
    }
    else if (g.out) {

        free(g.out);
    }

    free(g.history);
    free(g.background);

    return u;
}

static int stbi__gif_info(stbi__context* s, int* x, int* y, int* comp)
{
    return stbi__gif_info_raw(s, x, y, comp);
}

static int stbi__hdr_test_core(stbi__context* s, const char* signature)
{
    int i;
    for (i = 0; signature[i]; ++i)
        if (stbi__get8(s) != signature[i])
            return 0;
    stbi__rewind(s);
    return 1;
}

static int stbi__hdr_test(stbi__context* s)
{
    int r = stbi__hdr_test_core(s, "#?RADIANCE\n");
    stbi__rewind(s);
    if (!r) {
        r = stbi__hdr_test_core(s, "#?RGBE\n");
        stbi__rewind(s);
    }
    return r;
}

static char* stbi__hdr_gettoken(stbi__context* z, char* buffer)
{
    int len = 0;
    char c = '\0';

    c = (char)stbi__get8(z);

    while (!stbi__at_eof(z) && c != '\n') {
        buffer[len++] = c;
        if (len == 1024 - 1) {

            while (!stbi__at_eof(z) && stbi__get8(z) != '\n')
                break;
        }
        c = (char)stbi__get8(z);
    }

    buffer[len] = 0;
    return buffer;
}

static void stbi__hdr_convert(float* output, stbi_uc* input, int req_comp)
{
    if (input[3] != 0) {
        float f1;

        f1 = (float)ldexp(1.0f, input[3] - (int)(128 + 8));
        if (req_comp <= 2)
            output[0] = (input[0] + input[1] + input[2]) * f1 / 3;
        else {
            output[0] = input[0] * f1;
            output[1] = input[1] * f1;
            output[2] = input[2] * f1;
        }
        if (req_comp == 2) output[1] = 1;
        if (req_comp == 4) output[3] = 1;
    }
    else {
        switch (req_comp) {
        case 4: output[3] = 1;
        case 3: output[0] = output[1] = output[2] = 0;
            break;
        case 2: output[1] = 1;
        case 1: output[0] = 0;
            break;
        }
    }
}

static float* stbi__hdr_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
{
    char buffer[1024];
    char* token;
    int valid = 0;
    int width, height;
    stbi_uc* scanline;
    float* hdr_data;
    int len;
    unsigned char count, value;
    int i, j, k, c1, c2, z;
    const char* headerToken;
    (void)sizeof(ri);

    headerToken = stbi__hdr_gettoken(s, buffer);
    if (strcmp(headerToken, "#?RADIANCE") != 0 && strcmp(headerToken, "#?RGBE") != 0)
        return ((float*)(size_t)(stbi__err("not HDR") ? ((void*)0) : ((void*)0)));

    for (;;) {
        token = stbi__hdr_gettoken(s, buffer);
        if (token[0] == 0) break;
        if (strcmp(token, "FORMAT=32-bit_rle_rgbe") == 0) valid = 1;
    }

    if (!valid) return ((float*)(size_t)(stbi__err("unsupported format") ? ((void*)0) : ((void*)0)));

    token = stbi__hdr_gettoken(s, buffer);
    if (strncmp(token, "-Y ", 3)) return ((float*)(size_t)(stbi__err("unsupported data layout") ? ((void*)0) : ((void*)0)));
    token += 3;
    height = (int)strtol(token, &token, 10);
    while (*token == ' ') ++token;
    if (strncmp(token, "+X ", 3)) return ((float*)(size_t)(stbi__err("unsupported data layout") ? ((void*)0) : ((void*)0)));
    token += 3;
    width = (int)strtol(token, ((void*)0), 10);

    *x = width;
    *y = height;

    if (comp) *comp = 3;
    if (req_comp == 0) req_comp = 3;

    if (!stbi__mad4sizes_valid(width, height, req_comp, sizeof(float), 0))
        return ((float*)(size_t)(stbi__err("too large") ? ((void*)0) : ((void*)0)));

    hdr_data = (float*)stbi__malloc_mad4(width, height, req_comp, sizeof(float), 0);
    if (!hdr_data)
        return ((float*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));

    if (width < 8 || width >= 32768) {

        for (j = 0; j < height; ++j) {
            for (i = 0; i < width; ++i) {
                stbi_uc rgbe[4];
            main_decode_loop:
                stbi__getn(s, rgbe, 4);
                stbi__hdr_convert(hdr_data + j * width * req_comp + i * req_comp, rgbe, req_comp);
            }
        }
    }
    else {

        scanline = ((void*)0);

        for (j = 0; j < height; ++j) {
            c1 = stbi__get8(s);
            c2 = stbi__get8(s);
            len = stbi__get8(s);
            if (c1 != 2 || c2 != 2 || (len & 0x80)) {

                stbi_uc rgbe[4];
                rgbe[0] = (stbi_uc)c1;
                rgbe[1] = (stbi_uc)c2;
                rgbe[2] = (stbi_uc)len;
                rgbe[3] = (stbi_uc)stbi__get8(s);
                stbi__hdr_convert(hdr_data, rgbe, req_comp);
                i = 1;
                j = 0;
                free(scanline);
                goto main_decode_loop;
            }
            len <<= 8;
            len |= stbi__get8(s);
            if (len != width) { free(hdr_data); free(scanline); return ((float*)(size_t)(stbi__err("invalid decoded scanline length") ? ((void*)0) : ((void*)0))); }
            if (scanline == ((void*)0)) {
                scanline = (stbi_uc*)stbi__malloc_mad2(width, 4, 0);
                if (!scanline) {
                    free(hdr_data);
                    return ((float*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));
                }
            }

            for (k = 0; k < 4; ++k) {
                int nleft;
                i = 0;
                while ((nleft = width - i) > 0) {
                    count = stbi__get8(s);
                    if (count > 128) {

                        value = stbi__get8(s);
                        count -= 128;
                        if (count > nleft) { free(hdr_data); free(scanline); return ((float*)(size_t)(stbi__err("corrupt") ? ((void*)0) : ((void*)0))); }
                        for (z = 0; z < count; ++z)
                            scanline[i++ * 4 + k] = value;
                    }
                    else {

                        if (count > nleft) { free(hdr_data); free(scanline); return ((float*)(size_t)(stbi__err("corrupt") ? ((void*)0) : ((void*)0))); }
                        for (z = 0; z < count; ++z)
                            scanline[i++ * 4 + k] = stbi__get8(s);
                    }
                }
            }
            for (i = 0; i < width; ++i)
                stbi__hdr_convert(hdr_data + (j * width + i) * req_comp, scanline + i * 4, req_comp);
        }
        if (scanline)
            free(scanline);
    }

    return hdr_data;
}

static int stbi__hdr_info(stbi__context* s, int* x, int* y, int* comp)
{
    char buffer[1024];
    char* token;
    int valid = 0;
    int dummy;

    if (!x) x = &dummy;
    if (!y) y = &dummy;
    if (!comp) comp = &dummy;

    if (stbi__hdr_test(s) == 0) {
        stbi__rewind(s);
        return 0;
    }

    for (;;) {
        token = stbi__hdr_gettoken(s, buffer);
        if (token[0] == 0) break;
        if (strcmp(token, "FORMAT=32-bit_rle_rgbe") == 0) valid = 1;
    }

    if (!valid) {
        stbi__rewind(s);
        return 0;
    }
    token = stbi__hdr_gettoken(s, buffer);
    if (strncmp(token, "-Y ", 3)) {
        stbi__rewind(s);
        return 0;
    }
    token += 3;
    *y = (int)strtol(token, &token, 10);
    while (*token == ' ') ++token;
    if (strncmp(token, "+X ", 3)) {
        stbi__rewind(s);
        return 0;
    }
    token += 3;
    *x = (int)strtol(token, ((void*)0), 10);
    *comp = 3;
    return 1;
}

static int stbi__bmp_info(stbi__context* s, int* x, int* y, int* comp)
{
    void* p;
    stbi__bmp_data info;

    info.all_a = 255;
    p = stbi__bmp_parse_header(s, &info);
    stbi__rewind(s);
    if (p == ((void*)0))
        return 0;
    if (x) *x = s->img_x;
    if (y) *y = s->img_y;
    if (comp) {
        if (info.bpp == 24 && info.ma == 0xff000000)
            *comp = 3;
        else
            *comp = info.ma ? 4 : 3;
    }
    return 1;
}

static int stbi__psd_info(stbi__context* s, int* x, int* y, int* comp)
{
    int channelCount, dummy, depth;
    if (!x) x = &dummy;
    if (!y) y = &dummy;
    if (!comp) comp = &dummy;
    if (stbi__get32be(s) != 0x38425053) {
        stbi__rewind(s);
        return 0;
    }
    if (stbi__get16be(s) != 1) {
        stbi__rewind(s);
        return 0;
    }
    stbi__skip(s, 6);
    channelCount = stbi__get16be(s);
    if (channelCount < 0 || channelCount > 16) {
        stbi__rewind(s);
        return 0;
    }
    *y = stbi__get32be(s);
    *x = stbi__get32be(s);
    depth = stbi__get16be(s);
    if (depth != 8 && depth != 16) {
        stbi__rewind(s);
        return 0;
    }
    if (stbi__get16be(s) != 3) {
        stbi__rewind(s);
        return 0;
    }
    *comp = 4;
    return 1;
}

static int stbi__psd_is16(stbi__context* s)
{
    int channelCount, depth;
    if (stbi__get32be(s) != 0x38425053) {
        stbi__rewind(s);
        return 0;
    }
    if (stbi__get16be(s) != 1) {
        stbi__rewind(s);
        return 0;
    }
    stbi__skip(s, 6);
    channelCount = stbi__get16be(s);
    if (channelCount < 0 || channelCount > 16) {
        stbi__rewind(s);
        return 0;
    }
    (void)stbi__get32be(s);
    (void)stbi__get32be(s);
    depth = stbi__get16be(s);
    if (depth != 16) {
        stbi__rewind(s);
        return 0;
    }
    return 1;
}

static int stbi__pic_info(stbi__context* s, int* x, int* y, int* comp)
{
    int act_comp = 0, num_packets = 0, chained, dummy;
    stbi__pic_packet packets[10];

    if (!x) x = &dummy;
    if (!y) y = &dummy;
    if (!comp) comp = &dummy;

    if (!stbi__pic_is4(s, "\x53\x80\xF6\x34")) {
        stbi__rewind(s);
        return 0;
    }

    stbi__skip(s, 88);

    *x = stbi__get16be(s);
    *y = stbi__get16be(s);
    if (stbi__at_eof(s)) {
        stbi__rewind(s);
        return 0;
    }
    if ((*x) != 0 && (1 << 28) / (*x) < (*y)) {
        stbi__rewind(s);
        return 0;
    }

    stbi__skip(s, 8);

    do {
        stbi__pic_packet* packet;

        if (num_packets == sizeof(packets) / sizeof(packets[0]))
            return 0;

        packet = &packets[num_packets++];
        chained = stbi__get8(s);
        packet->size = stbi__get8(s);
        packet->type = stbi__get8(s);
        packet->channel = stbi__get8(s);
        act_comp |= packet->channel;

        if (stbi__at_eof(s)) {
            stbi__rewind(s);
            return 0;
        }
        if (packet->size != 8) {
            stbi__rewind(s);
            return 0;
        }
    } while (chained);

    *comp = (act_comp & 0x10 ? 4 : 3);

    return 1;
}
static int stbi__pnm_test(stbi__context* s)
{
    char p, t;
    p = (char)stbi__get8(s);
    t = (char)stbi__get8(s);
    if (p != 'P' || (t != '5' && t != '6')) {
        stbi__rewind(s);
        return 0;
    }
    return 1;
}

static void* stbi__pnm_load(stbi__context* s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
{
    stbi_uc* out;
    (void)sizeof(ri);

    if (!stbi__pnm_info(s, (int*)&s->img_x, (int*)&s->img_y, (int*)&s->img_n))
        return 0;

    *x = s->img_x;
    *y = s->img_y;
    if (comp) *comp = s->img_n;

    if (!stbi__mad3sizes_valid(s->img_n, s->img_x, s->img_y, 0))
        return ((unsigned char*)(size_t)(stbi__err("too large") ? ((void*)0) : ((void*)0)));

    out = (stbi_uc*)stbi__malloc_mad3(s->img_n, s->img_x, s->img_y, 0);
    if (!out) return ((unsigned char*)(size_t)(stbi__err("outofmem") ? ((void*)0) : ((void*)0)));
    stbi__getn(s, out, s->img_n * s->img_x * s->img_y);

    if (req_comp && req_comp != s->img_n) {
        out = stbi__convert_format(out, s->img_n, req_comp, s->img_x, s->img_y);
        if (out == ((void*)0)) return out;
    }
    return out;
}

static int stbi__pnm_isspace(char c)
{
    return c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
}

static void stbi__pnm_skip_whitespace(stbi__context* s, char* c)
{
    for (;;) {
        while (!stbi__at_eof(s) && stbi__pnm_isspace(*c))
            *c = (char)stbi__get8(s);

        if (stbi__at_eof(s) || *c != '#')
            break;

        while (!stbi__at_eof(s) && *c != '\n' && *c != '\r')
            *c = (char)stbi__get8(s);
    }
}

static int stbi__pnm_isdigit(char c)
{
    return c >= '0' && c <= '9';
}

static int stbi__pnm_getinteger(stbi__context* s, char* c)
{
    int value = 0;

    while (!stbi__at_eof(s) && stbi__pnm_isdigit(*c)) {
        value = value * 10 + (*c - '0');
        *c = (char)stbi__get8(s);
    }

    return value;
}

static int stbi__pnm_info(stbi__context* s, int* x, int* y, int* comp)
{
    int maxv, dummy;
    char c, p, t;

    if (!x) x = &dummy;
    if (!y) y = &dummy;
    if (!comp) comp = &dummy;

    stbi__rewind(s);

    p = (char)stbi__get8(s);
    t = (char)stbi__get8(s);
    if (p != 'P' || (t != '5' && t != '6')) {
        stbi__rewind(s);
        return 0;
    }

    *comp = (t == '6') ? 3 : 1;

    c = (char)stbi__get8(s);
    stbi__pnm_skip_whitespace(s, &c);

    *x = stbi__pnm_getinteger(s, &c);
    stbi__pnm_skip_whitespace(s, &c);

    *y = stbi__pnm_getinteger(s, &c);
    stbi__pnm_skip_whitespace(s, &c);

    maxv = stbi__pnm_getinteger(s, &c);

    if (maxv > 255)
        return stbi__err("max value > 255");
    else
        return 1;
}

static int stbi__info_main(stbi__context* s, int* x, int* y, int* comp)
{

    if (stbi__jpeg_info(s, x, y, comp)) return 1;

    if (stbi__png_info(s, x, y, comp)) return 1;

    if (stbi__gif_info(s, x, y, comp)) return 1;

    if (stbi__bmp_info(s, x, y, comp)) return 1;

    if (stbi__psd_info(s, x, y, comp)) return 1;

    if (stbi__pic_info(s, x, y, comp)) return 1;

    if (stbi__pnm_info(s, x, y, comp)) return 1;

    if (stbi__hdr_info(s, x, y, comp)) return 1;

    if (stbi__tga_info(s, x, y, comp))
        return 1;

    return stbi__err("unknown image type");
}

static int stbi__is_16_main(stbi__context* s)
{

    if (stbi__png_is16(s)) return 1;

    if (stbi__psd_is16(s)) return 1;

    return 0;
}

extern int stbi_info(char const* filename, int* x, int* y, int* comp)
{
    FILE* f = stbi__fopen(filename, "rb");
    int result;
    if (!f) return stbi__err("can't fopen");
    result = stbi_info_from_file(f, x, y, comp);
    fclose(f);
    return result;
}

extern int stbi_info_from_file(FILE* f, int* x, int* y, int* comp)
{
    int r;
    stbi__context s;
    long pos = ftell(f);
    stbi__start_file(&s, f);
    r = stbi__info_main(&s, x, y, comp);
    fseek(f, pos, 0);
    return r;
}

extern int stbi_is_16_bit(char const* filename)
{
    FILE* f = stbi__fopen(filename, "rb");
    int result;
    if (!f) return stbi__err("can't fopen");
    result = stbi_is_16_bit_from_file(f);
    fclose(f);
    return result;
}

extern int stbi_is_16_bit_from_file(FILE* f)
{
    int r;
    stbi__context s;
    long pos = ftell(f);
    stbi__start_file(&s, f);
    r = stbi__is_16_main(&s);
    fseek(f, pos, 0);
    return r;
}

extern int stbi_info_from_memory(stbi_uc const* buffer, int len, int* x, int* y, int* comp)
{
    stbi__context s;
    stbi__start_mem(&s, buffer, len);
    return stbi__info_main(&s, x, y, comp);
}

extern int stbi_info_from_callbacks(stbi_io_callbacks const* c, void* user, int* x, int* y, int* comp)
{
    stbi__context s;
    stbi__start_callbacks(&s, (stbi_io_callbacks*)c, user);
    return stbi__info_main(&s, x, y, comp);
}

extern int stbi_is_16_bit_from_memory(stbi_uc const* buffer, int len)
{
    stbi__context s;
    stbi__start_mem(&s, buffer, len);
    return stbi__is_16_main(&s);
}

extern int stbi_is_16_bit_from_callbacks(stbi_io_callbacks const* c, void* user)
{
    stbi__context s;
    stbi__start_callbacks(&s, (stbi_io_callbacks*)c, user);
    return stbi__is_16_main(&s);
}