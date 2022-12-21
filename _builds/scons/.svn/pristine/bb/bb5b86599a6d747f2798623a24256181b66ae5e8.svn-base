"""StdMSBuildTool
##################################################################
* StdMSBuildTool.py - Wrapper functions que construyen los builders
    para proyectos estandar MSBuild
##################################################################
 Created by: Ernesto Castro (ercastro@siderca.com) Sep. '07
 Created by: Leandro Conde  (leandor@gmail.com)    Dic. '07
       $Id:  $
##################################################################
"""
import sys
import os
import SCons.Util
import xml.dom.minidom
from SCons.Script.SConscript import SConsEnvironment # just do this once
from SCons.Script import *
from xml.dom.minidom import Node

def _recurse(node, searchTag):
    nodeList = []
    for child in node.childNodes:
        if child.nodeType == Node.ELEMENT_NODE:
            if child.tagName in searchTag:
                nodeList.append(child)
            else:
                nodeList.extend(_recurse(child, searchTag))

    return nodeList

def _getRerefenceNodes(csproj_dom):
    searchTag = ['Reference', 'ProjectReference']
    return _recurse(csproj_dom, searchTag)

def _getReferencesAliasName(nodeList, tag, hasExtension):
    _alias = []
    result = []

    for node in nodeList:
        _alias.extend(_recurse(node, tag))

    for alias in _alias:
        file = os.path.basename(alias.childNodes[0].nodeValue)
        (base, ext) = os.path.splitext(file)
        if hasExtension:
            result.append(base)
        else:
            result.append(file)

    return result

def _StdMSBuildTool(env
                  , source
                  , target              = None
                  , version_stamp       = False
                  , version_template    = 'AssemblyVersionInfo.Template.cs'
                  , workdir             = '.'
                  , release_file        = '${TMPDIR}/release.ini'
                  ):
    """
      _StdMSBuildTool: Produces all the builders necessary for an APRE standard visual studio project.
    """
    wd = env.Dir(workdir)
    version_template = wd.File(version_template)
    release_file = env.File(release_file)
    local = env.Clone()
    
    (root, ext) = os.path.splitext(source)
    if ext == '.sln':
        raise "Building solutions is not supported right now, please add a builder \
                for each single visual studio project"

    version_source = None
    if version_stamp or local.File(version_template).exists():
      version_source = local.VersionStamp(
          target = 'AssemblyVersionInfo.cs'
        , source = [
                release_file
              , version_template
              ]
        )


    dependencies = []
    doc = xml.dom.minidom.parse(source)
    nodes = _getRerefenceNodes(doc)
    dependencies.extend(_getReferencesAliasName(nodes, 'HintPath', True))
    dependencies.extend(_getReferencesAliasName(nodes, 'Name', False))

    create_asm = env.MSBuild(source)

    (base, ext) = os.path.splitext(os.path.basename(source))

    local.Alias(base, create_asm)
    if version_source: local.Depends(create_asm, version_source)
    if version_source: local.Depends('prepare-dotnet', version_source)
    local.Depends(Alias('compile'), create_asm)

    for dep in dependencies:
        local.Depends(create_asm, Alias(dep))
    
    if target:
        local.Depends(target, create_asm)
    
    return create_asm


def StdMSBuildTool(env
                  , source
                  , target = None
                  ):

    return _StdMSBuildTool(env
                  , source = source
                  , target = target
                  )


def generate(env):
    """Attaches the function Wrappers to the specified environment."""
    
    SConsEnvironment.StdMSBuildTool = StdMSBuildTool

def exists(env):
    return 1
