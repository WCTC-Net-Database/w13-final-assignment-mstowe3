using ConsoleRpg.Services;
namespace ConsoleRpg.Helpers;

public class MenuManager
{
    private readonly OutputManager _outputManager;

    public MenuManager(OutputManager outputManager)
    {
        _outputManager = outputManager;
    }

    public bool ShowMainMenu()
    {
        _outputManager.AddLogEntry("Welcome to the RPG Game!");
        _outputManager.AddLogEntry("1. Start Game");
        _outputManager.AddLogEntry("0. Exit");

        return HandleMainMenuInput();
    }

    private bool HandleMainMenuInput()
    {
        while (true)
        {
            var input = _outputManager.GetUserInput("Selection:");
            switch (input)
            {
                case "1":
                    _outputManager.AddLogEntry("Starting game...");
                    return true;
                case "0":
                    _outputManager.AddLogEntry("Exiting game...");
                    Environment.Exit(0);
                    return false;
                default:
                    _outputManager.AddLogEntry("Invalid selection. Please choose a valid option.");
                    break;
            }
        }
    }
}

