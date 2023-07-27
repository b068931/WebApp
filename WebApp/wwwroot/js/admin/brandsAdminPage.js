$(function () {
    var selectedBrand = null;
    var dynamicElements = {
        id: $("#id").remove(),
        label: $("#textLabel").remove(),
        nameRequestField: $("#requestQuery").remove()
    };

    function setFormAction(action) {
        $("#confirmationForm").attr("action", action);
    }
    function emptyConfirmationForm() {
        $("#id").remove();
        $("#textLabel").remove();
        $("#requestQuery").remove();

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
        $("#create").addClass("d-none");

        showConfirmation();
    }
    function renameBrand(brand) {
        startSelectBrandAction(
            brand,
            function (brand) {
                addMyId(brand.data("myid"));
                addNameQuery(brand.children().first().html());
            },
            "/brands/action/rename"
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

        addButton("Rename", "btn btn-warning me-2", function () { renameBrand(me); });
        addButton("Delete", "btn btn-danger", function () { deleteBrand(me); });
    });

    $("#abort").on("click", function () {
        abortLastAction();
    });
});