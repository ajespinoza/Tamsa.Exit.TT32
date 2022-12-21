"""msbuild
##################################################################
* msbuild.py - MSBuild builder for vsprojects
##################################################################
 Created by: Ernesto Castro (ercastro@siderca.com) Apr. '08
       $Id: delphi.py 7217 2007-10-23 19:51:29Z t61248 $
##################################################################
"""

import os
import ListVar
import SCons.Action
import SCons.Builder
import SCons.Util

#==========================================================================
# _getMSBuildPath()
#
# Devuelve el path donde esta instalado el SDK del Framework 2.0 de .NET
#==========================================================================
def _getMSBuildPath(env, msvsversion=None):
  """
  Returns the path where the .NET2 Framework SDK is installed
  """

  VERSIONMAP = {
      '8.0': '2.0',
      '9.0': 3.5 , 
      '10.0': 4.0,
      '11.0': 4.0,
	  '12.0': 4.0,
	  '14.0': 4.0,
	  '15.0': 4.0,
	  '16.0': 4.0
    }

  if not msvsversion: msvsversion = env.get('MSVSVERSION', '10.0')
  KEYPATH1 = r'SOFTWARE\Microsoft\VisualStudio\%s\MSBuild\MSBuildBinPath' % msvsversion
  KEYPATH2 = r'SOFTWARE\Microsoft\MSBuild\ToolsVersions\%s\MSBuildToolsPath' % VERSIONMAP[msvsversion]
  print "KEY1 (%s) - KEY2 (%s)\n" % (KEYPATH1, KEYPATH2)
  return env.RegGetValue(KEYPATH1) or env.RegGetValue(KEYPATH2) or ""

def _runide(target, source, env):
    import subprocess
    args = []
    args.append(source[0].abspath)
    args.extend(map(lambda x: x.abspath, source[1:]))
    subprocess.Popen(args, env=env['ENV'])

def generate(env):
    """
    Add Builders and construction variables for MSBuild to an Environment.
    """
    if not exists(env):
        return 0

    msvsversion = env.get('MSVSVERSION', '9.0')
    
    MSBuildBuilder = env.Builder(
                                action = SCons.Action.Action(
                                        '$MSBUILD $MSBUILDFLAGS ${SOURCES[0].abspath}' 
                                        , cmdstr = "$MSBUILDCOMSTR"
                                        )
                                , src_suffix = '.csproj'
                                , target_suffix = '.dll'
                                )

    MSBuildPath = _getMSBuildPath(env, msvsversion)
    homedir = env.Dir(MSBuildPath)

    env['MSBUILD']             = 'MSBuild.exe'
    """
     /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=../../../_builds/scons/tools/Tenaris.snk /p:Platform=x86 '
     """
    env['MSBUILDFLAGS']        = '/nologo /v:m'
    env['MSBUILDCOMSTR']       = 'COMPILING VS ${MSVSVERSION} PROJECT: $SOURCE'
    env['BUILDERS']['MSBuild'] = MSBuildBuilder

    # Agrego al PATH el directorio del MSBuild
    env.PrependENVPath(
        'PATH',
        homedir.abspath
        )

    RootDir = env.Dir('#')
    IDEDir = env.RegGetValue(r'SOFTWARE\Microsoft\VisualStudio\%s\InstallDir' % msvsversion)
#    print "IDEDir [%s]\n" % (IDEDir)
    if IDEDir:
        IDEDir = env.Dir(IDEDir)
        
        runide = env.Command(
            target  = None
          , source  = [IDEDir.File('DEVENV.EXE'), '$SOLUTION']
          , action  = _runide
          , chdir   = RootDir
          )
        ide = env.Alias('msvs', runide)
        env.Depends(ide, env.Alias('prepare-dotnet'))

def options(vars):
    vars.Add('SOLUTION', 'Allows to override the default \'all.sln\' used when launching the Visual Studio IDE.', '#all.sln')
    vars.Add('MSVSVERSION', 'Specifies the IDE msvsversion to use.', '10.0')

def exists(env):
    MSBuildPath = _getMSBuildPath(env)
    if MSBuildPath: 
      return os.path.exists(os.path.join(MSBuildPath , 'MSBuild.exe'))
    return False
