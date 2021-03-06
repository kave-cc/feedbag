﻿/*
 * Copyright 2014 Technische Universität Darmstadt
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *    http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text.RegularExpressions;
using KaVE.Commons.Utils;
using KaVE.JetBrains.Annotations;
using KaVE.RS.Commons.Utils;

namespace KaVE.VS.FeedbackGenerator.SessionManager.Presentation
{
    public static class JsonSyntaxHighlighter
    {
        [NotNull]
        public static string AddJsonSyntaxHighlightingWithXaml([NotNull] this string json)
        {
            var escapedJson = json.EncodeSpecialChars();
            return Regex.Replace(
                escapedJson,
                @"(""(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\""])*""(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)",
                match => CreateReplacement(match.Value));
        }

        private static string CreateReplacement(string match)
        {
            if (IsStringConstant(match))
            {
                return IsPropertyKey(match) ? FormatPropertyKey(match) : FormatStringConstant(match);
            }
            if (IsBooleanConstant(match))
            {
                return FormatBooleanConstant(match);
            }
            if (IsNullConstant(match))
            {
                return FormatNullConstant(match);
            }
            return FormatNumberConstant(match);
        }

        private static string FormatStringConstant(string match)
        {
            return match;
        }

        private static string FormatPropertyKey(string match)
        {
            return XamlFormattingUtil.Bold(match);
        }

        private static string FormatBooleanConstant(string match)
        {
            return match;
        }

        private static string FormatNullConstant(string match)
        {
            return match;
        }

        private static string FormatNumberConstant(string match)
        {
            return match;
        }

        private static bool IsStringConstant(string match)
        {
            return Regex.IsMatch(match, "^\"");
        }

        private static bool IsPropertyKey(string match)
        {
            return Regex.IsMatch(match, ":$");
        }

        private static bool IsBooleanConstant(string match)
        {
            return Regex.IsMatch(match, "true|false");
        }

        private static bool IsNullConstant(string match)
        {
            return Regex.IsMatch(match, "null");
        }
    }
}