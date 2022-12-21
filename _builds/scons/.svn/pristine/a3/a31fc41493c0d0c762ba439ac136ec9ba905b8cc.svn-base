import sys
import os
import time
import datetime
import re
import string
import fnmatch
import externalsfreeze
from optparse import OptionParser

def getOptions():
    op = OptionParser()
    op.add_option('-F', '--force', action="store_true", dest="force", help="Fuerza que se haga un release sin importar el mensaje de log del ultimo commit", default=False)
    return op.parse_args()

class GetLogMessage:
    def __init__(self, msg):
        self.msg = msg

    def __call__(self):
        return True, self.msg

def ssl_server_trust_prompt( trust_dict ):
    return True, 0, False

def spliturl(URL):

    temp = URL.split('//', 2)

    parts = temp[1].split('/')

    repos = string.join(parts[0:2], '/')

    if "trunk" in parts:
        tag = "unstable"
        kind = "trunk"
        index = parts.index("trunk", 2)
        name = "trunk"
    elif "branches" in parts:
        tag = "stable"
        kind = "branch"
        index = parts.index("branches", 2)
        name = parts[index + 1]
    elif "sandbox" in parts:
        tag = "experimental"
        kind = "sandbox"
        index = parts.index("sandbox", 2)
        name = parts[index + 1]
    else:
        tag = "unknown"
        kind = "unknown"
        name = "unknown"
        index = len(parts)-3

    module = string.join(parts[2:index], '/')
    branch = string.join(parts[index:], '/')

    return {  'repos'   : repos
            , 'module'  : module
            , 'branch'  : branch
            , 'tag'     : tag
            , 'name'    : name
            , 'kind'    : kind
            }

def IndentLogLines(message, level = 1):

    indent = ' ' * level
    lines = message.splitlines()
    if len(lines) == 0:
        return []

    logs = []
    logs.append('%s* %s' % (indent, lines[0].lstrip()))

    indent = ' ' * (level + 1)
    for line in lines[1:]:
        if not line.strip(): continue
        if '*' not in line[:5]:
            line = "* '''" + line + "'''"
        else:
            index = line.find('*', 0, 5)
            if index < 0: index = 0
            line = ' ' * index + line.lstrip()
        logs.append('%s%s' % (indent, line))

    return logs

try:
    print >>sys.stdout, "[Release script] starting ..."

    import pysvn

    opts, args = getOptions()

    publish_tag = '#release'
    force_publish_tag = '#forcerelease'

    wc_path = args[0] if len(args) != 0 else '.'

    release_dir = 'release'
    release_path = os.path.normpath(os.path.join(wc_path, release_dir))

    assert os.path.exists(release_path), "ASSERT: Release path %s doesn't exists" % release_path

    client = pysvn.Client()
    
    client.callback_ssl_server_trust_prompt = ssl_server_trust_prompt
    
    project_root = client.info2(wc_path, recurse = False)[0][1]['URL']
    project_url = spliturl(project_root)

    last_rev = client.info2(
                    project_root
                    , revision=pysvn.Revision(pysvn.opt_revision_kind.head)
                    , recurse=False
                    )[0][1]['last_changed_rev'].number

    print >>sys.stdout, "[Release script] Last source revision is: %d" % last_rev

    log = client.log(
                    project_root
                    , revision_start = pysvn.Revision( pysvn.opt_revision_kind.number, last_rev)
                    , revision_end = pysvn.Revision( pysvn.opt_revision_kind.number, last_rev)
                    , discover_changed_paths=False
                    , strict_node_history=True
                    , limit=0
                    )[0]

    tag_author = log['author']
    tag_date = time.strftime("%Y-%m-%dT%H:%M:%S.000Z", datetime.datetime.utcfromtimestamp(log['date']).timetuple())
    tag_message = log['message']
    tag_revision = log['revision'].number

    kw_regex = re.compile('.*' + force_publish_tag + '.*', re.IGNORECASE)
    force_release = opts.force or (re.search(kw_regex, (tag_message)) != None)

    kw_regex = re.compile('.*' + publish_tag + '.*', re.IGNORECASE)
    isReleasing = (force_release or (re.search(kw_regex, (tag_message)) != None))

    try:
        release_root = client.info2(release_path, recurse = False)[0][1]['URL']
    except Exception, ex:
        print >>sys.stderr, str(ex)
        print >>sys.stderr, "No version info in release path, maybe you are missing the svn:externals prop???"
        if isReleasing:
            raise Ex
        else:
            exit()


    if isReleasing:
        entry = client.info(wc_path)

        try:
            prop = client.propget("proj:version", wc_path, recurse = False)
            if not prop:
                raise Exception("Se debe setear la property 'proj:version=x.yy' en el root el Working Copy! Donde 'x' es el version mayor e 'yy' es el version menor")
        except Exception, ex:
            print >>sys.stderr, str(ex)
            print >>sys.stderr, "Asumo que la version es '0.00'"
            prop = {'.': '0.00'}

        (major, minor) = [int(p) for p in prop.popitem()[1].split(".")]
        revision = entry.commit_revision.number
        revision_high = int(revision / 10000)
        revision_low = int(revision % 10000)

        version = '%d.%d.%d.%d' % (major, minor, revision_high, revision_low)

        try:
            last_commit_rev = int(client.propget('revision', release_path).values()[0])
        except:
            last_commit_rev = 0

        if (last_commit_rev >= last_rev) and not force_release:
            print "[Release script] Up to date... nothing to be done"
            exit(0)

        print >>sys.stdout, "[Release script] Last commited revision is: %d" % last_commit_rev

        try:
            trac = client.propget("trac:project", wc_path, recurse = False).popitem()[1]
        except Exception, ex:
            trac = 'all'

        print >>sys.stdout, "[Release script] Generating ChangeLog.txt"
        loglist = [];
        loglist.append('')
        loglist.append('--------')
        loglist.append('= [%s %s] branch: %s (%s) =' % (project_root, project_url['module'], project_url['branch'], version))
        loglist.append('')
        loglist.append(' * Trac: %s:source:/' % trac)
        loglist.append(' * Author: %s' % tag_author)
        loglist.append(' * Date: %s' % tag_date)
        loglist.append(' * Message:')
        loglist.extend(IndentLogLines(tag_message, 2))
        loglist.append('')
        loglist.append('== Change Log == ')
        loglist.append('')

        fulllog = client.log(
                        project_root
                        , revision_start = pysvn.Revision( pysvn.opt_revision_kind.number, last_commit_rev)
                        , revision_end = pysvn.Revision( pysvn.opt_revision_kind.number, last_rev)
                        , discover_changed_paths = False
                        , strict_node_history = True
                        , limit = 0
                        )

        for pylog in reversed(fulllog):
            loglist.append('==== %s:r%d Author: %s Date: %s ====' % (trac, pylog['revision'].number, pylog['author'], str(datetime.datetime.fromtimestamp(pylog['date']))))
            try:
                loglist.extend(IndentLogLines(pylog['message']))
            except KeyError:
                loglist.extend(IndentLogLines('ERROR: Could not read message property from svn log.'))

        logname = os.path.join(release_path, 'ChangeLog.txt')
        try:
            logfile = open(logname, 'r')
            loglist.extend(logfile.readlines())
            logfile.close()
        except:
            print "[Release script] No previous ChangeLog file"

        logfile = open(logname, 'w')
        for line in loglist:
            try:
                logfile.write(line.encode('ascii', 'backslashreplace').rstrip('\n') + '\n')
            except UnicodeDecodeError,Ex:
                logfile.write('ERROR: Could not encode line!!!\n')
        logfile.close()

        print >>sys.stdout, "[Release script] Adding new release files ..."
        print >>sys.stdout, "release_path [%s]" % (release_path)		
        client.add(release_path, recurse = False, force = True)

        for targetdir in os.listdir(release_path):
            if targetdir[0] == '.':
                continue

            target = os.path.join(release_path, targetdir)

            if not os.path.isdir(target):
                continue

            client.add(target, recurse = False, force = True, ignore = True)
            for item in os.listdir(target):
                if item[0] == ".":
                    continue

                path = os.path.join(target, item)
                if os.path.isdir(path):
                    continue

                try:
                    ignore_list = client.propget('svn:ignore', target).popitem()[1].splitlines()
                except:
                    ignore_list = []

                if not '*.vshost.*' in ignore_list: ignore_list.append('*.vshost.*')
                if targetdir.lower() != "debug":
                    if not '*.map' in ignore_list: ignore_list.append('*.map')
                    if not '*.rsm' in ignore_list: ignore_list.append('*.rsm')
                    if not '*.pdb' in ignore_list: ignore_list.append('*.pdb')
                    client.propset('svn:ignore', '\n'.join(ignore_list), target)
                    if any(map(lambda x: fnmatch.fnmatch(item, x), ignore_list)):
                        continue

                client.add(path, recurse = False, force = True, ignore = True)
                client.propset('info:file-version',  version, path)
                client.propset('info:file-date',  tag_date, path)

        client.add(release_path, recurse = True, force = True, ignore = True)
        client.propset('svn:mime-type', 'text/x-trac-wiki', logname)
        client.propset('svn:eol-style', 'native', logname)

        print >>sys.stdout, "[Release script] Updating release directory ..."
        client.update(release_path)
        client.propset('author', tag_author, release_path)
        client.propset('date', tag_date, release_path)
        client.propset('revision', str(tag_revision), release_path)
        print >>sys.stdout, "[Release script] Commiting changes ..."
        client.checkin(release_path, tag_message, True)
        print >>sys.stdout, "[Release script] New version committed."

        branch_re = re.compile('.*/branches/.*', re.IGNORECASE)
        
        if branch_re.match(project_root):
            externalsfreeze.main(sys.argv)
        
		#RELEASE TAGS Creation
        if branch_re.match(release_root):
            relurl = spliturl(release_root)
            parts = release_root.split('/branches/')
            tag_path = '%s/tags/%s' % (parts[0], version)
            try:
                extprop = client.propget('svn:externals' , wc_path + "/_temp/_bin" )

                if len(extprop) > 0:
					print >>sys.stdout, "El proyecto tiene externals en /_temp/_bin y se setean en el repositorio release, de la rama externals"
					client.propset('svn:externals', extprop.values()[0], release_path + "/externals", revision=pysvn.Revision( pysvn.opt_revision_kind.head ))
					#Agregada la linea de abajo temporal para poner externals al branch temporal en el SVN				
					client.checkin(release_path, "Se hace commit temporal del release\branches con externals")

                client.callback_get_log_message = GetLogMessage("[TAGGED] %s: %s -> tags/%s\n * Tag generado para la version %s" % (relurl['module'], relurl['branch'], version, version))
                print >>sys.stdout, "Antes de Copy src=release_path [%s], dest=tag_path [%s]" % (release_path, tag_path)
#Script Mariano. Copia la carpeta release que se encuentra en la WC al repositorio https://server/.../Modulo/tags/w.x.y.z
                #client.copy(release_path, tag_path)
#Script Original. Copia del repositorio https://../Modulo/branches/1.xx al repositorio https://server/.../Modulo/tags/w.x.y.z
                client.copy(release_root, tag_path)
                print >>sys.stdout, "Despues de Copy [%s], tag [%s]" % (release_path, tag_path)

                client.callback_get_log_message = None
                client.revpropset('svn:date', tag_date, tag_path, revision=pysvn.Revision( pysvn.opt_revision_kind.head ))

#Agregadas las lineas de abajo temporal para quitar externals al branch temporal en el SVN
                if len(extprop) > 0:
					print >>sys.stdout, "Quitando los externals del repositorio release, de la rama externals en branches. Ya se copio en tags/revision"
					client.propdel('svn:externals', release_path+"/externals", revision=pysvn.Revision( pysvn.opt_revision_kind.head ))
					client.checkin(release_path,"Se hace commit para eliminar externals temporales de release\branches")				

            except IndexError:
                print >>sys.stdout, "[Release script] Without externals."

            print >>sys.stdout, "RELEASE Tags created succesfully ..."

		#SOURCE TAGS Creation
        if branch_re.match(project_root):
            proj_parts = project_root.split('/branches/')
            proj_tag_path = '%s/tags/%s' % (proj_parts[0], version)

            client.callback_get_log_message = GetLogMessage("[TAGGED] %s: %s -> tags/%s\n * Tag generado para la version %s" % (project_url['module'], project_url['branch'], version, version))
            client.copy(wc_path, proj_tag_path)
            client.callback_get_log_message = None
            client.revpropset('svn:date', tag_date, proj_tag_path, revision=pysvn.Revision( pysvn.opt_revision_kind.head ))

            print >>sys.stdout, "SOURCE Tags created succesfully ..."
            client.revpropset('svn:date', tag_date, release_root, revision=pysvn.Revision( pysvn.opt_revision_kind.head ))

        print >>sys.stdout, "[Release script] terminated successfully."
    else:
        print >>sys.stdout, "[Release script] release not triggered: #RELEASE keyword missing."

    exit(0)

except Exception, error:
    print >>sys.stderr, error
    exit(1)
