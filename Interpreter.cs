using System;
using System.Collections.Generic;
using System.IO;

namespace KizhiPart2
{
    public class Interpreter
    {
        private const string VariableNotFoundError = "Переменная отсутствует в памяти";
        private static Dictionary<string, Action<string[]>> kizhiCommands;
        private static Dictionary<string, Action> interpreterCommands;

        private readonly TextWriter writer;
        private readonly List<string> codeLines;
        private readonly Dictionary<string, int> functions;
        private bool settingCodeMode;

        private readonly Dictionary<string, int> variables;
        private readonly Stack<int> callStack;
        private int currentLineIndex;

        public Interpreter(TextWriter writer)
        {
            kizhiCommands = new Dictionary<string, Action<string[]>>
            {
                {"set", SetVariable},
                {"sub", Sub},
                {"print", Print},
                {"rem", Remove},
                {"call", CallFunction}
            };
            interpreterCommands = new Dictionary<string, Action>
            {
                {"set code", SetCode},
                {"end set code", EndSetCode},
                {"run", RunProgram}
            };
            this.writer = writer;
            codeLines = new List<string>();
            functions = new Dictionary<string, int>();
            variables = new Dictionary<string, int>();
            callStack = new Stack<int>();
            currentLineIndex = 0;
        }

        public void ExecuteLine(string command)
        {
            if (command == null || command.Equals(""))
            {
                return;
            }

            if (interpreterCommands.ContainsKey(command))
            {
                interpreterCommands[command].Invoke();
                return;
            }

            if (settingCodeMode)
            {
                ParseCodeLines(command.Split('\n'));
                return;
            }

            ExecuteKizhiCommand(command);
        }

        private void ParseCodeLines(string[] lines)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("def"))
                {
                    functions.Add(lines[i].Split()[1], i);
                }

                codeLines.Add(lines[i]);
            }
        }

        private void SetCode() => settingCodeMode = true;

        private void EndSetCode() => settingCodeMode = false;

        private void ExecuteKizhiCommand(string command)
        {
            var parsedCommand = command.Trim().Split();
            if (kizhiCommands.ContainsKey(parsedCommand[0]))
            {
                kizhiCommands[parsedCommand[0]].Invoke(parsedCommand);
            }
            else
            {
                throw new ArgumentException("Wrong command", command);
            }
        }

        private void SetVariable(string[] args)
        {
            var name = args[1];
            if (int.TryParse(args[2], out var value))
            {
                if (value > 0)
                {
                    variables[name] = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(value.ToString(), "Value should be greater than 0");
                }
            }
        }

        private void Sub(string[] args)
        {
            var name = args[1];
            if (int.TryParse(args[2], out var value))
            {
                if (variables.ContainsKey(name))
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException(value.ToString(), "Value should be greater than 0");
                    }

                    if (variables[name] < value)
                    {
                        throw new ArithmeticException("Operation result should be greater than 0");
                    }

                    variables[name] -= value;
                }
                else
                {
                    writer.WriteLine(VariableNotFoundError);
                }
            }
        }

        private void Print(string[] args)
        {
            var name = args[1];
            if (variables.ContainsKey(name))
            {
                writer.WriteLine(variables[name]);
            }
            else
            {
                writer.WriteLine(VariableNotFoundError);
            }
        }

        private void Remove(string[] args)
        {
            var name = args[1];
            if (variables.ContainsKey(name))
            {
                variables.Remove(name);
            }
            else
            {
                writer.WriteLine(VariableNotFoundError);
            }
        }

        private void CallFunction(string[] args)
        {
            var name = args[1];
            if (functions.ContainsKey(name))
            {
                callStack.Push(currentLineIndex);
                currentLineIndex = functions[name];
            }
            else
            {
                throw new ArgumentException("This function doesn't exist", name);
            }
        }

        private void RunProgram()
        {
            while (true)
            {
                SkipFunctionLines();
                if (currentLineIndex >= codeLines.Count)
                {
                    Clear();
                    return;
                }

                ExecuteKizhiCommand(codeLines[currentLineIndex]);
                GetNextLineToExecute();
                if (currentLineIndex == 0)
                {
                    return;
                }
            }
        }

        private void GetNextLineToExecute()
        {
            currentLineIndex++;

            if (currentLineIndex >= codeLines.Count
                || char.IsWhiteSpace(codeLines[currentLineIndex - 1][0])
                && !char.IsWhiteSpace(codeLines[currentLineIndex][0]))
            {
                if (callStack.Count != 0)
                {
                    currentLineIndex = callStack.Pop() + 1;
                }

                if (currentLineIndex >= codeLines.Count)
                {
                    Clear();
                }
            }
        }

        private void SkipFunctionLines()
        {
            if (codeLines[currentLineIndex].StartsWith("def"))
            {
                while (currentLineIndex < codeLines.Count
                       && (char.IsWhiteSpace(codeLines[currentLineIndex][0])
                           || codeLines[currentLineIndex].StartsWith("def")))
                {
                    currentLineIndex++;
                }
            }
        }

        private void Clear()
        {
            variables.Clear();
            callStack.Clear();
            currentLineIndex = 0;
        }
    }
}