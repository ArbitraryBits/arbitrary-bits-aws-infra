#!/bin/bash
set -e

#--test-cert
sudo rm -rf /etc/letsencrypt/live/aika.cloud && \
    sudo certbot certonly --webroot --agree-tos -w /home/ubuntu/certbot/www --email reloni@ya.ru -d aika.cloud --rsa-key-size 4096 && \
    docker exec -it $(docker ps | grep abstack_nginx | awk '{print $1;}') nginx -s reload