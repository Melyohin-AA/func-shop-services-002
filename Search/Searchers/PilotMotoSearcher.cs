using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using ShopServices.Search.Models;

namespace ShopServices.Search.Searchers;

internal class PilotMotoSearcher : ProductCardSearcherSearcher
{
	public override string Source => "pilotmoto.ru";
	public override string Name => "PILOT";

	public PilotMotoSearcher(string article, bool exactArticle, ILogger logger) :
		base(article, exactArticle, logger) { }

	protected override Uri MakeSearchUri()
	{
		const string url = $"https://pilotmoto.ru/catalog/search/";
		return new Uri(QueryHelpers.AddQueryString(url, "search_str", article));
	}

	protected override Product[] ParseSearchResult(HtmlDocument doc)
	{
		// Natively limited to 20 results
		const int limit = 8;
		HtmlNode root = doc.DocumentNode;
		HtmlNodeCollection productNodes = root.SelectNodes("id('form_goodlist')//h3/a");
		if (productNodes == null)
			return new Product[0];
		var products = new List<Product>(limit);
		int i = 0;
		foreach (HtmlNode productLink in productNodes)
		{
			if (i++ == limit)
			{
				logger.LogWarning(
					$"Cannot process {productNodes.Count} products from '{Source}', max number is {limit}.");
				break;
			}
			string productCardUrl =  productLink.Attributes["href"].Value;
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
			root.SelectSingleNode("//p[@class='sub-info']/span[contains(text(),'Артикул')]/span")?.InnerText;
		if (realArticle != null)
			realArticle = realArticle.Trim();
		if (exactArticle && (realArticle != article)) return null;
		product.Article = realArticle;
		product.Title = root.SelectSingleNode("//div[@class='title']/h1")?.InnerText;
		product.Availability =
			root.SelectSingleNode("id('form_goodcard')//span[../div[contains(@class,'product-info__cnt')]]")?.InnerText;
		HtmlNode priceNode = root.SelectSingleNode("//p[@class='price-product']");
		string price = priceNode?.InnerText?.Trim();
		string currency = GetCurrencyFromPriceNode(priceNode);
		product.Price = $"{price} {currency}";
		product.Completed = true;
		return product;
	}

	private string GetCurrencyFromPriceNode(HtmlNode priceNode)
	{
		const string currencyToken = "fa-";
		if (priceNode == null) return null;
		HtmlNode currencyNode = priceNode.SelectSingleNode("./i[contains(@class,'fa')]");
		if (currencyNode == null) return null;
		string class_ = currencyNode.GetAttributeValue("class", null);
		if (class_ == null) return null;
		foreach (string classWord in class_.Split())
		{
			if (classWord.StartsWith(currencyToken))
			{
				string currency = classWord.Substring(currencyToken.Length);
				return currency;
			}
		}
		return null;
	}
}
