# About
FileStorage is a simple file storage system with API server and CLI.

# Quick start
## Server installation
There is two ways to run the server such as Docker container and .NET. You can choose one of them.

### Run on Docker
To run the API server as a docker container, build the docker image and run the container.
```
cd src
docker build -t skgch/file-storage-api .
docker run --rm -it -p 5000:5000 skgch/file-storage-api
```
Now the containerized server is listening on `http://localhost:5000`.

### Run on .NET
If you have .NET on your machine, you can run the server on your machine directly.  
.NET is cross platform. Download .NET 7.0 from [here](https://dotnet.microsoft.com/en-us/download) and start the API server. 
```
cd src/FileStorage.WebApi
dotnet run
```
Now the server is listening on `http://localhost:5000`.

## CLI installation
Download CLI from the links below depending on your OS.
- Linux
- Mac OS
- Windows

Give the file permission to execute.
```
chmod +x fs-store
```

## Usage
```
./fs-store upload <path to file>
./fs-store list
./fs-store delete <file id>
```

# Server
## API Docs
API Docs is created by swagger when you run the server with Development mode.
Run the server and access to `http://localhost:5000/swagger/index.html`.　　
Execute the docker container which creates the swagger document
```
docker build -t skgch/file-storage-api-swagger -f Dockerfile.Swagger .
docker run --rm -it -p 5000:5000 skgch/file-storage-api-swagger
```
or just run with dotnet command.
```
cd src/FileStorage.WebApi
dotnet run
```

Note that with docker container mode, uploaded files are located on `/store` and they will disappear when you remove the container.  
To keep those files, mount `/store` on your host machine or copy them from the container.

# CLI
## Usage
Detailed usage is shown with `--help` option.
```
Description:
  File Storage CLI

Usage:
  fs-store [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  upload <file path>  Upload the specified file to the storage.
  delete <file id>    Delete the specified file on the storage.
  list                Show list of files on the storage.

```

The CLI connects to `http://localhost:5000` by default.  
If you want to change the API's base URL to connect, please set `FS_API_BASE_URL` like below.
```
FS_API_BASE_URL=https://example.com
```

## Build
To build and pack into a single executable binary, execute the following command.
```
cd src/FileStorage.Cli
dotnet publish -c Release --use-current-runtime
```
Executable binary is created to the following path.
```
src/FileStorage.Cli/bin/Release/net7.0/{your runtime}/publish/fs-store
```

# Test
Change current directory to the project root and execute the following command.
```
dotnet test
```