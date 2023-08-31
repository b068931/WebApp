$(function () {
    const maxIntegerValue = 2147483647;
    const currentDate = new Date();

    const lastMonthDate = new Date();
    lastMonthDate.setDate(currentDate.getDate() - 30);

    $("#currentYear")
        .html(currentDate.getFullYear());

    $("#newProductsBanner").on("click", function () {
        window.location.href = "/products?sort=date&order=reversed&mindate=" +
            currentDate.getFullYear() + "-01-01";
    });

    $("#allProductsBanner").on("click", function () {
        window.location.href = "/products";
    });

    $("#bestProductsBanner").on("click", function () {
        window.location.href = "/products?sort=stars&order=reversed&minratings=10";
    });

    $("#discountsBanner").on("click", function () {
        window.location.href = "/products?sort=discount&order=reversed";
    });

    function performProductsSearch(arguments, callback) {
        $.get("/products/search",
            arguments,
            function (data, status) {
                if (status === "success") {
                    callback(data);
                }
                else {
                    alert("Unable to load some data on the page.");
                }
            },
            "json"
        );
    }

    performProductsSearch(
        {
            maxid: 0,
            includesearchresult: false,
            sortviews: maxIntegerValue,
            ordertype: "reversed",
            mincreated: lastMonthDate.toISOString().split('T')[0]
        },
        (data) => $("#lastMonthPopularContainer")[0]
            .insertAdjacentHTML("beforeend", data["html"])
    )

    $(".carousel-image").makeImageFitBoxRems(45, 0);

    var cachedSearchResults = {};

    function fillInSearchResultContainer(data) {
        $("#productTypeSelectContainer").children().remove();
        $("#productTypeSelectContainer")[0]
            .insertAdjacentHTML("beforeend", data);
    }
    function performSearchAndCache(query, searchType) {
        cachedSearchResults[searchType] = "";
        performProductsSearch(
            query,
            (data) => {
                cachedSearchResults[searchType] = data["html"];
                fillInSearchResultContainer(data["html"]);
            }
        );
    }
    function performSearchOnClick(query, searchTypeName) {
        if (cachedSearchResults[searchTypeName] === undefined) {
            performSearchAndCache(query, searchTypeName);
        }
        else {
            fillInSearchResultContainer(cachedSearchResults[searchTypeName]);
        }
    }

    $("#recommendedSearchSelect").on("click", function () {
        performSearchOnClick(
            {
                maxid: 0,
                includesearchresult: false,
                sortviews: maxIntegerValue,
                ordertype: "reversed"
            },
            "recommended"
        );
    });
    $("#newSearchSelect").on("click", function () {
        performSearchOnClick(
            {
                maxid: 0,
                includesearchresult: false,
                sortdate: "9999-12-31",
                ordertype: "reversed"
            },
            "new"
        );
    });
    $("#discountSearchSelect").on("click", function () {
        performSearchOnClick(
            {
                maxid: 0,
                includesearchresult: false,
                sortdiscount: maxIntegerValue,
                ordertype: "reversed"
            },
            "discount"
        );
    });

    performSearchAndCache(
        {
            maxid: 0,
            includesearchresult: false,
            sortviews: maxIntegerValue,
            ordertype: "reversed"
        },
        "recommended"
    );
});