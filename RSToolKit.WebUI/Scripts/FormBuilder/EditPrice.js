$(document).ready(function () {
    $('.delete-price').on('click', function (e) {
        e.preventDefault();
        removePrice(this);
    });
    $('.price-start').datetimepicker({
        autoclose: true
    });

    $('#newPrice').on('click', function () {
        var index = $('#prices tbody tr').length;
        var price = '<tr><td><input type="text" value="0.00" data-type="money" name="Price[' + index + '].Amount" class="price-ammount form-control input-sm" /><input type="hidden" name="Price[' + index + '].UId" value="" class="price-uid"></td>';
        price += '<td><input type="text" value="' + moment().format("M/D/YYYY h:mm A ZZ") + '" name="Price[' + index + '].Start" data-date-format="M/DD/YYYY H:MM A ZZ" data-type="datetime" class="price-start form-control input-sm" /></td>';
        price += '<td><a class="delete-price"><span class="glyphicon glyphicon-trash"></span> Delete</a></td></tr>';
        $('#prices tbody').append(price);
        $('#prices tbody tr').last().find('.delete-price').on('click', function (e) {
            e.preventDefault();
            removePrice(this);
        });
        $('#prices tr').last().find('.price-start').datetimepicker({
            autoclose: true
        }).data('DateTimePicker').setDate(moment());
    });
});

function removePrice(t) {
    var item = $(t).parent().parent();
    item.remove();
    $('#prices tr').each(function (i) {
        $(this).find('.price-uid').attr('name', 'Price[' + i + '].UId');
        $(this).find('.price-ammount').attr('name', 'Price[' + i + '].Ammount');
        $(this).find('.price-start').attr('name', 'Price[' + i + '].Start');
    });
}