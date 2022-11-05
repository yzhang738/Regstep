var checkboxgroups = {};
var radiogroups = {};

$(document).ready(function () {
    $('input[data-component-type="datetime"]').datetimepicker({
        showMeridian: true
    });

    $('input[type="hidden"][data-component-type="checkboxgroup"]').each(function () {
        checkboxgroups[$(this).attr('id')] = { parent: $(this), timeExclusion: $(this).parent().attr('data-component-timeexclusion') == "True", Items: [] };
        $(this).parent().find('input[type="checkbox"]').each(function () {
            if (/^Waitlistings/.test($(this).attr('name')))
                return;
            var parentId = $(this).attr('data-parent');
            checkboxgroups[parentId].Items.push($(this));
            if (checkboxgroups[parentId].timeExclusion)
            {
                var t_timeExclusionMessage = checkboxgroups[parentId].parent.attr('data-component-timeexclusion-message');
                if (typeof (t_timeExclusionMessage) === 'undefined') {
                    t_timeExclusionMessage = 'There was a scheduling conflict with items.';
                }
                checkboxgroups[parentId].timeExclusionMessage = t_timeExclusionMessage;
                $(this).on('change', function () {
                    var parentId = $(this).attr('data-parent');
                    var json = [];
                    var history = [];
                    var checked = [];
                    var collision = false;
                    for (var i = 0; i < checkboxgroups[parentId].Items.length; i++) {
                        if (checkboxgroups[parentId].Items[i].prop('checked')) {
                            if (checkboxgroups[parentId].Items[i].val() != $(this).val()) {
                                history.push(checkboxgroups[parentId].Items[i]);
                            }
                            json.push(checkboxgroups[parentId].Items[i].val());
                            checked.push(checkboxgroups[parentId].Items[i]);
                        }
                    }
                    for (var i = 0; i < checked.length; i++) {
                        var aStart = moment(checked[i].attr('data-agenda-start'));
                        var aEnd = moment(checked[i].attr('data-agenda-end'));
                        for (var j = 0; j < checked.length; j++) {
                            if (checked[i] == checked[j])
                                continue;
                            var bStart = moment(checked[j].attr('data-agenda-start'));
                            var bEnd = moment(checked[j].attr('data-agenda-end'));
                            if ((bEnd.isAfter(aStart) || bEnd.isSame(aStart)) && (bEnd.isBefore(aEnd) || bEnd.isSame(aEnd)))
                                collision = true;
                        }
                    }
                    if (collision) {
                        // we need to roll back to what we had before
                        for (var i = 0; i < checkboxgroups[parentId].Items.length; i++) {
                            checkboxgroups[parentId].Items[i].prop('checked', false);
                        }
                        for (var i = 0; i < history.length; i++) {
                            history[i].prop('checked', true);
                        }
                        alert(checkboxgroups[parentId].timeExclusionMessage);
                    } else {
                        checkboxgroups[parentId].parent.val(JSON.stringify(json));
                    }
                });
            }
            else
            {
                $(this).on('change', function () {
                    var parentId = $(this).attr('data-parent');
                    var checked = [];
                    for (var i = 0; i < checkboxgroups[parentId].Items.length; i++) {
                        if (checkboxgroups[parentId].Items[i].prop('checked'))
                            checked.push(checkboxgroups[parentId].Items[i].val());
                    }
                    checkboxgroups[parentId].parent.val(JSON.stringify(checked));
                })
            }
        });
    });

    $('input[type="hidden"][data-component-type="radiogroup"]').each(function () {
        radiogroups[$(this).attr('id')] = { parent: $(this), Items: [] };
        $(this).parent().find('input[type="radio"]').each(function () {
            var parentId = $(this).attr('data-parent');
            radiogroups[parentId].Items.push($(this));
            $(this).on('change', function () {
                var parentId = $(this).attr('data-parent');
                radiogroups[parentId].parent.val($(this).val());
            })
        });
    });

    $('.uploaded-image').each(function (i) {
        $.ajax({
            type: "Get",
            url: '../../Cloud/RegistrantImageThumbnail/' + $(this).attr('data-form-registrant') + '?component=' + $(this).attr('data-form-component'),
            success: function (data) {
                $(this).attr('src', data);
            },
        });
    });
});