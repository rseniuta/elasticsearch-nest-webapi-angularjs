# elasticsearch-nest-webapi-angularjs

Modernized Elasticsearch sample using NEST, ASP.NET Web API 2, and an Angular 17 front-end.

## Installation

* Install Elasticsearch 2.3.0 from https://www.elastic.co/downloads/past-releases/elasticsearch-2-3-0
* Ensure Elasticsearch is listening on http://localhost:9200/

## Download test data

* Download test data from https://archive.org/download/stackexchange,
* Unzip the archive and copy the `Posts.xml` file to the `data` folder.

## Create the index

* Open the solution in Visual Studio, select Restore NuGet packages and rebuild the solution.
* Make a GET request to http://localhost:53694/api/index?fileName=posts.xml&maxItems=10000

## Run the application

1. Restore and build the ASP.NET Web API project as usual.
2. Install Node.js 18.17+ and npm 9+.
3. From the `client/` directory run `npm install` and then `npm run build` to compile the Angular application into `elasticsearch-nest-webapi-angularjs/Content/client-app`.
4. Start the Web API (`F5`/`Ctrl+F5`). Browse to http://localhost:53694/ to load the Angular client.

### Client development workflow

* `npm start` runs the Angular dev server with live reload.
* `npm test` executes the Karma unit test suite.
* `npm run lint` checks the project with ESLint.
