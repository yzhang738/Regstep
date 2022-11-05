$(document).ready(function () {

    $('.UploadButton').on('click', function () {
        $('#Image').click();
    });

    $('#Image').on('change', function () {
        $('.ChosenFile').text($('#Image').val());
    });

    $('#fileForm').on('submit', function () {
        var formdata = new FormData(); //FormData object
        var fileInput = $('#Image').prop('files');
        //Iterating through each files selected in fileInput
        for (i = 0; i < fileInput.length; i++) {
            //Appending each file to FormData object
            if (!(/^image/.test(fileInput[i].type))) {
                $('.Error').html('The file must be an image.');
                continue;
            }
            var size = fileInput[i].size;
            if (size > (2 * 1024 * 1024)) {
                $('.Error').html('The file is too large.  You must upload a file less than 2MiB.');
                continue;
            }
            formdata.append(fileInput[i].name, fileInput[i]);
            $('.Error').html('');
        }
        formdata.append("cid", $('#cid').val());
        //Creating an XMLHttpRequest and sending
        var xhr = new XMLHttpRequest();
        xhr.open('POST', '/FormBuilder/AddImageAjax');
        xhr.send(formdata);
        $('#Loading').show();
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                $('.ChosenFile').html('No File Selected');
                $('#Loading').hide();
                GetImages();
            }
        }
        return false;
    });

    GetImages();

});

function GetImages() {
    $("#Images").html("");
    $('#Loading').show();
    $.ajax({
        url: "/FormBuilder/GetImages?cid=" + $('#cid').val(),
        success: function (data) {
            $('#Images').html(data);
            $('#Loading').hide();
        },
        type: 'GET'
    });
}