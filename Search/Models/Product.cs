using System;
using Newtonsoft.Json.Linq;

namespace ShopServices.Search.Models;

internal class Product
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
