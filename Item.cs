namespace RhythmGame;

public class Item : IInteractable
{
    public string Name { get; init; }
    public int MaxStackSize { get; init; }
    public int Id { get; init; }
    
    public string ImagePath { get; init; }
}