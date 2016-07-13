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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
    /// <summary>
    /// simple wrapper for hierarchy information
    /// from the Hierarchy view
    /// </summary>
    internal class HierarchyItem
    {
        internal int ItemId { get; private set; }

        internal string Path { get; private set; }

        internal string Leaf { get; private set; }

        internal HierarchyItem(SqlDataReader dataReader)
        {
			if(null == dataReader) throw new ArgumentNullException("dataReader");
            int colIdx = dataReader.GetOrdinal("ItemId");
            this.ItemId = dataReader.GetInt32(colIdx);
            colIdx = dataReader.GetOrdinal("Path");
            this.Path = dataReader.GetString(colIdx);
            colIdx = dataReader.GetOrdinal("Leaf");
            this.Leaf = dataReader.GetString(colIdx);
        }
    }
}