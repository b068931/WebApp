$(function () {
    var imageMaxSide = 40;
    var shownImageWidth = 470;
    var shownImageHeight = 420;

    $(".min-image").each(function (index, element) {
        if ($(this).prop("complete") === true) {
            $(this).makeImageFitBox(imageMaxSide, imageMaxSide);
        }

        $(this).on("load", function () {
            $(this).makeImageFitBox(imageMaxSide, imageMaxSide);
        });

        $(this).on("click", function () {
            $("#shownImage").attr("src", $(this).attr("src"));
        });
    });

    var isZoomed = false;
    var zoomFactor = 2;
    function toggleZoom(image) {
        if (isZoomed) {
            image.makeImageFitBox(shownImageWidth, shownImageHeight);

            $("#shownImageScrollContainer").removeClass("overflow-auto");
            isZoomed = false;
        }
        else {
            image.attr("width", zoomFactor * image.attr("width"));
            image.attr("height", zoomFactor * image.attr("height"));

            $("#shownImageScrollContainer").addClass("overflow-auto");
            isZoomed = true;
        }
    }

    if ($("#shownImage").prop("complete") === true) {
        $("#shownImage").makeImageFitBox(shownImageWidth, shownImageHeight);
    }

    $("#shownImage")
        .on("load", function () {
            $(this).makeImageFitBox(shownImageWidth, shownImageHeight);
            if (isZoomed) {
                $("#shownImageScrollContainer").removeClass("overflow-auto");
                isZoomed = false;
            }
        })
        .on("click", function () {
            toggleZoom($(this));
        });
});