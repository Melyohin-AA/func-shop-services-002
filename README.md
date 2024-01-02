<div align="center">
  <h2 align="center">func-shop-services-002</h2>
  <p align="center">An Azure Function App for the one specific e-shop's needs</p>
</div>

### Services

#### Search

Search service allows to search for a product by its article agregating search results from several providers.

Providers:
* `kkkk-parts.ru`
* `bikeland.ru`
* `mr-moto.ru`
* `pilotmoto.ru`
* `ktmzip.ru`

### Endpoints

#### /search

`/search` is an endpoint of GUI for the [search service](#search).

#### /api/search

`/api/search` is an endpoint of API for the [search service](#search).

Parameters:
* `article` - an article to search by; string value; required parameter.
* `exact` - a flag option to include only those products which article is the same as the specified one; `true` for true state and any other value for false state; optional parameter, false by default.

Normally returns JSON object which structure is:
```json
{
	"formatVersion": 2, // Version of the structure
	"scopes": [
		{ // Search results from one provider
			"name": "Name of the provider",
			"source": "Domain of the provider",
			"ref": "Link to a page with the search results of the provider",
			"error": null, // May include error string
			"products": [ // May be `null` if `error` is not `null`
				{ // Single search result
					"article": "Real article of the found product", // Optional
					"title": "Title of the found product", // Optional
					"availability": "The number of available units or other availability info", // Optional
					"price": "Price of the found product", // Optional
					"ref": "Link to the found product's page", // Optional
				},
				// ...
			],
		},
		// ...
	],
}
```

If no parameters are provided, redirects to `/search` endpoint.
