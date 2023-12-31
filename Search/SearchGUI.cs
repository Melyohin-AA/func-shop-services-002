using Microsoft.AspNetCore.Mvc;

internal static class SearchGUI
{
	public static IActionResult Get()
	{
		return new ContentResult() {
			Content = 
@"<!DOCTYPE html>
<html lang='en'>

<head>
	<meta charset='UTF-8'>
	<title>Search - ShopServices</title>
</head>

<body>
	<button id='searchButton' onclick='onSearchButtonClick()'>Поиск</button>
	<input id='articleInput' placeholder='артикул' autofocus onkeydown='if (event.keyCode == 13) onSearchButtonClick()'/>
	<br/>
	<label>Строгий поиск</label>
	<input id='exactInput' type='checkbox' checked/>
	<br/>
	<hr/>
	<font id='resultArea' size='4'></font>
</body>

<script>
	var SUPPORTED_FORMAT_VERSION = 1;

	function onSearchButtonClick() {
		if (searchButton.disabled) return;
		article = articleInput.value.trim();
		if (article.length == 0) {
			alert('Поле `Артикул` является обязательным');
			return;
		}
		search(article, exactInput.checked);
	}

	function search(article, exact) {
		searchButton.disabled = true;
		clearResultArea();
		query = formQuery(article, exact);
		url = `/api/search?${query}`;
		fetch(url).then((response) => {
			if (response.status !== 200) {
				console.log(`Failed to fetch, code '${response.status}'`);
				showSearchFailure();
				return;
			}
			console.log('Fetched successfully')
			response.json().then((result) => {
				console.log(result);
				formatVersion = result['formatVersion'];
				if (formatVersion != SUPPORTED_FORMAT_VERSION) {
					console.log(`Unexpected format version: expected '${SUPPORTED_FORMAT_VERSION}', actual is '${formatVersion}'`);
					alert('Загруженная версия страницы больше не поддерживается. Страница будет обновлена при закрытии данного сообщения');
					location.reload();
					return;
				}
				updateResultTable(result.scopes);
				searchButton.disabled = false;
			}).catch((error) => {
				console.log('Failed to parse response:', error);
				showSearchFailure();
			});
		}).catch((error) => {
			console.log('Failed to fetch:', error);
			showSearchFailure();
		});
	}

	function formQuery(article, exact) {
		params = {
			article: article,
			exact: exact,
		};
		return new URLSearchParams(params).toString();
	}

	function clearResultArea() {
		resultArea.innerHTML = null;
	}

	function updateResultTable(scopes) {
		for (let i = 0; i < scopes.length; i++) {
			scope = scopes[i];
			addScopeTitle(scope);
			if (scope.error == null) {
				addScopeTable(scope);
			} else {
				addScopeError(scope);
			}
		}
	}

	function addScopeTitle(scope) {
		resultArea.appendChild(document.createElement('br'));
		scopeCapElem = document.createElement('div');
		scopeCapElem.innerHTML = `<font size='6' color='green'><b>${scope['name']}</b></font>`;
		ref = scope['ref'];
		if (ref != null) {
			searchRefElem = document.createElement('a');
			searchRefElem.setAttribute('href', ref);
			searchRefElem.innerHTML = ref;
			scopeCapElem.innerHTML += '&nbsp';
			scopeCapElem.appendChild(searchRefElem);
		}
		resultArea.appendChild(scopeCapElem);
	}

	function addScopeTable(scope) {
		tableElem = document.createElement('table');
		tableElem.setAttribute('border', '1');
		tableElem.insertRow(0).innerHTML = '<th>Артикул</th><th>Наименование</th><th>Наличие</th><th>Цена</th><th>Ссылка на товар</th>';
		products = scope['products'];
		for (let i = 0; i < products.length; i++) {
			product = products[i];
			row = tableElem.insertRow(-1);
			['article', 'title', 'availability', 'price'].forEach((key) => row.insertCell().innerHTML = product[key]);
			refCell = row.insertCell();
			ref = product['ref'];
			if (ref != null) {
				refElem = document.createElement('a');
				refElem.setAttribute('href', ref);
				refElem.innerHTML = 'ссылка';
				refCell.appendChild(refElem);
			}
		}
		resultArea.appendChild(tableElem);
	}

	function addScopeError(scope) {
		errorElem = document.createElement('p');
		errorElem.innerHTML = `<b>Ошибка: </b>${scope.error}`;
		resultArea.appendChild(errorElem);
	}

	function showSearchFailure() {
		alert('Поиск не удался');
		searchButton.disabled = false;
	}
</script>

</html>", ContentType = "text/html; charset=utf-8"};
	}
}
