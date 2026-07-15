# PDD.ir — شرکت طراح داده پیشرو

> پلتفرم مدیریت محتوا و محصولات شرکت نرم‌افزاری بیمارستانی

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-WASM-512BD4)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)

---

## فهرست مطالب

- [معرفی پروژه](#معرفی-پروژه)
- [نحوه اجرا با Docker (توصیه شده)](#نحوه-اجرا-با-docker)
- [نحوه اجرا بدون Docker](#نحوه-اجرا-بدون-docker)
- [ساختار پوشه‌ها](#ساختار-پوشه‌ها)
- [تکنولوژی‌ها](#تکنولوژی‌ها)
- [راهنمای توسعه](#راهنمای-توسعه)

---

## معرفی پروژه

**PDD.ir** وب‌سایت رسمی شرکت **طراح داده پیشرو** — ارائه‌کننده نرم‌افزارهای بیمارستانی (HIS, CIS, RIS, MIS).

### بخش‌ها

| بخش | توضیح |
|-----|-------|
| صفحه اصلی | اسلایدر هیرو + لوگوی مشتریان + آمار + محصولات |
| محصولات | نمایش نرم‌افزارها |
| وبلاگ | مقالات |
| نمونه‌کارها | پروژه‌ها |
| پنل مدیریت | داشبورد + CRUD ها |

---

## نحوه اجرا با Docker

### پیش‌نیاز

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) نصب باشد

### اجرا

```bash
git clone https://github.com/ho3eines/Pdd.ir.git
cd Pdd.ir
docker compose up -d
```

منتظر بمانید تا:
- SQL Server راه‌اندازی شود (~30 ثانیه)
- پروژه build شود (~3-5 دقیقه اولین بار)

### دسترسی

| سرویس | آدرس |
|-------|------|
| **وب‌سایت** | `http://localhost:8080` |
| **پنل مدیریت** | `http://localhost:8080/admin` |
| **SQL Server** | `localhost:1433` |

### کاربر پیش‌فرض

| نام کاربری | رمز عبور |
|-----------|----------|
| `admin` | `admin123` |

### مدیریت Docker

```bash
# توقف
docker compose down

# حذف کامل (بجز دیتا)
docker compose down

# حذف کامل + دیتا
docker compose down -v

# لاگ‌ها
docker compose logs -f
```

### ذخیره دیتا

دیتابیس SQL Server در پوشه `sql-data/` ذخیره می‌شود. حتی اگر کانتینرها رو حذف کنید، دیتا باقی می‌ماند.

```
pdd.ir/
├── sql-data/          ← دیتابیس اینجا ذخیره می‌شود
├── Dockerfile
├── docker-compose.yml
└── ...
```

---

## نحوه اجرا بدون Docker

### پیش‌نیازها

| نرم‌افزار | نسخه |
|----------|-------|
| .NET SDK | 10.0+ |
| SQL Server | 2019+ |

### مراحل

```bash
# ۱. کلون
git clone https://github.com/ho3eines/Pdd.ir.git
cd Pdd.ir

# ۲. اجرا
dotnet run --project Pdd.ir.Server
```

### کاربر پیش‌فرض

| نام کاربری | رمز عبور |
|-----------|----------|
| `admin` | `admin123` |

> **توجه:** اسکریپت‌های SQL خودکار اجرا می‌شوند و دیتابیس ساخته می‌شود.

---

## ساختار پوشه‌ها

```
Pdd.ir/
├── Pdd.ir.Client/              # کلاینت Blazor WASM
│   ├── Pages/                  # صفحات
│   │   ├── Home.razor          # صفحه اصلی
│   │   ├── Products.razor      # محصولات
│   │   ├── Blog.razor          # وبلاگ
│   │   ├── Login.razor         # ورود
│   │   └── Admin/              # پنل مدیریت
│   ├── Shared/                 # کامپوننت‌ها
│   └── Services/               # سرویس‌ها
│
├── Pdd.ir.Server/              # سرور ASP.NET Core
│   ├── Controllers/            # REST API
│   ├── WebSocket/              # ارتباط بلادرنگ
│   ├── Services/               # سرویس‌ها
│   └── wwwroot/resource/       # اسکریپت‌های SQL
│
├── Pdd.ir.Business/            # لایه بیزینس
├── Pdd.ir.Data/                # لایه دیتا (Dapper)
│
├── Dockerfile                  # تصویر Docker
├── docker-compose.yml          # Docker Compose
├── sql-data/                   # دیتابیس (persist)
└── Pdd.ir.slnx                 # Solution
```

---

## تکنولوژی‌ها

| بخش | تکنولوژی |
|-----|----------|
| فرانت‌اند | Blazor WASM, Bootstrap 5, GSAP |
| بک‌اند | ASP.NET Core 10, Dapper |
| دیتابیس | SQL Server 2022 |
| احراز هویت | JWT |
| ارتباط | WebSocket + HTTP |
| فونت | Vazirmatn (فارسی) |

---

## راهنمای توسعه

### قوانین

| قانون | توضیح |
|-------|-------|
| Skeleton Loading | لودینگ با Skeleton |
| Modal-First | CRUD با مودال |
| Glassmorphism | استایل شیشه‌ای |
| RTL | فارسی |
| Git | هر تغییر → commit + push |

---

**شرکت طراح داده پیشرو** — [pdd.ir](https://pdd.ir)
