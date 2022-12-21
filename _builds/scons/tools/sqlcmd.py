import os  
from SCons.Script import *
from SCons.Util import is_Sequence, Selector, WhereIs
from SCons.Variables import BoolVariable

def options(opts):
    opts.AddVariables(
        PathVariable( 
             'SQLCMD_HOME'
          ,  'Overrides the normal path where the SQLCMD can be found.'
          ,  None
          ,  PathVariable.PathAccept
          )
        , (
              'SQLCMD_SERVER'
          ,  'Defines the SQLSERVER machine to which we must connect.'
          ,  '.'
          )
        , (
              'SQLCMD_DATABASE'
          ,  'Defines the SQLSERVER database which must be USEd to run the scripts.'
          ,  'tempdb'
          )
    )

#SqlcmdAction = Action('${SQLCMD_COMMAND} -E -S${SQLCMD_SERVER} -d${SQLCMD_DATABASE} -i${SOURCE.abspath} -o${TARGET.abspath}')
SqlcmdAction = Action('@ECHO. && TYPE ${SOURCE.abspath} && ECHO. && ECHO -------------- && ECHO. && ECHO EJECUTE SOURCE.abspath > ${TARGET.abspath}')

SqlcmdBuilder = Builder(
    action = SqlcmdAction
  , multi = 1
  , source_suffix = '.sql'
  )

def tryPath(env, path):
    entry = env.File(path)
    return entry if entry.exists() else None
  
def tryVersion(env, version):
    return tryPath(env, "${MSSQL_ROOT}/%(version)d/Tools/Binn/SQLCMD.EXE" % {"version": version})
  
def generate(env):

    try:
        print 'Buscando SQL'
        SQLCMD_EXE    = 'SQLCMD'
        env['MSSQL_ROOT'] = env.Dir('${PROGRAM_FILES}\\Microsoft SQL Server')
    
        sqlcmd = tryPath(env, "${SQLCMD_HOME}/SQLCMD.EXE") or tryVersion(env, 100) or tryVersion(env, 90) or tryPath(env, env.WhereIs(SQLCMD_EXE)) or tryPath(env, WhereIs(SQLCMD_EXE))
    
        if not sqlcmd: return

        sqlcmd_path = sqlcmd.dir.abspath
    
        env['SQLCMD_HOME'] = sqlcmd_path
        env['SQLCMD_COMMAND'] = sqlcmd
        env['SQLCMD_SERVER'] = '.'
        env['SQLCMD_DATABASE'] = 'tempdb'
        env.InstallBuilder('SQLBatch', SqlcmdBuilder)
    except:
	    print 'NO existe el SQL, se ignora y continua'
	    return

def exists(env):
    return 1
