"""StdDelphiTool
##################################################################
* StdDelphiTool.py - Wrapper functions que construyen los builders
    para proyectos estandar delphi
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

def _StdTypelibProject(
                    env
                  , project_name
                  , typelib_name
                  , output_ext
                  , output_subdir
                  , publish_dir
                  , output_dir
                  , idl_dir
                  , typelib_deps
                  , typelib_alias_deps
                  , release_file        = '${TMPDIR}/release.ini'
                  , workdir             = '.'
                  ):
    """
      _StdTypelibProject: Produces all the builders necessary for an APRE standard typelibrary project.
    """

    wd = env.Dir(workdir)
    publish_dir = env.Dir(publish_dir)
    output_dir = env.Dir(output_dir)

    local = env.Clone(
            NAME      = project_name
          , RELEXT    = output_ext
          , OUTDIR    = output_dir
          , SUBDIR    = output_subdir
          , RELDIR    = '${OUTDIR}/${SUBDIR}'
          , OUTPUT    = '${NAME}${TAG}'
          , RELEASE   = '${RELDIR}/${OUTPUT}'
          , IDLDIR    = idl_dir
          , TLB       = typelib_name or project_name
          , TLB_PAS   = '${TLB}_TLB'
          , TLB_SRC   = '$IDLDIR/' + project_name
          , PUBLISH   = publish_dir
          , TLB_DEST  = '${PUBLISH}/${TLB_PAS}'
        )

    compile_typelib = None
    import_typelib = None

    prepare = Alias('prepare-delphi')
    compile  = Alias('compile')

    typelib_deps = env.arg2nodes(Split(typelib_deps), env.fs.File)
    typelib_deps += env.arg2nodes(Split(typelib_alias_deps), env.Alias)

    idl_version_tmpl = local.File('$IDLDIR/version.template.h')
    
    if idl_version_tmpl.exists():

        version_h = local.VersionStamp(
            target = '$IDLDIR/version.h'
          , source = [
                release_file
              , idl_version_tmpl
              ]
          )

        typelib_deps += [version_h]

    #MIDL to compile the NAME.idl and generate NAME.tlb
    compile_typelib = local.idl('${RELEASE}', '${TLB_SRC}')

    #Alias to ask for creation of NAME.tlb
    _alias = local.Alias('${NAME}.tlb', compile_typelib)

    #Compilation of .IDL must depend on typelib_deps
    Depends(compile_typelib, typelib_deps)
    Depends(compile_typelib, Alias('register-depends'))

    #And the virtual target prepare is used to compile the .tlb
    Depends(prepare, _alias)

    #Call tlibimp to generate the xx_TLB.pas file
    import_typelib = local.DelphiTypelib('${TLB_PAS}.pas', '${RELEASE}')
    release_typelib = local.DelphiTypelib('${TLB_DEST}.pas', '${RELEASE}')

    def DeleteTLBs(target, source, env):
        for p in source:
            try:
                os.remove(p.abspath)
                print "-- Delete spureous TLB file in project directory: file = %s " % str(p)
            except:
                print "Failed to delete this spureous TLB file in project directory: file = %s " % str(p)

    tlbname = local.subst('${TLB_PAS}')
    delete_typelibs = local.Command(
        target = '-(not-exists)-'
      , source = local.GlobFiles(basedir=wd, includes = [r'.*_TLB\..*'], excludes = [r'.*' + tlbname + r'\..*'])
      , action = Action(DeleteTLBs)
      )
      
    Alias('deletetlbs', delete_typelibs)

    Depends(import_typelib, [delete_typelibs])
    Depends(release_typelib, [delete_typelibs])

    #Define an alias xx_TLB.pas for asking the generation of this file
    _alias = local.Alias('${TLB_PAS}.pas', import_typelib)

    #Generation of xx_TLB depends con compilation of their .tlb source
    Depends(import_typelib, compile_typelib)

    #And also prepare calls for creation of xx_TLB.pas
    Depends(prepare, _alias)

    local.Alias('${TLB_PAS}', release_typelib)

    # Alias para ser usados en dependencias
    _alias = local.Alias('${NAME}', compile_typelib)
    Depends(compile, import_typelib)
    Depends(compile, release_typelib)

    return compile_typelib


def StdTypelib(env
                  , project_name
                  , typelib_name = None
                  , output_ext = '.tlb'
                  , output_subdir = 'tlb'
                  , publish_dir = '$TLBDIR'
                  , output_dir = '$RELDIR'
                  , idl_dir = 'idl'
                  , typelib_deps = []
                  , typelib_alias_deps = []
                  , release_file        = '${TMPDIR}/release.ini'
                  , workdir             = '.'
                  ):
    """
      StdDelphiProgram: Produces all the builders necessary for an APRE standard delphi application.
          param:  project_name -> This must be the file name part of the DPR
          param:  typelib_name -> Is the name of the project typelibrary,
              i.e: the name which the sentence 'library xx' within the IDL file.
              If None, the is assumed that the project has no type library and
              no directives are generated for this.
          param:  output_ext -> Extension attached to the compiler
              generated file.
          param:  output_subdir -> Subdirectory within output_dir (below)
              to which the output file is generated.
          param:  publish_dir -> Directory where the generated
              <typelib_name>_TLB.pas is generated by means of the publish target.
          param:  output_dir -> Directory where to place all the
              compiler generated output files into.
          param:  idl_dir -> Subdirectory, in relation to the workdir, from
              which to take the IDL source file.
          param:  typelib_deps -> List which contains the extra nodes to be
              added as a dependence to the typelib compilation. These normally will
              be extra binaries which need to be places into a directory within the
              system PATH variable.
    """
    return _StdTypelibProject(
                    env = env
                  , project_name = project_name
                  , typelib_name = (typelib_name if typelib_name else project_name)
                  , output_ext = output_ext
                  , output_subdir = output_subdir
                  , publish_dir = publish_dir
                  , output_dir = output_dir
                  , idl_dir = idl_dir
                  , typelib_deps = typelib_deps
                  , typelib_alias_deps = typelib_alias_deps
                  , release_file = release_file
                  , workdir = workdir
                  )


def generate(env):
    """Attaches the function Wrappers to the specified environment."""

    SConsEnvironment.StdTypelibProject = StdTypelib

def exists(env):
    return 1
