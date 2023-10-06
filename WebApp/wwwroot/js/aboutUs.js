$(function () {
    var boxSize = 155;
    $(".brand-preview-image").each(function () {
        if ($(this).prop("complete") === true) {
            $(this).makeImageFitBox(boxSize, boxSize)
                .removeClass("d-none");
        }
        else {
            $(this).on("load", function () {
                $(this).makeImageFitBox(boxSize, boxSize)
                    .removeClass("d-none");
            });
        }
    });
});