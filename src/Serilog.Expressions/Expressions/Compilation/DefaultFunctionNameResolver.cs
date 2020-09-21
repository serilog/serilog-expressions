using System.Collections.Generic;
using System.Linq;
using Serilog.Expressions.Runtime;

namespace Serilog.Expressions.Compilation
{
    static class DefaultFunctionNameResolver
    {
        public static NameResolver Build(IEnumerable<NameResolver>? orderedResolvers)
        {
            var defaultResolver = new StaticMemberNameResolver(typeof(RuntimeOperators));
            return orderedResolvers == null
                ? (NameResolver) defaultResolver
                : new OrderedNameResolver(
                    new NameResolver[] {defaultResolver}.Concat(orderedResolvers));
        }
    }
}