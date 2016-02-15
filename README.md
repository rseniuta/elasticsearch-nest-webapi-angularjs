# elasticsearch-nest-webapi-angularjs
Introduction to Elasticsearch with NEST, ASP.NET Web API 2 and AngularJS

##Installation

* Install Elasticsearch from https://www.elastic.co/downloads/elasticsearch
* Ensure Elasticsearch is listening on http://localhost:9200/ 

##Download test data

* Download test data from https://archive.org/download/stackexchange,
* Unzip the archive and copy the `Posts.xml` file to the `data` folder.

##Create the index

* Open the solution in Visual Studio 2015, select Restore NuGet packages and rebuild the solution.
* Make a GET request to http://localhost:53694/api/index?fileName=posts.xml&maxItems=10000

##Run the application
* Debug or Start without debugging to run the Web API in IIS Express.
* Browse to http://localhost:53694/index.html
