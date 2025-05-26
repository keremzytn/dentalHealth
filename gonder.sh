#!/bin/bash

# Kullanıcıdan repo URL'si alınır
read -p "GitHub repo URL'sini gir (örnek: https://github.com/kullanici/repo.git): " REPO_URL

# Git başlatılır
git init

# Dosyalar eklenir
git add .

# Commit yapılır
git commit -m "İlk yükleme"

# Ana dal main olarak ayarlanır
git branch -M main

# Uzak repo eklenir
git remote add origin "$REPO_URL"

# Push işlemi yapılır
git push -u origin main

echo "Kodlar başarıyla GitHub'a yüklendi."
