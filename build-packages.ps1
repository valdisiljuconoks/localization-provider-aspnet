cd .\.nuget

.\nuget.exe pack ..\src\DbLocalizationProvider.AspNet\DbLocalizationProvider.AspNet.csproj -Properties Configuration=Release #-IncludeReferencedProjects
.\nuget.exe pack ..\src\DbLocalizationProvider.AdminUI\DbLocalizationProvider.AdminUI.csproj -Properties Configuration=Release #-IncludeReferencedProjects
.\nuget.exe pack ..\src\DbLocalizationProvider.Xliff\DbLocalizationProvider.Xliff.csproj -Properties Configuration=Release #-IncludeReferencedProjects

cd ..\
