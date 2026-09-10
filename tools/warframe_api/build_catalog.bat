@echo off
setlocal
cd /d "%~dp0\..\.."
if not exist "bin" mkdir "bin"
rem The standalone repository bundles the audited census under knowledge.
rem RENOVICE_CENSUS_SITES remains an explicit override for a newly audited one.
set "CENSUS_SITES=%RENOVICE_CENSUS_SITES%"
if "%CENSUS_SITES%"=="" set "CENSUS_SITES=%CD%\knowledge\research\DE LUAU TRANSLATOR\NATIVE API AND LIVE CANDIDATE CENSUS\result_consumption_sites.tsv"
if not exist "%CENSUS_SITES%" (
  echo [wf_api] FAIL: audited census not found at "%CENSUS_SITES%"
  exit /b 1
)
g++ -std=c++17 -O2 -Wall -Wextra -Werror -static -o "bin\wf_api_catalog.exe" "tools\warframe_api\wf_api_catalog.cpp"
if errorlevel 1 exit /b 1
"bin\wf_api_catalog.exe" ^
  "api\warframe\selection_seeds.tsv" ^
  "%CENSUS_SITES%" ^
  "api\warframe\contracts.tsv" ^
  "api\warframe\selected_catalog.tsv"
if errorlevel 1 exit /b 1
echo [wf_api] catalog build PASS
exit /b 0
