# دليل التكامل مع Flutter و Angular

## نظرة عامة

التطبيق يدعم نوعين من العملاء:
1. **Angular (Web Application)** - يستخدم HttpOnly Cookies
2. **Flutter (Mobile Application)** - يستخدم Authorization Header

**مهم:** التطبيق يدعم استقبال التوكن بالطريقتين في نفس الوقت!

## آلية الاستقبال المزدوج

التطبيق يقرأ التوكن بالترتيب التالي:
1. **أولاً:** يبحث في `Authorization Header` (Flutter)
2. **ثانياً:** إذا لم يجده، يبحث في `accessToken` cookie (Angular)

```csharp
OnMessageReceived = context =>
{
    // 1. يبحث في Authorization Header أولاً
    var token = context.Request.Headers["Authorization"]
        .FirstOrDefault()?.Split(" ").Last();

    // 2. إذا لم يجده، يبحث في الكوكيز
    if (string.IsNullOrEmpty(token))
    {
        token = context.Request.Cookies["accessToken"];
    }

    context.Token = token;
    return Task.CompletedTask;
}
```

## اختبار الاستقبال المزدوج

### Endpoint للاختبار
```http
GET /api/auth/test-token-reception
Authorization: Bearer YOUR_TOKEN
```

### استجابة الاختبار
```json
{
  "Message": "Token received successfully",
  "TokenSource": "Authorization Header (Flutter)",
  "HasAuthorizationHeader": true,
  "HasAccessTokenCookie": false,
  "UserId": "user-id",
  "UserName": "username",
  "Roles": ["User"]
}
```

## إعدادات CORS

```json
{
  "ALLOWED_ORIGINS": "http://localhost:4200;https://syrianopenstor.netlify.app"
}
```

## التوثيق مع Angular (Web)

### 1. تسجيل الدخول
```typescript
// POST /api/auth/login
const response = await this.http.post('/api/auth/login', {
  email: 'user@example.com',
  password: 'password'
}).toPromise();

// الكوكيز يتم إرسالها تلقائياً مع الطلبات اللاحقة
```

### 2. إرسال الطلبات المصادقة
```typescript
// الكوكيز يتم إرسالها تلقائياً
const response = await this.http.get('/api/auth/user-data', {
  withCredentials: true
}).toPromise();
```

### 3. تحديث التوكن
```typescript
// POST /api/auth/refresh-token
const response = await this.http.post('/api/auth/refresh-token', {}, {
  withCredentials: true
}).toPromise();
```

### 4. اختبار طريقة الاستقبال
```typescript
// GET /api/auth/test-token-reception
const response = await this.http.get('/api/auth/test-token-reception', {
  withCredentials: true
}).toPromise();

console.log(response.TokenSource); // "HttpOnly Cookie (Angular)"
```

## التوثيق مع Flutter (Mobile)

### 1. تسجيل الدخول
```dart
// POST /api/auth/login-flutter
final response = await http.post(
  Uri.parse('$baseUrl/api/auth/login-flutter'),
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode({
    'email': 'user@example.com',
    'password': 'password'
  }),
);

final data = jsonDecode(response.body);
final accessToken = data['AccessToken'];
final refreshToken = data['RefreshToken'];

// حفظ التوكن في التخزين المحلي
await storage.write(key: 'access_token', value: accessToken);
await storage.write(key: 'refresh_token', value: refreshToken);
```

### 2. إرسال الطلبات المصادقة
```dart
// إضافة التوكن في Authorization Header
final accessToken = await storage.read(key: 'access_token');
final response = await http.get(
  Uri.parse('$baseUrl/api/auth/user-data'),
  headers: {
    'Authorization': 'Bearer $accessToken',
    'Content-Type': 'application/json',
  },
);
```

### 3. تحديث التوكن
```dart
// POST /api/auth/refresh-token-flutter
final refreshToken = await storage.read(key: 'refresh_token');
final response = await http.post(
  Uri.parse('$baseUrl/api/auth/refresh-token-flutter'),
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode({
    'refreshToken': refreshToken,
    'day': 1
  }),
);

final data = jsonDecode(response.body);
final newAccessToken = data['AccessToken'];
final newRefreshToken = data['RefreshToken'];

// تحديث التوكن في التخزين المحلي
await storage.write(key: 'access_token', value: newAccessToken);
await storage.write(key: 'refresh_token', value: newRefreshToken);
```

### 4. اختبار طريقة الاستقبال
```dart
// GET /api/auth/test-token-reception
final accessToken = await storage.read(key: 'access_token');
final response = await http.get(
  Uri.parse('$baseUrl/api/auth/test-token-reception'),
  headers: {
    'Authorization': 'Bearer $accessToken',
    'Content-Type': 'application/json',
  },
);

final data = jsonDecode(response.body);
print(data['TokenSource']); // "Authorization Header (Flutter)"
```

## إدارة الأخطاء

### 1. التوكن منتهي الصلاحية
```dart
// في Flutter
if (response.statusCode == 401) {
  // محاولة تحديث التوكن
  final refreshed = await refreshToken();
  if (refreshed) {
    // إعادة الطلب الأصلي
    return await makeAuthenticatedRequest();
  } else {
    // إعادة توجيه إلى شاشة تسجيل الدخول
    navigateToLogin();
  }
}
```

### 2. معالج الطلبات في Flutter
```dart
class ApiService {
  static Future<http.Response> authenticatedRequest(
    String url, {
    String method = 'GET',
    Map<String, dynamic>? body,
  }) async {
    final accessToken = await storage.read(key: 'access_token');
    
    final response = await http.request(
      Uri.parse(url),
      method: method,
      headers: {
        'Authorization': 'Bearer $accessToken',
        'Content-Type': 'application/json',
      },
      body: body != null ? jsonEncode(body) : null,
    );

    if (response.statusCode == 401) {
      // محاولة تحديث التوكن
      final refreshed = await refreshToken();
      if (refreshed) {
        // إعادة الطلب الأصلي
        return await authenticatedRequest(url, method: method, body: body);
      }
    }

    return response;
  }
}
```

## إعدادات الأمان

### 1. إعدادات الكوكيز
```csharp
// HttpOnly = true - يمنع الوصول من JavaScript
// Secure = true - يضمن الإرسال عبر HTTPS
// SameSite = SameSiteMode.None - يسمح بالكوكيز عبر النطاقات
// IsEssential = true - يضمن الإرسال مع إعدادات الخصوصية الصارمة
```

### 2. إعدادات JWT
```csharp
// Audience = "https://syrianopenstor.netlify.app/"
// Issuer = "http://localhost:7109"
// Expiration = 60 minutes
// Refresh Token = 5 days
```

## نصائح للتطوير

### 1. للـ Angular
- تأكد من إعداد `withCredentials: true` في جميع الطلبات المصادقة
- استخدم `HttpInterceptor` لإدارة التوكن تلقائياً
- تعامل مع أخطاء 401 بتحديث التوكن

### 2. للـ Flutter
- استخدم `flutter_secure_storage` لتخزين التوكن بأمان
- أنشئ `ApiService` مركزي لإدارة الطلبات
- نفذ آلية تحديث التوكن التلقائية
- استخدم `dio` أو `http` مع interceptors

### 3. اختبار التكامل
```bash
# اختبار Angular
curl -X POST http://localhost:7109/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password"}' \
  -c cookies.txt

# اختبار Flutter
curl -X POST http://localhost:7109/api/auth/login-flutter \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password"}'

# اختبار الاستقبال المزدوج
curl -X GET http://localhost:7109/api/auth/test-token-reception \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## استكشاف الأخطاء

### 1. مشاكل CORS
- تأكد من إضافة النطاق في `ALLOWED_ORIGINS`
- تأكد من إعداد `AllowCredentials()` في CORS

### 2. مشاكل الكوكيز في Flutter
- استخدم endpoints المخصصة للـ Flutter
- تأكد من إرسال التوكن في Authorization Header

### 3. مشاكل التوكن
- تحقق من صحة `JWT_AUDIENCE` و `JWT_ISSUER`
- تأكد من عدم انتهاء صلاحية التوكن
- تحقق من صحة التوقيع

### 4. اختبار الاستقبال المزدوج
- استخدم endpoint `/api/auth/test-token-reception` لاختبار طريقة استقبال التوكن
- تأكد من أن التطبيق يتعرف على الطريقة المستخدمة بشكل صحيح 