dotnet build Cross.Cache.sln --configuration Release
REM nuget.exe pack config.nuspec -Symbols -SymbolPackageFormat snupkg
nuget.exe pack config.nuspec -Symbols
