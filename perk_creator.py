from perk_class import Perk
from perk_node_class import PerkNode
import json
import usefulModdingFunctions as mod

type_dict = {"general":0,"physical":1,"elemental":2,"mystical":3}

def create_new_perk(id:str,
                    aura_curse:str,
                    desc:str,
                    icon:str):
    perk = Perk()
    
    perk.AuraCurseBonus=aura_curse
    perk.CustomDescription=f"custom_binbin_{desc}"
    perk.Icon=icon
    perk.ID=f"binbin_mainperk_{id}"

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
                         category:str="general"
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
                                         category="general")
    
    mod.save_object_to_json(new_perk,f"{config_directory}/perk",f"binbin_mainperk_{id}")
    mod.save_object_to_json(new_perk_node,f"{config_directory}/perkNode",f"{new_perk_node.ID}.json")

    mod.create_json_to_load_folders(f"{directory_name}","Testing for perks",f"{importing_dir}",types=["perk","perkNode"])


def get_perk_from_name(perk_name,perk_folder, is_vanilla=True)->Perk:
    if is_vanilla:
        perk_file = f"VanillaPerkData/perk/{perk_name}.json"
    else:
        perk_file = f"{perk_folder}/{perk_name}.json"
    new_perk = Perk()
    perk_dict = mod.create_dict_from_json(perk_file)
    new_perk.map_dict_to_obj(perk_dict)
    return new_perk

def test2():
    testname = "mainperkbleed0"
    get_perk_from_name(testname)


if __name__=="__main__":
    #test1()
    test2()


    
    