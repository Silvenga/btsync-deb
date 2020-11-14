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
        Information(string.Format("Cleaning workspace of {0}.", package.Name));

        var path = PackageToPath(package.Name);
        Run("debuild", "-- clean", path);
        DeleteFiles(string.Format("./{0}*.build", package.Name));
        DeleteFiles(string.Format("./{0}*.changes", package.Name));
        DeleteFiles(string.Format("./{0}*.deb", package.Name));
        DeleteFiles(string.Format("./{0}*.dsc", package.Name));
        DeleteFiles(string.Format("./{0}*.tar.gz", package.Name));
    }
});

Task("Build-Src")
    .IsDependentOn("Clean")
    .Does(() =>
{
    foreach(var package in packageProfiles)
    {
        Information(string.Format("Building source of {0}.", package.Name));
        BuildDebianSrc(package.Name);
    }
});

Task("Build-Deb")
    .IsDependentOn("Build-Src")
    .Does(() =>
{
    foreach(var package in packageProfiles)
    {
        Information(string.Format("Building Debian package for {0}.", package.Name));
        BuildDebianPackage(package.Name, package.Arches);
    }
});

Task("Default")
    .IsDependentOn("Build-Deb");

RunTarget(target);
