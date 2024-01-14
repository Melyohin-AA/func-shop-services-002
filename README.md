<div align="center">
  <h1 align="center">func-shop-services-002</h2>
  <p align="center">An Azure Function App for the one specific e-shop's needs</p>
</div>

Current tasks: https://docs.google.com/spreadsheets/d/15q-sjOsu0wgkCtnZ0fMapuC013Fy_olsk4irZE5OTz8/edit?usp=sharing

# Services

## Search

Search service allows to search for a product by its article agregating search results from several providers.

Providers:
* `kkkk-parts.ru`
* `bikeland.ru`
* `mr-moto.ru`
* `pilotmoto.ru`
* `ktmzip.ru`
* `motodart.ru`
* `moto-falcon.ru`

### Endpoints

#### /search

`/search` is an endpoint of GUI for the search service.

#### /api/search

`/api/search` is an endpoint of API for the search service.

Supported methods:
* `GET` - retrieves info about the products found at the providers by the passed article.

##### GET

Parameters:
* `article` - an article to search by; string value; required parameter.
* `exact` - a flag option to include only those products which article is the same as the specified one; `true` for true state and any other value for false state; optional parameter, false by default.

Normally returns JSON object which complies the following structure:
* `formatVersion` (`integer`) - version of the structure; the current version is `2`
* `scopes` (`array` of `objects`) - collection of objects which contain search results per each provider
  * `name` (`string`, `null`) - name of the provider
  * `source` (`string`, `null`) - domain of the provider
  * `ref` (`string`, `null`) - link to a page with the search results of the provider
  * `error` (`string`, `null`) - error message
  * `products` (`array` of `objects`, `null`) - collection of objects which contain search results within the provider; may be `null` if `error` is not `null`
    * `article` (`string`, `null`) - real article of the found product
    * `title` (`string`, `null`) - title of the found product
    * `availability` (`string`, `null`) - the number of available units or other availability info
    * `price` (`string`, `null`) - price of the found product
    * `ref` (`string`, `null`) - link to the found product's page

If no parameters are provided, redirects to `/search` endpoint (legacy behaviour).
