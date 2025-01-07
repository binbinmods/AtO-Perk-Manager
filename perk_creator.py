from perk_class import Perk
from perk_node_class import PerkNode
import json
import usefulModdingFunctions as mod
import os
import regex as re

type_dict = {"General":0,"Physical":1,"Elemental":2,"Mystical":3}
aura_curse_string = '''bless
    block
    buffer
    courage
    energize
    evasion
    fast
    fortify
    furnace
    fury
    haste
    inspire
    insulate
    invulnerable
    luckyscarab
    mitigate
    powerful
    regeneration
    reinforce
    sharp
    shield
    stanzai
    stanzaii
    stanzaiii
    stealth
    stealthbonus
    taunt
    thorns
    vitality
    zeal
    bleed
    burn
    chill
    crack
    dark
    daze
    decay
    disarm
    doom
    exhaust
    fatigue
    paralyze
    insane
    mark
    poison
    sanctify
    scourge
    shackle
    sight
    silence
    slow
    spark
    stress
    vulnerable
    weak
    wet'''
    
additional_sprites = set([file.split(".png")[0] for file in os.listdir("Assets/perkSprites") if file.endswith(".png")])
aura_curses=set(aura_curse_string.split())

PERK_DESC_STEM = "custom_binbin_mainperk_"
PERK_ID_STEM = "binbin_mainperk_"
NODE_ID_STEM = "binbin_perknode_"

VANILLA_NODE_STEM = "nodeperk"
VANILLA_PERK_STEM = "mainperk"

NODE_DIR = "CustomPerks/perkNode/"
PERK_DIR = "CustomPerks/perk/"

def create_new_perk(id:str,
                    aura_curse:str="",
                    aura_curse_bonus:int = 0,
                    icon:str=""):
    
    if id.startswith(PERK_ID_STEM):
        id = id.replace(PERK_ID_STEM,'')
    
    if (id.startswith("forttify1")):
        id= id.replace("forttify1","fortify1")

    perk = Perk() 
    perk.AuraCurseBonus="None"# if aura_curse_bonus == 0 else aura_curse
    perk.AuraCurseBonusValue=aura_curse_bonus
    perk.CustomDescription=f"{PERK_DESC_STEM}{id}"
    perk.Icon=icon
    perk.ID= f"{PERK_ID_STEM}{id}" 
    perk.IconTextValue="+1"
    perk.CardClass="None"
    if id=="shackle1a":
        perk.AuraCurseBonus="shackle"
        perk.AuraCurseBonusValue = 1
    if id=="health6b":
        perk.MaxHealth = -12
    if id=="health6c":
        perk.MaxHealth = 36
    if id=="resistance5b":
        perk.ResistModified = "All"
        perk.ResistModifiedValue = -4
    if id=="resistance5c":
        perk.ResistModified = "All"
        perk.ResistModifiedValue = 12
    if id=="paralyze1a":
        perk.AuraCurseBonus="paralyze"
        perk.AuraCurseBonusValue = 1
        perk.CustomDescription=""
    if id=="poison2h":
        perk.AuraCurseBonus="poison"
        perk.AuraCurseBonusValue = -1
    if id =="mark1f":
        perk.AuraCurseBonus="mark"
        perk.AuraCurseBonusValue=1


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


    ''''''
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


def create_new_perk_node_improved(perk:Perk,
                         perk_node_name:str,                         
                         col:int,
                         row:int,
                         previous_perk:str=None,
                         sprite:str=None,
                         cost:int=3,
                         prevent_stacking:bool=False,
                         locked_in_town:bool=False,
                         category:int=0
                        ):
    perkNode = PerkNode()
    perkNode.Perk=perk.ID
    perkNode.Column=col
    perkNode.Row = row
    perkNode.PerkRequired=previous_perk if previous_perk != None else ""
    #perkNode.Cost=cost
    perkNode.NotStack=prevent_stacking
    perkNode.LockedInTown=locked_in_town
    perkNode.Type=category
    perkNode.ID=perk_node_name
    perkNode.Cost = "PerkCostAdvanced"
    perkNode.Sprite = sprite
    if (perk_node_name=="binbin_perknode_forttify1d"):
        perkNode.Sprite = "fortify"
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


def get_perk_node_from_name(perk_node_name,perk_folders:list[str])->PerkNode:#, is_vanilla=True)->Perk:
    node_file = ""
    for folder in perk_folders:
        if perk_node_name=="nodeperkFortify1":
            potential_file = f"{folder}/perkNode/nodeperkForttify1.json" 
        else:

            potential_file = f"{folder}/perkNode/{perk_node_name}.json"        
        if os.path.exists(potential_file):
            node_file = potential_file
            break
    if node_file == "":
        raise ValueError("Perk Node not Found " +perk_node_name)
    new_node = PerkNode()
    node_dict = mod.create_dict_from_json(node_file)
    new_node.map_dict_to_obj(node_dict)
    return new_node


def create_perk_from_id(short_ID):
    id = short_ID
    ac = re.split(r'(\d+)', id)[0]
    is_ac = ac in aura_curses
    ac_str = ac if is_ac else ""
    icon = ac if is_ac else ""
    icon = ac if ac in additional_sprites else ""
    p = create_new_perk(id=id,aura_curse=ac_str,icon=icon)
    file_name = p.ID
    save_object_to_json(p,PERK_DIR+file_name)
    #print(p.CustomDescription)


def get_perk_inputs_from_id(full_id:str):
    match_list = re.split(r'(\d+)', full_id)
    id = full_id.split("_")[-1]
    ac = match_list[0].split("_")[-1]
    is_ac = ac in aura_curses
    ac_str = ac if is_ac else ""
    icon = ac if is_ac else ""
    icon = ac if ac in additional_sprites else ""
    return (id,ac_str,icon)


def create_perk_nodes_from_base(base:str):
    perk_base_name = base
    perk_node_filename = NODE_ID_STEM+perk_base_name
    perk_folders = ["CustomPerks"]
    #print(perk_node_filename)
    perk_base:PerkNode = get_perk_node_from_name(perk_node_filename,perk_folders)

    #print(perk_base.PerksConnected)
    for connected_perk in perk_base.PerksConnected:
        id,ac_str,icon = get_perk_inputs_from_id(connected_perk)
        perk = create_new_perk(id=id,aura_curse=ac_str,icon=icon)

        new_perk_node_name = NODE_ID_STEM+id
        p = create_new_perk_node_improved(perk=perk,
                            perk_node_name=new_perk_node_name,
                            col=perk_base.Column,
                            row=perk_base.Row,
                            sprite=icon,
                            cost=perk_base.Cost,
                            prevent_stacking=True,
                            category=perk_base.Type
                            )
        #print(p.ID)
        save_object_to_json(p,NODE_DIR+p.ID)
        save_object_to_json(perk,PERK_DIR+perk.ID)


def create_new_perk_base(id,row,col,n:int=0,category:str="General"):
        p = PerkNode()
        p.ID=NODE_ID_STEM+id
        p.Column = col
        p.Row = row
        p.PerksConnected=[NODE_ID_STEM+id+chr(ord('a')+i) for i in range(n)]
        p.Type=type_dict[category]
        p.Perk=""
        p.Sprite="perk"
        p.Cost = "PerkCostAdvanced"
        return p


def create_all_perk_jsons(tuple_array):
    for tuple in tuple_array:
        id,r,c,n,category = tuple
        print(id)
        p:PerkNode = create_new_perk_base(id,r,c,n,category)
        save_object_to_json(p,f"{NODE_DIR}{p.ID}")
        #print(p.PerksConnected)
        create_perk_nodes_from_base(id)


def add_perks_to_existing_node(node:PerkNode, n_perks_to_add,is_vanilla):
    connected_nodes:list[str] = node.PerksConnected
    if len(connected_nodes) == 0:
        raise ValueError("Incorrect Node type")
    
    last_letter = ord('a')
    for existing_node in connected_nodes:
        if existing_node[-1].isdigit():
            raise ValueError("Connected Perk has wrong format: " + existing_node)
        if ord(existing_node[-1])>last_letter:
            last_letter=ord(existing_node[-1])
    
    if is_vanilla==False:
        raise ValueError("Non-Vanilla implemenation not added yet")

    perk_base = node.ID.replace("nodeperk",'').lower()
    for i in range(n_perks_to_add):
        next_perk_id = PERK_ID_STEM+perk_base+chr(last_letter+i+1)
        id,ac_str,icon = get_perk_inputs_from_id(next_perk_id)
        new_perk:Perk = create_new_perk(id=id,aura_curse=ac_str,icon=icon)
        save_object_to_json(new_perk,PERK_DIR+new_perk.ID)

        new_perk_node_name = NODE_ID_STEM+id
        p = create_new_perk_node_improved(perk=new_perk,
                            perk_node_name=new_perk_node_name,
                            col=node.Column,
                            row=node.Row,
                            sprite=icon,
                            cost=node.Cost,
                            prevent_stacking=True,
                            category=node.Type
                            )
        p.PerkRequired=node.PerkRequired
        save_object_to_json(p,NODE_DIR+p.ID)

        connected_nodes.append(p.ID)

    save_object_to_json(node,NODE_DIR+node.ID)


def create_new_perk_node_and_perk_jsons(source_node:PerkNode,full_perk_id:str):
    id,ac_str,icon = get_perk_inputs_from_id(full_perk_id)
    new_perk:Perk = create_new_perk(id=id,aura_curse=ac_str,icon=icon)
    save_object_to_json(new_perk,PERK_DIR+new_perk.ID)

    new_perk_node_name = NODE_ID_STEM+id
    p = create_new_perk_node_improved(perk=new_perk,
                        perk_node_name=new_perk_node_name,
                        col=source_node.Column,
                        row=source_node.Row,
                        sprite=icon,
                        cost=source_node.Cost,
                        prevent_stacking=True,
                        category=source_node.Type
                        )
    p.PerkRequired=source_node.PerkRequired
    source_node.PerksConnected.append(p.ID)
    save_object_to_json(p,NODE_DIR+p.ID)


def create_new_split_node(node_id:str,n_to_add:int):
    orig_node:PerkNode = get_perk_node_from_name(node_id,["VanillaPerkData"])
    orig_perk_id = orig_node.Perk
    new_perk_id = orig_perk_id + "a"
    old_node_id = orig_node.ID
    new_node_id = orig_node.ID+"a"
    orig_node.ID=new_node_id
    save_object_to_json(orig_node,NODE_DIR+new_node_id)

    orig_node.PerksConnected=[new_node_id]
    orig_node.Perk = ""

    
    orig_perk:Perk = get_perk_from_name(orig_perk_id,["VanillaPerkData"])
    orig_perk.ID= new_perk_id
    save_object_to_json(orig_perk,PERK_DIR+new_perk_id)

    for i in range(1,n_to_add+1):
        next_letter = chr(ord('a')+i)
        next_perk_id = PERK_ID_STEM+orig_perk_id.replace(VANILLA_PERK_STEM,'')+next_letter
        create_new_perk_node_and_perk_jsons(orig_node,next_perk_id)
            
    orig_node.ID=old_node_id
    save_object_to_json(orig_node,NODE_DIR+orig_node.ID)


def handle_new_nodes():
    # Creates completely new nodes and associated perks
    #name, row, col, number, sheet
    tuples = [
        (
            "disarm1",
            6,
            4,
            2,
            "General"
        ),
        (
            "silence1",
            6,
            6,
            2,
            "General"
        ),
        (
            "mitigate1",
            6,
            10,
            5,
            "Physical"
        ),
        (
            "shackle1",
            6,
            8,
            6,
            "Physical"
        ),
        (
            "energize1",
            6,
            8,
            4,
            "Elemental"
        ),
        (
            "spellsword1",
            6,
            2,
            4,
            "Elemental"
        ),
        (
            "paralyze1",
            6,
            5,
            3,
            "Elemental"
        ),
        # (
        #     "zeal1",
        #     6,
        #     2,
        #     5,
        #     "Elemental"
        # ),
        # (
        #     "scourge1",
        #     6,
        #     5,
        #     5,
        #     "Elemental"
        # ),
        # (
        #     "weak1",
        #     6,
        #     8,
        #     4,
        #     "Elemental"
        # ),
        
    ]
    create_all_perk_jsons(tuples)


def handle_adding_perks_to_vanilla_nodes():
    # Adds perks to currently existing node
    tuples = [
        ("poison2",5),
        ("bleed2",4),
        ("thorns1",3),
        ("reinforce1",1),
        ("taunt1",1),
        ("sight1",2),
        ("burn2",1),
        ("chill2",3),
        ("spark2",4),
        ("wet1",1),
        ("inspire0",1),
        ("insane2",3),
        ("dark2",1),
        ("sanctify2",2),
        ("decay1",2),
        ("courage1",1),
        ("vitality1",1),
        ("bless1",1),
        ("powerful1",1),
        ("fury1",1),
        ("mark1",3),
        ("fortify1",2),
        ("sharp1",1),
        ("energy2",1),
        ("insulate1",2),
        ("regeneration1",1),
        ("stanza0",2),
        ("crack2",6),
    ]
    for tuple in tuples:
        node_name,n = tuple

        node:PerkNode = get_perk_node_from_name(f"{VANILLA_NODE_STEM}{node_name.capitalize()}",["VanillaPerkData"])
        add_perks_to_existing_node(node,n,True)


def handle_creating_new_split_nodes():
    # Creates new split nodes from currently existing nodes
    tuples = [
        ("health6",3),
        ("resistance5",3),
        ("currency6",4),
        ("shards5",2),
        ("block5",3),
        ("fast0",2),
        ("slow0",2),
        ("shield5",2),
        ("heal5",2),
        ("evasion0",1),
    ]
    for tuple in tuples:
        node_id = VANILLA_NODE_STEM+tuple[0].capitalize()
        n_to_add = tuple[1]
        create_new_split_node(node_id,n_to_add)
        

if __name__=="__main__":
    
    handle_new_nodes()
    handle_adding_perks_to_vanilla_nodes()
    handle_creating_new_split_nodes()


    