"""
##################################################################
* delphi.py - Builders SCONS para proyectos delphi.
##################################################################
DelphiProgram:

    DelphiPackage(target_file_path, source_file_path)
    DelphiProgram(target_file_path, source_file_path)
    DelphiLibrary(target_file_path, source_file_path)
    DelphiActiveX(target_file_path, source_file_path)

    Tanto el source como el target son ARCHIVOS! no directorios.

    Variables de entorno asociadas:
        'DCC32_INCLUDE'     = Path de busqueda de archivos include
        'DCC32_UNIT'        = Path de busqueda de units
        'DCC32_RES'         = Path de busqueda de archivos de resources
        'DCC32_DCU'         = Directorio de salida de los dcu's generados
        'DCC32_SWITCHES'    = Switches para el compilador. En este string los switch se deben escribir con e '$' escapado: '-$$x+'
        'DCC32_OPTS'        = Opciones para el compilador (no incluye los switches)

------------------------------------------------------------------
DelphiTypelib:
   DelphiTypelib(target_file_path, source_file_path)

   Tanto el source como el target son ARCHIVOS! no directorios.

------------------------------------------------------------------
DelphiResource:
   DelphiResource(target_file_path, source_file_path)

##################################################################
 Created by: Ernesto Castro (ercastro@siderca.com) Sep. '07
       $Id: delphi.py 24475 2009-09-08 19:14:32Z t61248 $
##################################################################
"""

import os
import glob
import string
import SCons
import win32api
import win32con
import ConfigParser
import SCons.Builder
import SCons.Action
from SCons.Util import is_List, is_Sequence
from SCons.Script.SConscript import SConsEnvironment # just do this once
from subprocess import Popen
##import utils


class VERSIONS:
    DELPHI7 = r'Borland\Delphi\7.0'
    BDS5 = r'Borland\BDS\5.0'
    BDS6 = r'CodeGear\BDS\6.0'
    DELPHI = DELPHI7
    D70 = DELPHI7
    BDS = BDS5
    DELPHI2009 = BDS6
    DELPHI2007 = BDS5
    D2009 = BDS6
    D2007 = BDS5

class IDES:
    DELPHI = r'BIN\DELPHI32.EXE'
    BDS = r'BIN\BDS.EXE'
    DELPHI7 = DELPHI
    D70 = DELPHI
    BDS6 = BDS
    BDS5 = BDS
    DELPHI2009 = BDS
    DELPHI2007 = BDS
    D2009 = BDS
    D2007 = BDS
    
class BPGSUFFIX:
    DELPHI = r'.bpg'
    BDS = r'.groupproj'
    DELPHI7 = DELPHI
    D70 = DELPHI
    BDS6 = BDS
    BDS5 = BDS
    DELPHI2009 = BDS
    DELPHI2007 = BDS
    D2009 = BDS
    D2007 = BDS

def _ReadDelphiOptions(env, dof):
    ini = ConfigParser.ConfigParser()
    ini.read(dof)

    UsePackages = ini.get('Directories', 'UsePackages')
    if UsePackages:
        Packages = map(env.Alias, ini.get('Directories', 'Packages').split(';'))
    else:
        Packages = []

    return (UsePackages, Packages)


#==========================================================================
# _getDelphiPath( version)
#
# Devuelve el path donde esta instalado Delphi para la version especifica solicitada
#==========================================================================
def _getDelphiPath(env, version = None):
  """
  Returns the path where Delphi is installed
  """  
  if not version: version = r'DELPHI7'  
  if not '\\' in version: version = VERSIONS.__dict__.get(version, VERSIONS.DELPHI7)
  KEYPATH = r'SOFTWARE\%s\RootDir' % version  
  return env.RegGetValue(KEYPATH) or ''
 

#==========================================================================
# Funciones asociadas al builder para '.dpr'
#
# DelphiProgramEmitter(target, source, env)
#
#   Emitter, calcula los fuentes y targets a partir del dpr origen y
#         el target.
#
# DelphiCompileProjectAction(target, source, env)
#
#   Action, funcion que invoca al compilador con todos los parametros
#         necesarios.
#==========================================================================
def DelphiProgramEmitter(target, source, env):
    """
    Emitter para el dcc32, para que elimine tanto el ejecutable como el rsm y el map
    """

    name = str(target[0])
    base, ext = SCons.Util.splitext(name)

    targets = [
        base + '.exe',
        base + '.rsm',
        base + '.map'
        ]
        
    return (targets, source)

def DelphiLibraryEmitter(target, source, env):
    """
    Emitter para el dcc32, para que elimine tanto el ejecutable como el rsm y el map
    """

    name = str(target[0])
    base, ext = SCons.Util.splitext(name)

    targets = [
        base + '.dll',
        base + '.rsm',
        base + '.map'
        ]

    return (targets, source)

def DelphiLocaleEmitter(target, source, env):
    target = target[0]
    source = source[0]
    
    locale = source.dir.name
    name = target.name
    dir = target.dir
    
    name, suffix = SCons.Util.splitext(name)

    targets = [dir.File(name + '.' + locale)]
    return (targets, source)

def DelphiActiveXEmitter(target, source, env):
    """
    Emitter para el dcc32, para que elimine tanto el ejecutable como el rsm y el map
    """

    name = str(target[0])
    base, ext = SCons.Util.splitext(name)

    targets = [
        base + '.ocx',
        base + '.rsm',
        base + '.map'
        ]

    return (targets, source)

def DelphiPackageEmitter(target, source, env):
    """
    Emitter para el dcc32, para que elimine tanto el ejecutable como el rsm y el map
    """

    tname = str(target[0])
    sname = str(source[0])
    tbase, text = SCons.Util.splitext(tname)
    sbase, sext = SCons.Util.splitext(sname)

    dcpdir = env.Dir('$DCPDIR')
    dcudir = env.Dir('$DCUDIR')
    targets = [
        tbase + '.bpl'
        , tbase + '.rsm'
        , tbase + '.map'
        , dcpdir.File(sbase + '.dcp')
        , dcudir.File(sbase + '.dcu')
        ]
        
    return (targets, source)

def DelphiCommandLineGenerator(source, target, env, for_program, for_signature = 0, for_locale = 0):
    """
    Action to build a Delphi program, it change to the dir were the source file
    is before running the compiler, prior to this, we remember the path to the target dir
    """

    use_packages = 0
    packages = []
    source_packages = []

    if for_program:
        use_packages = env.get('DCC32_USEPACKAGES')
        packages = env.get('DCC32_PACKAGES', [])

    if is_Sequence(source):
        project = source[0]
        if for_program and len(source) > 1:
            use_packages = 1
            source_packages = source[1:]
    else:
        project = source

    if is_Sequence(target):
        target = target[0]

    outdir = target.dir

    base, ext = SCons.Util.splitext(project.name)

    dcpdir = env.Dir(env.get('DCC32_DCP'))
    dcudir = env.Dir(env.get('DCC32_DCU'))

    unit_dirs = env.get('DCC32_UNIT')
    inc_dirs = env.get('DCC32_INCLUDE')
    res_dirs = env.get('DCC32_RES')
    
    if not for_locale:        
        switches = str(env.get('DCC32_SWITCHES')) 
    else:        
        switches = '-H -W -Z -M -Q'
    
    options = env.get('DCC32_OPTS')
    defines = env.get('DCC32_DEFINES', [])
    if not is_List(defines): defines = [defines]

    unit_dirs = unit_dirs and map(lambda x: '-U' + str(x), unit_dirs)
    inc_dirs = inc_dirs and map(lambda x: '-I' + str(x), inc_dirs)
    res_dirs = res_dirs and map(lambda x: '-R' + str(x), res_dirs)

    if use_packages:
        packages.extend(source_packages)
        packages = map(lambda x: '-LU' + str(x), packages)

    cmdline = ['$DCC32_BIN']
    cmdline += ['$DCC32_SWITCHES']
    cmdline += ['$DCC32_OPTS']
    if for_program:
        if outdir: cmdline += ['-E' + outdir.abspath]
    else:
        if outdir: cmdline += ['-LE' + outdir.abspath]
        if dcpdir: cmdline += ['-LN' + dcpdir.abspath]
    if dcudir: cmdline += ['-N' + dcudir.abspath]
    
    if defines: cmdline += ['-D' + ';'.join(defines)]
    
    if use_packages:
        if packages: cmdline += packages
        
    if unit_dirs: cmdline += unit_dirs
    if inc_dirs: cmdline += inc_dirs
    if res_dirs: cmdline += res_dirs
    cmdline += [project.name]

    return [cmdline]

def DelphiPackageCompileGenerator(source, target, env, for_signature = 0):
    """
    Generator that constructs the command line to build Delphi package
    """
    return DelphiCommandLineGenerator(source, target, env, for_program=0, for_signature=for_signature)

def DelphiProgramCompileGenerator(source, target, env, for_signature = 0):
    """
    Generator that constructs the command line to build Delphi program
    """
    return DelphiCommandLineGenerator(source, target, env, for_program=1, for_signature=for_signature)

def DelphiLocaleCompileGenerator(source, target, env, for_signature = 0):
    """
    Generator that constructs the command line to build Delphi program
    """
    return DelphiCommandLineGenerator(source, target, env, for_program=1, for_signature=for_signature, for_locale=1)

#==========================================================================
# Funciones asociadas al builder para '.tlb'
#
# DelphiTypelibEmitter(target, source, env)
#
#   Emitter, calcula los fuentes y targets a partir del dpr origen y
#         el target.
#
# DelphiTypelibAction(target, source, env)
#
#   Action, funcion que invoca al compilador con todos los parametros
#         necesarios.
#==========================================================================
def DelphiTypelibEmitter(target, source, env):
    """
    Emitter para el tlibimp, para que elimine tanto el .pas que genera como el .dcr
    """
    
    node = target[0]
    outdir = env.Dir(node.dir)
    
    base, ext = SCons.Util.splitext(node.name)
    
    targets = [
        outdir.File(base + '.pas')
      , outdir.File(base + '.dcr')
      ]
    return (targets, source)

def DelphiTypelibGenerator(target, source, env):
    pass

def DelphiTypelibAction(target, source, env):
  """
  Action to generate _TLB.pas
  """

  for src in source:

     TLIB_COM = '$TLIB_BIN $TLIB_FLAGS -D' + str(target[0].dir) + ' ' + os.path.abspath(str(src))
     TLIB_MSG = '**** [TLIBIMP] Generando units ' + \
        '\n\tSOURCE: ' + str(src) + \
        '\n\tTARGET: ' + str(target[0]) + \
        '\n\tFLAGS: $TLIB_FLAGS'
        
     env.Execute(SCons.Action.Action(TLIB_COM, TLIB_MSG))

def runWrapper(env, target, command, *args, **kwargs):
    def runide(target, source, env):        
        args = []
        args.append(source[0].abspath)
        args.append('-ns')
        args.extend(map(lambda x: x.abspath, source[1:]))
        Popen(args, env=env['ENV'])
    
    ####
    RootDir = env.Dir('#') 
    runide = env.Command(
        target  = None
      , source  = (command,) + args
      , action  = runide
      , chdir   = RootDir
      , **kwargs
      )
      
    ####
    return env.Alias(target, runide)

def _DumpDelphiPath(target, source, env):
    search_path = env['DCC32_UNIT']
    
    search_path = map(lambda p: env.Dir(p).path, search_path)
    search_path = filter(lambda p: p[0] != '"', search_path)
    
    def transform_vars(p):
        p = p.replace('source\\_ext\\_lib', '$(LIBDIR)')
        p = p.replace('source\\_ext\\_tlb', '$(TLBDIR)')
        p = p.replace('release\\binaries', '$(BINDIR)')
        p = p.replace('_temp\\_bin', '$(DEPDIR)')
        p = p.replace('source\\library', '$(PROJLIB)')
        p = p.replace('source\\common', '$(COMMONSRC)')
        return p

    def transform_rel(p):
        p = p.replace('source\\_ext\\_lib', '..\\..\\_ext\\_lib')
        p = p.replace('source\\_ext\\_tlb', '..\\..\\_ext\\_tlb')
        p = p.replace('release\\binaries', '..\\..\\..\\release\\binaries')
        p = p.replace('_temp\\_bin', '..\\..\\..\\_temp\\_bin')
        p = p.replace('source\\library', '..\\..\\library')
        return p

    search_path1 = map(transform_vars, search_path)
    search_path2 = map(transform_rel, search_path)
    
    print "DELPHI PATH: -----------------------------------------"
    for p in search_path:
        print "  --> [%s]" % p
    print "------------------------------------------------------"
    
    print "DELPHI PATH FOR PROJECT OPTIONS: ---------------------"
    
    f = env.File('#Delphi-search.path.txt')
    fp = file(f.abspath, "w")
    for p in search_path:
        fp.write(p)
        fp.write('\n')
    fp.write('\n\n')
    fp.write(';'.join(search_path1))
    fp.write('\n\n')
    fp.write(';'.join(search_path2))
    fp.write('\n')
    fp.close()

    print "Dumped to file: %s" % f.path
    print "------------------------------------------------------"
    
    Popen(['notepad', f.path], cwd = env.Dir('#').abspath)

def generate(env):
    """
    Add Builders and construction variables for delphi to an Environment.
    """

    version = env.get('DELPHI_VERSION', None)    
    delphi = env.Dir(_getDelphiPath(env, version))
    env['DELPHI'] = delphi
    env['DELPHI_IDE'] = delphi.File(IDES.__dict__.get(version, IDES.DELPHI))
    env['DELPHI_BPGSUFFIX'] = BPGSUFFIX.__dict__.get(version, BPGSUFFIX.DELPHI)    

    # Builder para los DPRs ...
    env['DCC32_BIN']        = delphi.File('BIN/DCC32.EXE')
    env['DCC32_DPR_MSG']    = ' * [DCC32] Compiling project: [$SOURCE] to: [$TARGET] using options: "$DCC32_OPTS" and defines: "$DCC32_DEFINES"'
    env['DCC32_DPK_MSG']    = ' * [DCC32] Compiling package: [$SOURCE] to: [$TARGET] using options: "$DCC32_OPTS" and defines: "$DCC32_DEFINES"'
    env['DCC32_INCLUDE']    = []
    env['DCC32_DCU']        = ''
    env['DCC32_DCP']        = ''
    env['DCC32_BPL']        = ''
    env['DCC32_RES']        = ['"${DELPHI.abspath}/Lib"']
    env['DCC32_UNIT']       = ['"${DELPHI.abspath}/Lib"']
    env['DCC32_SWITCHES']   = ''
    env['DCC32_OPTS']       = ''
    env['DCC32_USEPACKAGES']= 0
    env['DCC32_PACKAGES']   = []
    env['DCC32_DEFINES']    = []

    DelphiProgram = env.Builder(
                                action = SCons.Action.Action(
                                        DelphiProgramCompileGenerator
                                        , generator = 1
                                        , cmdstr = "$DCC32_DPR_MSG"
                                        )
                                , emitter = DelphiProgramEmitter
                                , src_suffix = '.dpr'
                                , target_suffix = '.exe'
                                )

    env.InstallBuilder('DelphiProgram', DelphiProgram, ['DPR', 'DelphiProg'])
    
    # Builder para los DPRs ...
    DelphiLibrary = env.Builder(
                                action = SCons.Action.Action(
                                        DelphiProgramCompileGenerator
                                        , generator = 1
                                        , cmdstr = "$DCC32_DPR_MSG"
                                        )
                                , emitter = DelphiLibraryEmitter
                                , src_suffix = '.dpr'
                                , target_suffix = '.dll'
                                )

    env.InstallBuilder('DelphiLibrary', DelphiLibrary, ['DelphiDLL', 'DelphiLib'])

    DelphiLocale = env.Builder(
                                action = SCons.Action.Action(
                                        DelphiLocaleCompileGenerator
                                        , generator = 1
                                        , cmdstr = "$DCC32_DPR_MSG"
                                        )
                                , emitter = DelphiLocaleEmitter
                                , src_suffix = '.dpr'
                                )

    env.InstallBuilder('DelphiLocale', DelphiLocale)

    # Builder para los DPRs ...
    DelphiActiveX = env.Builder(
                                action = SCons.Action.Action(
                                        DelphiProgramCompileGenerator
                                        , generator = 1
                                        , cmdstr = "$DCC32_DPR_MSG"
                                        )
                                , emitter = DelphiActiveXEmitter
                                , src_suffix = '.dpr'
                                , target_suffix = '.ocx'
                                )

    env.InstallBuilder('DelphiActiveX', DelphiActiveX, ['DelphiControl'])
    
    # Builder para los DPRs ...
    DelphiPackage = env.Builder(
                                action = SCons.Action.Action(
                                        DelphiPackageCompileGenerator
                                        , generator = 1
                                        , cmdstr = "$DCC32_DPK_MSG"
                                        )
                                , emitter = DelphiPackageEmitter
                                , src_suffix = '.dpk'
                                , target_suffix = '.bpl'
                                )

    env.InstallBuilder('DelphiPackage', DelphiPackage, ['DPK', 'BPL'])

    # Builder para la tlb...
    env['TLIB_BIN']         = env.Dir('$DELPHI').File('BIN/TLIBIMP.EXE')
    env['TLIB_FLAGS']       = '-P+ -Ha- -Hr- -Hs- -Pt+ -Cw+ -R-'
    env['TLIB_COM']         = '$TLIB_BIN $TLIB_FLAGS -D${TARGET.dir.abspath} ${SOURCE.abspath}'
    env['TLIB_MSG']         = '**** [TLIBIMP] Generating Delphi source for: [${SOURCE}] to: [${TARGET}]'
    

    DelphiTypelib = env.Builder(
                action = SCons.Action.Action('$TLIB_COM', '$TLIB_MSG')
            ,   single_source = 1
            ,   emitter = DelphiTypelibEmitter
            ,   target_suffix = '.pas'
            ,   src_suffix = ['.tlb', '.dll', '.ocx', '.exe']
            )

    env.InstallBuilder('DelphiTypelib', DelphiTypelib, ['DelphiTLIB', 'TLB'])

    # Builder para el res...
    env['BRCC32_BIN'] = env.Dir('$DELPHI').File('BIN/BRCC32.EXE')
    env['BRCC32_COM'] = '$BRCC32_BIN -fo$TARGET $SOURCE'
    env['BRCC32_MSG'] = "**** [BRCC32]: Compiling resource: '$TARGET' from: '$SOURCE'"
    
    DelphiResource = env.Builder(
                                action = SCons.Action.Action('$BRCC32_COM', '$BRCC32_MSG')
                                , src_suffix = '.rc'
                                , target_suffix = '.res'
                                )

    env.InstallBuilder('DelphiResource', DelphiResource, ['DelphiRES'])

    delphi_lib = env.Dir('${DELPHI}/Lib')
    
    dcps = glob.glob(delphi_lib.abspath + '/*.dcp')

    for dcp in dcps:
        (base, ext) = os.path.splitext(dcp)
        name = os.path.basename(base)
        env.Alias(name, dcp)

    SConsEnvironment.ReadDelphiOptions = _ReadDelphiOptions
    
    home = env.Dir(_getDelphiPath(env, version))
    ide = runWrapper(env, 'delphi', env.File('${DELPHI_IDE}'), '${BPGNAME}${DELPHI_BPGSUFFIX}')
    env.Depends(ide, env.Alias('prepare-delphi'))
    
    dump = env.Command(target = '$BUILDTMP/dump', source = None, action = _DumpDelphiPath)
    env.Alias('dump', dump)


def options(vars):
    vars.Add('BPGNAME', 'Allows to override the default \'all.sln\' used when launching the Visual Studio IDE.', '#all')
    vars.Add('DELPHI_VERSION', 'Specifies the IDE msvsversion to use.', r'DELPHI7')

def exists(env): 
  version = env.get('DELPHI_VERSION', None)
  default = _getDelphiPath(env, version)
  bds5 = _getDelphiPath(env, r'Borland\BDS\5.0')
  bds6 = _getDelphiPath(env, r'Borland\BDS\5.0')
  delphi7 = _getDelphiPath(env, r'Borland\Delphi\7.0')
  return os.path.exists(default) or os.path.exists(delphi7) or os.path.exists(bds5) 
