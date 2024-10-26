
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

    internal IEnumerable<Entry> GetDirectoryContents(string? path)
    {
        var searchDir = path is null ? _root : new DirectoryInfo(Path.Combine(_root.FullName, path));

        return searchDir.GetDirectories().Select(fi => new Entry("folder", fi.Name, string.Empty)).Union(searchDir.GetFiles().Select(fi => new Entry(fi.Name.EndsWith(".tsjson") ? "testfile" : "file", fi.Name, Path.GetRelativePath(_root.FullName, fi.Directory!.FullName).Replace('\\', '/'))));
    }

    internal IEnumerable<FileInfo> GetMatchingTestFiles(string pattern, bool includeFiles = false)
    {
        var matcher = new Matcher();
        matcher.AddIncludePatterns([pattern + ".tsjson"]);

        if (includeFiles)
        {
            matcher.AddIncludePatterns([pattern]);
        }

        matcher.AddExclude(".config/");

        return matcher.GetResultsInFullPath(_root.FullName).Select(f => new FileInfo(f));
    }
}

public record class Entry(string Type, string Name, string Path);
