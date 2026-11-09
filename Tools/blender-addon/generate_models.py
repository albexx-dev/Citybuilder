"""Procedural low-poly generator for Citybuilder Hospital (3x3) and Park (2x2) (EPIC-20).

Run via Blender CLI:
  blender -b -P Tools/blender-addon/generate_models.py
"""
import hashlib
import os
import sys
import bpy

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
if SCRIPT_DIR not in sys.path:
    sys.path.insert(0, SCRIPT_DIR)

import citybuilder_export as exporter


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for mesh in list(bpy.data.meshes):
        bpy.data.meshes.remove(mesh)
    for mat in list(bpy.data.materials):
        bpy.data.materials.remove(mat)


def make_material(name, rgba):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = False
    mat.diffuse_color = rgba
    return mat


def add_box(name, size_xyz, center_xyz, mat):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=center_xyz)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = size_xyz
    if obj.data.materials:
        obj.data.materials[0] = mat
    else:
        obj.data.materials.append(mat)
    return obj


def add_cone(name, radius, depth, center_xyz, mat, vertices=8):
    bpy.ops.mesh.primitive_cone_add(
        vertices=vertices, radius1=radius, radius2=0.0, depth=depth, location=center_xyz
    )
    obj = bpy.context.active_object
    obj.name = name
    if obj.data.materials:
        obj.data.materials[0] = mat
    else:
        obj.data.materials.append(mat)
    return obj


def build_hospital():
    """Hospital footprint 3x3 cells (2.8m x 2.8m margin, ~2.2m height) with red cross."""
    clear_scene()
    mat_wall = make_material("HospitalWall", (0.94, 0.96, 0.98, 1.0))
    mat_roof = make_material("HospitalRoof", (0.78, 0.82, 0.86, 1.0))
    mat_cross = make_material("HospitalCross", (0.88, 0.16, 0.16, 1.0))
    mat_glass = make_material("HospitalGlass", (0.35, 0.65, 0.85, 1.0))

    parts = [
        add_box("BasePlinth", (2.8, 2.8, 0.15), (0.0, 0.0, 0.075), mat_roof),
        add_box("MainBlock", (2.5, 1.8, 1.6), (0.0, 0.15, 0.95), mat_wall),
        add_box("EntranceWing", (1.2, 0.7, 1.1), (0.0, -1.0, 0.65), mat_wall),
        add_box("RoofTower", (1.3, 1.1, 0.45), (0.0, 0.15, 1.975), mat_roof),
        add_box("EntranceCanopy", (0.9, 0.35, 0.1), (0.0, -1.42, 0.85), mat_roof),
        add_box("EntranceDoor", (0.5, 0.08, 0.65), (0.0, -1.36, 0.45), mat_glass),
        # Red Cross on front facade
        add_box("CrossVertFront", (0.22, 0.08, 0.66), (0.0, -0.76, 1.42), mat_cross),
        add_box("CrossHorzFront", (0.66, 0.08, 0.22), (0.0, -0.76, 1.42), mat_cross),
        # Red Cross on roof
        add_box("CrossVertRoof", (0.26, 0.78, 0.08), (0.0, 0.15, 2.24), mat_cross),
        add_box("CrossHorzRoof", (0.78, 0.26, 0.08), (0.0, 0.15, 2.24), mat_cross),
    ]

    exporter.apply_transforms(parts)
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    joined = bpy.context.active_object
    joined.name = "Hospital"
    return exporter.export_building_fbx("hospital", objects=[joined])


def build_park():
    """Park footprint 2x2 cells (1.9m x 1.9m lawn, cross paths, 3 low-poly trees, bench)."""
    clear_scene()
    mat_grass = make_material("ParkGrass", (0.30, 0.66, 0.32, 1.0))
    mat_path = make_material("ParkPath", (0.76, 0.70, 0.56, 1.0))
    mat_trunk = make_material("ParkTrunk", (0.45, 0.28, 0.15, 1.0))
    mat_leaves = make_material("ParkLeaves", (0.18, 0.52, 0.22, 1.0))
    mat_bench = make_material("ParkBench", (0.58, 0.36, 0.18, 1.0))

    parts = [
        add_box("LawnBase", (1.9, 1.9, 0.08), (0.0, 0.0, 0.04), mat_grass),
        add_box("PathNS", (0.36, 1.86, 0.04), (0.0, 0.0, 0.09), mat_path),
        add_box("PathEW", (1.86, 0.36, 0.04), (0.0, 0.0, 0.09), mat_path),
        # Bench
        add_box("BenchSeat", (0.42, 0.16, 0.06), (0.42, -0.35, 0.16), mat_bench),
        add_box("BenchBack", (0.42, 0.05, 0.14), (0.42, -0.41, 0.24), mat_bench),
    ]

    tree_coords = [(-0.55, 0.55, 1.0), (0.55, 0.52, 0.9), (-0.52, -0.52, 0.85)]
    for idx, (tx, ty, scale) in enumerate(tree_coords, 1):
        parts.append(add_box(f"Trunk{idx}", (0.12, 0.12, 0.36 * scale), (tx, ty, 0.08 + 0.18 * scale), mat_trunk))
        parts.append(add_cone(f"CrownA{idx}", 0.34 * scale, 0.52 * scale, (tx, ty, 0.08 + 0.52 * scale), mat_leaves))
        parts.append(add_cone(f"CrownB{idx}", 0.26 * scale, 0.42 * scale, (tx, ty, 0.08 + 0.78 * scale), mat_leaves))

    exporter.apply_transforms(parts)
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    joined = bpy.context.active_object
    joined.name = "Park"
    return exporter.export_building_fbx("park", objects=[joined])


def sha256_file(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        h.update(f.read())
    return h.hexdigest()


if __name__ == "__main__":
    hosp_path, hosp_msg = build_hospital()
    hosp_obj = bpy.context.scene.objects.get("Hospital")
    hosp_hash1 = sha256_file(hosp_path)
    hosp_path, _ = exporter.export_building_fbx("hospital", objects=[hosp_obj])
    hosp_hash2 = sha256_file(hosp_path)
    assert hosp_hash1 == hosp_hash2, "Hospital FBX export must be deterministic!"

    park_path, park_msg = build_park()
    park_obj = bpy.context.scene.objects.get("Park")
    park_hash1 = sha256_file(park_path)
    park_path, _ = exporter.export_building_fbx("park", objects=[park_obj])
    park_hash2 = sha256_file(park_path)
    assert park_hash1 == park_hash2, "Park FBX export must be deterministic!"

    print(f"HOSPITAL_OK: {hosp_path} ({hosp_msg}) sha256={hosp_hash1[:12]}")
    print(f"PARK_OK: {park_path} ({park_msg}) sha256={park_hash1[:12]}")
