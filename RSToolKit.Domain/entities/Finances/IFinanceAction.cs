using System;

namespace RSToolKit.Domain.Entities.Finances
{
    /// <summary>
    /// Holds information about a financial transaction.
    /// </summary>
    public interface IFinanceAction
    {
        /// <summary>
        /// The amount of the transaction.
        /// </summary>
        decimal Amount { get; }
        /// <summary>
        /// The description of the transaction.
        /// </summary>
        string Description { get; }
        /// <summary>
        /// The date the transaction was initiated.
        /// </summary>
        DateTimeOffset Date { get; }
        /// <summary>
        /// The date the transaction was settled.
        /// </summary>
        DateTimeOffset SettledDate { get; }
        /// <summary>
        /// If the finance is a gain or loss.
        /// </summary>
        FinanceType Type { get; }
    }
}
