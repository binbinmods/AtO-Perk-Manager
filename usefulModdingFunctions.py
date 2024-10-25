from typing import Dict, List, Optional, Any
from dataclasses import dataclass, field
import json
import os
import shutil
from datetime import date
from PIL import Image


@dataclass
class LoadingJson:
    """Class generated from JSON data."""

    Name: str = ''
    Author: str = 'binbin'
    Description: str = ''
    Version: str = '1.0.0'
    Date: int = 20241016
    Link: str = 'https://across-the-obelisk.thunderstore.io/'
    Dependencies: List[str] = field(default_factory=lambda: ['stiffmeds-Obeliskial_Content-1.2.2'])
    ContentFolder: str = 'NodeTest'
    Priority: int = 100
    Type: List[str] = field(default_factory=lambda: ['event', 'eventReply', 'eventRequirement', 'node', 'sprite'])
    Comment: str = ''
    Enabled: bool = True

def create_manifest(directory):
    if not os.path.isfile(f"{directory}/manifest.json"):
        pass

def create_icon(directory):
    if not os.path.isfile(f"{directory}/icon.png"):
        image = Image.new('RGB', (256, 256))
        image.save(f"{directory}/icon.png", "PNG")

def create_plugins(directory,name):
    # TODO fix this
    fp = f"{directory}/plugins"
    if not os.path.exists(fp):
        os.makedirs(f"{fp}")

    return

def create_config(directory,name):
    # TODO fix this
    fp = f"{directory}/config/Obeliskial_importing/{name}/"
    if not os.path.exists(directory):
        os.makedirs(f"{directory}")
    return

def create_readme(directory):
    dst = f"{directory}/README.md"
    if not os.path.isfile(dst):
        shutil.copyfile("Assets/README.md",dst)

def create_changelog(directory):
    dst = f"{directory}/CHANGELOG.md"

    if not os.path.isfile(dst):
        shutil.copyfile("Assets/CHANGELOG.md",dst)

def create_thunderstore_folder(directory,mod_name):
    create_manifest(directory)
    create_icon(directory)
    create_plugins(directory)
    create_config(directory)
    create_readme(directory)
    create_changelog(directory)


def save_object_to_json(obj,folder_to_create_json_in,json_filename):
    if not os.path.exists(folder_to_create_json_in):
        os.makedirs(folder_to_create_json_in)
    with open(f"{folder_to_create_json_in}/{json_filename}.json","w") as f:
        json.dump(obj.__dict__,f,indent=4)


def create_json_to_load_folders(name:str,desc:str,output_folder:str,types:List[str]):
    new_json = LoadingJson()
    new_json.Name=name
    new_json.Description=desc
    new_json.Date = int(date.today().strftime("%Y%m%d"))
    output_file= f"{output_folder}/{name}.json"
    new_json.Type=types
    new_json.ContentFolder=name

    with open(output_file,"w") as f:
        json.dump(new_json.__dict__,f,indent=4)

def create_dict_from_json(json_filepath:str)->Dict:
    if not json_filepath.endswith(".json"):
        json_filepath+=".json"
        
    with open(json_filepath) as f:
        json_dict = json.load(f)
        return json_dict
