// Copyright Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if NETSTANDARD2_0

using System;

namespace Serilog.Expressions
{
    /// <summary>
    /// Helper methods.
    /// </summary>
    internal static class Helpers
    {
        /// <summary>
        /// Backport .NET Standard 2.1 additions to maintain .NET Standard 2.0 compatibility.
        /// Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.
        ///
        /// From;
        /// https://github.com/dotnet/runtime/issues/22198
        /// https://stackoverflow.com/questions/444798/case-insensitive-containsstring/444818#444818
        /// </summary>
        /// <param name="source">input string</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType">Specifies the rule to use in the comparison.</param>
        /// <returns></returns>
        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source?.IndexOf(value, comparisonType) >= 0;
        }
    }
}
#endif