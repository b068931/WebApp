$(function () {
    $("#productImages").on("change", function () {
        var imageMaxSide = 300;
        var imageUpscaleFactor = 4;

        $("#selectedImagesContainer").children().remove();

        var filesList = $(this)[0].files;
        for (var index = 0; index < filesList.length; ++index) {
            var newImage = $("<img>", {
                src: URL.createObjectURL(filesList[index])
            })
                .data("myname", filesList[index].name)
                .data("myurl", URL.createObjectURL(filesList[index]));

            newImage.on("load", function () {
                $(this).makeImageFitBox(imageMaxSide, 0);
                $(this).on("click", function () {
                    $("#largeImageBody").children().remove();
                    $("#largeImageTitle").html(
                        $(this).data("myname")
                    );

                    $("#largeImageBody").append(
                        $("<img>", {
                            src: $(this).data("myurl")
                        })
                            .attr("width", $(this).attr("width") * imageUpscaleFactor)
                            .attr("height", $(this).attr("height") * imageUpscaleFactor)
                    );

                    $("#largeImage").modal("show");
                });
            });

            var imageContainer = $("<div>", {
                class: "border border-1 rounded m-2 p-1 d-inline-block"
            });

            imageContainer.append(newImage);
            $("#selectedImagesContainer").append(imageContainer);
        }
    });
});