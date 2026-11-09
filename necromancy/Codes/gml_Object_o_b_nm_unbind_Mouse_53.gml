if collision_point(global.guiMouseX, global.guiMouseY, id.object_index, 0, 0)
{
    if visible
    {
        var _mastery = asset_get_index("o_nm_mastery_of_binding_ico")
        var _free = (_mastery != -1 && instance_exists(_mastery) && _mastery.is_open)
        if instance_exists(o_player)
        {
            if _free
            {
                with (o_player)
                    scr_restore_mp(id, (0.1 * Magic_Power), "Mastery of Binding")
            }
            else
                scr_allturn()
        }
        if instance_exists(target)
        {
            var _servant = asset_get_index("o_b_nm_servant")
            if (_servant != -1)
            {
                with (scr_instance_exists_in_list(_servant, target.buffs))
                    instance_destroy()
            }
            with (target)
                instance_destroy()
        }
        instance_destroy()
    }
}
