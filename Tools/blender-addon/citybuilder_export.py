"""Citybuilder FBX Exporter addon for Blender 4.x (EPIC-20).

Provides an N-panel 'Citybuilder' in the 3D Viewport and a headless helper
function `export_building_fbx(name, output_path)` with deterministic Unity FBX
export settings (1 unit = 1 meter = 1 grid cell, Y up, -Z forward, pivot at ground).
"""
import os
import re
import bpy

bl_info = {
    "name": "Citybuilder FBX Exporter",
    "author": "Citybuilder Team",
    "version": (1, 1, 0),
    "blender": (4, 0, 0),
    "location": "View3D > Sidebar > Citybuilder",
    "description": "Validate and export low-poly Citybuilder buildings to Unity FBX",
    "category": "Import-Export",
}


def validate_mesh_objects(objects):
    """Validate mesh objects before export: non-empty, applied scale, <= 2000 tris."""
    meshes = [obj for obj in objects if obj.type == "MESH"]
    if not meshes:
        return False, "No mesh objects found for export."
    total_tris = 0
    for obj in meshes:
        for comp in obj.scale:
            if abs(comp - 1.0) > 1e-3:
                return False, f"Object '{obj.name}' has unapplied scale {tuple(round(s, 3) for s in obj.scale)}."
        mesh = obj.data
        mesh.calc_loop_triangles()
        total_tris += len(mesh.loop_triangles)
    if total_tris > 2000:
        return False, f"Polygon budget exceeded: {total_tris} tris > 2000 limit."
    return True, f"OK ({len(meshes)} mesh(es), {total_tris} tris)"


def apply_transforms(objects):
    """Apply location/rotation/scale on mesh objects so pivot sits at world origin (0,0,0)."""
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        if obj.type == "MESH":
            obj.select_set(True)
            bpy.context.view_layer.objects.active = obj
    if bpy.context.selected_objects:
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)


def normalize_fbx_timestamps(filepath):
    """Normalize CreationTime/TimeStamp fields in binary FBX for byte-identical reproducible exports."""
    with open(filepath, "rb") as f:
        data = f.read()
    data = re.sub(rb"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}:\d{3}", b"2026-01-01 00:00:00:000", data)
    for tag in (b"YearI", b"MonthI", b"DayI", b"HourI", b"MinuteI", b"SecondI", b"MillisecondI"):
        data = re.sub(tag + rb"....", tag + b"\x00\x00\x00\x00", data, flags=re.DOTALL)
    with open(filepath, "wb") as f:
        f.write(data)


def export_building_fbx(name, repo_root=None, objects=None):
    """Deterministically export mesh objects to Assets/Citybuilder/Models/<name>.fbx."""
    target_objects = objects if objects is not None else [o for o in bpy.context.scene.objects if o.type == "MESH"]
    apply_transforms(target_objects)
    ok, message = validate_mesh_objects(target_objects)
    if not ok:
        raise ValueError(message)

    if repo_root is None:
        script_dir = os.path.dirname(os.path.abspath(__file__))
        repo_root = os.path.abspath(os.path.join(script_dir, "..", ".."))
    models_dir = os.path.join(repo_root, "Assets", "Citybuilder", "Models")
    os.makedirs(models_dir, exist_ok=True)
    out_path = os.path.join(models_dir, f"{name}.fbx")

    bpy.ops.object.select_all(action="DESELECT")
    for obj in target_objects:
        if obj.type == "MESH":
            obj.select_set(True)

    bpy.ops.export_scene.fbx(
        filepath=out_path,
        use_selection=True,
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        use_space_transform=True,
        bake_space_transform=True,
        object_types={"MESH"},
        use_mesh_modifiers=True,
        mesh_smooth_type="OFF",
        use_tspace=False,
        add_leaf_bones=False,
        bake_anim=False,
        axis_forward="-Z",
        axis_up="Y",
    )
    normalize_fbx_timestamps(out_path)
    return out_path, message


class CITYBUILDER_OT_export_fbx(bpy.types.Operator):
    bl_idname = "citybuilder.export_fbx"
    bl_label = "Export FBX to Unity"
    bl_description = "Apply transforms, validate budget (<=2k tris), and export FBX to Assets/Citybuilder/Models"

    building_name: bpy.props.StringProperty(name="Building ID", default="hospital")

    def execute(self, context):
        try:
            out_path, msg = export_building_fbx(self.building_name)
            self.report({"INFO"}, f"Exported {out_path}: {msg}")
            return {"FINISHED"}
        except Exception as exc:
            self.report({"ERROR"}, str(exc))
            return {"CANCELLED"}


class CITYBUILDER_PT_panel(bpy.types.Panel):
    bl_label = "Citybuilder"
    bl_idname = "CITYBUILDER_PT_panel"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    bl_category = "Citybuilder"

    def draw(self, context):
        layout = self.layout
        layout.label(text="Unity Low-Poly Pipeline (1m = 1 cell)")
        op = layout.operator("citybuilder.export_fbx", text="Export FBX")


classes = (CITYBUILDER_OT_export_fbx, CITYBUILDER_PT_panel)


def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)


if __name__ == "__main__":
    register()
