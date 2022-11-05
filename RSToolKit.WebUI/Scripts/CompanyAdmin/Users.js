var user;
$(document).ready(function () {

    $('#submitNewUser').on('click', function () {
        $('#addUser').modal('hide');
    });

    $('.addGroup, .addRole').on('click', function (e) {
        e.preventDefault();
        user = $(this).attr('data-user');
    });
    $('#setGroup').on('click', function () {
        var t_url = $(this).attr('data-url');
        var data = { id: user, groupKey: $('#addGroupSelect').val() };
        processing.showPleaseWait();
        $.ajax({
            url: t_url,
            type: "post",
            data: AddJsonAntiForgeryToken(data),
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    if (typeof (result.Location) == 'undefined' || result.Location === null || result.Location === undefined) {
                        processing.hidePleaseWait();
                    } else if (result.Location == 'refresh') {
                        window.location.reload();
                    } else {
                        window.location.replace(result.Location);
                    }
                } else {
                    alert(result.Message);
                    if (typeof (result.Location) == 'undefined' || result.Location === null || result.Location === undefined) {
                        processing.hidePleaseWait();
                    } else if (result.Location == 'refresh') {
                        window.location.reload();
                    } else {
                        window.location.replace(result.Location);
                    }
                }
            },
            error: function (result) {
                processing.hidePleaseWait();
                alert(t_failMessage);
            }
        });
    });

    $('#setRole').on('click', function () {
        var t_url = $(this).attr('data-url');
        var data = { id: user, role: $('#addRoleSelect').val() };
        processing.showPleaseWait();
        $.ajax({
            url: t_url,
            type: "post",
            data: AddJsonAntiForgeryToken(data),
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    if (typeof (result.Location) == 'undefined' || result.Location === null || result.Location === undefined) {
                        processing.hidePleaseWait();
                    } else if (result.Location == 'refresh') {
                        window.location.reload();
                    } else {
                        window.location.replace(result.Location);
                    }
                } else {
                    alert(result.Message);
                    if (typeof (result.Location) == 'undefined' || result.Location === null || result.Location === undefined) {
                        processing.hidePleaseWait();
                    } else if (result.Location == 'refresh') {
                        window.location.reload();
                    } else {
                        window.location.replace(result.Location);
                    }
                }
            },
            error: function (result) {
                processing.hidePleaseWait();
                alert(t_failMessage);
            }
        });
    });

});