using Godot;

namespace RhythmGame;

public partial class Metronome : GridContainer
{
    [Export] private Timer _timer;
    private int _activeTempobox = 0;

    public override void _Ready()
    {
        _timer.Timeout += _onTimerTimeout;
    }

    private void _UpdateTempoBoxes()
    {
        GetNode<TextureRect>("TempoBox1").Modulate = Globals.MetronomeOffColor;
        GetNode<TextureRect>("TempoBox2").Modulate = Globals.MetronomeOffColor;
        GetNode<TextureRect>("TempoBox3").Modulate = Globals.MetronomeOffColor;
        GetNode<TextureRect>("TempoBox4").Modulate = Globals.MetronomeOffColor;

        var tempobox = "TempoBox" + _activeTempobox;

        GetChild<TextureRect>(_activeTempobox - 1).Modulate = Globals.MetronomeOnColor;
    }

    private void _onTimerTimeout()
    {
        if (_activeTempobox + 1 > Globals.TimeSignature)
        {
            _activeTempobox = 1;
        }
        else
        {
            _activeTempobox += 1;
        }
        
        _UpdateTempoBoxes();
    }
}