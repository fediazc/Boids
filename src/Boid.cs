using Godot;
using System;

public class Boid : KinematicBody2D 
{
    public float Speed { set; get; } = 0.0f; // use float to avoid casts for math with Vector2
    public Vector2 Direction { set; get; } = Vector2.Zero;
    public double ViewDistance { set; get; } = 0.0;
    public double ViewAngle { set; get; } = 0.0;
    public float TurnSpeed { set; get; } = 0.0f;

    private Vector2 _lastKnownDirection;
    private Vector2 _currentDir = Vector2.Zero;

    public override void _PhysicsProcess(float delta)
    {
        if (Direction == Vector2.Zero) 
        {
            Direction = _lastKnownDirection;
        } 
        else
        {
            _lastKnownDirection = Direction;
        }
        Direction = Direction.Normalized();
        _currentDir.x = Mathf.Lerp(_currentDir.x, Direction.x, TurnSpeed);
        _currentDir.y = Mathf.Lerp(_currentDir.y, Direction.y, TurnSpeed);
        MoveAndSlide(Speed * _currentDir.Normalized());
        Rotation = _currentDir.Angle();
    }
}
