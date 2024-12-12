﻿using ConsoleRpgEntities.Models.Abilities.PlayerAbilities;
using ConsoleRpgEntities.Models.Equipments;

namespace ConsoleRpgEntities.Models.Characters;

public interface IPlayer
{
    int Id { get; set; }
    string Name { get; set; }
    ICollection<Ability> Abilities { get; set; }
    Inventory Inventory { get; set; }
    Equipment Equipment { get; set; }
    int Health { get; set; }
}
