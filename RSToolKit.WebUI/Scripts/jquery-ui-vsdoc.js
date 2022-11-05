// Simple jquery ui vsdoc file 
// (doesn't contain working JS code, and is not complete.. there's probably also a few bugs)
// Intial version by Jay E. Kimble (http://theruntime.com/blogs/jaykimble)

(function () {
    var
    // Will speed up references to window, and allows munging its name.
	window = this;
    // Map over jQuery in case of overwrite
    _jQuery = window.jQuery;
    // Map over the $ in case of overwrite
    _$ = window.$;

    jQuery.ui = window.jQuery.ui = window.$.ui = (function (c) {
    })(_jQuery);

    _$.prototype["draggable"] = function (o) {
        ///	<summary>
        ///		Makes the selection draggable
        ///	</summary>
        ///	<param name="o" type="object">
        ///		options object
        ///	</param>
        ///	<returns type="jQuery" />
    }
    _$.prototype["droppable"] = function (o) {
        ///	<summary>
        ///		Makes the selection drappable
        ///	</summary>
        ///	<param name="o" type="object">
        ///		options object
        ///	</param>
        ///	<returns type="jQuery" />    
    }
    _$.prototype["resizable"] = function (o) {
        ///	<summary>
        ///		Makes the selection resizable
        ///	</summary>
        ///	<param name="o" type="object">
        ///		options object
        ///	</param>
        ///	<returns type="jQuery" />        
    }
    _$.prototype["selectable"] = function (o) {
        ///	<summary>
        ///		Makes the selection selectable
        ///	</summary>
        ///	<param name="o" type="object">
        ///		options object
        ///	</param>
        ///	<returns type="jQuery" />            
    }
    _$.prototype["sortable"] = function (o) {
        ///	<summary>
        ///		Makes the selection sortable
        ///	</summary>
        ///	<param name="o" type="object">
        ///		options object
        ///	</param>
        ///	<returns type="jQuery" />          
    }
    _$.prototype["accordian"] = function (o) {
        ///	<summary>
        ///		Makes the selection an accordian
        ///	</summary>
        ///	<param name="o" type="object">
        ///		options object
        ///	</param>
        ///	<returns type="jQuery" />          
    }
    _$.prototype["datepicker"] = function (o) {
        ///	<summary>
        ///		Makes the selection a datepicker
        ///	</summary>
        ///	<param name="o" type="object">
        ///		options object
        ///	</param>
        ///	<returns type="jQuery" />          
    }
    _$.prototype["dialog"] = function (o) {
        ///	<summary>
        ///		Makes the selection a dialog
        ///	</summary>
        ///	<param name="o" type="object">
        ///		options object
        ///	</param>
        ///	<returns type="jQuery" />          
    }
    _$.prototype["progressbar"] = function (o) {
        ///	<summary>
        ///		Makes the selection a progressbar
        ///	</summary>
        ///	<param name="o" type="object">
        ///		options object
        ///	</param>
        ///	<returns type="jQuery" />          
    }
    _$.prototype["slider"] = function (o) {
        ///	<summary>
        ///		Makes the selection a slider
        ///	</summary>
        ///	<param name="o" type="object">
        ///		options object
        ///	</param>
        ///	<returns type="jQuery" />          
    }
    _$.prototype["tabs"] = function (o) {
        ///	<summary>
        ///		Makes the selection a tab
        ///	</summary>
        ///	<param name="o" type="object">
        ///		options object
        ///	</param>
        ///	<returns type="jQuery" />          
    }

    var fx = function (effectName, o) {
        ///	<summary>
        ///		Makes the selection have an effect
        ///	</summary>
        ///	<param name="effectName" type="string">
        ///		can be one of the following: blind, clip, drop, explode, fold, puff, slide, scale,
        ///     bounce, highlight, pulsate, shake, size, transfer
        ///	</param>
        ///	<param name="o" type="object">
        ///		options object
        ///	</param>
        ///	<returns type="jQuery" />                  
    }
    _$.prototype["effects"] = fx;

})();