$(function () {
    var previewImageWidth = 6;
    var previewImageHeight = 13;

    var shownImageWidth = 27;
    var shownImageHeight = 40;

    var brandImageWidth = 12.5;
    var brandImageHeight = 12.5;

    $(".min-image").each(function (index, element) {
        if ($(this).prop("complete") === true) {
            $(this)
                .makeImageFitBoxRems(previewImageWidth, previewImageHeight)
                .removeClass("d-none");
        }
        else {
            $(this).on("load", function () {
                $(this)
                    .makeImageFitBoxRems(previewImageWidth, previewImageHeight)
                    .removeClass("d-none");
            });
        }

        $(this).on("click", function () {
            $("#shownImage").attr("src", $(this).attr("src"));
            if ($("#shownImage").prop("complete") === true) {
                $("#shownImage").makeImageFitBoxRems(shownImageWidth, shownImageHeight);
            }
        });
    });

    var isZoomed = false;
    var zoomFactor = 2;
    function toggleZoom(image) {
        if (isZoomed) {
            image.makeImageFitBoxRems(shownImageWidth, shownImageHeight);

            $("#shownImageScrollContainer").removeClass("overflow-auto");
            isZoomed = false;
        }
        else {
            image.makeImageFitBoxRems(zoomFactor * shownImageWidth, zoomFactor * shownImageHeight);

            $("#shownImageScrollContainer").addClass("overflow-auto");
            isZoomed = true;
        }
    }

    if ($("#brandImage").prop("complete") === true) {
        $("#brandImage")
            .makeImageFitBoxRems(brandImageWidth, brandImageHeight)
            .removeClass("d-none");
    }
    else {
        $("#brandImage").on("load", function () {
            $(this)
                .makeImageFitBoxRems(brandImageWidth, brandImageHeight)
                .removeClass("d-none");
        });
    }

    if ($("#shownImage").prop("complete") === true) {
        $("#shownImage")
            .makeImageFitBoxRems(shownImageWidth, shownImageHeight)
            .on("click", function () {
                toggleZoom($(this));
            })
            .removeClass("d-none");
    }
    else {
        $("#shownImage")
            .on("load", function () {
                $(this).makeImageFitBoxRems(shownImageWidth, shownImageHeight);
                if (isZoomed) {
                    $("#shownImageScrollContainer").removeClass("overflow-auto");
                    isZoomed = false;
                }
            })
            .on("click", function () {
                toggleZoom($(this));
            })
            .removeClass("d-none");
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
                if (data.length === 0)
                    $("#notShownInSearch").removeClass("d-none");

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

    $(".rating-star")
        .on("click", function () {
            $.post("/products/product/" + location[location.length - 1] + "/rate",
                {
                    stars: $(this).data("ratingvalue"),
                    __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                }
            )
                .done(() => window.location.reload())
                .fail(function (xhr, status, error) {
                    $("#ratingError")
                        .html(xhr.responseText)
                        .removeClass("d-none");
                });
        });
});