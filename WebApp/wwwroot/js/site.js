(function ($) {
    $.fn.makeImageFitBox = function (imageMaxWidth, imageMaxHeight) {
        return this.each(function () {
            var bestCoefficient = Math.max(
                $(this).prop("naturalWidth") / imageMaxWidth,
                $(this).prop("naturalHeight") / imageMaxHeight
            );

            var bestWidth = Math.round($(this).prop("naturalWidth") / bestCoefficient);
            var bestHeight = Math.round($(this).prop("naturalHeight") / bestCoefficient);

            $(this).attr("width", bestWidth);
            $(this).attr("height", bestHeight);
        });
    };
}(jQuery));

$(function () {
    $("a").addClass("link_color");
});