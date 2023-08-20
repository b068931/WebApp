(function ($) {
    $.fn.makeImageFitBox = function (imageMaxWidth, imageMaxHeight) {
        return this.each(function () {
            var bestCoefficient = Math.max(
                (imageMaxWidth > 0) ? $(this).prop("naturalWidth") / imageMaxWidth : 0,
                (imageMaxHeight > 0) ? $(this).prop("naturalHeight") / imageMaxHeight : 0
            );

            if (bestCoefficient > 0) {
                var bestWidth = Math.round($(this).prop("naturalWidth") / bestCoefficient);
                var bestHeight = Math.round($(this).prop("naturalHeight") / bestCoefficient);

                $(this).attr("width", bestWidth);
                $(this).attr("height", bestHeight);
            }
        });
    };
}(jQuery));

(function ($) {
    $.fn.animatedSlideDown = function (target, maximizer) {
        return this.each(function () {
            target.hide();
            $(this).on("click", function () {
                if (target.is(":hidden")) {
                    target.slideDown(
                        "fast",
                        () => maximizer.html("-")
                    );
                }
                else {
                    maximizer.html("+");
                    target.slideUp(
                        "fast",
                        () => maximizer.html("+")
                    );
                }
            });
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