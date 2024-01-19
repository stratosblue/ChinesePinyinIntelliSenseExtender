namespace System;

internal class MemoryBlockSpaceComparer<T>
    : IComparer<NativeMemoryBlock<T>>
    where T : struct
{
    public static MemoryBlockSpaceComparer<T> Instance { get; } = new();

    public int Compare(NativeMemoryBlock<T> x, NativeMemoryBlock<T> y) => x.FreeCount - y.FreeCount;
}
