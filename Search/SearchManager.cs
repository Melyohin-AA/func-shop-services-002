using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ShopServices.Functions.Search;

public static class SearchManager
{
	public static async Task<string> Search(string article, bool exactArticle, ILogger logger)
	{
		var searchers = new Searcher[] {
			new K4PartsSearcher(article, exactArticle, logger),
			new BikelandSearcher(article, exactArticle, logger),
			new MrMotoSearcher(article, exactArticle, logger),
			new PilotMotoSearcher(article, exactArticle, logger),
			new KtmZipSearcher(article, exactArticle, logger),
			new MotodartSearcher(article, exactArticle, logger),
			new MotoFalconSearcher(article, exactArticle, logger),
		};
		try
		{
			Searcher.Scope[] scopes = await Task.WhenAll(searchers.Select(s => s.Search()));
			var result = new Searcher.Result(scopes);
			return result.ToJson().ToString(Newtonsoft.Json.Formatting.None);
		}
		catch (Exception e)
		{
			logger.LogError(e, $"Failed to search for '{article}'");
			return null;
		}
	}
}
