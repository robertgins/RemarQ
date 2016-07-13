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