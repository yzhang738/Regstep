using Newtonsoft.Json;
using RSToolKit.Domain;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Security;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Entities.Clients;
using System.Threading;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles = "Super Administrators,Administrators,Form Builders,Programmers")]
    public class FormBuilderController : RSController
    {
        protected static List<Editor> Editors = new List<Editor>();
        protected DateTimeOffset LastEditorClean = DateTimeOffset.UtcNow;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (LastEditorClean.AddMinutes(30) < DateTimeOffset.UtcNow)
                Editors.Where(e => e.Date.AddMinutes(30) < DateTimeOffset.UtcNow).ToList().ForEach(e => Editors.Remove(e));
            string s_id = "";
            if (filterContext.RouteData.Values.Keys.Contains("id"))
                s_id = filterContext.RouteData.Values["id"].ToString();
            Guid id;
            if (Guid.TryParse(s_id, out id))
            {
                var editor = Editors.Where(e => e.UserKey == user.UId).FirstOrDefault();
                if (editor == null)
                {
                    editor = Editor.New(id, user);
                    Editors.Add(editor);
                }
                else
                {
                    editor.Component = id;
                    editor.Date = DateTimeOffset.UtcNow;
                }
                ViewBag.Editors = Editors.Where(e => e.Component == id).ToList();
            }
            else
                ViewBag.Editors = new List<Editor>();
        }

        public FormBuilderController(EFDbContext context)
            : base(context)
        {
            iLog.LoggingMethod = "FormBuilderController";
        }

        // GET: FormBuilder
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Forms = Repository.Search<Form>(f => !f.Survey && f.CompanyKey == company.UId).ToList();
            return View();
        }

        [Authorize(Roles = "Super Administrators,Administrators")]
        [HttpDelete]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult ClearRegistrations(Guid id)
        {
            foreach (var registrant in Repository.Search<Registrant>(r => r.FormKey == id).ToList())
            {
                Repository.Remove(registrant);
            }
            Repository.Commit();
            ThreadPool.QueueUserWorkItem(FormReportController.ClearTableCallback, id);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpGet]
        public ActionResult Start(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Index");
            return View(form);
        }

        [HttpPut]
        [ActionName("Start")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditStart(Form model)
        {
            var form = Repository.Search<Form>(f => f.UId == model.UId).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            form.Start = model.Start;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        #region Form

        [HttpPost]
        [ActionName("Survey")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewSurvey(Guid? formKey)
        {
            Form form = null;
            var name = "New Survey " + DateTimeOffset.UtcNow.ToString("s");
            if (formKey.HasValue)
                form = Repository.Search<Form>(f => f.UId == formKey.Value && !f.Survey).FirstOrDefault();
            if (form != null)
                name = "Survey for " + form.Name;
            var survey = Form.NewSurvey(Repository, company, user, name, form);
            //now we create the confirmaiton page
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("") } };
        }

        [HttpPost]
        [ActionName("Form")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewForm(string name = null)
        {
            var form = Form.New(Repository, company, user, name);
            var companyKey = company.UId;
            var folder = Repository.Search<Folder>(f => f.CompanyKey == companyKey && f.ParentKey == null).FirstOrDefault();
            if (folder == null)
            {
                folder = Folder.New(Repository, company, user, company.Name);
            }
            var pointer = new Pointer() { Target = form.UId, Folder = folder };
            folder.Pointers.Add(pointer);
                
            //Now we need to create the rsvp page
            var rsvpPage = Page.New(Repository, form, company, user, "RSVP Page", "RSVP Page", PageType.RSVP);
            var rsvpPanelText = Panel.New(Repository, rsvpPage, company, user, "RSVP Panel", "RSVP Panel");
            rsvpPanelText.Locked = true;
            var rsvpText = FreeText.New(Repository, rsvpPanelText, company, user, name: "RSVP Text", description: "RSVP Text", row: 1, order: 1);
            rsvpText.Locked = true;
            rsvpText.Html = "Please RSVP.";
            rsvpPage.PageNumber = 1;

            //Now we need to create the audience page
            var audPage = Page.New(Repository, form, company, user, "Audience Page", "Audience Page", PageType.Audience);
            var audPanelText = Panel.New(Repository, audPage, company, user, "Audience Panel", "Audience Panel");
            audPanelText.Locked = true;
            var audText = FreeText.New(Repository, audPanelText, company, user, name: "Audience Text", description: "Audience Text", row: 1, order: 1);
            audText.Locked = true;
            audText.Html = "Please Choose your audience.";
            audPage.PageNumber = 2;

            //now we create the confirmaiton page
            var confPage = Page.New(Repository, form, company, user, "Confirmation Page", "Confirmation Page", PageType.Confirmation);
            confPage.PageNumber = -1;
            var confPanelText = Panel.New(Repository, confPage, company, user, "Confirmation Panel", "Confirmation Panel");
            confPanelText.Locked = true;
            var confText = FreeText.New(Repository, confPanelText, company, user, name: "Confirmation Text", description: "Confirmation Text", row: 1, order: 1);
            confText.Locked = true;
            confText.Html = "Thank you for registering.";

            //Now we create the billing page
            var billPage = Page.New(Repository, form, company, user, "Billing Page", "Billing Page", PageType.Billing);
            billPage.PageNumber = -1;
            var billPanelText = Panel.New(Repository, billPage, company, user, "Billing Panel", "Billing Panel");
            billPanelText.Locked = true;
            var billText = FreeText.New(Repository, billPanelText, company, user, name: "Billing Text", description: "Billing Text", row: 1, order: 1);
            billText.Locked = true;
            billText.Html = "Here is your current bill.";

            //Now we create the billing confirmation page
            var billConfPage = Page.New(Repository, form, company, user, "Billing Confirmation Page", "Billing Confirmation Page", PageType.BillingComfirnmation);
            billConfPage.PageNumber = -1;
            var billConfPanelText = Panel.New(Repository, billConfPage, company, user, "BIlling Confirmation Panel", "Billing Confirmation Panel");
            billConfPanelText.Locked = true;
            var billConfText = FreeText.New(Repository, billConfPanelText, company, user, name: "Billing Text", description: "Billing Text", row: 1, order: 1);
            billConfText.Locked = true;
            billConfText.Html = "Here is your current bill.";

            // Now we populate the styles
            var styles = Repository.Search<DefaultFormStyle>(ds => ds.CompanyKey == company.UId).ToList();
            foreach (var style in styles)
            {
                var nStyle = new FormStyle() { Owner = user.UId, Group = company.UId, GroupName = style.GroupName, Name = style.Name, Variable = style.Variable, Value = style.Value, Sort = style.Sort, SubSort = style.SubSort, Type = style.Type };
                form.FormStyles.Add(nStyle);
            }

            form.DefaultComponentOrders.AddRange(new DefaultComponentOrder[]
                {
                    new DefaultComponentOrder(typeof(CheckboxGroup)),
                    new DefaultComponentOrder(typeof(CheckboxItem)),
                    new DefaultComponentOrder(typeof(RadioGroup)),
                    new DefaultComponentOrder(typeof(RadioItem)),
                    new DefaultComponentOrder(typeof(Input)),
                    new DefaultComponentOrder(typeof(DropdownGroup))
                });

            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Form", "FormBuilder", new { id = form.UId }, Request.Url.Scheme) } };
        }

        [HttpPost]
        [ActionName("CopyForm")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult CopyForm(Guid formKey, Guid? companyKey = null, string name = null)
        {
            var form = Repository.Search<Form>(f => f.UId == formKey).FirstOrDefault();
            var company = form.Company;
            if (companyKey.HasValue)
                company = Repository.Search<Company>(c => c.UId == companyKey).FirstOrDefault();
            if (company == null)
                company = form.Company;
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            var newForm = form.DeepCopy(name, company, user, Repository);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Form", "FormBuilder", new { id = newForm.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [ActionName("Form")]
        public ActionResult GetForm(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Error404", "Error");
            if (form.Survey)
                ViewBag.Forms = Repository.Search<Form>(f => f.CompanyKey == company.UId).ToList();
            return View("Form", form);
        }

        [HttpPut]
        [ActionName("Form")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditForm(Form modal)
        {
            Form form = null;
            form = Repository.Search<Form>(f => f.UId == modal.UId).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            form.Name = modal.Name ?? form.Name;
            form.AccessType = modal.AccessType;
            form.BillingOption = modal.BillingOption;
            form.Cancelable = modal.Cancelable;
            form.Close = modal.Close;
            form.Currency = modal.Currency;
            form.DateModified = DateTimeOffset.UtcNow;
            form.Description = modal.Description;
            form.Editable = modal.Editable;
            form.CultureString = modal.CultureString;
            form.Tax = modal.Tax;
            form.TaxDescription = modal.TaxDescription;
            form.CoordinatorEmail = modal.CoordinatorEmail;
            form.CoordinatorName = modal.CoordinatorName;
            form.CoordinatorPhone = modal.CoordinatorPhone;
            form.Status = modal.Status;
            form.DisableShoppingCart = modal.DisableShoppingCart;
            form.EventEnd = modal.EventEnd;
            form.EventStart = modal.EventStart;
            form.EventTimeZone = modal.EventTimeZone;
            form.Location = modal.Location;
                
            if (modal.EmailListKey.HasValue)
            {
                var list = Repository.Search<IEmailList>(l => l.UId == modal.EmailListKey.Value).FirstOrDefault();
                form.EmailList = list;
            }
            else
            {
                form.ContactReport = null;
                form.ContactReportKey = null;
                form.InvitationList = null;
                form.InvitationListKey = null;
            }

            var merchant = Repository.Search<RSToolKit.Domain.Entities.MerchantAccount.MerchantAccountInfo>(m => m.UId == modal.MerchantAccountKey).FirstOrDefault();
            form.MerchantAccount = merchant;
            form.ModificationToken = Guid.NewGuid();
            form.ModifiedBy = user.UId;
            form.Open = modal.Open;
            form.Price = modal.Price;
            form.LoginHeaders = modal.LoginHeaders;
            var type = Repository.Search<NodeType>(t => t.UId == modal.TypeKey).FirstOrDefault();
            form.Type = type;
            var tags = JsonConvert.DeserializeObject<Guid[]>(Request.Form["tags"]);
            for (var i = 0; i < form.Tags.Count; i++)
            {
                if (!tags.Contains(form.Tags[i].UId))
                {
                    form.Tags.RemoveAt(i);
                    i--;
                }
            }
            foreach (var tag in tags)
            {
                if (form.Tags.Where(t => t.UId == tag).Count() != 1)
                {
                    var newTag = Repository.Search<Tag>(t => t.UId == tag).FirstOrDefault();
                    if (newTag != null)
                        form.Tags.Add(newTag);
                }
            }
            var pages = new List<PageInfo>();
            var pagePosition = 0;
            while (Request.Form.AllKeys.Contains("pages[" + pagePosition + "].UId"))
            {
                var uid = Request.Form["pages[" + pagePosition + "].UId"];
                var en = Request.Form["pages[" + pagePosition + "].Enabled"];
                var enCheck = Regex.IsMatch(en, "true") ? true : false;
                pages.Add(new PageInfo() {
                    UId = Guid.Parse(uid),
                    Enabled = enCheck,
                    PageNumber = int.Parse(Request.Form["pages[" + pagePosition + "].PageNumber"])
                });
                pagePosition++;
            }
            foreach (var page in pages)
            {
                var actualPage = form.Pages.FirstOrDefault(p => p.UId == page.UId);
                if (actualPage != null)
                {
                    actualPage.PageNumber = page.PageNumber;
                    actualPage.Enabled = page.Enabled;
                }
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        [HttpDelete]
        [ActionName("Form")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteForm(Guid id)
        {
            Form form = null;
            form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            form.Delete(Context);
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        #region PromotionCodes

        [HttpGet]
        public ActionResult PromotionCodes(Guid id, string error = null)
        {
            ViewBag.Error = error;
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Index");
            return View(form);
        }

        [HttpPost]
        [ActionName("PromotionCode")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewPromotionCode(Guid id, string codeEntry = null)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var code = PromotionCode.New(Repository, form, user, codeEntry);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("PromotionCode", "FormBuilder", new { id = code.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [ActionName("PromotionCode")]
        public ActionResult EditPromotionCode(Guid id)
        {
            var code = Repository.Search<PromotionCode>(p => p.UId == id).FirstOrDefault();
            if (code == null)
                return RedirectToAction("Index");
            return View("PromotionCode", code);
        }

        [HttpPut]
        [ActionName("PromotionCode")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditPromotionCode(PromotionCode model)
        {
            var code = Repository.Search<PromotionCode>(p => p.UId == model.UId).FirstOrDefault();
            if (code == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            code.Code = model.Code;
            code.Amount = model.Amount;
            code.Description = model.Description;
            code.Action = model.Action;
            code.Limit = model.Limit;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        [HttpDelete]
        [ActionName("PromotionCode")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeletePromotionCode(Guid id)
        {
            var code = Repository.Search<PromotionCode>(p => p.UId == id).FirstOrDefault();
            if (code == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            if (code.Entries.Count > 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Insufficient Permissions" } };
            Repository.Remove(code);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        #region Seating

        [HttpGet]
        public ActionResult Seatings(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Index");
            return View(form);
        }

        [HttpPost]
        [ActionName("Seating")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewSeating(Guid id, int maxSeats = -1, SeatingType type = SeatingType.NoSeating, bool waitlistable = false)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var seating = Seating.New(Repository, form, user, type, maxSeats, waitlistable);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Seating", "FormBuilder", new { id = seating.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [ActionName("Seating")]
        public ActionResult EditSeating(Guid id)
        {
            var seating = Repository.Search<Seating>(s => s.UId == id).FirstOrDefault();
            if (seating == null)
                return RedirectToAction("Index");
            return View("Seating", seating);
        }

        [HttpPut]
        [ActionName("Seating")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditSeating(Seating model)
        {
            var seating = Repository.Search<Seating>(s => s.UId == model.UId).FirstOrDefault();
            if (seating == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            seating.Name = model.Name;
            seating.ModificationToken = Guid.NewGuid();
            seating.ModifiedBy = user.UId;
            seating.DateModified = DateTimeOffset.UtcNow;
            seating.FullLabel = model.FullLabel;
            seating.Waitlistable = model.Waitlistable;
            seating.MaxSeats = model.MaxSeats;
            seating.WaitlistLabel = model.WaitlistLabel;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        [HttpDelete]
        [ActionName("Seating")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteSeating(Guid id)
        {
            var seating = Repository.Search<Seating>(s => s.UId == id).FirstOrDefault();
            if (seating == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(seating);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        #region Styles

        [HttpGet]
        public ActionResult Styles(Guid id)
        {
            using (var context = new EFDbContext())
            {
                var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
                if (form == null)
                    return RedirectToAction("Index");
                return View(form);
            }
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Styles()
        {
            Guid id = Guid.Empty;
            var formData = Request.Form;
            if (!Guid.TryParse(formData["UId"], out id))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var styles = formData.AllKeys.Where(k => k != "UId");
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            foreach (var style in styles)
            {
                Guid styleId;
                if (!Guid.TryParse(style, out styleId))
                    continue;
                var oldStyle = form.FormStyles.Where(f => f.UId == styleId).FirstOrDefault();
                oldStyle.Value = String.IsNullOrWhiteSpace(formData[style].Trim()) || formData[style] == "null" ? null : formData[style].Trim();
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        #endregion

        #region Audiences

        [HttpGet]
        public ActionResult Audiences(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Error403", "Error");
            return View(form);
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Audiences()
        {
            var id = Guid.Empty;
            var formData = Request.Form;
            if (!Guid.TryParse(formData["UId"], out id))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var ai = 0;
            while (formData.AllKeys.Contains("audience[" + ai + "].UId"))
            {
                var aId = Guid.Parse(formData["audience[" + ai + "].UId"]);
                var audience = form.Audiences.Where(a => a.UId == aId).FirstOrDefault();
                if (audience == null)
                {
                    ai++;
                    continue;
                }
                audience.Order = Int32.Parse(formData["audience[" + ai + "].Order"]);
                ai++;
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpPost]
        [ActionName("Audience")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewAudience(Guid id, string name = null)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var audience = Audience.New(Repository, company, user, form, name: name);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Audience", "FormBuilder", new { id = audience.UId }, Request.Url.Scheme) } };
        }
        
        [HttpGet]
        [ActionName("Audience")]
        public ActionResult GetAudience(Guid id)
        {
            var audience = Repository.Search<Audience>(a => a.UId == id).FirstOrDefault();
            if (audience == null)
                return RedirectToAction("Error404", "Error");
            return View("Audience", audience);
        }

        [HttpPut]
        [ActionName("Audience")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditAudience(Audience model)
        {
            var audience = Repository.Search<Audience>(a => a.UId == model.UId).FirstOrDefault();
            if (audience == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            audience.Name = model.Name;
            audience.Label = model.Label;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        [HttpDelete]
        [ActionName("Audience")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteAudience(Guid id)
        {
            var audience = Repository.Search<Audience>(a => a.UId == id).FirstOrDefault();
            if (audience == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(audience);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        #region Header

        [HttpGet]
        public ActionResult Header(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Index");
            return View(form);
        }

        [HttpPut]
        [ActionName("Header")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditHeader(Form model)
        {
            var form = Repository.Search<Form>(f => f.UId == model.UId).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            form.Header = model.Header;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        #endregion

        #region Under Maintenance

        [HttpGet]
        public ActionResult UnderMaintenance(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Error404", "Error");
            return View(form);
        }

        [HttpPut]
        [ActionName("UnderMaintenance")]
        [JsonValidateAntiForgeryToken]
        public ActionResult EditUnderMaintenance(Form model)
        {
            var form = Repository.Search<Form>(f => f.UId == model.UId).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            form.UnderMaintenance = model.UnderMaintenance;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        #endregion

        #region Form Closed

        [HttpGet]
        public ActionResult FormClosed(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Error404", "Error");
            return View(form);
        }

        [HttpPut]
        [ActionName("FormClosed")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditFormClosed(Form model)
        {
            var form = Repository.Search<Form>(f => f.UId == model.UId).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            form.FormCloseMessage = model.FormCloseMessage;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        #endregion

        #region Footer

        [HttpGet]
        public ActionResult Footer(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Error404", "Error");
            return View(form);
        }

        [HttpPut]
        [ActionName("Footer")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditFooter(Form model)
        {
            var form = Repository.Search<Form>(f => f.UId == model.UId).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            form.Footer = model.Footer;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        #endregion

        #region Content Blocks

        [HttpGet]
        public ActionResult ContentBlocks(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Index");
            return View(form);
        }

        [HttpPost]
        [ActionName("ContentBlock")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewContentBlock(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var contentBlock = CustomText.New(Repository, company, user, form);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("ContentBlock", "FormBuilder", new { id = contentBlock.UId }, Request.Url.Scheme) } };
        }

        [HttpDelete]
        [ActionName("ContentBlock")]
        [JsonValidateAntiForgeryToken]
        public ActionResult DeleteContentBlock(Guid id)
        {
            var customText = Repository.Search<CustomText>(ct => ct.UId == id).FirstOrDefault();
            if (customText == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var formKey = customText.FormKey;
            Repository.Remove(customText);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpGet]
        public ActionResult ContentBlock(Guid id)
        {
            var customText = Repository.Search<CustomText>(ct => ct.UId == id).FirstOrDefault();
            if (customText == null)
                return RedirectToAction("Error404", "Error");
            return View(customText);
        }

        [HttpPut]
        [ActionName("ContentBlock")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditContentBlock(CustomText model)
        {
            var customText = Repository.Search<CustomText>(ct => ct.UId == model.UId).FirstOrDefault();
            if (customText == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            customText.Variable = model.Variable;
            customText.Text = model.Text;
            customText.Name = model.Variable;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false, Location = Url.Action("ContentBlock", "FormBuilder", new { id = customText.UId }, Request.Url.Scheme) } };
        }

        #endregion

        #region Content Logic

        [HttpGet]
        public ActionResult ContentLogics(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Index");
            return View(form);
        }

        [HttpPost]
        [ActionName("ContentLogic")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewContentLogic(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var contentLogic = new LogicBlock()
            {
                Name = "New Content Logic " + DateTime.UtcNow.ToShortDateString(),
                Owner = user.UId,
                Group = company.UId,
                FormKey = form.UId
            };
            Repository.Add(contentLogic);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("ContentLogic", "FormBuilder", new { id = contentLogic.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        public ActionResult ContentLogic(Guid id)
        {
            var contentLogic = Repository.Search<LogicBlock>(c => c.UId == id).FirstOrDefault();
            if (contentLogic == null)
                return RedirectToAction("Index");
            return View(contentLogic);
        }

        [HttpPut]
        [ActionName("ContentLogic")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditContentLogic(ContentBlockModel model)
        {
            var contentLogic = Repository.Search<LogicBlock>(c => c.UId == model.UId).FirstOrDefault();
            if (contentLogic == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            contentLogic.Name = model.Name;
            foreach (var logicInfo in model.Logics)
            {
                var logic = contentLogic.Logics.FirstOrDefault(l => l.UId == logicInfo.UId);
                if (logic == null)
                    continue;
                logic.Order = logicInfo.Order;
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        [HttpDelete]
        [ActionName("ContentLogic")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteContentLogic(Guid id)
        {
            var contentLogic = Repository.Search<LogicBlock>(c => c.UId == id).FirstOrDefault();
            if (contentLogic == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(contentLogic);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        #region JLogic

        [HttpPost]
        [ActionName("JLogic")]
        [Authorize(Roles = "Super Administrators,Administrators,Form Builders,Programmers,Email Builders")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewJLogic(Guid id, bool onLoad)
        {
            ILogicHolder parent = Repository.Search<ILogicHolder>(l => l.UId == id).FirstOrDefault();
            if (parent == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object." } };
            var logic = new JLogic();
            logic.Order = parent.JLogics.Count + 1;
            logic.OnLoad = onLoad;
            parent.JLogics.Add(logic);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("JLogic", "FormBuilder", new { id = parent.UId, lid = logic.Id }, Request.Url.Scheme) } };
        }

        [HttpPut]
        [ActionName("JLogic")]
        [Authorize(Roles = "Super Administrators,Administrators,Form Builders,Programmers,Email Builders")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditJLogic(Guid id, JLogic model)
        {
            var logicH = Repository.Search<ILogicHolder>(l => l.UId == id).FirstOrDefault();
            if (logicH == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            var logic = logicH.JLogics.FirstOrDefault(l => l.Id == model.Id);
            if (logic == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            logic.Name = model.Name;
            logic.OnLoad = model.OnLoad;
            logic.Order = model.Order;
            logic.Commands = model.Commands;
            logic.Filters = model.Filters;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpGet]
        [Authorize(Roles = "Super Administrators,Administrators,Form Builders,Programmers,Email Builders")]
        [ActionName("JLogic")]
        public ActionResult JLogic(Guid id, string lid)
        {
            var logicH = Repository.Search<ILogicHolder>(l => l.UId == id).FirstOrDefault();
            if (logicH == null)
                return RedirectToAction("Error404", "Error");
            var logic = logicH.JLogics.FirstOrDefault(l => l.Id == lid);
            if (logic == null)
                return RedirectToAction("Error404", "Error");
            if (Request.IsAjaxRequest())
            {
                var headers = new List<JTableHeader>();
                Form form = null;
                if (logicH is IFormItem)
                    form = (logicH as IFormItem).GetForm();
                if (logicH is IEmail)
                    form = (logicH as IEmail).GetNode() as Form;
                if (form != null)
                {
                    headers.AddRange(GetFormHeaders(form));
                    headers.AddRange(GetContactHeaders());
                }
                else
                {
                    headers.AddRange(GetContactHeaders());
                }
                logic.Headers = headers;
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Logic = logic } };
            }
            else
            {
                Crumb.Label = "Logic for " + logicH.Name;
                UserManager.UpdateAsync(user);
                return View(new Tuple<Guid, string>(id, lid));
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Super Administrators,Administrators,Form Builders,Programmers,Email Builders")]
        [ActionName("JLogic")]
        public JsonNetResult DeleteJLogic(Guid id, string lid)
        {
            var logicH = Repository.Search<ILogicHolder>(l => l.UId == id).FirstOrDefault();
            if (logicH == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            var logic = logicH.JLogics.FirstOrDefault(l => l.Id == lid);
            if (logic == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            logicH.JLogics.Remove(logic);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("JLogics", "FormBuilder", new { id = id }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [Authorize(Roles = "Super Administrators,Administrators,Form Builders,Programmers,Email Builders")]
        [ActionName("JLogics")]
        public ActionResult JLogics(Guid id, bool onLoad = true)
        {
            var logicHolder = Repository.Search<ILogicHolder>(lh => lh.UId == id).FirstOrDefault();
            if (logicHolder == null)
                return RedirectToAction("Error404", "Error");
            Crumb.Label = "Logics for " + logicHolder.Name;
            UserManager.UpdateAsync(user);
            return View(new Tuple<Guid, List<JLogic>, bool>(id, logicHolder.JLogics.Where(l => l.OnLoad == onLoad).ToList(), onLoad));
        }

        /// <summary>
        /// Gets a list of contact headers wrapped in a JTableHeader format.
        /// </summary>
        /// <returns>An enumerable collection of JTableHeaders representing Contact Headers.</returns>
        protected IEnumerable<JTableHeader> GetContactHeaders()
        {
            var headers = new List<JTableHeader>();
            foreach (var head in Repository.Search<ContactHeader>(h => h.CompanyKey == company.UId).OrderBy(h => h.Name))
                headers.Add(new JTableHeader() { Editable = true, Id = head.UId.ToString(), Label = head.Name, Type = "contact", Group = "contact", PossibleValues = head.Values.Select(v => new JTableHeaderPossibleValue() { Id = v.Id.ToString(), Label = v.Value }).ToList() });
            return headers;
        }

        /// <summary>
        /// Gets a list of Registrant data that can be modified.
        /// </summary>
        /// <param name="form">The form to use for the headers.</param>
        /// <returns>The form data headers an enumerable collection of JTableHeaders.</returns>
        protected IEnumerable<JTableHeader> GetFormHeaders(Form form)
        {
            var headers = new List<JTableHeader>();
            headers.Add(new JTableHeader() { Id = "email", Label = "Email", Group = "form" });
            headers.Add(new JTableHeader() { Id = "confirmation", Group = "form", Label = "Confirmation" });
            headers.Add(new JTableHeader() { Editable = true, Id = "rsvp", Group = "form", Label = "RSVP", Type = "boolean", PossibleValues = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "1", Label = form.RSVPAccept }, new JTableHeaderPossibleValue() { Label = form.RSVPDecline, Id = "0" } } });
            var audPosVals = new List<JTableHeaderPossibleValue>();
            foreach (var audience in form.Audiences)
            {
                audPosVals.Add(new JTableHeaderPossibleValue() { Id = audience.UId.ToString(), Label = audience.Label });
            }
            var audienceHeader = new JTableHeader() { Id = "audience", Group = "form", Label = "Audience", Type = "itemParent", PossibleValues = audPosVals, Editable = true };
            if (audPosVals.Count > 00)
                headers.Add(audienceHeader);
            headers.Add(new JTableHeader() { Id = "dateregistered", Group = "form", Label = "Date Created", Type = "datetime" });
            headers.Add(new JTableHeader() { Id = "datemodified", Group = "form", Label = "Last Edit Date", Type = "datetime" });
            foreach (var component in form.GetComponents().Where(c => !(c is FreeText) && !(c is IComponentItem)))
            {
                var label = component.Variable.Value;
                Guid t_id;
                if (Guid.TryParse(label, out t_id))
                    label = component.LabelText;
                if (component is IComponentItemParent)
                {
                    var t_values = new List<JTableHeaderPossibleValue>();
                    var t_type = "itemParent";
                    if (component is IComponentMultipleSelection)
                        t_type = "multipleSelection";
                    else
                        t_values.Add(new JTableHeaderPossibleValue() { Id = "", Label = "No Selection" });
                    t_values.AddRange((component as IComponentItemParent).Children.OrderBy(i => i.Order).Select(c => new JTableHeaderPossibleValue() { Id = c.UId.ToString(), Label = c.LabelText }));
                    headers.Add(new JTableHeader() { Editable = true, Id = component.UId.ToString(), Label = label, Type = t_type, PossibleValues = t_values, Group = "form" });
                    foreach (var item in (component as IComponentItemParent).Children.OrderBy(i => i.Order))
                    {
                        if (item.Seating != null)
                            headers.Add(new JTableHeader() { Group = "form", Editable = true, Id = "seating_" + item.UId.ToString(), Type = "itemParent", Label = "&nbsp;&nbsp;&nbsp;&nbsp;" + item.LabelText + " Capacity (" + item.Seating.Name.GetElipse(20) + ")", PossibleValues = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "seated", Label = "Seated" }, new JTableHeaderPossibleValue() { Id = "waited", Label = "Waited" } } });
                    }
                }
                else if (component is Input)
                {
                    var t_inp = component as Input;
                    var t_type = "text";
                    switch (t_inp.Type)
                    {
                        case Domain.Entities.Components.InputType.Date:
                            t_type = "date";
                            break;
                        case Domain.Entities.Components.InputType.DateTime:
                            t_type = "datetime";
                            break;
                        case Domain.Entities.Components.InputType.Time:
                            t_type = "time";
                            break;
                        case Domain.Entities.Components.InputType.File:
                            headers.Add(new JTableHeader() { Id = component.UId.ToString(), Label = label, Group = "form", PossibleValues = new List<JTableHeaderPossibleValue>() { new JTableHeaderPossibleValue() { Id = "0", Label = "File Uploaded" }, new JTableHeaderPossibleValue() { Id = "1", Label = "No File Uploaded" } } });
                            continue;
                        case Domain.Entities.Components.InputType.Default:
                            switch (t_inp.ValueType)
                            {
                                case Domain.Entities.Components.ValueType.Decimal:
                                    t_type = "money";
                                    break;
                                case Domain.Entities.Components.ValueType.Number:
                                    t_type = "number";
                                    break;
                                default:
                                    t_type = "text";
                                    break;
                            }
                            break;
                        default:
                            t_type = "text";
                            break;
                    }
                    headers.Add(new JTableHeader() { Editable = true, Id = component.UId.ToString(), Label = label, Type = t_type, Group = "form" });
                }
                else
                {
                    headers.Add(new JTableHeader() { Editable = true, Id = component.UId.ToString(), Label = label, Group = "form" });
                }
            }
            return headers;
        }


        #endregion

        #region Logic

        [HttpPost]
        [ActionName("Logic")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewLogic(Guid id, bool onLoad = true)
        {
            ILogicHolder parent = Repository.Search<ILogicHolder>(l => l.UId == id).FirstOrDefault();
            if (parent == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object." } };
            if (parent.Logics.Count > 0)
            {
                var logic = Logic.New(Repository, parent, company, user, incoming: onLoad);
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Logic", "FormBuilder", new { id = logic.UId }, Request.Url.Scheme) } };
            }
            else
            {
                var logic = new JLogic();
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Logic", "FormBuilder", new { id = logic.Id }, Request.Url.Scheme) } };
            }
        }

        [HttpGet]
        [ActionName("Logic")]
        public ActionResult EditLogic(Guid id, string lid = null)
        {
            var logic = Repository.Search<Logic>(c => c.UId == id).FirstOrDefault();
            if (logic != null)
            {
                Form form;
                var forms = new List<LogicFormModel>();

                if (logic.LogicBlockKey.HasValue)
                {
                    ViewBag.Form = form = logic.LogicBlock.Form;
                }
                else if (logic.ComponentKey.HasValue)
                {
                    if (logic.Component is CheckboxItem || logic.Component is RadioItem || logic.Component is DropdownItem)
                    {
                        var item = (IComponentItem)logic.Component;
                        ViewBag.Parent = item.ParentKey;
                        ViewBag.Page = item.Parent.Panel.Page;
                        ViewBag.Panel = item.Parent.Panel;
                        ViewBag.Form = form = item.Parent.Panel.Page.Form;
                    }
                    else
                    {
                        ViewBag.Page = logic.Component.Panel.Page;
                        ViewBag.Panel = logic.Component.Panel;
                        ViewBag.Form = form = logic.Component.Panel.Page.Form;
                    }
                }
                else if (logic.PageKey.HasValue)
                {
                    ViewBag.Page = logic.Page;
                    ViewBag.Form = form = logic.Page.Form;
                }
                else
                {
                    ViewBag.Page = logic.Panel.Page;
                    ViewBag.Panel = logic.Panel;
                    ViewBag.Form = form = logic.Panel.Page.Form;
                }
                forms = Context.Forms.Where(f => f.CompanyKey == form.CompanyKey).Select(f => new LogicFormModel() { UId = f.UId, Name = f.Name, CompanyKey = f.CompanyKey }).ToList();
                ViewBag.Forms = forms;
                foreach (var group in logic.LogicGroups)
                {
                    foreach (var statement in group.LogicStatements)
                    {
                        Context.Entry(statement).Reference(l => l.Form).Load();
                        switch (statement.Variable.ToLower())
                        {
                            case "audience":
                                var audience = statement.Form.Audiences.FirstOrDefault(a => a.UId == Guid.Parse(statement.Value));
                                if (audience != null)
                                    statement.ValueName = audience.Name;
                                break;
                            case "status":
                                statement.ValueName = ((RegistrationStatus)(int.Parse(statement.Value))).GetStringValue();
                                break;
                            case "type":
                                statement.ValueName = ((RegistrationType)(int.Parse(statement.Value))).GetStringValue();
                                break;
                            case "rsvp":
                                statement.ValueName = (statement.Value == "True" ? "Accept" : "Decline");
                                break;
                            default:
                                statement.ValueName = statement.Form.GetValueName(statement.Variable, statement.Value);
                                break;
                        }
                        statement.VariableName = statement.Form.GetVariableName(statement.Variable);
                    }
                }
                return View("Logic", logic);
            }
            else
            {
                Guid lId;
                if (!Guid.TryParse(lid, out lId))
                    return RedirectToAction("Error404", "Error");
                var logicHolder = Repository.Search<ILogicHolder>(l => l.UId == id).FirstOrDefault();
                if (logicHolder == null)
                    return RedirectToAction("Error404", "Error");
                var jlogic = logicHolder.JLogics.FirstOrDefault(l => l.Id == lid);
                if (jlogic == null)
                    return RedirectToAction("Error404", "Error");
                return View("JLogic", jlogic);
            }
        }

        [HttpPut]
        [ActionName("Logic")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditLogic(Logic model)
        {
            var logic = Repository.Search<Logic>(c => c.UId == model.UId).FirstOrDefault();
            if (logic == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            logic.Incoming = model.Incoming;
            // Now we iterate through the logic groups send over the header
            foreach (var h_group in model.LogicGroups)
            {
                var group = logic.LogicGroups.Where(l => l.UId == h_group.UId).FirstOrDefault();
                if (group == null)
                {
                    h_group.UId = Guid.NewGuid();
                    logic.LogicGroups.Add(h_group);
                }
                else
                {
                    group.Link = h_group.Link;
                    group.Order = h_group.Order;
                    foreach (var h_statement in h_group.LogicStatements)
                    {
                        var statement = group.LogicStatements.Where(l => l.UId == h_statement.UId).FirstOrDefault();
                        if (statement == null)
                        {
                            h_statement.UId = Guid.NewGuid();
                            group.LogicStatements.Add(h_statement);
                        }
                        else
                        {
                            statement.Order = h_statement.Order;
                            statement.Link = h_statement.Link;
                            statement.FormKey = h_statement.FormKey;
                            statement.Variable = h_statement.Variable;
                            statement.Value = h_statement.Value;
                            statement.VariableName = h_statement.VariableName;
                            statement.ValueName = h_statement.ValueName;
                            statement.Test = h_statement.Test;
                        }
                    }
                }
            }
            for (var i = 0; i < logic.LogicGroups.Count; i++)
            {
                var group = logic.LogicGroups[i];
                var h_group = model.LogicGroups.Where(g => g.UId == group.UId).FirstOrDefault();
                if (h_group == null)
                {
                    Repository.Remove(group);
                    i--;
                }
                else
                {
                    for (var j = 0; j < group.LogicStatements.Count; j++)
                    {
                        var statement = group.LogicStatements[j];
                        var h_statement = h_group.LogicStatements.Where(s => s.UId == statement.UId).FirstOrDefault();
                        if (h_statement == null)
                        {
                            Repository.Remove(statement);
                            i--;
                        }
                    }
                }
            }
            foreach (var h_command in model.Commands)
            {
                var command = logic.Commands.FirstOrDefault(l => l.UId == h_command.UId);
                if (command == null)
                {
                    h_command.UId = Guid.NewGuid();
                    logic.Commands.Add(h_command);
                }
                else
                {
                    command.Order = h_command.Order;
                    command.Parameters = h_command.Parameters;
                    command.FormKey = h_command.FormKey;
                    command.CommandType = h_command.CommandType;
                    command.Command = h_command.Command;
                }
            }
            for (var i = 0; i < logic.Commands.Count; i++)
            {
                var command = logic.Commands[i];
                var h_command = model.Commands.FirstOrDefault(c => c.UId == command.UId);
                if (h_command == null)
                {
                    Repository.Remove(command);
                    i--;
                }
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        [HttpDelete]
        [ActionName("Logic")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteLogic(Guid id)
        {
            var logic = Repository.Search<Logic>(l => l.UId == id).FirstOrDefault();
            if (logic == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            bool onLoad = logic.Incoming;
            ILogicHolder node = null;
            if (logic.LogicBlockKey.HasValue)
                node = logic.LogicBlock;
            else if (logic.PageKey.HasValue)
                node = logic.Page;
            else if (logic.PanelKey.HasValue)
                node = logic.Panel;
            else if (logic.ComponentKey.HasValue)
                node = logic.Component;
            if (node == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            node.Logics.Remove(logic);
            Repository.Remove(logic);
            int order = 1;
            foreach (var l in node.Logics)
                l.Order = order++;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        #region Page

        [HttpPost]
        [ActionName("Page")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewPage(Guid id, string name = null, string description = null)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var page = Page.New(Repository, form, company, user, name, description);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Page", "FormBuilder", new { id = page.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [ActionName("Page")]
        public ActionResult EditPage(Guid id)
        {
            var page = Repository.Search<Page>(p => p.UId == id).FirstOrDefault();
            if (page == null)
                return RedirectToAction("Error404", "Error");
            page.Panels.Sort();
            ViewBag.AudienceList = page.Form.Audiences.ToList();
            switch (page.Type)
            {
                case PageType.UserDefined:
                    return View("Page", page);
                case PageType.RSVP:
                case PageType.Audience:
                case PageType.Confirmation:
                case PageType.BillingComfirnmation:
                case PageType.Billing:
                    ViewBag.Html = page.Panels[0].Components.OfType<FreeText>().FirstOrDefault().Html;
                    return View("TextPage", page);
                default:
                    return RedirectToAction("EditForm", new { id = page.FormKey });
            }
        }

        [HttpPut]
        [ActionName("Page")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditPage(Page model)
        {
            var formData = Request.Form;
            var page = Repository.Search<Page>(p => p.UId == model.UId).FirstOrDefault();
            if (page == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var audiences = page.Form.Audiences.ToList();
            if (page.Type != PageType.UserDefined)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Insufficient Permissions" } };
            page.AdminOnly = model.AdminOnly;
            var audienceUIds = JsonConvert.DeserializeObject<List<Guid>>(formData["audienceUIds"]);
            foreach (var uid in audienceUIds)
            {
                if (page.Audiences.Where(a => a.UId == uid).Count() == 0)
                    page.Audiences.Add(audiences.Where(a => a.UId == uid).FirstOrDefault());
            }
            for(var i = 0; i < page.Audiences.Count; i++)
            {
                var audience = page.Audiences[i];
                if (!audienceUIds.Contains(audience.UId))
                {
                    page.Audiences.RemoveAt(i);
                    i--;
                }
            }
            page.Name = model.Name;
            page.Description = model.Description;
            page.Enabled = model.Enabled;
            page.RSVP = model.RSVP;
            var panelCount = 0;
            while (formData.AllKeys.Contains("Panel[" + panelCount + "].UId"))
            {
                var panel = page.Panels.FirstOrDefault(p => p.UId == Guid.Parse(formData["Panel[" + panelCount + "].UId"]));
                if (panel == null)
                    continue;
                panel.Order = Int32.Parse(formData["Panel[" + panelCount + "].Order"]);
                panel.Enabled = formData["Panel[" + panelCount + "].Enabled"].Contains("true");
                panelCount++;
            }
            page.Panels.Sort();
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = new Dictionary<string, string>() } };
        }

        [HttpPut]
        [ActionName("TextPage")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditTextPage(PageText model)
        {
            var page = Repository.Search<Page>(p => p.UId == model.UId).FirstOrDefault();
            if (page == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            FreeText freeText = null;
            foreach (var panel in page.Panels)
            {
                foreach (var component in panel.Components)
                {
                    if (component is FreeText)
                        freeText = (FreeText)component;
                }
            }
            if (page.Type == PageType.RSVP)
            {
                page.Form.RSVPAccept = model.RSVPAccept;
                page.Form.RSVPDecline = model.RSVPDecline;
            }
            freeText.Html = model.Html;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        [HttpDelete]
        [ActionName("Page")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeletePage(Guid id)
        {
            var page = Repository.Search<Page>(p => p.UId == id).FirstOrDefault();
            if (page == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var form = page.Form;
            Repository.Remove(page);
            var pages = form.Pages.Where(p => p.Type == PageType.UserDefined).ToList();
            var pageIndex = 3;
            foreach (var pg in pages)
            {
                pg.PageNumber = pageIndex++;
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpGet]
        public ActionResult PageLogics(Guid id, bool pageLoad = true)
        {
            ViewBag.OnLoad = pageLoad;
            var page = Repository.Search<Page>(p => p.UId == id).FirstOrDefault();
            if (page == null)
                return RedirectToAction("Error404", "Error");
            ViewBag.Title = pageLoad ? "Page Load Logics" : "Page Advance Logics";
            ViewBag.NewText = pageLoad ? "New Page Load Logic" : "New Page Advance Logic";
            return View(page);
        }

        #endregion

        #region Panels

        [HttpPost]
        [ActionName("Panel")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewPanel(Guid id, string name = null, string description = "")
        {
            var page = Repository.Search<Page>(p => p.UId == id).FirstOrDefault();
            if (page == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var panel = Panel.New(Repository, page, company, user, name, description);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Panel", "FormBuilder", new { id = panel.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        public ActionResult PanelLogics(Guid id)
        {
            var panel = Repository.Search<Panel>(p => p.UId == id).FirstOrDefault();
            if (panel == null)
                return RedirectToAction("Error404", "Error");
            return View(panel);
        }

        [HttpGet]
        [ActionName("Panel")]
        public ActionResult EditPanel(Guid id)
        {
            var panel = Repository.Search<Panel>(p => p.UId == id).FirstOrDefault();
            if (panel == null)
                return RedirectToAction("Error404", "Error");
            var audiences = panel.Page.Form.Audiences;
            ViewBag.AudienceList = audiences;
            return View("Panel", panel);
        }

        [HttpPut]
        [ActionName("Panel")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditPanel(Panel model)
        {
            var formData = Request.Form;
            var panel = Repository.Search<Panel>(p => p.UId == model.UId).FirstOrDefault();
            if (panel == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            panel.Name = model.Name;
            panel.Description = model.Description;
            panel.Enabled = model.Enabled;
            panel.RSVP = model.RSVP;
            panel.AdminOnly = model.AdminOnly;
            var audienceUIds = JsonConvert.DeserializeObject<List<Guid>>(formData["audienceUIds"]);
            foreach (var uid in audienceUIds)
            {
                if (panel.Audiences.Where(a => a.UId == uid).Count() == 0)
                    panel.Audiences.Add(Repository.Search<Audience>(a => a.UId == uid).FirstOrDefault());
            }
            for (var i = 0; i < panel.Audiences.Count; i++)
            {
                var audience = panel.Audiences[i];
                if (!audienceUIds.Contains(audience.UId))
                {
                    panel.Audiences.RemoveAt(i);
                    i--;
                }
            }
            var componentOrder = 0;
            while (formData.AllKeys.Contains("Components[" + componentOrder + "].UId"))
            {
                var component = panel.Components.FirstOrDefault(p => p.UId == Guid.Parse(formData["Components[" + componentOrder + "].UId"]));
                if (component == null)
                    continue;
                component.Order = Int32.Parse(formData["Components[" + componentOrder + "].Order"]);
                component.Row = Int32.Parse(formData["Components[" + componentOrder + "].Row"]);
                componentOrder++;
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        [HttpDelete]
        [ActionName("Panel")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeletePanel(Guid id)
        {
            var panel = Repository.Search<Panel>(p => p.UId == id).FirstOrDefault();
            if (panel == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(panel);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        #region Components

        [HttpGet]
        public ActionResult ComponentLogics(Guid id)
        {
            var component = Repository.Search<Component>(c => c.UId == id).FirstOrDefault();
            if (component == null)
                return RedirectToAction("Error404", "Error");
            Form form = null;
            if (component is IComponentItem)
                form = ((IComponentItem)component).Parent.Panel.Page.Form;
            else
                form = component.Panel.Page.Form;
            return View(component);
        }

        //Restful Operations

        [HttpPost]
        [ActionName("Component")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewComponent(Guid id, string type, string subtype = "")
        {
            var m_type = type.ToLower().Trim();
            Panel panel = null;
            Component parent = null;

            if (type.EndsWith("item"))
            {
                switch (type)
                {
                    case "radioitem":
                        parent = Repository.Search<Component>(c => c.UId == id).OfType<RadioGroup>().FirstOrDefault();
                        break;
                    case "dropdownitem":
                        parent = Repository.Search<Component>(c => c.UId == id).OfType<DropdownGroup>().FirstOrDefault();
                        break;
                    case "checkboxitem":
                        parent = Repository.Search<Component>(c => c.UId == id).OfType<CheckboxGroup>().FirstOrDefault();
                        break;
                    default:
                        return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Item Type" } };
                }
                if (parent == null)
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Parent Object" } };
                panel = parent.Panel;
            }
            else
            {
                panel = Repository.Search<Panel>(p => p.UId == id).FirstOrDefault();
                if (panel == null)
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            }

            var rows = panel.Components.Select(c => c.Row).Distinct().OrderBy(r => r).ToList();
            var row = rows.Count > 0 ? rows.LastOrDefault() : 1;
            var orders = panel.Components.Where(c => c.Row == row).Select(c => c.Order).Distinct().OrderBy(o => o).ToList();
            var order = orders.Count > 0 ? orders.LastOrDefault() + 1 : 1;


            Component component = null;

            if (m_type == "input")
            {
                #region Input
                component = Input.New(Repository, panel, company, user, row: row, order: order);
                var input = (Input)component;
                switch (subtype)
                {
                    case "date":
                        input.Type = Domain.Entities.Components.InputType.Date;
                        break;
                    case "datetime":
                        input.Type = Domain.Entities.Components.InputType.DateTime;
                        break;
                    case "time":
                        input.Type = Domain.Entities.Components.InputType.Time;
                        break;
                    case "file":
                        input.Type = Domain.Entities.Components.InputType.File;
                        break;
                    case "text":
                    default:
                        input.Type = Domain.Entities.Components.InputType.Default;
                        break;
                }

                #endregion
            }
            else if (m_type == "dropdown")
            {
                #region Dropdown Group
                component = DropdownGroup.New(Repository, panel, company, user, row: row, order: order);
                var input = (DropdownGroup)component;
                switch (subtype)
                {
                    case "country":
                        for (var i = 0; i < Countries.Names.Length; i++)
                        {
                            var item = DropdownItem.New(Repository, (DropdownGroup)component, company, user, name: Countries.Names[i]);
                        }
                        break;
                    case "state":
                        for (var i = 0; i < States.Names.Length; i++)
                        {
                            var item = DropdownItem.New(Repository, (DropdownGroup)component, company, user, name: States.Names[i]);
                        }
                        break;
                    case "stateabbr":
                        for (var i = 0; i < States.Abbreviations.Length; i++)
                        {
                            var item = DropdownItem.New(Repository, (DropdownGroup)component, company, user, name: States.Abbreviations[i]);
                        }
                        break;
                    case "yesno":
                        var yesItem = DropdownItem.New(Repository, (DropdownGroup)component, company, user, name: "Yes");
                        var noItem = DropdownItem.New(Repository, (DropdownGroup)component, company, user, name: "No");
                        break;
                    case "default":
                    default:
                        break;
                }

                #endregion
            }
            else if (m_type == "dropdownitem")
            {
                #region Dropdown Item
                component = DropdownItem.New(Repository, (DropdownGroup)parent, company, user);
                #endregion
            }
            else if (m_type == "radiogroup")
            {
                #region Radio Group

                component = RadioGroup.New(Repository, panel, company, user, row: row, order: order);
                switch (type)
                {
                    case "default":
                    default:
                        break;
                }

                #endregion
            }
            else if (m_type == "radioitem")
            {
                #region Radio Item

                component = RadioItem.New(Repository, (RadioGroup)parent, company, user);

                #endregion
            }
            else if (m_type == "checkboxgroup")
            {
                #region Checkbox Group

                component = CheckboxGroup.New(Repository, panel, user, company, row: row, order: order);
                switch (type)
                {
                    case "default":
                    default:
                        break;
                }

                #endregion
            }
            else if (m_type == "checkboxitem")
            {
                #region Checkbox Item

                component = CheckboxItem.New(Repository, (CheckboxGroup)parent, company, user);

                #endregion
            }
            else if (m_type == "freetext")
            {
                #region Free Text

                component = FreeText.New(Repository, panel, company, user, row: row, order: order);
                switch (type)
                {
                    case "default":
                    default:
                        break;
                }

                #endregion
            }
            else if (m_type == "rating")
            {
                #region Rating
                component = RatingSelect.New(Repository, panel, company, user, row: row, order: order);
                #endregion
            }
            else if (m_type == "numberselector")
            {
                #region NumberSelector
                component = NumberSelector.New(Repository, panel, company, user, 0, 10, row: row, order: order);
                #endregion
            }
            if (component == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Component Type" } };
            component.GetForm().ModificationToken = Guid.NewGuid();
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Component", "FormBuilder", new { id = component.UId }, Request.Url.Scheme) } };

        }

        [HttpGet]
        public ActionResult Component(Guid id)
        {
            var component = Repository.Search<Component>(c => c.UId == id).FirstOrDefault();
            if (component == null)
                return RedirectToAction("Error404", "Error");
            Form form = null;
            if (component is IComponentItem)
                form = ((IComponentItem)component).Parent.Panel.Page.Form;
            else
                form = component.Panel.Page.Form;
            if (form.Survey && form.ParentForm != null)
            {
                var components = form.ParentForm.GetComponents().ToList();
                var mappableComponents = new List<IComponent>();
                components.ForEach(c => { if (c is IComponentSurveyMappable) { mappableComponents.Add(c); } });
                ViewBag.MappableComponents = mappableComponents;
            }
            List<ContactHeader> headers = null;
            var audiences = form.Audiences.ToList();
            var seatings = form.Seatings.ToList();
            if (form.InvitationList == null)
                headers = Repository.Search<ContactHeader>(h => h.CompanyKey == form.CompanyKey && h.SavedListKey == null).ToList();
            else
                headers = Repository.Search<ContactHeader>(h => h.CompanyKey == form.CompanyKey && (h.SavedListKey == null || h.SavedListKey == form.InvitationListKey)).ToList();
            headers.Sort((a, b) => a.Name.CompareTo(b.Name));
            ViewBag.Headers = headers;
            component.DisplayComponentOrder.Items.Sort();
            ViewBag.AudienceList = audiences;
            ViewBag.Seatings = seatings;
            if (component is Input)
                return View("Input", component);
            else if (component is FreeText)
                return View("FreeText", component);
            else if (component is CheckboxGroup)
            {
                ((CheckboxGroup)component).Items.Sort();
                return View("CheckboxGroup", component);
            }
            else if (component is RadioGroup)
            {
                ((RadioGroup)component).Items.Sort();
                return View("RadioGroup", component);
            }
            else if (component is DropdownGroup)
            {
                ((DropdownGroup)component).Items.Sort();
                return View("DropdownGroup", component);
            }
            else if (component is CheckboxItem)
            {
                return View("CheckboxItem", component);
            }
            else if (component is RadioItem)
            {
                return View("RadioItem", component);
            }
            else if (component is DropdownItem)
            {
                return View("DropdownItem", component);
            }
            else if (component is RatingSelect)
            {
                return View("RatingSelect", component);
            }
            else if (component is NumberSelector)
            {
                return View("NumberSelector", component);
            }
            return RedirectToAction("Index");
        }

        [HttpPut]
        [ActionName("Component")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditComponent(Component model)
        {
            var formData = Request.Form;
            var component = Repository.Search<Component>(c => c.UId == model.UId).FirstOrDefault();
            if (component == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Form form = null;
            if (component is IComponentItem)
                form = ((IComponentItem)component).Parent.Panel.Page.Form;
            else
                form = component.Panel.Page.Form;

            // This holds the errors from the form
            var errors = new Dictionary<string, string>();

            // These fields are common to all components.  We will update them.
            #region Common component fields
            component.DisplayComponentOrder = model.DisplayComponentOrder;
            component.Description = model.Description;
            component.Enabled = model.Enabled;
            component.LabelText = model.LabelText;
            component.AltText = model.AltText;
            component.Required = model.Required;
            component.RSVP = model.RSVP;
            component.SeatingKey = model.SeatingKey != Guid.Empty ? model.SeatingKey : null;
            component.AgendaStart = model.AgendaStart;
            component.AgendaEnd = model.AgendaEnd;
            component.AgendaItem = model.AgendaItem;
            component.AdminOnly = model.AdminOnly;
            component.MappedToKey = model.MappedToKey;
            component.Name = model.LabelText;
            component.Display = model.Display;

            // Now we look through audiences

            var audienceUIds = JsonConvert.DeserializeObject<List<Guid>>(formData["audienceUIds"]);
            foreach (var uid in audienceUIds)
            {
                if (component.Audiences.Where(a => a.UId == uid).Count() == 0)
                    component.Audiences.Add(Repository.Search<Audience>(a => a.UId == uid).FirstOrDefault());
            }
            for (var i = 0; i < component.Audiences.Count; i++)
            {
                var audience = component.Audiences[i];
                if (!audienceUIds.Contains(audience.UId))
                {
                    component.Audiences.RemoveAt(i);
                    i--;
                }
            }

            // Lets check to see if the variable obeys the regulations set forth by the Sith Lord, Darth Vader.

            // Invalid values
            var protectedVariables = new List<string>()
            {
                "audience",
                "rsvp",
                "email"
            };
            Guid c_key;
            if (!(component is FreeText || component is CheckboxItem || component is DropdownItem || component is RadioItem))
            {
                // See if the variable is still a Guid
                if (Guid.TryParse(model.Variable.Value ?? "", out c_key))
                {
                    // The variable is still the Guid.
                    component.Name = component.LabelText;
                }
                else
                {
                    // The variable has been changed from the default. We need to run checks on it.
                    // Regex syntax tests
                    // Must be at least one character and start with a letter
                    var noSpace = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

                    var variable = model.Variable.Value;
                    if (String.IsNullOrEmpty(variable))
                    {
                        // The variable cannot be blank.
                        errors["Variable.Value"] = "The variable cannot be blank.";
                    }
                    else
                    {
                        // The variable is not blank. Now we run some more tests.
                        if (protectedVariables.Contains(variable.ToLower()))
                            errors["Variable.Value"] = "Variable or Name cannot be: audience, rsvp, or email.";
                        else if (!noSpace.IsMatch(variable))
                            errors["Variable.Value"] = "Variable or Name must contain only letters numbers and underscores and must start with a letter or underscore.";
                        else if (form.Variables.Where(v => v.Value == variable && v.UId != component.UId).Count() > 0)
                            errors["Variable.Value"] = "The variable is already in use by another component.";
                        else
                        {
                            component.Name = variable;
                            component.Variable.Value = variable;
                        }
                    }
                }
            }
            
            #endregion


            if (component is Input)
            {
                #region Input

                var model_input = new Input();
                UpdateModel(model_input);

                var input = component as Input;
                input.Height = model_input.Height;
                input.RegexPattern = model_input.RegexPattern;
                input.Type = model_input.Type;
                input.MinDate = model_input.MinDate;
                input.MaxDate = model_input.MaxDate;
                input.FileType = model_input.FileType;
                input.ValueType = model_input.ValueType;
                input.Length = model_input.Length;
                input.Formatting = model_input.Formatting;
                if (input.Type != Domain.Entities.Components.InputType.Default)
                    input.ValueType = Domain.Entities.Components.ValueType.Default;
                if (input.Type == Domain.Entities.Components.InputType.Date || input.Type == Domain.Entities.Components.InputType.DateTime || input.Type == Domain.Entities.Components.InputType.Time)
                    input.ValueType = RSToolKit.Domain.Entities.Components.ValueType.DateTime;

                #endregion
            }
            else if (component is FreeText)
            {
                #region Free Text

                var model_freeText = new FreeText();
                UpdateModel(model_freeText);
                var freeText = (FreeText)component;
                freeText.Html = model_freeText.Html;
                freeText.Name = model_freeText.Name;
                freeText.HideReview = model_freeText.HideReview;

                #endregion
            }
            else if (component is CheckboxGroup)
            {
                #region Checkbox Group

                var model_checkboxGroup = new CheckboxGroup();
                UpdateModel(model_checkboxGroup);

                var checkboxGroup = (CheckboxGroup)component;
                checkboxGroup.AgendaDisplay = model_checkboxGroup.AgendaDisplay;
                checkboxGroup.TimeExclusion = model_checkboxGroup.TimeExclusion;
                checkboxGroup.DialogText = model_checkboxGroup.DialogText;

                //Manage the items

                foreach (var item in checkboxGroup.Items)
                {
                    var f_item = model_checkboxGroup.Items.FirstOrDefault(i => i.UId == item.UId);
                    if (f_item == null)
                        continue;
                    item.Order = f_item.Order;
                }


                #endregion
            }
            else if (component is RadioGroup)
            {
                #region Radio Group

                var model_radioGroup = new RadioGroup();
                UpdateModel(model_radioGroup);

                var radioGroup = (RadioGroup)component;


                //Manage the items

                foreach (var item in radioGroup.Items)
                {
                    var f_item = model_radioGroup.Items.FirstOrDefault(i => i.UId == item.UId);
                    if (f_item == null)
                        continue;
                    item.Order = f_item.Order;
                }

                #endregion
            }
            else if (component is DropdownGroup)
            {
                #region Dropdown Group

                var model_dropDown = new DropdownGroup();
                UpdateModel(model_dropDown);

                var dropDown = (DropdownGroup)component;


                //Manage the items

                foreach (var item in dropDown.Items)
                {
                    var f_item = model_dropDown.Items.FirstOrDefault(i => i.UId == item.UId);
                    if (f_item == null)
                        continue;
                    item.Order = f_item.Order;
                }

                #endregion
            }
            else if (component is RatingSelect)
            {
                #region Rating Select

                var model_rs = new RatingSelect();
                UpdateModel(model_rs);

                var rs = component as RatingSelect;
                rs.MinRating = model_rs.MinRating;
                rs.MaxRating = model_rs.MaxRating;
                rs.RatingSelectType = model_rs.RatingSelectType;
                rs.Step = model_rs.Step;
                rs.Color = model_rs.Color;

                #endregion
            }
            else if (component is NumberSelector)
            {
                #region Number Selector

                var model_numbSelect = new NumberSelector();
                UpdateModel(model_numbSelect);

                var numbSelect = (NumberSelector)component;
                numbSelect.Min = model_numbSelect.Min;
                numbSelect.Max = model_numbSelect.Max;
                #endregion
            }
            if (errors.Count > 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = true, ErrorList = errors } };
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        [HttpDelete]
        [ActionName("Component")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteComponent(Guid id)
        {
            var component = Repository.Search<Component>(c => c.UId == id).FirstOrDefault();
            if (component == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Form form = null;
            if (component is IComponentItem)
                form = ((IComponentItem)component).Parent.Panel.Page.Form;
            else
                form = component.Panel.Page.Form;
            Repository.Remove(component);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        [ActionName("MoveComponent")]
        public JsonNetResult MoveComponent(Guid id, Guid panelKey)
        {
            var comp = Repository.Search<Component>(c => c.UId == id).FirstOrDefault();
            var panel = Repository.Search<Panel>(p => p.UId == panelKey).FirstOrDefault();
            if (comp == null || panel == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            comp.Panel = panel;
            comp.PanelKey = panel.UId;
            comp.Order = panel.Components.Count + 1;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = "refresh" } };
        }

        #endregion

        #region Prices

        [HttpGet]
        [ActionName("Prices")]
        public ActionResult EditPrices(Guid id, string name = null)
        {
            var component = Repository.Search<Component>(c => c.UId == id).FirstOrDefault();
            if (component == null)
                return RedirectToAction("Error404", "Error");
            Form form = null;
            if (component is IComponentItem)
                form = ((IComponentItem)component).Parent.Panel.Page.Form;
            else
                form = component.Panel.Page.Form;
            var prices = component.PriceGroup;
            if (prices == null)
            {
                prices = PriceGroup.New(Repository, component, company, user, name);
            }
            return View(prices);
        }

        [HttpPost]
        [ActionName("Price")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewPrice(Guid id, string name = null)
        {
            var priceGroup = Repository.Search<PriceGroup>(pg => pg.UId == id).FirstOrDefault();
            if (priceGroup == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Price Group" } };
            var component = priceGroup.Component;
            Form form = null;
            if (component is IComponentItem)
                form = ((IComponentItem)component).Parent.Panel.Page.Form;
            else
                form = component.Panel.Page.Form;
            var prices = Prices.New(Repository, priceGroup, company, user, name);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Price", "FormBuilder", new { id = prices.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [ActionName("Price")]
        public ActionResult EditPrice(Guid id)
        {
            var prices = Repository.Search<Prices>(p => p.UId == id).FirstOrDefault();
            if (prices == null)
                return RedirectToAction("Error404", "Error");
            var component = prices.PriceGroup.Component;
            Form form = null;
            if (component is IComponentItem)
                form = ((IComponentItem)component).Parent.Panel.Page.Form;
            else
                form = component.Panel.Page.Form;
            Guid formKey;
            if (prices.PriceGroup.Component is CheckboxItem)
            {
                formKey = ((CheckboxItem)prices.PriceGroup.Component).CheckboxGroup.Panel.Page.FormKey;
            }
            else if (prices.PriceGroup.Component is RadioItem)
            {
                formKey = ((RadioItem)prices.PriceGroup.Component).RadioGroup.Panel.Page.FormKey;
            }
            else if(prices.PriceGroup.Component is DropdownItem)
            {
                formKey = ((DropdownItem)prices.PriceGroup.Component).DropdownGroup.Panel.Page.FormKey;
            }
            else
            {
                formKey = prices.PriceGroup.Component.Panel.Page.FormKey;
            }
            var audienceList = prices.GetForm().Audiences;
            prices.Price.Sort((a, b) => a.Start.CompareTo(b.Start));
            ViewBag.AudienceList = audienceList;
            return View(prices);
        }

        [HttpPut]
        [ActionName("Price")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditPrice(Prices model)
        {
            var formData = Request.Form;
            var prices = Repository.Search<Prices>(p => p.UId == model.UId).FirstOrDefault();
            if (prices == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var component = prices.PriceGroup.Component;
            Form form = null;
            if (component is IComponentItem)
                form = ((IComponentItem)component).Parent.Panel.Page.Form;
            else
                form = component.Panel.Page.Form;
            prices.Name = model.Name;
            for (var i = 0; i < model.Price.Count; i++)
            {
                var f_price = model.Price[i];
                var price = prices.Price.FirstOrDefault(p => p.UId == f_price.UId);
                if (price == null)
                {
                    model.Price[i] = Price.New(Repository, prices, company, user, f_price.Start, f_price.Amount);
                }
                else
                {
                    price.Amount = f_price.Amount;
                    price.Start = f_price.Start;
                }
            }
            for (var i = 0; i < prices.Price.Count; i++)
            {
                var f_price = model.Price.FirstOrDefault(p => p.UId == prices.Price[i].UId);
                if (f_price != null)
                    continue;
                Repository.Remove(prices.Price[i]);
                i--;

            }
            var audienceUIds = JsonConvert.DeserializeObject<List<Guid>>(formData["audienceUIds"]);
            foreach (var uid in audienceUIds)
            {
                if (prices.Audiences.Where(a => a.UId == uid).Count() == 0)
                    prices.Audiences.Add(Repository.Search<Audience>(a => a.UId == uid).FirstOrDefault());
            }
            for (var i = 0; i < prices.Audiences.Count; i++)
            {
                var audience = prices.Audiences[i];
                if (!audienceUIds.Contains(audience.UId))
                {
                    prices.Audiences.RemoveAt(i);
                    i--;
                }
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        [HttpDelete]
        [ActionName("Price")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeletePrice(Guid id)
        {
            var prices = Repository.Search<Prices>(p => p.UId == id).FirstOrDefault();
            if (prices == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var component = prices.PriceGroup.Component;
            Form form = null;
            if (component is IComponentItem)
                form = ((IComponentItem)component).Parent.Panel.Page.Form;
            else
                form = component.Panel.Page.Form;
            Repository.Remove(prices);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        #region Tags

        [HttpGet]
        public ActionResult Tags()
        {
            var tagList = Repository.Search<Tag>(t => t.CompanyKey == company.UId).ToList();
            return View(tagList);
        }

        [HttpPost]
        [ActionName("Tag")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewTag(string name = null)
        {
            var tag = Tag.New(Repository, company, user, name);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Tag", "FormBuilder", new { id = tag.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [ActionName("Tag")]
        public ActionResult EditTag(Guid id)
        {
            var tag = Repository.Search<Tag>(t => t.UId == id).FirstOrDefault();
            if (tag == null)
                return RedirectToAction("Error404", "Error");
            return View("Tag", tag);
        }

        [HttpPut]
        [ActionName("Tag")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditTag(Tag model)
        {
            var tag = Repository.Search<Tag>(t => t.UId == model.UId).FirstOrDefault();
            if (tag == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            tag.Name = model.Name;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Tag", "FormBuilder", new { id = tag.UId }, Request.Url.Scheme) } };
        }

        [HttpDelete]
        [ActionName("Tag")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteTag(Guid id)
        {
            var tag = Repository.Search<Tag>(t => t.UId == id).FirstOrDefault();
            if (tag == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(tag);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        #endregion

        #region Types

        [HttpGet]
        public ActionResult Types()
        {
            var typesList = Repository.Search<NodeType>(t => t.CompanyKey == company.UId).ToList();
            return View(typesList);
        }

        [HttpPost]
        [ActionName("Type")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewType()
        {
            var type = new NodeType()
            {
                Name = "New Type " + DateTime.UtcNow.ToString(),
                Owner = user.UId,
                Group = company.UId,
                CompanyKey = company.UId
            };
            Repository.Add(type);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Type", "FormBuilder", new { id = type.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        public ActionResult Type(Guid id)
        {
            var type = Repository.Search<NodeType>(t => t.UId == id).FirstOrDefault();
            if (type == null)
                return RedirectToAction("Index");
            return View(type);
        }

        [HttpPut]
        [ActionName("Type")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditType(Tag model)
        {
            var type = Repository.Search<NodeType>(t => t.UId == model.UId).FirstOrDefault();
            if (type == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            type.Name = model.Name;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        [HttpDelete]
        [ActionName("Type")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteType(Guid id)
        {
            var type = Repository.Search<NodeType>(t => t.UId == id).FirstOrDefault();
            if (type == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(type);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        #endregion

        [HttpGet]
        public JsonNetResult FormVariables(Guid id)
        {
            var list = new List<ComponentInfo>();
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object." } };
            foreach (var page in form.Pages)
            {
                foreach (var panel in page.Panels)
                {
                    foreach (var component in panel.Components)
                    {
                        if (!(component is FreeText))
                        {
                            var componentInfo = new ComponentInfo()
                            {
                                Name = component.Name,
                                UId = component.UId.ToString(),
                                Type = Regex.Match(component.GetType().ToString(), @"^.*\.(\w*)_.*$").Groups[1].Value.ToLower()
                            };
                            list.Add(componentInfo);
                        }
                    }
                }
            }
            list.AddRange(new ComponentInfo[]
                {
                    new ComponentInfo()
                    {
                        Name = "Email",
                        UId = "email",
                        Type = "input",
                    },
                    new ComponentInfo()
                    {
                        Name = "Confirmation",
                        UId = "confirmation",
                        Type = "input"
                    },
                    new ComponentInfo()
                    {
                        Name = "Date Registered",
                        UId = "dateregistered",
                        Type = "datetime"
                    },
                    new ComponentInfo()
                    {
                        Name = "Status",
                        UId = "status",
                        Type = "status"
                    },
                    new ComponentInfo()
                    {
                        Name = "Audience",
                        UId = "audience",
                        Type = "audience"
                    },
                    new ComponentInfo()
                    {
                        Name = "RSVP",
                        UId = "rsvp",
                        Type = "rsvp"
                    }
                });
            list = list.OrderBy(i => i.Name).ToList();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Message = "Success", UId = id, Variables = list } };
        }

        [HttpGet]
        public JsonNetResult GetComponentItems(string id, Guid? form)
        {
            Guid uid;
            var list = new List<ComponentInfo>();
            if (Guid.TryParse(id, out uid))
            {
                var component = Repository.Search<Component>(c => c.UId == uid).FirstOrDefault();
                if (component == null)
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
                if (component is CheckboxGroup)
                {
                    var checkboxGroup = (CheckboxGroup)component;
                    list.AddRange(checkboxGroup.Items.OrderBy(i => i.LabelText).Select(c => new ComponentInfo() { Name = c.Name, Type = "", UId = c.UId.ToString() }).ToList());
                }
                else if (component is RadioGroup)
                {
                    var radioGroup = (RadioGroup)component;
                    list.AddRange(radioGroup.Items.OrderBy(i => i.LabelText).Select(c => new ComponentInfo() { Name = c.Name, Type = "", UId = c.UId.ToString() }).ToList());
                }
                else if (component is DropdownGroup)
                {
                    var dropdownGroup = (DropdownGroup)component;
                    list.AddRange(dropdownGroup.Items.OrderBy(i => i.LabelText).Select(c => new ComponentInfo() { Name = c.Name, Type = "", UId = c.UId.ToString() }).ToList());
                }
            }
            else
            {
                switch (id)
                {
                    case "status":
                        list.AddRange(new ComponentInfo[]
                        {
                            new ComponentInfo()
                            {
                                Name = "Incomplete",
                                UId = "0",
                                Type = "enum"
                            },
                            new ComponentInfo()
                            {
                                Name = "Submitted",
                                UId = "1",
                                Type = "enum"
                            },
                            new ComponentInfo()
                            {
                                Name = "Canceled",
                                UId = "2",
                                Type = "enum"
                            },
                            new ComponentInfo()
                            {
                                Name = "Canceled by Company",
                                UId = "3",
                                Type = "enum"
                            },
                            new ComponentInfo()
                            {
                                Name = "Canceled by Administrator",
                                UId = "4",
                                Type = "enum"
                            },
                            new ComponentInfo()
                            {
                                Name = "Complete",
                                UId = "5",
                                Type = "enum"
                            }
                        });
                        break;
                    case "type":
                        list.AddRange(new ComponentInfo[]
                        {
                            new ComponentInfo()
                            {
                                Name = "Test",
                                UId = "0",
                                Type = "enum"
                            },
                            new ComponentInfo()
                            {
                                Name = "Live",
                                UId = "1",
                                Type = "enum"
                            }
                        });
                        break;
                    case "audience":
                        if (!form.HasValue)
                            return new JsonNetResult() { Data = new { Success = false, Message = "You must provide a form to get audience." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        var s_form = Repository.Search<Form>(f => f.UId == form.Value).FirstOrDefault();
                        if (form == null)
                            return new JsonNetResult() { Data = new { Success = false, Message = "You must provide a form to get audience." }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                        list.AddRange(s_form.Audiences.OrderBy(a => a.Order).Select(a => new ComponentInfo() { Name = a.Name, Type = "audience", UId = a.UId.ToString() }));
                        break;
                    case "rsvp":
                        list.AddRange(new ComponentInfo[] { new ComponentInfo() { Name = "Accept", Type = "rsvp", UId = "True" }, new ComponentInfo() { Name = "Decline", Type = "rsvp", UId = "False" } });
                        break;
                }
            }
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Message = "Success", Items = list } };
        }


    }

    public class PageText
    {
        public Guid UId { get; set; }
        [AllowHtml]
        public string Html { get; set; }
        public string RSVPAccept { get; set; }
        public string RSVPDecline { get; set; }
    }

    public class ComponentInfo
    {
        public string UId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class Editor
    {
        public Guid UserKey { get; set; }
        public DateTimeOffset Date { get; set; }
        public Guid Component { get; set; }
        public string Name { get; set; }

        public Editor()
        {

        }

        public static Editor New(Guid Component, User user)
        {
            var editor = new Editor();
            editor.Component = Component;
            editor.UserKey = user.UId;
            editor.Name = user.UserName;
            editor.Date = DateTimeOffset.UtcNow;
            return editor;
        }
    }
}