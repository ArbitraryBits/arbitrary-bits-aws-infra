#!/bin/bash
set -e

SECRETS_BUCKET_REGION=us-east-1
SECRETS_BUCKET_NAME=task-manager-setup-secrets
SECRETS_FILE_NAME=task-manager-service-rds-prod.txt

aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${SECRETS_BUCKET_NAME}/${SECRETS_FILE_NAME} - > todoprod.env && \
    docker stack rm nginx || true && \
    docker secret rm nginx.conf || true && \
    docker secret create nginx.conf default.conf && \
    env $(cat todoprod.env | grep ^[A-Z] | xargs) docker stack deploy --with-registry-auth --compose-file compose.yaml nginx && \
    rm todoprod.env