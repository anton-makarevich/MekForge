using Sanet.MekForge.Core.Data;
using Sanet.MekForge.Core.Models.Game.Players;
using Sanet.MekForge.Core.Models.Map;
using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Models.Units;

public abstract class Unit
{
    protected readonly List<UnitPart> _parts; 
    protected Unit(string chassis, string model, int tonnage,
        int walkMp,
        IEnumerable<UnitPart> parts,
        Guid? id = null)
    {
        Chassis = chassis;
        Model = model;
        Name = $"{chassis} {model}";
        Tonnage = tonnage;
        BaseMovement = walkMp;
        _parts = parts.ToList();
        if (id.HasValue)
        {
            Id = id.Value;
        }
    }

    public string Chassis { get; }
    public string Model { get; }
    public string Name { get; }
    public int Tonnage { get; }
    
    public IPlayer? Owner { get; internal set; }
    
    public UnitStatus Status { get; protected set; }

    public WeightClass Class => Tonnage switch
    {
        <= 35 => WeightClass.Light,
        <= 55 => WeightClass.Medium,
        <= 75 => WeightClass.Heavy,
        <= 100 => WeightClass.Assault,
        _ => WeightClass.Unknown
    };

    // Base movement (walking)
    protected int BaseMovement { get; }
    
    // Movement capabilities
    public virtual int GetMovementPoints(MovementType type)
    {
        return type switch
        {
            MovementType.Walk => BaseMovement,
            MovementType.Run => (int)Math.Ceiling(BaseMovement * 1.5),
            MovementType.Jump => GetAllComponents<JumpJets>().Sum(j => j.JumpMp),
            MovementType.Sprint => BaseMovement * 2,
            MovementType.Masc => HasActiveComponent<Masc>() ? BaseMovement * 2 : (int)(BaseMovement * 1.5),
            _ => 0
        };
    }

    /// <summary>
    /// Determines if the unit can move backward with the given movement type
    /// </summary>
    public abstract bool CanMoveBackward(MovementType type);

    // Location and facing
    public HexPosition? Position { get; private set; }

    public bool IsDeployed => Position != null;

    public void Deploy(HexPosition position)
    {
        if (Position != null)
        {
            throw new InvalidOperationException($"{Name} is already deployed.");
        }
        Position = position;
    }

    // Heat management
    public int CurrentHeat { get; protected set; }
    public virtual void ApplyHeat(int heat) { } // Default no-op for units that don't use heat
    
    // Parts management
    public IReadOnlyList<UnitPart> Parts =>_parts;
    public Guid Id { get; private set; } = Guid.Empty;

    // Movement tracking
    public int MovementPointsSpent { get; private set; }
    public MovementType? MovementTypeUsed { get; private set; }
    public int DistanceCovered { get; private set; }

    public bool HasMoved => MovementTypeUsed.HasValue;

    public bool HasFiredWeapons { get; private set; }

    public void ResetMovement()
    { 
        MovementPointsSpent = 0;
        MovementTypeUsed = null;
        DistanceCovered = 0;
    }

    public void FireWeapons()
    {
        if (!IsDeployed)
        {
            throw new InvalidOperationException("Unit is not deployed.");
        }
        HasFiredWeapons = true;
    }
    
    // Methods
    public abstract int CalculateBattleValue();
    
    // Status management
    public virtual void Startup()
    {
        if (Status is UnitStatus.Shutdown or UnitStatus.PoweredDown)
        {
            Status = UnitStatus.Active;
        }
    }

    public virtual void Shutdown()
    {
        if (Status == UnitStatus.Active)
        {
            Status = UnitStatus.Shutdown;
        }
    }
    
    public virtual void ApplyDamage(int damage, UnitPart targetPart)
    {
        var remainingDamage = targetPart.ApplyDamage(damage);
        
        // If there's remaining damage and this is a limb, transfer to the connected part
        if (remainingDamage > 0)
        {
            var transferLocation = GetTransferLocation(targetPart.Location);
            if (transferLocation.HasValue)
            {
                var transferPart = _parts.Find(p => p.Location == transferLocation.Value);
                if (transferPart != null)
                {
                    ApplyDamage(remainingDamage, transferPart);
                }
            }
        }
    }

    // Different unit types will have different damage transfer patterns
    protected abstract PartLocation? GetTransferLocation(PartLocation location);

    protected IEnumerable<T> GetAllComponents<T>() where T : Component
    {
        return Parts.SelectMany(p => p.GetComponents<T>());
    }

    protected bool HasActiveComponent<T>() where T : Component
    {
        return GetAllComponents<T>().Any(c => c.IsActive && !c.IsDestroyed);
    }

    public void Move(MovementType movementType, List<PathSegmentData> movementPath)
    {
        if (Position == null)
        {
            throw new InvalidOperationException("Unit is not deployed.");
        } 
        var position = movementType==MovementType.StandingStill
            ? Position.Value
            :new HexPosition(movementPath.Last().To);
        var distance = Position.Value.Coordinates.DistanceTo(position.Coordinates);
        DistanceCovered = distance;
        MovementPointsSpent = movementPath.Sum(s=>s.Cost);
        MovementTypeUsed = movementType;
        Position = position; 
    }
}