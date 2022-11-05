using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Collections;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Exceptions;
using RSToolKit.Domain.Data;
using System.Threading;
using RSToolKit.WebUI.Models.Views.Contact;
using RSToolKit.WebUI.Models.Inputs.Contact;
using System.IO;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using RSToolKit.Domain;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles = "Super Administrators,Administrators,Cloud Users,Cloud+ Users,Programmers")]
    public class ContactController
        : RegStepController
    {

        /// <summary>
        /// Holds the tokens for the contact list.
        /// </summary>
        public static TokenDictionary<ContactTableInformation> stTokens;
        public static Dictionary<Guid, ContactUploadListInput> stUploads;
        /// <summary>
        /// Excel MIME types.
        /// </summary>
        public static string[] excel_mime = new string[]
        {
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-excel",
            "application/msexcel",
            "application/x-msexcel",
            "application/x-ms-excel",
            "application/x-excel",
            "application/x-dos_ms_excel",
            "application/xls",
            "application/x-xls"
        };

        /// <summary>
        /// The current progress info.
        /// </summary>
        protected IProgressInfo _info;
        /// <summary>
        /// The current context being worked.
        /// </summary>
        protected Contact _contact;

        #region Constructors
        public ContactController()
            : base()
        {
            stTokens = stTokens ?? new TokenDictionary<ContactTableInformation>();
            stUploads = stUploads ?? new Dictionary<Guid, ContactUploadListInput>();
            this._info = null;
            this._contact = null;
        }
        #endregion

        // GET: Contact
        [HttpGet]
        [ComplexId("id")]
        public ActionResult Index(object id)
        {
            if (id is long && (long)id == -1)
                return List();
            var contact = this._GetContact(id);
            UpdateTrailLabel("Contact: " + contact.Email);
            return View("Index", contact);
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult Get(object id)
        {
            if (id is long && (long)id == -1)
                return List();
            return Index(id);
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Data(ContactDataInput input)
        {
            var contact = _GetContact(input.ContactId);
            var result = contact.SetData(input);
            if (result.Success)
                Context.SaveChanges();
            ThreadPool.QueueUserWorkItem(ContactController.UpdateContactCallback, contact.UId);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, id = contact.SortingId, token = contact.UId, value = result.PrettyValue } };
        }

        [HttpGet]
        public ActionResult List(Guid? id = null, int page = 1)
        {
            if (Request.IsAjaxRequest())
            {
                var token = this._GetToken(id);
                token.Page = 1;
                var pInfo = token.GetProgress();
                if (pInfo.Progress < 1)
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = false, token = token.Key, retry = true, progress = pInfo.Progress, message = pInfo.Message } };
                var table = token.Item.GetTable(token);
                var filterHeaders = token.Item.FilterHeaders;
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, table = table, headers = filterHeaders } };
            }
            else
            {
                var view = new ListView()
                    {
                        Name = WorkingCompany.Name,
                        TokenRequestKey = WorkingCompany.UId
                    };
                return View("List", view);
            }
        }

        [HttpPut]
        [IsAjax]
        public JsonNetResult Set(Guid id, List<JsonFilter> filters = null, List<JsonSorting> sortings = null, List<JsonTableHeader> headers = null, int? recordsPerPage = null)
        {
            var token = this._GetToken(id);
            if (filters != null || sortings != null)
                token.Page = 1;
            token.Filters = filters ?? token.Filters;
            token.Sortings = sortings ?? token.Sortings;
            token.Headers = headers ?? token.Headers;
            token.RecordsPerPage = recordsPerPage ?? token.RecordsPerPage;
            var table = token.Item.GetTable(token);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, table = table } };
        }

        #region Upload Actions
        [HttpGet]
        public ActionResult Upload()
        {
            var view = new UploadView();
            view.SavedLists = Context.SavedLists.Where(l => l.CompanyKey == WorkingCompany.UId).OrderBy(l => l.Name).Select(l => new SavedListView() { Key = l.UId, Label = l.Name }).ToList();
            return View("Upload", view);
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Upload(Guid? id)
        {
            // We need to grab the file.
            var file = Request.Files[0];
            if (file == null)
                throw new FileNotFoundException("You must upload a file.");
            if (!excel_mime.Contains(file.ContentType))
                // If the file is not excel we send back a failure message.
                throw new FormatException("The file must be an excel file.");

            // Next we need to handle the file;
            // First we assign a file unique identifier
            var fileId = Guid.NewGuid();
            // Now we save the file in case the server crashes.
            // To do this we need to create a file path
            var path = Path.Combine(Server.MapPath("~/TempFiles/ContactListSheets"), fileId.ToString() + Path.GetExtension(file.FileName));
            file.SaveAs(path);

            //Now we need to create an object to hold data.
            var uploadModel = new ContactUploadListInput()
            {
                CompanyKey = WorkingCompany.UId,
                SavedListKey = id,
                FilePath = path,
                Token = fileId
            };
            stUploads.Add(fileId, uploadModel);

            // Now we start a new thread to handle the data and return a message that we got the file.
            ThreadPool.QueueUserWorkItem(ContactController.UploadContactListCallback, fileId);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = false, retry = true, postBack = Url.Action("UploadStatus", "Contact", new { id = fileId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        public JsonNetResult UploadStatus(Guid id)
        {
            ContactUploadListInput upload = null;
            if (stUploads.Keys.Contains(id))
            {
                upload = stUploads[id];
                if (upload.CriticalFailure)
                {
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = false, retry = false, message = upload.Progress.Message } };
                }
                if (upload.JobDone)
                {
                    stUploads.Remove(id);
                    if (System.IO.File.Exists(upload.FilePath))
                        System.IO.File.Delete(upload.FilePath);
                }
            }
            if (upload == null)
                throw new ArgumentNullException("The upload not found in memory.");
            var url = "";
            if (upload.JobDone && !upload.CriticalFailure)
                url = Url.Action("List", "Contact", null, Request.Url.Scheme);
            else if (upload.NeedsRectified)
                url = Url.Action("RectifyUpload", "Contact", new { id = id }, Request.Url.Scheme);
            else
                url = Url.Action("UploadStatus", "Contact", new { id = id });
            var retry = !upload.CriticalFailure && !upload.NeedsRectified;
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = upload.JobDone, postBack = url, retry = retry, complete = upload.JobDone, progress = upload.Progress.Progress, message = upload.Progress.Message } };
        }

        [HttpGet]
        public ActionResult RectifyUpload(Guid id)
        {
            ContactUploadListInput uploadModel = null;
            if (!stUploads.Keys.Contains(id))
                throw new FileNotFoundException("The upload file was not found.");
            else
                uploadModel = stUploads[id];
            var uploadView = new ContactUploadListView()
            {
                ListInput = uploadModel
            };
            uploadView.Headers = Context.ContactHeaders.Where(c => c.CompanyKey == uploadModel.CompanyKey && c.SavedListKey == null).ToList();
            if (uploadModel.SavedListKey.HasValue)
                uploadView.Headers.AddRange(Context.ContactHeaders.Where(c => c.CompanyKey == uploadModel.CompanyKey && c.SavedListKey == uploadModel.SavedListKey));
            return View("RectifyUpload", uploadView);
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult RectifyUpload(RectifyContactUploadListInput model)
        {
            ContactUploadListInput uploadModel = null;
            if (!stUploads.Keys.Contains(model.Token))
                throw new FileNotFoundException("The uploaded file was not found.");
            else
                uploadModel = stUploads[model.Token];
            uploadModel.NeedsRectified = false;
            ThreadPool.QueueUserWorkItem(ContactController.RectifyContactListCallback, model);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = false, retry = true, postBack = Url.Action("UploadStatus", "Contact", new { id = model.Token }, Request.Url.Scheme) } };
        }

        #region Contact Upload Helpers'
        /// <summary>
        /// Handles the contact list upload.
        /// </summary>
        /// <param name="arg">The arguments being handled.</param>
        public static void UploadContactListCallback(object arg)
        {
            if (!(arg is Guid))
                return;
            var token = (Guid)arg;
            if (!stUploads.ContainsKey(token))
                return;
            var upload = stUploads[token];
            using (var context = new EFDbContext())
            {
                var company = context.Companies.Find(upload.CompanyKey);
                if (company == null)
                {
                    upload.Progress.Update(-1F, "The company does not exist.");
                    upload.CriticalFailure = true;
                }
                var contacts = context.Contacts.Where(c => c.CompanyKey == upload.CompanyKey).ToList();
                var c_headers = context.ContactHeaders.Where(h => h.CompanyKey == upload.CompanyKey && h.SavedListKey == null).ToList();
                if (upload.SavedListKey.HasValue)
                    c_headers.AddRange(context.ContactHeaders.Where(h => h.SavedListKey == upload.SavedListKey.Value).ToList());
                var currentCount = contacts.Count;

                // First lets check to see how many spots we have open
                var clSpotsFree = company.ContactLimit - currentCount;

                // Next we grab the file

                SavedList savedList = null;
                if (upload.SavedListKey.HasValue)
                    savedList = context.SavedLists.Find(upload.SavedListKey);

                // Now we run through the uploaded excel file.
                if (!System.IO.File.Exists(upload.FilePath))
                {
                    upload.CriticalFailure = true;
                    upload.Progress.Update(message: "The file no longer exists.");
                }
                using (var u_package = new ExcelPackage(new FileInfo(upload.FilePath)))
                {
                    try
                    {
                        upload.Progress.Update(message: "Analyzes uplaoded data.");
                        var progressPerTick = 0F;
                        var totalRows = 0;
                        var totalCols = 0;
                        // We grab the first sheet here
                        if (upload.SheetSelection == -1)
                            upload.SheetSelection = 1;
                        var u_sheet = u_package.Workbook.Worksheets[1];
                        // Now we need to grab the number of rows.
                        for (var i = 1; !String.IsNullOrWhiteSpace(u_sheet.Cells[1, i].Text) || u_sheet.Cells[1, i].Text == "!end!"; i++)
                        {
                            totalCols++;
                            if (totalCols > 1000)
                            {
                                upload.CriticalFailure = true;
                                upload.Progress.Update(message: "Only 1000 headers can be uploaded at a time.");
                                return;
                            }
                        }
                        // We grab all the headers for the contact list and saved list if specified.
                        // Now we grab the headers from the excel sheet
                        for (var i = 1; !String.IsNullOrWhiteSpace(u_sheet.Cells[i, 1].Text) || u_sheet.Cells[i, 1].Text.ToLower() == "!end"; i++)
                        {
                            totalRows++;
                            if (totalRows > 2500)
                            {
                                upload.CriticalFailure = true;
                                upload.Progress.Update(message: "Only 2500 contacts can be uploaded at a time.");
                                return;
                            }
                        }
                        progressPerTick = (float)0.8 / ((float)(totalCols * totalRows));
                        var progressPerHeader = (float)0.1 / (float)totalCols;
                        upload.Progress.Update(message: "Retrieving headers from table.");
                        // Now we put the headers into the columns.
                        ContactUploadHeaderInput h_emailHeader = null;
                        for (var i = 1; i <= totalCols; i++)
                        {
                            var u_header = new ContactUploadHeaderInput()
                            {
                                HeaderName = u_sheet.Cells[1, i].Text,
                                CellIndex = i
                            };
                            var headerSearch = u_header.HeaderName.ToLower();
                            if (headerSearch.In("email", "e-mail"))
                            {
                                // The header is email and we this is the primary key.  It will not match to a contact header
                                u_header.HeaderKey = null;
                                u_header.Email = true;
                                h_emailHeader = u_header;
                            }
                            else
                            {
                                upload.Headers.Add(u_header);
                                // Lets see if we can find an exact match of the contact list headers as lower case
                                var m_header = c_headers.Where(h => h.Name.ToLower() == headerSearch).FirstOrDefault();
                                if (m_header == null)
                                {
                                    // We did not find a match, lets try a few different methods
                                    // Lets try removing spaces now
                                    headerSearch = Regex.Replace(headerSearch, @"\s", "");
                                    m_header = c_headers.Where(h => h.Name.ToLower() == headerSearch).FirstOrDefault();
                                    if (m_header == null)
                                        // We still found nothing.
                                        u_header.HeaderKey = null;
                                }
                                if (m_header != null)
                                {
                                    // If we found something, we will set the header to the contact header UId
                                    u_header.HeaderKey = m_header.UId;
                                }
                            }
                            upload.Progress.UpdateAdd(progress: progressPerHeader);
                        }
                        // Now we read the contents of the file in
                        upload.Progress.Update(message: "Processing contact data.");
                        // Now we read the actual contact data
                        if (h_emailHeader == null)
                        {
                            upload.CriticalFailure = true;
                            upload.Progress.Update(message: "You must provide the email column");
                            return;
                        }
                        for (var i = 2; !String.IsNullOrWhiteSpace(u_sheet.Cells[i, 1].Text) || u_sheet.Cells[1, i].Text.ToLower() == "!end"; i++)
                        {
                            var u_contact = new ContactUploadInput();
                            u_contact.Email = u_sheet.Cells[i, h_emailHeader.CellIndex].Text;
                            // Lets see if this contact exists.

                            var o_contact = contacts.FirstOrDefault(c => u_contact.Email.ToLower().In(c.GetEmails().ToArray()));
                            if (o_contact != null)
                            {
                                // We found a contact. We need to assign that to the contact upload.
                                u_contact.ContactKey = o_contact.UId;
                            }
                            upload.Contacts.Add(u_contact);
                            // Lets read in all the data
                            var j = 1;
                            foreach (var u_header in upload.Headers.Where(h => h.HeaderName.ToLower() != "email"))
                            {
                                j++;
                                // Lets read in the data
                                u_contact.Data.Add(new ContactDataUploadInput() { Header = u_header, Contact = u_contact, Value = u_sheet.Cells[i, u_header.CellIndex].Text });
                                upload.Progress.UpdateAdd(progress: progressPerTick);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        upload.CriticalFailure = true;
                        upload.Progress.Update(message: e.Message);
                        return;
                    }
                }
                string message = "";
                if (upload.Headers.Where(h => !h.HeaderKey.HasValue && !h.Email).Count() > 0)
                {
                    // There are some unassigned headers.  We need to set the message
                    message += "Not all the headers where matched. Please click here to fix the issues.<br />";
                }
                if (upload.Contacts.Where(c => c.ContactKey.HasValue).Count() > 0)
                {
                    // There are some unassigned headers.  We need to set the message
                    message += "Some of the contacts already exists.  Click here fix the issues.";
                }
                // If there was an error we need to allow it to be rectified
                if (!String.IsNullOrEmpty(message))
                {
                    upload.NeedsRectified = true;
                    upload.Progress.Update(message: message);
                    return;
                }

                // Lets check and see if the company has enough available slots;
                if (upload.Contacts.Count > clSpotsFree && company.ContactLimit != -1)
                {
                    upload.Progress.Update(message: "Your company has " + clSpotsFree + " available contact space and you tried uploading " + upload.Contacts.Count + ".");
                    upload.CriticalFailure = true;
                    return;
                }
            }
            InsertUploadList(upload);
        }

        /// <summary>
        /// Rectifies a contact list.
        /// </summary>
        /// <param name="arg">The RectifyContactListInput object.</param>
        public static void RectifyContactListCallback(object arg)
        {
            var model = arg as RectifyContactUploadListInput;
            if (model == null)
                return;
            var uploadModel = stUploads[model.Token];
            if (uploadModel == null)
                return;
            using (var context = new EFDbContext())
            {
                var company = context.Companies.Find(uploadModel.CompanyKey);
                if (company == null)
                {
                    uploadModel.CriticalFailure = true;
                    uploadModel.Progress.Update(message: "The company does not exists.");
                }
                var currentCount = context.Contacts.Count(c => c.CompanyKey == uploadModel.CompanyKey);
                var clSpotsFree = company.ContactLimit - currentCount;

                SavedList savedList = null;
                if (uploadModel.SavedListKey.HasValue)
                    savedList = context.SavedLists.Find(uploadModel.SavedListKey.Value);

                var progressPerTick = 0.8F / (float)((float)uploadModel.Contacts.Count * uploadModel.Headers.Count);
                // Grab all headers
                var c_headers = context.ContactHeaders.Where(c => c.CompanyKey == uploadModel.CompanyKey && c.SavedListKey == null).ToList();
                if (uploadModel.SavedListKey.HasValue)
                    c_headers.AddRange(context.ContactHeaders.Where(c => c.CompanyKey == uploadModel.CompanyKey && c.SavedListKey == uploadModel.SavedListKey).ToList());
                uploadModel.Progress.Update(0.1F, "Processing contact data.");
                // Now we will go through and match up headers.
                foreach (var header in model.Headers)
                {
                    var o_header = uploadModel.Headers.Where(h => h.HeaderName == header.HeaderName).FirstOrDefault();
                    o_header.HeaderKey = header.HeaderKey;
                    if (o_header.HeaderKey == Guid.Empty)
                    {
                        // The user told us to create this header.
                        // We will create it as an ordinary text field.
                        var newHeader = new ContactHeader()
                        {
                            Name = o_header.HeaderName,
                            CompanyKey = uploadModel.CompanyKey,
                            UId = Guid.NewGuid()
                        };
                        o_header.HeaderKey = newHeader.UId;
                        context.ContactHeaders.Add(newHeader);
                        context.SaveChanges();
                    }
                }

                // Lets check and see if the company has enough available slots;
                if (uploadModel.Contacts.Where(c => !c.ContactKey.HasValue).Count() > clSpotsFree && company.ContactLimit != -1)
                {
                    uploadModel.Progress.Update(message: "Your company has " + clSpotsFree + " available contact space and you tried uploading " + uploadModel.Contacts.Count + ".");
                    uploadModel.CriticalFailure = true;
                    return;
                }
            }
            InsertUploadList(uploadModel, model.Overwrite);
        }

        /// <summary>
        /// Inserts the data into the database.
        /// </summary>
        /// <param name="uploadModel">The upload model.</param>
        /// <param name="overwrite">Set to true to overwrite data.</param>
        public static void InsertUploadList(ContactUploadListInput uploadModel, bool overwrite = false)
        {
            using (var context = new EFDbContext())
            {
                var company = context.Companies.Find(uploadModel.CompanyKey);
                if (company == null)
                {
                    uploadModel.CriticalFailure = true;
                    uploadModel.Progress.Update(message: "The company does not exist.");
                }
                var contacts = company.Contacts.ToList();
                var currentCount = context.Contacts.Count(c => c.CompanyKey == uploadModel.CompanyKey);
                var clSpotsFree = company.ContactLimit - currentCount;
                var totalData = (uploadModel.Headers.Count + 1) * uploadModel.Contacts.Count;
                var h_perContact = (uploadModel.Headers.Count + 1);
                var h_count = 0;

                // Grab all headers
                var c_headers = context.ContactHeaders.Where(c => c.CompanyKey == uploadModel.CompanyKey && c.SavedListKey == null).ToList();
                if (uploadModel.SavedListKey.HasValue)
                    c_headers.AddRange(context.ContactHeaders.Where(c => c.CompanyKey == uploadModel.CompanyKey && c.SavedListKey == uploadModel.SavedListKey).ToList());

                SavedList savedList = null;
                if (uploadModel.SavedListKey.HasValue)
                    savedList = context.SavedLists.Find(uploadModel.SavedListKey.Value);

                // Now we add the data to the database.
                var percentPerTick = 0.1F / ((float)uploadModel.Headers.Count * uploadModel.Contacts.Count);
                uploadModel.Progress.Update(message: "Inserting data into database.");
                var contactsToUpdate = new List<Contact>();
                foreach (var contact in uploadModel.Contacts)
                {
                    Contact o_contact = null;
                    if (contact.ContactKey.HasValue)
                        o_contact = contacts.FirstOrDefault(c => c.UId == contact.ContactKey);
                    if (o_contact != null && !overwrite)
                    {
                        h_count += h_perContact;
                        continue;
                    }

                    h_count++;
                    Contact u_contact;
                    if (o_contact == null)
                    {
                        u_contact = new Contact()
                        {
                            Email = contact.Email,
                            CompanyKey = uploadModel.CompanyKey,
                            UId = Guid.NewGuid()
                        };
                        context.Contacts.Add(u_contact);
                    }
                    else
                    {
                        u_contact = o_contact;
                    }
                    contactsToUpdate.Add(u_contact);
                    foreach (var data in contact.Data)
                    {
                        h_count++;
                        uploadModel.Progress.UpdateAdd(percentPerTick);
                        if (String.IsNullOrWhiteSpace(data.Value))
                            continue;
                        if (!data.Header.HeaderKey.HasValue)
                            continue;
                        var o_data = u_contact.Data.Where(d => d.HeaderKey == data.Header.HeaderKey).FirstOrDefault();
                        if (o_data == null)
                        {
                            var c_header = c_headers.Where(h => h.UId == data.Header.HeaderKey).FirstOrDefault();
                            if (c_header == null)
                                continue;
                            var n_data = ContactData.New(u_contact, c_header, data.Value);
                        }
                        else
                        {
                            o_data.Value = data.Value;
                        }
                    }
                    if (savedList != null)
                    {
                        if (savedList.Contacts.Where(c => c.UId == u_contact.UId).FirstOrDefault() == null)
                            savedList.Contacts.Add(u_contact);
                    }
                }
                uploadModel.JobDone = true;
                context.SaveChanges();
                foreach (var contact in contactsToUpdate)
                {
                    UpdateContactCallback(contact.UId);
                }
            }
        }
        #endregion
        #endregion

        #region Helpers
        /// <summary>
        /// Gets the contact from the database.
        /// </summary>
        /// <param name="id">The id as either long or guid.</param>
        /// <returns>The contact found.</returns>
        protected Contact _GetContact(object id)
        {
            Contact contact = null;
            if (id is Guid)
                contact = Context.Contacts.Find((Guid)id);
            else if (id is long)
                contact = Context.Contacts.FirstOrDefault(c => c.SortingId == (long)id);
            if (contact == null)
                throw new RegStepException("The contact does not exist.");
            if (contact.CompanyKey != WorkingCompany.UId)
                throw new InsufficientPermissionsException();
            this._contact = contact;
            return contact;
        }

        /// <summary>
        /// Retrieves the token specified if it exists. If the token does not exist or is not supplied, a new token is issued.
        /// If the ContactTableInformation does not exists, then the process of creating it is started.
        /// </summary>
        /// <param name="id">The id of the token.</param>
        /// <returns>The Token.</returns>
        protected JsonTableToken<ContactTableInformation> _GetToken(Guid? id)
        {
            JsonTableToken<ContactTableInformation> token = null;
            // We grab the info here. If it does not exists, it will be created in a new thread.
            var info = this._GetTableInfo(WorkingCompany.UId);
            if (id == null)
            {
                // In this case, we need to create a new token.
                token = stTokens.Create(WorkingCompany.UId);
            }
            else
            {
                // A token was supplied, so we grab it.
                token = stTokens.Get(id.Value);
                if (token == null)
                    // The token does not exists. We need to create one.
                    token = stTokens.Create(WorkingCompany.UId);
            }
            if (token == null)
                // The token still equals null. Something unexpected happened so we throw an error.
                throw new RegStepException("The token was not found and could not be created. This probably has something to do with generating a new token and the referenced table information being missing.");
            return token;
        }

        /// <summary>
        /// Gets the info for the table.
        /// </summary>
        /// <param name="id">The id of the company that the table references (this is also the key of the table).</param>
        /// <returns>The TableInformation</returns>
        protected ContactTableInformation _GetTableInfo(Guid id)
        {
            var info = stTokens.GetInfo(id);
            if (info == null)
            {
                // We need to retrieve the data.
                info = new ContactTableInformation(WorkingCompany.UId);
                stTokens.AddInfo(info);
                ThreadPool.QueueUserWorkItem(_StartTableLoad, info.Key);
            }
            return info;
        }

        /// <summary>
        /// Loads the
        /// </summary>
        /// <param name="ctx"></param>
        protected void _StartTableLoad(object ctx)
        {
            if (!(ctx is Guid))
                return;
            Guid id = (Guid)ctx;
            var info = stTokens.GetInfo(id);
            if (info == null)
                return;
            using (var context = new EFDbContext())
            {
                var company = context.Companies.Find(id);
                if (company == null)
                    return;
                info.UpdateTable(company);
            }
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Updates the contact in the memory table.
        /// </summary>
        /// <param name="oid">The id of the contact.</param>
        public static void UpdateContactCallback(object oid)
        {
            if (!(oid is Guid))
                return;
            var id = (Guid)oid;
            using (var context = new EFDbContext())
            {
                var contact = context.Contacts.Find(id);
                if (contact == null)
                    return;
                var info = stTokens.GetInfo(contact.CompanyKey);
                if (info == null)
                    return;
                // There was a table, so we need to update that row in the table.
                var row = info.Rows.FirstOrDefault(r => r.Id == contact.SortingId);
                if (row != null)
                {
                    // The row existed, we need clear the values currently there.
                    row.Values.Clear();
                }
                else
                {
                    // The row did not exists so we need to add it.
                    row = new JsonTableRow() { Id = contact.SortingId, Token = contact.UId };
                    info.Rows.Add(row);
                }
                // Now that we have the row, we need to add back in the updated values.
                foreach (var header in info.Headers)
                    row.Values.Add(contact.GetJsonValue(header));
                // The table has been modified so we create a new modification token for synchonisity.
            }
        }
        #endregion
    }
}