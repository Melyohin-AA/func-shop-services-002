using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ShopServices.Search.Models;

namespace ShopServices.Search.Searchers;

internal class K4PartsSearcher : Searcher
{
	private const int Limit = 8;

	public override string Source => "kkkk-parts.ru";
	public override string Name => "BUTOVO";

	public K4PartsSearcher(string article, bool exactArticle, ILogger logger) :
		base(article, exactArticle, logger) { }

	protected override string GetSearchReference(string article) => "http://kkkk-parts.ru/shop/";

	protected override async Task<HttpResponseMessage> Fetch()
	{
		const string url = "http://kkkk-parts.ru/moto_retail_get_store.aspx";
		var uri = new Uri(QueryHelpers.AddQueryString(url, new Dictionary<string, string> {
			{ "start", "0" },
			{ "end", Limit.ToString() },
			{ "NR", article },
			{ "idClient", "0" },
			{ "ShowPrices", "1" },
			{ "firm", "0" },
		}));
		using (HttpPool.AllocatedHttp allocatedHttp = await HttpPool.Acquire())
		{
			return await allocatedHttp.Http.GetAsync(uri);
		}
	}

	protected override async Task<IEnumerable<Product>> Parse(HttpContent content)
	{
		string json = await content.ReadAsStringAsync();
		if (json.StartsWith("ERROR"))
		{
			if (json.Contains("не удалось получить ответ"))
				return new Product[0]; // That is a normal response if nothing is found
			logger.LogError($"Fetched unexpected response '{json}' from '{Source}'");
		}
		JArray jarr = JArray.Parse(json);
		var products = new List<Product>(jarr.Count);
		foreach (JToken jarrItem in jarr)
		{
			var productJobj = (JObject)jarrItem;
			string realArticle = (string)(JValue)productJobj.GetValue("f3");
			if (exactArticle && (realArticle != article)) continue;
			string title1 = (string)(JValue)productJobj.GetValue("f1");
			string title2 = (string)(JValue)productJobj.GetValue("f5");
			string title = $"{title1} {title2}";
			string availability = (string)(JValue)productJobj.GetValue("stat");
			string price = (string)(JValue)productJobj.GetValue("f8b");
			string reference = $"http://kkkk-parts.ru/shop/cards/{realArticle}/{realArticle}.html";
			var product = new Product(realArticle, title, availability, price, reference) { Completed = true };
			products.Add(product);
		}
		return products;
	}
}
