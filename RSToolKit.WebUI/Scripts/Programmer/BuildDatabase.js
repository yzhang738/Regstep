/// <reference path="../jquery-2.0.3.intellisense.js" />
/// <reference path="../jquery-ui-vsdoc.js" />
/// <reference path="../jquery-2.0.3.min.map" />
/// <reference path="../jquery-2.0.3.js" />

var current = 0;
var retry = false;

$(document).ready(function () {
    $("#ProcessImage").hide();
    NextCall();
});

function NextCall() {
    if (current == -1) {
        $('#Console').append("<br />Operations Failed. Please Contact the Programming Team.");
        return;
    }
    switch (current) {
        case 0:
            $('#Message').html("<br />Building Database...");
            $('#ProcessImage').show({
                durration: 400,
            });
            $.ajax({
                url: "BuildDatabase?code=" + code,
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
                        $('#Console').append("<br />Database Creation Failed. System will retry.");
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
        case 1:
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
        case 2:
            $('#Message').html("<br />Inserting Data...");
            $('#ProcessImage').show({
                durration: 400,
            });
            $.ajax({
                url: "PopulateTables?code=" + code,
                success: function (data) {
                    if (data.success) {
                        retry = false;
                        current++;
                        $('#Console').append("<br />Table Population Successful.");
                        return NextCall();
                    } else {
                        if (retry) {
                            current == -1;
                        }
                        $('#Console').append("<br />Table Population Failed. System will retry.");
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
        default:
            $('#Console').append("<br />Database Creation Successful<br />Disconnected From Server<br />All Functions Complete.");
    }
}