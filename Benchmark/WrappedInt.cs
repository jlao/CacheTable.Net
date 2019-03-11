using System;

namespace Benchmark
{
    struct WrappedInt : IEquatable<WrappedInt>
    {
        public int Value;

        public bool Equals(WrappedInt other) => this.Value == other.Value;

        public override int GetHashCode() => 42;

        public override bool Equals(object obj) => this.Equals((WrappedInt)obj);

        public static implicit operator WrappedInt(int i) => new WrappedInt { Value = i };
    }
}
