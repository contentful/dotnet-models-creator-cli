using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Errors;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Contentful.ModelsCreator.Cli
{
    [Command(Name = "contentful.modelscreator.cli", FullName = "Contentful ModelsCreator", Description = "Creates c# classes from a Contentful content model.")]
    [HelpOption]
    public class ModelsCreator
    {
        [Option(CommandOptionType.SingleValue, Description = "The Contentful API key for the Content Delivery API")]
        [Required(ErrorMessage = "You must specify the Contentful API key for the Content Delivery API")]
        public string ApiKey { get; set; }
        [Option(CommandOptionType.SingleValue, Description = "The space id to fetch content types from")]
        [Required(ErrorMessage = "You must specify the space id to fetch content types from.")]
        public string SpaceId { get; set; }
        [Option(CommandOptionType.SingleValue, Description = "The namespace the classes should be created in")]
        public string Namespace { get; set; } = "Replace.Me.NameSpace";

        [Option(CommandOptionType.SingleValue, Description = "The environment to fetch the content model from")]
        public string Environment { get; set; } = "master";

        [Option(CommandOptionType.NoValue, Description = "Automatically overwrite files that already exist")]
        public bool Force { get; }

        [Option(CommandOptionType.SingleValue, Description = "Path to the file or directory to create files in")]
        public string Path { get; }

        [VersionOption("0.9.1")]
        public bool Version { get; }

        private string _templateStart = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contentful.Core.Models;
";

        private IEnumerable<ContentType> _contentTypes;

        public async Task<int> OnExecute(CommandLineApplication app, IConsole console)
        {

            var http = new HttpClient();
            var options = new ContentfulOptions
            {
                DeliveryApiKey = ApiKey,
                SpaceId = SpaceId,
                Environment = Environment
            };
            var client = new ContentfulClient(http, options);

            try
            {
                _contentTypes = await client.GetContentTypes();
            }
            catch (ContentfulException ce)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("There was an error communicating with the Contentful API: " + ce.Message);
                Console.Error.WriteLine($"Request ID: {ce.RequestId}");
                Console.Error.WriteLine("" + ce.ErrorDetails?.Errors);
                Console.Error.WriteLine("Please verify that your api key and access token are correct");
                Console.ResetColor();
                return Program.ERROR;
            }

            Console.WriteLine($"Found {_contentTypes.Count()} content types.");
            var path = "";
            if (string.IsNullOrEmpty(Path))
            {
                path = Directory.GetCurrentDirectory();
                Console.WriteLine($"No path specified, creating files in current working directory {path}");
            }
            else
            {
                Console.WriteLine($"Path specified. Files will be created at {Path}");
                path = Path;
            }

            var dir = new DirectoryInfo(path);

            if (dir.Exists == false)
            {
                Console.WriteLine($"Path {path} does not exist and will be created.");
                dir.Create();
            }

            foreach (var contentType in _contentTypes)
            {
                var safeFileName = GetSafeFilename(contentType.Name);

                var file = new FileInfo($"{dir.FullName}{System.IO.Path.DirectorySeparatorChar}{safeFileName}.cs");
                if (file.Exists && !Force)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    var prompt = Prompt.GetYesNo($"The folder already contains a file with the name {file.Name}. Do you want to overwrite it?", true);
                    Console.ResetColor();
                    if (prompt == false)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Skipping {file.Name}");
                        Console.ResetColor();
                    }
                }

                using (var sw = file.CreateText())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(_templateStart);
                    sb.AppendLine($"namespace {Namespace}");
                    //start namespace
                    sb.AppendLine("{");

                    sb.AppendLine($"    public class {FormatClassName(contentType.Name)}");
                    //start class
                    sb.AppendLine("    {");

                    sb.AppendLine("        public SystemProperties Sys { get; set; }");

                    foreach (var field in contentType.Fields)
                    {
                        sb.AppendLine($"        public {GetDataTypeForField(field)} {FirstLetterToUpperCase(field.Id)} {{ get; set; }}");
                    }

                    //end class
                    sb.AppendLine("    }");
                    //end namespace
                    sb.AppendLine("}");

                    sw.WriteLine(sb.ToString());
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Files successfully created!");
            Console.ResetColor();

            return Program.OK;
        }

        private string FormatClassName(string name)
        {
            return RemoveUnallowedCharacters(FirstLetterToUpperCase(name));
        }

        private string FirstLetterToUpperCase(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        private string RemoveUnallowedCharacters(string s)
        {
            return Regex.Replace(s, @"[^A-Za-z0-9_]", "");
        }

        private string GetSafeFilename(string filename)
        {
            var withoutInvalidPathChars = string.Concat(filename.Split(System.IO.Path.GetInvalidFileNameChars()));
            return RemoveUnallowedCharacters(FirstLetterToUpperCase(withoutInvalidPathChars));
        }

        private string GetDataTypeForField(Field field)
        {
            switch (field.Type)
            {
                case "Symbol":
                case "Text":
                    return "string";
                case "Integer":
                    return "int";
                case "Date":
                    return "DateTime";
                case "Number":
                    return "float";
                case "Boolean":
                    return "bool";
                case "Location":
                    return "Location";
                case "Link":
                    return GetDataTypeForLinkField(field);
                case "Array":
                    return GetDataTypeForListField(field);
                case "Object":
                    return "object";
                default:
                    break;
            }

            return "object";
        }

        private string GetDataTypeForLinkField(Field field)
        {
            if (field.LinkType == "Asset")
            {
                return "Asset";
            }

            if (field.LinkType == "Entry")
            {
                if (field.Validations != null && field.Validations.Any(c => c is LinkContentTypeValidator))
                {
                    var linkContentTypeValidator = field.Validations.FirstOrDefault(c => c is LinkContentTypeValidator) as LinkContentTypeValidator;

                    if (linkContentTypeValidator.ContentTypeIds.Count == 1)
                    {
                        return GetDataTypeForContentTypeId(linkContentTypeValidator.ContentTypeIds[0]);
                    }
                }

                return "object";
            }
            return "object";
        }

        private string GetDataTypeForContentTypeId(string contentTypeId)
        {
            var contentType = _contentTypes.FirstOrDefault(c => c.SystemProperties.Id == contentTypeId);

            if (contentType == null)
            {
                return "object";
            }

            return FormatClassName(contentType.Name);
        }

        private string GetDataTypeForListField(Field field)
        {
            if (field.Items.LinkType == "Entry")
            {
                if (field.Items.Validations != null && field.Items.Validations.Any(c => c is LinkContentTypeValidator))
                {
                    var linkContentTypeValidator = field.Items.Validations.FirstOrDefault(c => c is LinkContentTypeValidator) as LinkContentTypeValidator;

                    if (linkContentTypeValidator.ContentTypeIds.Count == 1)
                    {
                        return $"List<{GetDataTypeForContentTypeId(linkContentTypeValidator.ContentTypeIds[0])}>";
                    }
                }

                return "List<object>";
            }

            if (field.Items.LinkType == "Asset")
            {
                return "List<Asset>";
            }

            if (field.Items.Type == "Symbol")
            {
                return "List<string>";
            }

            return "object";
        }
    }
}
