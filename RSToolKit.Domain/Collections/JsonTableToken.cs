using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Collections
{
    public class JsonTableToken<T>
        : ITableToken<T>
        where T : class, ITableInformation
    {
        /// <summary>
        /// The key of the item.
        /// </summary>
        public Guid ItemKey { get; protected set; }
        /// <summary>
        /// The key of the Token
        /// </summary>
        public Guid Key { get; protected set; }
        /// <summary>
        /// The JsonTableInformation
        /// </summary>
        public T Item { get; set; }
        /// <summary>
        /// The headers being used.
        /// </summary>
        public List<JsonTableHeader> Headers { get; set; }
        /// <summary>
        /// The Filters being used.
        /// </summary>
        public List<JsonFilter> Filters { get; set; }
        /// <summary>
        /// The Sortings being used.
        /// </summary>
        public List<JsonSorting> Sortings { get; set; }
        /// <summary>
        /// The records per page to use.
        /// </summary>
        public int RecordsPerPage { get; set; }
        /// <summary>
        /// The page the token is on.
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// Options related to the token.
        /// </summary>
        public Dictionary<TokenOption, string> Options { get; set; }

        /// <summary>
        /// Initializes the class with the specified values.
        /// </summary>
        /// <param name="key">The token.</param>
        /// <param name="item">The key the token references.</param>
        /// <param name="item">The progress item of type T.</param>
        public JsonTableToken(Guid key, Guid itemKey, T item)
        {
            Key = key;
            ItemKey = itemKey;
            Item = item;
            Page = 1;
            RecordsPerPage = 15;
            Sortings = new List<JsonSorting>();
            Headers = new List<JsonTableHeader>();
            Filters = new List<JsonFilter>();
            Options = new Dictionary<TokenOption, string>();
        }

        /// <summary>
        /// Updates the progress with the specified values.
        /// </summary>
        /// <param name="progress">The fraction of completion.</param>
        /// <param name="message">The message.</param>
        public void Update(float progress = 0F, string message = "Processing")
        {
            if (Item != null && Item.Info != null)
                Item.Info.Update(progress, message);
        }

        /// <summary>
        /// Updates the progress by adding the passed progress to the current and setting the message.
        /// </summary>
        /// <param name="progress">The fraction of completion.</param>
        /// <param name="message">The message.</param>
        public void UpdateAdd(float progress = 0F, string message = "Processing")
        {
            if (Item != null && Item.Info != null)
                Item.Info.UpdateAdd(progress, message);
        }

        /// <summary>
        /// Gets a progress message associated with this item.
        /// </summary>
        /// <returns>The progress message.</returns>
        public ProgressMessage GetProgress()
        {
            if (Item != null && Item.Info != null)
                return new ProgressMessage(Item.Info.Message, Item.Info.Progress, Item.Info.Details, Item.Info.Complete);
            return new ProgressMessage("Processing", -1, Item.Info.Details);
        }
    }

    public enum TokenOption
    {
        FormReportType = 0
    }
}
