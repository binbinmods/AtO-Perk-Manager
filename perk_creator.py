from perk_class import Perk
from perk_node_class import PerkNode
import json
import usefulModdingFunctions as mod
import os

type_dict = {"General":0,"Physical":1,"Elemental":2,"Mystical":3}

def create_new_perk(id:str,
                    aura_curse:str,
                    icon:str):
    perk = Perk()
    
    perk.AuraCurseBonus=aura_curse
    perk.CustomDescription=f"custom_binbin_{id}"
    perk.Icon=icon
    perk.ID=f"binbin_mainperk_{id}"
    perk.IconTextValue="+1"
    return perk
    
    

def get_next_letter(base:PerkNode):
    if base == None:
        return "a"
    ascii_a = ord("a")
    next_letter = chr(ascii_a + len(base.PerksConnected))
    return next_letter

def create_new_perk_node(perk:Perk,
                         col:int,
                         row:int,
                         previous_perk:str=None,
                         node_base:PerkNode=None,
                         cost:int=3,
                         prevent_stacking:bool=False,
                         locked_in_town:bool=False,
                         category:str="General"
                        ):
    
    perkNode = PerkNode()
    perkNode.Perk=perk.ID
    perk_base_id = perk.ID.split("_")[-1]
    perkNode.Column=col
    perkNode.Row=row



    if node_base!=None:
        perk_node_id=f"binbin_perknode_{perk_base_id}{get_next_letter(node_base)}"
        node_base.PerksConnected.append(perk_node_id)
        perkNode.Cost=node_base.Cost
    else:
        perk_node_id=f"binbin_perknode_{perk_base_id}"
        perkNode.Cost="PerkCostBase" if cost==1 else "PerkCostAdvanced"

    perkNode.ID = perk_node_id
    perkNode.NotStack=prevent_stacking
    perkNode.LockedInTown=locked_in_town
    perkNode.PerkRequired = previous_perk if previous_perk != None else ""
    #perkNode.Sprite=icon

    if category not in type_dict:
        raise TypeError("PerkNodeData - Invalid Category")
    perkNode.Type=type_dict[category]

    return perkNode
    
def save_object_to_json(obj,file_to_create):
    if not file_to_create.endswith(".json"):
        file_to_create+=".json"

    with open(file_to_create,"w") as f:
        json.dump(obj.__dict__,f,indent=4)
    

def create_new_perk_split():
    base = PerkNode()
    return base

def test1():
    id = "zeal0"
    ac = "zeal"
    desc = ""
    icon=ac
    base = PerkNode()
    directory_name="TestPerks"
    importing_dir = f"{directory_name}/config/Obeliskial_importing"
    config_directory =f"{directory_name}/config/Obeliskial_importing/{directory_name}"
    new_perk = create_new_perk(id=id,aura_curse=ac,desc=desc,icon=icon,)
    new_perk_node = create_new_perk_node(perk = new_perk,
                                         col=5,
                                         row=1,
                                         previous_perk="",
                                         #node_base=base,
                                         cost=3,
                                         prevent_stacking = False,
                                         locked_in_town=False,
                                         category="General")
    
    mod.save_object_to_json(new_perk,f"{config_directory}/perk",f"binbin_mainperk_{id}")
    mod.save_object_to_json(new_perk_node,f"{config_directory}/perkNode",f"{new_perk_node.ID}.json")

    mod.create_json_to_load_folders(f"{directory_name}","Testing for perks",f"{importing_dir}",types=["perk","perkNode"])


def get_perk_from_name(perk_name,perk_folders)->Perk:#, is_vanilla=True)->Perk:
    perk_file = ""
    for folder in perk_folders:   
        potential_file = f"{folder}/perk/{perk_name}.json"
        if os.path.exists(potential_file):
            perk_file = potential_file
            break
    if perk_file == "":
        raise ValueError("Perk not Found")
    new_perk = Perk()
    perk_dict = mod.create_dict_from_json(perk_file)
    new_perk.map_dict_to_obj(perk_dict)
    return new_perk


def get_perk_node_from_name(perk_node_name,perk_folders)->PerkNode:#, is_vanilla=True)->Perk:
    node_file = ""
    for folder in perk_folders:   
        potential_file = f"{folder}/perkNode/{perk_node_name}.json"
        if os.path.exists(potential_file):
            node_file = potential_file
            break
    if node_file == "":
        raise ValueError("Perk Node not Found")
    new_node = PerkNode()
    node_dict = mod.create_dict_from_json(node_file)
    new_node.map_dict_to_obj(node_dict)
    return new_node


def test2():
    testname = "mainperkbleed0"
    get_perk_from_name(testname)


if __name__=="__main__":
    #test1()
    test2()


    
    