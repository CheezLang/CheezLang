namespace Cheez
{
    public partial class Workspace
    {
        private void Pass7()
        {
            foreach (var func in mFunctions)
            {
                if (func.IsGeneric) continue;

                AnalyseFunction(func);
            }

            foreach (var i in mImpls)
            {
                foreach (var f in i.Functions)
                {
                    if (f.IsGeneric) continue;

                    AnalyseFunction(f);
                }
            }

            foreach (var i in mTraitImpls)
            {
                foreach (var f in i.Functions)
                {
                    if (f.IsGeneric) continue;

                    AnalyseFunction(f);
                }
            }
        }
    }
}
