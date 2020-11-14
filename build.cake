#load "cake/functions.cake"
#load "cake/objects.cake"
#addin "nuget:?package=Cake.FileHelpers&version=3.3.0"

// https://download-cdn.getsync.com/stable/linux-x64/resilio-sync_x64.tar.gz

// --target Clean
var target = Argument<string>("target", "Default");
// --packages btsync-common,btsync-core,btsync-gui,btsync-user,btsync
var packageArgs = Argument<string>("packages", "btsync-core,btsync").Split(',');

System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

var availablePackageProfiles = new List<PackageProfile>()
    {
      new PackageProfile {
        Name = "btsync-common",
        Arches = "i386 amd64 armel armhf powerpc".Split()
      },
      new PackageProfile {
        Name = "btsync-core",
        Arches = "i386 amd64 armel armhf arm64".Split()
      },
      new PackageProfile {
        Name = "btsync-gui",
        Arches = "all".Split()
      },
      new PackageProfile {
        Name = "btsync-user",
        Arches = "all".Split()
      },
      new PackageProfile {
        Name = "btsync",
        Arches = "all".Split()
      }
    };

var packageProfiles = availablePackageProfiles
                      .Where(x => packageArgs.Contains(x.Name))
                      .ToList();

Task("Clean")
    .Does(() =>
{
    foreach(var package in packageProfiles)
    {
        Information($"Cleaning workspace of {package.Name}.");

        var path = PackageToPath(package.Name);
        Run("debuild", "--no-tgz-check -- clean", path);
        DeleteFiles($"./{package.Name}*.build");
        DeleteFiles($"./{package.Name}*.changes");
        DeleteFiles($"./{package.Name}*.deb");
        DeleteFiles($"./{package.Name}*.dsc");
        DeleteFiles($"./{package.Name}*.tar.gz");

        DeleteFiles($"./*.buildinfo");
        DeleteFiles($"./*.debian.tar.xz");
    }
});

Task("Build-Src")
    .IsDependentOn("Clean")
    .Does(() =>
{
    foreach(var package in packageProfiles)
    {
        Information($"Building source of {package.Name}.");
        var path = PackageToPath(package.Name);
        Run(path + "/debian/rules", "get-orig-source", path);
        Run("debuild", "-S -sa -uc -us", path);
    }
});

Task("Build-Deb")
    .IsDependentOn("Build-Src")
    .Does(() =>
{
    foreach(var package in packageProfiles)
    {
        Information($"Building Debian package for {package.Name}.");

        Information("Extracting source.");

        Run("bash", $"-c \"tar xvf {package.Name}_*.orig.tar.gz -C ./\"", "./");

        var path = PackageToPath(package.Name);
        foreach(var arch in package.Arches)
        {
                Information($"Building for arch {arch}.");
                var options = arch == "all"
                              ? "-uc -us -b"
                              : "-uc -us -b -a" + arch;
            Run("debuild", options, path);
        }
    }
});

Task("Default")
    .IsDependentOn("Build-Deb");

RunTarget(target);
