using System.Text;
namespace RantCore;

public static class MessageBuilder
{
    private static string[] messageFolderNames = ["msg", "messages", "msgs", "mesg", "mesgs", "builtin_msgs"];
    private static string[] defTypes = ["class", "record", "struct", "record class", "record struct"];
    private static Dictionary<string, string> pythonTypeMap = new Dictionary<string, string>
    {
        { "string", "str" },
        { "char", "str" },
        { "byte", "int" },
        { "sbyte", "int" },
        { "short", "int" },
        { "ushort", "int" },
        { "uint", "int" },
        { "long", "int" },
        { "ulong", "int" },
        { "double", "float" },
        { "decimal", "float" },
    };

    private static Dictionary<string, string> csharpTypeMap = new Dictionary<string, string>
    {
        { "float64", "double" },
        { "Float64", "double" },
        { "float32", "float" },
        { "Float32", "float" },
        { "int32", "int" },
        { "Int32", "int" },
        { "int64", "long" },
        { "Int64", "long" },
        { "String", "string" },
        { "geometry_msgs/Vector3", "Vector3" },
        { "geometry_msgs/Point", "Vector3" },
        { "geometry_msgs/Point32", "Vector3" },
        { "geometry_msgs/Quaternion", "Quaternion" },
        { "geometry_msgs/Transform", "Transform" },
        { "geometry_msgs/Pose", "Transform" },
    };



    private static string[] csharpReservedWords = ["abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"];

    public static bool debug = false;

    public static bool generateCsharp = true;
    public static bool generatePython = true;

    private static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static string BuildCsharpFromString(string messageName, string messageDefinition)
    {
        var lines = messageDefinition
            .Split("\n")
            .Select(x => x.Trim().Replace(";", string.Empty))
            .Where(x => !string.IsNullOrEmpty(x)).ToList();

        var splits = lines.Select(x => x.Split(" "));

        var types = splits.Select(x => x[0]).ToList();
        var names = splits.Select(x => x[1]).ToList();
        var addedNamespaces = new List<string>();

        void SkipLine(int index)
        {
            lines.RemoveAt(index);
            types.RemoveAt(index);
            names.RemoveAt(index);
        }

        var builder = new StringBuilder();
        builder.AppendLine("using Rant.Messages;");
        addedNamespaces.Add("Rant.Messages");
        builder.AppendLine("");

        var defType = "record struct";
        // ----- first pass to find metadata -----
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var type = types[i];
            var name = names[i];

            switch (type)
            {
                case "type":
                    if (!defTypes.Contains(name))
                    {
                        LogError($@"Msg: {messageName} - Expected {string.Join(", ", defTypes)} - Found {name}");
                        break;
                    }
                    defType = name;
                    SkipLine(i);
                    break;

                case "namespace":
                    var namespaceName = name + ".Messages";
                    if (addedNamespaces.Contains(namespaceName))
                    {
                        LogWarning($"Msg: {messageName} - Skipping duplicate namespace {name}");
                        break;
                    }
                    builder.AppendLine($"namespace {namespaceName};\n");
                    addedNamespaces.Add(namespaceName);
                    SkipLine(i);
                    break;

                case "using":
                    if (addedNamespaces.Contains(name))
                    {
                        LogWarning($"Msg: {messageName} - Skipping duplicate namespace {name}");
                        break;
                    }
                    builder.AppendLine($"using {name};\n");
                    addedNamespaces.Add(name);
                    SkipLine(i);
                    break;

                default:
                    // remove inline namespaces from types
                    var splitByType = type.Split(".");

                    if (splitByType.Length > 1)
                    {
                        var usingJoined = string.Join(".", splitByType.Take(splitByType.Length - 1));
                        if (addedNamespaces.Contains(usingJoined))
                        {
                            LogWarning($"Msg: {messageName} - Skipping duplicate namespace {usingJoined}");
                            break;
                        }
                        
                        builder.AppendLine($"using {usingJoined};\n");
                        addedNamespaces.Add(usingJoined);

                        types[i] = splitByType[^1]; // assign the raw type back
                    }
                    break;
            }

        }

        builder.AppendLine($"public {defType} {messageName} : IMessage");
        builder.AppendLine("{");
        builder.AppendLine("    public DateTime Timestamp { get; set; } = DateTime.UtcNow;");
        builder.AppendLine("");

        var lastLineWasComment = false;
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var type = types[i];
            var name = names[i];

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type)) 
            {
                LogWarning($"Msg: {messageName} - Skipping line {i} - type or name is empty");
                builder.AppendLine($"    // Message line {i} is invalid. Skipped.");
                continue;
            }

            var startsWithHash = line.StartsWith('#');
            var startsWithSlash = line.StartsWith("//");
            if (startsWithHash || startsWithSlash)
            {
                // Remove the # or // from the line
                if (startsWithHash) line = line[1..];
                else line = line[2..];

                if (!lastLineWasComment) // build multi line comment start
                    builder.AppendLine("    /// <summary>");

                builder.AppendLine($"    /// {line}"); // add line to comment
                lastLineWasComment = true;
                continue;
            }
            else if (lastLineWasComment)
            {
                builder.AppendLine("    /// </summary>"); // build multi line comment end
                lastLineWasComment = false;
            }

            if (csharpTypeMap.TryGetValue(type, out string? value))
            {
                type = value;
            }

            if (csharpReservedWords.Contains(name))
            {
                name = "@" + name;
            }

            // split out ros types
            type = type.Split("/")[^1];

            builder.AppendLine($"    public {type} {name};");
            builder.AppendLine("");
        }

        builder.AppendLine($"    public {messageName}(){{}}");
        builder.AppendLine("}");
        return builder.ToString();
    }

    public static async Task<string> Build(string filepath, string outputFolder)
    {
        if (string.IsNullOrEmpty(filepath) || !File.Exists(filepath))
        {
            return string.Empty;
        }
        
        var messageName = Path.GetFileNameWithoutExtension(filepath);

        // format message name to be PascalCase
        messageName = messageName.Split(" ")
            .Select(x => char.ToUpper(x[0]) + x[1..])
            .Aggregate((a, b) => $"{a}{b}");

        if (debug) Console.WriteLine($"Building {messageName}...");

        var messageDefinition = await File.ReadAllTextAsync(filepath);
        var csharp =  "";
        // var python = "";

        if (generateCsharp)
            csharp = BuildCsharpFromString(messageName, messageDefinition);

        if (string.IsNullOrEmpty(outputFolder))
        {
            var dirName = Path.GetDirectoryName(filepath);
            outputFolder = Path.Combine(dirName ?? "./", "built");
        }

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        
        if (generateCsharp)
        {
            var csharpOutputPath = Path.Combine(outputFolder, $"{messageName}.cs");
            await File.WriteAllTextAsync(csharpOutputPath, csharp);
            if (debug) Console.WriteLine($"Saved {messageName}.cs to {csharpOutputPath}");
        }

        // if (generatePython)
        // {
        //     var pythonOutputPath = Path.Combine(outputFolder, $"{messageName}.py");
        //     await File.WriteAllTextAsync(pythonOutputPath, python);
        //     if (debug) Console.WriteLine($"Saved {messageName}.py to {pythonOutputPath}");
        // }

        return csharp;
    }

    public static async Task BuildAllInFolder(string folderPath, string outputFolder)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            return;
        }

        var files = Directory.GetFiles(folderPath, "*.msg");
        
        foreach (var file in files)
        {
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
            {
                return;
            }
            
            await Build(file, outputFolder);
        }
    }

    public static async Task BuildAllInFolderRecursive(string folderPath, string outputFolder)
    {
        var folders = Directory.GetDirectories(folderPath);
        var tasks = new List<Task>();
        foreach (var folder in folders)
        {
            var folderName = Path.GetFileNameWithoutExtension(folder);
            if (string.IsNullOrEmpty(folder) || !messageFolderNames.Contains(folderName))
            {
                continue;
            }

            tasks.Add(BuildAllInFolderRecursive(folder, outputFolder));
        }

        await BuildAllInFolder(folderPath, outputFolder);

        Task.WaitAll([.. tasks]);
    }

    public static async Task BuildAllInFolderRecursive(string folderPath)
    {
        await BuildAllInFolderRecursive(folderPath, null);
        if (debug) Console.WriteLine($"Built all messages in {folderPath}");
    }

}