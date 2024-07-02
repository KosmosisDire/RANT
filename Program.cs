using RantCore;
using DotMake.CommandLine;
using Examples.Messages;
await Cli.RunAsync<RantRootCliCommand>(args);

[CliCommand(Description = "Root rant command")]
public class RantRootCliCommand
{
    public void Run(CliContext context)
    {
        context.ShowHelp();
    }

    [CliCommand(Description = "tools for working with Rant messages")]
    public class MsgCommand
    {
        public void Run(CliContext context)
        {
            context.ShowHelp();
        }

        [CliCommand(Description = "build messages at a given path")]
        public class BuildCommand
        {
            [CliArgument(Description = "path to a folder containing .msg files")]
            #if DEBUG
            public string Path { get; set; } = "C:/Main Documents/Projects/Robotics Action Networking Toolkit/Rant";
            #else
            public string Path { get; set; } = "./";
            #endif

            [CliOption(Description = "build messages recursively")]
            public bool Recursive { get; set; } = true;

            public void Run(CliContext context)
            {

                MessageBuilder.debug = true;
                if (Recursive)
                {
                    _ = MessageBuilder.BuildAllInFolderRecursive(Path);
                }
                else
                {
                    _ = MessageBuilder.BuildAllInFolder(Path, null);
                }
            }
        }
    }


    [CliCommand(Description = "run a simple publish and subscribe test")]
    public class TestCommand
    {
        [CliArgument(Description = "rate in hz")]
        public float Rate { get; set; } = 60;

        public void Run(CliContext context)
        {
            var topic = new Topic<Test>("test");
            topic.BeginEcho();

            var rate = new Rate(Rate);
            rate.callback += () =>
            {
                topic.Publish(new Test {data="This is a test of the rant broadcast system!"});
            };
        }
    }

    [CliCommand(Description = "tools for working with Rant topics")]
    public class TopicCommand
    {
        public void Run(CliContext context)
        {
            context.ShowHelp();
        }

        [CliCommand(Description = "list all published topics")]
        public class ListCommand
        {
            public void Run(CliContext context)
            {
                Console.WriteLine("");
                Console.WriteLine(string.Join("\n", Rant.GetAllTopicNames()));
            }
        }
    }

}