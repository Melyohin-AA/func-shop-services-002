using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace ShopServices.Functions.Search;

public class KtmZipSearcher : ProductCardSearcherSearcher
{
	public override string Source => "ktmzip.ru";
	public override string Name => "ktmzip";

	public KtmZipSearcher(string article, bool exactArticle, ILogger logger) :
		base(article, exactArticle, logger) { }

	protected override Uri MakeSearchUri()
	{
		const string url = "https://ktmzip.ru/rezultat-poiska";
		return new Uri(QueryHelpers.AddQueryString(url, "query", article));
	}

	protected override Product[] ParseSearchResult(HtmlDocument doc)
	{
		// Natively [un]limited to 116 results
		const int limit = 8;
		HtmlNode root = doc.DocumentNode;
		HtmlNodeCollection productNodes = root.SelectNodes("//div[@class='wrap_prod']");
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
			string realArticle = productNode.SelectSingleNode(".//div[contains(@class,'article')]")?.InnerText;
			if (realArticle != null)
				realArticle = realArticle.Replace("Art: ", null).Trim();
			if (exactArticle && (realArticle != article)) continue;
			HtmlNode titleNode = productNode.SelectSingleNode(".//div[contains(@class,'product-title')]");
			string title = null, reference = null;
			if (titleNode != null)
			{
				title = titleNode.SelectSingleNode(".//h4")?.InnerText;
				reference = titleNode.SelectSingleNode("./a")?.Attributes["href"]?.Value;
				if (reference != null)
					reference = $"https://ktmzip.ru{reference}";
			}
			string price = productNode.SelectSingleNode(".//div[contains(@class,'price-new')]")?.InnerText?.Trim();
			var product = new Product(realArticle, title, null, price, reference);
			products.Add(product);
		}
		return products.ToArray();
	}

	protected override async Task<Product> ParseProductCard(Product product, HttpContent content, string reference)
	{
		var doc = new HtmlDocument();
		doc.Load(await content.ReadAsStreamAsync());
		HtmlNode root = doc.DocumentNode;
		product.Availability = root.SelectSingleNode("//*[contains(@class,'FB5A26')]")?.InnerText;
		product.Completed = true;
		return product;
	}
}
