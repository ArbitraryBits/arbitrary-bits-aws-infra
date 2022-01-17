#!/bin/bash
set -e

sudo mkdir -p /etc/letsencrypt/live/test.aika.cloud && \
    sudo openssl req -x509 -nodes -newkey rsa:4096 -days 1\
        -keyout '/etc/letsencrypt/live/test.aika.cloud/privkey.pem' \
        -out '/etc/letsencrypt/live/test.aika.cloud/fullchain.pem' \
        -subj '/CN=localhost'