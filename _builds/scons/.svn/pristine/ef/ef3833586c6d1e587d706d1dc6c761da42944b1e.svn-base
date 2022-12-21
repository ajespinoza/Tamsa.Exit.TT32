# -*- coding: iso-8859-1 -*-

import pysvn
import string
import sys
import os
import svntools

SCHEME = "https:"
SERVER = "svnauto.tamsa.ot"
SRC = "source"
REL = "release"

def main(args):

    wc = os.path.abspath(args[1])

    client = pysvn.Client()
    client.callback_ssl_server_trust_prompt = svntools.ssl_server_trust_prompt

    root = svntools.SplitURL(client.info2(wc, recurse = False)[0][1]['URL'])
    plist = client.propget('svn:externals', wc, recurse = True)
	
    for (dir, value) in plist.iteritems():

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
                temp = path[index + 1:]
                if temp.isdigit():
                    path = path[:index]
                    revision = int(temp)

            if not path.startswith("http")   \
			        or not path.startswith("https"): ## es un PATH relativo
                if path.startswith("//"):
                    path = SCHEME + '//' + path
                elif path.startswith("/"):
                    path = SCHEME + '//'+ SERVER + path
                elif path.startswith("^"):
                    path = path.replace("^", '/'.join((SCHEME + '//'+ SERVER, SRC)))

            url = svntools.SplitURL(path)
            result.append((name, revision, url))

        value = []
        for (name, revision, url) in result:
            if not revision:
                 revision = client.info2(url.url, recurse=False)[0][1]['rev'].number
            value.append("%s%s %s" % (url.url, '@'+ str(revision) if revision else "", name))

        client.propset('svn:externals', '\n'.join(value) , dir)


if __name__ == "__main__":
    main(sys.argv)
