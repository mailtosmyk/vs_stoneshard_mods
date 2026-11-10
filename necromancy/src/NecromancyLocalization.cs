using ModShardLauncher;
using ModShardLauncher.Mods;
using Necromancy.Skills;

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
        PatchBuffs();
        PatchBooks();
        PatchSpeeches();
    }

    static void PatchBooks()
    {
        foreach (var book in NecromancySkills.EnabledBooks)
        {
            var skillLines = string.Join(
                "#",
                NecromancySkills.ByTier(book.Tier).Select(s => $"~lg~{s.Name}~/~"));

            var mid = string.IsNullOrEmpty(skillLines)
                ? book.Description
                : $"##~gr~Allows you to learn the following Necromancy abilities:~/~##{skillLines}##Reading this book grants some ~y~Experience~/~.";

            var name = new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, book.Name },
                { ModLanguage.Russian, book.Name },
            };
            var type = new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Necromantic treatise" },
                { ModLanguage.Russian, "Некромантский трактат" },
            };
            var description = new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, string.IsNullOrWhiteSpace(book.Description) ? mid : $"{book.Description}{mid}" },
                { ModLanguage.Russian, string.IsNullOrWhiteSpace(book.Description) ? mid : $"{book.Description}{mid}" },
            };

            Msl.InjectTableItemLocalization(book.LocalizationId, name, type, description);
        }
    }

    static void PatchBuffs()
    {
        PendingBuffLocs.Clear();

        // Buff bar titles use global.buff_name_text from table_effects (scr_buff_atr),
        // not table_items — InjectTableItemLocalization alone shows "o_b_bone_ward".
        RegisterBuffLocalization(
            "o_db_withering_curse",
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
                { ModLanguage.English, "~r~-8%~/~ Fortitude#~r~-8%~/~ Magic Resistance#~r~-5%~/~ Max Health#~lg~+3%~/~ Damage Taken##Worsens a random body part each turn." },
                { ModLanguage.Japanese, "~r~-8%~/~頑健#~r~-8%~/~魔法耐性#~r~-5%~/~最大体力#~lg~+3%~/~被ダメージ##毎ターン部位を悪化させる。" },
                { ModLanguage.Chinese, "~r~-8%~/~坚韧#~r~-8%~/~魔法抗性#~r~-5%~/~最大生命#~lg~+3%~/~受到伤害##每回合恶化随机部位。" },
                { ModLanguage.German, "~r~-8%~/~ Zähigkeit#~r~-8%~/~ Magiewiderstand#~r~-5%~/~ max. Gesundheit#~lg~+3%~/~ erlittener Schaden##Verschlechtert jeden Zug einen Körperteil." },
                { ModLanguage.Russian, "~r~-8%~/~ стойкости#~r~-8%~/~ маг. сопротивления#~r~-5%~/~ макс. здоровья#~lg~+3%~/~ получаемого урона##Каждый ход ухудшает часть тела." },
            });

        RegisterBuffLocalization(
            "o_b_bone_ward",
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
                { ModLanguage.English, "~lg~+12%~/~ Physical Resistance#~lg~+15%~/~ Bleed Resistance#~lg~+10%~/~ Magic Resistance#~lg~+10%~/~ Move Resistance##Shatters into bone shards when it ends." },
                { ModLanguage.Japanese, "~lg~+12%~/~物理耐性#~lg~+15%~/~出血耐性#~lg~+10%~/~魔法耐性#~lg~+10%~/~移動耐性##終了時に骨片が飛散する。" },
                { ModLanguage.Chinese, "~lg~+12%~/~物理抗性#~lg~+15%~/~出血抗性#~lg~+10%~/~魔法抗性#~lg~+10%~/~移动抗性##结束时碎裂为骨片。" },
                { ModLanguage.German, "~lg~+12%~/~ physischer Widerstand#~lg~+15%~/~ Blutungswiderstand#~lg~+10%~/~ Magiewiderstand#~lg~+10%~/~ Bewegungswiderstand##Zerbricht am Ende in Knochensplitter." },
                { ModLanguage.Russian, "~lg~+12%~/~ физ. сопротивления#~lg~+15%~/~ сопротивления кровотечению#~lg~+10%~/~ маг. сопротивления#~lg~+10%~/~ сопротивления перемещению##При окончании разлетается осколками." },
            });

        RegisterBuffLocalization(
            "o_b_nm_desecration",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Unholy Damage" },
                { ModLanguage.Japanese, "不浄ダメージ" },
                { ModLanguage.Chinese, "不洁伤害" },
                { ModLanguage.German, "Unheiliger Schaden" },
                { ModLanguage.Russian, "Нечестивый урон" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "~ur~+Unholy Damage~/~##Granted by ~lg~\"Dark Bolt\"~/~. Does not stack." },
                { ModLanguage.Japanese, "~ur~+不浄ダメージ~/~##~lg~「ダークボルト」~/~により付与。重複しない。" },
                { ModLanguage.Chinese, "~ur~+不洁伤害~/~##由~lg~\"暗矢\"~/~赋予。不可叠加。" },
                { ModLanguage.German, "~ur~+Unheiliger Schaden~/~##Durch ~lg~\"Dunkler Bolzen\"~/~ gewährt. Stapelt nicht." },
                { ModLanguage.Russian, "~ur~+нечестивого урона~/~##Даётся ~lg~\"Тёмным зарядом\"~/~. Не суммируется." },
            });

        RegisterBuffLocalization(
            "o_b_nm_skeleton_timer",
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
                { ModLanguage.English, "A summoned Skeleton fights for you.##Ends early if the Skeleton is destroyed." },
                { ModLanguage.Japanese, "召喚したスケルトンが味方として戦う。##スケルトンが倒れると効果は終了する。" },
                { ModLanguage.Chinese, "召唤的骷髅为你而战。##骷髅被摧毁时提前结束。" },
                { ModLanguage.German, "Ein beschworenes Skelett kämpft für dich.##Endet vorzeitig, wenn das Skelett zerstört wird." },
                { ModLanguage.Russian, "Призванный скелет сражается на вашей стороне.##Заканчивается раньше, если скелет уничтожен." },
            });

        RegisterBuffLocalization(
            "o_b_nm_thrall_timer",
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
                { ModLanguage.English, "A reanimated Undead fights for you.##Ends early if the thrall is destroyed." },
                { ModLanguage.Japanese, "蘇生したアンデッドが味方として戦う。##スレイブが倒れると効果は終了する。" },
                { ModLanguage.Chinese, "复活的亡灵为你而战。##仆从被摧毁时提前结束。" },
                { ModLanguage.German, "Ein wiederbelebter Untoter kämpft für dich.##Endet vorzeitig, wenn der Diener zerstört wird." },
                { ModLanguage.Russian, "Оживлённая нежить сражается на вашей стороне.##Заканчивается раньше, если слуга уничтожен." },
            });

        RegisterBuffLocalization(
            "o_b_nm_essence_charge",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Essence Charge" },
                { ModLanguage.Japanese, "本質充填" },
                { ModLanguage.Chinese, "精华充能" },
                { ModLanguage.German, "Essenzladung" },
                { ModLanguage.Russian, "Заряд сущности" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "~lg~+5%~/~ Magic Power#~r~-5%~/~ Abilities Energy Cost#~lg~+4~/~ Max Energy##Stacks up to II. Spells consume 1 stack." },
                { ModLanguage.Japanese, "~lg~+5%~/~魔力#~r~-5%~/~能力のエネルギー消費#~lg~+4~/~最大エネルギー##最大IIまで重複。呪文使用で1減少。" },
                { ModLanguage.Chinese, "~lg~+5%~/~魔法强度#~r~-5%~/~技能能量消耗#~lg~+4~/~最大能量##最多叠至II。施放法术消耗1层。" },
                { ModLanguage.German, "~lg~+5%~/~ Magiekraft#~r~-5%~/~ Energiekosten von Fähigkeiten#~lg~+4~/~ max. Energie##Stapelt bis II. Zauber verbrauchen 1 Stapel." },
                { ModLanguage.Russian, "~lg~+5%~/~ силы магии#~r~-5%~/~ стоимости энергии способностей#~lg~+4~/~ макс. энергии##До II стаков. Заклинания снимают 1 стак." },
            });

        RegisterBuffLocalization(
            "o_b_nm_deaths_blessing",
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
                { ModLanguage.English, "~lg~+6%~/~ Life Drain#~lg~+6%~/~ Energy Drain#~lg~+8%~/~ Damage Reflection" },
                { ModLanguage.Japanese, "~lg~+6%~/~生命吸収#~lg~+6%~/~エネルギー吸収#~lg~+8%~/~ダメージ反射" },
                { ModLanguage.Chinese, "~lg~+6%~/~生命汲取#~lg~+6%~/~能量汲取#~lg~+8%~/~伤害反射" },
                { ModLanguage.German, "~lg~+6%~/~ Lebensraub#~lg~+6%~/~ Energieraub#~lg~+8%~/~ Schadensreflexion" },
                { ModLanguage.Russian, "~lg~+6%~/~ вампиризма#~lg~+6%~/~ кражи энергии#~lg~+8%~/~ отражения урона" },
            });

        RegisterBuffLocalization(
            "o_b_nm_grave_blight",
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
                { ModLanguage.English, "A blighted 3x3 zone.##Enemies inside take ~ur~2 Unholy Damage~/~ per turn and ~r~-4%~/~ Magic Resistance (stacks up to 3)." },
                { ModLanguage.Japanese, "3x3の疫病地帯。##内部の敵は毎ターン~ur~不浄ダメージ2~/~と魔法耐性~r~-4%~/~（最大3スタック）。" },
                { ModLanguage.Chinese, "3x3疫病区域。##区域内敌人每回合受到~ur~2点邪秽伤害~/~，并获得~r~-4%~/~魔法抗性（最多3层）。" },
                { ModLanguage.German, "Verdorbenes 3x3-Gebiet.##Feinde darin erleiden ~ur~2 Unheiligen Schaden~/~ pro Zug und ~r~-4%~/~ Magiewiderstand (bis 3 Stapel)." },
                { ModLanguage.Russian, "Заражённая зона 3x3.##Враги внутри получают ~ur~2 урона нечестием~/~ за ход и ~r~-4%~/~ маг. сопротивления (до 3 стаков)." },
            });

        RegisterBuffLocalization(
            "o_db_nm_grave_blight",
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
                { ModLanguage.English, "~r~-4%~/~ Magic Resistance per stack (up to III) while inside the blight." },
                { ModLanguage.Japanese, "疫病内でスタックごとに魔法耐性~r~-4%~/~（最大III）。" },
                { ModLanguage.Chinese, "处于疫病中时每层~r~-4%~/~魔法抗性（最高III）。" },
                { ModLanguage.German, "~r~-4%~/~ Magiewiderstand pro Stapel (bis III) in der Fäule." },
                { ModLanguage.Russian, "~r~-4%~/~ маг. сопротивления за стак (до III), пока в порче." },
            });

        Msl.LoadGML("gml_GlobalScript_table_effects")
            .Apply(InjectNecromancyTableEffectsRows)
            .Save();
    }

    static readonly List<(string Id, Dictionary<ModLanguage, string> Name, Dictionary<ModLanguage, string> Desc)> PendingBuffLocs = new();

    static void RegisterBuffLocalization(
        string id,
        Dictionary<ModLanguage, string> name,
        Dictionary<ModLanguage, string> description)
    {
        PendingBuffLocs.Add((id, name, description));
    }

    static IEnumerable<string> InjectNecromancyTableEffectsRows(IEnumerable<string> lines)
    {
        const string nameMarker = "\";buff_name_end;";
        const string descMarker = "\";buff_desc_end;";

        var nameInsert = new System.Text.StringBuilder();
        var descInsert = new System.Text.StringBuilder();
        foreach (var entry in PendingBuffLocs)
        {
            nameInsert.Append(SkillCsvRow(entry.Id, entry.Name));
            descInsert.Append(SkillCsvRow(entry.Id, entry.Desc));
        }

        var nameText = nameInsert.ToString();
        var descText = descInsert.ToString();
        const string nameGuard = "\"o_b_bone_ward;";

        foreach (var line in lines)
        {
            var result = line;
            if (result.Contains(nameMarker, StringComparison.Ordinal))
            {
                if (result.Contains(nameGuard, StringComparison.Ordinal))
                {
                    foreach (var entry in PendingBuffLocs)
                        result = UpsertEffectsCsvRow(result, entry.Id, entry.Name, nameMarker);
                }
                else
                {
                    result = result.Replace(nameMarker, nameText + nameMarker, StringComparison.Ordinal);
                }
            }

            if (result.Contains(descMarker, StringComparison.Ordinal))
            {
                // After names are present (same giant return line), upsert/insert descs too.
                if (result.Contains(nameGuard, StringComparison.Ordinal))
                {
                    foreach (var entry in PendingBuffLocs)
                        result = UpsertEffectsCsvRow(result, entry.Id, entry.Desc, descMarker);
                }
                else
                {
                    result = result.Replace(descMarker, descText + descMarker, StringComparison.Ordinal);
                }
            }

            yield return result;
        }
    }

    static string UpsertEffectsCsvRow(
        string line,
        string id,
        Dictionary<ModLanguage, string> texts,
        string sectionEndMarker)
    {
        var newRow = SkillCsvRow(id, texts);
        var sectionEnd = line.IndexOf(sectionEndMarker, StringComparison.Ordinal);
        if (sectionEnd < 0)
            return line;

        // Name and desc share one giant return line — only touch the matching section.
        var sectionStartMarker = sectionEndMarker.Contains("buff_name", StringComparison.Ordinal)
            ? "\";buff_name;"
            : "\";buff_desc;";
        var sectionStart = line.LastIndexOf(sectionStartMarker, sectionEnd, StringComparison.Ordinal);
        if (sectionStart < 0)
            sectionStart = 0;

        var needle = "\"" + id + ";";
        var start = line.IndexOf(needle, sectionStart, StringComparison.Ordinal);
        if (start >= 0 && start < sectionEnd)
        {
            var end = line.IndexOf("\",", start, StringComparison.Ordinal);
            if (end >= 0 && end < sectionEnd)
                return line.Substring(0, start) + newRow + line.Substring(end + 2);
        }

        return line.Substring(0, sectionEnd) + newRow + line.Substring(sectionEnd);
    }

    static void PatchSchool()
    {
        // o_skillmenu Create does:
        //   name = ds_map_find_value(global.tier_name, text)
        //   scr_stringGetHeightExt(name, ...)
        // global.tier_name is built from table_text's Tier_name section — NOT table_items.
        // table_text is one giant `return [...]` line: InsertAbove puts code outside the
        // array; MatchFrom+ReplaceBy truncates the rest of the line. Splice via Apply.
        Msl.LoadGML("gml_GlobalScript_table_text")
            .Apply(InjectNecromancyTableTextRows)
            .Save();
    }

    /// <summary>
    /// CSV columns: id;RU;EN;ZH;DE;ES;FR;IT;PT;PL;TR;JA;KO
    /// </summary>
    static IEnumerable<string> InjectNecromancyTableTextRows(IEnumerable<string> lines)
    {
        const string tierMarker = "\";Tier_name_end;";
        const string hoverMarker = "\";skilltree_hover_end;";
        const string tierRow =
            "\"Necromancy;Некромантия;Necromancy;死灵术;Nekromantie;Nigromancia;Nécromancie;Necromanzia;Necromancia;Nekromancja;Ölüm Büyüleri;ネクロマンシー;강령술;\", ";

        const string hoverEn =
            "Challenge magical bounds and master the chaotic dance between life and death.##~y~Main focus:~/~#~w~Support~/~, ~w~Minion Management~/~, ~w~Survivability~/~, ~w~Weakening Effects~/~";
        const string hoverRu =
            "Бросьте вызов магическим ограничениям и познайте тайны на грани жизни и смерти.##~y~Особенности:~/~#~w~Поддержка~/~, ~w~Призыв слуг~/~, ~w~Выживаемость~/~, ~w~Ослабление врагов~/~";
        const string hoverZh =
            "挑战魔法界限，掌握生与死之间的混沌舞步。##~y~主要方向:~/~#~w~支援~/~, ~w~召唤物管理~/~, ~w~生存~/~, ~w~弱化效果~/~";
        const string hoverDe =
            "Fordert magische Grenzen heraus und meistert den Tanz zwischen Leben und Tod.##~y~Fokus:~/~#~w~Unterstützung~/~, ~w~Dienerverwaltung~/~, ~w~Überleben~/~, ~w~Schwächungseffekte~/~";
        const string hoverJa =
            "魔法の境界に挑み、生と死の混沌を操る。##~y~主な特徴:~/~#~w~支援~/~, ~w~手下管理~/~, ~w~生存~/~, ~w~弱体化~/~";

        var hoverRow =
            "\"Necromancy;" + hoverRu + ";" + hoverEn + ";" + hoverZh + ";" + hoverDe + ";" +
            hoverEn + ";" + hoverEn + ";" + hoverEn + ";" + hoverEn + ";" + hoverEn + ";" + hoverEn + ";" +
            hoverJa + ";" + hoverEn + ";\", ";

        foreach (var line in lines)
        {
            var result = line;
            if (result.Contains(tierMarker, StringComparison.Ordinal) &&
                !result.Contains("\"Necromancy;Некромантия;", StringComparison.Ordinal))
            {
                result = result.Replace(tierMarker, tierRow + tierMarker, StringComparison.Ordinal);
            }

            if (result.Contains(hoverMarker, StringComparison.Ordinal) &&
                !result.Contains("\"Necromancy;" + hoverRu, StringComparison.Ordinal))
            {
                result = result.Replace(hoverMarker, hoverRow + hoverMarker, StringComparison.Ordinal);
            }

            yield return result;
        }
    }

    static void PatchSkills()
    {
        PendingSkillLocs.Clear();

        RegisterSkillLocalization(
            "desecration",
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Dark Bolt" },
                { ModLanguage.Japanese, "ダークボルト" },
                { ModLanguage.Chinese, "暗矢" },
                { ModLanguage.German, "Dunkler Bolzen" },
                { ModLanguage.Russian, "Тёмный заряд" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Dark Bolt" },
                { ModLanguage.Japanese, "ダークボルト" },
                { ModLanguage.Chinese, "暗矢" },
                { ModLanguage.German, "Dunkler Bolzen" },
                { ModLanguage.Russian, "Тёмный заряд" },
            },
            new Dictionary<ModLanguage, string>
            {
                { ModLanguage.English, "Shoots a bolt of dark energy, dealing ~ur~8 Unholy Damage~/~ with Accuracy.##If the spell hits the target, burns its Energy for ~lg~33%~/~ of the damage dealt and replenishes the same amount of Energy to the caster.##Grants Unholy Damage for ~w~8 turns~/~.##This effect doesn't stack." },
                { ModLanguage.Japanese, "暗黒エネルギーのボルトを放ち、命中時に~ur~8の不浄ダメージ~/~を与える。##命中すると、与ダメージの~lg~33%~/~分のエネルギーを焼き、同量を術者に回復する。##~w~8ターン~/~の間、不浄ダメージを付与する。##この効果は重複しない。" },
                { ModLanguage.Chinese, "射出一道黑暗能量箭，造成~ur~8点邪秽伤害~/~（需命中）。##若命中目标，按其受到伤害的~lg~33%~/~燃烧能量，并将等量能量回复给施法者。##赋予邪秽伤害，持续~w~8回合~/~。##此效果不叠加。" },
                { ModLanguage.German, "Schießt einen Blitz dunkler Energie, der ~ur~8 Unheiligen Schaden~/~ mit Treffergenauigkeit verursacht.##Bei einem Treffer verbrennt der Zauber ~lg~33%~/~ des verursachten Schadens als Energie und stellt dem Wirker denselben Betrag wieder her.##Gewährt Unheiligen Schaden für ~w~8 Züge~/~.##Dieser Effekt stapelt nicht." },
                { ModLanguage.Russian, "Выпускает заряд тёмной энергии, наносящий ~ur~8 урона нечестием~/~ с учётом точности.##При попадании сжигает энергию цели в размере ~lg~33%~/~ от нанесённого урона и восстанавливает столько же энергии заклинателю.##Даёт урон нечестием на ~w~8 ходов~/~.##Эффект не стакается." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Activates ~lg~\"Bone Ward\"~/~ for ~w~8~/~ turns (~lg~+1~/~ turn if a corpse is adjacent):##~lg~+12%~/~ Physical Resistance#~lg~+15%~/~ Bleed Resistance#~lg~+10%~/~ Magic Resistance#~lg~+10%~/~ Move Resistance##When the effect expires or is activated again, bone shards deal ~ur~4 Unholy~/~ and ~w~3 Piercing Damage~/~ to all adjacent enemies, with ~w~50%~/~ chance to Stagger them." },
                { ModLanguage.Japanese, "~lg~「骨の守護」~/~を~w~8~/~ターン発動する（隣接に死体がある場合~lg~+1~/~ターン）：##~lg~+12%~/~物理耐性#~lg~+15%~/~出血耐性#~lg~+10%~/~魔法耐性#~lg~+10%~/~移動耐性##効果終了時または再発動時、骨片が隣接する全敵に~ur~不浄ダメージ4~/~と刺突ダメージ3を与え、~w~50%~/~の確率でよろけさせる。" },
                { ModLanguage.Chinese, "激活~lg~“骸骨结界”~/~，持续~w~8~/~回合（相邻有尸体时~lg~+1~/~回合）：##~lg~+12%~/~物理抗性#~lg~+15%~/~出血抗性#~lg~+10%~/~魔法抗性#~lg~+10%~/~移动抗性##效果结束或再次激活时，骨片对所有相邻敌人造成~ur~4点邪秽伤害~/~与~w~3点穿刺伤害~/~，并有~w~50%~/~几率使其踉跄。" },
                { ModLanguage.German, "Aktiviert ~lg~„Knochenwächter“~/~ für ~w~8~/~ Züge (~lg~+1~/~ Zug, wenn eine Leiche angrenzt):##~lg~+12%~/~ physischer Widerstand#~lg~+15%~/~ Blutungswiderstand#~lg~+10%~/~ Magiewiderstand#~lg~+10%~/~ Bewegungswiderstand##Beim Auslaufen oder erneuten Aktivieren fügen Knochensplitter allen angrenzenden Feinden ~ur~4 Unheiligen~/~ und ~w~3 Stichschaden~/~ zu, mit ~w~50%~/~ Chance zu staunen." },
                { ModLanguage.Russian, "Активирует ~lg~«Костяной щит»~/~ на ~w~8~/~ ходов (~lg~+1~/~ ход, если рядом труп):##~lg~+12%~/~ физ. сопротивления#~lg~+15%~/~ сопротивления кровотечению#~lg~+10%~/~ маг. сопротивления#~lg~+10%~/~ сопротивления перемещению##По окончании или при повторной активации осколки костей наносят всем соседним врагам ~ur~4 урона нечестием~/~ и ~w~3 колющего урона~/~ с ~w~50%~/~ шансом ошеломить." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Summon skeleton on the targeted tile (no corpse required).##Duration, Max Health %, and Max Energy % scale with Willpower (starts at ~w~20~/~ turns, ~lg~33%~/~ Max Health / ~lg~25%~/~ Max Energy, Armor Durability ~w~0%~/~).##Only one Skeleton can exist at a time." },
                { ModLanguage.Japanese, "対象タイルにスケルトンを召喚する（死体不要）。##持続時間・最大体力%・最大エネルギー%は意志力に比例する（初期: ~w~20~/~ターン、最大体力~lg~33%~/~ / 最大エネルギー~lg~25%~/~、防具耐久~w~0%~/~）。##同時に存在できるスケルトンは1体のみ。" },
                { ModLanguage.Chinese, "在目标格子召唤骷髅（无需尸体）。##持续时间、最大生命%与最大能量%随意志力提升（起始：~w~20~/~回合，~lg~33%~/~最大生命 / ~lg~25%~/~最大能量，护甲耐久~w~0%~/~）。##同时仅可存在一具骷髅。" },
                { ModLanguage.German, "Beschwört ein Skelett auf dem Ziel-Feld (keine Leiche nötig).##Dauer, max. Gesundheit % und max. Energie % skalieren mit Willenskraft (Start: ~w~20~/~ Züge, ~lg~33%~/~ max. Gesundheit / ~lg~25%~/~ max. Energie, Rüstungshalbarkeit ~w~0%~/~).##Nur ein Skelett gleichzeitig." },
                { ModLanguage.Russian, "Призывает скелета на выбранную клетку (труп не нужен).##Длительность, % макс. здоровья и % макс. энергии зависят от Воли (старт: ~w~20~/~ ходов, ~lg~33%~/~ макс. здоровья / ~lg~25%~/~ макс. энергии, прочность брони ~w~0%~/~).##Одновременно может существовать только один скелет." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Applies the target with ~r~\"Withering Curse\"~/~ for ~w~10~/~ turns (Range: ~w~6~/~):##~r~-8%~/~ Fortitude#~r~-8%~/~ Magic Resistance#~r~-5%~/~ Max Health#~lg~+3%~/~ Damage Taken##Each turn worsens a random body part by ~r~0.25%~/~.##Hitting with ~lg~\"Dark Bolt\"~/~ or repeating the spell on the same target refreshes the duration (does not stack at Tier 1)." },
                { ModLanguage.Japanese, "対象に~r~「枯死の呪い」~/~を~w~10~/~ターン付与する（射程: ~w~6~/~）：##~r~-8%~/~頑健#~r~-8%~/~魔法耐性#~r~-5%~/~最大体力#~lg~+3%~/~被ダメージ##毎ターン、ランダムな部位の状態を~r~0.25%~/~悪化させる。##~lg~「ダークボルト」~/~の命中または同一対象への再詠唱で持続を更新する（ティア1では重複しない）。" },
                { ModLanguage.Chinese, "对目标施加~r~“凋零诅咒”~/~，持续~w~10~/~回合（射程：~w~6~/~）：##~r~-8%~/~坚韧#~r~-8%~/~魔法抗性#~r~-5%~/~最大生命#~lg~+3%~/~受到伤害##每回合使随机部位状态恶化~r~0.25%~/~。##以~lg~“暗矢”~/~命中或对同一目标重复施放会刷新持续时间（第1阶不叠加）。" },
                { ModLanguage.German, "Belegt das Ziel mit ~r~„Verdorrender Fluch“~/~ für ~w~10~/~ Züge (Reichweite: ~w~6~/~):##~r~-8%~/~ Zähigkeit#~r~-8%~/~ Magiewiderstand#~r~-5%~/~ max. Gesundheit#~lg~+3%~/~ erlittener Schaden##Jeder Zug verschlechtert einen zufälligen Körperteil um ~r~0,25%~/~.##Treffer mit ~lg~„Dunkler Bolzen“~/~ oder erneutes Wirken auf dasselbe Ziel erneuert die Dauer (stapelt nicht auf Stufe 1)." },
                { ModLanguage.Russian, "Накладывает на цель ~r~«Иссушающее проклятие»~/~ на ~w~10~/~ ходов (дальность: ~w~6~/~):##~r~-8%~/~ стойкости#~r~-8%~/~ маг. сопротивления#~r~-5%~/~ макс. здоровья#~lg~+3%~/~ получаемого урона##Каждый ход ухудшает случайную часть тела на ~r~0,25%~/~.##Попадание ~lg~«Тёмным зарядом»~/~ или повторное применение обновляет длительность (на 1-м ярусе не стакается)." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Devours a targeted corpse or steals ~lg~8%~/~ Max Health and ~lg~8%~/~ Max Energy from an allied Undead or Skeleton, transferring half of them to the caster.##Activates ~lg~\"Essence Charge\"~/~ for ~w~12~/~ turns:##~lg~+5%~/~ Magic Power#~r~-5%~/~ Abilities Energy Cost#~lg~+4~/~ Max Energy##Repeated use grants an extra stack (up to II) and refreshes duration.##Using Spells reduces stacks by ~r~1~/~." },
                { ModLanguage.Japanese, "対象の死体を喰らうか、味方のアンデッド/スケルトンから最大体力~lg~8%~/~と最大エネルギー~lg~8%~/~を奪い、半分を術者へ移す。##~lg~「本質充填」~/~を~w~12~/~ターン発動：##~lg~+5%~/~魔力#~r~-5%~/~能力のエネルギー消費#~lg~+4~/~最大エネルギー##再使用で追加スタック（最大II）と持続更新。##呪文使用でスタックが~r~1~/~減少。" },
                { ModLanguage.Chinese, "吞噬目标尸体，或从友方亡灵/骷髅窃取~lg~8%~/~最大生命与~lg~8%~/~最大能量，并将一半转移给施法者。##激活~lg~“精华充能”~/~，持续~w~12~/~回合：##~lg~+5%~/~魔法强度#~r~-5%~/~技能能量消耗#~lg~+4~/~最大能量##重复使用额外叠加（最高II）并刷新持续时间。##施放法术会使层数减~r~1~/~。" },
                { ModLanguage.German, "Verschlingt eine gezielte Leiche oder stiehlt ~lg~8%~/~ max. Gesundheit und ~lg~8%~/~ max. Energie von einem verbündeten Untoten oder Skelett und überträgt die Hälfte auf den Wirker.##Aktiviert ~lg~„Essenzladung“~/~ für ~w~12~/~ Züge:##~lg~+5%~/~ Magiekraft#~r~-5%~/~ Energiekosten von Fähigkeiten#~lg~+4~/~ max. Energie##Wiederholte Nutzung gibt einen Extra-Stapel (bis II) und erneuert die Dauer.##Zauber verringern Stapel um ~r~1~/~." },
                { ModLanguage.Russian, "Пожирает выбранный труп или крадёт ~lg~8%~/~ макс. здоровья и ~lg~8%~/~ макс. энергии у союзной нежити/скелета, передавая половину заклинателю.##Активирует ~lg~«Заряд сущности»~/~ на ~w~12~/~ ходов:##~lg~+5%~/~ силы магии#~r~-5%~/~ стоимости энергии способностей#~lg~+4~/~ макс. энергии##Повторное применение даёт доп. стак (до II) и обновляет длительность.##Заклинания уменьшают стаки на ~r~1~/~." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Replenishes Health to the target (based on Magic Power) and improves body part condition by ~lg~25%~/~ of health restored.##~lg~50%~/~ more effective on allied Undead.##Grants ~lg~\"Death's Blessing\"~/~ for ~w~8~/~ turns:##~lg~+6%~/~ Life Drain#~lg~+6%~/~ Energy Drain#~lg~+8%~/~ Damage Reflection." },
                { ModLanguage.Japanese, "対象の体力を回復し（魔力依存）、回復量の~lg~25%~/~分だけ部位状態を改善する。##味方アンデッドには~lg~50%~/~効果上昇。##~lg~「死の祝福」~/~を~w~8~/~ターン付与：##~lg~+6%~/~生命吸収#~lg~+6%~/~エネルギー吸収#~lg~+8%~/~ダメージ反射。" },
                { ModLanguage.Chinese, "恢复目标生命（基于魔法强度），并按恢复量的~lg~25%~/~改善部位状态。##对友方亡灵效果提升~lg~50%~/~。##赋予~lg~“死亡赐福”~/~，持续~w~8~/~回合：##~lg~+6%~/~生命汲取#~lg~+6%~/~能量汲取#~lg~+8%~/~伤害反射。" },
                { ModLanguage.German, "Stellt Gesundheit des Ziels wieder her (basiert auf Magiekraft) und verbessert den Zustand der Körperteile um ~lg~25%~/~ der geheilten Menge.##~lg~50%~/~ wirksamer bei verbündeten Untoten.##Gewährt ~lg~„Segen des Todes“~/~ für ~w~8~/~ Züge:##~lg~+6%~/~ Lebensraub#~lg~+6%~/~ Energieraub#~lg~+8%~/~ Schadensreflexion." },
                { ModLanguage.Russian, "Восстанавливает здоровье цели (зависит от силы магии) и улучшает состояние частей тела на ~lg~25%~/~ от восстановленного здоровья.##На ~lg~50%~/~ эффективнее на союзной нежити.##Даёт ~lg~«Благословение смерти»~/~ на ~w~8~/~ ходов:##~lg~+6%~/~ вампиризма#~lg~+6%~/~ кражи энергии#~lg~+8%~/~ отражения урона." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Reanimates a targeted corpse as an Undead for ~w~20~/~ turns at ~lg~50%~/~ Max Health / ~lg~50%~/~ Max Energy and sets Armor Durability to ~w~0%~/~.##The animated corpse has a chance to lose control.##Increases morale (up to ~lg~70%~/~).##Slowly decreases Sanity but no less than ~r~60%~/~.##Can't summon Undead higher than the skill's level cap.##Prevents receiving ~ly~\"Blessing\"~/~." },
                { ModLanguage.Japanese, "対象の死体を~w~20~/~ターンのアンデッドとして蘇生する（最大体力~lg~50%~/~ / 最大エネルギー~lg~50%~/~、防具耐久~w~0%~/~）。##蘇生した死体は制御を失うことがある。##士気を上昇（最大~lg~70%~/~）。##正気を徐々に減少させるが~r~60%~/~未満にはならない。##スキルのレベル上限を超えるアンデッドは召喚不可。##「祝福」を受けられない。" },
                { ModLanguage.Chinese, "将目标尸体复活为亡灵，持续~w~20~/~回合（~lg~50%~/~最大生命 / ~lg~50%~/~最大能量，护甲耐久设为~w~0%~/~）。##复活的尸体有几率失控。##提升士气（最高~lg~70%~/~）。##缓慢降低理智但不低于~r~60%~/~。##无法召唤高于技能等级上限的亡灵。##无法获得“祝福”。" },
                { ModLanguage.German, "Belebt eine gezielte Leiche als Untoten für ~w~20~/~ Züge mit ~lg~50%~/~ max. Gesundheit / ~lg~50%~/~ max. Energie und setzt die Rüstungshalbarkeit auf ~w~0%~/~.##Die belebte Leiche kann die Kontrolle verlieren.##Erhöht die Moral (bis ~lg~70%~/~).##Senkt langsam den Verstand, aber nicht unter ~r~60%~/~.##Keine Untoten über dem Level-Limit.##Verhindert „Segen“." },
                { ModLanguage.Russian, "Оживляет выбранный труп как нежить на ~w~20~/~ ходов при ~lg~50%~/~ макс. здоровья / ~lg~50%~/~ макс. энергии и ставит прочность брони на ~w~0%~/~.##Оживлённый труп может выйти из-под контроля.##Повышает мораль (до ~lg~70%~/~).##Медленно снижает рассудок, но не ниже ~r~60%~/~.##Нельзя призвать нежить выше уровня навыка.##Запрещает получение «Благословения»." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Creates a blighted 3x3 area on the targeted tile (Range: ~w~6~/~) for ~w~4~/~ turns.##Enemies remaining in the area take ~ur~2 Unholy Damage~/~ per turn and gain ~r~-4%~/~ Magic Resistance (stacks up to ~w~3~/~ while inside).##Also allows ~r~\"Withering Curse\"~/~ to stack up to II. Hitting with ~lg~\"Dark Bolt\"~/~ or repeating the curse adds a stack.##Bodypart worsen becomes ~r~0.35%~/~ per turn per stack." },
                { ModLanguage.Japanese, "対象タイルに3x3の疫病地帯を~w~4~/~ターン生成する（射程: ~w~6~/~）。##領域内の敵は毎ターン~ur~不浄ダメージ2~/~を受け、魔法耐性~r~-4%~/~を得る（内部で最大~w~3~/~スタック）。##~r~「枯死の呪い」~/~を最大IIまで重複可能にし、~lg~「ダークボルト」~/~の命中または呪いの再詠唱でスタック追加。##部位悪化はスタックごとに毎ターン~r~0.35%~/~。" },
                { ModLanguage.Chinese, "在目标格子创造3x3疫病区域（射程：~w~6~/~），持续~w~4~/~回合。##留在区域内的敌人每回合受到~ur~2点邪秽伤害~/~，并获得~r~-4%~/~魔法抗性（在区域内最多叠加~w~3~/~层）。##同时使~r~“凋零诅咒”~/~可叠至II。以~lg~“暗矢”~/~命中或重复诅咒可增加层数。##部位恶化变为每层每回合~r~0.35%~/~。" },
                { ModLanguage.German, "Erzeugt ein verdorbenes 3x3-Gebiet auf dem Zielfeld (Reichweite: ~w~6~/~) für ~w~4~/~ Züge.##Feinde darin erleiden ~ur~2 Unheiligen Schaden~/~ pro Zug und erhalten ~r~-4%~/~ Magiewiderstand (stapelt bis ~w~3~/~, solange sie drin sind).##Erlaubt ~r~„Verdorrender Fluch“~/~ bis II zu stapeln. Treffer mit ~lg~„Dunkler Bolzen“~/~ oder erneuter Fluch fügt einen Stapel hinzu.##Körperteilverschlechterung: ~r~0,35%~/~ pro Zug und Stapel." },
                { ModLanguage.Russian, "Создаёт заражённую область 3x3 на выбранной клетке (дальность: ~w~6~/~) на ~w~4~/~ хода.##Враги в зоне получают ~ur~2 урона нечестием~/~ за ход и ~r~-4%~/~ маг. сопротивления (до ~w~3~/~ стаков, пока внутри).##Также позволяет ~r~«Иссушающему проклятию»~/~ стакаться до II. Попадание ~lg~«Тёмным зарядом»~/~ или повтор проклятия добавляет стак.##Ухудшение частей тела: ~r~0,35%~/~ за ход за стак." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Deals ~ur~12 Unholy Damage~/~ (Range: ~w~4~/~) with Immobilization Chance.##Replenishes Health to all allies within Vision for ~lg~50%~/~ of the damage dealt.##If the target has ~r~\"Withering Curse\"~/~: ~lg~+25%~/~ damage and adds ~w~1~/~ curse stack.##Consuming ~w~1~/~ ~lg~\"Essence Charge\"~/~ grants ~lg~+15%~/~ Magic Power for this cast only." },
                { ModLanguage.Japanese, "~ur~不浄ダメージ12~/~を与える（射程: ~w~4~/~、拘束確率あり）。##視界内の全味方の体力を、与ダメージの~lg~50%~/~分回復する。##対象が~r~「枯死の呪い」~/~を持つ場合：ダメージ~lg~+25%~/~、呪いスタック~lg~+1~/~。##~lg~「本質充填」~/~を~w~1~/~消費すると、この詠唱のみ魔力~lg~+15%~/~。" },
                { ModLanguage.Chinese, "造成~ur~12点邪秽伤害~/~（射程：~w~4~/~），附带定身几率。##按造成伤害的~lg~50%~/~恢复视野内所有盟友的生命。##若目标带有~r~“凋零诅咒”~/~：伤害~lg~+25%~/~，并增加~w~1~/~层诅咒。##消耗~w~1~/~层~lg~“精华充能”~/~可在本次施法中获得~lg~+15%~/~魔法强度。" },
                { ModLanguage.German, "Verursacht ~ur~12 Unheiligen Schaden~/~ (Reichweite: ~w~4~/~) mit Immobilisierungschance.##Stellt allen Verbündeten in Sichtweite ~lg~50%~/~ des verursachten Schadens als Gesundheit wieder her.##Hat das Ziel ~r~„Verdorrender Fluch“~/~: ~lg~+25%~/~ Schaden und ~lg~+1~/~ Fluchstapel.##Verbrauch von ~w~1~/~ ~lg~„Essenzladung“~/~ gewährt ~lg~+15%~/~ Magiekraft nur für diesen Wurf." },
                { ModLanguage.Russian, "Наносит ~ur~12 урона нечестием~/~ (дальность: ~w~4~/~) со шансом обездвижить.##Восстанавливает здоровье всем союзникам в поле зрения на ~lg~50%~/~ от нанесённого урона.##Если на цели ~r~«Иссушающее проклятие»~/~: ~lg~+25%~/~ урона и ~lg~+1~/~ стак проклятия.##Расход ~w~1~/~ ~lg~«Заряда сущности»~/~ даёт ~lg~+15%~/~ силы магии только для этого применения." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Targets a corpse or allied Undead/Skeleton and makes it explode, destroying it.##Deals Unholy and Piercing Damage in a base 3x3 area around the target and has a chance to apply Bleeding to affected enemies.##Damage, Bleeding chance, and area size scale with Willpower (area grows from 3x3 toward 5x5 / 7x7).##Exploding an ally causes a small Sanity loss." },
                { ModLanguage.Japanese, "死体または味方のアンデッド/スケルトンを爆発させ破壊する。##対象周囲の基本3x3に不浄と刺突ダメージを与え、被弾敵に出血を付与する確率がある。##ダメージ・出血確率・範囲は意志力に比例（範囲は3x3から5x5 / 7x7へ拡大）。##味方を爆発させると正気をわずかに失う。" },
                { ModLanguage.Chinese, "以尸体或友方亡灵/骷髅为目标使其爆炸并摧毁。##在目标周围基础3x3范围内造成邪秽与穿刺伤害，并有几率使受影响敌人出血。##伤害、出血几率与范围随意志力提升（范围从3x3扩大至5x5 / 7x7）。##炸毁盟友会造成少量理智损失。" },
                { ModLanguage.German, "Zielt auf eine Leiche oder einen verbündeten Untoten/Skelett und lässt sie explodieren (Zerstörung).##Verursacht Unheiligen und Stichschaden in einem Basisgebiet 3x3 und kann Blutung verursachen.##Schaden, Blutungschance und Gebietsgröße skalieren mit Willenskraft (Gebiet wächst von 3x3 zu 5x5 / 7x7).##Explosion eines Verbündeten verursacht kleinen Verstandverlust." },
                { ModLanguage.Russian, "Целится в труп или союзную нежить/скелета и взрывает их, уничтожая.##Наносит урон нечестием и колющий урон в базовой области 3x3 и может наложить кровотечение.##Урон, шанс кровотечения и размер области зависят от Воли (область растёт с 3x3 до 5x5 / 7x7).##Взрыв союзника вызывает небольшую потерю рассудка." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Unbinding allies doesn't take a turn and replenishes Energy equal to ~lg~10%~/~ of your Magic Power.##Increases the level of summoning abilities by ~lg~1~/~.##Summon Skeleton: ~lg~+4~/~ turns base duration and ~lg~+10%~/~ Max Health / Max Energy.##Resurrection thralls start at ~lg~60%~/~ Max Health.##Still limited to one safe Undead at a time unless a higher-tier skill allows more." },
                { ModLanguage.Japanese, "味方の束縛解除はターンを消費せず、魔力の~lg~10%~/~相当のエネルギーを回復する。##召喚系能力のレベル~lg~+1~/~。##スケルトン召喚：基本持続~lg~+4~/~ターン、最大体力/最大エネルギー~lg~+10%~/~。##復活の下僕は最大体力~lg~60%~/~で開始。##上位スキルがない限り、安全なアンデッドは同時に1体まで。" },
                { ModLanguage.Chinese, "解除盟友束缚不消耗回合，并回复相当于魔法强度~lg~10%~/~的能量。##召唤类技能等级~lg~+1~/~。##召唤骷髅：基础持续时间~lg~+4~/~回合，最大生命/最大能量~lg~+10%~/~。##复活仆从以~lg~60%~/~最大生命起步。##除非更高阶技能允许，否则同时仅可安全控制一个亡灵。" },
                { ModLanguage.German, "Lösen von Verbündeten kostet keinen Zug und stellt Energie in Höhe von ~lg~10%~/~ eurer Magiekraft wieder her.##Erhöht das Level von Beschwörungsfähigkeiten um ~lg~1~/~.##Skelett beschwören: ~lg~+4~/~ Züge Basisdauer und ~lg~+10%~/~ max. Gesundheit / max. Energie.##Auferstehungsdiener starten mit ~lg~60%~/~ max. Gesundheit.##Weiterhin auf einen sicheren Untoten gleichzeitig begrenzt, sofern keine höhere Fähigkeit mehr erlaubt." },
                { ModLanguage.Russian, "Освобождение союзников не тратит ход и восстанавливает энергию, равную ~lg~10%~/~ вашей силы магии.##Повышает уровень способностей призыва на ~lg~1~/~.##Призыв скелета: ~lg~+4~/~ хода базовой длительности и ~lg~+10%~/~ макс. здоровья / макс. энергии.##Слуги воскрешения начинают с ~lg~60%~/~ макс. здоровья.##По-прежнему не более одной безопасной нежити одновременно, если более высокий навык не позволяет больше." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Detonates ~r~\"Withering Curse\"~/~ on the target (Range: ~w~5~/~), or on the strongest cursed enemy in a 3x3 if cast on a tile: removes stacks, deals ~ur~6 Unholy Damage~/~ × stacks to the target and ~ur~3 Unholy Damage~/~ × stacks to adjacent enemies.##Allows ~r~\"Withering Curse\"~/~ to stack up to III.##Bodypart worsen becomes ~r~0.5%~/~ per turn per stack while at III." },
                { ModLanguage.Japanese, "対象の~r~「枯死の呪い」~/~を起爆する（射程: ~w~5~/~）。##タイル詠唱時は3x3内で最も強い呪い持ちを対象：スタックを除去し、対象に~ur~不浄ダメージ6~/~×スタック、隣接敵に~ur~3~/~×スタック。##呪いを最大IIIまで重複可能。##III時の部位悪化はスタックごとに毎ターン~r~0.5%~/~。" },
                { ModLanguage.Chinese, "引爆目标身上的~r~“凋零诅咒”~/~（射程：~w~5~/~）；若对格子施放，则引爆3x3内最强的被诅咒敌人：移除层数，对目标造成~ur~6×层数邪秽伤害~/~，对相邻敌人造成~ur~3×层数邪秽伤害~/~。##使~r~“凋零诅咒”~/~可叠至III。##达到III时，部位恶化变为每层每回合~r~0.5%~/~。" },
                { ModLanguage.German, "Zündet ~r~„Verdorrender Fluch“~/~ am Ziel (Reichweite: ~w~5~/~) oder am stärksten verfluchten Feind in 3x3 bei Feldwirken: entfernt Stapel, verursacht ~ur~6 Unheiligen Schaden~/~ × Stapel am Ziel und ~ur~3~/~ × Stapel an angrenzenden Feinden.##Erlaubt Stapelung bis III.##Bei III: Körperteilverschlechterung ~r~0,5%~/~ pro Zug und Stapel." },
                { ModLanguage.Russian, "Детонирует ~r~«Иссушающее проклятие»~/~ на цели (дальность: ~w~5~/~) или на сильнейшем проклятом враге в 3x3 при применении на клетку: снимает стаки, наносит цели ~ur~6 урона нечестием~/~ × стаки и соседним врагам ~ur~3~/~ × стаки.##Позволяет стакать проклятие до III.##При III ухудшение частей тела: ~r~0,5%~/~ за ход за стак." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Halves sanity loss of Occultism abilities.##Grants ~lg~33%~/~ Unholy Resistance." },
                { ModLanguage.Japanese, "神秘学能力の正気減少を半減する。##不浄耐性~lg~33%~/~を付与する。" },
                { ModLanguage.Chinese, "使神秘学技能的理智损失减半。##赋予~lg~33%~/~邪秽抗性。" },
                { ModLanguage.German, "Halbiert den Verstandverlust von Okkultismus-Fähigkeiten.##Gewährt ~lg~33%~/~ Unheiligen Widerstand." },
                { ModLanguage.Russian, "Вдвое снижает потерю рассудка от способностей оккультизма.##Даёт ~lg~33%~/~ сопротивления нечестию." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Prevents dying from lethal hits as long as Max Health and Morale are both higher than ~lg~50%~/~.##When Health drops below ~w~2~/~, replenishes ~lg~33%~/~ of Max Health and Max Energy, and applies:##~r~-10%~/~ Morale#~lg~+2%~/~ Fatigue##Prevents death from starvation or dehydration." },
                { ModLanguage.Japanese, "最大体力と士気がともに~lg~50%~/~超なら致死ヒットで死亡しない。##体力が~w~2~/~未満になると、最大体力と最大エネルギーの~lg~33%~/~を回復し：##~r~-10%~/~士気#~lg~+2%~/~疲労##餓死・渇死を防ぐ。" },
                { ModLanguage.Chinese, "只要最大生命与士气均高于~lg~50%~/~，就不会因致命一击死亡。##当生命降至~w~2~/~以下时，回复~lg~33%~/~最大生命与最大能量，并施加：##~r~-10%~/~士气#~lg~+2%~/~疲劳##防止因饥饿或脱水而死。" },
                { ModLanguage.German, "Verhindert den Tod durch tödliche Treffer, solange max. Gesundheit und Moral beide über ~lg~50%~/~ liegen.##Sinkt die Gesundheit unter ~w~2~/~, werden ~lg~33%~/~ von max. Gesundheit und max. Energie wiederhergestellt und angewendet:##~r~-10%~/~ Moral#~lg~+2%~/~ Erschöpfung##Verhindert Tod durch Hunger oder Durst." },
                { ModLanguage.Russian, "Предотвращает смерть от смертельных ударов, пока макс. здоровье и мораль выше ~lg~50%~/~.##Когда здоровье падает ниже ~w~2~/~, восстанавливает ~lg~33%~/~ макс. здоровья и макс. энергии и накладывает:##~r~-10%~/~ морали#~lg~+2%~/~ усталости##Предотвращает смерть от голода или жажды." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Successfully raising the dead allows you to learn one of their active abilities at a time (while available via skill menu; lost when the thrall dies).##Increases Experience Gain of Undead kills by ~lg~100%~/~." },
                { ModLanguage.Japanese, "死者の蘇生に成功すると、その主動能力を1つ習得できる（スキルメニューで利用可能中。下僕が死ぬと失う）。##アンデッド撃破の経験値獲得~lg~+100%~/~。" },
                { ModLanguage.Chinese, "成功复活死者后，可同时学习其一项主动技能（在技能菜单可用期间；仆从死亡后失去）。##亡灵击杀的经验获取~lg~+100%~/~。" },
                { ModLanguage.German, "Erfolgreiches Auferwecken der Toten erlaubt das Erlernen einer ihrer aktiven Fähigkeiten gleichzeitig (solange im Menü verfügbar; verloren, wenn der Diener stirbt).##Erhöht Erfahrungsgewinn durch Untoten-Kills um ~lg~100%~/~." },
                { ModLanguage.Russian, "Успешное воскрешение мёртвых позволяет изучить одну их активную способность за раз (пока доступна в меню навыков; теряется при смерти слуги).##Увеличивает получаемый опыт за убийства нежити на ~lg~100%~/~." },
            });
        RegisterSkillLocalization(
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
                { ModLanguage.English, "Summons a Wraith on the targeted tile; power depends on ~lg~\"Essence Charge\"~/~ stacks.##Removes ~lg~\"Essence Charge\"~/~.##If no weapon equipped: ~r~-50%~/~ Cooldown Duration and Halves Miscast Chance.##Can't summon Wraiths higher than the skill's level cap." },
                { ModLanguage.Japanese, "対象タイルにレイスを召喚する。強さは~lg~「本質充填」~/~のスタックに依存。##~lg~「本質充填」~/~を除去。##武器未装備時：クールダウン~r~-50%~/~、詠唱失敗率半減。##スキルのレベル上限を超えるレイスは召喚不可。" },
                { ModLanguage.Chinese, "在目标格子召唤怨灵；强度取决于~lg~“精华充能”~/~层数。##移除~lg~“精华充能”~/~。##未装备武器时：冷却时间~r~-50%~/~，施法失败几率减半。##无法召唤高于技能等级上限的怨灵。" },
                { ModLanguage.German, "Beschwört einen Geist auf dem Zielfeld; Stärke hängt von ~lg~„Essenzladung“~/~-Stapeln ab.##Entfernt ~lg~„Essenzladung“~/~.##Ohne Waffe: ~r~-50%~/~ Abklingzeit und halbe Fehlschlagchance.##Keine Geister über dem Level-Limit." },
                { ModLanguage.Russian, "Призывает умертвие на выбранную клетку; сила зависит от стаков ~lg~«Заряда сущности»~/~.##Снимает ~lg~«Заряд сущности»~/~.##Без оружия: ~r~-50%~/~ длительности перезарядки и вдвое меньший шанс сбоя.##Нельзя призвать умертвия выше уровня навыка." },
            });
        // Skill tooltips resolve via global.skill_name_text / global.skill_desc from
        // table_skills (scr_skill_atr). InjectTableItemLocalization only feeds table_items.
        Msl.LoadGML("gml_GlobalScript_table_skills")
            .Apply(InjectNecromancyTableSkillsRows)
            .Save();
    }

    static readonly List<(string Id, Dictionary<ModLanguage, string> Name, Dictionary<ModLanguage, string> Desc)> PendingSkillLocs = new();

    static void RegisterSkillLocalization(
        string id,
        Dictionary<ModLanguage, string> name,
        Dictionary<ModLanguage, string> _unused,
        Dictionary<ModLanguage, string> description)
    {
        PendingSkillLocs.Add((id, name, description));
    }

    // table_skills / table_effects rows are semicolon-delimited (id;RU;EN;...).
    // A literal ';' inside a translation shifts every language column — English then
    // reads leftover Russian. Strip/replace before emit.
    static string EscapeSkillCsv(string value) =>
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

    static string SkillCsvRow(string id, Dictionary<ModLanguage, string> texts)
    {
        // CSV columns: id;RU;EN;ZH;DE;ES;FR;IT;PT;PL;TR;JA;KO
        var en = EscapeSkillCsv(LocPick(texts, ModLanguage.English));
        var ru = EscapeSkillCsv(LocPick(texts, ModLanguage.Russian));
        var zh = EscapeSkillCsv(LocPick(texts, ModLanguage.Chinese));
        var de = EscapeSkillCsv(LocPick(texts, ModLanguage.German));
        var ja = EscapeSkillCsv(LocPick(texts, ModLanguage.Japanese));
        return "\"" + id + ";" + ru + ";" + en + ";" + zh + ";" + de + ";" +
               en + ";" + en + ";" + en + ";" + en + ";" + en + ";" + en + ";" +
               ja + ";" + en + ";\", ";
    }

    static IEnumerable<string> InjectNecromancyTableSkillsRows(IEnumerable<string> lines)
    {
        const string nameMarker = "\";skill_name_end;";
        const string descMarker = "\";skill_desc_end;";
        // First-patch insert uses desecration as the guard that names were already added.
        const string nameGuard = "\"desecration;";

        var nameInsert = new System.Text.StringBuilder();
        var descInsert = new System.Text.StringBuilder();
        foreach (var entry in PendingSkillLocs)
        {
            nameInsert.Append(SkillCsvRow(entry.Id, entry.Name));
            descInsert.Append(SkillCsvRow(entry.Id, entry.Desc));
        }

        var nameText = nameInsert.ToString();
        var descText = descInsert.ToString();
        var descGuard = "\"desecration;" + EscapeSkillCsv(LocPick(PendingSkillLocs[0].Desc, ModLanguage.Russian));

        foreach (var line in lines)
        {
            var result = line;
            if (result.Contains(nameMarker, StringComparison.Ordinal))
            {
                if (result.Contains(nameGuard, StringComparison.Ordinal))
                {
                    // Re-patch: upsert every skill so newly added skills (e.g. grave_blight)
                    // get EN/RU columns instead of falling back to a wrong language.
                    foreach (var entry in PendingSkillLocs)
                        result = UpsertSkillsCsvRow(result, entry.Id, entry.Name, nameMarker);
                }
                else
                {
                    result = result.Replace(nameMarker, nameText + nameMarker, StringComparison.Ordinal);
                }
            }

            if (result.Contains(descMarker, StringComparison.Ordinal))
            {
                if (result.Contains(descGuard, StringComparison.Ordinal) ||
                    result.Contains(nameGuard, StringComparison.Ordinal))
                {
                    foreach (var entry in PendingSkillLocs)
                        result = UpsertSkillsCsvRow(result, entry.Id, entry.Desc, descMarker);
                }
                else
                {
                    result = result.Replace(descMarker, descText + descMarker, StringComparison.Ordinal);
                }
            }

            yield return result;
        }
    }

    /// <summary>
    /// Upsert one skill CSV row inside the matching name/desc section of table_skills.
    /// Columns: id;RU;EN;ZH;DE;ES;FR;IT;PT;PL;TR;JA;KO — EN must stay in the EN slot.
    /// </summary>
    static string UpsertSkillsCsvRow(
        string line,
        string id,
        Dictionary<ModLanguage, string> texts,
        string sectionEndMarker)
    {
        var newRow = SkillCsvRow(id, texts);
        var sectionEnd = line.IndexOf(sectionEndMarker, StringComparison.Ordinal);
        if (sectionEnd < 0)
            return line;

        var sectionStartMarker = sectionEndMarker.Contains("skill_name", StringComparison.Ordinal)
            ? "\";skill_name;"
            : "\";skill_desc;";
        var sectionStart = line.LastIndexOf(sectionStartMarker, sectionEnd, StringComparison.Ordinal);
        if (sectionStart < 0)
            sectionStart = 0;

        var needle = "\"" + id + ";";
        var start = line.IndexOf(needle, sectionStart, StringComparison.Ordinal);
        if (start >= 0 && start < sectionEnd)
        {
            var end = line.IndexOf("\",", start, StringComparison.Ordinal);
            if (end >= 0 && end < sectionEnd)
                return line.Substring(0, start) + newRow + line.Substring(end + 2);
        }

        return line.Substring(0, sectionEnd) + newRow + line.Substring(sectionEnd);
    }

    static void PatchSpeeches()
    {
        // Cast bubbles resolve "MC_" + skill (e.g. MC_bone_ward). Bare skill ids are
        // also registered for tooltip / non-MC speech lookups (vanilla + BW pattern).
        Msl.InjectTableDialogLocalization(BuildSpeechSentences(
            ("desecration", Speech(
                en: "In Nag Zur...",
                ja: "イン・ナグ・ズール…",
                zh: "因纳戈祖尔……",
                de: "In Nag Zur...",
                ru: "In Nag Zur...")),
            ("bone_ward", Speech(
                en: "Ossarak Veth!",
                ja: "オッサラク・ヴェス！",
                zh: "奥萨拉克·维斯！",
                de: "Ossarak Veth!",
                ru: "Ossarak Veth!")),
            ("summon_skeleton", Speech(
                en: "Rattlek Narzul!",
                ja: "ラットレク・ナルズル！",
                zh: "拉特雷克·纳祖尔！",
                de: "Rattlek Narzul!",
                ru: "Rattlek Narzul!")),
            ("withering_curse", Speech(
                en: "Shaggrath Azud...",
                ja: "シャグラト・アズード！",
                zh: "沙格拉斯'阿祖得！",
                de: "Shaggrath Azud...",
                ru: "Shaggrath Azud...")),
            ("essence_leech", Speech(
                en: "En'thero!",
                ja: "エンセロー！",
                zh: "恩塞罗！",
                de: "En'thero!",
                ru: "En'thero!")),
            ("deaths_blessing", Speech(
                en: "Yagrak Atot!",
                ja: "ヤグラク・アトット！",
                zh: "亚格拉克'阿托特！",
                de: "Yagrak Atot!",
                ru: "Yagrak Atot!")),
            ("resurrection", Speech(
                en: "Lagur! Lagur! Lagur!",
                ja: "ラグール！ ラグール！ ラグール！",
                zh: "拉古尔！拉古尔！拉古尔！",
                de: "Lagur! Lagur! Lagur!",
                ru: "Lagur! Lagur! Lagur!")),
            ("grave_blight", Speech(
                en: "Morgath En'ther!",
                ja: "モルガス・エンサー！",
                zh: "莫加斯·恩瑟！",
                de: "Morgath En'ther!",
                ru: "Morgath En'ther!")),
            ("death_touch", Speech(
                en: "Korrug Namar!",
                ja: "コールグ・ナマール！",
                zh: "廓卢戈'纳玛尔！",
                de: "Korrug Namar!",
                ru: "Korrug Namar!")),
            ("corpse_burst", Speech(
                en: "Barath Shatterk!",
                ja: "バラース・シャッターク！",
                zh: "巴拉斯·沙特克！",
                de: "Barath Shatterk!",
                ru: "Barath Shatterk!")),
            ("mastery_of_binding", Speech(
                en: "Vinculum Mortis!",
                ja: "ウィンクルム・モルティス！",
                zh: "温库鲁姆·莫提斯！",
                de: "Vinculum Mortis!",
                ru: "Vinculum Mortis!")),
            ("black_sepulchre", Speech(
                en: "Shaggrath Namar!",
                ja: "シャグラト・ナマール！",
                zh: "沙格拉斯'纳玛尔！",
                de: "Shaggrath Namar!",
                ru: "Shaggrath Namar!")),
            ("sealed_mind", Speech(
                en: "Nox Mentis Seal!",
                ja: "ノックス・メンティス・シール！",
                zh: "诺克斯·门提斯·封印！",
                de: "Nox Mentis Seal!",
                ru: "Nox Mentis Seal!")),
            ("immortality", Speech(
                en: "Aeternum Mortis!",
                ja: "アエテルヌム・モルティス！",
                zh: "埃特尔努姆·莫提斯！",
                de: "Aeternum Mortis!",
                ru: "Aeternum Mortis!")),
            ("blooming_madness", Speech(
                en: "Florath Insania!",
                ja: "フロラス・インサニア！",
                zh: "弗洛拉斯·因萨尼亚！",
                de: "Florath Insania!",
                ru: "Florath Insania!")),
            ("wraith_summoning", Speech(
                en: "Lagura mithensa!",
                ja: "ラグーラ・ミセンサ！",
                zh: "拉古拉·米森萨！",
                de: "Lagura mithensa!",
                ru: "Lagura mithensa!"))
        ));
    }

    static Dictionary<ModLanguage, string> Speech(
        string en, string ja, string zh, string de, string ru) =>
        new()
        {
            { ModLanguage.English, en },
            { ModLanguage.Japanese, ja },
            { ModLanguage.Chinese, zh },
            { ModLanguage.German, de },
            { ModLanguage.Russian, ru },
        };

    static LocalizationSentence[] BuildSpeechSentences(
        params (string SkillId, Dictionary<ModLanguage, string> Text)[] speeches)
    {
        var result = new List<LocalizationSentence>(speeches.Length * 2);
        foreach (var (skillId, text) in speeches)
        {
            result.Add(new LocalizationSentence(skillId, text));
            result.Add(new LocalizationSentence("MC_" + skillId, text));
        }

        return result.ToArray();
    }
}
