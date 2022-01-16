#!/bin/bash
set -e

docker swarm init && \
    yes | docker network rm ingress && \
    sleep 5 && \
    docker network create --driver overlay --ingress --subnet=10.11.0.0/16 --gateway=10.11.0.2 --opt com.docker.network.driver.mtu=1200 ingress