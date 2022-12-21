"""BuildTools
##################################################################
* BuildTools.py - Wrapper functions
##################################################################
 Created by: Ernesto Castro (ercastro@siderca.com) Sep. '07
       $Id: delphi.py 7217 2007-10-23 19:51:29Z t61248 $
##################################################################
"""
import os
import re
import string
import glob
import pythoncom
import fnmatch
import utils
import SCons.Util
from SCons.Script.SConscript import SConsEnvironment

def GetTypelibName(env, file):
    return utils.GetTypelibName(file)

def AppendToPath(env, **kw):
    """
    Sean (k, v) los elementos de kw, esta funcion apendea los v a la variable en env referida por sus k.
        Si se especifica mapping=funcion(x) aplica a cada elemento la funcion
        Si se especifica sep="value" usa "value" como separador de union de los paths
        Por default se asume que si no se especifica mapping se usa os.path.abspath y
          si no se especifica sep se usa os.pathsep
    """

    sep = os.pathsep

    if "sep" in kw:
        sep = kw["sep"]
        del kw["sep"]

    for (key, new) in kw.items():
        try:
            pathvar = env[key]
        except KeyError:
            pathvar = []

        newvalue = map(lambda x: os.path.abspath(os.path.normcase(os.path.normpath(env.subst(str(x))))), env.Split(new))
        pathvar.extend(newvalue)
        env[key] = pathvar

def SConscripts(env, base_dirs, script='SConscript', include='.*', exclude='[\.]+.*', exports='', debug=0):

    if not SCons.Util.is_Sequence(base_dirs):
        import glob
        base_dirs = glob.glob(env.Dir(base_dirs).abspath)

    base_dirs = map(env.Dir, base_dirs)

    for base in base_dirs:
        wd = base.abspath
        if debug > 1: print 'Searching base directory: %s' % (wd)
        for root,dirs,files in os.walk(wd):
			for file in [f for f in files if f == script]:
				name = os.path.join(root, file)
				if debug > 0: print 'SConscript file named: %s has been read succesfully' % (name)
				env.SConscript(name, exports)


def InstallBuilder(env, name, builder, aliases = None):
    try:
        bld = env['BUILDERS'][name]
    except KeyError:
        bld = builder
        env['BUILDERS'][name] = bld

    if aliases:
        for alias in aliases:
            env['BUILDERS'][alias] = bld

    return bld


def _GlobFiles(env, basedir, includes = '.*', excludes = None, dirincludes = '.*', direxcludes = None):
    import re

    class _Test:
        def __init__(self, includers, excluders):
            if not includers: includers = [None] * len(excluders)
            if not excluders: excluders = [None] * len(includers)
            self.matchers = zip(includers, excluders)
        
        def __call__(self, item):
            
            for inc, exc in self.matchers:
                
                if inc and not inc.match(item): 
                    return False 

                if exc and exc.match(item): 
                    return False 
            
            return True
    
    includes = env.Listify(includes)
    excludes = env.Listify(excludes)
    dirincludes = env.Listify(dirincludes)
    direxcludes = env.Listify(direxcludes)
    
    compileit = lambda i, e = env: re.compile(e.subst(i)) if i else None
    
    fileinc = map(compileit, includes)
    fileexc = map(compileit, excludes)
    dirinc = map(compileit, dirincludes)
    direxc = map(compileit, direxcludes)
    
    basedir = env.Dir(basedir)

    nodes = list()
    
    dirtest = _Test(dirinc, direxc)
    filetest = _Test(fileinc, fileexc)
    
    for root, dirs, files in os.walk(basedir.abspath):
        
        for d in dirs:
            path = os.path.join(root, d)
            if not dirtest(path):
                dirs.remove(d)

        for i in files:
            path = os.path.join(root, i)
            if filetest(path):
                nodes.append(env.File(path))
        
    return nodes

def _GlobSCons( env, basedir = '.', includes = SCons.Util.Split( '*' ), excludes = None):
   """Similar to glob.glob, except globs SCons nodes, and thus sees
   generated files and files from build directories.  Basically, it sees
   anything SCons knows about.  A key subtlety is that since this function
   operates on generated nodes as well as source nodes on the filesystem,
   it needs to be called after builders that generate files you want to
   include.
   It will return both Dir entries and File entries
   """

   def fn_filter(node):
      fn = os.path.basename(str(node))
      match = 0
      for include in includes:
         if fnmatch.fnmatchcase( fn, include ):
            match = 1
            break
      if match == 1 and not excludes is None:
         for exclude in excludes:
            if fnmatch.fnmatchcase( fn, exclude ):
               match = 0
               break
      return match

   def filter_nodes(where):
       children = filter(fn_filter, where.all_children(scan=0))
       nodes = []
       for f in children:
           nodes.append(gen_node(f))
       return nodes

   def gen_node(n):
       """Checks first to see if the node is a file or a dir, then
       creates the appropriate node. [code seems redundant, if the node
       is a node, then shouldn't it just be left as is?
       """
       if type(n) in (type(''), type(u'')):
           path = n
       else:
           path = n.abspath
       if os.path.isdir(path):
           return Dir(n)
       else:
           return File(n)

   here = env.Dir(basedir)
   nodes = filter_nodes(here)
   node_srcs = [n.srcnode() for n in nodes]
   src = here.srcnode()
   if src is not here:
       for s in filter_nodes(src):
           if s not in node_srcs:
               # Probably need to check if this node is a directory
               nodes.append(gen_node(os.path.join(basedir,os.path.basename(str(s)))))
   return nodes

def GenerateDependences(env, sources_dir, output_dir):
    """
    This funcion generates one builder per binary file located in 'deps_dir' that has
    type info. Each target has a name following this schema:
    		module_name_TLB.pas

    Every file created by the builder is sent to out_dir
    """

    targets = []
    sources = []
    typelibs = []
    packages = []

    sources_dir = env.Entry(sources_dir).abspath
    output_dir = env.Entry(output_dir).abspath

    paths = []
    
    for root, dirs, files in os.walk(sources_dir):
        
        root_node = env.Dir(root)
        
        for fname in files:

            source_node = root_node.File(fname)
            
            (name, ext) = os.path.splitext(fname)
            ext = ext.lower()

            if ext in ['.dll', '.ocx', '.tlb', '.exe', '.dcp']:

                sources.append(source_node)

                alias_node = env.Alias(fname, source_node)
                alias_node = env.Alias(name, source_node)
                
                if ext == '.dcp':
                    packages.append(source_node)
                    packages.append(alias_node)
                    
                    if not root_node in paths:
                        paths.append(root_node)
                    continue

                # This shouldn't be here... but it a simple way of doing it.
                # If ever restructured this script, this should get out of here
                # This is only for interop assemblies.
                if fnmatch.fnmatch(fname, '*interop*'):
                    continue

                # Target file name
                tlibname = env.GetTypelibName(source_node.abspath)

                # If it is an empty string, then it has no type info
                if not tlibname:
                    continue
                
                tlibname += '_TLB'
                typelibs.append(source_node)

                if not env.ans.lookup(tlibname):
                    # full target name
                    target = os.path.normpath(os.path.join(output_dir, tlibname))

                    # Creates the builders for this target
                    tlib = env.DelphiTypelib(target, source_node)
                    targets.append(tlib)
    
                    # Adds the builder to a list that is returnes by this function

                    # Generates a human-friendly target with the same name of the generated typelib file name
                    env.Alias(tlibname, tlib)
                    env.Alias(tlibname + '.pas', tlib)

    if len(paths) > 0:    
        env.PrependENVPath('PATH', map(lambda x: x.abspath, paths))
        env.AppendToPath(DCC32_UNIT = paths)

    if packages: env.Alias('packages', packages)
    return (targets, sources, typelibs)

def AddBuilders(env, path):

    if not os.path.exists(path):
        return

    mask = os.path.join(path, '*.py')
    for entry in glob.iglob(mask):

        (base, fn) = os.path.split(entry)
        (name, ext) = os.path.splitext(fn)

        if os.path.isfile(entry) and name != '__init__':
            env.Tool(name, toolpath=[path])

def AddTools(env, paths):
    
    paths = env.Nodify(paths, env.Dir)
    
    for path in paths:
        if not path.exists(): continue
    
        for name in os.listdir(path.abspath):
            if name[0] == '.': continue
            
            toolpath = os.path.join(path.abspath, name, 'scons')
            if not os.path.isdir(toolpath): continue
            
            env.Tool(name, [toolpath])
            

def Listify(env, argument):
    """ Given any argument try to generate from it a list of items using some heuristics."""
    
    if not argument: 
        argument = []
    
    if SCons.Util.is_String(argument): 
        argument = env.Split(argument)
    
    if not SCons.Util.is_Sequence(argument): 
        argument = [argument]
        
    return argument
        


def Nodify(env, argument, node_factory): 
    """ Given any argument try to generate from it a list of node created from the specified node_factory."""
    return env.arg2nodes(env.Listify(argument), node_factory)   

HKCR = SCons.Util.HKEY_CLASSES_ROOT
HKLM = SCons.Util.HKEY_LOCAL_MACHINE
HKCU = SCons.Util.HKEY_CURRENT_USER
HKU = SCons.Util.HKEY_USERS
def RegGetValue(env, key, throw = False, default = None, root = HKLM):
    try:
        value, type = SCons.Util.RegGetValue(root, key)
        return value
    except Exception, err:
        if throw: 
            raise err
        return default

SConsEnvironment.Glob = _GlobFiles
SConsEnvironment.GlobFiles = _GlobFiles
SConsEnvironment.GlobNodes = _GlobSCons
SConsEnvironment.InstallBuilder = InstallBuilder
SConsEnvironment.SConscripts = SConscripts
SConsEnvironment.AppendToPath = AppendToPath
SConsEnvironment.ImportDependences = GenerateDependences
SConsEnvironment.GetTypelibName = GetTypelibName
SConsEnvironment.AddBuilders = AddBuilders
SConsEnvironment.AddTools = AddTools
SConsEnvironment.Listify = Listify
SConsEnvironment.Nodify = Nodify
SConsEnvironment.RegGetValue = RegGetValue

class ForeachAction:
    """ Functional object executes the received Action 
        for each pair of (target, source) on which is called."""
    def __init__(self, action, reversed = False, **kw):
        self.kwargs = kw
        self.action = action
        self.reversed = reversed

    def __call__(self, target, source, env, **kw):
        kw.update(self.kwargs)
        
        if self.reversed:
            target.reverse()
            source.reverse()
            
        for t, s in zip(target, source):
            result = apply(self.action, (t, s, env), kw)
            if result:
                return result
        return 0

    def strfunction(self, target, source, env):
        return " * Registering COM modules ..."

