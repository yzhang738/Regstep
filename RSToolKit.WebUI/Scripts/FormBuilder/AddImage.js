$(document).ready(function () {
    $('#form').on('submit', function () {
        var formdata = new FormData(); //FormData object
        var fileInput = $('#Image').prop('files');
        //Iterating through each files selected in fileInput
        for (i = 0; i < fileInput.length; i++) {
            //Appending each file to FormData object
              formdata.append(fileInput[i].name, fileInput[i]);
        }
        formdata.append("cid", $('#cid').val());
        //Creating an XMLHttpRequest and sending
        var xhr = new XMLHttpRequest();
        xhr.open('POST', '/FormBuilder/AddImageAjax');
        xhr.send(formdata);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                alert(xhr.responseText);
            }
        }
        return false;
    });
});