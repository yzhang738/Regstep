CKEDITOR.plugins.add('emailcontext', {
    init: function (editor) {
        editor.addCommand('removeEmailArea', {
            exec: function (editor) {
                editor.fire('removearea.ckeditor');
            }
        });
        editor.addCommand('emailAreaUp', {
            exec: function (editor) {
                editor.fire('areaUp.ckeditor');
            }
        });
        editor.addCommand('emailAreaDown', {
            exec: function (editor) {
                editor.fire('areaDown.ckeditor');
            }
        });
        editor.addCommand('emailAreaVariables', {
            exec: function (editor) {
                editor.fire('emailareavariables.ckeditor');
            }
        });
        editor.addMenuGroup('emailArea');
        editor.addMenuItem('emailAreaUp', {
            label: 'Move Up',
            command: 'emailAreaUp',
            group: 'emailArea'
        });
        editor.addMenuItem('emailAreaDown', {
            label: 'Move Down',
            command: 'emailAreaDown',
            group: 'emailArea'
        });
        editor.addMenuItem('removeEmailArea', {
            label: 'Remove Element',
            command: 'removeEmailArea',
            group: 'emailArea'
        });
        editor.addMenuItem('emailAreaVariables', {
            label: 'Variables',
            command: 'emailAreaVariables',
            group: 'emailArea'
        });
        editor.contextMenu.addListener(function (element) {
            return { emailAreaUp: CKEDITOR.TRISTATE_OFF };
        });
        editor.contextMenu.addListener(function (element) {
            return { emailAreaDown: CKEDITOR.TRISTATE_OFF };
        });
        editor.contextMenu.addListener(function (element) {
            return { removeEmailArea: CKEDITOR.TRISTATE_OFF };
        });
        editor.contextMenu.addListener(function (element) {
            return { emailAreaVariables: CKEDITOR.TRISTATE_OFF };
        });
    }
});