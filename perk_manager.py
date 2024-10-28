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


class PerkManagerApp:

    def __init__(self, master, vanilla_path, custom_path_list):
        self.perk_dict: Dict[int,Dict[Tuple[int,int],Dict]] = {0:{},1:{},2:{},3:{}}
        self.sheet_dict = {0:"General",1:"Physical",2:"Elemental",3:"Mystical"}

        self.sprite_images = []
        self.perk_names = []

        self.parent_nodes = set()
        self.child_nodes = set()

        self.set_parent_child_nodes(vanilla_path)

        for path in custom_path_list:
            self.set_parent_child_nodes(path)

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
        self.add_perks_to_dict(vanilla_path)

        # Adds custom perks
        for path in custom_path_list:
            self.add_perks_to_dict(path)

        for sheet, color in tabs:
            tab = ttk.Frame(self.notebook)
            sheet_info = self.perk_dict[sheet]
            sheet_name = self.sheet_dict[sheet]
            self.notebook.add(tab, text=sheet_name)
            self.create_table(tab, color, sheet_info)

    def create_table(self, parent, bg_color, sheet_info:Dict[Tuple[int,int],str]):
        # Create a frame with the specified background color
        perk_node_frame = tk.Frame(parent, bg=bg_color)
        perk_node_frame.pack(fill=tk.BOTH, expand=True)
        #self.frame.grid(row=1, column=0, pady=10)
        i=0
        for row in range(7):
            for col in range(12):
                if (row,col) in sheet_info.keys():
                    d = sheet_info[(row,col)]
                    if i==0:
                        print(d)
                        i+=1
                    self.create_perk_button(row,col,perk_node_frame,d)

                else:
                    self.create_add_button(row,col,perk_node_frame)


    def create_perk_button(self,row,col,perk_node_frame,d):
        perk_name = d["Perk"]
        self.perk_names.append(perk_name)
        sprite_name = d["Sprite"]
        sprite_path = f"Assets/perkSprites/{sprite_name}.png"
        image = Image.open(sprite_path)  # Replace with your image path
        photo = ImageTk.PhotoImage(image)
        self.sprite_images.append(photo)

        #perk:Perk  = perk_creator.get_perk_from_name(perk_name,"",is_vanilla=True)
        node_name = d["ID"]
        
        perk_node:PerkNode = perk_creator.get_perk_node_from_name(node_name,"",is_vanilla=True)
        if self.is_child_node(perk_node):
            return
        
        
        btn = tk.Button(perk_node_frame,image=photo, relief="flat")
        btn.grid(row=row, column=col, padx=2, pady=2)

        #btn.config(command=lambda p=perk_node: object_editor.EditObjectForm(root, p))
        btn.config(command=lambda p=perk_node: self.handle_perk_node_click(root,p))
        

    def is_parent_node(self,perk_node:PerkNode):
        return perk_node.ID in self.parent_nodes

    def is_child_node(self,perk_node:PerkNode):
        return perk_node.ID in self.child_nodes

    def handle_perk_node_click(self,root,perk_node:PerkNode):
        if (self.is_parent_node(perk_node)):
            print(f"found parent {perk_node.ID}")
            self.parent_node_popup(root,perk_node)
        object_editor.EditObjectForm(root, perk_node)
        

    def parent_node_popup(self,root,perk_node:PerkNode):
        # Create popup window
        popup = tk.Toplevel(root)
        popup.title("Circle Grid")
        
        # Position the popup window below the button
        #button_x = self.btn.winfo_rootx()
        #button_y = self.btn.winfo_rooty()
        #popup.geometry(f"+{button_x}+{button_y + 50}")
        
        # Create canvas in popup
        connected_perks = perk_node.PerksConnected
        perks = [perk_creator.get_perk_node_from_name(perk_node.ID,)]
        n_perks = len(connected_perks)

        child_node_frame = tk.Frame(root)
        child_node_frame.pack(fill=tk.BOTH, expand=True)
        #self.frame.grid(row=1, column=0, pady=10)
        i=0
        for col in range(n_perks+1):
            child_node = perks[col]
            self.create_child_perk_button(child_node_frame,)


    def create_child_perk_button(child_node_frame,perk_node:PerkNode):
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
    

    def create_add_button(self,row,col,perk_node_frame):
        btn = tk.Button(perk_node_frame,image=self.add_sprite, relief="flat")
        btn.grid(row=row, column=col, padx=2, pady=2)
        btn.config(command=lambda r=row,c=col: self.add_new_node(r,c))

    
    def add_new_node(self,row,col):
        
        return row+col

    def open_perk_popup(self, perk_name):
        perk:Perk  = perk_creator.get_perk_from_name(perk_name)


    def get_perk_list(self,perk_node_path)-> List[Dict]:#List[Tuple[str,int,int,int,str]]:
        l = []

        for filename in sorted(os.listdir(perk_node_path),reverse=True):
            file_to_open = f"{perk_node_path}/{filename}"
            with open(file_to_open) as f:
                json_data= json.load(f)
                l.append(json_data)

        return l
    
    


    def add_perks_to_dict(self,perk_path):
        perk_list = self.get_perk_list(perk_path)
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
    custom_perk_path_list = []
    app = PerkManagerApp(root,vanilla_path,custom_perk_path_list)
    perk_list = app.get_perk_list(vanilla_path)
    #simpleList = sorted(perk_list)[:10]
    #print(simpleList)
    print(sorted(app.parent_nodes))
    print(sorted(app.child_nodes))
    root.mainloop()
    
