﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cake.Core.IO;
using Cake.Core.Scripting;
using Cake.Scripting.Abstractions.Models;
using Cake.Scripting.CodeGen.Generators;
using Cake.Scripting.Reflection.Emitters;

namespace Cake.Scripting.CodeGen
{
    public sealed class CakeScriptAliasGenerator
    {
        private readonly IFileSystem _fileSystem;
        private readonly IScriptAliasFinder _aliasFinder;
        private readonly CakeMethodAliasGenerator _methodGenerator;
        private readonly CakePropertyAliasGenerator _propertyGenerator;

        public CakeScriptAliasGenerator(IFileSystem fileSystem, IScriptAliasFinder aliasFinder)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _aliasFinder = aliasFinder ?? throw new ArgumentNullException(nameof(aliasFinder));

            var typeEmitter = new TypeEmitter();
            var parameterEmitter = new ParameterEmitter(typeEmitter);

            _methodGenerator = new CakeMethodAliasGenerator(typeEmitter, parameterEmitter);
            _propertyGenerator = new CakePropertyAliasGenerator(typeEmitter);
        }

        public CakeScript Generate(FilePath assembly, bool verify)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }
            if (!_fileSystem.Exist(assembly))
            {
                return CakeScript.Empty;
            }

            // Find aliases.
            var aliases = _aliasFinder.FindAliases(assembly);

            // Create the response.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var response = new CakeScript();
            response.Source = GenerateSource(aliases);
            response.Usings.AddRange(aliases.SelectMany(a => a.Namespaces));
            response.References.Add(assembly.FullPath);

            // Return the response.
            return response;
        }

        private string GenerateSource(IEnumerable<CakeScriptAlias> aliases)
        {
            var writer = new StringWriter();

            foreach (var alias in aliases)
            {
                if (alias.Type == ScriptAliasType.Method)
                {
                    _methodGenerator.Generate(writer, alias);
                }
                else
                {
                    _propertyGenerator.Generate(writer, alias);
                }

                writer.WriteLine();
                writer.WriteLine();
            }

            return writer.ToString();
        }
    }
}
