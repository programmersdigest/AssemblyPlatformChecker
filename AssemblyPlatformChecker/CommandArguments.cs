using System;

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
        /// Filters for the result output.
        /// </summary>
        public BinaryTypeFilters Filters { get; private set; }

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
                if (arg == "-h")
                {
                    ShowHelp = true;
                }
                else if (arg == "-v")
                {
                    Verbose = true;
                }
                else if (arg.StartsWith("-f:") && arg.Length > 3)
                {
                    ParseFilters(arg);
                }
                else
                {
                    SearchPath = arg;
                }
            }
        }

        /// <summary>
        /// Parses the filter argument into the Filters property.
        /// </summary>
        /// <param name="arg">The command line argument -f:...</param>
        private void ParseFilters(string arg)
        {
            var filters = arg.Substring(3).Split(',');
            foreach (var filter in filters)
            {
                if (Enum.TryParse<BinaryTypeFilters>(filter, true, out var value))
                {
                    Filters |= value;
                }
                else
                {
                    Console.Error.WriteLine($"Unknown filter: {filter}.");
                }
            }
        }
    }
}
