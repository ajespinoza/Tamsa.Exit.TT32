import os
from SCons.Script import *
from SCons.Util import is_Sequence, Selector
from SCons.Variables import BoolVariable

PowershellAction = Action('${PSHELL_CMD}', '${PSHELL_STR}')

PowershellBuilder = Builder(
    action = PowershellAction
  , multi = 1
  , source_suffix = '.ps1'
  )

def generate(env):
    
    env["POWERSHELL"] = env.WhereIs('POWERSHELL.EXE')
    env["PSHELL_OPTS"] = '-NoLogo -NonInteractive -NoProfile'
    env["PSHELL_CMD"] = '${POWERSHELL} ${PSHELL_OPTS} -command "& { ${SOURCE.path} ${ARGS} >${OUTPUT.path} }"'
    env["PSHELL_STR"] = '= Running powershell script ${SOURCE.name} ='
        
    env.InstallBuilder('PowerShell', PowershellBuilder)

def exists(env):
    return env.WhereIs('POWERSHELL.EXE') 

