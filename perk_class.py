from typing import Dict, List, Optional, Any
from dataclasses import dataclass

@dataclass
class Perk:
    """Class generated from JSON data."""

    AdditionalCurrency: int = 0
    AdditionalShards: int = 0
    AuraCurseBonus: str = '<bleed>'
    AuraCurseBonusValue: int = 0
    CardClass: str = ''
    CustomDescription: str = ''
    DamageFlatBonus: str = 'None'
    DamageFlatBonusValue: int = 0
    EnergyBegin: int = 0
    HealQuantity: int = 0
    Icon: str = '<bleed>'
    IconTextValue: str = '1'
    ID: str = ''
    Level: int = 0
    MainPerk: bool = True
    MaxHealth: int = 0
    ObeliskPerk: bool = False
    ResistModified: str = 'None'
    ResistModifiedValue: int = 0
    Row: int = 0
    SpeedQuantity: int = 0
    
    def map_dict_to_obj(self, dict):
        for k, v in dict.items():
            setattr(self, k, v)
