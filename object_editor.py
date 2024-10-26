import tkinter as tk
from tkinter import ttk
from typing import Any, List
from copy import deepcopy
from perk_class import Perk

class ListEditor:
    def __init__(self, parent, value_list: list, item_type=str, on_update=None):
        self.window = tk.Toplevel(parent)
        self.window.title("Edit List")
        self.window.grab_set()
        
        self.value_list = value_list
        self.item_type = item_type
        self.on_update = on_update
        
        # Create main frame
        self.main_frame = ttk.Frame(self.window, padding="10")
        self.main_frame.pack(fill=tk.BOTH, expand=True)
        
        # Create listbox
        self.listbox = tk.Listbox(self.main_frame, width=40, height=10)
        self.listbox.grid(row=0, column=0, columnspan=2, pady=5, sticky=tk.NSEW)
        
        # Add scrollbar
        scrollbar = ttk.Scrollbar(self.main_frame, orient=tk.VERTICAL, command=self.listbox.yview)
        scrollbar.grid(row=0, column=2, sticky=tk.NS)
        self.listbox.configure(yscrollcommand=scrollbar.set)
        
        # Buttons frame
        btn_frame = ttk.Frame(self.main_frame)
        btn_frame.grid(row=1, column=0, columnspan=3, pady=5)
        
        # Add buttons
        ttk.Button(btn_frame, text="Add", command=self.add_item).pack(side=tk.LEFT, padx=2)
        ttk.Button(btn_frame, text="Edit", command=self.edit_item).pack(side=tk.LEFT, padx=2)
        ttk.Button(btn_frame, text="Remove", command=self.remove_item).pack(side=tk.LEFT, padx=2)
        ttk.Button(btn_frame, text="Done", command=self.save_and_close).pack(side=tk.LEFT, padx=2)
        
        # Make grid expandable
        self.main_frame.columnconfigure(0, weight=1)
        self.main_frame.rowconfigure(0, weight=1)
        
        # Populate listbox
        self.refresh_listbox()
        
        # Center window
        self.window.update_idletasks()
        width = self.window.winfo_width()
        height = self.window.winfo_height()
        x = (self.window.winfo_screenwidth() // 2) - (width // 2)
        y = (self.window.winfo_screenheight() // 2) - (height // 2)
        self.window.geometry(f'+{x}+{y}')
    
    def refresh_listbox(self):
        self.listbox.delete(0, tk.END)
        for item in self.value_list:
            self.listbox.insert(tk.END, str(item))
    
    def add_item(self):
        dialog = ItemDialog(self.window, self.item_type)
        if dialog.result is not None:
            self.value_list.append(dialog.result)
            self.refresh_listbox()
    
    def edit_item(self):
        if not self.listbox.curselection():
            return
        
        idx = self.listbox.curselection()[0]
        dialog = ItemDialog(self.window, self.item_type, self.value_list[idx])
        if dialog.result is not None:
            self.value_list[idx] = dialog.result
            self.refresh_listbox()
    
    def remove_item(self):
        if not self.listbox.curselection():
            return
        
        idx = self.listbox.curselection()[0]
        del self.value_list[idx]
        self.refresh_listbox()
    
    def save_and_close(self):
        if self.on_update:
            self.on_update(self.value_list)
        self.window.destroy()

class ItemDialog:
    def __init__(self, parent, item_type, initial_value=None):
        self.window = tk.Toplevel(parent)
        self.window.title("Edit Item")
        self.window.grab_set()
        
        self.item_type = item_type
        self.result = None
        
        # Create and pack widgets
        frame = ttk.Frame(self.window, padding="10")
        frame.pack(fill=tk.BOTH, expand=True)
        
        self.entry_var = tk.StringVar(value=str(initial_value) if initial_value is not None else "")
        self.entry = ttk.Entry(frame, textvariable=self.entry_var)
        self.entry.pack(pady=5)
        
        btn_frame = ttk.Frame(frame)
        btn_frame.pack(pady=5)
        
        ttk.Button(btn_frame, text="OK", command=self.save).pack(side=tk.LEFT, padx=2)
        ttk.Button(btn_frame, text="Cancel", command=self.window.destroy).pack(side=tk.LEFT, padx=2)
        
        # Center window
        self.window.update_idletasks()
        width = self.window.winfo_width()
        height = self.window.winfo_height()
        x = parent.winfo_rootx() + (parent.winfo_width() // 2) - (width // 2)
        y = parent.winfo_rooty() + (parent.winfo_height() // 2) - (height // 2)
        self.window.geometry(f'+{x}+{y}')
        
        self.entry.focus_set()
        self.window.wait_window()
    
    def save(self):
        try:
            value = self.entry_var.get()
            if self.item_type == int:
                self.result = int(value)
            elif self.item_type == float:
                self.result = float(value)
            elif self.item_type == bool:
                self.result = value.lower() in ('true', '1', 't', 'y', 'yes')
            else:
                self.result = value
            self.window.destroy()
        except ValueError:
            tk.messagebox.showerror("Error", f"Invalid value for type {self.item_type.__name__}")

class EditObjectForm:
    def __init__(self, parent, obj: Any, on_save=None):
        self.window = tk.Toplevel(parent)
        self.window.title(f"Edit {obj.__class__.__name__}")
        self.window.grab_set()
        
        self.obj = obj
        self.on_save = on_save
        self.entries = {}
        
        self.main_frame = ttk.Frame(self.window, padding="10")
        self.main_frame.pack(fill=tk.BOTH, expand=True)
        
        self._create_form_fields()
        self._create_buttons()
        
        # Center window
        self.window.update_idletasks()
        width = self.window.winfo_width()
        height = self.window.winfo_height()
        x = (self.window.winfo_screenwidth() // 2) - (width // 2)
        y = (self.window.winfo_screenheight() // 2) - (height // 2)
        self.window.geometry(f'+{x}+{y}')
    
    def _create_form_fields(self):
        attributes = [attr for attr in dir(self.obj) 
                     if not attr.startswith('_') and 
                     not callable(getattr(self.obj, attr))]
        
        for idx, attr in enumerate(attributes):
            label = ttk.Label(self.main_frame, text=attr.replace('_', ' ').title())
            label.grid(row=idx, column=0, padx=5, pady=5, sticky=tk.W)
            
            value = getattr(self.obj, attr)
            
            if isinstance(value, list):
                # Create a frame to hold list info and edit button
                frame = ttk.Frame(self.main_frame)
                frame.grid(row=idx, column=1, padx=5, pady=5, sticky=tk.EW)
                
                # Add label showing list length
                length_var = tk.StringVar(value=f"{len(value)} items")
                length_label = ttk.Label(frame, textvariable=length_var)
                length_label.pack(side=tk.LEFT)
                
                # Store both the list and its length variable
                self.entries[attr] = {
                    'value': deepcopy(value),
                    'length_var': length_var
                }
                
                # Add edit button
                edit_btn = ttk.Button(
                    frame,
                    text="Edit List",
                    command=lambda a=attr, v=value: self._edit_list(a, v)
                )
                edit_btn.pack(side=tk.LEFT, padx=5)
                
            elif isinstance(value, bool):
                var = tk.BooleanVar(value=value)
                entry = ttk.Checkbutton(self.main_frame, variable=var)
                self.entries[attr] = var
                entry.grid(row=idx, column=1, padx=5, pady=5, sticky=tk.W)
                
            elif isinstance(value, int):
                var = tk.StringVar(value=str(value))
                entry = ttk.Spinbox(self.main_frame, from_=-9999, to=9999, textvariable=var)
                self.entries[attr] = var
                entry.grid(row=idx, column=1, padx=5, pady=5, sticky=tk.EW)
                
            elif isinstance(value, float):
                var = tk.StringVar(value=str(value))
                entry = ttk.Entry(self.main_frame, textvariable=var)
                self.entries[attr] = var
                entry.grid(row=idx, column=1, padx=5, pady=5, sticky=tk.EW)
                
            else:
                var = tk.StringVar(value=str(value))
                entry = ttk.Entry(self.main_frame, textvariable=var)
                self.entries[attr] = var
                entry.grid(row=idx, column=1, padx=5, pady=5, sticky=tk.EW)
        
        self.main_frame.columnconfigure(1, weight=1)
    
    def _edit_list(self, attr, original_list):
        # Determine the type of items in the list
        item_type = str
        if original_list:
            item_type = type(original_list[0])
        
        def update_list(new_list):
            self.entries[attr]['value'] = new_list
            self.entries[attr]['length_var'].set(f"{len(new_list)} items")
        
        ListEditor(
            self.window,
            self.entries[attr]['value'],
            item_type=item_type,
            on_update=update_list
        )
    
    def _create_buttons(self):
        button_frame = ttk.Frame(self.main_frame)
        button_frame.grid(row=len(self.entries), column=0, columnspan=2, pady=10)
        
        save_button = ttk.Button(button_frame, text="Save", command=self._save_changes)
        save_button.pack(side=tk.LEFT, padx=5)
        
        cancel_button = ttk.Button(button_frame, text="Cancel", command=self.window.destroy)
        cancel_button.pack(side=tk.LEFT, padx=5)
    
    def _save_changes(self):
        for attr, var in self.entries.items():
            if isinstance(var, dict) and 'value' in var:  # List type
                setattr(self.obj, attr, var['value'])
            else:  # Other types
                value = var.get()
                original_value = getattr(self.obj, attr)
                
                if isinstance(original_value, bool):
                    value = bool(value)
                elif isinstance(original_value, int):
                    try:
                        value = int(value)
                    except ValueError:
                        value = original_value
                elif isinstance(original_value, float):
                    try:
                        value = float(value)
                    except ValueError:
                        value = original_value
                
                setattr(self.obj, attr, value)
        
        if self.on_save:
            self.on_save(self.obj)
        
        self.window.destroy()

# Example usage
class ExampleObject:
    def __init__(self):
        self.name = "Example"
        self.age = 25
        self.height = 1.75
        self.is_active = True
        self.favorite_numbers = [1, 2, 3, 4, 5]
        self.tags = ["python", "tkinter", "gui"]

def show_current_values(obj):
    info = "\n".join(f"{attr}: {getattr(obj, attr)}"
                    for attr in dir(obj)
                    if not attr.startswith('_') and
                    not callable(getattr(obj, attr)))
    info_label.config(text=info)

def on_save(updated_obj):
    show_current_values(updated_obj)

if __name__ == "__main__":
    root = tk.Tk()
    root.title("Object Editor Demo")
    
    obj = Perk()
    
    
    edit_button = ttk.Button(
        root,
        text="Edit Object",
        command=lambda: EditObjectForm(root, obj, on_save)
    )
    edit_button.pack(padx=20, pady=20)
    
    info_label = ttk.Label(root, text="")
    info_label.pack(padx=20, pady=5)
    
    show_current_values()
    
    root.mainloop()