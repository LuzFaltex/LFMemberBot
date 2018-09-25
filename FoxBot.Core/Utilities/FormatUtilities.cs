using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace FoxBot.Core.Utilities
{
    public static class FormatUtilities
    {
        private static readonly Regex _buildContentRegex = new Regex(@"```([^\s]+|)");

        /// <summary>
        /// Prepares a piece of code for processing
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string BuildContent(string code)
        {
            return StripFormatting(code);
        }

        /// <summary>
        /// Prepares a piece of input code for use in HTTP operations
        /// </summary>
        /// <param name="code">The code to prepare</param>
        /// <returns>The resulting StringContent for HTTP operations</returns>
        public static StringContent ToHTMLSafeString(this string code)
        {
            return new StringContent(BuildContent(code), Encoding.UTF8, "text/plain");
        }

        /// <summary>
        /// Attempts to get the language of the code piece
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns>The code language if a match is found, null of none are found</returns>
        public static string GetCodeLanguage(string message)
        {
            var match = _buildContentRegex.Match(message);
            if (match.Success)
            {
                string codeLanguage = match.Groups[1].Value;
                return string.IsNullOrEmpty(codeLanguage) ? null : codeLanguage;
            }
            else
            {
                return null;
            }
        }

        public static string StripFormatting(string code)
        {
            
            var cleanCode = _buildContentRegex.Replace(code.Trim(), string.Empty); // Strip out the ` characters and code block markers
            cleanCode.Replace("\t", "    "); // Tabs => spaces
            cleanCode = FixIndentation(cleanCode);
            return cleanCode;
        }

        /// <summary>
        /// Attempts to fix the indentation of a piece of code by aligning the left side
        /// </summary>
        /// <param name="code">The code to align</param>
        /// <returns>The newly aligned code</returns>
        public static string FixIndentation(string code)
        {
            var lines = code.Split('\n');
            var indentLine = lines.SkipWhile(d => d.FirstOrDefault() != ' ').FirstOrDefault();

            if (indentLine != null)
            {
                var indent = indentLine.LastIndexOf(' ') + 1;

                var pattern = $@"^[^\S\n]{{{indent}}}";

                return Regex.Replace(code, pattern, "", RegexOptions.Multiline);
            }

            return code;
        }
    }
}
