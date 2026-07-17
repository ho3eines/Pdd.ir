using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace Pdd.ir.Client.Services;

public class TranslateService : ITranslateService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private Dictionary<string, string> _translations = new();
    private string _currentLanguage = "fa";
    private bool _loaded = false;

    public string CurrentLanguage => _currentLanguage;
    public event Action? OnLanguageChanged;

    public TranslateService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task LoadLanguageAsync(string culture)
    {
        _currentLanguage = culture;

        // Load translations
        try
        {
            var json = await _http.GetStringAsync($"lang/{culture}.json");
            _translations = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            _loaded = true;
        }
        catch
        {
            // Fallback: try loading from embedded or default
            _translations = GetFallbackTranslations(culture);
            _loaded = true;
        }

        // Save to localStorage and set direction
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "app_lang", culture);
            await _js.InvokeVoidAsync("setDocDirection", culture);
        }
        catch { }

        OnLanguageChanged?.Invoke();
    }

    public string Text(string key)
    {
        if (!_loaded) return key;
        return _translations.TryGetValue(key, out var value) ? value : key;
    }

    private Dictionary<string, string> GetFallbackTranslations(string culture)
    {
        if (culture == "fa") return GetFaTranslations();
        if (culture == "en") return GetEnTranslations();
        return GetEnTranslations();
    }

    private Dictionary<string, string> GetFaTranslations() => new()
    {
        ["home"] = "خانه",
        ["products"] = "محصولات",
        ["blog"] = "بلاگ",
        ["portfolio"] = "نمونه کارها",
        ["about"] = "درباره ما",
        ["contact"] = "تماس با ما",
        ["login"] = "ورود",
        ["logout"] = "خروج",
        ["dashboard"] = "داشبورد",
        ["settings"] = "تنظیمات",
        ["save"] = "ذخیره",
        ["cancel"] = "لغو",
        ["delete"] = "حذف",
        ["edit"] = "ویرایش",
        ["search"] = "جستجو",
        ["loading"] = "در حال بارگذاری...",
        ["no_data"] = "داده‌ای موجود نیست",
        ["active"] = "فعال",
        ["inactive"] = "غیرفعال",
        ["status"] = "وضعیت",
        ["actions"] = "عملیات",
        ["records"] = "رکورد",
        ["of"] = "از",
        ["success"] = "موفق",
        ["error"] = "خطا",
        ["warning"] = "هشدار",
        ["info"] = "اطلاعات",
        ["saved_successfully"] = "با موفقیت ذخیره شد",
        ["deleted_successfully"] = "با موفقیت حذف شد",
        ["are_you_sure_delete"] = "آیا از حذف مطمئن هستید؟",
        ["required_field"] = "فیلد الزامی است",
        ["username_required"] = "نام کاربری الزامی است",
        ["role_required"] = "نام نقش الزامی است",
        ["password_required"] = "رمز عبور را وارد کنید",
        ["password_changed"] = "رمز عبور تغییر کرد",
        ["login_failed"] = "نام کاربری یا رمز عبور اشتباه است",
        ["login_empty"] = "نام کاربری و رمز عبور را وارد کنید",
        ["login_title"] = "ورود به پنل مدیریت",
        ["login_subtitle"] = "نام کاربری و رمز عبور خود را وارد کنید",
        ["username"] = "نام کاربری",
        ["password"] = "رمز عبور",
        ["full_name"] = "نام کامل",
        ["role"] = "نقش",
        ["category"] = "دسته‌بندی",
        ["title"] = "عنوان",
        ["subtitle"] = "زیرعنوان",
        ["description"] = "توضیحات",
        ["content"] = "محتوا",
        ["slug"] = "Slug",
        ["summary"] = "خلاصه",
        ["author"] = "نویسنده",
        ["published"] = "منتشر شده",
        ["draft"] = "پیش‌نویس",
        ["view_count"] = "بازدید",
        ["sort_order"] = "ترتیب نمایش",
        ["image_url"] = "تصویر URL",
        ["project_url"] = "آدرس پروژه",
        ["email_field"] = "ایمیل",
        ["phone_field"] = "تلفن",
        ["date"] = "تاریخ",
        ["name"] = "نام",
        ["subject_field"] = "موضوع",
        ["message"] = "پیام",
        ["read"] = "خوانده شده",
        ["unread"] = "جدید",
        ["product_saved"] = "محصول ذخیره شد",
        ["blog_saved"] = "مقاله ذخیره شد",
        ["portfolio_saved"] = "نمونه کار ذخیره شد",
        ["user_saved"] = "کاربر ذخیره شد",
        ["role_saved"] = "نقش ذخیره شد",
        ["product_new"] = "محصول جدید",
        ["product_edit"] = "ویرایش محصول",
        ["blog_new"] = "مقاله جدید",
        ["blog_edit"] = "ویرایش مقاله",
        ["portfolio_new"] = "نمونه کار جدید",
        ["portfolio_edit"] = "ویرایش نمونه کار",
        ["user_new"] = "کاربر جدید",
        ["user_edit"] = "ویرایش کاربر",
        ["role_new"] = "نقش جدید",
        ["role_edit"] = "ویرایش نقش",
        ["password_change"] = "تغییر رمز عبور",
        ["password_new"] = "رمز عبور جدید",
        ["permissions"] = "دسترسی‌ها",
        ["select_all"] = "انتخاب همه",
        ["deselect_all"] = "لغو انتخاب",
        ["guide"] = "راهنما",
        ["admin_panel"] = "پنل مدیریت",
        ["back_to_site"] = "بازگشت به سایت",
        ["admin_products"] = "مدیریت محصولات",
        ["admin_blog"] = "مدیریت بلاگ",
        ["admin_portfolio"] = "مدیریت نمونه کارها",
        ["admin_messages"] = "پیام‌ها",
        ["admin_users"] = "کاربران",
        ["admin_settings"] = "تنظیمات",
        ["new_item"] = "جدید",
        ["confirm"] = "تأیید",
        ["yes"] = "بله",
        ["no"] = "خیر",
        ["send_message"] = "ارسال پیام",
        ["contact_us"] = "تماس با ما",
        ["back_to_site"] = "بازگشت به سایت",
        ["years_experience"] = "سال تجربه",
        ["medical_centers"] = "مرکز درمانی",
        ["subsystems"] = "زیرسیستم",
        ["support_24_7"] = "پشتیبانی ۲۴/۷",
        ["address"] = "آدرس",
        ["phone"] = "تلفن",
        ["email"] = "ایمیل",
        ["company_name"] = "شرکت مهندسی طراح داده پیشرو",
        ["all_rights_reserved"] = "تمامی حقوق محفوظ است",
        ["hero_title_1"] = "راهکارهای هوشمند نرم‌افزاری",
        ["hero_subtitle_1"] = "طراحی سیستم‌های مدرن برای آینده",
        ["hero_title_2"] = "مدیریت هوشمند بیمارستان",
        ["hero_subtitle_2"] = "نرم‌افزار یکپارچه درمانی برای بیمارستان‌های مدرن",
        ["hero_title_3"] = "محصولات ما",
        ["hero_subtitle_3"] = "شش سیستم یکپارچه برای مدیریت جامع مراکز درمانی",
        ["view_products"] = "مشاهده محصولات",
        ["ready_to_start"] = "آماده شروع هستید؟",
        ["contact_description"] = "با ما تماس بگیرید و بهترین راهکار را دریافت کنید",
        ["latest_projects"] = "آخرین پروژه‌های شرکت",
        ["company_goal"] = "هدف اصلی شرکت",
        ["customer_growth"] = "سیر صعودی مشتریان شرکت"
    };

    private Dictionary<string, string> GetEnTranslations() => new()
    {
        ["home"] = "Home",
        ["products"] = "Products",
        ["blog"] = "Blog",
        ["portfolio"] = "Portfolio",
        ["about"] = "About Us",
        ["contact"] = "Contact Us",
        ["login"] = "Login",
        ["logout"] = "Logout",
        ["dashboard"] = "Dashboard",
        ["settings"] = "Settings",
        ["save"] = "Save",
        ["cancel"] = "Cancel",
        ["delete"] = "Delete",
        ["edit"] = "Edit",
        ["search"] = "Search",
        ["loading"] = "Loading...",
        ["no_data"] = "No data available",
        ["active"] = "Active",
        ["inactive"] = "Inactive",
        ["status"] = "Status",
        ["actions"] = "Actions",
        ["records"] = "records",
        ["of"] = "of",
        ["success"] = "Success",
        ["error"] = "Error",
        ["warning"] = "Warning",
        ["info"] = "Info",
        ["saved_successfully"] = "Saved successfully",
        ["deleted_successfully"] = "Deleted successfully",
        ["are_you_sure_delete"] = "Are you sure you want to delete?",
        ["required_field"] = "This field is required",
        ["username_required"] = "Username is required",
        ["role_required"] = "Role name is required",
        ["password_required"] = "Please enter password",
        ["password_changed"] = "Password has been changed",
        ["login_failed"] = "Invalid username or password",
        ["login_empty"] = "Please enter username and password",
        ["login_title"] = "Admin Panel Login",
        ["login_subtitle"] = "Enter your username and password",
        ["username"] = "Username",
        ["password"] = "Password",
        ["full_name"] = "Full Name",
        ["role"] = "Role",
        ["category"] = "Category",
        ["title"] = "Title",
        ["subtitle"] = "Subtitle",
        ["description"] = "Description",
        ["content"] = "Content",
        ["slug"] = "Slug",
        ["summary"] = "Summary",
        ["author"] = "Author",
        ["published"] = "Published",
        ["draft"] = "Draft",
        ["view_count"] = "Views",
        ["sort_order"] = "Sort Order",
        ["image_url"] = "Image URL",
        ["project_url"] = "Project URL",
        ["email_field"] = "Email",
        ["phone_field"] = "Phone",
        ["date"] = "Date",
        ["name"] = "Name",
        ["subject_field"] = "Subject",
        ["message"] = "Message",
        ["read"] = "Read",
        ["unread"] = "Unread",
        ["product_saved"] = "Product saved",
        ["blog_saved"] = "Article saved",
        ["portfolio_saved"] = "Portfolio saved",
        ["user_saved"] = "User saved",
        ["role_saved"] = "Role saved",
        ["product_new"] = "New Product",
        ["product_edit"] = "Edit Product",
        ["blog_new"] = "New Article",
        ["blog_edit"] = "Edit Article",
        ["portfolio_new"] = "New Portfolio",
        ["portfolio_edit"] = "Edit Portfolio",
        ["user_new"] = "New User",
        ["user_edit"] = "Edit User",
        ["role_new"] = "New Role",
        ["role_edit"] = "Edit Role",
        ["password_change"] = "Change Password",
        ["password_new"] = "New Password",
        ["permissions"] = "Permissions",
        ["select_all"] = "Select All",
        ["deselect_all"] = "Deselect All",
        ["guide"] = "Guide",
        ["admin_panel"] = "Admin Panel",
        ["back_to_site"] = "Back to Site",
        ["admin_products"] = "Product Management",
        ["admin_blog"] = "Blog Management",
        ["admin_portfolio"] = "Portfolio Management",
        ["admin_messages"] = "Messages",
        ["admin_users"] = "Users",
        ["admin_settings"] = "Settings",
        ["new_item"] = "New",
        ["confirm"] = "Confirm",
        ["yes"] = "Yes",
        ["no"] = "No",
        ["send_message"] = "Send Message",
        ["contact_us"] = "Contact Us",
        ["years_experience"] = "Years Experience",
        ["medical_centers"] = "Medical Centers",
        ["subsystems"] = "Subsystems",
        ["support_24_7"] = "24/7 Support",
        ["address"] = "Address",
        ["phone"] = "Phone",
        ["email"] = "Email",
        ["company_name"] = "PDD Engineering Company",
        ["all_rights_reserved"] = "All Rights Reserved",
        ["hero_title_1"] = "Smart Software Solutions",
        ["hero_subtitle_1"] = "Designing modern systems for the future",
        ["hero_title_2"] = "Smart Hospital Management",
        ["hero_subtitle_2"] = "Integrated healthcare software for modern hospitals",
        ["hero_title_3"] = "Our Products",
        ["hero_subtitle_3"] = "Six integrated systems for comprehensive healthcare management",
        ["view_products"] = "View Products",
        ["ready_to_start"] = "Ready to Get Started?",
        ["contact_description"] = "Contact us and get the best solution",
        ["latest_projects"] = "Latest Company Projects",
        ["company_goal"] = "Company Main Goal",
        ["customer_growth"] = "Company Customer Growth"
    };
}
