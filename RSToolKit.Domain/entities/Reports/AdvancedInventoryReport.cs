using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Clients;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.RegScript;
using RSToolKit.Domain.Security;
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Collections;

namespace RSToolKit.Domain.Entities
{
    public class AdvancedInventoryReport
        : IFormReport, IPermissionHolder, IProtected
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Index(IsClustered = true, IsUnique = true)]
        public long SortingId { get; set; }
        [Key]
        [Index(IsClustered = false)]
        public Guid UId { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid FormKey { get; set; }

        [NotMapped]
        public Guid ParentKey
        {
            get
            {
                return FormKey;
            }
            set
            {
                return;
            }
        }

        public string Script { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        public bool Favorite { get; set; }

        public AdvancedInventoryReport()
        {
            DateCreated = DateModified = DateTimeOffset.Now;
            Name = "New Advanced Inventory Report";
            Script = "";
        }

        public static AdvancedInventoryReport New(FormsRepository repository, User user, Company company, Form form, Guid? owner = null, Guid? group = null, string permission = "770", string name = null)
        {
            var node = new AdvancedInventoryReport()
            {
                UId = Guid.NewGuid(),
                Form = form,
                FormKey = form.UId,
                Company = company,
                CompanyKey = company.UId,
                Name = name == null ? "New advanced inventory report." : name,
            };
            PermissionSet.CreateDefaultPermissions(repository, node, company.UId);
            repository.Add(node);
            repository.Commit();
            return node;
        }

        public TableInformation GetTableInformation(ITokenDictionary tokens)
        {
            var table = new TableInformation(UId);
            tokens.Add(table);
            table.UpdateMessage("Executing", "Executing script.");
            var engine = new RegScriptTableEngine(Form, Script, table);
            var results = engine.Run();
            var rows = new List<JsonTableRow>();
            foreach (var kvp in results)
            {
                var header = new JsonTableHeader()
                {
                    Id = kvp.Key,
                    Value = kvp.Key,
                };
                table.Headers.Add(header);
                if (kvp.Value is List<string>)
                {
                    var t_list = kvp.Value as List<string>;
                    for (var r_i = 0; r_i < t_list.Count; r_i++)
                    {
                        JsonTableRow t_row;
                        if (table.Rows.Count <= r_i)
                        {
                            t_row = new JsonTableRow()
                            {
                                Id = 0
                            };
                            table.Rows.Add(t_row);
                        }
                        else
                        {
                            t_row = table.Rows[r_i];
                        }
                        var t_col = new JsonTableValue()
                        {
                            HeaderId = kvp.Key,
                            Id = "",
                            Value = t_list[r_i]
                        };
                        t_row.Values.Add(t_col);
                    }
                }
                else
                {
                    JsonTableRow t_row;
                    if (table.Rows.Count == 0)
                    {
                        t_row = new JsonTableRow()
                        {
                            Id = 0
                        };
                        table.Rows.Add(t_row);
                    }
                    else
                    {
                        t_row = table.Rows[0];
                    }
                    t_row.Values.Add(new JsonTableValue()
                    {
                        HeaderId = kvp.Key,
                        Id = "",
                        Value = kvp.Value.ToString()
                    }
                    );
                }
                table.Info.Complete = true;
            }
            return table;

        }

        protected void UpdateInfo(ProgressInfo info, float progress, string message = "Processing")
        {
            if (info == null)
                return;
            progress = progress * 0.1F + 0.1F;
            info.Update(progress, message);
        }

    }
}
