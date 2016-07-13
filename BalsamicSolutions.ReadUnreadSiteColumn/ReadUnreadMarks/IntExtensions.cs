// -----------------------------------------------------------------------------
//This is free and unencumbered software released into the public domain.
//Anyone is free to copy, modify, publish, use, compile, sell, or
//distribute this software, either in source code form or as a compiled
//binary, for any purpose, commercial or non-commercial, and by any
//means.
//In jurisdictions that recognize copyright laws, the author or authors
//of this software dedicate any and all copyright interest in the
//software to the public domain.We make this dedication for the benefit
//of the public at large and to the detriment of our heirs and
//successors.We intend this dedication to be an overt act of
//relinquishment in perpetuity of all present and future rights to this
//software under copyright law.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
//OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
//OTHER DEALINGS IN THE SOFTWARE.
//For more information, please refer to<http://unlicense.org>
// ----------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
    /// <summary>
    /// common extensions for Int object types
    /// </summary>
    internal static class IntExtensions
    {
        static readonly string[] _Words0 = { "", "One ", "Two ", "Three ", "Four ", "Five ", "Six ", "Seven ", "Eight ", "Nine " };
        static readonly string[] _Words1 = { "Ten ", "Eleven ", "Twelve ", "Thirteen ", "Fourteen ", "Fifteen ", "Sixteen ", "Seventeen ", "Eighteen ", "Nineteen " };
        static readonly string[] _Words2 = { "Twenty ", "Thirty ", "Forty ", "Fifty ", "Sixty ", "Seventy ", "Eighty ", "Ninety " };
        static readonly string[] _Words3 = { "Thousand ", "Million ", "Billion " };

        /// <summary>
        /// return an English text equilivant of the number
        /// </summary>
        /// <param name="thisNum"></param>
        /// <returns></returns>
        public static string ToText(this int thisNum)
        {
            return NumberToText(thisNum);
        }

        /// <summary>
        ///  return an English text equilivant of the number
        /// </summary>
        /// <param name="textifyMe"></param>
        /// <returns></returns>
        public static string NumberToText(int textifyMe)
        {
            return NumberToText(textifyMe, false);
        }

        /// <summary>
        ///  return an English text equilivant of the number, optionally
        ///  using UK conventions
        /// </summary>
        /// <param name="textifyMe"></param>
        /// <param name="isUK"></param>
        /// <returns></returns>
        public static string NumberToText(int textifyMe, bool isUK)
        {
            if (textifyMe == int.MaxValue || textifyMe == int.MinValue)
                throw new ArgumentOutOfRangeException("textifyMe");
            if (textifyMe == 0)
                return "Zero";
            string and = isUK ? "and " : ""; // deals with UK or US numbering
            int[] numberParts = new int[4];
            int firstValidNumberPos = 0;
            int unitsPos, hundredsPos, thousandsPos;
            System.Text.StringBuilder returnValue = new System.Text.StringBuilder();
            if (textifyMe < 0)
            {
                if (isUK)
                {
                    returnValue.Append("Minus ");
                }
                else
                {
                    returnValue.Append("Negative ");
                }
                textifyMe = -textifyMe;
            }

            numberParts[0] = textifyMe % 1000;           // units
            numberParts[1] = textifyMe / 1000;
            numberParts[2] = textifyMe / 1000000;
            numberParts[1] = numberParts[1] - 1000 * numberParts[2];  // thousands
            numberParts[3] = textifyMe / 1000000000;     // billions
            numberParts[2] = numberParts[2] - 1000 * numberParts[3];  // millions
            for (int numPartIdx = 3; numPartIdx > 0; numPartIdx--)
            {
                if (numberParts[numPartIdx] != 0)
                {
                    firstValidNumberPos = numPartIdx;
                    break;
                }
            }
            for (int numPartIdx = firstValidNumberPos; numPartIdx >= 0; numPartIdx--)
            {
                if (numberParts[numPartIdx] == 0)
                    continue;
                unitsPos = numberParts[numPartIdx] % 10;              // ones
                thousandsPos = numberParts[numPartIdx] / 10;
                hundredsPos = numberParts[numPartIdx] / 100;             // hundreds
                thousandsPos = thousandsPos - 10 * hundredsPos;               // tens
                if (hundredsPos > 0)
                    returnValue.Append(_Words0[hundredsPos] + "Hundred ");
                if (unitsPos > 0 || thousandsPos > 0)
                {
                    if (hundredsPos > 0 || numPartIdx < firstValidNumberPos)
                        returnValue.Append(and);
                    if (thousandsPos == 0)
                        returnValue.Append(_Words0[unitsPos]);
                    else if (thousandsPos == 1)
                        returnValue.Append(_Words1[unitsPos]);
                    else

                        returnValue.Append(_Words2[thousandsPos - 2] + _Words0[unitsPos]);
                }
                if (numPartIdx != 0)
                    returnValue.Append(_Words3[numPartIdx - 1]);
            }
            return returnValue.ToString().TrimEnd();
        }
    }
}