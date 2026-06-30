docker build -t registry.uixe.net/iotsharp/iotsharp:latest  .  -f  IoTSharp/Dockerfile
docker push registry.uixe.net/iotsharp/iotsharp:latest
docker image prune -f
