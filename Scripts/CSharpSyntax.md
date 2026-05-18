# C# 语法笔记：StS2 Mod 开发常用部分

这份笔记只记录当前制作 Renko mod 时最常见、最容易困惑的 C# 语法。示例尽量贴近卡牌、Power、DynamicVar、角色和遗物代码。

## 1. namespace：代码所在的命名空间

```csharp
namespace Renko.Scripts.Cards;
```

`namespace` 用来给类分组，避免不同 mod 或不同模块里的类重名。

例如：

```csharp
namespace Renko.Scripts.Cards;

public class RenkoStrike : AbstractRenkoCard
{
}
```

其他文件要使用它时，可以写：

```csharp
using Renko.Scripts.Cards;
```

常见命名空间规划：

```csharp
Renko.Scripts.Cards
Renko.Scripts.Powers
Renko.Scripts.Relics
Renko.Scripts.Character
Renko.Scripts.Pools
Renko.Scripts.RenkoVar
```

## 2. using：引入别的命名空间

```csharp
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
```

`using` 的作用是让你可以直接写类名。

有 `using MegaCrit.Sts2.Core.Commands;` 时：

```csharp
await CreatureCmd.GainBlock(...);
```

如果没有 `using`，理论上要写完整路径，会很长。

## 3. class：定义一个类

```csharp
public class RenkoStrike : AbstractRenkoCard
{
}
```

这表示：

- `public`：这个类可以被其他文件访问。
- `class`：定义一个类。
- `RenkoStrike`：类名。
- `: AbstractRenkoCard`：继承 `AbstractRenkoCard`。

继承后，`RenkoStrike` 就拥有 `AbstractRenkoCard` 和更上层 `CustomCardModel` 的能力，并且可以重写其中的属性和方法。

## 4. 继承：`: 父类`

```csharp
public class ObservationVar : DynamicVar
```

意思是：`ObservationVar` 是一种 `DynamicVar`。

所以它可以被放进：

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    new ObservationVar(3m)
];
```

因为 `ObservationVar` 继承了 `DynamicVar`，所以它可以当作 `DynamicVar` 使用。

这就是“子类可以作为父类使用”的意思。

## 5. 构造函数：创建对象时执行

```csharp
public RenkoStrike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
}
```

这是 `RenkoStrike` 的构造函数。

构造函数特点：

- 名字必须和类名一样。
- 没有返回值，不能写 `void`。
- `new RenkoStrike()` 时会执行。

错误示例：

```csharp
public class RenkoMeiri : AbstractRenkoCard
{
    public RenkoStrike() : base(...)
    {
    }
}
```

类名是 `RenkoMeiri`，构造函数却叫 `RenkoStrike`，这会编译失败。

正确：

```csharp
public class RenkoMeiri : AbstractRenkoCard
{
    public RenkoMeiri() : base(...)
    {
    }
}
```

## 6. base：调用父类

### 构造函数里的 base

```csharp
public RenkoDefend() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
}
```

这里的 `base(...)` 是调用父类 `AbstractRenkoCard` 的构造函数。

也就是把费用、类型、稀有度、目标类型交给父类初始化。

### 方法里的 base

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    await base.OnPlay(choiceContext, cardPlay);

    await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
}
```

`base.OnPlay(...)` 表示先执行父类的出牌逻辑。

如果 Renko 后续把通用机制写进 `AbstractRenkoCard.OnPlay`，每张卡调用 `base.OnPlay(...)` 才能吃到这些通用机制。

## 7. override：重写父类成员

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    new DamageVar(6m, ValueProp.Move)
];
```

`override` 表示：父类里已经有这个属性或方法，当前类要替换它的实现。

常见重写：

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars => ...

protected override async Task OnPlay(...) { ... }

protected override void OnUpgrade() { ... }

public override bool GainsBlock => true;
```

没有父类允许重写的成员时，不能随便写 `override`。

## 8. 访问修饰符：public / protected / private

### public

```csharp
public class RenkoStrike : AbstractRenkoCard
```

`public` 表示其他地方可以访问。

卡牌类、Power 类、角色类通常要 `public`，因为 BaseLib/游戏会通过反射或模型数据库找到它们。

### protected

```csharp
protected override void OnUpgrade()
```

`protected` 表示只有当前类和子类能访问。

很多游戏模型的钩子方法都是 `protected override`，因为它们不应该被外部随便调用。

### private

```csharp
private bool IsTemporaryChoiceCard(string cardId)
```

`private` 表示只有当前类内部能访问。

工具方法、不想暴露的内部状态适合用 `private`。

## 9. const / static readonly：常量和静态只读字段

```csharp
public const string Key = "Renko-Observation";
public static readonly string LocKey = Key.ToUpperInvariant();
```

### const

```csharp
public const string Key = "Renko-Observation";
```

`const` 是编译期常量，值必须在代码里直接写死。

适合不会变化的字符串 key。

这里的 `Key` 用途是统一索引：

```csharp
DynamicVars[ObservationVar.Key].BaseValue
```

等价于：

```csharp
DynamicVars["Renko-Observation"].BaseValue
```

好处是以后不需要到处手写字符串，减少拼写错误。

### static readonly

```csharp
public static readonly string LocKey = Key.ToUpperInvariant();
```

`static` 表示这个字段属于类本身，不属于某个对象。

所以可以这样访问：

```csharp
ObservationVar.LocKey
```

`readonly` 表示初始化后不能再改。

`Key.ToUpperInvariant()` 是运行时计算出来的，不是直接写死的字面量，所以不能用 `const`，适合用 `static readonly`。

结果是：

```text
Renko-Observation -> RENKO-OBSERVATION
```

它通常对应本地化 key：

```json
{
  "RENKO-OBSERVATION.title": "观测",
  "RENKO-OBSERVATION.description": "观测的说明文本。"
}
```

## 10. 属性表达式：`=>`

```csharp
public override bool GainsBlock => true;
```

等价于：

```csharp
public override bool GainsBlock
{
    get
    {
        return true;
    }
}
```

短属性常用 `=>`。

例如：

```csharp
protected override HashSet<CardTag> CanonicalTags =>
[
    CardTag.Strike,
];
```

表示这个属性返回一个 `HashSet<CardTag>`。

## 11. 集合表达式：`[...]`

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    new DamageVar(6m, ValueProp.Move),
    new BlockVar(5m, ValueProp.Move)
];
```

`[...]` 是 C# 的集合表达式，表示创建一个集合。

它可以返回给：

```csharp
IEnumerable<DynamicVar>
HashSet<CardTag>
IReadOnlyList<RelicModel>
```

例如：

```csharp
protected override HashSet<CardTag> CanonicalTags =>
[
    CardTag.Strike,
];
```

## 12. 泛型：`PowerVar<T>` / `ModelDb.Card<T>()`

泛型可以理解为“把类型作为参数传进去”。

```csharp
new PowerVar<StrengthPower>(2m)
```

意思是：创建一个和 `StrengthPower` 绑定的动态数值。

```csharp
DynamicVars.Power<StrengthPower>().BaseValue
```

意思是：从动态变量里读取 `StrengthPower` 对应的数值。

```csharp
ModelDb.Card<RenkoStrike>()
```

意思是：从模型数据库中取出 `RenkoStrike` 这张卡。

```csharp
PowerCmd.Apply<StrengthPower>(...)
```

意思是：施加 `StrengthPower` 这个 Power。

## 13. new：创建对象

```csharp
new DamageVar(6m, ValueProp.Move)
new BlockVar(5m, ValueProp.Move)
new ObservationVar(3m)
```

`new` 会创建一个对象，并调用它的构造函数。

例如：

```csharp
new ObservationVar(3m)
```

会执行：

```csharp
public ObservationVar(decimal baseValue) : base(Key, baseValue)
{
    this.WithTooltip(LocKey);
}
```

## 14. decimal 和 `m` 后缀

```csharp
6m
3m
0.1m
```

`m` 表示这是 `decimal` 类型。

StS2 的很多数值系统使用 `decimal`，所以卡牌数值通常写成：

```csharp
new DamageVar(6m, ValueProp.Move)
DynamicVars.Damage.UpgradeValueBy(3m)
```

如果写 `6`，它是 `int`。很多地方也能自动转换，但写 `6m` 更清楚。

## 15. async / await / Task：异步命令

很多游戏命令是异步的，因为它们要进入战斗动作队列、播放动画或等待流程完成。

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    await base.OnPlay(choiceContext, cardPlay);
    await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
}
```

含义：

- `async`：这个方法里可以使用 `await`。
- `Task`：这个方法异步执行，不直接返回普通值。
- `await`：等待这个命令执行完成后，再继续下一行。

如果调用返回 `Task` 的命令，通常要 `await`：

```csharp
await DamageCmd.Attack(...).Execute(choiceContext);
await PowerCmd.Apply<StrengthPower>(...);
await CardPileCmd.Draw(...);
```

## 16. nullable：`?` 和空值检查

```csharp
cardPlay.Target
```

有些目标可能是 `null`。比如 `TargetType.Self` 或 `TargetType.None` 的牌通常没有敌方目标。

对于必须有目标的攻击牌，建议写：

```csharp
ArgumentNullException.ThrowIfNull(cardPlay.Target);
```

检查之后再用：

```csharp
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .Execute(choiceContext);
```

`?.` 是空安全访问：

```csharp
Owner.Creature.GetPower<ObservationPower>()?.Amount
```

如果 `GetPower<ObservationPower>()` 返回 `null`，后面的 `.Amount` 不会执行，整体结果也是 `null`。

`??` 是空值兜底：

```csharp
Owner.Creature.GetPower<ObservationPower>()?.Amount ?? 0m
```

意思是：如果左边是 `null`，就用 `0m`。

## 17. 字典索引：`DynamicVars[Key]`

```csharp
DynamicVars[ObservationVar.Key].BaseValue
```

`DynamicVars` 类似一个字典，可以通过字符串 key 找到对应的动态变量。

这就是为什么要写：

```csharp
public const string Key = "Renko-Observation";
```

它不是“给父类作为索引”，更准确地说是：**给 `DynamicVars` 这类字典式集合当索引 key**。

声明变量时：

```csharp
new ObservationVar(3m)
```

内部等价于：

```csharp
new DynamicVar("Renko-Observation", 3m)
```

读取变量时：

```csharp
DynamicVars[ObservationVar.Key]
```

等价于：

```csharp
DynamicVars["Renko-Observation"]
```

## 18. this：当前对象

```csharp
this.WithTooltip(LocKey);
```

`this` 表示当前对象。

在 `ObservationVar` 构造函数里：

```csharp
public ObservationVar(decimal baseValue) : base(Key, baseValue)
{
    this.WithTooltip(LocKey);
}
```

`this` 就是刚创建出来的这个 `ObservationVar`。

卡牌里也经常传 `this`：

```csharp
await PowerCmd.Apply<StrengthPower>(
    Owner.Creature,
    DynamicVars.Strength.BaseValue,
    Owner.Creature,
    this
);
```

这里的 `this` 表示当前这张卡，告诉游戏这个 Power 是由哪张卡施加的。

## 19. 链式调用

```csharp
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .WithHitFx("vfx/vfx_attack_slash")
    .Execute(choiceContext);
```

这是链式调用。

可以拆开理解：

```csharp
var command = DamageCmd.Attack(DynamicVars.Damage.BaseValue);
command.FromCard(this);
command.Targeting(cardPlay.Target);
command.WithHitFx("vfx/vfx_attack_slash");
await command.Execute(choiceContext);
```

链式写法更紧凑，也更像“配置一个攻击命令，然后执行”。

## 20. lambda：`=>` 作为匿名函数

有时 `=>` 不是属性表达式，而是 lambda。

例如：

```csharp
cards.OrderBy(c => c.Rarity)
```

意思是：对每张卡 `c`，取它的 `Rarity` 作为排序依据。

在当前 Renko 卡牌里暂时不多见，但后续做选牌、筛牌、统计牌堆时会常用。

## 21. 特性 Attribute：`[Pool(...)]`

```csharp
[Pool(typeof(RenkoCardPool))]
public abstract class AbstractRenkoCard : CustomCardModel
{
}
```

`[...]` 放在类或方法上时，通常是 Attribute。

这里的 `[Pool(typeof(RenkoCardPool))]` 告诉 BaseLib：继承这个基类的卡牌属于 `RenkoCardPool`。

`typeof(RenkoCardPool)` 表示拿到 `RenkoCardPool` 这个类型本身，而不是创建一个对象。

## 22. 常用卡牌文件骨架

```csharp
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Renko.Scripts.Cards;

public class RenkoExample : AbstractRenkoCard
{
    public RenkoExample() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8m, ValueProp.Move),
        new CardsVar(1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.OnPlay(choiceContext, cardPlay);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
```

