docker build -t registry.uixe.net/uixe/iotsharp  .  -f  IoTSharp/Dockerfile_with_vue
docker push  registry.uixe.net/uixe/iotsharp
docker image prune -f