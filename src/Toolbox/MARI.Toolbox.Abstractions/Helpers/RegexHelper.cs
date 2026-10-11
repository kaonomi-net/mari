// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Helpers
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper Around regex
    /// </summary>
    public static class RegexHelper
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        static RegexHelper()
        {
            MultiSpace = new Regex(Pattern.MULTI_SPACE, RegexOptions.Compiled);
            Base64 = new Regex(Pattern.BASE64, RegexOptions.Compiled);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the multi space.
        /// </summary>
        public static Regex MultiSpace { get; }

        /// <summary>
        /// Gets the base64 validation regex
        /// </summary>
        public static Regex Base64 { get; }

        #endregion

        #region Nested

        /// <summary>
        /// Define regex classic pattern
        /// </summary>
        public static class Pattern
        {
            /// <summary>
            /// The unique identifier pattern
            /// </summary>
            public const string GUID = "[a-zA-Z0-9]{8}[-]?[a-zA-Z0-9]{4}[-]?[a-zA-Z0-9]{4}[-]?[a-zA-Z0-9]{4}[-]?[a-zA-Z0-9]{12}";

            /// <summary>
            /// Pattern to identify or validate a base64 string
            /// </summary>
            /// <remarks>
            ///     Attention start and end flag '^$' are not inserted
            /// </remarks>
            public const string BASE64 = "(?:[A-Za-z0-9+\\/]{4})*(?:[A-Za-z0-9+\\/]{2}==|[A-Za-z0-9+\\/]{3}=|[A-Za-z0-9+\\/]{4})";

            /// <summary>
            /// The multi space
            /// </summary>
            public const string MULTI_SPACE = "[\\s]+";
        }

        #endregion
    }
}
