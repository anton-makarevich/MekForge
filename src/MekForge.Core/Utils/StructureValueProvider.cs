using Sanet.MekForge.Core.Models.Units;
using System.Collections.Generic;

namespace Sanet.MekForge.Core.Utils
{
    public class InnerSphereStructureProvider : IStructureValueProvider
    {
        public Dictionary<PartLocation, int> GetStructureValues(int tonnage)
        {
            var structureValues = new Dictionary<PartLocation, int>();

            // Example structure values based on tonnage
            // These values should be adjusted according to the classic Battletech rules
            int baseValue = tonnage / 10;

            structureValues[PartLocation.Head] = baseValue;
            structureValues[PartLocation.CenterTorso] = baseValue * 2;
            structureValues[PartLocation.LeftTorso] = baseValue;
            structureValues[PartLocation.RightTorso] = baseValue;
            structureValues[PartLocation.LeftArm] = baseValue;
            structureValues[PartLocation.RightArm] = baseValue;
            structureValues[PartLocation.LeftLeg] = baseValue;
            structureValues[PartLocation.RightLeg] = baseValue;

            return structureValues;
        }
    }
}
