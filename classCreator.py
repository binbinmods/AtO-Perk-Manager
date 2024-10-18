import json
from typing import Any, Dict
import keyword

def sanitize_identifier(name: str) -> str:
    """Sanitize a string to be a valid Python identifier."""
    # Replace invalid characters with underscores
    sanitized = ''.join(c if c.isalnum() else '_' for c in name)
    # Ensure it doesn't start with a number
    if sanitized[0].isdigit():
        sanitized = f'_{sanitized}'
    # Ensure it's not a Python keyword
    if keyword.iskeyword(sanitized):
        sanitized = f'{sanitized}_'
    return sanitized

def get_type_hint(value: Any) -> str:
    """Get the appropriate type hint for a value."""
    if isinstance(value, bool):
        return 'bool'
    elif isinstance(value, int):
        return 'int'
    elif isinstance(value, float):
        return 'float'
    elif isinstance(value, str):
        return 'str'
    elif isinstance(value, list):
        if value:
            inner_type = get_type_hint(value[0])
            return f'List[{inner_type}]'
        return 'List[Any]'
    elif isinstance(value, dict):
        return 'Dict[str, Any]'
    elif value is None:
        return 'Optional[Any]'
    return 'Any'

def generate_class_code(json_data: Dict[str, Any], class_name: str) -> str:
    """Generate Python code for a class based on JSON data."""
    lines = [
        'from typing import Dict, List, Optional, Any',
        'from dataclasses import dataclass, field',
        '',
        '@dataclass',
        f'class {class_name}:',
        '    """Class generated from JSON data."""',
        ''
    ]
    
    # Generate class properties
    for key, value in json_data.items():
        sanitized_key = sanitize_identifier(key)
        type_hint = get_type_hint(value)
        
        # Add property with type hint and default value
        if value is None:
            lines.append(f'    {sanitized_key}: {type_hint} = None')
        elif type(value) == list:
            lines.append(f'    {sanitized_key}: {type_hint} = field(default_factory=lambda: {repr(value)})')
        else:
            lines.append(f'    {sanitized_key}: {type_hint} = {repr(value)}')
    
    return '\n'.join(lines)

def generate_class_file(input_json_path: str, output_py_path: str, class_name: str):
    try:
        # Read JSON file
        with open(input_json_path, 'r') as file:
            json_data = json.load(file)
        
        if not isinstance(json_data, dict):
            raise ValueError("Root JSON must be an object")
        
        # Generate class code
        class_code = generate_class_code(json_data, class_name)
        
        # Write to Python file
        with open(output_py_path, 'w') as file:
            file.write(class_code)
        
        print(f"Successfully generated class in {output_py_path}")
        return True
    except Exception as e:
        print(f"Error generating class: {str(e)}")
        return False

# Example usage
if __name__ == "__main__":
        
    # Generate Python class file
    generate_class_file("TestPerks/perkNode/nodeperkBleed0.json", "perk_node_class.py", "PerkNode")