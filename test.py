import tkinter as tk

class ButtonGridApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Button Grid Generator")
        
        # Create main button
        self.main_button = tk.Button(root, text="Generate Button Grid", command=self.create_popup)
        self.main_button.pack(pady=20)
        
    def button_click(self, row, col):
        """Handle clicks on grid buttons"""
        print(f"Button clicked at row {row}, column {col}")
        
    def create_popup(self):
        # Create popup window
        popup = tk.Toplevel(self.root)
        popup.title("Button Grid")
        
        # Position the popup window below the main button
        button_x = self.main_button.winfo_rootx()
        button_y = self.main_button.winfo_rooty()
        popup.geometry(f"+{button_x}+{button_y + 50}")
        
        # Create frame to hold the button grid
        frame = tk.Frame(popup)
        frame.pack(padx=20, pady=20)
        
        # Button properties
        button_width = 10
        button_height = 2
        
        # Create 2x3 grid of buttons
        for row in range(2):
            for col in range(3):
                button = tk.Button(
                    frame,
                    text=f"Button {row},{col}",
                    width=button_width,
                    height=button_height,
                    command=lambda r=row, c=col: self.button_click(r, c)
                )
                # Use grid geometry manager to place buttons
                button.grid(
                    row=row, 
                    column=col, 
                    padx=5,  # Horizontal spacing between buttons
                    pady=5   # Vertical spacing between buttons
                )

# Create and run the application
if __name__ == "__main__":
    root = tk.Tk()
    root.geometry("200x100")  # Set size for main window
    app = ButtonGridApp(root)
    root.mainloop()