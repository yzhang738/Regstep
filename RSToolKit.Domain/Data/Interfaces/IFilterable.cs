using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Data
{
    [Obsolete]
    public interface IFilterable
    {
        /// <summary>
        /// Tests a value against a LogicTest for filtering.
        /// </summary>
        /// <param name="field">The field to check.</param>
        /// <param name="testValue">The value to test against.</param>
        /// <param name="test">The logic test.</param>
        /// <param name="company">The company the class belongs to.</param>
        /// <param name="repository">The <code>FormsRepository</code> used for the database.</param>
        /// <param name="caseSensitive">Whether the search is case sensitive or not.</param>
        /// <returns>A boolean value of the test result.</returns>
        bool TestValue(string field, string testValue, LogicTest test, Company company, FormsRepository repository, bool caseSensitive = false);
        bool TestValue(string field, string testValue, string test, bool caseSensitive = false);
    }
}
