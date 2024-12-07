using Sanet.MekForge.Core.Models.Units;

namespace Sanet.MekForge.Core.Utils
{
    public class StructureValueProvider : IStructureValueProvider
    {
        public Dictionary<PartLocation, int> GetStructureValues(int tonnage)
        {
            var structureValues = new Dictionary<PartLocation, int>();

            // Structure values based on tonnage
            switch (tonnage)
            {
                case 20:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 6;
                    structureValues[PartLocation.LeftTorso] = 5;
                    structureValues[PartLocation.RightTorso] = 5;
                    structureValues[PartLocation.LeftArm] = 3;
                    structureValues[PartLocation.RightArm] = 3;
                    structureValues[PartLocation.LeftLeg] = 4;
                    structureValues[PartLocation.RightLeg] = 4;
                    break;
                case 25:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 8;
                    structureValues[PartLocation.LeftTorso] = 6;
                    structureValues[PartLocation.RightTorso] = 6;
                    structureValues[PartLocation.LeftArm] = 4;
                    structureValues[PartLocation.RightArm] = 4;
                    structureValues[PartLocation.LeftLeg] = 6;
                    structureValues[PartLocation.RightLeg] = 6;
                    break;
                case 30:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 10;
                    structureValues[PartLocation.LeftTorso] = 7;
                    structureValues[PartLocation.RightTorso] = 7;
                    structureValues[PartLocation.LeftArm] = 5;
                    structureValues[PartLocation.RightArm] = 5;
                    structureValues[PartLocation.LeftLeg] = 7;
                    structureValues[PartLocation.RightLeg] = 7;
                    break;
                case 35:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 11;
                    structureValues[PartLocation.LeftTorso] = 8;
                    structureValues[PartLocation.RightTorso] = 8;
                    structureValues[PartLocation.LeftArm] = 6;
                    structureValues[PartLocation.RightArm] = 6;
                    structureValues[PartLocation.LeftLeg] = 8;
                    structureValues[PartLocation.RightLeg] = 8;
                    break;
                case 40:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 12;
                    structureValues[PartLocation.LeftTorso] = 10;
                    structureValues[PartLocation.RightTorso] = 10;
                    structureValues[PartLocation.LeftArm] = 6;
                    structureValues[PartLocation.RightArm] = 6;
                    structureValues[PartLocation.LeftLeg] = 10;
                    structureValues[PartLocation.RightLeg] = 10;
                    break;
                case 45:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 14;
                    structureValues[PartLocation.LeftTorso] = 11;
                    structureValues[PartLocation.RightTorso] = 11;
                    structureValues[PartLocation.LeftArm] = 7;
                    structureValues[PartLocation.RightArm] = 7;
                    structureValues[PartLocation.LeftLeg] = 11;
                    structureValues[PartLocation.RightLeg] = 11;
                    break;
                case 50:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 16;
                    structureValues[PartLocation.LeftTorso] = 12;
                    structureValues[PartLocation.RightTorso] = 12;
                    structureValues[PartLocation.LeftArm] = 8;
                    structureValues[PartLocation.RightArm] = 8;
                    structureValues[PartLocation.LeftLeg] = 12;
                    structureValues[PartLocation.RightLeg] = 12;
                    break;
                case 55:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 18;
                    structureValues[PartLocation.LeftTorso] = 13;
                    structureValues[PartLocation.RightTorso] = 13;
                    structureValues[PartLocation.LeftArm] = 9;
                    structureValues[PartLocation.RightArm] = 9;
                    structureValues[PartLocation.LeftLeg] = 13;
                    structureValues[PartLocation.RightLeg] = 13;
                    break;
                case 60:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 20;
                    structureValues[PartLocation.LeftTorso] = 14;
                    structureValues[PartLocation.RightTorso] = 14;
                    structureValues[PartLocation.LeftArm] = 10;
                    structureValues[PartLocation.RightArm] = 10;
                    structureValues[PartLocation.LeftLeg] = 14;
                    structureValues[PartLocation.RightLeg] = 14;
                    break;
                case 65:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 21;
                    structureValues[PartLocation.LeftTorso] = 15;
                    structureValues[PartLocation.RightTorso] = 15;
                    structureValues[PartLocation.LeftArm] = 10;
                    structureValues[PartLocation.RightArm] = 10;
                    structureValues[PartLocation.LeftLeg] = 15;
                    structureValues[PartLocation.RightLeg] = 15;
                    break;
                case 70:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 22;
                    structureValues[PartLocation.LeftTorso] = 15;
                    structureValues[PartLocation.RightTorso] = 15;
                    structureValues[PartLocation.LeftArm] = 11;
                    structureValues[PartLocation.RightArm] = 11;
                    structureValues[PartLocation.LeftLeg] = 15;
                    structureValues[PartLocation.RightLeg] = 15;
                    break;
                case 75:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 23;
                    structureValues[PartLocation.LeftTorso] = 16;
                    structureValues[PartLocation.RightTorso] = 16;
                    structureValues[PartLocation.LeftArm] = 12;
                    structureValues[PartLocation.RightArm] = 12;
                    structureValues[PartLocation.LeftLeg] = 16;
                    structureValues[PartLocation.RightLeg] = 16;
                    break;
                case 80:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 25;
                    structureValues[PartLocation.LeftTorso] = 17;
                    structureValues[PartLocation.RightTorso] = 17;
                    structureValues[PartLocation.LeftArm] = 13;
                    structureValues[PartLocation.RightArm] = 13;
                    structureValues[PartLocation.LeftLeg] = 17;
                    structureValues[PartLocation.RightLeg] = 17;
                    break;
                case 85:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 27;
                    structureValues[PartLocation.LeftTorso] = 18;
                    structureValues[PartLocation.RightTorso] = 18;
                    structureValues[PartLocation.LeftArm] = 14;
                    structureValues[PartLocation.RightArm] = 14;
                    structureValues[PartLocation.LeftLeg] = 18;
                    structureValues[PartLocation.RightLeg] = 18;
                    break;
                case 90:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 29;
                    structureValues[PartLocation.LeftTorso] = 19;
                    structureValues[PartLocation.RightTorso] = 19;
                    structureValues[PartLocation.LeftArm] = 15;
                    structureValues[PartLocation.RightArm] = 15;
                    structureValues[PartLocation.LeftLeg] = 19;
                    structureValues[PartLocation.RightLeg] = 19;
                    break;
                case 95:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 30;
                    structureValues[PartLocation.LeftTorso] = 20;
                    structureValues[PartLocation.RightTorso] = 20;
                    structureValues[PartLocation.LeftArm] = 16;
                    structureValues[PartLocation.RightArm] = 16;
                    structureValues[PartLocation.LeftLeg] = 20;
                    structureValues[PartLocation.RightLeg] = 20;
                    break;
                case 100:
                    structureValues[PartLocation.Head] = 3;
                    structureValues[PartLocation.CenterTorso] = 31;
                    structureValues[PartLocation.LeftTorso] = 21;
                    structureValues[PartLocation.RightTorso] = 21;
                    structureValues[PartLocation.LeftArm] = 17;
                    structureValues[PartLocation.RightArm] = 17;
                    structureValues[PartLocation.LeftLeg] = 21;
                    structureValues[PartLocation.RightLeg] = 21;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Invalid tonnage");
            }

            return structureValues;
        }
    }
}
