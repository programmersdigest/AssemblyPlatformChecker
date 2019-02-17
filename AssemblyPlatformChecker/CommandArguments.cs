namespace AssemblyPlatformChecker
{
    /// <summary>
    /// Parses the command line arguments.
    /// </summary>
    internal class CommandArguments
    {
        /// <summary>
        /// Denotes whether the help should be displayed. If true, no other actions
        /// should be performed.
        /// </summary>
        public bool ShowHelp { get; }

        /// <summary>
        /// Denotes whether verbose output should be displayed.
        /// </summary>
        public bool Verbose { get; }

        /// <summary>
        /// The search path in which to check the assemblies.
        /// </summary>
        public string SearchPath { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="CommandArguments"/> class.
        /// Parses the given <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The command line arguments to parse.</param>
        public CommandArguments(string[] args)
        {
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "-v":
                        Verbose = true;
                        break;
                    case "-h":
                        ShowHelp = true;
                        break;
                    default:
                        SearchPath = arg;
                        break;
                }
            }
        }
    }
}
