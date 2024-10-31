import tkinter as tk
from tkinter import ttk
from tkinter import messagebox
import json


file_to_open = "CustomPerks/perk/binbin_mainperk_zeal1d.json"
with open(file_to_open) as f:
    json_data= json.load(f)
    print(json_data)
