$(function () {
    $("#create").on("click", function () {
        $("#stockSettingsContainer").attr("action", "/product/stocks/action/create");

        $(".colour-select").prop("checked", false);
        $(".size-select").prop("checked", false);
        $("#stockSizeInput").val("0");

        $("#stockChangeForm").modal("show");
    });

    $(".change-stock-btn").on("click", function () {
        $("#stockSettingsContainer").attr("action", "/product/stocks/action/update/" + $(this).data("stockid"));

        $("#colour-" + $(this).data("selectedcolour")).prop("checked", true);
        $("#size-" + $(this).data("selectedsize")).prop("checked", true);
        $("#stockSizeInput").val($(this).data("stocksize"));

        $("#stockChangeForm").modal("show");
    });

    $(".delete-stock-btn").on("click", function () {
        $("#stockIdDeleteInput").val($(this).data("stockid"));
        $("#stockDeleteConfirmation").modal("show");
    });
});