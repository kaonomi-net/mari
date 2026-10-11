// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace System
{
    using MARI.Toolbox.Collections;
    using MARI.Toolbox.Memories;
    using MARI.Toolbox.Models;

    using Microsoft.Extensions.Options;
    using Microsoft.VisualBasic;

    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Extensions methods about <see cref="string"/>
    /// </summary>
    public static class StringExtensions
    {
        #region Methods

        /// <summary>
        /// Extensions to simplify the call of <see cref="string.Format"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string WithArguments(this string str, params object?[] arguments)
        {
            return string.Format(str, arguments);
        }

        /// <summary>
        /// OptiSplit the string using multiple string as separators with <paramref name="includeSeparator"/> you could keep the separator in the result
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<string> OptiSplit(this string str,
                                                      params string[] separators)
        {
            return OptiSplit(str,
                             StringIncludeSeparatorMode.None,
                             StringComparison.Ordinal,
                             StringSplitOptions.None,
                             separators);
        }

        /// <summary>
        /// OptiSplit the string using multiple string as separators with <paramref name="includeSeparator"/> you could keep the separator in the result
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<string> OptiSplit(this string str,
                                                      StringComparison comparison,
                                                      StringSplitOptions splitOptions,
                                                      params string[] separators)
        {
            return OptiSplit(str,
                             StringIncludeSeparatorMode.None,
                             comparison,
                             splitOptions,
                             separators);
        }

        /// <summary>
        /// OptiSplit the string using multiple string as separators with <paramref name="includeSeparator"/> you could keep the separator in the result
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<string> OptiSplit(this string str,
                                                      StringIncludeSeparatorMode includeSeparator,
                                                      params string[] separators)
        {
            return OptiSplit(str,
                             includeSeparator,
                             StringComparison.Ordinal,
                             StringSplitOptions.None,
                             separators);
        }

        /// <summary>
        /// OptiSplit the string using multiple string as separators with <paramref name="includeSeparator"/> you could keep the separator in the result
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<string> OptiSplit(this string str,
                                                      StringComparison comparison,
                                                      params string[] separators)
        {
            return OptiSplit(str,
                             StringIncludeSeparatorMode.None,
                             comparison,
                             StringSplitOptions.None,
                             separators);
        }

        /// <summary>
        /// OptiSplit the string using multiple string as separators with <paramref name="includeSeparator"/> you could keep the separator in the result
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<string> OptiSplit(this string str,
                                                      StringSplitOptions splitOptions,
                                                      params string[] separators)
        {
            return OptiSplit(str,
                             StringIncludeSeparatorMode.None,
                             StringComparison.Ordinal,
                             splitOptions,
                             separators);
        }

        /// <summary>
        /// OptiSplit the string using multiple string as separators with <paramref name="includeSeparator"/> you could keep the separator in the result
        /// </summary>
        [Obsolete("Use version with StringIncludeSeparatorMode includeSeparator")]
        public static IReadOnlyList<string> OptiSplit(this string str,
                                                      bool includeSeparator,
                                                      StringComparison comparison,
                                                      StringSplitOptions splitOptions,
                                                      params string[] separators)
        {
            return OptiSplit(str,
                             includeSeparator ? StringIncludeSeparatorMode.Isolated : StringIncludeSeparatorMode.None,
                             comparison,
                             splitOptions,
                             separators);
        }

        /// <summary>
        /// OptiSplit the string using multiple string as separators with <paramref name="includeSeparator"/> you could keep the separator in the result
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<string> OptiSplit(this string str,
                                                      StringIncludeSeparatorMode includeSeparator,
                                                      StringComparison comparison,
                                                      StringSplitOptions splitOptions,
                                                      params string[] separators)

        {
            return OptiSplit(str,
                             includeSeparator,
                             splitOptions,
                             GetSeparatorIndex(separators, EnumerableHelper<string>.ReadOnlyArray, comparison));
        }

        /// <summary>
        /// OptiSplit the string using multiple string as separators with <paramref name="includeSeparator"/> you could keep the separator in the result
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<string> OptiSplit(this string str,
                                                      StringIncludeSeparatorMode includeSeparator,
                                                      StringComparison comparison,
                                                      StringSplitOptions splitOptions,
                                                      IReadOnlyCollection<string> separatorsInclude,
                                                      IReadOnlyCollection<string> separatorsExclude)

        {
            return OptiSplit(str,
                             includeSeparator,
                             splitOptions,
                             GetSeparatorIndex(separatorsInclude, separatorsExclude, comparison));
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<string> OptiSplit(this string str,
                                                      StringIncludeSeparatorMode includeSeparator,
                                                      StringSplitOptions splitOptions,
                                                      StringIndexedContext context)
        {
            if (str is null)
                return EnumerableHelper<string>.ReadOnlyArray;

            if (str.Length == 0)
                return str.AsEnumerable().ToReadOnlyList();

            return OptiSplitImpl(str, includeSeparator, splitOptions, context);
        }

        /// <summary>
        /// Converts to lower case with <paramref name="separator"/> set before any old upper.
        /// </summary>
        public static string ToLowerWithSeparator(this string value, char separator)
        {
            return ToLowerWithSeparator((ReadOnlySpan<char>)value, separator);
        }

        /// <summary>
        /// Converts to lower case with <paramref name="separator"/> set before any old upper.
        /// </summary>
        public static string ToLowerWithSeparator(this ReadOnlySpan<char> value, char separator)
        {
            Span<char> span = stackalloc char[value.Length * 2];
            int index = 0;

            foreach (char c in value)
            {
                var newChar = c;

                if (char.IsUpper(c))
                {
                    if (index > 0)
                    {
                        span[index] = separator;
                        index++;
                    }
                    newChar = char.ToLower(c);
                }
                span[index] = newChar;
                index++;
            }

            return span.Slice(0, index).ToString();
        }

        #region Tools

        /// <summary>
        /// 
        /// </summary>
        private static IReadOnlyList<string> OptiSplitImpl(this string str,
                                                           StringIncludeSeparatorMode includeSeparator,
                                                           StringSplitOptions splitOptions,
                                                           StringIndexedContext context)
        {
            bool attachedToPrevious = includeSeparator == StringIncludeSeparatorMode.AttachedToPrevious;
            int lastStartPoint = 0;

            var results = new List<string>(Math.Min(100, str.Length));
            ReadOnlySpan<char> source = str;

            for (var i = 0; i < str.Length; i++)
            {
                if (context.Search(source, i, out var included, out var deepFounded))
                {
                    // When execluded just pass the area inpected
                    if (included == false)
                    {
                        i += deepFounded!.Value - 1;
                        continue;
                    }

                    var length = i - lastStartPoint;

                    i += deepFounded!.Value - 1;

                    if (attachedToPrevious)
                        length += deepFounded!.Value;

                    if (length > 0)
                    {
                        var remain = ComputeString(str, splitOptions, lastStartPoint, source, length);

                        lastStartPoint += length;

                        if (remain is not null)
                            results.Add(remain);
                    }

                    if (attachedToPrevious)
                        continue;

                    if (includeSeparator != StringIncludeSeparatorMode.None)
                    {
                        var separatorFounded = ComputeString(str, splitOptions, lastStartPoint, source, deepFounded!.Value);

                        if (separatorFounded is not null)
                            results.Add(separatorFounded);
                    }

                    lastStartPoint += deepFounded!.Value;
                }
            }

            if (lastStartPoint < str.Length)
            {
                var separatorFounded = ComputeString(str, splitOptions, lastStartPoint, source, str.Length - lastStartPoint);

                if (separatorFounded is not null)
                    results.Add(separatorFounded);
            }

            return results;
        }

        private static string? ComputeString(string str, StringSplitOptions splitOptions, int startPoint, in ReadOnlySpan<char> source, int length)
        {
            if (splitOptions == StringSplitOptions.None)
                return str.Substring(startPoint, length);

            var resultSpan = source.Slice(startPoint, length);

            if (splitOptions == StringSplitOptions.RemoveEmptyEntries && (resultSpan.IsEmpty || resultSpan.IsWhiteSpace()))
                return null;

            if (splitOptions == StringSplitOptions.TrimEntries)
                resultSpan = resultSpan.Trim();

            if (resultSpan.Length == 0)
                return null;

            return resultSpan.ToString();
        }

        ///// <summary>
        ///// Insert in the collection if allaw by the options
        ///// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static void LessInsertInResult(in IList<string> results, in ReadOnlySpan<char> remainStr, int index, int size, StringSplitOptions options)
        //{
        //    if (size == 0)
        //        return;

        //    string str;

        //    if ((options & StringSplitOptions.TrimEntries) == StringSplitOptions.TrimEntries)
        //        str = remainStr.Slice(index, size).Trim().ToString();
        //    else
        //        str = remainStr.Slice(index, size).ToString();

        //    if (str.Length == 0 || ((options & StringSplitOptions.RemoveEmptyEntries) == StringSplitOptions.RemoveEmptyEntries && string.IsNullOrWhiteSpace(str)))
        //        return;

        //    results.Add(str);
        //}

        ///// <summary>
        ///// OptiSplit the string using multiple string as separators with <paramref name="includeSeparator"/> you could keep the separator in the result
        ///// </summary>
        //private static IReadOnlyList<string> OptiSplitImpl(this string str,
        //                                                   StringIncludeSeparatorMode includeSeparator,
        //                                                   StringComparison comparison,
        //                                                   StringSplitOptions splitOptions,
        //                                                   IReadOnlyCollection<string> separators,
        //                                                   IReadOnlyCollection<string> excludeSeparators)

        //{
        //    if (separators.Count == 0)
        //        return str.AsEnumerable().ToReadOnlyList();

        //    if (string.IsNullOrEmpty(str))
        //        return EnumerableHelper<string>.ReadOnlyArray;

        //    var ordersSeparators = separators.OrderByDescending(s => s.Length).ToArray();

        //    ReadOnlySpan<char> remainStr = str;

        //    if ((splitOptions & StringSplitOptions.TrimEntries) == StringSplitOptions.TrimEntries)
        //        remainStr = remainStr.Trim();

        //    var source = remainStr;

        //    var index = -1;
        //    var size = 0;
        //    int resultCount = 0;

        //    Span<MemoryRange> resultRanges = stackalloc MemoryRange[Math.Min(65536, str.Length)]; // (512 k)

        //    List<string>? results = null;

        //    int relativeStartIndx = 0;

        //    while (true)
        //    {
        //        index = -1;
        //        size = 0;

        //        foreach (var separator in ordersSeparators)
        //        {
        //            var localIndex = remainStr.IndexOf(separator, comparison);
        //            if (localIndex > -1 && (index < 0 || localIndex < index))
        //            {
        //                index = localIndex;
        //                size = separator.Length;
        //            }
        //        }

        //        if (index < 0 || size == 0)
        //            break;

        //        if (includeSeparator == StringIncludeSeparatorMode.AttachedToPrevious)
        //        {
        //            index = index + size;
        //            size = 0;
        //        }

        //        if (index > 0)
        //        {
        //            if (InsertInResult(ref resultRanges[resultCount], remainStr, 0, index, splitOptions, relativeStartIndx))
        //                resultCount++;
        //        }

        //        if (resultCount >= resultRanges.Length)
        //        {
        //            var exceedResults = ComputeResults(splitOptions, source, resultCount, resultRanges);
        //            results = results.AddRangeOnNull(exceedResults, exceedResults.Length);

        //            resultCount = 0;
        //        }

        //        if (includeSeparator == StringIncludeSeparatorMode.Isolated)
        //        {
        //            if (InsertInResult(ref resultRanges[resultCount], remainStr, index, size, splitOptions, relativeStartIndx))
        //                resultCount++;
        //        }

        //        if (resultCount >= resultRanges.Length)
        //        {
        //            var exceedResults = ComputeResults(splitOptions, source, resultCount, resultRanges);
        //            results = results.AddRangeOnNull(exceedResults, exceedResults.Length);

        //            resultCount = 0;
        //        }

        //        remainStr = remainStr.Slice(index + size);

        //        relativeStartIndx += index + size;
        //    }

        //    if (remainStr.Length > 0)
        //    {
        //        ref var remainIndx = ref resultRanges[resultCount++];
        //        remainIndx.Start = relativeStartIndx;
        //        remainIndx.Length = remainStr.Length;
        //    }

        //    var resultArray = ComputeResults(splitOptions, source, resultCount, resultRanges);

        //    if (results is not null && results.Count > 0)
        //    {
        //        results.AddRange(resultArray);
        //        return results;
        //    }

        //    return resultArray;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string[] ComputeResults(StringSplitOptions splitOptions, ReadOnlySpan<char> source, int resultCount, Span<MemoryRange> resultRanges)
        {
            var resultArray = new string[resultCount];
            for (int i = 0; i < resultCount; i++)
            {
                ref var range = ref resultRanges[i];

                var slice = source.Slice(range.Start, range.Length);

                if ((splitOptions & StringSplitOptions.TrimEntries) == StringSplitOptions.TrimEntries)
                    slice = slice.Trim();

                resultArray[i] = slice.ToString();
            }

            return resultArray;
        }

        /// <summary>
        /// Insert in the collection if allaw by the options
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool InsertInResult(ref MemoryRange toSet, in ReadOnlySpan<char> remainStr, int index, int size, StringSplitOptions options, int relativeStartIndx)
        {
            if (size == 0)
                return false;

            var result = remainStr.Slice(index, size);

            if ((options & StringSplitOptions.TrimEntries) == StringSplitOptions.TrimEntries)
            {
                var trimResult = result.TrimStart();

                relativeStartIndx += result.Length - trimResult.Length;
                result = trimResult.TrimEnd();
            }

            if (result.Length == 0 || ((options & StringSplitOptions.RemoveEmptyEntries) == StringSplitOptions.RemoveEmptyEntries && result.IsWhiteSpace()))
                return false;

            toSet.Start = index + relativeStartIndx;
            toSet.Length = result.Length;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static StringIndexedContext GetSeparatorIndex(IReadOnlyCollection<string> included,
                                                              IReadOnlyCollection<string> excluded,
                                                              StringComparison stringComparison)
        {
            EqualityComparer<char>? comparer = null;

            switch (stringComparison)
            {
                case StringComparison.OrdinalIgnoreCase:
                    comparer = EqualityComparer<char>.Create((a, b) => char.ToLower(a) == char.ToLower(b),
                                                             c => char.ToLower(c).GetHashCode());
                    break;

                case StringComparison.CurrentCultureIgnoreCase:
                    comparer = EqualityComparer<char>.Create((a, b) => CultureInfo.CurrentCulture.TextInfo.ToLower(a) == CultureInfo.CurrentCulture.TextInfo.ToLower(b),
                                                             c => CultureInfo.CurrentCulture.TextInfo.ToLower(c).GetHashCode());
                    break;

                case StringComparison.InvariantCultureIgnoreCase:
                    comparer = EqualityComparer<char>.Create((a, b) => CultureInfo.InvariantCulture.TextInfo.ToLower(a) == CultureInfo.InvariantCulture.TextInfo.ToLower(b),
                                                             c => CultureInfo.InvariantCulture.TextInfo.ToLower(c).GetHashCode());
                    break;
            }

            return StringIndexedContext.Create(included, excluded, comparer);
        }

        #endregion

        #endregion
    }
}
