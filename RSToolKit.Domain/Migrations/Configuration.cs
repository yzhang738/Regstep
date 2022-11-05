namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using RSToolKit.Domain.Entities;
    using RSToolKit.Domain.Entities.Clients;
    using RSToolKit.Domain.Entities.Email;
    using System.Collections;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.IO;

    internal sealed class Configuration : DbMigrationsConfiguration<RSToolKit.Domain.Data.EFDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(RSToolKit.Domain.Data.EFDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //


            var company = new Company()
            {
                UId = Guid.Parse("{27e170e9-80bf-4292-82e3-b8fa348352e3}"),
                ContactLimit = 1000000000000,
                Description = "System Owner",
                Name = "RegStep Technologies",
            };

            var folder = new Folder()
            {
                UId = Guid.NewGuid(),
                CompanyKey = company.UId,
                ParentKey = null,
                Name = company.Name
            };

            var sServ = new SmtpServer()
            {
                UId = Guid.NewGuid(),
                Name = "Primary",
                Description = "SendGrid",
                Port = 587,
                SSL = false,
                Address = "smtp.sendgrid.net",
                Username = "admin@regstep.com",
                Primary = true,
                CompanyKey = company.UId
            };

            sServ.CompanyKey = company.UId;
            sServ.EncryptPassword("RS1q2w3e4r");
            if (context.SmtpServers.FirstOrDefault(s => s.Name == "Primary" && s.CompanyKey == company.UId) == null)
                context.SmtpServers.Add(sServ);
            if (context.Companies.FirstOrDefault(c => c.UId == company.UId) == null)
                context.Companies.Add(company);
            if (context.Folders.FirstOrDefault(f => f.CompanyKey == folder.CompanyKey) == null)
                context.Folders.Add(folder);

            if (context.DefaultFormStyles.Where(c => c.CompanyKey == null).Count() == 0)
            {
                #region FormStyles

                #region body

                var b1s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "body",
                    Variable = "font-family",
                    Value = "sans-serif",
                    Name = "Font",
                    Sort = "Default Fonts",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(b1s1);
                var b1s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "body",
                    Variable = "color",
                    Value = "#3e3e3e",
                    Name = "Font Color",
                    Sort = "Default Fonts",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(b1s2);
                var b1s2a = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "body",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Default Fonts",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(b1s2a);
                var b1s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "body",
                    Variable = "background-color",
                    Value = "#cccccc",
                    Name = "Background Color",
                    Sort = "Page Background",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(b1s3);
                var b1s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "body",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background Image",
                    Sort = "Page Background",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(b1s4);
                var b1s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "body",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "Page Background",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(b1s5);
                var b1s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "body",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "Page Background",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(b1s6);
                var b1s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "body",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "Page Background",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(b1s7);

                #endregion
                #region .form-wrapper

                var f1s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-wrapper",
                    Variable = "background-color",
                    Value = "#ffffff",
                    Name = "Background Color",
                    Sort = "Form",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f1s1);
                var f1s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-wrapper",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background-image",
                    Sort = "Form",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f1s2);
                var f1s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-wrapper",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "Form",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f1s3);
                var f1s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-wrapper",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "Form",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f1s4);
                var f1s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-wrapper",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "Form",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(f1s5);
                var f1s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-wrapper",
                    Variable = "border-top",
                    Value = "none",
                    Name = "Border Top",
                    Sort = "Form",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f1s6);
                var f1s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-wrapper",
                    Variable = "border-bottom",
                    Value = "none",
                    Name = "Border Bottom",
                    Sort = "Form",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f1s7);
                var f1s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-wrapper",
                    Variable = "border-left",
                    Value = "none",
                    Name = "Border Left",
                    Sort = "Form",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f1s8);
                var f1s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-wrapper",
                    Variable = "border-right",
                    Value = "none",
                    Name = "Border Right",
                    Sort = "Form",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f1s9);
                var f1s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-wrapper",
                    Variable = "border-radius",
                    Value = "4px",
                    Name = "Border Radius",
                    Sort = "Form",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f1s10);
                var f1s11 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-wrapper",
                    Variable = "margin-top",
                    Value = "15px",
                    Name = "Margin Top",
                    Sort = "Form",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f1s11);

                #endregion
                #region .form-header

                var f2s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-header",
                    Variable = "background-color",
                    Value = "#ffffff",
                    Name = "Background Color",
                    Sort = "Header",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f2s1);
                var f2s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-header",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background-image",
                    Sort = "Header",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f2s2);
                var f2s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-header",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "Header",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f2s3);
                var f2s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-header",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "Header",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f2s4);
                var f2s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-header",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "Header",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(f2s5);
                var f2s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-header",
                    Variable = "border-top",
                    Value = "none",
                    Name = "Border Top",
                    Sort = "Header",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f2s6);
                var f2s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-header",
                    Variable = "border-bottom",
                    Value = "none",
                    Name = "Border Bottom",
                    Sort = "Header",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f2s7);
                var f2s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-header",
                    Variable = "border-left",
                    Value = "none",
                    Name = "Border Left",
                    Sort = "Header",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f2s8);
                var f2s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-header",
                    Variable = "border-right",
                    Value = "none",
                    Name = "Border Right",
                    Sort = "Header",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f2s9);
                var f2s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-header",
                    Variable = "border-top-left-radius",
                    Value = ".form-wrapper_border-radius",
                    Name = "Border Top Left Radius",
                    Sort = null,
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f2s10);
                var f2s11 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-header",
                    Variable = "border-top-right-radius",
                    Value = ".form-wrapper_border-radius",
                    Name = "Border Top Right Radius",
                    Sort = null,
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f2s11);

                #endregion
                #region .form-info-bar

                var f3s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-info-bar",
                    Variable = "background-color",
                    Value = "transparent",
                    Name = "Background Color",
                    Sort = "Info Bar",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f3s1);
                var f3s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-info-bar",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background-image",
                    Sort = "Info Bar",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f3s2);
                var f3s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-info-bar",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "Info Bar",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f3s3);
                var f3s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-info-bar",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "Info Bar",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f3s4);
                var f3s4a = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-info-bar",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "Info Bar",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(f3s4a);

                var f3s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-info-bar",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "Header",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(f3s5);
                var f3s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-info-bar",
                    Variable = "border-top",
                    Value = "none",
                    Name = "Border Top",
                    Sort = "Info Bar",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f3s6);
                var f3s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-info-bar",
                    Variable = "border-bottom",
                    Value = "1px solid #dddddd",
                    Name = "Border Bottom",
                    Sort = "Info Bar",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f3s7);
                var f3s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-info-bar",
                    Variable = "border-left",
                    Value = "none",
                    Name = "Border Left",
                    Sort = "Info Bar",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f3s8);
                var f3s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-info-bar",
                    Variable = "border-right",
                    Value = "none",
                    Name = "Border Right",
                    Sort = "Info Bar",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f3s9);
                var f3s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-info-bar",
                    Variable = "color",
                    Value = "#000000",
                    Name = "Font Color",
                    Sort = "Info Bar",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f3s10);


                #endregion
                #region .form-content-area

                var f4s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-content-area",
                    Variable = "background-color",
                    Value = "transparent",
                    Name = "Background Color",
                    Sort = "Content Area",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f4s1);
                var f4s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-content-area",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background-image",
                    Sort = "Content Area",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f4s2);
                var f4s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-content-area",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "Content Area",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f4s3);
                var f4s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-content-area",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "Content Area",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f4s4);
                var f4s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-content-area",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "Content Area",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(f4s5);
                var f4s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-content-area",
                    Variable = "border-top",
                    Value = "none",
                    Name = "Border Top",
                    Sort = "Content Area",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f4s6);
                var f4s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-content-area",
                    Variable = "border-bottom",
                    Value = "none",
                    Name = "Border Bottom",
                    Sort = "Content Area",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f4s7);
                var f4s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-content-area",
                    Variable = "border-left",
                    Value = "none",
                    Name = "Border Left",
                    Sort = "Content Area",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f4s8);
                var f4s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-content-area",
                    Variable = "border-right",
                    Value = "none",
                    Name = "Border Right",
                    Sort = "Content Area",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f4s9);

                #endregion
                #region .form-footer

                var f5s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-footer",
                    Variable = "background-color",
                    Value = "transparent",
                    Name = "Background Color",
                    Sort = "Footer",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f5s1);
                var f5s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-footer",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background-image",
                    Sort = "Footer",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f5s2);
                var f5s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-footer",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "Footer",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f5s3);
                var f5s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-footer",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "Footer",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f5s4);
                var f5s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-footer",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "Footer",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(f5s5);
                var f5s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-footer",
                    Variable = "border-top",
                    Value = "none",
                    Name = "Border Top",
                    Sort = "Footer",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f5s6);
                var f5s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-footer",
                    Variable = "border-bottom",
                    Value = "none",
                    Name = "Border Bottom",
                    Sort = "Footer",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f5s7);
                var f5s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-footer",
                    Variable = "border-left",
                    Value = "none",
                    Name = "Border Left",
                    Sort = "Footer",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f5s8);
                var f5s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-footer",
                    Variable = "border-right",
                    Value = "none",
                    Name = "Border Right",
                    Sort = "Footer",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f5s9);
                var f5s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-footer",
                    Variable = "border-bottom-left-radius",
                    Value = ".form-wrapper_border-radius",
                    Name = "Border Bottom Left Radius",
                    Sort = null,
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f5s10);
                var f5s11 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-footer",
                    Variable = "border-bottom-right-radius",
                    Value = ".form-wrapper_border-radius",
                    Name = "Border Bottom Right Radius",
                    Sort = null,
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f5s11);

                #endregion
                #region .form-error-message

                var f6s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-error",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font",
                    Sort = "Error Message",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f6s1);
                var f6s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-error",
                    Variable = "color",
                    Value = "#ffffff",
                    Name = "Font Color",
                    Sort = "Error Message",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f6s2);
                var f6s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-error",
                    Variable = "font-size",
                    Value = "12px",
                    Name = "Font Size",
                    Sort = "Error Message",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f6s3);
                var f6s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-error",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Error Message",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f6s4);
                var f6s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-error",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Error Message",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f6s5);


                #endregion
                #region .form-error-warning

                var f7s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-error-warning",
                    Variable = "background-color",
                    Value = "#ff0000",
                    Name = "Background Color",
                    Sort = "Error Warning",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f7s1);
                var f7s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-error-warning",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Error Warning",
                    Type = "font-family"
                };
                context.DefaultFormStyles.Add(f7s7);
                var f7s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-error-warning",
                    Variable = "color",
                    Value = "#ffffff",
                    Name = "Font Color",
                    Sort = "Error Warning",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f7s8);
                var f7s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-error-warning",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Error Warning",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f7s9);
                var f7s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-error-warning",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Error Warning",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f7s10);
                var f7s11 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-error-warning",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Error Warning",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f7s11);

                #endregion
                #region .form-panel

                var f8s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "background-color",
                    Value = "transparent",
                    Name = "Background Color",
                    Sort = "Panels",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f8s1);
                var f8s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background Image",
                    Sort = "Panels",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f8s2);
                var f8s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "background-position",
                    Value = "center center",
                    Name = "Background Position",
                    Sort = "Panels",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f8s3);
                var f8s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "background-repeat",
                    Value = "no-repeat",
                    Name = "Background Repeat",
                    Sort = "Panels",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f8s4);
                var f8s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "border-top",
                    Value = "none",
                    Name = "Border Top",
                    Sort = "Panels",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f8s5);
                var f8s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "border-right",
                    Value = "none",
                    Name = "Border Right",
                    Sort = "Panels",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f8s6);
                var f8s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "border-bottom",
                    Value = "none",
                    Name = "Border Bottom",
                    Sort = "Panels",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f8s7);
                var f8s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "border-left",
                    Value = "none",
                    Name = "Border Left",
                    Sort = "Panels",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f8s8);
                var f8s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "margin-top",
                    Value = "15px",
                    Name = "Margin Top",
                    Sort = "Panels",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f8s9);
                var f8s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "margin-bottom",
                    Value = "0",
                    Name = "Margin Bottom",
                    Sort = "Panels",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f8s10);
                var f8s11 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "padding-top",
                    Value = "30px",
                    Name = "Padding Top",
                    Sort = "Panels",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f8s11);
                var f8s12 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "padding-bottom",
                    Value = "0",
                    Name = "Padding Bottom",
                    Sort = "Panels",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f8s12);
                var f8s13 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "border-radius",
                    Value = "4px",
                    Name = "Border Radius",
                    Sort = "Panels",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f8s13);
                var f8s14 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "border-top",
                    Value = null,
                    Name = "Top Border",
                    Sort = "Panels",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f8s14);
                var f8s15 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "border-right",
                    Value = null,
                    Name = "Right Border",
                    Sort = "Panels",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f8s15);
                var f8s16 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "border-bottom",
                    Value = null,
                    Name = "Bottom Border",
                    Sort = "Panels",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f8s16);
                var f8s17 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-panel",
                    Variable = "border-left",
                    Value = null,
                    Name = "Left Border",
                    Sort = "Panels",
                    Type = "single-border"
                };
                context.DefaultFormStyles.Add(f8s17);

                #endregion
                #region .form-component

                var f9s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font",
                    Sort = "Components",
                    SubSort = "All Component Elements",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f9s1);
                var f9s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Components",
                    SubSort = "All Component Elements",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f9s2);
                var f9s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Components",
                    SubSort = "All Component Elements",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f9s3);
                var f9s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Components",
                    SubSort = "All Component Elements",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f9s4);
                var f9s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Components",
                    SubSort = "All Component Elements",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f9s5);

                #endregion
                #region .form-component.required-component

                var f10s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component.required-component",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font",
                    Sort = "Components - Required",
                    SubSort = "All Component Elements",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f10s1);
                var f10s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component.required-component",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Components - Required",
                    SubSort = "All Component Elements",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f10s2);
                var f10s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component.required-component",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Components - Required",
                    SubSort = "All Component Elements",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f10s3);
                var f10s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component.required-component",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Components - Required",
                    SubSort = "All Component Elements",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f10s4);
                var f10s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component.required-component",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Components - Required",
                    SubSort = "All Component Elements",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f10s5);

                #endregion
                #region .form-component-label

                var f11s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-label",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font",
                    Sort = "Components",
                    SubSort = "Label",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f11s1);
                var f11s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-label",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Components",
                    SubSort = "Label",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f11s2);
                var f11s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-label",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Components",
                    SubSort = "Label",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f11s3);
                var f11s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-label",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Components",
                    SubSort = "Label",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f11s4);
                var f11s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-label",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Components",
                    SubSort = "Label",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f11s5);

                #endregion
                #region .required-component .form-component-label

                var f12s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-component .form-component-label",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font",
                    Sort = "Components - Required",
                    SubSort = "Component Label",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f12s1);
                var f12s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-component .form-component-label",
                    Variable = "color",
                    Value = "#ff0000",
                    Name = "Font Color",
                    Sort = "Components - Required",
                    SubSort = "Component Label",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f12s2);
                var f12s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-component .form-component-label",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Components - Required",
                    SubSort = "Component Label",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f12s3);
                var f12s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-component .form-component-label",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Components - Required",
                    SubSort = "Component Label",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f12s4);
                var f12s = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-component .form-component-label",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Components - Required",
                    SubSort = "Component Label",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f12s);

                #endregion
                #region .form-component-description

                var f13s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-description",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Components",
                    SubSort = "Component Description",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f13s1);
                var f13s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-description",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Components",
                    SubSort = "Component Description",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f13s2);
                var f13s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-description",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Components",
                    SubSort = "Component Description",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f13s3);
                var f13s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-description",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Components",
                    SubSort = "Component Description",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f13s4);
                var f13s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-component-description",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Components",
                    SubSort = "Component Description",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f13s5);

                #endregion
                #region .required-component .form-component-description

                var f14s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-component .form-component-description",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font",
                    Sort = "Components - Required",
                    SubSort = "Component Description",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f14s1);
                var f14s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-component .form-component-description",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Components - Required",
                    SubSort = "Component Description",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f14s2);
                var f14s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-component .form-component-description",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Components - Required",
                    SubSort = "Component Description",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f14s3);
                var f14s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-component .form-component-description",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Components - Required",
                    SubSort = "Component Description",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f14s4);
                var f14s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-component .form-component-description",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Components - Required",
                    SubSort = "Component Description",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f14s5);

                #endregion
                #region .form-item

                var f15s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Items",
                    SubSort = "All Item Elements",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f15s1);
                var f15s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Items",
                    SubSort = "All Item Elements",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f15s2);
                var f15s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Items",
                    SubSort = "All Item Elements",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f15s3);
                var f15s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Items",
                    SubSort = "All Item Elements",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f15s4);
                var f15s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Items",
                    SubSort = "All Item Elements",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f15s5);

                #endregion
                #region .form-item.required-item

                var f16s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item.required-item",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font",
                    Sort = "Items - Required",
                    SubSort = "All Item Elements",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f16s1);
                var f16s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item.required-item",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Items - Required",
                    SubSort = "All Item Elements",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f16s2);
                var f16s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item.required-item",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Items - Required",
                    SubSort = "All Item Elements",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f16s3);
                var f16s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item.required-item",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Items - Required",
                    SubSort = "All Item Elements",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f16s4);
                var f16s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item.required-item",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Items - Required",
                    SubSort = "All Item Elements",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f16s5);

                #endregion
                #region .form-item-label

                var f17s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item-label",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font",
                    Sort = "Items",
                    SubSort = "Label",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f17s1);
                var f17s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item-label",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Items",
                    SubSort = "Label",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f17s2);
                var f17s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item-label",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Items",
                    SubSort = "Label",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f17s3);
                var f17s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item-label",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Items",
                    SubSort = "Label",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f17s4);
                var f17s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item-label",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Items",
                    SubSort = "Label",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f17s5);

                #endregion
                #region .required-item .form-item-label

                var f18s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-item .form-item-label",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font",
                    Sort = "Items - Required",
                    SubSort = "Label",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f18s1);
                var f18s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-item .form-item-label",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Items - Required",
                    SubSort = "Label",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f18s2);
                var f18s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-item .form-item-label",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Items - Required",
                    SubSort = "Label",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f18s3);
                var f18s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-item .form-item-label",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Items - Required",
                    SubSort = "Label",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f18s4);
                var f18s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-item .form-item-label",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Items - Required",
                    SubSort = "Label",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f18s5);

                #endregion
                #region .form-item-description

                var f19s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item-description",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font",
                    Sort = "Items",
                    SubSort = "Description",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f19s1);
                var f19s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item-description",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Items",
                    SubSort = "Description",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f19s2);
                var f19s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item-description",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Items",
                    SubSort = "Description",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f19s3);
                var f19s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item-description",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Items",
                    SubSort = "Description",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f19s4);
                var f19s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-item-description",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Items",
                    SubSort = "Description",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f19s5);

                #endregion
                #region .required-item .form-item-description

                var f20s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-item .form-item-description",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font",
                    Sort = "Items - Required",
                    SubSort = "Description",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f20s1);
                var f20s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-item .form-item-description",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Items - Required",
                    SubSort = "Description",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f20s2);
                var f20s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-item .form-item-description",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Items - Required",
                    SubSort = "Description",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f20s3);
                var f20s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-item .form-item-description",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Items - Required",
                    SubSort = "Description",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f20s4);
                var f20s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".required-item .form-item-description",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Items - Required",
                    SubSort = "Description",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f20s5);

                #endregion
                #region .form-price

                var f21s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-price",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Price",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f21s1);
                var f21s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-price",
                    Variable = "color",
                    Value = "#0000ff",
                    Name = "Font Color",
                    Sort = "Price",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f21s2);
                var f21s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-price",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Price",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f21s3);
                var f21s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-price",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Price",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f21s4);
                var f21s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-price",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Price",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f21s5);

                #endregion
                #region .form-agenda

                var f22s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-agenda",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Agenda",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f22s1);
                var f22s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-agenda",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Agenda",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f22s2);
                var f22s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-agenda",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Agenda",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f22s3);
                var f22s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-agenda",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Agenda",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f22s4);
                var f22s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-agenda",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Agenda",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f22s5);

                #endregion
                #region .form-at-capacity

                var f23s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-at-capacity",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "At Capacity",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f23s1);
                var f23s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-at-capacity",
                    Variable = "color",
                    Value = "#ff6600",
                    Name = "Font Color",
                    Sort = "At Capacity",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f23s2);
                var f23s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-at-capacity",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "At Capacity",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f23s3);
                var f23s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-at-capacity",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "At Capacity",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f23s4);
                var f23s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = ".form-at-capacity",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "At Capacity",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f23s5);

                #endregion
                #region h1

                var f25s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h1",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Header 1",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f25s1);
                var f25s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h1",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Header 1",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f25s2);
                var f25s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h1",
                    Variable = "font-size",
                    Value = "24px",
                    Name = "Font Size",
                    Sort = "Header 1",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f25s3);
                var f25s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h1",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Header 1",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f25s4);
                var f25s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h1",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Header 1",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f25s5);
                var f25s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h1",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "Header 1",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f25s6);
                var f25s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h1",
                    Variable = "margin-top",
                    Value = "0",
                    Name = "Margin Top",
                    Sort = "Header 1",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f25s7);
                var f25s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h1",
                    Variable = "margin-bottom",
                    Value = "0",
                    Name = "Margin Bottom",
                    Sort = "Header 1",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f25s8);
                var f25s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h1",
                    Variable = "padding-top",
                    Value = "0",
                    Name = "Padding Top",
                    Sort = "Header 1",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f25s9);
                var f25s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h1",
                    Variable = "padding-bottom",
                    Value = "5px",
                    Name = "Padding Bottom",
                    Sort = "Header 1",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f25s10);

                #endregion
                #region h2

                var f26s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h2",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Header 2",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f26s1);
                var f26s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h2",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Header 2",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f26s2);
                var f26s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h2",
                    Variable = "font-size",
                    Value = "22px",
                    Name = "Font Size",
                    Sort = "Header 2",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f26s3);
                var f26s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h2",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Header 2",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f26s4);
                var f26s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h2",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Header 2",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f26s5);
                var f26s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h2",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "Header 2",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f26s6);
                var f26s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h2",
                    Variable = "margin-top",
                    Value = "0",
                    Name = "Margin Top",
                    Sort = "Header 2",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f26s7);
                var f26s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h2",
                    Variable = "margin-bottom",
                    Value = "0",
                    Name = "Margin Bottom",
                    Sort = "Header 2",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f26s8);
                var f26s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h2",
                    Variable = "padding-top",
                    Value = "0",
                    Name = "Padding Top",
                    Sort = "Header 2",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f26s9);
                var f26s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h2",
                    Variable = "padding-bottom",
                    Value = "5px",
                    Name = "Padding Bottom",
                    Sort = "Header 2",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f26s10);

                #endregion
                #region h3

                var f27s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h3",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Header 3",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f27s1);
                var f27s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h3",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Header 3",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f27s2);
                var f27s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h3",
                    Variable = "font-size",
                    Value = "20px",
                    Name = "Font Size",
                    Sort = "Header 3",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f27s3);
                var f27s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h3",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Header 3",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f27s4);
                var f27s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h3",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Header 3",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f27s5);
                var f27s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h3",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "Header 3",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f27s6);
                var f27s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h3",
                    Variable = "margin-top",
                    Value = "0",
                    Name = "Margin Top",
                    Sort = "Header 3",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f27s7);
                var f27s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h3",
                    Variable = "margin-bottom",
                    Value = "0",
                    Name = "Margin Bottom",
                    Sort = "Header 3",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f27s8);
                var f27s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h3",
                    Variable = "padding-top",
                    Value = "0",
                    Name = "Padding Top",
                    Sort = "Header 3",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f27s9);
                var f27s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h3",
                    Variable = "padding-bottom",
                    Value = "5px",
                    Name = "Padding Bottom",
                    Sort = "Header 3",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f27s10);

                #endregion
                #region h4

                var f28s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h4",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Header 4",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f28s1);
                var f28s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h4",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Header 4",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f28s2);
                var f28s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h4",
                    Variable = "font-size",
                    Value = "18px",
                    Name = "Font Size",
                    Sort = "Header 4",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f28s3);
                var f28s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h4",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Header 4",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f28s4);
                var f28s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h4",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Header 4",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f28s5);
                var f28s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h4",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "Header 4",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f28s6);
                var f28s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h4",
                    Variable = "margin-top",
                    Value = "0",
                    Name = "Margin Top",
                    Sort = "Header 4",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f28s7);
                var f28s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h4",
                    Variable = "margin-bottom",
                    Value = "0",
                    Name = "Margin Bottom",
                    Sort = "Header 4",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f28s8);
                var f28s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h4",
                    Variable = "padding-top",
                    Value = "0",
                    Name = "Padding Top",
                    Sort = "Header 4",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f28s9);
                var f28s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h4",
                    Variable = "padding-bottom",
                    Value = "5px",
                    Name = "Padding Bottom",
                    Sort = "Header 4",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f28s10);

                #endregion
                #region h5

                var f29s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h5",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Header 5",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f29s1);
                var f29s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h5",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Header 5",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f29s2);
                var f29s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h5",
                    Variable = "font-size",
                    Value = "16px",
                    Name = "Font Size",
                    Sort = "Header 5",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f29s3);
                var f29s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h5",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Header 5",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f29s4);
                var f29s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h5",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Header 5",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f29s5);
                var f29s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h5",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "Header 5",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f29s6);
                var f29s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h5",
                    Variable = "margin-top",
                    Value = "0",
                    Name = "Margin Top",
                    Sort = "Header 5",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f29s7);
                var f29s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h5",
                    Variable = "margin-bottom",
                    Value = "0",
                    Name = "Margin Bottom",
                    Sort = "Header 5",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f29s8);
                var f29s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h5",
                    Variable = "padding-top",
                    Value = "0",
                    Name = "Padding Top",
                    Sort = "Header 5",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f29s9);
                var f29s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h5",
                    Variable = "padding-bottom",
                    Value = "5px",
                    Name = "Padding Bottom",
                    Sort = "Header 5",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f29s10);

                #endregion
                #region h6

                var f30s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h6",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Header 6",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f30s1);
                var f30s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h6",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Header 6",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f30s2);
                var f30s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h6",
                    Variable = "font-size",
                    Value = "15px",
                    Name = "Font Size",
                    Sort = "Header 6",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f30s3);
                var f30s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h6",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Header 6",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f30s4);
                var f30s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h6",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Header 6",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f30s5);
                var f30s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h6",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "Header 6",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f30s6);
                var f30s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h6",
                    Variable = "margin-top",
                    Value = "0",
                    Name = "Margin Top",
                    Sort = "Header 6",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f30s7);
                var f30s8 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h6",
                    Variable = "margin-bottom",
                    Value = "0",
                    Name = "Margin Bottom",
                    Sort = "Header 6",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f30s8);
                var f30s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h6",
                    Variable = "padding-top",
                    Value = "0",
                    Name = "Padding Top",
                    Sort = "Header 6",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f30s9);
                var f30s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "h6",
                    Variable = "padding-bottom",
                    Value = "5px",
                    Name = "Padding Bottom",
                    Sort = "Header 6",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f30s10);

                #endregion
                #region p

                var f31s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "p",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Paragraphs",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f31s1);
                var f31s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "p",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Paragraphs",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f31s2);
                var f31s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "p",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Paragraphs",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f31s3);
                var f31s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "p",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Paragraphs",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f31s4);
                var f31s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "p",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Paragraphs",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f31s5);
                var f31s9 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "p",
                    Variable = "padding-top",
                    Value = "0",
                    Name = "Padding Top",
                    Sort = "Paragraphs",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f31s9);
                var f31s10 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "p",
                    Variable = "padding-bottom",
                    Value = "15px",
                    Name = "Padding Bottom",
                    Sort = "Paragraphs",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f31s10);

                #endregion
                #region a

                var f32s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Link",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f32s2);
                var f32s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a",
                    Variable = "text-decoration",
                    Value = "none",
                    Name = "Text Decoration",
                    Sort = "Link",
                    Type = "text-decoration"
                };
                context.DefaultFormStyles.Add(f32s3);
                var f32s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Link",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f32s4);
                var f32s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Link",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f32s5);

                #endregion
                #region a:hover

                var f33s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a:hover",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Link",
                    SubSort = "Hover",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f33s2);
                var f33s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a:hover",
                    Variable = "text-decoration",
                    Value = null,
                    Name = "Text Decoration",
                    Sort = "Link",
                    SubSort = "Hover",
                    Type = "text-decoration"
                };
                context.DefaultFormStyles.Add(f33s3);
                var f33s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a:hover",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Link",
                    SubSort = "Hover",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f33s4);
                var f33s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a:hover",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Link",
                    SubSort = "Hover",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f33s5);

                #endregion
                #region a.btn-rs

                var f34s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Buttons",
                    SubSort = "Normal",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f34s1);
                var f34s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Buttons",
                    SubSort = "Normal",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f34s2);
                var f34s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Buttons",
                    SubSort = "Normal",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f34s3);
                var f34s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Buttons",
                    SubSort = "Normal",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f34s4);
                var f34s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Buttons",
                    SubSort = "Normal",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f34s5);
                var f34s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "Buttons",
                    SubSort = "Normal",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f34s6);
                var f34s11 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs",
                    Variable = "max-width",
                    Value = null,
                    Name = "Max Width",
                    Sort = "Buttons",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f34s11);
                var f34s12 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs",
                    Variable = "border-radius",
                    Value = null,
                    Name = "Border Radius",
                    Sort = "Buttons",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f34s12);
                var f34s15 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background Image",
                    Sort = "Buttons",
                    SubSort = "Normal",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f34s15);
                var f34s16 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "Buttons",
                    SubSort = "Normal",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f34s16);
                var f34s17 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "Buttons",
                    SubSort = "Normal",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f34s17);
                var f34s18 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs",
                    Variable = "background-color",
                    Value = null,
                    Name = "Background Color",
                    Sort = "Buttons",
                    SubSort = "Normal",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f34s18);

                #endregion
                #region a.btn-rs:hover

                var f35s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs:hover",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Buttons",
                    SubSort = "Hover",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f35s1);
                var f35s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs:hover",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Buttons",
                    SubSort = "Hover",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f35s2);
                var f35s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs:hover",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Buttons",
                    SubSort = "Hover",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f35s3);
                var f35s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs:hover",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Buttons",
                    SubSort = "Hover",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f35s4);
                var f35s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs:hover",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Buttons",
                    SubSort = "Hover",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f35s5);
                var f35s14 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs:hover",
                    Variable = "background-color",
                    Value = "#dddddd",
                    Name = "Border Color",
                    Sort = "Buttons",
                    SubSort = "Hover",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f35s14);
                var f35s15 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs:hover",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background Image",
                    Sort = "Buttons",
                    SubSort = "Hover",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f35s15);
                var f35s16 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs:hover",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "Buttons",
                    SubSort = "Hover",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f35s16);
                var f35s17 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs:hover",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "Buttons",
                    SubSort = "Hover",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f35s17);
                var f35s18 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rs:hover",
                    Variable = "background-color",
                    Value = null,
                    Name = "Background Color",
                    Sort = "Buttons",
                    SubSort = "Hover",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f35s18);


                #endregion
                #region a.btn-rsvp

                var f36s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "RSVP Buttons",
                    SubSort = "Normal",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f36s1);
                var f36s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "RSVP Buttons",
                    SubSort = "Normal",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f36s2);
                var f36s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "RSVP Buttons",
                    SubSort = "Normal",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f36s3);
                var f36s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "RSVP Buttons",
                    SubSort = "Normal",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f36s4);
                var f36s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "RSVP Buttons",
                    SubSort = "Normal",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f36s5);
                var f36s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "RSVP Buttons",
                    SubSort = "Normal",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f36s6);
                var f36s7 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "border-radius",
                    Value = null,
                    Name = "Border Radius",
                    Sort = "RSVP Buttons",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f36s7);
                var f36s13 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "max-width",
                    Value = null,
                    Name = "Max Width",
                    Sort = "RSVP Buttons",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f36s13);
                var f36s14 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "background-color",
                    Value = "#dddddd",
                    Name = "Background Color",
                    Sort = "RSVP Buttons",
                    SubSort = "Normal",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f36s14);
                var f36s15 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background Image",
                    Sort = "RSVP Buttons",
                    SubSort = "Normal",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f36s15);
                var f36s16 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "RSVP Buttons",
                    SubSort = "Normal",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f36s16);
                var f36s17 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "RSVP Buttons",
                    SubSort = "Normal",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f36s17);
                var f36s18 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "RSVP Buttons",
                    SubSort = "Normal",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(f36s18);

                #endregion
                #region a.btn-rsvp:hover

                var f37s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp:hover",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "RSVP Buttons",
                    SubSort = "Hover",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f37s1);
                var f37s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp:hover",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "RSVP Buttons",
                    SubSort = "Hover",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f37s2);
                var f37s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp:hover",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "RSVP Buttons",
                    SubSort = "Hover",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f37s3);
                var f37s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp:hover",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "RSVP Buttons",
                    SubSort = "Hover",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f37s4);
                var f37s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp:hover",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "RSVP Buttons",
                    SubSort = "Hover",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f37s5);
                var f37s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp:hover",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "RSVP Buttons",
                    SubSort = "Hover",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f37s6);
                var f37s14 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp:hover",
                    Variable = "background-color",
                    Value = "#dddddd",
                    Name = "Background Color",
                    Sort = "RSVP Buttons",
                    SubSort = "Hover",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f37s14);
                var f37s15 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp:hover",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background Image",
                    Sort = "RSVP Buttons",
                    SubSort = "Hover",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f37s15);
                var f37s16 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp:hover",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "RSVP Buttons",
                    SubSort = "Hover",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f37s16);
                var f37s17 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp:hover",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "RSVP Buttons",
                    SubSort = "Hover",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f37s17);
                var f37s18 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-rsvp:hover",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "Navigation Buttons",
                    SubSort = "Hover",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(f37s18);


                #endregion
                #region a.btn-audience

                var f38s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Audience Buttons",
                    SubSort = "Normal",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f38s1);
                var f38s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Audience Buttons",
                    SubSort = "Normal",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f38s2);
                var f38s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Audience Buttons",
                    SubSort = "Normal",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f38s3);
                var f38s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Audience Buttons",
                    SubSort = "Normal",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f38s4);
                var f38s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Audience Buttons",
                    SubSort = "Normal",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f38s5);
                var f38s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "Audience Buttons",
                    SubSort = "Normal",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f38s6);
                var f38s11 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "max-width",
                    Value = null,
                    Name = "Max Width",
                    Sort = "Audience Buttons",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f38s11);
                var f38s12 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "border-radius",
                    Value = null,
                    Name = "Border Radius",
                    Sort = "Audience Buttons",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f38s12);
                var f38s14 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "background-color",
                    Value = "#dddddd",
                    Name = "Background Color",
                    Sort = "Audience Buttons",
                    SubSort = "Normal",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f38s14);
                var f38s15 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background Image",
                    Sort = "Audience Buttons",
                    SubSort = "Normal",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f38s15);
                var f38s16 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "Audience Buttons",
                    SubSort = "Normal",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f38s16);
                var f38s17 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "Audience Buttons",
                    SubSort = "Normal",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f38s17);
                var f38s18 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(f38s18);

                #endregion
                #region a.btn-audience:hover

                var f39s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience:hover",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Audience Buttons",
                    SubSort = "Hover",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f39s1);
                var f39s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience:hover",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Audience Buttons",
                    SubSort = "Hover",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f39s2);
                var f39s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience:hover",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Audience Buttons",
                    SubSort = "Hover",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f39s3);
                var f39s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience:hover",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Audience Buttons",
                    SubSort = "Hover",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f39s4);
                var f39s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience:hover",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Audience Buttons",
                    SubSort = "Hover",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f39s5);
                var f39s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience:hover",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "Audience Buttons",
                    SubSort = "Hover",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f39s6);
                var f39s14 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience:hover",
                    Variable = "background-color",
                    Value = "#dddddd",
                    Name = "Background Color",
                    Sort = "Audience Buttons",
                    SubSort = "Hover",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f39s14);
                var f39s15 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience:hover",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background Image",
                    Sort = "Audience Buttons",
                    SubSort = "Hover",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f39s15);
                var f39s16 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience:hover",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "Audience Buttons",
                    SubSort = "Hover",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f39s16);
                var f39s17 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience:hover",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "Audience Buttons",
                    SubSort = "Hover",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f39s17);
                var f39s18 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-audience:hover",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(f39s18);

                #endregion
                #region a.btn-navigation #40

                var f40s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f40s1);
                var f40s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f40s2);
                var f40s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f40s3);
                var f40s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f40s4);
                var f40s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f40s5);
                var f40s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f40s6);
                var f40s11 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "max-width",
                    Value = null,
                    Name = "Max Width",
                    Sort = "Navigation Buttons",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f40s11);
                var f40s12 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "border-radius",
                    Value = null,
                    Name = "Border Radius",
                    Sort = "Navigation Buttons",
                    Type = "measurement"
                };
                context.DefaultFormStyles.Add(f40s12);
                var f40s14 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "background-color",
                    Value = null,
                    Name = "Border Color",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f40s14);
                var f40s15 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background Image",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f40s15);
                var f40s16 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f40s16);
                var f40s17 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f40s17);
                var f40s18 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "Navigation Buttons",
                    SubSort = "Normal",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(f40s18);


                #endregion
                #region a.btn-navigation:hover #41

                var f41s1 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation:hover",
                    Variable = "font-family",
                    Value = null,
                    Name = "Font Family",
                    Sort = "Navigation Buttons",
                    SubSort = "Hover",
                    Type = "font"
                };
                context.DefaultFormStyles.Add(f41s1);
                var f41s2 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation:hover",
                    Variable = "color",
                    Value = null,
                    Name = "Font Color",
                    Sort = "Navigation Buttons",
                    SubSort = "Hover",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f41s2);
                var f41s3 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation:hover",
                    Variable = "font-size",
                    Value = null,
                    Name = "Font Size",
                    Sort = "Navigation Buttons",
                    SubSort = "Hover",
                    Type = "font-size"
                };
                context.DefaultFormStyles.Add(f41s3);
                var f41s4 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation:hover",
                    Variable = "font-weight",
                    Value = null,
                    Name = "Font Weight",
                    Sort = "Navigation Buttons",
                    SubSort = "Hover",
                    Type = "font-weight"
                };
                context.DefaultFormStyles.Add(f41s4);
                var f41s5 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation:hover",
                    Variable = "font-style",
                    Value = null,
                    Name = "Font Style",
                    Sort = "Navigation Buttons",
                    SubSort = "Hover",
                    Type = "font-style"
                };
                context.DefaultFormStyles.Add(f41s5);
                var f41s6 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation:hover",
                    Variable = "text-align",
                    Value = null,
                    Name = "Font Align",
                    Sort = "Navigation Buttons",
                    SubSort = "Hover",
                    Type = "text-align"
                };
                context.DefaultFormStyles.Add(f41s6);
                var f41s14 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation:hover",
                    Variable = "background-color",
                    Value = null,
                    Name = "Background Color",
                    Sort = "Navigation Buttons",
                    SubSort = "Hover",
                    Type = "color"
                };
                context.DefaultFormStyles.Add(f41s14);
                var f41s15 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation:hover",
                    Variable = "background-image",
                    Value = null,
                    Name = "Background Image",
                    Sort = "Navigation Buttons",
                    SubSort = "Hover",
                    Type = "image"
                };
                context.DefaultFormStyles.Add(f41s15);
                var f41s16 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation:hover",
                    Variable = "background-position",
                    Value = null,
                    Name = "Background Position",
                    Sort = "Navigation Buttons",
                    SubSort = "Hover",
                    Type = "background-position"
                };
                context.DefaultFormStyles.Add(f41s16);
                var f41s17 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation:hover",
                    Variable = "background-repeat",
                    Value = null,
                    Name = "Background Repeat",
                    Sort = "Navigation Buttons",
                    SubSort = "Hover",
                    Type = "background-repeat"
                };
                context.DefaultFormStyles.Add(f41s17);
                var f41s18 = new DefaultFormStyle()
                {
                    CompanyKey = null,
                    GroupName = "a.btn-navigation:hover",
                    Variable = "background-size",
                    Value = null,
                    Name = "Background Size",
                    Sort = "Navigation Buttons",
                    SubSort = "Hover",
                    Type = "background-size"
                };
                context.DefaultFormStyles.Add(f41s18);

                #endregion

                #endregion
            }

            context.SaveChanges();

            var stylesCount = context.DefaultFormStyles.Where(c => c.CompanyKey == company.UId).Count();
            if (stylesCount == 0)
            {
                var defStyles = context.DefaultFormStyles.Where(d => d.Company == null).ToList();
                foreach (var style in defStyles)
                {
                    var nStyle = new DefaultFormStyle()
                    {
                        CompanyKey = company.UId,
                        Name = style.Name,
                        GroupName = style.GroupName,
                        Variable = style.Variable,
                        Value = style.Value,
                        Sort = style.Sort,
                        SubSort = style.SubSort,
                        Type = style.Type
                    };
                    context.DefaultFormStyles.Add(nStyle);
                }
                context.SaveChanges();
            }

            var um = new AppUserManager(new UserStore<User>(context));
            var user = new User()
            {
                UserName = "andrew.jackson",
                CompanyKey = company.UId,
                Email = "andrew.jackson@lamir.net",
                EmailConfirmed = true,
                IsConfirmed = true
            };
            var user2 = new User()
            {
                UserName = "jason.white",
                CompanyKey = company.UId,
                Email = "jason.white@regstep.com",
                EmailConfirmed = true,
                IsConfirmed = true
            };
            var rm = new AppRoleManager(new RoleStore<AppRole>(context));
            rm.Create(new AppRole() { Name = "Super Administrators" });
            rm.Create(new AppRole() { Name = "Form Builders" });
            rm.Create(new AppRole() { Name = "Email Users" });
            rm.Create(new AppRole() { Name = "Cloud Users" });
            rm.Create(new AppRole() { Name = "Cloud+ Users" });
            rm.Create(new AppRole() { Name = "Company Administrators" });
            if (um.Create(user, "Elise315#!%").Succeeded)
                um.AddToRole(user.Id, "Super Administrators");
            if (um.Create(user2, "RS1q2w3e4r").Succeeded)
                um.AddToRole(user2.Id, "Super Administrators");

            var companyLoad = context.Companies.Where(c => c.UId == company.UId).First();
            if (companyLoad.AvailableRoles.Count == 0)
            {
                var fU = rm.FindByName("Form Builders");
                var fR = AvailableRoles.New(companyLoad, fU);
                var cU = rm.FindByName("Cloud Users");
                var fC = AvailableRoles.New(companyLoad, cU);
                var eU = rm.FindByName("Email Users");
                var fE = AvailableRoles.New(companyLoad, eU);
                companyLoad.AvailableRoles.AddRange(new AvailableRoles[] { fR, fC, fE });
                context.SaveChanges();
            }
        }
    }
}
