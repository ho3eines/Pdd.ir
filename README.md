# PDD.ir — شرکت طراح داده پیشرو

> پلتفرم مدیریت محتوا و محصولات شرکت نرم‌افزاری بیمارستانی

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-WASM-512BD4)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)

---

## فهرست مطالب

- [معرفی پروژه](#معرفی-پروژه)
- [ویژگی‌ها](#ویژگی‌ها)
- [نحوه اجرا با Docker](#نحوه-اجرا-با-docker)
- [نحوه اجرا بدون Docker](#نحوه-اجرا-بدون-docker)
- [معماری پروژه](#معماری-پروژه)
- [ساختار پوشه‌ها](#ساختار-پوشه‌ها)
- [تکنولوژی‌ها](#تکنولوژی‌ها)
- [راهنمای توسعه](#راهنمای-توسعه)

---

## معرفی پروژه

**PDD.ir** وب‌سایت رسمی شرکت **طراح داده پیشرو** است که ارائه‌کننده نرم‌افزارهای بیمارستانی (HIS, CIS, RIS, MIS) می‌باشد.

### بخش‌های اصلی

| بخش | توضیح |
|-----|-------|
| **صفحه اصلی** | اسلایدر هیرو، لوگوی مشتریان، آمار، محصولات |
| **محصولات** | نمایش نرم‌افزارهای بیمارستانی |
| **وبلاگ** | مقالات و اخبار شرکت |
| **نمونه‌کارها** | پروژه‌های انجام شده |
| **تماس با ما** | فرم ارتباط با مشتری |
| **پنل مدیریت** | داشبورد + CRUD محصولات/بلاگ/کاربران |

---

## ویژگی‌ها

- ✅ **Blazor WebAssembly** — اپلیکیشن SPA کامل
- ✅ **WebSocket + HTTP Fallback** — ارتباط بلادرنگ
- ✅ **SQL Server + Dapper** — دسترسی سریع به دیتابیس
- ✅ **JWT Authentication** — احراز هویت امن
- ✅ **Role-Based Access** — سیستم نقش و دسترسی
- ✅ **Bootstrap 5 + Glassmorphism** — طراحی مدرن
- ✅ **RTL Support** — پشتیبانی کامل فارسی
- ✅ **Dark/Light Theme** — تم تاریک و روشن
- ✅ **GSAP Animations** — انیمیشن‌های حرفه‌ای
- ✅ **Docker Ready** — اجرای آسان با Docker

---

## نحوه اجرا با Docker

> **توصیه شده:** ساده‌ترین روش اجرا، استفاده از Docker است.

### پیش‌نیاز

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) نصب باشد

### اجرا

```bash
# ۱. کلون کردن ریپازیتوری
git clone https://github.com/ho3eines/Pdd.ir.git
cd Pdd.ir

# ۲. اجرای Docker Compose
docker compose up -d
```

منتظر بمانید تا：
- دیتابیس SQL Server راه‌اندازی شود (~30 ثانیه)
- پروژه build و اجرا شود (~2-3 دقیقه)

### دسترسی

| سرویس | آدرس |
|-------|------|
| **وب‌سایت** | [http://localhost:8080](http://localhost:8080) |
| **پنل مدیریت** | [http://localhost:8080/admin](http://localhost:8080/admin) |
| **SQL Server** | `localhost:1433` |

### کاربر پیش‌فرض

| نام کاربری | رمز عبور |
|-----------|----------|
| `admin` | `admin123` |

### مدیریت Docker

```bash
# توقف سرویس‌ها
docker compose down

# حذف سرویس‌ها + دیتا
docker compose down -v

# مشاهده لاگ‌ها
docker compose logs -f

# مشاهده وضعیت
docker compose ps
```

---

## نحوه اجرا بدون Docker

### پیش‌نیازها

| نرم‌افزار | نسخه |
|----------|-------|
| .NET SDK | 8.0+ |
| SQL Server | 2019+ |

### مراحل

```bash
# ۱. کلون کردن
git clone https://github.com/ho3eines/Pdd.ir.git
cd Pdd.ir

# ۲. باز کردن در Visual Studio
# فایل Pdd.ir.slnx را باز کنید

# ۳. تنظیم رشته اتصال
# فایل Pdd.ir.Server/appsettings.json را ویرایش کنید

# ۴. اجرا
# پروژه Pdd.ir.Server را به عنوان Startup انتخاب کنید
# یا:
dotnet run --project Pdd.ir.Server

# ۵. اجرای اسکریپت‌های SQL
# فایل‌های SQL در مسیر زیر هستند:
# Pdd.ir.Server/wwwroot/resource/
# ابتدا 202607111200_Create_All.sql
# سپس بقیه فایل‌ها به ترتیب تاریخ
```

---

## معماری پروژه

```
┌─────────────────────────────────────────────────────┐
│               Pdd.ir.Client (Blazor WASM)           │
│  Pages → Shared → Services → Models                 │
└──────────────────────┬──────────────────────────────┘
                       │ WebSocket / HTTP
┌──────────────────────┴──────────────────────────────┐
│               Pdd.ir.Server (ASP.NET Core)           │
│  Controllers → WebSocket → Services                  │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────┴──────────────────────────────┐
│               Pdd.ir.Data (Dapper)                   │
│  DbService → Queries → Models                        │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────┴──────────────────────────────┐
│               Pdd.ir.Business (Logic)                │
│  Services → Models → Validators                      │
└─────────────────────────────────────────────────────┘
```

---

## ساختار پوشه‌ها

```
Pdd.ir/
├── Pdd.ir.Client/              # کلاینت Blazor WASM
│   ├── Pages/                  # صفحات
│   │   ├── Home.razor          # صفحه اصلی
│   │   ├── Products.razor      # محصولات
│   │   ├── Blog.razor          # وبلاگ
│   │   ├── Portfolio.razor     # نمونه‌کارها
│   │   ├── Contact.razor       # تماس با ما
│   │   ├── Login.razor         # ورود
│   │   └── Admin/              # پنل مدیریت
│   ├── Shared/                 # کامپوننت‌های مشترک
│   ├── Services/               # سرویس‌های کلاینت
│   └── Layout/                 # لایوت‌ها
│
├── Pdd.ir.Server/              # سرور ASP.NET Core
│   ├── Controllers/            # REST API
│   ├── WebSocket/              # ارتباط بلادرنگ
│   ├── Services/               # سرویس‌های سرور
│   └── wwwroot/resource/       # اسکریپت‌های SQL
│
├── Pdd.ir.Business/            # لایه منطق کسب‌وکار
│   ├── Services/               # سرویس‌های بیزینس
│   └── Models/                 # مدل‌ها
│
├── Pdd.ir.Data/                # لایه دسترسی به داده
│   ├── DbService.cs            # اتصال دیتابیس
│   └── Queries/                # کوئری‌های TSQL
│
├── Dockerfile                  # فایل Docker
├── docker-compose.yml          # Docker Compose
├── init-db.sql                 # اسکریپت ایجاد دیتابیس
└── Pdd.ir.slnx                 # فایل Solution
```

---

## تکنولوژی‌ها

### فرانت‌اند
| تکنولوژی | کاربرد |
|----------|--------|
| Blazor WASM | فریمورک UI |
| Bootstrap 5 | CSS Framework |
| GSAP | انیمیشن‌ها |
| Vazirmatn | فونت فارسی |

### بک‌اند
| تکنولوژی | کاربرد |
|----------|--------|
| ASP.NET Core 8 | وب سرور |
| Dapper | ORM |
| SQL Server | دیتابیس |
| JWT | احراز هویت |
| WebSocket | ارتباط بلادرنگ |

---

## راهنمای توسعه

### الگوی اضافه کردن ماژول جدید

```bash
# ۱. مدل در Pdd.ir.Business/Models
# ۲. کوئری در Pdd.ir.Data/Queries
# ۳. سرویس در Pdd.ir.Business/Services
# ۴. کنترلر در Pdd.ir.Server/Controllers
# ۵. صفحه در Pdd.ir.Client/Pages/Admin
# ۶. ثبت سرویس در Pdd.ir.Server/Program.cs
```

### قوانین توسعه

| قانون | توضیح |
|-------|-------|
| **Skeleton Loading** | لودینگ با Skeleton (نه spinner) |
| **Modal-First** | CRUD ها با مودال |
| **Glassmorphism** | استایل شیشه‌ای |
| **RTL** | تمام متن‌ها فارسی |
| **i18n** | استفاده از `T.Text()` |
| **Git** | هر تغییر → commit + push |

---

## مجوز

متعلق به شرکت **طراح داده پیشرو** — [pdd.ir](https://pdd.ir)
