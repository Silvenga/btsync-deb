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

void BuildDebianPackage(string package, params string[] arches)
{
    Information("Extracting source.");
    var extractOptions = string.Format("-c \"tar xvf {0}_*.orig.tar.gz -C ./\"", package);
    Run("bash", extractOptions, "./");

    var path = PackageToPath(package);
    foreach(var arch in arches)
    {
            Information(string.Format("Building for arch {0}.", arch));
            var options = (arch == "all")
                                        ? "-uc -us -b"
                                        : "-uc -us -b -a" + arch;
        Run("debuild", options, path);
    }
}

void BuildDebianSrc(string package)
{
    var path = PackageToPath(package);
    Run(path + "/debian/rules", "get-orig-source", path);
    Run("debuild", "-S -sa -uc -us", path);
}
