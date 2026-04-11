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

## 🌐 Phase 3: Nginx Reverse Proxy Setup (Full Config)

Nginx must be configured to pass the **full path** to the API Gateway and handle Swagger docs.

### 1. Edit Config
```bash
sudo nano /etc/nginx/sites-available/default
```

### 2. Full Configuration
Delete the existing contents and paste this block:

```nginx
server {
    listen 80;

    # 1. API Documentation (Swagger)
    location /swagger/ {
        proxy_pass http://localhost:5000/;
        proxy_set_header Host $host;
    }

    location /swagger/v1/ {
        proxy_pass http://localhost:5000/swagger/v1/;
        proxy_set_header Host $host;
    }

    # 2. API Gateway routing for services
    location /gateway-users/ {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
    }

    location /gateway-accounts/ {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
    }

    location /gateway-transactions/ {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
    }

    location /gateway-obligations/ {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
    }

    # 3. Frontend - Next.js (Matches everything else)
    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### 3. Test and Restart
```bash
sudo nginx -t
sudo systemctl restart nginx
```

---

## 🧨 Phase 4: Troubleshooting & "The Network Error" Fix

### 1. The "Localhost" Trap (Problem Faced)
**The Symptom**: Login fails with "Network Error".
**The Cause**: Frontend called `localhost:5000`.
**The Fix**: Use `NEXT_PUBLIC_API_URL=` (empty).

### 2. 404 Not Found on API
**The Cause**: Using `proxy_pass http://localhost:5000/;` (with slash) stripped the prefix.
**The Fix**: Use `proxy_pass http://localhost:5000;` (without slash) to keep the prefix required by the .NET Gateway.

### 3. Swagger Assets Not Loading
**The Cause**: Swagger UI assets are served from the root of port 5000.
**The Fix**: Added `location /swagger/` with a trailing slash in `proxy_pass` to point directly to the Gateway's root UI.

### 4. Nginx Config Fail ("location directive not allowed")
**The Cause**: Pasting `location` blocks outside a `server` block.
**The Fix**: Wrap everything in `server { ... }`.

---

## 📝 Checklists

### ✅ Verification Checklist
- [ ] Browser Console shows request to `http://<vm-ip>/gateway-users/...`
- [ ] `docker ps` shows all containers as "Up".
- [ ] Swagger is accessible at `http://<vm-ip>/swagger/`.
- [ ] Web App is accessible at `http://<vm-ip>/`.
