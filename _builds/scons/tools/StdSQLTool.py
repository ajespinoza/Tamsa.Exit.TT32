import os

def SQLScript(
                env
              , root
              , source
              , script_name = "create"
              , source_ext  = ".sql"
              , depend_ext  = ".dep"
              , target_dir  = "${BUILDTMP}/.sqlcmd"
              , output_ext  = ".log"
              ):

    target_dir = env.Dir(target_dir)
    create = source.File(script_name + source_ext)
    depend = source.File(script_name + depend_ext)
    
    depends = []
    if depend.exists():
        lines = open(depend.abspath).read().split()
        for line in lines:
            target = target_dir.Dir(line).File(script_name + output_ext)
            depends.append(target)
    
    output = target_dir.Dir(source.get_path(root)).File(script_name + output_ext)
    
    target = env.SQLBatch(
            target = output
        ,   source = create
        )
    
    env.Depends(target, depends)
    
    return target
    
def SQLObjects(env, root, source):
    targets = []
    for entry in os.listdir(source.abspath):
        entry = source.Entry(entry)
        if entry.isdir():
            targets.append(env.SQLObject(root, env.Dir(entry)))
    return targets

def SQLObject(env, root, source):
    return env.SQLScript(root, source)


def SQLSchema(env, root, source):
    targets = []
    
    targets.append(env.SQLScript(root, source))
    for entry in os.listdir(source.abspath):
        entry = source.Entry(entry)
        if entry.isdir():
            targets.append(env.SQLObjects(root, env.Dir(entry)))

    return targets

def SQLProject(env, root, source):

    targets = []
    root = env.Dir(root)
    source = env.Nodify(source, env.Dir)
   
    for item in source:
        item = root.Dir(item)
        targets.append(env.SQLScript(root, item))
        for entry in os.listdir(item.abspath):
            entry = item.Entry(entry)
            if entry.isdir():
                targets.append(env.SQLSchema(root, env.Dir(entry)))
                
    
    env.Alias('sql', targets)
    
def generate(env):
    env.AddMethod(SQLProject)
    env.AddMethod(SQLSchema)
    env.AddMethod(SQLScript)
    env.AddMethod(SQLObjects)
    env.AddMethod(SQLObject)

def exists(env):
    return 1
