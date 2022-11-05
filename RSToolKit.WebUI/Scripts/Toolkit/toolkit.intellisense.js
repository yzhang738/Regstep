//#region crumbs
// Holds information about the current pheromone and methods to update it.
var trail = {
    /// <field name="current" type="Crumb">The current pheromone.</field>
    current: undefined,
    updatePheromone: function () {
        /// <signature>
        /// <summary>Update the current pheromone into the database.</summary>
        /// </signature>
    },
    setLabel: function (label) {
        /// <signature>
        /// <summary>Update the current pheromone into the database.</summary>
        /// <param name="label" type="String">The label to use</param>
        /// </signature>
    }
};
var Pheromone = function () {
    /// <signature>
    /// <summary>Creates a crumb to be used in BreadCrumbs.</summary>
    /// <returns type="Crumb" />
    /// </signature>
    /// <field name="action" type="String">The action the crumb references.</field>
    this.action = undefined;
    /// <field name="controller" type="String">The controller the crumb references.</field>
    this.controller = undefined;
    /// <field name="parameters" type="Object">The route data parameters the crumb references.</field>
    this.parameters = undefined;
    /// <field name="actionDate" type="moment">The date the crumb was made.</field>
    this.actionDate = undefined;
    /// <field name="id" type="String">The id of the crumb (Guid).</field>
    this.id = undefined;

    this.toAjax = function () {
        /// <signature>
        /// <summary>Returns an object that is used to serialize into an ajax request.</summary>
        /// <returns type="Object" />
        /// </signature>
    };
};
//#endregion
//#region toolkit
var toolkit = {
    /// <field type="String" name="jsonAntiForgeryToken">The current antiforgery token for HTTP-GET, HTTP-DELETE, HTTP-POST, and HTTP-PUT.</field>
    jsonAntiForgeryToken: undefined,
    addJsonAntiForgeryToken: function (data) {
        /// <signature>
        /// <summary>Adds the antiforgery token to an object.</summary>
        /// <param name="data" type="object">The object (or FormData) to append the antiforgery token to.</param>
        /// <returns type="object">Returns the object passed.</returns>
        /// </signature>
        /// <signature>
        /// <summary>Adds the antiforgery token to a FormData object.</summary>
        /// <param name="data" type="FormData">The FormData object to append the antiforgery token to.</param>
        /// <returns type="FormData">Returns the FormData object passed.</returns>
        /// </signature>
    },
    // The company Id.
    companyId: ''
};
//#endregion
//#region processing
var processing = prettyProcessing = {
    showPleaseWait: function(title, message, percent) {
        /// <signature>
        /// <summary>Shows the proccessing modal with the desired settings.</summary>
        /// <param name="title" type="String">The title of the modal.</param>
        /// <param name="message" type="String">The message to show.</param>
        /// <param name="percent" type="Number">The percent completed.</param>
        /// </signature>
    },
    update: function (title, message, percent) {
        /// <signature>
        /// <summary>Updates the progress bar percent.</summary>
        /// <param name="title" type="String">The title of the modal.</param>
        /// <param name="message" type="String">The message to show.</param>
        /// <param name="percent" type="Number">The percent completed.</param>
        /// </signature>
    },
    hidePleaseWait: function () {
        /// <signature>
        /// <summary>Hides the dialog.</summary>
        /// </signature>
    }
};
//#endregion
//#region restful
var RESTFUL = {
    xhrError: function (event, message) {
        /// <signature>
        /// <summary>Default error function for XMLHttpRequest.onError</summary>
        /// <param name="event" type="object">The event parameter of the onError delegate.</param>
        /// <param name="message" type="string">The message to display</param>
        /// </signature>
    },
    parse: function (xhr) {
        /// <signature>
        /// <summary>Gets the response text parsed out into a JavaScript object.</summary>
        /// <param name="xhr" type="XMLHttpRequest">The XMLHttpRequest object.</param>
        /// <returns type="Object">Returns an object parsed from the response text.</param>
        /// </signature>
    },
    jsonHeader: function (xhr) {
        /// <signature>
        /// <summary>Sets a json header.</summary>
        /// <param name="xhr" type="XMLHttpRequest">The XMLHttpRequest object.</param>
        /// </signature>
    },
    ajaxHeader: function (xhr) {
        /// <signature>
        /// <summary>Sets an Ajax header.</summary>
        /// <param name="xhr" type="XMLHttpRequest">The XMLHttpRequest object.</param>
        /// </signature>
    },
    showError: function (message, title) {
        /// <signature>
        /// <summary>Displays a message in a modal.</summary>
        /// <param name="message" type="string">The message to display</param>
        /// <param name="title" type="string">The title to display</param>
        /// </signature>
    },
    handleError: function (xhr) {
        /// <signature>
        /// <summary>Handles the error of the xml request.</summary>
        /// <param name="xhr" type="XMLHttpRequest">The XMLHttpRequest.</param>
        /// </signature>
    },
    showConfirmation: function (message, title, yesFn, noFn, yesBtn, noBtn) {
        /// <signature>
        /// <summary>Displays a message in a modal.</summary>
        /// <param name="message" type="String">The message to display</param>
        /// <param name="title" type="String">The title to display</param>
        /// <param name="yesFn" type="Function">The function to run when yes is clicked.</param>
        /// <param name="noFn" type="Function">The function to run when no is clicked.</param>
        /// <param name="yesBtn" type="String">The label for the yes button.</param>
        /// <param name="noBtn" type="String">The label for the no button.</param>
        /// </signature>
    },
    bind: function (selector) {
        /// <signature>
        /// <summary>Binds the specified element to the RESTFUL click event.</summary>
        /// <param name="selector" type="String">The css selector to use for finding DOM objects to bind.</param>
        /// </signature>
    },
};
//#endregion
//#region extensions.js
String.prototype.getURI = function () {
    /// <signature>
    /// <summary>Returns a fully qualified URI.</summar>
    /// <return type="String">
    /// </signature>
}
String.prototype.root = function (root) {
    /// <signature>
    /// <summary>Returns the root of the strin up to the supplied root ending.</summar>
    /// <param name="root" type="String">Grabs everything to the left of the first occurence of this string.</param>
    /// <return type="String">
    /// </signature>
}
Math.roundAdv = function (number, decimals) {
    /// <signature>
    /// <summary>Returns a roounded number with up to the amount of decimals supplied.</summar>
    /// <param name="number" type="Number">The number to round.</param>
    /// <param name="number" type="Number" integer="true">The number to o decimal places to round to.</param>
    /// <return type="Number">
    /// </signature>
}
var EXT = {
    getName: function (obj) {
        /// <signature>
        /// <summary>Returns the name of the type of object.</summar>
        /// <return type="String">
        /// </signature>
    }
}
//#endregion