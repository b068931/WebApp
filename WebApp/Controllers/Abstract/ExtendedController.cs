using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;
using WebApp.Utilities.Exceptions;

namespace WebApp.Controllers.Abstract
{
	public class ExtendedController : Controller
	{
		protected async Task<string> RenderViewAsync<TModel>(string viewName, TModel model, bool partial = false)
		{
			if (string.IsNullOrEmpty(viewName))
			{
				viewName = ControllerContext.ActionDescriptor.ActionName;
			}

			ViewData.Model = model;

			using (var writer = new StringWriter())
			{
				IViewEngine viewEngine =
					(HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine))
						as ICompositeViewEngine
					) ?? throw new ViewRenderException("We are fucked.");

				ViewEngineResult viewResult = viewEngine.FindView(ControllerContext, viewName, !partial);
				if (viewResult.Success == false)
				{
					throw new ViewRenderException($"View with name {viewName} does not exist.");
				}

				ViewContext viewContext = new ViewContext(
					ControllerContext,
					viewResult.View,
					ViewData,
					TempData,
					writer,
					new HtmlHelperOptions()
				);

				await viewResult.View.RenderAsync(viewContext);
				return writer.GetStringBuilder().ToString();
			}
		}

		protected int GetUserId()
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
				return 0;

			return int.Parse(userId);
		}
	}
}
