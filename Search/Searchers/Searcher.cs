using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShopServices.Search.Models;

namespace ShopServices.Search.Searchers;

internal abstract class Searcher
{
	protected readonly string article;
	protected readonly bool exactArticle;
	protected readonly ILogger logger;

	public abstract string Source { get; }
	public abstract string Name { get; }

	public Searcher(string article, bool exactArticle, ILogger logger)
	{
		this.article = article;
		this.exactArticle = exactArticle;
		this.logger = logger;
	}

	public async Task<Scope> Search()
	{
		string searchReference = GetSearchReference(article);
		HttpResponseMessage response;
		try
		{
			response = await Fetch();
		}
		catch (Exception e)
		{
			logger.LogError(e, $"Failed to fetch from '{Source}'");
			return new Scope(Name, Source, searchReference, "Нет ответа", null);
		}
		if (!response.IsSuccessStatusCode)
		{
			logger.LogError($"Unexpected resposnse code '{(int)response.StatusCode}' from '{Source}'");
			return new Scope(Name, Source, searchReference,
				$"Неожидаемый ответ, код '{(int)response.StatusCode}'", null);
		}
		try
		{
			IEnumerable<Product> products = await Parse(response.Content);
			return new Scope(Name, Source, searchReference, null, products.Where(p => p != null).ToArray());
		}
		catch (Exception e)
		{
			logger.LogError(e, $"Failed to parse response from '{Source}'");
			return new Scope(Name, Source, searchReference, "Не удалось обработать результаты поиска", null);
		}
	}

	protected abstract string GetSearchReference(string article);
	protected abstract Task<HttpResponseMessage> Fetch();
	protected abstract Task<IEnumerable<Product>> Parse(HttpContent content);
}
