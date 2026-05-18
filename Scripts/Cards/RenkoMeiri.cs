
using Renko.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Renko.Scripts.Cards {
    // Renko 的基础打击牌。
    // 这张牌会出现在 Renko 的初始牌组里，所以稀有度使用 Basic，并带有 CardTag.Strike 标签。
    // 实现思路主要参考 HIRO 的 Strike.cs：声明伤害动态变量，打出时对单个敌人造成伤害，升级时提高伤害。
    public class RenkoMeiri : AbstractRenkoCard {
        // 构造函数用于定义这张卡的基本模型数据。
        // 参数含义依次是：
        // 1：费用为 1 点能量。
        // CardType.Attack：这是攻击牌，会受力量、易伤、攻击相关遗物/能力等系统影响。
        // CardRarity.Basic：基础牌，通常用于起始牌组，不作为普通奖励牌的稀有度池核心内容。
        // TargetType.AnyEnemy：使用时需要选择任意一个敌人作为目标。
        public RenkoStrike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) {
        }

        // 卡牌标签。
        // CardTag.Strike 告诉游戏这是一张“打击”类卡牌。
        // 这个标签可能被遗物、能力、事件或其他卡牌效果读取，例如“强化所有 Strike/Defend”一类逻辑。
        protected override HashSet<CardTag> CanonicalTags =>
        [
            CardTag.Strike,
        ];

        // 卡牌动态数值。
        // DamageVar 表示这张牌拥有一个可被预览、升级、力量等系统修正的伤害值。
        // 6m 是基础伤害；m 表示 decimal 字面量，符合 StS2 数值系统常用类型。
        // ValueProp.Move 表示这个数值属于卡牌动作/招式本身，用于游戏内部的数值来源标记。
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];
        // 打出卡牌时执行的实际效果。
        // choiceContext 是本次玩家选择/出牌上下文，Execute 时需要传入它来进入游戏动作队列。
        // cardPlay 保存这次出牌的信息，包括目标、消耗、来源等。
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            // 先执行 AbstractRenkoCard 中定义的 Renko 通用出牌逻辑。
            // 目前那里主要是预留给关键词联动；后续如果 Renko 有通用机制，具体卡牌调用 base 后就能自动吃到。
            await base.OnPlay(choiceContext, cardPlay);

            // 攻击牌需要目标。正常情况下 TargetType.AnyEnemy 会保证玩家必须选中敌人。
            // 这里仍然显式检查一次，避免异常调用或未来改动导致空目标继续传入伤害命令。
            ArgumentNullException.ThrowIfNull(cardPlay.Target);

            // 构造并执行一次来自这张卡的攻击伤害。
            // DynamicVars.Damage.BaseValue 读取上面 DamageVar 当前的基础数值；
            // FromCard(this) 会把伤害来源标记为当前卡牌，让力量、易伤、遗物、Hook 等系统有机会参与计算；
            // Targeting(cardPlay.Target) 指定目标；
            // WithHitFx(...) 指定命中时播放的攻击特效。
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        // 卡牌升级逻辑。
        // 这里把 DamageVar 的基础伤害提高 3 点，所以 RenkoStrike 会从 6 伤害升级为 9 伤害。
        // 如果之后想贴近 ManboAttack 的模板，可以改成 +2；当前保持 HIRO Strike 的 +3 写法。
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
    }
}
