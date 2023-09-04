$(function () {
    $(".main-image-btn").on("click", function () {
        $("#mainImage").val(
            $(this).data("myid")
        );
    });

    $(".delete-image-btn").on("click", function () {
        var myid = $(this).data("myid");
        var newDeleteInput = $("<input>", {
            id: "deleteImage" + myid,
            name: "deleteImages",
            type: "hidden",
            value: myid
        });

        $("#confirmationForm").prepend(
            newDeleteInput
        );

        var myparent = $(this).parent();
        var detachedInteractionButtons = myparent.children().detach();
        var cancelDeleteButton = $("<button>", {
            class: "btn btn-outline-primary w-100"
        })
            .on("click", function () {
                myparent.children().remove();
                $("#deleteImage" + myid).remove();

                detachedInteractionButtons.appendTo(myparent);
            })
            .html("Cancel Delete");

        myparent.append(cancelDeleteButton);
    });
});