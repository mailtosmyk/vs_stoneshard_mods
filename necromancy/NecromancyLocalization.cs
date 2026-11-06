using ModShardLauncher;
using ModShardLauncher.Mods;

namespace Necromancy;

/// <summary>
/// Injects skill names, descriptions, and cast speeches for the necromancy school.
/// </summary>
public static class NecromancyLocalization
{
    public static void PatchAll()
    {
        PatchSchool();
        PatchSkills();
        PatchSpeeches();
    }

    static void PatchSchool()
    {
        Msl.InjectTableItemLocalization(
            "necromancy",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Necromancy" },
                { ModLanguage.Japanese, "ネクロマンシー" },
                { ModLanguage.Chinese, "死灵术" },
                { ModLanguage.German, "Nekromantie" },
                { ModLanguage.Russian, "Некромантия" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Necromancy" },
                { ModLanguage.Japanese, "ネクロマンシー" },
                { ModLanguage.Chinese, "死灵术" },
                { ModLanguage.German, "Nekromantie" },
                { ModLanguage.Russian, "Некромантия" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Necromancy" },
                { ModLanguage.Japanese, "ネクロマンシー" },
                { ModLanguage.Chinese, "死灵术" },
                { ModLanguage.German, "Nekromantie" },
                { ModLanguage.Russian, "Некромантия" },
            });
    }

    static void PatchSkills()
    {
        Msl.InjectTableItemLocalization(
            "desecration",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Desecration" },
                { ModLanguage.Japanese, "冒涜" },
                { ModLanguage.Chinese, "亵渎" },
                { ModLanguage.German, "Entweihung" },
                { ModLanguage.Russian, "Осквернение" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Desecration" },
                { ModLanguage.Japanese, "冒涜" },
                { ModLanguage.Chinese, "亵渎" },
                { ModLanguage.German, "Entweihung" },
                { ModLanguage.Russian, "Осквернение" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Shoots a bolt of dark energy (Range: 6), dealing 7 Unholy Damage with Accuracy. If the spell hits the target, burns its Energy for 33% of the damage dealt and replenishes the same amount of Energy to the caster. Grants Unholy Damage for 8–10 turns. This effect doesn't stack." },
                { ModLanguage.Japanese, "暗黒エネルギーのボルトを放つ（射程: 6）。命中時に7の不浄ダメージを与える。命中すると、与ダメージの33%分のエネルギーを焼き、同量を術者に回復する。8〜10ターンの間、不浄ダメージを付与する。この効果は重複しない。" },
                { ModLanguage.Chinese, "射出一道黑暗能量箭（射程：6），造成7点邪秽伤害（需命中）。若命中目标，按其受到伤害的33%燃烧能量，并将等量能量回复给施法者。赋予邪秽伤害，持续8–10回合。此效果不叠加。" },
                { ModLanguage.German, "Schießt einen Blitz dunkler Energie (Reichweite: 6), der 7 Unheiligen Schaden mit Treffergenauigkeit verursacht. Bei einem Treffer verbrennt der Zauber 33% des verursachten Schadens als Energie und stellt dem Wirker denselben Betrag wieder her. Gewährt Unheiligen Schaden für 8–10 Züge. Dieser Effekt stapelt nicht." },
                { ModLanguage.Russian, "Выпускает заряд тёмной энергии (дальность: 6), наносящий 7 урона нечестием с учётом точности. При попадании сжигает энергию цели в размере 33% от нанесённого урона и восстанавливает столько же энергии заклинателю. Даёт урон нечестием на 8–10 ходов. Эффект не стакается." },
            });
        Msl.InjectTableItemLocalization(
            "bone_ward",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Bone Ward" },
                { ModLanguage.Japanese, "骨の守護" },
                { ModLanguage.Chinese, "骸骨结界" },
                { ModLanguage.German, "Knochenwächter" },
                { ModLanguage.Russian, "Костяной щит" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Bone Ward" },
                { ModLanguage.Japanese, "骨の守護" },
                { ModLanguage.Chinese, "骸骨结界" },
                { ModLanguage.German, "Knochenwächter" },
                { ModLanguage.Russian, "Костяной щит" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Activates \"Bone Ward\" for 4 turns (+1 turn if a corpse is adjacent): +12% Physical Resistance, +15% Bleed Resistance, +10% Magic Resistance, +10% Move Resistance. When the effect expires or is activated again, bone shards deal 4 Unholy and 3 Piercing Damage to all adjacent enemies, with 50% chance to Stagger them." },
                { ModLanguage.Japanese, "「骨の守護」を4ターン発動する（隣接に死体がある場合+1ターン）：物理耐性+12%、出血耐性+15%、魔法耐性+10%、移動耐性+10%。効果終了時または再発動時、骨片が隣接する全敵に不浄ダメージ4と刺突ダメージ3を与え、50%の確率でよろけさせる。" },
                { ModLanguage.Chinese, "激活“骸骨结界”，持续4回合（相邻有尸体时+1回合）：+12%物理抗性、+15%出血抗性、+10%魔法抗性、+10%移动抗性。效果结束或再次激活时，骨片对所有相邻敌人造成4点邪秽伤害与3点穿刺伤害，并有50%几率使其踉跄。" },
                { ModLanguage.German, "Aktiviert „Knochenwächter“ für 4 Züge (+1 Zug, wenn eine Leiche angrenzt): +12% physischer Widerstand, +15% Blutungswiderstand, +10% Magiewiderstand, +10% Bewegungswiderstand. Beim Auslaufen oder erneuten Aktivieren fügen Knochensplitter allen angrenzenden Feinden 4 Unheiligen und 3 Stichschaden zu, mit 50% Chance zu staunen." },
                { ModLanguage.Russian, "Активирует «Костяной щит» на 4 хода (+1 ход, если рядом труп): +12% физ. сопротивления, +15% сопротивления кровотечению, +10% маг. сопротивления, +10% сопротивления перемещению. По окончании или при повторной активации осколки костей наносят всем соседним врагам 4 урона нечестием и 3 колющего урона с 50% шансом ошеломить." },
            });
        Msl.InjectTableItemLocalization(
            "summon_skeleton",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Summon Skeleton" },
                { ModLanguage.Japanese, "スケルトン召喚" },
                { ModLanguage.Chinese, "召唤骷髅" },
                { ModLanguage.German, "Skelett beschwören" },
                { ModLanguage.Russian, "Призыв скелета" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Summon Skeleton" },
                { ModLanguage.Japanese, "スケルトン召喚" },
                { ModLanguage.Chinese, "召唤骷髅" },
                { ModLanguage.German, "Skelett beschwören" },
                { ModLanguage.Russian, "Призыв скелета" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Summons 1 Skeleton on the targeted tile (no corpse required). Duration, Max Health %, and Max Energy % scale with Willpower (starts at 5 turns, 33% Max Health / 25% Max Energy, Armor Durability 0%). Only one Skeleton can exist at a time. Slowly decreases Sanity (no less than 70%). Risk of \"Obsession\": Low. Can't summon Undeads higher than the skill's level cap. Cannot be cast under \"Obsession\"." },
                { ModLanguage.Japanese, "対象タイルにスケルトン1体を召喚する（死体不要）。持続時間・最大体力%・最大エネルギー%は意志力に比例する（初期: 5ターン、最大体力33% / 最大エネルギー25%、防具耐久0%）。同時に存在できるスケルトンは1体のみ。正気を徐々に減少させる（最低70%）。「執着」のリスク: 低。スキルのレベル上限を超えるアンデッドは召喚できない。「執着」中は詠唱不可。" },
                { ModLanguage.Chinese, "在目标格子召唤1具骷髅（无需尸体）。持续时间、最大生命%与最大能量%随意志力提升（起始：5回合，33%最大生命 / 25%最大能量，护甲耐久0%）。同时仅可存在一具骷髅。缓慢降低理智（不低于70%）。“执念”风险：低。无法召唤高于技能等级上限的亡灵。“执念”状态下无法施放。" },
                { ModLanguage.German, "Beschwört 1 Skelett auf dem Ziel-Feld (keine Leiche nötig). Dauer, max. Gesundheit % und max. Energie % skalieren mit Willenskraft (Start: 5 Züge, 33% max. Gesundheit / 25% max. Energie, Rüstungshalbarkeit 0%). Nur ein Skelett gleichzeitig. Senkt langsam den Verstand (nicht unter 70%). Risiko von „Besessenheit“: Gering. Kann keine Untoten über dem Level-Limit der Fähigkeit beschwören. Nicht unter „Besessenheit“ wirkbar." },
                { ModLanguage.Russian, "Призывает 1 скелета на выбранную клетку (труп не нужен). Длительность, % макс. здоровья и % макс. энергии зависят от Воли (старт: 5 ходов, 33% макс. здоровья / 25% макс. энергии, прочность брони 0%). Одновременно может существовать только один скелет. Медленно снижает рассудок (не ниже 70%). Риск «Одержимости»: низкий. Нельзя призвать нежить выше уровня навыка. Нельзя использовать под «Одержимостью»." },
            });
        Msl.InjectTableItemLocalization(
            "withering_curse",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Withering Curse" },
                { ModLanguage.Japanese, "枯死の呪い" },
                { ModLanguage.Chinese, "凋零诅咒" },
                { ModLanguage.German, "Verdorrender Fluch" },
                { ModLanguage.Russian, "Иссушающее проклятие" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Withering Curse" },
                { ModLanguage.Japanese, "枯死の呪い" },
                { ModLanguage.Chinese, "凋零诅咒" },
                { ModLanguage.German, "Verdorrender Fluch" },
                { ModLanguage.Russian, "Иссушающее проклятие" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Applies the target with \"Withering Curse\" for 10 turns (Range: 6): -8% Fortitude, -8% Magic Resistance, -5% Max Health, +3% Damage Taken. Each turn worsens a random body part by 0.25%. Hitting with \"Desecration\" or repeating the spell on the same target refreshes the duration (does not stack at Tier 1)." },
                { ModLanguage.Japanese, "対象に「枯死の呪い」を10ターン付与する（射程: 6）：頑健-8%、魔法耐性-8%、最大体力-5%、被ダメージ+3%。毎ターン、ランダムな部位の状態を0.25%悪化させる。「冒涜」の命中または同一対象への再詠唱で持続を更新する（ティア1では重複しない）。" },
                { ModLanguage.Chinese, "对目标施加“凋零诅咒”，持续10回合（射程：6）：-8%坚韧、-8%魔法抗性、-5%最大生命、+3%受到伤害。每回合使随机部位状态恶化0.25%。以“亵渎”命中或对同一目标重复施放会刷新持续时间（第1阶不叠加）。" },
                { ModLanguage.German, "Belegt das Ziel mit „Verdorrender Fluch“ für 10 Züge (Reichweite: 6): -8% Zähigkeit, -8% Magiewiderstand, -5% max. Gesundheit, +3% erlittener Schaden. Jeder Zug verschlechtert einen zufälligen Körperteil um 0,25%. Treffer mit „Entweihung“ oder erneutes Wirken auf dasselbe Ziel erneuert die Dauer (stapelt nicht auf Stufe 1)." },
                { ModLanguage.Russian, "Накладывает на цель «Иссушающее проклятие» на 10 ходов (дальность: 6): -8% стойкости, -8% маг. сопротивления, -5% макс. здоровья, +3% получаемого урона. Каждый ход ухудшает случайную часть тела на 0,25%. Попадание «Осквернением» или повторное применение обновляет длительность (на 1-м ярусе не стакается)." },
            });
        Msl.InjectTableItemLocalization(
            "essence_leech",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Essence Leech" },
                { ModLanguage.Japanese, "本質吸収" },
                { ModLanguage.Chinese, "精华汲取" },
                { ModLanguage.German, "Essenzsauger" },
                { ModLanguage.Russian, "Поглощение сущности" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Essence Leech" },
                { ModLanguage.Japanese, "本質吸収" },
                { ModLanguage.Chinese, "精华汲取" },
                { ModLanguage.German, "Essenzsauger" },
                { ModLanguage.Russian, "Поглощение сущности" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Devours a targeted corpse or steals 8% Max Health and 8% Max Energy from an allied Undead or Skeleton, transferring half of them to the caster. Activates \"Essence Charge\" for 12–15 turns: +5% Magic Power, -5% Abilities Energy Cost, +4 Max Energy. Repeated use grants an extra stack (up to II) and refreshes duration. Using Spells reduces stacks by 1. No Obsession risk from Essence Charge at Tier 2." },
                { ModLanguage.Japanese, "対象の死体を喰らうか、味方のアンデッド/スケルトンから最大体力8%と最大エネルギー8%を奪い、半分を術者へ移す。「本質充填」を12〜15ターン発動：魔力+5%、能力のエネルギー消費-5%、最大エネルギー+4。再使用で追加スタック（最大II）と持続更新。呪文使用でスタックが1減少。ティア2の本質充填に執着リスクなし。" },
                { ModLanguage.Chinese, "吞噬目标尸体，或从友方亡灵/骷髅窃取8%最大生命与8%最大能量，并将一半转移给施法者。激活“精华充能”，持续12–15回合：+5%魔法强度、-5%技能能量消耗、+4最大能量。重复使用额外叠加（最高II）并刷新持续时间。施放法术会使层数减1。第2阶的精华充能无执念风险。" },
                { ModLanguage.German, "Verschlingt eine gezielte Leiche oder stiehlt 8% max. Gesundheit und 8% max. Energie von einem verbündeten Untoten oder Skelett und überträgt die Hälfte auf den Wirker. Aktiviert „Essenzladung“ für 12–15 Züge: +5% Magiekraft, -5% Energiekosten von Fähigkeiten, +4 max. Energie. Wiederholte Nutzung gibt einen Extra-Stapel (bis II) und erneuert die Dauer. Zauber verringern Stapel um 1. Kein Besessenheitsrisiko durch Essenzladung auf Stufe 2." },
                { ModLanguage.Russian, "Пожирает выбранный труп или крадёт 8% макс. здоровья и 8% макс. энергии у союзной нежити/скелета, передавая половину заклинателю. Активирует «Заряд сущности» на 12–15 ходов: +5% силы магии, -5% стоимости энергии способностей, +4 макс. энергии. Повторное применение даёт доп. стак (до II) и обновляет длительность. Заклинания уменьшают стаки на 1. На 2-м ярусе «Заряд сущности» не даёт риска одержимости." },
            });
        Msl.InjectTableItemLocalization(
            "deaths_blessing",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Death's Blessing" },
                { ModLanguage.Japanese, "死の祝福" },
                { ModLanguage.Chinese, "死亡赐福" },
                { ModLanguage.German, "Segen des Todes" },
                { ModLanguage.Russian, "Благословение смерти" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Death's Blessing" },
                { ModLanguage.Japanese, "死の祝福" },
                { ModLanguage.Chinese, "死亡赐福" },
                { ModLanguage.German, "Segen des Todes" },
                { ModLanguage.Russian, "Благословение смерти" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Replenishes Health to the target (based on Magic Power) and improves bodypart condition by 25% of health restored. 50% more effective on allied Undeads. Grants \"Death's Blessing\" for 8–10 turns: +6% Life Drain, +6% Energy Drain, +8% Damage Reflection." },
                { ModLanguage.Japanese, "対象の体力を回復し（魔力依存）、回復量の25%分だけ部位状態を改善する。味方アンデッドには50%効果上昇。「死の祝福」を8〜10ターン付与：生命吸収+6%、エネルギー吸収+6%、ダメージ反射+8%。" },
                { ModLanguage.Chinese, "恢复目标生命（基于魔法强度），并按恢复量的25%改善部位状态。对友方亡灵效果提升50%。赋予“死亡赐福”，持续8–10回合：+6%生命汲取、+6%能量汲取、+8%伤害反射。" },
                { ModLanguage.German, "Stellt Gesundheit des Ziels wieder her (basiert auf Magiekraft) und verbessert den Zustand der Körperteile um 25% der geheilten Menge. 50% wirksamer bei verbündeten Untoten. Gewährt „Segen des Todes“ für 8–10 Züge: +6% Lebensraub, +6% Energieraub, +8% Schadensreflexion." },
                { ModLanguage.Russian, "Восстанавливает здоровье цели (зависит от силы магии) и улучшает состояние частей тела на 25% от восстановленного здоровья. На 50% эффективнее на союзной нежити. Даёт «Благословение смерти» на 8–10 ходов: +6% вампиризма, +6% кражи энергии, +8% отражения урона." },
            });
        Msl.InjectTableItemLocalization(
            "resurrection",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Resurrection" },
                { ModLanguage.Japanese, "復活" },
                { ModLanguage.Chinese, "复活" },
                { ModLanguage.German, "Auferstehung" },
                { ModLanguage.Russian, "Воскрешение" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Resurrection" },
                { ModLanguage.Japanese, "復活" },
                { ModLanguage.Chinese, "复活" },
                { ModLanguage.German, "Auferstehung" },
                { ModLanguage.Russian, "Воскрешение" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Reanimates a targeted corpse as a lasting Undead at 50–55% Max Health / 50% Max Energy and sets Armor Durability to 0%. Increases morale (up to 70%). Slowly decreases Sanity but no less than 60%. Risk of \"Obsession\": High. Summoning more than one Undead instantly causes \"Obsession\". Can't summon Undeads higher than the skill's level cap. Prevents receiving \"Blessing\". Cannot be cast under \"Obsession\"." },
                { ModLanguage.Japanese, "対象の死体を永続アンデッドとして蘇生する（最大体力50〜55% / 最大エネルギー50%、防具耐久0%）。士気を上昇（最大70%）。正気を徐々に減少させるが60%未満にはならない。「執着」のリスク: 高。2体以上のアンデッド召喚で即座に「執着」。スキルのレベル上限を超えるアンデッドは召喚不可。「祝福」を受けられない。「執着」中は詠唱不可。" },
                { ModLanguage.Chinese, "将目标尸体复活为持久亡灵（50–55%最大生命 / 50%最大能量，护甲耐久设为0%）。提升士气（最高70%）。缓慢降低理智但不低于60%。“执念”风险：高。召唤超过一个亡灵会立刻导致“执念”。无法召唤高于技能等级上限的亡灵。无法获得“祝福”。“执念”状态下无法施放。" },
                { ModLanguage.German, "Belebt eine gezielte Leiche als dauerhaften Untoten mit 50–55% max. Gesundheit / 50% max. Energie und setzt die Rüstungshalbarkeit auf 0%. Erhöht die Moral (bis 70%). Senkt langsam den Verstand, aber nicht unter 60%. Risiko von „Besessenheit“: Hoch. Mehr als ein Untoter verursacht sofort „Besessenheit“. Keine Untoten über dem Level-Limit. Verhindert „Segen“. Nicht unter „Besessenheit“ wirkbar." },
                { ModLanguage.Russian, "Оживляет выбранный труп как постоянную нежить при 50–55% макс. здоровья / 50% макс. энергии и ставит прочность брони на 0%. Повышает мораль (до 70%). Медленно снижает рассудок, но не ниже 60%. Риск «Одержимости»: высокий. Призыв более одной нежити мгновенно вызывает «Одержимость». Нельзя призвать нежить выше уровня навыка. Запрещает получение «Благословения». Нельзя использовать под «Одержимостью»." },
            });
        Msl.InjectTableItemLocalization(
            "grave_blight",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Grave Blight" },
                { ModLanguage.Japanese, "墓の疫病" },
                { ModLanguage.Chinese, "坟场疫病" },
                { ModLanguage.German, "Grabfäule" },
                { ModLanguage.Russian, "Могильная порча" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Grave Blight" },
                { ModLanguage.Japanese, "墓の疫病" },
                { ModLanguage.Chinese, "坟场疫病" },
                { ModLanguage.German, "Grabfäule" },
                { ModLanguage.Russian, "Могильная порча" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Creates a blighted 3x3 area on the targeted tile (Range: 5–6) for 4 turns. Enemies remaining in the area take 2 Unholy Damage per turn and gain -4% Magic Resistance (stacks up to 3 while inside). Also allows \"Withering Curse\" to stack up to II; hitting with \"Desecration\" or repeating the curse adds a stack. Bodypart worsen becomes 0.35% per turn per stack." },
                { ModLanguage.Japanese, "対象タイルに3x3の疫病地帯を4ターン生成する（射程: 5〜6）。領域内の敵は毎ターン不浄ダメージ2を受け、魔法耐性-4%を得る（内部で最大3スタック）。「枯死の呪い」を最大IIまで重複可能にし、「冒涜」の命中または呪いの再詠唱でスタック追加。部位悪化はスタックごとに毎ターン0.35%。" },
                { ModLanguage.Chinese, "在目标格子创造3x3疫病区域（射程：5–6），持续4回合。留在区域内的敌人每回合受到2点邪秽伤害，并获得-4%魔法抗性（在区域内最多叠加3层）。同时使“凋零诅咒”可叠至II；以“亵渎”命中或重复诅咒可增加层数。部位恶化变为每层每回合0.35%。" },
                { ModLanguage.German, "Erzeugt ein verdorbenes 3x3-Gebiet auf dem Zielfeld (Reichweite: 5–6) für 4 Züge. Feinde darin erleiden 2 Unheiligen Schaden pro Zug und erhalten -4% Magiewiderstand (stapelt bis 3, solange sie drin sind). Erlaubt „Verdorrender Fluch“ bis II zu stapeln; Treffer mit „Entweihung“ oder erneuter Fluch fügt einen Stapel hinzu. Körperteilverschlechterung: 0,35% pro Zug und Stapel." },
                { ModLanguage.Russian, "Создаёт заражённую область 3x3 на выбранной клетке (дальность: 5–6) на 4 хода. Враги в зоне получают 2 урона нечестием за ход и -4% маг. сопротивления (до 3 стаков, пока внутри). Также позволяет «Иссушающему проклятию» стакаться до II; попадание «Осквернением» или повтор проклятия добавляет стак. Ухудшение частей тела: 0,35% за ход за стак." },
            });
        Msl.InjectTableItemLocalization(
            "death_touch",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Death Touch" },
                { ModLanguage.Japanese, "死の接触" },
                { ModLanguage.Chinese, "死亡之触" },
                { ModLanguage.German, "Todesberührung" },
                { ModLanguage.Russian, "Касание смерти" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Death Touch" },
                { ModLanguage.Japanese, "死の接触" },
                { ModLanguage.Chinese, "死亡之触" },
                { ModLanguage.German, "Todesberührung" },
                { ModLanguage.Russian, "Касание смерти" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Deals 10–12 Unholy Damage (Range: 4–5) with Immobilization Chance. Replenishes Health to all allies within Vision for 40–50% of the damage dealt. If the target has \"Withering Curse\": +25% damage and adds 1 curse stack. Consuming 1 \"Essence Charge\" grants +15% Magic Power for this cast only." },
                { ModLanguage.Japanese, "不浄ダメージ10〜12を与える（射程: 4〜5、拘束確率あり）。視界内の全味方の体力を、与ダメージの40〜50%分回復する。対象が「枯死の呪い」を持つ場合：ダメージ+25%、呪いスタック+1。「本質充填」を1消費すると、この詠唱のみ魔力+15%。" },
                { ModLanguage.Chinese, "造成10–12点邪秽伤害（射程：4–5），附带定身几率。按造成伤害的40–50%恢复视野内所有盟友的生命。若目标带有“凋零诅咒”：伤害+25%，并增加1层诅咒。消耗1层“精华充能”可在本次施法中获得+15%魔法强度。" },
                { ModLanguage.German, "Verursacht 10–12 Unheiligen Schaden (Reichweite: 4–5) mit Immobilisierungschance. Stellt allen Verbündeten in Sichtweite 40–50% des verursachten Schadens als Gesundheit wieder her. Hat das Ziel „Verdorrender Fluch“: +25% Schaden und +1 Fluchstapel. Verbrauch von 1 „Essenzladung“ gewährt +15% Magiekraft nur für diesen Wurf." },
                { ModLanguage.Russian, "Наносит 10–12 урона нечестием (дальность: 4–5) со шансом обездвижить. Восстанавливает здоровье всем союзникам в поле зрения на 40–50% от нанесённого урона. Если на цели «Иссушающее проклятие»: +25% урона и +1 стак проклятия. Расход 1 «Заряда сущности» даёт +15% силы магии только для этого применения." },
            });
        Msl.InjectTableItemLocalization(
            "corpse_burst",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Corpse Burst" },
                { ModLanguage.Japanese, "死体爆破" },
                { ModLanguage.Chinese, "尸爆" },
                { ModLanguage.German, "Leichenexplosion" },
                { ModLanguage.Russian, "Взрыв трупа" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Corpse Burst" },
                { ModLanguage.Japanese, "死体爆破" },
                { ModLanguage.Chinese, "尸爆" },
                { ModLanguage.German, "Leichenexplosion" },
                { ModLanguage.Russian, "Взрыв трупа" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Targets a corpse or allied Undead/Skeleton and makes it explode, destroying it. Deals Unholy and Piercing Damage in a base 3x3 area around the target and has a chance to apply Bleeding to affected enemies. Damage, Bleeding chance, and area size scale with Willpower (area grows from 3x3 toward 5x5 / 7x7). Exploding an ally causes a small Sanity loss." },
                { ModLanguage.Japanese, "死体または味方のアンデッド/スケルトンを爆発させ破壊する。対象周囲の基本3x3に不浄と刺突ダメージを与え、被弾敵に出血を付与する確率がある。ダメージ・出血確率・範囲は意志力に比例（範囲は3x3から5x5 / 7x7へ拡大）。味方を爆発させると正気をわずかに失う。" },
                { ModLanguage.Chinese, "以尸体或友方亡灵/骷髅为目标使其爆炸并摧毁。在目标周围基础3x3范围内造成邪秽与穿刺伤害，并有几率使受影响敌人出血。伤害、出血几率与范围随意志力提升（范围从3x3扩大至5x5 / 7x7）。炸毁盟友会造成少量理智损失。" },
                { ModLanguage.German, "Zielt auf eine Leiche oder einen verbündeten Untoten/Skelett und lässt sie explodieren (Zerstörung). Verursacht Unheiligen und Stichschaden in einem Basisgebiet 3x3 und kann Blutung verursachen. Schaden, Blutungschance und Gebietsgröße skalieren mit Willenskraft (Gebiet wächst von 3x3 zu 5x5 / 7x7). Explosion eines Verbündeten verursacht kleinen Verstandverlust." },
                { ModLanguage.Russian, "Целится в труп или союзную нежить/скелета и взрывает их, уничтожая. Наносит урон нечестием и колющий урон в базовой области 3x3 и может наложить кровотечение. Урон, шанс кровотечения и размер области зависят от Воли (область растёт с 3x3 до 5x5 / 7x7). Взрыв союзника вызывает небольшую потерю рассудка." },
            });
        Msl.InjectTableItemLocalization(
            "mastery_of_binding",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Mastery of Binding" },
                { ModLanguage.Japanese, "束縛の極意" },
                { ModLanguage.Chinese, "束缚精通" },
                { ModLanguage.German, "Meisterschaft der Bindung" },
                { ModLanguage.Russian, "Мастерство связывания" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Mastery of Binding" },
                { ModLanguage.Japanese, "束縛の極意" },
                { ModLanguage.Chinese, "束缚精通" },
                { ModLanguage.German, "Meisterschaft der Bindung" },
                { ModLanguage.Russian, "Мастерство связывания" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Unbinding allies doesn't take a turn and replenishes Energy equal to 10% of your Magic Power. Increases the level of summoning abilities by 1. Summon Skeleton: +2 turns base duration and +10% Max Health / Max Energy. Resurrection thralls start at 60% Max Health. Still limited to one safe Undead at a time unless a higher-tier skill allows more." },
                { ModLanguage.Japanese, "味方の束縛解除はターンを消費せず、魔力の10%相当のエネルギーを回復する。召喚系能力のレベル+1。スケルトン召喚：基本持続+2ターン、最大体力/最大エネルギー+10%。復活の下僕は最大体力60%で開始。上位スキルがない限り、安全なアンデッドは同時に1体まで。" },
                { ModLanguage.Chinese, "解除盟友束缚不消耗回合，并回复相当于魔法强度10%的能量。召唤类技能等级+1。召唤骷髅：基础持续时间+2回合，最大生命/最大能量+10%。复活仆从以60%最大生命起步。除非更高阶技能允许，否则同时仅可安全控制一个亡灵。" },
                { ModLanguage.German, "Lösen von Verbündeten kostet keinen Zug und stellt Energie in Höhe von 10% eurer Magiekraft wieder her. Erhöht das Level von Beschwörungsfähigkeiten um 1. Skelett beschwören: +2 Züge Basisdauer und +10% max. Gesundheit / max. Energie. Auferstehungsdiener starten mit 60% max. Gesundheit. Weiterhin auf einen sicheren Untoten gleichzeitig begrenzt, sofern keine höhere Fähigkeit mehr erlaubt." },
                { ModLanguage.Russian, "Освобождение союзников не тратит ход и восстанавливает энергию, равную 10% вашей силы магии. Повышает уровень способностей призыва на 1. Призыв скелета: +2 хода базовой длительности и +10% макс. здоровья / макс. энергии. Слуги воскрешения начинают с 60% макс. здоровья. По-прежнему не более одной безопасной нежити одновременно, если более высокий навык не позволяет больше." },
            });
        Msl.InjectTableItemLocalization(
            "black_sepulchre",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Black Sepulchre" },
                { ModLanguage.Japanese, "黒の墓廟" },
                { ModLanguage.Chinese, "黑色墓室" },
                { ModLanguage.German, "Schwarze Grabstätte" },
                { ModLanguage.Russian, "Чёрная усыпальница" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Black Sepulchre" },
                { ModLanguage.Japanese, "黒の墓廟" },
                { ModLanguage.Chinese, "黑色墓室" },
                { ModLanguage.German, "Schwarze Grabstätte" },
                { ModLanguage.Russian, "Чёрная усыпальница" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Detonates \"Withering Curse\" on the target (Range: 5–6), or on the strongest cursed enemy in a 3x3 if cast on a tile: removes stacks, deals 6 Unholy Damage × stacks to the target and 3 Unholy Damage × stacks to adjacent enemies. Allows \"Withering Curse\" to stack up to III. Bodypart worsen becomes 0.5% per turn per stack while at III." },
                { ModLanguage.Japanese, "対象の「枯死の呪い」を起爆する（射程: 5〜6）。タイル詠唱時は3x3内で最も強い呪い持ちを対象：スタックを除去し、対象に不浄ダメージ6×スタック、隣接敵に3×スタック。呪いを最大IIIまで重複可能。III時の部位悪化はスタックごとに毎ターン0.5%。" },
                { ModLanguage.Chinese, "引爆目标身上的“凋零诅咒”（射程：5–6）；若对格子施放，则引爆3x3内最强的被诅咒敌人：移除层数，对目标造成6×层数邪秽伤害，对相邻敌人造成3×层数邪秽伤害。使“凋零诅咒”可叠至III。达到III时，部位恶化变为每层每回合0.5%。" },
                { ModLanguage.German, "Zündet „Verdorrender Fluch“ am Ziel (Reichweite: 5–6) oder am stärksten verfluchten Feind in 3x3 bei Feldwirken: entfernt Stapel, verursacht 6 Unheiligen Schaden × Stapel am Ziel und 3 × Stapel an angrenzenden Feinden. Erlaubt Stapelung bis III. Bei III: Körperteilverschlechterung 0,5% pro Zug und Stapel." },
                { ModLanguage.Russian, "Детонирует «Иссушающее проклятие» на цели (дальность: 5–6) или на сильнейшем проклятом враге в 3x3 при применении на клетку: снимает стаки, наносит цели 6 урона нечестием × стаки и соседним врагам 3 × стаки. Позволяет стакать проклятие до III. При III ухудшение частей тела: 0,5% за ход за стак." },
            });
        Msl.InjectTableItemLocalization(
            "sealed_mind",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Sealed Mind" },
                { ModLanguage.Japanese, "封印された精神" },
                { ModLanguage.Chinese, "封印之心" },
                { ModLanguage.German, "Versiegelter Geist" },
                { ModLanguage.Russian, "Запечатанный разум" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Sealed Mind" },
                { ModLanguage.Japanese, "封印された精神" },
                { ModLanguage.Chinese, "封印之心" },
                { ModLanguage.German, "Versiegelter Geist" },
                { ModLanguage.Russian, "Запечатанный разум" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Halves sanity loss of Occultism abilities and partially decreases penalties of \"Obsession\". Grants 33% Unholy Resistance." },
                { ModLanguage.Japanese, "神秘学能力の正気減少を半減し、「執着」のペナルティを部分的に軽減する。不浄耐性33%を付与する。" },
                { ModLanguage.Chinese, "使神秘学技能的理智损失减半，并部分降低“执念”惩罚。赋予33%邪秽抗性。" },
                { ModLanguage.German, "Halbiert den Verstandverlust von Okkultismus-Fähigkeiten und verringert teilweise die Nachteile von „Besessenheit“. Gewährt 33% Unheiligen Widerstand." },
                { ModLanguage.Russian, "Вдвое снижает потерю рассудка от способностей оккультизма и частично уменьшает штрафы «Одержимости». Даёт 33% сопротивления нечестию." },
            });
        Msl.InjectTableItemLocalization(
            "immortality",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Immortality" },
                { ModLanguage.Japanese, "不死" },
                { ModLanguage.Chinese, "不朽" },
                { ModLanguage.German, "Unsterblichkeit" },
                { ModLanguage.Russian, "Бессмертие" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Immortality" },
                { ModLanguage.Japanese, "不死" },
                { ModLanguage.Chinese, "不朽" },
                { ModLanguage.German, "Unsterblichkeit" },
                { ModLanguage.Russian, "Бессмертие" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Prevents dying from lethal hits as long as Max Health and Morale are both higher than 50%. When Health drops below 2, replenishes 33% of Max Health and Max Energy, and applies: -10% Morale, +2% Fatigue. Prevents death from starvation or dehydration." },
                { ModLanguage.Japanese, "最大体力と士気がともに50%超なら致死ヒットで死亡しない。体力が2未満になると、最大体力と最大エネルギーの33%を回復し、-10%士気、+2%疲労を付与。餓死・渇死を防ぐ。" },
                { ModLanguage.Chinese, "只要最大生命与士气均高于50%，就不会因致命一击死亡。当生命降至2以下时，回复33%最大生命与最大能量，并施加：-10%士气、+2%疲劳。防止因饥饿或脱水而死。" },
                { ModLanguage.German, "Verhindert den Tod durch tödliche Treffer, solange max. Gesundheit und Moral beide über 50% liegen. Sinkt die Gesundheit unter 2, werden 33% von max. Gesundheit und max. Energie wiederhergestellt und angewendet: -10% Moral, +2% Erschöpfung. Verhindert Tod durch Hunger oder Durst." },
                { ModLanguage.Russian, "Предотвращает смерть от смертельных ударов, пока макс. здоровье и мораль выше 50%. Когда здоровье падает ниже 2, восстанавливает 33% макс. здоровья и макс. энергии и накладывает: -10% морали, +2% усталости. Предотвращает смерть от голода или жажды." },
            });
        Msl.InjectTableItemLocalization(
            "blooming_madness",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Blooming Madness" },
                { ModLanguage.Japanese, "咲き乱れる狂気" },
                { ModLanguage.Chinese, "绽放的疯狂" },
                { ModLanguage.German, "Blühender Wahnsinn" },
                { ModLanguage.Russian, "Расцветающее безумие" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Blooming Madness" },
                { ModLanguage.Japanese, "咲き乱れる狂気" },
                { ModLanguage.Chinese, "绽放的疯狂" },
                { ModLanguage.German, "Blühender Wahnsinn" },
                { ModLanguage.Russian, "Расцветающее безумие" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Successfully raising the dead allows you to learn one of their active abilities at a time (while available via skill menu; lost when the thrall dies). Increases Experience Gain of Undead kills by 100%." },
                { ModLanguage.Japanese, "死者の蘇生に成功すると、その主動能力を1つ習得できる（スキルメニューで利用可能中。下僕が死ぬと失う）。アンデッド撃破の経験値獲得+100%。" },
                { ModLanguage.Chinese, "成功复活死者后，可同时学习其一项主动技能（在技能菜单可用期间；仆从死亡后失去）。亡灵击杀的经验获取+100%。" },
                { ModLanguage.German, "Erfolgreiches Auferwecken der Toten erlaubt das Erlernen einer ihrer aktiven Fähigkeiten gleichzeitig (solange im Menü verfügbar; verloren, wenn der Diener stirbt). Erhöht Erfahrungsgewinn durch Untoten-Kills um 100%." },
                { ModLanguage.Russian, "Успешное воскрешение мёртвых позволяет изучить одну их активную способность за раз (пока доступна в меню навыков; теряется при смерти слуги). Увеличивает получаемый опыт за убийства нежити на 100%." },
            });
        Msl.InjectTableItemLocalization(
            "archtheurgy",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Archtheurgy" },
                { ModLanguage.Japanese, "大神業" },
                { ModLanguage.Chinese, "大奇术" },
                { ModLanguage.German, "Archtheurgie" },
                { ModLanguage.Russian, "Архитеургия" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Archtheurgy" },
                { ModLanguage.Japanese, "大神業" },
                { ModLanguage.Chinese, "大奇术" },
                { ModLanguage.German, "Archtheurgie" },
                { ModLanguage.Russian, "Архитеургия" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Casting Spells deals Unholy Damage to all enemies within vision (based on 3.33% of Magic Power). Raising the dead, summoning wraiths, Stealing Essence, or receiving \"Death's Blessing\" has 33% chance to activate \"Ascension\" for 6 turns: +33% Unholy Resistance, +66% Miracle Potency, converts Unholy Damage into Sacred Damage." },
                { ModLanguage.Japanese, "呪文詠唱時、視界内の全敵に不浄ダメージを与える（魔力の3.33%基準）。死者蘇生・レイス召喚・本質略奪・「死の祝福」受け取り時、33%で「昇華」を6ターン発動：不浄耐性+33%、奇跡効力+66%、不浄ダメージを聖なるダメージへ変換。" },
                { ModLanguage.Chinese, "施放法术时对视野内所有敌人造成邪秽伤害（基于魔法强度的3.33%）。复活死者、召唤怨灵、窃取精华或获得“死亡赐福”时，有33%几率激活“升扬”，持续6回合：+33%邪秽抗性、+66%奇迹效力，并将邪秽伤害转化为神圣伤害。" },
                { ModLanguage.German, "Zauberwirken verursacht Unheiligen Schaden an allen Feinden in Sichtweite (basiert auf 3,33% Magiekraft). Auferwecken, Geisterbeschwörung, Essenzstehlen oder Erhalt von „Segen des Todes“ hat 33% Chance, „Aufstieg“ für 6 Züge zu aktivieren: +33% Unheiliger Widerstand, +66% Wunderstärke, wandelt Unheiligen Schaden in Heiligen Schaden um." },
                { ModLanguage.Russian, "Применение заклинаний наносит урон нечестием всем врагам в поле зрения (на основе 3,33% силы магии). Воскрешение, призыв умертвий, кража сущности или получение «Благословения смерти» с шансом 33% активирует «Вознесение» на 6 ходов: +33% сопротивления нечестию, +66% силы чудес, преобразует урон нечестием в священный урон." },
            });
        Msl.InjectTableItemLocalization(
            "wraith_summoning",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Wraith Summoning" },
                { ModLanguage.Japanese, "レイス召喚" },
                { ModLanguage.Chinese, "召唤怨灵" },
                { ModLanguage.German, "Geisterbeschwörung" },
                { ModLanguage.Russian, "Призыв умертвия" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Wraith Summoning" },
                { ModLanguage.Japanese, "レイス召喚" },
                { ModLanguage.Chinese, "召唤怨灵" },
                { ModLanguage.German, "Geisterbeschwörung" },
                { ModLanguage.Russian, "Призыв умертвия" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Summons a Wraith on the targeted tile; power depends on \"Essence Charge\" stacks. Removes \"Essence Charge\". If no weapon equipped: -50% Cooldown Duration and Halves Miscast Chance. Risk of \"Obsession\": Extremely High. Summoning more than one Wraith instantly causes \"Obsession\". Can't summon Wraiths higher than the skill's level cap. Cannot cast under \"Obsession\"." },
                { ModLanguage.Japanese, "対象タイルにレイスを召喚する。強さは「本質充填」のスタックに依存。「本質充填」を除去。武器未装備時：クールダウン-50%、詠唱失敗率半減。「執着」のリスク: 極めて高い。レイス2体以上で即座に「執着」。スキルのレベル上限を超えるレイスは召喚不可。「執着」中は詠唱不可。" },
                { ModLanguage.Chinese, "在目标格子召唤怨灵；强度取决于“精华充能”层数。移除“精华充能”。未装备武器时：冷却时间-50%，施法失败几率减半。“执念”风险：极高。召唤超过一个怨灵会立刻导致“执念”。无法召唤高于技能等级上限的怨灵。“执念”状态下无法施放。" },
                { ModLanguage.German, "Beschwört einen Geist auf dem Zielfeld; Stärke hängt von „Essenzladung“-Stapeln ab. Entfernt „Essenzladung“. Ohne Waffe: -50% Abklingzeit und halbe Fehlschlagchance. Risiko von „Besessenheit“: Extrem hoch. Mehr als ein Geist verursacht sofort „Besessenheit\". Keine Geister über dem Level-Limit. Nicht unter „Besessenheit“ wirkbar." },
                { ModLanguage.Russian, "Призывает умертвие на выбранную клетку; сила зависит от стаков «Заряда сущности». Снимает «Заряд сущности». Без оружия: -50% длительности перезарядки и вдвое меньший шанс сбоя. Риск «Одержимости»: крайне высокий. Призыв более одного умертвия мгновенно вызывает «Одержимость». Нельзя призвать умертвия выше уровня навыка. Нельзя использовать под «Одержимостью»." },
            });
    }

    static void PatchSpeeches()
    {
        Msl.InjectTableDialogLocalization(new LocalizationSentence[]
        {
            new LocalizationSentence(
                "desecration",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "In Nag Zur..." },
                    { ModLanguage.Japanese, "イン・ナグ・ズール…" },
                    { ModLanguage.Chinese, "因纳戈祖尔……" },
                    { ModLanguage.German, "In Nag Zur..." },
                    { ModLanguage.Russian, "In Nag Zur..." },
                }),
            new LocalizationSentence(
                "bone_ward",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Ossarak Veth!" },
                    { ModLanguage.Japanese, "オッサラク・ヴェス！" },
                    { ModLanguage.Chinese, "奥萨拉克·维斯！" },
                    { ModLanguage.German, "Ossarak Veth!" },
                    { ModLanguage.Russian, "Ossarak Veth!" },
                }),
            new LocalizationSentence(
                "summon_skeleton",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Rattlek Narzul!" },
                    { ModLanguage.Japanese, "ラットレク・ナルズル！" },
                    { ModLanguage.Chinese, "拉特雷克·纳祖尔！" },
                    { ModLanguage.German, "Rattlek Narzul!" },
                    { ModLanguage.Russian, "Rattlek Narzul!" },
                }),
            new LocalizationSentence(
                "withering_curse",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Shaggrath Azud..." },
                    { ModLanguage.Japanese, "シャグラト・アズード！" },
                    { ModLanguage.Chinese, "沙格拉斯'阿祖得！" },
                    { ModLanguage.German, "Shaggrath Azud..." },
                    { ModLanguage.Russian, "Shaggrath Azud..." },
                }),
            new LocalizationSentence(
                "essence_leech",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "En'thero!" },
                    { ModLanguage.Japanese, "エンセロー！" },
                    { ModLanguage.Chinese, "恩塞罗！" },
                    { ModLanguage.German, "En'thero!" },
                    { ModLanguage.Russian, "En'thero!" },
                }),
            new LocalizationSentence(
                "deaths_blessing",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Yagrak Atot!" },
                    { ModLanguage.Japanese, "ヤグラク・アトット！" },
                    { ModLanguage.Chinese, "亚格拉克'阿托特！" },
                    { ModLanguage.German, "Yagrak Atot!" },
                    { ModLanguage.Russian, "Yagrak Atot!" },
                }),
            new LocalizationSentence(
                "resurrection",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Lagur! Lagur! Lagur!" },
                    { ModLanguage.Japanese, "ラグール！ ラグール！ ラグール！" },
                    { ModLanguage.Chinese, "拉古尔！拉古尔！拉古尔！" },
                    { ModLanguage.German, "Lagur! Lagur! Lagur!" },
                    { ModLanguage.Russian, "Lagur! Lagur! Lagur!" },
                }),
            new LocalizationSentence(
                "grave_blight",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Morgath En'ther!" },
                    { ModLanguage.Japanese, "モルガス・エンサー！" },
                    { ModLanguage.Chinese, "莫加斯·恩瑟！" },
                    { ModLanguage.German, "Morgath En'ther!" },
                    { ModLanguage.Russian, "Morgath En'ther!" },
                }),
            new LocalizationSentence(
                "death_touch",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Korrug Namar!" },
                    { ModLanguage.Japanese, "コールグ・ナマール！" },
                    { ModLanguage.Chinese, "廓卢戈'纳玛尔！" },
                    { ModLanguage.German, "Korrug Namar!" },
                    { ModLanguage.Russian, "Korrug Namar!" },
                }),
            new LocalizationSentence(
                "corpse_burst",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Barath Shatterk!" },
                    { ModLanguage.Japanese, "バラース・シャッターク！" },
                    { ModLanguage.Chinese, "巴拉斯·沙特克！" },
                    { ModLanguage.German, "Barath Shatterk!" },
                    { ModLanguage.Russian, "Barath Shatterk!" },
                }),
            new LocalizationSentence(
                "mastery_of_binding",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Vinculum Mortis!" },
                    { ModLanguage.Japanese, "ウィンクルム・モルティス！" },
                    { ModLanguage.Chinese, "温库鲁姆·莫提斯！" },
                    { ModLanguage.German, "Vinculum Mortis!" },
                    { ModLanguage.Russian, "Vinculum Mortis!" },
                }),
            new LocalizationSentence(
                "black_sepulchre",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Shaggrath Namar!" },
                    { ModLanguage.Japanese, "シャグラト・ナマール！" },
                    { ModLanguage.Chinese, "沙格拉斯'纳玛尔！" },
                    { ModLanguage.German, "Shaggrath Namar!" },
                    { ModLanguage.Russian, "Shaggrath Namar!" },
                }),
            new LocalizationSentence(
                "sealed_mind",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Nox Mentis Seal!" },
                    { ModLanguage.Japanese, "ノックス・メンティス・シール！" },
                    { ModLanguage.Chinese, "诺克斯·门提斯·封印！" },
                    { ModLanguage.German, "Nox Mentis Seal!" },
                    { ModLanguage.Russian, "Nox Mentis Seal!" },
                }),
            new LocalizationSentence(
                "immortality",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Aeternum Mortis!" },
                    { ModLanguage.Japanese, "アエテルヌム・モルティス！" },
                    { ModLanguage.Chinese, "埃特尔努姆·莫提斯！" },
                    { ModLanguage.German, "Aeternum Mortis!" },
                    { ModLanguage.Russian, "Aeternum Mortis!" },
                }),
            new LocalizationSentence(
                "blooming_madness",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Florath Insania!" },
                    { ModLanguage.Japanese, "フロラス・インサニア！" },
                    { ModLanguage.Chinese, "弗洛拉斯·因萨尼亚！" },
                    { ModLanguage.German, "Florath Insania!" },
                    { ModLanguage.Russian, "Florath Insania!" },
                }),
            new LocalizationSentence(
                "archtheurgy",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Archon Theurgos!" },
                    { ModLanguage.Japanese, "アルコン・テウルゴス！" },
                    { ModLanguage.Chinese, "阿尔孔·特乌尔戈斯！" },
                    { ModLanguage.German, "Archon Theurgos!" },
                    { ModLanguage.Russian, "Archon Theurgos!" },
                }),
            new LocalizationSentence(
                "wraith_summoning",
                new Dictionary<ModLanguage, string>
                {
                    { ModLanguage.English, "Lagura mithensa!" },
                    { ModLanguage.Japanese, "コールグ・ナマール！" },
                    { ModLanguage.Chinese, "廓卢戈'纳玛尔！" },
                    { ModLanguage.German, "Korrug Namar!" },
                    { ModLanguage.Russian, "Korrug Namar!" },
                })
        });
    }
}
