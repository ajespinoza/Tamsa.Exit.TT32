"""version
##################################################################
* version.py - Builder para leer la version del subversion
##################################################################
 Created by: Ernesto Castro (ercastro@siderca.com) Sep. '07
       $Id:  $
##################################################################
"""
import sys
import pysvn
import time
import datetime
import os
import SCons.Util
import SCons.Builder
from SCons.Script.SConscript import SConsEnvironment # just do this once
import SCons
import win32api
import win32con
from string import Template


properties = {
        "svn_version_major"     :   0
        , "svn_version_minor"   :   0
        , "svn_project_release" :   0
        , "svn_commit_revision" :   0
        , "svn_commit_time"     :   datetime.datetime.today()
        , "svn_revision"        :   0
        , "svn_commit_author"   :   'none'
        , "svn_url"             :   ''
        , "svn_project_major"   :   '$svn_version_major'
        , "svn_project_minor"   :   '$svn_version_minor'
        , "svn_project_build"   :   '$svn_commit_revision'
        , "version_major"       :   '$svn_version_major'
        , "version_minor"       :   '$svn_version_minor'
        , "version_release"     :   '$svn_project_release'
        , "version_build"       :   '$svn_commit_revision'
        , "version_short"       :   '${version_major}.${version_minor}'
        , "version_long"        :   '${version_major}.${version_minor}.${version_release}.${version_build}'
        , "product_version"     :   '${version_major}.${version_minor}'
        , "product_build"       :   '${version_release}.${version_build}'
        , "comma_version"       :   '${version_major},${version_minor},${version_release},${version_build}'
        , "version_tag"         :   '${version_major}${version_minor}'
    }

def _SvnGetVersion(env, source):
    """
    Action to read version info from a SVN dir define some env variables out of it
    """

    data = dict(properties)

    try:
        if SCons.Util.is_Sequence(source):
            source = source[0]

        client = pysvn.Client()
        
        entry = client.info(source)

        try:
            prop = client.propget("proj:version", source, recurse=False)
            if not prop:raise Exception("Se debe setear la property 'proj:version=x.yy' en el root el Working Copy! Donde 'x' es el version mayor e 'yy' es el version menor")
        except Exception, ex:
            print >>sys.stderr, str(ex)
            print >>sys.stderr, "Asumo que la version es '0.00'"
            prop = {'.': '0.00'}

        (major, minor) = [int(p) for p in prop.popitem()[1].split(".")]

        revision = entry.commit_revision.number
        
        revision_high = int(revision / 10000)
        revision_low = int(revision % 10000)
        
        data["svn_revision"] = revision
        data["svn_version_major"] = major
        data["svn_version_minor"] = minor
        data["svn_project_release"] = revision_high
        data["svn_commit_revision"] = revision_low
        data["svn_commit_time"] = datetime.datetime.fromtimestamp(entry.commit_time)
        data["svn_commit_author"] = entry.commit_author
        data["svn_url"] = entry.url

    except Exception, error:
        print >>sys.stderr, "ERROR: ", error

    return data

def FileReadVersion(env, source):
    """
    Action to read version info from a SVN dir define some env variables out of it
    """
    try:
        if SCons.Util.is_Sequence(source):
            source = source[0]

        if not SCons.Util.is_String(source):
            source = str(source.dir)

        client = pysvn.Client()
        entry = client.info(source)

        try:
            prop = client.propget("proj:version", root, recurse=False)
            if not prop:raise Exception("Se debe setear la property 'proj:version=x.yy' en el root el Working Copy! Donde 'x' es el version mayor e 'yy' es el version menor")
        except Exception, ex:
            print >>sys.stderr, str(ex)
            print >>sys.stderr, "Asumo que la version es '0.00'"
            prop = {'.': '0.00'}

        (major, minor) = [int(p) for p in prop.popitem()[1].split(".")]

        revision = entry.commit_revision.number

        revision_high = int(revision / 10000)
        revision_low = int(revision % 10000)

        env["svn_revision"] = revision
        env["svn_version_major"] = major
        env["svn_version_minor"] = minor
        env["svn_project_major"] = major
        env["svn_project_minor"] = minor
        env["svn_project_release"] = revision_high
        env["svn_commit_revision"] = revision_low
        env["svn_project_build"] = entry.commit_revision.number
        env["svn_commit_time"] = datetime.datetime.fromtimestamp(entry.commit_time)
        env["svn_commit_author"] = entry.commit_author
        env["svn_url"] = entry.url
        env["version_major"] = major
        env["version_minor"] = minor
        env["version_release"] = release
        env["version_build"] = entry.commit_revision.number
        env["version_short"] = '${version_major}.${version_minor}'
        env["version_long"] = '${version_major}.${version_minor}.${version_release}.${version_build}'
        env["product_version"] = '${version_major}.${version_minor}'
        env["product_build"] = '${version_release}.${version_build}'
        env["comma_version"] = '${version_major},${version_minor},${version_release},${version_build}'
        env["version_tag"] = '${version_major}${version_minor}'

    except Exception, error:
        print >>sys.stderr, "ERROR: ", error


def _SvnReadVersion(env, source):
    """
    Action to read version info from a SVN dir and define some env variables out of it
    """
    data = _SvnGetVersion(env, source)
    for (key, value) in data.iteritems():
        env[key]= data[key]
    
def _VersionReadAction(target, source, env):
    """
    Action to read version info from a SVN dir and writint it out to a file
    """

    try:
        # Work around... este builder siempre saca la version del directorio actual...
        # Si dejo que se pueda explicitar el directorio del cual tomar la
        # info de version se genera una dependencia ciclica cuando se invoca
        # por ejemplo: env.Version('_temp/release.ini', '.')
        root = str(source[0].dir)
        targ = str(target[0])

        client = pysvn.Client()
        entry = client.info(root)

        if os.path.exists(targ):
            os.remove(targ)

        try:
            prop = client.propget("proj:version", root, recurse=False)
            if not prop:raise Exception("Se debe setear la property 'proj:version=x.yy' en el root el Working Copy! Donde 'x' es el version mayor e 'yy' es el version menor")
        except Exception, ex:
            print >>sys.stderr, str(ex)
            print >>sys.stderr, "Asumo que la version es '0.00'"
            prop = {'.': '0.00'}

        (major, minor) = [int(p) for p in prop.popitem()[1].split(".")]

        revision = entry.commit_revision.number

        revision_high = int(revision / 10000)
        revision_low = int(revision % 10000)

        filedata = []
        filedata.append("svn_revision=" + str(revision) + "\n")
        filedata.append("svn_version_major=" + str(major) + "\n")
        filedata.append("svn_version_minor=" + str(minor) + "\n")
        filedata.append("svn_project_major=" + str(major) + "\n")
        filedata.append("svn_project_minor=" + str(minor) + "\n")
        filedata.append("svn_project_release=" + str(revision_high) + "\n")
        filedata.append("svn_commit_revision=" + str(revision_low) + "\n")
        filedata.append("svn_commit_time=" + str(datetime.datetime.fromtimestamp(entry.commit_time)) + "\n")
        filedata.append("svn_commit_author=" + entry.commit_author + "\n")
        filedata.append("svn_url=" + entry.url + "\n")

        try:
            fp = open(targ, "w")
            fp.writelines(filedata)
            fp.close()
        except:
            print >>sys.stderr, "No se pudo abrir el destino para escritura"

    except Exception, error:
        print >>sys.stderr, "ERROR: ", error


def _VersionStampAction(target, source, env):
    """
    Action to stamp version info into version.rc
    """

    try:

        version_info = str(source[0])
        version_template = str(source[1])
        targ = str(target[0])

        if os.path.exists(targ):
            os.remove(targ)

        try:
            info_fp = open(version_info, "r")
            template_fp = open(version_template, "r")
            local = env.Clone()
            try:

                # Genero el dictionary para substituir en el template
                replace_dict = local.__dict__
                replace_dict['now'] = str(datetime.datetime.now())

                for line in info_fp:
                    line = line.strip('\n')
                    __temp = line.split('=')
                    replace_dict[__temp[0]] = __temp[1]

                # Genero un string con todo el contenido del archivo template
                template_string = ''
                for line in template_fp: template_string = template_string + line

            finally:
                info_fp.close()
                template_fp.close()
                
        except:
            print >>sys.stderr, "ERROR: No se pudo abrir alguno de los archivos: info = '%s', template = '%s' " % (version_info, version_template)
            raise

        _template = Template(template_string)
        _version_rc = _template.substitute(replace_dict)

        output_fp = open(targ, "w")
        try:
            output_fp.writelines(_version_rc)
        finally:
            output_fp.close()

    except Exception, error:
        print >>sys.stderr, "ERROR: ", error
        raise

def generate(env):
    """
    Add Builders for Version reading and stamping.
    """
    
    VersionRead = SCons.Builder.Builder(
                                        action = _VersionReadAction,
                                        source_factory = SCons.Node.FS.Entry
									   )
    
    SvnRead = SCons.Builder.Builder(
                                        action = _VersionStampAction,
                                        source_factory = SCons.Node.FS.Entry
									   )
		
    VersionStamp = SCons.Builder.Builder(
                                        action = _VersionStampAction,
                                        target_factory = SCons.Node.FS.File,
                                        source_factory = SCons.Node.FS.File
									   )
		
    SConsEnvironment.SvnGetVersion = _SvnGetVersion
    SConsEnvironment.SvnReadVersion = _SvnReadVersion
    
    env.InstallBuilder('VersionStamp', VersionStamp, ['VerStamp', 'Stamp'])
    env.InstallBuilder('VersionRead', VersionRead, ['VerRead'])

    _SvnReadVersion(env, env.Dir('#').abspath)

def exists(env):
    return 1
