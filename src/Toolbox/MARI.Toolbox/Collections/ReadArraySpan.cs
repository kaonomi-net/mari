// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Collections
{
    using MARI.Toolbox.Memories;

    using System;

    /// <summary>
    /// Split a contigus memory in multi part and allow iteration on it
    /// </summary>
    public readonly ref struct ReadArraySpan<T>
    {
        #region Filds

        private readonly ReadOnlySpan<T> _source;
        private readonly Span<MemoryRange> _parts;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadArraySpan{T}"/> struct.
        /// </summary>
        public ReadArraySpan(Span<MemoryRange> parts, in ReadOnlySpan<T> source)
        {
            this._source = source;

            this._parts = parts;
            this.Length = parts.Length;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the <see cref="ReadOnlySpan{T}"/> at the specified index.
        /// </summary>
        public ReadOnlySpan<T> this[int index]
        {
            get
            {
                var range = this._parts[index];
                return this._source.Slice(range.Start, range.Length);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the part.
        /// </summary>
        public void AddPart(int index, int start, int length)
        {
            //this._parts[index] = new MemoryRange(start, length);
            ref var range = ref this._parts[index];
            range.Start = start;
            range.Length = length;
        }

        /// <summary>
        /// Simpl bubble Sort
        /// </summary>
        public void Sort(Func<MemoryRange, MemoryRange, bool> isAbove)
        {
            bool moved = true;

            while (moved)
            {
                moved = false;
                for (var i = 0; i + 1 < this.Length; i++)
                {
                    if (isAbove(this._parts[i], this._parts[i + 1]))
                    {
                        ref var sourceRange = ref this._parts[i];
                        ref var targetRange = ref this._parts[i + 1];

                        var tmpStart = sourceRange.Start;
                        var tmpLength = sourceRange.Length;

                        targetRange.Start = sourceRange.Start;
                        targetRange.Length = sourceRange.Length;

                        sourceRange.Start = tmpStart;
                        sourceRange.Length = tmpLength;
                        
                        moved = true;
                    }
                }
            }
        }

        #endregion
    }
}
