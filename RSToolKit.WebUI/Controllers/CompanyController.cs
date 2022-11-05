using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.WebUI.Models.Views.Company;
using RSToolKit.WebUI.Models.Inputs.Company;
using RSToolKit.WebUI.Infrastructure.Json;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles = "Company Adminstrators,Super Administrators,System Administrators")]
    public class CompanyController
        : RegStepController
    {
        #region Fields and Properties
        protected Company _company;
        protected CompanyRepository _compRepo;

        protected readonly Dictionary<string, string[]> _allowedTypes = new Dictionary<string, string[]>()
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
            }
        };

        #endregion

        #region Construction
        public CompanyController(EFDbContext context)
            : base(context)
        {
            this._company = null;
            this._compRepo = new CompanyRepository(Context);
        }

        public CompanyController()
        {
            this._company = null;
            this._compRepo = new CompanyRepository(Context);
        }
        #endregion

        #region Actions
        [ComplexId("id")]
        [HttpGet]
        public ActionResult Index(object id)
        {
            return Get(id);
        }

        [ComplexId("id")]
        [HttpGet]
        public ActionResult Get(object id = null)
        {
            if (id == null)
                return List();
            this._SetCompany(id);
            var view = new CompanyView()
            {
                Id = this._company.UId,
                Name = this._company.Name,
                BillingAddressLine1 = this._company.BillingAddressLine1,
                BillingAddressLine2 = this._company.BillingAddressLine2,
                BillingCity = this._company.BillingCity,
                BillingState = this._company.BillingState,
                BillingCountry = this._company.BillingCountry,
                BillingZipCode = this._company.BillingZip,
                ShippingAddressLine1 = this._company.ShippingAddressLine1,
                ShippingAddressLine2 = this._company.ShippingAddressLine2,
                ShippingCity = this._company.ShippingCity,
                ShippingCountry = this._company.ShippingCountry,
                ShippingState = this._company.ShippingState,
                ShippingZipCode = this._company.ShippingZip,
                InvoiceAddressLine1 = this._company.RegistrationAddressLine1,
                InvoiceAddressLine2 = this._company.RegistrationAddressLine2,
                InvoiceCity = this._company.RegistrationCity,
                InvoiceCountry = this._company.RegistrationCountry,
                InvoiceEmail = this._company.RegistrationEmail,
                InvoiceFax = this._company.RegistrationFax,
                InvoicePhone = this._company.RegistrationPhone,
                InvoiceState = this._company.RegistrationState,
                InvoiceZipCode = this._company.RegistrationZip,
                Title = this._company.Name,
                LogoUploaded = this._company.Logo.Count > 0
            };
            return View("Company", view);
        }

        [HttpGet]
        public ActionResult List()
        {
            var companies = this._compRepo.GetAll();
            var view = new CompaniesView();
            view.Companies = companies.Select(c => new CompaniesView_CompanyView() { Id = c.SortingId, Name = c.Name }).OrderBy(c => c.Name).ToList();
            return View("Companies", view);
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Update(CompanyInput input)
        {
            this._SetCompany(input.Id);
            CompanyLogo logo = this._company.Logo.FirstOrDefault();
            byte[] logoBinary = null;
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file.ContentLength > 0)
                {
                    if (file.ContentLength > 2097152)
                    {
                        ModelState.AddModelError("Logo", "The logo cannot exceed 2 MB");
                    }
                    else if (!this._allowedTypes["picture"].Contains(file.ContentType))
                    {
                        ModelState.AddModelError("Logo", "The logo must be of type jpg, bmp, gif, tiff, png");
                    }
                    else
                    {
                        // If we hit here, the file is valid.
                        logoBinary = new byte[file.ContentLength];
                        file.InputStream.Read(logoBinary, 0, file.ContentLength);
                        if (logo == null)
                        {
                            // There was no company logo, we create it now.
                            logo = new CompanyLogo()
                            {
                                UId = Guid.NewGuid(),
                                Company = this._company
                            };
                            this._company.Logo.Add(logo);
                            Context.Logos.Add(logo);
                        }
                        logo.BinaryData = logoBinary;
                        logo.MIME = file.ContentType;
                        logo.SizeInBytes = file.ContentLength;
                        // Now we need to convert it to a image so we can gather more information.
                        using (var m_stream = new MemoryStream(logoBinary))
                        {
                            var img = Image.FromStream(m_stream);
                            logo.Width = img.Width;
                            logo.Height = img.Height;
                        }
                    }
                }
            }
            if (!ModelState.IsValid)
            {
                // The model state is invalid, so we return the errors.
                return JsonNetResult.Failure(ModelState, Url.Action("Get", "Company", new { id = this._company.SortingId }), "The operation failed.");
            }
            this._company.Name = input.Name;
            this._company.BillingAddressLine1 = input.BillingAddressLine1;
            this._company.BillingAddressLine2 = input.BillingAddressLine2;
            this._company.BillingCity = input.BillingCity;
            this._company.BillingState = input.BillingState;
            this._company.BillingCountry = input.BillingCountry;
            this._company.BillingZip = input.BillingZipCode;
            this._company.ShippingAddressLine1 = input.ShippingAddressLine1;
            this._company.ShippingAddressLine2 = input.ShippingAddressLine2;
            this._company.ShippingCity = input.ShippingCity;
            this._company.ShippingCountry = input.ShippingCountry;
            this._company.ShippingState = input.ShippingState;
            this._company.ShippingZip = input.ShippingZipCode;
            this._company.RegistrationAddressLine1 = input.InvoiceAddressLine1;
            this._company.RegistrationAddressLine2 = input.InvoiceAddressLine2;
            this._company.RegistrationCity = input.InvoiceCity;
            this._company.RegistrationCountry = input.InvoiceCountry;
            this._company.RegistrationEmail = input.InvoiceEmail;
            this._company.RegistrationFax = input.InvoiceFax;
            this._company.RegistrationPhone = input.InvoicePhone;
            this._company.RegistrationState = input.InvoiceState;
            this._company.RegistrationZip = input.InvoiceZipCode;
            this._compRepo.Commit();
            return JsonNetResult.Success(location: Url.Action("Get", "Company", new { id = this._company.SortingId }));
        }

        [HttpDelete]
        [JsonValidateAntiForgeryToken]
        [ComplexId("id")]
        public JsonNetResult Delete(object id)
        {
            this._SetCompany(id);
            var success = this._compRepo.Remove(this._company);
            if (success)
            {
                if (this._compRepo.Commit() > 0)
                    return JsonNetResult.Success(location: Url.Action("List", "Company"));
                return JsonNetResult.Failure(location: Url.Action("Get", "Company", new { id = this._company.SortingId }), message: "The operation failed to complete in the database.", errors: null);
            }
            return JsonNetResult.Failure(location: Url.Action("Get", "Company", new { id = this._company.SortingId }), message: "The operation failed at the repository level.", errors: null);
        }

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        [Authorize(Roles = "Super Administrators,System Administrators")]
        public JsonNetResult New(string name = null)
        {
            name = name ?? "New Company: " + DateTime.Now.ToString("YYYY-MM-DD");
            this._company = new Company()
            {
                Name = name,
                ContactLimit = 1000,
                UId = Guid.NewGuid()
            };
            foreach (var style in Context.DefaultFormStyles.Where(s => s.CompanyKey == null))
            {
                var item = new Domain.Entities.DefaultFormStyle()
                {
                    GroupName = style.GroupName,
                    Name = style.Name,
                    Sort = style.Sort,
                    SubSort = style.SubSort,
                    Type = style.Type,
                    Value = style.Value,
                    Variable = style.Variable,
                    CompanyKey = this._company.UId
                };
                this._company.FormStyles.Add(item);
            }
            this._compRepo.Add(this._company);
            if (this._compRepo.Commit() > 0)
                return JsonNetResult.Success(location: Url.Action("Get", "Company", new { id = this._company.SortingId }));
            return JsonNetResult.Failure(location: Url.Action("List", "Company"), message: "The company could not be added.", errors: null);
        }
        #endregion

        #region Logo
        [HttpGet]
        [ComplexId("id")]
        public FileContentResult Logo(object id)
        {
            this._SetCompany(id);
            var logo = this._company.Logo.FirstOrDefault();
            if (logo == null)
                return new FileContentResult(new byte[0], "image/jpg");
            return new FileContentResult(logo.BinaryData, logo.MIME);
        }

        [HttpGet]
        [ComplexId("id")]
        public FileContentResult LogoThumbnail(object id, int? width = null, int? height = null)
        {
            this._SetCompany(id);
            if (!width.HasValue)
                width = 100;
            var binaryData = new byte[0];
            var logo = this._company.Logo.FirstOrDefault();
            if (logo == null)
                return new FileContentResult(binaryData, "image/jpg");
            using (var m_stream = new MemoryStream(logo.BinaryData))
            using (var img_stream = new MemoryStream())
            {
                Image img = Image.FromStream(m_stream);
                Image thumbNail = img;
                if (width.HasValue && width.Value < img.Width)
                {
                    var callback = new Image.GetThumbnailImageAbort(this._ThumbnailCallback);
                    if (!height.HasValue)
                        height = (width.Value * img.Height) / img.Width;
                    thumbNail = img.GetThumbnailImage(width.Value, height.Value, callback, new IntPtr());
                }
                var encoder = _GetEncoderInfo(logo.MIME);
                var encoderQuality = System.Drawing.Imaging.Encoder.Quality;
                if (encoder == null)
                    return new FileContentResult(binaryData, "image/jpg");
                var encoderParams = new EncoderParameters(1);
                var encoderParam = new EncoderParameter(encoderQuality, 25L);
                encoderParams.Param[0] = encoderParam;
                thumbNail.Save(img_stream, encoder, encoderParams);
                binaryData = img_stream.ToArray();
            }
            return new FileContentResult(binaryData, logo.MIME);
        }

        #endregion

        #region Helpers
        /// <summary>
        /// Sets the company being used in this controller.
        /// </summary>
        /// <param name="id">
        /// The id of the company.
        /// It can be a <code>Guid</code> or <code>long</code>.
        /// </param>
        /// <returns></returns>
        protected Company _SetCompany(object id)
        {
            if (id is Guid)
                this._company = this._compRepo.Find((Guid)id);
            else if (id is long)
                this._company = this._compRepo.First(c => c.SortingId == (long)id);
            else
                throw new InvalidIdException();
            return this._company;
        }

        protected bool _ThumbnailCallback() { return true; }

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

        #endregion
    }
}