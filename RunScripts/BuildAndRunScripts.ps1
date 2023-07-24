Param(
    # The Site Url
    $cmsUrl,
    # The Hapi Version
	$hapiVersion,
    # The CMS Core Version
	$cmsCoreVersion,
    # The CMS UI Version
	$cmsUIVersion,
    # The Find Version
	$findVersion,
    # The CloudPlatform Version
	$cloudVersion,
    # The working folder
    $WorkingFolder,
    # The source of postman scripts, cloned from github
    $SourcePath
)


if (!$SourcePath) {
    $SourcePath = "..\"
}

if (!$WorkingFolder) {
    $WorkingFolder = Get-Date -Format "MM-dd-HHmm"
}

if (!$cmsUrl) {
    $cmsUrl = Read-Host -Prompt 'Input cmsUrl'
} 

if (!$hapiVersion) {
    $hapiVersion = Read-Host -Prompt 'Input hapiVersion'
}

if (!$cmsCoreVersion) {
    $cmsCoreVersion = Read-Host -Prompt 'Input cmsCoreVersion'
}

if (!$cmsUIVersion) {
    $cmsUIVersion = Read-Host -Prompt 'Input cmsUIVersion'
}

if (!$cloudVersion) {
    $cloudVersion = Read-Host -Prompt 'Input cloudVersion'
}


$hapiReleaseVersion = $hapiVersion.Split("-")[0];


#Create ScriptFolder
if (Test-Path -Path $WorkingFolder) {
    $WorkingFolder = $WorkingFolder + (Get-Date -Format "-HHmm")
}
mkdir ".\$($WorkingFolder)"

#Copy Setup, CD, CDA, CMA, CMA Media, Configuration scripts to WorkingFolder
copy  "$($SourcePath)\AutomationSetup.postman_collection.json" ".\$($WorkingFolder)"
copy  "$($SourcePath)\CD\CD-NetCore.postman_collection.json" ".\$($WorkingFolder)"
copy  "$($SourcePath)\CDA\CDA-NetCore.postman_collection.json" ".\$($WorkingFolder)"
copy  "$($SourcePath)\CMA\CMA-NetCore.postman_collection.json" ".\$($WorkingFolder)"
copy  "$($SourcePath)\CMA\CMA-Media-NetCore.postman_collection.json" ".\$($WorkingFolder)"
copy  "$($SourcePath)\CD\HAPI-NetCore-Configuration-Checklist.postman_collection.json" ".\$($WorkingFolder)"

#Copy Enviroment, Data file to working WorkingFolder
copy  "$($SourcePath)\Data\TestData-NetCore.json" ".\$($WorkingFolder)"
copy  "$($SourcePath)\Environment\CMS-NetCore.postman_environment.json" ".\$($WorkingFolder)"

#Gen script file
$content = '
call newman run "AutomationSetup.postman_collection.json" --env-var "cmsUrl=[cmsUrl]" -e "CMS-NetCore.postman_environment.json" --export-environment "CMS-NetCore.postman_environment.json" 

@echo You need to run indexing schedule job first before run scripts.
@echo Step1 Login with admin/devLab08@
@echo Step2 Run [cmsUrl]/Automation/RunIndexingJob

:loop
set /p input="When you finish run indexing job, please type Y to continue (Y/N)?:"

IF %input% NEQ Y Goto :loop

@echo Started-CD: %date% %time%
call newman run "CD-NetCore.postman_collection.json" --env-var "authType=token" -e "CMS-NetCore.postman_environment.json" -r htmlextra --reporter-htmlextra-title "CD-NetCore [hapiReleaseVersion] DXP Test report" --reporter-htmlextra-titleSize 4 --reporter-htmlextra-cmsCoreVersion [cmsCoreVersion] --reporter-htmlextra-cmsUIVersion [cmsUIVersion] --reporter-htmlextra-hapiVersion [hapiVersion] --reporter-htmlextra-findVersion [findVersion] --reporter-htmlextra-cloudVersion [cloudVersion]
@echo Completed-CD: %date% %time%

@echo Started-CDA: %date% %time%
call newman run "CDA-NetCore.postman_collection.json" --env-var "authType=app" -e "CMS-NetCore.postman_environment.json" --working-dir "[workingDir]"  -r htmlextra --reporter-htmlextra-title "CDA-NetCore with app auth DXP [hapiReleaseVersion] Test report" --reporter-htmlextra-titleSize 3 --reporter-htmlextra-cmsCoreVersion [cmsCoreVersion] --reporter-htmlextra-cmsUIVersion [cmsUIVersion] --reporter-htmlextra-hapiVersion [hapiVersion] --reporter-htmlextra-findVersion [findVersion] --reporter-htmlextra-cloudVersion [cloudVersion]
@echo Completed-CDA: %date% %time%

@echo Started-CMA: %date% %time%
call newman run "CMA-NetCore.postman_collection.json" --env-var "authType=token" -e "CMS-NetCore.postman_environment.json" -d "TestData-NetCore.json" -r htmlextra --reporter-htmlextra-title "CMA [hapiReleaseVersion] DXP Test report" --reporter-htmlextra-titleSize 4 --reporter-htmlextra-cmsCoreVersion [cmsCoreVersion] --reporter-htmlextra-cmsUIVersion [cmsUIVersion] --reporter-htmlextra-hapiVersion [hapiVersion] --reporter-htmlextra-findVersion [findVersion] --reporter-htmlextra-cloudVersion [cloudVersion]
@echo Completed-CMA: %date% %time%

call increase-memory-limit
call set NODE_OPTIONS=--max_old_space_size=240000

@echo Started-CMA-Media: %date% %time%
call newman run "CMA-Media-NetCore.postman_collection.json" --env-var "authType=app" -e "CMS-NetCore.postman_environment.json" --working-dir "[workingDir]" -d "TestData-NetCore.json" -r htmlextra --reporter-htmlextra-title "CMA-Media-NetCore [hapiReleaseVersion] with app auth DXP Test report" --reporter-htmlextra-titleSize 3 --reporter-htmlextra-cmsCoreVersion [cmsCoreVersion] --reporter-htmlextra-cmsUIVersion [cmsUIVersion] --reporter-htmlextra-hapiVersion [hapiVersion] --reporter-htmlextra-findVersion [findVersion] --reporter-htmlextra-cloudVersion [cloudVersion]
@echo Completed-CMA-Media: %date% %time%

@echo Started-Configuration: %date% %time%
call newman run "HAPI-NetCore-Configuration-Checklist.postman_collection.json" "authType=token" -e "CMS-NetCore.postman_environment.json" -r htmlextra --reporter-htmlextra-title "Configuration-Checklist [hapiReleaseVersion] DXP Test report" --reporter-htmlextra-titleSize 4 --reporter-htmlextra-cmsCoreVersion [cmsCoreVersion] --reporter-htmlextra-cmsUIVersion [cmsUIVersion] --reporter-htmlextra-hapiVersion [hapiVersion] --reporter-htmlextra-findVersion [findVersion] --reporter-htmlextra-cloudVersion [cloudVersion]
@echo Complicated-Configuration: %date% %time%

pause
'

$content = $content.Replace("[cmsUrl]", $cmsUrl);
$content = $content.Replace("[hapiVersion]", $hapiVersion);
$content = $content.Replace("[hapiReleaseVersion]", $hapiReleaseVersion);
$content = $content.Replace("[cmsCoreVersion]", $cmsCoreVersion);
$content = $content.Replace("[cmsUIVersion]", $cmsUIVersion);
$content = $content.Replace("[findVersion]", $findVersion);
$content = $content.Replace("[cloudVersion]", $cloudVersion);
if ($SourcePath -eq "..\") {
 $content = $content.Replace("[workingDir]", "..\..\CMA");
}
else {
 $content = $content.Replace("[workingDir]", $SourcePath + "CMA");
}



$batPath = ".\$($WorkingFolder)\runScripts.bat"
New-Item $batPath
Set-Content $batPath $content

$commandPath = ".\$($WorkingFolder)\commands_manual.txt"
$content = $content.Replace("call ","")
New-Item $commandPath
Set-Content $commandPath $content

Set-Location $WorkingFolder

cmd.exe -/c "runScripts.bat"

pause