(function ($) {
    $(document).ready(function () {
        $('html').on('click', function () {
            $('[data-navigationboot-dropdownmenu]').slideUp();
        });
        $('[data-navigationboot]').each(function () {
            $(this).addClass('navigation-boot');
            $('[data-navigationboot-brand]', this).addClass('navigation-boot-header');
            $('[data-navigationboot-brand] > button', this).addClass('navigation-boot-toggle').html('<span class="icon-bar"></span><span class="icon-bar"></span><span class="icon-bar"></span>');
            $('[data-navigationboot-brand] > div', this).addClass('navigation-boot-brand');
            $('[data-navigationboot-body]', this).addClass('navigation-boot-body').css('height', 'auto');
            $('[data-navigationboot-dropdown]', this).addClass('navigation-boot-dropdown');
            $('[data-navigationboot-dropdownmenu]', this).addClass('navigation-boot-dropdownmenu');
            $('[data-navigationboot-nav-right]', this).addClass('navigation-boot-nav-right');
            if (!$('[data-navigationboot-toggle]').is(':visible')) {
                var width = $('[data-navigationboot] > div').width();
                var brandWidth = $('[data-navigationboot-brand]').width();
                $('[data-navigationboot-body]').width(width - brandWidth - 20);
            } else {
                $('[data-navigationboot-body]').width(width);
            }
            $('button[data-navigationboot-toggle=collapse]', this).on('click', function (e) {
                var target = $(this).attr('data-target');
                $(target).slideToggle();
            });
            $('[data-navigationboot-dropdownmenu]', this).on('click', function (e) {
                e.stopPropagation();
            });
            $('[data-navigationboot-dropdown] > div', this).append(' <span class="glyphicon glyphicon-chevron-down" style="font-size: 12px;"></span>');
            $('[data-navigationboot-dropdown] > div', this).on('click', function (e) {
                var current = $(this).parent()[0];
                $('[data-navigationboot-dropdownmenu]').each(function () {
                    var p = $(this).parent()[0];
                    if (p != current) {
                        $(this).slideUp();
                    }
                });
                $(this).parent().children('[data-navigationboot-dropdownmenu]').slideToggle();
                e.stopPropagation();
            });
        });
        $(window).on('resize', function () {
            var width = $('[data-navigationboot] > div').width();
            if (!$('[data-navigationboot-toggle]').is(':visible')) {
                var brandWidth = $('[data-navigationboot-brand]').width();
                $('[data-navigationboot-body]').width(width - brandWidth - 20);
            } else {
                $('[data-navigationboot-body]').width(width);
            }
        });
    });
}(jQuery));