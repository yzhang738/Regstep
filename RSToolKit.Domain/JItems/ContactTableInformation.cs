using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain;

namespace RSToolKit.Domain.JItems
{
    /// <summary>
    /// Holds information about the contact table.
    /// </summary>
    public class ContactTableInformation
        : TableInformation
    {
        /// <summary>
        /// Initializes the table information based on the list of contacts.
        /// </summary>
        /// <param name="key">The key being used.</param>
        /// <param name="company">The company that has the list of contacts.</param>
        public ContactTableInformation(Company company)
            : base(company.UId)
        {
            UpdateTable(company);
        }

        /// <summary>
        /// Initializes the table information.
        /// </summary>
        /// <param name="key">The key being used.</param>
        public ContactTableInformation(Guid key)
            : base(key)
        { }

        /// <summary>
        /// Reanalyzes the data and extracts the data.
        /// </summary>
        /// <param name="company">The company that has the list of contacts used as the data.</param>
        public void UpdateTable(Company company)
        {
            var headerCount = company.ContactHeaders.Count + 1;
            var contactCount = company.Contacts.Count;
            var totalCount = headerCount + (headerCount * contactCount);
            var progressPerCount = 1F / (float)totalCount;
            Rows.Clear();
            Headers.Clear();
            Headers.Add(new JsonTableHeader() { Id = "Email", Editable = true, Token = Guid.Empty, Type = "email", Value = "Email" });
            foreach (var header in company.ContactHeaders)
            {
                Headers.Add(new JsonTableHeader() { Id = header.SortingId.ToString(), Editable = true, Token = header.UId, Type = header.Descriminator.GetJTableType(), Value = header.Name });
                FilterHeaders.Add(new JsonFilterHeader() { Id = header.SortingId.ToString(), Label = header.Name, Type = header.Descriminator.GetJTableType(), PossibleValues = header.Values.Select(v => new JsonFilterValue() { Id = v.Id.ToString(), Value = v.Value }).ToList() });
                Info.UpdateAdd(progressPerCount, "Generating Headers");
            }
            foreach (var contact in company.Contacts)
            {
                var row = new JsonTableRow() { Id = contact.SortingId, Token = contact.UId, Editable = true };
                foreach (var header in Headers)
                {
                    row.Values.Add(contact.GetJsonValue(header));
                    Info.UpdateAdd(progressPerCount, "Generating Rows");
                }
                Rows.Add(row);
            }
            Info.Update(1F, "Finished");
        }
    }
}
