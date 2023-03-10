using Godot;
using System;
using System.Collections.Generic;

public class BoidController : Node2D
{
    private PackedScene _boidResource = GD.Load<PackedScene>("res://src/Boid.tscn");
    
    [Export] public int NumberOfBoids { set; get; } = 100;
    [Export] public int GridStepSize { set; get; } = 100;
    [Export] public float Speed { set; get; } = 100.0f;
    [Export] public float ViewAngleDegrees { set; get; } = 90.0f;
    [Export] public double ViewDistance { set; get; } = 150;
    [Export] public float TurnSpeed { set; get; } = 1.0f;

    [Export] public float AlignCoeff { set; get; } = 1.0f;
    [Export] public float SeparationCoeff { set; get; } = 1.0f;
    [Export] public float CohesionCoeff { set; get; } = 1.0f;

    [Export] public bool KeepInBounds { set; get; } = true;

    private List<Boid> _boidList = new List<Boid>();
    private Dictionary<Vector2, List<Boid>> _gridCells = new Dictionary<Vector2, List<Boid>>();
    private Vector2 _screenSize;

    public override void _Ready()
    {
        _screenSize = GetViewport().GetVisibleRect().Size;
        GD.Print(_screenSize);
        GD.Randomize();
        CreateBoids();
    }

    public override void _Process(float delta)
    {
        GroupBoids();
        foreach (Boid boid in _boidList)
        {
            List<Boid> neighbors = Neighbors(boid);
            Vector2 alignment = AlignCoeff * Alignment(neighbors);
            Vector2 separation = SeparationCoeff * Separation(neighbors, boid);
            Vector2 cohesion = CohesionCoeff * Cohesion(neighbors, boid);
            Vector2 newDir = separation + alignment + cohesion;
            
            newDir /= (AlignCoeff + SeparationCoeff + CohesionCoeff);
            boid.Direction = newDir;

            if (KeepInBounds)
            {
                Vector2 pos = boid.GlobalPosition;
                pos.x = pos.x < 0? _screenSize.x : (pos.x > _screenSize.x? 0 : pos.x);
                pos.y = pos.y < 0? _screenSize.y : (pos.y > _screenSize.y? 0 : pos.y);
                boid.GlobalPosition = pos;
            }
        }
    }

    private void CreateBoids()
    {
        for (int i = 0; i < NumberOfBoids; i++)
        {
            Boid boid = (Boid)_boidResource.Instance();
            boid.Speed = Speed;
            boid.Direction = new Vector2((float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1));
            boid.ViewAngle = Mathf.Deg2Rad(ViewAngleDegrees);
            boid.ViewDistance = ViewDistance;
            boid.TurnSpeed = TurnSpeed;
            double spread = (double)NumberOfBoids / 3;
            float posX = (float)GD.RandRange(-spread, spread) + _screenSize.x / 2;
            float posY = (float)GD.RandRange(-spread, spread) + _screenSize.y / 2;
            boid.GlobalPosition = new Vector2(posX, posY);
            _boidList.Add(boid);
            AddChild(boid);
        }
    }

    private void GroupBoids()
    {
        _gridCells.Clear();
        foreach (Boid boid in _boidList)
        {
            Vector2 cell = GetBoidCell(boid);
            if (_gridCells.ContainsKey(cell))
            {
                _gridCells[cell].Add(boid);
            }
            else
            {
                var newFlock = new List<Boid>();
                newFlock.Add(boid);
                _gridCells[cell] = newFlock;
            }
        }
    }

    private List<Boid> Neighbors(Boid target)
    {
        var neighbors = new List<Boid>();
        double viewDistSquared = target.ViewDistance*target.ViewDistance;
        double viewAngle = target.ViewAngle;
        Vector2 targetPos = target.GlobalPosition;
        Vector2 cell = GetBoidCell(target);

        var nearbyBoids = new List<Boid>();
        for (int x = (int)cell.x - 1; x <= cell.x + 1; x++)
        {
            for (int y = (int)cell.y - 1; y <= cell.y + 1; y++)
            {
                Vector2 c = new Vector2(x, y);
                if (_gridCells.ContainsKey(c))
                {
                    nearbyBoids.AddRange(_gridCells[c]);
                }
            }
        }

        foreach (Boid boid in nearbyBoids)
        {
            if (target == boid) continue;

            Vector2 boidPos = boid.GlobalPosition;
            bool close = viewDistSquared > targetPos.DistanceSquaredTo(boidPos);
            bool visible = viewAngle > targetPos.AngleTo(boidPos);
            if (close && visible)
            {
                neighbors.Add(boid);
            }
        }

        return neighbors;
    }

    private Vector2 Alignment(List<Boid> flock)
    {
        Vector2 alignment = Vector2.Zero;
        foreach (Boid boid in flock)
        {
            alignment += boid.Direction;
        }

        return alignment.Normalized();
    }

    private Vector2 Separation(List<Boid> flock, Boid target)
    {
        Vector2 separation = Vector2.Zero;
        Vector2 targetPos = target.GlobalPosition;
        foreach (Boid boid in flock)
        {
            Vector2 oppositeDir = (targetPos - boid.GlobalPosition);
            float oppositeDirLength = oppositeDir.Length();
            separation += oppositeDir;
        }

        return separation.Normalized();
    }

    private Vector2 Cohesion(List<Boid> flock, Boid target)
    {
        int flockSize = flock.Count;
        if (flockSize == 0) return Vector2.Zero;
        
        Vector2 averagePos = Vector2.Zero;
        foreach (Boid boid in flock)
        {
            averagePos += boid.GlobalPosition;
        }
        averagePos /= flockSize;

        return (averagePos - target.GlobalPosition).Normalized();
    }

    private Vector2 GetBoidCell(Boid b)
    {
        int cellX = (int)b.Position.x / GridStepSize;
        int cellY = (int)b.Position.y / GridStepSize;
        return new Vector2(cellX, cellY);
    }
}