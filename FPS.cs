using Godot;

namespace RhythmGame;

public partial class FPS : Label
{
    public override void _Process(double delta)
    {
        Text = "FPS: " + Engine.GetFramesPerSecond();
    }
}