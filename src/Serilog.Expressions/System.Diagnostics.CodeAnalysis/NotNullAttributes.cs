#if NETSTANDARD2_0
//https://medium.com/@SergioPedri/enabling-and-using-c-9-features-on-older-and-unsupported-runtimes-ce384d8debb
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>Specifies that an output will not be null even if the corresponding type allows it. Specifies that an input argument was not null when the call returns.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed class NotNullAttribute : Attribute { }

    /// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter may be null even if the corresponding type disallows it.</summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class MaybeNullWhenAttribute : Attribute
    {
        /// <summary>Initializes the attribute with the specified return value condition.</summary>
        /// <param name="returnValue">
        /// The return value condition. If the method returns this value, the associated parameter may be null.
        /// </param>
        public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

        /// <summary>Gets the return value condition.</summary>
        public bool ReturnValue { get; }
    }

    // NOTE: you can find the full list of attributes in this gist:
    // https://gist.github.com/Sergio0694/eb988b243dd4a720a66fe369b63e5b08.
    // Keeping this one shorter so that the Medium embed doesn't take up too much space.
}
#endif