﻿using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SixLabors.ImageSharp.Memory
{
    /// <summary>
    /// Implements <see cref="MemoryManager"/> by allocating memory from <see cref="ArrayPool{T}"/>.
    /// </summary>
    public class ArrayPoolMemoryManager : MemoryManager
    {
        /// <summary>
        /// Defines the default maximum size of pooled arrays.
        /// Currently set to a value equivalent to 16 Megapixels of an <see cref="Rgba32"/> image.
        /// </summary>
        public const int DefaultMaxSizeInBytes = 4096 * 4096 * 4;

        private readonly ArrayPool<byte> pool;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayPoolMemoryManager"/> class.
        /// </summary>
        public ArrayPoolMemoryManager()
        {
            this.pool = ArrayPool<byte>.Create(DefaultMaxSizeInBytes, 50);
        }

        /// <inheritdoc />
        internal override Buffer<T> Allocate<T>(int itemCount)
        {
            return this.Allocate<T>(itemCount, false);
        }

        /// <inheritdoc />
        internal override Buffer<T> Allocate<T>(int itemCount, bool clear)
        {
            int itemSizeBytes = Unsafe.SizeOf<T>();
            int bufferSizeInBytes = itemCount * itemSizeBytes;

            byte[] byteBuffer = this.pool.Rent(bufferSizeInBytes);
            var buffer = new Buffer<T>(Unsafe.As<T[]>(byteBuffer), itemCount, this);
            if (clear)
            {
                buffer.Clear();
            }

            return buffer;
        }

        /// <inheritdoc />
        internal override void Release<T>(Buffer<T> buffer)
        {
            byte[] byteBuffer = Unsafe.As<byte[]>(buffer.Array);
            this.pool.Return(byteBuffer);
        }
    }
}