using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Extensions
{
    public static class StringExtensions
    {

        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static bool IsNotNullOrEmpty(this string text)
        {
            return !string.IsNullOrEmpty(text);
        }

        public static string ToStringMessage<T>(this IEnumerable<T> list)
        {
            var str = list.Aggregate("", (seed, value) => seed + $"{value}\r\n");

            return str.TrimEnd('\r', '\n');
        }

        public static string ToFormat(this string text, params object[] args)
        {
            return string.Format(text, args);
        }


        /// <summary>
        /// Returns truncate string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }


        /// <summary>
        /// Returns characters from right of specified length
        /// </summary>
        /// <param name="value">String value</param>
        /// <param name="length">Max number of charaters to return</param>
        /// <returns>Returns string from right</returns>
        public static string Right(this string value, int length)
        {
            return value != null && value.Length > length ? value.Substring(value.Length - length) : value;
        }

        /// <summary>
        /// Returns characters from left of specified length
        /// </summary>
        /// <param name="value">String value</param>
        /// <param name="length">Max number of charaters to return</param>
        /// <returns>Returns string from left</returns>
        public static string Left(this string value, int length)
        {
            return value != null && value.Length > length ? value.Substring(0, length) : value;
        }

        /// <summary>
        /// Try convert string to Int32. If impossible return 0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToInt32(this string value)
        {
            return Int32.TryParse(value, out int number) ? number : 0;
        }

        /// <summary>
        /// Try convert string to Datetime with format defined. If impossible return DateTime.MinValue
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string value, string format = "dd/MM/yyyy")
        {
            return DateTime.TryParseExact(value, format, null, DateTimeStyles.None, out DateTime parsedDate) ? parsedDate : DateTime.MinValue;
        }

        /// <summary>
        /// Return the last n characters. 
        /// Es. "Scanner" -> numChar = 3 -> "ner" 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="numChar"></param>
        /// <returns></returns>
        public static string LastChars(this string value, int numChar)
        {
            return value == null || numChar >= value.Length ? value : value.Substring(value.Length - numChar);
        }


        /// <summary>
        /// Return a part of string between startWord and endWord
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startWord"></param>
        /// <param name="endWord"></param>
        /// <returns></returns>
        public static string BetweenWord(this string value, string startWord, string endWord)
        {
            if (value.Contains(startWord) && value.Contains(endWord))
            {
                var startPos = value.IndexOf(startWord) + startWord.Length;
                var length = value.IndexOf(endWord) - startPos;
                return value.Substring(startPos, length);
            }
            else if (value.Contains(startWord))
            {
                var startPos = value.IndexOf(startWord) + startWord.Length;
                return value.Substring(startPos);
            }
            else if (value.Contains(endWord))
            {
                var length = value.IndexOf(endWord);
                return value.Substring(0, length);
            }

            return value;
        }



    

    }
}
