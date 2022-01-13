#!/bin/bash
set -e

SECRETS_BUCKET_REGION=us-east-1
SECRETS_BUCKET_NAME=task-manager-setup-secrets
SECRETS_FILE_NAME=task-manager-service-rds-dev.txt
aws s3 --region ${SECRETS_BUCKET_REGION} cp s3://${SECRETS_BUCKET_NAME}/${SECRETS_FILE_NAME} - > secrets.txt && \
    kubectl create namespace todo-dev --dry-run=client -o yaml | kubectl apply -f - && \
    kubectl create secret generic -n todo-dev todo-dev-secret --from-env-file=secrets.txt --dry-run=client -o yaml | kubectl apply -f - && \
    kubectl apply -f ToDoServiceDev.yaml && \
    rm secrets.txt