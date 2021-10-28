docker build -t iotsharp/iotsharp   .  -f  IoTSharp/Dockerfile
docker push iotsharp/iotsharp 
docker image prune -f