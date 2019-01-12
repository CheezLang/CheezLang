namespace Cheez
{
    public partial class Workspace
    {
        private void Pass7()
        {
            foreach (var func in mFunctions)
            {
                AnalyzeFunction(func);
            }
        }
    }
}
