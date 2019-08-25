@echo off
rem This will only be run on Windows

set SolutionDir=%1
set TargetDir=%2
set ConfigurationName=%3
set csi="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csi.exe"

%csi%  "%SolutionDir%build_scripts\build-pre.csx" %SolutionDir% %TargetDir%
%csi%  "%SolutionDir%build_scripts\build-sign.csx" %SolutionDir% %TargetDir%
