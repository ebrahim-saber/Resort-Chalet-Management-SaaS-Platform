// LuxeStay - Client-side Bilingual Translation & RTL Engine

const translations = {
    en: {
        // Storefront Keys
        "luxury_chalet_stays": "Luxury Chalet Stays",
        "find_mountain_hideaway": "Find Your Ultimate Mountain Hideaway",
        "browse_luxury_spaces": "Browse luxury spaces, review pricing, and book your stay instantly.",
        "check_room_availability": "Check Room Availability",
        "arrival_date": "Arrival Date",
        "departure_date": "Departure Date",
        "guests": "Guests",
        "update_search": "Update Search",
        "available_premium_properties": "Available Premium Properties",
        "max_capacity_guests": "Max {count} Guests",
        "free_wifi": "Free WiFi",
        "hot_tub": "Hot Tub",
        "fireplace": "Fireplace",
        "stay_value": "Stay Value",
        "for_nights_stay": "For {count} nights stay",
        "book_now": "Book Now",
        "guest_self_checkout": "Guest Self Checkout",
        "selected_chalet": "Selected Chalet:",
        "est_gross_total": "Est. Gross Total:",
        "guest_contact_details": "Guest Contact Details",
        "first_name": "First Name",
        "last_name": "Last Name",
        "email_address": "Email Address",
        "phone_number": "Phone Number",
        "passport_id_number": "Passport / ID Number",
        "secure_mock_payments": "Secure Mock Payments",
        "credit_card_number": "Credit Card Number",
        "complete_reservation": "Complete Reservation",
        "cancel": "Cancel",
        "staff_login": "Staff Login",
        "no_chalets_available": "No chalets available",
        "try_expanding_dates": "Try expanding your check-in dates or choosing a smaller group configuration.",
        "payment_mock_info": "Resort reservation is automatically guaranteed by credit card. Card details will be verified at check-in.",
        "max_nightly_price": "Max Nightly Price",
        "included_amenities": "Included Amenities",
        "instant_stay_value_calculator": "Instant Stay Value Calculator",
        "proceed_to_checkout": "Proceed to Checkout",
        "view_details": "Details",
        "filters": "Filters",
        "reset_all": "Reset All",
        "resorts": "Resorts",
        "show_filters": "Show Filters",
        "location": "Location",
        "available": "Available",

        // Layout Keys
        "nav_dashboard": "Dashboard",
        "nav_properties": "Properties",
        "nav_resorts": "Resorts",
        "nav_units": "Units",
        "nav_operations": "Operations",
        "nav_bookings": "Reservations",
        "nav_calendar": "Calendar",
        "nav_housekeeping": "Housekeeping",
        "nav_maintenance": "Maintenance",
        "nav_financials": "Financials",
        "nav_billing": "Billing",
        "nav_analytics": "Analytics",
        "nav_owner": "Owner Yields",
        "nav_guests": "Guests",
        "nav_storefront": "Storefront",
        "btn_new_booking": "New Booking",
        "lbl_notifications": "Notifications",
        "lbl_activity_feed": "Activity Feed & Alerts",
        "lbl_mark_all_read": "Mark all read",
        
        // Dashboard Keys
        "lbl_welcome_back": "Welcome Back, {name}",
        "lbl_dashboard_sub": "Here is what's happening across your properties today.",
        "lbl_total_revenue": "Total Revenue",
        "lbl_current_occupancy": "Current Occupancy",
        "lbl_active_bookings": "Active Bookings",
        "lbl_pending_invoices": "Pending Invoices: {amount}",
        "lbl_recent_stays_bookings": "Recent Stays & Bookings",
        "lbl_view_all_stays": "View All Stays",
        "lbl_table_res_id": "Reservation ID",
        "lbl_table_stay_period": "Stay Period",
        "lbl_table_amount": "Amount",
        "lbl_table_status": "Status",
        "lbl_top_properties": "Top Properties",
        "lbl_active": "Active",
        "lbl_no_recent_stays": "No recent stays or bookings found in database.",
        "lbl_no_properties_registered": "No properties registered yet.",
        "lbl_add_first_property": "Add First Property"
    },
    ar: {
        // Storefront Keys
        "luxury_chalet_stays": "إقامات شاليهات فاخرة",
        "find_mountain_hideaway": "ابحث عن ملاذك الجبلي المثالي",
        "browse_luxury_spaces": "تصفح المساحات الفاخرة، راجع الأسعار، واحجز إقامتك على الفور.",
        "check_room_availability": "التحقق من توافر الغرف",
        "arrival_date": "تاريخ الوصول",
        "departure_date": "تاريخ المغادرة",
        "guests": "الضيوف",
        "update_search": "تحديث البحث",
        "available_premium_properties": "الشاليهات المميزة المتاحة",
        "max_capacity_guests": "الحد الأقصى {count} ضيوف",
        "free_wifi": "إنترنت مجاني",
        "hot_tub": "جاكوزي",
        "fireplace": "مدفأة",
        "stay_value": "قيمة الإقامة",
        "for_nights_stay": "لإقامة لمدة {count} ليالٍ",
        "book_now": "احجز الآن",
        "guest_self_checkout": "الدفع الذاتي للنزيل",
        "selected_chalet": "الشاليه المحدد:",
        "est_gross_total": "الإجمالي التقديري:",
        "guest_contact_details": "تفاصيل الاتصال للنزيل",
        "first_name": "الاسم الأول",
        "last_name": "الاسم الأخير",
        "email_address": "البريد الإلكتروني",
        "phone_number": "رقم الهاتف",
        "passport_id_number": "رقم جواز السفر / الهوية",
        "secure_mock_payments": "مدفوعات تجريبية آمنة",
        "credit_card_number": "رقم بطاقة الائتمان",
        "complete_reservation": "تأكيد الحجز",
        "cancel": "إلغاء",
        "staff_login": "دخول الموظفين",
        "no_chalets_available": "لا توجد شاليهات متاحة",
        "try_expanding_dates": "حاول تغيير تواريخ الوصول أو المغادرة، أو تقليل عدد الضيوف.",
        "payment_mock_info": "حجز المنتجع مضمون تلقائياً ببطاقة الائتمان. سيتم التحقق من بيانات البطاقة عند الوصول.",
        "max_nightly_price": "الحد الأقصى لسعر الليلة",
        "included_amenities": "المرافق المشمولة",
        "instant_stay_value_calculator": "حاسبة قيمة الإقامة الفورية",
        "proceed_to_checkout": "الذهاب إلى الدفع",
        "view_details": "التفاصيل",
        "filters": "الفلاتر",
        "reset_all": "إعادة تعيين الكل",
        "resorts": "المنتجعات",
        "show_filters": "عرض الفلاتر",
        "location": "الموقع",
        "available": "متاح",

        // Layout Keys
        "nav_dashboard": "لوحة التحكم",
        "nav_properties": "العقارات",
        "nav_resorts": "المنتجعات",
        "nav_units": "الوحدات",
        "nav_operations": "العمليات",
        "nav_bookings": "الحجوزات",
        "nav_calendar": "التقويم",
        "nav_housekeeping": "التنظيف",
        "nav_maintenance": "الصيانة",
        "nav_financials": "المالية",
        "nav_billing": "الفواتير",
        "nav_analytics": "التحليلات",
        "nav_owner": "أرباح المالك",
        "nav_guests": "النزلاء",
        "nav_storefront": "واجهة الحجز",
        "btn_new_booking": "حجز جديد",
        "lbl_notifications": "الإشعارات",
        "lbl_activity_feed": "ملخص النشاط والتنبيهات",
        "lbl_mark_all_read": "تحديد الكل كمقروء",
        
        // Dashboard Keys
        "lbl_welcome_back": "أهلاً بك مجدداً، {name}",
        "lbl_dashboard_sub": "إليك ما يحدث في عقاراتك ومنتجعاتك اليوم.",
        "lbl_total_revenue": "إجمالي الإيرادات",
        "lbl_current_occupancy": "نسبة الإشغال الحالية",
        "lbl_active_bookings": "الحجوزات النشطة",
        "lbl_pending_invoices": "الفواتير المعلقة: {amount}",
        "lbl_recent_stays_bookings": "آخر الإقامات والحجوزات",
        "lbl_view_all_stays": "عرض جميع الإقامات",
        "lbl_table_res_id": "رقم الحجز",
        "lbl_table_stay_period": "فترة الإقامة",
        "lbl_table_amount": "القيمة",
        "lbl_table_status": "الحالة",
        "lbl_top_properties": "أبرز العقارات",
        "lbl_active": "نشط",
        "lbl_no_recent_stays": "لم يتم العثور على إقامات أو حجوزات أخيرة في قاعدة البيانات.",
        "lbl_no_properties_registered": "لا توجد عقارات مسجلة بعد.",
        "lbl_add_first_property": "أضف عقارك الأول"
    }
};

// Get current language from Cookie or LocalStorage (Default: English)
function getLanguage() {
    return localStorage.getItem("luxestay_lang") || "en";
}

// Set language and reload
function setLanguage(lang) {
    localStorage.setItem("luxestay_lang", lang);
    document.cookie = "luxestay_lang=" + lang + ";path=/;max-age=31536000";
    
    // Apply immediately and reload to let backend layout adjust if needed
    applyTranslation();
    window.location.reload();
}

// Toggle language
function toggleLanguage() {
    const nextLang = getLanguage() === "en" ? "ar" : "en";
    setLanguage(nextLang);
}

// Apply translation and layout direction
function applyTranslation() {
    const lang = getLanguage();
    const isRtl = lang === "ar";
    
    // 1. Update HTML language and direction attributes
    const htmlEl = document.documentElement;
    htmlEl.setAttribute("lang", lang);
    htmlEl.setAttribute("dir", isRtl ? "rtl" : "ltr");
    
    // 2. Add Arabic Font dynamically if Arabic is active
    if (isRtl) {
        if (!document.getElementById("arabic-font-link")) {
            const fontLink = document.createElement("link");
            fontLink.id = "arabic-font-link";
            fontLink.rel = "stylesheet";
            fontLink.href = "https://fonts.googleapis.com/css2?family=Cairo:wght@300;400;600;700;800&display=swap";
            document.head.appendChild(fontLink);
        }
        document.body.style.fontFamily = "'Cairo', 'Outfit', sans-serif";
    } else {
        document.body.style.fontFamily = "";
    }
    
    // 3. Update Language Switcher Toggle Indicator if exists
    const langIndicator = document.getElementById("lang-indicator");
    if (langIndicator) {
        langIndicator.textContent = lang === "ar" ? "English" : "العربية";
    }
    
    // 4. Scan and translate elements with 'data-translate-key'
    const dict = translations[lang] || translations.en;
    document.querySelectorAll("[data-translate-key]").forEach(el => {
        const key = el.getAttribute("data-translate-key");
        if (dict[key]) {
            let translationText = dict[key];
            
            // Handle parameterized translations
            if (key === "lbl_welcome_back") {
                const name = el.getAttribute("data-param-name") || "Ibrahim";
                translationText = translationText.replace("{name}", name);
            } else if (key === "lbl_pending_invoices") {
                const amount = el.getAttribute("data-param-amount") || "$0.00";
                translationText = translationText.replace("{amount}", amount);
            } else if (key === "max_capacity_guests") {
                const count = el.getAttribute("data-param-count") || "2";
                translationText = translationText.replace("{count}", count);
            } else if (key === "for_nights_stay") {
                const count = el.getAttribute("data-param-count") || "0";
                translationText = translationText.replace("{count}", count);
            }

            // Set content or placeholder
            if (el.tagName === "INPUT" || el.tagName === "TEXTAREA") {
                el.setAttribute("placeholder", translationText);
            } else {
                // If it contains children icons or HTML elements, preserve them if possible
                const icon = el.querySelector(".material-symbols-outlined, i");
                if (icon) {
                    let textNodeSet = false;
                    el.childNodes.forEach(node => {
                        if (node.nodeType === Node.TEXT_NODE) {
                            if (!textNodeSet) {
                                node.textContent = isRtl ? " " + translationText + " " : " " + translationText + " ";
                                textNodeSet = true;
                            } else {
                                node.textContent = "";
                            }
                        }
                    });
                } else {
                    el.textContent = translationText;
                }
            }
        }
    });

    // 4. Adjust specific alignment layouts for RTL
    if (isRtl) {
        document.querySelectorAll(".text-left").forEach(el => {
            el.classList.replace("text-left", "text-right");
        });
        document.querySelectorAll(".ms-auto").forEach(el => {
            el.classList.replace("ms-auto", "me-auto");
        });
        document.querySelectorAll(".me-auto").forEach(el => {
            if (!el.classList.contains("ms-auto")) {
                el.classList.replace("me-auto", "ms-auto");
            }
        });
    }

    // Special custom Welcome Title resolution
    const welcomeTitle = document.getElementById("storefront-welcome-title");
    if (welcomeTitle) {
        const customText = isRtl ? welcomeTitle.getAttribute("data-welcome-ar") : welcomeTitle.getAttribute("data-welcome-en");
        if (customText) {
            welcomeTitle.textContent = customText;
        }
    }
}

// Run translation when DOM is ready
document.addEventListener("DOMContentLoaded", applyTranslation);
