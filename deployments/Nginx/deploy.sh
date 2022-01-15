#!/bin/bash
set -e

SECRETS_BUCKET_REGION=us-east-1
SECRETS_BUCKET_NAME=task-manager-setup-secrets
SECRETS_FILE_NAME=task-manager-service-rds-prod.txt
REPO=437377620726.dkr.ecr.us-east-1.amazonaws.com
TODOPRODIMAGE=todo-service:1.3.0-release-2-release

aws ecr get-login-password --region us-east-1 | \
    docker login --username AWS --password-stdin $REPO && \
    docker pull $REPO/$TODOPRODIMAGE && \
    aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${SECRETS_BUCKET_NAME}/${SECRETS_FILE_NAME} - > todoprod.env && \
    docker stack rm nginx || true && \
    docker secret rm nginx.conf || true && \
    docker secret create nginx.conf default.conf && \
    env $(cat todoprod.env | grep ^[A-Z] | xargs) docker stack deploy --compose-file compose.yaml nginx && \
    rm todoprod.env

    