using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Serilog.Expressions
{
    /// <summary>
    /// A <see cref="NameResolver"/> that matches public static members of a class by name.
    /// </summary>
    public class StaticMemberNameResolver : NameResolver
    {
        readonly IReadOnlyDictionary<string, MethodInfo> _methods;

        /// <summary>
        /// Create a <see cref="StaticMemberNameResolver"/> that returns members of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/> with public static members implementing runtime functions.</param>
        public StaticMemberNameResolver(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            _methods = type
                .GetTypeInfo()
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
        }
        
        /// <inheritdoc />
        public override bool TryResolveFunctionName(string name, out MethodInfo implementation)
        {
            return _methods.TryGetValue(name, out implementation);
        }
    }
}