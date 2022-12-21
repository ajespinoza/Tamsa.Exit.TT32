@ECHO OFF
SET WORKDIR=%~dp0
PUSHD %WORKDIR% && (SVN update && CALL scons.bat prepare-dotnet && CALL scons.bat msvs MSVSVERSION=10.0) || PAUSE
POPD