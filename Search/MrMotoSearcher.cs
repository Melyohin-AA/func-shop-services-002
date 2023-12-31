using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace ShopServices.Functions.Search;

public class MrMotoSearcher : ProductCardSearcherSearcher
{
	public override string Source => "mr-moto.ru";
	public override string Name => "MrMoto";

	public MrMotoSearcher(string article, bool exactArticle, ILogger logger) :
		base(article, exactArticle, logger) { }

	protected override Uri MakeSearchUri()
	{
		const string url = "https://mr-moto.ru/catalog/search/";
		return new Uri(QueryHelpers.AddQueryString(url, "q", article));
	}

	protected override Product[] ParseSearchResult(HtmlDocument doc)
	{
		// Natively limited to 24 results
		const int limit = 8;
		HtmlNode root = doc.DocumentNode;
		HtmlNodeCollection productNodes = root.SelectNodes("//div[@class='page-content']" +
			"/div[@class='catalog-list']//div[@class='slider-card__title']/a[contains(@href,'/catalog/')]");
		if (productNodes == null)
			return new Product[0];
		var products = new List<Product>(limit);
		foreach (HtmlNode productLink in productNodes)
		{
			if (products.Count == limit)
			{
				logger.LogWarning($"Abrupted processing of {productNodes.Count} products from '{Source}' on {limit}.");
				break;
			}
			string productCardPath =  productLink.Attributes["href"].Value;
			string productCardUrl = $"https://mr-moto.ru{productCardPath}";
			var product = new Product(null, null, null, null, productCardUrl);
			products.Add(product);
		}
		return products.ToArray();
	}

	protected override async Task<Product> ParseProductCard(Product product, HttpContent content, string reference)
	{
		var doc = new HtmlDocument();
		doc.Load(await content.ReadAsStreamAsync());
		HtmlNode root = doc.DocumentNode;
		string realArticle =
			root.SelectSingleNode("//div[@class='part__code']/p[contains(text(),'Артикул')]/span")?.InnerText;
		if (realArticle != null)
			realArticle = realArticle.Trim();
		if (exactArticle && (realArticle != article)) return null;
		product.Article = realArticle;
		product.Title = root.SelectSingleNode("//h1[@class='product-title']")?.InnerText;
		HtmlNode availabilityNode = root.SelectSingleNode("id('page-tab-store')/div[@class='availability']");
		product.Availability = GetCountFromAvailabilityNode(availabilityNode)?.ToString();
		product.Price = root.SelectSingleNode("//div[@class='part__price-title']")?.InnerText?.Trim();
		product.Completed = true;
		return product;
	}

	private int? GetCountFromAvailabilityNode(HtmlNode availabilityNode)
	{
		if (availabilityNode == null) return null;
		HtmlNodeCollection columns = availabilityNode.SelectNodes("./table/tbody/tr/td");
		if (columns == null) return null;
		int totalCount = 0;
		int i = 0;
		foreach (HtmlNode column in columns)
		{
			if (i++ < 3) continue;
			string rawColumnCount = column.InnerText.Replace("+", null);
			if (int.TryParse(rawColumnCount, out int columnCount))
				totalCount += columnCount;
		}
		return totalCount;
	}
}
