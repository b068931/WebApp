$(function () {
    $("#productImages").on("change", function () {
        var imageMaxWidth = 300;
        var imageMaxHeight = 300;
        var imageUpscaleFactor = 4;

        $("#selectedImagesContainer").children().remove();

        var filesList = $(this)[0].files;
        for (var index = 0; index < filesList.length; ++index) {
            var newImage = $("<img>", {
                src: URL.createObjectURL(filesList[index]),
                class: "rounded img-thumbnail m-2"
            })
                .data("myname", filesList[index].name)
                .data("myurl", URL.createObjectURL(filesList[index]));

            newImage.on("load", function () {
                var bestCoefficient = Math.max(
                    $(this).prop("naturalWidth") / imageMaxWidth,
                    $(this).prop("naturalHeight") / imageMaxHeight
                );

                var bestWidth = Math.round($(this).prop("naturalWidth") / bestCoefficient);
                var bestHeight = Math.round($(this).prop("naturalHeight") / bestCoefficient);

                $(this).attr("width", bestWidth);
                $(this).attr("height", bestHeight);

                $(this).on("click", function () {
                    $("#largeImageBody").children().remove();
                    $("#largeImageTitle").html(
                        $(this).data("myname")
                    );

                    $("#largeImageBody").append(
                        $("<img>", {
                            src: $(this).data("myurl")
                        })
                            .attr("width", bestWidth * imageUpscaleFactor)
                            .attr("height", bestHeight * imageUpscaleFactor)
                    );

                    $("#largeImage").modal("show");
                });
            });

            $("#selectedImagesContainer").append(newImage);
        }
    });
});