using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Serilog.Expressions.Compilation
{
    class OrderedNameResolver : NameResolver
    {
        readonly NameResolver[] _orderedResolvers;

        public OrderedNameResolver(IEnumerable<NameResolver> orderedResolvers)
        {
            _orderedResolvers = orderedResolvers.ToArray();
        }

        public override bool TryResolveFunctionName(string name, [MaybeNullWhen(false)] out MethodInfo implementation)
        {
            foreach (var resolver in _orderedResolvers)
            {
                if (resolver.TryResolveFunctionName(name, out implementation))
                    return true;
            }

            implementation = null;
            return false;
        }
    }
}