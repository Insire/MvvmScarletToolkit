using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using System.Text;
using System.Text.RegularExpressions;

namespace Build.Tasks
{
    /// <summary>
    /// Writes the notes of the GitHub release to <see cref="BuildContext.ReleaseNotesFile"/>, for the Publish job of the CD pipeline:
    /// links to every package, the commits since the previous release tag and the Dependabot updates, collapsed per package.
    /// Merge commits are left out. Needs the git tags, run it locally to preview the notes of the next release.
    /// </summary>
    public sealed partial class CreateReleaseNotes : FrostingTask<BuildContext>
    {
        private const string DependabotAuthor = "dependabot[bot]";

        public override void Run(BuildContext context)
        {
            var previousRelease = GetPreviousRelease(context);
            context.Information("Release notes for {0}, changes since {1}", context.ReleaseVersion, previousRelease);

            var commits = GitLines(context, "log", "--no-merges", "--format=%an%x1f%h%x1f%s", $"{previousRelease}..HEAD")
                .Select(line => line.Split('\u001f'))
                .Select(parts => (Author: parts[0], Hash: parts[1], Subject: parts[2]))
                .ToList();

            var changes = commits.Where(c => c.Author != DependabotAuthor).ToList();
            var dependencyUpdates = CollapseDependencyUpdates(commits.Where(c => c.Author == DependabotAuthor).Select(c => c.Subject));

            var notes = new StringBuilder();
            AppendPackages(notes);

            notes.AppendLine($"## Changes since {previousRelease}").AppendLine();
            if (changes.Count == 0)
            {
                notes.AppendLine("None.");
            }

            foreach (var (_, hash, subject) in changes)
            {
                notes.AppendLine($"- {subject} ({hash})");
            }

            if (dependencyUpdates.Count > 0)
            {
                notes.AppendLine()
                    .AppendLine("<details>")
                    .AppendLine($"<summary>Dependency updates ({dependencyUpdates.Count})</summary>")
                    .AppendLine();

                foreach (var update in dependencyUpdates)
                {
                    notes.AppendLine($"- {update}");
                }

                notes.AppendLine().AppendLine("</details>");
            }

            context.EnsureDirectoryExists(context.ReleaseNotesFile.GetDirectory());
            File.WriteAllText(context.ReleaseNotesFile.FullPath, notes.ToString());

            context.Information("{0} changes, {1} dependency updates, written to {2}", changes.Count, dependencyUpdates.Count, context.ReleaseNotesFile.FullPath);
        }

        /// <summary>
        /// The newest release tag reachable from HEAD. Skips the tag of this release, which exists when the job runs again after the draft was published.
        /// </summary>
        private static string GetPreviousRelease(BuildContext context)
        {
            var tag = GitLines(context, "describe", "--tags", "--abbrev=0", "--match", "*.*.*", "--exclude", context.ReleaseVersion, "HEAD")
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(tag)
                ? throw new CakeException("No previous release tag found. The checkout needs the git tags and the full history.")
                : tag!;
        }

        private static void AppendPackages(StringBuilder notes)
        {
            var groups = BuildContext.GetWindowsNugetPackageProjects()
                .Select(p => System.IO.Path.GetFileNameWithoutExtension(p.ProjectFile))
                .GroupBy(GetCategory)
                .OrderBy(g => g.Key.Order);

            notes.AppendLine("NuGet packages for this release:").AppendLine();

            foreach (var group in groups)
            {
                notes.AppendLine($"### {group.Key.Title}").AppendLine();

                foreach (var package in group)
                {
                    notes.AppendLine($"- [{package}](https://www.nuget.org/packages/{package}/)");
                }

                notes.AppendLine();
            }
        }

        private static (int Order, string Title) GetCategory(string package)
        {
            return package switch
            {
                "MvvmScarletToolkit.Wpf" or "MvvmScarletToolkit.Avalonia" => (1, "framework specific"),
                "MvvmScarletToolkit.Abstractions" => (2, "interfaces and models"),
                _ => (0, "general purpose"),
            };
        }

        /// <summary>
        /// Turns several "Bump X from a to b" into one "X from first a to last b". Other subjects, like the grouped
        /// "Bump Avalonia and Avalonia.Desktop", are kept once. Expects the subjects newest first, like git log returns them.
        /// </summary>
        private static List<string> CollapseDependencyUpdates(IEnumerable<string> subjects)
        {
            var bumps = new Dictionary<string, (string From, string To)>(StringComparer.OrdinalIgnoreCase);
            var order = new List<string>();

            foreach (var subject in subjects.Reverse())
            {
                var match = BumpPattern().Match(subject);
                var key = match.Success
                    ? match.Groups["package"].Value
                    : subject.StartsWith("Bump ", StringComparison.Ordinal) ? subject["Bump ".Length..] : subject;

                if (!bumps.TryGetValue(key, out var existing))
                {
                    order.Add(key);
                    bumps[key] = match.Success ? (match.Groups["from"].Value, match.Groups["to"].Value) : ("", "");
                }
                else if (match.Success)
                {
                    bumps[key] = (existing.From, match.Groups["to"].Value);
                }
            }

            return order
                .Select(key => bumps[key] is { From: not "" } bump ? $"{key} from {bump.From} to {bump.To}" : key)
                .OrderBy(update => update, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> GitLines(BuildContext context, params string[] arguments)
        {
            var builder = new ProcessArgumentBuilder();
            foreach (var argument in arguments)
            {
                builder.AppendQuoted(argument);
            }

            var settings = new ProcessSettings
            {
                Arguments = builder,
                RedirectStandardOutput = true,
                WorkingDirectory = context.Environment.WorkingDirectory,
            };

            var exitCode = context.StartProcess("git", settings, out var output);
            if (exitCode != 0)
            {
                throw new CakeException($"git {arguments[0]} exited with {exitCode}");
            }

            return output.ToList();
        }

        [GeneratedRegex(@"^Bump (?<package>\S+) from (?<from>\S+) to (?<to>\S+)$")]
        private static partial Regex BumpPattern();
    }
}
