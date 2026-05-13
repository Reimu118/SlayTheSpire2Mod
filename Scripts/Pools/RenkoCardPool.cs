using BaseLib.Abstracts;
using Godot;
using Renko.Scripts.Character;

namespace Renko.Scripts.Pools;

public class RenkoCardPool : CustomCardPoolModel
{
    // 卡池 ID。这里不是游戏内显示名称，而是模型注册、卡牌归属、筛选和本地化等系统使用的内部标识。
    // 保持和 RenkoCharacter.CharacterId 一致，可以避免角色、卡池、资源路径之间出现不必要的硬编码差异。
    public override string Title => RenkoCharacter.CharacterId;

    // 战斗界面左下角的大能量图标。
    // 当前路径沿用 Manbo 教学模板的资源组织方式：<ModId>/Images/Charui/big_energy.png。
    // 现在 MyMod 还没有正式资源包时，这里属于预留写法；后续需要补 Renko 自己的图片或改回可用占位路径。
    // public override string BigEnergyIconPath => "Charui/big_energy.png".ImagePath();

    // 卡牌描述文本中显示的能量小图标。
    // 例如描述里出现费用、能量相关文本时，游戏会使用这个小图标来匹配当前角色卡池的能量颜色。
    // public override string TextEnergyIconPath => "Charui/text_energy.png".ImagePath();

    // 卡牌边框/卡背的 HSV 调色参数。
    // BaseLib 会在默认卡框材质上套一层 HSV shader，而不是直接替换整张卡框图。
    // 这三个值通常在 0 到 1 之间微调：H 控制色相，S 控制饱和度，V 控制明度。
    // 如果之后提供完全自定义的卡框图片，可以考虑保持这些值为默认或改用 CustomFrame。
    public override float H => 0.95f;
    public override float S => 0.98f;
    public override float V => 0.7f;

    // 另一种做法是提供完整的自定义卡框图，而不是使用 HSV 给默认卡框染色。
    // 需要时可以取消下面的重写，并让它返回 Renko 自己的 frame 贴图。
    /*public override Texture2D CustomFrame(CustomCardModel card)
	{
		// 这里会尝试读取 <ModId>/Images/cards/frame.png。
		return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
	}*/

    // 图鉴、牌组列表等紧凑 UI 中，卡牌条目和小图标使用的主色。
    // 建议和 RenkoCharacter.Color 保持同一色系，但可以略暗以保证文字可读性。
    public override Color DeckEntryCardColor => new("840240");

    // 能量数字/能量图标外轮廓颜色。
    // 这个颜色会影响卡牌费用位置和部分 UI 的描边观感。
    public override Color EnergyOutlineColor => new("651565");

    // 是否作为无色卡池处理。
    // 角色专属卡池应保持 false；事件牌、状态牌、诅咒牌等共享卡池才通常设为 true。
    public override bool IsColorless => false;
}
