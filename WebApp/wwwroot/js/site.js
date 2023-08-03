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

(function ($) {
    $.fn.animatedToggle = function (target) {
        return this.each(function () {
            target.css("opacity", 0);
            $(this).on("click", function () {
                var resultOpacity = target.hasClass("d-none") ? 1 : 0;
                if (resultOpacity == 1) {
                    target.removeClass("d-none");
                }

                target.animate(
                    {
                        opacity: resultOpacity
                    },
                    "fast",
                    function () {
                        if (resultOpacity == 0) {
                            target.addClass("d-none");
                        }
                    }
                );
            });
        });
    };
}(jQuery));

$(function () {
    $("a").addClass("link_color");
});