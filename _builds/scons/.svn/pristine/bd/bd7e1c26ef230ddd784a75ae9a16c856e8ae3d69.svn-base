"""

StdInstallTool
--------------

Wrapper functions for common installing tasks

 * StdInstall: Wraps the standard SCons Install builder and provides some convenience parameters for allowing more reduced install 
   procedures.
   
 * ComInstall: Uses the above and adds a PostAction that executes REGSVR32 on every installed target 
 
--------
 Created by: Leandro Conde  (leandor@gmail.com)    May. '08
--------
   $Id:  $
"""


class _source_factory:
        
    def __init__(self, env):
        self.env = env
        
    def __call__(self, node):
        return self.env.arg2nodes(node.sources, self.env.File)

def StdInstall(env, target_dir, source_files = None, source_alias = None, name = None, preactions = None, postactions = None, requires = None, depends = None, requires_alias = None, depends_alias = None):
    
    assert source_alias or source_files, "Either source_alias or source_files must be specified but both were None."
    
    root = env.Dir('$PREFIX')   
    target = root.Dir(env.subst(target_dir))

    source = env.Nodify(source_files, env.File)
    
    source_alias = env.Nodify(source_alias, env.Alias)
    
    if source_alias:
        factory = _source_factory(env)
        source += map(factory, source_alias)
    
    result = env.Install(target, source)
    
    preactions = env.Listify(preactions)
    postactions = env.Listify(postactions)
    
    requires = env.Nodify(requires, env.File)
    requires += env.Nodify(requires_alias, env.Alias) 
    depends = env.Nodify(depends, env.File)
    depends += env.Nodify(depends_alias, env.Alias)
    
    for action in preactions:
        env.AddPreAction(result, action)
           
    for action in postactions:
        env.AddPostAction(result, action)

    if requires: env.Requires(result, requires)
    if depends: env.Depends(result, depends)
    if name: env.Alias(name, result)
    
    return result
        
def ComInstall(env, target_dir, postactions = None, **kw):
    from regsvr32 import RegisterTargets 
    
    postactions = env.Listify(postactions)
    postactions.append(RegisterTargets)
    return StdInstall(env, target_dir, postactions = postactions, **kw)

def exists(env):
    return 1

def options(opts):
    
    def __convertPrefix(value, env = None):
        if not env:
            return value
        root = env.Dir('#')
        return root.Dir(value)
    
    # Defines de 'PREFIX' path used for installations
    opts.Add(
        key         = 'PREFIX'
      , help        = 'Specifies the target directory for the installation procedure.'
      , default     = '#../_INSTALL_DIR'
      , converter   = __convertPrefix
      )

def generate(env):
    env.AddMethod(StdInstall)
    env.AddMethod(ComInstall)


