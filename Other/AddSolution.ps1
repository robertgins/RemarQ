#First load the SharePoint commands
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

Write-Host "Add-SPSolution"
Add-SPSolution -LiteralPath $solutionFile

Write-Host 'Updating Local OWSTimer Assembly Registrations'
& $currentDir"\Utilities.exe"  '/REGISTER'

Write-Host "Install-SPSolution" 
Install-SPSolution –identity $solutionName -GACDeployment -AllWebApplications -Force
WaitForJobToFinish -solutionFileName $solutionName

Write-Host "Restarting local OWS Timer Service"
Restart-Service SPTimerV4


Write-Host "Restarting local SPAdminV4 Service"
Restart-Service SPAdminV4

Remove-PsSnapin Microsoft.SharePoint.PowerShell
If ( $psISE) 
{
	Write-Host "Done "
}
Else
{
	Write-Host "Press any key to exit ..." 
	$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}