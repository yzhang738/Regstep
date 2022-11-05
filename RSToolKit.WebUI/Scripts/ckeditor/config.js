/**
 * @license Copyright (c) 2003-2015, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here. For example:
	// config.language = 'fr';
    // config.uiColor = '#AADC6E';
    config.enterMode = CKEDITOR.ENTER_BR;
    config.extraPlugins = 'emailcontext,sourcedialog';
    config.removePlugins = 'sourcearea';
    config.allowedContent = true;
    // Toolbar configuration generated automatically by the editor based on config.toolbarGroups.
    config.toolbar = [
        { name: 'document', items: ['Sourcedialog', 'Scayt'] },
        { name: 'clipboard', items: ['Cut', 'Copy', 'Past', '-', 'Undo', 'Redo'] },
        { name: 'styles', items: ['Font', 'FontSize', 'TextColor'] },
        '/',
        { name: 'font', items: ['Bold', 'Italic', 'Underline', 'Subscript', 'Superscript'] },
        { name: 'elements', items: ['Link', 'Unlink', 'Anchor', 'Image', 'SpecialChar'] }
    ];
};
