$(document).ready(function () {
    $('[data-toggle="popover"]').popover({ html : true });
});

$(document).ready(function () {
    $('[data-toggle="tooltip"]').tooltip();
});

$(document).ready(function () {
    $('[data-toggle="collapse"]').collapse();
});

$(document).ready(function () {
    $('.selectpicker').selectpicker();

    $("#searchButton").click(function () {
        window.location = "/Home/SearchForTag?tag=" + $("#selectBox option:selected").text();
    });
});