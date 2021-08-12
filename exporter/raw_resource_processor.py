# usage: 
# python3 scripts/resource_processor.py export_models_and_textures -i resources -o /home/hermit/Documents/Godot/under_standing/resources/ -g /home/hermit/Documents/Godot/under_standing/

import argparse
import os
import glob
from pathlib import Path
import shutil
import platform
import json

def format_path(path):
    if not path: return path
    
    if path[-1]=="/" or path[-1]=="\\": path = path[:-1]
    path = os.path.realpath(path)
    path = path.replace("\\", "/")
    return path

working_directory = format_path(os.path.realpath(__file__))
working_directory = working_directory[:working_directory.rfind("/")]
print(working_directory)
args = None
is_on_windows = platform.system() == "Windows"
image_magick_cmd = "convert"

file_registry = {}
file_mtime_registry_json_path = "file_registry.json"

def read_file_registry():
    file_mtime_registry = {}
    with open(file_mtime_registry_json_path, "r", encoding="utf-8") as file:
        text = file.read()
        if text != "":
            file_mtime_registry = json.loads(text)
        else:
            file_mtime_registry = {}
    return file_mtime_registry

def save_file_registry(file_mtime_registry):
    with open(file_mtime_registry_json_path, "w", encoding="utf-8") as file:
        json.dump(file_mtime_registry, file, indent=2)

def is_file_changed(filepath):
    global file_registry
    filepath = format_path(filepath)
    file = Path(filepath)
    stat = file.stat()
    if filepath in file_registry:
        if stat.st_mtime>file_registry[filepath]["mtime"]:
            file_registry[filepath]["mtime"] = stat.st_mtime
            return True
    else:
        file_registry[filepath] = {"mtime": stat.st_mtime}
        return True
    return False
    

def copy_file(source, destination):
    fileA = open(source, 'rb')
    fileB = open(destination, 'wb')
    shutil.copyfileobj(fileA, fileB)


def parse_args(actions):
    parser = argparse.ArgumentParser()
    actions_list = ", ".join(actions.keys())
    parser.add_argument('action', help="can be: "+actions_list)
    parser.add_argument('-i', '--raw-folder')
    parser.add_argument('-f', '--file')
    parser.add_argument('-o', '--output-folder')
    parser.add_argument('-g', '--game-root')
    parser.add_argument('-v', '--verbose', action='store_true', default=False)

    args = parser.parse_args()
    args.raw_folder = format_path(args.raw_folder)
    args.output_folder = format_path(args.output_folder)
    args.game_root = format_path(args.game_root)
    args.file = format_path(args.file)

    return args

def make_dirs(path):
    if path[path.rfind("/")+1:].find(".")!=-1:
        path = path[:path.rfind("/")]
    Path(path).mkdir(parents=True, exist_ok=True)

def run_command(cmd):
    if not args.verbose and not is_on_windows:
        cmd += " > /dev/null"
    os.system(cmd)

def export_blend(blend_filepath, output_filepath):
    model_name = blend_filepath[blend_filepath.rfind("/")+1:blend_filepath.rfind(".")]
    print(model_name)
    cmd = f'blender "{blend_filepath}" --background --python "{working_directory}/blend_export.py" "{output_filepath}" "{args.game_root}"'
    run_command(cmd)

def export_model(filepath, root_dir, output_folder):
    filepath = format_path(filepath)
    export_folder = output_folder + filepath[len(root_dir):filepath.rfind("/")]
    make_dirs(export_folder)
    print("Exporting blend project: "+filepath)
    export_blend(filepath, export_folder)


def export_models(root_dir, output_folder):
    for filepath in glob.glob(root_dir+"/**/*.blend", recursive=True):
        if is_file_changed(filepath):
            export_model(filepath, root_dir, output_folder)

def is_file_execluding(filepath, extension, execlude_ends_with):
    for ending in execlude_ends_with: 
        if filepath.endswith(ending+"."+extension):
            return True

# custom_export_function(filepath, root_dir, output_folder)
def export_file(root_dir, output_folder, extension, execlude_ends_with=[], file_description="file", custom_export_function=None):
    for filepath in glob.glob(root_dir+"/**/*."+extension, recursive=True):
        filepath:str = format_path(filepath)
        
        if not execlude_ends_with is None:
            if is_file_execluding(filepath, extension, execlude_ends_with): continue
        
        export_path = output_folder + filepath[len(root_dir):]
        if is_file_changed(filepath):
            make_dirs(export_path)
            if custom_export_function is None:
                print(f"Copying {extension} {file_description}: {filepath}")
                copy_file(filepath, export_path)
            else:
                print(f"Exporting {extension} {file_description}: {filepath}")
                custom_export_function(filepath, root_dir, output_folder)


def export_textures(root_dir, output_folder):
    export_file(root_dir, output_folder, "png", ["_uv"], "image")
    export_file(root_dir, output_folder, "ico", [], "image")

def convert_psd_to_png(filepath, root_dir, output_folder):
    export_path = output_folder + filepath[len(root_dir):].replace(".psd", ".png")
    cmd = f'{image_magick_cmd} "{filepath}[0]" "{export_path}"'
    run_command(cmd)

def export_psd_textures(root_dir, output_folder):
    export_file(root_dir, output_folder, "psd", [], "image", convert_psd_to_png)
    
def export_models_and_textures():
    export_models(args.raw_folder, args.output_folder)
    export_textures(args.raw_folder, args.output_folder)
    export_psd_textures(args.raw_folder, args.output_folder)

def export_one_model():
    export_model(args.file, args.raw_folder, args.output_folder)

def export_sounds():
    root_dir = args.raw_folder
    output_folder = args.output_folder
    export_file(root_dir, output_folder, "wav", [], "sound")
    export_file(root_dir, output_folder, "mp3", [], "sound")
    export_file(root_dir, output_folder, "ogg", [], "sound")

def export_textures_only():
    export_textures(args.raw_folder, args.output_folder)
    export_psd_textures(args.raw_folder, args.output_folder)

def main():
    global args
    global file_registry
    
    file_registry = read_file_registry()

    actions = {
        "export_models_and_textures": export_models_and_textures,
        "export_sounds": export_sounds,
        "export_model": export_one_model,
        "export_textures_only": export_textures_only
    }

    args = parse_args(actions)
    if args.action in actions:
        actions[args.action]()

    save_file_registry(file_registry)

if __name__=="__main__":
    main()