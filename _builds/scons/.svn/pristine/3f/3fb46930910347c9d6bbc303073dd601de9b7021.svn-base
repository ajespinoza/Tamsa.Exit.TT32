import string
import UserList

class ListVar(UserList.UserList):
    """
    Clase que sirve para mantener una lista de elementos que cuando se necesita expandir
      utiliza 'separator', 'prefix' y 'suffix' para generar un string resultante
    """

    def __init__(self, initlist = [], separator = None, prefix = None, suffix = None):
        UserList.UserList.__init__(self, initlist)
        self._separator = separator or " "
        self._prefix = prefix or ""
        self._suffix = suffix or ""

    def toStr(self, prefix = None, suffix = None, separator = None):
        prefix = prefix or self._prefix
        suffix = suffix or self._suffix
        separator = separator or self._separator
        items = map(lambda x: prefix + str(x) + suffix, self.data)
        return string.join(items, separator)

    def __semi_deepcopy__(self):
        result = self.__class__(initlist = self, separator = self._separator, prefix = self._prefix, suffix = self._suffix)
        return result

    def __str__(self):
        return self.toStr()

    def __repr__(self):
        return self.toStr()

