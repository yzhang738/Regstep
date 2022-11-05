
namespace RSToolKit.Domain.Entities.Reports
{
    /// <summary>
    /// An item that uses reg script syntax.
    /// </summary>
    public interface IRegScript
        : Data.IDataItem
    {
        /// <summary>
        /// The script.
        /// </summary>
        string Script { get; set; }
    }
}
