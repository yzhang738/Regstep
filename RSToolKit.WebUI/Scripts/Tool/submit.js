$(document).on('ready', function () {
    $('#submit').on('click', function (e) {
        e.preventDefault();
        var object = {};
        object.emailType = emailType;
        object.email = $('#email').val();
        object.values = {};
        var valid = true;
        $('.form-item').each(function (i) {
            if ($(this).hasClass('required') && (typeof ($(this).val()) === 'undefined' || $(this).val() === '')) {
                valid = false;
                return;
            }
            if ($(this).attr('type') === 'checkbox' && !$(this).prop('checked')) {
                return;
            }
            if (typeof (object.values[$(this).attr('name')]) === 'undefined' || object.values[$(this).attr('name')] === null) {
                object.values[$(this).attr('name')] = $(this).val();
            } else {
                object.values[$(this).attr('name')] += ', ' + $(this).val();
            }
        });
        if (!valid) {
            $('#errors').html("You must fill out all required fields. Only checkboxes are not required.");
            return;
        }
        var object2 = $.extend({}, object);
        object.emailType = emailType + '-lead';
        object2.emailType = emailType + '-rs';
        object2.email = 'info@regstep.com';
        var process = '<section id="intro"><div class="container-fluid"><div class="container"><div class="row">';
        process += '<div class="col-xs-12"><h1>Processing Information</h1></div>';
        process += '<div class="col-sm-10 col-sm-offset-1 col-md-8 col-md-offset-2 add-padding-vertical"><p>We are processing the information you provided.  You will recieve an email shortly from us.  In a few seconds you will be redirected to a confirmation page.</p><p>We look forward to speaking with you.</p></div>';
        process += '</div></div></div></section>';
        $('#mainBody').html(process);
        $.ajax({
            url: 'https://devtoolkit.regstep.com/api/submit',
            contentType: 'application/json',
            data: JSON.stringify(object),
            dataType: 'json',
            type: 'POST'
        }).done(function (a, b, c) {
            window.location.href = successLocation;
        }).error(function (a, b, c) {
            window.location.href = successLocation;
        });
        $.ajax({
            url: 'https://devtoolkit.regstep.com/api/submit',
            contentType: 'application/json',
            data: JSON.stringify(object2),
            dataType: 'json',
            type: 'POST'
        });
    });
});