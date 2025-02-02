using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.NativeInterop;
using Array = Godot.Collections.Array;
using Expression = Godot.Expression;

namespace RhythmGame.addons.copper_dc.scripts;

public partial class DebugConsole : CanvasLayer
{
    public interface IMonitorType
    { 
        dynamic Value { get; set; }
    }

    public class MonitorStringType : IMonitorType
    {
        public dynamic Value { get; set; }

        public MonitorStringType(string value)
        {
            Value = value;
        }
    }

    public interface IConsoleLogType
    {
        dynamic Value { get; set; }

        dynamic Get()
        {
            return Value;
        }

        string ToString()
        {
            return Value;
        }
    }

    public class DebugCommandConsoleLogType : IConsoleLogType
    {
        public dynamic Value { get; set; }

        public DebugCommandConsoleLogType(DebugCommand command)
        {
            Value = command;
        }
    }

    public class StringConsoleLogType : IConsoleLogType
    {
        public dynamic Value { get; set; }

        public StringConsoleLogType(string value)
        {
            Value = value;
        }
    }

    public interface IHintType
    {
        dynamic Value { get; set; }
        
        string ToString();

        dynamic Get(int idx)
        {
            return -1;
        }

        dynamic Get();

        int Count();
    }

    public class StringHintType : IHintType
    {
        public dynamic Value { get; set; }

        public StringHintType(string value)
        {
            Value = value;
        }

        public dynamic Get()
        {
            return Value.ToString();
        }

        public int Count()
        {
            string value = Value;
            return value.Length;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public class DebugCommandHintType : IHintType
    {
        public dynamic Value { get; set; }
        public int Count()
        {
            return -1;
        }

        public dynamic Get()
        {
            return (DebugCommand) Value;
        }

        public DebugCommandHintType(DebugCommand value)
        {
            Value = value;
        }
    }


    public class ListHintType : IHintType
    {
        public dynamic Value { get; set; }

        public dynamic Get(int idx)
        {
            List<dynamic> list = Value;
            return list[idx];
        }

        public dynamic Get()
        {
            return (List<dynamic>)Value;
        }
        
        public int Count()
        {
            List<dynamic> list = Value;

            return list.Count;
        }
    }
    
    
    public class Monitor
    {
        private string Id { get; set; }
        public string DisplayName { get; set; }
        public IMonitorType Value { get; set; }
        public bool Visible { get; set; }

        public Monitor(string id, string displayName, IMonitorType value, bool visible)
        {
            Id = id;
            DisplayName = displayName;
            Value = value;
            Visible = visible;
        }
    }

    private List<IConsoleLogType> _consoleLog = new ();
    private Dictionary<string, DebugCommand> _commands = new ();
    private Dictionary<string, Monitor> _monitors = new ();
    private List<IConsoleLogType> _history = new();
    private int _currentHistory = -1;

    private bool _pauseOnOpen = false;
    private bool _showStats = false;
    private bool _showMiniLog = false;


    private Panel _consolePanel;
    private LineEdit _commandField;
    private Panel _commandHintsPanel;
    private ScrollContainer _commandHintsParent;
    private RichTextLabel _commandHintsLabel;
    private Panel _commandHintHeader;
    private RichTextLabel _commandHintHeaderLabel;

    private Label _stats;
    private ScrollContainer _miniLog;
    private ScrollContainer _logField;
    private VScrollBar _logScrollBar;
    private VScrollBar _miniLogScrollBar;
    

    public override void _Ready()
    {
        _consolePanel = GetNode<Panel>("ConsolePanel");
        _commandField = GetNode<LineEdit>("Command Field");
        _commandHintsPanel = GetNode<Panel>("Command Hints Panel"); 
        _commandHintsParent = GetNode<ScrollContainer>("Command Hints"); 
        _commandHintsLabel = GetNode<RichTextLabel>("Command Hints/RichTextLabel"); 
        _commandHintHeader = GetNode<Panel>("Command Hint Header"); 
        _commandHintHeaderLabel = GetNode<RichTextLabel>("Command Hint Header/RichTextLabel");

        _stats = GetNode<Label>("Stats"); 
        _miniLog = GetNode<ScrollContainer>("Mini Log"); 
        _logField = GetNode<ScrollContainer>("Log"); 
        _logScrollBar = _logField.GetVScrollBar(); 
        _miniLogScrollBar = _miniLog.GetVScrollBar();
        
        _HideConsole();
        _logScrollBar.Changed += _OnScrollBarChanged;

        _AddMonitor("fps", "FPS");
        _AddMonitor("process", "Process", false);
        _AddMonitor("navigation_process", "Navigation Process", false);
        _AddMonitor("physics_process", "Physics Process", false);
        _AddMonitor("static_memory", "Static Memory", false);
        _AddMonitor("static_memory_max", "Static Memory Max", false);
        _AddMonitor("objects", "Objects", false);
        _AddMonitor("nodes", "Nodes", false);

        GetTree().CreateTimer(0.05).Timeout += () => new _BuiltInCommands().init();
    }

    private void _OnScrollBarChanged()
    {
        _logField.ScrollVertical = (int) _logScrollBar.MaxValue;
    }

    public override void _Process(double delta)
    {
        if (_stats.Visible)
        {
            if (_IsMonitorVisible("fps"))
                update_monitor("fps", Performance.GetMonitor(Performance.Monitor.TimeFps));
            if (_IsMonitorVisible("process"))
                update_monitor("process", Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.TimeProcess), 0.001));
            if (_IsMonitorVisible("physics_process"))
                update_monitor("physics_process",
                    Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess), 0.001));
            if (_IsMonitorVisible("navigation_process"))
                update_monitor("navigation_process",
                    Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.TimeNavigationProcess), 0.001));
            if (_IsMonitorVisible("static_memory"))
                update_monitor("static_memory",
                    Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.MemoryStatic), 0.001));
            if (_IsMonitorVisible("static_memory_max"))
                update_monitor("static_memory_max",
                    Mathf.Snapped(Performance.GetMonitor(Performance.Monitor.MemoryStaticMax), 0.001));
            if (_IsMonitorVisible("objects"))
                update_monitor("objects", Performance.GetMonitor(Performance.Monitor.ObjectCount));
            if (_IsMonitorVisible("nodes"))
                update_monitor("nodes", Performance.GetMonitor(Performance.Monitor.ObjectNodeCount));
        }

        _stats.Text = "";
        foreach (Monitor monitor in _monitors.Values)
        {
            if (monitor.Visible)
            {
                monitor.Value ??= new MonitorStringType("unset");
                _stats.Text += $"{monitor.DisplayName}: {monitor.Value}\n";
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Open Debug
        if (!_consolePanel.Visible && (@event.IsActionPressed("open_debug") || @event.IsActionPressed("toggle_debug")))
        {
            _ShowConsole();
            _OnCommandFieldTextChanged(_commandField.Text);
            // This is stupid but it works
            GetTree().CreateTimer(0.02).Timeout += () => _commandField.GrabFocus();
        }
        else if (_consolePanel.Visible && @event.IsActionPressed("ui_cancel") ||
                 @event.IsActionPressed("toggle_debug"))
        {
            _HideConsole(_showStats, _showMiniLog);
        }
        else if (_consolePanel.Visible && @event.IsActionPressed("ui_text_submit"))
        {
            if (_commandField.Text.Length > 0)
            {
                Log($"> {_commandField.Text}");
                _ProcessCommand(_commandField.Text);
                _commandField.Clear();
            }
        }
        // Back in History
        else if (_consolePanel.Visible && @event.IsActionPressed("ui_up"))
        {
            if (_history.Count > 0 && _currentHistory != -1)
            {
                if (_currentHistory > 0)
                    _currentHistory -= 1;
                _commandField.Text = _history[_currentHistory].ToString();
                GetTree().ProcessFrame += () => _commandField.SetCaretColumn(_commandField.Text.Length);

            }
        }
        // Forward in History
        else if (_consolePanel.Visible && @event.IsActionPressed("ui_down"))
        {
            if (_history.Count > 0 && _currentHistory < _history.Count - 1)
            {
                _currentHistory += 1;
                _commandField.Text = _history[_currentHistory].ToString();
                GetTree().ProcessFrame += () => _commandField.SetCaretColumn(_commandField.Text.Length);
            }
            else if (_currentHistory == _history.Count - 1)
            {
                _commandField.Text = "";
                _currentHistory = _history.Count;
                GetTree().ProcessFrame += () => _commandField.SetCaretColumn(_commandField.Text.Length);
            }
        }
        else if (_consolePanel.Visible && _IsTabPress(@event))
        {
            _AttemptAutocompletion();
        }
    }

    private static bool _IsTabPress(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent)
            return false;

        return keyEvent.Keycode == Key.Tab && keyEvent.Pressed && !keyEvent.Echo;
    }

    private void _AttemptAutocompletion()
    {
        _OnCommandFieldTextChanged(_commandField.Text);
        List<IHintType> hints = _commandHintsLabel.Text.Split("\n").Select(hint => new StringHintType(hint.Split(']')[1].Split("[")[0].Split(" ")[0])).Cast<IHintType>().ToList();
        
        var l = hints.GetRange(0, hints.Count - 1);

        var commonPrefix = "";

        if (hints.Count != 0)
        {
            for (var i = 0; i < 1000; i++)
            {
                if (!hints.All(h => h.Count() > i && h.Get(i) == hints[0].Get(i)))
                {
                    break;
                }
                commonPrefix += hints[0].Get(i);
            }
        }

        if (!_commandHintsLabel.Visible || commonPrefix == "")
        {
            return;
        }

        if (hints.Count == 1)
        {
            commonPrefix += " "; // Only one hint, so complete the whole word
        }
        // Replace the last word, if any, with `common_prefix`
        var r = new RegEx();
        r.Compile("(\\w+)?$");
        var newText = r.Sub(_commandField.Text, commonPrefix);
        _commandField.Text = newText;
        _commandField.CaretColumn = newText.Length;
        _OnCommandFieldTextChanged(newText);
    }

    private void _OnCommandFieldTextChanged(string newText)
    {
        List<IHintType> commandHints = new();
        var commandSplit = newText.Split(" ");
        var commandId = commandSplit[0];

        if (commandSplit.Length > 1 && _commands.Keys.Contains(commandId))
        {
            _commandHintsParent.Visible = true;
            _commandHintsLabel.Visible = true;
            _commandHintsPanel.Visible = true;
            _commandHintHeader.Visible = true;
            _commandHintsLabel.Text = "";
            
            // Get parameters filled
            var parameterCount = 0;
            var readingString = false;

            foreach (var word in commandSplit)
            {
                if (word.StartsWith("\""))
                {
                    if (!readingString) parameterCount++;
                    if (word != "\"")
                    {
                        if (!word.EndsWith("\""))
                        {
                            readingString = true;
                        }
                    }
                    else
                    {
                        readingString = !readingString;
                    }
                }
                else if (word.EndsWith("\""))
                {
                    readingString = false;
                }
                else
                {
                    if (!readingString) parameterCount++;
                }
            }

            parameterCount -= 2;
            _commandHintHeaderLabel.Text = _GetParameterText(_commands[commandId]);
            if (parameterCount < _commands[commandId].Parameters.Count)
            {
                var options = _commands[commandId].Parameters[parameterCount].Options;
                if (options.Count != 0)
                {
                    foreach (var option in options)
                    {
                        if (option.ToString().StartsWith(commandSplit[^1]))
                        {
                            _commandHintsLabel.Text += $"[url]{option.ToString()}[/url]\n";
                        }
                    }
                }
            }
        }
        else
        {
            var sortedCommands = _commands.Keys.ToList();
            sortedCommands.Sort();

            foreach (var command in sortedCommands)
            {
                if (command.StartsWith(commandId))
                    commandHints.Add(new DebugCommandHintType(_commands[command]));
            }

            _commandHintHeader.Visible = false;

            if (commandHints.Count != 0)
            {
                _commandHintsParent.Visible = true;
                _commandHintsLabel.Visible = true;
                _commandHintsPanel.Visible = true;
                _commandHintsLabel.Text = "";
                foreach (var command in commandHints)
                {
                    _commandHintsLabel.Text += $"[url={((DebugCommand)command.Get()).Id}]{_GetParameterText(command)}[/url]\n";
                }
            }
            else
            {
                _commandHintsParent.Visible = false;
                _commandHintsLabel.Visible = false;
                _commandHintsPanel.Visible = false;
            }
        }
    }

    private void _OnCommandHintsMetaClicked()
    {
        
    }

    private static string _GetParameterText(DebugCommand command, int currentParameter = -1)
    {
        var text = command.Id;

        var isHeader = currentParameter < command.Parameters.Count && currentParameter >= 0;

        foreach (var parameter in command.Parameters)
        {
            if (isHeader && parameter.Name == command.Parameters[currentParameter].Name)
            {
                text += $" [b]<{parameter.Name}: {parameter.Type}>[/b]";
            }
            else
            {
                text += $" <{parameter.Name}: {parameter.Type}>";
            }

            var value = command.GetFunction?.Call();
            if (value != null)
            {
                text += $" === {value}";
            }
        }

        return text;
    }

    private void _ProcessCommand(DebugCommand command)
    {
        // Avoid duplicating history entries
        if (_history.Count == 0 || command != _history[-1])
        {
            _history.Add(new DebugCommandConsoleLogType(command));
            _currentHistory = _history.Count;
        }
        // Splits command
        var commandSplit = command.ToString().Split(" ");
        // Checks if Command is valid
        if (!_commands.ContainsKey(commandSplit[0]))
        {
            LogError($"Command not found: {commandSplit[0]}");
            return;
        }
        
        // Keeps track of current parameter being read
        var commandData = _commands[commandSplit[0]];
        var currentParameter = 0;
        
        // Checks that function is not lambda
        if (commandData.Function.Method == "<anonymous lambda>")
        {
            LogError("Command function must be named...");
            Log(commandData.Function.Method);
            return;
        }

        var commandFunction = commandData.Function.Method + "(";
        var currentString = "";

        for (var i = 1; i < commandSplit.Length; i++)
        {
            if (commandSplit[i] == "") continue;
            if (commandData.Parameters.Count <= currentParameter)
            {
                LogError($"Command \"{commandData.Id}\" requires {commandData.Parameters.Count} parameters, but too many were given...");
                return;
            }

            var currentParameterObj = commandData.Parameters[currentParameter];

            switch (currentParameterObj.Type)
            {
                case DebugCommand.ParameterType.Int:
                    if (!int.TryParse(commandSplit[i], out _))
                    {
                        LogError($"Parameter {currentParameterObj.Name} should be an integer, but an incorrect value was passed...");
                        return;
                    }

                    commandFunction += commandSplit[i] + ",";
                    currentParameter += 1;
                    break;
                case DebugCommand.ParameterType.Float:
                    if (!float.TryParse(commandSplit[i], out _))
                    {
                        LogError($"Parameter {currentParameterObj.Name} should be a float, but an incorrect value was passed...");
                        return;
                    }

                    commandFunction += commandSplit[i] + ",";
                    currentParameter += 1;
                    break;
                case DebugCommand.ParameterType.String:
                    var word = commandSplit[i];

                    if (word.StartsWith("\""))
                    {
                        if (word.EndsWith("\""))
                        {
                            if (word == "\"")
                            {
                                if (currentString == "")
                                    currentString += "\" ";
                                else
                                {
                                    commandFunction += currentString + "\",";
                                    currentParameter += 1;
                                }
                            }
                            else
                            {
                                commandFunction += word + ",";
                                currentParameter += 1;
                            }
                        }
                        else if (currentString != "")
                        {
                            LogError("Cannot create a string within a string...");
                            return;
                        }
                        else
                        {
                            currentString += word + " ";
                        }
                    }
                    else if (currentString != "")
                    {
                        if (word.EndsWith("\""))
                        {
                            currentString += word;
                            commandFunction += currentString + ",";
                            currentString = "";
                            currentParameter += 1;
                        }
                        else
                            currentString += word + " ";
                    }
                    else
                    {
                        commandFunction += "\"" + word + "\",";
                        currentParameter += 1;
                    }
                    break;
                case DebugCommand.ParameterType.Bool:
                    var value = commandSplit[i].ToLower();

                    value = value switch
                    {
                        "true" or "on" or "1" => "true",
                        "false" or "off" or "0" => "false",
                        _ => "error"
                    };

                    if (value == "error")
                    {
                        LogError($"Parameter {currentParameterObj.Name} should be a bool, but an incorrect value was passed...");
                        return;
                    }

                    commandFunction += value + ",";
                    currentParameter += 1;
                    break;
                case DebugCommand.ParameterType.Options:
                    if (currentParameterObj.Options.Count == 0)
                    {
                        LogError($"Parameter \"{currentParameterObj.Name}\" is meant to have options, but none were set...");
                        return;
                    }

                    if (!currentParameterObj.Options.Contains(commandSplit[i]))
                    {
                        LogError($"\"{commandSplit[i]}\" is not a valid option for parameter \"{currentParameterObj.Name}\"...");
                        return;
                    }

                    commandFunction += $"\"{commandSplit[i]}\",";
                    currentParameter += 1;
                    break;
                default:
                    LogError($"Parameter \"{currentParameterObj.Name}\" received an invalid value...");
                    break;
            }
        }

        // Checks if all parameters are entered
        if (commandData.Parameters.Count != currentParameter)
        {
            LogError($"Command {commandData.Id} requires {commandData.Parameters.Count} parameters, but only {currentParameter} were given...");
            return;
        }

        commandFunction += ")";

        var expression = new Expression();
        var error = expression.Parse(commandFunction);
        if (error != Error.Ok)
        {
            LogError($"Parsing error: {_ErrorString(error)}");
            return;
        }

        expression.Execute(new Array(), commandData.FunctionInstance);
    }
    public static void Log(string message)
    {
        GetConsole()._consoleLog.Add(new StringConsoleLogType(message));
        _UpdateLog();
        
        GD.Print(message);
    }
    
    public static void LogError(string error)
    {
        GetConsole()._consoleLog.Add(new StringConsoleLogType($"[color=red]{error}[/color]"));
        _UpdateLog();
        
        GD.Print(error);
    }

    public static void ClearLog()
    {
        GetConsole()._consoleLog.Clear();
        _UpdateLog();
    }
    
    public static void AddCommand(string id, Callable function, GodotObject functionInstance, List<DebugCommand.Parameter> parameters = null,
        string helpText = "", Callable? getFunction = null)
    {
        GetConsole()._commands[id] =
            new DebugCommand(id, function, functionInstance, parameters, helpText, getFunction);
    }
    
    private static DebugConsole GetConsole()
    {
        return (DebugConsole)((SceneTree)Engine.GetMainLoop()).Root.GetNode("/root/debug_console");
    }

    public static void SetPauseOnOpen(bool state)
    {
        Globals.DebugConsole.Call("set_pause_on_open", state);
    }

    public static void AddCommandSetVar(string id, Callable function, GodotObject functionInstance, DebugCommand.ParameterType type,
        string helpText, Callable? getFunction)
    {
        Globals.DebugConsole.Call("add_command_setvar", id, function, functionInstance, type, helpText, getFunction);
    }
}