## Log of Design Decisions

- Use Git to version control the project.
- Use Docker to make it easy to set up, primarily because I don't have SQL Server (MSSQL) setup and Redis available.
- Use ./ local linked folders for simplicity of storage for containers.
- Use Docker Compose to start everything together easily.
- Have separate Docker containers for scalability.
- Have an "app" container for development, and place containers' folders inside that folder.
- Use ./app linked folder with the host for development.
- Make containers restart unless stopped
- to start and stop:
  - docker-compose up -d
  - docker-compose down
- For development I use vs-code and connect into docker container of numbers-task-app 
- wanted to use entities for sql
- persist root folder of app docker container in app-root to make it not resinstall vscode-server each time
-  added dotnet tools to path 
```
cat << \EOF >> ~/.bash_profile
# Add .NET Core SDK tools
export PATH="$PATH:/root/.dotnet/tools"
EOF
```
source ~/.bash_profile

to craete a context object i used the commands 

/app/ServiceA/> dotnet ef migrations add <name>
/app/ServiceA/> dotnet ef database update

 - desided to have pubsub notifications of inserted messages
 - desided to use semaphore of max 1 for when processing messages and run it again it was triggered in middle of processing, used a loop to avoid max stack exceeded.
 - if redis is shared desided to not use redis "CONFIG SET notify-keyspace-events KEA" and subscribe to "__keyspace@0__:message:*" because it could trigger on many otyher keys too much not requried, rather i desided to send it twice to redis once for storage and once for pubsub.
 - 

# to develop it

 need to run the dev docker container , it is possible to open terminal to /app/ServiceA/ in vscode and then run `dotnet run` you can open several terminals in parallel (split) and run all at once

 There is solution view in bottom of file explorer of vscode if you install c# vscode extension
 I have added existing projects to the solution and after that I have added dependency of shared project .

# usage it is possible to trigger the random number from curl

for project ServiceA

```
curl http://localhost:5114/message

output:
{"id":43,"content":"test","randomNumber":7951,"processed":false}root@c3dbb65a6427:/app/Ser
```

for project ServiceB, for development

```

curl http://localhost:5029/process

output:
"processed ok"
```


for project SharedProject, for development
```

curl http://localhost:5030/testdb

output:
"Database connection successful."
```

