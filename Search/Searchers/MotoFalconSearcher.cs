using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using System.Collections.Generic;
using ShopServices.Search.Models;

namespace ShopServices.Search.Searchers;

/// <remarks>
/// Note: <c>MotoFalconSearcher</c> ignores <c>exactArticle</c> flag state and returns all search results (up to 8).
/// </remarks>
internal class MotoFalconSearcher : Searcher
{
	private readonly Uri searchUri;

	public override string Source => "moto-falcon.ru";
	public override string Name => "Falcon";

	public MotoFalconSearcher(string article, bool exactArticle, ILogger logger) :
		base(article, exactArticle, logger)
	{
		searchUri = MakeSearchUri();
	}

	private Uri MakeSearchUri()
	{
		const string url = "http://moto-falcon.ru/search/";
		return new Uri(QueryHelpers.AddQueryString(url, "searchstring", article));
	}

	protected override string GetSearchReference(string article) => searchUri.ToString();

	protected override async Task<HttpResponseMessage> Fetch()
	{
		using (HttpPool.AllocatedHttp allocatedHttp = await HttpPool.Acquire())
		{
			return await allocatedHttp.Http.GetAsync(searchUri);
		}
	}

	protected override async Task<IEnumerable<Product>> Parse(HttpContent content)
	{
		// Natively limited to 21 results
		const int limit = 8;
		var doc = new HtmlDocument();
		doc.Load(await content.ReadAsStreamAsync());
		HtmlNode root = doc.DocumentNode;
		HtmlNodeCollection productNodes =
			root.SelectNodes("//form[contains(@class,'product_brief_block')]/div[contains(@class,'padder')]//a");
		if (productNodes == null)
			return new Product[0];
		var products = new List<Product>(limit);
		foreach (HtmlNode productNode in productNodes)
		{
			if (products.Count == limit)
			{
				logger.LogWarning($"Abrupted processing of {productNodes.Count} products from '{Source}' on {limit}.");
				break;
			}
			string title = productNode.InnerText;
			string reference = productNode.Attributes["href"]?.Value;
			if (reference != null)
				reference = $"http://moto-falcon.ru{reference}";
			var product = new Product(null, title, null, null, reference) { Completed = true };
			products.Add(product);
		}
		return products.ToArray();
	}
}
