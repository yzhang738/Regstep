$(document).ready(function () {

    $(".ShoppingCart").on("mouseenter", function () {
        $(".ShoppingCartInformation").show("fade", 500);
    });
    $(".ShoppingCart").on("mouseleave", function () {
        $(".ShoppingCartInformation").hide("fade", 500);
    });

});