$(function () {
	var searchQuery = {
		maxid: 0
	};

	function addNewProduct(product) {
		var productContainer = $("<div>").html(product.name);
		var productImage = $("<img>", { src: "/images/image/" + product.mainImageId });
		var showFull = $("<a>", { href: "/products/product/" + product.id, target: "_blank" }).html("showfull");

		productContainer.append(showFull);
		productContainer.append(productImage);
		$("#productsContainer").append(productContainer);
	}

	function onNoProducts() {
		$("#loadMore")
			.removeClass("btn-outline-secondary")
			.addClass("btn-outline-danger")
			.addClass("disabled")
			.html("Більше нічого немає");
	}

	function updateSearchPage(element) {
		if (searchQuery["maxdate"] != undefined) {
			searchQuery["maxdate"] = element.date;
		}
		else if (searchQuery["maxviews"] != undefined) {
			searchQuery["maxviews"] = element.viewsCount;
		}
		else if (searchQuery["maxstars"] != undefined) {
			searchQuery["maxstars"] = element.stars;
		}
	}

	function serveNewPortion() {
		$.get(
			"/products/search",
			searchQuery,
			function (data, status) {
				if (status === "success") {
					if (data.length === 0) {
						onNoProducts();
					}

					data.forEach(
						element => {
							searchQuery["maxid"] = Math.max(searchQuery["maxid"], element.id);
							addNewProduct(element);
						}
					)

					updateSearchPage(data[data.length - 1]);
				}
				else {
					alert("Ooops. Unable to load new products.");
				}
			},
			"json"
		)
	}

	function readSearchAndServe() {
		$("#productsContainer").children().remove();
		$("#loadMore")
			.addClass("btn-outline-secondary")
			.removeClass("btn-outline-danger")
			.removeClass("disabled")
			.html("Завантажити ще");

		var newSearchQuery = {
			maxid: 0
		};

		var brand = $("#brandSearch").val();
		if (brand != 0) {
			newSearchQuery["brand"] = brand;
		}

		var category = $("#category").val();
		if (category != 0) {
			newSearchQuery["category"] = category;
		}

		var maxPrice = $("#maxPrice").val();
		var minPrice = $("#minPrice").val();
		if (maxPrice != minPrice) {
			if (maxPrice != "") {
				newSearchQuery["maxprice"] = maxPrice;
			}
			if (minPrice != "") {
				newSearchQuery["minprice"] = minPrice;
			}
		}

		var queryText = $("#query").val();
		if (queryText != "") {
			newSearchQuery["namecontains"] = queryText;
		}

		var minRating = $("#minRating").val();
		if (minRating != "") {
			newSearchQuery["minrating"] = minRating;
		}

		var sortType = $("#sortType").val();
		if (sortType != 0) {
			newSearchQuery["ordertype"] = $("#orderType").val();
			switch (sortType) {
				case "date":
					newSearchQuery["maxdate"] =
						(newSearchQuery["ordertype"] == "regular")
							? "0001-01-01"
							: "9999-12-31";
					break;

				case "views":
					newSearchQuery["maxviews"] = 
						(newSearchQuery["ordertype"] == "regular")
							? 0
							: 2147483647;
					break;

				case "stars":
					newSearchQuery["maxstars"] =
						(newSearchQuery["ordertype"] == "regular")
							? 0
							: 2147483647;
					break;
			}
		}

		searchQuery = newSearchQuery;
		serveNewPortion();
	}

	$("#searchToggler").animatedToggle($("#searchInputsContainer"));
	$("#searchButton").on("click", function () {
		readSearchAndServe();
	});
	$("#loadMore").on("click", function () {
		serveNewPortion();
	});

	readSearchAndServe();

	$(".search-element").each(function () {
		$(this)
			.find(".search-toggler")
			.animatedSlideDown(
				$(this).find(".search-target"),
				$(this).find(".maximizer")
			);
	});
});