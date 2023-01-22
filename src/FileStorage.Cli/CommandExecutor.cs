using System.CommandLine;
using System.CommandLine.Parsing;

namespace FileStorage.Cli;

internal class CommandExecutor
{
    private readonly IApiClient _apiClient;
    private Command RootCommand { get; }

    public CommandExecutor(IApiClient apiClient)
    {
        _apiClient = apiClient;

        RootCommand = new RootCommand("File Storage CLI") { Name = "fs-store" };
        RootCommand.AddCommand(BuildUploadCommand());
        RootCommand.AddCommand(BuildDeleteCommand());
        RootCommand.AddCommand(BuildListCommand());
    }

    internal async Task ExecuteAsync(string[] args)
        => await RootCommand.InvokeAsync(args);

    private static FileInfo? ValidateFile(ArgumentResult result)
    {
        var filePath = result.Tokens.Single().Value;
        if (!File.Exists(filePath))
        {
            result.ErrorMessage = "File does not exist.";
            return null;
        }

        var file = new FileInfo(filePath);
        if (file.Name.Length > 100)
        {
            result.ErrorMessage = "File name must not be longer than 100.";
            return null;
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            result.ErrorMessage = "File size must not be larger than 5MB.";
            return null;
        }

        return new FileInfo(filePath);
    }

    private static string? ValidateCursor(ArgumentResult result)
    {
        if (!result.Tokens.Any())
            return null;

        var cursor = result.Tokens.Single().Value;
        var timeAndId = cursor.Split("_", StringSplitOptions.RemoveEmptyEntries);

        if (timeAndId.Length != 2
            || !long.TryParse(timeAndId[0], out _)
            || !Guid.TryParse(timeAndId[1], out _))
        {
            result.ErrorMessage = "Next cursor is invalid. Get next cursor from the previous page.";
            return null;
        }

        return cursor;
    }

    private Command BuildUploadCommand()
    {
        var uploadCommand = new Command("upload", "Upload the specified file to the storage.");
        var filePathArg = new Argument<FileInfo?>(
            name: "file path",
            parse: ValidateFile,
            isDefault: false,
            description: "Path of the file to upload.");

        uploadCommand.AddArgument(filePathArg);

        uploadCommand.SetHandler(async file =>
        {
            try
            {
                var id = await _apiClient.UploadAsync(file!);
                Console.WriteLine("File has been successfully uploaded.");
                Console.WriteLine($"fileId: {id}");
            }
            catch
            {
                await Console.Error.WriteLineAsync("Something is wrong. Please check your settings.");
                Environment.Exit(1);
            }
        }, filePathArg);

        return uploadCommand;
    }

    private Command BuildDeleteCommand()
    {
        var deleteCommand = new Command("delete", "Delete the specified file on the storage.");
        var fileIdArg = new Argument<string>("file id", "Id of the file to delete.");

        deleteCommand.AddArgument(fileIdArg);

        deleteCommand.SetHandler(async fileId =>
        {
            try
            {
                await _apiClient.DeleteAsync(fileId);
                Console.WriteLine("File has been successfully deleted.");
            }
            catch (FileNotFoundException)
            {
                await Console.Error.WriteLineAsync("File does not exist.");
                Environment.Exit(1);
            }
            catch
            {
                await Console.Error.WriteLineAsync("Something is wrong. Please check your settings.");
                Environment.Exit(1);
            }
        }, fileIdArg);

        return deleteCommand;
    }

    private Command BuildListCommand()
    {
        var listCommand = new Command("list", "Show list of files on the storage.");
        var nextOption = new Option<string?>(
            name: "--next",
            parseArgument:ValidateCursor,
            isDefault: false,
            description: "Cursor to the next page.");

        listCommand.AddOption(nextOption);

        listCommand.SetHandler(async cursor =>
        {
            try
            {
                var fileList = await _apiClient.GetListAsync(cursor);
                Console.WriteLine("ID | CREATED TIME | UPDATED TIME | FILE NAME");
                
                foreach (var item in fileList.Items)
                    Console.WriteLine($"{item.Id} | {item.CreatedAt} | {item.UpdatedAt} | {item.FileName}");

                if (fileList.NextCursor != null)
                    Console.WriteLine($"Next cursor: {fileList.NextCursor}");
            }
            catch
            {
                await Console.Error.WriteLineAsync("Something is wrong. Please check your settings.");
                Environment.Exit(1);
            }
        }, nextOption);

        return listCommand;
    }
}