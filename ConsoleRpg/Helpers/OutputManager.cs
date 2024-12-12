using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

namespace ConsoleRpg.Helpers
{
    public class OutputManager
    {
        private readonly string _initialMapContent;
        private readonly Layout _layout;
        private readonly Panel _mapPanel;
        private readonly List<string> _logContent;
        private const int MaxLogLines = 15; // Maximum number of visible log lines

        public OutputManager(string initialMapContent = "### Initial Map Area ###")
        {
            _initialMapContent = initialMapContent;
            _logContent = new List<string>();

            // Create the initial map panel
            _mapPanel = new Panel(_initialMapContent)
                .Expand()
                .Border(BoxBorder.Square)
                .Padding(1, 1, 1, 1)
                .Header("Map");

            // Create the layout with two regions
            _layout = new Layout()
                .SplitRows(
                    new Layout("Map").Size(10), // Fixed size for the map area
                    new Layout("Logs"));       // Flexible size for the log area

            // Set the initial content for each region
            _layout["Map"].Update(_mapPanel);
            _layout["Logs"].Update(CreateLogPanel());
        }

        public void Initialize()
        {
            // Render the initial layout
            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(_layout);
        }

        // Overloaded method to add log entry with optional color
        public void AddLogEntry(string logEntry, ConsoleColor color = ConsoleColor.White)
        {
            _logContent.Add(logEntry);
            var visibleLogs = _logContent.Skip(Math.Max(0, _logContent.Count - MaxLogLines)).ToList();
            _layout["Logs"].Update(CreateLogPanel(visibleLogs));
            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(_layout);
        }

        public string GetUserInput(string prompt)
        {
            var visibleLogs = _logContent.Skip(Math.Max(0, _logContent.Count - MaxLogLines)).ToList();
            visibleLogs.Add($"[yellow]{Markup.Escape(prompt)}[/]");
            _layout["Logs"].Update(CreateLogPanel(visibleLogs));
            AnsiConsole.Write(_layout);
            var cursorTop = Console.CursorTop;
            Console.SetCursorPosition(2, cursorTop); // 2 spaces for padding after the border
            Console.Write("> ");
            var userInput = Console.ReadLine()?.Trim() ?? string.Empty;
            AddLogEntry($"[yellow]User Input:[/] {Markup.Escape(userInput)}");
            return userInput;
        }

        public void UpdateMapContent(string newMapContent)
        {
            var updatedMapPanel = new Panel(newMapContent)
                .Expand()
                .Border(BoxBorder.Square)
                .Padding(1, 1, 1, 1)
                .Header("Map");

            _layout["Map"].Update(updatedMapPanel);
            AnsiConsole.Clear();
            AnsiConsole.Write(_layout);
        }

        private Panel CreateLogPanel(IEnumerable<string> logs = null)
        {
            var logText = string.Join("\n", logs ?? _logContent);
            return new Panel(new Markup(logText))
                .Expand()
                .Border(BoxBorder.Square)
                .Padding(1, 1, 1, 1)
                .Header($"Logs ({_logContent.Count})");
        }
    }
}
