using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
    /// <summary>
    /// Different column rendering modes
    /// Iconic=Show an image in the row for unread or read items
    /// BoldDisplay=Highlight unread rows by updating the font weight and color
    /// </summary>
    internal enum ColumnRenderMode : int
    {
        None = 0,
        Iconic = 1,
        BoldDisplay = 2
    }
}