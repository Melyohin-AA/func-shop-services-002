using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace ShopServices.Functions;

public static class HttpSearchEP
{
	[FunctionName(nameof(HttpSearchEP))]
	public static Task<IActionResult> Run(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search")] HttpRequest request,
		ILogger logger)
	{
		return Task.FromResult(SearchGUI.Get());
	}
}
