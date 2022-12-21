"""tlbimp
##################################################################
* tlbimp.py - tlbimp compiler for COM typelibs
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
import win32api
import win32con


#==========================================================================
# _getNETSDKPath()
#
# Devuelve el path donde esta instalado el SDK del Framework 2.0 de .NET
#==========================================================================
def _getNETSDKPath():
  """
  Returns the path where the .NET2 Framework SDK is installed
  """
  try:
    dotNETSDK_root_key = win32api.RegOpenKeyEx(win32con.HKEY_LOCAL_MACHINE, 'SOFTWARE\\Microsoft\\Microsoft SDKs\\.NETFramework\\v2.0', 0, win32con.KEY_READ)
    found = False
    i = 0
    try:
      try:
        while not found:
          name, obj, ntype = win32api.RegEnumValue(dotNETSDK_root_key, i)
          i = i + 1
          if name=='InstallationFolder':
            return obj
            found = True
      except:
        win32api.RegCloseKey(dotNETSDK_root_key)
        return ''
    finally:
      win32api.RegCloseKey(dotNETSDK_root_key)
  except:
    return ''

def TLBImpGenerator(
                      target
                    , source
                    , env
                    , for_program = 0
                    , for_signature = 0
                    ):
    """
    Action to build a Interop Assembly
    """

    src = source[0].children()

    assert len(src) >= 1, "[.NET] TLBIMP: At least one source is needed. Check your declarations."

    cmdline = env['TLBIMP']
    cmdline += ' '
    cmdline += env['TLBIMPFLAGS']
    if env.get('namespace') != None:
        cmdline += ' /namespace:$namespace'
    if env.get('key_file') != None:
        cmdline += ' /keyfile:$key_file'
    cmdline += ' /out:' + target[0].abspath
    for refnode in source[1:]:
        for ref in refnode.children():
            cmdline += ' /reference:' + ref.abspath
    cmdline += ' ' + src[0].abspath

    return [cmdline]


def generate(env):
    """
    Add Builders and construction variables for tlbimp to an Environment.
    """
    if not exists(env):
        return 0;

    TLBImpBuilder = env.Builder(
                                action = SCons.Action.Action(
                                        TLBImpGenerator
                                        , generator = 1
                                        #, cmdstr = "$TLBIMPCOMSTR"
                                        )
                                , src_suffix = '.dll'
                                , target_suffix = '.dll'
                                )

    dotNETSDK = _getNETSDKPath()
    homedir = env.Dir(dotNETSDK)
    bindir = homedir.Dir('bin')

    env['TLBIMP']             = 'tlbimp.exe'
    env['TLBIMPFLAGS']        = '/nologo /silent /strictref:nopia'
    env['TLBIMPCOMSTR']       = '[.NET] TLBIMP: Generating interop assembly for typelib in: $SOURCE to: $TARGET'
    env['BUILDERS']['TLBImp'] = TLBImpBuilder

    # Agrego al PATH el directorio del tlbimp
    env.PrependENVPath(
        'PATH',
        bindir.abspath
        )

def exists(env):
    dotNETSDK = _getNETSDKPath()
    bindir = os.path.join(dotNETSDK, 'bin')
    return os.path.exists(os.path.join(bindir, 'tlbimp.exe'))
