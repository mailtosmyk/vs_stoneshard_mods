function scr_nm_refresh_summon_enchants() //gml_Script_scr_nm_refresh_summon_enchants
{
    // Re-apply gear + Seal of Unholiness bonuses on every living player summon.
    with (o_enemy)
    {
        if (variable_instance_exists(id, "nm_summon") && nm_summon)
            scr_nm_apply_summon_enchants(id)
    }
    return 1
}
