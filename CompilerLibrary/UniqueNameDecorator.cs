using Cheez.Ast.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    public interface INamed
    {
        AstIdExpr Name { get; }
    }

    public class UniqueNameDecorator
    {
        private Dictionary<object, Dictionary<object, string>> mScopeMap = new Dictionary<object, Dictionary<object, string>>();

        private object mCurrentScope = null;
        private Dictionary<object, string> mCurrentScopeMap = null;

        public void SetCurrentScope(object o)
        {
            mCurrentScope = o;

            if (!mScopeMap.ContainsKey(mCurrentScope))
            {
                mScopeMap[mCurrentScope] = new Dictionary<object, string>();
            }

            mCurrentScopeMap = mScopeMap[mCurrentScope];
        }

        public object GetCurrentScope()
        {
            return mCurrentScope;
        }

        public string GetDecoratedName(INamed named)
        {
            if (mCurrentScopeMap.ContainsKey(named))
                return mCurrentScopeMap[named];

            string name = named.Name.Name;

            while (mCurrentScopeMap.Values.Any(n => n == name))
            {
                int i = name.LastIndexOf("___");
                if (i < 0)
                {
                    name = $"{name}___1";
                    continue;
                }

                var part1 = name.Substring(0, i);
                var part2 = int.Parse(name.Substring(i + 3));
                name = $"{part1}___{part2 + 1}";
            }

            mCurrentScopeMap[named] = name;
            return name;
        }
    }
}
