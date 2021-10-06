using Cake.Common.Solution.Project.Properties;
using Cake.Core.IO;
using Cake.Frosting;
using System;

namespace Build
{
    public sealed class UpdateAssemblyInfo : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var assemblyInfoParseResult = context.ParseAssemblyInfo(BuildContext.AssemblyInfoPath);

            var settings = new AssemblyInfoSettings()
            {
                Product = assemblyInfoParseResult.Product,
                Company = assemblyInfoParseResult.Company,
                Trademark = assemblyInfoParseResult.Trademark,
                Copyright = $"© {DateTime.Today.Year} Insire",

                InternalsVisibleTo = assemblyInfoParseResult.InternalsVisibleTo,

                MetaDataAttributes = new[]
                {
                new AssemblyInfoMetadataAttribute()
                {
                    Key = "Platform",
                    Value = BuildContext.Platform,
                },
                new AssemblyInfoMetadataAttribute()
                {
                    Key = "CompileDate",
                    Value = "[UTC]" + DateTime.UtcNow.ToString(),
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
                    Value = context.GitVersion.GitCommitId,
                },
                new AssemblyInfoMetadataAttribute()
                {
                    Key = "Version",
                    Value = context.GitVersion.SemVer2,
                },
            }
            };

            context.CreateAssemblyInfo(new FilePath(BuildContext.AssemblyInfoPath), settings);
        }
    }
}
