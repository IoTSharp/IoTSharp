docker-compose  pull
docker-compose  up -d
sleep 20
docker exec mongodb1 /bin/sh /scripts/mongodb-init.sh
