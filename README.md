# .NET 10 GitOps Application

Simple .NET 10 minimal API for GitOps study with AKS deployment.

## Endpoint

- `GET /api/info` - Returns name, email, and version
- `GET /health` - Health check

## Local Development

```bash
dotnet run
```

## Setup Azure Container Registry & AKS

### 1. Create Azure Resources

```bash
# Variables
RESOURCE_GROUP="rg-gitops-demo"
LOCATION="eastus"
ACR_NAME="<your-unique-acr-name>"
AKS_NAME="aks-gitops-demo"

# Create Resource Group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create ACR
az acr create --resource-group $RESOURCE_GROUP --name $ACR_NAME --sku Basic

# Create AKS
az aks create \
  --resource-group $RESOURCE_GROUP \
  --name $AKS_NAME \
  --node-count 2 \
  --attach-acr $ACR_NAME \
  --generate-ssh-keys

# Get AKS credentials
az aks get-credentials --resource-group $RESOURCE_GROUP --name $AKS_NAME
```

### 2. Configure GitHub Secrets

Add these secrets to your GitHub repository:

- `ACR_USERNAME`: Azure Container Registry username
- `ACR_PASSWORD`: Azure Container Registry password

```bash
# Get ACR credentials
az acr credential show --name $ACR_NAME
```

### 3. Update Configuration

Update the following files with your ACR name:
- `.github/workflows/ci-cd.yml` - Line 9
- `k8s/values.yaml` - Line 4

### 4. Install Flux v2 on AKS

```bash
# Install Flux extension
az k8s-extension create \
  --cluster-type managedClusters \
  --cluster-name $AKS_NAME \
  --resource-group $RESOURCE_GROUP \
  --name flux \
  --extension-type microsoft.flux

# Create Flux configuration
az k8s-configuration flux create \
  --resource-group $RESOURCE_GROUP \
  --cluster-name $AKS_NAME \
  --cluster-type managedClusters \
  --name gitops-config \
  --namespace flux-system \
  --scope cluster \
  --url https://github.com/<your-username>/dotnet-gitops-app \
  --branch main \
  --kustomization name=apps path=./k8s prune=true
```

### 5. Deploy

Push code to GitHub main branch - the pipeline will automatically:
1. Build the Docker image
2. Push to Azure Container Registry
3. Update the Helm values with new image tag
4. Flux will detect changes and deploy to AKS

## Verify Deployment

```bash
# Check pods
kubectl get pods

# Check service
kubectl get svc dotnet-gitops-app

# Get external IP
kubectl get svc dotnet-gitops-app -o jsonpath='{.status.loadBalancer.ingress[0].ip}'

# Test the API
curl http://<EXTERNAL-IP>/api/info
```
