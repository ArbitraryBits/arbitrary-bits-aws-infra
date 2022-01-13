#!/bin/bash
set -e

kubectl apply -f 01-ns-and-sa.yaml
kubectl apply -f 02-config.yaml
kubectl apply -f 03-rbac.yaml
kubectl apply -f 04-jobs.yaml
kubectl apply -f 05-deployment.yaml