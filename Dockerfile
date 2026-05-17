# Frontend
FROM node:22 AS frontend-build

WORKDIR /frontend

COPY comments-app/package*.json ./

RUN npm install

COPY comments-app .

RUN npm run build


# Backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build

WORKDIR /src

COPY backend/*.csproj ./backend/

WORKDIR /src/backend

RUN dotnet restore

WORKDIR /src

COPY backend ./backend

COPY --from=frontend-build /frontend/dist/comments-app/browser ./backend/wwwroot

WORKDIR /src/backend

RUN dotnet publish SPA-app.csproj -c Release -o /app/publish


# RUN
FROM mcr.microsoft.com/dotnet/aspnet:9.0

RUN apt-get update && apt-get install -y \
    fontconfig \
    fonts-dejavu \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=backend-build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SPA-app.dll"]