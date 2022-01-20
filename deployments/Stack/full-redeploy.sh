#!/bin/bash
set -e

SECRETS_BUCKET_REGION=us-east-1
SECRETS_BUCKET_NAME=task-manager-setup-secrets
SECRETS_FILE_NAME=task-manager-service-rds-prod.txt

aws ecr get-login-password --region us-east-1 | \
    docker login --username AWS --password-stdin 437377620726.dkr.ecr.us-east-1.amazonaws.com &&
    aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${SECRETS_BUCKET_NAME}/${SECRETS_FILE_NAME} - > todoprod.env && \
    docker stack rm abstack || true && \
    docker secret rm nginx.conf || true && \
    docker secret create nginx.conf default.conf && \
    docker secret rm options-ssl-nginx.conf || true && \
    docker secret create options-ssl-nginx.conf options-ssl-nginx.conf && \
    docker secret rm ssl-dhparams.pem || true && \
    docker secret create ssl-dhparams.pem ssl-dhparams.pem && \
    env $(cat todoprod.env | grep ^[A-Z] | xargs) docker stack deploy --with-registry-auth --compose-file compose.yaml abstack && \
    rm todoprod.env