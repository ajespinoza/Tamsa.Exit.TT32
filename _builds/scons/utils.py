"""utils
##################################################################
* utils.py - Various miscelaneous utilities
##################################################################
 Created by: Ernesto Castro (ercastro@siderca.com) Sep. '07
       $Id: delphi.py 7217 2007-10-23 19:51:29Z t61248 $
##################################################################
"""
import os
import string
import re
import pythoncom

def GetDirectories(path, include = '.*', exclude = ''):
    dirs = []

    re_inc = re.compile(include)
    re_exc = re.compile(exclude)
	
    for entry in os.listdir(path):
        if os.path.isdir(os.path.join(path, entry)):
            if re_inc.match(entry) and not exclude or not re_exc.match(entry):
                dirs.append(entry)
    return dirs

def GetTypelibName(file):
  if not os.path.exists(file):
    raise Exception("%s does not exist" % file)
  try:
    tlb = pythoncom.LoadTypeLib(file)
    module_name = tlb.GetDocumentation(-1)[0]
  except:
    module_name = ''
  return module_name

def PathJoin(seq, **kw):
    """
    Permite convertir una sequencia de valores interpretados como paths en una lista separada por separadores.
        Como keywords solo acepta mapping y/o sep:
            Si se especifica mapping=funcion(x) aplica a cada elemento la funcion dada
            Si se especifica sep="value" usa "value" como separador de union de los paths
        Por default se asume que si no se especifica mapping se usa os.path.abspath y
          si no se especifica sep se usa os.pathsep
    """
    
    sep = os.pathsep

    if "sep" in kw:
        sep = kw["sep"]
        del kw["sep"]

    dirs = map(lambda x: str(x), seq)
    path = sep.join(dirs)
    return path


def AppendCmdline(cmdline, value, prefix=' '):
    """
    Funcion simple que permite ir agregando sucesivamente valores a un string
        prefijandolos con prefix
    """
    if value:
        cmdline += prefix + str(value)
    return cmdline


def SearchFile(unitName, search_path):
    """
    Given a search path, find file
    """
    from os.path import exists, join, abspath
    from os import pathsep
    from string import split
    from SCons.Util import is_List, is_String

    (base, ext) = os.path.splitext(unitName)

    filename = base + (ext or ".pas")

    if is_List(search_path):
        paths = search_path
    else:
        paths = split(str(search_path), pathsep)

    for path in paths:
        fullname = abspath(join(path, filename))
        if exists(fullname):
            return fullname

    return None
