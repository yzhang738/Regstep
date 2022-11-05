/// <reference path="../jquery-2.0.3.intellisense.js" />
/// <reference path="../jquery-ui-vsdoc.js" />
/// <reference path="../jquery-2.0.3.min.map" />
/// <reference path="../jquery-2.0.3.js" />

var current = 0;
var retry = false;

$(document).ready(function () {
    $("#ProcessImage").hide();
    $('#Console').append("Ready");
    $('.Command').on('click', function () {
        var command = $(this).attr('data-command');
        RunCommand(command);
    });
});

function RunCommand(command) {
    var cmd = parseInt(command, 10);
    switch (cmd) {
        case 1:
            $('#ProcessImage').show({
                durration: 400,
            });
            $('#Message').html("Running Command - Delete Database");
            $('#Console').append("<br />Connecting to Server...");
            $.ajax({
                url: "DeleteDatabase",
                beforeSend: function () {
                    $('#Console').append("<br />Connected");
                    $('#Console').append("<br />Sending Command...");
                },
                success: function (data) {
                    if (data.success) {
                        current++;
                        retry = false;
                        $('#Console').append("<br />Database Successfully Deleted.");
                    } else {
                        if (retry) {
                            current == -1;
                        }
                        $('#Console').append("<br />Database Deletion Failed. System will retry.");
                        retry = true;
                    }
                    $('#ProcessImage').hide({
                        durration: 400,
                    });
                },
                type: 'GET'
            });
            $('#Console').append("<br />Ready");
            break;
        case 2:
            $('#ProcessImage').show({
                durration: 400,
            });
            $('#Message').html("Running Command - Create Database");
            $('#ProcessImage').show({
                durration: 400,
            });
            $('#Console').append("<br />Connecting to Server...");
            $('#Message').html("<br />Building Database...");
            $.ajax({
                url: "BuildDatabase",
                success: function (data) {
                    if (data.success) {
                        current++;
                        retry = false;
                        $('#Console').append("<br />Database Creation Successful.");
                        return NextCall();
                    } else {
                        if (retry) {
                            current == -1;
                        }
                        $('#Console').append("<br />Database Creation Failed.");
                        if (data.message != null) {
                            $('#Console').append(" " + data.message);
                        }
                    }
                    $('#ProcessImage').hide({
                        durration: 400,
                    });
                },
                type: 'GET'
            });
            $('#Message').html("<br />Building Tables...");
            $('#ProcessImage').show({
                durration: 400,
            });
            $.ajax({
                url: "BuildTables?code=" + code,
                success: function (data) {
                    if (data.success) {
                        current++;
                        retry = false;
                        $('#Console').append("<br />Table Creation Successful.");
                        return NextCall();
                    } else {
                        if (retry) {
                            current == -1;
                        }
                        $('#Console').append("<br />Table Creation Failed. System will retry.");
                        retry = true;
                        return NextCall();
                    }
                    $('#ProcessImage').hide({
                        durration: 400,
                    });
                },
                type: 'GET'
            });

            break;
    }
}