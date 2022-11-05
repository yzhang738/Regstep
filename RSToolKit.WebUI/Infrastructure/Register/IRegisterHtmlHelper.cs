using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.JItems;
using System;
using System.Collections.Generic;
using System.Web;

namespace RSToolKit.WebUI.Infrastructure.Register
{
    /// <summary>
    /// Renders html for displaying on the view.
    /// </summary>
    public interface IRegisterHtmlHelper
        : IDisposable
    {
        /// <summary>
        /// Flag to determine if this is for admin registrations.
        /// </summary>
        bool AdminRegistration { get; }
        /// <summary>
        /// A dictionary to hold logic commands for logic holders.
        /// </summary>
        Dictionary<string, IEnumerable<JLogicCommand>> Commands { get; }
        /// <summary>
        /// The context being used for operations.
        /// </summary>
        EFDbContext Context { get; }
        /// <summary>
        /// A flag to determine if this is for editing or not.
        /// </summary>
        bool Editing { get; }
        /// <summary>
        /// A list of the errors to render.
        /// </summary>
        List<SetDataError> Errors { get; }
        /// <summary>
        /// The form that this helper uses.
        /// </summary>
        Form Form { get; }
        /// <summary>
        /// The page being rendered.
        /// </summary>
        Page Page { get; }
        /// <summary>
        /// The registrant to use.
        /// </summary>
        Registrant Registrant { get; }
        /// <summary>
        /// A flag to use the sorting id instead of the UId.
        /// </summary>
        bool UseSortingId { get; }

        /// <summary>
        /// Checks to see if a IFormComponent is blank.
        /// </summary>
        /// <typeparam name="T">The form component type.</typeparam>
        /// <param name="item">The form component to check.</param>
        /// <returns>True if blank, false otherwise.</returns>
        bool IsBlank<T>(T item) where T : IFormComponent;
        /// <summary>
        /// Renders an IComponent into html.
        /// </summary>
        /// <param name="component">The component to render.</param>
        /// <returns>The html string.</returns>
        HtmlString Render(IComponent component);
        /// <summary>
        /// Renders the audiences.
        /// </summary>
        /// <returns>The html string.</returns>
        HtmlString RenderAudiences();
        /// <summary>
        /// Renders the component for confirmation.
        /// </summary>
        /// <param name="component">The component to render.</param>
        /// <returns>The html string.</returns>
        HtmlString RenderConfirmationComponent(IComponent component);
        /// <summary>
        /// Renders the page for confirmation.
        /// </summary>
        /// <param name="page">The page to render.</param>
        /// <returns>The html string.</returns>
        HtmlString RenderConfirmationPage(Page page);
        /// <summary>
        /// Renders the panel for confirmation.
        /// </summary>
        /// <param name="panel">The panel to render.</param>
        /// <returns>The html string.</returns>
        HtmlString RenderConfirmationPanel(Panel panel);
        /// <summary>
        /// Renders the footer.
        /// </summary>
        /// <returns>The html string.</returns>
        HtmlString RenderFooter();
        /// <summary>
        /// Renders the form styles.
        /// </summary>
        /// <returns>The html string.</returns>
        HtmlString RenderFormStyle();
        /// <summary>
        /// Renders the form header.
        /// </summary>
        /// <returns>The html string.</returns>
        HtmlString RenderHeader();
        /// <summary>
        /// Renders the hidden components.
        /// </summary>
        /// <returns>The html string.</returns>
        HtmlString RenderHiddens();
        /// <summary>
        /// Renders the page numbers.
        /// </summary>
        /// <returns>The html string.</returns>
        HtmlString RenderPageNumbers();
        /// <summary>
        /// Renders the panel.
        /// </summary>
        /// <param name="panel">The panel to render.</param>
        /// <returns>The html string.</returns>
        HtmlString RenderPanel(Panel panel);
        /// <summary>
        /// Renders the rsvp selections.
        /// </summary>
        /// <param name="first">A flag if this is the users first time seeing this.</param>
        /// <returns>The html string.</returns>
        HtmlString RenderRsvp(bool first = false);
        /// <summary>
        /// Renders the registrants shopping cart.
        /// </summary>
        /// <returns>The html string.</returns>
        HtmlString RenderShoppingCart();
        /// <summary>
        /// Sets the page being rendered.
        /// </summary>
        /// <param name="page">The page number.</param>
        void SetPage(int page);
    }
}