// Unit.cs
using System.Collections.Generic;
using System.Linq;
using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Models.Units;

public abstract class Unit
{
    private Dictionary<MovementType, int>? _cachedMovement;

    protected Unit(string name, int tonnage, int walkMP)
    {
        Name = name;
        Tonnage = tonnage;
        BaseMovement = walkMP;
        Parts = new List<UnitPart>();
    }

    public string Name { get; }
    public int Tonnage { get; }
    
    // Base movement (walking)
    protected int BaseMovement { get; }
    
    // Movement capabilities
    public Dictionary<MovementType, int> Movement
    {
        get
        {
            // Cache the movement values until next turn or equipment state changes
            if (_cachedMovement != null)
                return _cachedMovement;

            _cachedMovement = new Dictionary<MovementType, int>
            {
                { MovementType.Walk, BaseMovement },
                { MovementType.Run, CalculateRunMP() },
                { MovementType.Sprint, CalculateSprintMP() },
                { MovementType.Jump, CalculateJumpMP() },
                { MovementType.Masc, CalculateMascMP() }
            };

            return _cachedMovement;
        }
    }

    protected virtual int CalculateRunMP() => BaseMovement * 3 / 2;  // 1.5x walking
    protected virtual int CalculateSprintMP() => BaseMovement * 2;   // 2x walking
    protected virtual int CalculateJumpMP() => GetAllComponents<JumpJets>().Sum(j => j.JumpMP);
    protected virtual int CalculateMascMP() => HasActiveComponent<Masc>() ? BaseMovement * 2 : CalculateRunMP();

    // Location and facing
    public HexCoordinates Position { get; set; }
    public int Facing { get; set; } // 0-5 for hex facings

    // Heat management
    public int CurrentHeat { get; protected set; }
    
    // Parts management
    public List<UnitPart> Parts { get; }

    public void InvalidateMovementCache() => _cachedMovement = null;

    // Methods
    public abstract int CalculateBattleValue();
    
    public virtual void ApplyDamage(int damage, UnitPart targetPart)
    {
        var remainingDamage = targetPart.ApplyDamage(damage);
        
        // If there's remaining damage and this is a limb, transfer to the connected part
        if (remainingDamage > 0)
        {
            var transferLocation = GetTransferLocation(targetPart.Location);
            if (transferLocation.HasValue)
            {
                var transferPart = Parts.Find(p => p.Location == transferLocation.Value);
                if (transferPart != null)
                {
                    ApplyDamage(remainingDamage, transferPart);
                }
            }
        }
    }

    // Different unit types will have different damage transfer patterns
    protected abstract PartLocation? GetTransferLocation(PartLocation location);

    protected IEnumerable<T> GetAllComponents<T>() where T : UnitComponent
    {
        return Parts.SelectMany(p => p.GetComponents<T>());
    }

    protected bool HasActiveComponent<T>() where T : UnitComponent
    {
        return GetAllComponents<T>().Any(c => c.IsActive && !c.IsDestroyed);
    }
}