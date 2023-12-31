using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ShopServices.Functions.Search;

public abstract class Searcher
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

	public class Product
	{
		public string Article { get; set; }
		public string Title { get; set; }
		public string Availability { get; set; }
		public string Price { get; set; }
		public string Reference { get; set; }
		public bool Completed { get; set; }

		public Product(string article, string title, string availability, string price, string reference)
		{
			Article = article;
			Title = title;
			Availability = availability;
			Price = price;
			Reference = reference;
		}

		public override string ToString()
		{
			return $"[article: {Article}; title: {Title}; availability: {Availability}; " +
				$"price: {Price}; ref: {Reference}]";
		}

		public JObject ToJson()
		{
			return new JObject() {
				{ "article", new JValue(Article) },
				{ "title", new JValue(Title) },
				{ "availability", new JValue(Availability) },
				{ "price", new JValue(Price) },
				{ "ref", new JValue(Reference) },
			};
		}
	}

	public class Scope
	{
		public string Name { get; }
		public string Source { get; }
		public string Reference { get; }
		public string Error { get; }
		public Product[] Products { get; }

		public Scope(string name, string source, string reference, string error, Product[] products)
		{
			Name = name;
			Source = source;
			Reference = reference;
			Error = error;
			Products = products;
		}

		public JObject ToJson()
		{
			return new JObject() {
				{ "name", new JValue(Name) },
				{ "source", new JValue(Source) },
				{ "ref", new JValue(Reference) },
				{ "error", new JValue(Error) },
				{ "products", ProductsToJson() },
			};
		}
		private JToken ProductsToJson()
		{
			if (Products == null) return null;
			var jarr = new JArray();
			for (int i = 0; i < Products.Length; i++)
				jarr.Add(Products[i].ToJson());
			return jarr;
		}
	}

	public class Result
	{
		public const int FormatVersion = 1;

		public Scope[] Scopes { get; }

		public Result(Scope[] scopes)
		{
			Scopes = scopes;
		}

		public JObject ToJson()
		{
			return new JObject() {
				{ "formatVersion", new JValue(FormatVersion) },
				{ "scopes", ScopesToJson() },
			};
		}

		private JArray ScopesToJson()
		{
			var jarr = new JArray();
			for (int i = 0; i < Scopes.Length; i++)
				if (Scopes[i] != null)
					jarr.Add(Scopes[i].ToJson());
			return jarr;
		}
	}
}
