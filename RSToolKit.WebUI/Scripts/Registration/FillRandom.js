$('#randomFill').on('click', function (e) {
    e.preventDefault();
    $('input[type=text]').each(function () {
        var input = $(this);
        var type = input.attr('data-component-type');
        if (input.attr('data-component-adminonly') == 'true')
            return;
        switch (type) {
            case 'datetime':
            case 'date':
            case 'time':
                input.val(input.attr('data-date-mindate'));
                break;
            case 'phone':
                input.val(chance.phone());
                break;
            default:
                if (input.attr('type') == 'text')
                    input.val(chance.word());
                break;
        }
    });
    $('input[type=checkbox][data-component-required=true]').each(function () {
        var input = $(this);
        input.prop('checked', 'true');
    });
    $('select').each(function () {
        var input = $(this);
        var dom = input.get()[0];
        var options = dom.options;
        var random = Math.floor(Math.random() * (options.length - 1)) + 2;
        input.find(':nth-child(' + random + ')').prop('selected', true);
    });
});