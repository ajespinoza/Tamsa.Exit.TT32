# -*- coding: iso-8859-1 -*-

import pysvn
import string
import sys
import os
import svntools

SCHEME = "https"
SERVER = "https://svn.apre.siderca.ot"
SRC = "source"
REL = "release"

def main(args):
    wc = os.path.abspath(args[1])
    client = pysvn.Client()

    root = svntools.SplitURL(client.info2(wc, recurse = False)[0][1]['URL'])
    externals = client.propget('svn:externals', wc, recurse = True)

    for (dir, value) in externals.iteritems():

        result = []
        externals = value.strip('\n').split('\n')

        for ex in externals:
            parts = ex.split()

            if len(parts) == 0: continue
            left = parts[0].strip()
            revision = None

            if left.startswith("https")         \
                    or left.startswith("http")  \
                    or left.startswith("^")     \
                    or left.startswith("/")     \
                    or left.startswith("//")    \
                    or left.startswith(".."):
                path = left
                name = parts[-1].strip()

            else:
                path = parts[-1].strip()
                name = parts[0].strip()

            if len(parts) == 3:
                revspec = parts[1].strip()
                assert revspec[0] == '-', "revision must start with '-'"
                assert revspec[1] == 'r', "invalid revision specifier"
                revision = int(revspec[2:])

            elif '@' in path:
                index = path.index('@')
                temp = path[index + 1:-1]
                if temp.isdigit():
                    path = path[:index]
                    revision = int(temp)

            if not path.startswith("http"): ## es un PATH relativo
                if path.startswith("//"):
                    path = string.join((SCHEME, path))
                elif path.startswith("/"):
                    path = string.join((SERVER, path))
                elif path.startswith("^"):
                    path = path.replace("^", string.join((SERVER, SRC)))

            info = svntools.SplitURL(path)

            if info.repos == "n2":
                info.repos = SRC

            if info.repos == SRC:
                ## Reemplazo de PATHs cambiados
                if info.prefix.startswith("sources"):
                    info.prefix = ""

                module = info.module

                module = module.replace("@lib/sid/", "library/")
                module = module.replace("@lib/", "library/")
                module = module.replace("sv/", "service/")
                module = module.replace("sys/", "core/")
                module = module.replace("sv/", "service/")
                module = module.replace("core/escuadras", "core/team")
                module = module.replace("core/produccion", "core/production")
                module = module.replace("core/bloqueos", "core/blocking")
                module = module.replace("core/alarmas", "core/alarm")
                module = module.replace("core/interrupciones", "core/interruption")
                module = module.replace("core/maquinas", "core/maquinas")
                module = module.replace("core/producto", "core/product")
                module = module.replace("core/usuarios", "core/user")
                module = module.replace("util/builds", "service/build")

                info._module = module

            if info.scheme == "http:":
                info.scheme = "https:"

            value = "%s%s %s" % (info.Relative(root, reposlen = 1), '@'+ str(revision) if revision else "", name)

            result.append(value)

        print "externals in dir: %s " % dir

        #for item in result:
        #    print " - " + item

        client.propset('svn:externals', '\n'.join(result), dir)

if __name__ == "__main__":
    main(sys.argv)
