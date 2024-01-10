using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace ShopServices.Functions.Search;

public class MotodartSearcher : ProductCardSearcherSearcher
{
	public override string Source => "motodart.ru";
	public override string Name => "motodart.ru"; //~ rename

	public MotodartSearcher(string article, bool exactArticle, ILogger logger) :
		base(article, exactArticle, logger) { }

	protected override Uri MakeSearchUri()
	{
		const string url = "https://motodart.ru/search/";
		return new Uri(QueryHelpers.AddQueryString(url, "query", article));
	}

	protected override Product[] ParseSearchResult(HtmlDocument doc)
	{
		// Natively limited to 20 results
		const int limit = 8;
		HtmlNode root = doc.DocumentNode;
		HtmlNodeCollection productNodes = root.SelectNodes("id('product-list')/ul[contains(@class,'product-list')]/li/div[contains(@class,'box-container')]");
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
			string realArticle = productNode.SelectSingleNode(".//div[contains(@class,'art_num')]")?.InnerText;
			if (realArticle != null)
				realArticle = realArticle.Trim();
			if (exactArticle && (realArticle != article)) continue;
			HtmlNode titleNode = productNode.SelectSingleNode(".//div[contains(@class,'product-name')]/p/a");
			string title = null, reference = null;
			if (titleNode != null)
			{
				title = titleNode.SelectSingleNode("./span")?.InnerText;
				reference = titleNode.Attributes["href"]?.Value;
				if (reference != null)
					reference = $"https://motodart.ru{reference}";
			}
			string price = productNode.SelectSingleNode(".//div[contains(@class,'pricing')]/span")?.InnerText;
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
		HtmlNodeCollection stockCountNodes = root.SelectNodes("//div[contains(@class,'stock-div')]");
		if (stockCountNodes != null)
		{
			var stockCounts = new List<string>();
			foreach (HtmlNode stockCountNode in stockCountNodes)
			{
				string count = stockCountNode.SelectSingleNode(".//span")?.InnerText;
				string name = stockCountNode.SelectSingleNode(".//div[contains(@class,'stock-name')]")?.InnerText;
				stockCounts.Add(count + name);
			}
			product.Availability = string.Join("; ", stockCounts);
		}
		product.Completed = true;
		return product;
	}
}
