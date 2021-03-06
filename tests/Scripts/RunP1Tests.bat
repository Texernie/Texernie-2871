@echo Off

REM Working directory one level up
cd ..

REM Configuration
set config=Release
set testCategory="P1Tests"
set solutionPath="NuGetGallery.FunctionalTests.sln"
set exitCode=0

REM Required Tools
set msbuild="%PROGRAMFILES(X86)%\MsBuild\14.0\Bin\msbuild"
set xunit="..\packages\xunit.runner.console.2.3.1\tools\net452\xunit.console.exe"
set nuget="nuget.exe"
set mstest="C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\mstest.exe"

REM Clean previous test results
if exist functionaltests.P1.xml (
    del functionaltests.P1.xml
)
if exist NuGetGallery.P1.*.trx (
    del NuGetGallery.P1.*.trx
)

REM Restore packages
if not exist nuget (
    call PowerShell -NoProfile -ExecutionPolicy Bypass -File %cd%\Scripts\DownloadLatestNuGetExeRelease.ps1
)
call %nuget% restore "%solutionPath%" -NonInteractive
if not "%errorlevel%"=="0" goto failure

REM Build the solution
call %msbuild% "%solutionPath%" /p:Configuration="%config%" /p:Platform="Any CPU" /p:CodeAnalysis=true /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diagnostic /nr:false
if not "%errorlevel%"=="0" goto failure

REM Run functional tests
set testDir="NuGetGallery.FunctionalTests\bin\%config%"
copy %nuget% %testDir%
call %xunit% "%testDir%\NuGetGallery.FunctionalTests.dll" -trait "Category=%testCategory%" -xml functionaltests.P1.xml
if not "%errorlevel%"=="0" set exitCode=-1

REM Run web UI tests
call %mstest% /TestContainer:"NuGetGallery.WebUITests.P1\bin\%config%\NuGetGallery.WebUITests.P1.dll" /TestSettings:Local.testsettings /detail:stdout /resultsfile:NuGetGallery.P1.WebUITests.trx
if not "%errorlevel%"=="0" set exitCode=-1

REM Run Load tests
call %mstest% /TestContainer:"NuGetGallery.LoadTests\bin\%config%\NuGetGallery.LoadTests.dll" /TestSettings:Local.testsettings /detail:stdout /category:%testCategory% /resultsfile:NuGetGallery.P1.LoadTests.trx
if not "%errorlevel%"=="0" set exitCode=-1

exit /B %exitCode%

:failure
exit -1