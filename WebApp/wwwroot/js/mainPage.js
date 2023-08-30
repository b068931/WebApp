$(function () {
    const currentDate = new Date();

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
});