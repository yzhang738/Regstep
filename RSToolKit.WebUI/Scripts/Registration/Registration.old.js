$(window).load(function () {
    $('#formValidator').dialog({
        autoOpen: false,
        modal: true,
        draggable: true,
        position: { my: "center", at: "center", of: window },
        title: "Registration Error",
        width: (window.innerWidth * 0.4),
        buttons:
        [{
            text: "Ok",
            click: function () { $(this).dialog("close"); }
        }],
        draggable: true,
        show:
        {
            effect: "fade",
            duration: 1000
        },
        hide:
        {
            effect: "fade",
            duration: 1000
        }
    });
});