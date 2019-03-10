namespace UnitTests
{
    struct BadHashCode
    {
        public int Value;

        public override int GetHashCode() => 42;

        public static implicit operator BadHashCode(int i) => new BadHashCode { Value = i };

        public static implicit operator int(BadHashCode bhc) => bhc.Value;
    }
}
