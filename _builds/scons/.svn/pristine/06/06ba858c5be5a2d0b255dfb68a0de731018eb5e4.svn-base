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
import fnmatch
import SCons.Util
from SCons.Script.SConscript import SConsEnvironment # just do this once
from SCons.Script import *

def StdDelphiProject(
                    env
                  , project_name
                  , output_subdir
                  , workdir             = '.'
                  , typelib_name        = None
                  , source_ext          = '.dpr'
                  , output_ext          = '.exe'
                  , project_tag         = ''
                  , project_prefix      = ''
                  , publish_dir         = '$TLBDIR'
                  , publish             = False
                  , output_dir          = '$RELEASEDIR'
                  , debug_dir           = '$DEBUGDIR'
                  , idl_dir             = 'idl'
                  , compile_deps        = []
                  , typelib_deps        = []
                  , compile_alias_deps  = []
                  , typelib_alias_deps  = []
                  , builder_name        = 'DelphiProgram'
                  , src_packages        = []
                  , use_packages        = False
                  , version_stamp       = True
                  , version_template    = 'version.template.rc'
                  , module_stamp        = True
                  , module_file         = 'module.rc'
                  , module_data         = 'module.xml'
                  , release_file        = '${TMPDIR}/release.ini'
                  , unit_test           = False
                  , unit_test_log       = '${TMPDIR}/${NAME}.test.log'
                  , test_dir            = '$RELEASEDIR'
                  ):
    """
      StdDelphiProject: Produces all the builders necessary for an APRE standard delphi project.
          param:  builder_name -> The existing Builder to call, one of:
              'DelphiProgram',
              'DelphiLibrary',
              'DelphiActiveX' or
              'DelphiPackage'
    """

    wd = env.Dir(workdir)
    publish_dir = env.Dir(publish_dir)
    output_dir = env.Dir(output_dir)
    test_dir = env.Dir(test_dir)
    version_template = wd.File(version_template)
    module_file = wd.File(module_file)
    module_data = wd.File(module_data)
    release_file = env.File(release_file)

    local = env.Clone(
            NAME      = project_name
          , TAG       = project_tag
          , PREFIX    = project_prefix
          , SRCEXT    = source_ext
          , RELEXT    = output_ext
          , DBGDIR    = debug_dir
          , DPR       = '${NAME}'
          , OUTDIR    = output_dir
          , SUBDIR    = output_subdir
          , RELDIR    = '${OUTDIR}/${SUBDIR}'
          , RELDBG    = '${DBGDIR}/${SUBDIR}'
          , OUTPUT    = '${PREFIX}${NAME}${TAG}'
          , RELEASE   = '${RELDIR}/${OUTPUT}'
          , DEBUG     = '${RELDBG}/${OUTPUT}'
          , IDLDIR    = idl_dir
          , TLB       = typelib_name
          , TLB_PAS   = ('${TLB}_TLB' if typelib_name else None)
          , TLB_SRC   = '$IDLDIR/' + project_name
          , TESTDIR   = test_dir
          , TESTLOG   = unit_test_log

        )

    delete_config = None
    compile_typelib = None
    import_typelib = None
    compile_project = None
    prepare = Alias('prepare-delphi')
    compile  = Alias('compile')
    test = Alias('test')

    compile_deps = env.arg2nodes(Split(compile_deps), env.fs.File)
    typelib_deps = env.arg2nodes(Split(typelib_deps), env.fs.File)
    compile_deps += env.arg2nodes(Split(compile_alias_deps), env.Alias)
    typelib_deps += env.arg2nodes(Split(typelib_alias_deps), env.Alias)

    compile_deps += [Alias('depends')]

    delete_config = local.Command(
        target = '${DPR}.cfg'
      , source = '${DPR}${SRCEXT}'
      , action = Delete(
          '$TARGET'
        )
      )

    local.Command(
        target = '${DEBUG}.map'
      , source = '${RELEASE}.map'
      , action = Copy(
            '$TARGET'
          , '$SOURCE'
          )
      )

    local.Command(
        target = '${DEBUG}.rsm'
      , source = '${RELEASE}.rsm'
      , action = Copy(
            '$TARGET'
          , '$SOURCE'
          )
      )

    builder = local['BUILDERS'][builder_name]

    compile_project = builder(
        env = local
      , source = '${DPR}'
      , target = '${RELEASE}'
      , chdir  = wd
      )

    Depends(compile_project, [delete_config])

    idl_version_tmpl = local.File('$IDLDIR/version.template.h')
    
    if typelib_name and idl_version_tmpl.exists():

        version_h = local.VersionStamp(
            target = '$IDLDIR/version.h'
          , source = [
                release_file
              , idl_version_tmpl
              ]
          )

        typelib_deps += [version_h]

    if typelib_name:
        #MIDL to compile the NAME.idl and generate NAME.tlb
        compile_typelib = local.idl('${NAME}', '${TLB_SRC}')

        #Alias to ask for creation of NAME.tlb
        _alias = local.Alias('${NAME}.tlb', compile_typelib)

        #Compilation of .IDL must depend on typelib_deps
        Depends(compile_typelib, typelib_deps)
        Depends(compile_typelib, Alias('register-depends'))

        #And the virtual target prepare is used to compile the .tlb
        Depends(prepare, _alias)

        #And add the .tlb as dependence to compile the project
        compile_deps += [compile_typelib]

    if compile_typelib:
        #Call tlibimp to generate the xx_TLB.pas file
        import_typelib = local.DelphiTypelib('${TLB_PAS}.pas', '${NAME}')

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

        #Define an alias xx_TLB.pas for asking the generation of this file
        _alias = local.Alias('${TLB_PAS}.pas', import_typelib)

        #Generation of xx_TLB depends con compilation of their .tlb source
        Depends(import_typelib, compile_typelib)

        #And also prepare calls for creation of xx_TLB.pas
        Depends(prepare, _alias)

        #Compilation of project also depends on this file
        compile_deps += [import_typelib]

    if version_stamp and local.File(version_template).exists():
        version_source = local.VersionStamp(
            target = 'version.rc'
          , source = [
                release_file
              , version_template
              ]
          )

        version_resource = local.DelphiResource('version.res', 'version.rc')
        Depends(prepare, [version_source, version_resource])

        compile_deps += [version_resource]


    if module_stamp and local.File(module_file).exists():
        depends_resource = local.DelphiResource(
            'module.res'
          , module_file
          )

        compile_deps += [depends_resource]
        Depends(prepare, [depends_resource])


    # Dependencias entre los targets
    Depends(compile_project, compile_deps)

    if typelib_name and publish:
        # Copia la TLB generada para este projecto en el directorio _tlb del WC
        publish_typelib = local.Command(
            '$TLBDIR/${TLB_PAS}.pas'
          , '${TLB_PAS}.pas'
          , Copy(
                '${TARGET.dir}'
              , '$SOURCE'
              )
          )

        local.Alias('${TLB_PAS}', publish_typelib)
        Depends(publish_typelib, compile_project)


    # Alias para ser usados en dependencias
    _alias = local.Alias('${NAME}', compile_project)
    Depends(compile, _alias)

    if unit_test:
        test_action = env.Command(
                  target = '${TESTLOG}'
                , source = '${RELEASE}'
                , action = [
                      Delete('${TARGET}')
                    , Action('${SOURCE.file} > ${TARGET.abspath}')
                    ]
                , chdir = env.Dir('${TESTDIR}')
                , ENV = env['ENV']
                )

        Depends(test_action, compile_project)
        Depends(test, test_action)

    return compile_project


def StdDelphiProgram(env
                  , project_name
                  , workdir = '.'
                  , typelib_name = None
                  , source_ext = '.dpr'
                  , output_ext = '.exe'
                  , output_subdir = ''
                  , project_tag = ''
                  , project_prefix = ''
                  , publish_dir = '$TLBDIR'
                  , publish = False
                  , output_dir = '$RELEASEDIR'
                  , idl_dir = 'idl'
                  , compile_deps = []
                  , typelib_deps = []
                  , compile_alias_deps = []
                  , typelib_alias_deps = []
                  , src_packages = []
                  , use_packages = False
                  , version_stamp = True
                  , version_template = 'version.template.rc'
                  , module_stamp = True
                  , module_file = 'module.rc'
                  , module_data = 'module.xml'
                  , release_file = '${TMPDIR}/release.ini'):
    """
      StdDelphiProgram: Produces all the builders necessary for an APRE standard delphi application.
          param:  project_name -> This must be the file name part of the DPR
          param:  workdir  -> The directory to which is necessary to 'cd'
              before compilation starts. Normally this is the same dir as the
              Project.scons file. Defaults to '.', which is correct asuming that
              this are being called from the Project.scons in the same dir as the
              DPR.
          param:  typelib_name -> Is the name of the project typelibrary,
              i.e: the name which the sentence 'library xx' within the IDL file.
              If None, the is assumed that the project has no type library and
              no directives are generated for this.
          param:  source_ext -> The extension of the project file,
              normally is .DPR, but can also be '.DPK' for packages.
          param:  output_ext -> Extension attached to the compiler
              generated file.
          param:  output_subdir -> Subdirectory within output_dir (below)
              to which the output file is generated.
          param:  project_tag -> This must be specified only if the output
              file has to tagged like this: OutFileXXX.ext, where XXX is this tag.
          param:  publish_dir -> Directory where the generated
              <typelib_name>_TLB.pas is generated by means of the publish target.
          param:  publish -> Enables the publish action to generate the
              typelib .PAS source into publish_dir (above).
          param:  output_dir -> Directory where to place all the
              compiler generated output files into.
          param:  idl_dir -> Subdirectory, in relation to the workdir, from
              which to take the IDL source file.
          param:  compile_deps -> List which contains the extra nodes to be
              added as a dependence for compilation. These are normally files that
              the delphi compiler will need to be found in some directory outside
              our project workdir.
          param:  typelib_deps -> List which contains the extra nodes to be
              added as a dependence to the typelib compilation. These normally will
              be extra binaries which need to be places into a directory within the
              system PATH variable.
          param:  src_packages -> List of nodes for dependant packages that
              will be added to the command line using the -LI directive.
          param:  use_packages -> This enables using runtime packages,
              if this is specified the list of src_packages is used.
          param:  version_stamp -> Enables the version stamping of the
              compiled executable, by using the version info obtained from the
              SVN properties of the WC. This requires a 'version.template.rc'
              from which a new version.rc is generated. A new name can be specified
              by using the next argument below.
          param:  version_template -> This is the name
              of the template file which is used to generate the version.rc script.
          param:  module_stamp -> This enables the inclusion of the
              module.xml file as an embeded resource within the compiled binary.
              Is this is specified in True then two files must exists: module.rc and
              module.xml, but their names can be specified using the argument below.
          param:  module_file -> This allows to change the expected
              name of the module.rc file.
          param:  module_data -> Idem, but for the module.xml.
          param:  release_file -> This is the name of the
              release.ini file which is used as a repository for version info taken
              from the SVN properties.
    """
    return StdDelphiProject(
                    env = env
                  , project_name = project_name
                  , builder_name = 'DelphiProgram'
                  , workdir = workdir
                  , typelib_name = typelib_name
                  , source_ext = source_ext
                  , output_ext = output_ext
                  , output_subdir = output_subdir
                  , project_tag = project_tag
                  , project_prefix = project_prefix
                  , publish_dir = publish_dir
                  , publish = publish
                  , output_dir = output_dir
                  , idl_dir = idl_dir
                  , compile_deps = compile_deps
                  , typelib_deps = typelib_deps
                  , compile_alias_deps = compile_alias_deps
                  , typelib_alias_deps = typelib_alias_deps
                  , src_packages = src_packages
                  , use_packages = use_packages
                  , version_stamp = version_stamp
                  , version_template = version_template
                  , module_stamp = module_stamp
                  , module_file = module_file
                  , module_data = module_data
                  , release_file = release_file)


def StdDelphiActiveX(env
                  , project_name
                  , typelib_name
                  , workdir = '.'
                  , source_ext = '.dpr'
                  , output_ext = '.ocx'
                  , output_subdir = ''
                  , project_tag = ''
                  , project_prefix = ''
                  , publish_dir = '$TLBDIR'
                  , publish = True
                  , output_dir = '$RELEASEDIR'
                  , idl_dir = 'idl'
                  , compile_deps = []
                  , typelib_deps = []
                  , compile_alias_deps = []
                  , typelib_alias_deps = []
                  , src_packages = []
                  , use_packages = False
                  , version_stamp = True
                  , version_template = 'version.template.rc'
                  , module_stamp = True
                  , module_file = 'module.rc'
                  , module_data = 'module.xml'
                  , release_file = '$TMPDIR/release.ini'):
    """
      StdDelphiProgram: Produces all the builders necessary for an APRE standard COM library application.
          param:  project_name -> This must be the file name part of the DPR
          param:  workdir  -> The directory to which is necessary to 'cd'
              before compilation starts. Normally this is the same dir as the
              Project.scons file. Defaults to '.', which is correct asuming that
              this are being called from the Project.scons in the same dir as the
              DPR.
          param:  typelib_name -> Is the name of the project typelibrary,
              i.e: the name which the sentence 'library xx' within the IDL file.
              If None, the is assumed that the project has no type library and
              no directives are generated for this.
          param:  source_ext -> The extension of the project file,
              normally is .DPR, but can also be '.DPK' for packages.
          param:  output_ext -> Extension attached to the compiler
              generated file.
          param:  output_subdir -> Subdirectory within output_dir (below)
              to which the output file is generated.
          param:  project_tag -> This must be specified only if the output
              file has to tagged like this: OutFileXXX.ext, where XXX is this tag.
          param:  publish_dir -> Directory where the generated
              <typelib_name>_TLB.pas is generated by means of the publish target.
          param:  publish -> Enables the publish action to generate the
              typelib .PAS source into publish_dir (above).
          param:  output_dir -> Directory where to place all the
              compiler generated output files into.
          param:  idl_dir -> Subdirectory, in relation to the workdir, from
              which to take the IDL source file.
          param:  compile_deps -> List which contains the extra nodes to be
              added as a dependence for compilation. These are normally files that
              the delphi compiler will need to be found in some directory outside
              our project workdir.
          param:  typelib_deps -> List which contains the extra nodes to be
              added as a dependence to the typelib compilation. These normally will
              be extra binaries which need to be places into a directory within the
              system PATH variable.
          param:  src_packages -> List of nodes for dependant packages that
              will be added to the command line using the -LI directive.
          param:  use_packages -> This enables using runtime packages,
              if this is specified the list of src_packages is used.
          param:  version_stamp -> Enables the version stamping of the
              compiled executable, by using the version info obtained from the
              SVN properties of the WC. This requires a 'version.template.rc'
              from which a new version.rc is generated. A new name can be specified
              by using the next argument below.
          param:  version_template -> This is the name
              of the template file which is used to generate the version.rc script.
          param:  module_stamp -> This enables the inclusion of the
              module.xml file as an embeded resource within the compiled binary.
              Is this is specified in True then two files must exists: module.rc and
              module.xml, but their names can be specified using the argument below.
          param:  module_file -> This allows to change the expected
              name of the module.rc file.
          param:  module_data -> Idem, but for the module.xml.
          param:  release_file -> This is the name of the
              release.ini file which is used as a repository for version info taken
              from the SVN properties.
    """
    return StdDelphiProject(
                    env                 = env
                  , project_name        = project_name
                  , builder_name        = 'DelphiActiveX'
                  , workdir             = workdir
                  , typelib_name        = typelib_name
                  , source_ext          = source_ext
                  , output_ext          = output_ext
                  , output_subdir       = output_subdir
                  , project_tag         = project_tag
                  , project_prefix      = project_prefix
                  , publish_dir         = publish_dir
                  , publish             = publish
                  , output_dir          = output_dir
                  , idl_dir             = idl_dir
                  , compile_deps        = compile_deps
                  , typelib_deps        = typelib_deps
                  , compile_alias_deps  = compile_alias_deps
                  , typelib_alias_deps  = typelib_alias_deps
                  , src_packages        = src_packages
                  , use_packages        = use_packages
                  , version_stamp       = version_stamp
                  , version_template    = version_template
                  , module_stamp        = module_stamp
                  , module_file         = module_file
                  , module_data         = module_data
                  , release_file        = release_file
                  )

def StdDelphiPackage(
                    env
                  , project_name
                  , workdir             = '.'
                  , source_ext          = '.dpk'
                  , output_ext          = '.bpl'
                  , output_subdir       = ''
                  , project_tag         = ''
                  , project_prefix      = ''
                  , output_dir          = '$RELEASEDIR'
                  , idl_dir             = 'idl'
                  , compile_deps        = []
                  , typelib_deps        = []
                  , compile_alias_deps  = []
                  , typelib_alias_deps  = []
                  , src_packages        = []
                  , use_packages        = False
                  , version_stamp       = True
                  , version_template    = 'version.template.rc'
                  , module_stamp        = True
                  , module_file         = 'module.rc'
                  , module_data         = 'module.xml'
                  , release_file        = '$TMPDIR/release.ini'
                  ):

    return StdDelphiProject(
                    env                 = env
                  , project_name        = project_name
                  , builder_name        = 'DelphiPackage'
                  , workdir             = workdir
                  , typelib_name        = None
                  , source_ext          = source_ext
                  , output_ext          = output_ext
                  , output_subdir       = output_subdir
                  , project_tag         = project_tag
                  , project_prefix      = project_prefix
                  , publish_dir         = ''
                  , publish             = False
                  , output_dir          = output_dir
                  , idl_dir             = idl_dir
                  , compile_deps        = compile_deps
                  , typelib_deps        = typelib_deps
                  , compile_alias_deps  = compile_alias_deps
                  , typelib_alias_deps  = typelib_alias_deps
                  , src_packages        = src_packages
                  , use_packages        = use_packages
                  , version_stamp       = version_stamp
                  , version_template    = version_template
                  , module_stamp        = module_stamp
                  , module_file         = module_file
                  , module_data         = module_data
                  , release_file        = release_file
                  )

def StdDelphiLibrary(
                    env
                  , project_name
                  , typelib_name
                  , workdir             = '.'
                  , source_ext          = '.dpr'
                  , output_ext          = '.dll'
                  , output_subdir       = ''
                  , project_tag         = ''
                  , project_prefix      = ''
                  , publish_dir         = '$TLBDIR'
                  , publish             = True
                  , output_dir          = '$RELEASEDIR'
                  , idl_dir             = 'idl'
                  , compile_deps        = []
                  , typelib_deps        = []
                  , compile_alias_deps  = []
                  , typelib_alias_deps  = []
                  , src_packages        = []
                  , use_packages        = False
                  , version_stamp       = True
                  , version_template    = 'version.template.rc'
                  , module_stamp        = True
                  , module_file         = 'module.rc'
                  , module_data         = 'module.xml'
                  , release_file        = '$TMPDIR/release.ini'
                  ):

    return StdDelphiProject(
                    env                 = env
                  , project_name        = project_name
                  , builder_name        = 'DelphiLibrary'
                  , workdir             = workdir
                  , typelib_name        = typelib_name
                  , source_ext          = source_ext
                  , output_ext          = output_ext
                  , output_subdir       = output_subdir
                  , project_tag         = project_tag
                  , project_prefix      = project_prefix
                  , publish_dir         = publish_dir
                  , publish             = publish
                  , output_dir          = output_dir
                  , idl_dir             = idl_dir
                  , compile_deps        = compile_deps
                  , typelib_deps        = typelib_deps
                  , compile_alias_deps  = compile_alias_deps
                  , typelib_alias_deps  = typelib_alias_deps
                  , src_packages        = src_packages
                  , use_packages        = use_packages
                  , version_stamp       = version_stamp
                  , version_template    = version_template
                  , module_stamp        = module_stamp
                  , module_file         = module_file
                  , module_data         = module_data
                  , release_file        = release_file
                  )

def StdDelphiTest(
                    env
                  , project_name
                  , unit_test_log       = '${TMPDIR}/${NAME}.test.log'
                  , test_dir            = '${RELEASEDIR}'
                  , workdir             = '.'
                  , source_ext          = '.dpr'
                  , output_ext          = '.exe'
                  , output_subdir       = 'test'
                  , project_tag         = ''
                  , project_prefix      = ''
                  , publish_dir         = '$TLBDIR'
                  , publish             = False
                  , output_dir          = '$RELEASEDIR'
                  , idl_dir             = 'idl'
                  , compile_deps        = []
                  , typelib_deps        = []
                  , compile_alias_deps  = []
                  , typelib_alias_deps  = []
                  , src_packages        = []
                  , use_packages        = False
                  , version_stamp       = True
                  , version_template    = 'version.template.rc'
                  , module_stamp        = True
                  , module_file         = 'module.rc'
                  , module_data         = 'module.xml'
                  , release_file        = '$TMPDIR/release.ini'
                  , typelib_name        = None
                  ):

    return StdDelphiProject(
                    env                 = env
                  , project_name        = project_name
                  , workdir             = workdir
                  , test_dir            = test_dir
                  , typelib_name        = typelib_name
                  , source_ext          = source_ext
                  , output_ext          = output_ext
                  , output_subdir       = output_subdir
                  , project_tag         = project_tag
                  , project_prefix      = project_prefix
                  , publish_dir         = publish_dir
                  , publish             = publish
                  , output_dir          = output_dir
                  , idl_dir             = idl_dir
                  , compile_deps        = compile_deps
                  , typelib_deps        = typelib_deps
                  , compile_alias_deps  = compile_alias_deps
                  , typelib_alias_deps  = typelib_alias_deps
                  , src_packages        = src_packages
                  , use_packages        = use_packages
                  , version_stamp       = version_stamp
                  , version_template    = version_template
                  , module_stamp        = module_stamp
                  , module_file         = module_file
                  , module_data         = module_data
                  , release_file        = release_file
                  , builder_name        = 'DelphiProgram'
                  , unit_test_log       = unit_test_log
                  , unit_test           = True)


def generate(env):
    """Attaches the function Wrappers to the specified environment."""

    SConsEnvironment.StdDelphiProgram = StdDelphiProgram
    SConsEnvironment.StdDelphiLibrary = StdDelphiLibrary
    SConsEnvironment.StdDelphiActiveX = StdDelphiActiveX
    SConsEnvironment.StdDelphiPackage = StdDelphiPackage
    SConsEnvironment.StdDelphiProject = StdDelphiProject
    SConsEnvironment.StdDelphiTest    = StdDelphiTest


def exists(env):
    return 1
