namespace Serilog.Expressions.Runtime
{
    sealed class Undefined
    {
        public static readonly Undefined Value = new Undefined();

        Undefined() { }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object obj)
        {
            return false;
        }
    }
}
