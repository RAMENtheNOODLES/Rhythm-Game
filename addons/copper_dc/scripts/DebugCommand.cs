using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace RhythmGame.addons.copper_dc.scripts;

public class DebugCommand
{
    public enum ParameterType
    {
        Int,
        Float,
        String,
        Bool,
        Options
    }

    public struct Parameter
    {
        public string Name { get; set; }
        public ParameterType Type { get; set; }
        public Array Options { get; set; }

        public Parameter(string name, ParameterType type, Array options)
        {
            Name = name;
            Type = type;
            Options = options;
        }
    }
    
    public string Id { get; set; }
    public List<Parameter> Parameters { get; set; }
    public Callable Function { get; set; }
    public GodotObject FunctionInstance { get; set; }
    public string HelpText { get; set; }
    public Callable? GetFunction { get; set; }

    public DebugCommand(string id, Callable function, GodotObject functionInstance, List<Parameter> parameters,
        string helpText, Callable? getFunction)
    {
        Id = id;
        Function = function;
        FunctionInstance = functionInstance;
        Parameters = parameters;
        HelpText = helpText;
        GetFunction = getFunction;
    }

    public override string ToString()
    {
        return $"{Id} {Parameters} {Function} {FunctionInstance} {HelpText} {GetFunction}";
    }
}