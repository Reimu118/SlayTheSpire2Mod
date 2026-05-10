using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MoeNegiMod.Manbo.Extensions;
using MoeNegiMod.Manbo.Cards;
using System;
using MoeNegiMod.Manbo.Relics;

namespace Renko.Scipts.Characters;

// PlaceholderCharacterModel为预设类,部分资源未定义则转到官方资源文件
// 真自定义类为CustomCharacterModel
public class RenkoCharacters : PlaceholderCharacterModel
{
	public const string CharacterId = "Renko";//非强制要求,用于后面卡池,遗物池的命名
	// 角色选择背景资源,godot,后期补
	//public override string CustomCharacterSelectBg => "res://Selphina/Scenes/Char_Select/char_select_bg_selphina.tscn";
	public override string PlaceholderID => "necrobinder";//人物素材路径,暂时借用原版
	public static readonly Color Color = new Color("c4278a");//角色主题颜色,后续方便通过Renko.Color调用
	public override Color NameColor => Color;
	public override CharacterGender Gender => CharacterGender.Feminine;//角色性别属性Masculine男,Neuter中立,Feminine女
	public override int StartingHp => 70;//角色初始最大生命值

	// 初始卡组,手动实现,通过ModelDb.Card自动New出来
	public override IEnumerable<CardModel> StartingDeck => [
		ModelDb.Card<RenkoStrike>(),
		ModelDb.Card<RenkoStrike>(),
		ModelDb.Card<RenkoDefend>(),
		ModelDb.Card<RenkoDefend>(),
		ModelDb.Card<RenkoMeiri>(),
		ModelDb.Card<RenkoPowerUp>()
	];

	public override IReadOnlyList<RelicModel> StartingRelics => [
		ModelDb.Relic<ManboToy>()
	];//初始遗物

	public override CardPoolModel CardPool => ModelDb.CardPool<ManboCardPool>();
	public override RelicPoolModel RelicPool => ModelDb.RelicPool<ManboRelicPool>();
	public override PotionPoolModel PotionPool => ModelDb.PotionPool<SharedPotionPool>();

	/*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
		override all the other methods that define those assets.
		These are just some of the simplest assets, given some placeholders to differentiate your character with.
		You don't have to, but you're suggested to rename these images. */
	public override string CustomVisualPath => "res://Manbo/Scenes/ManboVisual.tscn";
	public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
	public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
	public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
	public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
}
