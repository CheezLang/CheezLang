using Cheez.Ast;
using Cheez.Parsing;
using System;

namespace Cheez
{
    public class CompilationQueue
    {
        private ErrorHandler mErrorHandler;
        
        private Semanticer mSematicer = new Semanticer();

        // constructor
        public CompilationQueue(int threadCount)
        {
            mErrorHandler = new ErrorHandler();
            mSematicer.Start();
        }

        public void Complete()
        {
            mSematicer.Complete();
        }

        public Statement[] GetCompiledStatements()
        {
            return mSematicer.GetCompiledStatements();
        }

        public void CompileFile(string name)
        {
            try
            {
                var lexer = Lexer.FromFile(name);
                CompileFromLexer(lexer);
            }
            catch (Exception e)
            {
                mErrorHandler.ReportCompileError(e);
            }
        }
        
        public void CompileString(string str)
        {
            var lexer = Lexer.FromString(str);
            CompileFromLexer(lexer);
        }

        private void CompileFromLexer(Lexer lexer)
        {
            var parser = new Parser(lexer);

            while (true)
            {
                try
                {
                    var statement = parser.ParseStatement();
                    if (statement == null)
                        break;

                    mSematicer.CompileStatement(statement);
                }
                catch (ParsingError err)
                {
                    mErrorHandler.ReportParsingError(err);
                    break;
                }
            }
        }
    }
}
