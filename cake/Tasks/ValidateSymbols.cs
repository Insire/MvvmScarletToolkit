using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Build.Tasks
{
    /// <summary>
    /// Checks that every assembly in a package has a portable PDB in the matching symbol package, and that the PDB was built together with the assembly.
    /// </summary>
    public sealed class ValidateSymbols : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var errors = new List<string>();
            var packages = context.GetFiles(context.PackagePath.FullPath + "/*.nupkg");
            if (packages.Count == 0)
            {
                throw new CakeException($"No packages found in {context.PackagePath.FullPath}");
            }

            foreach (var package in packages)
            {
                var symbolPackage = package.ChangeExtension(".snupkg");
                if (!context.FileExists(symbolPackage))
                {
                    errors.Add($"{package.GetFilename()}: symbol package {symbolPackage.GetFilename()} is missing");
                    continue;
                }

                using var assemblies = ZipFile.OpenRead(package.FullPath);
                using var symbols = ZipFile.OpenRead(symbolPackage.FullPath);

                var dlls = assemblies.Entries
                    .Where(p => p.FullName.StartsWith("lib/", StringComparison.Ordinal) && p.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (dlls.Count == 0)
                {
                    errors.Add($"{package.GetFilename()}: contains no assemblies");
                }

                foreach (var dll in dlls)
                {
                    var pdbPath = System.IO.Path.ChangeExtension(dll.FullName, ".pdb");
                    var pdb = symbols.GetEntry(pdbPath);
                    if (pdb is null)
                    {
                        errors.Add($"{package.GetFilename()}: {pdbPath} is missing from {symbolPackage.GetFilename()}");
                        continue;
                    }

                    var error = Validate(dll, pdb);
                    if (error is null)
                    {
                        context.Verbose("{0}: {1} is portable and matches", package.GetFilename(), pdbPath);
                    }
                    else
                    {
                        errors.Add($"{package.GetFilename()}: {pdbPath} {error}");
                    }
                }
            }

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    context.Error(error);
                }

                throw new CakeException($"{errors.Count} symbol problem(s) found in {packages.Count} package(s)");
            }

            context.Information("Symbols of {0} package(s) are portable and match their assemblies", packages.Count);
        }

        private static string? Validate(ZipArchiveEntry dll, ZipArchiveEntry pdb)
        {
            // zip entry streams can't seek, which PEReader and MetadataReaderProvider need
            using var pdbStream = Copy(pdb);
            Span<byte> signature = stackalloc byte[4];
            pdbStream.ReadExactly(signature);
            pdbStream.Position = 0;

            // portable PDBs are ECMA-335 metadata, Windows PDBs start with "Microsoft C/C++ MSF 7.00"
            if (!signature.SequenceEqual("BSJB"u8))
            {
                return "is not a portable PDB";
            }

            using var dllStream = Copy(dll);
            using var peReader = new PEReader(dllStream);
            var codeView = peReader.ReadDebugDirectory().FirstOrDefault(p => p.Type == DebugDirectoryEntryType.CodeView);
            if (codeView.Type != DebugDirectoryEntryType.CodeView)
            {
                return "has no CodeView entry in its assembly";
            }

            if (!codeView.IsPortableCodeView)
            {
                return "is referenced as a Windows PDB by its assembly";
            }

            var expectedId = peReader.ReadCodeViewDebugDirectoryData(codeView).Guid;

            using var provider = MetadataReaderProvider.FromPortablePdbStream(pdbStream, MetadataStreamOptions.LeaveOpen);
            var id = new BlobContentId(provider.GetMetadataReader().DebugMetadataHeader!.Id);

            return id.Guid == expectedId
                ? null
                : $"has id {id.Guid}, but its assembly expects {expectedId}";
        }

        private static MemoryStream Copy(ZipArchiveEntry entry)
        {
            var result = new MemoryStream();
            using (var stream = entry.Open())
            {
                stream.CopyTo(result);
            }

            result.Position = 0;
            return result;
        }
    }
}
