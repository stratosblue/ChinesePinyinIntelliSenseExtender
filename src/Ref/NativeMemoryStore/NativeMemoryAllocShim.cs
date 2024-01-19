#pragma warning disable CS8500

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System;

/// <summary>
/// 内存分配垫片
/// </summary>
internal static unsafe class NativeMemoryAllocShim
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TElement* Alloc<TElement>(int elementCount) where TElement : struct => (TElement*)Alloc(Convert.ToUInt32(elementCount), (uint)sizeof(TElement));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* Alloc(uint elementCount, uint elementSize)
    {
#if DEBUG
        var ptr = InternalAlloc(elementCount, elementSize);
        Debug.WriteLine($"[MemoryAlloc] Size: {elementCount * elementSize} at 0x{new IntPtr(ptr):x16}");
        return ptr;
#else
        return InternalAlloc(elementCount, elementSize);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free(void* ptr)
    {
        Debug.WriteLine($"[MemoryFree]: 0x{new IntPtr(ptr):x16}");
#if NET6_0_OR_GREATER
        NativeMemory.Free(ptr);
#else
        Marshal.FreeHGlobal(new IntPtr(ptr));
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* InternalAlloc(uint elementCount, uint elementSize)
    {
#if NET6_0_OR_GREATER
        return NativeMemory.Alloc(elementCount, elementSize);
#else
        var size = elementCount * elementSize;
        if (size < int.MaxValue)
        {
            return (void*)Marshal.AllocHGlobal((int)size);
        }
        throw new ArgumentException($"\"{size}\" is too large to alloc.");
#endif
    }
}