# -*- coding: iso-8859-1 -*-

import os
import re
import glob
import os.path
import ConfigParser
import string
import sys
import parse
import utils
import sqlite3
from optparse import OptionParser

sys.setrecursionlimit(64*1024)

stdout = sys.stdout
debug = True

_CREATEDB_STMTs = [
#-------------------------------------------
"""
BEGIN TRANSACTION;

drop table if exists USES;

drop table if exists UNIT;

create table if not exists UNIT (
        id          varchar(64)     NOT NULL,
        name        varchar(64)     NOT NULL,
        location    varchar(255)    NULL,
        constraint id_unit primary key (id)
    );
    
create table if not exists USES(
        unit        varchar(64)     NOT NULL,
        unit_used   varchar(64)     NOT NULL,
        uses_kind   varchar(24)     NOT NULL,
        constraint pk_uses_id primary key (unit, unit_used)
    );

COMMIT;
"""
#-------------------------------------------
]


_EXISTS_UNIT_STMT = """
select count(name) from UNIT where id = ?;
"""

_SELECT_UNIT_STMT = """
select * from UNIT where id = ?;
"""

_INSERT_UNIT_STMT = """
insert into UNIT(id, name, location) values (?, ?, ?);
"""

_EXISTS_USES_STMT = """
select count(uses_kind) from USES where unit = ? and unit_used = ?;
"""

_INSERT_USES_STMT = """
insert into USES(unit, unit_used, uses_kind) values (?, ?, ?);
"""

_SELECT_USES_STMT = """
select
    a.name as uses,
    b.name as used,
    r.uses_kind as kind,
    a.location as uses_location,
    b.location as used_location
from
    USES r
join
    UNIT a
on
    r.unit = a.id
join
    UNIT b
on
    r.unit_used = b.id
where
    unit = ?;
"""

_SELECT_USED_STMT = """
select
    a.name as uses,
    b.name as used,
    r.uses_kind as kind,
    a.location as uses_location,
    b.location as used_location
from
    USES r
join
    UNIT a
on
    r.unit = a.id
join
    UNIT b
on
    r.unit_used = b.id
where
    r.unit_used = ?;
"""

def DBCreate(con):
    cur = con.cursor()
    for s in _CREATEDB_STMTs:
        cur.executescript(s)
    return con

def DBOpen(dbname, rebuild=0):
    found = os.path.exists(dbname)
    
    if rebuild and found:
        try:
            os.remove(dbname)
        except:
            pass
        
    con = sqlite3.connect(dbname, isolation_level = None)
    if rebuild or not found:
        DBCreate(con)
    return con

def makeRelative(path, basedir):
    """ makes an absolute path name to a relative pathname."""
    if os.path.isabs(path):
        basedir = os.path.join(basedir, "")
        if not path.startswith(basedir):
            return path
        path = path.replace(basedir, '')

    return path


def DBSaveUnit(prj, con, unit):
    
    cur = con.cursor()
    try:
        location = unit.location
        if location:
            location, tail = os.path.split(unit.location)
            location = makeRelative(location, prj.workdir)

        cur.execute(_EXISTS_UNIT_STMT, (unit.id,))
        res = cur.fetchone()
        if res[0] == 0:
            cur.execute(_INSERT_UNIT_STMT, (unit.id, unit.name, location))
            print >>stdout, "--- storing UNIT(%(name)s, %(location)s)" % dict(name = unit.name, location = location)
            
    finally:
        cur.close()

def DBSaveUses(con, ref):
    
    cur = con.cursor()
    try:
        cur.execute(_EXISTS_USES_STMT, (ref.module.id, ref.unit.id))
        res = cur.fetchone()
        if res[0] == 0:
            cur.execute(_INSERT_USES_STMT, (ref.module.id, ref.unit.id, ref.kind))
            print >>stdout, "--- storing USES(%(unit)s, %(used)s, %(kind)s)" % dict(unit = ref.module.name, used  = ref.unit.name, kind = ref.kind)
            
    finally:
        cur.close()
        
class EDependsError(Exception):
    pass

class Project():

    def __init__(self, filename, workdir, path = []):

        base, tail = os.path.split(filename)
        name, ext = os.path.splitext(tail)
        self.location = os.path.abspath(filename)
        self.workdir = workdir
        self.base = base
        self.name = name
        self.path = []
        if ext.lower() == ".dpr" or ext.lower() == ".dpk":
            self.path = self._getProjectPath()
        if path:
            self.path.extend(path)
        self.uses = []

    @property
    def id(self):
        return self.name.lower()

    def _expandVars(self, path):
        while '$' in path:
            path = os.path.expandvars(path)
        return os.path.normpath(path)

    def _getProjectPath(self):
        ini = ConfigParser.RawConfigParser()
        dof = os.path.join(self.base, self.name + ".dof")
        ini.read(dof)
        pathstr = ini.get("Directories", "SearchPath")
        pathstr = pathstr.replace('$(', '${').replace(')', '}')
        paths = map(self._expandVars, pathstr.split(os.pathsep))
        return paths

    def findModule(self, module):
        if not module:
            return None

        location = module.location
        if location:
            location = os.path.expandvars(location)
            
        if location and not os.path.isabs(location):
            location = os.path.join(self.base, location)

        return module


class Module:
    def __init__(self, name, location, typ = None):
        self.name = name
        self.type = typ
        self.uses = []
        if location:
            path, x = os.path.split(location)
        else:
            path = None
            
        self.path = path or "."
        self.location = location
        
    def __repr__(self):
        return self.name
        
    @property
    def id(self):
        return self.name.lower()

class ModuleList:

    def __init__(self):
        self.dict = {}

    def __iter__(self):
        return self.dict.itervalues()

    def __getitem__(self, key):
        return self.dict[key]
    
    def __contains__( self, item):
        return item.id in self.dict.keys()
        
    def __repr__(self):
        return repr(self.dict.values())

    def append(self, unit):
        self.dict[unit.id] = unit
        return unit

    def extend(self, list):
        for item in list:
            self.dict[item.id] = item
        return self

    def lookup(self, module):
        return self.get(module.id) or module

    def get(self, id):
        id = id.lower()
        if id in self.dict:
            return self.dict[id]
        return None

    def Add(self, name, location):
        return self.append(Module(name, location))

class DependsReference(object):

    def __init__(self, module, unit, kind):
        self.module = module
        self.unit = unit
        self.kind = kind

    def __repr__(self):
        return self.module.name + " -- [" + self.kind + "] -> " + self.unit.name

class DependsParser():
    
    _Mapping = {}

    def __init__(self, project):
        self.project = project

    @classmethod
    def Parse(cls, project, module):
        self = cls(project)
        self.parser = self._open(module)
        return self

    def project(self):
        return self.project

    def _open(self, module):

        assert module, "filename not specified"

        location = os.path.expandvars(module.location)
        if not os.path.isabs(location):
            location = os.path.join(self.project.base, location)

        return parse.Parser(location)


    COMMENT_START = ["{", "(*", "//"]
    COMMENT_END = ["}", "*)", None]
    COMMENT_START_INITIAL = map(lambda x: x[0] if x else None, COMMENT_START)
    COMMENT_END_INITIAL = map(lambda x: x[0] if x else None, COMMENT_END)
    DEFINED_SYMBOLS = {}

##    def _doSync(self):
##        if self.parser.Match('{$'):
##            directive = self.parser.Next().upper()
##
##            if directive == "IFDEF" or directive == "IFNDEF":
##                symbol = self.parser.Next().upper()
##
##            self.parser.Check('}')
##
##
##            if      (directive == "IFDEF" and symbol in DependsParser.DEFINED_SYMBOLS) \
##                or  (directive == "IFNDEF" and symbol not in DependsParser.DEFINED_SYMBOLS):
##
##
##
##
##        elif self.parser.current in DependsParser.COMMENT_START_INITIAL:
##            self._skipComments()
##

    def _skipComments(self):
        
        CS = DependsParser.COMMENT_START
        CE = DependsParser.COMMENT_END
        CSI = DependsParser.COMMENT_START_INITIAL
        CEI = DependsParser.COMMENT_END_INITIAL

        while True:
            if not self.parser.current in CSI:
                break

            index = CSI.index(self.parser.current)
            starter = CS[index]
            
            if not self.parser.Match(starter):
                break
            
            end = CEI[index] or self.parser.eof
            ender = CE[index] or self.parser.eof
            
            while self.parser.current != end or not self.parser.Match(ender):
                self.parser.Advance()

        return self.parser

    def _skipUntil(self, predicate):
        while not predicate(self.parser.current):
            self.parser.Advance()
        return self.parser

    def _parseUsesItem(self, pool):

        self._skipComments()

        unitname = self.parser.Next()
        
        while self.parser.current == '.':
            self._skipComments()
            self.parser.Advance()
            unitname += '.'       
            unitname += self.parser.Next()
        
        unitpath = None
        
        self._skipComments()

        if self.parser.current.lower() == 'in':

            self.parser.Next()
            self._skipComments()
            
            unitpath = self.parser.Next()
            
            if unitpath and unitpath[0] == "'":
                unitpath = unitpath.strip("'")

            self._skipComments()

        return pool.get(unitname) or Module(unitname, unitpath)

    def _parseUsesList(self, pool):

        List = ModuleList()

        while True:

            unit = self._parseUsesItem(pool)
            List.append(unit)
            if self.parser.current == ';':
                break
            self._skipComments()
            self.parser.Check(",", doAdvance = True)

        self._skipComments()
        self.parser.Check(';')
        self._skipComments()
        
        return List

    def _parseUsesClause(self, pool):
        if self.parser.current.lower() == 'uses':
            self.parser.Advance()
            self._skipComments()
            return self._parseUsesList(pool)
        else:
            return ModuleList()

    def _parseProgram(self, pool):
        self.parser.Check('program')
        self._skipUntil(lambda x: x.lower() == 'uses')
        uses = self._parseUsesClause(pool)
        return {'uses': uses}

    def _parseLibrary(self, pool):
        self.parser.Check('library')
        self._skipUntil(lambda x: x.lower() == 'uses')
        uses = self._parseUsesClause(pool)
        return {'uses': uses}

    def _parsePackage(self, pool):
        self.parser.Check('package')
        self._skipComments()
        self._skipUntil(lambda x: x.lower() == 'requires')
        self.parser.Advance()
        self._skipComments()
        reqs = self._parseUsesList(pool)
        self._skipUntil(lambda x: x.lower() == 'contains')
        self.parser.Advance()
        self._skipComments()
        cont = self._parseUsesList(pool)
        return {'requires': reqs, 'contains': cont}

    def _parseUnit(self, pool):
        self.parser.Check('unit')
        self._skipUntil(lambda x: x.lower() == 'interface')
        self.parser.Advance()
        self._skipComments()
        intf = self._parseUsesClause(pool)
        self._skipUntil(lambda x: x.lower() == 'implementation')
        self.parser.Advance()
        self._skipComments()
        impl = self._parseUsesClause(pool)
        return {'interface': intf, 'implementation': impl}

    _Mapping['program'] = _parseProgram
    _Mapping['package'] = _parsePackage
    _Mapping['library'] = _parseLibrary
    _Mapping['unit'] = _parseUnit

    def parseModule(self, pool):
        self._skipComments()
        tok = self.parser.current.lower()
        return self._Mapping[tok](self, pool)

def parseModule(project, pool, module, parent = None, visited = ModuleList()):

    module = project.findModule(pool.lookup(module))

    if not module or not module.location:
        return visited
    
    parser = DependsParser.Parse(project, module)
    
    depends = parser.parseModule(pool)
    
    for typ in depends.iterkeys():
        units = depends[typ]

        for unit in units:
            print "--> Module: [%s] references: [%s in '%s'] from %s clause" % (module.name, unit.name, unit.location, typ)

            ref = DependsReference(module, unit, typ)
            module.uses.append(ref)

            if visited.get(unit.id):
                continue

            visited.append(unit)
            
            parseModule(project, pool, unit, module, visited)

    return visited

def parsePath(pathstr):
    if pathstr:
        list = string.split(pathstr, os.pathsep)
    else:
        list = []
    return list

def parseProject(prjfile, workdir, path):
    project = Project(prjfile, workdir, path)
    pool = findUnits(project.path)
    result = parseModule(project, pool, project)
    return (project, result)

def storeNode(prj, con, module, visited = []):
    visited.append(module.id)
    DBSaveUnit(prj, con, module)
    
    if len(module.uses) == 0: print "para [%s] module.uses está vacío!!!!" % module.name
    for ref in module.uses:
        unit = ref.unit
        DBSaveUnit(prj, con, unit)
        DBSaveUses(con, ref)
        if unit.id in visited:
            continue
        storeNode(prj, con, unit, visited)

def storeProject(con, prj, units):
    storeNode(prj, con, prj)

def findUnits(path):
    
    units = ModuleList()
    for p in path:
        sources = glob.glob(os.path.join(p, '*.pas'))
        for s in sources:
            base, tail = os.path.split(s)
            name, ext = os.path.splitext(tail)
            units.Add(name, s)
    return units

def getRow(cur, res):
    row = {}
    
    desc = cur.description
    for ix in range(len(desc)):
        fd = desc[ix]
        row[fd[0]] = res[ix]
            
    return row

def getRows(cur):
    while True:
        res = cur.fetchone()
        if not res:
            break
        yield getRow(cur, res)

def getModule(row, name = 'name', location = 'location'):
    return Module(row[name], row[location])

def fetchModule(con, unit, name = 'name', location = 'location'):
    cur = con.cursor()
    try:
        cur.execute(_SELECT_UNIT_STMT, (str(unit), ))
        row = getRow(cur, cur.fetchone())
    finally:
        cur.close()
    return getModule(row)

def enumUses(con, module):
    cur = con.cursor()
    try:
        cur.execute(_SELECT_USES_STMT, (module.id, ))
        for row in getRows(cur):
            unit = getModule(row, 'used', 'used_location')
            yield DependsReference(module, unit, row['kind'])
    finally:
        cur.close()
        

def enumUsed(con, module):
    cur = con.cursor()
    try:
        cur.execute(_SELECT_USED_STMT, (module.id, ))
        for row in getRows(cur):
            unit = getModule(row, 'uses', 'uses_location')
            yield DependsReference(module, unit, row['kind'])
    finally:
        cur.close()

def getUses(con, module, recurse = 1, results = ModuleList()):
    
    results.append(module)
    if recurse:
        recurse = recurse - 1
        for ref in enumUses(con, module):
            unit = ref.unit
            module.uses.append(ref)
            if results.get(unit.id):
                continue
            getUses(con, unit, recurse, results)

    return module
    
def getUsed(con, module, recurse = 1, results = ModuleList()):

    results.append(module)
    if recurse:
        recurse = recurse - 1
        for ref in enumUsed(con, module):
            unit = ref.unit
            module.uses.append(ref)
            if results.get(unit.id):
                continue
            getUsed(con, unit, recurse, results)

    return module

def setEnv(workdir):
    os.environ['DELPHI'] = r"$ProgramFiles\Borland\Delphi7"
    os.environ['WORKDIR'] = os.path.abspath(workdir)
    os.environ['WC'] = os.path.abspath(workdir)
    os.environ['DCUDIR'] = r"$WORKDIR\_temp\_dcu"
    os.environ['DCPDIR'] = r"$WORKDIR\_temp\_bin\lib"
    os.environ['RTLDIR'] = r"$WORKDIR\_temp\_bin\rtl"
    os.environ['SRCDIR'] = r"$WORKDIR\source"
    os.environ['BINDIR'] = r"$WORKDIR\release\binaries"
    os.environ['LIBDIR'] = r"$SRCDIR\_ext\_lib"
    os.environ['TLBDIR'] = r"$SRCDIR\_ext\_tlb"
    os.environ['SYSDIR'] = r"$SRCDIR\_ext\_sys"
    os.environ['PATH'] = r"$BINDIR\rtl;$RTLDIR;$PATH"

def printNode(module, dir):
    uses = module.uses
    if dir == ">":
        text = "    " + module.name + ' -> %s [label = "%s"] ;'
    else:
        text = "    " + '%s -> ' + module.name + ' [label = "%s"] ;'
    for ref in uses:
        unit = ref.unit
        print >>stdout, text % (ref.unit.name, ref.kind)
        printNode(ref.unit, dir)

def printDot(module, dir = '>'):
    print >>stdout, "digraph Depends { "
    printNode(module, dir)
    print >>stdout, " };"

def printTree(module, dir = '>'):
    uses = module.uses
    text = ' %-40s: %s--------------[%-14s]-------------%s %s '
    for ref in uses:
        unit = ref.unit
        print >>stdout, text % (module.name, dir, ref.kind, dir, unit.name)
        printTree(unit, dir)

def printList(modules):
    for module in modules:
        print >>stdout, "%-40s in '%s'" % (module.name, module.location or '')

def build(con, prjfile, workdir, path):
    (project, units) = parseProject(prjfile, workdir, path)
    storeProject(con, project, units)

def getOptions():
    op = OptionParser()
    op.add_option('-F', '--format', type="choice", choices = ['list', 'tree', 'dot'], dest="format", help="Selecciona el formato de salida del listado", default="list")
    op.add_option('-d', '--deep', dest="deep", action="store_const", const=-1, help="Habilita el scaneo de uses recursivo")
    op.add_option('-o', '--output', dest="output", help="Redirije la salida estandar a el archivo especificado")
    op.add_option('-W', '--workdir', dest="workdir", help="Previo a procesar ejecuta un 'cd <workdir>'")
    op.add_option('-Q', '--quiet', dest="quiet", help="Elimina todos los mensajes de estado exceptuando los de error")
    op.add_option('-n', '--dbname', dest="dbname", help="Nombre de la base de datos que se busca/crea", default='.depends.dblite')
    op.add_option('-U', '--unit-path', dest="path", help="Lista de directorios donde buscar las units referenciadas")
    op.add_option('-L', '--levels', dest="deep", type="int", help="Habilita el scaneo de uses recursivo", default=1)

    return op.parse_args()

if __name__ == "__main__":

    opts, args = getOptions()

    workdir = opts.workdir
    dbname = opts.dbname or '.depends.dblite'
    deep = opts.deep or -1
    format = opts.format or "list"
    if opts.output:
        fp = file(opts.output, "w")
        stdout = fp
    else:
        fp = None
    debug = not opts.quiet
    path = parsePath(opts.path)

    buildcmds = ("build", "rebuild")
    try:
        if workdir:
            os.chdir(workdir)
        else:
            workdir = os.path.abspath(os.getcwd())

        setEnv(workdir)

        dbname = os.path.join(workdir, '.depends.dblite')
        command = args[0].lower()
        
        con = DBOpen(dbname, rebuild = command == "rebuild")
        try:
            if command not in buildcmds:
                assert len(args) == 2, "No esta especificado la unit que se desea consultar. Uso: %s {uses | used} <unit>" % (sys.argv[0])

                moduleid = args[1].lower()

                module = fetchModule(con, moduleid)
                
                if not module:
                    raise Exception, "Module %s not found" % moduleid
                
                results = ModuleList()
                if command == "uses":
                    root = getUses(con, module, deep, results)
                elif command == "used":
                    root = getUsed(con, module, deep, results)
                else:
                    root = None

                if not root:
                    raise Exception, "Unknown command: %s" % command

                if format == "dot":
                    printDot(root)
                elif format == "tree":
                    printTree(root, '>' if command == "uses" else '<')
                elif format == "list":
                    printList(sorted(results))
                else:
                    raise Exception, "Unknown format: %s" % format


            else:
                assert len(args) == 2, "Se necesitan dos parametros: %s [re]build <project-file> [<path-list>]" % (sys.argv[0])

                prjfile = os.path.normpath(args[1])
                if not os.path.isabs(prjfile):
                    prjfile = os.path.abspath(prjfile)

                build(con, prjfile, workdir, path)
                
        finally:
            con.close()
            if fp: fp.close()

    except Exception, error:
        print >>sys.stderr, "error: " + str(error)


