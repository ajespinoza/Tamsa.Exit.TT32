# -*- coding: iso-8859-1 -*-

#-------------------------------------------------------------------------------
# Name:        parse.py
# Purpose:     
#-------------------------------------------------------------------------------
#!/usr/bin/env python

"""
Implementa un simple parseo de un stream texto usando para ello el modulo estandar shlex
"""

import shlex
import os

class TokenError(Exception):
    """
    Excepción levantada por el parser cuando falla un check, i.e. cuando el token esperado es distinto al token actual.
    """
    pass

class Parser:
    """
    Implementa una simple encapsulación del parser shlex que mantiene estado actual
        y permite ir evaluando la entrada sin consumir forzozamente el token actual.
    """
    def __init__(self, filename):
        """
        Construcción del parser dado el nombre de un archivo para leer
        """
        assert filename and os.path.exists(filename), "filename: '%s' doesn't exists" % (filename)
        self._tok = None
        self._prev = None
        self.FileName = filename
        self.file = open(filename, 'r')
        self._parser = shlex.shlex(self.file, filename)
        self._parser.commenters = '//'
        self.Next()

    @property
    def current(self):
        """
        Devuelve la Token actual. No consume tokens de la entrada.
        """
        return self._tok

    @property
    def previous(self):
        """
        Devuelve la Token actual. No consume tokens de la entrada.
        """
        return self._prev

    @property
    def eof(self):
        return self._tok == self._parser.eof

    def Check(self, expectedToken, doAdvance = True, caseInsensitive = True):
        """
        Revisa si el token actual es igual al valor esperado y si no lo es lanza una excepción.
            Opcionalmente si se especifica doAdvance en True, se consume el token actual y se avanza al proximo
        """

        token = self._tok 
        if caseInsensitive:
            expectedToken = expectedToken.lower()
            token = token.lower() 
            
        if token != expectedToken:
            raise TokenError, "Input stream error (file: '%s' - line: %d): Expected '%s' but '%s' found" % (self.FileName, self._parser.lineno, expectedToken, self._tok)

        if doAdvance:
            self.Next()

        return self

    def Next(self, returnPrevious = True):
        """
        Se consume el siguiente token de la entrada y se memoriza en una variable local.
        Se retorna éste o el anterior, dependiendo del valor del parametro returnPrevious
        """

        self._prev = self._tok
        self._tok = self._parser.get_token()

        return self._prev if returnPrevious else self._tok

    def Advance(self):
        self.Next()
        return self
    
    def Match(self, sequence):

        tokens = list(sequence)
        saved = []
        
        for tk in sequence:
            if self.current != tk:
                break
            tokens.pop(0) #- Elimino el proximo del top
            assert self.Next() == tk, "Something's wrong: Next() didn't return '%s'" % (tk)
            saved.append(tk) #- lo salvo por si hay que resetear el match

        if len(tokens) == 0:
            return True
    
        if len(saved) > 0:
            for tk in reversed(saved):
                self.Undo(tk)
            
        return False

    def Undo(self, tok):
        self._parser.push_token(self._tok)
        self._tok = self._prev
        self._prev = tok
