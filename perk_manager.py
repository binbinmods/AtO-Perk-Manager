import tkinter as tk
from tkinter import ttk, PhotoImage
import json
import usefulModdingFunctions as mod
from perk_class import Perk
import os
import shutil
from typing import List,Dict,Tuple
from PIL import Image, ImageTk
import perk_creator
import object_editor
from perk_node_class import PerkNode

VANILLA_FOLDER = "VanillaPerkData"
PERK_FOLDER = "CustomPerks"
PERK_PATH = f"{PERK_FOLDER}/perk"
PERK_NODE_PATH = f"{PERK_FOLDER}/perkNode"

SHEET_DICT = {0:"General",1:"Physical",2:"Elemental",3:"Mystical"}

class PerkManagerApp:

    def __init__(self, master, custom_path_list):
        self.perk_dict: Dict[int,Dict[Tuple[int,int],Dict]] = {0:{},1:{},2:{},3:{}}        

        self.sprite_images = []
        self.perk_names = []

        self.folders = custom_path_list

        self.parent_nodes = set()
        self.child_nodes = set()

        #self.set_parent_child_nodes(vanilla_path)

        for path in self.folders:
            p = f"{path}/perkNode"
            self.set_parent_child_nodes(p)

        self.add_sprite = ImageTk.PhotoImage(Image.open("Assets/perkSprites/add.png"))

        self.master = master
        self.master.title("Tabbed Table Application")
        self.master.geometry("800x600")

        # Create notebook (tabbed interface)
        self.notebook = ttk.Notebook(self.master)
        self.notebook.pack(fill=tk.BOTH, expand=True)

        # Define tab names and colors
        tabs = [
            (0, "#2c2f32"),  # Darker Gray
            (1, "#363339"),  # Lighter Gray
            (2, "#2c2f32"),  # Darker Gray
            (3, "#363339")  # Lighter Gray
        ]
        
        # Adds vanilla perks
        #self.add_perks_to_dict(vanilla_path)

        # Adds custom perks
        for path in self.folders:
            self.add_perks_to_dict(path)

        for sheet, color in tabs:
            tab = ttk.Frame(self.notebook)
            sheet_info = self.perk_dict[sheet]
            sheet_name = SHEET_DICT[sheet]
            self.notebook.add(tab, text=sheet_name)
            self.create_table(tab, color, sheet_info,sheet_name)

    def create_table(self, parent, bg_color, sheet_info:Dict[Tuple[int,int],str],sheet_name:str):
        # Create a frame with the specified background color
        perk_node_frame = tk.Frame(parent, bg=bg_color)
        perk_node_frame.pack(fill=tk.BOTH, expand=True)

        for row in range(7):
            for col in range(12):
                if (row,col) in sheet_info.keys():
                    d = sheet_info[(row,col)]
                    node_name=d["ID"]
                    perk_node:PerkNode = perk_creator.get_perk_node_from_name(node_name,self.folders)
                    if self.is_child_node(perk_node):
                        continue
                    self.create_perk_node_button(row,col,perk_node_frame,perk_node)

                else:
                    self.create_add_button(row,col,perk_node_frame,sheet_name)
        

    def create_perk_node_button(self,row,col,frame,perk_node:PerkNode):
        sprite_name = perk_node.Sprite
        sprite_path = f"Assets/perkSprites/{sprite_name}.png"
        image = Image.open(sprite_path)  # Replace with your image path
        photo = ImageTk.PhotoImage(image)
        self.sprite_images.append(photo)
        
        btn = tk.Button(frame,image=photo, relief="flat")
        btn.grid(row=row, column=col, padx=2, pady=2)

        btn.config(command=lambda p=perk_node: self.handle_perk_node_click(frame,p))


    def is_parent_node(self,perk_node:PerkNode):
        return perk_node.ID in self.parent_nodes

    def is_child_node(self,perk_node:PerkNode):
        return perk_node.ID in self.child_nodes

    def save_perk(self,perk:Perk):
        # Hardcoded save location for now
        save_location = "CustomPerks/perk"
        perk_filename = perk.ID
        mod.save_object_to_json(perk,f"{save_location}",perk_filename)
        print(f"Saved Perk: {perk.ID}")
    
    def save_perk_node(self,node:PerkNode):
        # Hardcoded save location for now
        save_location = "CustomPerks/perkNode"
        node_filename = node.ID
        mod.save_object_to_json(node,f"{save_location}",node_filename)
        print(f"Saved PerkNode: {node.ID}")

    def handle_perk_node_click(self,parent,perk_node:PerkNode):
        if (self.is_parent_node(perk_node)):
            print(f"found parent {perk_node.ID}")
            self.parent_node_popup(parent,perk_node)
        object_editor.EditObjectForm(parent, perk_node,on_save=self.save_perk_node)

        associated_perk_name = perk_node.Perk
        for folder in self.folders:
            potential_file = f"{folder}/perk/{associated_perk_name}.json"
            # print(f"Potential file: {potential_file}")
            if os.path.exists(potential_file):
                # print(f"trying to open {potential_file}")
                associated_perk:Perk = perk_creator.get_perk_from_name(associated_perk_name,self.folders)
                object_editor.EditObjectForm(parent,associated_perk,on_save=self.save_perk)
                break
        

    def parent_node_popup(self,button_pressed, parent_frame,perk_node:PerkNode):
        # Create popup window
        popup = tk.Toplevel(parent_frame)
        
        popup.title("Child Perk Nodes")
        
        child_node_frame = tk.Frame(popup)
        connected_perks:List[str] = perk_node.PerksConnected
        connected_nodes = [perk_creator.get_perk_node_from_name(perk_node,self.folders) for perk_node in connected_perks]

        child_node_frame.grid()
        for ind,node in enumerate(connected_nodes):
            row = 1
            col = ind+1
            print("creating child nodes")
            self.create_perk_node_button(row,col,child_node_frame,node)
        self.create_add_button(row,col+1,child_node_frame)


    def create_child_perk_button(frame,perk_node:PerkNode):
        return


    def set_parent_child_nodes(self,perk_node_path):#List[Tuple[str,int,int,int,str]]:
        #returns list of tuple of associated perk, page, row and column
        for filename in os.listdir(perk_node_path):
            file_to_open = f"{perk_node_path}/{filename}"
            with open(file_to_open) as f:
                json_data= json.load(f)
                if json_data["Perk"]=="" and json_data["Perk"] != None:
                    # Parent Node
                    self.parent_nodes.add(json_data["ID"])
                    self.child_nodes |= set(json_data["PerksConnected"])
    

    def create_add_button(self,row,col,perk_node_frame,sheet_name:str):
        btn = tk.Button(perk_node_frame,image=self.add_sprite, relief="flat")
        btn.grid(row=row, column=col, padx=2, pady=2)
        btn.config(command=lambda r=row,c=col,b=btn: self.add_new_node(perk_node_frame,b,r,c,sheet_name))

    
    def add_new_node(self,parent_frame, source_button:tk.Button,row,col,sheet_name:str):
        popup = tk.Toplevel(parent_frame)
        popup.title(f"Create New Perk Node and Perk")
        
        # Get the specific button's position
        button_x = source_button.winfo_rootx()
        button_y = source_button.winfo_rooty()
        
        # Position popup next to the button that created it
        popup.geometry(f"+{button_x + source_button.winfo_width()}+{button_y}")
        
        # Choose between "Single" or "Split"
        # Create frame for dialog content
        frame = ttk.Frame(popup, padding="20")
        frame.pack()
        
        # Add label
        ttk.Label(
            frame,
            text="Does this PerkNode have multiple options?",
            font=('Arial', 10, 'bold')
        ).pack(pady=10)
        
        # Add buttons
        btn1:ttk.Button = ttk.Button(
            frame,
            text="No - Single Node"
        )
        btn1.pack(pady=5)
        btn1.config(command=lambda b=btn1: self.create_single_perk_node(frame,b,row,col,sheet_name))

        btn2 = ttk.Button(
            frame,
            text="Yes - Split Node",
        )
        btn2.pack(pady=5)
        btn2.config(command=lambda b=btn2:self.create_split_perk_node(frame,b,row,col,sheet_name))

    def create_split_perk_node(self,parent_element,source_button,sheet_name:str):
        print("split")
        return

    def create_single_perk_node(self,parent_element,source_button,row,col,sheet_name:str):
        popup = tk.Toplevel(parent_element)
        popup.title(f"Create New Perk Node and Perk")
        
        # Get the specific button's position
        button_x = source_button.winfo_rootx()
        button_y = source_button.winfo_rooty()
        popup.geometry(f"+{button_x + source_button.winfo_width()}+{button_y}")


        # Create frame for form
        form_frame = ttk.Frame(popup, padding="20")
        form_frame.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))
        
        id = "bleed0"
        aura_curse = "bleed"
        icon = "bleed1"
        no_stack = False
        locked_in_town = False
        prev_perk_id = ""

        # Create and place form elements
        i=0
        ttk.Label(form_frame, text="ID").grid(
            row=i, column=0, sticky=tk.W, pady=5
        )
        ttk.Entry(
            form_frame,
            textvariable=id
        ).grid(row=i, column=1, padx=5, pady=5)
        i+=1
        ttk.Label(form_frame, text="AuraCurse").grid(
            row=i, column=0, sticky=tk.W, pady=5
        )
        ttk.Entry(
            form_frame,
            textvariable=aura_curse
        ).grid(row=i, column=1, padx=5, pady=5)
        i+=1
        ttk.Label(form_frame, text="Sprite").grid(
            row=i, column=0, sticky=tk.W, pady=5
        )
        ttk.Entry(
            form_frame,
            textvariable=icon            
        ).grid(row=i, column=1, padx=5, pady=5)
        i+=1
        ttk.Label(form_frame, text="Is the Perk Unstackable?").grid(
            row=i, column=0, sticky=tk.W, pady=5
        )
        ttk.Checkbutton(
            form_frame,
            variable=no_stack
        ).grid(row=i, column=1, padx=5, pady=5)
        i+=1
        ttk.Label(form_frame, text="Is the Perk Locked in Town").grid(
            row=i, column=0, sticky=tk.W, pady=5
        )
        ttk.Checkbutton(
            form_frame,
            textvariable=locked_in_town
        ).grid(row=i, column=1, padx=5, pady=5)
        i+=1
        ttk.Label(form_frame, text="What is ID of the required perk? (if exists)").grid(
            row=i, column=0, sticky=tk.W, pady=5
        )
        ttk.Entry(
            form_frame,
            textvariable=prev_perk_id            
        ).grid(row=i, column=1, padx=5, pady=5)
        
        i+=1
        # Save button
        ttk.Button(
            form_frame,
            text="Create",
            command=lambda: self.create_and_save_single_perk_node(id,aura_curse,icon,row,col,sheet_name)
        ).grid(row=i, column=0, columnspan=2, pady=20)
        
    def create_and_save_single_perk_node(self,id,aura_curse,icon,row,col,sheet_name:str):
        # Create the objects
        print(id)
        print(aura_curse)
        print(icon)
        new_perk:Perk = perk_creator.create_new_perk(id,aura_curse,icon)
        prev_perk = ""
        no_stack = True
        lock_in_town = False
        
        new_perk_node:PerkNode = perk_creator.create_new_perk_node(new_perk,
                                                                   col,
                                                                   row,
                                                                   prev_perk,
                                                                   node_base=None,
                                                                   prevent_stacking=no_stack,
                                                                   locked_in_town=lock_in_town,
                                                                   category=sheet_name
                                                                   )
        # Save objects to Json files
        perk_creator.save_object_to_json(new_perk,f"{PERK_PATH}/{new_perk.ID}")
        perk_creator.save_object_to_json(new_perk_node,f"{PERK_NODE_PATH}/{new_perk_node.ID}")


    def get_perk_list(self,perk_node_path)-> List[Dict]:#List[Tuple[str,int,int,int,str]]:
        l = []

        for filename in sorted(os.listdir(perk_node_path),reverse=True):
            file_to_open = f"{perk_node_path}/{filename}"
            with open(file_to_open) as f:
                json_data= json.load(f)
                l.append(json_data)

        return l
    
    


    def add_perks_to_dict(self,folder_path):
        node_path = f"{folder_path}/perkNode"
        perk_list = self.get_perk_list(node_path)
        self.add_perks_from_list_to_dict(perk_list)


    def add_single_perk_to_dict(self,perk_file):
        #TODO
        return


    def add_perks_from_list_to_dict(self,perk_list: List[Dict]):#Tuple[str,int,int,int,str]]):
        for perk_dict in perk_list:
            row = perk_dict["Row"]
            col = perk_dict["Column"]
            sheet = perk_dict["Type"]
            self.perk_dict[sheet][(row,col)]=perk_dict
        

if __name__ == "__main__":
    root = tk.Tk()
    vanilla_path = "VanillaPerkData/perkNode"
    custom_perk_path_list = [PERK_FOLDER,VANILLA_FOLDER] # Vanilla should always come first
    app = PerkManagerApp(root,custom_perk_path_list)
    #perk_list = app.get_perk_list(vanilla_path)
    #simpleList = sorted(perk_list)[:10]
    #print(simpleList)
    #print(sorted(app.parent_nodes))
    #print(sorted(app.child_nodes))
    root.mainloop()
    
