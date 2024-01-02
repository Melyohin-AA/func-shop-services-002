using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using ShopServices.Functions.Search;

namespace ShopServices.Functions;

public static class HttpSearchApiEP
{
	[FunctionName(nameof(HttpSearchApiEP))]
	public static async Task<IActionResult> Run(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/search")] HttpRequest request,
		ILogger logger)
	{
		if (request.Query.Count > 0)
			return await Search(request, logger);
		return new RedirectResult("/search");
	}

	private static async Task<IActionResult> Search(HttpRequest request, ILogger logger)
	{
		if (!request.Query.TryGetValue("article", out StringValues article))
			return new BadRequestObjectResult("'article' parameter is required");
		//request.Query.TryGetValue("nocache", out StringValues nocache);
		bool exactArticle = request.Query.TryGetValue("exact", out StringValues exactParam) ?
			exactParam == "true" : false;
		string results = await SearchManager.Search(article, exactArticle, logger);
		return (results != null) ? new OkObjectResult(results) : new StatusCodeResult(500);
	}
}
