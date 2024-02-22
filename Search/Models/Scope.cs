using System;
using Newtonsoft.Json.Linq;

namespace ShopServices.Search.Models;

internal class Scope
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
