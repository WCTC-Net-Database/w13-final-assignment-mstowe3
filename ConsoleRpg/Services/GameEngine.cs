﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using ConsoleRpg.Helpers;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models.Abilities.PlayerAbilities;
using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Characters;
using ConsoleRpgEntities.Models.Characters.Monsters;
using ConsoleRpgEntities.Models.Equipments;
using ConsoleRpgEntities.Models.Rooms;
using ConsoleRpgEntities.Services;
using Spectre.Console;

namespace ConsoleRpg.Services;

public class GameEngine
{
    private GameContext _context;
    private readonly MenuManager _menuManager;
    private readonly MapManager _mapManager;
    private readonly PlayerService _playerService;
    private readonly MonsterService _monsterService;
    private readonly OutputManager _outputManager;
    private Table _logTable;
    private Panel _mapPanel;

    private Player _player;

    public object EditCharacterRequested { get; internal set; }

    public GameEngine(GameContext context, MenuManager menuManager, MapManager mapManager, PlayerService playerService, OutputManager outputManager, MonsterService monsterService)
    {
        _menuManager = menuManager;
        _mapManager = mapManager;
        _playerService = playerService;
        _monsterService = monsterService;
        _outputManager = outputManager;
        _context = context;
        _menuManager = new MenuManager(_outputManager);
    }

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
        {
            StartGame();
        }
    }

    private void GameLoop()
    {
        while (true)
        {
            _outputManager.AddLogEntry("=-=-=-= Main Menu =-=-=-=");
            _outputManager.AddLogEntry("N/E/S/W. Navigate");
            _outputManager.AddLogEntry("1. Attack");
            _outputManager.AddLogEntry("2. Manage Characters");
            _outputManager.AddLogEntry("3. Manage Rooms");
            _outputManager.AddLogEntry("4. Swap Character");
            _outputManager.AddLogEntry("5. Admin Options");
            _outputManager.AddLogEntry("6. Quit");
            var input = _outputManager.GetUserInput("Choose an action:");

            switch (input.ToUpper())
            {
                case "N":
                case "S":
                case "E":
                case "W":
                    Navigate(input.ToUpper());
                    _mapManager.DisplayMap();
                    break;
                case "1":
                    AttackCharacter();
                    break;
                case "2":
                    ManageCharacters(); 
                    break;
                case "3":
                    ManageRooms();
                    break;
                case "4":
                    SwapCharacter();
                    break;
                case "5":
                    AdminOptions();
                    break;
                case "6":
                    _outputManager.AddLogEntry("Exiting game...");
                    Environment.Exit(0);
                    break;
                default:
                    _outputManager.AddLogEntry("Invalid selection. Please choose a valid option.");
                    break;
            }
        }
    }
    
    private void SwapCharacter()
{
    _outputManager.AddLogEntry("=-=-=-= Select Your Character =-=-=-=");
    var characters = _context.Players.ToList(); // Fetch the characters as a list

    if (!characters.Any())
    {
        _outputManager.AddLogEntry("No characters available for selection.", ConsoleColor.Red);
        return;
    }

    foreach (var character in characters)
    {
        _outputManager.AddLogEntry($"ID: {character.Id}, Name: {character.Name}", ConsoleColor.Cyan);
    }

    var input = _outputManager.GetUserInput("Selection:");
    try
    {
        var id = int.Parse(input);
        var character = characters.FirstOrDefault(player => player.Id == id);
        if (character != null)
        {
            _player = character;
            _mapManager._currentRoom = _player.Room;
            _mapManager.DisplayMap();
            _outputManager.AddLogEntry($"You are now playing as {_player.Name}.");
        }
        else
        {
            _outputManager.AddLogEntry("Invalid input: Please select a listed character's ID.");
        }
    }
    catch (FormatException)
    {
        _outputManager.AddLogEntry("Invalid input: Please enter a valid integer for Character Selection.");
    }
}


    public void ManageCharacters()
    {
        _outputManager.AddLogEntry("=-=-=-= Character Management =-=-=-=");
        _outputManager.AddLogEntry("1. Display All Characters");
        _outputManager.AddLogEntry("2. Character Search (Details & Abilities)");
        _outputManager.AddLogEntry("3. Add Character");
        _outputManager.AddLogEntry("4. Edit Character");
        _outputManager.AddLogEntry("5. Add Ability");
        _outputManager.AddLogEntry("6. Back");
        var input = _outputManager.GetUserInput("Choose an action:");

        switch (input)
        {
            case "1":
                DisplayCharacters();
                break;
            case "2":
                SearchCharacters();
                break;
            case "3":
                AddCharacter();
                break;
            case "4":
                EditCharacter();
                break;
            case "5":
                AddAbility();
                break;
            case "6":
                break;
            default:
                _outputManager.AddLogEntry("Invalid selection. Please choose a valid action.");
                break;
        }
    }

    public void ManageRooms()
    {
        _outputManager.AddLogEntry("=-=-=-= Room Management =-=-=-=");
        _outputManager.AddLogEntry("1. Current Room Information");
        _outputManager.AddLogEntry("2. Room Search");
        _outputManager.AddLogEntry("3. Add Room");
        _outputManager.AddLogEntry("4. Back");
        var input = _outputManager.GetUserInput("Choose an action:");

        switch (input)
        {
            case "1":
                GetCurrentRoomData();
                break;
            case "2":
                SearchRooms();
                break;
            case "3":
                AddRoom();
                break;
            case "4":
                break;
            default:
                _outputManager.AddLogEntry("Invalid selection. Please choose a valid action.");
                break;
        }
    }
    public void AddCharacter()
    {
        _outputManager.AddLogEntry("1. Add Player");
        _outputManager.AddLogEntry("2. Add Monster");
        _outputManager.AddLogEntry("3. Back");
        var input = _outputManager.GetUserInput("Choose an action:");

        switch (input)
        {
            case "1":
                AddPlayer();
                break;
            case "2":
                AddMonster();
                break;
            case "3":
                ManageCharacters();
                break;
            default:
                _outputManager.AddLogEntry("Invalid selection. Please choose a valid action.");
                break;
        }
    }
    public void AddPlayer()
    {
        var roomName = _outputManager.GetUserInput("Enter exact room name for the player to reside in:");

        // Find the room by using LINQ
        var targetroom = _context.Rooms.FirstOrDefault(obj => obj.Name.ToUpper() == roomName.ToUpper());

        if (targetroom == null)
        {
            // If the room doesn't exist, return
            _outputManager.AddLogEntry("Room not found.");
            return;
        }
        else
        {
            // If the room exists, create a character to add to the room
            var name = _outputManager.GetUserInput("Enter Player Name:");

            var xpInput = _outputManager.GetUserInput("Enter Player XP:");
            var hpInput = _outputManager.GetUserInput("Enter Player HP:");
            try
            {
                var xp = int.Parse(xpInput);
                var hp = int.Parse(hpInput);

                ICollection<Ability> abilities = new Collection<Ability>();

                var character = new Player
                {
                    Name = name,
                    Health = hp,
                    Experience = xp,
                    Room = targetroom,
                    Equipment = GetUniqueWeapon(),
                    Abilities = abilities
                };

                _context.Players.Add(character);

                targetroom.Players.Add(character);

                // Save changes to database
                _context.SaveChanges();

                _outputManager.AddLogEntry($"Player '{name}' added to the game.");
                _outputManager.AddLogEntry("Navigate add abilties for this character to learn new abilities.");
                _outputManager.AddLogEntry("Navigate to character editing to make changes.");
            }
            catch (FormatException)
            {
                    _outputManager.AddLogEntry("Invalid input: Please enter an integer for XP and HP.");
                    return;
            }
        }
    }
    private Equipment GetUniqueWeapon()
    {
        var equipment = new Equipment();
        Random rnd = new Random();
        var idsInEquipment = _context.Equipments.Select(x => x.WeaponId);
        var weapons = _context.Items.Where(item => item.Type == "Weapon");
        var weaponsNotInEquipment = weapons.Where(item => !(idsInEquipment.Contains(item.Id))).ToHashSet();

        int r = rnd.Next(weaponsNotInEquipment.Count); // Selects a random weapon not already in a character's equipment.
        var item = weaponsNotInEquipment.ElementAt(r);
        if (item != null)
        {
            equipment.WeaponId = item.Id;
            _outputManager.AddLogEntry("New player was given the random weapon: " + item.Name);
            return equipment;
        }
        _outputManager.AddLogEntry("New player does not have a weapon.");
        return equipment;
    }
    public void AddMonster()
    {
        var roomName = _outputManager.GetUserInput("Enter exact room name for the monster to reside in:");

        // Find the room by using LINQ
        var targetroom = _context.Rooms.FirstOrDefault(obj => obj.Name.ToUpper() == roomName.ToUpper());

        if (targetroom == null)
        {
            // If the room doesn't exist, return
            _outputManager.AddLogEntry("Room not found.");
            return;
        }
        else
        {
            // If the room exists, create a character to add to the room
            var name = _outputManager.GetUserInput("Enter Monster Name:");

            var aggroInput = _outputManager.GetUserInput("Enter Monster Aggression:");
            var hpInput = _outputManager.GetUserInput("Enter Monster HP:");
            var type = _outputManager.GetUserInput("Enter Monster Type:");
            try
            {
                var aggro = int.Parse(aggroInput);
                var hp = int.Parse(hpInput);

                switch (type.ToLower())
                {
                case "goblin":
                    {
                        var character = new Goblin
                        {
                            Name = name,
                            AggressionLevel = aggro,
                            Health = hp,
                            Sneakiness = AddIntegerProperty("Goblin", "Sneakiness")
                        };
                        _context.Monsters.Add(character);
                        targetroom.Monsters.Add(character);
                        _context.SaveChanges();
                        _outputManager.AddLogEntry("Monster '" + name + "' added to " + targetroom.Name + ".");
                        _outputManager.AddLogEntry("Navigate to character editing to make changes.");
                        break;
                    }
                        // Would add cases for new monster types
                        // Might implement better system if there were lots and lots of types later on
                default:
                    _outputManager.AddLogEntry("Monster type does not exist.");
                    break;
                }
            }
            catch (FormatException)
            {
                _outputManager.AddLogEntry("Invalid input: Please enter an integer for XP and HP.");
                return;
            }
        }
    }
    private int AddIntegerProperty(string objectName, string property)
    {
        while (true)
        {
            var input = _outputManager.GetUserInput("Enter " + objectName + " " + property + ":");
            try
            { 
                var inputParsed = int.Parse(input);
                return inputParsed;
            }
            catch (FormatException)
            {
                _outputManager.AddLogEntry("Invalid input: Please enter an integer for " + property + ".");
            }
        }
    }

    public void EditCharacter()
    {
        _outputManager.AddLogEntry("1. Edit a Player");
        _outputManager.AddLogEntry("2. Edit a Monster");
        _outputManager.AddLogEntry("3. Back");
        var input = _outputManager.GetUserInput("Choose an action:");

        switch (input)
        {
            case "1":
                EditPlayer();
                break;
            case "2":
                EditMonster();
                break;
            case "3":
                ManageCharacters(); 
                break;
            default:
                _outputManager.AddLogEntry("Invalid selection. Please choose a valid action.");
                break;
        }
    }
    private void EditPlayer()
    {
        var input = _outputManager.GetUserInput("Search Player Name:");
        var playercharacter = _context.Players.FirstOrDefault(player => player.Name.ToUpper() == (input.ToUpper()));
        if (playercharacter != null)
        {
            bool editing = true;
            while (editing)
            {
                _outputManager.AddLogEntry("Editing: " + playercharacter.Id + ". " + playercharacter.Name);
                _outputManager.AddLogEntry("1. Edit Name");
                _outputManager.AddLogEntry("2. Edit Experience");
                _outputManager.AddLogEntry("3. Edit Health");
                _outputManager.AddLogEntry("4. Edit Room");
                _outputManager.AddLogEntry("5. Back");
                var edit = _outputManager.GetUserInput("Choose an action:");

                switch (edit)
                {
                    case "1":
                        var name = _outputManager.GetUserInput("New Name:");
                        playercharacter.Name = name;
                        break;
                    case "2":
                        var expInput = _outputManager.GetUserInput("New Experience:");
                        try
                        {
                            var exp = int.Parse(expInput);
                            playercharacter.Experience = exp;
                        }
                        catch (FormatException)
                        {
                            _outputManager.AddLogEntry("Invalid input: Please enter an integer.");
                        }
                        break;
                    case "3":
                        var hpInput = _outputManager.GetUserInput("New Health:");
                        try
                        {
                            var hp = int.Parse(hpInput);
                            playercharacter.Health = hp;
                        }
                        catch (FormatException)
                        {
                            _outputManager.AddLogEntry("Invalid input: Please enter an integer.");
                        }
                        break;
                    case "4":
                        var roomInput = _outputManager.GetUserInput("Exact Name of Room:");
                        var room = _context.Rooms.FirstOrDefault(room => room.Name.ToUpper() == (roomInput.ToUpper()));
                        if (room == null)
                            _outputManager.AddLogEntry("Room not found.");
                        else
                        {
                            var oldRoom = playercharacter.Room;
                            playercharacter.Room = room;
                            playercharacter.RoomId = room.Id;
                            oldRoom?.Players.Remove(playercharacter);
                            room.Players.Add(playercharacter);
                        }
                        break;
                    case "5":
                        _context.SaveChanges();
                        editing = false;
                        EditCharacter();
                        break;
                    default:
                        _outputManager.AddLogEntry("Invalid selection. Changes saved.");
                        _context.SaveChanges();
                        editing = false;
                        EditCharacter();
                        break;
                }
            }
        }
        else
        {
            _outputManager.AddLogEntry("Player not found.");
        }    
    }
    private void EditMonster()
    {
        var input = _outputManager.GetUserInput("Search Monster Name:");
        var monstercharacter = _context.Monsters.FirstOrDefault(monster => monster.Name.ToUpper() == (input.ToUpper()));
        if (monstercharacter != null)
        {
            bool editing = true;
            while (editing)
            {
                _outputManager.AddLogEntry("Editing: " + monstercharacter.Id + ". " + monstercharacter.Name);
                _outputManager.AddLogEntry("1. Edit Name");
                _outputManager.AddLogEntry("2. Edit Aggression Level");
                _outputManager.AddLogEntry("3. Edit Health");
                _outputManager.AddLogEntry("4. Edit Room");
                _outputManager.AddLogEntry("5. Back");
                var edit = _outputManager.GetUserInput("Choose an action:");

                switch (edit)
                {
                    case "1":
                        var name = _outputManager.GetUserInput("New Name:");
                        monstercharacter.Name = name;
                        break;
                    case "2":
                        var aggroInput = _outputManager.GetUserInput("New Aggression Level:");
                        try
                        {
                            var aggro = int.Parse(aggroInput);
                            monstercharacter.AggressionLevel = aggro;
                        }
                        catch (FormatException)
                        {
                            _outputManager.AddLogEntry("Invalid input: Please enter an integer.");
                        }
                        break;
                    case "3":
                        var hpInput = _outputManager.GetUserInput("New Health:");
                        try
                        {
                            var hp = int.Parse(hpInput);
                            monstercharacter.Health = hp;
                        }
                        catch (FormatException)
                        {
                            _outputManager.AddLogEntry("Invalid input: Please enter an integer.");
                        }
                        break;
                    case "4":
                        var roomInput = _outputManager.GetUserInput("Exact Name of Room:");
                        var room = _context.Rooms.FirstOrDefault(room => room.Name.ToUpper() == (roomInput.ToUpper()));
                        if (room == null)
                            _outputManager.AddLogEntry("Room not found.");
                        else
                        {
                            var oldRoom = monstercharacter.Room;
                            monstercharacter.Room = room;
                            monstercharacter.RoomId = room.Id;
                            oldRoom?.Monsters.Remove(monstercharacter);
                            room.Monsters.Add(monstercharacter);
                        }
                        break;
                    case "5":
                        _context.SaveChanges();
                        editing = false;
                        EditCharacter();
                        break;
                    default:
                        _outputManager.AddLogEntry("Invalid selection. Please choose a valid action.");
                        break;
                }
            }
        }
        else
        {
            _outputManager.AddLogEntry("Monster not found.");
        }
    }
    private void DisplayCharacters()
    {
        var playercharacters = _context.Players;
        var monstercharacters = _context.Monsters;
        _outputManager.AddLogEntry("=-=-=-=-= Player Characters =-=-=-=-=");
        foreach (var character in playercharacters)
        {
            _outputManager.AddLogEntry(character.Name + " | " + character.Room.Name ?? "None");
        }
        _outputManager.GetUserInput("Press Enter to Continue");
        _outputManager.AddLogEntry("=-=-=-=-= Monster Characters =-=-=-=-=");
        foreach (var character in monstercharacters)
        {
            var room = "None";
            if (character.Room == null)
            {
                room = "Defeated";
            }
            else
            {
                room = character.Room.Name;
            }
            _outputManager.AddLogEntry(character.Name + " | " + room);
        }
        _outputManager.GetUserInput("Press Enter to Continue");
        _outputManager.AddLogEntry("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");

    }
    private void SearchCharacters()
    {
        var input = _outputManager.GetUserInput("Search Character Name:");
        var playercharacters = _context.Players.Where(player => player.Name.ToUpper().Contains(input.ToUpper()));
        var monstercharacters = _context.Monsters.Where(monster => monster.Name.ToUpper().Contains(input.ToUpper()));
        if (playercharacters.Any())
        {
            _outputManager.AddLogEntry("=-=-=-=-= Player Characters =-=-=-=-=");
            foreach (var character in playercharacters)
            {
                _outputManager.AddLogEntry("Name: " + character.Name + " - HP: " + character.Health.ToString() + " - XP: " + character.Experience.ToString());
                if (character.Abilities.Any())
                { 
                    _outputManager.AddLogEntry("Abilities:");
                    foreach (Ability ability in character.Abilities)
                    {
                        string[] exclusionList = {"Players", "Id", "LazyLoader"};
                        StringBuilder sb = new StringBuilder();
                        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(ability))
                        {
                            string name = descriptor.Name;
                            
                            if (!exclusionList.Contains(name))
                            {
                                object value = descriptor.GetValue(ability);
                                sb.Append(String.Format(" - {0}: {1}", name, value));
                            }
                        }
                        _outputManager.AddLogEntry(sb.ToString());
                    }
                }
            }
            _outputManager.GetUserInput("Press Enter to Continue");
        }
        else
        {
            _outputManager.AddLogEntry("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            _outputManager.AddLogEntry("No Player characters matched the search.");
        }
        if (monstercharacters.Any())
        {
            _outputManager.AddLogEntry("=-=-=-=-= Monster Characters =-=-=-=-=");
            foreach (var character in monstercharacters)
            {
                _outputManager.AddLogEntry("Name: " + character.Name + " - HP: " + character.Health.ToString() + " - Aggression: " + character.AggressionLevel.ToString() + " - Type: " + character.MonsterType);
            }
            _outputManager.GetUserInput("Press Enter to Continue");
            _outputManager.AddLogEntry("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
        }
        else
        {
            _outputManager.AddLogEntry("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            _outputManager.AddLogEntry("No Monster characters matched the search.");
            _outputManager.AddLogEntry("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
        }
    }

    public void AddAbility()
    {
        var input = _outputManager.GetUserInput("Search Player Name to add Ability:");
        var playercharacter = _context.Players.FirstOrDefault(player => player.Name.ToUpper() == (input.ToUpper()));
        if (playercharacter != null)
        {
            var name = _outputManager.GetUserInput("Enter Ability Name:");
            var description = _outputManager.GetUserInput("Enter Ability Description:");
            var type = _outputManager.GetUserInput("Enter Ability Type:");
            var damage = AddIntegerProperty("Ability", "damage");
            switch (type.ToLower())
            {
                case "shove":
                case "shoveability":
                    {
                        var distance = AddIntegerProperty("Ability", "distance");
                        _outputManager.AddLogEntry($"Name: {name}");
                        _outputManager.AddLogEntry($"Desc: {description}");
                        _outputManager.AddLogEntry($"Type: {type}");
                        _outputManager.AddLogEntry($"Damage: {damage.ToString()}");
                        _outputManager.AddLogEntry($"Damage: {distance.ToString()}");
                        var confirm = _outputManager.GetUserInput("Does this look right? Y/N:");
                        if (confirm.ToUpper() == "Y")
                        {
                            var ability = new ShoveAbility
                            {
                                Name = name,
                                Description = description,
                                AbilityType = "ShoveAbility",
                                Damage = damage,
                                Distance = distance

                            };
                            playercharacter.Abilities.Add(ability);
                            _context.SaveChanges();
                            _outputManager.AddLogEntry(playercharacter.Name + " learned " + name + ".");
                        }
                        else
                        {
                            _outputManager.AddLogEntry("Add ability cancelled.");
                        }
                        break;
                    }
                case "smash":
                case "smashability":
                    {
                        _outputManager.AddLogEntry($"Name: {name}");
                        _outputManager.AddLogEntry($"Desc: {description}");
                        _outputManager.AddLogEntry($"Type: {type}");
                        _outputManager.AddLogEntry($"Damage: {damage.ToString()}");
                        var confirm = _outputManager.GetUserInput("Does this look right? Y/N:");
                        if (confirm.ToUpper() == "Y")
                        {
                            var ability = new SmashAbility
                            {
                                Name = name,
                                Description = description,
                                AbilityType = "SmashAbility",
                                Damage = damage,
                            };
                            playercharacter.Abilities.Add(ability);
                            _context.SaveChanges();
                            _outputManager.AddLogEntry(playercharacter.Name + " learned " + name + ".");
                        }
                        else
                        {
                            _outputManager.AddLogEntry("Add ability cancelled.");
                        }
                        break;
                    }
                default:
                    {
                        _outputManager.AddLogEntry("Ability type not found.");
                        break;
                    }
            }
        }
        else
        {
            _outputManager.AddLogEntry("Player not found.");
        }
    }

    private void GetCurrentRoomData()
    {
        var currentroom = _mapManager._currentRoom;
        _outputManager.AddLogEntry("=-=-=-=-= Current Room =-=-=-=-=");
        _outputManager.AddLogEntry("Room: " + currentroom.Name);
        _outputManager.AddLogEntry("Description: " + currentroom.Description);
        _outputManager.GetUserInput("Press Enter to Continue");
        GetRoomCharacters(currentroom);
        _outputManager.GetUserInput("Press Enter to Continue");
        _outputManager.AddLogEntry("Adjacent Rooms:");
        if (currentroom.North != null)
            _outputManager.AddLogEntry("- North: " + (currentroom.North.Name ?? "None"));
        if (currentroom.East != null)
            _outputManager.AddLogEntry("- East: " + (currentroom.East.Name ?? "None"));
        if (currentroom.South != null)
            _outputManager.AddLogEntry("- South: " + (currentroom.South.Name ?? "None"));
        if (currentroom.West != null)
            _outputManager.AddLogEntry("- West: " + (currentroom.West.Name ?? "None"));
        _outputManager.GetUserInput("Press Enter to Continue");
    }
    public void GetRoomCharacters(Room room)
    {
        var players = room.Players;
        var monsters = room.Monsters;
        _outputManager.AddLogEntry("Characters:");
        if (players.Any())
        {
            _outputManager.AddLogEntry("Players: ");
            foreach (Player character in players)
            {
                _outputManager.AddLogEntry(" - Name: " + character.Name + " - HP: " + character.Health.ToString() + " - XP: " + character.Experience.ToString());
            }
        }
        else
        {
            _outputManager.AddLogEntry("No Players in this Room.");
        }
        if (monsters.Any())
        {
            _outputManager.AddLogEntry("Monsters: ");
            foreach (Monster character in monsters)
            {
                _outputManager.AddLogEntry(" - Name: " + character.Name + " - HP: " + character.Health.ToString() + " - Aggression: " + character.AggressionLevel.ToString() + " - Type: " + character.MonsterType);
            }
        }
        else
        {
            _outputManager.AddLogEntry("No Monsters in this Room.");
        }
    }
    private void SearchRooms()
    {
        var input = _outputManager.GetUserInput("Search Room Name:");
        var rooms = _context.Rooms.Where(room => room.Name.ToUpper().Contains(input.ToUpper()));
        if (rooms.Any())
        {
            foreach (Room room in rooms)
            {
                _outputManager.AddLogEntry("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
                _outputManager.AddLogEntry("Room: " + room.Name);
                _outputManager.AddLogEntry("Description: " + room.Description);
                GetRoomCharacters(room);
                _outputManager.GetUserInput("Press Enter to Continue");
            }
            _outputManager.AddLogEntry("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
        }
        else
        {
            _outputManager.AddLogEntry("No Rooms were found.");
        }
    }

    private void AddRoom()
    {
        var name = _outputManager.GetUserInput("Enter Room Name:");
        var description = _outputManager.GetUserInput("Enter Room Description:");
        _outputManager.AddLogEntry($"Name : {name}");
        _outputManager.AddLogEntry($"Description : {description}");
        var confirm = _outputManager.GetUserInput("Does this look right? Y/N:");
        if (confirm.ToUpper() == "Y")
        {
            while (true)
            {
                Room newRoom = null;
                var direction = _outputManager.GetUserInput("Enter expansion direction (N/E/S/W):");
                var connectionName = _outputManager.GetUserInput("Enter Room Name to expand from:");

                var connectionRoom = _context.Rooms.FirstOrDefault(room => room.Name.ToUpper() == connectionName.ToUpper());
                if (connectionRoom != null)
                {
                    switch (direction.ToUpper())
                    {
                        case "N":
                            if (connectionRoom.NorthId == null)
                            {
                                newRoom = new Room { Name = name, Description = description, SouthId = connectionRoom.Id };
                                _context.Rooms.Add(newRoom);
                                _context.SaveChanges();
                                newRoom = _context.Rooms.FirstOrDefault(room => room.Name.ToUpper() == name.ToUpper());

                                connectionRoom.NorthId = newRoom.Id;
                            }
                            else
                            {
                                _outputManager.AddLogEntry(connectionRoom.Name + " already has a North room.");
                            }
                            break;
                        case "S":
                            if (connectionRoom.SouthId == null)
                            {
                                newRoom = new Room { Name = name, Description = description, NorthId = connectionRoom.Id };
                                _context.Rooms.Add(newRoom);
                                _context.SaveChanges();
                                newRoom = _context.Rooms.FirstOrDefault(room => room.Name.ToUpper() == name.ToUpper());

                                connectionRoom.SouthId = newRoom.Id;
                            }
                            else
                            {
                                _outputManager.AddLogEntry(connectionRoom.Name + " already has a South room.");
                            }
                            break;
                        case "E":
                            if (connectionRoom.EastId == null)
                            {
                                newRoom = new Room { Name = name, Description = description, WestId = connectionRoom.Id };
                                _context.Rooms.Add(newRoom);
                                _context.SaveChanges();
                                newRoom = _context.Rooms.FirstOrDefault(room => room.Name.ToUpper() == name.ToUpper());

                                connectionRoom.EastId = newRoom.Id;
                            }
                            else
                            {
                                _outputManager.AddLogEntry(connectionRoom.Name + " already has a East room.");
                            }
                            break;
                        case "W":
                            if (connectionRoom.WestId == null)
                            {
                                newRoom = new Room { Name = name, Description = description, EastId = connectionRoom.Id };
                                _context.Rooms.Add(newRoom);
                                _context.SaveChanges();
                                newRoom = _context.Rooms.FirstOrDefault(room => room.Name.ToUpper() == name.ToUpper());

                                connectionRoom.WestId = newRoom.Id;
                            }
                            else
                            {
                                _outputManager.AddLogEntry(connectionRoom.Name + " already has a West room.");
                            }
                            break;
                        default:
                            _outputManager.AddLogEntry("Select a valid direction.");
                            break;
                    }
                    if (newRoom != null)
                    {
                        newRoom.Players = new List<Player>();
                        newRoom.Monsters = new List<Monster>();
                        _context.SaveChanges();
                        _outputManager.AddLogEntry($"Room '{newRoom.Name}' added to the game.");
                        _mapManager.DisplayMap();
                        break;
                    }
                }
                else
                {
                    _outputManager.AddLogEntry("Connection Room not found.");
                }
            }
        }
    }

    private void Navigate(string direction)
    {
        MapManager map = _mapManager;
        switch (direction)
        {
            case "N":
                if (map._currentRoom.North != null )
                {
                    map._currentRoom.Players.Remove(_player);
                    map._currentRoom = map._currentRoom.North;
                    _player.Room = map._currentRoom;
                    _player.RoomId = map._currentRoom.Id;
                    map._currentRoom.Players.Add(_player);
                }
                else
                {
                    _outputManager.AddLogEntry("There's nowhere to go North of here.");
                }
                break;
            case "S":
                if (map._currentRoom.South != null)
                {
                    map._currentRoom.Players.Remove(_player);
                    map._currentRoom = map._currentRoom.South;
                    _player.Room = map._currentRoom;
                    _player.RoomId = map._currentRoom.Id;
                    map._currentRoom.Players.Add(_player);
                }
                else
                {
                    _outputManager.AddLogEntry("There's nowhere to go South of here.");
                }
                break;
            case "E":
                if (map._currentRoom.East != null)
                {
                    map._currentRoom.Players.Remove(_player);
                    map._currentRoom = map._currentRoom.East;
                    _player.Room = map._currentRoom;
                    _player.RoomId = map._currentRoom.Id;
                    map._currentRoom.Players.Add(_player);
                }
                else
                {
                    _outputManager.AddLogEntry("There's nowhere to go East of here.");
                }
                break;
            case "W":
                if (map._currentRoom.West != null)
                {
                    map._currentRoom.Players.Remove(_player);
                    map._currentRoom = map._currentRoom.West;
                    _player.Room = map._currentRoom;
                    _player.RoomId = map._currentRoom.Id;
                    map._currentRoom.Players.Add(_player);
                }
                else
                {
                    _outputManager.AddLogEntry("There's nowhere to go West of here.");
                }
                break;
        }
        _context.SaveChanges();
    }

    private void AdminOptions()
    {
        _outputManager.AddLogEntry("=-=-=-= Admin Options =-=-=-=");
        _outputManager.AddLogEntry("1. Current Room Characters by Attribute");
        _outputManager.AddLogEntry("2. Characters by Room");
        _outputManager.AddLogEntry("3. Locate Equipment");
        _outputManager.AddLogEntry("4. Back");
        var input = _outputManager.GetUserInput("Choose an action:");

        switch (input)
        {
            case "1":
                ListRoomCharacters();
                break;
            case "2":
                AllCharacterRooms();
                break;
            case "3":
                FindEquipment();
                break;
            case "4":
                break;
            default:
                _outputManager.AddLogEntry("Invalid selection. Please choose a valid action.");
                break;
        }
    }
    private void ListRoomCharacters()
    {
        var players = _player.Room.Players;
        var monsters = _player.Room.Monsters;
        _outputManager.AddLogEntry("1. List by Name");
        _outputManager.AddLogEntry("2. List by Health");
        _outputManager.AddLogEntry("3. Back");
        var input = _outputManager.GetUserInput("Choose an action:");

        switch (input)
        {
            case "1":
                // Need to due a workaround here, LINQ wouldn't be easier due to players and monsters being seperate.
                _outputManager.AddLogEntry("=-=-= This room's Characters sorted by Name =-=-=");
                List<string> names = new List<string>();
                foreach (Player player in players) { names.Add(player.Name); }
                foreach (Monster monster in monsters) { names.Add(monster.Name); }
                names.Sort();
                foreach (string name in names)
                {
                    _outputManager.AddLogEntry(" - " + name);
                }
                _outputManager.AddLogEntry("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
                _outputManager.GetUserInput("Press Enter to Continue");
                break;
            case "2":
                _outputManager.AddLogEntry("=-=-= This room's Characters sorted by Health =-=-=");
                // Need to due a workaround here, LINQ wouldn't be easier due to players and monsters being seperate.
                Dictionary<string, int> health = new Dictionary<string, int>();
                foreach (Player player in players) { health.Add(player.Name, player.Health); }
                foreach (Monster monster in monsters) { health.Add(monster.Name, monster.Health); }
                var sortedDict = from entry in health orderby entry.Value descending select entry;
                foreach (KeyValuePair<string, int> entry in sortedDict)
                {
                    _outputManager.AddLogEntry(" - " + entry.Key + ": " + entry.Value.ToString());
                }
                _outputManager.AddLogEntry("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
                _outputManager.GetUserInput("Press Enter to Continue");
                break;
            case "3":
                AdminOptions();
                break;
            default:
                _outputManager.AddLogEntry("Invalid selection. Please choose a valid action.");
                break;
        }
    }

    private void AllCharacterRooms()
    {
        //seperating players and monsters so group can be done simply with LINQ.
        _outputManager.AddLogEntry("1. Players");
        _outputManager.AddLogEntry("2. Monsters");
        _outputManager.AddLogEntry("3. Back");
        var input = _outputManager.GetUserInput("Choose an action:");

        switch (input)
        {
            case "1":
                var playerrooms = _context.Players.ToList().GroupBy(x => x.Room);
                foreach (var room in playerrooms)
                {
                    if (room.Key != null)
                    {
                        _outputManager.AddLogEntry("=-=-=-=-= " + room.Key.Name + " =-=-=-=-=");
                        foreach (var player in room)
                        {
                            _outputManager.AddLogEntry(" - " + player.Name);
                        }
                        _outputManager.GetUserInput("Press Enter to Continue");
                    }
                }
                break;
            case "2":
                var monsterrooms = _context.Monsters.ToList().GroupBy(x => x.Room);
                foreach (var room in monsterrooms)
                {
                    if (room.Key != null)
                    {
                        _outputManager.AddLogEntry("=-=-=-=-= " + room.Key.Name + " =-=-=-=-=");
                        foreach (var monster in room)
                        {
                            _outputManager.AddLogEntry(" - " + monster.Name);
                        }
                        _outputManager.GetUserInput("Press Enter to Continue");
                    }
                }
                break;
            case "3":
                AdminOptions();
                break;
            default:
                _outputManager.AddLogEntry("Invalid selection. Please choose a valid action.");
                break;
        }
    }
    private void FindEquipment()
    {
        var players = _context.Players;
        Dictionary<Item, Player> itemLocation = new Dictionary<Item, Player>();
        foreach (Player player in players)
        {
            if (player.Equipment.Weapon != null)
                itemLocation.Add(player.Equipment.Weapon, player);
            if (player.Equipment.Armor != null)
                itemLocation.Add(player.Equipment.Armor, player);
            if (player.Inventory is not null)
            {
                if (player.Inventory.Items is not null)
                {
                    if (player.Inventory.Items.Any())
                    {
                        foreach (Item item in player.Inventory.Items)
                        {
                            itemLocation.Add(item, player);
                        }
                    }
                }
            }
        }
        _outputManager.AddLogEntry("=-=-=-= Equipment Locations =-=-=-=");
        foreach (KeyValuePair<Item, Player> entry in itemLocation)
        {
            _outputManager.AddLogEntry(entry.Key.Name + ": " + entry.Value.Name + " | " + entry.Value.Room.Name);
        }
        _outputManager.GetUserInput("Press Enter to Continue");
    }
    private void AttackCharacter()
    {
        var monsters = _mapManager._currentRoom.Monsters;
        if (monsters.Any())
        {
            var target = monsters.First();
            if (target is ITargetable targetable)
            {
                _playerService.Attack(_player, targetable);
                if (_player.Abilities.Any() && targetable.Health > 0)
                {
                    Random rnd = new Random();
                    int r = rnd.Next(_player.Abilities.Count); // Selects a random ability from the player's abilities
                    _playerService.UseAbility(_player, _player.Abilities.ElementAt(r), targetable);
                }
                if (target.Health > 0)
                {
                    _monsterService.Attack(target, _player);
                }
                else
                {
                    monsters.Remove(target);
                    _outputManager.AddLogEntry(target.Name + " has been defeated.");
                    _player.Experience += 20;
                    _outputManager.AddLogEntry(_player.Name + " gained 20 XP.");
                }
            }
            _context.SaveChanges();
            _outputManager.GetUserInput("Press Enter to Continue");
        }
        else
        {
            _outputManager.AddLogEntry("There's no monsters here.");
        }
    }
    public void StartGame()
{
    _outputManager.AddLogEntry("Starting game...", ConsoleColor.Green);
    while (_player == null) { SwapCharacter(); } 
    // Load monsters into random rooms (if needed) 
    // Load map 
    _mapManager.LoadInitialRoom(_player.Room.Id); 
    _mapManager.DisplayMap(); // Pause before starting the game loop 
    Thread.Sleep(500);
    GameLoop();
}

private void DisplayPlayerSelection()
{
    
    var players = _context.Players.ToList();
    if (!players.Any())
    {
        _outputManager.AddLogEntry("No characters available for selection.", ConsoleColor.Red);
        return;
    }

    _outputManager.AddLogEntry("Select a Character by ID:", ConsoleColor.Yellow);
    foreach (var player in players)
    {
        _outputManager.AddLogEntry($"ID: {player.Id}, Name: {player.Name}", ConsoleColor.Cyan);
    }
    
}


private void HandlePlayerSelection()
{
    var input = _outputManager.GetUserInput("Selection:");
    if (int.TryParse(input, out int playerId))
    {
        var player = _context.Players.FirstOrDefault(p => p.Id == playerId);
        if (player != null)
        {
            _player = player;
            _outputManager.AddLogEntry($"Character '{player.Name}' selected!", ConsoleColor.Green);
            
            // Proceed with the game setup using the selected character
        }
        else
        {
            _outputManager.AddLogEntry("Invalid character ID. Please try again.", ConsoleColor.Red);
            
            DisplayPlayerSelection(); // Redisplay characters for selection
            HandlePlayerSelection(); // Prompt for selection again
        }
    }
    else
    {
        _outputManager.AddLogEntry("Invalid input. Please enter a valid character ID.", ConsoleColor.Red);
        
        DisplayPlayerSelection(); // Redisplay characters for selection
        HandlePlayerSelection(); // Prompt for selection again
    }
}



}