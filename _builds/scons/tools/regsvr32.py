import os
from SCons.Script import *
from SCons.Util import is_Sequence, Selector
from SCons.Variables import BoolVariable
from BuildTools import ForeachAction

def options(opts):
    opts.AddVariables(
        BoolVariable( 
             'ALLOW_REGISTRATION'
          ,  'Enable for allowing SCons to register COM modules.'
          ,  False
          )
      , BoolVariable( 
             'REGSVR_CREATE_TARGETS'
          ,  'Enables/Disables de creation of file markers used to keep track of the actions already executed.'
          ,  True
          )
    )

def _do_nothing(target, source, env):
    return 0

register_lib = Action('$REGSVR_REGISTER_DLL', '$REGSVR_REGISTER_DLL_S')
register_exe = Action('$REGSVR_REGISTER_EXE', '$REGSVR_REGISTER_EXE_S')
register_tlb = Action(_do_nothing, show = 0)
unregister_lib = Action('$REGSVR_UNREGISTER_DLL', '$REGSVR_UNREGISTER_DLL_S')
unregister_exe = Action('$REGSVR_UNREGISTER_EXE', '$REGSVR_UNREGISTER_EXE_S')
unregister_tlb = Action(_do_nothing, show = 0)

register = Selector(
      {
        '.dll': register_lib
      , '.ocx': register_lib
      , '.exe': register_exe
      , '.tlb': register_tlb
      }
    )

unregister = Selector(
      {
        '.dll': unregister_lib
      , '.ocx': unregister_lib
      , '.exe': unregister_exe
      , '.tlb': unregister_tlb
      }
    )

class ModuleRegistrator:
    """ Functional object that wraps calls to to register/de-register action """

    _selector_map = {
           False:  unregister
         , True:   register           
         }

    def __init__(self, registering = True, register_source = True, **kw):
        self.selector = ModuleRegistrator._selector_map[registering]
        self.kwargs = kw
        self.kwargs['show'] = True
        self.register_source = register_source
        
        if register_source:
            marker = '$TARGET.abspath'
        else:
            marker = '$SOURCE.abspath'
        
        if registering:
            self.toucher = Touch(marker)
        else:
            self.toucher = Delete(marker)

    def __call__(self, target, source, env, **kw):
        
        if not is_Sequence(target): target = [target]
        if not is_Sequence(source): source = [source]
        
        if not self.register_source:
            # swap arguments
            target, source = source, target 
        
        action = self.selector(env, source)
        
        if not env.get('ALLOW_REGISTRATION', False):
            #print "Skipping [%s] becuse ENV variable 'ALLOW_REGISTRATION' is not explicitly given" % action.strfunction(target, source, env)
            return 0
        
        create_marker_targets = env.get('REGSVR_CREATE_TARGETS', True)
        
        kw.update(self.kwargs)
        
        result = apply(action, (target, source, env), kw)
        
        if create_marker_targets:
            kw['show'] = False
            apply(self.toucher, (target, source, env), kw)
        
        return result
        
    def strfunction(self, target, source, env):
        return " * Registering COM module"

RegisterSource = ModuleRegistrator(registering = True, register_source = True, show = 0)
UnregisterSource = ModuleRegistrator(registering = False, register_source = True, show = 0)
RegisterTarget = ModuleRegistrator(registering = True, register_source = False, show = 0)
UnregisterTarget = ModuleRegistrator(registering = False, register_source = False, show = 0)

RegisterSources = ForeachAction(RegisterSource, show = 0)
UnregisterSources = ForeachAction(UnregisterSource, reversed = True, show = 0)
RegisterTargets = ForeachAction(RegisterTarget, show = 0)
UnregisterTargets = ForeachAction(UnregisterTarget, reversed = True, show = 0)

RegisterModule = RegisterSource
UnregisterModule = UnregisterSource
RegisterModules = RegisterSources
UnregisterModules = UnregisterSources 

def Emitter(target, source, env):

    tdir = env.Dir('$REGSVR_TARGET_DIR')

    if is_Sequence(target):
        target = target[0]

    tdir = tdir.Dir(target.name)

    result = []
    for item in source:
        t = tdir.File(item.name + '.marker')
        result.append(t)

    return result, source

RegisterServer = Builder(
            emitter = Emitter
            , action = RegisterSources
            , show = 0
            )

UnregisterServer = Builder(
            emitter = Emitter
            , action = UnregisterSources
            , show = 0
            )

def generate(env):
    
    regsvr = env.WhereIs('REGSVR32') or env.get('REGSVR_CMD', '%SystemRoot%\\System32\\REGSVR32.EXE')
    
    env.Replace(
        REGSVR_COMMAND          = env.File(regsvr)
    ,   REGSVR_SILENT           = '/s'
    ,   REGSVR_UNREGISTER       = '/u'
    ,   REGSVR_TARGET_DIR       = '$BUILDTMP'
    ,   REGSVR_REGISTER_DLL     = '$REGSVR_COMMAND $REGSVR_SILENT ${SOURCE.abspath}'
    ,   REGSVR_REGISTER_EXE     = '${SOURCE.abspath} /regserver'
    ,   REGSVR_UNREGISTER_DLL   = '$REGSVR_COMMAND $REGSVR_SILENT $REGSVR_UNREGISTER ${SOURCE.abspath}'
    ,   REGSVR_UNREGISTER_EXE   = '${SOURCE.abspath} /unregserver'
    ,   REGSVR_REGISTER_DLL_S   = '  * [REGSVR32] Registering COM library   : ${SOURCE.name.ljust(30)} \tfrom: "${SOURCE.dir}"'
    ,   REGSVR_REGISTER_EXE_S   = '  * [REGSVR32] Registering COM server    : ${SOURCE.name.ljust(30)} \tfrom: "${SOURCE.dir}"'
    ,   REGSVR_UNREGISTER_DLL_S = '  * [REGSVR32] Unregistering COM library : ${SOURCE.name.ljust(30)} \tfrom: "${SOURCE.dir}"'
    ,   REGSVR_UNREGISTER_EXE_S = '  * [REGSVR32] Unregistering COM server  : ${SOURCE.name.ljust(30)} \tfrom: "${SOURCE.dir}"'
    )
    
    env.InstallBuilder('RegisterServer', RegisterServer)
    env.InstallBuilder('UnregisterServer', UnregisterServer)

def exists(env):
    return 1
