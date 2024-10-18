from typing import Dict, List, Optional, Any
from dataclasses import dataclass, field
import json
import os
from datetime import date

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
