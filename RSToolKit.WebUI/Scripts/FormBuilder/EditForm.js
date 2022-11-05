/// <reference path="jquery-2.0.3.js" />
/// <reference path="jquery-2.0.3.min.map" />
/// <reference path="Common.js" />
/// <reference path="../Moment/moment.js" />
/// <reference path="../jQuery/Plugins/disableSelect.js" />

$(document).ready(function () {
    $('.folder-link').on('click', function () {
        var newFolder = $(this).attr('data-id');
        $.ajax({
            url: '../Folder/MoveNode',
            type: "post",
            data: JSON.stringify({ target: formId, newFolder: newFolder }),
            contentType: "application/json",
            dataType: "json",
            traditional: "true",
            success: function (jsonResult) {
                if (jsonResult.Success) {
                    $(this).parents('.modal').modal('hide');
                    $('.folder-selected').removeClass('folder-selected');
                    $('.folder-selected-text').addClass('folder-not-selected');
                    $('.folder-link[data-id="' + newFolder + '"]').addClass('folder-slected').parent().children('.folder-selected-text').removeClass('folder-not-selected');
                    $('#folderName').html(jsonResult.FolderName);
                } else {
                    alert(jsonResult.Message);
                }
            },
            error: function (result) {
                alert(result);
            }
        });
    });

    $('#addTags').on('click', function () {
        $('.tags-notSelected > .tags > .tag.tag-visible > .tag-input:checked').each(function () {
            $(this).parent().removeClass('tag-visible').addClass('tag-hidden');
            $('.tags-selected > .tags > .tag[data-id="' + $(this).attr('data-id') + '"]').addClass('tag-visible').removeClass('tag-hidden');
            $(this).attr('checked', false);
        });
        CompileSelectedTags();
    });
    $('#removeTags').on('click', function () {
        $('.tags-selected > .tags > .tag.tag-visible > .tag-input:checked').each(function () {
            $(this).parent().removeClass('tag-visible').addClass('tag-hidden');
            $('.tags-notSelected > .tags > .tag[data-id="' + $(this).attr('data-id') + '"]').addClass('tag-visible').removeClass('tag-hidden');
            $(this).attr('checked', false);
        });
        CompileSelectedTags();
    });

    $('#accessType').on('change', function (event) {
        if ($(this).val() === "2") {
            $('.access-custom-login').show('slow');
        } else {
            $('.access-custom-login').hide('slow');
        }
    });

    $('.remove-column').on('click', function (event) {
        var t_parent = $(this).closest('li');
        var t_rid = $(this).find('input').val();
        $('#accessColumn').children('option[value="' + t_rid + '"]').show();
        t_parent.remove();
        $('#custom-login-columns').children('li').each(function (ind, el) {
            $(el).find('input').attr('name', 'LoginHeaders[' + ind + ']');
            $(el).attr('id', 'ColumnHeader_' + ind);
        });
    });


    $('#addAccessColumn').on('click', function (event) {
        event.preventDefault();
        var t_column = $('#accessColumn').children('option:selected');
        var t_id = t_column.attr('value');
        var t_name = t_column.html();
        var t_index = 0;
        t_index = $('#custom-login-columns').children('li').length;
        $('#custom-login-columns').append('<li class="column-access" id="ColumnHeader_' + t_index + '"><input type="hidden" name="LoginHeaders[' + t_index + ']" value="' + t_id + '">' + t_name + ' <span class="remove-column text-danger text-underline cursor-pointer">remove</span></li>');
        $('#ColumnHeader_' + t_index).find('.remove-column').on('click', function (event) {
            var t_parent = $(this).closest('li');
            var t_rid = $(this).find('input').val();
            $('#accessColumn').children('option[value="' + t_rid + '"]').show();
            t_parent.remove();
            $('#custom-login-columns').children('li').each(function (ind, el) {
                $(el).find('input').attr('name', 'LoginHeaders[' + ind + ']');
                $(el).attr('id', 'ColumnHeader_' + ind);
            });
        });
    });

    $('#dtPickerOpen').datetimepicker({
        pickerPosition: "bottom-left",
        maxView: 3,
        minuteStep: 1
    }).on("dp.change", function (e) {
        $('#OpenStatic').html(e.date.format('M/D/YYYY hh:mm:00 A Z'));
        $('#Open').val(e.date.format('M/D/YYYY hh:mm:00 A Z'));
    });
    $('#dtPickerClose').datetimepicker({
        autoclose: true,
        pickerPosition: "bottom-left",
        maxView: 3,
        minuteStep: 1
    }).on("dp.change", function (e) {
        $('#CloseStatic').html(e.date.format('M/D/YYYY hh:mm:00 A Z'));
        $('#Close').val(e.date.format('M/D/YYYY hh:mm:00 A Z'));
    });

    

    $('.datetimepicker').datetimepicker({
    });
    var evt = $('#EventTimeZone');
    var t_val = evt.attr('value');
    console.debug(t_val);
    evt.html('');
    for (var tz_i = 0; tz_i < timezones.length; tz_i++) {
        evt.append('<option value="' + timezones[tz_i].label + '"' + (t_val === timezones[tz_i].label ? ' selected="selected"' : '') + '>' + timezones[tz_i].label + '</option>');
    }

    $('ol.pages-sortable').sortable({
        handle: 'span.icon-move',
        onDrop: function  (item, targetContainer, _super) {
            var clonedItem = $('<li/>').css({height: 0})
            item.before(clonedItem)
            clonedItem.animate({'height': item.height()})
    
            item.animate(clonedItem.position(), function () {
                clonedItem.detach()
                _super(item)
                $('.pages-sortable li').each(function (i) {
                    var pageNumber = i + 3;
                    var index = i + 2;
                    if (f_survey) {
                        pageNumber = i + 1;
                        index = i;
                    }
                    $(this).find('input.page-number').val(pageNumber);
                    $(this).find('.form-page-number').html('Page ' + pageNumber + ':');
                });
            });

        }
    }).disableSelection();
});

function CompileSelectedTags() {
    var tags = [];
    $('.tags-selected > .tags > .tag.tag-visible').each(function () {
        tags.push($(this).attr('data-id'));
    });
    $('input[name=tags]').val(JSON.stringify(tags));
}

var timezones = JSON.parse('[{ "label": "Africa/Abidjan", "offset": "00:00" }, { "label": "Africa/Accra", "offset": "00:00" }, { "label": "Africa/Addis_Ababa", "offset": "+03:00" }, { "label": "Africa/Algiers", "offset": "+01:00" }, { "label": "Africa/Asmara", "offset": "+03:00" }, { "label": "Africa/Bamako", "offset": "00:00" }, { "label": "Africa/Bangui", "offset": "+01:00" }, { "label": "Africa/Banjul", "offset": "00:00" }, { "label": "Africa/Bissau", "offset": "00:00" }, { "label": "Africa/Blantyre", "offset": "+02:00" }, { "label": "Africa/Brazzaville", "offset": "+01:00" }, { "label": "Africa/Bujumbura", "offset": "+02:00" }, { "label": "Africa/Cairo", "offset": "+02:00" }, { "label": "Africa/Casablanca", "offset": "00:00" }, { "label": "Africa/Ceuta", "offset": "+01:00" }, { "label": "Africa/Conakry", "offset": "00:00" }, { "label": "Africa/Dakar", "offset": "00:00" }, { "label": "Africa/Dar_es_Salaam", "offset": "+03:00" }, { "label": "Africa/Djibouti", "offset": "+03:00" }, { "label": "Africa/Douala", "offset": "+01:00" }, { "label": "Africa/El_Aaiun", "offset": "00:00" }, { "label": "Africa/Freetown", "offset": "00:00" }, { "label": "Africa/Gaborone", "offset": "+02:00" }, { "label": "Africa/Harare", "offset": "+02:00" }, { "label": "Africa/Johannesburg", "offset": "+02:00" }, { "label": "Africa/Juba", "offset": "+03:00" }, { "label": "Africa/Kampala", "offset": "+03:00" }, { "label": "Africa/Khartoum", "offset": "+03:00" }, { "label": "Africa/Kigali", "offset": "+02:00" }, { "label": "Africa/Kinshasa", "offset": "+01:00" }, { "label": "Africa/Lagos", "offset": "+01:00" }, { "label": "Africa/Libreville", "offset": "+01:00" }, { "label": "Africa/Lome", "offset": "00:00" }, { "label": "Africa/Luanda", "offset": "+01:00" }, { "label": "Africa/Lubumbashi", "offset": "+02:00" }, { "label": "Africa/Lusaka", "offset": "+02:00" }, { "label": "Africa/Malabo", "offset": "+01:00" }, { "label": "Africa/Maputo", "offset": "+02:00" }, { "label": "Africa/Maseru", "offset": "+02:00" }, { "label": "Africa/Mbabane", "offset": "+02:00" }, { "label": "Africa/Mogadishu", "offset": "+03:00" }, { "label": "Africa/Monrovia", "offset": "00:00" }, { "label": "Africa/Nairobi", "offset": "+03:00" }, { "label": "Africa/Ndjamena", "offset": "+01:00" }, { "label": "Africa/Niamey", "offset": "+01:00" }, { "label": "Africa/Nouakchott", "offset": "00:00" }, { "label": "Africa/Ouagadougou", "offset": "00:00" }, { "label": "Africa/Porto-Novo", "offset": "+01:00" }, { "label": "Africa/Sao_Tome", "offset": "00:00" }, { "label": "Africa/Tripoli", "offset": "+02:00" }, { "label": "Africa/Tunis", "offset": "+01:00" }, { "label": "Africa/Windhoek", "offset": "+01:00" }, { "label": "America/Adak", "offset": "-10:00" }, { "label": "America/Anchorage", "offset": "-09:00" }, { "label": "America/Anguilla", "offset": "-04:00" }, { "label": "America/Antigua", "offset": "-04:00" }, { "label": "America/Araguaina", "offset": "-03:00" }, { "label": "America/Argentina/Buenos_Aires", "offset": "-03:00" }, { "label": "America/Argentina/Catamarca", "offset": "-03:00" }, { "label": "America/Argentina/Cordoba", "offset": "-03:00" }, { "label": "America/Argentina/Jujuy", "offset": "-03:00" }, { "label": "America/Argentina/La_Rioja", "offset": "-03:00" }, { "label": "America/Argentina/Mendoza", "offset": "-03:00" }, { "label": "America/Argentina/Rio_Gallegos", "offset": "-03:00" }, { "label": "America/Argentina/Salta", "offset": "-03:00" }, { "label": "America/Argentina/San_Juan", "offset": "-03:00" }, { "label": "America/Argentina/San_Luis", "offset": "-03:00" }, { "label": "America/Argentina/Tucuman", "offset": "-03:00" }, { "label": "America/Argentina/Ushuaia", "offset": "-03:00" }, { "label": "America/Aruba", "offset": "-04:00" }, { "label": "America/Asuncion", "offset": "-04:00" }, { "label": "America/Atikokan", "offset": "-05:00" }, { "label": "America/Bahia", "offset": "-03:00" }, { "label": "America/Bahia_Banderas", "offset": "-06:00" }, { "label": "America/Barbados", "offset": "-04:00" }, { "label": "America/Belem", "offset": "-03:00" }, { "label": "America/Belize", "offset": "-06:00" }, { "label": "America/Blanc-Sablon", "offset": "-04:00" }, { "label": "America/Boa_Vista", "offset": "-04:00" }, { "label": "America/Bogota", "offset": "-05:00" }, { "label": "America/Boise", "offset": "-07:00" }, { "label": "America/Cambridge_Bay", "offset": "-07:00" }, { "label": "America/Campo_Grande", "offset": "-04:00" }, { "label": "America/Cancun", "offset": "-06:00" }, { "label": "America/Caracas", "offset": "-04:30" }, { "label": "America/Cayenne", "offset": "-03:00" }, { "label": "America/Cayman", "offset": "-05:00" }, { "label": "America/Chicago", "offset": "-06:00" }, { "label": "America/Chihuahua", "offset": "-07:00" }, { "label": "America/Costa_Rica", "offset": "-06:00" }, { "label": "America/Creston", "offset": "-07:00" }, { "label": "America/Cuiaba", "offset": "-04:00" }, { "label": "America/Curacao", "offset": "-04:00" }, { "label": "America/Danmarkshavn", "offset": "00:00" }, { "label": "America/Dawson", "offset": "-08:00" }, { "label": "America/Dawson_Creek", "offset": "-07:00" }, { "label": "America/Denver", "offset": "-07:00" }, { "label": "America/Detroit", "offset": "-05:00" }, { "label": "America/Dominica", "offset": "-04:00" }, { "label": "America/Edmonton", "offset": "-07:00" }, { "label": "America/Eirunepe", "offset": "-05:00" }, { "label": "America/El_Salvador", "offset": "-06:00" }, { "label": "America/Fortaleza", "offset": "-03:00" }, { "label": "America/Glace_Bay", "offset": "-04:00" }, { "label": "America/Godthab", "offset": "-03:00" }, { "label": "America/Goose_Bay", "offset": "-04:00" }, { "label": "America/Grand_Turk", "offset": "-04:00" }, { "label": "America/Grenada", "offset": "-04:00" }, { "label": "America/Guadeloupe", "offset": "-04:00" }, { "label": "America/Guatemala", "offset": "-06:00" }, { "label": "America/Guayaquil", "offset": "-05:00" }, { "label": "America/Guyana", "offset": "-04:00" }, { "label": "America/Halifax", "offset": "-04:00" }, { "label": "America/Havana", "offset": "-05:00" }, { "label": "America/Hermosillo", "offset": "-07:00" }, { "label": "America/Indiana/Indianapolis", "offset": "-05:00" }, { "label": "America/Indiana/Knox", "offset": "-06:00" }, { "label": "America/Indiana/Marengo", "offset": "-05:00" }, { "label": "America/Indiana/Petersburg", "offset": "-05:00" }, { "label": "America/Indiana/Tell_City", "offset": "-06:00" }, { "label": "America/Indiana/Vevay", "offset": "-05:00" }, { "label": "America/Indiana/Vincennes", "offset": "-05:00" }, { "label": "America/Indiana/Winamac", "offset": "-05:00" }, { "label": "America/Inuvik", "offset": "-07:00" }, { "label": "America/Iqaluit", "offset": "-05:00" }, { "label": "America/Jamaica", "offset": "-05:00" }, { "label": "America/Juneau", "offset": "-09:00" }, { "label": "America/Kentucky/Louisville", "offset": "-05:00" }, { "label": "America/Kentucky/Monticello", "offset": "-05:00" }, { "label": "America/Kralendijk", "offset": "-04:00" }, { "label": "America/La_Paz", "offset": "-04:00" }, { "label": "America/Lima", "offset": "-05:00" }, { "label": "America/Los_Angeles", "offset": "-08:00" }, { "label": "America/Lower_Princes", "offset": "-04:00" }, { "label": "America/Maceio", "offset": "-03:00" }, { "label": "America/Managua", "offset": "-06:00" }, { "label": "America/Manaus", "offset": "-04:00" }, { "label": "America/Marigot", "offset": "-04:00" }, { "label": "America/Martinique", "offset": "-04:00" }, { "label": "America/Matamoros", "offset": "-06:00" }, { "label": "America/Mazatlan", "offset": "-07:00" }, { "label": "America/Menominee", "offset": "-06:00" }, { "label": "America/Merida", "offset": "-06:00" }, { "label": "America/Metlakatla", "offset": "-08:00" }, { "label": "America/Mexico_City", "offset": "-06:00" }, { "label": "America/Miquelon", "offset": "-03:00" }, { "label": "America/Moncton", "offset": "-04:00" }, { "label": "America/Monterrey", "offset": "-06:00" }, { "label": "America/Montevideo", "offset": "-03:00" }, { "label": "America/Montserrat", "offset": "-04:00" }, { "label": "America/Nassau", "offset": "-05:00" }, { "label": "America/New_York", "offset": "-05:00" }, { "label": "America/Nipigon", "offset": "-05:00" }, { "label": "America/Nome", "offset": "-09:00" }, { "label": "America/Noronha", "offset": "-02:00" }, { "label": "America/North_Dakota/Beulah", "offset": "-06:00" }, { "label": "America/North_Dakota/Center", "offset": "-06:00" }, { "label": "America/North_Dakota/New_Salem", "offset": "-06:00" }, { "label": "America/Ojinaga", "offset": "-07:00" }, { "label": "America/Panama", "offset": "-05:00" }, { "label": "America/Pangnirtung", "offset": "-05:00" }, { "label": "America/Paramaribo", "offset": "-03:00" }, { "label": "America/Phoenix", "offset": "-07:00" }, { "label": "America/Port-au-Prince", "offset": "-05:00" }, { "label": "America/Port_of_Spain", "offset": "-04:00" }, { "label": "America/Porto_Velho", "offset": "-04:00" }, { "label": "America/Puerto_Rico", "offset": "-04:00" }, { "label": "America/Rainy_River", "offset": "-06:00" }, { "label": "America/Rankin_Inlet", "offset": "-06:00" }, { "label": "America/Recife", "offset": "-03:00" }, { "label": "America/Regina", "offset": "-06:00" }, { "label": "America/Resolute", "offset": "-06:00" }, { "label": "America/Rio_Branco", "offset": "-05:00" }, { "label": "America/Santa_Isabel", "offset": "-08:00" }, { "label": "America/Santarem", "offset": "-03:00" }, { "label": "America/Santiago", "offset": "-04:00" }, { "label": "America/Santo_Domingo", "offset": "-04:00" }, { "label": "America/Sao_Paulo", "offset": "-03:00" }, { "label": "America/Scoresbysund", "offset": "-01:00" }, { "label": "America/Sitka", "offset": "-09:00" }, { "label": "America/St_Barthelemy", "offset": "-04:00" }, { "label": "America/St_Johns", "offset": "-03:30" }, { "label": "America/St_Kitts", "offset": "-04:00" }, { "label": "America/St_Lucia", "offset": "-04:00" }, { "label": "America/St_Thomas", "offset": "-04:00" }, { "label": "America/St_Vincent", "offset": "-04:00" }, { "label": "America/Swift_Current", "offset": "-06:00" }, { "label": "America/Tegucigalpa", "offset": "-06:00" }, { "label": "America/Thule", "offset": "-04:00" }, { "label": "America/Thunder_Bay", "offset": "-05:00" }, { "label": "America/Tijuana", "offset": "-08:00" }, { "label": "America/Toronto", "offset": "-05:00" }, { "label": "America/Tortola", "offset": "-04:00" }, { "label": "America/Vancouver", "offset": "-08:00" }, { "label": "America/Whitehorse", "offset": "-08:00" }, { "label": "America/Winnipeg", "offset": "-06:00" }, { "label": "America/Yakutat", "offset": "-09:00" }, { "label": "America/Yellowknife", "offset": "-07:00" }, { "label": "Antarctica/Casey", "offset": "+08:00" }, { "label": "Antarctica/Davis", "offset": "+07:00" }, { "label": "Antarctica/DumontDUrville", "offset": "+10:00" }, { "label": "Antarctica/Macquarie", "offset": "+11:00" }, { "label": "Antarctica/Mawson", "offset": "+05:00" }, { "label": "Antarctica/McMurdo", "offset": "+12:00" }, { "label": "Antarctica/Palmer", "offset": "-04:00" }, { "label": "Antarctica/Rothera", "offset": "-03:00" }, { "label": "Antarctica/Syowa", "offset": "+03:00" }, { "label": "Antarctica/Troll", "offset": "+01:00" }, { "label": "Antarctica/Vostok", "offset": "+06:00" }, { "label": "Arctic/Longyearbyen", "offset": "+01:00" }, { "label": "Asia/Aden", "offset": "+03:00" }, { "label": "Asia/Almaty", "offset": "+06:00" }, { "label": "Asia/Amman", "offset": "+02:00" }, { "label": "Asia/Anadyr", "offset": "+12:00" }, { "label": "Asia/Aqtau", "offset": "+05:00" }, { "label": "Asia/Aqtobe", "offset": "+05:00" }, { "label": "Asia/Ashgabat", "offset": "+05:00" }, { "label": "Asia/Baghdad", "offset": "+03:00" }, { "label": "Asia/Bahrain", "offset": "+03:00" }, { "label": "Asia/Baku", "offset": "+04:00" }, { "label": "Asia/Bangkok", "offset": "+07:00" }, { "label": "Asia/Beirut", "offset": "+02:00" }, { "label": "Asia/Bishkek", "offset": "+06:00" }, { "label": "Asia/Brunei", "offset": "+08:00" }, { "label": "Asia/Chita", "offset": "+08:00" }, { "label": "Asia/Choibalsan", "offset": "+08:00" }, { "label": "Asia/Colombo", "offset": "+05:30" }, { "label": "Asia/Damascus", "offset": "+02:00" }, { "label": "Asia/Dhaka", "offset": "+06:00" }, { "label": "Asia/Dili", "offset": "+09:00" }, { "label": "Asia/Dubai", "offset": "+04:00" }, { "label": "Asia/Dushanbe", "offset": "+05:00" }, { "label": "Asia/Gaza", "offset": "+02:00" }, { "label": "Asia/Hebron", "offset": "+02:00" }, { "label": "Asia/Ho_Chi_Minh", "offset": "+07:00" }, { "label": "Asia/Hong_Kong", "offset": "+08:00" }, { "label": "Asia/Hovd", "offset": "+07:00" }, { "label": "Asia/Irkutsk", "offset": "+08:00" }, { "label": "Asia/Jakarta", "offset": "+07:00" }, { "label": "Asia/Jayapura", "offset": "+09:00" }, { "label": "Asia/Jerusalem", "offset": "+02:00" }, { "label": "Asia/Kabul", "offset": "+04:30" }, { "label": "Asia/Kamchatka", "offset": "+12:00" }, { "label": "Asia/Karachi", "offset": "+05:00" }, { "label": "Asia/Kathmandu", "offset": "+05:00" }, { "label": "Asia/Khandyga", "offset": "+09:00" }, { "label": "Asia/Kolkata", "offset": "+05:30" }, { "label": "Asia/Krasnoyarsk", "offset": "+07:00" }, { "label": "Asia/Kuala_Lumpur", "offset": "+08:00" }, { "label": "Asia/Kuching", "offset": "+08:00" }, { "label": "Asia/Kuwait", "offset": "+03:00" }, { "label": "Asia/Macau", "offset": "+08:00" }, { "label": "Asia/Magadan", "offset": "+10:00" }, { "label": "Asia/Makassar", "offset": "+08:00" }, { "label": "Asia/Manila", "offset": "+08:00" }, { "label": "Asia/Muscat", "offset": "+04:00" }, { "label": "Asia/Nicosia", "offset": "+02:00" }, { "label": "Asia/Novokuznetsk", "offset": "+07:00" }, { "label": "Asia/Novosibirsk", "offset": "+06:00" }, { "label": "Asia/Omsk", "offset": "+06:00" }, { "label": "Asia/Oral", "offset": "+05:00" }, { "label": "Asia/Phnom_Penh", "offset": "+07:00" }, { "label": "Asia/Pontianak", "offset": "+07:00" }, { "label": "Asia/Pyongyang", "offset": "+09:00" }, { "label": "Asia/Qatar", "offset": "+03:00" }, { "label": "Asia/Qyzylorda", "offset": "+06:00" }, { "label": "Asia/Rangoon", "offset": "+06:30" }, { "label": "Asia/Riyadh", "offset": "+03:00" }, { "label": "Asia/Sakhalin", "offset": "+10:00" }, { "label": "Asia/Samarkand", "offset": "+05:00" }, { "label": "Asia/Seoul", "offset": "+09:00" }, { "label": "Asia/Shanghai", "offset": "+08:00" }, { "label": "Asia/Singapore", "offset": "+08:00" }, { "label": "Asia/Srednekolymsk", "offset": "+11:00" }, { "label": "Asia/Taipei", "offset": "+08:00" }, { "label": "Asia/Tashkent", "offset": "+05:00" }, { "label": "Asia/Tbilisi", "offset": "+04:00" }, { "label": "Asia/Tehran", "offset": "+03:30" }, { "label": "Asia/Thimphu", "offset": "+06:00" }, { "label": "Asia/Tokyo", "offset": "+09:00" }, { "label": "Asia/Ulaanbaatar", "offset": "+08:00" }, { "label": "Asia/Urumqi", "offset": "+06:00" }, { "label": "Asia/Ust-Nera", "offset": "+10:00" }, { "label": "Asia/Vientiane", "offset": "+07:00" }, { "label": "Asia/Vladivostok", "offset": "+10:00" }, { "label": "Asia/Yakutsk", "offset": "+09:00" }, { "label": "Asia/Yekaterinburg", "offset": "+05:00" }, { "label": "Asia/Yerevan", "offset": "+04:00" }, { "label": "Atlantic/Azores", "offset": "-01:00" }, { "label": "Atlantic/Bermuda", "offset": "-04:00" }, { "label": "Atlantic/Canary", "offset": "00:00" }, { "label": "Atlantic/Cape_Verde", "offset": "-01:00" }, { "label": "Atlantic/Faroe", "offset": "00:00" }, { "label": "Atlantic/Madeira", "offset": "00:00" }, { "label": "Atlantic/Reykjavik", "offset": "00:00" }, { "label": "Atlantic/South_Georgia", "offset": "-02:00" }, { "label": "Atlantic/St_Helena", "offset": "00:00" }, { "label": "Atlantic/Stanley", "offset": "-03:00" }, { "label": "Australia/Adelaide", "offset": "+09:30" }, { "label": "Australia/Brisbane", "offset": "+10:00" }, { "label": "Australia/Broken_Hill", "offset": "+09:30" }, { "label": "Australia/Currie", "offset": "+10:00" }, { "label": "Australia/Darwin", "offset": "+09:30" }, { "label": "Australia/Eucla", "offset": "+08:00" }, { "label": "Australia/Hobart", "offset": "+10:00" }, { "label": "Australia/Lindeman", "offset": "+10:00" }, { "label": "Australia/Lord_Howe", "offset": "+10:00" }, { "label": "Australia/Melbourne", "offset": "+10:00" }, { "label": "Australia/Perth", "offset": "+08:00" }, { "label": "Australia/Sydney", "offset": "+10:00" }, { "label": "Europe/Amsterdam", "offset": "+01:00" }, { "label": "Europe/Andorra", "offset": "+01:00" }, { "label": "Europe/Athens", "offset": "+02:00" }, { "label": "Europe/Belgrade", "offset": "+01:00" }, { "label": "Europe/Berlin", "offset": "+01:00" }, { "label": "Europe/Bratislava", "offset": "+01:00" }, { "label": "Europe/Brussels", "offset": "+01:00" }, { "label": "Europe/Bucharest", "offset": "+02:00" }, { "label": "Europe/Budapest", "offset": "+01:00" }, { "label": "Europe/Busingen", "offset": "+01:00" }, { "label": "Europe/Chisinau", "offset": "+02:00" }, { "label": "Europe/Copenhagen", "offset": "+01:00" }, { "label": "Europe/Dublin", "offset": "00:00" }, { "label": "Europe/Gibraltar", "offset": "+01:00" }, { "label": "Europe/Guernsey", "offset": "00:00" }, { "label": "Europe/Helsinki", "offset": "+02:00" }, { "label": "Europe/Isle_of_Man", "offset": "00:00" }, { "label": "Europe/Istanbul", "offset": "+02:00" }, { "label": "Europe/Jersey", "offset": "00:00" }, { "label": "Europe/Kaliningrad", "offset": "+02:00" }, { "label": "Europe/Kiev", "offset": "+02:00" }, { "label": "Europe/Lisbon", "offset": "00:00" }, { "label": "Europe/Ljubljana", "offset": "+01:00" }, { "label": "Europe/London", "offset": "00:00" }, { "label": "Europe/Luxembourg", "offset": "+01:00" }, { "label": "Europe/Madrid", "offset": "+01:00" }, { "label": "Europe/Malta", "offset": "+01:00" }, { "label": "Europe/Mariehamn", "offset": "+02:00" }, { "label": "Europe/Minsk", "offset": "+03:00" }, { "label": "Europe/Monaco", "offset": "+01:00" }, { "label": "Europe/Moscow", "offset": "+03:00" }, { "label": "Europe/Oslo", "offset": "+01:00" }, { "label": "Europe/Paris", "offset": "+01:00" }, { "label": "Europe/Podgorica", "offset": "+01:00" }, { "label": "Europe/Prague", "offset": "+01:00" }, { "label": "Europe/Riga", "offset": "+02:00" }, { "label": "Europe/Rome", "offset": "+01:00" }, { "label": "Europe/Samara", "offset": "+04:00" }, { "label": "Europe/San_Marino", "offset": "+01:00" }, { "label": "Europe/Sarajevo", "offset": "+01:00" }, { "label": "Europe/Simferopol", "offset": "+03:00" }, { "label": "Europe/Skopje", "offset": "+01:00" }, { "label": "Europe/Sofia", "offset": "+02:00" }, { "label": "Europe/Stockholm", "offset": "+01:00" }, { "label": "Europe/Tallinn", "offset": "+02:00" }, { "label": "Europe/Tirane", "offset": "+01:00" }, { "label": "Europe/Uzhgorod", "offset": "+02:00" }, { "label": "Europe/Vaduz", "offset": "+01:00" }, { "label": "Europe/Vatican", "offset": "+01:00" }, { "label": "Europe/Vienna", "offset": "+01:00" }, { "label": "Europe/Vilnius", "offset": "+02:00" }, { "label": "Europe/Volgograd", "offset": "+03:00" }, { "label": "Europe/Warsaw", "offset": "+01:00" }, { "label": "Europe/Zagreb", "offset": "+01:00" }, { "label": "Europe/Zaporozhye", "offset": "+02:00" }, { "label": "Europe/Zurich", "offset": "+01:00" }, { "label": "Indian/Antananarivo", "offset": "+03:00" }, { "label": "Indian/Chagos", "offset": "+06:00" }, { "label": "Indian/Christmas", "offset": "+07:00" }, { "label": "Indian/Cocos", "offset": "+06:30" }, { "label": "Indian/Comoro", "offset": "+03:00" }, { "label": "Indian/Kerguelen", "offset": "+05:00" }, { "label": "Indian/Mahe", "offset": "+04:00" }, { "label": "Indian/Maldives", "offset": "+05:00" }, { "label": "Indian/Mauritius", "offset": "+04:00" }, { "label": "Indian/Mayotte", "offset": "+03:00" }, { "label": "Indian/Reunion", "offset": "+04:00" }, { "label": "Pacific/Apia", "offset": "+13:00" }, { "label": "Pacific/Auckland", "offset": "+12:00" }, { "label": "Pacific/Bougainville", "offset": "+11:00" }, { "label": "Pacific/Chatham", "offset": "+12:00" }, { "label": "Pacific/Chuuk", "offset": "+10:00" }, { "label": "Pacific/Easter", "offset": "-06:00" }, { "label": "Pacific/Efate", "offset": "+11:00" }, { "label": "Pacific/Enderbury", "offset": "+13:00" }, { "label": "Pacific/Fakaofo", "offset": "+13:00" }, { "label": "Pacific/Fiji", "offset": "+12:00" }, { "label": "Pacific/Funafuti", "offset": "+12:00" }, { "label": "Pacific/Galapagos", "offset": "-06:00" }, { "label": "Pacific/Gambier", "offset": "-08:00" }, { "label": "Pacific/Guadalcanal", "offset": "+11:00" }, { "label": "Pacific/Guam", "offset": "+10:00" }, { "label": "Pacific/Honolulu", "offset": "-10:00" }, { "label": "Pacific/Johnston", "offset": "-10:00" }, { "label": "Pacific/Kiritimati", "offset": "+14:00" }, { "label": "Pacific/Kosrae", "offset": "+11:00" }, { "label": "Pacific/Kwajalein", "offset": "+12:00" }, { "label": "Pacific/Majuro", "offset": "+12:00" }, { "label": "Pacific/Marquesas", "offset": "-09:30" }, { "label": "Pacific/Midway", "offset": "-11:00" }, { "label": "Pacific/Nauru", "offset": "+12:00" }, { "label": "Pacific/Niue", "offset": "-11:00" }, { "label": "Pacific/Norfolk", "offset": "+11:30" }, { "label": "Pacific/Noumea", "offset": "+11:00" }, { "label": "Pacific/Pago_Pago", "offset": "-11:00" }, { "label": "Pacific/Palau", "offset": "+09:00" }, { "label": "Pacific/Pitcairn", "offset": "-08:00" }, { "label": "Pacific/Pohnpei", "offset": "+11:00" }, { "label": "Pacific/Port_Moresby", "offset": "+10:00" }, { "label": "Pacific/Rarotonga", "offset": "-10:00" }, { "label": "Pacific/Saipan", "offset": "+10:00" }, { "label": "Pacific/Tahiti", "offset": "-10:00" }, { "label": "Pacific/Tarawa", "offset": "+12:00" }, { "label": "Pacific/Tongatapu", "offset": "+13:00" }, { "label": "Pacific/Wake", "offset": "+12:00" }, { "label": "Pacific/Wallis", "offset": "+12:00" }]');