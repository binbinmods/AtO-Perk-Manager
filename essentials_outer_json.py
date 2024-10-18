from typing import Dict, List, Optional, Any
from dataclasses import dataclass, field

@dataclass
class LoadingJson:
    """Class generated from JSON data."""

    Name: str = ''
    Author: str = 'binbin'
    Description: str = ''
    Version: str = '1.0.0'
    Date: int = 20241016
    Link: str = 'https://across-the-obelisk.thunderstore.io/'
    Dependencies: List[str] = field(default_factory=lambda: ['stiffmeds-Obeliskial_Content-1.2.2'])
    ContentFolder: str = 'NodeTest'
    Priority: int = 100
    Type: List[str] = field(default_factory=lambda: ['event', 'eventReply', 'eventRequirement', 'node', 'sprite'])
    Comment: str = ''
    Enabled: bool = True