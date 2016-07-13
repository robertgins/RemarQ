add-PSSnapIn Microsoft.SharePoint.PowerShell

$currentDir =(Get-Item -Path ".\" -Verbose).FullName
$solutionName = "balsamicsolutions.readunreadsitecolumn.wsp"
$solutionFile = $currentDir + "\" + $solutionName

function WaitForJobToFinish([string]$solutionFileName)
{ 
	Start-Sleep -Seconds 2
    $jobName = "*solution-deployment*$solutionFileName*"
    $job = Get-SPTimerJob | ?{ $_.Name -like $jobName }
    if ($job -eq $null) 
    {
        Write-Host 'Timer job not found'
    }
    else
    {
        $jobFullName = $job.Name
        Write-Host -NoNewLine "Waiting to finish job $jobFullName"
        
        while ((Get-SPTimerJob $jobFullName) -ne $null) 
        {
            Write-Host -NoNewLine .
            Start-Sleep -Seconds 2
        }
        Write-Host  "Finished waiting for job..."
    }
}


Write-Host 'Uninstall-SPSolution'
Uninstall-SPSolution -identity $solutionName  -AllWebApplications -confirm:$false
WaitForJobToFinish -solutionFileName $solutionName 

Write-Host 'Remove-SPSolution'
Remove-SPSolution -identity $solutionName -confirm:$false
Start-Sleep -Seconds 2


 
Write-Host 'Updating local OWSTimer Assembly Registrations'
& $currentDir"\Utilities.exe"  '/REGISTER'

Write-Host 'Restarting FARM OWS Timer services'
$farm = Get-SPFarm
$timerServices = $farm.TimerService.Instances 
foreach ($timerSvc in $timerServices)
{
	$timerSvc.Stop(); 
	Start-Sleep -Seconds 1
	$timerSvc.Start();
	Write-Host $timerSvc.Server.Name "Restarted"
} 
Write-Host "Restarting local SPAdminV4 Service"
Restart-Service SPAdminV4


Write-Host "Add-SPSolution"
Add-SPSolution -LiteralPath $solutionFile

Write-Host "Install-SPSolution"
Install-SPSolution –identity $solutionName -GACDeployment -AllWebApplications -Force
WaitForJobToFinish -solutionFileName $solutionName
 
Write-Host "Restarting local SPTimerV4 Service"
Restart-Service SPTimerV4

Write-Host 'Re-applying RemarQ farm settings'
& $currentDir"\Utilities.exe"  '/RESET'

Remove-PsSnapin Microsoft.SharePoint.PowerShell
If ( $psISE) 
{
	Write-Host "Done"
}
Else
{
	Write-Host "Press any key to exit ..." 
	$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}