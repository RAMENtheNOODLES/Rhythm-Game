using Godot;

namespace RhythmGame;

public partial class Player : CharacterBody2D
{
    /** Signals **/
    [Signal]
    public delegate void HitEventHandler();

    [Export] private int _speed = 360;
    [Export] private ColorRect _interactCursor;
    [Export] private Timer _interactTimer;
    [Export] private Label _playerCoords;

    private Vector2 _screenSize;

    private enum Facing
    {
        Up,
        Right,
        Down,
        Left,
    }

    private Facing _facing = Facing.Up;
    
    public override void _Ready()
    { 
        _screenSize = GetViewport().GetVisibleRect().Size;
    }

    public override void _PhysicsProcess(double delta)
    {
        _playerCoords.Text = "" + Globals.GetRoundedXy(Position);
        Velocity = Vector2.Zero;
        
        if (Input.IsActionPressed("move_right"))
        {
            _facing = Facing.Right;
            Velocity += new Vector2I(1, 0);
        }
        if (Input.IsActionPressed("move_left"))
        {
            _facing = Facing.Left;
            Velocity += new Vector2I(-1, 0);
        }
        if (Input.IsActionPressed("move_down"))
        {
            _facing = Facing.Down;
            Velocity += new Vector2I(0, 1);
        }
        if (Input.IsActionPressed("move_up"))
        {
            _facing = Facing.Up;
            Velocity += new Vector2I(0, -1);
        }

        if (Velocity.Length() > 0)
        {
            Velocity = Velocity.Normalized() * _speed;
            GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play();
        }
        else
        {
            GetNode<AnimatedSprite2D>("AnimatedSprite2D").Stop();
            Position = Position.Snapped(Vector2.One * Globals.TileLength / 2);
        }

        MoveAndSlide();
        
        _MoveCursor();

        Globals.RaiseGetTileEvent(Position.X, Position.Y);

        if (Input.IsActionPressed("interact") && _interactTimer.TimeLeft == 0)
        {
            GD.Print("Interact cursor pos: " + _interactCursor.Position / 16);
            Globals.RaiseInteractTileEvent(_interactCursor.Position.X, _interactCursor.Position.Y);
            _interactTimer.Start();
        }
    }

    private void _MoveCursor()
    {
        var xy = Globals.GetRoundedXy(Position);

        xy *= new Vector2I(16, 16);

        var vectorX = 0;
        var vectorY = 0;

        switch (_facing)
        {
            case Facing.Up:
                vectorY = -1;
                break;
            case Facing.Right:
                vectorX = 1;
                break;
            case Facing.Down:
                vectorY = 1;
                break;
            case Facing.Left:
                vectorX = -1;
                break;
            default:
                vectorX = 0;
                vectorY = 0;
                break;
        }

        _interactCursor.Position = new Vector2I(vectorX * 16, vectorY * 16) + xy;
    }

    private Globals.ITile _returnTile(Vector2 xy)
    {
        return null;
    }
}