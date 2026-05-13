using BaseLib.Abstracts;
using Renko.Scripts.Character;
namespace Renko.Scripts.Pools;

public class RenkoPotionPool : CustomPotionPoolModel {
    public override string EnergyColorName => RenkoCharacter.CharacterId;
}
