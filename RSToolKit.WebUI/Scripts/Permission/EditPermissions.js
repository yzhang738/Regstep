$(document).on("ready", function () {

    for (var i = 0; i < currentSet.length; i++) {
        $('#' + currentSet[i].Owner).remove();
    }

    $('#showGroup').on('click', function (e) {
        e.preventDefault();
        
        $('#addGroup').modal('show');
    });

    $('.access-checkbox').on('change', function(e) {
        SendAccess($(this));
    });

    $('.access-delete').on('click', function (e) {
        DeleteAccess($(this));
    });


    $('#setGroup').on('click', function (e) {
        $('#addGroup').modal('hide');
        processing.showPleaseWait();
        var p_target = $('#customGroup').val();
        var p_name = $('#customGroup option:selected').html();
        var permissionSet = {};
        permissionSet.Target = nodeKey;
        permissionSet.Owner = p_target;
        permissionSet.OwnerName = p_name;
        permissionSet.Read = false;
        permissionSet.Write = false;
        permissionSet.Execute = false;
        permissionSet.Type = type;
        currentSet.push(permissionSet);
        var p_data = permissionSet;
        AddJsonAntiForgeryToken(p_data);
        var p_xhr = new XMLHttpRequest();
        p_xhr.open('put', window.location.origin + '/permission/set', true);
        p_xhr.setRequestHeader('Content-Type', 'application/json');
        p_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (event.currentTarget.status == 200) {
                var result = JSON.parse(c_xhr.responseText);
                if (result.Success) {
                    var html = '<tr id="tr_' + p_data.Owner + '" data-owner="' + p_data.Owner + '"><td>' + p_data.OwnerName + '</td><td><input type="checkbox" class="read-access access-checkbox"></td><td><input type="checkbox" class="write-access access-checkbox"></td><td><input type="checkbox" class="execute-access access-checkbox"></td><td><span class="glyphicon glyphicon-trash access-delete cursor-pointer"></span></td></tr>';
                    $('#permissions').append(html);
                    $('#tr_' + p_data.Owner).find('.access-checkbox').on('change', function (e) {
                        SendAccess($(this));
                    });
                    $('#tr_' + p_data.Owner).find('.access-delete').on('click', function (e) {
                        DeleteAccess($(this));
                    });
                    processing.hidePleaseWait();
                } else {
                    processing.hidePleaseWait();
                    alert(result.Message);
                }
            } else {
                processing.hidePleaseWait();
                alert("Unhandled Error");
            }
        };
        p_xhr.onerror = function (event) {
            processing.hidePleaseWait();
            alert("Unhandled Error");
        };
        p_xhr.send(JSON.stringify(p_data));
    });
});

function SendAccess(j_checkbox) {
    var tr = j_checkbox.parent().parent();
    var p_data = {};
    for (var i = 0; i < currentSet.length; i++) {
        if (tr.attr('data-owner') == currentSet[i].Owner) {
            p_data = currentSet[i];
            break;
        }
    }
    if (j_checkbox.hasClass('read-access')) {
        p_data.Read = j_checkbox.prop('checked');
    } else if (j_checkbox.hasClass('write-access')) {
        p_data.Write = j_checkbox.prop('checked');
    } else if (j_checkbox.hasClass('execute-access')) {
        p_data.Execute = j_checkbox.prop('checked');
    }
    AddJsonAntiForgeryToken(p_data);
    var p_xhr = new XMLHttpRequest();
    p_xhr.open('put', window.location.origin + '/permission/set', true);
    p_xhr.setRequestHeader('Content-Type', 'application/json');
    p_xhr.onload = function (event) {
        var c_xhr = event.currentTarget;
        if (event.currentTarget.status == 200) {
            var result = JSON.parse(c_xhr.responseText);
            if (result.Success) {
                //Do Nothing
            } else {
                alert(result.Message);
            }
        } else {
            alert("Unhandled Error");
        }
    };
    p_xhr.onerror = function (event) {
        processing.hidePleaseWait();
        alert("Unhandled Error");
    };
    p_xhr.send(JSON.stringify(p_data));
}

function DeleteAccess(j_span) {
    processing.showPleaseWait();
    var tr = j_span.parent().parent();
    var p_data = {};
    for (var i = 0; i < currentSet.length; i++) {
        if (tr.attr('data-owner') == currentSet[i].Owner) {
            p_data = currentSet[i];
            break;
        }
    }
    AddJsonAntiForgeryToken(p_data);
    var p_xhr = new XMLHttpRequest();
    p_xhr.open('delete', window.location.origin + '/permission/delete', true);
    p_xhr.setRequestHeader('Content-Type', 'application/json');
    p_xhr.onload = function (event) {
        var c_xhr = event.currentTarget;
        if (event.currentTarget.status == 200) {
            var result = JSON.parse(c_xhr.responseText);
            if (result.Success) {
                tr.remove();
                $('#customGroup').append('<option value="' + p_data.Owner + '" id="' + p_data.Owner + '">' + p_data.OwnerName + '</option>');
                processing.hidePleaseWait();
            } else {
                processing.hidePleaseWait();
                alert(result.Message);
            }
        } else {
            processing.hidePleaseWait();
            alert("Unhandled Error");
        }
    };
    p_xhr.onerror = function (event) {
        processing.hidePleaseWait();
        alert("Unhandled Error");
    };
    p_xhr.send(JSON.stringify(p_data));
}