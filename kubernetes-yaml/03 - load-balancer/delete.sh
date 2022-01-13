#!/bin/bash
set -e

kubectl delete -f 05-deployment.yaml
kubectl delete -f 04-jobs.yaml
kubectl delete -f 03-rbac.yaml
kubectl delete -f 02-config.yaml
kubectl delete -f 01-ns-and-sa.yaml



