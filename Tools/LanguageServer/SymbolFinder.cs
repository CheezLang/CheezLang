using Cheez;
using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Visitors;
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
            foreach (var s in file.Statements)
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

        public override object VisitStructTypeExpr(AstStructTypeExpr type, List<SymbolInformation> list)
        {
            list.Add(new SymbolInformation
            {
                containerName = type.Scope.Name,
                kind = SymbolKind.Class,
                location = CastLocation(type.Location),
                name = type.Name
            });
            return default;
        }

        public override object VisitFuncExpr(AstFuncExpr function, List<SymbolInformation> list)
        {
            list.Add(new SymbolInformation
            {
                containerName = function.Scope.Name,
                kind = SymbolKind.Function,
                location = CastLocation(function.Location),
                name = function.Name
            });

            function.Body?.Accept(this, list);

            return default;
        }

        public override object VisitIfExpr(AstIfExpr ifs, List<SymbolInformation> list)
        {
            ifs.IfCase.Accept(this, list);
            ifs.ElseCase?.Accept(this, list);
            return default;
        }

        public override object VisitWhileStmt(AstWhileStmt ws, List<SymbolInformation> list)
        {
            ws.Body.Accept(this, list);
            return default;
        }

        public override object VisitBlockExpr(AstBlockExpr block, List<SymbolInformation> list)
        {

            foreach (var s in block.Statements)
            {
                s.Accept(this, list);
            }

            return default;
        }

        public override object VisitVariableDecl(AstVariableDecl variable, List<SymbolInformation> list)
        {
            list.Add(new SymbolInformation
            {
                containerName = variable.Scope.Name,
                kind = SymbolKind.Variable,
                location = CastLocation(variable.Pattern.Location),
                name = variable.Pattern.ToString()
            });
            return default;
        }
    }
}
