cd .\.nuget

nuget push .\LocalizationProvider.AspNet.5.3.0.nupkg -source https://api.nuget.org/v3/index.json
nuget push .\LocalizationProvider.AdminUI.5.3.0.nupkg -source https://api.nuget.org/v3/index.json
nuget push .\LocalizationProvider.Xliff.5.3.0.nupkg -source https://api.nuget.org/v3/index.json

cd ..\
