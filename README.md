# PDD.ir — شرکت طراح داده پیشرو

> پلتفرم مدیریت محتوا و محصولات شرکت طراح داده پیشرو

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-WASM-512BD4)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927)
![Bootstrap](https://img.shields.io/badge/Bootstrap_5-563D7C?logo=bootstrap&logoColor=white)

---

## فهرست مطالب

- [معرفی پروژه](#معرفی-پروژه)
- [امکانات پروژه](#امکانات-پروژه)
- [نصب و راه‌اندازی در ویندوز](#نصب-و-راه‌اندازی-در-ویندوز)
- [تنظیم دیتابیس](#تنظیم-دیتابیس)
- [اجرای پروژه](#اجرای-پروژه)
- [ساختار پوشه‌ها](#ساختار-پوشه‌ها)
- [ساختار جداول دیتابیس](#ساختار-جداول-دیتابیس)
- [معماری ارتباط Client/Server](#معماری-ارتباط-clientserver)
- [راهنمای ساخت Entity جدید](#راهنمای-ساخت-entity-جدید)
- [فهرست ماژول‌ها و فایل‌ها](#فهرست-ماژولها-و-فایلها)
- [تکنولوژی‌ها](#تکنولوژیها)
- [قوانین توسعه](#قوانین-توسعه)

---

## معرفی پروژه

**PDD.ir** وب‌سایت رسمی شرکت **طراح داده پیشرو** — ارائه‌کننده نرم‌افزارهای بیمارستانی (HIS, CIS, RIS, MIS).

### بخش‌های سایت

| بخش | توضیح |
|-----|-------|
| صفحه اصلی | اسلایدر هیرو + لوگوی مشتریان + آمار + محصولات |
| محصولات | نمایش نرم‌افزارها |
| وبلاگ | مقالات و اخبار |
| نمونه‌کارها | پروژه‌ها |
| رویدادها | رویدادها و همایش‌ها |
| مشتریان | بیمارستان‌ها و مراکز درمانی |
| تماس | فرم تماس |
| پنل مدیریت | داشبورد + تمام CRUD‌ها |

### پنل مدیریت (Admin)

| ماژول | عملیات |
|-------|--------|
| داشبورد | نمای آمار کلی |
| محصولات | ایجاد، ویرایش، حذف |
| وبلاگ | ایجاد، ویرایش، حذف |
| نمونه‌کارها | ایجاد، ویرایش، حذف |
| مشتریان | ایجاد، ویرایش، حذف |
| رویدادها | ایجاد، ویرایش، حذف |
| اسلایدرها | ایجاد، ویرایش، حذف |
| محصولات صفحه اصلی | ایجاد، ویرایش، حذف |
| پیام‌ها | مشاهده، حذف، علامت خوانده شده |
| کاربران | ایجاد، ویرایش، حذف |
| نقش‌ها | مدیریت دسترسی‌ها |
| تنظیمات | تنظیمات سایت |

---

## امکانات پروژه

- **SPA کامل** — Blazor WebAssembly با رندر سمت کلاینت
- **ارتباط بلادرنگ** — WebSocket + HTTP Fallback
- **امنیت** — رمزنگاری AES-256-CBC + JWT Auth
- **CRUD خودکار** — اضافه کردن Entity جدید فقط با ۳ خط کد
- **رابط کاربری فارسی** — RTL + فونت Vazirmatn
- **طراحی Glassmorphism** — تم تاریک و روشن
- **جدول هوشمند** — PddTable با جستجو، صفحه‌بندی، و نمای ریسپانسیو
- **آپلود تصویر** — آپلود با GUID ذخیره در سرور
- **تقویم شمسی** — PersianDatePicker برای تاریخ‌ها
- **ترجمه چندزبانه** — fa.json + en.json
- **Skeleton Loading** — لودینگ زیبا بدون spinner
- **انیمیشن‌ها** — GSAP + CSS Animations

---

## نصب و راه‌اندازی در ویندوز

### پیش‌نیازها

| نرم‌افزار | نسخه | لینک دانلود |
|----------|-------|------------|
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download/dotnet/10.0 |
| SQL Server | 2019+ | https://www.microsoft.com/en-us/sql-server/sql-server-downloads |
| SQL Server Management Studio | آخرین نسخه | https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms |
| Visual Studio 2022 | 17.10+ | https://visualstudio.microsoft.com/downloads/ |

### مرحله ۱: نصب .NET SDK

1. فایل نصب .NET SDK 10.0 را دانلود و اجرا کنید
2. بعد از نصب، در PowerShell اجرا کنید:
```powershell
dotnet --version
# خروجی باید: 10.0.xxx
```

### مرحله ۲: نصب SQL Server

1. SQL Server Express (رایگان) را نصب کنید
2. در هنگام نصب، **Authentication Mode** را روی **Mixed Mode** بگذارید
3. رمز عبور SA را تنظیم کنید (مثلاً: `123456`)
4. SQL Server را روی پورت پیش‌فرض (`localhost`) نصب کنید

### مرحله ۳: تنظیم SQL Server در PowerShell

```powershell
# بررسی اتصال به SQL Server
sqlcmd -S . -U sa -P 123456 -Q "SELECT @@VERSION"
```

اگر خطا گرفتید، مطمئن شوید سرویس SQL Server در حال اجراست:
```powershell
Get-Service MSSQLSERVER
# اگر متوقف بود:
Start-Service MSSQLSERVER
```

---

## تنظیم دیتابیس

### مرحله ۱: ساخت دیتابیس

```sql
-- در SSMS یا sqlcmd اجرا کنید
CREATE DATABASE pdd;
GO
```

یا از PowerShell:
```powershell
sqlcmd -S . -U sa -P 123456 -Q "CREATE DATABASE pdd;"
```

### مرحله ۲: تنظیم Connection String

فایل `Pdd.ir.Server/appsettings.json` را ویرایش کنید:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "workstation id=support;password=123456;packet size=4096;user id=sa;data source=.;persist security info=false;initial catalog=pdd;Encrypt=False"
  },
  "ApiKey": "pdd-ir-ws-2026-secure-key"
}
```

**تغییرات لازم:**
- `password` → رمز عبور SQL Server شما
- `data source` → آدرس سرور SQL (`.` = لوکال)
- `initial catalog` → نام دیتابیس (`pdd`)
- `ApiKey` → کلید اشتراکی رمزنگاری (ثابت بگذارید)

### مرحله ۳: اجرای خودکار اسکریپت‌ها

اسکریپت‌های SQL در پوشه `Pdd.ir.Server/wwwroot/resource/` به صورت خودکار اجرا می‌شوند. این اسکریپت‌ها تمام جداول را می‌سازند.

**اسکریپت‌های موجود:**

| فایل | توضیح |
|------|-------|
| `202607111200_Create_All.sql` | جداول اصلی (Users, Products, Blog, Contact, Pages, Settings) |
| `202607111400_Create_Blog.sql` | جدول وبلاگ |
| `202607111401_Create_Portfolio.sql` | جدول نمونه‌کارها |
| `202607131200_Add_Permissions.sql` | جداول نقش و دسترسی |
| `202607141200_Fix_Admin_Password.sql` | رمز عبور ادمین پیش‌فرض |
| `202607191800_Create_AuthSessions.sql` | جدول نشست‌های احراز هویت |
| `202607191800_Create_ClientSessions.sql` | جدول نشست‌های کلاینت |
| `202607222400_Create_Events.sql` | جدول رویدادها |
| `202607241000_Create_Clients_Clean.sql` | جدول مشتریان |
| `202607241100_Create_HomeSlides.sql` | جدول اسلایدرها |
| `202607241200_Create_HomeProducts.sql` | جدول محصولات صفحه اصلی |

> **نکته:** اسکریپت‌ها فقط یکبار اجرا می‌شوند و با `IF OBJECT_ID ... IS NULL` ایمن هستند.

---

## اجرای پروژه

### روش ۱: Visual Studio

1. فایل `Pdd.ir.slnx` را با Visual Studio 2022 باز کنید
2. پروژه `Pdd.ir.Server` را به عنوان پروژه Startup تنظیم کنید
3. کلید `F5` را بزنید

### روش ۲: خط فرمان

```powershell
# کلون پروژه
git clone https://github.com/ho3eines/Pdd.ir.git
cd Pdd.ir

# اجرای سرور (کلاینت خودکار build می‌شود)
dotnet run --project Pdd.ir.Server
```

### دسترسی

| سرویس | آدرس |
|-------|------|
| **وب‌سایت** | `http://localhost:5000` |
| **پنل مدیریت** | `http://localhost:5000/admin` |
| **ورود** | `http://localhost:5000/login` |

### کاربر پیش‌فرض

| نام کاربری | رمز عبور |
|-----------|----------|
| `admin` | `admin123` |

---

## ساختار پوشه‌ها

```
Pdd.ir/
├── Pdd.ir.Client/                    # کلاینت Blazor WASM
│   ├── Program.cs                    # ثبت سرویس‌ها
│   ├── App.razor                     # ریشه برنامه
│   ├── Layout/
│   │   ├── MainLayout.razor          # لایوت اصلی (Admin vs Public)
│   │   └── NavMenu.razor             # سایدبار مدیریت
│   ├── Pages/
│   │   ├── Home.razor                # صفحه اصلی
│   │   ├── Products.razor            # محصولات
│   │   ├── Blog.razor                # وبلاگ
│   │   ├── BlogDetail.razor          # مقاله
│   │   ├── Portfolio.razor           # نمونه‌کارها
│   │   ├── Events.razor              # رویدادها
│   │   ├── About.razor               # درباره ما
│   │   ├── Contact.razor             # تماس
│   │   ├── Login.razor               # ورود
│   │   ├── NotFound.razor            # 404
│   │   └── Admin/                    # پنل مدیریت
│   │       ├── Dashboard.razor       # داشبورد
│   │       ├── Products.razor        # مدیریت محصولات
│   │       ├── BlogAdmin.razor       # مدیریت وبلاگ
│   │       ├── PortfolioAdmin.razor  # مدیریت نمونه‌کارها
│   │       ├── Clients.razor         # مدیریت مشتریان
│   │       ├── Events.razor          # مدیریت رویدادها
│   │       ├── HomeSlides.razor      # مدیریت اسلایدرها
│   │       ├── HomeProducts.razor    # مدیریت محصولات صفحه اصلی
│   │       ├── Messages.razor        # مدیریت پیام‌ها
│   │       ├── Users.razor           # مدیریت کاربران
│   │       ├── Roles.razor           # مدیریت نقش‌ها
│   │       └── Settings.razor        # تنظیمات
│   ├── Shared/
│   │   ├── Components/
│   │   │   ├── PddTable.razor        # جدول هوشمند با جستجو و صفحه‌بندی
│   │   │   ├── Modal.razor           # مودال سراسری
│   │   │   ├── SearchableList.razor  # لیست dropdown با جستجو
│   │   │   ├── PersianDatePicker.razor # تقویم شمسی
│   │   │   ├── CKEditorBlazor.razor  # ویرایشگر متن غنی
│   │   │   ├── FileUpload.razor      # آپلود فایل و تصویر
│   │   │   ├── BootstrapNumericInput.razor # ورودی عدد فرمت‌دار
│   │   │   └── Skeleton.razor        # اسکلت لودینگ
│   │   └── Dialogs/                  # دیالوگ‌های CRUD
│   │       ├── ProductDialog.razor
│   │       ├── BlogDialog.razor
│   │       ├── PortfolioDialog.razor
│   │       ├── ClientDialog.razor
│   │       ├── EventDialog.razor
│   │       ├── HomeSlideDialog.razor
│   │       ├── HomeProductDialog.razor
│   │       ├── ContactMessageDialog.razor
│   │       ├── MessageViewDialog.razor
│   │       ├── PasswordDialog.razor
│   │       ├── UserDialog.razor
│   │       └── RoleDialog.razor
│   ├── Services/
│   │   ├── ICommunicationService.cs  # رابط ارتباط
│   │   ├── CommunicationService.cs   # ارتباط WS + HTTP
│   │   ├── SecurityService.cs        # Handshake + Session + Auth Header
│   │   ├── EncryptionService.cs      # رمزنگاری AES
│   │   ├── PddEncryptionService.cs   # رمزنگاری PDD
│   │   ├── AuthService.cs            # احراز هویت
│   │   ├── TranslateService.cs       # ترجمه
│   │   ├── ITranslateService.cs      # رابط ترجمه
│   │   ├── FileUploadService.cs      # آپلود فایل
│   │   ├── ModalService.cs           # مدیریت مودال
│   │   ├── IModalService.cs          # رابط مودال
│   │   ├── AlertService.cs           # Toast notification
│   │   ├── IAlertService.cs          # رابط Alert
│   │   ├── AppStateService.cs        # وضعیت برنامه
│   │   ├── IAppStateService.cs       # رابط AppState
│   │   ├── ClientStorageService.cs   # LocalStorage/Session/Cookie
│   │   ├── IClientStorageService.cs  # رابط Storage
│   │   ├── IEncryptionService.cs     # رابط رمزنگاری
│   │   ├── ConnectionService.cs      # مدیریت اتصال
│   │   ├── AnimationService.cs       # انیمیشن‌ها
│   │   └── ApiClient.cs              # کلاینت API
│   ├── Models/                       # DTOهای کلاینت
│   └── wwwroot/
│       ├── css/app.css               # فایل CSS واحد
│       ├── lang/fa.json              # ترجمه فارسی
│       ├── lang/en.json              # ترجمه انگلیسی
│       └── js/                       # فایل‌های JavaScript
│
├── Pdd.ir.Server/                    # سرور ASP.NET Core
│   ├── Program.cs                    # نقطه شروع + ثبت سرویس‌ها
│   ├── Controllers/                  # REST API Controllers
│   │   ├── AuthController.cs         # احراز هویت (لاگین/لاگاوت)
│   │   ├── UserController.cs         # مدیریت کاربران
│   │   ├── RoleController.cs         # مدیریت نقش‌ها
│   │   ├── PermissionController.cs   # مدیریت دسترسی‌ها
│   │   ├── ProductController.cs      # محصولات
│   │   ├── BlogController.cs         # وبلاگ
│   │   ├── PortfolioController.cs    # نمونه‌کارها
│   │   ├── ClientController.cs       # مشتریان
│   │   ├── EventController.cs        # رویدادها
│   │   ├── ContactController.cs      # پیام‌ها
│   │   ├── PageController.cs         # صفحات
│   │   ├── SettingsController.cs     # تنظیمات
│   │   ├── HomeSlideController.cs    # اسلایدرها
│   │   ├── HomeProductController.cs  # محصولات صفحه اصلی
│   │   ├── UploadController.cs       # آپلود فایل
│   │   ├── ImageController.cs        # مدیریت تصاویر
│   │   ├── FileController.cs         # مدیریت فایل‌ها
│   │   └── ClientImageController.cs  # تصاویر مشتریان
│   ├── WebSocket/
│   │   └── WebSocketHandler.cs       # مدیریت ارتباط WebSocket
│   ├── Services/
│   │   ├── SessionAuthAttribute.cs   # فیلتر احراز هویت
│   │   ├── RequestDecryptionMiddleware.cs  # رمزگشایی درخواست‌ها
│   │   ├── ResponseEncryptionMiddleware.cs # رمزنگاری پاسخ‌ها
│   │   ├── JwtService.cs             # سرویس JWT
│   │   ├── CryptoJsService.cs        # سرویس رمزنگاری
│   │   ├── AesKeyStore.cs            # ذخیره کلید AES
│   │   ├── ConnectionManager.cs      # مدیریت اتصالات WebSocket
│   │   ├── AuthService.cs            # سرویس احراز هویت
│   │   └── ScriptExecutor.cs         # اجرای خودکار اسکریپت‌ها
│   └── wwwroot/resource/             # اسکریپت‌های SQL
│
├── Pdd.ir.Business/                  # لایه بیزینس
│   ├── Models/
│   │   ├── Entities/                 # مدل‌های دیتابیس
│   │   └── DTOs/                     # Data Transfer Objects
│   └── Services/
│       ├── AuthBusinessService.cs    # احراز هویت
│       ├── UserBusinessService.cs    # کاربران
│       ├── ProductBusinessService.cs # محصولات
│       ├── BlogBusinessService.cs    # وبلاگ
│       ├── PortfolioBusinessService.cs # نمونه‌کارها
│       ├── ClientBusinessService.cs  # مشتریان
│       ├── EventBusinessService.cs   # رویدادها
│       ├── ContactBusinessService.cs # پیام‌ها
│       ├── PageBusinessService.cs    # صفحات
│       ├── SettingsBusinessService.cs # تنظیمات
│       ├── RolePermissionBusinessService.cs # نقش و دسترسی
│       ├── HomeSlideBusinessService.cs # اسلایدرها
│       └── HomeProductBusinessService.cs # محصولات صفحه اصلی
│
├── Pdd.ir.Data/                      # لایه دیتا (Dapper)
│   ├── IDbService.cs                 # رابط دیتابیس
│   ├── DbService.cs                  # پیاده‌سازی Dapper
│   └── Queries/
│       ├── UserQueries.cs            # کوئری کاربران
│       ├── ProductQueries.cs         # کوئری محصولات
│       ├── BlogQueries.cs            # کوئری وبلاگ
│       ├── PortfolioQueries.cs       # کوئری نمونه‌کارها
│       ├── ClientQueries.cs          # کوئری مشتریان
│       ├── EventQueries.cs           # کوئری رویدادها
│       ├── ContactQueries.cs         # کوئری پیام‌ها
│       ├── PageQueries.cs            # کوئری صفحات
│       ├── RolePermissionQueries.cs  # کوئری نقش و دسترسی
│       ├── HomeSlideQueries.cs       # کوئری اسلایدرها
│       └── HomeProductQueries.cs     # کوئری محصولات صفحه اصلی
│
├── Pdd.ir.Tests/                     # تست‌ها
│
├── Pdd.ir.slnx                       # Solution فایل
├── AGENTS.md                         # قوانین توسعه
└── README.md                         # این فایل
```

---

## ساختار جداول دیتابیس

### جداول اصلی

| جدول | فیلدهای کلیدی | توضیح |
|------|--------------|-------|
| `Users` | Id, Username, PasswordHash, FullName, Role, IsActive | کاربران |
| `Roles` | Id, Name, Description, Permissions | نقش‌ها |
| `Products` | Id, Title, TitleEn, Description, ImageUrl, Price, IsActive | محصولات |
| `BlogPosts` | Id, Title, TitleEn, Content, ImageUrl, Category, IsActive | مقالات |
| `PortfolioItems` | Id, Title, TitleEn, Description, ImageUrl, Category, IsActive | نمونه‌کارها |
| `ContactMessages` | Id, FullName, Email, Phone, Subject, Message, IsRead, IsActive | پیام‌ها |
| `Pages` | Id, Title, Slug, Content, IsActive | صفحات |
| `Settings` | Id, Key, Value | تنظیمات |

### جداول محتوایی

| جدول | فیلدهای کلیدی | توضیح |
|------|--------------|-------|
| `Events` | Id, Title, TitleEn, Description, ImageUrl, Location, EventDate, SortOrder, IsActive | رویدادها |
| `Clients` | Id, Name, NameEn, ImageUrl, Website, IsActive | مشتریان |
| `HomeSlides` | Id, Title, Subtitle, ImageUrl, SortOrder, IsActive | اسلایدرها |
| `HomeProducts` | Id, Title, TitleEn, Description, ImageUrl, SortOrder, IsActive | محصولات صفحه اصلی |

### جداول امنیتی

| جدول | فیلدهای کلیدی | توضیح |
|------|--------------|-------|
| `AuthSessions` | Id, UserId, Token, ExpiresAt | نشست‌های احراز هویت |
| `ClientSessions` | Id, SessionKey, Data, ExpiresAt | نشست‌های کلاینت |

### نکات مهم

- **تاریخ‌ها:** از `BIGINT` (Unix timestamp) استفاده می‌شود نه `DATETIME`
- **IDs:** `INT IDENTITY` خودکار
- **فعال/غیرفعال:** فیلد `IsActive` به صورت `BIT DEFAULT 1`
- **مرتب‌سازی:** فیلد `SortOrder` برای ترتیب نمایش

---

## معماری ارتباط Client/Server

### روش ارتباط

```
┌──────────────┐                    ┌──────────────┐
│  Blazor WASM │  ◄── WebSocket ──► │  ASP.NET Core│
│   (Client)   │  ◄── HTTP REST ──► │   (Server)   │
└──────────────┘                    └──────────────┘
                                             │
                                      ┌──────┴──────┐
                                      │  SQL Server  │
                                      │   (Dapper)   │
                                      └─────────────┘
```

### اولویت ارتباط

1. **WebSocket** — اگر وصل باشد → همه درخواست‌ها از WS
2. **HTTP Fallback** — اگر WS قطع باشد → همه از HTTP
3. **Auth Endpoints** — `/auth/*` همیشه HTTP

### نحوه ارتباط در صفحات

```csharp
@inject ICommunicationService Comm

// لیست
var items = await Comm.GetAsync<List<EventDto>>("api/event");

// ایجاد
await Comm.PostAsync<object>("api/event", new { Title = "..." });

// ویرایش
await Comm.PutAsync<object>($"api/event/{id}", new { Title = "..." });

// حذف
await Comm.DeleteAsync($"api/event/{id}");
```

### مسیریابی خودکار WebSocket

`CommunicationService.MapUrlToAction` آدرس URL را خودکار به WS action تبدیل می‌کند:

| URL | WS Action | قانون |
|-----|-----------|-------|
| `api/event` | `event.list` | `{entity}.list` |
| `api/event/5` | `event.get` | `{entity}.get` |
| `POST api/event` | `event.create` | `POST → {entity}.create` |
| `PUT api/event/5` | `event.update` | `PUT + ID → {entity}.update` |
| `DELETE api/event/5` | `event.delete` | `DELETE + ID → {entity}.delete` |

### رمزنگاری

- **الگوریتم:** AES-256-CBC
- **کلید اشتراکی:** `pdd-ir-ws-2026-secure-key`
- **Key Derivation:** SHA256(key) → 32 byte key
- **IV:** تصادفی و به ciphertext اضافه می‌شود

---

## راهنمای ساخت Entity جدید

### مثال: ساخت ماژول "Event" (رویداد)

برای اضافه کردن یک Entity جدید، مراحل زیر را به ترتیب انجام دهید:

---

### مرحله ۱: Entity (مدل دیتابیس)

📁 `Pdd.ir.Business/Models/Entities/Event.cs`

```csharp
namespace Pdd.ir.Business.Models.Entities
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string TitleEn { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Location { get; set; } = "";
        public long EventDate { get; set; }        // Unix timestamp
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public long CreatedAt { get; set; }
    }
}
```

**قوانین:**
- فیلدها با `PascalCase`
- تاریخ‌ها: `long` (BigInt) نه DateTime
- bool با `default`

---

### مرحله ۲: DTO سمت سرور

📁 `Pdd.ir.Business/Models/DTOs/EventDto.cs`

```csharp
namespace Pdd.ir.Business.Models.DTOs
{
    public class EventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string TitleEn { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Location { get; set; } = "";
        public long EventDate { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class EventCreateRequest
    {
        public string Title { get; set; } = "";
        public string TitleEn { get; set; } = "";
        public string Description { get; set; } = "";
        public string? ImageBase64 { get; set; }  // برای آپلود تصویر
        public string Location { get; set; } = "";
        public long EventDate { get; set; }
        public int SortOrder { get; set; }
    }
}
```

---

### مرحله ۳: DTO سمت کلاینت

📁 `Pdd.ir.Client/Models/EventDto.cs`

```csharp
// دقیقاً مثل DTO سرور (تکراری)
namespace Pdd.ir.Client.Models
{
    public class EventDto { /* همان فیلدها */ }
    public class EventCreateRequest { /* همان فیلدها */ }
}
```

---

### مرحله ۴: SQL Queries

📁 `Pdd.ir.Data/Queries/EventQueries.cs`

```csharp
namespace Pdd.ir.Data.Queries
{
    public static class EventQueries
    {
        public const string GetAll = "SELECT * FROM Events WHERE IsActive = 1 ORDER BY SortOrder, Id";
        public const string GetById = "SELECT * FROM Events WHERE Id = @Id";
        public const string Insert = @"
            INSERT INTO Events (Title, TitleEn, Description, ImageUrl, Location, EventDate, SortOrder, IsActive, CreatedAt)
            VALUES (@Title, @TitleEn, @Description, @ImageUrl, @Location, @EventDate, @SortOrder, 1, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT)";
        public const string Update = @"
            UPDATE Events
            SET Title = @Title, TitleEn = @TitleEn, Description = @Description,
                ImageUrl = @ImageUrl, Location = @Location, EventDate = @EventDate, SortOrder = @SortOrder
            WHERE Id = @Id";
        public const string Delete = "DELETE FROM Events WHERE Id = @Id";
    }
}
```

---

### مرحله ۵: BusinessService

📁 `Pdd.ir.Business/Services/EventBusinessService.cs`

```csharp
namespace Pdd.ir.Business.Services
{
    public class EventBusinessService
    {
        private readonly IDbService _db;

        public EventBusinessService(IDbService db) { _db = db; }

        // ⚠️ نام و امضا دقیقاً باید اینها باشد:
        public async Task<IEnumerable<EventDto>> GetAllAsync() { ... }
        public async Task<EventDto?> GetByIdAsync(int id) { ... }
        public async Task<int> InsertAsync(EventCreateRequest dto) { ... }
        public async Task<bool> UpdateAsync(EventDto dto) { ... }
        public async Task<bool> DeleteAsync(int id) { ... }
    }
}
```

**نکته:** متد `InsertAsync` باید `int` برگرداند (ID جدید).

---

### مرحله ۶: Controller

📁 `Pdd.ir.Server/Controllers/EventController.cs`

```csharp
[ApiController]
[Route("api/event")]  // ⚠️ نام entity در route
public class EventController : ControllerBase
{
    private readonly EventBusinessService _service;

    public EventController(EventBusinessService service) { _service = service; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(new { success = true, data = items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(new { success = true, data = item });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventCreateRequest request)
    {
        var id = await _service.InsertAsync(request);
        return Ok(new { success = true, data = new { id } });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EventDto dto)
    {
        dto.Id = id;
        await _service.UpdateAsync(dto);
        return Ok(new { success = true });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(new { success = true });
    }
}
```

---

### مرحله ۷: ثبت در Program.cs

📁 `Pdd.ir.Server/Program.cs`

```csharp
// اگر constructor پارامتر ندارد:
builder.Services.AddScoped<EventBusinessService>();

// اگر constructor پارامتر دارد (مثلاً مسیر تصویر):
builder.Services.AddScoped<EventBusinessService>(sp =>
{
    var db = sp.GetRequiredService<IDbService>();
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var imagePath = Path.Combine(env.WebRootPath, "uploads", "events");
    return new EventBusinessService(db, imagePath);
});
```

---

### مرحله ۸: ثبت در WebSocketHandler (فقط یک خط!)

📁 `Pdd.ir.Server/WebSocket/WebSocketHandler.cs` → متد `GetService`

```csharp
"event" => sp.GetService<EventBusinessService>(),
```

**⚠️ فقط این یک خط اضافه شود!**

---

### مرحله ۹: Dialog (مودال CRUD)

📁 `Pdd.ir.Client/Shared/Dialogs/EventDialog.razor`

```razor
@inject ICommunicationService Comm
@inject IAlertService Alert
@inject ITranslateService T
@inject IFileUploadService FileUpload
@inject IModalService Modal

<div class="aco-dialog-body">
    @* راهنمای فرم *@
    <div style="background:var(--bg-tertiary);border-radius:12px;padding:12px 16px;margin-bottom:16px;">
        <div style="display:flex;align-items:center;gap:8px;margin-bottom:4px;">
            <i class="bi bi-info-circle" style="color:var(--accent-primary);"></i>
            <strong style="font-size:0.875rem;">@T.Text("event_form_guide_title")</strong>
        </div>
        <p style="margin:0;font-size:0.8125rem;color:var(--text-secondary);">
            @T.Text("event_form_guide_desc")
        </p>
    </div>

    <div class="row g-3">
        <div class="col-12 col-md-6">
            <label class="form-label">@T.Text("title_fa") *</label>
            <input type="text" class="form-control" @bind="Model.Title" />
        </div>
        @* فیلدهای دیگر *@
    </div>
</div>
<div class="aco-dialog-footer">
    <button class="btn btn-secondary" @onclick="Close">@T.Text("cancel")</button>
    <button class="btn btn-primary" @onclick="Save">
        <i class="bi bi-floppy me-1"></i>@T.Text("save")
    </button>
</div>

@code {
    [Parameter] public int? Id { get; set; }
    EventDto Model = new();

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var item = await Comm.GetAsync<EventDto>($"api/event/{Id}");
            if (item != null) Model = item;
        }
    }

    async Task Save()
    {
        // آپلود تصویر اگر data URL باشد
        if (!string.IsNullOrEmpty(Model.ImageUrl) && Model.ImageUrl.StartsWith("data:"))
        {
            var imageUrl = await FileUpload.UploadImageAsync(Model.ImageUrl);
            if (!string.IsNullOrEmpty(imageUrl))
                Model.ImageUrl = imageUrl;
        }

        if (Id.HasValue)
            await Comm.PutAsync<object>($"api/event/{Id}", Model);
        else
            await Comm.PostAsync<object>("api/event", Model);

        await Alert.ShowSuccessAsync(T.Text("success"), T.Text("saved_successfully"));
        Modal.Close();
    }

    void Close() => Modal.Close();
}
```

---

### مرحله ۱۰: صفحه Admin

📁 `Pdd.ir.Client/Pages/Admin/Events.razor`

```razor
@page "/admin/events"
@attribute [AuthorizeRole("admin")]
@inject ICommunicationService Comm
@inject ITranslateService T
@inject IModalService Modal
@inject IAlertService Alert

<PageTitle>@T.Text("event_management") | PDD</PageTitle>

<div class="page-enter">
    <PddTable TItem="EventDto"
              Items="@Items"
              Title="@T.Text("event_management")"
              IsLoading="@IsLoading"
              PageSize="10"
              CanInsert="true"
              CanEdit="true"
              CanDelete="true"
              Columns="@GetColumns()"
              OnInsert="OpenCreate"
              OnEdit="OpenEdit"
              OnDelete="Delete" />
</div>

@code {
    List<EventDto> Items = new();
    bool IsLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    async Task LoadData()
    {
        IsLoading = true;
        Items = await Comm.GetAsync<List<EventDto>>("api/event") ?? new();
        IsLoading = false;
    }

    List<PddTableColumn<EventDto>> GetColumns() => new()
    {
        new() { Title = "#", GetValue = x => x.Id.ToString() },
        new() { Title = T.Text("title_fa"), GetValue = x => x.Title },
        new() { Title = T.Text("location"), GetValue = x => x.Location },
        new() { Title = T.Text("status"), Template = c => builder =>
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", $"badge {(c.IsActive ? "bg-success" : "bg-secondary")}");
            builder.AddContent(2, c.IsActive ? T.Text("active") : T.Text("inactive"));
            builder.CloseElement();
        }}
    };

    async Task OpenCreate()
    {
        await Modal.Show<EventDialog>(T.Text("new_event"));
        await LoadData();
    }

    async Task OpenEdit(EventDto item)
    {
        await Modal.Show<EventDialog>(T.Text("edit_event"), new Dictionary<string, object> { { "Id", item.Id } });
        await LoadData();
    }

    async Task Delete(EventDto item)
    {
        await Comm.DeleteAsync($"api/event/{item.Id}");
        await Alert.ShowSuccessAsync(T.Text("success"), T.Text("deleted_successfully"));
        await LoadData();
    }
}
```

---

### مرحله ۱۱: ترجمه‌ها

📁 `wwwroot/lang/fa.json` + `wwwroot/lang/en.json`

```json
// fa.json
{
  "event_management": "مدیریت رویدادها",
  "new_event": "رویداد جدید",
  "edit_event": "ویرایش رویداد",
  "event_form_guide_title": "راهنما",
  "event_form_guide_desc": "اطلاعات رویداد را وارد کنید.",
  "title_fa": "عنوان فارسی",
  "title_en": "عنوان انگلیسی",
  "description_fa": "توضیحات فارسی",
  "location": "مکان",
  "event_date": "تاریخ رویداد",
  "sort_order": "ترتیب",
  "status": "وضعیت",
  "active": "فعال",
  "inactive": "غیرفعال",
  "save": "ذخیره",
  "cancel": "لغو",
  "success": "موفق",
  "saved_successfully": "با موفقیت ذخیره شد",
  "deleted_successfully": "با موفقیت حذف شد",
  "are_you_sure_delete": "آیا از حذف مطمئن هستید؟"
}
```

```json
// en.json
{
  "event_management": "Event Management",
  "new_event": "New Event",
  "edit_event": "Edit Event",
  "event_form_guide_title": "Guide",
  "event_form_guide_desc": "Enter event information.",
  "title_fa": "Persian Title",
  "title_en": "English Title",
  "description_fa": "Persian Description",
  "location": "Location",
  "event_date": "Event Date",
  "sort_order": "Sort Order",
  "status": "Status",
  "active": "Active",
  "inactive": "Inactive",
  "save": "Save",
  "cancel": "Cancel",
  "success": "Success",
  "saved_successfully": "Saved successfully",
  "deleted_successfully": "Deleted successfully",
  "are_you_sure_delete": "Are you sure you want to delete?"
}
```

---

### مرحله ۱۲: لینک در NavMenu

📁 `Pdd.ir.Client/Layout/NavMenu.razor`

```razor
<div class="admin-nav-item">
    <NavLink class="admin-nav-link" href="admin/events">
        <span class="admin-nav-icon"><i class="bi bi-calendar-event"></i></span>
        <span class="admin-nav-text">@T.Text("event_management")</span>
    </NavLink>
</div>
```

---

### مرحله ۱۳: SQL Migration

📁 `Pdd.ir.Server/wwwroot/resource/{timestamp}_Create_Events.sql`

```sql
IF OBJECT_ID('Events', 'U') IS NULL
BEGIN
    CREATE TABLE Events (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Title NVARCHAR(500) NOT NULL,
        TitleEn NVARCHAR(500) NOT NULL DEFAULT '',
        Description NVARCHAR(MAX) NOT NULL DEFAULT '',
        ImageUrl NVARCHAR(500) NOT NULL DEFAULT '',
        Location NVARCHAR(500) NOT NULL DEFAULT '',
        EventDate BIGINT NOT NULL DEFAULT 0,
        SortOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt BIGINT NOT NULL
    );
END
GO
```

---

### چک‌لیست نهایی

| # | بررسی | فایل |
|---|-------|------|
| 1 | Entity با PascalCase و long برای تاریخ | `Entities/Event.cs` |
| 2 | DTO در Business و Client | `DTOs/EventDto.cs` + `Client/Models/EventDto.cs` |
| 3 | Queries با SCOPE_IDENTITY | `Queries/EventQueries.cs` |
| 4 | BusinessService با ۵ متد استاندارد | `Services/EventBusinessService.cs` |
| 5 | Controller با Route("api/event") | `Controllers/EventController.cs` |
| 6 | ثبت در Program.cs | `Server/Program.cs` |
| 7 | `GetService` در WebSocketHandler | `WebSocket/WebSocketHandler.cs` |
| 8 | Dialog با `ICommunicationService` | `Dialogs/EventDialog.razor` |
| 9 | صفحه Admin با PddTable | `Pages/Admin/Events.razor` |
| 10 | ترجمه‌ها در fa.json + en.json | `wwwroot/lang/*.json` |
| 11 | لینک در NavMenu | `Layout/NavMenu.razor` |
| 12 | SQL Migration | `wwwroot/resource/*.sql` |
| 13 | Build موفق | `dotnet build` |

---

## فهرست ماژول‌ها و فایل‌ها

### Entity → DTO → Queries → Service → Controller → Admin Page → Dialog

| Entity | DTO | Queries | Service | Controller | Admin Page | Dialog |
|--------|-----|---------|---------|------------|------------|--------|
| `User` | `UserDto` | `UserQueries` | `UserBusinessService` | `UserController` | `Users.razor` | `UserDialog.razor` |
| `Product` | `ProductDto` | `ProductQueries` | `ProductBusinessService` | `ProductController` | `Products.razor` | `ProductDialog.razor` |
| `BlogPost` | `BlogDto` | `BlogQueries` | `BlogBusinessService` | `BlogController` | `BlogAdmin.razor` | `BlogDialog.razor` |
| `PortfolioItem` | `PortfolioDto` | `PortfolioQueries` | `PortfolioBusinessService` | `PortfolioController` | `PortfolioAdmin.razor` | `PortfolioDialog.razor` |
| `Client` | `ClientDto` | `ClientQueries` | `ClientBusinessService` | `ClientController` | `Clients.razor` | `ClientDialog.razor` |
| `Event` | `EventDto` | `EventQueries` | `EventBusinessService` | `EventController` | `Events.razor` | `EventDialog.razor` |
| `ContactMessage` | `ContactDto` | `ContactQueries` | `ContactBusinessService` | `ContactController` | `Messages.razor` | `ContactMessageDialog.razor` |
| `HomeSlide` | `HomeSlideDto` | `HomeSlideQueries` | `HomeSlideBusinessService` | `HomeSlideController` | `HomeSlides.razor` | `HomeSlideDialog.razor` |
| `HomeProduct` | `HomeProductDto` | `HomeProductQueries` | `HomeProductBusinessService` | `HomeProductController` | `HomeProducts.razor` | `HomeProductDialog.razor` |
| `Page` | `PageDto` | `PageQueries` | `PageBusinessService` | `PageController` | — | — |
| `Role` | `RoleDto` | `RoleQueries` | `RoleBusinessService` | `RoleController` | `Roles.razor` | `RoleDialog.razor` |
| `Settings` | `SettingsDto` | `SettingsQueries` | `SettingsBusinessService` | `SettingsController` | `Settings.razor` | — |

### سرویس‌های اضافی (غیر CRUD)

| Controller | توضیح |
|-----------|-------|
| `AuthController` | لاگین، لاگاوت، Handshake |
| `UploadController` | آپلود فایل و تصویر |
| `ImageController` | دریافت و مدیریت تصاویر |
| `FileController` | دانلود فایل |
| `ClientImageController` | تصاویر اختصاصی مشتریان |

---

## تکنولوژی‌ها

| بخش | تکنولوژی | نسخه |
|-----|----------|-------|
| فرانت‌اند | Blazor WebAssembly | .NET 10.0 |
| UI | Bootstrap 5 + Glassmorphism | 5.3 |
| آیکون‌ها | Bootstrap Icons | 1.11 |
| انیمیشن | GSAP + CSS Animations | 3.12 |
| بک‌اند | ASP.NET Core | 10.0 |
| ORM | Dapper | — |
| دیتابیس | SQL Server | 2019+ |
| احراز هویت | JWT | — |
| ارتباط | WebSocket + HTTP | — |
| رمزنگاری | AES-256-CBC | — |
| فونت | Vazirmatn (فارسی) | — |
| تقویم | PersianDatePicker | — |

---

## قوانین توسعه

### قوانین اصلی

| قانون | توضیح |
|-------|-------|
| Modal-First | تمام CRUD‌ها با مودال (نه صفحه جدا) |
| Skeleton Loading | لودینگ با Skeleton (نه spinner) |
| Glassmorphism | استایل شیشه‌ای |
| RTL | راست به چپ فارسی |
| Git | هر تغییر → commit + push |
| `ICommunicationService` | تمام ارتباطات از این سرویس |
| `PddTable` | تمام جدول‌ها با این کامپوننت |
| `SearchableList` | لیست‌های dropdown با جستجو |
| `PersianDatePicker` | تاریخ‌ها با تقویم شمسی |
| `T.Text()` | تمام متن‌ها از سیستم ترجمه |

### ممنوعیت‌ها

| ممنوع | دلیل |
|-------|------|
| `spinner-border` | از Skeleton استفاده کن |
| `<table>` دستی | از PddTable استفاده کن |
| `HttpClient` مستقیم | از ICommunicationService استفاده کن |
| متن فارسی هاردکد | از lang/*.json استفاده کن |
| فایل CSS جداگانه | فقط app.css |
| `NavigateTo(url, true)` | باعث reload کامل صفحه می‌شود |

---

**شرکت طراح داده پیشرو** — [pdd.ir](https://pdd.ir)
