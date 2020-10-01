using Serilog.Expressions.Runtime;

namespace Serilog.Expressions.Compilation
{
    static class DefaultFunctionNameResolver
    {
        public static NameResolver Build(NameResolver? additionalNameResolver)
        {
            var defaultResolver = new StaticMemberNameResolver(typeof(RuntimeOperators));
            return additionalNameResolver == null
                ? (NameResolver) defaultResolver
                : new OrderedNameResolver(new[] {defaultResolver, additionalNameResolver });
        }
    }
}