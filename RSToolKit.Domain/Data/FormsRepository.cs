using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Security;
using System.Security;
using System.Security.Principal;
using HtmlAgilityPack;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Objects;

namespace RSToolKit.Domain.Data
{
    public class FormsRepository : IDisposable
    {

        public User User { get; set; }
        public IPrincipal Principal { get; set; }

        public EFDbContext Context;
        protected bool pr_contextInScope;
        protected bool pr_UseSecurity;

        public UserManager<User> UserManager { get; set; }
        public RoleManager<AppRole> RoleManager { get; set; }

        public FormsRepository(EFDbContext context, User user = null, IPrincipal principal = null)
        {
            pr_contextInScope = false;
            pr_UseSecurity = true;
            Context = context;
            User = user;
            Principal = principal;
            if ((User != null && Principal == null) || (User == null && Principal != null))
                throw new Exception("Both user and principal must be defined or null.");
            if (User == null && Principal == null)
                pr_UseSecurity = false;
            UserManager = new UserManager<User>(new UserStore<User>(Context));
            RoleManager = new RoleManager<AppRole>(new RoleStore<AppRole>(Context));
        }

        public FormsRepository()
        {
            pr_contextInScope = true;
            pr_UseSecurity = false;
            Context = new EFDbContext();
            User = null;
            UserManager = new UserManager<User>(new UserStore<User>(Context));
            RoleManager = new RoleManager<AppRole>(new RoleStore<AppRole>(Context));
        }

        /// <summary>
        /// Turns off security measures for this repository.
        /// </summary>
        public void DiscardSecurity()
        {
            pr_UseSecurity = false;
        }

        /// <summary>
        /// Activates security for nodes.
        /// </summary>
        public void EnableSecurity()
        {
            if (User != null && Principal != null)
                pr_UseSecurity = true;
        }

        /// <summary>
        /// Marks an item to be added to the repository.
        /// </summary>
        /// <typeparam name="Titem">The type of item to be adding.</typeparam>
        /// <param name="item">The item to be added.</param>
        /// <returns>True on seccess and false otherwise.</returns>
        public bool Add<Titem>(Titem item)
            where Titem : class
        {
            var set = Context.Set<Titem>();
            if (set == null)
                return false;
            set.Add(item);
            return true;
        }

        /// <summary>
        /// Marks a group of items to be added to the repository.
        /// </summary>
        /// <typeparam name="Titem">The type of items to be adding.</typeparam>
        /// <param name="items">The group of items to add.</param>
        /// <returns>True on seccess and false otherwise.</returns>
        public bool AddRange<Titem>(IEnumerable<Titem> items)
            where Titem : class
        {
            var set = Context.Set<Titem>();
            if (set == null)
                return false;
            set.AddRange(items);
            return true;
        }

        /// <summary>
        /// Marks an item for removal from the repository.
        /// </summary>
        /// <typeparam name="Titem">The type of item to be removed.</typeparam>
        /// <param name="item">The item to remove</param>
        /// <returns>True on seccess and false otherwise.</returns>
        public bool Remove<Titem>(Titem item)
            where Titem : class
        {
            if (item == null)
                return true;
            var type = item.GetType();
            var set = Context.Set(type);
            if (set == null)
                return false;
            var clears = typeof(Titem).GetProperties().Where(p => Attribute.IsDefined(p, typeof(ClearKeyOnDeleteAttribute))).ToList();
            foreach (var prop in clears)
            {
                var attribute = prop.GetCustomAttribute(typeof(ClearKeyOnDeleteAttribute)) as ClearKeyOnDeleteAttribute;
                var property = attribute.PropertyName;
                var cd = prop.GetValue(item);
                if (cd is System.Collections.IEnumerable)
                {
                    foreach (var cl in cd as System.Collections.IEnumerable)
                    {
                        cl.GetType().GetProperty(property).SetValue(cl, null);
                    }
                }
                else
                {
                    var t_prop = cd.GetType().GetProperty(property);
                    t_prop.SetValue(cd, null);
                }
            }
            var clearRel = typeof(Titem).GetProperties().Where(p => Attribute.IsDefined(p, typeof(ClearRelationshipAttribute))).ToList();
            foreach (var prop in clearRel)
            {
                var attribute = prop.GetCustomAttribute(typeof(ClearRelationshipAttribute)) as ClearRelationshipAttribute;
                var property = attribute.PropertyName;
                var propertyValue = prop.GetValue(item);
                prop.SetValue(item, null);
                item.GetType().GetProperty(property).SetValue(item, null);
            }

            var deletes = item.GetType().GetProperties().Where(p => Attribute.IsDefined(p, typeof(CascadeDeleteAttribute))).ToList();
            foreach (var prop in deletes)
            {
                var cd = prop.GetValue(item);
                if (cd is IEnumerable)
                {
                    /*
                    var clearMethod = typeof(FormRepository).GetMethod("ClearList");
                    var clearGeneric = clearMethod.MakeGenericMethod(prop.PropertyType.GetGenericArguments()[0]);
                    var t_items = (IEnumerable)clearGeneric.Invoke(this, new object[] { cd as IList });
                    //*/
                    var method = typeof(FormsRepository).GetMethod("RemoveRange");
                    var generic = method.MakeGenericMethod(prop.PropertyType.GetGenericArguments()[0]);
                    var success = (bool)generic.Invoke(this, new object[] { cd as IEnumerable });
                }
                else
                {
                    var method = typeof(FormsRepository).GetMethod("Remove");
                    var generic = method.MakeGenericMethod(prop.PropertyType);
                    var success = (bool)generic.Invoke(this, new object[] { cd });
                }
            }
            try
            {
                set.Remove(item);
            }
            catch (Exception)
            { }
            return true;
        }

        /// <summary>
        /// Marks a group of items for removal from the repository.
        /// </summary>
        /// <typeparam name="Titem">The type of items to be removed.</typeparam>
        /// <param name="items">The items being removed.</param>
        /// <returns>True on seccess and false otherwise.</returns>
        public bool RemoveRange<Titem>(IEnumerable<Titem> items)
            where Titem : class
        {
            foreach (var item in items.ToList())
                Remove(item);
            return true;
        }
        
        /// <summary>
        /// Traverses throgh entity items and ensures propper permissions are met for adding, deleting, and editing.
        /// </summary>
        protected void RemoveNonPermissives()
        {
            foreach (var changeSet in Context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged && e.State != EntityState.Detached))
            {
                switch (changeSet.State)
                {
                    case EntityState.Added:
                        if (!CanAccess(changeSet.Entity, SecurityAccessType.Write, adding: true))
                            changeSet.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                    case EntityState.Modified:
                        if (!CanAccess(changeSet.Entity, SecurityAccessType.Write))
                            changeSet.State = EntityState.Unchanged;
                        break;
                }
            }
        }

        /// <summary>
        /// Saves the repository.
        /// </summary>
        /// <param name="force">If set to true, the user saving will not be used in any circumstance.</param>
        /// <returns>The number of rows affected.</returns>
        public int Commit(bool force = false, bool form = false, bool skipFormatting = false)
        {
            if (User != null)
            {
                if (pr_UseSecurity && !force)
                    RemoveNonPermissives();
            }
            return FinalCommit(form, skipFormatting);
        }

        /// <summary>
        /// Runs all the final manipulations on data before saving the changes.
        /// </summary>
        /// <returns></returns>
        protected int FinalCommit(bool form, bool skipFormatting)
        {
            if (!skipFormatting)
            {
                UpdateEmails();
                UpdateRegistrants(form);
                UpdateData(form);
                UpdateISecure(form);
            }
            return Context.SaveChanges();
        }

        /// <summary>
        /// Records an access log entry for ISecure items.
        /// </summary>
        public void UpdateISecure(bool form = false)
        {
            foreach (var changeSet in Context.ChangeTracker.Entries().Where(e => e.Entity is ISecure && e.State != EntityState.Unchanged && e.State != EntityState.Detached))
            {
                var node = (ISecure)changeSet.Entity;
                var aLog = new AccessLog();
                aLog.AccessedItem = node.UId;
                aLog.AccessTime = DateTimeOffset.UtcNow;
                aLog.AccessKey =  null;
                if (User != null)
                {
                    aLog.AccessKey = User.Id;
                    aLog.CompanyKey = User.CompanyKey;
                }
                else
                {
                    var s_id = node.UId.ToString();
                    var data = Search<ISecureMap>(d => d.Value == s_id, silent: true, checkAccess: false).FirstOrDefault();
                    if (data != null)
                    {
                        var parent = data.Parent;
                        if (parent is Registrant)
                            aLog.Company = (parent as Registrant).Form.Company;
                        else if (parent is Contact)
                            aLog.Company = (parent as Contact).Company;
                    }             
                }
                var type = Context.GetObjectType(node);
                aLog.Type = type.Name;
                if (changeSet.State == EntityState.Deleted)
                {
                    node.Delete = true;
                    changeSet.State = EntityState.Modified;
                    if (User != null)
                        aLog.Note = "Marked for deletion by " + User.UserName;
                    aLog.AccessType = SecureAccessType.Delete;
                }
                else if (changeSet.State == EntityState.Modified)
                {
                    if (User != null)
                        aLog.Note = "Modified by " + User.UserName;
                    aLog.AccessType = SecureAccessType.Write;
                }
                else if (changeSet.State == EntityState.Added)
                {
                    if (User != null)
                        aLog.Note = "Added by " + User.UserName;
                    aLog.AccessType = SecureAccessType.Insertion;
                }
                if (form)
                    aLog.Note = "Accessed by registering user";
                Context.AccessLog.Add(aLog);
            }
        }

        /// <summary>
        /// Checks to see if registrants need a changelog
        /// </summary>
        protected void UpdateRegistrants(bool form = false)
        {
            // This is the UId of the modifier;
            var modifier = Guid.Empty;
            if (User != null && !form)
                modifier = User.UId;

            // This is a list of registrant Ids that have already been processed.
            var regIds = new List<Guid>();

            // We iterate over each registrant or registrantItem
            var registrantEntries = Context.ChangeTracker.Entries().Where(e => (e.State == EntityState.Modified || e.State == EntityState.Added) && (e.Entity is Registrant || e.Entity is IRegistrantItem)).ToList();
            foreach (var changeSet in registrantEntries)
            {
                var reg = changeSet.Entity as Registrant;
                if (reg != null && reg.Form != null && (changeSet.State != EntityState.Added && changeSet.State != EntityState.Deleted && changeSet.State != EntityState.Detached))
                    reg.UpdateAccounts();
                if (changeSet.State != EntityState.Modified)
                    continue;
                if (reg == null)
                {
                    // If this hits, then the entity was not a Registrant and was instead a IRegistrantItem and we need to grab the registrant.
                    reg = (changeSet.Entity as IRegistrantItem).Registrant;
                    if (reg == null)
                        // Again, if reg is null we do nothing and move to next item.
                        continue;
                }
                var oldReg = Context.GetObjectContext().ObjectStateManager.GetObjectStateEntry(reg);
                var origValues = oldReg.OriginalValues;
                var startChangeLog = false;
                if ((origValues["Status"] as RegistrationStatus?).Value == RegistrationStatus.Incomplete && reg.Status == RegistrationStatus.Submitted)
                    startChangeLog = true;
                else if (reg.ModifiedBy != modifier)
                    startChangeLog = true;
                else if (reg.DateModified.AddMinutes(30) < DateTimeOffset.Now)
                    startChangeLog = true;

                // If the status is still incomplete, we don't make a change log.
                if (reg.Status == RegistrationStatus.Incomplete)
                    startChangeLog = false;

                // If we have not processed this registrant yet, we do that now.
                if (!regIds.Contains(reg.UId))
                {
                    // We add this registrant to those that have been processed.
                    regIds.Add(reg.UId);
                    reg.UpdateAccounts();
                    // If the registrants DateModified is 30 minutes old or the registrant is being modified by a new person, we need to add a change log.
                    if (startChangeLog)
                    {
                        var old = new OldRegistrant();
                        old.ModifiedBy = reg.ModifiedBy;
                        old.DateModified = old.DateCreated = (origValues["DateModified"] as DateTimeOffset?).Value;
                        old.Email = reg.Email;
                        old.Form = reg.Form;
                        old.Name = reg.Name;
                        old.RSVP = (origValues["RSVP"] as bool?).Value;
                        old.Status = reg.Status;
                        old.Type = reg.Type;
                        old.UId = Guid.NewGuid();
                        old.CurrentRegistration = reg;
                        old.Confirmation = reg.Confirmation;
                        old.AudienceKey = origValues["AudienceKey"] as Guid?;
                        Context.OldRegistrants.Add(old);
                        foreach (var data in reg.Data)
                        {
                            var state = Context.GetObjectContext().ObjectStateManager.GetObjectStateEntry(data);
                            var oldData = new OldRegistrantData();
                            oldData.Registrant = old;
                            oldData.VariableUId = data.VariableUId;
                            oldData.Value = data.Value;
                            oldData.UId = Guid.NewGuid();
                            oldData.Registrant = old;
                            Context.OldRegistrantData.Add(oldData);
                            if (state.State == EntityState.Modified)
                            {
                                var origValue = state.OriginalValues["Value"];
                                oldData.Value = origValue as string;
                            }
                        }
                    }

                    reg.DateModified = DateTimeOffset.Now;
                    reg.ModifiedBy = modifier;
                }
            }
        }

        /// <summary>
        /// Updates DateModified and ModificationToken for IRSData objects.  Also updates DateCreated if the object is in the added state.
        /// </summary>
        protected void UpdateData(bool form = false)
        {
            var modifier = Guid.Empty;
            if (User != null && !form)
                modifier = User.UId;
            foreach (var changeSet in Context.ChangeTracker.Entries().Where(e => e.Entity is INode && (e.State == EntityState.Modified || e.State == EntityState.Added)))
            {
                var node = changeSet.Entity as INode;
                switch (changeSet.State)
                {
                    case EntityState.Added:
                        node.DateCreated = node.DateModified = DateTimeOffset.UtcNow;
                        break;
                    case EntityState.Modified:
                        node.DateModified = DateTimeOffset.UtcNow;
                        break;
                }
                node.ModifiedBy = modifier;
                node.ModificationToken = Guid.NewGuid();
            }
        }

        /// <summary>
        /// Updates email hyperlinks for click tracking.
        /// </summary>
        protected void UpdateEmails()
        {
            foreach (var changeSet in Context.ChangeTracker.Entries().Where(e => e.Entity is IEmail && (e.State == EntityState.Added || e.State == EntityState.Modified)))
            {
                if (changeSet.Entity is RSHtmlEmail && !String.IsNullOrEmpty((changeSet.Entity as RSHtmlEmail).Html))
                {
                    var email = changeSet.Entity as RSHtmlEmail;
                    var a_html = new HtmlDocument();
                    a_html.LoadHtml(email.Html);
                    var a_Nodes = a_html.DocumentNode.SelectNodes("//a[@href]") ?? new HtmlNodeCollection(a_html.DocumentNode);
                    foreach (HtmlNode link in a_Nodes)
                    {
                        var data_a_id = link.Attributes.FirstOrDefault(a => a.Name == "data-a-id");
                        if (data_a_id != null)
                            continue;
                        link.Attributes.Add("data-a-id", Guid.NewGuid().ToString());
                    }
                    email.Html = a_html.DocumentNode.InnerHtml;
                }
                else if (changeSet.Entity is RSEmail)
                {
                    var email = changeSet.Entity as RSEmail;
                    foreach (var area in email.EmailAreas)
                    {
                        var a_html = new HtmlDocument();
                        a_html.LoadHtml(area.Html);
                        if (a_html.DocumentNode == null)
                            continue;
                        var nodes = a_html.DocumentNode.SelectNodes("//a[@href]");
                        if (nodes == null)
                            continue;
                        foreach (HtmlNode link in nodes)
                        {
                            var data_a_id = link.Attributes.FirstOrDefault(a => a.Name == "data-a-id");
                            if (data_a_id != null)
                                continue;
                            link.Attributes.Add("data-a-id", Guid.NewGuid().ToString());
                        }
                        area.Html = a_html.DocumentNode.InnerHtml;
                    }
                }
            }
        }

        /// <summary>
        /// Disposes of the repository.
        /// </summary>
        public void Dispose()
        {
            if (pr_contextInScope)
                Context.Dispose();
            if (UserManager != null)
                UserManager.Dispose();
            if (UserManager != null)
                RoleManager.Dispose();
        }

        /// <summary>
        /// Checks to see if an item exists.
        /// </summary>
        /// <typeparam name="Titem">The type of item to search for. Must be a class and inherit IData.</typeparam>
        /// <param name="search">The search expression.</param>
        /// <returns>Returns the list of Guids for the items found.</returns>
        public IEnumerable<Guid> ItemExists<Titem>(Expression<Func<Titem, bool>> search)
            where Titem : class, INode
        {
            var set = Context.Set<Titem>();
            var items = new List<Titem>();
            try
            {
                items = set.Where(search).ToList();
            }
            catch(Exception)
            {
                return new List<Guid>();
            }
            if (items.Count < 1)
                return new List<Guid>();
            return items.Select(i => i.UId).ToList();
        }

        /// <summary>
        /// Updates the selected field securely without ever reading the information.
        /// </summary>
        /// <typeparam name="Titem">The type of item to update.</typeparam>
        /// <param name="search">The search expression.</param>
        /// <param name="property">The property name to update.</param>
        /// <param name="value">The value to update.</param>
        /// <returns></returns>
        public bool UpdateSecure<Titem>(Expression<Func<Titem, bool>> search, string property, object value, bool silent = false)
            where Titem : class, ISecure
        {
            var set = Context.Set<Titem>();
            var items = new List<Titem>();
            try
            {
                items = set.Where(search).ToList();
            }
            catch(Exception)
            {
                return false;
            }
            if (items.Count < 1)
                return false;
            var prop = typeof(Titem).GetProperty(property);
            if (prop == null)
                return false;
            foreach (var item in items)
            {
                prop.SetValue(item, value);
                if (!silent)
                    AccessLog.New(User, item, SecureAccessType.Write, User.WorkingCompany, "No read made, updated blindly by system.");
            }
            return true;
        }

        /// <summary>
        /// Returns a list of peeks on the secure data.  The peek is defined in the ISecure class method Peek() and should not reveal any sensitive information since it is NOT LOGGED as an access.
        /// </summary>
        /// <typeparam name="Titem">The type of item to peek at.</typeparam>
        /// <param name="search">The search expression.</param>
        /// <returns>A list of strings that represent the peeks.</returns>
        public IEnumerable<string> SecurePeek<Titem>(Expression<Func<Titem, bool>> search)
            where Titem : class, ISecure
        {
            var items = Search(search, true).ToList();
            var peeks = new List<string>();
            foreach (var item in items)
                peeks.Add(item.Peek());
            return peeks;
        }

        /// <summary>
        /// Searches for database entities.
        /// </summary>
        /// <typeparam name="Titem">The type of item to search for.</typeparam>
        /// <param name="search">The exression to match against while searching for <paramref name="Titem"/></param>
        /// <param name="silent">If set to true, no access log entries are entered when <typeparamref name="Titem"/> is <code>ISecure</code>. This should only be set to true if the data is not being delivered to the Web UI.</param>
        /// <param name="checkAccess">Whether or not to check if a user has access to the desired information.</param>
        /// <returns>An IEnumerable list containing <typeparamref name="Titem"/></returns>
        public IEnumerable<Titem> Search<Titem>(Expression<Func<Titem, bool>> search, bool silent = false, bool checkAccess = true)
            where Titem : class
        {
            var type = typeof(Titem);
            // We need to see if the system is trying to access secure information.
            if (typeof(ISecure).IsAssignableFrom(type))
            {
                // We are trying to get secure information.  We need to mark the user accessing the data.
                if (User == null && !silent)
                {
                    // No user supplied and it is not a silent run, we will not return any data.
                    return new List<Titem>();
                }
                if (type.IsInterface)
                {
                    var interf = typeof(Titem);
                    var list = new List<Titem>();
                    var assembly = Assembly.Load("RSToolKit.Domain");
                    var types = assembly.GetTypes().Where(p => type.IsAssignableFrom(p) && !p.IsInterface).ToList();
                    foreach (var c_type in types)
                    {
                        var convertMethod = typeof(FormsRepository).GetMethod("ConvertExpression");
                        var convertGeneric = convertMethod.MakeGenericMethod(typeof(Titem), c_type);
                        var expression = convertGeneric.Invoke(this, new object[] { search });
                        var method = typeof(FormsRepository).GetMethod("SearchInterface");
                        var generic = method.MakeGenericMethod(c_type, interf);
                        var items = (List<Titem>)generic.Invoke(this, new object[] { expression, false });
                        list.AddRange(items);
                    }
                    return CheckAccess(list, checkAccess: checkAccess);
                }
                else
                {
                    var items = new List<Titem>();
                    try
                    {
                        items = Context.Set<Titem>().Where(search).ToList();
                        items = CheckAccess(items, checkAccess: checkAccess).ToList();
                    }
                    catch (Exception)
                    {
                        return new List<Titem>();
                    }
                    if (!silent)
                    {
                        foreach (var item in items)
                        {
                            // Now we create an access log for each item and save it into the database.
                            var note = "Read by " + User.UserName + (!String.IsNullOrWhiteSpace(User.LastName) && !String.IsNullOrWhiteSpace(User.FirstName) ? " {" + User.LastName + ", " + User.FirstName + "}" : "");
                            AccessLog.New(User, (ISecure)item, SecureAccessType.Read, User.WorkingCompany, note);
                        }
                    }
                    return items;
                }
            }
            // Lets see if an AccessLog is trying to be matched.
            else if (typeof(AccessLog).IsAssignableFrom(type))
            {
                var items = new List<Titem>();
                try
                {
                    items = Context.Set<Titem>().Where(search).ToList();
                }
                catch(Exception)
                {
                    return new List<Titem>();
                }
                foreach (var item in items)
                {
                    var a_item = item as AccessLog;
                    if (a_item.AccessKey != null)
                        a_item.AccessUser = UserManager.FindById(a_item.AccessKey.ToString());
                }
                return CheckAccess(items, checkAccess: checkAccess);
            }
            else
            {
                if (typeof(Titem).IsInterface)
                {
                    var interf = typeof(Titem);
                    var list = new List<Titem>();
                    var assembly = Assembly.Load("RSToolKit.Domain");
                    var types = assembly.GetTypes().Where(p => type.IsAssignableFrom(p) && !p.IsInterface).ToList();
                    foreach (var c_type in types)
                    {
                        var convertMethod = typeof(FormsRepository).GetMethod("ConvertExpression");
                        var convertGeneric = convertMethod.MakeGenericMethod(typeof(Titem), c_type);
                        var expression = convertGeneric.Invoke(this, new object[] { search });
                        var method = typeof(FormsRepository).GetMethod("SearchInterface");
                        var generic = method.MakeGenericMethod(c_type, interf);
                        var items = (List<Titem>)generic.Invoke(this, new object[] { expression, silent });
                        list.AddRange(items);
                    }
                    return CheckAccess(list, checkAccess: checkAccess);
                }
                else
                {
                    try
                    {
                        return CheckAccess(Context.Set<Titem>().Where(search).ToList(), checkAccess: checkAccess);
                    }
                    catch(Exception)
                    {
                        return new List<Titem>();
                    }
                }
            }
        }

        /// <summary>
        /// Searches interfaces that are used in the repository.
        /// </summary>
        /// <typeparam name="Titem">The type of item that is being searched.</typeparam>
        /// <typeparam name="Tinterf">The interface searching with.</typeparam>
        /// <param name="search">The search expression.</param>
        /// <returns>A list of items cast as the specified interface.</returns>
        public IEnumerable<Tinterf> SearchInterface<Titem, Tinterf>(Expression<Func<Titem, bool>> search, bool silent = false)
            where Titem : class
            where Tinterf : class
        {
            if (typeof(Titem).IsInterface)
                return new List<Tinterf>();
            if (!typeof(Tinterf).IsInterface)
                return new List<Tinterf>();
            var list = new List<Tinterf>();
            if (typeof(Titem) is ISecure && User == null && !silent)
                return new List<Tinterf>(); // Not a silent run and data is secure with no supplied user.
            var set = Context.Set<Titem>();
            foreach (var item in Context.Set<Titem>().Where(search).ToList())
            {
                list.Add(item as Tinterf);
                if (!silent)
                {
                    if (item.GetType() is ISecure)
                    {
                        var note = "Read by " + User.UserName + (!String.IsNullOrWhiteSpace(User.LastName) && !String.IsNullOrWhiteSpace(User.FirstName) ? " {" + User.LastName + ", " + User.FirstName + "}" : "");
                        AccessLog.New(User, (ISecure)item, SecureAccessType.Read, User.WorkingCompany, note);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Converts the expression to the desired class type.
        /// </summary>
        /// <typeparam name="Tfrom">The previous class in the expression.</typeparam>
        /// <typeparam name="Tto">The desired class in the expression.</typeparam>
        /// <param name="search">The expression to convert.</param>
        /// <returns>The converted expression.</returns>
        public Expression<Func<Tto, bool>> ConvertExpression<Tfrom, Tto>(Expression<Func<Tfrom, bool>> search)
        {
            var subtitutes = new Dictionary<Expression, Expression>();
            var oldParam = search.Parameters[0];
            var newParam = Expression.Parameter(typeof(Tto), oldParam.Name);
            subtitutes.Add(oldParam, newParam);
            var body = ConvertNode(search.Body, subtitutes);
            return Expression.Lambda<Func<Tto, bool>>(body, newParam);
        }

        /// <summary>
        /// Converts the expression.
        /// </summary>
        /// <param name="node">The current expression node.</param>
        /// <param name="subst">The dictionary of expressions being used.</param>
        /// <returns>The converted expression node.</returns>
        protected Expression ConvertNode(Expression node, IDictionary<Expression, Expression> subst)
        {
            if (node == null)
                return null;
            if (subst.ContainsKey(node))
                return subst[node];
            switch (node.NodeType)
            {
                case ExpressionType.Constant:
                    return node;
                case ExpressionType.MemberAccess:
                    {
                        var me = (MemberExpression)node;
                        var newNode = ConvertNode(me.Expression, subst);
                        if (newNode != null)
                            return Expression.MakeMemberAccess(newNode, newNode.Type.GetMember(me.Member.Name).Single());
                        else
                            return Expression.MakeMemberAccess(newNode, node.Type.GetMember(me.Member.Name).Single());
                    }
                case ExpressionType.NotEqual:
                case ExpressionType.Not:
                case ExpressionType.Or:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.IsFalse:
                case ExpressionType.IsTrue:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Equal:
                    {
                        var be = (BinaryExpression)node;
                        return Expression.MakeBinary(be.NodeType, ConvertNode(be.Left, subst), ConvertNode(be.Right, subst), be.IsLiftedToNull, be.Method);
                    }
                default:
                    throw new NotSupportedException(node.NodeType.ToString());
            }
        }

        /// <summary>
        /// Checks a list of items against user permissions.
        /// </summary>
        /// <typeparam name="Titem">The type of the items.</typeparam>
        /// <param name="items">The items to be checked.</param>
        /// <param name="action">The type of action being performed.</param>
        /// <param name="checkAccess">Whether we realy need to check access or not.</param>
        /// <returns>The items that can be actioned upon.</returns>
        protected IEnumerable<Titem> CheckAccess<Titem>(IEnumerable<Titem> items, SecurityAccessType action = SecurityAccessType.Read, bool checkAccess = true)
            where Titem : class
        {
            if (!pr_UseSecurity || !checkAccess)
                return PostProcessing(items);
            var ret_items = new List<Titem>();
            foreach (var item in items)
            {
                if (CanAccess(item, action))
                    ret_items.Add(item);
            }
            return PostProcessing(ret_items);
        }

        /// <summary>
        /// Runs post processing measures on items.
        /// </summary>
        /// <typeparam name="Titem">The type of items being manipulated.</typeparam>
        /// <param name="items">The collection of items.</param>
        /// <returns>A collection of items.</returns>
        protected IEnumerable<Titem> PostProcessing<Titem>(IEnumerable<Titem> items)
            where Titem : class
        {
            if (items.Count() == 0)
                return items;

            foreach(var reg in items.OfType<Registrant>())
                reg.UpdateAccounts();

            return items;
        }

        /// <summary>
        /// Checks to see if the user has access to perform actions on the specified item.
        /// </summary>
        /// <param name="item">The item to check against.</param>
        /// <param name="action">The action being performed.</param>
        /// <param name="adding">If the item is being added into the context or not.</param>
        /// <returns>True on success and false on failure.</returns>
        public bool CanAccess(object item, SecurityAccessType action, bool adding = false)
        {
            return true;
            //return true;
            //*
            // If there is no User or Principal, we just return false.
            if (User == null || Principal == null)
                return false;

            // Now we will check to make sure the user has access.
            // First we check if it is a base item.
            if (Principal.IsInRole("Super Administrators") || Principal.IsInRole("Administrators"))
                return true;

            if (item is IBaseItem && adding)
            {
                foreach (var role in (item as IBaseItem).roles)
                {
                    if (Principal.IsInRole(role))
                        return true;
                }

                if ((item as IBaseItem).CompanyKey == User.CompanyKey && Principal.IsInRole("Company Administrators"))
                    return true;

                return false;
            }

            if (item is ICompanyHolder && (item as ICompanyHolder).CompanyKey == User.CompanyKey && Principal.IsInRole("Company Administrators"))
                return true;

            if (item is Company)
            {
                if (User.CompanyKey == (item as Company).UId && Principal.IsInRole("Company Administrators"))
                    return true;
                else
                    return false;
            }

            // Now we check if the item is an INode which holds the security permissions.
            if (item is INode)
                return CanAccessINode(item as INode, action);

            // Now we grab the INode and check permissions on them.
            if (item is INodeItem)
                return CanAccessINode((item as INodeItem).GetNode(), action);
            else
                return false;
            //*/
        }

        protected bool CanAccessINode(INode node, SecurityAccessType action)
        {
            //return true;
            //*
            if (node == null)
                return true;

            if (node is ICompanyHolder && (node as ICompanyHolder).CompanyKey == User.CompanyKey && Principal.IsInRole("Company Administrators"))
                return true;

            if (node is Company && User.WorkingCompanyKey == node.UId)
                return true;

            var permissions = Search<RSToolKit.Domain.Security.PermissionSet>(p => p.Target == node.UId, checkAccess: false);
            RSToolKit.Domain.Security.PermissionSet permission = null;
            permission = permissions.FirstOrDefault(p => p.Owner == User.UId);
            if (permission == null || !permission.Access(action))
            {
                permission = permissions.FirstOrDefault(p => p.Owner == User.CompanyKey);
            }
            if (permission == null || !permission.Access(action))
            {
                foreach (var group in User.CustomGroups)
                {
                    permission = permissions.FirstOrDefault(p => p.Owner == group.UId);
                    if (permission != null && permission.Access(action))
                        return true;
                }
            }
            permission = permission ?? permissions.FirstOrDefault(p => p.Owner == Guid.Empty);
            permission = permission ?? new RSToolKit.Domain.Security.PermissionSet();
            return permission.Access(action);
            //*/
        }

        /// <summary>
        /// Clears a list of items as pertaining to a list.
        /// </summary>
        /// <typeparam name="Titem">The items in the list.</typeparam>
        /// <param name="items">The list of items.</param>
        /// <returns>Returns an enumeration of the items cleared.</returns>
        public IEnumerable<Titem> ClearList<Titem>(IList<Titem> items)
        {
            var enumerable = items.ToList().AsEnumerable();
            items.Clear();
            return enumerable;
        }
    }
}
