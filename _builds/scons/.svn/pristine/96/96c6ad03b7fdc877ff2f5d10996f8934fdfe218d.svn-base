#!/usr/bin/env python
#-------------------------------------------------------------------------------
# Name:        TestRunner
# Purpose:      Run dunit testers after they finish construction
#
# Author:      LeandroConde
# Created:     07/04/2008
#-------------------------------------------------------------------------------

import sys, os
import subprocess
import regsvr32
import SCons.Builder
from SCons.Script import *
from SCons.Util import is_List, is_String

def generate(env):
    """ Add Builders and construction variables for running tests to an Environment. """

    builder = env.Builder(
                    action = RunAction,
                    target_factory = SCons.Node.FS.Entry,
                    source_factory = SCons.Node.FS.File   
                  )

    env.InstallBuilder('DunitRun', builder, ['DUnitRun'])
    env.AddMethod(DunitTest)
    env.AddMethod(DunitTest, 'DUnitTest')


def DunitTest(
        env
      , name        = None
      , program     = None
      , output      = None
      , depends     = None
      , register    = None
      , args        = None
      , outdir      = '$TEMP'
      , testingdir  = '$TESTDIR'
      , programdir  = ''
      , workdir     = '${TESTINGDIR}/${PROGRAMDIR}'
      , curdir      = '.'
      ):
    """ Method that allows setting up the targets needed to execute a unit test and capture its output to a log file.

        @param name:
            The tester's executable name with or without the '.exe' extension, which must be relative to the 'workdir' directory.
        @param program:
            The full path to the tester's executable.
        @param output:
            The full path to the tester's output log file which is generated as a target.
            The best is to put this file outside the working copy, normally in %TEMP% directory is best.
        @param depends:
            A list of nodes on which this test is to be dependant. The purpouse is to allow SCons to detect dependences and invoke
            the proper actions before executing the test.
        @param register:
            A list of nodes which needs to be registered before the the tester is executed and de-registered after.
            That action is only taken when scons is executed with the argument: ALLOW-REGISTRATION=1 on the command line,
            which defaults to False (0). So the test will fail if the component needed are not registered on a particular
            workstation and that argument are not specified.
            @IMPORTANT: Keep in mind that /if/ the components are registered in your workstation an you run scons with that
            argument they *will be* de-registered after the test's execution, so your installation *will be* broken after that.
            *DON'T COMPLAINT* if you do this.
            These can be ActiveX DLLs, OCXs, or other EXEs and they are also added to the dependences needed by the tester, so,
            if they are produced by this working copy, they are fisrt built before executing the test.
        @param args:
            Extra arguments to be passed to the tester in their command line. May be None.
        @param outdir:
            Directory where to put the generated log file. It's preferable to put this file outside the working copy,
            to avoid scons executing the tester when building all the targets within it (i.e when executing scons .)
        @param testingdir:
            For specifing the testing directory, which normally will be assumed to be #release/testing but by using this parameter
            can be changed. It defaults to $TESTDIR which are defined by the standard SConstruct to be the above mentioned directory.
        @param programdir:
            A sub-directory within 'testingdir' where the executable is located
        @param workdir:
            Defaults to ${TESTINGDIR}/${PROGRAMDIR} and is for allowing overriding the two above settings in one place
    """

    if not program:
        assert name, "'program' o 'name' se tienen que especificar"
        program = '${WORKDIR}/${NAME}.exe'

    if not name:
        assert program, "'program' o 'name' se tienen que especificar"
        programdir, name = os.path.split(program)

    if '.' in name:
        name, ext = os.path.splitext(name)

    if not output:
        output = '${NAME}.log'
    elif os.path.exists(output) and os.path.isfile(output):
        outdir, tail = os.path.split(output)
    else:
        outdir = output

    local = env.Clone(
        OUTDIR      = outdir
      , WORKDIR     = workdir
      , NAME        = name
      , OUTPUT      = output
      , PROGRAM     = program
      , TESTINGDIR  = testingdir
      , PROGRAMDIR  = programdir
      )

    curdir = local.Dir(curdir)

    local.Append(
        TEMPLOG     = local.File('${OUTDIR}/$OUTPUT')
      , TESTLOG     = local.File('${WORKDIR}/$OUTPUT')
      , CURDIR      = curdir
      )

    target = local.Entry('${TEMPLOG}')
    program = local.File('${PROGRAM}')
    sources = [program]

    if args: sources += Split(args)

    workdir = local.Dir('$WORKDIR')
    depends = map(lambda dep: curdir.File(local.subst(dep)), Split(depends))

    for dep in list(depends):
        #print "depends on file: %s" % dep.abspath
        if not dep.exists(): continue
        if not dep.isfile(): continue

        depdir = dep.dir
        if depdir == workdir:
            continue

        #print "%s is not on the WORKDIR directory. Will first copy it" % dep.abspath
        newfile = workdir.File(dep.name)
        local.Command(
            target = newfile
          , source = dep
          , action = Copy(
                '$TARGET'
              , '$SOURCE'
              )
          )

        depends[depends.index(dep)] = newfile

    register = local.arg2nodes(Split(register), local.File)

    test = local.DunitRun(
            target = target
          , source = sources
          , chdir = workdir
          )

    local.AddPostAction(target, Copy('${TESTLOG.abspath}', '${TARGET.abspath}'))
    
    if len(depends) > 0:
        Depends(test, depends)
        
    if len(register) > 0: 
        depends += register
        regmodules = local.RegisterServer(target = '${NAME}', source = register)
        
        Depends(test, regmodules)
        

    runtest = local.Alias('run:${NAME}', [target])
    local.Alias('test', runtest)

    return test

def RunAction(target, source, env):
    """ Action for the builder that executes the tester.
        TARGET is the output log file and ...
        SOURCE is a list containing the TESTER at index 0 and any extra arguments needed for running it. """

    logfile = "$TEMP/RunAction.log"

    if source and not is_List(source):
        source = [source]

    assert len(source) >= 1, """ ASSERT: Especificacion de RunTest incorrectamente definida.""" \
                """Source tiene que ser una lista de al menos un elemento y el tester debe ser el elemento cero"""

    tester = source[0]

    if not target:
        target = tester

    fact = env
    if is_List(target):
        target = target[0]

    if not is_String(target):
        if target.isdir:
            fact = target
        target = fact.File(target)

    name, ext = os.path.splitext(target.abspath)
    logfile = fact.File(name + ".log")

    logfile = file(logfile.abspath, 'w')

    pgenv = env['ENV']
    path, basename = os.path.split(tester.abspath)

    print >>logfile, "= Test: %s =" % basename
    print >>logfile, " * Running from: '%s'" % os.getcwd()

    print >>logfile, "--------"
    print >>logfile, "== Arguments: =="
    for ss in source[1:]:
        print >>logfile, " * %s" % (ss)

    print >>logfile, "--------"
    print >>logfile, "== Environment: =="
    for p, v in pgenv.iteritems():
        print >>logfile, " * %-30s = [%s]" % (p, v)

    args = map(lambda x: x.abspath, source)
    result, stdout, stderr = RunTest(args, env=pgenv)

    if result: print >>sys.stderr, "TEST FAILED: %s result was: %d" % (basename, result)
    print >>logfile, "--------"
    print >>logfile, "== Return code: =="
    print >>logfile, " * Value: %d" % result

    if result:
        print >>logfile, "== Error Output: =="
        if stderr:
            print >>logfile, "{{{"
            print >>logfile, stderr
            print >>logfile, "}}}"

    print >>logfile, "--------"
    print >>logfile, "== Standard Output =="
    if stdout:
        print >>logfile, "{{{"
        print >>logfile, stdout
        print >>logfile, "}}}"
    
    return result

def RunTest(*args, **kw):
    """ Executes a sub-process, waits for termination, and captures their normal and error output, returning
    a three-tuple with their error code and both outputs streams. """

    if "stdout" not in kw:
        kw["stdout"] = subprocess.PIPE

    if "stderr" not in kw:
        kw["stderr"] = subprocess.PIPE

    if "universal_newlines" not in kw:
        kw["universal_newlines"] = True

    text = None
    if "input" in kw:
        text = kw["input"]
        del kw["input"]

    p = subprocess.Popen(args, **kw)

    out, err = p.communicate(text)

    return p.returncode, out, err

def exists(env):
    """ Only needed by signaling SCons that the tool is present. """
    return 1
