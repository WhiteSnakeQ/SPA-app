Database schema files are located in: additional/db/

Configure .env file manually or rename:
.env.example -> .env

The project runs entirely in Docker.
From the project root folder execute:
docker compose up --build

After startup the application will be available at: https://tabby-spiffy-blinks.ngrok-free.dev

Seed Test Comments
To generate default comments: https://tabby-spiffy-blinks.ngrok-free.dev/api/comments/seed 
Seed works only if there are no comments in the database.

GraphQL endpoint:
https://tabby-spiffy-blinks.ngrok-free.dev/graphql

Features
    Cascading comments system
    Unlimited nesting level for replies
    Pagination support
    25 root comments per page
    Root comments can be sorted by:
        Creation date
        Username
        Email
	Background image resize processing
	GraphQL API support
	Lazy loading replies
	Caching support for paginated comments\Redis caching
	Realtime comments updates via SignalR
	Fully dockerized development environment

Uploaded files are stored in Docker volume: upload_data

To store files directly on the host machine instead of Docker volume,
replace this line in docker-compose.yml:

- upload_data:/app/wwwroot/uploads

with:

- ./uploads:/app/wwwroot/uploads


Stack
	- ASP.NET Core 9
	- Angular
	- MSSQL Server
	- Entity Framework Core
	- GraphQL (HotChocolate)
	- SignalR
	- ImageSharp
	- Docker / Docker Compose

