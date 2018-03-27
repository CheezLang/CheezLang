using System;
using System.Collections.Generic;

namespace Cheez
{
    public class Workspace
    {
        public Scope GlobalScope { get; set; } = new Scope();

        private List<CheezFile> mFiles = new List<CheezFile>();

        public void AddFile(CheezFile file)
        {
            mFiles.Add(file);
        }
    }
}
