#!/bin/bash
set -e

sudo mkdir -p /etc/letsencrypt/live/aika.cloud && \
    sudo openssl req -x509 -nodes -newkey rsa:4096 -days 1\
        -keyout '/etc/letsencrypt/live/aika.cloud/privkey.pem' \
        -out '/etc/letsencrypt/live/aika.cloud/fullchain.pem' \
        -subj '/CN=localhost'