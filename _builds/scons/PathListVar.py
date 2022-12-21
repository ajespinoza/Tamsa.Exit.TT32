import os
import ListVar

class PathListVar(ListVar.ListVar):
    
    def __init__(self, initlist = [], separator = os.pathsep, prefix = None, suffix = None):
        ListVar.ListVar.__init__(self, initlist = initlist, separator = separator, prefix = prefix, suffix = suffix)
