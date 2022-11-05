using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;
using System.Threading;
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.JItems;
using RSToolKit.WebUI.Models.Views.Register;
using RSToolKit.WebUI.Models.Inputs.Register;
using RSToolKit.Domain.Exceptions;
using RSToolKit.Domain;
using System.Net.Mail;
using RSToolKit.Domain.Entities.Components;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RSToolKit.WebUI.Infrastructure.Register;
using System.IO;
using RSToolKit.Domain.Entities.MerchantAccount;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using RSToolKit.Domain.Engines;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles = "Super Administrators,Administrators,Cloud Users,Cloud+ Users,Programmers")]
    public class AdminRegisterController
        : RegStepController
    {
        #region Properties and Fields
        protected Registrant _registrant;
        protected FormRepository _formRepo;
        protected Form _form;
        protected RegistrantRepository _regRepo;
        protected Dictionary<string, IEnumerable<JLogicCommand>> _allCommands;
        protected RSParser _parser;
        protected IRegisterHtmlHelper _htmlHelper;
        protected int _pageNumber;
        protected Page _page;

        /// <summary>
        /// The current form being used.
        /// </summary>
        public Form Form
        {
            get
            {
                return this._form;
            }
        }
        /// <summary>
        /// The current page being used.
        /// </summary>
        public Page CurrentPage
        {
            get
            {
                if (this._page != null)
                    return _page;
                else if (this._form != null)
                    return this._form.Pages.FirstOrDefault(p => p.PageNumber == this._pageNumber);
                return null;
            }
        }
        /// <summary>
        /// The page number of the current page.
        /// </summary>
        public int CurrentPageNumber
        {
            get
            {
                return this._pageNumber;
            }
        }
        /// <summary>
        /// The registrant being used.
        /// </summary>
        public Registrant Registrant
        {
            get
            {
                return this._registrant;
            }
        }

        #region The static variables
        #region Upload MIME dictionary.
        /// <summary>
        /// Holds the allowed MIME types for various upload types.
        /// </summary>
        public static readonly Dictionary<string, string[]> AllowedTypes = new Dictionary<string, string[]>()
        {
            {
                "picture", new string[]
                {
                    "image/jpeg",
                    "image/bmp",
                    "image/gif",
                    "image/tiff",
                    "image/png"
                }
            },
            {
                "pdf", new string[]
                {
                    "application/pdf"
                }
            },
            {
                "text", new string[]
                {
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "application/msword",
                    "text/richtext",
                    "text/plain",
                    "application/msword",
                    "application/x-iwork-pages-sffpages"
                }
            }
        };
        #endregion
        #region View Constants
        /// <summary>
        /// The notice used for the start screen;
        /// </summary>
        public static HtmlString st_StartNotice = new HtmlString("Please enter the email of the <b>registrant</b> below.<br /><br />Only one registration record is allowed per email. If you are registering on behalf of someone else, please input <b>their email</b> below.<br /><br />When you click <b>Start Registration</b>, you will receive a secure link via email to continue this registration in case you do not complete it now.<br /><br />If you complete this registration, you will receive a confirmation email with a link to your confirmation page.");
        #endregion
        #endregion

        #endregion

        #region Constructors
        public AdminRegisterController()
            : base()
        {
            this._regRepo = new RegistrantRepository(Context);
            Repositories.Add(this._regRepo);
            this._formRepo = new FormRepository(Context);
            Repositories.Add(this._formRepo);
            this._registrant = null;
            this._form = null;
            Log.LoggingMethod = "AdminRegisterController";
            this._parser = null;
            this._htmlHelper = null;
            this._pageNumber = -1;
            this._page = null;
            this._allCommands = new Dictionary<string, IEnumerable<JLogicCommand>>();
        }
        #endregion

        #region Actions

        #region Start a form
        [HttpGet]
        [ComplexId("id")]
        public ActionResult Index(object id, string email = "")
        {
            return Start(id, email);
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult Start(object id, string email = "")
        {
            this._GetForm(id);
            var view = new StartView()
            {
                Notice = String.IsNullOrWhiteSpace(this._form.Start) ? st_StartNotice : new HtmlString(this._form.Start),
                FormState = this._form.Status.GetStringValue(),
                FormName = this._form.Name,
                OpenForm = this._form.Status.In(FormStatus.Open, FormStatus.Ready),
                Email = email
            };
            this._FillHelper();
            this._FillRegisterView(view);
            return View("Start", view);
        }

        [HttpPost]
        [IsAjax]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Start(StartInput input)
        {
            this._GetForm(input.FormKey);
            var edit = false;
            // We make the email lowercase.
            var email = input.Email.ToLower();
            // We try to make the email into a mail address.
            try
            {
                var mailAddress = new MailAddress(email);
            }
            catch (Exception)
            {
                // Invalid mail address. We return an error.
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = false, errors = new List<dynamic>() { new { id = "Email", message = "Invalid email." } } } };
            }
            // We look for the registrant.
            this._registrant = this._regRepo.Search(r => r.Email.ToLower() == input.Email.ToLower() && r.FormKey == input.FormKey && r.Type == RegistrationType.Live).FirstOrDefault();
            if (this._registrant != null)
            {
                // The registrant email exists so we are now editing an existing registrant.
                edit = true;
            }
            else
            {
                // The registrant email did not exists, so we start a new registration and add it to the context.
                this._registrant = new Registrant()
                {
                    Email = email,
                    Form = this._form,
                    FormKey = this._form.UId,
                    Type = RegistrationType.Live,
                    Status = RegistrationStatus.Incomplete,
                    StatusDate = DateTimeOffset.Now,
                    UId = Guid.NewGuid()
                };
                this._regRepo.Add(this._registrant);
                TempData["firstRSVP"] = true;
            }
            
            // Now we look to see if there is an associated contact.
            var contact = Context.Contacts.FirstOrDefault(c => this._registrant.Email == c.Name && c.CompanyKey == this._form.CompanyKey);
            if (!edit)
            {
                if (contact != null)
                {
                    // There was a contact, we assign it to the registrant.
                    this._registrant.Contact = contact;
                    #region Setting Mapped Values
                    // We are not editing, so we set values based on the contact data.
                    foreach (var page in this._form.Pages)
                    {
                        foreach (var panel in page.Panels)
                        {
                            foreach (var component in panel.Components)
                            {
                                if (component.MappedTo == null)
                                    continue;
                                var c_data = contact.Data.Where(d => d.HeaderKey == component.MappedTo.UId).FirstOrDefault();
                                if (c_data == null)
                                    continue;
                                if (component is Input)
                                {
                                    var r_data = new RegistrantData()
                                    {
                                        RegistrantKey = this._registrant.UId,
                                        Registrant = this._registrant,
                                        VariableUId = component.UId,
                                        Component = component,
                                        Value = c_data.Value
                                    };
                                    this._registrant.Data.Add(r_data);
                                }
                                else if (component is CheckboxGroup)
                                {
                                    var cbg = (CheckboxGroup)component;
                                    var r_items = c_data.Value.Split(',');
                                    var c_items = new List<CheckboxItem>();
                                    foreach (var r_item in r_items)
                                    {
                                        var r_t_item = r_item.Trim().ToLower();
                                        var c_item = cbg.Items.FirstOrDefault(i => i.LabelText.ToLower() == r_t_item);
                                        if (c_item == null)
                                            c_item = cbg.Items.FirstOrDefault(i => Regex.Replace(i.LabelText.ToLower(), @"\s", "") == Regex.Replace(r_t_item, @"\s", ""));
                                        if (c_item == null)
                                            c_item = cbg.Items.FirstOrDefault(i => i.UId.ToString().ToLower() == r_t_item);
                                        if (c_item != null)
                                            c_items.Add(c_item);
                                    }
                                    var r_data = new RegistrantData()
                                    {
                                        RegistrantKey = this._registrant.UId,
                                        Registrant = this._registrant,
                                        VariableUId = component.UId,
                                        Component = component,
                                        Value = JsonConvert.SerializeObject(c_items)
                                    };
                                    this._registrant.Data.Add(r_data);
                                }
                                else if (component is DropdownGroup)
                                {
                                    var ddg = (DropdownGroup)component;
                                    var c_item = ddg.Items.FirstOrDefault(i => i.LabelText.ToLower() == c_data.Value.ToLower());
                                    if (c_item == null)
                                        c_item = ddg.Items.FirstOrDefault(i => i.UId.ToString().ToLower() == c_data.Value.ToLower());
                                    var itemValue = c_item != null ? c_item.UId.ToString() : null;
                                    var r_data = new RegistrantData()
                                    {
                                        RegistrantKey = this._registrant.UId,
                                        Registrant = this._registrant,
                                        VariableUId = component.UId,
                                        Component = component,
                                        Value = itemValue
                                    };
                                    this._registrant.Data.Add(r_data);
                                }
                                else if (component is RadioGroup)
                                {
                                    var rg = (RadioGroup)component;
                                    var c_item = rg.Items.FirstOrDefault(i => i.LabelText.ToLower() == c_data.Value.ToLower());
                                    if (c_item == null)
                                        c_item = rg.Items.FirstOrDefault(i => i.UId.ToString().ToLower() == c_data.Value.ToLower());
                                    var itemValue = c_item != null ? c_item.UId.ToString() : null;
                                    var r_data = new RegistrantData()
                                    {
                                        RegistrantKey = this._registrant.UId,
                                        Registrant = this._registrant,
                                        VariableUId = component.UId,
                                        Component = component,
                                        Value = itemValue
                                    };
                                    this._registrant.Data.Add(r_data);
                                }
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    // No contact found so we set mapped values to blank.
                    #region Nulling Mapped Values
                    foreach (var page in this._form.Pages)
                    {
                        foreach (var panel in page.Panels)
                        {
                            foreach (var component in panel.Components)
                            {
                                if (component.MappedTo == null)
                                    continue;
                                if (component is Input)
                                {
                                    var r_data = new RegistrantData()
                                    {
                                        RegistrantKey = this._registrant.UId,
                                        Registrant = this._registrant,
                                        VariableUId = component.UId,
                                        Component = component
                                    };
                                    this._registrant.Data.Add(r_data);
                                }
                                else if (component is CheckboxGroup)
                                {
                                    var cbg = (CheckboxGroup)component;
                                    var c_items = new List<Guid>();
                                    var r_data = new RegistrantData()
                                    {
                                        RegistrantKey = this._registrant.UId,
                                        Registrant = this._registrant,
                                        VariableUId = component.UId,
                                        Component = component,
                                        Value = "[]"
                                    };
                                    this._registrant.Data.Add(r_data);
                                }
                                else if (component is DropdownGroup)
                                {
                                    var ddg = (DropdownGroup)component;
                                    var r_data = new RegistrantData()
                                    {
                                        RegistrantKey = this._registrant.UId,
                                        Registrant = this._registrant,
                                        VariableUId = component.UId,
                                        Component = component
                                    };
                                    this._registrant.Data.Add(r_data);
                                }
                                else if (component is RadioGroup)
                                {
                                    var rg = (RadioGroup)component;
                                    var r_data = new RegistrantData()
                                    {
                                        RegistrantKey = this._registrant.UId,
                                        Registrant = this._registrant,
                                        VariableUId = component.UId,
                                        Component = component
                                    };
                                    this._registrant.Data.Add(r_data);
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
            // Now we save the context.
            this._regRepo.Commit();
            // Now we find the next page. In this case it is rsvp.
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, location = Url.Action("Page", "AdminRegister", new { page = 1, id = this._registrant.SortingId }) } };
        }
        #endregion

        #region Page
        [HttpPost]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult RSVP(RSVPInput input)
        {
            this._GetRegistrant(input.RegistrantKey);
            this._form = this._registrant.Form;
            this._registrant.RSVP = input.RSVP;
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, location = Url.Action("Page", new { id = this._registrant.SortingId, page = 2 }) } };
        }

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Audience(AudienceInput input)
        {
            this._GetRegistrant(input.RegistrantKey);
            this._form = this._registrant.Form;
            this._registrant.AudienceKey = input.Audience;
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, location = Url.Action("Page", new { id = this._registrant.SortingId, page = 3 }) } };
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult Page(object id, int page)
        {
            this._registrant = this._GetRegistrant(id);
            this._form = this._registrant.Form;
            this._FillHelper();
            // We need to get the page we are on.
            Page nextPage = null;
            if (page > 0)
            {
                nextPage = this._NextPage(page - 1);
                if (nextPage == null)
                    page = PageNumber.Review;
            }
            if (nextPage == null)
            {
                #region Registrant Clean Up
                // We are finished with the registration. Now we need to let the user perform registration cleanup.
                if (page == PageNumber.Review)
                {
                    // The user needs to review the selections.
                    #region Review
                    var reviewView = new ReviewView();
                    this._FillRegisterView(reviewView);
                    var summary = @"<table class=""table confirmation-table""><tbody><tr><td>Confirmation Number:</td><td>" + this._registrant.Confirmation + @"</td></tr><tr><td>Email:</td><td>" + this._registrant.Email + @"</td></tr>";
                    foreach (var p in this._form.Pages.Where(p => p.Type == RSToolKit.Domain.Entities.PageType.Audience || p.Type == RSToolKit.Domain.Entities.PageType.RSVP || p.Type == RSToolKit.Domain.Entities.PageType.UserDefined).OrderBy(p => p.PageNumber))
                        summary += this._htmlHelper.RenderConfirmationPage(p).ToString();
                    summary += "</tbody></table>";
                    reviewView.Summary = new HtmlString(summary);
                    /* Need to create view */
                    #endregion
                    reviewView.PreviousPage = this._BackPage(PageNumber.Review);
                    return View("Review", reviewView);
                }
                if (page == PageNumber.Promotions || page == PageNumber.CheckOut)
                {
                    var paymentSkipped = this._form.DisableShoppingCart ||
                        this._registrant.TotalOwed <= 0 ||
                        this._registrant.Data.Any(d => d.Component.Variable.Value == "__SkipPayment" && d.Value == "true");
                    var noPromo = this._registrant.Data.Any(d => d.Component.Variable.Value == "__NoPromo" && d.Value == "true");

                    if (noPromo)
                    {
                        // Promotion codes not allowed for this registrant. We clear them and change the page number.
                        page = PageNumber.CheckOut;
                        this._registrant.PromotionalCodes.Clear();
                        // We save the cleared promotion codes.
                        this._registrant.PromotionalCodes.ForEach(p => Context.RemovePromotionCodeEntry(p));
                        this._registrant.PromotionalCodes.Clear();
                        Context.SaveChanges();
                    }
                    if (!paymentSkipped)
                    {
                        if (page == PageNumber.Promotions && this._form.PromotionalCodes.Count > 0)
                        {
                            // The use needs to enter promotions
                            #region Promotions
                            var promotionsView = new PromotionsView();
                            promotionsView.SkipPayment = paymentSkipped;
                            this._FillRegisterView(promotionsView);
                            var codeHtml = "";
                            foreach (var pc in this._registrant.PromotionalCodes)
                                codeHtml += @"<tr><td>" + pc.Code.Code + "</td><td>" + pc.Code.Description + @"</td><td class=""fill""></td></tr>";
                            promotionsView.Codes = new HtmlString(codeHtml);
                            #endregion
                            promotionsView.PreviousPage = this._BackPage(PageNumber.Promotions);
                            return View("PromotionCodes", promotionsView);
                        }
                        else
                        {
                            page = PageNumber.CheckOut;
                        }

                        if (page == PageNumber.CheckOut)
                        {
                            // The user is checking out.
                            #region Checkout
                            var checkoutView = new CheckOutView();
                            this._FillRegisterView(checkoutView);
                            checkoutView.Payments = this._form.MerchantAccount != null;
                            checkoutView.Invoice = new HtmlString(this._registrant.Parse("[inv=>details]", AppUser));
                            #endregion
                            checkoutView.PreviousPage = this._BackPage(PageNumber.CheckOut);
                            return View("CheckOut", checkoutView);
                        }
                    }
                }
                // We are on confirmation so we redirect to there.  We redirect so it can be bookmarked if wanted.
                return RedirectToAction("Confirmation", new { id = this._registrant.SortingId, checkStatus = true });
                #endregion
            }

            var commands = this._ExecuteLogic(nextPage);
            var skippingPage = false;
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.PageSkip:
                        skippingPage = true;
                        break;
                    case JLogicWork.SetVar:
                        if (skippingPage)
                            continue;
                        #region SetVar
                        if (!command.Parameters.ContainsKey("Form") || Guid.Parse(command.Parameters["Form"]) == this._registrant.FormKey)
                        {
                            Guid variableUId;
                            var variable = command.Parameters["Variable"];
                            var value = command.Parameters["Value"];
                            RegistrantData dataPoint;
                            bool exit = false;
                            switch (variable.ToLower())
                            {
                                case "email":
                                    this._registrant.Email = value;
                                    exit = true;
                                    break;
                                case "RSVP":
                                    this._registrant.RSVP = bool.Parse(value);
                                    exit = true;
                                    break;
                                case "Audience":
                                    this._registrant.Audience = this._form.Audiences.Where(a => a.UId == Guid.Parse(value)).First();
                                    exit = true;
                                    break;
                                case "Status":
                                    this._registrant.Status = (RegistrationStatus)Int32.Parse(variable);
                                    exit = true;
                                    break;
                            }
                            if (exit)
                                break;
                            variableUId = Guid.Parse(variable);
                            dataPoint = this._registrant.Data.Where(d => d.VariableUId == variableUId).FirstOrDefault();
                            if (dataPoint == null)
                            {
                                dataPoint = new RegistrantData()
                                {
                                    VariableUId = variableUId,
                                    Value = value,
                                };
                                dataPoint.Component = Context.Components.First(c => c.UId == variableUId);
                                this._registrant.Data.Add(dataPoint);
                            }
                            dataPoint.Value = value;
                        }
                        else
                        {
                            Guid variableUId;
                            Guid formUId = Guid.Parse(command.Parameters["Form"]);
                            var c_form = Context.Forms.First(f => f.UId == formUId);
                            var c_registrant = Context.Registrants.FirstOrDefault(r => r.FormKey == c_form.UId && r.Email == this._registrant.Email);
                            if (c_registrant == null)
                                break;
                            var variable = command.Parameters["Variable"];
                            var value = command.Parameters["Value"];
                            RegistrantData dataPoint;
                            bool exit = false;
                            switch (variable.ToLower())
                            {
                                case "email":
                                    c_registrant.Email = value;
                                    exit = true;
                                    break;
                                case "RSVP":
                                    c_registrant.RSVP = bool.Parse(value);
                                    exit = true;
                                    break;
                                case "Audience":
                                    c_registrant.Audience = c_form.Audiences.Where(a => a.UId == Guid.Parse(value)).First();
                                    exit = true;
                                    break;
                                case "Status":
                                    c_registrant.Status = (RegistrationStatus)Int32.Parse(variable);
                                    exit = true;
                                    break;
                            }
                            if (exit)
                                break;
                            variableUId = Guid.Parse(variable);
                            dataPoint = c_registrant.Data.Where(d => d.VariableUId == variableUId).FirstOrDefault();
                            if (dataPoint == null)
                            {
                                dataPoint = new RegistrantData()
                                {
                                    VariableUId = variableUId,
                                    Value = value
                                };
                                dataPoint.Component = Context.Components.First(c => c.UId == variableUId);
                            }
                            dataPoint.Value = value;
                        }
                        break;
                        #endregion
                }
            }

            if (skippingPage)
            {
                this._SetPageValuesBlank(nextPage);
                Context.SaveChanges();
                return RedirectToAction("Page", new { id = this._registrant.SortingId, page = nextPage.PageNumber + 1 });
            }
            Context.SaveChanges();
            if (nextPage.Type == PageType.RSVP)
            {
                // We are rendering the RSVP page.
                var rsvpView = new RSVPView();
                this._FillRegisterView(rsvpView);
                rsvpView.First = false;
                if (TempData.ContainsKey("firstRSVP"))
                    rsvpView.First = true;
                rsvpView.RSVP = this._htmlHelper.RenderRsvp(rsvpView.First);
                rsvpView.Notice = this._form.Parse(this._htmlHelper.Render(nextPage.Panels[0].Components[0]), AppUser);
                return View("RSVP", rsvpView);
            }
            if (nextPage.Type == PageType.Audience)
            {
                // We are rendering the audience page.
                var audienceView = new AudiencesView();
                this._FillRegisterView(audienceView);
                audienceView.Audiences = this._htmlHelper.RenderAudiences();
                audienceView.Notice = this._form.Parse(this._htmlHelper.Render(nextPage.Panels[0].Components[0]), AppUser);
                audienceView.PreviousPage = this._BackPage(2);
                return View("Audience", audienceView);
            }
            var pageView = new PageView();
            this._FillRegisterView(pageView);
            // Now we fill the page view.
            foreach (var panel in nextPage.Panels.OrderBy(p => p.Order))
            {
                pageView.PanelHtml.Add(this._htmlHelper.RenderPanel(panel));
            }
            pageView.PreviousPage = this._BackPage(nextPage.PageNumber);
            return View(pageView);
        }

        [HttpPost]
        [IsAjax]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Page(PageInput input)
        {
            this._GetRegistrant(input.RegistrantKey);
            this._form = this._registrant.Form;
            var errors = new List<SetDataError>();

            #region Run through uploaded files

            foreach (var fileKey in Request.Files.AllKeys)
            {
                Guid fUId;
                if (!Guid.TryParse(fileKey, out fUId))
                    continue;
                var fileComponent = Context.Components.OfType<Input>().FirstOrDefault(c => c.UId == fUId);
                if (fileComponent == null)
                    continue;
                var f_dp = this._registrant.Data.FirstOrDefault(d => d.VariableUId == fUId);
                if (f_dp == null)
                {
                    f_dp = new RegistrantData()
                    {
                        VariableUId = fUId,
                        Value = "",
                        Registrant = this._registrant,
                        RegistrantKey = this._registrant.UId
                    };
                    this._registrant.Data.Add(f_dp);
                }
                f_dp.Component = fileComponent;
                if (Request.Files[fileKey].ContentLength < 1)
                {
                    continue;
                }
                var file = Request.Files[fileKey];
                var fileSize = file.ContentLength;
                if (fileSize > 2097152)
                {
                    errors.Add(new SetDataError(fUId.ToString(), "Your file cannot exceed 2 Mb."));
                    continue;
                }
                var mimeType = file.ContentType;
                if (!AllowedTypes[fileComponent.FileType].Contains(mimeType))
                {
                    errors.Add(new SetDataError(fUId.ToString(), "Unauthorized file type. The file must be a " + fileComponent.FileType + "."));
                    continue;
                }
                var stream = file.InputStream;
                if (f_dp.File == null)
                    f_dp.File = new RegistrantFile()
                    {
                        FileType = mimeType,
                        Extension = Path.GetExtension(file.FileName),
                        BinaryData = new byte[fileSize]
                    };
                stream.Read(f_dp.File.BinaryData, 0, file.ContentLength);
                f_dp.Value = "UPLOADED";
            }

            #endregion

            #region Run through non file components

            //First we run through wait listings
            foreach (var kvp in input.Waitlistings)
            {
                // Lets grab the component
                var component = Context.Components.FirstOrDefault(c => c.UId == kvp.Key);
                if (component == null || component.Seating == null)
                    continue;
                if (component.Seating.Waitlistable)
                {
                    // It is wait listing
                    if (kvp.Value)
                    {
                        if (component.Seating.Seaters.Where(s => s.ComponentKey == component.UId && s.RegistrantKey == this._registrant.UId && s.Seated == false).Count() < 1)
                        {
                            // Not currently wait listing.
                            var t_seat = Seater.New(component.Seating, this._registrant, component, false);
                        }
                    }
                    else
                    {
                        var seater = component.Seating.Seaters.Where(s => s.ComponentKey == component.UId && s.RegistrantKey == this._registrant.UId && s.Seated == false).FirstOrDefault();
                        if (seater != null)
                            Context.RemoveSeater(seater); // Remove wait listing if previously was.
                    }
                }
            }

            //First thing we do is iterate over the components sent to us.
            foreach (var kvp in input.Components)
            {
                var result = this._registrant.SetData(kvp.Key.ToString(), kvp.Value, ignoreValidation: false, ignoreRequired: true, resetValueOnError: false, ignoreCapacity: true);
                result.Errors.ForEach(e => errors.Add(e));
            }
            #endregion

            if (errors.Count > 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = false, errors = errors } };
            this._regRepo.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, location = Url.Action("Page", new { id = this._registrant.SortingId, page = input.PageNumber + 1 }) } };
        }
        #endregion

        #region Promotions
        [HttpPost]
        [IsAjax]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult PromotionCode(Guid id, string code)
        {
            this._GetRegistrant(id);
            this._form = this._registrant.Form;
            var promotionCode = Context.PromotionCodes.FirstOrDefault(c => c.Code == code && c.FormKey == this._form.UId);
            if (code == null)
                throw new RegStepException("The promotion code was not found.");
            var userCode = this._registrant.PromotionalCodes.FirstOrDefault(c => c.CodeKey == promotionCode.UId);
            if (userCode != null)
                throw new RegStepException("The promotion code has already been used.");
            var entry = new PromotionCodeEntry()
            {
                UId = Guid.NewGuid(),
                Registrant = this._registrant,
                RegistrantKey = this._registrant.UId,
                Code = promotionCode,
                CodeKey = promotionCode.UId
            };
            this._registrant.PromotionalCodes.Add(entry);
            Context.SaveChanges();
            return new JsonNetResult()
            {
                Data = new { success = true, message = promotionCode.Description },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        #endregion

        #region Confirmation
        [HttpGet]
        [ComplexId("id")]
        public ActionResult Confirmation(object id)
        {
            this._GetRegistrant(id);
            this._form = this._registrant.Form;
            if (!this._registrant.Status.In(RegistrationStatus.Deleted, RegistrationStatus.Canceled, RegistrationStatus.CanceledByAdministrator, RegistrationStatus.CanceledByCompany))
                this._registrant.Status = RegistrationStatus.Submitted;
            this._regRepo.Commit();
            var cPage = this._form.Pages.FirstOrDefault(p => p.Type == PageType.Confirmation);
            var model = new ConfirmationView();
            this._FillRegisterView(model);
            model.Title = "Confirmation - " + this._form.Name;
            model.ShowShoppingCart = !this._form.DisableShoppingCart;
            if (model.ShowShoppingCart)
                model.Invoice = new HtmlString(this._registrant.Parse("[inv=>details]", AppUser));
            var summary = @"<table class=""table confirmation-table""><tbody><tr><td>Confirmation Number:</td><td>" + this._registrant.Confirmation + @"</td></tr><tr><td>Email:</td><td>" + this._registrant.Email + @"</td></tr>";
            foreach (var page in this._form.Pages.Where(p => p.Type == RSToolKit.Domain.Entities.PageType.Audience || p.Type == RSToolKit.Domain.Entities.PageType.RSVP || p.Type == RSToolKit.Domain.Entities.PageType.UserDefined).OrderBy(p => p.PageNumber))
                summary += this._htmlHelper.RenderConfirmationPage(page).ToString();
            summary += "</tbody></table>";
            model.Summary = new HtmlString(summary);
            model.FormBadge = null;
            model.RegistrantStatus = this._registrant.Status;
            model.RegistrantEmail = this._registrant.Email;
            model.RegistrantConfirmation = this._registrant.Confirmation;
            model.FormId = this._form.SortingId;
            if (cPage != null)
                model.Notice = this._registrant.Parse(this._htmlHelper.Render(cPage.Panels[0].Components[0]), AppUser);
            return View(model);
        }
        #endregion

        #region Merchant
        [HttpGet]
        [ComplexId("id")]
        public ActionResult Charge(object id)
        {
            this._GetRegistrant(id);
            var forceBillMe = this._registrant.Data.Any(d => d.Component.Variable != null && d.Component.Variable.Value == "__ForceBillMe" && (d.Value == "true" || d.Value == "True"));
            if (forceBillMe)
                return BillMe(id);
            var view = new ChargeView();
            this._FillRegisterView(view);
            view.FullInputs = this._form.MerchantAccount.Descriminator == "paypal";
            if (this._registrant.TotalOwed < 0.00m)
            {
                return Confirmation(id);
            }
            return View("Charge", view);
        }

        [HttpPost]
        [IsAjax]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Charge(ChargeInput input)
        {
            #region Initialization
            this._GetRegistrant(input.RegistrantKey);
            //var formData = Request.Form;
            var errors = new List<SetDataError>();
            #endregion

            var ammount = this._registrant.TotalOwed;

            if (this._form.MerchantAccount == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, location = Url.Action("Page", new { id = this._registrant.SortingId, page = PageNumber.Confirmation }) } };
            IMerchantAccount<TransactionRequest> gateway = this._form.MerchantAccount.GetGateway();
            //Now we build the transaction Request.
            var request = new TransactionRequest()
            {
                Zip = input.ZipCode,
                Ammount = ammount,
                CVV = input.CardCode,
                CardNumber = input.CardNumber,
                NameOnCard = input.NameOnCard,
                ExpMonthAndYear = input.ExpMonth + input.ExpYear,
                Cart = JsonConvert.SerializeObject(this._registrant.GetShoppingCartItems().ToCart()),
                CompanyKey = this._form.CompanyKey,
                MerchantAccountKey = this._form.MerchantAccountKey,
                FormKey = this._form.UId,
                Form = this._form,
                RegistrantKey = this._registrant.UId,
                Registrant = this._registrant,
                TransactionType = TransactionType.AuthorizeCapture,
                Mode = (this._registrant.Type == RegistrationType.Live ? ServiceMode.Live : ServiceMode.Test),
                LastFour = input.CardNumber.GetLast(4),
                DateCreated = DateTimeOffset.UtcNow,
                DateModified = DateTimeOffset.UtcNow,
                Currency = this._form.Currency,
                Address = input.Line1,
                Address2 = input.Line2,
                City = input.City,
                Country = input.Country,
                State = input.State,
                Phone = input.Phone
            };
            if (this._form.MerchantAccount.Descriminator == "paypal")
            {
                request.CardType = input.CardType;
            }
            var names = input.NameOnCard.Split(' ');
            if (names.Length < 2)
            {
                errors.Add(new SetDataError("NameOnCard", "You must supply a first and last name."));
                request.FirstName = "";
                request.LastName = input.NameOnCard;
            }
            else
            {
                request.FirstName = names[0];
                request.LastName = names[1];
            }
            TransactionDetail td;
            if (this._registrant.Type == RegistrationType.Live)
            {
                request.Mode = ServiceMode.Live;
                if (String.IsNullOrEmpty(input.CardNumber))
                    errors.Add(new SetDataError("CardNumber", "The number field is required."));
                if (!CCHelper.ValidateCard(input.CardNumber))
                    errors.Add(new SetDataError("CardNumber", "Not a valid credit card number."));
                if (String.IsNullOrEmpty(input.CardCode))
                    errors.Add(new SetDataError("CardCode", "The card code is required."));
                if (!Regex.IsMatch(input.CardCode, @"^[0-9]+$"))
                    errors.Add(new SetDataError("CardCode", "The code must be numbers only."));
                if (String.IsNullOrWhiteSpace(input.NameOnCard))
                    errors.Add(new SetDataError("NameOnCard", "The name is required."));
                if (String.IsNullOrWhiteSpace(input.ZipCode))
                    errors.Add(new SetDataError("ZipCode", "The postal code is required."));
                if (errors.Count > 0)
                {
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = false, errors = errors } };
                }
                td = gateway.AuthorizeCapture(request);
            }
            else
            {
                td = new TransactionDetail()
                {
                    Approved = true,
                    Ammount = request.Ammount,
                    AuthorizationCode = "TEST",
                    FormKey = request.FormKey,
                    Message = "Success",
                    TransactionType = TransactionType.AuthorizeCapture,
                    TransactionID = "__TEST__" + Guid.NewGuid().ToString()
                };
            }
            if (!this._registrant.TransactionRequests.Contains(request))
                this._registrant.TransactionRequests.Add(request);
            if (!request.Details.Contains(td))
                request.Details.Add(td);
            Context.TransactionRequests.Add(request);
            this._registrant.UpdateAccounts();
            Context.SaveChanges();
            if (td.Approved)
            {
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, location = Url.Action("ChargeConfirmation", new { id = this._registrant.SortingId }) } };
            }
            else
            {
                errors.Add(new SetDataError("", "The charge was declined."));
                errors.Add(new SetDataError("", td.Message));
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = false, errors = errors } };
            }
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult ChargeConfirmation(object id)
        {
            this._GetRegistrant(id);
            var view = new ChargeConfirmationView();
            this._FillRegisterView(view);
            return View("ChargeConfirmation", view);
        }

        [HttpPost]
        [IsAjax]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult ChargeConfirmation(MerchantConfirmationInput input)
        {
            this._GetRegistrant(input.RegistrantKey);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, location = Url.Action("Page", new { id = this._registrant.SortingId, page = PageNumber.Confirmation }) } };
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult BillMe(object id)
        {
            this._GetRegistrant(id);
            var view = new BaseRegisterView();
            this._FillRegisterView(view);
            return View("BillMe", view);
        }

        [HttpPost]
        [IsAjax]
        [ValidateAntiForgeryToken]
        public JsonNetResult BillMe(MerchantConfirmationInput input)
        {
            this._GetRegistrant(input.RegistrantKey);
            this._registrant.PayingAgentNumber = input.PayingAgentNumber;
            this._registrant.PayingAgentName = input.PayingAgentName;
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, location = Url.Action("Page", new { id = this._registrant.SortingId, page = PageNumber.Confirmation }) } };
        }

        #endregion

        #region Invoice
        [HttpGet]
        [ComplexId("id")]
        public FileContentResult Invoice(object id)
        {
            this._GetRegistrant(id);
            var binaryData = new byte[0];
            using (var m_stream = new MemoryStream())
            {
                var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
                var subTitleFont = FontFactory.GetFont("Arial", 14, Font.BOLD);
                var boldTableFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
                var endingMessageFont = FontFactory.GetFont("Arial", 10, Font.ITALIC);
                var bodyFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);
                var document = new Document(PageSize.A4, 40, 25, 40, 65);
                PdfWriter.GetInstance(document, m_stream);
                document.Open();
                document.Add(new Paragraph(this._registrant.Form.Name + ": Invoice/Reciept", titleFont));
                var table = new Table(4);
                table.DefaultHorizontalAlignment = 0;
                table.DefaultCellBorder = 0;
                table.WidthPercentage = 100;
                table.SpaceInsideCell = 5;
                table.Border = 0;
                table.AddCell(new Phrase("Item", boldTableFont));
                table.AddCell(new Phrase("Price", boldTableFont));
                table.AddCell(new Phrase("Quantity", boldTableFont));
                table.AddCell(new Phrase("Total", boldTableFont));

                var shoppingCart = this._registrant.GetShoppingCartItems();
                var runningTotal = 0.00m;

                foreach (var item in shoppingCart.Items)
                {
                    var total = (decimal)item.Quanity * item.Ammount;
                    runningTotal += total;
                    table.AddCell(new Phrase(item.Name, bodyFont));
                    table.AddCell(new Phrase(Math.Round(item.Ammount).ToString("c", this._registrant.Form.Culture), bodyFont));
                    table.AddCell(new Phrase(item.Quanity.ToString(), bodyFont));
                    table.AddCell(new Phrase(Math.Round(total).ToString("c", this._registrant.Form.Culture), bodyFont));
                }
                table.AddCell(new Phrase(""));
                table.AddCell(new Phrase(""));
                table.AddCell(new Phrase("Total Fees:", boldTableFont));
                table.AddCell(new Phrase(Math.Round(runningTotal).ToString("c", this._registrant.Form.Culture), boldTableFont));

                foreach (var code in this._registrant.PromotionalCodes.Where(c => c.Code.Action == RSToolKit.Domain.Entities.ShoppingCartAction.Add))
                {
                    runningTotal += code.Code.Amount;
                    table.AddCell(new Phrase(code.Code.Code, bodyFont));
                    table.AddCell(new Phrase("Add", bodyFont));
                    table.AddCell(new Phrase(Math.Round(code.Code.Amount).ToString("c", this._registrant.Form.Culture), bodyFont));
                    table.AddCell(new Phrase(Math.Round(runningTotal).ToString("c", this._registrant.Form.Culture), bodyFont));
                }
                foreach (var code in this._registrant.PromotionalCodes.Where(c => c.Code.Action == RSToolKit.Domain.Entities.ShoppingCartAction.Subtract))
                {
                    runningTotal -= code.Code.Amount;
                    table.AddCell(new Phrase(code.Code.Code, bodyFont));
                    table.AddCell(new Phrase("Subtract", bodyFont));
                    table.AddCell(new Phrase(Math.Round(code.Code.Amount).ToString("c", this._registrant.Form.Culture), bodyFont));
                    table.AddCell(new Phrase(Math.Round(runningTotal).ToString("c", this._registrant.Form.Culture), bodyFont));
                }
                foreach (var code in this._registrant.PromotionalCodes.Where(c => c.Code.Action == RSToolKit.Domain.Entities.ShoppingCartAction.Multiply))
                {
                    var workingTotal = runningTotal;
                    runningTotal *= code.Code.Amount;
                    table.AddCell(new Phrase(code.Code.Code, bodyFont));
                    table.AddCell(new Phrase("Percentage (" + ((1 - code.Code.Amount).ToString("p", this._registrant.Form.Culture)) + ")", bodyFont));
                    table.AddCell(new Phrase(Math.Round((workingTotal * (1 - code.Code.Amount))).ToString("c", this._registrant.Form.Culture), bodyFont));
                    table.AddCell(new Phrase(Math.Round(runningTotal).ToString("c", this._registrant.Form.Culture), bodyFont));
                }
                if (this._registrant.PromotionalCodes.Count > 0)
                {
                    table.AddCell(new Phrase(""));
                    table.AddCell(new Phrase(""));
                    table.AddCell(new Phrase("Less Discounts:", boldTableFont));
                    table.AddCell(new Phrase(Math.Round(runningTotal).ToString("c", this._registrant.Form.Culture), boldTableFont));
                }
                if (this._registrant.Form.Tax.HasValue)
                {
                    runningTotal += shoppingCart.Taxes();
                    table.AddCell(new Phrase(this._registrant.Form.TaxDescription, bodyFont));
                    table.AddCell(new Phrase(""));
                    table.AddCell(new Phrase(this._registrant.Form.Tax.Value.ToString("p", this._registrant.Form.Culture), bodyFont));
                    table.AddCell(new Phrase(Math.Round(shoppingCart.Taxes()).ToString("c", this._registrant.Form.Culture), bodyFont));

                    table.AddCell(new Phrase(""));
                    table.AddCell(new Phrase(""));
                    table.AddCell(new Phrase("Total:", boldTableFont));
                    table.AddCell(new Phrase(Math.Round(runningTotal).ToString("c", this._registrant.Form.Culture), boldTableFont));
                }
                foreach (var detail in this._registrant.Details())
                {
                    var charge = (detail.TransactionType == RSToolKit.Domain.Entities.MerchantAccount.TransactionType.AuthorizeCapture || detail.TransactionType == RSToolKit.Domain.Entities.MerchantAccount.TransactionType.Capture);
                    if (charge)
                    {
                        runningTotal -= detail.Ammount;
                    }
                    else
                    {
                        runningTotal += detail.Ammount;
                    }
                    table.AddCell(new Phrase(charge ? "Payment" : "Refund", bodyFont));
                    table.AddCell(new Phrase(detail.DateCreated.LocalDateTime.ToString(this._registrant.Form.Culture), bodyFont));
                    table.AddCell(new Phrase(""));
                    table.AddCell(new Phrase((charge ? "-" : "") + Math.Round(detail.Ammount).ToString("c", this._registrant.Form.Culture), bodyFont));
                }
                table.AddCell(new Phrase(""));
                table.AddCell(new Phrase(""));
                table.AddCell(new Phrase("Grand Total:", boldTableFont));
                table.AddCell(new Phrase(Math.Round(this._registrant.TotalOwed).ToString("c", this._registrant.Form.Culture), boldTableFont));

                document.Add(table);

                document.Close();
                binaryData = m_stream.ToArray();
            }

            return File(binaryData, "application/pdf", "invoice.pdf");
        }

        #endregion

        #region Cancel
        [HttpGet]
        [ComplexId("id")]
        public ActionResult Cancel(object id)
        {
            this._GetRegistrant(id);
            if (User.IsInRole("Super Administrators") || User.IsInRole("Administrators"))
                this._registrant.Status = RegistrationStatus.CanceledByAdministrator;
            else
                this._registrant.Status = RegistrationStatus.CanceledByCompany;
            this._registrant.StatusDate = DateTime.UtcNow;
            var view = new BaseRegisterView();
            this._FillRegisterView(view);
            Context.SaveChanges();
            return View("Cancel", view);
        }

        #endregion

        #endregion

        #region On[Methods]
        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
            ViewBag.Form = this._form;
            ViewBag.Registrant = this._registrant;
        }
        #endregion

        #region Context Manipulation
        /// <summary>
        /// Retrieves a form from the database.
        /// </summary>
        /// <param name="id">The id of the form as either a unique identifier or a long.</param>
        /// <returns>The form retrieved.</returns>
        protected Form _GetForm(object id)
        {
            if (id is Guid)
                this._form = this._formRepo.Find((Guid)id);
            if (id is long)
                this._form = this._formRepo.First(f => f.SortingId == (long)id);
            if (this._form != null)
                return this._form;
            throw new InvalidIdException("The id must be either a unique identifier or a 64 bit integer");
        }

        /// <summary>
        /// Retrieves a registrant from the database.
        /// It sets the protected _registrant variable and the protected _form.
        /// </summary>
        /// <param name="id">The id of the registrant as either a unique identifier or a long.</param>
        /// <returns>The registrant retrieved.</returns>
        protected Registrant _GetRegistrant(object id)
        {
            if (id is Guid)
                this._registrant = this._regRepo.Find((Guid)id);
            if (id is long)
                this._registrant = this._regRepo.First(r => r.SortingId == (long)id);
            if (this._registrant != null)
            {
                this._form = this._registrant.Form;
                return this._registrant;
            }
            throw new InvalidIdException("The id must be either a unique identifier or a 64 bit integer");
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Updates the registrant in the loaded table information.
        /// </summary>
        /// <param name="id">The unique identifier of the registrant.</param>
        protected void _UpdateRegistrantInTable(Guid id)
        {
            ThreadPool.QueueUserWorkItem(FormReportController.UpdateTableRegistrantCallback, id);
        }

        /// <summary>
        /// Gets the commands in memory that have previously been run or runs them.
        /// </summary>
        /// <param name="holder">The holder of the logic.</param>
        /// <param name="onLoad">If it should run on load logic or advancing logic. True by default.</param>
        /// <returns>The logic commands of the execution.</returns>
        protected IEnumerable<JLogicCommand> _ExecuteLogic(ILogicHolder holder, bool onLoad = true)
        {
            var commands = LogicEngine.RunLogic(holder, registrant: this._registrant, onLoad: onLoad, runCommands: false, allCommands: this._allCommands);
            return commands;
        }

        /// <summary>
        /// Fills the data into the base view.
        /// </summary>
        /// <param name="view">The view to fill.</param>
        protected void _FillRegisterView(BaseRegisterView view)
        {
            if (this._form == null)
                throw new FormNotFoundException("The form must be present to fill the registrant view");
            if (this._htmlHelper == null)
                this._FillHelper();
            view.Title = this._form.Name + " - Admin Registration";
            view.Header = this._htmlHelper.RenderHeader();
            view.HiddenInputs = this._htmlHelper.RenderHiddens();
            view.Footer = this._htmlHelper.RenderFooter();
            view.PageNumber = this._htmlHelper.RenderPageNumbers();
            view.ShoppingCart = this._htmlHelper.RenderShoppingCart();
            view.Styles = this._htmlHelper.RenderFormStyle();
            if (this._registrant != null)
                view.RegistrantId = this._registrant.SortingId;
        }

        /// <summary>
        /// Assigns the RegisterHtmlHelper.
        /// </summary>
        protected void _FillHelper()
        {
            this._htmlHelper = RegisterHtmlHelper.Create(this);
        }

        /// <summary>
        /// Gets the next page available to the registrant.
        /// </summary>
        /// <param name="currentPage">
        /// The current page number. It starts with the next page number for it's search.
        /// 0: start search at RSVP
        /// 1: start search at Audience
        /// 2: start search at Page 3 (User defined pages)
        /// </param>
        /// <returns></returns>
        protected Page _NextPage(int currentPage)
        {
            if (currentPage < 0)
                return null;
            Page nextPage = null;
            if (currentPage < 1)
                nextPage = this._form.Pages.FirstOrDefault(p => p.Type == PageType.RSVP && p.Enabled);
            if (currentPage < 2 && nextPage == null)
            {
                // Not using RSVP page so we will try Audience page.
                if (this._form.Audiences.Count > 0)
                    // We have available audiences so let's get the page.
                    nextPage = this._form.Pages.FirstOrDefault(p => p.Type == PageType.Audience && p.Enabled);
            }
            if (nextPage == null)
            {
                // Not using audience so we start searching pages.
                foreach (var page in this._form.Pages.Where(p => p.Type == PageType.UserDefined && p.Enabled && p.PageNumber > currentPage).OrderBy(p => p.PageNumber))
                {
                    // We see if rsvp mathces.
                    if (page.RSVP == RSVPType.Accept && this._registrant.RSVP)
                        nextPage = page;
                    if (page.RSVP == RSVPType.Decline && !this._registrant.RSVP)
                        nextPage = page;
                    if (page.RSVP == RSVPType.None)
                        nextPage = page;
                    if (nextPage == null)
                    {
                        // RSVP checks failed so we continue;
                        this._SetPageValuesBlank(nextPage);
                        continue;
                    }
                    // No we check audiences
                    if (nextPage.Audiences.Count > 0 && (this._registrant.Audience == null || !nextPage.Audiences.Contains(this._registrant.Audience)))
                    {
                        // The registrant isn't in the right audience. We continue;
                        this._SetPageValuesBlank(nextPage);
                        continue;
                    }
                    // The page is rsvp compatible so we check if the page is blank.
                    if (this._htmlHelper.IsBlank(nextPage))
                    // The page is blank so we null out nextPage.
                    {
                        this._SetPageValuesBlank(page);
                        nextPage = null;
                        continue;
                    }
                    if (nextPage != null)
                        // We found the page so we exit the loop.
                        break;
                    // We didn't find the page so the loop continues;
                }
            }
            if (nextPage != null && this._htmlHelper != null)
            {
                this._htmlHelper.SetPage(nextPage.PageNumber);
                this._pageNumber = nextPage.PageNumber;
            }
            this._page = nextPage;
            return nextPage;
        }

        /// <summary>
        /// Gets the previous page that is valid for the registrant.
        /// </summary>
        /// <param name="page">The page number to start from.</param>
        /// <returns></returns>
        protected int _BackPage(int page)
        {
            if (page == PageNumber.CheckOut)
            {
                // The previous page would be either promotions or shopping cart.
                var paymentSkipped = !this._form.DisableShoppingCart ||
                    this._registrant.TotalOwed <= 0 ||
                    this._registrant.Data.Any(d => d.Component.Variable.Value == "__SkipPayment" && d.Value == "true");
                if (paymentSkipped)
                    // payment was skipped so we return the review page number.
                    return PageNumber.Review;
                // Payments are not being skipped so we return the shopping cart page number.
                var noPromo = this._registrant.Data.Any(d => d.Component.Variable.Value == "__NoPromo" && d.Value == "true") ||
                    this._form.PromotionalCodes.Count == 0;
                if (noPromo)
                    // Promotions are not being used. So we returnt he review page.
                    return PageNumber.Review;
                return PageNumber.Promotions;
            }
            if (page == PageNumber.Promotions)
                    return PageNumber.Review;
            var pages = this._form.Pages.Where(p => p.Type == PageType.UserDefined && p.PageNumber < page).OrderByDescending(p => p.PageNumber);
            if (page == PageNumber.Review)
                // We set the page number to the lasat page number
                page = this._form.Pages.Where(p => p.Type == PageType.UserDefined).OrderByDescending(p => p.PageNumber).First().PageNumber;
            if (page > 3)
            {
                foreach (var p in pages)
                {
                    Page nextPage = null;
                    // We see if rsvp mathces.
                    if (p.RSVP == RSVPType.Accept && this._registrant.RSVP)
                        nextPage = p;
                    if (p.RSVP == RSVPType.Decline && !this._registrant.RSVP)
                        nextPage = p;
                    if (p.RSVP == RSVPType.None)
                        nextPage = p;
                    if (nextPage == null)
                        continue;
                    // No we check audiences
                    if (nextPage.Audiences.Count > 0 && (this._registrant.Audience == null || !nextPage.Audiences.Contains(this._registrant.Audience)))
                        continue;
                    // The page is rsvp compatible so we check if the page is blank.
                    if (this._htmlHelper.IsBlank(nextPage))
                        // The page is blank so we null out nextPage.
                        nextPage = null;
                    if (nextPage != null)
                        // We found the page so we exit the loop.
                        return p.PageNumber;
                    // We didn't find the page so the loop continues;
                }
            }
            // Now we try the audience page.
            if (page > 2 && this._form.Audiences.Count > 0 && this._form.Pages.Any(p => p.Type == PageType.Audience && p.Enabled && (p.RSVP == RSVPType.None || (p.RSVP == RSVPType.Accept && this._registrant.RSVP) || (p.RSVP == RSVPType.Decline && !this._registrant.RSVP))))
                return 2;
            if (page > 1 && this._form.Pages.Any(p => p.Type == PageType.RSVP && p.Enabled))
                return 1;
            return 0;
        }

        /// <summary>
        /// Sets page values to blank.
        /// </summary>
        /// <param name="page">The page to blank out.</param>
        protected void _SetPageValuesBlank(Page page)
        {
            if (page.AdminOnly)
                return;
            foreach (var panel in page.Panels)
            {
                if (panel.AdminOnly)
                    continue;
                foreach (var component in panel.Components)
                {
                    if (component.AdminOnly || component is FreeText || component is CheckboxItem || component is RadioItem || component is DropdownItem)
                        continue;
                    var data = this._registrant.Data.FirstOrDefault(d => d.VariableUId == component.UId);
                    if (data == null)
                    {
                        data = new RegistrantData()
                        {
                            VariableUId = component.UId,
                            Component = component
                        };
                        this._registrant.Data.Add(data);
                    }
                    data.Value = null;
                }
            }
            Context.SaveChanges();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (this._registrant != null)
                this._UpdateRegistrantInTable(this._registrant.UId);
            base.Dispose(disposing);
        }

    }

}