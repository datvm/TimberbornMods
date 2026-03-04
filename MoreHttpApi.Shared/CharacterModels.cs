namespace MoreHttpApi.Shared;

public record HttpCharacter(
    HttpEntityModel Entity,
    bool IsBeaver,
    bool IsChild,
    HttpNamedEntity Name
)
{
    
    public bool IsBot => !IsBeaver;

}
