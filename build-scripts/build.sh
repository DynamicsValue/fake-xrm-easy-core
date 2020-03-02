cd .. 
cd tests 
dotnet restore
dotnet build --configuration FAKE_XRM_EASY_2013
dotnet build --configuration FAKE_XRM_EASY_2015
dotnet build --configuration FAKE_XRM_EASY_2016
dotnet build --configuration FAKE_XRM_EASY_365
dotnet test --configuration FAKE_XRM_EASY_365


