/***************************/
/* Editing Html Email      */
/* Coded by DJ Enzyme      */
/* Started 20141103        */
/***************************/

$(document).on('ready', function (e) {

    AddJsonAntiForgeryToken(email);

    $('#saveEmail').on('click', function (e) {
        e.preventDefault();
        SaveEmail();
    });

    /****************/
    /* Email Inputs */
    /****************/

    $('#Name').on('input', function (e) {
        email.Name = $(this).val();
    });
    $('#EmailListKey').on('change', function (e) {
        email.EmailListKey = $(this).val();
    });
    $('#Subject').on('input', function (e) {
        email.Subject = $(this).val();
    });
    $('#Description').on('input', function (e) {
        email.Description = $(this).val();
    });
    $('#From').on('input', function (e) {
        email.From = $(this).val();
    });
    $('#CC').on('input', function (e) {
        email.CC = $(this).val();
    });
    $('#BCC').on('input', function (e) {
        email.BCC = $(this).val();
    });
    $('#EmailType').on('change', function (e) {
        email.EmailType = $(this).val();
    });
    $('#SendTime').on('input', function (e) {
        email.SendTime = $(this).val();
    });
    $('#RepeatSending').on('change', function (e) {
        email.RepeatSending = $(this).is(':checked');
    });
    $('#SendTime').datetimepicker({
        autoclose: true,
        minuteStep: 15,
        format: 'M/D/YYYY hh:mm:00 A Z'
    }).on("dp.change", function (e) {
        email.SendTime = e.date.format('M/D/YYYY hh:mm:00 A Z');
    });

    $('#To').on('input', function (e) {
        email.To = $(this).val();
    });

    $('#b_testSend').on('click', function (e) {
        $('#m_testSend').modal('hide');
        TestSend();
    });

    $('#b_s_sendSettings').on('click', function (e) {
        var i_interval = $('#SendInterval');
        $('#m_sendSettings').modal('hide');
        var r_interval = i_interval.val();
        var m_interval = 0;
        var i_interval_suffix = i_interval.attr('data-interval');
        switch (i_interval_suffix) {
            case 'sec':
                m_interval = r_interval;
                break;
            case 'min':
                m_interval = r_interval * 60;
                break;
            case 'hr':
                m_interval = r_interval * 60 * 60;
                break;
            case 'day':
                m_interval = r_interval * 24 * 60 * 60;
                break;
            case 'mo':
                m_interval = r_interval * 30.5 * 24 * 60 * 60;
                break;
            case 'yr':
                m_interval = r_interval * 365 * 24 * 60 * 60;
                break;
        }
        email.IntervalSeconds = m_interval;
        email.MaxSends = $('#MaxSends').val();
    });

    $('#suffix_SendInterval').on('click', function (e) {
        var input = $('#SendInterval');
        var suffix = input.attr('data-interval');
        var n_suffix = '';
        switch (suffix) {
            case 'sec':
                n_suffix = 'min';
                break;
            case 'min':
                n_suffix = 'hr';
                break;
            case 'hr':
                n_suffix = 'day';
                break;
            case 'day':
                n_suffix = 'mo';
                break;
            case 'mo':
                n_suffix = 'yr';
                break;
            case 'yr':
                n_suffix = 'sec';
                break;
        }
        input.attr('data-interval', n_suffix);
        $(this).html(n_suffix);
    });

    $('#b_pullPlainText').on('click', function () {
        pullPlainText();
    });

    $('#b_plainText').on('click', function () {
        $(this).closest('.modal').modal('hide');
        email.PlainText = $('#plainText').val();
    });

    $('#b_c_plainText, #h_c_plainText').on('click', function () {
        $(this).closest('.modal').modal('hide');
        $('#plainText').val(email.PlainText);
    });

    $('#Html').on('input', function () {
        email.Html = $(this).val();
    });

    /********************/
    /* End Email Inputs */
    /********************/

});

function TestSend() {
    var t_xhr = new XMLHttpRequest();
    var t_data = { id: email.UId, to: $('#testSend_To').val() };
    AddJsonAntiForgeryToken(t_data);
    t_xhr.open('post', '../../Email/TestSend', true);
    t_xhr.setRequestHeader('Content-Type', 'application/json');
    t_xhr.onerror = function (result) {
        processing.hidePleaseWait();
        alert('Unhandled server error.');
    };
    t_xhr.onload = function (result) {
        if (result.currentTarget.status == 200) {
            var c_xhr = result.currentTarget;
            var result = JSON.parse(c_xhr.responseText);
            if (result.Success) {
                processing.hidePleaseWait();
            } else {
                processing.hidePleaseWait();
                alert(result.Message);
            }
        } else {
            processing.hidePleaseWait();
            alert('Page not found.');
        }
    };
    t_xhr.send(JSON.stringify(t_data));
}

/**************/
/* Save Email */
/**************/

function SaveEmail() {
    var t_xhr = new XMLHttpRequest();
    t_xhr.open('put', '../../Email/HtmlEmail', true);
    t_xhr.setRequestHeader('Content-Type', 'application/json');
    t_xhr.onerror = function (result) {
        processing.hidePleaseWait();
        alert('Unhandled server error.');
    };
    t_xhr.onload = function (result) {
        if (result.currentTarget.status == 200) {
            var c_xhr = result.currentTarget;
            var result = JSON.parse(c_xhr.responseText);
            if (result.Success) {
                processing.hidePleaseWait();
            } else {
                processing.hidePleaseWait();
                alert(result.Message);
            }
        } else {
            processing.hidePleaseWait();
            alert('Page not found.');
        }
    };
    processing.showPleaseWait();
    t_xhr.send(JSON.stringify(email));
}

/******************/
/* End Save Email */
/******************/

function pullPlainText() {
    // Set up the email areas.
    var t_plain = "";
    // This will hold the html email.
    var c_html = email.Html;
    // We will put the email into a jQuery object for easy access.
    var t_email = $(c_html);
    // Now we need to grab only the text;
    t_plain = $(c_html).text().trim().replace(/\s{2,}/g, '\r\n');;
    email.PlainText = t_plain;
    $('#plainText').val(t_plain);
}