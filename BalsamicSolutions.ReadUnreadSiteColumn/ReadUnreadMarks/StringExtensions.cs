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
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// common string extensions
	/// </summary>
	internal static class StringExtensions
	{
		public static readonly Regex RegExNumericMatchPattern = new Regex(@"^(([0-9]*)|(([0-9]*).([0-9]*)))$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// return true if the character is a control char
		/// </summary>
		/// <param name="thisChar"></param>
		/// <returns></returns>
		public static bool IsControlCharacter(this char thisChar)
		{
			if (thisChar == '\r')
			{
				return true;
			}
			if (thisChar == '\n')
			{
				return true;
			}
			if (thisChar == '\t')
			{
				return true;
			}
			if (thisChar == '\b')
			{
				return true;
			}
			if (thisChar == '\0')
			{
				return true;
			}
			return false;
		}

		public static bool CaseInsensitiveEquals(this String thisStr, string compareTo)
		{
			if (null == thisStr && null == compareTo)
			{
				return true;
			}
			if (null == thisStr || null == compareTo)
			{
				return false;
			}
			return thisStr.Equals(compareTo, StringComparison.InvariantCultureIgnoreCase);
		}

		public static bool CaseInsensitiveContains(this String thisStr, string compareTo)
		{
			if (null == thisStr && null == compareTo)
			{
				return true;
			}
			if (null == thisStr || null == compareTo)
			{
				return false;
			}
			return thisStr.IndexOf(compareTo, StringComparison.InvariantCultureIgnoreCase) > -1;
		}

		/// <summary>
		/// Removes all html tags returning only the valid text
		/// </summary>
		/// <param name="thisStr"></param>
		/// <returns></returns>
		public static string StripTags(this string thisStr)
		{
			return thisStr.StripTags(-1);
		}

		/// <summary>
		/// Removes all html tags returning only the valid text
		/// </summary>
		/// <param name="thisStr"></param>
		/// <param name="maxLength"></param>
		/// <returns></returns>
		public static string StripTags(this string thisStr, int maxLength)
		{
			if (null == thisStr)
			{
				return string.Empty;
			}
			if (maxLength == -1)
			{
				maxLength = thisStr.Length;
			}
			char[] allChars = new char[maxLength];
			int arrayIndex = 0;
			bool inTag = false;

			for (int charPos = 0; charPos < thisStr.Length; charPos++)
			{
				if (charPos >= maxLength)
				{
					break;
				}
				char currentChar = thisStr[charPos];
				if (currentChar == '<')
				{
					inTag = true;
					continue;
				}
				if (currentChar == '>')
				{
					inTag = false;
					if (arrayIndex > 0 && allChars[arrayIndex] != ' ')
					{
						allChars[arrayIndex] = ' ';
						arrayIndex++;
					}
					continue;
				}
				if (!inTag)
				{
					allChars[arrayIndex] = currentChar;
					arrayIndex++;
				}
			}
			return new string(allChars, 0, arrayIndex);
		}

		/// <summary>
		/// simple JavaScript compression, strips comments
		/// and white space, variable names are left alone as
		/// they are neccssary for global uniqueness.
		/// This seems to be a good bit faster then RegEx 
		/// </summary>
		/// <param name="thisStr"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static string MinifyJavaScript(this string thisStr)
		{
			
			if (string.IsNullOrEmpty(thisStr))
			{
				return thisStr;
			}
			char[] returnValue = new char[thisStr.Length];
			int returnValueIndex = 0;
			bool inQuote = false;
			bool inSingleQuote = false;
			bool inCommentLine = false;
			bool inlineComment = false;
			bool ignoreNextChar = false;

			int charLength = thisStr.Length - 1;
			for (int charPos = 0; charPos <= charLength; charPos++)
			{
				if (ignoreNextChar)
				{
					ignoreNextChar = false;
					continue;
				}
				char currentChar = thisStr[charPos];
				char nextChar = charPos < charLength ? thisStr[charPos + 1] : '\0';
				char prevChar = charPos > 0 ? thisStr[charPos - 1] : '\0';
				if (!inCommentLine && !inlineComment)
				{
					if (!inSingleQuote && !inQuote)
					{
						//test for /* and for //
						if (currentChar == '/')
						{
							if (nextChar == '*')
							{
								inlineComment = true;
								ignoreNextChar = true;
								continue;
							}
							else if (nextChar == '/')
							{
								inCommentLine = true;
								ignoreNextChar = true;
								continue;
							}
						}
					}
					if (currentChar == '\'' && prevChar != '\\')
					{
						if (!inQuote)
						{
							inSingleQuote = !inSingleQuote;
						}
					}
					if (currentChar == '"' && prevChar != '\\')
					{
						if (!inSingleQuote)
						{
							inQuote = !inQuote;
						}
					}
				}
				else if (inCommentLine)
				{
					//looking for the end of the line 
					if (currentChar == '\r' || currentChar == '\n')
					{
						inCommentLine = false;
						ignoreNextChar = (nextChar == '\r' || nextChar == '\n');
					}
					continue;
				}
				else if (inlineComment)
				{
					/*looking for end comment tag*/
					if (currentChar == '*' && nextChar == '/')
					{
						inlineComment = false;
						ignoreNextChar = true;
					}
					continue;
				}

				//Ok now we know where we are so process the character
				if (inQuote || inSingleQuote)
				{
					returnValue[returnValueIndex] = currentChar;
					returnValueIndex++;
				}
				else
				{
					if (currentChar.IsControlCharacter())
					{
						//drop it by doing nothing
						continue;
					}
					else if (currentChar == ' ')
					{
						if (prevChar == ' ')
						{
							//no white space allowed unless in quotes
							continue;
						}
						else
						{
							returnValue[returnValueIndex] = currentChar;
							returnValueIndex++;
						}
					}
					else
					{
						returnValue[returnValueIndex] = currentChar;
						returnValueIndex++;
					}
				}
			}
			return new string(returnValue, 0, returnValueIndex);
		}

		/// <summary>
		/// Simple Html compression, strips out white spaces
		/// but leaves the Html formatting alone
		/// This seems to be a good bit faster then RegEx 
		/// </summary>
		/// <param name="thisStr"></param>
		/// <param name="maxLength"></param>
		/// <returns></returns>
		public static string MinifyHtml(this string thisStr)
		{
			if (string.IsNullOrEmpty(thisStr))
			{
				return thisStr;
			}
			char[] returnValue = new char[thisStr.Length];
			int returnValueIndex = 0;

			bool inQuote = false;
			bool inSingleQuote = false;
			for (int charPos = 0; charPos < thisStr.Length; charPos++)
			{
				char currentChar = thisStr[charPos];
				char prevChar = charPos > 0 ? thisStr[charPos - 1] : '\0';
				if (currentChar == '\'' && prevChar != '\\')
				{
					if (!inQuote)
					{
						inSingleQuote = !inSingleQuote;
					}
				}
				else if (currentChar == '"' && prevChar != '\\')
				{
					if (!inSingleQuote)
					{
						inQuote = !inQuote;
					}
				}
				if (inQuote || inSingleQuote)
				{
					returnValue[returnValueIndex] = currentChar;
					returnValueIndex++;
				}
				else
				{
					if (currentChar.IsControlCharacter())
					{
						//drop it by doing nothing
						continue;
					}
					else if (currentChar == ' ' && returnValueIndex > 0)
					{
						if (prevChar == ' ' || prevChar == '>')
						{
							//TODO we may want to make sure we just closed the tag
							//before we strip the character
							continue;
						}
						else
						{
							returnValue[returnValueIndex] = currentChar;
							returnValueIndex++;
						}
					}
					else
					{
						returnValue[returnValueIndex] = currentChar;
						returnValueIndex++;
					}
				}
			}
			return new string(returnValue, 0, returnValueIndex);
		}

		/// <summary>
		/// shorten a string to a provided maximum  length
		/// </summary>
		/// <param name="thisStr"></param>
		/// <param name="maxLength"></param>
		/// <returns></returns>
		public static string TrimTo(this string thisStr, int maxLength)
		{
			return thisStr.TrimTo(maxLength, false);
		}

		/// <summary>
		/// shorten a string to a provided maximum  length
		/// </summary>
		/// <param name="thisStr"></param>
		/// <param name="maxLength"></param>
		/// <returns></returns>
		public static string TrimTo(this string thisStr, int maxLength, bool addEllipsis)
		{
			if (string.IsNullOrEmpty(thisStr))
			{
				return string.Empty;
			}
			if (addEllipsis)
			{
				maxLength -= 3;
			}
			if (thisStr.Length <= maxLength)
			{
				return thisStr;
			}
			string returnValue = thisStr.Substring(0, maxLength);
			if (addEllipsis)
			{
				returnValue += "...";
			}
			return returnValue;
		}

		public static string ToCamelCase(this string thisStr)
		{
			if (null == thisStr)
			{
				throw new ArgumentNullException("thisStr");
			}
			char firstChar = thisStr[0];
			return (firstChar.ToString().ToLowerInvariant() + thisStr.Substring(1));
		}

		public static string ToPascalCase(this string thisStr)
		{
			if (null == thisStr)
			{
				throw new ArgumentNullException("thisStr");
			}
			char firstChar = thisStr[0];
			return (firstChar.ToString().ToUpperInvariant() + thisStr.Substring(1).ToLowerInvariant());
		}

		public static string HtmlEncode(this string thisStr)
		{
			if (null == thisStr)
			{
				return string.Empty;
			}
			return System.Web.HttpUtility.HtmlEncode(thisStr);
		}

		public static string HtmlDecode(this string thisStr)
		{
			if (null == thisStr)
			{
				return string.Empty;
			}
			return System.Web.HttpUtility.HtmlDecode(thisStr);
		}

		public static string UrlEncode(this string thisStr)
		{
			if (null == thisStr)
			{
				return string.Empty;
			}
			return System.Web.HttpUtility.UrlEncode(thisStr);
		}

		public static string UrlDecode(this string thisStr)
		{
			if (null == thisStr)
			{
				return string.Empty;
			}
			return System.Web.HttpUtility.UrlDecode(thisStr);
		}

		public static bool RegExMatch(this String thisStr, string regularExpressionText)
		{
			if (string.IsNullOrWhiteSpace(thisStr))
			{
				return false;
			}
			return Regex.IsMatch(thisStr, regularExpressionText);
		}

		public static bool IsNumeric(this String thisStr)
		{
			if (string.IsNullOrWhiteSpace(thisStr))
			{
				return false;
			}
			return RegExNumericMatchPattern.IsMatch(thisStr);
		}

		public static string ToEmailAddress(this String thisStr)
		{
			MailAddress testIt = new MailAddress(thisStr);
			return testIt.Address;
		}

		public static bool IsEmail(this String thisStr)
		{
			if (string.IsNullOrWhiteSpace(thisStr))
			{
				return false;
			}
			try
			{
				//The mail test allows for single name host domains
				//we do not, so also check for the domain delimiter
				MailAddress testIt = new MailAddress(thisStr);
				if (testIt.Address.IndexOf('.') < 0)
				{
					throw new System.FormatException("The specified string is not in the form required for an e-mail address");
				}
			}
			catch (System.FormatException)
			{
				return false;
			}
			return true;
		}

		public static bool IsNullOrWhiteSpace(this String thisStr)
		{
			return string.IsNullOrWhiteSpace(thisStr);
		}

		public static bool IsNullOrEmpty(this String thisStr)
		{
			return string.IsNullOrEmpty(thisStr);
		}

		public static int WordCount(this String thisStr)
		{
			if (null == thisStr)
			{
				throw new ArgumentNullException("thisStr");
			}
			return thisStr.Split(new char[] { ' ', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries).Length;
		}

		public static string[] ToUniqueArray(this String thisStr, char stringDelimiter)
		{
			if (null == thisStr)
			{
				throw new ArgumentNullException("thisStr");
			}
			HashSet<string> returnValue = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			string[] textValues = thisStr.Split(new char[] { stringDelimiter });
			foreach (string textValue in textValues)
			{
				string possibleAddress = textValue.Trim();
				if (!possibleAddress.IsNullOrWhiteSpace())
				{
					returnValue.Add(possibleAddress);
				}
			}
			return returnValue.ToArray();
		}

		/// <summary>
		/// returns a string formatted with 
		/// standard culture nameing conventions
		/// </summary>
		/// <param name="thisString"></param>
		/// <returns></returns>
		public static string ToStandardCultureNameFormat(this string thisString)
		{
			if (null == thisString)
			{
				throw new ArgumentNullException("thisString");
			}
			string[] nameParts = thisString.Split('-');
			string returnValue = nameParts[0].Trim().ToLowerInvariant();
			switch(nameParts.Length)
			{
				case 2:
						{
							returnValue += "-" + nameParts[1].Trim().ToUpperInvariant();
						}
					break;
				case 3:
						{
							returnValue += "-" + nameParts[1].Trim().ToCamelCase();
							returnValue += "-" + nameParts[2].Trim().ToUpperInvariant();
						}
					break;
				case 4:
						{
							returnValue += "-" + nameParts[1].Trim().ToCamelCase();
							returnValue += "-" + nameParts[2].Trim().ToCamelCase();
							returnValue += "-" + nameParts[3].Trim().ToUpperInvariant();
						}
					break;
			}
			return returnValue;
		}

		/// <summary>
		/// borrowed From SPDiscussionEmailHandler, its the same or similar 
		/// as used to be used in Exchange and other threaded index managers
		/// where you need a sortable value to represent time, relationship
		/// and index position simultaneously 
		/// </summary>
		/// <param name="thisStr"></param>
		/// <returns></returns>
		public static string IncrementThreadIndex(this String thisStr)
		{
			if (thisStr.IsNullOrWhiteSpace())
			{
				throw new ArgumentNullException("thisStr");
			}
			if (!thisStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				throw new FormatException("Not a Hex String");
			}
			char[] charArray = thisStr.ToLowerInvariant().ToCharArray();
			int strLen = thisStr.Length - 1;
			while (strLen > 1)
			{
				if (48 <= charArray[strLen] && charArray[strLen] < '9' || 97 <= charArray[strLen] && charArray[strLen] < 'f')
				{
					charArray[strLen] = (char)(charArray[strLen] + 1);
					break;
				}
				else if (charArray[strLen] != '9')
				{
					if (charArray[strLen] != 'f')
					{
						throw new ArgumentException("Invalid hex string");
					}
					charArray[strLen] = '0';
					if (strLen <= 2)
					{
						throw new OverflowException("Invalid hex string");
					}
					strLen--;
				}
				else
				{
					charArray[strLen] = 'a';
					break;
				}
			}
			return new string(charArray);
		}

	
	}
}