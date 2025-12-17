using System.Collections.Immutable;
using Navigator.UI.Models.Nodes;

namespace Navigator.UI.Utils;

public static class NodeSorter
{
    public static ImmutableArray<BaseNode> Sort(ImmutableArray<BaseNode> nodes, NodeSortOrder sortOrder = NodeSortOrder.NameAsc)
    {
        // first split into 2 lists one for files and one for directories
        List<BaseNode> directories = [];
        List<BaseNode> files = [];
        foreach (var node in nodes)
        {
            if (node is DirectoryNode)
                directories.Add(node);
            else
                files.Add(node);
        }

        if (sortOrder == NodeSortOrder.NameAsc)
        {
            directories.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));
            files.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        if (sortOrder == NodeSortOrder.SizeAsc)
        {
            // directories have no size, keep original order
            files.Sort((x, y) => ((FileNode)x).Size.CompareTo(((FileNode)y).Size));
        }

        if (sortOrder == NodeSortOrder.DateAsc)
        {
            directories.Sort((x, y) => ((DirectoryNode)x).LastModifiedDate.CompareTo(((DirectoryNode)y).LastModifiedDate));
            files.Sort((x, y) => ((FileNode)x).LastModifiedDate.CompareTo(((FileNode)y).LastModifiedDate));
        }

        if (sortOrder == NodeSortOrder.NameDesc)
        {
            directories.Sort((x, y) => string.Compare(y.Name, x.Name, StringComparison.InvariantCultureIgnoreCase));
            files.Sort((x, y) => string.Compare(y.Name, x.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        if (sortOrder == NodeSortOrder.SizeDesc)
        {
            // directories have no size, keep original order
            files.Sort((x, y) => ((FileNode)y).Size.CompareTo(((FileNode)x).Size));
        }

        if (sortOrder == NodeSortOrder.DateDesc)
        {
            directories.Sort((x, y) => ((DirectoryNode)y).LastModifiedDate.CompareTo(((DirectoryNode)x).LastModifiedDate));
            files.Sort((x, y) => ((FileNode)y).LastModifiedDate.CompareTo(((FileNode)x).LastModifiedDate));
        }


        // concatenate the sorted lists based on sort order
        return [..directories.Concat(files)];
    }
}

public enum NodeSortOrder
{
    NameAsc,
    NameDesc,
    SizeAsc,
    SizeDesc,
    DateAsc,
    DateDesc,
}
