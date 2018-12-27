namespace Cheez.Compiler
{
    public struct CheezValue
    {
        public CheezType type;
        public object value;

        public CheezValue(CheezType t, object v)
        {
            type = t;
            value = v;
        }

        public override string ToString()
        {
            return value?.ToString() ?? "null";
        }

        public override bool Equals(object obj)
        {
            if (obj is CheezValue v)
            {
                if (type != v.type)
                    return false;

                return value?.Equals(v.value) ?? false;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return type.GetHashCode() + (value?.GetHashCode() ?? 0);
        }

        public static bool operator ==(CheezValue a, CheezValue b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CheezValue a, CheezValue b)
        {
            return !(a == b);
        }
    }
}
