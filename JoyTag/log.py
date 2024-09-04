import os
from wsgilog import WsgiLog
class Log(WsgiLog):
    def __init__(self, application):
        dir_path = os.path.dirname(os.path.realpath(__file__))
        WsgiLog.__init__(
            self,
            application,
            logformat = '%(message)s',
            tofile = True,
            toprint = True,
            file = dir_path + "/logs.txt",
            interval = "s",
            backups = 1
            )