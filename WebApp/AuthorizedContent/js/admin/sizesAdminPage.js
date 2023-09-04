$(function () {
    var selectedSize = null;
    var dynamicElements = {
        id: $("#id").remove(),
        inputs: $("#sizeInfoInputs").remove()
    };

    function setFormAction(action) {
        $("#confirmationForm").attr("action", action);
    }
    function emptyConfirmationForm() {
        $("#id").remove();
        $("#sizeInfoInputs").remove();

        $("#confirm").addClass("d-none");
        $("#abort").addClass("d-none");

        setFormAction("#");
    }

    function addMyId(id) {
        $("#confirmationForm").prepend(
            dynamicElements["id"].val(id)
        );
    }
    function addSizeInfoInput(name) {
        $("#inputsContainer").prepend(dynamicElements["inputs"]);
        $("#requestQuery").val(name);
    }
    function showConfirmation() {
        $("#confirm").removeClass("d-none");
        $("#abort").removeClass("d-none");
    }

    function selectSize(size) {
        selectedSize = size;

        size.children().eq(-1).addClass("d-none");
        size.children().eq(-2).addClass("d-none");

        size.addClass("active");
    }
    function unselectSize() {
        if (selectedSize != null) {
            selectedSize.children().eq(-1).removeClass("d-none");
            selectedSize.children().eq(-2).removeClass("d-none");

            selectedSize.removeClass("active");
            selectedSize = null;
        }
    }

    function abortLastAction() {
        emptyConfirmationForm();
        unselectSize();

        $("#create").removeClass("d-none");
    }

    function createSize() {
        abortLastAction();
        setFormAction("/sizes/action/create");

        addSizeInfoInput("New size");
        $("#create").addClass("d-none");

        showConfirmation();
    }
    function updateSize(size) {
        abortLastAction();
        selectSize(size);

        $("#create").addClass("d-none");
        setFormAction("/sizes/action/update");

        addMyId(size.data("myid"));
        addSizeInfoInput(size.find(".size-name").html());

        showConfirmation();
    }
    function deleteSize(size) {
        abortLastAction();
        selectSize(size);

        $("#create").addClass("d-none");
        setFormAction("/sizes/action/delete");

        addMyId(size.data("myid"));
        showConfirmation();
    }

    $("#create").on("click", createSize);
    $("#abort").on("click", function () {
        abortLastAction();
    });

    $(".size-list-element").each(function (index, element) {
        var me = $(this);
        function addButton(name, className, onClick) {
            var newElement =
                $("<button>", { class: className })
                    .html(name)
                    .on("click", onClick);

            me.append(newElement);
        }

        addButton("Update", "btn btn-warning me-2", function () { updateSize(me); });
        addButton("Delete", "btn btn-danger", function () { deleteSize(me); });
    });
});