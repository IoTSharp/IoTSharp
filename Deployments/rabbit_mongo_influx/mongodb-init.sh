#!/bin/bash

mongo <<EOF
var config = {
    "_id": "dbrs",
    "version": 1,
    "members": [
        {
            "_id": 1,
            "host": "host.docker.internal:27017",
            "priority": 3
        },
        {
            "_id": 2,
            "host": "host.docker.internal:27018",
            "priority": 2
        },
        {
            "_id": 3,
            "host": "host.docker.internal:27019",
            "priority": 1
        }
    ]
};
rs.initiate(config, { force: true });
rs.status();
EOF