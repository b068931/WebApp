$(function () {
    var boxSize = 155;
    $(".brand-preview-image").each(function () {
        if ($(this).prop("complete") === true) {
            $(this).makeImageFitBox(boxSize, boxSize);
        }
        else {
            $(this).on("load", function () {
                $(this).makeImageFitBox(boxSize, boxSize);
            });
        }
    });
});