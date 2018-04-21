namespace Cheez.Compiler
{
    public static class Util
    {
        private static int mId = 0;

        public static int NewId => mId++;
    }
}
