` docker pull truevoly/oracle-12c 1`

` docker run -d -p 8080:8080 -p 1521:1521 truevoly/oracle-12c `

`
hostname: localhost #主机名
port: 1521 #端口号
sid: xe 
service name: xe #服务名
username: system #用户名
password: oracle #密码
`
