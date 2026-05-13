using BaseLib.Abstracts;
using Godot;
using Renko.Scripts.Character;
namespace Renko.Scripts.Pools;

public partial class RenkoRelicPool : CustomRelicPoolModel {
    public override string EnergyColorName => RenkoCharacter.CharacterId;

    public override Color LabOutlineColor => RenkoCharacter.Color;
}
