using Cheez.Compiler;
using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using Cheez.Compiler.Visitor;
using LanguageServer.Parameters;
using System.Collections.Generic;
using System.Linq;

namespace CheezLanguageServer
{
    class SymbolFinder : VisitorBase<object, List<SymbolInformation>>
    {
        public List<SymbolInformation> FindSymbols(Workspace w, PTFile file)
        {
            var list = new List<SymbolInformation>();
            foreach (var s in w.Statements.Where(s => s.GenericParseTreeNode.SourceFile == file))
            {
                s.Accept(this, list);
            }

            return list;
        }

        private LanguageServer.Parameters.Location CastLocation(ILocation loc)
        {
            var beg = loc.Beginning;
            var end = loc.End;
            return new LanguageServer.Parameters.Location
            {
                range = new Range
                {
                    start = new Position { line = beg.line - 1, character = beg.index - beg.lineStartIndex },
                    end = new Position { line = end.line - 1, character = end.end - end.lineStartIndex }
                }
            };
        }

        public override object VisitTypeDeclaration(AstTypeDecl type, List<SymbolInformation> list)
        {
            list.Add(new SymbolInformation
            {
                containerName = type.Scope.Name,
                kind = SymbolKind.Class,
                location = CastLocation(type.ParseTreeNode.Name),
                name = type.Name
            });
            return default;
        }

        public override object VisitFunctionDeclaration(AstFunctionDecl function, List<SymbolInformation> list)
        {
            list.Add(new SymbolInformation
            {
                containerName = function.Scope.Name,
                kind = SymbolKind.Function,
                location = CastLocation(function.ParseTreeNode.Name),
                name = function.Name
            });

            function.Body?.Accept(this, list);

            return default;
        }

        public override object VisitIfStatement(AstIfStmt ifs, List<SymbolInformation> list)
        {
            ifs.IfCase.Accept(this, list);
            ifs.ElseCase?.Accept(this, list);
            return default;
        }

        public override object VisitWhileStatement(AstWhileStmt ws, List<SymbolInformation> list)
        {
            ws.Body.Accept(this, list);
            return default;
        }

        public override object VisitBlockStatement(AstBlockStmt block, List<SymbolInformation> list)
        {

            foreach (var s in block.Statements)
            {
                s.Accept(this, list);
            }

            return default;
        }

        public override object VisitVariableDeclaration(AstVariableDecl variable, List<SymbolInformation> list)
        {
            list.Add(new SymbolInformation
            {
                containerName = variable.Scope.Name,
                kind = SymbolKind.Variable,
                location = CastLocation(variable.ParseTreeNode.Name),
                name = variable.Name
            });
            return default;
        }
    }
}
