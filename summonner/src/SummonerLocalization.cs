using System.Text;
using ModShardLauncher;
using ModShardLauncher.Mods;
using Summoner.Skills;

namespace Summoner;

/// <summary>
/// Injects Summoner school / skill / buff localization.
/// Top languages (same as Necromancy): EN, RU, ZH, DE, JA.
/// CSV columns: id;RU;EN;ZH;DE;ES;FR;IT;PT;PL;TR;JA;KO
/// </summary>
public static class SummonerLocalization
{
    static readonly List<(string Id, Dictionary<ModLanguage, string> Name, Dictionary<ModLanguage, string> Desc)> PendingSkills = new();
    static readonly List<(string Id, Dictionary<ModLanguage, string> Name, Dictionary<ModLanguage, string> Desc)> PendingBuffs = new();

    public static void PatchAll()
    {
        PendingSkills.Clear();
        PendingBuffs.Clear();
        RegisterAllSkills();
        RegisterAllBuffs();
        PatchSchool();

        Msl.LoadGML("gml_GlobalScript_table_skills")
            .Apply(InjectTableSkillsRows)
            .Save();

        Msl.LoadGML("gml_GlobalScript_table_effects")
            .Apply(InjectTableEffectsRows)
            .Save();
    }

    static void RegisterAllSkills()
    {
        RegisterSkill(
            "summon_wolf",
            Loc(
                en: "Summon Wolf",
                ru: "Призыв волка",
                zh: "召唤狼",
                de: "Wolf beschwören",
                ja: "狼の召喚"),
            Loc(
                en: "Summons your bonded ~lg~Wolf~/~ on the targeted tile.##The Wolf keeps its ~lg~own~/~ Experience and level (separate from other companions) when dismissed or when it dies.##Each Wolf level raises Max Health, Damage, and core attributes.##Base limit: ~w~1~/~ summon at a time.##If the Wolf dies in combat, you suffer ~r~-6 Morale~/~ for ~w~600~/~ turns (does not stack).",
                ru: "Призывает связанного ~lg~Волка~/~ на выбранную клетку.##Волк сохраняет ~lg~свой~/~ опыт и уровень (отдельно от других компаньонов) при роспуске и при смерти.##Каждый уровень Волка повышает макс. здоровье, урон и основные характеристики.##Базовый лимит: ~w~1~/~ призыв одновременно.##Если Волк погибает в бою, вы получаете ~r~-6 Морали~/~ на ~w~600~/~ ходов (не стакается).",
                zh: "在目标格子召唤你羁绊的~lg~狼~/~。##解散或死亡时，狼保留~lg~自身~/~经验与等级（与其他同伴分开）。##每级提升最大生命、伤害与核心属性。##基础上限：同时~w~1~/~只召唤物。##狼在战斗中死亡时，你承受~r~-6士气~/~，持续~w~600~/~回合（不叠加）。",
                de: "Beschwört deinen gebundenen ~lg~Wolf~/~ auf dem Zielfeld.##Der Wolf behält seine ~lg~eigene~/~ Erfahrung und Stufe (getrennt von anderen Gefährten) beim Entlassen und beim Tod.##Jede Wolfstufe erhöht max. Gesundheit, Schaden und Kernattribute.##Basislimit: ~w~1~/~ Beschwörung gleichzeitig.##Stirbt der Wolf im Kampf, erleidest du ~r~-6 Moral~/~ für ~w~600~/~ Züge (stapelt nicht).",
                ja: "対象マスに絆を結んだ~lg~狼~/~を召喚する。##解散時も死亡時も、狼は他の仲間と別の~lg~独自~/~経験とレベルを保持する。##レベルごとに最大体力・ダメージ・基本能力が上がる。##基本上限：同時に召喚~w~1~/~体。##戦闘で狼が死ぬと~r~士気-6~/~を~w~600~/~ターン受ける（重複しない）。"));

        RegisterSkill(
            "unsummon",
            Loc(
                en: "Unsummon",
                ru: "Распустить",
                zh: "解散召唤",
                de: "Entlassen",
                ja: "召喚解除"),
            Loc(
                en: "Safely dismisses a targeted allied summon (or the nearest one).##That companion's ~lg~own~/~ Experience and level are kept.##Does ~lg~not~/~ apply the Morale penalty that occurs when a summon dies in combat.",
                ru: "Безопасно распускает выбранного союзного призыва (или ближайшего).##~lg~Свой~/~ опыт и уровень компаньона сохраняются.##~lg~Не~/~ накладывает штраф Морали, как при гибели призыва в бою.",
                zh: "安全解散目标友方召唤物（或最近的一只）。##该同伴的~lg~自身~/~经验与等级会保留。##~lg~不会~/~施加战斗中死亡时的士气惩罚。",
                de: "Entlässt sicher eine gezielte verbündete Beschwörung (oder die nächste).##Die ~lg~eigene~/~ Erfahrung und Stufe des Gefährten bleiben erhalten.##Verursacht ~lg~nicht~/~ die Moralstrafe, die bei Tod im Kampf entsteht.",
                ja: "対象の味方召喚（または最も近い一体）を安全に解散する。##その仲間の~lg~独自~/~経験とレベルは保持される。##戦闘死時の士気ペナルティは~lg~発生しない~/~。"));

        RegisterSkill(
            "hunt_command",
            Loc(
                en: "Hunt Command",
                ru: "Приказ охоты",
                zh: "狩猎指令",
                de: "Jagdkommando",
                ja: "狩りの号令"),
            Loc(
                en: "Marks an enemy for ~w~6~/~ turns.##Your summons gain ~lg~+12%~/~ Weapon Damage and ~lg~+10%~/~ Accuracy against the marked target.",
                ru: "Помечает врага на ~w~6~/~ ходов.##Ваши призывы получают ~lg~+12%~/~ урона оружием и ~lg~+10%~/~ точности против помеченной цели.",
                zh: "标记一名敌人，持续~w~6~/~回合。##你的召唤物对标记目标获得~lg~+12%~/~武器伤害与~lg~+10%~/~命中。",
                de: "Markiert einen Feind für ~w~6~/~ Züge.##Deine Beschwörungen erhalten ~lg~+12%~/~ Waffenschaden und ~lg~+10%~/~ Trefferchance gegen das markierte Ziel.",
                ja: "敵に~w~6~/~ターンの印を付ける。##召喚体は印の付いた対象に対し~lg~武器ダメージ+12%~/~と~lg~命中+10%~/~を得る。"));

        RegisterSkill(
            "mend_pack",
            Loc(
                en: "Mend Pack",
                ru: "Исцеление стаи",
                zh: "治愈兽群",
                de: "Rudel heilen",
                ja: "群れの癒やし"),
            Loc(
                en: "Restores Health to all active summons within Vision (scales with Magic Power / Willpower).##Also clears ~lg~1~/~ minor injury stage on each healed companion.",
                ru: "Восстанавливает здоровье всем активным призывам в зоне видимости (зависит от силы магии / Воли).##Также снимает ~lg~1~/~ ступень лёгкой травмы с каждого исцелённого компаньона.",
                zh: "恢复视野内所有活跃召唤物的生命（随魔法强度/意志缩放）。##并对每个被治疗的同伴清除~lg~1~/~级轻微伤势。",
                de: "Stellt allen aktiven Beschwörungen in Sichtweite Gesundheit wieder her (skaliert mit Magiekraft / Willenskraft).##Entfernt außerdem ~lg~1~/~ leichte Verletzungsstufe bei jedem geheilten Gefährten.",
                ja: "視界内の全召喚体の体力を回復する（魔法威力/意志で変動）。##回復した各仲間の軽傷段階を~lg~1~/~段階取り除く。"));

        RegisterSkill(
            "pack_instincts",
            Loc(
                en: "Pack Instincts",
                ru: "Инстинкты стаи",
                zh: "兽群本能",
                de: "Rudelinstinkte",
                ja: "群れの本能"),
            Loc(
                en: "Increases your summon limit by ~lg~+1~/~.##All summons gain ~lg~+4%~/~ Max Health and ~lg~+3%~/~ Weapon Damage.##Each companion's Experience gain is increased by ~lg~10%~/~ (still tracked per species).",
                ru: "Увеличивает лимит призывов на ~lg~+1~/~.##Все призывы получают ~lg~+4%~/~ макс. здоровья и ~lg~+3%~/~ урона оружием.##Прирост опыта каждого компаньона увеличен на ~lg~10%~/~ (по-прежнему отдельно по видам).",
                zh: "召唤上限~lg~+1~/~。##所有召唤物获得~lg~+4%~/~最大生命与~lg~+3%~/~武器伤害。##每个同伴的经验获取~lg~+10%~/~（仍按物种分别计算）。",
                de: "Erhöht dein Beschwörungslimit um ~lg~+1~/~.##Alle Beschwörungen erhalten ~lg~+4%~/~ max. Gesundheit und ~lg~+3%~/~ Waffenschaden.##Der Erfahrungsgewinn jedes Gefährten steigt um ~lg~10%~/~ (weiterhin pro Art).",
                ja: "召喚上限が~lg~+1~/~。##全召喚体が~lg~最大体力+4%~/~と~lg~武器ダメージ+3%~/~を得る。##各仲間の経験獲得量が~lg~+10%~/~（種ごとに別管理のまま）。"));

        RegisterSkill(
            "alphas_howl",
            Loc(
                en: "Alpha's Howl",
                ru: "Вой альфы",
                zh: "头狼之嚎",
                de: "Heulen des Alphas",
                ja: "アルファの遠吠え"),
            Loc(
                en: "Your summons enter ~lg~\"Alpha's Howl\"~/~ for ~w~8~/~ turns:##~lg~+10%~/~ Weapon Damage#~lg~+5%~/~ Crit Chance#~lg~+10%~/~ Move Resistance##Enemies adjacent to any summon have a chance to become ~r~Hesitant~/~ for ~w~1~/~ turn.",
                ru: "Ваши призывы получают ~lg~«Вой альфы»~/~ на ~w~8~/~ ходов:##~lg~+10%~/~ урона оружием#~lg~+5%~/~ шанса крита#~lg~+10%~/~ сопротивления перемещению##Враги рядом с любым призывом могут стать ~r~Нерешительными~/~ на ~w~1~/~ ход.",
                zh: "你的召唤物进入~lg~“头狼之嚎”~/~，持续~w~8~/~回合：##~lg~+10%~/~武器伤害#~lg~+5%~/~暴击几率#~lg~+10%~/~移动抗性##相邻任何召唤物的敌人有几率陷入~r~犹豫~/~，持续~w~1~/~回合。",
                de: "Deine Beschwörungen erhalten ~lg~„Heulen des Alphas“~/~ für ~w~8~/~ Züge:##~lg~+10%~/~ Waffenschaden#~lg~+5%~/~ Krit-Chance#~lg~+10%~/~ Bewegungsresistenz##Feinde neben einer Beschwörung können ~r~Zögerlich~/~ für ~w~1~/~ Zug werden.",
                ja: "召喚体が~lg~「アルファの遠吠え」~/~を~w~8~/~ターン得る：##~lg~武器ダメージ+10%~/~#~lg~クリティカル率+5%~/~#~lg~移動耐性+10%~/~##いずれかの召喚に隣接する敵は~w~1~/~ターン~r~ためらい~/~になることがある。"));

        RegisterSkill(
            "summon_crawler",
            Loc(
                en: "Summon Crawler",
                ru: "Призыв ползуна",
                zh: "召唤爬行者",
                de: "Kriecher beschwören",
                ja: "クローラーの召喚"),
            Loc(
                en: "Summons your bonded ~lg~Crawler~/~ on the targeted tile.##Keeps its ~lg~own~/~ Experience and level, independent of Wolf / Bear / Bison / Gulon.##Ambusher: higher Damage, can apply ~r~Weakness~/~ or slow.##Death in combat causes ~r~-8 Morale~/~ for ~w~600~/~ turns (does not stack).",
                ru: "Призывает связанного ~lg~Ползуна~/~ на выбранную клетку.##Сохраняет ~lg~свой~/~ опыт и уровень, отдельно от Волка / Медведя / Бизона / Гулона.##Засадник: выше урон, может накладывать ~r~Слабость~/~ или замедление.##Смерть в бою даёт ~r~-8 Морали~/~ на ~w~600~/~ ходов (не стакается).",
                zh: "在目标格子召唤你羁绊的~lg~爬行者~/~。##保留~lg~自身~/~经验与等级，独立于狼/熊/野牛/狼獾。##伏击者：伤害更高，可施加~r~虚弱~/~或减速。##战斗死亡造成~r~-8士气~/~，持续~w~600~/~回合（不叠加）。",
                de: "Beschwört deinen gebundenen ~lg~Kriecher~/~ auf dem Zielfeld.##Behält seine ~lg~eigene~/~ Erfahrung und Stufe, unabhängig von Wolf / Bär / Bison / Gulon.##Lauerer: höherer Schaden, kann ~r~Schwäche~/~ oder Verlangsamung verleihen.##Tod im Kampf verursacht ~r~-8 Moral~/~ für ~w~600~/~ Züge (stapelt nicht).",
                ja: "対象マスに絆を結んだ~lg~クローラー~/~を召喚する。##狼/熊/バイソン/グロンとは別の~lg~独自~/~経験とレベルを保持する。##奇襲型：ダメージが高く、~r~弱体~/~や減速を付与できる。##戦闘死で~r~士気-8~/~を~w~600~/~ターン（重複しない）。"));

        RegisterSkill(
            "summon_bear",
            Loc(
                en: "Summon Bear",
                ru: "Призыв медведя",
                zh: "召唤熊",
                de: "Bär beschwören",
                ja: "熊の召喚"),
            Loc(
                en: "Summons your bonded ~lg~Bear~/~ on the targeted tile.##Keeps its ~lg~own~/~ Experience and level, independent of other companions.##Heavy fighter: solid Health and Damage; strong Knockback (less bulk than Bison).##Death in combat causes ~r~-10 Morale~/~ for ~w~600~/~ turns (does not stack).",
                ru: "Призывает связанного ~lg~Медведя~/~ на выбранную клетку.##Сохраняет ~lg~свой~/~ опыт и уровень, отдельно от других компаньонов.##Тяжёлый боец: крепкое здоровье и урон; сильный отброс (меньше массы, чем у Бизона).##Смерть в бою даёт ~r~-10 Морали~/~ на ~w~600~/~ ходов (не стакается).",
                zh: "在目标格子召唤你羁绊的~lg~熊~/~。##保留~lg~自身~/~经验与等级，独立于其他同伴。##重型斗士：生命与伤害扎实；击退强（体型不如野牛）。##战斗死亡造成~r~-10士气~/~，持续~w~600~/~回合（不叠加）。",
                de: "Beschwört deinen gebundenen ~lg~Bären~/~ auf dem Zielfeld.##Behält seine ~lg~eigene~/~ Erfahrung und Stufe, unabhängig von anderen Gefährten.##Schwerer Kämpfer: solide Gesundheit und Schaden; starker Rückstoß (weniger Masse als Bison).##Tod im Kampf verursacht ~r~-10 Moral~/~ für ~w~600~/~ Züge (stapelt nicht).",
                ja: "対象マスに絆を結んだ~lg~熊~/~を召喚する。##他の仲間とは別の~lg~独自~/~経験とレベルを保持する。##重戦士：体力とダメージが堅実；強いノックバック（バイソンより軽量）。##戦闘死で~r~士気-10~/~を~w~600~/~ターン（重複しない）。"));

        RegisterSkill(
            "summon_bison",
            Loc(
                en: "Summon Bison",
                ru: "Призыв бизона",
                zh: "召唤野牛",
                de: "Bison beschwören",
                ja: "バイソンの召喚"),
            Loc(
                en: "Summons your bonded ~lg~Bison~/~ on the targeted tile.##Keeps its ~lg~own~/~ Experience and level, independent of other companions.##Tanky bruiser: highest Max Health among companions, lower Damage; strong Knockback; lower mobility.##Death in combat causes ~r~-10 Morale~/~ for ~w~600~/~ turns (does not stack).",
                ru: "Призывает связанного ~lg~Бизона~/~ на выбранную клетку.##Сохраняет ~lg~свой~/~ опыт и уровень, отдельно от других компаньонов.##Танк-громила: наибольшее макс. здоровье среди компаньонов, ниже урон; сильный отброс; ниже мобильность.##Смерть в бою даёт ~r~-10 Морали~/~ на ~w~600~/~ ходов (не стакается).",
                zh: "在目标格子召唤你羁绊的~lg~野牛~/~。##保留~lg~自身~/~经验与等级，独立于其他同伴。##坦克猛兽：同伴中最高最大生命、伤害较低；击退强；机动性较低。##战斗死亡造成~r~-10士气~/~，持续~w~600~/~回合（不叠加）。",
                de: "Beschwört deinen gebundenen ~lg~Bison~/~ auf dem Zielfeld.##Behält seine ~lg~eigene~/~ Erfahrung und Stufe, unabhängig von anderen Gefährten.##Tankiger Schläger: höchste max. Gesundheit unter den Gefährten, weniger Schaden; starker Rückstoß; geringere Mobilität.##Tod im Kampf verursacht ~r~-10 Moral~/~ für ~w~600~/~ Züge (stapelt nicht).",
                ja: "対象マスに絆を結んだ~lg~バイソン~/~を召喚する。##他の仲間とは別の~lg~独自~/~経験とレベルを保持する。##タンク型：仲間中最高の最大体力、ダメージは低め；強いノックバック；機動力は低め。##戦闘死で~r~士気-10~/~を~w~600~/~ターン（重複しない）。"));

        RegisterSkill(
            "savage_bond",
            Loc(
                en: "Savage Bond",
                ru: "Дикая связь",
                zh: "野性羁绊",
                de: "Wilder Bund",
                ja: "野性の絆"),
            Loc(
                en: "For ~w~10~/~ turns, you and your summons share a bond:##Summons gain ~lg~+10%~/~ Damage Reflection#You gain ~lg~+8%~/~ Life Drain while any summon is alive##If a summon would die during the bond, it instead survives at ~w~1~/~ Health once (then the bond ends).",
                ru: "На ~w~10~/~ ходов вы и ваши призывы связаны узами:##Призывы получают ~lg~+10%~/~ отражения урона#Вы получаете ~lg~+8%~/~ вампиризма, пока жив хотя бы один призыв##Если призыв должен погибнуть во время связи, он один раз выживает с ~w~1~/~ здоровья (затем связь заканчивается).",
                zh: "持续~w~10~/~回合，你与召唤物共享羁绊：##召唤物获得~lg~+10%~/~伤害反射#任一召唤存活时你获得~lg~+8%~/~生命汲取##羁绊期间召唤本应死亡时，会以~w~1~/~生命存活一次（随后羁绊结束）。",
                de: "Für ~w~10~/~ Züge teilst du mit deinen Beschwörungen einen Bund:##Beschwörungen erhalten ~lg~+10%~/~ Schadensreflexion#Du erhältst ~lg~+8%~/~ Lebensraub, solange eine Beschwörung lebt##Würde eine Beschwörung während des Bunds sterben, überlebt sie einmal mit ~w~1~/~ Gesundheit (danach endet der Bund).",
                ja: "~w~10~/~ターン、あなたと召喚体が絆を共有する：##召喚体は~lg~ダメージ反射+10%~/~#召喚が生存中、あなたは~lg~ライフドレイン+8%~/~##絆中に召喚が死ぬ場合、一度だけ体力~w~1~/~で生き残る（その後絆は終了）。"));

        RegisterSkill(
            "summon_gulon",
            Loc(
                en: "Summon Gulon",
                ru: "Призыв гулона",
                zh: "召唤狼獾",
                de: "Gulon beschwören",
                ja: "グロンの召喚"),
            Loc(
                en: "Summons your bonded ~lg~Gulon~/~ on the targeted tile.##Keeps its ~lg~own~/~ Experience and level, independent of other companions.##Apex predator: high Damage and Crit; feeds off Hunt Command marks.##Death in combat causes ~r~-12 Morale~/~ for ~w~600~/~ turns (does not stack).",
                ru: "Призывает связанного ~lg~Гулона~/~ на выбранную клетку.##Сохраняет ~lg~свой~/~ опыт и уровень, отдельно от других компаньонов.##Вершина хищничества: высокий урон и крит; усиливается от меток Приказа охоты.##Смерть в бою даёт ~r~-12 Морали~/~ на ~w~600~/~ ходов (не стакается).",
                zh: "在目标格子召唤你羁绊的~lg~狼獾~/~。##保留~lg~自身~/~经验与等级，独立于其他同伴。##顶级掠食者：高伤害与暴击；受益于狩猎指令标记。##战斗死亡造成~r~-12士气~/~，持续~w~600~/~回合（不叠加）。",
                de: "Beschwört deinen gebundenen ~lg~Gulon~/~ auf dem Zielfeld.##Behält seine ~lg~eigene~/~ Erfahrung und Stufe, unabhängig von anderen Gefährten.##Spitzenprädator: hoher Schaden und Krit; profitiert von Jagdkommando-Markierungen.##Tod im Kampf verursacht ~r~-12 Moral~/~ für ~w~600~/~ Züge (stapelt nicht).",
                ja: "対象マスに絆を結んだ~lg~グロン~/~を召喚する。##他の仲間とは別の~lg~独自~/~経験とレベルを保持する。##頂点捕食者：高ダメージとクリティカル；狩りの号令の印で強化される。##戦闘死で~r~士気-12~/~を~w~600~/~ターン（重複しない）。"));

        RegisterSkill(
            "kindred_spirit",
            Loc(
                en: "Kindred Spirit",
                ru: "Родственный дух",
                zh: "同族之灵",
                de: "Seelenverwandter",
                ja: "同族の魂"),
            Loc(
                en: "Your bond with beasts deepens.##Each companion's Experience gain ~lg~+25%~/~ (still separate per species).##Morale loss from summon death (600 turns, does not stack) is reduced by ~lg~50%~/~.##While you have at least two summons active: ~lg~+5%~/~ Magic Power and ~lg~+5%~/~ Fortitude.",
                ru: "Ваша связь со зверями углубляется.##Прирост опыта каждого компаньона ~lg~+25%~/~ (по-прежнему отдельно по видам).##Потеря Морали от смерти призыва (600 ходов, не стакается) снижена на ~lg~50%~/~.##Пока активны хотя бы два призыва: ~lg~+5%~/~ силы магии и ~lg~+5%~/~ стойкости.",
                zh: "你与野兽的羁绊加深。##每个同伴的经验获取~lg~+25%~/~（仍按物种分开）。##召唤死亡的士气损失（600回合，不叠加）减少~lg~50%~/~。##至少两只召唤存活时：~lg~+5%~/~魔法强度与~lg~+5%~/~坚韧。",
                de: "Dein Bund mit Bestien vertieft sich.##Erfahrungsgewinn jedes Gefährten ~lg~+25%~/~ (weiterhin pro Art).##Moralverlust durch Tod einer Beschwörung (600 Züge, stapelt nicht) ist um ~lg~50%~/~ verringert.##Mit mindestens zwei aktiven Beschwörungen: ~lg~+5%~/~ Magiekraft und ~lg~+5%~/~ Zähigkeit.",
                ja: "獣との絆が深まる。##各仲間の経験獲得~lg~+25%~/~（種ごとに別管理のまま）。##召喚死亡の士気損失（600ターン・非重複）が~lg~50%~/~減少。##召喚が2体以上いる間：~lg~魔法威力+5%~/~と~lg~頑健+5%~/~。"));

        RegisterSkill(
            "primal_apex",
            Loc(
                en: "Primal Apex",
                ru: "Первобытная вершина",
                zh: "原初巅峰",
                de: "Ursprünglicher Höhepunkt",
                ja: "原初の頂点"),
            Loc(
                en: "For ~w~6~/~ turns, all active summons enter ~lg~\"Primal Apex\"~/~:##~lg~+15%~/~ Weapon Damage#~lg~+10%~/~ Crit Chance#~lg~+15%~/~ Max Health (temporary)##During Apex, each summon gains ~lg~bonus Experience~/~ on hits into its ~lg~own~/~ XP track.##When Apex ends, each participating summon is Exhausted for ~w~2~/~ turns.",
                ru: "На ~w~6~/~ ходов все активные призывы получают ~lg~«Первобытную вершину»~/~:##~lg~+15%~/~ урона оружием#~lg~+10%~/~ шанса крита#~lg~+15%~/~ макс. здоровья (временно)##Во время Вершины каждый призыв получает ~lg~бонусный опыт~/~ с ударов в ~lg~свой~/~ трек опыта.##По окончании каждый участник получает Истощение на ~w~2~/~ хода.",
                zh: "持续~w~6~/~回合，所有活跃召唤进入~lg~“原初巅峰”~/~：##~lg~+15%~/~武器伤害#~lg~+10%~/~暴击几率#~lg~+15%~/~最大生命（临时）##巅峰期间，每次命中为该召唤的~lg~自身~/~经验轨提供~lg~额外经验~/~。##结束时，参与的召唤陷入疲惫~w~2~/~回合。",
                de: "Für ~w~6~/~ Züge erhalten alle aktiven Beschwörungen ~lg~„Ursprünglicher Höhepunkt“~/~:##~lg~+15%~/~ Waffenschaden#~lg~+10%~/~ Krit-Chance#~lg~+15%~/~ max. Gesundheit (temporär)##Währenddessen erhält jede Beschwörung ~lg~Bonus-Erfahrung~/~ bei Treffern in ihre ~lg~eigene~/~ XP-Spur.##Danach ist jede beteiligte Beschwörung ~w~2~/~ Züge Erschöpft.",
                ja: "~w~6~/~ターン、全召喚体が~lg~「原初の頂点」~/~に入る：##~lg~武器ダメージ+15%~/~#~lg~クリティカル率+10%~/~#~lg~最大体力+15%~/~（一時）##頂点中、命中ごとに~lg~独自~/~経験へ~lg~追加経験~/~。##終了後、参加した召喚は~w~2~/~ターン疲労する。"));
    }

    static void RegisterAllBuffs()
    {
        RegisterBuff(
            "o_b_sm_summon_death",
            Loc(
                en: "Companion's Death",
                ru: "Смерть компаньона",
                zh: "同伴之死",
                de: "Tod des Gefährten",
                ja: "仲間の死"),
            Loc(
                en: "~r~Morale~/~ penalty from a companion dying in combat.##Lasts ~w~600~/~ turns.##Does ~lg~not~/~ stack.",
                ru: "Штраф ~r~Морали~/~ за гибель компаньона в бою.##Длится ~w~600~/~ ходов.##~lg~Не~/~ стакается.",
                zh: "同伴在战斗中死亡造成的~r~士气~/~惩罚。##持续~w~600~/~回合。##~lg~不~/~叠加。",
                de: "~r~Moral~/~strafe durch den Tod eines Gefährten im Kampf.##Dauert ~w~600~/~ Züge.##Stapelt ~lg~nicht~/~.",
                ja: "戦闘で仲間が死んだ際の~r~士気~/~ペナルティ。##~w~600~/~ターン続く。##~lg~重複しない~/~。"));

        RegisterBuff(
            "o_b_sm_servant",
            Loc(
                en: "Bound Companion",
                ru: "Связанный компаньон",
                zh: "羁绊同伴",
                de: "Gebundener Gefährte",
                ja: "絆の仲間"),
            Loc(
                en: "This beast is bound to your will.",
                ru: "Этот зверь связан вашей волей.",
                zh: "此兽受你意志束缚。",
                de: "Diese Bestie ist an deinen Willen gebunden.",
                ja: "この獣はあなたの意志に縛られている。"));

        RegisterBuff(
            "o_b_sm_hunt_mark",
            Loc(
                en: "Hunt Mark",
                ru: "Метка охоты",
                zh: "狩猎标记",
                de: "Jagdmal",
                ja: "狩りの印"),
            Loc(
                en: "Marked for the pack.##Bound companions gain ~lg~+12%~/~ Weapon Damage and ~lg~+10%~/~ Accuracy against this target.",
                ru: "Помечен для стаи.##Связанные компаньоны получают ~lg~+12%~/~ урона оружием и ~lg~+10%~/~ точности против этой цели.",
                zh: "已被兽群标记。##羁绊同伴对该目标获得~lg~+12%~/~武器伤害与~lg~+10%~/~命中。",
                de: "Für das Rudel markiert.##Gebundene Gefährten erhalten ~lg~+12%~/~ Waffenschaden und ~lg~+10%~/~ Trefferchance gegen dieses Ziel.",
                ja: "群れの標的。##絆の仲間はこの対象に対し~lg~武器ダメージ+12%~/~と~lg~命中+10%~/~を得る。"));

        RegisterBuff(
            "o_b_sm_hunt_focus",
            Loc(
                en: "Hunt Focus",
                ru: "Охотничий фокус",
                zh: "狩猎专注",
                de: "Jagdfokus",
                ja: "狩りの集中"),
            Loc(
                en: "~lg~+12%~/~ Weapon Damage#~lg~+10%~/~ Accuracy##Against the Hunt-marked target.",
                ru: "~lg~+12%~/~ урона оружием#~lg~+10%~/~ точности##Против цели с Меткой охоты.",
                zh: "~lg~+12%~/~武器伤害#~lg~+10%~/~命中##对狩猎标记目标生效。",
                de: "~lg~+12%~/~ Waffenschaden#~lg~+10%~/~ Trefferchance##Gegen das mit Jagdmal markierte Ziel.",
                ja: "~lg~武器ダメージ+12%~/~#~lg~命中+10%~/~##狩りの印の対象に対して。"));

        RegisterBuff(
            "o_b_sm_alphas_howl",
            Loc(
                en: "Alpha's Howl",
                ru: "Вой альфы",
                zh: "头狼之嚎",
                de: "Heulen des Alphas",
                ja: "アルファの遠吠え"),
            Loc(
                en: "~lg~+10%~/~ Weapon Damage#~lg~+5%~/~ Crit Chance#~lg~+10%~/~ Move Resistance",
                ru: "~lg~+10%~/~ урона оружием#~lg~+5%~/~ шанса крита#~lg~+10%~/~ сопротивления перемещению",
                zh: "~lg~+10%~/~武器伤害#~lg~+5%~/~暴击几率#~lg~+10%~/~移动抗性",
                de: "~lg~+10%~/~ Waffenschaden#~lg~+5%~/~ Krit-Chance#~lg~+10%~/~ Bewegungsresistenz",
                ja: "~lg~武器ダメージ+10%~/~#~lg~クリティカル率+5%~/~#~lg~移動耐性+10%~/~"));

        RegisterBuff(
            "o_b_sm_savage_bond",
            Loc(
                en: "Savage Bond",
                ru: "Дикая связь",
                zh: "野性羁绊",
                de: "Wilder Bund",
                ja: "野性の絆"),
            Loc(
                en: "Summons: ~lg~+10%~/~ Damage Reflection#You: ~lg~+8%~/~ Life Drain while a summon lives##Once: a dying summon survives at ~w~1~/~ Health (then ends).",
                ru: "Призывы: ~lg~+10%~/~ отражения урона#Вы: ~lg~+8%~/~ вампиризма, пока жив призыв##Один раз: гибнущий призыв выживает с ~w~1~/~ здоровья (затем эффект кончается).",
                zh: "召唤物：~lg~+10%~/~伤害反射#你：有召唤存活时~lg~+8%~/~生命汲取##一次：濒死召唤以~w~1~/~生命存活（随后结束）。",
                de: "Beschwörungen: ~lg~+10%~/~ Schadensreflexion#Du: ~lg~+8%~/~ Lebensraub, solange eine Beschwörung lebt##Einmal: sterbende Beschwörung überlebt mit ~w~1~/~ Gesundheit (endet danach).",
                ja: "召喚：~lg~ダメージ反射+10%~/~#あなた：召喚生存中~lg~ライフドレイン+8%~/~##一度：瀕死の召喚が体力~w~1~/~で生き残る（その後終了）。"));

        RegisterBuff(
            "o_b_sm_primal_apex",
            Loc(
                en: "Primal Apex",
                ru: "Первобытная вершина",
                zh: "原初巅峰",
                de: "Ursprünglicher Höhepunkt",
                ja: "原初の頂点"),
            Loc(
                en: "~lg~+15%~/~ Weapon Damage#~lg~+10%~/~ Crit Chance#~lg~+15%~/~ Max Health (temporary)##Bonus Experience on hits into this summon's own XP track.",
                ru: "~lg~+15%~/~ урона оружием#~lg~+10%~/~ шанса крита#~lg~+15%~/~ макс. здоровья (временно)##Бонусный опыт с ударов в собственный трек опыта этого призыва.",
                zh: "~lg~+15%~/~武器伤害#~lg~+10%~/~暴击几率#~lg~+15%~/~最大生命（临时）##命中时为该召唤自身经验轨提供额外经验。",
                de: "~lg~+15%~/~ Waffenschaden#~lg~+10%~/~ Krit-Chance#~lg~+15%~/~ max. Gesundheit (temporär)##Bonus-Erfahrung bei Treffern in die eigene XP-Spur dieser Beschwörung.",
                ja: "~lg~武器ダメージ+15%~/~#~lg~クリティカル率+10%~/~#~lg~最大体力+15%~/~（一時）##命中でこの召喚の独自経験へ追加経験。"));
    }

    static Dictionary<ModLanguage, string> Loc(
        string en, string ru, string zh, string de, string ja) => new()
    {
        { ModLanguage.English, en },
        { ModLanguage.Russian, ru },
        { ModLanguage.Chinese, zh },
        { ModLanguage.German, de },
        { ModLanguage.Japanese, ja },
    };

    static void RegisterSkill(string id, Dictionary<ModLanguage, string> name, Dictionary<ModLanguage, string> desc) =>
        PendingSkills.Add((id, name, desc));

    static void RegisterBuff(string id, Dictionary<ModLanguage, string> name, Dictionary<ModLanguage, string> desc) =>
        PendingBuffs.Add((id, name, desc));

    static void PatchSchool()
    {
        // o_skillmenu Create does:
        //   name = ds_map_find_value(global.tier_name, text)
        //   scr_stringGetHeightExt(name, ...)
        // global.tier_name comes from table_text Tier_name — missing key => undefined crash.
        Msl.LoadGML("gml_GlobalScript_table_text")
            .Apply(InjectSummonerTableTextRows)
            .Save();
    }

    /// <summary>
    /// CSV columns: id;RU;EN;ZH;DE;ES;FR;IT;PT;PL;TR;JA;KO
    /// Id must match category Create `text` / SchoolName ("Summoner").
    /// </summary>
    static IEnumerable<string> InjectSummonerTableTextRows(IEnumerable<string> lines)
    {
        var school = SummonerSkills.SchoolName;
        const string tierMarker = "\";Tier_name_end;";
        const string hoverMarker = "\";skilltree_hover_end;";

        // RU;EN;ZH;DE;ES;FR;IT;PT;PL;TR;JA;KO
        var tierRow =
            "\"" + school + ";Призыватель;" + school + ";召唤师;Beschwörer;Invocador;Invocateur;Evocatore;Invocador;Przywoływacz;Çağırıcı;召喚師;소환사;\", ";

        const string hoverEn =
            "Bind living beasts as companions that keep their own experience and grow with you.##~y~Main focus:~/~#~w~Minion Management~/~, ~w~Support~/~, ~w~Pack Synergy~/~, ~w~Sustain~/~";
        const string hoverRu =
            "Приручайте живых зверей — компаньоны сохраняют опыт и растут вместе с вами.##~y~Особенности:~/~#~w~Управление призывами~/~, ~w~Поддержка~/~, ~w~Синергия стаи~/~, ~w~Устойчивость~/~";
        const string hoverZh =
            "束缚活体野兽作为同伴，它们保留自身经验并与你一同成长。##~y~主要方向:~/~#~w~召唤物管理~/~, ~w~支援~/~, ~w~群体协同~/~, ~w~续航~/~";
        const string hoverDe =
            "Binde lebende Bestien als Gefährten, die eigene Erfahrung behalten und mit dir wachsen.##~y~Fokus:~/~#~w~Dienerverwaltung~/~, ~w~Unterstützung~/~, ~w~Rudel-Synergie~/~, ~w~Durchhaltevermögen~/~";
        const string hoverJa =
            "生きた獣を仲間として束ね、経験を保持したまま共に成長する。##~y~主な特徴:~/~#~w~召喚管理~/~, ~w~支援~/~, ~w~群れの相乗~/~, ~w~持続~/~";

        var hoverRow =
            "\"" + school + ";" + hoverRu + ";" + hoverEn + ";" + hoverZh + ";" + hoverDe + ";" +
            hoverEn + ";" + hoverEn + ";" + hoverEn + ";" + hoverEn + ";" + hoverEn + ";" + hoverEn + ";" +
            hoverJa + ";" + hoverEn + ";\", ";

        var tierGuard = "\"" + school + ";Призыватель;";
        var hoverGuard = "\"" + school + ";" + hoverRu;

        foreach (var line in lines)
        {
            var result = line;
            if (result.Contains(tierMarker, StringComparison.Ordinal) &&
                !result.Contains(tierGuard, StringComparison.Ordinal))
                result = result.Replace(tierMarker, tierRow + tierMarker, StringComparison.Ordinal);

            if (result.Contains(hoverMarker, StringComparison.Ordinal) &&
                !result.Contains(hoverGuard, StringComparison.Ordinal))
                result = result.Replace(hoverMarker, hoverRow + hoverMarker, StringComparison.Ordinal);

            yield return result;
        }
    }

    static string Escape(string value) =>
        (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace(";", ".");

    static string LocPick(Dictionary<ModLanguage, string> texts, ModLanguage lang)
    {
        if (texts.TryGetValue(lang, out var value) && !string.IsNullOrEmpty(value))
            return value;
        return texts[ModLanguage.English];
    }

    /// <summary>CSV columns: id;RU;EN;ZH;DE;ES;FR;IT;PT;PL;TR;JA;KO — EN fills remaining slots.</summary>
    static string CsvRow(string id, Dictionary<ModLanguage, string> texts)
    {
        var en = Escape(LocPick(texts, ModLanguage.English));
        var ru = Escape(LocPick(texts, ModLanguage.Russian));
        var zh = Escape(LocPick(texts, ModLanguage.Chinese));
        var de = Escape(LocPick(texts, ModLanguage.German));
        var ja = Escape(LocPick(texts, ModLanguage.Japanese));
        return "\"" + id + ";" + ru + ";" + en + ";" + zh + ";" + de + ";" +
               en + ";" + en + ";" + en + ";" + en + ";" + en + ";" + en + ";" +
               ja + ";" + en + ";\", ";
    }

    static IEnumerable<string> InjectTableSkillsRows(IEnumerable<string> lines)
    {
        const string nameMarker = "\";skill_name_end;";
        const string descMarker = "\";skill_desc_end;";
        var descGuard = "\"" + PendingSkills[0].Id + ";" + Escape(LocPick(PendingSkills[0].Desc, ModLanguage.Russian));

        var names = new StringBuilder();
        var descs = new StringBuilder();
        foreach (var e in PendingSkills)
        {
            names.Append(CsvRow(e.Id, e.Name));
            descs.Append(CsvRow(e.Id, e.Desc));
        }

        var nameText = names.ToString();
        var descText = descs.ToString();

        foreach (var line in lines)
        {
            var result = line;
            if (result.Contains(nameMarker, StringComparison.Ordinal))
            {
                if (SectionHasId(result, PendingSkills[0].Id, "\";skill_name;", nameMarker))
                {
                    foreach (var e in PendingSkills)
                        result = UpsertCsvRow(result, e.Id, e.Name, nameMarker, "\";skill_name;");
                }
                else
                {
                    result = result.Replace(nameMarker, nameText + nameMarker, StringComparison.Ordinal);
                }
            }

            if (result.Contains(descMarker, StringComparison.Ordinal))
            {
                if (SectionHasId(result, PendingSkills[0].Id, "\";skill_desc;", descMarker) ||
                    result.Contains(descGuard, StringComparison.Ordinal))
                {
                    foreach (var e in PendingSkills)
                        result = UpsertCsvRow(result, e.Id, e.Desc, descMarker, "\";skill_desc;");
                }
                else
                {
                    result = result.Replace(descMarker, descText + descMarker, StringComparison.Ordinal);
                }
            }

            yield return result;
        }
    }

    static IEnumerable<string> InjectTableEffectsRows(IEnumerable<string> lines)
    {
        const string nameMarker = "\";buff_name_end;";
        const string descMarker = "\";buff_desc_end;";
        var descGuard = "\"" + PendingBuffs[0].Id + ";" + Escape(LocPick(PendingBuffs[0].Desc, ModLanguage.Russian));

        var names = new StringBuilder();
        var descs = new StringBuilder();
        foreach (var e in PendingBuffs)
        {
            names.Append(CsvRow(e.Id, e.Name));
            descs.Append(CsvRow(e.Id, e.Desc));
        }

        var nameText = names.ToString();
        var descText = descs.ToString();

        foreach (var line in lines)
        {
            var result = line;
            if (result.Contains(nameMarker, StringComparison.Ordinal))
            {
                if (SectionHasId(result, PendingBuffs[0].Id, "\";buff_name;", nameMarker))
                {
                    foreach (var e in PendingBuffs)
                        result = UpsertCsvRow(result, e.Id, e.Name, nameMarker, "\";buff_name;");
                }
                else
                {
                    result = result.Replace(nameMarker, nameText + nameMarker, StringComparison.Ordinal);
                }
            }

            if (result.Contains(descMarker, StringComparison.Ordinal))
            {
                if (SectionHasId(result, PendingBuffs[0].Id, "\";buff_desc;", descMarker) ||
                    result.Contains(descGuard, StringComparison.Ordinal))
                {
                    foreach (var e in PendingBuffs)
                        result = UpsertCsvRow(result, e.Id, e.Desc, descMarker, "\";buff_desc;");
                }
                else
                {
                    result = result.Replace(descMarker, descText + descMarker, StringComparison.Ordinal);
                }
            }

            yield return result;
        }
    }

    static bool SectionHasId(
        string line, string id, string sectionStartMarker, string sectionEndMarker)
    {
        var sectionEnd = line.IndexOf(sectionEndMarker, StringComparison.Ordinal);
        if (sectionEnd < 0)
            return false;
        var sectionStart = line.LastIndexOf(sectionStartMarker, sectionEnd, StringComparison.Ordinal);
        if (sectionStart < 0)
            sectionStart = 0;

        var needle = "\"" + id + ";";
        var start = line.IndexOf(needle, sectionStart, StringComparison.Ordinal);
        return start >= 0 && start < sectionEnd;
    }

    static string UpsertCsvRow(
        string line, string id, Dictionary<ModLanguage, string> texts, string sectionEndMarker, string sectionStartMarker)
    {
        var newRow = CsvRow(id, texts);
        var sectionEnd = line.IndexOf(sectionEndMarker, StringComparison.Ordinal);
        if (sectionEnd < 0)
            return line;

        var sectionStart = line.LastIndexOf(sectionStartMarker, sectionEnd, StringComparison.Ordinal);
        if (sectionStart < 0)
            sectionStart = 0;

        var needle = "\"" + id + ";";
        var start = line.IndexOf(needle, sectionStart, StringComparison.Ordinal);
        if (start >= 0 && start < sectionEnd)
        {
            var end = line.IndexOf("\",", start, StringComparison.Ordinal);
            if (end < 0 || end > sectionEnd)
                return line;
            return line[..start] + newRow + line[(end + 2)..];
        }

        return line[..sectionEnd] + newRow + line[sectionEnd..];
    }
}
