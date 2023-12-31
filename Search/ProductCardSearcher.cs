using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;

namespace ShopServices.Functions.Search;

public abstract class ProductCardSearcherSearcher : Searcher
{
	protected Uri SearchUri { get; }

	public ProductCardSearcherSearcher(string article, bool exactArticle, ILogger logger) :
		base(article, exactArticle, logger)
	{
		SearchUri = MakeSearchUri();
	}

	protected abstract Uri MakeSearchUri();

	protected override string GetSearchReference(string article) => SearchUri.ToString();

	protected override async Task<HttpResponseMessage> Fetch()
	{
		using (HttpPool.AllocatedHttp allocatedHttp = await HttpPool.Acquire())
		{
			return await allocatedHttp.Http.GetAsync(SearchUri);
		}
	}

	protected override async Task<IEnumerable<Product>> Parse(HttpContent content)
	{
		var doc = new HtmlDocument();
		doc.Load(await content.ReadAsStreamAsync());
		Product[] productsFromResult = ParseSearchResult(doc);
		var products = new List<Product>(productsFromResult.Length);
		var productTasks = new List<Task<Product>>(productsFromResult.Length);
		for (int i = 0; i < productsFromResult.Length; i++)
		{
			if (productsFromResult[i].Completed)
				products.Add(productsFromResult[i]);
			else productTasks.Add(FromProductCard(productsFromResult[i]));
		}
		products.AddRange(await Task.WhenAll(productTasks));
		return products;
	}

	protected abstract Product[] ParseSearchResult(HtmlDocument doc);

	private async Task<Product> FromProductCard(Product product)
	{
		HttpResponseMessage response;
		try
		{
			using (HttpPool.AllocatedHttp allocatedHttp = await HttpPool.Acquire())
			{
				response = await allocatedHttp.Http.GetAsync(product.Reference);
			}
		}
		catch (Exception e)
		{
			logger.LogError(e, $"Failed to fetch from '{product.Reference}'");
			return null;
		}
		if (!response.IsSuccessStatusCode)
		{
			logger.LogError($"Unexpected resposnse code '{response.StatusCode}' from '{product.Reference}'");
			return null;
		}
		try
		{
			return await ParseProductCard(product, response.Content, product.Reference);
		}
		catch (Exception e)
		{
			logger.LogError(e, $"Failed to parse response from '{product.Reference}'");
			return null;
		}
	}

	protected abstract Task<Product> ParseProductCard(Product product, HttpContent content, string reference);
}
