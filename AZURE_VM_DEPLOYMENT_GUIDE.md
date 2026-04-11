# 🚀 Azure VM Deployment Guide — Finance Flow App

This guide details the complete process of deploying the Finance Flow microservices architecture onto an Azure Ubuntu VM, including Nginx configuration and troubleshooting common pitfalls.

---

## 🏗️ Phase 1: Azure VM Provisioning

### 1. VM Specifications
*   **Operating System**: Ubuntu 22.04 LTS
*   **Size**: Standard_D2ds_v6 (2 vCPU, 8GB RAM recommended for SQL Server + Microservices)
*   **Networking (NSG Rules)**:
    *   `22` (SSH) — For management.
    *   `80` (HTTP) — For Nginx.
    *   `443` (HTTPS) — For future Certbot/SSL.

### 2. Initial Server Setup
Connect via SSH and install required dependencies:
```bash
sudo apt update && sudo apt upgrade -y
sudo apt install docker.io docker-compose nginx git -y

# Enable Docker
sudo systemctl enable --now docker
sudo usermod -aG docker $USER
# Log out and log back in for permissions to take effect
```

---

## 📁 Phase 2: Deploying the Application

### 1. Clone and Prepare
```bash
git clone <your-repo-url> ~/app
cd ~/app
```

### 2. Environment Configuration (Crucial)
Edit the frontend environment file to use relative paths. This ensures the browser talks to the server IP and not "localhost".

**File**: `apps/web/.env.production`
```env
NEXT_PUBLIC_API_URL=
```

### 3. Build and Run
```bash
# Build may take 5-10 minutes due to .NET and Next.js compilations
docker-compose up --build -d
```

---

## 🌐 Phase 3: Nginx Reverse Proxy Setup

Nginx acts as the front door, routing traffic to either the Next.js frontend or the API Gateway.

### 1. Edit Config
```bash
sudo nano /etc/nginx/sites-available/default
```

### 2. Apply Routing Rules
```nginx
server {
    listen 80;

    # Frontend
    location / {
        proxy_pass http://localhost:3000;
        proxy_set_header Host $host;
    }

    # API Gateway (stripping prefixes for backend consumption)
    location /gateway-users/ {
        proxy_pass http://localhost:5000/;
    }

    location /gateway-accounts/ {
        proxy_pass http://localhost:5000/;
    }

    location /gateway-transactions/ {
        proxy_pass http://localhost:5000/;
    }

    location /gateway-obligations/ {
        proxy_pass http://localhost:5000/;
    }
}
```

---

## 🧨 Phase 4: Troubleshooting & "The Network Error" Fix

### 1. The "Localhost" Trap (Problem Faced)
**The Symptom**: Frontend loads, but login/signup fails with "Network Error".
**The Cause**: The frontend was hardcoded to call `http://localhost:5000`. In a browser, `localhost` refers to the **user's computer**, not the Azure VM.
**The Fix**: Use `NEXT_PUBLIC_API_URL=` (empty) so the browser makes relative calls to the actual server IP via Nginx.

### 2. Next.js Build-Time Variables
**Problem**: Changing `.env.production` doesn't seem to update the app.
**Reason**: Next.js bakes `NEXT_PUBLIC_` variables into the JavaScript files **at build time**.
**Solution**: You MUST rebuild the container after any env change:
```bash
docker-compose build --no-cache web
docker-compose up -d web
```

### 3. SQL Server Memory Issues
**Problem**: Database container keeps restarting.
**Solution**: Ensure your VM has at least 4GB of RAM (SQL Server requirement) or add a Swap file:
```bash
sudo fallocate -l 2G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
```

---

## 📝 Checklists

### ✅ Verification Checklist
- [ ] Browser Console shows request to `http://<vm-ip>/gateway-users/...` (No 5000 port).
- [ ] `docker ps` shows all 7-8 containers as "Up".
- [ ] Nginx status is active: `sudo systemctl status nginx`.

### 🛠️ Useful Debug Commands
*   **View Logs**: `docker-compose logs -f web` (or `apigateway`)
*   **Test Backend from Server**: `curl http://localhost:5000/api/Auth/login`
*   **Test Nginx Routing**: `curl http://localhost/gateway-users/api/Auth/login`
