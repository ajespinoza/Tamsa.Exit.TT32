import os
import SCons
import win32con
import win32api
import SCons.Action
from SCons.Script.SConscript import SConsEnvironment # just do this once

def _getAdvInstallerPath():
  """
  Returns the path where Caphyon Advanced Installer is installed
  """
  try:
	advinst_root_key = win32api.RegOpenKeyEx( win32con.HKEY_LOCAL_MACHINE, 'SOFTWARE\\Caphyon\\Advanced Installer', 0, win32con.KEY_READ)
	found = False
	try:
	  try:
	    #Logica original, se agrego el RegQueryValueEx
        #name, obj, ntype = win32api.RegEnumValue(advinst_root_key, 0)
        #return os.path.join(os.environ['ProgramFiles'],"Caphyon",name)
		path = win32api.RegQueryValueEx(advinst_root_key, 'Advanced Installer Path')[0]
		return path
	  except:
		return ''
		win32api.RegCloseKey(advinst_root_key)
	finally:
	  win32api.RegCloseKey(advinst_root_key)
  except:
    return ''

def generate(env):
    """
    Add Builders and construction variables for AdvancedInstaller to an Environment.
    """
    if not exists(env):
        return 0;

    ADVInstBuilder = env.Builder(
                                action = SCons.Action.Action(
                                        '$ADVINSTALLER $ADVINSTFLAGS ${SOURCES[0].abspath} ${SOURCES[1].abspath}'
                                        , cmdstr = "$ADVINSTCOMSTR"
                                        )
                                , src_suffix = '.aip'
                                , target_suffix = '.msi'
                                )

    AdvInstallerPath = _getAdvInstallerPath()
    homedir = env.Dir(AdvInstallerPath)

    env['ADVINSTALLER']        = 'AdvancedInstaller.com'
    env['ADVINSTFLAGS']        = '/execute'
    env['ADVINSTCOMSTR']       = 'Compiling Adv. Installer project: $SOURCE'
    env['BUILDERS']['ADVInst'] = ADVInstBuilder

    env.PrependENVPath(
        'PATH',
        homedir.abspath
        )

def exists(env):
    AdvInstallerPath = _getAdvInstallerPath()
	#Return original, se agrego el bin\\x86\\...
    #return os.path.exists(os.path.join(AdvInstallerPath , 'AdvancedInstaller.com'))
    return os.path.exists(os.path.join(AdvInstallerPath , 'bin\\x86\\AdvancedInstaller.com'))
