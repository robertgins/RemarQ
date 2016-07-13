ECHO Per­form­ing IIS Reset
net stop SPSearchHostController
net stop SPTimerV4
net stop SPAdminV4
net stop SPTraceV4
net stop w3svc
net stop iisadmin
ECHO Purging caches
Del /F /Q /S %LOCALAPPDATA%\Microsoft\WebsiteCache\*.*
Del /F /Q /S %LOCALAPPDATA%\Temp\VWDWebCache\*.*
Del /F /Q /S "%LOCALAPPDATA%\Microsoft\Team Foundation\3.0\Cache\*.*"
Del /F /Q /S "%SystemRoot%\Microsoft.NET\Framework\v2.0.50727\Temporary ASP.NET Files\*.*"
Del /F /Q /S "%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\Temporary ASP.NET Files\*.*"
Del /F /Q /S "%SystemRoot%\Microsoft.NET\Framework64\v2.0.50727\Temporary ASP.NET Files\*.*"
Del /F /Q /S "%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files\*.*"
Del /S "%ALLUSERSPROFILE%\Microsoft\SharePoint\Config\*.xml"
ECHO Complete
net start iisadmin
net start w3svc
net start SPTraceV4
net start SPAdminV4
net start SPTimerV4
net start SPSearchHostController
pause

