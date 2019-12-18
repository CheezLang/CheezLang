#define CINDEX_LINKAGE __declspec(dllimport)
typedef void *CXIndex;

CINDEX_LINKAGE CXIndex clang_createIndex(int excludeDeclarationsFromPCH,
                                         int displayDiagnostics);

/**
 * Destroy the given index.
 *
 * The index must not be destroyed until all of the translation units created
 * within that index have been destroyed.
 */
CINDEX_LINKAGE void clang_disposeIndex(CXIndex index);

// typedef struct {
//     int a;
//     float f[5];
//     bool b[];
// } S1;

// struct S2 {
//     int a;
//     float f[5];
//     bool b;
//     S1 s;
// };

// typedef struct S3 {
//     int a;
//     float f[5];
//     bool b;
//     S2 s[6];
// } S3;

// typedef enum {
//     E1_A, E1_B, E1_C = 6, E1_D
// } E1;

// enum E2 {
//     E2_A, E2_B, E2_C = 6, E2_D
// };

// typedef enum E3 {
//     E3_A, E3_B, E3_C = 6, E3_D
// } E3;

// S1 f1(S1, S1*, S1[], S1[3]);
// S1 f2(S1 a, S1* b,  S1[] c, S1[3] d);
// void f3(S1 a, S1* b,  S1[] c, S1[3] d);
// int f4(S1 a, S1* b,  S1[] c, S1[3] d);

// void f1();

// char f1_1(char, char*, char[], char[123]);
// short f1_1(short, short*, short[], short[123]);
// int f1_1(int, int*, int[], int[123]);
// long f1_1(long, long*, long[], long[123]);
// long long f1_1(long long, long long*, long long[], long long[123]);
// unsigned char f1_1(unsigned char, unsigned char*, unsigned char[], unsigned char[123]);
// unsigned short f1_1(unsigned short, unsigned short*, unsigned short[], unsigned short[123]);
// unsigned int f1_1(unsigned int, unsigned int*, unsigned int[], unsigned int[123]);
// unsigned long f1_1(unsigned long, unsigned long*, unsigned long[], unsigned long[123]);
// unsigned long long f1_1(unsigned long long, unsigned long long*, unsigned long long[], unsigned long long[123]);

// float f1_1();
// double f1_1();
// bool f1_1();

// int  f5();
// int  f6(int);
// int  f7(bool a, int b);
// void f8(int[3], int[], int*);