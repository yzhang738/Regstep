(function ($) {

    /*****************/
    /* Configuration */
    /*****************/

    var hiddenDivId = "tree_hidden_div"
    
    var stylesheetlocation = "../../Content/Bootstrap/Tree.min.css";

    var moveUrl = 'Folder/Move';
    var moveMethod = 'put';

    var newFolderUrl = 'Folder/Folder';
    var newFolderMethod = 'post';

    var editFolderUrl = 'Folder/Folder';
    var editFolderMethod = 'put';

    var deleteFolderUrl = 'Folder/Folder';
    var deleteFolderMethod = 'delete';

    var editNodeUrl = 'Node/Node';

    var deleteNodeUrl = 'Node/Node';
    var deleteNodeMethod = 'delete';

    var copyNodeUrl = 'FormBuilder/CopyForm';
    var copyNodeMethod = 'post';

    var nodeCommands = [
        {
            command: 'link',
            label: 'Move',
            url: ''
        }
    ];

    /*********************/
    /* End Configuration */
    /*********************/

    $('body').append('<div style="display: none" id="' + hiddenDivId + '"></div>');

    var fileref = document.createElement("link");
    fileref.setAttribute("rel", "stylesheet");
    fileref.setAttribute("type", "text/css");
    fileref.setAttribute("href", "../../Content/Bootstrap/Tree.min.css");
    document.getElementsByTagName("head")[0].appendChild(fileref);

    var trees = 0;
    var t_modalHtml = '<div class="modal fade"><div class="modal-dialog"><div class="modal-header"><h3 class="modal-title">Folder Select</h3></div>';
    t_modalHtml += '<div class="modal-body"><div class="row"><div class="col-sm-12 folder-select-body"></div></div></div>';
    t_modalHtml += '<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button><button class="tree-folderselect-save btn btn-default">Set</button></div></div></div>';

    var t_defContext =  '<div class="tree-context" style="display: none;">';
    t_defContext += '<ul class="tree-context-group tree-context-group-folder">';
    t_defContext += '<li class="tree-context-folder-edit tree-context-item">Edit</li>';
    t_defContext += '<li class="tree-context-folder-delete tree-context-item">Delete</li>';
    t_defContext += '<li class="tree-context-folder-move tree-context-item">Move</li>';
    t_defContext += '<li class="tree-context-folder-new tree-context-item">New Folder</li>';
    t_defContext += '</ul>';
    t_defContext += '<ul class="tree-context-group tree-context-group-node">';
    t_defContext += '<li class="tree-context-node-edit tree-context-item">Edit</li>'
    t_defContext += '<li class="tree-context-node-delete tree-context-item">Delete</li>';
    t_defContext += '<li class="tree-context-node-move tree-context-item">Move</li>';
    t_defContext += '<li class="tree-context-node-copy tree-context-item">Copy</li>';
    t_defContext += '</ul>';
    t_defContext += '</div>';

    var t_defNewFolderModal = '<div class="modal fade"><div class="modal-dialog"><div class="modal-header"><h3 class="modal-title">New Folder</h3></div>';
    t_defNewFolderModal += '<div class="modal-body"><div class="row"><div class="col-sm-12 folder-select-body"><input type="text" class="new-folder-name form-control" placeholder="Folder Name" /></div></div></div>';
    t_defNewFolderModal += '<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button><button class="tree-newFolder-save btn btn-default">Create</button></div></div></div>';

    var t_defEditFolderModal = '<div class="modal fade"><div class="modal-dialog"><div class="modal-header"><h3 class="modal-title">Edit Folder</h3></div>';
    t_defEditFolderModal += '<div class="modal-body"><div class="row"><div class="col-sm-12 folder-select-body"><input type="text" class="edit-folder-name form-control" placeholder="Folder Name" /></div></div></div>';
    t_defEditFolderModal += '<div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button><button class="tree-editFolder-save btn btn-default">Update</button></div></div></div>';


    $.fn.tree = function (options) {

        // Set default options and override with provided options.
        var settings = $.extend({
            allowNodeEdit: true,
            allowNodeMove: true,
            allowFolderEdit: true,
            allowFolderMove: true,
            allowFolderDelete: true,
            allowNodeDelete: true,
            showButtons: false,
            allowNewFolder: true
        }, options);

        // Run through each tree.
        this.each(function () {
            trees++; //Increment the tree count for id purposes.

            // Set some tree variables
            var t_folder_id = null;
            var t_node_id = null;

            var t_tree = $(this);
            t_tree.attr('unselectable', 'on').css('user-select', 'none').on('selectstart', false);
            var t_editModal = $(t_defEditFolderModal);

            if (settings.allowFolderEdit) {
                $('body').append(t_editModal);
                t_editModal.find('.tree-editFolder-save').on('click', function (e) {
                    var t_folder = $('#' + t_folder_id);
                    var t_fs_folder = t_modal.find('.tree-folder[data-tree-folder-id="' + t_folder_id + '"]');
                    t_editModal.modal('hide');
                    processing.showPleaseWait();
                    var fe_xhr = new XMLHttpRequest();
                    var fe_data = { id: t_folder_id, name: t_editModal.find('.edit-folder-name').val() };
                    AddJsonAntiForgeryToken(fe_data);
                    fe_xhr.onerror = function (result) {
                        processing.hidePleaseWait();
                        alert('Unhandled Server Error');
                    };
                    fe_xhr.onload = function (event) {
                        processing.hidePleaseWait();
                        if (event.currentTarget.status == 200) {
                            var result = JSON.parse(fe_xhr.responseText);
                            if (result.Success) {
                                t_folder.children('.tree-item-name').text(fe_data.name);
                                t_fs_folder.children('.tree-item-name').text(fe_data.name);
                            } else {
                                alert(result.Message);
                            }
                        } else {
                            alert('Unhandled Server Error');
                        }
                    };
                    fe_xhr.open(editFolderMethod, editFolderUrl, true);
                    fe_xhr.setRequestHeader('Content-Type', 'application/json');
                    fe_xhr.send(JSON.stringify(fe_data));
                });
            }

            // Create the context menu
            var t_context = $(t_defContext);
            $('body').append(t_context);
            t_context.attr('id', 'tree_context_' + trees);
            t_context.children('ul').attr('data-tree-context', trees);

            var t_modal = null;
            if (settings.allowNodeMove || settings.allowFolderMove) {
                var t_modalBody = t_tree.clone();
                t_modal = $(t_modalHtml);
                t_modal.attr('id', 'tree_modal_' + trees);
                $('body').append(t_modal);
                t_modalBody.find('.tree-node').closest('li').remove();
                t_modalBody.find('.tree-folder').each(function (i) {
                    var t_id = $(this).attr('id');
                    $(this).attr('data-tree-folder-id', t_id);
                    $(this).attr('id', '');
                    $(this).append('<span class="tree-folder-select">Select</span>');
                });
                t_modal.find('.folder-select-body').append(t_modalBody);
                t_modalBody.find('.tree-folder-select').on('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    var f_id = $(this).closest('.tree-folder').attr('data-tree-folder-id');
                    t_modalBody.find('.tree-folder-select').not('#' + f_id).text('Select');
                    $(this).text('Selected');
                    t_modal.data('tree-folder', f_id);
                });
                $('li.parent_li', t_modalBody).find(' > ul > li').hide();
                t_modal.find('.tree-folderselect-save').on('click', function (e) {
                    t_modal.modal('hide');
                    processing.showPleaseWait();
                    var fs_xhr = new XMLHttpRequest();
                    var fs_data = { id: t_modal.data('tree-item'), targetFolder: t_modal.data('tree-folder') };
                    AddJsonAntiForgeryToken(fs_data);
                    fs_xhr.onerror = function (result) {
                        processing.hidePleaseWait();
                        alert('Unhandled Server Error');
                    };
                    fs_xhr.onload = function (event) {
                        processing.hidePleaseWait();
                        if (event.currentTarget.status == 200) {
                            var result = JSON.parse(fs_xhr.responseText);
                            if (result.Success) {
                                // Move the folder in the tree
                                var i_id = t_modal.data('tree-item');
                                var f_id = t_modal.data('tree-folder');
                                var tree_item = $('#' + i_id).closest('li');
                                var n_folder = $('#' + f_id).closest('li').children('ul');
                                n_folder.append(tree_item)

                                //Move the folder in the folder select if it is a folder being moved
                                if (tree_item.children('span').is('.tree-folder')) {
                                    var fs_item = $('.tree-folder[data-tree-folder-id="' + i_id + '"]').closest('li');
                                    var fs_n_folder = $('.tree-folder[data-tree-folder-id="' + f_id + '"]').closest('li').children('ul');
                                    fs_n_folder.append(fs_item);
                                }
                            } else {
                                alert(result.Message);
                            }
                        } else {
                            alert('Unhandled Server Error');
                        }
                    };
                    fs_xhr.open(moveMethod, moveUrl, true);
                    fs_xhr.setRequestHeader('Content-Type', 'application/json');
                    fs_xhr.send(JSON.stringify(fs_data));
                });
            }

            // Ensure that the tree has the 'tree' class
            if (!t_tree.hasClass('tree'))
                t_tree.addClass('tree');

            t_tree.data('tree-id', tree);

            // Opens the and closes folders
            $('li:has(ul)', t_tree).addClass('parent_li').find(' > .tree-haschildren').attr('title', 'Collapse this branch');
            $('li.parent_li', t_tree).find(' > ul > li').hide();
            $('li.parent_li > span', t_tree).on('click', function (e) {
                var children = $(this).parent('li.parent_li').find(' > ul > li');
                if (children.is(":visible")) {
                    children.hide('fast');
                    $(this).attr('title', 'Expand this branch').find('.glyphicon').addClass('glyphicon-folder-close').removeClass('glyphicon-folder-open');
                } else {
                    children.show('fast');
                    $(this).attr('title', 'Collapse this branch').find('.glyphicon').addClass('glyphicon-folder-open').removeClass('glyphicon-folder-close');
                }
                e.stopPropagation();
            });

            // If we are allowing folders or nodes to move, we need to turn the folder select into a workable tree.
            if (settings.allowFolderMove || settings.allowNodeMove) {
                $('li:has(ul)', t_modal).addClass('parent_li').find(' > .tree-haschildren').attr('title', 'Collapse this branch');
                $('li.parent_li', t_modal).find(' > ul > li').hide();
                $('li.parent_li > span', t_modal).on('click', function (e) {
                    var children = $(this).parent('li.parent_li').find(' > ul > li');
                    if (children.is(":visible")) {
                        children.hide('fast');
                        $(this).attr('title', 'Expand this branch').find('.glyphicon').addClass('glyphicon-folder-close').removeClass('glyphicon-folder-open');
                    } else {
                        children.show('fast');
                        $(this).attr('title', 'Collapse this branch').find('.glyphicon').addClass('glyphicon-folder-open').removeClass('glyphicon-folder-close');
                    }
                    e.stopPropagation();
                });
            }


            // See if folders can be edited.
            if (!settings.allowFolderEdit) {
                // Folders cannot be edited. We need to remove the context menu.
                t_context.find('.tree-context-folder-edit').remove();
            } else {
                // Folders can be edited. We need to bind the edit context menu.
                t_context.find('.tree-context-folder-edit').on('click', function (e) {
                    e.stopPropagation();
                    var t_folder = $('#' + t_folder_id).children('.tree-item-name').text();
                    t_editModal.find('.edit-folder-name').val(t_folder);
                    t_editModal.modal('show');
                });
            }

            // See if folders can be moved
            if (!settings.allowFolderMove) {
                // Move folders not allowed, we need to remove the context menu.
                t_context.find('.tree-context-folder-move').remove();
            } else {
                // If buttons are enabled, we add the button and bind event to move button
                if (settings.showButtons) {
                    $('.tree-folder').each(function (i) {
                        $(this).append(' <a href="#" class="tree-folder-move"><span class="glyphicon glyphicon-move"></span></a>');
                    });
                    t_tree.find('.tree-folder-move, .tree-node-move').on('click', function (e) {
                        e.preventDefault();
                        var f_id = $(this).closest('.parent_li').children('.tree-folder').attr('id');
                        var i_id = $(this).closest('.tree-item').attr('id');
                        t_modal.data('tree-item', i_id);
                        t_modal.data('tree-folder', f_id);
                        t_modal.find('.tree-folder').not('#' + f_id).find('.tree-folder-select').text('Select');
                        t_modal.find('#' + f_id).find('.tree-folder-select').text('Selected');
                        t_modal.modal('show');
                    });
                }
                // Bind the context menu.
                t_context.find('.tree-context-folder-move').on('click', function (e) {
                    e.stopPropagation();
                    var temp_item = $('#' + t_folder_id).parent();
                    var f_id = $(temp_item).closest('.parent_li').children('.tree-folder').attr('id');
                    var i_id = t_folder_id;
                    t_modal.data('tree-item', i_id);
                    t_modal.data('tree-folder', f_id);
                    t_modal.find('.tree-folder').not('#' + f_id).find('.tree-folder-select').text('Select');
                    t_modal.find('#' + f_id).find('.tree-folder-select').text('Selected');
                    t_modal.modal('show');
                    t_context.hide();
                });
            }

            // See if nodes can be edited.
            if (!settings.allowNodeEdit) {
                // Nodes cannot be edited so we remove the context menu.
                t_context.find('.tree-context-node-edit').remove();
            } else {
                // Add double click to edit
                t_tree.find('.tree-node').on('dblclick', function (e) {
                    e.stopPropagation();
                    window.location.href = editNodeUrl + '/' + $(this).attr('id');
                });
                // We need to add the button for editing nodes if buttons enabled.
                if (settings.showButtons) {
                    $('.tree-node').each(function (i) {
                        $(this).append(' <a href="' + editNodeUrl + '/' + $(this).attr('id') + '"><span class="glyphicon glyphicon-edit"></span></a>');
                    });
                }
                // We need to bind the context menu.
                t_context.find('.tree-context-node-edit').on('click', function (e) {
                    e.stopPropagation();
                    window.location.href = editNodeUrl + '/' + t_node_id;
                });
            }

            // See if nodes can be moved.
            if (!settings.allowNodeMove) {
                // Nodes cannot be moved.  We need to remove the move context.
                t_tree.find('.tree-context-node-move').remove();
            } else {
                // If buttons enabled we need to add the move button.
                if (settings.showButtons) {
                    $('.tree-node').each(function (i) {
                        $(this).append(' <a href="#" class="tree-node-move"><span class="glyphicon glyphicon-move"></span></a>');
                    });
                }
                // We need to bind the context menu.
                t_context.find('.tree-context-node-move').on('click', function (e) {
                    e.stopPropagation();
                    var temp_item = $('#' + t_node_id);
                    var f_id = $(temp_item).closest('.parent_li').children('.tree-folder').attr('id');
                    var i_id = t_node_id;
                    t_modal.data('tree-item', i_id);
                    t_modal.data('tree-folder', f_id);
                    t_modal.find('.tree-folder').not('#' + f_id).find('.tree-folder-select').text('Select');
                    t_modal.find('#' + f_id).find('.tree-folder-select').text('Selected');
                    t_modal.modal('show');
                    t_context.hide();
                });
            }

            // See if nodes can be deleted
            if (!settings.allowNodeDelete) {
                // Nodes cannot be deleted. We need to remove the context menu.
                t_context.find('.tree-context-node-delete').remove();
            } else {
                // We need to bind the delete context menu
                t_context.find('.tree-context-node-delete').on('click', function (e) {
                    e.stopPropagation();
                    t_context.hide();
                    var temp_item = $('#' + t_node_id).closest('li');
                    if (!confirm('Deleting an item cannot be undone? Do you want to continue.'))
                        return;
                    processing.showPleaseWait();
                    var fe_xhr = new XMLHttpRequest();
                    var fe_data = { id: t_node_id };
                    AddJsonAntiForgeryToken(fe_data);
                    fe_xhr.onerror = function (result) {
                        processing.hidePleaseWait();
                        alert('Unhandled Server Error');
                    };
                    fe_xhr.onload = function (event) {
                        processing.hidePleaseWait();
                        if (event.currentTarget.status == 200) {
                            var result = JSON.parse(fe_xhr.responseText);
                            if (result.Success) {
                                temp_item.remove();
                            } else {
                                alert(result.Message);
                            }
                        } else {
                            alert('Unhandled Server Error');
                        }
                    };
                    fe_xhr.open(deleteNodeMethod, deleteNodeUrl, true);
                    fe_xhr.setRequestHeader('Content-Type', 'application/json');
                    fe_xhr.send(JSON.stringify(fe_data));
                });
            }

            // See if folders can be deleted
            if (!settings.allowFolderDelete) {
                t_context.find('.tree-context-folder-delete').remove();
            } else {
                // We need to bind the delete context menu.
                t_context.find('.tree-context-folder-delete').on('click', function (e) {
                    e.stopPropagation();
                    t_context.hide();
                    var temp_item = $('#' + t_folder_id).closest('li');
                    if (!confirm('Deleting an item cannot be undone? Do you want to continue.'))
                        return;
                    processing.showPleaseWait();
                    var fe_xhr = new XMLHttpRequest();
                    var fe_data = { id: t_folder_id };
                    AddJsonAntiForgeryToken(fe_data);
                    fe_xhr.onerror = function (result) {
                        processing.hidePleaseWait();
                        alert('Unhandled Server Error');
                    };
                    fe_xhr.onload = function (event) {
                        processing.hidePleaseWait();
                        if (event.currentTarget.status == 200) {
                            var result = JSON.parse(fe_xhr.responseText);
                            if (result.Success) {
                                temp_item.remove();
                            } else {
                                alert(result.Message);
                            }
                        } else {
                            alert('Unhandled Server Error');
                        }
                    };
                    fe_xhr.open(deleteFolderMethod, deleteFolderUrl, true);
                    fe_xhr.setRequestHeader('Content-Type', 'application/json');
                    fe_xhr.send(JSON.stringify(fe_data));
                });
            }

            // We need to bind the copy context menu
            t_context.find('.tree-context-node-copy').on('click', function (e) {
                e.stopPropagation();
                t_context.hide();
                var temp_item = $('#' + t_node_id).closest('li');
                processing.showPleaseWait();
                var fe_xhr = new XMLHttpRequest();
                var fe_data = { key: t_node_id };
                AddJsonAntiForgeryToken(fe_data);
                fe_xhr.onerror = function (result) {
                    processing.hidePleaseWait();
                    alert('Unhandled Server Error');
                };
                fe_xhr.onload = function (event) {
                    processing.hidePleaseWait();
                    if (event.currentTarget.status == 200) {
                        var result = JSON.parse(fe_xhr.responseText);
                        if (result.Success) {
                            window.location.href = result.Location;
                        } else {
                            alert(result.Message);
                        }
                    } else {
                        alert('Unhandled Server Error');
                    }
                };
                fe_xhr.open("post", "Node/Copy", true);
                fe_xhr.setRequestHeader('Content-Type', 'application/json');
                fe_xhr.send(JSON.stringify(fe_data));
            });


            // Bind context menu to tree items
            t_tree.find('.tree-item').on('contextmenu', function (e) {
                contextMenu(e, $(this));
            });

            // Check if we allow new folders
            if (!settings.allowNewFolder) {
                // We do not allow it, remove the context menu item.
                t_context.find('.tree-context-folder-new').remove();
            } else {
                // Bind new folder on context menu.
                t_newFolderModal = $(t_defNewFolderModal);
                t_newFolderModal.attr('id', 'tree_' + trees + '_newFolder');
                t_context.find('.tree-context-folder-new').on('click', function (e) {
                    t_newFolderModal.find('.new-folder-name').val('');
                    t_newFolderModal.modal('show');
                });
                t_newFolderModal.find('.tree-newFolder-save').on('click', function (e) {
                    t_newFolderModal.modal('hide');
                    processing.showPleaseWait();
                    var n_name = t_newFolderModal.find('.new-folder-name').val();
                    if (typeof (n_name) === 'undefined' || n_name === null || n_name === '')
                        n_name = "New Folder";
                    e.stopPropagation();
                    e.preventDefault();
                    var t_parentId = t_folder_id;
                    fs_data = { parent: t_folder_id, name: n_name };
                    AddJsonAntiForgeryToken(fs_data);
                    var fs_xhr = new XMLHttpRequest();
                    fs_xhr.onerror = function (result) {
                        processing.hidePleaseWait();
                        alert('Unhandled Server Error');
                    };
                    fs_xhr.onload = function (event) {
                        if (event.currentTarget.status == 200) {
                            var result = JSON.parse(fs_xhr.responseText);
                            if (result.Success) {
                                var t_id = result.Id;
                                var t_container = $('#' + t_parentId).parent().children('ul');
                                var n_folder_html = '<li class="parent_li"><span id="' + t_id + '" class="tree-folder tree-item tree-haschildren"><span class="glyphicon glyphicon-folder-close"></span><span class="tree-item-name">' + n_name + '</span></span><ul></ul></li>"';
                                var n_folder = $(n_folder_html);
                                t_container.append(n_folder);
                                n_folder.find('.tree-folder').on('contextmenu', function (e) {
                                    contextMenu(e, $(this));
                                }).on('click', function (e) {
                                    var children = $(this).parent('li.parent_li').find(' > ul > li');
                                    if (children.is(":visible")) {
                                        children.hide('fast');
                                        $(this).attr('title', 'Expand this branch').find('.glyphicon').addClass('glyphicon-folder-close').removeClass('glyphicon-folder-open');
                                    } else {
                                        children.show('fast');
                                        $(this).attr('title', 'Collapse this branch').find('.glyphicon').addClass('glyphicon-folder-open').removeClass('glyphicon-folder-close');
                                    }
                                    e.stopPropagation();
                                });
                                t_modal.find('.tree-folder[data-tree-folder-id="' + t_container + '"]').append('<li><span data-tree-folder-id="' + t_id + '">' + name + '</span><ul></ul><li>');
                                processing.hidePleaseWait();
                            } else {
                                processing.hidePleaseWait();
                                alert(result.Message);
                            }
                        }
                    };
                    fs_xhr.open(newFolderMethod, newFolderUrl, true);
                    fs_xhr.setRequestHeader('Content-Type', 'application/json');
                    fs_xhr.send(JSON.stringify(fs_data));
                });
            }

            // Remove contxt menu if anything else is clicked.
            $('html').on('click', function (e) {
                t_context.hide();
            });
            $('html').on('contextmenu', function (e) {
                t_context.hide();
            });

            function contextMenu(e, j_item) {
                e.stopPropagation();
                temp_item = j_item;
                t_context.append($('.tree-context-group[data-tree-context=' + trees + ']'));
                if (temp_item.hasClass('tree-folder')) {
                    t_folder_id = temp_item.attr('id');
                    t_context.children('ul').not('.tree-context-group-folder').appendTo('#' + hiddenDivId);
                    if (temp_item.closest('ul').parent().hasClass('tree'))
                        t_context.find('.tree-context-folder-move').hide();
                    else
                        t_context.find('.tree-context-folder-move').show();
                } else {
                    t_node_id = temp_item.attr('id');
                    t_context.children('ul').not('.tree-context-group-node').appendTo('#' + hiddenDivId);
                }
                e.preventDefault();
                t_context.css('left', e.pageX).css('top', e.pageY);
                t_context.show();

            }

        });
    };

}(jQuery));