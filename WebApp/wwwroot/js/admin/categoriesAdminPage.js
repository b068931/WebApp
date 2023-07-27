$(function () {
    var categoryOnClickCallback = null;
    var formDynamicElements = {
        id: $("#id").remove(),
        parentId: $("#parentId").remove(),
        newNameRequestField: $("#newNameRequestField").remove(),
        submit: $("#confirm").remove()
    };

    function changeStatus(newStatus) {
        $("#statusField").html(newStatus);
    }
    function setFormAction(action) {
        $("#confirmationForm").attr("action", action);
    }
    function clearForm() {
        categoryOnClickCallback = null;

        $("#confirmationForm").attr("action", "#");

        $("#id").remove();
        $("#parentId").remove();
        $("#newNameRequestField").remove();
        $("#confirm").remove();

        $("#abort").addClass("d-none");
    }

    function appendInputToForm(inputName, value) {
        if (inputName == "newNameRequestField") {
            $("#confirmationForm").append(
                formDynamicElements[inputName]
            );
            $("#requestQuery").val(value);
        }
        else {
            $("#confirmationForm").append(
                formDynamicElements[inputName].val(value)
            );
        }
    }
    function appendInputToFormWithChain(inputName, value, next) {
        appendInputToForm(inputName, value);
        categoryOnClickCallback = next;
    }

    function addCategoryIdToForm(category, next) {
        appendInputToFormWithChain("id", category.data("myid"), next);
    }
    function addParentCategoryIdToForm(category, next) {
        appendInputToFormWithChain("parentId", category.data("myid"), next);
    }
    function addNewNameRequestFieldToForm(category, next) {
        appendInputToFormWithChain("newNameRequestField", category.html(), next);
    }
    function addConfirmationToForm(category, next) {
        appendInputToFormWithChain("submit", "Confirm action for category '" + category.html() + "'", next);
        $("#abort").removeClass("d-none");
    }

    $(".category").on("click", function () {
        if (categoryOnClickCallback != null) {
            categoryOnClickCallback($(this));
        }
    });

    $("#abort").on("click", function () {
        changeStatus("Last action was aborted.");
        clearForm();
    });

    $("#create").on("click", function () {
        clearForm();
        changeStatus("Choose the parent category for your new category. It is your responsibility to make sure that the parent category does not contain any products!");
        setFormAction("/categories/action/create");

        categoryOnClickCallback = function (category) {
            changeStatus("Enter the name and confirm/abort creation.");

            appendInputToForm("parentId", category.data("myid"));
            appendInputToForm("newNameRequestField", "New Category");
            addConfirmationToForm(category, null);
        }
    });

    $("#rename").on("click", function () {
        clearForm();
        changeStatus("Choose the category to rename.");
        setFormAction("/categories/action/rename");

        categoryOnClickCallback = function (category) {
            changeStatus("Change the name and confirm/abort.");

            appendInputToForm("id", category.data("myid"));
            appendInputToForm("newNameRequestField", category.html());
            addConfirmationToForm(category, null);
        }
    });

    $("#move").on("click", function () {
        clearForm();
        changeStatus("Choose the category to move.");
        setFormAction("/categories/action/move");

        var moveEnd = function (category) {
            changeStatus("Finish this operation by confirming/aborting it.");
            appendInputToForm("parentId", category.data("myid"));
            addConfirmationToForm(category, null);
        }

        categoryOnClickCallback = function (category) {
            changeStatus("Choose the new parent. You must make sure that parent category has NO products in it.");
            addCategoryIdToForm(category, moveEnd);
        }
    });

    $("#salvage").on("click", function () {
        clearForm();
        changeStatus("Choose the category to salvage (NOTE: this function is recursive).");
        setFormAction("/categories/action/salvage");

        var salvageEnd = function (category) {
            changeStatus("Finish this operation by confirming/aborting it.");
            appendInputToForm("parentId", category.data("myid"));
            addConfirmationToForm(category, null);
        }

        categoryOnClickCallback = function (category) {
            changeStatus("Choose the destination category for products.");
            addCategoryIdToForm(category, salvageEnd);
        }
    });

    $("#remove").on("click", function () {
        clearForm();
        changeStatus("Choose the category to delete. You must make sure that this category has NO products or they will be deleted.");
        setFormAction("/categories/action/delete");

        categoryOnClickCallback = function (category) {
            changeStatus("Finish this operation by confirming/aborting it.");

            appendInputToForm("id", category.data("myid"));
            addConfirmationToForm(category, null);
        }
    });
});