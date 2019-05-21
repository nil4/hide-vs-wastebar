@echo off

nuget restore
msbuild HideWasteBar.sln /p:Configuration=Release
