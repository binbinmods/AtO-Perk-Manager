import tkinter as tk
from tkinter import ttk
from tkinter import messagebox

class DynamicFormApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Dynamic Form Generator")
        
        # Initialize variables to store form data
        self.string_vars = []  # Will be populated based on user choice
        self.values = []       # Will store final values
        
        # Create main button
        self.main_button = ttk.Button(
            root,
            text="Create Form",
            command=self.prompt_field_count
        )
        self.main_button.pack(pady=20)
        
    def prompt_field_count(self):
        # Create a small dialog window
        dialog = tk.Toplevel(self.root)
        dialog.title("Select Field Count")
        
        # Position dialog near the main button
        button_x = self.main_button.winfo_rootx()
        button_y = self.main_button.winfo_rooty()
        dialog.geometry(f"+{button_x}+{button_y + 50}")
        
        # Make dialog modal (user must interact with it before continuing)
        dialog.transient(self.root)
        dialog.grab_set()
        
        # Create frame for dialog content
        frame = ttk.Frame(dialog, padding="20")
        frame.pack()
        
        # Add label
        ttk.Label(
            frame,
            text="How many fields would you like?",
            font=('Arial', 10, 'bold')
        ).pack(pady=10)
        
        # Add buttons
        ttk.Button(
            frame,
            text="4 Fields",
            command=lambda: self.handle_choice(dialog, 4)
        ).pack(pady=5)
        
        ttk.Button(
            frame,
            text="6 Fields",
            command=lambda: self.handle_choice(dialog, 6)
        ).pack(pady=5)
    
    def handle_choice(self, dialog, num_fields):
        dialog.destroy()  # Close the dialog
        self.create_form(num_fields)  # Open the form
        
    def save_form_data(self, popup):
        # Save the entered data to local variables
        self.values = [var.get() for var in self.string_vars]
        
        # Print the saved values (for demonstration)
        print("\nSaved values:")
        for i, value in enumerate(self.values, 1):
            print(f"Value {i}: {value}")
        
        # Show confirmation
        messagebox.showinfo(
            "Success",
            f"Saved {len(self.values)} values successfully!"
        )
        
        # Close the form
        popup.destroy()
        
    def create_form(self, num_fields):
        # Create popup window for form
        popup = tk.Toplevel(self.root)
        popup.title(f"Enter {num_fields} Values")
        
        # Position popup window near the main button
        button_x = self.main_button.winfo_rootx()
        button_y = self.main_button.winfo_rooty()
        popup.geometry(f"+{button_x}+{button_y + 50}")
        
        # Create frame for form
        form_frame = ttk.Frame(popup, padding="20")
        form_frame.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))
        
        # Create StringVars for the fields
        self.string_vars = [tk.StringVar() for _ in range(num_fields)]
        
        # Create and place form elements
        for i in range(num_fields):
            ttk.Label(
                form_frame,
                text=f"Field {i+1}:"
            ).grid(row=i, column=0, sticky=tk.W, pady=5)
            
            ttk.Entry(
                form_frame,
                textvariable=self.string_vars[i]
            ).grid(row=i, column=1, padx=5, pady=5)
        
        # Add buttons frame
        button_frame = ttk.Frame(form_frame)
        button_frame.grid(row=num_fields, column=0, columnspan=2, pady=20)
        
        # Save button
        ttk.Button(
            button_frame,
            text="Save",
            command=lambda: self.save_form_data(popup)
        ).pack(side=tk.LEFT, padx=5)
        
        # Clear button
        ttk.Button(
            button_frame,
            text="Clear",
            command=lambda: [var.set('') for var in self.string_vars]
        ).pack(side=tk.LEFT, padx=5)

# Create and run the application
if __name__ == "__main__":
    root = tk.Tk()
    root.geometry("200x100")  # Set size for main window
    app = DynamicFormApp(root)
    root.mainloop()