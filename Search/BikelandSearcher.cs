using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ShopServices.Functions.Search;

public class BikelandSearcher : Searcher
{
	public override string Source => "bikeland.ru";
	public override string Name => "BikeLand";

	public BikelandSearcher(string article, bool exactArticle, ILogger logger) :
		base(article, exactArticle, logger) { }

	protected override string GetSearchReference(string article) =>
		$"https://bikeland.ru/#/query={article}";

	protected override async Task<HttpResponseMessage> Fetch()
	{
		const string url = "https://api.searchbooster.net/api/cbd9e233-e8bd-4c4b-ad14-52eca7c5a2e1/completions";
		var uri = new Uri(QueryHelpers.AddQueryString(url, "query", article));
		using (HttpPool.AllocatedHttp allocatedHttp = await HttpPool.Acquire())
		{
			return await allocatedHttp.Http.GetAsync(uri);
		}
	}

	protected override async Task<IEnumerable<Product>> Parse(HttpContent content)
	{
		string json = await content.ReadAsStringAsync();
		JObject jobj = JObject.Parse(json);
		var searchBox = (JArray)jobj.GetValue("searchBox");
		var products = new List<Product>(searchBox.Count);
		foreach (JToken searchBoxItem in searchBox)
		{
			var productJobj = (JObject)searchBoxItem;
			string realArticle = (string)(JValue)productJobj.GetValue("offer_code");
			if (exactArticle && (realArticle != article)) continue;
			var text = (JObject)productJobj.GetValue("text");
			string title = (string)(JValue)text.GetValue("html");
			var offer = (JObject)productJobj.GetValue("offer");
			bool? isAvailable = (bool?)(JValue)offer.GetValue("available");
			int? price = (int?)(JValue)productJobj.GetValue("price");
			string currency = (string)(JValue)productJobj.GetValue("currency");
			string reference = (string)(JValue)productJobj.GetValue("url");
			if ((reference != null) && reference.StartsWith("/"))
				reference = $"https://bikeland.ru{reference}";
			var product = new Product(realArticle, title, isAvailable.ToString(), $"{price} {currency}", reference) {
				Completed = true,
			};
			products.Add(product);
		}
		return products;
	}
}
