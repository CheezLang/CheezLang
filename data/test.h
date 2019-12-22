using T1 = char;
typedef bool T2;

struct S3 {};
typedef struct {} S2;
typedef struct S1 {int i;} S1;
typedef enum E1 {A, B}   E1;
typedef union U1 {int i; float f;}  U1;

#define TEST 1

// void  f1();
// void  f2(int i);
// int   f3();
// float f4(float, float);
// void  f5(T1, T2);
// void  f6(char, short, int, long, long long);
// void  f7(unsigned char, unsigned short, unsigned int, unsigned long, unsigned long long);
// void* f8(int* x);
// void  f9(int x[5]);
// void  f9(int x[]);
// void  f12(S1*, E1*, U1*);


void  f11(S1, E1, U1);
int  f11_1(S1, E1, U1);
S1  f11_11(S1, E1, U1);
E1    f11_2(int );
E1    f11_3();
E1*   f11_4();
E1*   f11_5(int);