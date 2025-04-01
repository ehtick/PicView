namespace PicView.Core.FileAssociations;
public class FileAssociationInstructions
{
    public List<AssociationItem> ExtensionsToAssociate { get; set; } = [];
    public List<string> ExtensionsToUnassociate { get; set; } = [];
}