$SolutionDir		= $args[0]
$TargetDir			= $args[1]
$ConfigurationName	= $args[2]

if (Get-Command "csi" -ErrorAction SilentlyContinue) {
	$global:csi = "csi" # csi already on path
} elseif (Test-Path env:FPLEDIT_CSI_PATH) {
	$global:csi = $Env:FPLEDIT_CSI_PATH
} else {
	Write-Error "Set env variable FPLEDIT_CSI_PATH to path to csi.exe or add it to PATH!"
	exit 1
}

if ($ConfigurationName -eq "Release") {
	& $csi @("$($SolutionDir)build_scripts\build-release.csx", $TargetDir)
	& $csi @("$($SolutionDir)build_scripts\build-source.csx", $SolutionDir, $TargetDir)
}
