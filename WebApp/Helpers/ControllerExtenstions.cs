using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Helpers
{
	//stolen from stackoverflow
	public static class ControllerExtenstions
	{
		public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
		{
			if (string.IsNullOrEmpty(viewName))
			{
				viewName = controller.ControllerContext.ActionDescriptor.ActionName;
			}

			controller.ViewData.Model = model;

			using (var writer = new StringWriter())
			{
				IViewEngine viewEngine =
					(controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine))
						as ICompositeViewEngine
					) ?? throw new ViewRenderException("We are fucked.");

				ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);
				if (viewResult.Success == false)
				{
					throw new ViewRenderException($"View with name {viewName} does not exist.");
				}

				ViewContext viewContext = new ViewContext(
					controller.ControllerContext,
					viewResult.View,
					controller.ViewData,
					controller.TempData,
					writer,
					new HtmlHelperOptions()
				);

				await viewResult.View.RenderAsync(viewContext);
				return writer.GetStringBuilder().ToString();
			}
		}
	}
}
