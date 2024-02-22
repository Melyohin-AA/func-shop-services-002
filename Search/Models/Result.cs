using System;
using Newtonsoft.Json.Linq;

namespace ShopServices.Search.Models;

internal class Result
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
