$(function () {
    $.get("/categories/base",
        function (data, status) {
            var loadedSubcategories = {};
            var currentMaxLevel = 0;

            function appendSubcategory(element, destination, level, clickCallback) {
                function generateCategoryContents(destination) {
                    var categoryName = $("<div>", { text: element.name });
                    destination.append(categoryName);
                    if (!element.isLast) {
                        var followUpSign = $("<div>", { text: ">" });
                        destination.append(followUpSign);
                    }
                }
                function generateNewCategory() {
                    var className = "ps-4 pe-4 pt-2 pb-2 link_color d-flex ";
                    className += (element.isLast) ? "justify-content-start" : "justify-content-between";

                    return $("<div>",
                        {
                            class: className,
                            role: "button"
                        }
                    )
                        .data("id", element.id)
                        .data("level", level)
                        .on("click", clickCallback);
                }

                var newCategory = generateNewCategory();
                generateCategoryContents(newCategory);

                newCategory.appendTo(destination);
            }

            function popLastCategories(count) {
                for (var counter = 0; counter < count; ++counter) {
                    $("#categoriesList").children().last().remove();
                }
            }
            function pushNewCategories(data) {
                var categoriesContainer = $("<div>", { class: "overflow-auto w-25 border-end" });
                data.forEach(element => appendSubcategory(
                    element,
                    categoriesContainer,
                    currentMaxLevel,
                    (element.isLast) ? childCategoryOnClick : parentCategoryOnClick
                ));

                $("#categoriesList").append(categoriesContainer);
            }
            function loadCategories(data, myLevel) {
                popLastCategories(currentMaxLevel - myLevel);
                currentMaxLevel = myLevel + 1;

                pushNewCategories(data);
            }

            function childCategoryOnClick() {
                var myId = $(this).data("id");
                window.location.href = "/categories/category/" + myId + "/products";
            }
            function parentCategoryOnClick() {
                var myId = $(this).data("id");
                var myLevel = $(this).data("level");
                if (loadedSubcategories[myId] === undefined) {
                    $.get("/categories/category/" + myId + "/children",
                        function (data, status) {
                            if (status == "success") {
                                loadCategories(data, myLevel);
                                loadedSubcategories[myId] = data;
                            }
                        },
                        "json"
                    );
                }
                else {
                    loadCategories(loadedSubcategories[myId], myLevel)
                }
            }

            if (status === "success") {
                data.forEach(
                    element => appendSubcategory(
                        element,
                        "#baseCategories",
                        0,
                        (element.isLast) ? childCategoryOnClick : parentCategoryOnClick
                    )
                );
            }
        },
        "json"
    );

    $("#categoriesList").css("opacity", 0);
    $("#categoriesToggler").on("click", function () {
        var resultOpacity = $("#categoriesList").hasClass("d-none") ? 1 : 0;
        if (resultOpacity == 1) {
            $("#categoriesList").removeClass("d-none");
        }

        $("#categoriesList").animate(
            {
                opacity: resultOpacity
            },
            "fast",
            function () {
                if (resultOpacity == 0) {
                    $("#categoriesList").addClass("d-none");
                }
            }
        );
    });
});