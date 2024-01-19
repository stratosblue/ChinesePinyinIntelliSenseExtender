using System.Diagnostics;

namespace System.Text.StringTrie;

[DebuggerDisplay("{Value}")]
internal class ValueRef<T>
{
    public T Value;

    public ValueRef(T value)
    {
        Value = value;
    }
}