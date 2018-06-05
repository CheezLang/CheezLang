#include "int_lib.h"

du_int COMPILER_RT_ABI __udivmoddi4(du_int a, du_int b, du_int* rem);
du_int COMPILER_RT_ABI __umoddi3(du_int a, du_int b);
//
//su_int __builtin_clz(su_int x) {
//    su_int count = 0;
//    for (; x > 0; count++)
//        x >>= 1;
//
//    return 32 - count;
//}
//
//su_int __builtin_ctz(su_int x) {
//    su_int count = 0;
//    for (; (x & 0x1) == 0; count++)
//        x >>= 1;
//
//    return count;
//}

//////////////////////////////////////////////////

/* Returns: a / b */
COMPILER_RT_ABI di_int __divdi3(di_int a, di_int b)
{
    const int bits_in_dword_m1 = (int)(sizeof(di_int) * CHAR_BIT) - 1;
    di_int s_a = a >> bits_in_dword_m1;           /* s_a = a < 0 ? -1 : 0 */
    di_int s_b = b >> bits_in_dword_m1;           /* s_b = b < 0 ? -1 : 0 */
    a = (a ^ s_a) - s_a;                         /* negate if s_a == -1 */
    b = (b ^ s_b) - s_b;                         /* negate if s_b == -1 */
    s_a ^= s_b;                                  /*sign of quotient */
    return (__udivmoddi4(a, b, (du_int*)0) ^ s_a) - s_a;  /* negate if s_a == -1 */
}

/* Returns: a % b */
COMPILER_RT_ABI du_int __umoddi3(du_int a, du_int b)
{
    du_int r;
    __udivmoddi4(a, b, &r);
    return r;
}

COMPILER_RT_ABI di_int __moddi3(di_int a, di_int b)
{
    const int bits_in_dword_m1 = (int)(sizeof(di_int) * CHAR_BIT) - 1;
    di_int s = b >> bits_in_dword_m1;  /* s = b < 0 ? -1 : 0 */
    b = (b ^ s) - s;                   /* negate if s == -1 */
    s = a >> bits_in_dword_m1;         /* s = a < 0 ? -1 : 0 */
    a = (a ^ s) - s;                   /* negate if s == -1 */
    di_int r;
    __udivmoddi4(a, b, (du_int*)&r);
    return (r ^ s) - s;                /* negate if s == -1 */
}

/* Effects: if rem != 0, *rem = a % b
* Returns: a / b
*/
COMPILER_RT_ABI du_int __udivmoddi4(du_int a, du_int b, du_int* rem)
{
    const unsigned n_uword_bits = sizeof(su_int) * CHAR_BIT;
    const unsigned n_udword_bits = sizeof(du_int) * CHAR_BIT;
    udwords n;
    n.all = a;
    udwords d;
    d.all = b;
    udwords q;
    udwords r;
    unsigned sr;
    /* special cases, X is unknown, K != 0 */
    if (n.s.high == 0)
    {
        if (d.s.high == 0)
        {
            /* 0 X
            * ---
            * 0 X
            */
            if (rem)
                *rem = n.s.low % d.s.low;
            return n.s.low / d.s.low;
        }
        /* 0 X
        * ---
        * K X
        */
        if (rem)
            *rem = n.s.low;
        return 0;
    }
    /* n.s.high != 0 */
    if (d.s.low == 0)
    {
        if (d.s.high == 0)
        {
            /* K X
            * ---
            * 0 0
            */
            if (rem)
                *rem = n.s.high % d.s.low;
            return n.s.high / d.s.low;
        }
        /* d.s.high != 0 */
        if (n.s.low == 0)
        {
            /* K 0
            * ---
            * K 0
            */
            if (rem)
            {
                r.s.high = n.s.high % d.s.high;
                r.s.low = 0;
                *rem = r.all;
            }
            return n.s.high / d.s.high;
        }
        /* K K
        * ---
        * K 0
        */
        if ((d.s.high & (d.s.high - 1)) == 0)     /* if d is a power of 2 */
        {
            if (rem)
            {
                r.s.low = n.s.low;
                r.s.high = n.s.high & (d.s.high - 1);
                *rem = r.all;
            }
            return n.s.high >> __builtin_ctz(d.s.high);
        }
        /* K K
        * ---
        * K 0
        */
        sr = __builtin_clz(d.s.high) - __builtin_clz(n.s.high);
        /* 0 <= sr <= n_uword_bits - 2 or sr large */
        if (sr > n_uword_bits - 2)
        {
            if (rem)
                *rem = n.all;
            return 0;
        }
        ++sr;
        /* 1 <= sr <= n_uword_bits - 1 */
        /* q.all = n.all << (n_udword_bits - sr); */
        q.s.low = 0;
        q.s.high = n.s.low << (n_uword_bits - sr);
        /* r.all = n.all >> sr; */
        r.s.high = n.s.high >> sr;
        r.s.low = (n.s.high << (n_uword_bits - sr)) | (n.s.low >> sr);
    }
    else  /* d.s.low != 0 */
    {
        if (d.s.high == 0)
        {
            /* K X
            * ---
            * 0 K
            */
            if ((d.s.low & (d.s.low - 1)) == 0)     /* if d is a power of 2 */
            {
                if (rem)
                    *rem = n.s.low & (d.s.low - 1);
                if (d.s.low == 1)
                    return n.all;
                sr = __builtin_ctz(d.s.low);
                q.s.high = n.s.high >> sr;
                q.s.low = (n.s.high << (n_uword_bits - sr)) | (n.s.low >> sr);
                return q.all;
            }
            /* K X
            * ---
            *0 K
            */
            sr = 1 + n_uword_bits + __builtin_clz(d.s.low) - __builtin_clz(n.s.high);
            /* 2 <= sr <= n_udword_bits - 1
            * q.all = n.all << (n_udword_bits - sr);
            * r.all = n.all >> sr;
            * if (sr == n_uword_bits)
            * {
            *     q.s.low = 0;
            *     q.s.high = n.s.low;
            *     r.s.high = 0;
            *     r.s.low = n.s.high;
            * }
            * else if (sr < n_uword_bits)  // 2 <= sr <= n_uword_bits - 1
            * {
            *     q.s.low = 0;
            *     q.s.high = n.s.low << (n_uword_bits - sr);
            *     r.s.high = n.s.high >> sr;
            *     r.s.low = (n.s.high << (n_uword_bits - sr)) | (n.s.low >> sr);
            * }
            * else              // n_uword_bits + 1 <= sr <= n_udword_bits - 1
            * {
            *     q.s.low = n.s.low << (n_udword_bits - sr);
            *     q.s.high = (n.s.high << (n_udword_bits - sr)) |
            *              (n.s.low >> (sr - n_uword_bits));
            *     r.s.high = 0;
            *     r.s.low = n.s.high >> (sr - n_uword_bits);
            * }
            */
            q.s.low = (n.s.low << (n_udword_bits - sr)) &
                ((si_int)(n_uword_bits - sr) >> (n_uword_bits - 1));
            q.s.high = ((n.s.low << (n_uword_bits - sr))                       &
                ((si_int)(sr - n_uword_bits - 1) >> (n_uword_bits - 1))) |
                (((n.s.high << (n_udword_bits - sr)) |
                (n.s.low >> (sr - n_uword_bits)))                        &
                    ((si_int)(n_uword_bits - sr) >> (n_uword_bits - 1)));
            r.s.high = (n.s.high >> sr) &
                ((si_int)(sr - n_uword_bits) >> (n_uword_bits - 1));
            r.s.low = ((n.s.high >> (sr - n_uword_bits))                       &
                ((si_int)(n_uword_bits - sr - 1) >> (n_uword_bits - 1))) |
                (((n.s.high << (n_uword_bits - sr)) |
                (n.s.low >> sr))                                         &
                    ((si_int)(sr - n_uword_bits) >> (n_uword_bits - 1)));
        }
        else
        {
            /* K X
            * ---
            * K K
            */
            sr = __builtin_clz(d.s.high) - __builtin_clz(n.s.high);
            /* 0 <= sr <= n_uword_bits - 1 or sr large */
            if (sr > n_uword_bits - 1)
            {
                if (rem)
                    *rem = n.all;
                return 0;
            }
            ++sr;
            /* 1 <= sr <= n_uword_bits */
            /*  q.all = n.all << (n_udword_bits - sr); */
            q.s.low = 0;
            q.s.high = n.s.low << (n_uword_bits - sr);
            /* r.all = n.all >> sr;
            * if (sr < n_uword_bits)
            * {
            *     r.s.high = n.s.high >> sr;
            *     r.s.low = (n.s.high << (n_uword_bits - sr)) | (n.s.low >> sr);
            * }
            * else
            * {
            *     r.s.high = 0;
            *     r.s.low = n.s.high;
            * }
            */
            r.s.high = (n.s.high >> sr) &
                ((si_int)(sr - n_uword_bits) >> (n_uword_bits - 1));
            r.s.low = (n.s.high << (n_uword_bits - sr)) |
                ((n.s.low >> sr)                  &
                ((si_int)(sr - n_uword_bits) >> (n_uword_bits - 1)));
        }
    }
    /* Not a special case
    * q and r are initialized with:
    * q.all = n.all << (n_udword_bits - sr);
    * r.all = n.all >> sr;
    * 1 <= sr <= n_udword_bits - 1
    */
    su_int carry = 0;
    for (; sr > 0; --sr)
    {
        /* r:q = ((r:q)  << 1) | carry */
        r.s.high = (r.s.high << 1) | (r.s.low >> (n_uword_bits - 1));
        r.s.low = (r.s.low << 1) | (q.s.high >> (n_uword_bits - 1));
        q.s.high = (q.s.high << 1) | (q.s.low >> (n_uword_bits - 1));
        q.s.low = (q.s.low << 1) | carry;
        /* carry = 0;
        * if (r.all >= d.all)
        * {
        *      r.all -= d.all;
        *      carry = 1;
        * }
        */
        const di_int s = (di_int)(d.all - r.all - 1) >> (n_udword_bits - 1);
        carry = s & 1;
        r.all -= d.all & s;
    }
    q.all = (q.all << 1) | carry;
    if (rem)
        *rem = r.all;
    return q.all;
}
