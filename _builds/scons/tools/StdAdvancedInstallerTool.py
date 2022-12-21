"""StdAdvancedInstallerTool
##################################################################
* StdAdvancedInstallerTool.py - Wrapper functions que construyen
   los builders para proyectos estandar Advanced Installer
##################################################################
 Created by: Ernesto Castro (ercastro@siderca.com) Sep. '07
 Created by: Leandro Conde  (leandor@gmail.com)    Dic. '07
       $Id:  $
##################################################################
"""
import sys
import os
import SCons.Util
from SCons.Script.SConscript import SConsEnvironment # just do this once
from SCons.Script import *

def _StdAdvInsTool(env
                  , source
                  , target
                  , workdir             = '.'
                  , overrideFile        = 'OverrideDefault.execute'
                  , raiseerror          = False
                  , file_deps           = None
                  , alias_deps          = None
                  ):
    """
      _StdAdvInsTool: Produces all the builders necessary for an APRE standard advanced installer project.
    """
    
    if ('ADVInst' not in env['BUILDERS']) and not raiseerror:
      print 'Advanvce Installer is not available. Setups won\'t be generated.'
      return 0;

    wd = env.Dir(workdir)
    overrideFile = wd.File(overrideFile)
    outputFile = env.File(target)

    (root, ext) = os.path.splitext(source)

    overrideData = []

    overrideData.append(";aic\n")
    overrideData.append("SetVersion %d.%d.%d\n" % (env["svn_version_major"], env["svn_version_minor"], env["svn_revision"]))
    overrideData.append("SetPackageName \"%s\"\n" % outputFile)
    overrideData.append("build\n")

    output_fp = open(overrideFile.abspath, "w")
    try:
        output_fp.writelines(overrideData)
    finally:
        output_fp.close()

    source = [source, overrideFile]
    
    source += env.Nodify(file_deps, env.File)
    source += env.Nodify(alias_deps, env.Alias)

    create_setup = env.ADVInst(source = source, target = outputFile)

    Depends(Alias('release'), create_setup)

    Depends(create_setup, env.Dir('release/binaries'))
    Depends(create_setup, env.Dir('release/debug'))
    Depends(create_setup, env.Dir('release/tester'))

    return create_setup


def StdAdvInsTool(env
                  , source
                  , target
                  , raiseerror          = False
                  , workdir             = '.'
                  , overrideFile        = 'OverrideDefault.execute'
                  , file_deps           = None
                  , alias_deps          = None
                  ):

    return _StdAdvInsTool(env
                  , source = source
                  , target = target
                  , raiseerror          = raiseerror
                  , workdir             = workdir
                  , overrideFile        = overrideFile
                  , file_deps           = file_deps
                  , alias_deps          = alias_deps
                  )


def generate(env):
    """Attaches the function Wrappers to the specified environment."""

    SConsEnvironment.StdAdvInsTool = StdAdvInsTool

def exists(env):
    return 1
