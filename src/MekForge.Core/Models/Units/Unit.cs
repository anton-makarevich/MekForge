using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Models.Units;

public abstract class Unit
{
    private Dictionary<MovementType, int>? _cachedMovement;

    protected Unit(string chassis, string model, int tonnage, int walkMp,
        IReadOnlyDictionary<PartLocation, UnitPartData> partsData)
    {
        Chassis = chassis;
        Model = model;
        Name = $"{chassis} {model}";
        Tonnage = tonnage;
        BaseMovement = walkMp;
        InitializeParts(partsData);
    }

    public string Chassis { get; }
    public string Model { get; }
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
    public virtual void ApplyHeat(int heat) { } // Default no-op for units that don't use heat
    
    // Parts management
    public List<UnitPart> Parts { get; } = [];

    private void InitializeParts(IReadOnlyDictionary<PartLocation, UnitPartData> partsData)
    {
        foreach (var (location, data) in partsData)
        {
            var part = new UnitPart(
                name: data.Name,
                location: location,
                maxArmor: data.MaxArmor,
                maxStructure: data.MaxStructure,
                slots: data.Slots);

            foreach (var component in data.Components)
            {
                part.TryAddComponent(component);
            }

            Parts.Add(part);
        }
    }

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