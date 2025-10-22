# MediaNest

A web application designed to run as a mini HomeLab host on a family mini PC.
It provides a convenient UI to manage albums, upload and browse images, and handle user access.


## ✨ Features
- 🌄 Album Service
    - Upload images file
    - Tag-based search system
    - Browse image collections with dropdown-view or page-view 

- 🔑 User System
    - Login & Registration
    - Role-based access control
    - Admin Panel for user management

## 🛳️ Deployment
MediaNest supports Docker Compose for one-click deployment.  
This will start **Blazor frontend + WebAPI + MongoDB** together.

default administrator account
username : admin
password : admin

1. Clone the repository
```bash
git clone https://github.com/tigercat0828/MediaNest.git
cd MediaNest
```
2. Create `.env` file and edit
```bash
echo ASSETS_PATH=/absolute/path/to/your/assets > .env
vi .env   # or use your favorite editor
```
3. start the services
```bash
docker compose up -d
```


For developers (rebuild after code changes):
```bash
docker compose up -d --build
```
If cache issues occur:
```bash
docker compose build --no-cache
docker compose up -d
```

## 🖼️ Screenshots


|  |  |
|------|------|
| ![login](screenshots/login.png) | ![account_panel](screenshots/account_management.png) |
| ![index](screenshots/index.png) | ![new_album](screenshots/newalbum.png) |
| ![pageview](screenshots/pageview.png) | ![pageview](screenshots/preview.png) |



## 🛠️ Tech Stack

- Blazor Server 
- ASP.NET core WebAPI
- MongoDB (document-based storage for users, albums, metadata)
- Bootstrap 5 (UI components & responsive layout)

## 📋 Prerequisites
Before running the project, make sure you have:

-  .NET 10 Runtime
-  A running MongoDB service

## 🛳️ Roadmap
- Video service (upload & stream videos)
- Musci service