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

		var brand = $("#brand").val();
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
});