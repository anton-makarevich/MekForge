using Sanet.MekForge.Core.Models.Units.Components;

namespace Sanet.MekForge.Core.Models.Units;

public abstract class Unit
{
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

    protected IEnumerable<T> GetAllComponents<T>() where T : Component
    {
        return Parts.SelectMany(p => p.GetComponents<T>());
    }

    protected bool HasActiveComponent<T>() where T : Component
    {
        return GetAllComponents<T>().Any(c => c.IsActive && !c.IsDestroyed);
    }
}