$(function () {
    var selectedBrand = null;
    var dynamicElements = {
        id: $("#id").remove(),
        label: $("#textLabel").remove(),
        nameRequestField: $("#requestQuery").remove(),
        fileInput: $("#fileInputContainer").remove()
    };

    function setFormAction(action) {
        $("#confirmationForm").attr("action", action);
    }
    function emptyConfirmationForm() {
        $("#id").remove();
        $("#textLabel").remove();
        $("#requestQuery").remove();
        $("#fileInputContainer").remove();

        $("#confirm").addClass("d-none");
        $("#abort").addClass("d-none");

        setFormAction("#");
    }
    function addMyId(id) {
        $("#confirmationForm").prepend(
            dynamicElements["id"].val(id)
        );
    }
    function addNameQuery(initialValue) {
        $("#inputsContainer").prepend(
            dynamicElements["nameRequestField"].val(initialValue)
        );

        $("#inputsContainer").prepend(dynamicElements["label"]);
    }
    function addFileInput() {
        $("#confirmationForm").append(
            dynamicElements["fileInput"]
        );
    }
    function showConfirmation() {
        $("#confirm").removeClass("d-none");
        $("#abort").removeClass("d-none");
    }

    function selectBrand(brand) {
        selectedBrand = brand;

        brand.children().eq(-1).addClass("d-none");
        brand.children().eq(-2).addClass("d-none");

        brand.addClass("active");
    }
    function unselectBrand() {
        if (selectedBrand != null) {
            selectedBrand.children().eq(-1).removeClass("d-none");
            selectedBrand.children().eq(-2).removeClass("d-none");

            selectedBrand.removeClass("active");
            selectedBrand = null;
        }
    }

    function abortLastAction() {
        emptyConfirmationForm();
        unselectBrand();

        $("#create").removeClass("d-none");
    }
    function startSelectBrandAction(brand, configureForm, formAction) {
        abortLastAction();
        setFormAction(formAction);
        selectBrand(brand);

        configureForm(brand);
        $("#create").addClass("d-none");

        showConfirmation();
    }

    function createBrand() {
        abortLastAction();
        setFormAction("/brands/action/create");

        addNameQuery("New brand");
        addFileInput();
        $("#create").addClass("d-none");

        showConfirmation();
    }
    function updateBrand(brand) {
        startSelectBrandAction(
            brand,
            function (brand) {
                addMyId(brand.data("myid"));
                addNameQuery(brand.children().first().html());
                addFileInput();
            },
            "/brands/action/update"
        );
    }
    function deleteBrand(brand) {
        startSelectBrandAction(
            brand,
            function (brand) {
                addMyId(brand.data("myid"));
            },
            "/brands/action/delete"
        );
    }

    $("#create").on("click", createBrand);

    $(".brand-list-element").each(function (index, element) {
        var me = $(this);
        function addButton(name, className, onClick) {
            var newElement =
                $("<button>", { class: className })
                    .html(name)
                    .on("click", onClick);

            me.append(newElement);
        }

        addButton("Update", "site-button site-warning-button me-2", function () { updateBrand(me); });
        addButton("Delete", "site-button site-danger-button", function () { deleteBrand(me); });
    });

    $("#abort").on("click", function () {
        abortLastAction();
    });
});