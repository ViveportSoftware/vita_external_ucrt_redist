#addin "nuget:?package=Cake.Git&version=0.16.1"
#addin "nuget:?package=Cake.FileHelpers&version=2.0.0"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var configuration = Argument("configuration", "Debug");
var revision = EnvironmentVariable("BUILD_NUMBER") ?? Argument("revision", "9999");
var target = Argument("target", "Default");


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define git commit id
var commitId = "SNAPSHOT";

// Define product name and version
var product = "Htc.Vita.External.UCRT.Redist";
var companyName = "HTC";
var version = "0.9.0";
var semanticVersion = string.Format("{0}.{1}", version, revision);
var ciVersion = string.Format("{0}.{1}", version, "0");
var nugetTags = new [] {"htc", "vita", "external", "ucrt", "redist"};
var projectUrl = "https://github.com/ViveportSoftware/vita_external_ucrt_redist/";
var description = "HTC Vita external package: UCRT redistributable";

// Define copyright
var copyright = string.Format("Copyright © 2018 - {0}", DateTime.Now.Year);

// Define timestamp for signing
var lastSignTimestamp = DateTime.Now;
var signIntervalInMilli = 1000 * 5;

// Define path
var targetFileNameWin7X64 = "Windows6.1-KB3118401-x64.msu";
var targetFileNameWin7X86 = "Windows6.1-KB3118401-x86.msu";
var targetFileNameWin8X64 = "Windows8-RT-KB3118401-x64.msu";
var targetFileNameWin8X86 = "Windows8-RT-KB2999226-x86.msu";
var targetFileNameWin81X64 = "Windows8.1-KB3118401-x64.msu";
var targetFileNameWin81X86 = "Windows8.1-KB3118401-x86.msu";
var targetFileUrlWin7X64 = "https://download.microsoft.com/download/D/1/0/D107EB4D-2295-4127-A187-97FB290D7A3F/Windows6.1-KB3118401-x64.msu";
var targetFileUrlWin7X86 = "https://download.microsoft.com/download/6/1/D/61DE9D89-5A69-401A-B5BD-E74F1D6E1BD5/Windows6.1-KB3118401-x86.msu";
var targetFileUrlWin8X64 = "https://download.microsoft.com/download/8/E/3/8E3AED94-65F6-43A4-A502-1DE3881EA4DA/Windows8-RT-KB3118401-x64.msu";
var targetFileUrlWin8X86 = "https://download.microsoft.com/download/1/E/8/1E8AFE90-5217-464D-9292-7D0B95A56CE4/Windows8-RT-KB2999226-x86.msu";
var targetFileUrlWin81X64 = "https://download.microsoft.com/download/F/E/7/FE776F83-5C58-47F2-A8CF-9065FE6E2775/Windows8.1-KB3118401-x64.msu";
var targetFileUrlWin81X86 = "https://download.microsoft.com/download/5/E/8/5E888014-D156-44C8-A25B-CA30F0CCDA9F/Windows8.1-KB3118401-x86.msu";
var targetSha512sumFileNameWin7X64 = string.Format("{0}.sha512sum", targetFileNameWin7X64);
var targetSha512sumFileNameWin7X86 = string.Format("{0}.sha512sum", targetFileNameWin7X86);
var targetSha512sumFileNameWin8X64 = string.Format("{0}.sha512sum", targetFileNameWin8X64);
var targetSha512sumFileNameWin8X86 = string.Format("{0}.sha512sum", targetFileNameWin8X86);
var targetSha512sumFileNameWin81X64 = string.Format("{0}.sha512sum", targetFileNameWin81X64);
var targetSha512sumFileNameWin81X86 = string.Format("{0}.sha512sum", targetFileNameWin81X86);

// Define directories.
var distDir = Directory("./dist");
var tempDir = Directory("./temp");
var nugetDir = distDir + Directory(configuration) + Directory("nuget");
var tempOutputDir = tempDir + Directory(configuration);

// Define nuget push source and key
var nugetApiKey = EnvironmentVariable("NUGET_PUSH_TOKEN") ?? EnvironmentVariable("NUGET_APIKEY") ?? "NOTSET";
var nugetSource = EnvironmentVariable("NUGET_PUSH_PATH") ?? EnvironmentVariable("NUGET_SOURCE") ?? "NOTSET";


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Fetch-Git-Commit-ID")
    .ContinueOnError()
    .Does(() =>
{
    var lastCommit = GitLogTip(MakeAbsolute(Directory(".")));
    commitId = lastCommit.Sha;
});

Task("Display-Config")
    .IsDependentOn("Fetch-Git-Commit-ID")
    .Does(() =>
{
    Information("Build target: {0}", target);
    Information("Build configuration: {0}", configuration);
    Information("Build commitId: {0}", commitId);
    if ("Release".Equals(configuration))
    {
        Information("Build version: {0}", semanticVersion);
    }
    else
    {
        Information("Build version: {0}-CI{1}", ciVersion, revision);
    }
});

Task("Clean-Workspace")
    .IsDependentOn("Display-Config")
    .Does(() =>
{
    CleanDirectory(distDir);
    CleanDirectory(tempDir);
});

Task("Download-Dependent-Binaries")
    .IsDependentOn("Clean-Workspace")
    .Does(() =>
{
    CreateDirectory(tempOutputDir);

    var targetFileWin7X64 = tempOutputDir + File(targetFileNameWin7X64);
    DownloadFile(targetFileUrlWin7X64, targetFileWin7X64);
    FileWriteText(
            tempOutputDir + File(targetSha512sumFileNameWin7X64),
            string.Format(
                    "{0} *{1}",
                    CalculateFileHash(
                            targetFileWin7X64,
                            HashAlgorithm.SHA512
                    ).ToHex(),
                    targetFileNameWin7X64
            )
    );

    var targetFileWin7X86 = tempOutputDir + File(targetFileNameWin7X86);
    DownloadFile(targetFileUrlWin7X86, targetFileWin7X86);
    FileWriteText(
            tempOutputDir + File(targetSha512sumFileNameWin7X86),
            string.Format(
                    "{0} *{1}",
                    CalculateFileHash(
                            targetFileWin7X86,
                            HashAlgorithm.SHA512
                    ).ToHex(),
                    targetFileNameWin7X86
            )
    );

    var targetFileWin8X64 = tempOutputDir + File(targetFileNameWin8X64);
    DownloadFile(targetFileUrlWin8X64, targetFileWin8X64);
    FileWriteText(
            tempOutputDir + File(targetSha512sumFileNameWin8X64),
            string.Format(
                    "{0} *{1}",
                    CalculateFileHash(
                            targetFileWin8X64,
                            HashAlgorithm.SHA512
                    ).ToHex(),
                    targetFileNameWin8X64
            )
    );

    var targetFileWin8X86 = tempOutputDir + File(targetFileNameWin8X86);
    DownloadFile(targetFileUrlWin8X86, targetFileWin8X86);
    FileWriteText(
            tempOutputDir + File(targetSha512sumFileNameWin8X86),
            string.Format(
                    "{0} *{1}",
                    CalculateFileHash(
                            targetFileWin8X86,
                            HashAlgorithm.SHA512
                    ).ToHex(),
                    targetFileNameWin8X86
            )
    );

    var targetFileWin81X64 = tempOutputDir + File(targetFileNameWin81X64);
    DownloadFile(targetFileUrlWin81X64, targetFileWin81X64);
    FileWriteText(
            tempOutputDir + File(targetSha512sumFileNameWin81X64),
            string.Format(
                    "{0} *{1}",
                    CalculateFileHash(
                            targetFileWin81X64,
                            HashAlgorithm.SHA512
                    ).ToHex(),
                    targetFileNameWin81X64
            )
    );

    var targetFileWin81X86 = tempOutputDir + File(targetFileNameWin81X86);
    DownloadFile(targetFileUrlWin81X86, targetFileWin81X86);
    FileWriteText(
            tempOutputDir + File(targetSha512sumFileNameWin81X86),
            string.Format(
                    "{0} *{1}",
                    CalculateFileHash(
                            targetFileWin81X86,
                            HashAlgorithm.SHA512
                    ).ToHex(),
                    targetFileNameWin81X86
            )
    );
});

Task("Build-NuGet-Package")
    .IsDependentOn("Download-Dependent-Binaries")
    .Does(() =>
{
    CreateDirectory(nugetDir);
    var nugetPackVersion = semanticVersion;
    if (!"Release".Equals(configuration))
    {
        nugetPackVersion = string.Format("{0}-CI{1}", ciVersion, revision);
    }
    Information("Pack version: {0}", nugetPackVersion);
    var nuGetPackSettings = new NuGetPackSettings
    {
            Id = product,
            Version = nugetPackVersion,
            Authors = new[] {"HTC"},
            Description = description + " [CommitId: " + commitId + "]",
            Copyright = copyright,
            ProjectUrl = new Uri(projectUrl),
            Tags = nugetTags,
            RequireLicenseAcceptance= false,
            Files = new []
            {
                    new NuSpecContent
                    {
                            Source = "*.*",
                            Target = "content"
                    }
            },
            Properties = new Dictionary<string, string>
            {
                    {"Configuration", configuration}
            },
            BasePath = tempOutputDir,
            OutputDirectory = nugetDir
    };

    NuGetPack(nuGetPackSettings);
});

Task("Publish-NuGet-Package")
    .WithCriteria(() => "Release".Equals(configuration) && !"NOTSET".Equals(nugetApiKey) && !"NOTSET".Equals(nugetSource))
    .IsDependentOn("Build-NuGet-Package")
    .Does(() =>
{
    var nugetPushVersion = semanticVersion;
    if (!"Release".Equals(configuration))
    {
        nugetPushVersion = string.Format("{0}-CI{1}", ciVersion, revision);
    }
    Information("Publish version: {0}", nugetPushVersion);
    var package = string.Format("./dist/{0}/nuget/{1}.{2}.nupkg", configuration, product, nugetPushVersion);
    NuGetPush(
            package,
            new NuGetPushSettings
            {
                    Source = nugetSource,
                    ApiKey = nugetApiKey
            }
    );
});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build-NuGet-Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
