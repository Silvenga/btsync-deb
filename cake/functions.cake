void Run(string exe, string args, string path = "")
{
    var settings = new ProcessSettings
    {
        Arguments = args,
        WorkingDirectory = path
    };
    var exitCode = StartProcess(exe, settings);
    if(exitCode != 0)
    {
        throw new Exception("Process did not succeed.");
    }
}

string PackageToPath(string package)
{
    return string.Format("./{0}", package);
}
