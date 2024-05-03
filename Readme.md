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

