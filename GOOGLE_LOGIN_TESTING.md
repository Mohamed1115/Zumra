# Google Login API Testing Guide

## 🔍 Test External Login Flow

### Method 1: Direct Browser Test
1. Open browser and navigate to:
   ```
   https://localhost:7197/Auth/Account/ExternalLogin?provider=google&returnUrl=/
   ```

2. You will be redirected to Google login page
3. Login with your Google account
4. You'll be redirected back to the callback URL

---

## 🧪 Testing with Postman/Thunder Client

### Step 1: Initiate External Login
**Important:** This endpoint returns a Challenge, which redirects to Google. You need to handle this in a browser.

```http
POST https://localhost:7197/Auth/Account/ExternalLogin
Content-Type: application/x-www-form-urlencoded

provider=google&returnUrl=/
```

### Step 2: Open the redirect URL in Browser
The response will be a redirect (302) to Google OAuth page. Copy the location and open in browser.

### Step 3: After Google Login
After successful Google authentication, you'll be redirected to:
```
https://localhost:7197/Auth/Account/ExternalLoginCallback?returnUrl=/
```

---

## 🔧 Troubleshooting

### Error: "redirect_uri_mismatch"
**Solution:** Add this URL to Google Cloud Console Authorized redirect URIs:
```
https://localhost:7197/Auth/Account/ExternalLoginCallback
```

### Error: "access_denied"
**Solution:** User cancelled the Google login or didn't grant permissions.

### Error: "Error from external provider"
**Solution:** Check that:
1. Google Client ID and Secret are correct
2. Google+ API is enabled
3. OAuth consent screen is configured

---

## 📋 Expected Response

### Successful Login - New User:
1. User is created in database
2. External login is linked
3. User is signed in
4. Redirected to returnUrl

### Successful Login - Existing User:
1. User is found by email
2. External login is linked (if not already)
3. User is signed in
4. Redirected to returnUrl

---

## 🧪 Test Cases

### Test Case 1: New Google User
- Login with Google account not in database
- Expected: New user created with email confirmed

### Test Case 2: Existing User (Same Email)
- Login with Google account that matches existing user email
- Expected: Google login linked to existing account

### Test Case 3: Already Linked Account
- Login with previously linked Google account
- Expected: Direct sign-in

---

## ⚙️ Current Configuration

**Base URL:** https://localhost:7197
**Callback URL:** https://localhost:7197/Auth/Account/ExternalLoginCallback

**Google OAuth Settings Required:**
- Authorized JavaScript origins: https://localhost:7197
- Authorized redirect URIs: https://localhost:7197/Auth/Account/ExternalLoginCallback
