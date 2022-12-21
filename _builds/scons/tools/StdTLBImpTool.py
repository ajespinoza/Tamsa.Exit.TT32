"""StdDelphiTool
##################################################################
* StdTLBImpTool.py - Wrapper functions que construyen los builders
    para proyectos estandar tlbimp
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

def _StdTLBImpTool(env
                  , source
                  , target
                  , output_subdir = 'interop'
                  , output_dir = '$RELDIR'
                  , references = None
                  , namespace = None
                  , key_file = None
                  , raiseerror = True
                  , workdir = '.'
                  ):
    """
      _StdTLBImpTool: Produces all the builders necessary for an APRE standard interop project.
    """

    if ('TLBImp' not in env['BUILDERS']) and not raiseerror:
        print 'TLBImp is not available (maybe .NET Framework SDK is missing?)'
        return 0;

    name, ext = os.path.splitext(target)
    wd = env.Dir(workdir)
    
    if key_file:
        key_file = wd.File(key_file)
        assert key_file.exists(), "If specified, the KEY_FILE (%s) must already exist." % (key_file.abspath)
    
    src = []
    src.append(source)
    if references:
        src.extend(references)

    tgt = os.path.join(os.path.join(output_dir, output_subdir), target)

    create_interop = env.TLBImp(tgt, src, namespace = namespace, key_file = key_file)

    Alias(target, create_interop)
    Alias(name, create_interop)
    Depends(Alias('prepare-dotnet'), create_interop)

    return create_interop


def StdTLBImpTool(env
                  , source
                  , target
                  , output_subdir = 'interop'
                  , output_dir = '$RELDIR'
                  , references = None
                  , namespace = None
                  , key_file = None
                  , raiseerror = True
                  , workdir = '.'
                  ):

    return _StdTLBImpTool(env
                  , source = source
                  , target = target
                  , output_subdir = output_subdir
                  , output_dir = output_dir
                  , references = references
                  , namespace = namespace
                  , key_file = key_file
                  , raiseerror = raiseerror
                  , workdir = workdir
                  )


def generate(env):
    """Attaches the function Wrappers to the specified environment."""

    SConsEnvironment.StdTLBImpTool = StdTLBImpTool

def exists(env):
    return 1
