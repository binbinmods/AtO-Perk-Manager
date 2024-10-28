from typing import Dict, List, Optional, Any
from dataclasses import dataclass, field

@dataclass
class PerkNode:
    """Class generated from JSON data."""

    Column: int = 1
    Cost: str = 'PerkCostBase'
    ID: str = 'nodeperkBleed0'
    LockedInTown: bool = False
    NotStack: bool = False
    Perk: str = 'mainperkbleed0'
    PerkRequired: str = ''
    PerksConnected: List[Any] = field(default_factory=lambda: [])
    Row: int = 1
    Sprite: str = 'bleed'
    Type: int = 1

    def map_dict_to_obj(self, dict):
        for k, v in dict.items():
            setattr(self, k, v)
