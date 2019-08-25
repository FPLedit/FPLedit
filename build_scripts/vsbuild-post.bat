@echo off
rem This will only be run on Windows

set SolutionDir=%1
set TargetDir=%2
set ConfigurationName=%3
set csi="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csi.exe"

if %ConfigurationName% == Release (
	%csi%  "%SolutionDir%build_scripts\build-release.csx" %TargetDir%
	%csi%  "%SolutionDir%build_scripts\build-source.csx" %SolutionDir% %TargetDir%
)
