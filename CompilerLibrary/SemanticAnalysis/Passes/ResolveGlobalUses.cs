namespace Cheez
{
    public partial class Workspace
    {
        private void PassResolveGlobalUses()
        {
            foreach (var use in mGlobalUses)
            {
                AnalyseUseStatement(use);
            }
        }
    }
}
