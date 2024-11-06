docker run -it --name postgres --restart always -e POSTGRES_PASSWORD='future' -e ALLOW_IP_RANGE=0.0.0.0/0 -v /home/postgres/data:/var/lib/postgresql -p 5432:5432 -d postgres
