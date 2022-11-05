using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Collections;
using RSToolKit.Domain.JItems;
using RSToolKit.WebUI.Infrastructure;

namespace RSToolKit.WebUI.Infrastructure
{
    static class JsonTables
    {
        /// <summary>
        /// Holds tokens for the web application.
        /// </summary>
        static Dictionary<JsonTokenType, TokenDictionary<TableInformation>> Tokens { get; set; }
        /// <summary>
        /// The lasat time the table was checked for tables to remove.
        /// </summary>
        static DateTime LastCheck { get; set; }
        /// <summary>
        /// The syncronization lock.
        /// </summary>
        static object SyncRoot { get; set; }

        static JsonTables()
        {
            Tokens = new Dictionary<JsonTokenType, TokenDictionary<TableInformation>>();
            LastCheck = DateTime.Now;
            SyncRoot = new object();
        }

        /// <summary>
        /// Gets the token from the dictionaries.
        /// </summary>
        /// <param name="type">The type of token.</param>
        /// <param name="token">The token</param>
        /// <returns>The JsonTableToken.</returns>
        public static JsonTableToken<TableInformation> Get(JsonTokenType type, Guid token)
        {
            lock (SyncRoot)
            {
                TokenDictionary<TableInformation> dic = null;
                if (!Tokens.TryGetValue(type, out dic))
                {
                    // The dictionary is not found so we create it.
                    dic = new TokenDictionary<TableInformation>();
                    Tokens.Add(type, dic);
                }
                var t = dic.Get(token);
                if (t == null)
                    throw new TokenNotFoundException();
                return t;
            }
        }

        /// <summary>
        /// Gets the token dictionary.
        /// </summary>
        /// <param name="type">The type of tokens.</param>
        /// <returns></returns>
        public static TokenDictionary<TableInformation> GetTokens(JsonTokenType type)
        {
            lock (SyncRoot)
            {
                TokenDictionary<TableInformation> dic = null;
                if (!Tokens.TryGetValue(type, out dic))
                {
                    // The dictionary is not found so we create it.
                    dic = new TokenDictionary<TableInformation>();
                    Tokens.Add(type, dic);
                }
                return dic;
            }
        }

        /// <summary>
        /// Kills items if applicable.
        /// </summary>
        static void KillItems()
        {
            if (DateTime.Now.Subtract(LastCheck) < TimeSpan.FromMinutes(15))
                return;
            var checkTime = DateTime.Now;
            foreach (var kvp in Tokens)
            {
                var itemsToRemove = kvp.Value.Tables().Where(t => checkTime - t.LastActivity > TimeSpan.FromHours(6)).Select(t => t.Key);
                foreach (var table in itemsToRemove)
                    kvp.Value.RemoveInfo(table);
            }
        }
    }

    /// <summary>
    /// The different types of JsonTokens.
    /// </summary>
    public enum JsonTokenType
    {
        /// <summary>
        /// A form registration report.
        /// </summary>
        FormReport = 0,
        /// <summary>
        /// A form email report (invitation or general).
        /// </summary>
        FormEmailReport = 1,
        /// <summary>
        /// An advanced inventory report.
        /// </summary>
        InventoryReport = 2,
    }
}