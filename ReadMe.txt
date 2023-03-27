
Make sure you have installed docker and docker-compose tools and that current user can execute docker commands
To launch the system as docker network type in the project's root directory:
  docker compose up

To modify storage engine, edit the DocumentStorageApi/appsettings.json  file: 
  InMemory | HDD | MsSql | Mongo | EntityFramework

Stop the docker container network:
  docker compose down

Delete the created image:
  docker rmi documentstorageapi:latest 

and run the container chain again:
  docker compose up
