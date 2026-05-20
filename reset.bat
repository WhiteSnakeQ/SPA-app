@echo off

echo Stopping containers...

docker compose down -v

echo Starting containers...

docker compose up -d --build