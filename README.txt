Database schema files are located in: additional/db/

Configure .env file manually or rename:
.env.example -> .env

The project runs entirely in Docker.
From the project root folder execute:
docker compose up --build

OR FOR WINDOWS:
	.\start -> Start the docker
	.\reset -> Reload all docker data and docker
	.\stop -> Shutdown docker



Use your own HTTPS tunnel by configuring:
https://{addres}
- NGROK_AUTHTOKEN in `.env`
- `--url=` in `docker-compose.yml` (ngrok service)

Seed Test Comments
https://{addres}/api/comments/seed
Seed works only if there are no comments in the database.

GraphQL endpoint:
https://{addres}/graphql

Features
-	Cascading comments system
-	Unlimited nesting level for replies
-	Pagination support
-	25 root comments per page
-	Root comments can be sorted by:
        Creation date
        Username
        Email
-	Background image resize processing
-	GraphQL API support
-	Lazy loading replies
-	Caching support for paginated comments
-	Redis caching
-	Realtime comments updates via SignalR
-	Fully dockerized development environment

Security
-	 XSS protection
-	 HTML sanitization
-	 File validation
-	 SQL injection protection via EF Core
-	 CAPTCHA validation


Stack
-	 ASP.NET Core 9
-	 Angular
-	 MSSQL Server
-	 Entity Framework Core
-	 GraphQL (HotChocolate)
-	 SignalR
-	 ImageSharp
-	 Docker / Docker Compose

Uploaded files are stored in Docker volume: upload_data

To store files directly on the host machine instead of Docker volume,
replace this line in docker-compose.yml:

- upload_data:/app/wwwroot/uploads

with:

- ./uploads:/app/wwwroot/uploads


