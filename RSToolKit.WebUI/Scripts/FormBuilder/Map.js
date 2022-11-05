$(document).on('ready', function (e) {
    $('#autoMap').on('click', function (e) {
        e.preventDefault();
        var map = null;
        var variable = $('#Variable').val().toLowerCase();
        for (var i = 0; i < headers.length; i++) {
            var name = headers[i].Name.toLowerCase();
            if (name == variable) {
                $('#MappedToKey option[value="' + headers[i].UId + '"]').prop('selected', true);
                return;
            }
        }
        variableNoSPace = variable.replace(/\s/g, '');
        for (var i = 0; i < headers.length; i++) {
            var name = headers[i].Name.toLowerCase();
            if (name == variable) {
                $('#MappedToKey option[value="' + headers[i].UId + '"]').prop('selected', true);
                return;
            }
        }
        for (var i = 0; i < headers.length; i++) {
            var name = headers[i].Name.toLowerCase().replace(/\s/g, '');
            if (name == variable) {
                $('#MappedToKey option[value="' + headers[i].UId + '"]').prop('selected', true);
                return;
            }
        }
        for (var i = 0; i < headers.length; i++) {
            var name = headers[i].Name.toLowerCase().replace(/\s/g, '');
            if (name == variableNoSPace) {
                $('#MappedToKey option[value="' + headers[i].UId + '"]').prop('selected', true);
                return;
            }
        }

    });
});