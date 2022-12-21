# -*- coding: iso-8859-1 -*-

def GetIgnoreList(client, path):
    try:
        return client.propget('svn:ignore', path).popitem()[1].splitlines()
    except:
        return []

def SetIgnoreList(client, path, values):
    client.propset('svn:ignore', '\n'.join(values), path)

class LogMessageCallback:
    def __init__(self, msg):
        self.msg = msg

    def __call__(self):
        return True, self.msg

def ssl_server_trust_prompt( trust_dict ):
    return True, 0, False

def _toRelativeUrl(self, base, reposlen = None, prefixlen = None):
    if isinstance(base, str):
        base = SplitURL(base, reposlen or self._reposlen, prefixlen or self._prefixlen)

    if self.stem == base.stem:
        return "^/%(location)s" % self
    else:
        return "/%(repos)s/%(location)s" % self

def SplitURL(URL, prefixlen = 0, reposlen = 1, **kwargs):
    import string
    from UserDict import UserDict

    class URLInfo:
        def __init__(self, **kwargs):
            self.__dict__.update(kwargs)

        def __getitem__(self, x):
            return getattr(self, x)

        def __str__(self):
            return __getitem__(self, "url")

    (scheme, temp) = URL.split('//', 2)

    parts = temp.split('/')
    server = parts.pop(0)
    repos = string.join(parts[0:reposlen], '/')
    del parts[0:reposlen]


    ## regla hard-coded para eliminar el sources del nombre del Branch
    if len(parts) > 0 and parts[0] == "sources":
        prefixlen = 1

    if len(parts) > 0 and prefixlen > 0:
        prefix = string.join(parts[0:prefixlen], '/')
        del parts[0:prefixlen]
    else:
        prefix = None

    length = 2
    if "trunk" in parts:
        tag = "unstable"
        kind = "trunk"
        index = parts.index("trunk")
        length = 1
        name = ""
        branch = "trunk"
    elif "branches" in parts:
        tag = "stable"
        kind = "branch"
        branch = "branches"
        index = parts.index("branches")
        name = parts[index + 1]
    elif "branch" in parts:
        tag = "stable"
        kind = "branch"
        branch = "branch"
        index = parts.index("branch")
        name = parts[index + 1]
    elif "sandbox" in parts:
        tag = "testing"
        kind = "sandbox"
        branch = "sandbox"
        index = parts.index("sandbox")
        name = parts[index + 1]
    elif "tags" in parts:
        tag = "special"
        kind = "tag"
        branch = "tags"
        index = parts.index("tags")
        name = parts[index + 1]
    else:
        tag = ""
        kind = ""
        branch = ""
        name = ""
        index = len(parts)

    module = string.join(parts[0:index], '/')
    path   = string.join(parts[index + length:], '/')

    fields = dict(
                    _URL        = "%(stem)s/%(location)s"
                ,   _ROOT       = "%(scheme)s//%(server)s"
                ,   _STEM       = "%(root)s/%(repos)s"
                ,   _LOCATION   = "%(module)s%(branch)s%(path)s"
                ,   _PREFIX     = "%(_prefix)s/"
                ,   _MODULE     = "%(prefix)s%(_module)s/"
                ,   _BRANCH     = "%(_branch)s/%(branch_path)s"
                ,   _scheme     = scheme
                ,   _server     = server
                ,   _repos      = repos
                ,   _prefix     = prefix
                ,   _module     = module
                ,   _tag        = tag
                ,   _name       = name
                ,   _kind       = kind
                ,   _path       = path
                ,   _branch     = branch
                )

    properties = dict(
                    kind        = lambda self: self._kind
                ,   scheme      = lambda self: self._scheme
                ,   server      = lambda self: self._server
                ,   repos       = lambda self: self._repos
                ,   tag         = lambda self: self._tag
                ,   name        = lambda self: self._name
                ,   path        = lambda self: '/' + self._path if self._path else ""
                ,   is_stable   = lambda self: self._tag == "stable"
                ,   is_tag      = lambda self: self._kind == "tags"
                ,   is_branch   = lambda self: self._kind == "branches"
                ,   is_trunk    = lambda self: self._kind == "trunk"
                ,   is_sandbox  = lambda self: self._kind == "sandbox"
                ,   branch_path = lambda self: self._name if self._name else ""
                ,   prefix      = lambda self: self._PREFIX % self if self._prefix else ""
                ,   branch      = lambda self: self._BRANCH % self if not self.is_trunk else "trunk"
                ,   root        = lambda self: self._ROOT % self
                ,   url         = lambda self: self._URL % self
                ,   stem        = lambda self: self._STEM % self
                ,   location    = lambda self: self._LOCATION % self
                ,   module      = lambda self: self._MODULE % self if self._module else ""
                )


    for k, v in kwargs.iteritems():
        if callable(v):
            properties[k] = v
        else:
            fields[k] = v

    result = URLInfo(**fields)
    result.__class__.__dict__.update(map(lambda (k, v): (k, property(v)), properties.iteritems()))
    result.__class__.Relative = _toRelativeUrl
    result._reposlen = reposlen
    result._prefixlen = prefixlen

    return result

def test():

    URL = r"http://forge.apre.siderca.ot/src/delphi/sources/sys/interrupciones/branch/3.xx/source/library/test/data"

    info = SplitURL(URL, reposlen = 2)

    print """
Root          = %(root)s
Scheme        = %(scheme)s
Server        = %(server)s
Repository    = %(repos)s
Prefix        = %(prefix)s
Module        = %(module)s
Branch        = %(branch)s
Tag           = %(tag)s
Name          = %(name)s
Kind          = %(kind)s
Path          = %(path)s
Location      = %(location)s
""" % info

    print 'Original URL: %s' % (URL)
    print 'Mask        : %s' % (info._URL)
    print 'Rebuilt  URL: %s' % (info.url)

    assert info.url == URL, "info.url != URL -> (%s != %s)" % (info.url, URL)

    info._scheme = "https:"
    info._server = "svn.apre.siderca.ot"
    info._repos  = "source"

    print 'Rebuilt  URL: %s' % (info.url)
    print 'Relative URL: %s' % (info.Relative("https://svn.apre.siderca.ot/source", reposlen = 1))

    info = SplitURL("https://svn.apre.siderca.ot/sil/branches/1.5.9-fixes/source", reposlen = 1)
    print info.Relative("https://svn.apre.siderca.ot/source")

    print """
Root          = %(root)s
Scheme        = %(scheme)s
Server        = %(server)s
Repository    = %(repos)s
Prefix        = %(prefix)s
Module        = %(module)s
Branch        = %(branch)s
Tag           = %(tag)s
Name          = %(name)s
Kind          = %(kind)s
Path          = %(path)s
Location      = %(location)s
""" % info


    info = SplitURL("https://svn.apre.siderca.ot/source/core/production/branches/4.xx", reposlen = 1)
    print info.Relative("https://svn.apre.siderca.ot/source")

    print """
Root          = %(root)s
Scheme        = %(scheme)s
Server        = %(server)s
Repository    = %(repos)s
Prefix        = %(prefix)s
Module        = %(module)s
Branch        = %(branch)s
Tag           = %(tag)s
Name          = %(name)s
Kind          = %(kind)s
Path          = %(path)s
Location      = %(location)s
""" % info



if __name__ == '__main__':
    test()
