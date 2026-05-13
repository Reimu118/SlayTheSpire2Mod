using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Renko.Scripts.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Extensions;
using Renko.Scripts.Extensions;
using MegaCrit.Sts2.Core.Logging;
using Godot;

namespace Renko.Scripts.Cards;

// PoolAttribute 用来告诉 BaseLib：所有继承这个抽象基类的 Renko 卡牌，都应该被加入 RenkoCardPool。
// CustomCardModel 构造时会自动把具体卡牌类型注册进模型数据库；注册过程中 BaseLib 会检查这个 Pool 标记。
// 如果卡牌类或它的父类没有标记 [Pool(typeof(...))]，运行时会因为不知道这张卡属于哪个卡池而报错。
[Pool(typeof(RenkoCardPool))]
public abstract class AbstractRenkoCard : CustomCardModel {
    // 卡牌小图路径。
    // Id.Entry 是游戏/Mod 给当前卡牌生成的模型 ID，例如某张卡的 Entry 可能类似 "RENKO_STRIKE"。
    // ToLowerInvariant() 会把它转成小写，从而拼出 res://Renko/images/cards/renko_strike.png 这样的资源路径。
    // 这里沿用 HIRO 的写法，适合后续有 pck 资源包、并把卡图放在 Renko/images/cards/ 目录时使用。
    // 
    public override string PortraitPath {
        get {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
            Log.Info(">>>[Renko]CardPath="+path,2);
            return ResourceLoader.Exists(path)? path :"card.png".CardImagePath();
        }
    }


    // Renko 卡牌的统一构造入口。
    // 具体卡牌只需要继承 AbstractRenkoCard，然后把费用、类型、稀有度和目标传进来。
    //
    // energyCost：卡牌费用。常见值为 0、1、2、3，特殊 X 费牌需要看游戏/基类约定。
    // type：卡牌类型，例如 Attack、Skill、Power、Status、Curse 等。
    // rarity：卡牌稀有度，例如 Basic、Common、Uncommon、Rare 等。
    // targetType：使用目标，例如 AnyEnemy、Self、AllEnemies、None 等。
    // showInCardLibrary：是否显示在图鉴/卡牌库里。状态牌、临时 Token 牌有时会设为 false。
    // autoAdd：是否让 BaseLib 自动注册这张卡。普通 Mod 卡牌通常保持 true；特殊手动注册场景才会改。
    protected AbstractRenkoCard(int energyCost, CardType type, CardRarity rarity, TargetType targetType,
        bool showInCardLibrary = true, bool autoAdd = true)
        : base(energyCost, type, rarity, targetType, showInCardLibrary, autoAdd) {
    }

    // 所有 Renko 卡牌共用的出牌前/出牌时逻辑入口。
    // 具体卡牌如果 override OnPlay，通常应该根据需要调用 base.OnPlay(...)，
    // 这样这里定义的 Renko 通用关键词联动才会生效。
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay) {
        // Owner 是这张卡当前所属的玩家/实体。
        // 有些临时卡、预览卡或异常状态下 Owner 可能为空，所以这里用 ?. 安全访问。
        // creature 代表真正参与战斗的生物实体，后续施加能力时需要用它作为目标。
        var creature = Owner?.Creature;

        // 只有卡牌确实有拥有者，并且拥有者还活着时，才处理关键词带来的额外效果。
        // 这可以避免死亡、预览、无主卡牌等场景触发不该触发的能力逻辑。
        if (creature != null && !creature.IsDead) {
            // 如果这张卡带有 RenkoCardKeywords.Zhengyi 关键词，就给自己施加 1 层 Justice。
            // CanonicalKeywords 是卡牌模型声明的“基础关键词集合”，通常由具体卡牌 override 提供。
            // 这段逻辑来自 HIRO 的抽象卡牌基类，后续如果 Renko 不需要“正义”体系，可以移除或替换为 Renko 自己的关键词。
            //if (CanonicalKeywords.Contains(RenkoCardKeywords.Zhengyi)) {
                //await PowerCmd.Apply<Justice>(creature, 1m, creature, this);
            //}

            // 如果这张卡带有 RenkoCardKeywords.Error 关键词，就触发 KillImpulsePower 的层数增长。
            // 这里先读取 cardId，是为了过滤掉某些“选择项临时卡”，避免一张正式卡生成的选项牌也重复触发通用关键词收益。
            //if (CanonicalKeywords.Contains(RenkoCardKeywords.Error)) {
                //string cardId = Id?.Entry ?? string.Empty;

                // 临时选择牌只用于 UI 选择，不应当像正式打出卡牌一样叠加 KillImpulsePower。
                // 目前的判断规则仍沿用 HIRO：名字里包含 picnic/movie/game 的卡视为临时选择卡。
                //if (!IsTemporaryChoiceCard(cardId)) {
                  //  await KillImpulsePower.GainStacks(context, creature, 2m, creature, this);
                //}
            //}
        }

        // 调用基类 OnPlay，保留 CustomCardModel/CardModel 原本的出牌流程。
        // 如果没有这行，某些基类内部处理、Hook 或后续扩展点可能不会执行。
        await base.OnPlay(context, cardPlay);
    }

    // 判断某个 cardId 是否是“临时选择卡”。
    // 这类卡一般不是玩家牌组里的正式卡，而是某张卡打出后弹出的选项，例如从几个 Token 中选择一个效果。
    // 当前规则直接复用 HIRO 的命名判断；如果 Renko 后续有自己的 Token/选项牌命名，应在这里同步更新。
    private bool IsTemporaryChoiceCard(string cardId) {
        // 空 ID 不能可靠判断为正式卡。这里保守地当作临时卡处理，避免触发额外收益。
        if (string.IsNullOrEmpty(cardId)) return true;

        // 统一转成小写，避免因为模型 ID 大小写不同导致 contains 判断失效。
        string lowerId = cardId.ToLowerInvariant();

        // HIRO 的三个临时选项牌关键词。
        // Renko 如果不使用 picnic/movie/game 这些 Token，应把这里替换成自己的临时卡 ID 规则。
        return lowerId.Contains("picnic") || lowerId.Contains("movie") || lowerId.Contains("game");
    }
}
