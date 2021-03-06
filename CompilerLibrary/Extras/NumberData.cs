using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace Cheez.Extras
{
    public struct NumberData : IEquatable<NumberData>
    {
        public enum NumberType
        {
            Float,
            Int
        }

        public int IntBase { get; private set; }
        public string StringValue { get; private set; }
        public NumberType Type { get; private set; }
        public BigInteger IntValue { get; private set; }
        public double DoubleValue { get; private set; }
        public string Error { get; private set; }

        public NumberData(NumberType type, string val, int b)
        {
            IntBase = b;
            StringValue = val;
            Type = type;
            IntValue = default;
            DoubleValue = default;
            Error = null;

            if (type == NumberType.Int)
            {
                if (b == 10)
                    IntValue = BigInteger.Parse("0" + val, CultureInfo.InvariantCulture);
                else if (b == 16)
                    IntValue = BigInteger.Parse("0" + val, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                else if (b == 2)
                {
                    BigInteger currentDigit = 1;
                    IntValue = 0;

                    for (int i = val.Length - 1; i >= 0; i--)
                    {
                        if (val[i] == '1')
                            IntValue += currentDigit;
                        else if (val[i] == '0') { }  // do nothing
                        else throw new NotImplementedException();
                        currentDigit *= 2;
                    }
                }
            }
            else if (type == NumberType.Float)
            {
                double v;
                if (double.TryParse(val, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                {
                    DoubleValue = v;
                }
                else
                {
                    Error = "Literal is too big to fit in a double";
                }
            }
        }

        public override string ToString()
        {
            return StringValue;
        }

        public ulong ToUlong()
        {
            if (IntValue > long.MaxValue)
                return (ulong)IntValue;

            unsafe
            {
                long p = (long)IntValue;
                return *(ulong*)&p;
            }
        }

        public long ToLong()
        {
            return (long)IntValue;
        }

        public double ToDouble()
        {
            if (Type == NumberType.Int)
                return (double)IntValue;
            else
                return DoubleValue;
        }

        public NumberData Negate()
        {
            switch (Type)
            {
                case NumberType.Int:
                    return new NumberData
                    {
                        StringValue = "-" + StringValue,
                        IntBase = IntBase,
                        IntValue = -IntValue,
                        Type = Type
                    };


                case NumberType.Float:
                    return new NumberData
                    {
                        StringValue = "-" + StringValue,
                        IntBase = IntBase,
                        DoubleValue = -DoubleValue,
                        Type = Type
                    };

                default:
                    throw new NotImplementedException();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is NumberData o) return this == o;
            return false;
        }



        public static NumberData FromBigInt(BigInteger num)
        {
            return new NumberData {
                IntBase = 10,
                StringValue = num.ToString(CultureInfo.InvariantCulture),
                Type = NumberType.Int,
                IntValue = num,
                DoubleValue = default,
                Error = null,
            };
        }

        public static NumberData FromDouble(double num)
        {
            return new NumberData {
                IntBase = 10,
                StringValue = num.ToString(CultureInfo.InvariantCulture),
                Type = NumberType.Float,
                IntValue = default,
                DoubleValue = num,
                Error = null
            };
        }

        public override int GetHashCode()
        {
            var hashCode = 1753786285;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<BigInteger>.Default.GetHashCode(IntValue);
            hashCode = hashCode * -1521134295 + DoubleValue.GetHashCode();
            return hashCode;
        }

        public bool Equals([AllowNull] NumberData other)
        {
            return this == other;
        }

        public static implicit operator NumberData(BigInteger bi) => FromBigInt(bi);
        public static implicit operator NumberData(int i) => FromBigInt(new BigInteger(i));
        public static implicit operator NumberData(long l) => FromBigInt(new BigInteger(l));
        public static implicit operator NumberData(double d) => FromDouble(d);


        public static bool operator ==(NumberData a, NumberData b)
        {
            if (a.Type != b.Type) return false;
            if (a.Type == NumberType.Int) return a.IntValue == b.IntValue;
            return a.DoubleValue == b.DoubleValue;
        }
        public static bool operator !=(NumberData a, NumberData b) => !(a == b);
        public static bool operator >(NumberData a, NumberData b) => a.Type == b.Type && (a.Type == NumberType.Int ? a.IntValue > b.IntValue : a.DoubleValue > b.DoubleValue);
        public static bool operator >=(NumberData a, NumberData b) => a.Type == b.Type && (a.Type == NumberType.Int ? a.IntValue >= b.IntValue : a.DoubleValue >= b.DoubleValue);
        public static bool operator <(NumberData a, NumberData b) => a.Type == b.Type && (a.Type == NumberType.Int ? a.IntValue < b.IntValue : a.DoubleValue < b.DoubleValue);
        public static bool operator <=(NumberData a, NumberData b) => a.Type == b.Type && (a.Type == NumberType.Int ? a.IntValue <= b.IntValue : a.DoubleValue <= b.DoubleValue);

        public static NumberData operator +(NumberData a, NumberData b) => a.Type == NumberType.Int ? FromBigInt(a.IntValue + b.IntValue) : FromDouble(a.DoubleValue + b.DoubleValue);
        public static NumberData operator -(NumberData a, NumberData b) => a.Type == NumberType.Int ? FromBigInt(a.IntValue - b.IntValue) : FromDouble(a.DoubleValue - b.DoubleValue);
        public static NumberData operator *(NumberData a, NumberData b) => a.Type == NumberType.Int ? FromBigInt(a.IntValue * b.IntValue) : FromDouble(a.DoubleValue * b.DoubleValue);
        public static NumberData operator /(NumberData a, NumberData b) => a.Type == NumberType.Int ? FromBigInt(a.IntValue / b.IntValue) : FromDouble(a.DoubleValue / b.DoubleValue);
        public static NumberData operator %(NumberData a, NumberData b) => a.Type == NumberType.Int ? FromBigInt(a.IntValue % b.IntValue) : FromDouble(a.DoubleValue % b.DoubleValue);

        internal NumberData AsDouble()
        {
            if (Type == NumberType.Float)
                return this;
            return FromDouble(ToDouble());
        }
    }
}
