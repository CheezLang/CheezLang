namespace Cheez
{
    public partial class Workspace
    {
        private void Pass7()
        {
            foreach (var func in mFunctions)
            {
                AnalyseFunction(func);
            }

            foreach (var i in mImpls)
            {
                foreach (var f in i.Functions)
                {
                    AnalyseFunction(f);
                }
            }

            foreach (var i in mTraitImpls)
            {
                if (i.Trait == null)
                    continue;

                foreach (var f in i.Functions)
                {
                    AnalyseFunction(f);
                }
            }
        }
    }
}
