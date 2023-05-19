cd .\.nuget

.\nuget.exe push .\LocalizationProvider.AspNet.6.5.2.nupkg -source https://api.nuget.org/v3/index.json
.\nuget.exe push .\LocalizationProvider.AdminUI.6.5.2.nupkg -source https://api.nuget.org/v3/index.json
.\nuget.exe push .\LocalizationProvider.Xliff.6.5.2.nupkg -source https://api.nuget.org/v3/index.json

cd ..\
