#=================================================================================
# SConstruct
#
# Main project construction file
#
# Created
#      by: Ernesto Castro (ercastro@siderca.com) Sep. '07
#      by: Leandro Conde  (leandor@gmail.com)    Dec. '07
#=================================================================================

# First of all... we start a timer to profile a little bit the process of reading this script
import time
time.clock()

# Just initialize the environment.
env = Environment(tools = [])
env.SConscript('#_builds/scons/StdConstruct.scons', exports = 'env')

# Fifth tick...
tick = time.clock()
print 'Creating projects targets: %f' % tick

