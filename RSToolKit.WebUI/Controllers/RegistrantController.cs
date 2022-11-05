using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.Domain.Exceptions;
using RSToolKit.WebUI.Infrastructure;
using System;
using System.Web.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using RSToolKit.Domain.JItems;
using System.Collections.Generic;
using RSToolKit.Domain.Entities.Components;
using Microsoft.AspNet.Identity;
using RSToolKit.Domain;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Engines;
using System.Threading;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles="Super Administrators,Administrators,Cloud Users,Cloud+ Users,Programmers")]
    [RegStepHandleError(typeof(InvalidIdException), "InvalidIdException")]
    [RegStepHandleError(typeof(RegistrantNotFoundException), "RegistrantNotFound")]
    [RegStepHandleError(typeof(InsufficientPermissionsException), "InsufficientPermissionsException")]
    [RegStepHandleError(typeof(AdjustmentNotFoundException), "AdjustmentNotFoundException")]
    public class RegistrantController
        : RegStepController
    {

        protected RegistrantRepository _regRepo = null;

        /// <summary>
        /// Initializes the controller with a supplied context.
        /// </summary>
        public RegistrantController(EFDbContext context)
            : base(context)
        {
            this._regRepo = new RegistrantRepository(Context);
            Log.LoggingMethod = "RegistrantController";
            Repositories.Add(_regRepo);
        }

        /// <summary>
        /// Initializes the controller with a new context.
        /// </summary>
        public RegistrantController()
            : base()
        {
            this._regRepo = new RegistrantRepository(Context);
            Log.LoggingMethod = "RegistrantController";
            Repositories.Add(_regRepo);
        }

        #region Registrant

        [ComplexId("id")]
        [HttpGet]
        [MinimalView]
        [PopoutView]
        public ActionResult Index(object id)
        {
            var reg = this._GetReg(id);
            UpdateTrailLabel(reg.Form.Name + ": " + reg.Email);
            ViewBag.UserManager = UserManager;
            ViewBag.CanEdit = Context.CanAccess(reg, Domain.Security.SecurityAccessType.Write, true);
            return View("Index", reg);
        }

        [ComplexId("id")]
        [HttpGet]
        [MinimalView]
        [PopoutView]
        public ActionResult Get(object id)
        {
            return Index(id);
        }

        [ComplexId("id")]
        [ActionName("Index")]
        [HttpDelete]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteRegistrant(object id)
        {
            var reg = this._GetReg(id);
            var regId = reg.SortingId;
            var formId = reg.FormKey;
            this._regRepo.Remove(reg);
            var count = _regRepo.Commit();
            if (count > 0)
                ThreadPool.QueueUserWorkItem(FormReportController.RemoveTableRegistrantCallback, new object[] { regId, formId });
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = (count > 0) } };
        }

        [ComplexId("id")]
        [HttpDelete]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult MarkRegistrant(object id)
        {
            var reg = this._GetReg(id);
            reg.Status = RegistrationStatus.Deleted;
            this._regRepo.Commit();
            Task.Factory.StartNew(() => { FormReportController.UpdateTableRegistrant(reg.UId); });
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true } };
        }

        [ComplexId("id")]
        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Activate(object id)
        {
            var reg = this._GetReg(id);
            reg.Status = RegistrationStatus.Submitted;
            this._regRepo.Commit();
            Task.Factory.StartNew(() => { FormReportController.UpdateTableRegistrant(reg.UId); });
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true } };
        }

        [ComplexId("id")]
        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Deactivate(object id)
        {
            var reg = this._GetReg(id);
            reg.Status = RegistrationStatus.Canceled;
            this._regRepo.Commit();
            Task.Factory.StartNew(() => { FormReportController.UpdateTableRegistrant(reg.UId); });
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true } };
        }

        #endregion

        #region Registrant Data

        [HttpGet]
        [ActionName("Data")]
        [ComplexId("id")]
        [IsAjax]
        public JsonNetResult GetRegistrantData(object id, string component, float width)
        {
            long cLid;
            var registrant = this._GetReg(id);
            var comp_html = "";
            if (long.TryParse(component, out cLid))
            {
                var comp = Context.Components.FirstOrDefault(c => c.SortingId == cLid);
                if (comp == null)
                    throw new RegStepException("The component could not be found.");
                comp_html = HtmlComponentBuilder.Render(comp, registrant);
            }
            else
            {
                switch (component.ToLower())
                {
                    case "rsvp":
                        comp_html += "<div class=\"form-messagebox form-error-message\" style=\"display: none;\"><div class=\"form-messagebox-message\"></div></div><div class=\"row\"><label class=\"col-xs-12\">RSVP</label><select id=\"RSVP\" class=\"form-control\"><option value=\"true\"" + (registrant.RSVP ? " selected=\"true\"" : "") + " >" + registrant.Form.RSVPAccept + "</option><option value=\"false\"" + (!registrant.RSVP ? " selected=\"true\"" : "") + ">" + registrant.Form.RSVPDecline + "</option></select></div>";
                        break;
                    case "audience":
                        comp_html += "<div class=\"form-messagebox form-error-message\" style=\"display: none;\"><div class=\"form-messagebox-message\"></div></div><div class=\"row\"><label class=\"col-xs-12\">Audience</label>";
                        comp_html += "<select id=\"Audience\" class=\"form-control\">";
                        foreach (var audience in registrant.Form.Audiences.OrderBy(a => a.Order))
                        {
                            comp_html += "<option" + (registrant.Audience == audience ? " selected=\"true\"" : "") + " value=\"" + audience.UId + "\">" + audience.Label + "</option>";
                        }
                        comp_html += "</select></div>";
                        break;
                    case "email":
                        comp_html += "<div class=\"form-messagebox form-error-message\" style=\"display: none;\"><div class=\"form-messagebox-message\"></div></div><div class=\"row\"><label class=\"col-xs-12\">Email</label>";
                        comp_html += "<input class='form-control' id='Email' />";
                        comp_html += "</div>";
                        break;
                }
            }
            if (String.IsNullOrEmpty(comp_html))
                throw new RegStepException("Invalid component.");
            comp_html += "<input type='hidden' id='editingComp' value='" + component + "' /><input type='hidden' id='editingReg' value='" + id + "' />";
            var parser = new RSParser(registrant, Context, registrant.Form, null, registrant.Contact);
            comp_html = parser.ParseAvailable(comp_html);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, html = comp_html } };
        }

        [HttpPut]
        [ActionName("Data")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult SaveRegistrantData(ajaxRegistrantData model)
        {
            // Lets grab the registrant
            long rLid;
            Guid rUid;
            Registrant registrant;
            if (long.TryParse(model.registrantId, out rLid))
                registrant = Context.Registrants.FirstOrDefault(r => r.SortingId == rLid);
            else if (Guid.TryParse(model.registrantId, out rUid))
                registrant = Context.Registrants.Find(rUid);
            else
                throw new InvalidIdException();
            if (registrant == null)
                throw new RegistrantNotFoundException();
            var result = registrant.SetData(model.componentId, model.value, ignoreRequired: true, ignoreValidation: true);
            if (!result.Success)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = false, errors = result.Errors.Select(e => e.Message) } };
            registrant.UpdateAccounts();
            this._regRepo.Commit();
            // Now we need to see if any FormReportController table rows need updated.
            Task.Factory.StartNew(() => { FormReportController.UpdateTableRegistrant(registrant.UId); });
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, registrantId = registrant.SortingId.ToString(), value = result.PrettyValue, id = result.Id, headerId = model.componentId } };
        }

        [HttpGet]
        [NoTrail]
        public FileContentResult File(Guid id)
        {
            var data = Context.RegistrantData.FirstOrDefault(d => d.UId == id);
            if (!Context.CanAccess(data, Domain.Security.SecurityAccessType.Read))
                throw new InsufficientPermissionsException(Domain.Security.SecurityAccessType.Read);
            if (data == null)
                throw new ArgumentNullException();
            if (data.File == null)
                throw new ArgumentNullException();
            var component = data.Component;
            var variable = data.Component.Variable.Value;
            if (component != null)
                variable = component.Variable.Value;
            return File(data.File.BinaryData, data.File.FileType, variable + data.File.Extension);
        }

        [HttpGet]
        [AllowAnonymous]
        [ComplexId("id")]
        [NoTrail]
        public FileContentResult Thumbnail(object id, Guid component, int? width = null)
        {
            var registrant = this._GetReg(id, false);
            var binaryData = new byte[0];
            if (registrant == null)
                return new FileContentResult(binaryData, "image/jpg");
            var dataPoint = registrant.Data.Where(d => d.VariableUId == component).FirstOrDefault();
            if (dataPoint == null)
                return new FileContentResult(binaryData, "image/jpg");
            if (dataPoint.File == null)
                return new FileContentResult(binaryData, "image/jpg");
            using (var m_stream = new MemoryStream(dataPoint.File.BinaryData))
            using (var img_stream = new MemoryStream())
            {
                Image img = Image.FromStream(m_stream);
                Image thumbNail = img;
                if (width.HasValue && width.Value < img.Width)
                {
                    var callback = new Image.GetThumbnailImageAbort(this._ThumbnailCallback);
                    var height = (width.Value * img.Height) / img.Width;
                    thumbNail = img.GetThumbnailImage(width.Value, height, callback, new IntPtr());
                }
                var encoder = _GetEncoderInfo(dataPoint.File.FileType);
                var encoderQuality = Encoder.Quality;
                if (encoder == null)
                    return new FileContentResult(binaryData, "image/jpg");
                var encoderParams = new EncoderParameters(1);
                var encoderParam = new EncoderParameter(encoderQuality, 25L);
                encoderParams.Param[0] = encoderParam;
                thumbNail.Save(img_stream, encoder, encoderParams);
                binaryData = img_stream.ToArray();
            }
            return new FileContentResult(binaryData, dataPoint.File.FileType);
        }

        [HttpGet]
        [ComplexId("id")]
        [PopoutView]
        public ActionResult ChangeSet(object id)
        {
            var registrant = this._GetReg(id);
            if (registrant == null)
                throw new RegistrantNotFoundException();
            if (Request.IsAjaxRequest())
            {
                var form = registrant.Form;
                var table = new JsonTable();
                table.Name = registrant.Email + ": Change Log";
                table.Id = registrant.UId;
                var modifiers = new Dictionary<Guid, String>() { { AppUser.UId, AppUser.UserName }, { Guid.Empty, "Registrant" } };
                table.Headers = registrant.Form.GetRegistrantHeaders();
                var curRow = new JsonTableRow() { Id = registrant.SortingId, Token = registrant.UId };
                var curCols = new List<JsonTableValue>();
                foreach (var header in table.Headers)
                    curRow.Values.Add(registrant.GetJsonTableValue(header));
                table.Rows.Add(curRow);
                foreach (var oReg in registrant.OldRegistrations.OrderByDescending(o => o.DateCreated))
                {
                    var oRow = new JsonTableRow() { Id = oReg.SortingId };
                    foreach (var header in table.Headers)
                        oRow.Values.Add(oReg.GetJsonTableValue(header, Context));
                    table.Rows.Add(oRow);
                }
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, table = table } };
            }
            else
            {
                UpdateTrailLabel(registrant.Email + " Record Changes");
                return View(registrant);
            }
        }

        #endregion

        #region Transactions

        [ComplexId("id")]
        [HttpPost]
        [ActionName("Adjustment")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewAdjustment(object id, decimal amount, string description, string type, string transactionId, DateTimeOffset? transactionDate)
        {
            var reg = this._GetReg(id);
            if (String.IsNullOrEmpty(description))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = false, message = "Description is required" } };
            var adjustment = new Adjustment()
            {
                Name = description,
                Amount = amount,
                AdjustmentType = type,
                TransactionId = transactionId,
                TransactionDate = transactionDate,
                UId = Guid.NewGuid()
            };
            if (transactionDate.HasValue)
                adjustment.DateCreated = transactionDate.Value;
            if (adjustment.AdjustmentType != "Adjustment")
                adjustment.TransactionId = transactionId;
            reg.Adjustments.Add(adjustment);
            this._regRepo.Commit();
            Task.Factory.StartNew(() => { FormReportController.UpdateTableRegistrant(reg.UId); });
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true } };
        }

        [ComplexId("id")]
        [HttpDelete]
        [ActionName("Adjustment")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult VoidAdjustment(object id)
        {
            var adjustment = _regRepo.FindAdjustment(id);
            adjustment.Voided = true;
            adjustment.VoidedBy = AppUser.Id;
            this._regRepo.Commit();
            Task.Factory.StartNew(() => { FormReportController.UpdateTableRegistrant(adjustment.Registrant.UId); });
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, id = adjustment.SortingId, voider = AppUser.UserName } };
        }

        #endregion

        [IsAjax]
        [JsonValidateAntiForgeryToken]
        [ComplexId("id")]
        public JsonNetResult Email(object id, Guid emailId)
        {
            var reg = this._GetReg(id);
            IEmail email = this._GetEmail(emailId);
            var response = SendRegistrantEmail(reg, email);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = response.Success, message = "Email Status: " + response.Response, title = "Email Send Response", type = "message" } };
        }

        #region Helpers

        protected static ImageCodecInfo _GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        protected bool _ThumbnailCallback() { return true; }

        protected Registrant _GetReg(object id, bool usePermissions = true)
        {
            Registrant reg = null;
            if (id is Guid)
                reg = this._regRepo.Find((Guid)id, usePermissions);
            else if (id is long)
                reg = this._regRepo.First(r => r.SortingId == (long)id, usePermissions);
            else
                throw new InvalidIdException();
            return reg;
        }

        protected IEmail _GetEmail(Guid id)
        {
            IEmail email = null;
            email = Context.RSEmails.Find(id);
            if (email == null)
                email = Context.RSHtmlEmails.Find(id);
            if (email == null)
                throw new RegistrantNotFoundException("The email was not found.");
            return email;
        }

        protected EmailSendResponse SendRegistrantEmail(Registrant registrant, IEmail email)
        {
            // Now we grab the smtpServer.
            if (!SmtpServer.CanSend(email, registrant))
                return new EmailSendResponse() { Success = false, Response = "The email cannot be sent due to time interval or send count." };
            var form = registrant.Form;
            SmtpServer smtpServer = null;
            smtpServer = Context.SmtpServers.FirstOrDefault(s => s.CompanyKey == registrant.Form.CompanyKey);
            if (smtpServer == null)
                smtpServer = Context.SmtpServers.FirstOrDefault(s => s.Name == "Primary");
            if (smtpServer == null)
                return new EmailSendResponse() { Success = false };
            // We need to create the sendgrid header
            var header = new SendGridHeader();
            // Now we grab the parser and intialize the list of mail messages.
            var contact = Context.Contacts.FirstOrDefault(c => c.CompanyKey == form.CompanyKey && c.Name == registrant.Email);
            var parser = new RSParser(registrant, Context, form, null, contact);
            var emailMessages = new List<Tuple<EmailData, Guid?>>();
            var message = email.GenerateEmail(parser);
            var sending = true;
            var work = LogicEngine.RunLogic(email, registrant: registrant as Registrant, contact: registrant.Contact);
            //var work = email.RunLogic(person, true, Repository).ToList();
            if (work.Count() > 0)
                sending = work.First().Command == JLogicWork.SendEmail;
            if (!sending)
                return new EmailSendResponse() { Success = false, Response = "Logic kept email from sending." };
            return smtpServer.SendEmail(message, Context, form.Company, form, registrant);
        }


        #endregion
    }
}