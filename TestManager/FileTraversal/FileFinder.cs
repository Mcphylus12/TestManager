
using Microsoft.Extensions.FileSystemGlobbing;
using System.Text.RegularExpressions;

namespace TestManager.FileTraversal;

internal class FileFinder
{
    private readonly DirectoryInfo _root;

    public FileFinder(DirectoryInfo root)
    {
        _root = root;
    }

    internal IEnumerable<FileInfo> GetAllTestFiles()
    {
        return _root.GetFiles("*.tsjson", SearchOption.AllDirectories);
    }

    internal IEnumerable<Entry> GetTestFilesAndFolders(string? path)
    {
        var searchDir = path is null ? _root : new DirectoryInfo(Path.Combine(_root.FullName, path));

        return searchDir.GetDirectories().Select(fi => new Entry("folder", fi.Name)).Union(searchDir.GetFiles("*.tsjson", SearchOption.TopDirectoryOnly).Select(fi => new Entry("testfile", fi.Name)));
    }

    internal IEnumerable<FileInfo> GetMatchingTestFiles(string pattern)
    {
        var matcher = new Matcher();
        matcher.AddIncludePatterns([pattern + ".tsjson"]);

        return matcher.GetResultsInFullPath(_root.FullName).Select(f => new FileInfo(f));
    }
}

public record class Entry(string Type, string Name);
