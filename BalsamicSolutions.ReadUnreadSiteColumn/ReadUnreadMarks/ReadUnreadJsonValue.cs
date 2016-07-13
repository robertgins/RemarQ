using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
    /// <summary>
    /// Json serilization for a column read
    /// unread value pair
    /// </summary>
    [Serializable]
    public class ReadUnreadJsonValue
    {
        public bool IsRead { get; set; }

        public int ItemId { get; set; }

        public bool IsFolder { get; set; }

        public Guid ListId { get; set; }

        public ReadUnreadJsonValue()
        {
            this.IsRead = false;
            this.ItemId = -1;
            this.ListId = Guid.Empty;
            this.IsFolder = false;
        }

        public ReadUnreadJsonValue(ReadUnreadFieldValue fieldValue, bool isRead)
            : this()
        {
            if (null == fieldValue)
                throw new ArgumentNullException("fieldValue");
            this.IsRead = isRead;
            this.ItemId = fieldValue.ItemId;
            this.ListId = fieldValue.ListId;
            this.IsFolder = fieldValue.IsFolder;
        }

        public string ToJson()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(this);
        }
    }
}