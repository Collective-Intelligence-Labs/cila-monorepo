protoc --proto_path=Proto --csharp_out=Scheme --csharp_opt=file_extension=.g.cs  Proto/event.proto 
protoc --proto_path=Proto --csharp_out=Scheme --csharp_opt=file_extension=.g.cs  Proto/operation.proto
protoc --proto_path=Proto --csharp_out=Scheme --csharp_opt=file_extension=.g.cs  Proto/command.proto
protoc --proto_path=Proto --csharp_out=Scheme --csharp_opt=file_extension=.g.cs  Proto/message.proto


/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
touch ~/.zshrc
export PATH=/opt/homebrew/bin:$PATH
source ~/.zshrc