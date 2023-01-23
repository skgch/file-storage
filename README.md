# About
Simple file storage system with API server and CLI.

# Quick Start
## Server Installation
There are two ways to run the server: Docker container and .NET. You can choose one of them.

### Run on Docker Container
To run the API server as a docker container, build the docker image and run the container.
```
cd src
docker build -t skgch/file-storage-api .
docker run --rm -it -p 5000:5000 skgch/file-storage-api
```
Now the containerized server is listening on `http://localhost:5000`.

### Run on .NET
If you have .NET on your machine, you can run the server on your device directly.    
.NET is cross-platform. Download .NET 7.0 from [here](https://dotnet.microsoft.com/en-us/download) and start the API server. 
``` 
cd src/FileStorage.WebApi
dotnet run
```
Now the server is listening on `http://localhost:5000`. 

Before you access the API, create a directory where uploaded files are located.
```
mkdir /storage
```
You can also change the directory by editing `FileSystemStorage.Directory` in `appsettings.Development.json` file.
After changing the directory, restart the server to reflect the setting.

## CLI Installation
Download CLI from the links below, depending on your OS.
- Linux
- Mac OS
- Windows

Give the file permission to execute.
```
chmod +x ./fs-store
```

## Usage Examples
### Upload file
Upload a file by specifying the path.
```
./fs-store upload "./sample.txt"

File has been successfully uploaded.
fileId: a8a75ab7692d42858e1243f224d464cb
```

### Delete file
Delete a file by specifying the file id.
```
./fs-store delete a8a75ab7692d42858e1243f224d464cb
```

### Show list of files
```
./fs-store list

ID | CREATED TIME | UPDATED TIME | FILE NAME
a8a75ab7692d42858e1243f224d464cb | 2023/01/22 20:41:00 +09:00 | 2023/01/22 20:41:00 +09:00 | sample.txt
76ba97de641a416493017a168f7d6acb | 2023/01/22 20:41:19 +09:00 | 2023/01/22 20:41:19 +09:00 | test.png
...
Next cursor: 1674385388384_8e600a2ad26441e09718d0253cfc2256
```
If there is a next page, the next cursor is shown. See the next page by specifying the cursor. The max page size is 100.
```
./fs-store list -- next 1674385388384_8e600a2ad26441e09718d0253cfc2256
```

See more usage with `--help` option.

---
# Server
API Server supports the following actions.
- `POST /api/files`
- `DELETE /api/files/{id}`
- `GET /api/files`

The followings are examples of HTTP requests to the server. To see more details refer API document.
```
curl -X 'POST' \
  'http://localhost:5000/api/files' \
  -H 'accept: application/json' \
  -H 'Content-Type: multipart/form-data' \
  -F 'File=@sample.doc;type=application/msword'
```

```
curl -X 'GET' \
  'http://localhost:5000/api/files' \
  -H 'accept: application/json'
```

```
curl -X 'DELETE' \
  'http://localhost:5000/api/files/114906915bf14f0f988b3154735e83c8' \
  -H 'accept: */*'
```

Note that with docker container mode, uploaded files are located on `/store` in the container, and they will disappear when you remove the container.
To keep those files, mount `/store` on your host machine or copy them from the container.

## API Document
When you run the server with Development mode, swagger creates the API document.
Execute the docker container, which creates the swagger document like the following.
```
docker build -t skgch/file-storage-api-swagger -f Dockerfile.Swagger .
docker run --rm -it -p 5000:5000 skgch/file-storage-api-swagger
```
Or just run `dotnet run` command.
```
cd src/FileStorage.WebApi
dotnet run
```
Access `http://localhost:5000/swagger/index.html` to see the full API document.

## .NET Build and Publish 
The following command builds the server to run on any OS.
`{runtime id}` differs depending on the target host OS. They are typically `win-x64`, `linux-x64`, or `osx-x64`.
See details of `{runtime id}` [here](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog).
```
cd src/FileStorage.WebApi
dotnet publish -c Release -r `{runtime id}` --self-contained false
```
The results of the build are in `src\FileStorage.WebApi\bin\Release\net7.0\win-x64\publish` directory.
Copy them to your deploy target machine and execute `dotnet FileStorage.WebApi.dll` to start the server.
And then, Create `/store` directory on the host before accessing the API.

# CLI
CLI also supports `upload`, `delete`, and `list` commands.
Detailed usage is shown by `fs-store --help`.
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
If you want to change the API's base URL to connect, please set `FS_API_BASE_URL` environment variable like the following.
```
FS_API_BASE_URL=https://example.com
```

## Build and Publish
To build and pack into a single executable binary, execute the following command.
```
cd src/FileStorage.Cli
dotnet publish -c Release -r {runtime id}
```
The executable binary is created to the following path.
`{runtime id}` differs depending on your OS, typically `win-x64`, `linux-x64`, or `osx-x64`.
See details of `{runtime id}` [here](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog).
```
src/FileStorage.Cli/bin/Release/net7.0/{runtime}/publish/fs-store
```

# Test
Change the current directory to the solution root and execute the following command.
```
dotnet test
```