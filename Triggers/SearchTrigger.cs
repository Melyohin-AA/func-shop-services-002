using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using ShopServices.Search;

namespace ShopServices.Triggers;

internal static class SearchTrigger
{
	[FunctionName(nameof(SearchTrigger))]
	public static Task<IActionResult> Run(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search")] HttpRequest request,
		ILogger logger)
	{
		return Task.FromResult(SearchGui.Get());
	}
}
