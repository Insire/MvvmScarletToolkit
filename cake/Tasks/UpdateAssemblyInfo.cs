using Cake.Common.Solution.Project.Properties;
using Cake.Frosting;
using System.Globalization;

namespace Build.Tasks
{
    public sealed class UpdateAssemblyInfo : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var assemblyInfoParseResult = context.ParseAssemblyInfo(context.AssemblyInfoFile);

            var settings = new AssemblyInfoSettings()
            {
                Product = assemblyInfoParseResult.Product,
                Company = assemblyInfoParseResult.Company,
                Trademark = assemblyInfoParseResult.Trademark,
                Copyright = $"© {DateTime.Today.Year} Insire",

                InternalsVisibleTo = assemblyInfoParseResult.InternalsVisibleTo,

                MetaDataAttributes =
                [
                    new AssemblyInfoMetadataAttribute()
                    {
                        Key = "Platform",
                        Value = BuildContext.Platform,
                    },
                    new AssemblyInfoMetadataAttribute()
                    {
                        Key = "CompileDate",
                        Value = "[UTC]" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                    },
                    new AssemblyInfoMetadataAttribute()
                    {
                        Key = "PublicRelease",
                        Value = context.IsPublicRelease.ToString(),
                    },
                    new AssemblyInfoMetadataAttribute()
                    {
                        Key = "Branch",
                        Value = context.Branch,
                    },
                    new AssemblyInfoMetadataAttribute()
                    {
                        Key = "Commit",
                        Value = context.GitVersion?.GitCommitId ?? "",
                    },
                    new AssemblyInfoMetadataAttribute()
                    {
                        Key = "Version",
                        Value = context.GitVersion?.SemVer2 ?? "",
                    }
                ]
            };

            context.CreateAssemblyInfo(context.AssemblyInfoFile, settings);
        }
    }
}
