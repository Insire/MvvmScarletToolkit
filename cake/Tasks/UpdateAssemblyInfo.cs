using Cake.Common.Solution.Project.Properties;
using Cake.Common.Tools.GitVersion;
using Cake.Core.IO;
using Cake.Frosting;
using System;

public sealed class UpdateAssemblyInfo : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        var gitVersion = context.GitVersion();
        var assemblyInfoParseResult = context.ParseAssemblyInfo(Context.AssemblyInfoPath);

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
                    Value = Context.Platform,
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
                    Value = gitVersion.BranchName,
                },
                new AssemblyInfoMetadataAttribute()
                {
                    Key = "Commit",
                    Value = gitVersion.Sha,
                },
            }
        };

        context.CreateAssemblyInfo(new FilePath(Context.AssemblyInfoPath), settings);
    }
}
