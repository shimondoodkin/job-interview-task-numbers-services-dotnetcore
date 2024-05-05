
# job-interview-task-numbers-services-dotnetcore

  

## the task was:
 

Message Processing System

Develop a Message Processing System with ASP .Net Core

  

 1. ### Introduction
	* You have a microservices architecture that consists of multiple ASP .NET Core services communicating with each

	* other. The services use MSSQL for message persistence, Redis for caching, and Terminal to display results.

  

2. ### Task Description
	
	* Create three ASP .Net Core services named ServiceA, ServiceB, and ServiceC.

	* ServiceA saves a message with a random number to a MSSQL database.

	* ServiceB retrieves messages from the database, processes the number (e.g., doubles it), and stores the result in Redis.

	* ServiceC retrieves the processed numbers from Redis and displays them in a terminal.
	* 
 3. ### Technical Requirements
	
	* Use MSSQL for persisting messages in ServiceA and ServiceB.

	* Utilize Redis for caching the processed numbers in ServiceB.

	* Implement a terminal that fetches and displays the processed numbers from ServiceC.

	* Ensure proper error handling and logging in each service.

  

 4. ### Additional Points
	
	* Implement a mechanism to handle scalability and potential failures in the microservices architecture.

	* Provide clear instructions on how to set up, run, and test the services locally.

	* Use best practices for code organization, maintainability, and readability. Include unit tests to validate the functionality of each service.

	  

 5. ### Submission

	* Share the codebase via a version control system (e.g., GitHub).

	* Include a README file with instructions on how to set up, run, and test the services.

	* Explain any design decisions, trade-offs, or improvements you would make with more time.

  

- Good Luck! 🙂

  
## My result:

## starting the application

The application consists of 3 services. when running docker-compose launches 3 services and the 2 databases. ServiceA has an exposed port 3000 external to 8080 internal. ServiceC prints log of the received multiplied numbers and has an exposed port 3001 external to 8080 internal. To test the system first you need to see the logs then to submit a message that will create a random number the number will pass through SQL Server and Redis, and then will be received by ServiceC and it will be printed in the log.

### try it:

To start:
- docker-compose up -d servicea serviceb servicec

- docker logs -f numbers-task_servicec_1

In another terminal:

- curl http://127.0.0.1:3000/message
- curl http://127.0.0.1:3001/processed-messages

To close:

- docker-compose down

  
  

## develop it


- docker-compose up -d dev

- from Vscode docker extension attach to dev container

- in app folder See Services projects, in each project you can run `dotnet run`
-  note:  There is a Solution View in bottom of file explorer of vscode if you install c# Vscode extension

I have added existing projects to the solution. then on the project I have added dependency of SharedProject .
  
  

# in development url usage  

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

  
for project ServiceC, for development

  

```

  

curl http://localhost:5064/processed-messages

  

output:

["Message:45 = 634","Message:46 = 7100","Message:44 = 3470"]

```

  
  

for project SharedProject, for development

```

  

curl http://localhost:5030/testdb

  

output:

"Database connection successful."

```


  

## running tests

- tests are with xunit

- currently in folder /app/SeviceA, you can run `dotnet test`

  
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

- added dotnet tools to path

```

cat << \EOF >> ~/.bash_profile

# Add .NET Core SDK tools

export PATH="$PATH:/root/.dotnet/tools"

EOF

```

`source ~/.bash_profile`

  
when starting an application with migrations at first there is missing context object.
to craete a context object i used the commands
  
```
/app/ServiceA/> dotnet ef migrations add <name>

/app/ServiceA/> dotnet ef database update
```

- decided to have pub-sub notifications of inserted messages

- decided to use semaphore of max 1 for when processing messages and run it again it was triggered in middle of processing, used a loop to avoid max stack exceeded.

- if redis is shared decided to not use redis "CONFIG SET notify-keyspace-events KEA" and subscribe to "__keyspace@0__:message:*" because it could trigger on many otyher keys too much not requried, rather i desided to send it twice to redis once for storage and once for pubsub.


# what would I do differently?

To make it scalable I would make ServiceA to send values to a queue in Redis. Then ServiceB would process the Redis Queue. Then another service would save, or update DB based on a queue. Then with a load balancer in front it would be scalable.