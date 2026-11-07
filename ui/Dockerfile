# مرحله ۱: بیلد کردن React
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# مرحله ۲: سرو کردن با Nginx
FROM nginx:alpine
# کپی کردن فایل‌های بیلد شده
COPY --from=build /app/dist /usr/share/nginx/html
# کپی کردن تنظیمات Nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]