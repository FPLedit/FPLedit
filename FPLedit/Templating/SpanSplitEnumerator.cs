#nullable enable
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace FPLedit.Templating
{
    /// <summary>
    /// <see cref="SpanSplitEnumerator{T}"/> allows for enumeration of each element within a <see cref="System.ReadOnlySpan{T}"/>
    /// that has been split using a provided separator.
    /// </summary>
    internal ref struct SpanSplitEnumerator<T> where T : IEquatable<T>
    {
        private readonly ReadOnlySpan<T> buffer;
        private readonly ReadOnlySpan<T> separators;

        private int startCurrent;
        private int endCurrent;
        private int startNext;

        /// <summary>
        /// Returns an enumerator that allows for iteration over the split span.
        /// </summary>
        /// <returns>Returns a <see cref="SpanSplitEnumerator{T}"/> that can be used to iterate over the split span.</returns>
        public SpanSplitEnumerator<T> GetEnumerator() => this;

        /// <summary>
        /// Returns the current element of the enumeration.
        /// </summary>
        /// <returns>Returns a <see cref="System.Range"/> instance that indicates the bounds of the current element withing the source span.</returns>
        public Range Current => new Range(startCurrent, endCurrent);

        public SpanSplitEnumerator(ReadOnlySpan<T> span, ReadOnlySpan<T> separators)
        {
            buffer = span;
            this.separators = separators;
            startCurrent = 0;
            endCurrent = 0;
            startNext = 0;
        }

        /// <summary>
        /// Advances the enumerator to the next element of the enumeration.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator was successfully advanced to the next element; <see langword="false"/> if the enumerator has passed the end of the enumeration.</returns>
        public bool MoveNext()
        {
            if (startNext > buffer.Length)
                return false;

            ReadOnlySpan<T> slice = buffer.Slice(startNext);
            startCurrent = startNext;

            int separatorIndex = -1;
            for (int i = 0; i < separators.Length; i++)
            {
                int cmpIndex = slice.IndexOf(separators[i]);
                if (cmpIndex != -1 && (cmpIndex < separatorIndex || separatorIndex == -1))
                    separatorIndex = cmpIndex;
            }
            int elementLength = (separatorIndex != -1 ? separatorIndex : slice.Length);

            endCurrent = startCurrent + elementLength;
            startNext = endCurrent + 1;
            return true;
        }
    }
}
