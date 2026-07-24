# AGENTS.md — Project Guidelines

## 🚨 MANDATORY: Auto-Trigger Skill System

> **قانون اجباری شماره ۱:** هنگام دریافت هر درخواستی از کاربر، ابتدا **خودکار** مهارت(های) مرتبط را با `skill` tool بارگذاری کن.
>
> **قانون اجباری شماره ۲:** هرگز بدون بارگذاری مهارت مناسب کد تولید نکن.
>
> **قانون اجباری شماره ۳:** جدول Auto-Trigger را در بخش "🚀 Auto-Trigger System" بررسی کن.

### نحوه عمل:
```
1. درخواست کاربر را بخوان
2. کلمات کلیدی را شناسایی کن (جستجو در جدول Trigger Rules)
3. مهارت مناسب را با skill tool بارگذاری کن
4. بر اساس مهارت بارگذاری شده کد تولید کن
```

---

## 🚨 MANDATORY: Deep Analysis & Step-by-Step Execution (تحلیل عمیق و اجرای مرحله‌ای)

> **قانون اجباری:** هنگام دریافت هر درخواستی از کاربر، **بلافاصله کد تولید نکن**. ابتدا تحلیل عمیق انجام بده.

### ترتیب اجباری:
```
۱. تحلیل عمیق درخواست (بررسی پروژه، وابستگی‌ها، تأثیرات)
۲. پلن‌بندی (تقسیم به مراحل کوچک)
۳. پرسیدن سوالات (اگر ابهامی وجود دارد)
۴. اجرا قسمت به قسمت (با تأیید کاربر بین مراحل)
```

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔥 تحلیل اول | هرگز بدون تحلیل، کد تولید نکن |
| 🔥 بررسی پروژه | فایل‌های مرتبط را بخوان قبل از شروع |
| 🔥 شناسایی وابستگی‌ها | ببین تغییرات روی چه بخش‌هایی تأثیر دارد |
| 🔥 پلن مرحله‌ای | کار را به مراحل کوچک تقسیم کن |
| 🔥 سوال اول | اگر ابهامی داری، قبل از اجرا بپرس |
| 🔥 اجرا مرحله‌ای | هر مرحله را جداگانه اجرا و تأیید کن |
| 🔥 مستندسازی | تصمیمات و تغییرات را مستند کن |

### نحوه عمل:
```
۱. درخواست کاربر را بخوان
۲. فایل‌های مرتبط پروژه را بررسی کن
۳. تحلیل کن: چه چیزی تغییر می‌کند؟ چه وابستگی‌هایی دارد؟
۴. پلن بنویس: مراحل کار را لیست کن
۵. سوالاتت را بپرس (اگر داری)
۶. با تأیید کاربر، قسمت به قسمت اجرا کن
۷. بعد از هر مرحله، نتیجه را گزارش بده
```

---

## 🚨 MANDATORY: ساخت Entity جدید (کامل و استاندارد)

> **قانون اجباری:** هر بار که کاربر از شما بخواهد Entity/ماژول جدیدی بسازد (مثلاً ایونت، محصول، خبر، ...)، دقیقاً مراحل زیر را **ترتیبی** انجام دهید.
>
> **⚠️ قانون طلایی: به WebSocketHandler.cs و CommunicationService.cs دست نزنید!**
> این دو فایل زیرساخت ارتباطی هستند. اگر CRUD خودکار (dynamic) درست کار نمی‌کنه، مشکل از جای دیگه‌ست — نه از این فایل‌ها.

### مراحل ساخت Entity جدید

#### مرحله ۱: Entity (مدل دیتابیس)
📁 `Pdd.ir.Business/Models/Entities/{Entity}.cs`
```csharp
namespace Pdd.ir.Business.Models.Entities
{
    public class Entity
    {
        public int Id { get; set; }
        // فیلدها با PascalCase
        // تاریخ‌ها: long (BigInt) نه DateTime
        // bool با default
    }
}
```

#### مرحله ۲: DTO سمت سرور
📁 `Pdd.ir.Business/Models/DTOs/{Entity}Dto.cs`
```csharp
namespace Pdd.ir.Business.Models.DTOs
{
    public class EntityDto
    {
        public int Id { get; set; }
        // فیلدهای مورد نیاز UI
    }
    public class EntityCreateRequest
    {
        // فیلدها برای create (بدون Id)
    }
}
```

#### مرحله ۳: DTO سمت کلاینت
📁 `Pdd.ir.Client/Models/{Entity}Dto.cs`
```csharp
// دقیقاً مثل DTO سرور (تکراری)
namespace Pdd.ir.Client.Models
{
    public class EntityDto { /* همان فیلدها */ }
}
```

#### مرحله ۴: SQL Queries
📁 `Pdd.ir.Data/Queries/{Entity}Queries.cs`
```csharp
namespace Pdd.ir.Data.Queries
{
    public static class EntityQueries
    {
        public const string GetAll = "SELECT * FROM Entities WHERE IsActive = 1 ...";
        public const string GetById = "SELECT * FROM Entities WHERE Id = @Id";
        public const string Insert = "INSERT INTO Entities (...) VALUES (...); SELECT CAST(SCOPE_IDENTITY() AS INT)";
        public const string Update = "UPDATE Entities SET ... WHERE Id = @Id";
        public const string Delete = "DELETE FROM Entities WHERE Id = @Id";
    }
}
```

#### مرحله ۵: BusinessService (متد استاندارد الزامی!)
📁 `Pdd.ir.Business/Services/{Entity}BusinessService.cs`
```csharp
namespace Pdd.ir.Business.Services
{
    public class EntityBusinessService
    {
        private readonly IDbService _db;

        public EntityBusinessService(IDbService db) { _db = db; }

        // ⚠️ نام و امضا دقیقاً باید اینها باشد (CRUD خودکار به اینها وابسته است):
        public async Task<IEnumerable<EntityDto>> GetAllAsync() { ... }
        public async Task<EntityDto?> GetByIdAsync(int id) { ... }
        public async Task<int> InsertAsync(EntityCreateRequest dto) { ... }
        public async Task<bool> UpdateAsync(EntityDto dto) { ... }
        public async Task<bool> DeleteAsync(int id) { ... }
    }
}
```

#### مرحله ۶: Controller
📁 `Pdd.ir.Server/Controllers/{Entity}Controller.cs`
```csharp
[ApiController]
[Route("api/{entity}")]
public class EntityController : ControllerBase
{
    // GET, GET/{id}, POST, PUT/{id}, DELETE/{id}
}
```

#### مرحله ۷: ثبت در Program.cs
📁 `Pdd.ir.Server/Program.cs`
```csharp
builder.Services.AddScoped<EntityBusinessService>();
// اگر constructor پارامتر داره:
builder.Services.AddScoped<EntityBusinessService>(sp => {
    var db = sp.GetRequiredService<IDbService>();
    return new EntityBusinessService(db);
});
```

#### مرحله ۸: ثبت در WebSocketHandler (فقط یک خط!)
📁 `Pdd.ir.Server/WebSocket/WebSocketHandler.cs` → متد `GetService`
```csharp
"entity" => sp.GetService<EntityBusinessService>(),
```
**⚠️ فقط این یک خط اضافه شود! هیچ هندلر دیگری ننویسید!**

#### مرحله ۹: Dialog (مودال CRUD)
📁 `Pdd.ir.Client/Shared/Dialogs/{Entity}Dialog.razor`
- فرم با فیلدها
- `@inject ICommunicationService Comm`
- `@inject IFileUploadService FileUpload` (اگر تصویر داره)
- Save → `Comm.PostAsync` / `Comm.PutAsync`

#### مرحله ۱۰: صفحه Admin
📁 `Pdd.ir.Client/Pages/Admin/{Entity}s.razor`
- `@page "/admin/{entity}s"`
- `PddTable` با ستون‌ها
- جستجو، حذف، ویرایش

#### مرحله ۱۱: ترجمه‌ها
📁 `wwwroot/lang/fa.json` + `wwwroot/lang/en.json`
- اضافه کردن تمام کلیدهای متنی

#### مرحله ۱۲: Sidebar
📁 `Layout/NavMenu.razor`
- لینک به صفحه admin

#### مرحله ۱۳: SQL Migration
📁 `Server/wwwroot/resource/{timestamp}_{Create_Entity}.sql`
```sql
IF OBJECT_ID('Entities', 'U') IS NULL
BEGIN
    CREATE TABLE Entities ( ... );
END
GO
```

### چک‌لیست قبل از اتمام

| # | بررسی | وضعیت |
|---|-------|-------|
| 1 | Entity با PascalCase و long برای تاریخ | |
| 2 | DTO در Business و Client (هر دو) | |
| 3 | Queries با SCOPE_IDENTITY | |
| 4 | BusinessService با ۵ متد استاندارد | |
| 5 | Controller با Route("api/{entity}") | |
| 6 | ثبت در Program.cs | |
| 7 | `GetService` در WebSocketHandler (فقط ۱ خط) | |
| 8 | Dialog با `ICommunicationService` | |
| 9 | صفحه Admin با PddTable | |
| 10 | ترجمه‌ها در fa.json + en.json | |
| 11 | لینک در NavMenu | |
| 12 | SQL Migration | |
| 13 | Build موفق (بدون خطا) | |

### ❌ ممنوع مطلق
```
1. تغییر WebSocketHandler.cs (بجز ۱ خط در GetService)
2. تغییر CommunicationService.cs
3. تغییر SecurityService.cs
4. تغییر EncryptionService.cs
5. نوشتن هندلر اختصاصی در WebSocketHandler (HandleEntityGet, HandleEntityList و...)
6. نوشتن if در MapUrlToAction
7. کپی کردن کد بین entityها
```

### ✅ اگر CRUD خودکار کار نمی‌کنه
```
۱. بررسی کن BusinessService ثبت شده در Program.cs
۲. بررسی کن GetService در WebSocketHandler entity رو داره
۳. بررسی کن نام متد دقیقاً GetAllAsync, GetByIdAsync, InsertAsync, UpdateAsync, DeleteAsync باشه
۴. بررسی کن return type متد درسته (IEnumerable<T> برای list)
۵. لاگ سرور رو چک کن
۶. دست به WebSocketHandler نزن!
```

---

## 🚨 MANDATORY: Git Workflow

> **قانون اجباری:** ترتیب انجام کار: تغییر کد → Build → Commit → Push

### ترتیب اجباری
```
1. تغییر کد (edit/write)
2. dotnet build (مطمئن شو compile میشه)
3. git add -A
4. git commit -m "پیام توضیحی"
5. git push
```

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔥 اول commit | هرگز قبل از push کار دیگه‌ای نکن |
| 🔥 بعد از هر تغییر | فوراً commit + push کن — منتظر نمان |
| 🔥 پیام commit | انگلیسی، مختصر، توضیحی |

---

## 🚨 MANDATORY: Pdd.ir.Shared Services (اجباری)

> **قانون اجباری:** از سرویس‌ها و کامپوننت‌های `Pdd.ir.Shared` استفاده کن. هرگز سرویس مشابه نساز.

### سرویس‌های موجود در Pdd.ir.Shared

| سرویس | کاربرد | نحوه استفاده |
|-------|--------|-------------|
| `IModalService` | مودال داینامیک | `await Modal.Show<T>("عنوان")` |
| `IAlertService` | Toast notification | `await Alert.ShowSuccessAsync("عنوان", "پیام")` |
| `IClientStorageService` | LocalStorage/Session/Cookie | `await Storage.SetLocalAsync("key", value)` |
| `IEncryptionService` | AES-256-CBC رمزنگاری | `await Enc.EncryptDataAsync(data, key)` |

### کامپوننت‌های موجود در Pdd.ir.Shared

| کامپوننت | کاربرد | مثال |
|----------|--------|------|
| `Modal.razor` | کانتینر مودال سراسری | خودکار کار می‌کند |
| `SearchableList.razor` | dropdown با جستجو | `<SearchableList TItem="..." TValue="..." />` |
| `BootstrapNumericInput.razor` | ورودی عدد فرمت‌دار | `<BootstrapNumericInput @bind-Value="..." />` |
| `CKEditorBlazor.razor` | ویرایشگر متن غنی | `<CKEditorBlazor @bind-CurrentValue="..." />` |
| `PersianDatePicker.razor` | تقویم شمسی | `<PersianDatePicker @bind-Value="..." />` |

### نحوه ثبت در Program.cs
```csharp
builder.Services.AddPddSharedServices();
```

### نحوه import در _Imports.razor
```razor
@using Pdd.ir.Shared
@using Pdd.ir.Shared.Services
@using Pdd.ir.Shared.Components
@using Pdd.ir.Shared.Helpers
```

### DateHelper — تبدیل تاریخ
```csharp
@using Pdd.ir.Shared.Helpers

// Unix → String
DateHelper.ToShamsi(unix)              // "1402/01/15"
DateHelper.ToShamsiFull(unix)          // "15 فروردین 1402"
DateHelper.ToShamsiWithTime(unix)      // "1402/01/15 14:30"
DateHelper.ToMiladiString(unix)        // "2023-04-05"

// String → Unix
DateHelper.FromShamsi("1402/01/15")    // unix timestamp
DateHelper.FromMiladi("2023-04-05")    // unix timestamp

// String → String
DateHelper.ShamsiToMiladi("1402/01/15")  // "2023-04-05"
DateHelper.MiladiToShamsi("2023-04-05")  // "1402/01/15"
```

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔥 اجباری | `<Pdd.ir.Shared.Components.Modal />` حتماً در `App.razor` باشد |
| 🔥 اجباری | از `IAppStateService` برای رفرش بدون reload استفاده کن |
| 🔥 اجباری | از `IModalService` برای مودال استفاده کن |
| 🔥 اجباری | از `IAlertService` برای Toast استفاده کن |
| 🔥 اجباری | از `PddTable` برای تمام جدول‌ها استفاده کن |
| 🔥 اجباری | از `SearchableList` به جای `<select>` استفاده کن |
| 🔥 اجباری | از `PersianDatePicker` برای تاریخ شمسی استفاده کن |
| 🔥 اجباری | از `CKEditorBlazor` برای ویرایشگر متن استفاده کن |
| 🔥 اجباری | از `BootstrapNumericInput` برای ورودی عدد استفاده کن |
| 🔥 اجباری | از `DateHelper` برای تبدیل تاریخ استفاده کن |
| 🔥 اجباری | از `T.Text("کلید")` برای تمام متن‌های کاربر استفاده کن |
| 🔥 اجباری | هر متن هاردکد فارسی باید در `lang/fa.json` و `lang/en.json` باشد |
| 🔥 ممنوع | متن فارسی هاردکد در کد ممنوع است |
| 🔥 ممنوع | سرویس مشابه نساز |
| 🔥 ممنوع | از `spinner-border` استفاده نکن (از Skeleton استفاده کن) |
| 🔥 ممنوع | جدول دستی با `<table>` نساز (از PddTable استفاده کن) |
| 🔥 ممنوع | از `NavigateTo(url, true)` استفاده نکن — باعث reload کامل صفحه می‌شود |
| 🔥 ممنوع | از `NavigationManager.NavigateTo` با forceLoad: true استفاده نکن |
| 🔥 اجباری | برای آپلود/دریافت فایل حتماً از `IFileUploadService` استفاده کن |

### نحوه استفاده از FileUploadService
```razor
@inject IFileUploadService FileUpload

// آپلود تصویر
var url = await FileUpload.UploadImageAsync(dataUrl, "clients");

// دریافت تصویر
var file = await FileUpload.GetFileAsync("abc123", "clients");

// حذف فایل
await FileUpload.DeleteFileAsync("abc123", "clients");
```

### قوانین FileUpload
| قانون | توضیح |
|-------|-------|
| 🔥 اجباری | از `IFileUploadService` برای آپلود/دریافت فایل استفاده کن |
| 🔥 اجباری | فقط GUID در دیتابیس ذخیره شود |
| 🔥 اجباری | فایل‌ها در پوشه `uploads/` ذخیره شوند |
| 🔥 ممنوع | از `HttpClient` مستقیم برای آپلود استفاده نکن |

---

## 🚨 MANDATORY: Server-Client Communication (ارتباط سرور-کلاینت)

> **قانون اجباری:** تمام ارتباطات کلاینت با سرور باید از طریق `ICommunicationService` انجام شود. استفاده مستقیم از `HttpClient` در صفحات ممنوع است.

### آدرس‌ها از appsettings.json خوانده می‌شوند

فایل `wwwroot/appsettings.json` در کلاینت:
```json
{
  "Pdd": {
    "ApiSettings": {
      "BaseUrl": "http://localhost:5000",
      "WebSocketUrl": "ws://localhost:5000"
    }
  }
}
```

### نحوه ارتباط در صفحات
```razor
@inject ICommunicationService Comm

// لیست — وقتی WS وصله از WS، وگرنه HTTP
var items = await Comm.GetAsync<List<ProductDto>>("api/product");

// ایجاد
var result = await Comm.PostAsync<object>("api/product", new { Title = "..." });

// ویرایش
var ok = await Comm.PutAsync<object>($"api/product/{id}", new { Title = "..." });

// حذف
var ok = await Comm.DeleteAsync($"api/product/{id}");
```

### قوانین ارتباط
| قانون | توضیح |
|-------|-------|
| 🔥 فقط Comm | همه درخواست‌ها از `ICommunicationService` — هیچ سرویسی حق ندارد مستقیم از `HttpClient` استفاده کند |
| 🔥 WS-first | اگه WS وصله → همه از WS |
| 🔥 HTTP fallback | اگه WS قطعه → همه از HTTP |
| 🔥 Auth endpoints | `/auth/*` همیشه HTTP (نه WS) |
| 🔥 هر درخواست auth جدید | هر درخواست `X-Auth` header جدید میخواد (timestamp + nonce تکراری نباشه) |
| 🔥 Handshake | `MainLayout.OnAfterRenderAsync` → `Comm.InitializeAsync()` |
| 🔥 لاگین | از `Comm.PostAsync` با SharedKey encryption |

### ⚠️ قانون طلایی: سرویس‌های ارتباطی غیرقابل تغییر هستند

> **هیچ سرویسی حق ندارد مستقیم از `HttpClient` استفاده کند. تمام ارتباطات باید از طریق `ICommunicationService` انجام شود — حتی در سرویس‌هایی مانند `AuthService`.**

**سرویس‌هایی که نباید تغییر کنند (زیرساخت ارتباطی):**
- `ICommunicationService` / `CommunicationService`
- `SecurityService`
- `EncryptionService`

**سرویس‌های تجاری که باید از Comm استفاده کنند:**
- `AuthService` — باید از `_comm.PostAsync` برای لاگین استفاده کند
- هر سرویس دیگری که نیاز به ارتباط با سرور دارد

**نمونه صحیح (Users.razor):**
```razor
@inject ICommunicationService Comm

// ✅ صحیح — همه درخواست‌ها از Comm
var items = await Comm.GetAsync<List<UserDto>>("api/user");
await Comm.DeleteAsync($"api/user/{user.Id}");
```

**نمونه غلط (AuthService قبل از اصلاح):**
```csharp
// ❌ غلط — استفاده مستقیم از HttpClient
var response = await _http.PostAsJsonAsync("api/auth/login", new { Username = username, Password = password });

// ❌ غلط — تنظیم مستقیم header
_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
```

**نمونه صحیح (AuthService بعد از اصلاح):**
```csharp
// ✅ صحیح — استفاده از Comm
var result = await _comm.PostAsync<LoginResponse>("api/auth/login", new { Username = username, Password = password });
if (result != null) { Token = result.Token; ... }
```

### ❌ ممنوع
```razor
@inject HttpClient Http
var data = await Http.GetFromJsonAsync<List<Product>>("api/product");

// یا مستقیم _http.PostAsJsonAsync
```

### ✅ درست
```razor
@inject ICommunicationService Comm
var data = await Comm.GetAsync<List<ProductDto>>("api/product");
```

### WebSocket — URL Mapping (خودکار)

`CommunicationService.MapUrlToAction` آدرس URL رو **خودکار** به WS action تبدیل می‌کنه:

### قوانین خودکار

| URL | WS Action | قانون |
|-----|-----------|-------|
| `api/product` | `product.list` | `{entity}.list` |
| `api/product/5` | `product.get` | `{entity}.get` |
| `POST api/client` | `client.create` | `POST → {entity}.create` |
| `PUT api/client/5` | `client.update` | `PUT + ID → {entity}.update` |
| `DELETE api/client/5` | `client.delete` | `DELETE + ID → {entity}.delete` |
| `api/client/admin` | `client.admin` | `contains "admin" → {entity}.admin` |
| `api/contact/5/markread` | `contact.markread` | `contains "markread" → {entity}.markread` |
| `api/blog/admin` | `blog.admin` | `contains "admin" → {entity}.admin` |

### نحوه کار خودکار

```
URL: api/{entity}/{id}?action={special}
     ↓
MapUrlToAction(url, method):
  1. استخراج entity از اولین بخش URL
  2. استخراج id اگه عددی باشه
  3. تشخیص عملیات بر اساس method:
     POST   → {entity}.create
     PUT    → {entity}.update
     DELETE → {entity}.delete
     GET+id → {entity}.get
     GET    → {entity}.list
     حاوی "admin" → {entity}.admin
```

### قوانین سمت سرور (WebSocketHandler)

```
RequiresSession → action.Split('.') → [entity, operation]
  ↓
CRUD operations (خودکار با Reflection):
  list   → HandleList   → svc.GetAllAsync()
  get    → HandleGet    → svc.GetByIdAsync(id)
  create → HandleCreate → svc.InsertAsync(dto)
  update → HandleUpdate → svc.UpdateAsync(dto)
  delete → HandleDelete → svc.DeleteAsync(id)
  ↓
Special operations (مسیریابی سوئیچ):
  admin, markread, unread, count, submit, ...
```

### ⚠️ قانون طلایی

> **برای entity جدید، فقط ۳ چیز لازمه:**
> 1. `BusinessService` با متدهای `GetAllAsync`, `GetByIdAsync`, `InsertAsync`, `UpdateAsync`, `DeleteAsync`
> 2. `Controller` با attribute `Route("api/{entity}")`
> 3. صفحه `@inject ICommunicationService Comm`
>
> **مسیریابی خودکار انجام میشه! نیازی به اضافه کردن کد جدید در MapUrlToAction یا WebSocketHandler نیست.**

### ❌ ممنوع
```csharp
// اضافه کردن if جدید در MapUrlToAction
if (lower.Contains("newentity"))
{ ... }  // ❌ ممنوع - خودکار انجام میشه

// اضافه کردن case جدید در switch سرور
"newentity.list" => await HandleNewEntityList(scope)  // ❌ ممنوع
```

### ✅ درست
```csharp
// فقط BusinessService بساز با متدهای استاندارد
public class NewEntityBusinessService
{
    public async Task<List<NewEntity>> GetAllAsync() { ... }
    public async Task<NewEntity?> GetByIdAsync(int id) { ... }
    public async Task<int> InsertAsync(NewEntityDto dto) { ... }
    public async Task<bool> UpdateAsync(NewEntityDto dto) { ... }
    public async Task<bool> DeleteAsync(int id) { ... }
}

// و در صفحه استفاده کن
var items = await Comm.GetAsync<List<NewEntityDto>>("api/newentity");    // → newentity.list
await Comm.PostAsync("api/newentity", dto);                              // → newentity.create
await Comm.PutAsync($"api/newentity/{id}", dto);                        // → newentity.update
await Comm.DeleteAsync($"api/newentity/{id}");                          // → newentity.delete
```

### نمونه استفاده در صفحه
```razor
@inject IModalService Modal
@inject IAlertService Alert
@using Pdd.ir.Shared.Helpers

<button @onclick="OpenDialog">مودال جدید</button>

@code {
    async Task OpenDialog()
    {
        await Modal.Show<MyDialogComponent>("عنوان مودال");
    }

    async Task Save()
    {
        // ذخیره...
        await Alert.ShowSuccessAsync("موفق", "با موفقیت ذخیره شد");
    }
}
```

### IAppStateService — رفرش بدون reload
```razor
@inject IAppStateService AppState
@implements IDisposable

@code {
    protected override void OnInitialized()
    {
        AppState.OnStateChanged += Refresh;
    }

    void Refresh() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        AppState.OnStateChanged -= Refresh;
    }
}
```

```csharp
// وقتی زبان/تم/داده تغییر کرد — همه صفحات رفرش شوند
AppState.NotifyStateChanged();
```
```razor
<PddTable TItem="ProductDto"
          Items="@Items"
          Title="مدیریت محصولات"
          Subtitle="ایجاد، ویرایش و حذف"
          IsLoading="@IsLoading"
          PageSize="10"
          CanInsert="true"
          CanEdit="true"
          CanDelete="true"
          Columns="@_columns"
          SearchFilter="@Search"
          OnInsert="OpenCreate"
          OnEdit="OpenEdit"
          OnDelete="Delete"
          DeleteMessageSelector="@(x => $"آیا از حذف «{x.Title}» مطمئن هستید؟")" />

@code {
    List<PddTableColumn<ProductDto>> _columns = new()
    {
        new() { Title = "عنوان", GetValue = x => x.Title },
        new() { Title = "وضعیت", Template = c => builder =>
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", $"badge {(c.IsActive ? "bg-success" : "bg-secondary")}");
            builder.AddContent(2, c.IsActive ? "فعال" : "غیرفعال");
            builder.CloseElement();
        }}
    };

    async Task OpenCreate() => await Modal.Show<ProductDialog>("جدید");
    async Task OpenEdit(ProductDto x) => await Modal.Show<ProductDialog>("ویرایش", new Dictionary<string, object> { { "Id", x.Id } });
    async Task Delete(ProductDto x) { await Comm.DeleteAsync($"api/product/{x.Id}"); await Alert.ShowSuccessAsync("موفق", "حذف شد"); await Load(); }
}
```

### PddTable — پارامترها
| پارامتر | نوع | پیش‌فرض | توضیح |
|---------|-----|---------|-------|
| `TItem` | generic | — | نوع آیتم |
| `Items` | `List<TItem>` | — | داده‌ها |
| `Title` | `string` | `""` | عنوان |
| `Subtitle` | `string` | `""` | زیرعنوان |
| `IsLoading` | `bool` | `true` | حالت لودینگ |
| `CanInsert` | `bool` | `true` | نمایش دکمه جدید |
| `CanEdit` | `bool` | `true` | نمایش دکمه ویرایش |
| `CanDelete` | `bool` | `true` | نمایش دکمه حذف |
| `ShowSearch` | `bool` | `true` | نمایش فیلد جستجو |
| `ShowRowNumber` | `bool` | `true` | نمایش شماره ردیف |
| `PageSize` | `int` | `10` | تعداد ردیف در صفحه |
| `Columns` | `List<PddTableColumn<T>>` | — | تعریف ستون‌ها |
| `SearchFilter` | `Func<T, string, bool>` | خودکار | فیلتر جستجو |
| `OnInsert` | `EventCallback` | — | کلیک دکمه جدید |
| `OnEdit` | `EventCallback<T>` | — | کلیک ویرایش |
| `OnDelete` | `Func<T, Task>` | — | حذف |
| `DeleteMessageSelector` | `Func<T, string>` | پیش‌فرض | پیام confirm حذف |
| `RowActions` | `RenderFragment<T>` | — | دکمه‌های سفارشی |

### PddTable — نحوه تعریف ستون
```csharp
// متن ساده
new() { Title = "نام", GetValue = x => x.Name }

// قالب سفارشی (badge, آیکون و...)
new() { Title = "وضعیت", Template = c => builder =>
{
    builder.OpenElement(0, "span");
    builder.AddAttribute(1, "class", "badge bg-success");
    builder.AddContent(2, c.Status);
    builder.CloseElement();
}}
```

### نمونه استفاده در صفحه
```razor
@inject IModalService Modal
@inject IAlertService Alert
@using Pdd.ir.Shared.Helpers

<button @onclick="OpenDialog">مودال جدید</button>

@code {
    async Task OpenDialog()
    {
        await Modal.Show<MyDialogComponent>("عنوان مودال");
    }

    async Task Save()
    {
        // ذخیره...
        await Alert.ShowSuccessAsync("موفق", "با موفقیت ذخیره شد");
    }
}
```

---

## 🚨 MANDATORY: Skeleton Loading (NO Spinners)

> **قانون اجباری:** تمام state‌های لودینگ باید با **Skeleton Loading** پیاده‌سازی شوند. استفاده از `spinner-border` ممنوع است (به جز وضعیت دکمه هنگام submit).

### ❌ ممنوع
```razor
@if (IsLoading)
{
    <div class="text-center py-5">
        <div class="spinner-border text-primary"></div>
    </div>
}
```

### ✅ درست
```razor
@if (IsLoading)
{
    <div class="skeleton skeleton-card"></div>
}
```

### انواع Skeleton
| نوع | کاربرد | کد |
|------|--------|-----|
| `text` | متن ساده | `<Skeleton Type="text" />` |
| `title` | عنوان | `<Skeleton Type="title" />` |
| `avatar` | تصویر پروفایل | `<Skeleton Type="avatar" />` |
| `card` | کارت کامل | `<Skeleton Type="card" />` |
| `row` | ردیف لیست | `<Skeleton Type="row" />` |
| `table-row` | ردیف جدول | `<Skeleton Type="table-row" />` |
| `image` | تصویر | `<Skeleton Type="image" />` |
| `button` | دکمه | `<Skeleton Type="button" />` |

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔥 هر صفحه | state لودینگ باید Skeleton باشد |
| 🔥 هر دیالوگ | state لودینگ باید Skeleton باشد |
| 🔥 کامپوننت | از `<Skeleton>` استفاده شود |
| 🔥 استثنا | فقط وضعیت دکمه هنگام submit مجاز به spinner است |
| 🔥 CSS | استایل در `app-consolidated.css` موجود است |

---

## 🚨 MANDATORY: Animation & Effects (انیمیشن و افکت‌ها)

> **قانون اجباری:** هنگام اضافه کردن انیمیشن یا افکت به کامپوننت‌ها، **حتماً** از کلاس‌های CSS utility از پیش تعریف شده استفاده کن. از نوشتن CSS inline یا فایل CSS جداگانه خودداری کن.

### کلاس‌های موجود در `app.css`

| کلاس | کاربرد | کِی مصرف شود |
|-------|--------|-------------|
| `.fade-slide-in` | انیمیشن ورود (fade + slide up) | هر کامپوننتی که نیاز به انیمیشن ورود دارد |
| `.hover-lift` | افکت hover (lift + scale) | کارت‌ها، دکمه‌ها، آیتم‌های قابل کلیک |
| `.glow-effect` | نور متحرک iOS style | دکمه‌های CTA، کارت‌های ویژه، هدر |
| `.glass-card` | کارت شیشه‌ای (glassmorphism) | کارت‌ها، پنل‌ها، باکس‌های اطلاعاتی |
| `.stat-card-animated` | کارت آمار با نور متحرک | داشبورد، آمار، KPI |
| `.page-enter` | انیمیشن ورود صفحه | هر صفحه جدید (Page component) |
| `.stagger-list .list-item` | انیمیشن پلکانی لیست | لیست‌های آیتمی، activity list، منوها |

### ❌ ممنوع
```razor
<!-- نوشتن CSS inline برای انیمیشن -->
<div style="animation: fadeSlide 0.6s ease; opacity: 0;">

<!-- استفاده از کلاس‌های غیراستاندارد -->
<div class="my-custom-animation">

<!-- ایجاد فایل CSS جداگانه -->
<link href="css/animations.css" rel="stylesheet" />
```

### ✅ درست
```razor
<!-- استفاده از کلاس‌های utility -->
<div class="glass-card hover-lift fade-slide-in p-4">
    <h4>عنوان</h4>
    <p>توضیحات</p>
</div>

<!-- لیست با انیمیشن stagger -->
<div class="stagger-list">
    <div class="list-item">آیتم ۱</div>
    <div class="list-item">آیتم ۲</div>
    <div class="list-item">آیتم ۳</div>
</div>

<!-- کارت آمار داشبورد -->
<div class="stat-card-animated fade-slide-in">
    <div class="stat-number">125</div>
    <div class="stat-label">محصولات</div>
</div>

<!-- صفحه با انیمیشن ورود -->
<div class="page-enter">
    <!-- محتوای صفحه -->
</div>
```

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔥 اولویت | ابتدا از کلاس‌های utility استفاده کن، CSS inline آخرین گزینه |
| 🔥 ترکیب | می‌توانی چند کلاس را با هم ترکیب کنی (مثلاً `glass-card hover-lift fade-slide-in`) |
| 🔥 فایل CSS | تمام استایل‌ها در `app.css` — فایل جداگانه ممنوع |
| 🔥 responsive | انیمیشن‌ها باید در `prefers-reduced-motion` غیرفعال شوند (خودکار انجام می‌شود) |
| 🔥 performance | انیمیشن‌های سنگین فقط در دسکتاپ — در موبایل ساده‌تر شوند |
| 🔥 GSAP | برای انیمیشن‌های پیچیده scroll-based از `gsap-animations.js` استفاده کن |

### نمونه ترکیب‌ها

| الگو | کلاس‌ها | کاربرد |
|------|---------|--------|
| کارت داشبورد | `glass-card hover-lift fade-slide-in` | کارت‌های آماری |
| لیست فعالیت | `glass-card stagger-list` | آخرین فعالیت‌ها |
| دکمه CTA | `btn btn-primary glow-effect` | دکمه اصلی صفحه |
| هدر صفحه | `page-enter` | ورود به هر صفحه |
| کارت محصول | `glass-card hover-lift` | لیست محصولات |

---

## 🚨 MANDATORY: Form Description (توضیح فرم)

> **قانون اجباری:** هر فرم و دیالوگ باید در بالای خود یک **توضیح مختصر** داشته باشد تا کاربر قبل از پر کردن فرم بفهمد قرار است چه کاری انجام دهد.

### ❌ ممنوع
```razor
<div class="aco-dialog-body">
    <div class="row g-3">
        <div class="col-12">
            <label class="form-label">نام</label>
            <input type="text" class="form-control" @bind="Model.Name" />
        </div>
    </div>
</div>
```

### ✅ درست
```razor
<div class="aco-dialog-body">
    <div style="background:var(--bg-tertiary);border-radius:12px;padding:12px 16px;margin-bottom:16px;">
        <div style="display:flex;align-items:center;gap:8px;margin-bottom:4px;">
            <i class="bi bi-info-circle" style="color:var(--accent-primary);"></i>
            <strong style="font-size:0.875rem;">راهنما</strong>
        </div>
        <p style="margin:0;font-size:0.8125rem;color:var(--text-secondary);">
            اطلاعات مشتری جدید را وارد کنید. پس از ذخیره، امکان ثبت تعاملات، یادداشت‌ها و پیگیری‌ها وجود خواهد داشت.
        </p>
    </div>
    <div class="row g-3">
        <div class="col-12">
            <label class="form-label">نام</label>
            <input type="text" class="form-control" @bind="Model.Name" />
        </div>
    </div>
</div>
```

### محتوای توضیح
هر توضیح باید شامل موارد زیر باشد:
1. **هدف فرم** — کاربر قرار است چه چیزی ایجاد یا ویرایش کند
2. **اطلاعات لازم** — چه فیلدهایی باید پر شود
3. **رفتار سیستم** — پس از ذخیره چه اتفاقی می‌افتد

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔥 هر فرم | باید توضیح بالای فرم داشته باشد |
| 🔥 هر دیالوگ | باید توضیح بالای فرم داشته باشد |
| 🔥 مختصر | حداکثر ۲ جمله |
| 🔥 فارسی | توضیح به فارسی باشد |
| 🔥 استایل | از الگوی `bg-tertiary` + `info-circle` استفاده شود |

---

## 🚨 MANDATORY: CRUD Service with Cache (سرویس CRUD با کش)

> **قانون اجباری:** برای هر جدول جدید، یک **سرویس CRUD با کش در حافظه** ساخته شود. دیالوگ‌ها فقط UI باشند و منطق CRUD در سرویس باشد.

### ساختار سرویس
```csharp
// Services/MyTableService.cs
public class MyTableService
{
    private readonly IDbService _db;
    private List<MyTable> _cache = new();
    public event Action? OnDataChanged;

    public MyTableService(IDbService db) { _db = db; }
    public List<MyTable> Items => _cache;

    public async Task LoadAsync()
    {
        _cache = await _db.GetAllAsync<MyTable>();
        OnDataChanged?.Invoke();
    }

    public async Task InsertAsync(object data)
    {
        await _db.InsertAsync(data);
        await RefreshAsync();
    }

    public async Task UpdateAsync(object data, object where)
    {
        await _db.UpdateAsync(data, where);
        await RefreshAsync();
    }

    public async Task DeleteAsync(object where)
    {
        await _db.DeleteAsync(where);
        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        _cache = await _db.GetAllAsync<MyTable>();
        OnDataChanged?.Invoke();
    }

    public List<MyTable> Search(Func<MyTable, bool> predicate)
    {
        return _cache.Where(predicate).ToList();
    }
}
```

### ثبت در Program.cs
```csharp
builder.Services.AddScoped<MyTableService>();
```

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔥 هر جدول | باید سرویس جداگانه داشته باشد |
| 🔥 کش در حافظه | لیست یکبار لود و از کش خوانده شود |
| 🔥 رفرش خودکار | پس از Insert/Update/Delete، کش رفرش شود |
| 🔥 رویداد | `OnDataChanged` برای اطلاع‌رسانی به UI |
| 🔥 ثبت | حتماً در `Program.cs` ثبت شود |
| 🔥 دیالوگ | فقط UI باشد، CRUD در سرویس |

---

## 🚨 MANDATORY: Quick Add Button (+) beside SearchableList

> **قانون اجباری:** هر جا از `SearchableList` برای انتخاب آیتم از لیست داینامیک استفاده می‌شود، **حتماً** یک دکمه "+" کنار آن قرار بگیرد تا کاربر بتواند آیتم جدید را بدون ترک فرم فعلی ایجاد کند.

### ❌ ممنوع
```razor
<label class="form-label">نام بانک *</label>
<SearchableList @bind-Value="Model.BankId" Items="_banks" TextSelector="@(b => b.Name)" ValueSelector="@(b => b.Id)" Placeholder="انتخاب بانک..." />
```

### ✅ درست
```razor
<label class="form-label">نام بانک *</label>
<div class="d-flex align-items-center gap-2">
    <div class="flex-grow-1">
        <SearchableList @bind-Value="Model.BankId" Items="_banks" TextSelector="@(b => b.Name)" ValueSelector="@(b => b.Id)" Placeholder="انتخاب بانک..." />
    </div>
    <button class="btn btn-sm btn-outline-primary rounded-circle" style="width:32px;height:32px;padding:0;" @onclick="OpenBankDialog" title="تعریف بانک جدید">
        <i class="bi bi-plus-lg"></i>
    </button>
</div>
```

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔥 هر SearchableList | دکمه "+" کنارش باشد |
| 🔥 رفرش خودکار | پس از بسته شدن مودال، لیست بروزرسانی شود |
| 🔥 آیکون | `bi-plus-lg` در دکمه گرد |
| 🔥 عنوان | `title` برای tooltip داشته باشد |

---

## Project Overview

**Blazor WebAssembly** solution. All new code MUST follow this architecture.

## AGENT BEHAVIOR RULES

### Before Starting Work
1. Read `AGENTS.md` for technical rules and service usage patterns
2. This file is the source of truth for this project

### During Work
- When a new technical pattern/rule is discovered → **update `AGENTS.md`**
- Track what was done, what's pending, and what was decided

## Agent Role

Senior Blazor WebAssembly Architect + Code Generator Engine.

**Task:** Generate complete Blazor WebAssembly features using:
- SQL-first data access
- Bootstrap 5 + Glassmorphism UI
- Modal-first pattern (default)

---

## CORE RULE — Modal-First (ABSOLUTE DEFAULT)

**ALL features MUST be built with Dialog (Modal CRUD).**

Pages are ONLY allowed when the user explicitly requests:
- "page بساز"
- "page-based"
- "با صفحه"

---

## Smart Mode Decision System

The agent MUST automatically decide which mode to use:

### Mode 1: Dialog Only (DEFAULT)

**When:**
- Simple CRUD
- Standard form + list
- Management systems (ERP / CRUD)

**Output:**
- Model
- Modal Dialog (full CRUD)
- NO page

### Mode 2: Split View (Page + Modal)

**When:**
- Large data sets (Big Data Grid)
- Complex filters
- Reports
- Navigation needed

**Output:**
- Page (main list)
- Modal (Create/Edit)

### Mode 3: Page Only (Rare)

**When:**
- Dashboard
- Heavy reports
- UI navigation-heavy

**Output:** Page only

---

## Agent Decision Logic

| Input | Detection | Output |
|-------|-----------|--------|
| CRUD simple → | Form + list → | Dialog only |
| Data large → | Big grid → | Page + Modal |
| Report → | Complex filter → | Page + Modal |
| Management → | Standard CRUD → | Dialog only |
| Dashboard → | Stats/charts → | Page only |

---

## Code Generation Behavior Rules

### ALWAYS:
- Give complete, ready-to-run code
- No extra explanations or theory
- No unnecessary architecture
- Directly executable

### Every Feature MUST Have:
1. Model
2. Dialog (or Page if needed)
3. Full CRUD
4. Bootstrap 5 + Glassmorphism UI

### NEVER:
- Multiple architectures simultaneously
- Clean Architecture
- Backend design
- API design without request
- Extra explanations

---

## Responsive UI — Bootstrap 5

ALL UI must be responsive. Use Bootstrap 5 grid and utility classes. NO MudBlazor.

### index.html (REQUIRED)

```html
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet">
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet">
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
```

### Responsive Grid Breakpoints

| Breakpoint | Class | Width | Device |
|---|---|---|---|
| Extra small | `col-*` | <576px | Mobile portrait |
| Small | `col-sm-*` | ≥576px | Mobile landscape |
| Medium | `col-md-*` | ≥768px | Tablet |
| Large | `col-lg-*` | ≥992px | Laptop |
| Extra large | `col-xl-*` | ≥1200px | Desktop |
| XXL | `col-xxl-*` | ≥1400px | Large desktop |

### Layout Pattern

```razor
<div class="row g-3">
  <div class="col-12 col-sm-6 col-md-4 col-lg-3">Card 1</div>
  <div class="col-12 col-sm-6 col-md-4 col-lg-3">Card 2</div>
  <div class="col-12 col-sm-6 col-md-4 col-lg-3">Card 3</div>
  <div class="col-12 col-sm-6 col-md-4 col-lg-3">Card 4</div>
</div>
```

### Form Pattern (Responsive)

```razor
<div class="row g-3">
  <div class="col-12 col-md-6">
    <label class="form-label">Name</label>
    <InputText class="form-control" @bind-Value="Model.Name" />
  </div>
  <div class="col-12 col-md-6">
    <label class="form-label">Price</label>
    <InputNumber class="form-control" @bind-Value="Model.Price" />
  </div>
  <div class="col-12">
    <button class="btn btn-primary"><i class="bi bi-floppy me-1"></i>Save</button>
  </div>
</div>
```

### Table Pattern (Responsive)

```razor
<div class="table-responsive">
  <table class="table table-striped table-hover table-bordered">
    <thead class="table-dark"><tr><th>ID</th><th>Name</th></tr></thead>
    <tbody>@foreach(var item in list){ <tr><td>@item.Id</td><td>@item.Name</td></tr> }</tbody>
  </table>
</div>
```

### Mobile-First Rules

- Forms: `col-12` on mobile, `col-md-6` on tablet+
- Tables: always wrap in `<div class="table-responsive">`
- Buttons: stack on mobile (`d-grid d-sm-flex gap-2`)
- Sidebar: use `<div class="offcanvas offcanvas-start">` for mobile nav
- Cards: `col-12 col-sm-6 col-md-4 col-lg-3` for stats grids
- Modals: use `modal-fullscreen-sm-down` for mobile-friendly dialogs
- Padding: `px-2 px-sm-3 px-md-4` for responsive spacing

### Icons (Bootstrap Icons)

```html
<i class="bi bi-plus-lg"></i>   <!-- Plus -->
<i class="bi bi-pencil"></i>    <!-- Edit -->
<i class="bi bi-trash"></i>     <!-- Delete -->
<i class="bi bi-floppy"></i>    <!-- Save -->
<i class="bi bi-search"></i>    <!-- Search -->
<i class="bi bi-printer"></i>   <!-- Print -->
<i class="bi bi-gear"></i>      <!-- Settings -->
<i class="bi bi-person"></i>    <!-- User -->
```

---

## UI Design — Clean Flat Design

**Cards and surfaces use solid backgrounds with subtle borders. No backdrop-filter blur effects.**

### Card Base Pattern

```css
.card {
    background: var(--bg-secondary, #ffffff);
    border-radius: 12px;
    border: 1px solid var(--border-color, #e5e7eb);
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}
```

### Design Rules

- Use solid backgrounds (white/light-gray for light mode, dark-gray for dark mode)
- Subtle borders for separation (1px, low opacity)
- Soft shadows (not heavy)
- Consistent border-radius (12px cards, 8px buttons, 6px inputs)
- Animations < 300ms, subtle
- Use GPU-friendly CSS (transform, opacity)
- Maintain contrast for readability

---

## Animation Libraries (JS Interop)

### Available Libraries

| Library | CDN | Use Case |
|---------|-----|----------|
| **GSAP** | `cdnjs.cloudflare.com/ajax/libs/gsap/3.12.5/gsap.min.js` | Complex timelines, ScrollTrigger, parallax |
| **GSAP ScrollTrigger** | `cdnjs.cloudflare.com/ajax/libs/gsap/3.12.5/ScrollTrigger.min.js` | Scroll-driven animations |
| **Anime.js** | `cdnjs.cloudflare.com/ajax/libs/animejs/3.2.2/anime.min.js` | Stagger, keyframes, SVG morphing |
| **Lottie** | `unpkg.com/@dotlottie/player-component@2.7.2/dist/dotlottie-player.mjs` | After Effects animations |
| **AOS** | `unpkg.com/aos@2.3.4/dist/aos.js` | Simple scroll reveals |

### Decision Logic

| نیاز | کتابخانه |
|------|----------|
| Hover ساده | CSS |
| Scroll reveal ساده | AOS |
| Stagger لیست | Anime.js |
| Timeline پیچیده | GSAP |
| Parallax | GSAP ScrollTrigger |
| انیمیشن After Effects | Lottie |
| Pinning | GSAP ScrollTrigger |
| SVG morphing | Anime.js |

---

## Modal Styling Rules

### Structure

```razor
<div class="aco-dialog-body">
  <!-- Form content -->
</div>
<div class="aco-dialog-footer">
  <button class="btn btn-secondary" @onclick="Close">Cancel</button>
  <button class="btn btn-primary" @onclick="Save"><i class="bi bi-floppy me-1"></i>Save</button>
</div>
```

### Footer Buttons

- Cancel: `btn btn-secondary`
- Save: `btn btn-primary`
- Delete: `btn btn-danger`
- All buttons: `min-width: 100px`, `font-weight: 500`
- Gap between buttons: `gap: 10px`

### Mobile Dialog

- Padding: `12px 20px` on screens <576px
- Footer buttons: `flex-direction: column` on very small screens
- Full-width modal on mobile: `modal-fullscreen-sm-down`

---

## Sidebar Navigation Rules (HIERARCHICAL 3-LEVEL)

### ساختار اجباری سایدبار

سایدبار باید **۳ سطحی** باشد:

```
سطح ۱: ماژول‌ها (Dashboard, CMS, Accounting, Security)
سطح ۲: زیربرنامه‌ها (اطلاعات پایه, ورود اطلاعات, گزارشات, ...)
سطح ۳: آیتم‌ها (گروه کل, حساب کل, ...) → هرکدام مودال باز می‌کنند
```

### قوانین

| Rule | Description |
|------|-------------|
| 🔥 Rule 1 | سطح ۱ = ماژول‌های اصلی (قابل کلیک برای باز/بسته شدن) |
| 🔥 Rule 2 | سطح ۲ = زیربرنامه‌ها (زیر هر ماژول) |
| 🔥 Rule ۳ | سطح ۳ = آیتم‌های CRUD → با مودال باز می‌شوند |
| 🔥 Rule 4 | صفحات standalone (مثل Dashboard) → مستقیم ناوبری |
| 🔥 Rule 5 | آیتم‌های CRUD → هیچ‌وقت صفحه جدا نسازند، فقط مودال |
| 🔥 Rule 6 | هر ماژول باید قابلیت collapse/expand داشته باشد |
| 🔥 Rule 7 | آیتم فعال باید highlighted شود |

### CSS برای سایدبار سلسله مراتبی

```css
.sidebar-nav-subgroup {
    padding-inline-start: 16px;
    overflow: hidden;
    max-height: 0;
    transition: max-height 0.3s ease;
}

.sidebar-nav-subgroup.expanded {
    max-height: 1000px;
}

.sidebar-nav-subgroup-title {
    font-size: 0.75rem;
    color: var(--text-tertiary);
    padding: 6px 12px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 6px;
}

.sidebar-nav-subgroup-title:hover {
    color: var(--text-primary);
}

.sidebar-nav-subgroup-title .bi-chevron-down {
    transition: transform 0.2s ease;
    font-size: 0.6rem;
}

.sidebar-nav-subgroup.expanded .sidebar-nav-subgroup-title .bi-chevron-down {
    transform: rotate(180deg);
}

.sidebar-nav-item.sub {
    padding-inline-start: 36px;
    font-size: 0.8125rem;
}
```

---

## TSQL Optimization Rules

### 1. Use Index
```sql
-- ❌ Full Table Scan
SELECT * FROM Products WHERE Name LIKE '%keyword%'

-- ✅ Uses Index
SELECT * FROM Products WHERE Name LIKE 'keyword%'
```

### 2. Select Only Needed Columns
```sql
-- ❌ All columns
SELECT * FROM Products

-- ✅ Only needed columns
SELECT Id, Name, Price FROM Products
```

### 3. Use EXISTS instead of IN
```sql
-- ❌
SELECT * FROM Products WHERE CategoryId IN (SELECT Id FROM Categories WHERE IsActive=1)

-- ✅
SELECT p.* FROM Products p 
WHERE EXISTS (SELECT 1 FROM Categories c WHERE c.Id=p.CategoryId AND c.IsActive=1)
```

### 4. Use Covering Index
```sql
CREATE INDEX IX_Products_Name_Price ON Products (Name) INCLUDE (Price, CategoryId)
```

### 5. Use OFFSET/FETCH for Paging
```sql
SELECT * FROM Products
WHERE IsActive=1
ORDER BY Name
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
```

### 6. Use CTE for Complex Queries
```sql
WITH ActiveProducts AS (
    SELECT Id, Name, Price, CategoryId
    FROM Products
    WHERE IsActive=1
)
SELECT ap.*, c.Name AS CategoryName
FROM ActiveProducts ap
INNER JOIN Categories c ON ap.CategoryId=c.Id
```

### 7. Use NOLOCK for Read-only Queries
```sql
SELECT * FROM Products WITH (NOLOCK) WHERE IsActive=1
```

### 8. Use Proper JOIN
```sql
-- ❌ Implicit JOIN
SELECT * FROM Products, Categories WHERE Products.CategoryId=Categories.Id

-- ✅ Explicit JOIN
SELECT * FROM Products p
INNER JOIN Categories c ON p.CategoryId=c.Id
```

### 9. Use Parameterized Queries
```csharp
// ❌ SQL Injection
var items = await db.QueryAsync<Products>("SELECT * FROM Products WHERE Name='" + name + "'");

// ✅ Parameterized
var items = await db.QueryAsync<Products>("SELECT * FROM Products WHERE Name=@Name", new { Name = name });
```

---

## File Organization

```
Project/
├── Program.cs              # Entry point + service registration
├── App.razor               # Root component
├── Layout/
│   ├── MainLayout.razor    # Main layout
│   └── NavMenu.razor       # Navigation
├── Pages/
│   ├── Home.razor
│   └── [Feature].razor     # Feature pages (only if needed)
├── Dialogs/
│   └── [Feature]Dialog.razor
└── wwwroot/
    ├── appsettings.json
    ├── css/
    └── js/
```

---

## Naming Conventions

- Models: PascalCase, singular (`Cars`, `Users`, `Roles`)
- Services: `I[Name]Service` + `[Name]Service`
- Dialogs: `[Feature]Dialog.razor`
- Pages: `[Feature]Page.razor` or `[Feature].razor`
- Tables: `[Prefix]_[TableName]`
- DB columns: PascalCase matching C# property names

---

## .csproj Settings (REQUIRED for WASM)

```xml
<SelfContained>true</SelfContained>
<BlazorEnableCompression>false</BlazorEnableCompression>
<BlazorWebAssemblyEnableLinking>false</BlazorWebAssemblyEnableLinking>
<BlazorWebAssemblyLoadAllGlobalizationData>true</BlazorWebAssemblyLoadAllGlobalizationData>
<BlazorWebAssemblyPreserveCollationData>false</BlazorWebAssemblyPreserveCollationData>
<BlazorEnableTimeZoneSupport>false</BlazorEnableTimeZoneSupport>
```

---

## 🚀 Auto-Trigger System (MANDATORY)

> **قانون اجباری:** هنگام دریافت درخواست کاربر، Agent باید خودکار مهارت مناسب را بارگذاری کند. نیازی به پرسیدن از کاربر نیست.

### Auto-Trigger Detection Algorithm

```
1. خواندن درخواست کاربر
2. جستجوی کلمات کلیدی
3. اگر کلمه کلیدی پیدا شد → خودکار skill مربوطه را load کن
4. اگر چندین skill مرتبط بود → همه را load کن
5. اگر هیچ کلمه کلیدی پیدا نشد → از skill اصلی استفاده کن
```

---

## Critical Discoveries (from real bugs)

### 1. interop.js Functions Must Be Window Globals

`IJSRuntime.InvokeVoidAsync("functionName")` only finds functions on `window`. ES module exports are invisible.

```js
// ❌ WRONG — Blazor can't find this
export function setDarkMode(isDark) { ... }

// ✅ CORRECT — Available on window
window.setDarkMode = function(isDark) { ... };
```

And the script MUST be loaded in `index.html`:
```html
<script src="js/interop.js"></script>
```

### 2. localStorage Must Store JSON

`GetLocalAsync<string>()` deserializes as JSON. Plain strings fail.

```js
// ❌ WRONG — "dark" is not valid JSON
localStorage.setItem('theme', 'dark');

// ✅ CORRECT — JSON string
localStorage.setItem('theme', JSON.stringify('dark'));
```

And reading in index.html must parse:
```js
var saved = JSON.parse(localStorage.getItem('theme') || '"light"');
```

### 3. overflow:hidden Clips Dropdowns

Never put `overflow: hidden` on containers that have dropdown menus or popovers.

```css
/* ❌ WRONG — clips dropdown */
.topbar { overflow: hidden; }
.modal-content { overflow: hidden; }

/* ✅ CORRECT — let dropdowns escape */
.topbar { overflow: visible; }
.modal-content { overflow: visible; }
```

### 4. Modal Content Must Use Flex Column

For proper scroll + sticky footer inside modal:

```css
.modal-content {
    display: flex;
    flex-direction: column;
    max-height: calc(100vh - 48px);
    overflow: visible;
}
.modal-body {
    flex: 1;
    min-height: 0;
    overflow-y: auto;
    overscroll-behavior: contain;
}
```

### 5. Dropdowns Inside Modals Need position:fixed

Bootstrap dropdowns inside `.modal-body` get clipped. Force them to escape:

```css
.modal-content .dropdown-menu {
    position: fixed;
    z-index: 1070;
}
.modal-content .dropdown-menu.d-block {
    position: fixed;
    z-index: 1070;
    min-width: 280px;
    max-width: calc(100vw - 48px);
}
```

### 6. CSS Import Order Matters

```
_variables.css  → base variables (must be first)
_light.css      → light theme overrides
_dark.css       → dark theme overrides
_rtl.css        → RTL overrides
_layout files   → structural CSS
_component files → component-specific CSS
```

Theme files MUST come after variables but before layout/components.

### 7. Every Dialog Must Have Delete

All CRUD dialogs MUST include a delete button in the table and a `Delete` method:

```csharp
async Task Delete(T item)
{
    var confirmed = await JS.InvokeAsync<bool>("confirm", $"آیا از حذف «{item.Name}» مطمئن هستید؟");
    if (!confirmed) return;

    await db.DeleteAsync<T>(new { item.Id });
    await alert.ShowSuccessAsync("موفق", "حذف شد.");
    await LoadData();
}
```

### 8. ExtractId Only Returns Numeric IDs

`MapUrlToAction` uses `ExtractId` to detect if a URL has a numeric ID. It MUST only return values that parse as `int`. Entity names like "user", "blog", "product" must NOT be treated as IDs:

```csharp
// ❌ WRONG — "user" treated as ID → user.get instead of user.list
private static string? ExtractId(string url) {
    // ...
    return last; // returns "user", "blog", etc.
}

// ✅ CORRECT — only numeric IDs
private static string? ExtractId(string url) {
    var parts = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0) return null;
    var last = parts[^1];
    if (last is "api" or "admin" or "unread" or "count" or "markread" or "password") return null;
    if (int.TryParse(last, out _)) return last;  // only numbers
    return null;
}
```

### 9. HttpClient BaseAddress Must Come From Config

The `HttpClient` BaseAddress MUST read from `appsettings.json` via `IOptions<AppSettings>`, NOT from `builder.HostEnvironment.BaseAddress`:

```csharp
// ❌ WRONG — uses client's own URL (e.g., https://localhost:7125)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// ✅ CORRECT — reads from config
builder.Services.AddScoped(sp => {
    var baseUrl = builder.Configuration["Pdd:ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    return new HttpClient { BaseAddress = new Uri(baseUrl) };
});
```

### 10. AuthService Login Must Use SharedKey Encryption

Login body MUST be encrypted with `SharedKey` (not AES key, which isn't available yet). Use `CryptoUtils.encryptData(payload, SharedKey)` via JS interop:

```csharp
// ❌ WRONG — AES key not set yet, throws InvalidOperationException
var encrypted = await _encryption.EncryptAsync(payload);

// ✅ CORRECT — uses SharedKey directly
const string SharedKey = "pdd-ir-ws-2026-secure-key";
var encrypted = await _js.InvokeAsync<string>("CryptoUtils.encryptData", payload, SharedKey);
```

---

## Layout Splash Dismissal (MANDATORY)

### ❗ قانون اجباری
هر Layout جدیدی که می‌سازی (چه MainLayout چه PublicLayout) **حتماً** باید splash screen را dismiss کند. در غیر این صورت splash screen برای همیشه روی صفحه می‌ماند.

### ✅ درست
```razor
@inherits LayoutComponentBase
@inject IJSRuntime JS

@code {
    protected override async Task OnInitializedAsync()
    {
        try { await JS.InvokeVoidAsync("dismissSplash"); }
        catch { }
    }
}
```

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔥 هر Layout | MainLayout و PublicLayout هر دو باید dismissSplash داشته باشند |
| 🔥 try-catch | حتماً دور `dismissSplash` باشد چون ممکن است splash وجود نداشته باشد |
| 🔥 OnInitializedAsync | در `OnInitializedAsync` فراخوانی شود |

---

## Role-Based Authorization

### Page-level

```razor
@attribute [AuthorizeRole("admin")]
@attribute [AuthorizeRole("admin", "manager")]
```

### Code-level

```csharp
_auth.HasRole("admin")
_auth.HasAnyRole("admin", "manager")
_auth.IsAuthenticated
_auth.CurrentUser
```

---

## Enterprise Software Analyzer Agent (Pre-Analysis)

هر زمان کاربر فقط نام یک زیرسیستم خزانه‌داری را گفت (مثلاً: پرداخت‌ها، دریافت‌ها، مدیریت نقدینگی، خزانه‌داری، بانک، تسویه حساب) — ابتدا تحلیل زیر را انجام بده، سپس خروجی را به Design Agent بده.

### 1. Domain Detection

- اگر کاربر گفت "خزانه‌داری" → کل سیستم مالی عملیاتی را در نظر بگیر (پرداخت، دریافت، بانک، نقدینگی، کنترل موجودی)
- اگر گفت زیرسیستم → فقط همان بخش + وابستگی‌ها

### 2. Analysis Output Structure

خروجی نهایی حتماً این ساختار را رعایت کند:

```
TITLE: Treasury Subsystem Full Analysis

1. Domain Overview
   ...

2. Functional Requirements
   - ...

3. Non-Functional Requirements
   - ...

4. Accounting Rules
   - ...

5. Core Workflows
   Step 1...
   Step 2...

6. Entities (Database Tables Suggestion)
   - TableName: field1, field2, field3...

7. Business Rules
   - ...

8. Risks & Challenges
   - ...

9. Integration Points
   - ...

10. DESIGN AGENT HANDOFF
    - Database Tables: (exact SQL CREATE TABLE)
    - UI Pages: (Dialog-based Blazor list)
    - Fields: (with types and constraints)
```

### 3. Business Rules

- هیچ پرداختی بدون Approval انجام نمی‌شود
- تراکنش‌ها باید Idempotent باشند
- موجودی منفی فقط با اجازه خاص
- همه عملیات باید Audit شوند

### 4. Risks to Analyze

- Duplicate payment
- Bank sync mismatch
- Delay در تراکنش
- خطای انسانی در تایید
- Race condition در پرداخت‌ها

---

## 🖼️ MANDATORY: Smart Image System (سیستم تصویر هوشمند)

> **قانون اجباری:** هنگام نیاز به تصویر در پروژه، ابتدا مهارت `smart-image` را بارگذاری کن و بر اساس منطق تصمیم‌گیری عمل کن.

### Auto-Trigger Keywords

| کلمه کلیدی | اقدام |
|------------|-------|
| عکس، تصویر، image | بارگذاری smart-image skill |
| hero، بنر، پس‌زمینه | بارگذاری smart-image skill |
| آیکون، illustration | بارگذاری smart-image skill |
| داشبورد، dashboard UI | بارگذاری smart-image skill |
| عکس واقعی، stock | بارگذاری smart-image skill |
| glassmorphism، futuristic | بارگذاری smart-image skill |

### Decision Matrix

| نیاز | روش | مثال |
|------|-----|------|
| UI/Dashboard | ✅ AI Generate | glassmorphism, dark mode, futuristic |
| عکس واقعی | ✅ Web Search | بیمارستان واقعی، تیم واقعی |
| تصویر مفهومی | ✅ AI Generate | انتزاعی، gradient، modern |
| پس‌زمینه | ✅ AI Generate | abstract, texture, minimal |
| آیکون | ✅ AI Generate | flat design, clean lines |

### Prompt Templates

#### UI Dashboard
```
modern financial dashboard UI, glassmorphism, ios style, soft glow lighting, dark theme, futuristic
```

#### Hero Section
```
professional [industry] hero banner, [colors] gradient, clean modern design, cinematic lighting
```

#### Background
```
abstract [theme] background, [colors] gradient, subtle texture, minimalist, high resolution
```

### Industry Color Mapping

| صنعت | رنگ‌ها | حس |
|------|--------|-----|
| پزشکی | سفید، آبی، سبز | تمیز، حرفه‌ای |
| مالی | آبی، خاکستری، طلایی | اعتماد، ثبات |
| تکنولوژی | بنفش، فیروزه‌ای، صورتی | نوآوری، انرژی |
| آموزش | آبی، نارنجی، گرم | صمیمی، دسترسی |
| فروشگاهی | متغیر | جذاب، واضح |

### Output Format

```markdown
## Images

### Option 1: [Description]
- **Source**: AI Generated / Web Search
- **Style**: [keywords]
- **Best for**: [use case]
- **Link/Prompt**: [URL or prompt]

### Option 2: [Description]
...

## Recommendation
[کدام گزینه بهتر است و چرا]
```

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔥 اولویت | ابتدا smart-image skill را load کن |
| 🔥 گزینه | همیشه 2-3 گزینه پیشنهاد بده |
| 🔥 توضیح | دلیل انتخاب را توضیح بده |
| 🔥 prompt | برای AI، prompt دقیق بده |
| 🔥 لایسنس | مطمئن شو commercial use مجاز است |
| 🔥 بهینه | تصاویر برای web بهینه باشند |
| 🔥 alt text | حتماً alt text برای accessibility بده |

---

## 🎨 UI/UX Design System & Animation Pipeline

### Design Tokens

```css
:root {
  /* Dark Mode (Default) */
  --pdd-bg: #000000;
  --pdd-text: #ffffff;
  --pdd-border: #222222;

  /* Brand Colors */
  --pdd-blue: #0070f3;
  --pdd-pink: #f81ce5;
  --pdd-purple: #7928ca;
  --pdd-cyan: #50e3c2;

  /* Typography */
  --font-primary: 'Vazirmatn', 'Inter', system-ui, sans-serif;
  --font-heading: 'Inter', system-ui, sans-serif;
}
```

### Animation Pipeline

| Animation | Duration | Easing | Trigger |
|-----------|----------|--------|---------|
| Splash Screen | 1.2s | power2.inOut | Page load |
| Hero Entrance | 0.8s | power3.out | After splash |
| Product Cards Stagger | 0.7s each | power3.out | Scroll (top 82%) |
| Parallax Layers | Continuous | none (scrub) | Scroll position |
| Architecture Pin | 3 steps | power2.out | Scroll (300vh) |
| Typewriter Effect | Variable | none | Step 3 trigger |

### Component Architecture

```
Shared/
├── ParallaxScrolling.razor        # 4-layer parallax with Lenis
├── ArchitectureStorytelling.razor  # Scroll-pinned 3-step stepper
├── ProductCard.razor               # Glassmorphic card with glow
├── CorporateFooter.razor           # Real company info
├── SplashOverlay.razor             # SVG stroke animation
├── AnimatedSection.razor           # Scroll reveal wrapper
├── Skeleton.razor                  # Loading states
└── Toast.razor                     # Notifications
```

### GSAP Interop Functions

| Function | Purpose | File |
|----------|---------|------|
| `initScrollAnimations` | Scroll reveal for .anim-* classes | gsap-animations.js |
| `initMouseParallax` | Mouse-tracking parallax cards | gsap-animations.js |
| `initCounters` | Animated number counters | gsap-animations.js |
| `initHeroAnimation` | Hero section entrance | gsap-animations.js |
| `initTiltEffect` | 3D tilt on hover | gsap-animations.js |
| `initParallaxScroll` | Lenis + ScrollTrigger parallax | gsap-animations.js |
| `initArchitectureScroll` | Scroll-pinned stepper | gsap-animations.js |
| `initTypewriterEffect` | TextPlugin typewriter | gsap-animations.js |

### RTL Support Rules

1. All Persian text must use `dir="rtl"` attribute
2. CSS must handle RTL layout with `[dir="rtl"]` selectors
3. Form inputs for Persian text: `text-align: right`
4. Email/phone inputs: `text-align: left` (keep LTR)
5. Font family: Vazirmatn for Persian, Inter for English

### Mobile Responsiveness

| Breakpoint | Behavior |
|------------|----------|
| < 768px | Parallax layers simplified, sidebar hidden |
| 768px - 1024px | Tablet layout, 2-column grids |
| > 1024px | Full desktop layout, 4-column grids |

### Memory Management

All animated components MUST implement `IAsyncDisposable`:
```csharp
@implements IAsyncDisposable

public async ValueTask DisposeAsync()
{
    // Clean up GSAP ScrollTrigger instances
    // Destroy Lenis instance
    // Remove event listeners
}
```

---

## 🌐 MANDATORY: i18n (Multi-language Support)

> **قانون اجباری:** تمام متن‌های کاربر باید از سیستم ترجمه خوانده شوند.

### Usage
```razor
@inject ITranslateService T

<h1>@T.Text("hero_title_1")</h1>
<button>@T.Text("send_message")</button>
```

### Changing Language
```csharp
async Task SwitchLang(string lang)
{
    await T.LoadLanguageAsync(lang);
    StateHasChanged();
}
```

### Translation Files
```
wwwroot/lang/en.json
wwwroot/lang/fa.json
```

### Rules
| Rule | Description |
|------|-------------|
| 🔥 Use `T.Text()` | Never hardcode user-facing text |
| 🔥 Key naming | Use snake_case (hero_title_1, send_message) |
| 🔥 Fallback | If key not found, key itself is shown |
| 🔥 Add to both files | Always update en.json AND fa.json |

---

## 🎨 MANDATORY: Single CSS File

> **قانون اجباری:** تمام CSS پروژه در یک فایل `wwwroot/css/app.css` است.

### ❌ ممنوع
- فایل‌های CSS جداگانه
- Scoped CSS در ریزورها
- فایل‌های theme.css, layout.css و...

### ✅ درست
- فقط `wwwroot/css/app.css`
- استفاده از CSS variables برای تم
- Responsive با media queries

---

## 🏠 MANDATORY: Home Page Structure

> **قانون اجباری:** صفحه خانه شامل این بخش‌هاست:

### Sections
1. **HeroSlider** — فول‌اسکرین اسلایدر ۳ اسلایدی با GSAP
2. **ClientsSlider** — اسلایدر لوگوی مشتریان (infinite loop)
3. **Stats** — آمار شرکت (سال تجربه، مراکز درمانی و...)
4. **Products Preview** — پیش‌نمایش محصولات
5. **CTA** — دعوت به اقدام
6. **Footer** — فوتر شرکتی

### Hero Slider Rules
| Rule | Description |
|------|-------------|
| 🔥 Full Screen | 100vh height |
| 🔥 3 Slides | Company intro, Hospital, Products |
| 🔥 Auto-play | 5 second intervals |
| 🔥 Progress bar | Bottom progress indicator |
| 🔥 Manual control | Dots + arrows |

---

## 📁 Project File Structure

```
Pdd.ir.Client/
├── Program.cs                    # Service registration
├── App.razor                     # Root component
├── Layout/
│   ├── MainLayout.razor          # Landing vs Admin layout
│   └── NavMenu.razor             # Sidebar navigation
├── Pages/
│   ├── Home.razor                # Landing page
│   ├── Products.razor            # Products page
│   ├── Blog.razor                # Blog listing
│   ├── BlogDetail.razor          # Blog article
│   ├── Portfolio.razor           # Portfolio listing
│   ├── About.razor               # About page
│   ├── Contact.razor             # Contact page
│   ├── NotFound.razor            # 404 page
│   └── Admin/
│       ├── Dashboard.razor       # Admin dashboard
│       ├── Products.razor        # Product management
│       ├── BlogAdmin.razor       # Blog management
│       ├── PortfolioAdmin.razor  # Portfolio management
│       ├── Messages.razor        # Message management
│       ├── Users.razor           # User management
│       └── Settings.razor        # Site settings
├── Shared/
│   ├── HeroSlider.razor          # Hero slider component
│   ├── ClientsSlider.razor       # Client logos slider
│   ├── GlassToolbar.razor        # Landing page toolbar
│   ├── CorporateFooter.razor     # Footer component
│   ├── PageHeader.razor          # Page header
│   ├── Skeleton.razor            # Loading skeleton
│   └── AnimatedSection.razor     # Scroll reveal
├── Services/
│   ├── ITranslateService.cs      # Translation interface
│   ├── TranslateService.cs       # Translation implementation
│   ├── AuthService.cs            # Authentication
│   ├── ConnectionService.cs      # WebSocket connection
│   ├── EncryptionService.cs      # Encryption
│   └── ApiClient.cs              # API client
├── Models/                       # DTOs and models
└── wwwroot/
    ├── css/app.css               # Single CSS file
    ├── lang/en.json              # English translations
    ├── lang/fa.json              # Persian translations
    └── js/                       # JavaScript files
```

---

## 🚨 MANDATORY: Client-Server Communication (کامل و نهایی)

> **قانون اجباری:** تمام ارتباطات کلاینت با سرور باید از طریق `ICommunicationService` انجام شود. استفاده مستقیم از `HttpClient` در صفحات ممنوع است.

### آدرس‌ها از appsettings.json خوانده می‌شوند

فایل `wwwroot/appsettings.json` در کلاینت:
```json
{
  "Pdd": {
    "ApiSettings": {
      "BaseUrl": "http://localhost:5000",
      "WebSocketUrl": "ws://localhost:5000"
    }
  }
}
```

### Program.cs — ثبت configuration
```csharp
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("Pdd"));
builder.Services.AddScoped(sp =>
{
    var baseUrl = builder.Configuration["Pdd:ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    return new HttpClient { BaseAddress = new Uri(baseUrl) };
});
```

### نحوه ارتباط در صفحات
```razor
@inject ICommunicationService Comm

// لیست — وقتی WS وصله از WS، وگرنه HTTP
var items = await Comm.GetAsync<List<ProductDto>>("api/product");

// ایجاد
var result = await Comm.PostAsync<object>("api/product", new { Title = "..." });

// ویرایش
var ok = await Comm.PutAsync<object>($"api/product/{id}", new { Title = "..." });

// حذف
var ok = await Comm.DeleteAsync($"api/product/{id}");
```

### قوانین ارتباط
| قانون | توضیح |
|-------|-------|
| 🔥 فقط Comm | همه درخواست‌ها از `ICommunicationService` — هیچ سرویسی حق ندارد مستقیم از `HttpClient` استفاده کند |
| 🔥 WS-first | اگه WS وصله → همه از WS (GET/POST/PUT/DELETE) |
| 🔥 HTTP fallback | اگه WS قطعه → همه از HTTP |
| 🔥 Auth endpoints | `/auth/*` همیشه HTTP (نه WS) |
| 🔥 هر درخواست auth جدید | هر درخواست `X-Auth` header جدید میخواد (timestamp + nonce تکراری نباشه) |
| 🔥 Handshake | `MainLayout.OnAfterRenderAsync` → `Comm.InitializeAsync()` |
| 🔥 لاگین | از `Comm.PostAsync` با SharedKey encryption |

### ⚠️ قانون طلایی: سرویس‌های ارتباطی غیرقابل تغییر هستند

> **هیچ سرویسی حق ندارد مستقیم از `HttpClient` استفاده کند. تمام ارتباطات باید از طریق `ICommunicationService` انجام شود — حتی در سرویس‌هایی مانند `AuthService`.**

**سرویس‌هایی که نباید تغییر کنند (زیرساخت ارتباطی):**
- `ICommunicationService` / `CommunicationService`
- `SecurityService`
- `EncryptionService`

**سرویس‌های تجاری که باید از Comm استفاده کنند:**
- `AuthService` — باید از `_comm.PostAsync` برای لاگین استفاده کند
- هر سرویس دیگری که نیاز به ارتباط با سرور دارد

**نمونه صحیح (Users.razor):**
```razor
@inject ICommunicationService Comm

// ✅ صحیح — همه درخواست‌ها از Comm
var items = await Comm.GetAsync<List<UserDto>>("api/user");
await Comm.DeleteAsync($"api/user/{user.Id}");
```

**نمونه غلط (AuthService قبل از اصلاح):**
```csharp
// ❌ غلط — استفاده مستقیم از HttpClient
var response = await _http.PostAsJsonAsync("api/auth/login", new { Username = username, Password = password });

// ❌ غلط — تنظیم مستقیم header
_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
```

**نمونه صحیح (AuthService بعد از اصلاح):**
```csharp
// ✅ صحیح — استفاده از Comm
var result = await _comm.PostAsync<LoginResponse>("api/auth/login", new { Username = username, Password = password });
if (result != null) { Token = result.Token; ... }
```

### ❌ ممنوع
```razor
@inject HttpClient Http
var data = await Http.GetFromJsonAsync<List<Product>>("api/product");

// یا مستقیم _http.PostAsJsonAsync
```

### ✅ درست
```razor
@inject ICommunicationService Comm
var data = await Comm.GetAsync<List<ProductDto>>("api/product");
```

### WebSocket — URL Mapping (خودکار)

`CommunicationService.MapUrlToAction` آدرس URL رو **خودکار** به WS action تبدیل می‌کنه:

| URL | WS Action | قانون |
|-----|-----------|-------|
| `api/product` | `product.list` | `{entity}.list` |
| `api/product/5` | `product.get` | `{entity}.get` |
| `POST api/client` | `client.create` | `POST → {entity}.create` |
| `PUT api/client/5` | `client.update` | `PUT + ID → {entity}.update` |
| `DELETE api/client/5` | `client.delete` | `DELETE + ID → {entity}.delete` |
| `api/client/admin` | `client.admin` | `contains "admin" → {entity}.admin` |
| `api/contact/5/markread` | `contact.markread` | `contains "markread" → {entity}.markread` |
| `api/blog/admin` | `blog.admin` | `contains "admin" → {entity}.admin` |

**نکته مهم:** `ExtractId` فقط رشته‌های عددی (`int.TryParse`) رو ID میشناسه. اگه آخر URL کلمه‌ای مثل `"user"` یا `"blog"` باشه، به عنوان ID شناخته نمیشه و action `list` برمیگردونه.

### ساختار کامل سرویس‌های ارتباطی

```
Client/
├── Services/
│   ├── CommunicationService.cs     # WS-first + HTTP fallback (اصلی)
│   ├── SecurityService.cs          # Handshake + Session + Auth Header
│   ├── EncryptionService.cs        # AES-256-CBC ( بعد از handshake)
│   └── AuthService.cs              # لاگین/لاگاوت (از Comm استفاده می‌کنه)
├── Models/
│   └── AppSettings.cs              # BaseUrl + WebSocketUrl
└── wwwroot/
    └── appsettings.json            # آدرس‌های سرور

Server/
├── WebSocket/
│   └── WebSocketHandler.cs         # تمام WS actions
├── Services/
│   ├── SessionAuthAttribute.cs     # فیلتر auth برای HTTP
│   ├── RequestDecryptionMiddleware.cs  # decrypt درخواست‌ها
│   └── ResponseEncryptionMiddleware.cs # encrypt پاسخ‌ها
└── appsettings.json                # ApiKey = SharedKey
```

### Encryption Flow

```
Client                              Server
  │                                    │
  │── HTTP POST /api/auth/handshake ──→│  (SharedKey encrypt)
  │←── encrypted sessionToken ─────────│
  │                                    │
  │── WS: { action, auth, data } ─────→│  (SharedKey encrypt/decrypt)
  │←── WS: { action, data, success } ─│  (SharedKey encrypt)
  │                                    │
  │── HTTP: X-Auth header ────────────→│  (SharedKey encrypt per request)
  │←── encrypted response ────────────│
```

### Shared Key
- **Key**: `pdd-ir-ws-2026-secure-key`
- **Client**: `SecurityService.cs` → `const string SharedKey`
- **Server**: `appsettings.json` → `ApiKey`
- **Algorithm**: AES-256-CBC, SHA256(key) → 32 byte key, random IV prepended to ciphertext

---

## 🚨 MANDATORY: WebSocket Handler Pattern (Server-Side)

> **قانون اجباری:** سمت سرور باید WebSocket handler روی مسیر `/ws` باشد و تمام action ها در یک `switch` expression مدیریت شوند.

### ساختار WsRequest/WsResponse
```csharp
public class WsRequest
{
    public string? Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Data { get; set; }
}

public class WsResponse
{
    public string? Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Message { get; set; }
    public JsonElement? Data { get; set; }
}
```

### الگوی Handler
```csharp
private static async Task<WsResponse> HandleProductList(IServiceScope scope)
{
    var svc = scope.ServiceProvider.GetRequiredService<ProductBusinessService>();
    var data = await svc.GetAllAsync();
    return new WsResponse
    {
        Action = "product.list",
        Success = true,
        Data = JsonSerializer.SerializeToElement(data)
    };
}
```

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔹 Action naming | `entity.operation` (مثلاً `product.list`) |
| 🔹 Scope | هر درخواست `IServiceScope` جداگانه |
| 🔹 Error handling | `try-catch` جداگانه برای هر action |
| 🔹 JSON options | `PropertyNameCaseInsensitive = true` |
| 🔹 Large messages | `ReceiveFullMessageAsync` برای پیام‌های >64KB |
| 🔹 Response ID | درخواست و پاسخ باید `Id` مشترک داشته باشند |

### نمونه کامل action
```csharp
"product.list" => await HandleProductList(scope),
"product.get" => await HandleProductGet(scope, request.Data),
"product.category" => await HandleProductCategory(scope, request.Data),
```

---

## 🚨 MANDATORY: Role & Permission System

> **قانون اجباری:** سیستم احراز هویت باید مبتنی بر نقش باشد. هر صفحه admin باید بررسی نقش کند.

### دیتابیس
```
Roles → RolePermissions ← Permissions
  ↓
Users (ستون Role به‌صورت string)
```

### Permission Naming Convention
```
entity.operation
مثلاً: product.view, product.create, product.edit, product.delete
```

### دسته‌بندی دسترسی‌ها
| دسته | دسترسی‌ها |
|------|-----------|
| Products | view, create, edit, delete |
| Blog | view, create, edit, delete |
| Portfolio | view, create, edit, delete |
| Contact | view, delete |
| Pages | view, create, edit, delete |
| Users | view, create, edit, delete |
| Roles | view, create, edit, delete |
| Settings | view, edit |

### در صفحات
```razor
@attribute [AuthorizeRole("admin")]
@attribute [AuthorizeRole("admin", "manager")]
```

```csharp
_auth.HasRole("admin")
_auth.HasAnyRole("admin", "manager")
```

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔹 Admin | به‌صورت پیش‌فرض همه دسترسی‌ها |
| 🔹 Naming | کوچک با نقطه (`product.view`) |
| 🔹 Migration | فایل SQL در `wwwroot/resource/` |
| 🔹 Frontend | مدل‌ها در `Client/Models/` |

---

## 🚨 MANDATORY: Admin CRUD Page Pattern

> **قانون اجباری:** تمام صفحات admin باید از الگوی زیر پیروی کنند.

### ساختار صفحه
```razor
@page "/admin/[entity]"
@inject ICommunicationService Comm
@inject ITranslateService T
@inject IJSRuntime JS

<PageTitle>مدیریت [entity] | PDD</PageTitle>

<div class="page-enter">
    @* Header + دکمه جدید *@ @* Skeleton loading *@ @* جدول با جستجو *@
</div>

@if (ShowDialog) { @* مودال ایجاد/ویرایش *@ }
```

### متغیرهای الزامی
```csharp
List<EntityDto> Items = new();
List<EntityDto> FilteredItems = new();
EntityDto FormModel = new();
bool IsLoading = true;
bool ShowDialog = false;
bool IsEdit = false;
bool IsSaving = false;
string SearchText = "";
```

### الگوی جستجو
```csharp
void OnSearch(ChangeEventArgs e)
{
    SearchText = e.Value?.ToString() ?? "";
    Filter();
}

void Filter()
{
    var q = SearchText.Trim().ToLower();
    FilteredItems = string.IsNullOrEmpty(q) ? Items
        : Items.Where(x => (x.Name ?? "").ToLower().Contains(q)).ToList();
}
```

### الگوی CRUD
```csharp
// Load
async Task LoadData()
{
    IsLoading = true;
    Items = await Comm.GetAsync<List<EntityDto>>("api/entity") ?? new();
    Filter();
    IsLoading = false;
}

// Create
void OpenCreateDialog()
{
    FormModel = new EntityDto();
    IsEdit = false;
    ShowDialog = true;
}

// Edit
void OpenEditDialog(EntityDto item)
{
    FormModel = new EntityDto { Id = item.Id, Name = item.Name };
    IsEdit = true;
    ShowDialog = true;
}

// Save
async Task Save()
{
    IsSaving = true;
    if (IsEdit)
        await Comm.PutAsync<object>($"api/entity/{FormModel.Id}", FormModel);
    else
        await Comm.PostAsync<object>("api/entity", FormModel);
    await LoadData();
    ShowDialog = false;
    IsSaving = false;
}

// Delete
async Task Delete(EntityDto item)
{
    var confirmed = await JS.InvokeAsync<bool>("confirm", $"آیا از حذف مطمئن هستید؟");
    if (!confirmed) return;
    await Comm.DeleteAsync($"api/entity/{item.Id}");
    await LoadData();
}
```

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔹 Service | فقط `ICommunicationService` |
| 🔹 Loading | Skeleton (نه spinner) |
| 🔹 Search | Debounce با `CancellationTokenSource` |
| 🔹 Delete | همیشه با `confirm` |
| 🔹 Error | try-catch + نمایش خطا |
| 🔹 Naming | فایل `[Entity]s.razor` |
| 🔹 URL | `/admin/[entity]` |

---

## 🚨 MANDATORY: Server Controller Pattern

> **قانون اجباری:** تمام کنترلرها باید از الگوی زیر پیروی کنند.

### ساختار
```csharp
[ApiController]
[Route("api/[entity]")]
public class EntityController : ControllerBase
{
    private readonly EntityBusinessService _service;

    public EntityController(EntityBusinessService service)
    {
        _service = service;
    }

    [HttpGet]        // لیست
    [HttpGet("{id}")] // دریافت
    [HttpPost]       // ایجاد
    [HttpPut("{id}")] // ویرایش
    [HttpDelete("{id}")] // حذف
}
```

### قوانین
| قانون | توضیح |
|-------|-------|
| 🔹 Route | `api/[entity]` |
| 🔹 Validation | بررسی فیلدهای الزامی |
| 🔹 Response | `{ id, message }` |
| 🔹 Error | `BadRequest(new { message })` |
| 🔹 NotFound | `NotFound()` |
| 🔹 DI | `BusinessService` از طریق constructor |

---

## 🚨 MANDATORY: Project File Structure (Updated)

```
Pdd.ir.Client/
├── Program.cs
├── Services/
│   ├── CommunicationService.cs    # WS + HTTP (اصلی)
│   ├── ICommunicationService.cs   # Interface
│   ├── AuthService.cs             # JWT Auth
│   ├── EncryptionService.cs       # AES Encryption
│   ├── TranslateService.cs        # i18n
│   └── AnimationService.cs        # GSAP Animations
├── Pages/
│   └── Admin/
│       ├── Users.razor            # مدیریت کاربران
│       ├── Roles.razor            # مدیریت نقش‌ها
│       └── ...
├── Models/
│   ├── UserDto.cs
│   ├── RolePermissionDto.cs
│   └── ...
└── Shared/
    ├── CustomerShowcase.razor
    └── ...

Pdd.ir.Server/
├── Controllers/
│   ├── UserController.cs
│   ├── RoleController.cs
│   ├── PermissionController.cs
│   └── ...
├── WebSocket/
│   └── WebSocketHandler.cs       # تمام action ها
├── Services/
│   └── ConnectionManager.cs
└── wwwroot/resource/
    └── 202607131200_Add_Permissions.sql
```

