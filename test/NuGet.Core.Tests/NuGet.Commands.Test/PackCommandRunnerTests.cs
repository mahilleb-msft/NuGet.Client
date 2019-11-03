// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NuGet.Common;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.Commands.Test
{
    public class PackCommandRunnerTests
    {
        [Fact]
        public void BuildPackage_WithDefaultExcludes_ExcludesDefaultExcludes()
        {
            using (var test = ExcludesTest.Create())
            {
                var args = new PackArgs()
                {
                    CurrentDirectory = test.CurrentDirectory.FullName,
                    Exclude = Enumerable.Empty<string>(),
                    Logger = NullLogger.Instance,
                    Path = test.NuspecFile.FullName
                };
                var runner = new PackCommandRunner(args, createProjectFactory: null);

                runner.BuildPackage();

                using (FileStream stream = test.NupkgFile.OpenRead())
                using (var package = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    Assert.Equal(0, package.Entries.Count(entry => entry.Name == "b.nupkg"));
                    Assert.Equal(0, package.Entries.Count(entry => entry.Name == ".c"));

                    Assert.Equal(1, package.Entries.Count(entry => entry.Name == "d.e"));
                    Assert.Equal(1, package.Entries.Count(entry => entry.Name == "h.nuspec"));
                }
            }
        }

        private sealed class ExcludesTest : IDisposable
        {
            private readonly TestDirectory _testDirectory;

            internal DirectoryInfo CurrentDirectory { get; }
            internal FileInfo NupkgFile { get; }
            internal FileInfo NuspecFile { get; }
            internal DirectoryInfo PackRootDirectory { get; }

            private ExcludesTest(
                TestDirectory testDirectory,
                DirectoryInfo packRootDirectory,
                DirectoryInfo currentDirectory,
                FileInfo nuspecFile,
                FileInfo nupkgFile)
            {
                _testDirectory = testDirectory;
                PackRootDirectory = packRootDirectory;
                CurrentDirectory = currentDirectory;
                NuspecFile = nuspecFile;
                NupkgFile = nupkgFile;
            }

            internal static ExcludesTest Create()
            {
                TestDirectory testDirectory = TestDirectory.Create();

                var rootDirectory = new DirectoryInfo(testDirectory.Path);

                DirectoryInfo packRootDirectory = rootDirectory.CreateSubdirectory("a");

                File.WriteAllText(Path.Combine(packRootDirectory.FullName, "b.nupkg"), string.Empty);
                File.WriteAllText(Path.Combine(packRootDirectory.FullName, ".c"), string.Empty);
                File.WriteAllText(Path.Combine(packRootDirectory.FullName, "d.e"), string.Empty);

                DirectoryInfo currentDirectory = rootDirectory.CreateSubdirectory("f");

                var nuspecFile = new FileInfo(Path.Combine(currentDirectory.FullName, "g.nuspec"));

                string pattern = $"..{Path.DirectorySeparatorChar}{packRootDirectory.Name}{Path.DirectorySeparatorChar}**{Path.DirectorySeparatorChar}*.*";

                File.WriteAllText(nuspecFile.FullName, $@"<?xml version=""1.0""?>
<package>
    <metadata>
        <id>h</id>
        <version>1.0.0</version>
        <title>i</title>
        <description>j</description>
        <authors>k</authors>
        <requireLicenseAcceptance>false</requireLicenseAcceptance>
        <dependencies />
    </metadata>
    <files>
        <file src=""{pattern}"" target="""" />
    </files>   
</package>");

                var nupkgFile = new FileInfo(Path.Combine(currentDirectory.FullName, "h.1.0.0.nupkg"));

                return new ExcludesTest(testDirectory, packRootDirectory, currentDirectory, nuspecFile, nupkgFile);
            }

            public void Dispose()
            {
                _testDirectory.Dispose();
            }
        }
    }
}
