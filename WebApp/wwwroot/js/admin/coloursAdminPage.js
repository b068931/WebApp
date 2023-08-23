$(function () {
    var selectedColour = null;
    var dynamicElements = {
        id: $("#id").remove(),
        inputs: $("#colourInfoInputs").remove()
    };

    function setFormAction(action) {
        $("#confirmationForm").attr("action", action);
    }
    function emptyConfirmationForm() {
        $("#id").remove();
        $("#colourInfoInputs").remove();

        $("#confirm").addClass("d-none");
        $("#abort").addClass("d-none");

        setFormAction("#");
    }

    function addMyId(id) {
        $("#confirmationForm").prepend(
            dynamicElements["id"].val(id)
        );
    }
    function addColourInfoInput(name, colour) {
        $("#inputsContainer").prepend(dynamicElements["inputs"]);

        $("#requestQuery").val(name);
        $("#colourPicker").val(colour);
    }
    function showConfirmation() {
        $("#confirm").removeClass("d-none");
        $("#abort").removeClass("d-none");
    }

    function selectColour(colour) {
        selectedColour = colour;

        colour.children().eq(-1).addClass("d-none");
        colour.children().eq(-2).addClass("d-none");

        colour.addClass("active");
    }
    function unselectColour() {
        if (selectedColour != null) {
            selectedColour.children().eq(-1).removeClass("d-none");
            selectedColour.children().eq(-2).removeClass("d-none");

            selectedColour.removeClass("active");
            selectedColour = null;
        }
    }

    function abortLastAction() {
        emptyConfirmationForm();
        unselectColour();

        $("#create").removeClass("d-none");
    }

    function createColour() {
        abortLastAction();
        setFormAction("/colours/action/create");

        addColourInfoInput("New colour", "#000000");
        $("#create").addClass("d-none");

        showConfirmation();
    }
    function updateColour(colour) {
        abortLastAction();
        selectColour(colour);

        $("#create").addClass("d-none");
        setFormAction("/colours/action/update");

        addMyId(colour.data("myid"));
        addColourInfoInput(
            colour.find(".colour-name").html(),
            colour.find(".colour-sample").data("colourvalue")
        );

        showConfirmation();
    }
    function deleteColour(colour) {
        abortLastAction();
        selectColour(colour);

        $("#create").addClass("d-none");
        setFormAction("/colours/action/delete");

        addMyId(colour.data("myid"));
        showConfirmation();
    }

    $("#create").on("click", createColour);
    $("#abort").on("click", function () {
        abortLastAction();
    });

    $(".colour-list-element").each(function (index, element) {
        var me = $(this);
        function addButton(name, className, onClick) {
            var newElement =
                $("<button>", { class: className })
                    .html(name)
                    .on("click", onClick);

            me.append(newElement);
        }

        addButton("Update", "btn btn-warning me-2", function () { updateColour(me); });
        addButton("Delete", "btn btn-danger", function () { deleteColour(me); });
    });
});