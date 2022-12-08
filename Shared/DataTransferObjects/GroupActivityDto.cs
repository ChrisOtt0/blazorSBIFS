namespace blazorSBIFS.Shared.DataTransferObjects;

public class GroupActivityDto : IJson
{
    public GroupDto GroupRequest { get; set; }
    public ActivityDto ActivityRequest { get; set; }
}