using Semver;

public class PackageProfile
{
    public string Name { get; set; }
    public string[] Arches { get; set; }
}

public class Release
{
    public SemVersion Version { get; set; }
    public string Changes { get; set; }
}
