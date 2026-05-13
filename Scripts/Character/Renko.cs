using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using Renko.Scripts.Cards;
using Renko.Scripts.Pools;
using System;

namespace Renko.Scripts.Character;

// PlaceholderCharacterModel 是占位角色基类。
// 没有手动指定的角色资源，会根据 PlaceholderID 借用游戏内已有角色资源。
// 如果后续要完全自定义立绘、选择界面、能量 UI、音效等资源，可以改用 CustomCharacterModel。
public class RenkoCharacter : PlaceholderCharacterModel {
	// 非强制字段。用于统一保存角色 ID。
	// 后续卡池、遗物池、本地化、资源路径等地方可以复用，避免到处手写 "Renko"。
	public const string CharacterId = "Renko";

	// 角色选择界面背景。需要 Godot .tscn 场景资源。
	// 现在没有自己的资源时先注释掉，否则游戏会尝试加载这个路径。
	//public override string CustomCharacterSelectBg => "res://Selphina/Scenes/Char_Select/char_select_bg_selphina.tscn";

	// 占位资源 ID。
	// PlaceholderCharacterModel 会借用该 ID 对应的角色模型、图标、能量 UI、选择界面背景、音效等资源。
	// 这不是 Renko 的角色 ID，只是临时借用资源用的名字。
	public override string PlaceholderID => "necrobinder";

	// 角色主题色。
	// 后续可以通过 RenkoCharacters.Color 复用到名字颜色、卡池颜色、能量边框等地方。
	public static readonly Color Color = new Color("c4278a");

	// 角色名字显示颜色。
	public override Color NameColor => Color;

	// 角色性别。主要影响本地化文本里的称呼/代词，不影响模型外观。
	// 可选值通常是 Neutral、Feminine、Masculine。
	public override CharacterGender Gender => CharacterGender.Feminine;

	// 开局最大生命值。新开一局时通常是当前生命和最大生命都等于这个值。
	public override int StartingHp => 70;

	// 开局牌组。
	// 这里列出的卡会在新开一局时直接加入角色牌组。
	// ModelDb.Card<T>() 表示从游戏模型数据库里获取 T 这张卡的模型。
	public override IEnumerable<CardModel> StartingDeck => [
		ModelDb.Card<RenkoStrike>(),
		ModelDb.Card<RenkoStrike>(),
		ModelDb.Card<RenkoDefend>(),
		ModelDb.Card<RenkoDefend>(),
		ModelDb.Card<RenkoMeiri>(),
		ModelDb.Card<RenkoPowerUp>()
	];

	// 初始遗物。超统一物理学教材
	public override IReadOnlyList<RelicModel> StartingRelics => [
		ModelDb.Relic<UnifiedPhysicsBook>()
	];

	// 角色专属卡池。
	// 决定这个角色后续奖励、图鉴、卡牌池里有哪些牌。
	public override CardPoolModel CardPool => ModelDb.CardPool<RenkoCardPool>();

	// 角色专属遗物池。
	public override RelicPoolModel RelicPool => ModelDb.RelicPool<RenkoRelicPool>();

	// 角色药水池。
	public override PotionPoolModel PotionPool => ModelDb.PotionPool<RenkoPotionPool>();

	// 以下是自定义资源路径。
	// 如果不重写这些属性，PlaceholderCharacterModel 会根据 PlaceholderID 自动提供占位资源。
	// 当前这些路径仍然指向教程里的 Manbo 资源，后续应该替换成 Renko 自己的资源，或者先注释掉使用占位资源。

	// 战斗中的角色视觉场景。
	//public override string CustomVisualPath => "res://Manbo/Scenes/ManboVisual.tscn";

	// 角色小头像贴图，例如存档信息、顶部面板等位置可能会用到。
	//public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();

	// 角色选择界面图标。
	//public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();

	// 角色选择界面锁定状态图标。
	//public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();

	// 地图上的角色标记图标。
	//public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
}
