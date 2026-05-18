# 自定义卡牌制作笔记

本文以 `AbstractRenkoCard`、`RenkoStrike`、`RenkoDefend` 为基准，记录制作一张 Renko 卡牌时必须写的内容，以及常见效果对应的函数。

## 一张卡牌必须包含什么

### 1. 文件和命名

每张卡建议一个独立 `.cs` 文件：

```text
MyMod/Scripts/Cards/RenkoXXX.cs
```

类名、构造函数名、文件名最好保持一致：

```csharp
namespace Renko.Scripts.Cards;

public class RenkoXXX : AbstractRenkoCard
{
    public RenkoXXX() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }
}
```

注意：构造函数没有返回值，名字必须和类名完全一致。比如类叫 `RenkoMeiri`，构造函数也必须叫 `RenkoMeiri()`。

### 2. 继承 `AbstractRenkoCard`

Renko 自定义卡牌应继承：

```csharp
public class RenkoXXX : AbstractRenkoCard
```

`AbstractRenkoCard` 已经通过 `[Pool(typeof(RenkoCardPool))]` 把 Renko 卡牌注册进 Renko 卡池，所以普通 Renko 卡不需要重复写卡池注册。

### 3. 构造函数声明基础信息

```csharp
public RenkoXXX() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
}
```

四个核心参数：

- `energyCost`：费用。常见是 `0`、`1`、`2`、`3`。
- `CardType`：卡牌类型。
- `CardRarity`：稀有度。
- `TargetType`：目标类型。

常见 `CardType`：

```csharp
CardType.Attack
CardType.Skill
CardType.Power
CardType.Status
CardType.Curse
```

常见 `CardRarity`：

```csharp
CardRarity.Basic
CardRarity.Common
CardRarity.Uncommon
CardRarity.Rare
```

常见 `TargetType`：

```csharp
TargetType.Self        // 以自己为目标
TargetType.AnyEnemy    // 选择一个敌人
TargetType.AllEnemies  // 全体敌人
TargetType.RandomEnemy // 随机敌人
TargetType.None        // 不需要目标
```

### 4. 动态数值 `CanonicalVars`

`CanonicalVars` 是这张卡牌的“数字声明表”。它只声明数字，不执行效果。真正抽牌、获得格挡、施加 buff 等行为仍然要写在 `OnPlay` 里。

卡牌描述里会显示、升级时会变化、预览时需要计算、或者要被其他系统识别的数字，都建议放进 `CanonicalVars`。如果某个数字完全固定、不显示、不升级，也可以直接在 `OnPlay` 里写常量。

注意：“内置变量”不是每张卡自动拥有，而是游戏/BaseLib 已经提供了对应的变量类型和快捷读取方式。你仍然必须在当前卡牌里声明它。

#### 标准变量优先

伤害用 `DamageVar`。它会参与攻击相关计算，例如力量、易伤、攻击修正等。

```csharp
new DamageVar(6m, ValueProp.Move)
```

读取：

```csharp
DynamicVars.Damage.BaseValue
```

描述：

```text
造成{Damage:diff()}点伤害。
```

格挡用 `BlockVar`。它会参与格挡相关计算，例如敏捷、格挡修正等。

```csharp
new BlockVar(5m, ValueProp.Move)
```

读取：

```csharp
DynamicVars.Block.BaseValue
```

描述：

```text
获得{Block:diff()}点格挡。
```

抽牌、能量、治疗、金币也有专用变量，优先用这些，不要优先写成普通字符串变量：

```csharp
new CardsVar(2m)
new EnergyVar(1m)
new HealVar(4m)
new GoldVar(10m)
```

读取：

```csharp
DynamicVars.Cards.IntValue
DynamicVars.Energy.IntValue
DynamicVars.Heal.BaseValue
DynamicVars.Gold.IntValue
```

描述示例：

```text
抽{Cards:diff()}张牌。
获得{Energy:energyIcons()}。
回复{Heal:diff()}点生命。
获得{Gold:diff()}金币。
```

#### Power 层数用 `PowerVar<TPower>`

如果数字表示某个 buff/debuff 的层数，优先用 `PowerVar<TPower>`。

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    new PowerVar<StrengthPower>(2m),
    new PowerVar<DexterityPower>(2m),
    new PowerVar<WeakPower>(1m),
    new PowerVar<VulnerablePower>(1m)
];
```

读取：

```csharp
DynamicVars.Power<StrengthPower>().BaseValue
DynamicVars.Power<DexterityPower>().BaseValue
DynamicVars.Power<WeakPower>().BaseValue
DynamicVars.Power<VulnerablePower>().BaseValue
```

有些常见 Power 也有快捷属性，项目里已经用过：

```csharp
DynamicVars.Strength.BaseValue
DynamicVars.Weak.BaseValue
DynamicVars.Vulnerable.BaseValue
```

对应施加效果：

```csharp
await PowerCmd.Apply<StrengthPower>(
    Owner.Creature,
    DynamicVars.Power<StrengthPower>().BaseValue,
    Owner.Creature,
    this
);
```

#### 自定义普通变量

Renko 自己的机制数字，或者临时不需要专用类型的普通数字，可以用 `DynamicVar`：

```csharp
new DynamicVar("Observation", 3m)
new DynamicVar("Bonus", 5m)
```

读取：

```csharp
DynamicVars["Observation"].BaseValue
DynamicVars["Bonus"].BaseValue
```

升级：

```csharp
DynamicVars["Observation"].UpgradeValueBy(2m);
```

描述：

```text
获得{Observation:diff()}层观测。
```

如果这个自定义变量会被很多卡重复使用，建议再封装成专用类，例如 `ObservationVar`，避免字符串写错：

```csharp
public class ObservationVar : DynamicVar
{
    public const string Key = "Renko-Observation";
    public static readonly string LocKey = Key.ToUpperInvariant();

    public ObservationVar(decimal baseValue) : base(Key, baseValue)
    {
        this.WithTooltip(LocKey);
    }
}
```

使用：

```csharp
new ObservationVar(3m)
DynamicVars[ObservationVar.Key].BaseValue
```

#### 混合示例

一张牌如果是“抽 2 张牌，获得 1 点能量，获得 8 点格挡，获得 3 层观测”，其中抽牌固定，能量、格挡、观测升级时变化，可以写：

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    new CardsVar(2m),
    new EnergyVar(1m),
    new BlockVar(8m, ValueProp.Move),
    new DynamicVar("Observation", 3m)
];
```

出牌：

```csharp
await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
await PowerCmd.Apply<ObservationPower>(
    Owner.Creature,
    DynamicVars["Observation"].BaseValue,
    Owner.Creature,
    this
);
```

升级时不改抽牌，只改其余数值：

```csharp
protected override void OnUpgrade()
{
    DynamicVars.Energy.UpgradeValueBy(1m);
    DynamicVars.Block.UpgradeValueBy(3m);
    DynamicVars["Observation"].UpgradeValueBy(2m);
}
```

### 5. 出牌效果 `OnPlay`

每张有实际效果的牌通常重写：

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    await base.OnPlay(choiceContext, cardPlay);

    // 这里写实际效果
}
```

Renko 当前建议先调用 `await base.OnPlay(...)`，这样 `AbstractRenkoCard` 里预留的通用机制能正常生效。

如果是需要敌方目标的牌，先检查目标：

```csharp
ArgumentNullException.ThrowIfNull(cardPlay.Target);
```

`TargetType.Self` 的牌不要读 `cardPlay.Target`，直接用：

```csharp
Owner.Creature
```

### 6. 升级逻辑 `OnUpgrade`

升级伤害：

```csharp
protected override void OnUpgrade()
{
    DynamicVars.Damage.UpgradeValueBy(3m);
}
```

升级格挡：

```csharp
protected override void OnUpgrade()
{
    DynamicVars.Block.UpgradeValueBy(3m);
}
```

升级自定义数值：

```csharp
protected override void OnUpgrade()
{
    DynamicVars["Draw"].UpgradeValueBy(1m);
}
```

降低数值也可以，比如升级后消耗从 5 降到 3：

```csharp
protected override void OnUpgrade()
{
    DynamicVars["CostAmount"].UpgradeValueBy(-2m);
}
```

### 7. 标签 `CanonicalTags`

只有确实属于游戏内既有标签时才写。当前常用：

```csharp
protected override HashSet<CardTag> CanonicalTags =>
[
    CardTag.Strike,
];
```

```csharp
protected override HashSet<CardTag> CanonicalTags =>
[
    CardTag.Defend,
];
```

不要为了 Renko 自己的新分类硬加 `CardTag`。自定义分类更适合做成关键词、Power、接口或卡牌基类逻辑。

### 8. 格挡牌的 `GainsBlock`

如果这张牌会获得格挡，建议写：

```csharp
public override bool GainsBlock => true;
```

这能帮助游戏 UI、预览或其他系统识别这张牌是格挡牌。

### 9. 本地化和卡图

代码只定义逻辑。卡牌名称和描述还需要本地化文件。

一般需要在本地化里添加：

```json
{
  "RENKO-RENKO_XXX.title": "卡牌名",
  "RENKO-RENKO_XXX.description": "造成{Damage:diff()}点伤害。"
}
```

具体 key 要以运行时实际生成的 `Id.Entry` 为准。可以参考 HIRO 和 Manbo 的 `cards.json`。

卡图路径由 `AbstractRenkoCard.PortraitPath` 决定：

```text
res://Renko/images/cards/xxx.png
```

如果找不到，会 fallback 到：

```text
res://Renko/images/cards/card.png
```

## 最小模板

### 攻击牌模板

```csharp
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Renko.Scripts.Cards;

public class RenkoAttackExample : AbstractRenkoCard
{
    public RenkoAttackExample() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.OnPlay(choiceContext, cardPlay);

        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
```

### 防御牌模板

```csharp
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Renko.Scripts.Cards;

public class RenkoBlockExample : AbstractRenkoCard
{
    public RenkoBlockExample() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.OnPlay(choiceContext, cardPlay);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
```

## 常见特效函数速查

以下示例都默认写在 `OnPlay` 里。

### 造成单体伤害

```csharp
ArgumentNullException.ThrowIfNull(cardPlay.Target);

await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .WithHitFx("vfx/vfx_attack_slash")
    .Execute(choiceContext);
```

### 多段伤害

```csharp
ArgumentNullException.ThrowIfNull(cardPlay.Target);

await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .WithHitCount(3)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .WithHitFx("vfx/vfx_attack_slash")
    .Execute(choiceContext);
```

### 全体伤害

卡牌构造函数应使用：

```csharp
TargetType.AllEnemies
```

简单写法可以用 BaseLib：

```csharp
await CommonActions.CardAttack(this, cardPlay)
    .Execute(choiceContext);
```

需要添加：

```csharp
using BaseLib.Utils;
```

### 获得格挡

```csharp
await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
```

或者：

```csharp
await CommonActions.CardBlock(this, cardPlay);
```

### 同时攻击和格挡

```csharp
await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

ArgumentNullException.ThrowIfNull(cardPlay.Target);
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .Execute(choiceContext);
```

### 获得力量

```csharp
await PowerCmd.Apply<StrengthPower>(
    Owner.Creature,
    DynamicVars.Strength.BaseValue,
    Owner.Creature,
    this
);
```

需要 `CanonicalVars`：

```csharp
new PowerVar<StrengthPower>(2m)
```

### 获得敏捷

```csharp
await PowerCmd.Apply<DexterityPower>(
    Owner.Creature,
    DynamicVars.Power<DexterityPower>().BaseValue,
    Owner.Creature,
    this
);
```

需要：

```csharp
new PowerVar<DexterityPower>(2m)
```

### 给予敌人虚弱

```csharp
ArgumentNullException.ThrowIfNull(cardPlay.Target);

await PowerCmd.Apply<WeakPower>(
    cardPlay.Target,
    DynamicVars.Weak.BaseValue,
    Owner.Creature,
    this
);
```

需要：

```csharp
new PowerVar<WeakPower>(1m)
```

### 给予敌人易伤

```csharp
ArgumentNullException.ThrowIfNull(cardPlay.Target);

await PowerCmd.Apply<VulnerablePower>(
    cardPlay.Target,
    DynamicVars.Vulnerable.BaseValue,
    Owner.Creature,
    this
);
```

需要：

```csharp
new PowerVar<VulnerablePower>(1m)
```

### 给全体敌人易伤/虚弱

```csharp
await PowerCmd.Apply<VulnerablePower>(
    CombatState!.HittableEnemies,
    DynamicVars.Vulnerable.BaseValue,
    Owner.Creature,
    this
);
```

```csharp
await PowerCmd.Apply<WeakPower>(
    CombatState!.HittableEnemies,
    DynamicVars.Weak.BaseValue,
    Owner.Creature,
    this
);
```

### 施加自定义 buff

```csharp
await PowerCmd.Apply<ObservationPower>(
    Owner.Creature,
    DynamicVars["Observation"].BaseValue,
    Owner.Creature,
    this
);
```

如果目标是敌人：

```csharp
ArgumentNullException.ThrowIfNull(cardPlay.Target);

await PowerCmd.Apply<SomeDebuffPower>(
    cardPlay.Target,
    DynamicVars["DebuffAmount"].BaseValue,
    Owner.Creature,
    this
);
```

### 减少或消耗某个 buff 层数

```csharp
await PowerCmd.Apply<ObservationPower>(
    Owner.Creature,
    -DynamicVars["ObservationCost"].BaseValue,
    Owner.Creature,
    this
);
```

或者已有 power 实例时：

```csharp
var power = Owner.Creature.GetPower<ObservationPower>();
if (power != null)
{
    await PowerCmd.ModifyAmount(power, -1m, Owner.Creature, this);
}
```

### 移除某个 buff/debuff

```csharp
var power = Owner.Creature.GetPower<ObservationPower>();
if (power != null)
{
    await PowerCmd.Remove(power);
}
```

### 抽牌

```csharp
await CardPileCmd.Draw(choiceContext, DynamicVars["Draw"].IntValue, Owner);
```

需要：

```csharp
new DynamicVar("Draw", 2m)
```

### 获得能量

```csharp
await PlayerCmd.GainEnergy(DynamicVars["Energy"].IntValue, Owner);
```

需要：

```csharp
new DynamicVar("Energy", 1m)
```

### 获得金币

```csharp
await PlayerCmd.GainGold(DynamicVars["Gold"].IntValue, Owner);
```

需要：

```csharp
new DynamicVar("Gold", 10m)
```

### 回复生命

```csharp
await CreatureCmd.Heal(Owner.Creature, DynamicVars["Heal"].BaseValue);
```

需要：

```csharp
new DynamicVar("Heal", 4m)
```

### 眩晕敌人

```csharp
ArgumentNullException.ThrowIfNull(cardPlay.Target);

await CreatureCmd.Stun(cardPlay.Target);
```

### 击杀敌人

```csharp
ArgumentNullException.ThrowIfNull(cardPlay.Target);

await CreatureCmd.Kill(cardPlay.Target);
```

这类效果很强，建议只放在稀有牌或有严格条件的牌上。

### 播放角色动画

攻击动画：

```csharp
await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.AttackAnimDelay);
```

施法动画：

```csharp
await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
```

### 生成一张牌到手牌

```csharp
var card = ModelDb.Card<RenkoStrike>();

await CardPileCmd.AddGeneratedCardToCombat(
    card,
    PileType.Hand,
    addedByPlayer: true
);
```

常见目标牌堆：

```csharp
PileType.Hand
PileType.Draw
PileType.Discard
```

### 生成多张牌

```csharp
var cards = new List<CardModel>
{
    ModelDb.Card<RenkoStrike>(),
    ModelDb.Card<RenkoDefend>(),
};

await CardPileCmd.AddGeneratedCardsToCombat(
    cards,
    PileType.Hand,
    addedByPlayer: true
);
```

### 从战斗中移除一张牌

```csharp
await CardPileCmd.RemoveFromCombat(cardToRemove);
```

### 从牌堆选择牌

BaseLib 提供了简化方法：

```csharp
var selected = await CommonActions.SelectCards(
    this,
    "选择一张牌",
    choiceContext,
    PileType.Discard,
    count: 1
);
```

需要：

```csharp
using BaseLib.Utils;
```

### 条件发光

类似 Manbo 的 `ManboRelly`，如果满足条件让卡牌发金光：

```csharp
protected override bool ShouldGlowGoldInternal =>
    (Owner.Creature.GetPower<ObservationPower>()?.Amount ?? 0m) >= 5m;
```

实际效果仍然要在 `OnPlay` 里判断：

```csharp
if (ShouldGlowGoldInternal)
{
    await PowerCmd.Apply<StrengthPower>(Owner.Creature, 2m, Owner.Creature, this);
}
```

### 根据自身 buff 层数结算

```csharp
var amount = Owner.Creature.GetPower<ObservationPower>()?.Amount ?? 0m;

if (amount > 0)
{
    await DamageCmd.Attack(amount)
        .FromCard(this)
        .Targeting(cardPlay.Target)
        .Execute(choiceContext);
}
```

### 根据敌人 debuff 层数结算

```csharp
ArgumentNullException.ThrowIfNull(cardPlay.Target);

var weak = cardPlay.Target.GetPower<WeakPower>()?.Amount ?? 0m;
if (weak > 0)
{
    await DamageCmd.Attack(DynamicVars.Damage.BaseValue + weak)
        .FromCard(this)
        .Targeting(cardPlay.Target)
        .Execute(choiceContext);
}
```

## 常见 using

大多数卡牌会用到：

```csharp
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
```

如果用 BaseLib 简化函数：

```csharp
using BaseLib.Utils;
```

如果用自定义 Power：

```csharp
using Renko.Scripts.Powers;
```

## 常见错误

### 构造函数名和类名不一致

错误：

```csharp
public class RenkoMeiri : AbstractRenkoCard
{
    public RenkoStrike() : base(...)
    {
    }
}
```

正确：

```csharp
public class RenkoMeiri : AbstractRenkoCard
{
    public RenkoMeiri() : base(...)
    {
    }
}
```

### `TargetType.Self` 还读取 `cardPlay.Target`

`TargetType.Self` 的卡通常没有敌方目标，应使用：

```csharp
Owner.Creature
```

### 忘记 `await base.OnPlay(...)`

如果 Renko 后续把通用机制放进 `AbstractRenkoCard.OnPlay`，具体卡牌不调用 base 就吃不到这些通用效果。

### 声明了数值但描述没写

代码里有：

```csharp
new DamageVar(6m, ValueProp.Move)
```

本地化描述里要对应：

```text
造成{Damage:diff()}点伤害。
```

自定义变量也一样：

```csharp
new DynamicVar("Draw", 2m)
```

描述：

```text
抽{Draw:diff()}张牌。
```

### 用错 `BaseValue` 和 `IntValue`

大多数命令接受 `decimal` 时用：

```csharp
BaseValue
```

需要整数数量时用：

```csharp
IntValue
```

例如抽牌张数、获得能量通常用 `IntValue`。
