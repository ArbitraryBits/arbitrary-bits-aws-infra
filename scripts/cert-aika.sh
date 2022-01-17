#!/bin/bash
set -e

# --staging
sudo certbot certonly --webroot --agree-tos -w /home/ubuntu/certbot/www --email reloni@ya.ru -d test.aika.cloud --rsa-key-size 4096