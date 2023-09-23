$(function () {
    const imageMaxSide = 300;
    const imageUpscaleFactor = 4;
    const placeholdersCount = $("#previewImagesScript").data("placeholders");
    for (var index = 0; index < placeholdersCount; ++index) {
        $("#selectedImagesContainer").append(
            $("<img>", {
                src: "/resources/imagepreviewplaceholder.svg",
                class: "m-2",
                width: imageMaxSide,
                height: imageMaxSide
            })
        );
    }

    $("#productImages").on("change", function () {
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
                class: "image-preview-border m-2 d-inline-block",
                role: "button"
            });

            imageContainer.append(newImage);
            $("#selectedImagesContainer").append(imageContainer);
        }
        for (var index = filesList.length; index < placeholdersCount; ++index) {
            $("#selectedImagesContainer").append(
                $("<img>", {
                    src: "/resources/imagepreviewplaceholder.svg",
                    class: "m-2",
                    width: imageMaxSide,
                    height: imageMaxSide
                })
            );
        }
    });
});