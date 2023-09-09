using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Database.Models;
using WebApp.Services.Database.Products;
using WebApp.Utilities.Other;
using WebApp.ViewModels.Other;

namespace WebApp.Controllers.Products
{
	[Route("/sizes")]
	[Authorize(Roles = "admin")]
	public class SizesController : Controller
	{
		private readonly SizesManager _sizes;
		private readonly Performer<SizesController> _performer;

		private ResultWithErrorVM<List<Size>> GetViewModel(string error = "")
		{
			return new()
			{
				Result = _sizes.GetAllSizes(),
				Error = error
			};
		}
		private IActionResult PerformAction(Action callback)
		{
			return _performer.PerformActionMessage(
				() =>
				{
					callback();
					return View("AdminPage", GetViewModel());
				},
				(message) => View("AdminPage", GetViewModel(message))
			);
		}

		public SizesController(SizesManager sizes, ILogger<SizesController> logger)
		{
			_sizes = sizes;
			_performer = new Performer<SizesController>(logger);
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public IActionResult Create(
			[FromForm(Name = "sizeName")] string sizeName)
		{
			return PerformAction(() => _sizes.CreateSize(sizeName));
		}

		[HttpPost("action/update")]
		[ValidateAntiForgeryToken]
		public IActionResult Update(
			[FromForm(Name = "sizeId")] int id,
			[FromForm(Name = "sizeName")] string sizeName)
		{
			return PerformAction(() => _sizes.UpdateSize(id, sizeName));
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public IActionResult Delete(
			[FromForm(Name = "sizeId")] int id)
		{
			return PerformAction(() => _sizes.DeleteSize(id));
		}

		public IActionResult Index()
		{
			return View("AdminPage", GetViewModel());
		}
	}
}
