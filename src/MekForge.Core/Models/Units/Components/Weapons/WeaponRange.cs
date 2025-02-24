namespace Sanet.MekForge.Core.Models.Units.Components.Weapons;

/// <summary>
/// Represents the range bracket a weapon is firing at
/// </summary>
public enum WeaponRange
{
    /// <summary>
    /// Target is too close (within minimum range)
    /// </summary>
    Minimum,
    
    /// <summary>
    /// Target is at short range
    /// </summary>
    Short,
    
    /// <summary>
    /// Target is at medium range
    /// </summary>
    Medium,
    
    /// <summary>
    /// Target is at long range
    /// </summary>
    Long,
    
    /// <summary>
    /// Target is out of range
    /// </summary>
    OutOfRange
}
