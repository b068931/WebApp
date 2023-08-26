$(function () {
    var previewImageWidth = 118.5;
    var previewImageHeight = 218.5;

    var shownImageWidth = 447.5;
    var shownImageHeight = 665;

    var brandImageWidth = 200;
    var brandImageHeight = 200;

    $(".min-image").each(function (index, element) {
        if ($(this).prop("complete") === true) {
            $(this).makeImageFitBox(previewImageWidth, previewImageHeight);
        }
        else {
            $(this).on("load", function () {
                $(this).makeImageFitBox(previewImageWidth, previewImageHeight);
            });
        }

        $(this).on("click", function () {
            $("#shownImage").attr("src", $(this).attr("src"));
            if ($("#shownImage").prop("complete") === true) {
                $("#shownImage").makeImageFitBox(shownImageWidth, shownImageHeight);
            }
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

    if ($("#brandImage").prop("complete") === true) {
        $("#brandImage").makeImageFitBox(brandImageWidth, brandImageHeight);
    }
    else {
        $("#brandImage").on("load", function () {
            $(this).makeImageFitBox(brandImageWidth, brandImageHeight);
        });
    }

    if ($("#shownImage").prop("complete") === true) {
        $("#shownImage")
            .makeImageFitBox(shownImageWidth, shownImageHeight)
            .on("click", function () {
                toggleZoom($(this));
            });
    }
    else {
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
    }

    $("#descriptionShow")
        .animatedSlideDown($("#formatedProductDescription"), $("#descriptionMaximizer"));

    $("#stocksShow")
        .animatedSlideDown($("#stocksContainer"), $("#stocksMaximizer"));

    $("#deleteProductButton").on("click", function () {
        $("#productDeleteConfirmation").modal("show");
    });

    var loadedProductStocksInformation = null;
    var location = window.location.href.split("/");
    $.get("/product/stocks/json?productId=" + location[location.length - 1],
        function (data, status) {
            if (status === "success") {
                loadedProductStocksInformation = data;
                $(".stock-information-select")
                    .on("change", function () {
                        var selectedColour = $("#colours input[type=radio]:checked");
                        var selectedSize = $("#sizes input[type=radio]:checked");
                        if (selectedColour.length > 0 && selectedSize.length > 0) {
                            for (var stockInfo of loadedProductStocksInformation) {
                                if (stockInfo.colour.id == selectedColour.data("myid") && stockInfo.size.id == selectedSize.data("myid")) {
                                    $("#availableContainer")
                                        .html(stockInfo.productAmount + " одиниць.")
                                        .removeClass("text-danger");
                                    return;
                                }
                            }

                            $("#availableContainer")
                                .html("Немає у наявності.")
                                .addClass("text-danger");
                        }
                    });

                $("#stocksShow").removeClass("d-none");
            }
            else {
                alert("Unable to load product stocks information.");
            }
        },
        "json"
    );
});